using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KRedisCaching
{
    internal class Program
    {
        static void Main(string[] args)
        {
            RedisConnection connection = new RedisConnection
            {
                Host = "localhost",
                Port = 6379
            };

            redis[2].Set("k", 2, TimeSpan.FromSeconds(150));
        
        }
    }
}
