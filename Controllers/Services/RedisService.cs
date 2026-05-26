namespace epico_backend.Controllers.Services
{
    using StackExchange.Redis;

    public class RedisService
    {
        private readonly IDatabase _db;

        public RedisService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase(); // 👈 هذا هو IDatabase
        }

        public Task SetAsync(string key, string value, TimeSpan? expiry = null)
        {
            TimeSpan ttl = TimeSpan.FromSeconds(10);

           return _db.StringSetAsync(key, value, expiry: ttl);
        }

        public Task<RedisValue> GetAsync(string key)
        {
            return _db.StringGetAsync(key);
        }
    }
}
