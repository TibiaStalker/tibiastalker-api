using System.Diagnostics;
using Dapper;
using MediatR;
using Shared.Database.Queries.Sql;
using TibiaStalker.Application.Dapper;
using TibiaStalker.Application.Dtos;
using TibiaStalker.Application.Validations;

namespace TibiaStalker.Application.Queries.Character;

public record GetFilteredCharactersByFragmentNameQuery(
    string SearchText,
    int Page,
    int PageSize) : IRequest<FilteredCharactersDto>;

public class GetFilteredCharacterListByFragmentNameQueryHandler : IRequestHandler<GetFilteredCharactersByFragmentNameQuery,
        FilteredCharactersDto>
{
    private readonly IDapperConnectionProvider _connectionProvider;
    private readonly IRequestValidator _validator;

    public GetFilteredCharacterListByFragmentNameQueryHandler(IDapperConnectionProvider connectionProvider,
        IRequestValidator validator)
    {
        _connectionProvider = connectionProvider;
        _validator = validator;
    }

    public async Task<FilteredCharactersDto> Handle(GetFilteredCharactersByFragmentNameQuery request,
        CancellationToken cancellationToken)
    {
        _validator.ValidSearchTextLenght(request.SearchText);
        _validator.ValidSearchTextCharacters(request.SearchText);
        _validator.ValidNumberParameterRange(request.Page, nameof(request.Page), 1);
        _validator.ValidNumberParameterRange(request.PageSize, nameof(request.PageSize), 1, 100);

        using var connection = _connectionProvider.GetConnection(EDataBaseType.PostgreSql);
        var parameters = new
        {
            SearchText = request.SearchText.ToLower(),
            Page = request.Page,
            PageSize = request.PageSize
        };


        var names = await connection.QueryAsync<string>(GenerateQueries.GetFilteredCharacterNames, parameters);
        var count = await connection.QueryFirstOrDefaultAsync<int>(GenerateQueries.GetFilteredCharactersCount, parameters);


        var characterMatching = count == 0
            ? new FilteredCharactersDto { TotalCount = 0, Names = Array.Empty<string>() }
            : new FilteredCharactersDto { TotalCount = count, Names = names.ToArray() };


        return characterMatching;
    }
}