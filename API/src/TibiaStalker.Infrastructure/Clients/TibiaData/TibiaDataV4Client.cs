﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using TibiaStalker.Application.Interfaces;
using TibiaStalker.Application.TibiaData.Dtos;
using TibiaStalker.Application.TibiaData.Dtos.v4;

namespace TibiaStalker.Infrastructure.Clients.TibiaData;

public class TibiaDataV4Client : ITibiaDataClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TibiaDataV4Client> _logger;
    private readonly string _apiVersion;
    private readonly AsyncRetryPolicy _retryPolicy;

    public TibiaDataV4Client(HttpClient httpClient, IOptions<TibiaDataSection> tibiaData, ILogger<TibiaDataV4Client> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiVersion = tibiaData.Value.ApiVersion;
        _retryPolicy = Policy.Handle<TaskCanceledException>().Or<Exception>().WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    public async Task<IReadOnlyList<string>> FetchWorldsNames()
    {
        var currentRetry = 0;

        var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiVersion}/worlds");

        return await _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();
                var contentDeserialized = JsonConvert.DeserializeObject<TibiaDataV4WorldsResponse>(content);
                var worldNames = contentDeserialized.Worlds.RegularWorlds.Select(world => world.Name).ToArray();

                return worldNames;
            }
            catch (TaskCanceledException)
            {
                currentRetry++;
                _logger.LogError("{exceptionName} during invoke method {method}, attempt {retryCount}.",
                    nameof(TaskCanceledException), nameof(FetchWorldsNames), currentRetry);
            }
            catch (Exception exception)
            {
                currentRetry++;
                _logger.LogError("Method {method} problem, attempt {retryCount}. Exception {exception}",
                    nameof(FetchWorldsNames), currentRetry, exception);
            }

            return Array.Empty<string>();
        });
    }

    public async Task<IReadOnlyList<string>> FetchCharactersOnline(string worldName)
    {
        var currentRetry = 0;

        var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiVersion}/world/{worldName}");

        return await _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogInformation("Server '{serverName}' is off.", worldName);
                    return Array.Empty<string>();
                }

                var contentDeserialized = JsonConvert.DeserializeObject<TibiaDataV4WorldResponse>(content);
                if (contentDeserialized.World.OnlinePlayers is null || !contentDeserialized.World.OnlinePlayers.Any())
                {
                    _logger.LogInformation("Server '{serverName}' is out of players at that moment.", worldName);
                    return Array.Empty<string>();
                }

                return contentDeserialized.World.OnlinePlayers.Select(x => x.Name).ToArray();
            }
            catch (TaskCanceledException exception)
            {
                currentRetry++;
                _logger.LogError("{exceptionName} during invoke method {method}, world: '{worldName}', attempt {retryCount}. Exception {exception}",
                    nameof(TaskCanceledException), nameof(FetchCharactersOnline), worldName, currentRetry, exception);
            }
            catch (Exception exception)
            {
                currentRetry++;
                _logger.LogError("Method {method} problem, world: '{worldName}', attempt {retryCount}. Exception {exception}",
                    nameof(FetchCharactersOnline), worldName, currentRetry, exception);
            }

            return Array.Empty<string>();
        });
    }

    public async Task<CharacterResult> FetchCharacter(string characterName, bool withRetryPolicy = true)
    {
        // TODO: not implemented properly
        var currentRetry = 0;

        var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiVersion}/character/{characterName}");
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                using var response = await _httpClient.SendAsync(request);
                // response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();
                var contentDeserialized = JsonConvert.DeserializeObject<TibiaDataV4CharacterResponse>(content);

                return contentDeserialized.MapToCharacterResult();
            }
            catch (TaskCanceledException)
            {
                currentRetry++;
                _logger.LogError("{exceptionName} during invoke method {method}, character: '{characterName}', attempt {retryCount}.",
                    nameof(TaskCanceledException), nameof(FetchCharacter), characterName, currentRetry);
            }
            catch (Exception exception)
            {
                currentRetry++;
                _logger.LogError("Method {method} problem, attempt {retryCount}. Exception {exception}", nameof(FetchCharacter), currentRetry, exception);
            }

            return null;
        });
    }
}