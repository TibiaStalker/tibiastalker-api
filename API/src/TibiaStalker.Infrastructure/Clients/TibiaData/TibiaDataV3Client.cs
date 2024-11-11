using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TibiaStalker.Application.Exceptions;
using TibiaStalker.Application.Interfaces;
using TibiaStalker.Application.TibiaData.Dtos;
using TibiaStalker.Application.TibiaData.Dtos.v3;
using TibiaStalker.Infrastructure.Policies;

namespace TibiaStalker.Infrastructure.Clients.TibiaData;

public class TibiaDataV3Client : ITibiaDataClient
{
    public const int TotalRetryAttempts = 3;
    private readonly HttpClient _httpClient;
    private readonly ILogger<TibiaDataV3Client> _logger;
    private readonly TibiaDataSection _tibiaDataSection;
    private readonly ITibiaDataRetryPolicy _retryPolicy;

    public TibiaDataV3Client(HttpClient httpClient, IOptions<TibiaDataSection> tibiaData, ILogger<TibiaDataV3Client> logger, ITibiaDataRetryPolicy retryPolicy)
    {
        _httpClient = httpClient;

        _logger = logger;
        _tibiaDataSection = tibiaData.Value;
        _retryPolicy = retryPolicy;
    }


    public async Task<IReadOnlyList<string>> FetchWorldsNames()
    {
        var retryCount = 0;
        var retryPolicy = _retryPolicy.GetRetryPolicy(TotalRetryAttempts);

        return await retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_tibiaDataSection.ApiVersion}worlds");
                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();
                var contentDeserialized = JsonConvert.DeserializeObject<TibiaDataV3WorldsResponse>(content);
                if (string.IsNullOrWhiteSpace(contentDeserialized.Worlds.RecordDate))
                {
                    throw new TibiaDataApiEmptyResponseException();
                }

                var worldNames = contentDeserialized.Worlds.RegularWorlds.Select(world => world.Name).ToArray();

                return worldNames;
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("{exceptionName} during invoke method {method}, attempt {retryCount}.",
                    nameof(TaskCanceledException), nameof(FetchWorldsNames), retryCount++);
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError("Method {method} problem, attempt {retryCount}. Exception {exception}",
                    nameof(FetchWorldsNames), retryCount++, exception.Message);
                throw;
            }
        });
    }

    public async Task<IReadOnlyList<string>> FetchCharactersOnline(string worldName)
    {
        var retryCount = 0;
        var retryPolicy = _retryPolicy.GetRetryPolicy(TotalRetryAttempts);

        return await retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_tibiaDataSection.ApiVersion}world/{worldName}");
                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogInformation("Server '{serverName}' is off.", worldName);
                    return Array.Empty<string>();
                }

                var contentDeserialized = JsonConvert.DeserializeObject<TibiaDataV3WorldResponse>(content);
                if (string.IsNullOrWhiteSpace(contentDeserialized.Worlds.World.CreationDate))
                {
                    throw new TibiaDataApiEmptyResponseException();
                }

                if (contentDeserialized.Worlds.World.OnlinePlayers is null || !contentDeserialized.Worlds.World.OnlinePlayers.Any())
                {
                    _logger.LogInformation("Server '{serverName}' is out of players at that moment.", worldName);
                    return Array.Empty<string>();
                }

                return contentDeserialized.Worlds.World.OnlinePlayers.Select(x => x.Name).ToArray();
            }
            catch (TaskCanceledException exception)
            {
                _logger.LogError(
                    "{exceptionName} during invoke method {method}, world: '{worldName}', attempt {retryCount}. Exception {exception}",
                    nameof(TaskCanceledException), nameof(FetchCharactersOnline), worldName, retryCount++, exception.Message);
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    "Method {method} problem, world: '{worldName}', attempt {retryCount}. Exception {exception}",
                    nameof(FetchCharactersOnline), worldName, retryCount++, exception.Message);
                throw;
            }
        });
    }

    public async Task<CharacterResult> FetchCharacterWithRetry(string characterName)
    {
        return await FetchCharacterInternal(characterName, true);
    }

    public async Task<CharacterResult> FetchCharacterWithoutRetry(string characterName)
    {
        return await FetchCharacterInternal(characterName, false);
    }

    private async Task<CharacterResult> FetchCharacterInternal(string characterName, bool withRetryPolicy)
    {
        var retryCount = withRetryPolicy ? 0 : TotalRetryAttempts;
        var retryPolicy = withRetryPolicy ? _retryPolicy.GetRetryPolicy(TotalRetryAttempts) : _retryPolicy.GetRetryPolicy(0);

        TibiaDataV3CharacterResponse contentDeserialized = null;

        await retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_tibiaDataSection.ApiVersion}character/{characterName}");
                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();
                contentDeserialized = JsonConvert.DeserializeObject<TibiaDataV3CharacterResponse>(content);
                if (string.IsNullOrWhiteSpace(contentDeserialized.Characters.Character.Name))
                {
                    if (retryCount < TotalRetryAttempts)
                    {
                        throw new TibiaDataApiEmptyResponseException();
                    }
                }
            }
            catch (TaskCanceledException)
            {
                _logger.LogError(
                    "{exceptionName} during invoke method {method}, character: '{characterName}', attempt {retryCount}.",
                    nameof(TaskCanceledException), nameof(FetchCharacterInternal), characterName, retryCount++);
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    "Method {method} problem, character: '{characterName}', attempt {retryCount}. Exception {exception}",
                    nameof(FetchCharacterInternal), characterName, retryCount++, exception.Message);
                throw;
            }
        });

        return contentDeserialized.MapToCharacterResult();
    }
}