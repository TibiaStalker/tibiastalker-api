using Polly;

namespace TibiaStalker.Infrastructure.Policies;

public class TibiaDataRetryPolicy : ITibiaDataRetryPolicy
{
    public IAsyncPolicy GetRetryPolicy(int retryCount)
    {
        var policy = Policy.Handle<TaskCanceledException>().Or<Exception>().WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.5 , retryAttempt)));

        return policy;
    }
}