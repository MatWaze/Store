using ServiceStack.Redis;
using System.Text;

namespace Store.Infrastructure
{
    public class CacheService : IMyCacheStore
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
                data = await client.GetAsync<byte[]>(key, cancellationToken);
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

        public bool TryGetValue<T>(string key, out T? value, CancellationToken cancellationToken = default)
        {
            bool ans = false;

            byte[]? result = GetAsync(key, cancellationToken).Result;

            if (result != null)
            {
                ans = true;
                string encodedResults = Encoding.UTF8.GetString(result);
                value = JsonConvert.DeserializeObject<T>(encodedResults)!;
            }
            else
            {
                value = default(T);
            }
            return ans;
        }
    }
}
