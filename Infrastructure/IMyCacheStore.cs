using Microsoft.AspNetCore.OutputCaching;

namespace Store.Infrastructure
{
    public interface IMyCacheStore : IOutputCacheStore
    {
        public bool TryGetValue<T>(string key, out T? value, CancellationToken cancellationToken = default);
    }
}
