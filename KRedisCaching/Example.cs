using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KRedisCaching
{
    public class Example
    {
        static void Main(string[] args)
        {
            RedisConnection connection = new RedisConnection
            {
                Host = "127.0.0.1",
                Port = 6379
            };
            connection.Connect();
            
        }
    }
}
