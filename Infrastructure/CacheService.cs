using Microsoft.AspNetCore.OutputCaching;
using ServiceStack.Redis;

namespace Store.Infrastructure
{
    public class CacheService : IOutputCacheStore
    { 

        private IRedisClientAsync client;

        public CacheService(IRedisClientAsync redisClient)
        {
            client = redisClient;
        }

        public ValueTask EvictByTagAsync(string tag, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<byte[]?> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            byte[]? data = null;

            if (key != null)
            {
                data = await client.GetAsync<byte[]>(key);
            }
            return data;
        }

        public async ValueTask SetAsync(string key, byte[] value, string[]? tags,
            TimeSpan validFor, CancellationToken cancellationToken = default)
        {
            if (key != null)
            {
                await client.SetAsync(key, value, validFor, cancellationToken);
            }
        }
    }
}
