using Polly;

namespace TibiaStalker.Infrastructure.Policies;

public interface ITibiaDataRetryPolicy
{
    public IAsyncPolicy GetRetryPolicy(int retryCount);
}