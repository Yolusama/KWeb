
using DependencyInjection;

namespace KRedisCaching
{
    public class RedisCache
    {
        private readonly RedisConnection connection;
        public RedisCache()
        {
            connection = ServiceProvider.Get<RedisConnection>();
        }

        public RedisDatabase this[int index]
        {
            get { return connection[index]; }
        }
        public RedisDatabase GetDatabase(int index)
        {
            return connection[index];
        }
    }

}
