using Dapper;
using MediatR;
using Shared.Database.Queries.Sql;
using TibiaStalker.Application.Dapper;
using TibiaStalker.Application.Dtos;
using TibiaStalker.Application.Exceptions;
using TibiaStalker.Application.Interfaces;
using TibiaStalker.Application.Validations;

namespace TibiaStalker.Application.Queries.Character;

public record GetCharacterWithCorrelationsQuery(string Name) : IRequest<CharacterWithCorrelationsResult>;

public class GetCharacterWithCorrelationsQueryHandler : IRequestHandler<GetCharacterWithCorrelationsQuery, CharacterWithCorrelationsResult>
{
    private readonly IDapperConnectionProvider _connectionProvider;
    private readonly ITibiaDataClient _tibiaDataClient;
    private readonly IRequestValidator _validator;

    public GetCharacterWithCorrelationsQueryHandler(IDapperConnectionProvider connectionProvider, ITibiaDataClient tibiaDataClient, IRequestValidator validator)
    {
        _connectionProvider = connectionProvider;
        _tibiaDataClient = tibiaDataClient;
        _validator = validator;
    }

    public async Task<CharacterWithCorrelationsResult> Handle(GetCharacterWithCorrelationsQuery request, CancellationToken cancellationToken)
    {
        _validator.ValidSearchTextLenght(request.Name);
        _validator.ValidSearchTextCharacters(request.Name);

        var character = await _tibiaDataClient.FetchCharacterWithoutRetry(request.Name.Trim());
        if (character is null)
        {
            throw new TibiaDataApiConnectionException();
        }
        if (string.IsNullOrWhiteSpace(character.Name))
        {
            throw new NotFoundException(nameof(Character), request.Name);
        }

        using var connection = _connectionProvider.GetConnection(EDataBaseType.PostgreSql);
        var parameters = new
        {
            CharacterName = request.Name.ToLower()
        };

        var correlations = (await connection.QueryAsync<CorrelationResult>(GenerateQueries.GetOtherPossibleCharacters, parameters)).ToArray();

        if (correlations.Length == 0)
        {
            parameters = new
            {
                CharacterName = character.Name.ToLower()
            };
            correlations = (await connection.QueryAsync<CorrelationResult>(GenerateQueries.GetOtherPossibleCharacters, parameters)).ToArray();
        }

        var result = new CharacterWithCorrelationsResult
        {
            FormerNames = character.FormerNames ?? Array.Empty<string>(),
            FormerWorlds = character.FormerWorlds ?? Array.Empty<string>(),
            Name = character.Name,
            Level = character.Level,
            Traded = character.Traded,
            Vocation = character.Vocation,
            World = character.World,
            LastLogin = character.LastLogin,
            OtherVisibleCharacters = character.OtherCharacters ?? Array.Empty<string>(),
            PossibleInvisibleCharacters = correlations
        };

        return result;
    }
}
