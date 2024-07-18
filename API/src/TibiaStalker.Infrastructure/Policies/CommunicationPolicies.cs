using Polly;
using Polly.Extensions.Http;

namespace TibiaStalker.Infrastructure.Policies;

public static class CommunicationPolicies
{
    public static IAsyncPolicy<HttpResponseMessage> GetHttpClientRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}