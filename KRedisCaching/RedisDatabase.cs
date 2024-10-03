using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KRedisCaching
{
    public class RedisDatabase
    {
        private readonly RedisConnection connection;
        public RedisDatabase(RedisConnection connection) 
        {
           this.connection = connection;
        }

        public void Set(string key, object value)
        {
            connection.SendCommand("SET", key, value.ToString());
        }
        /// <summary>
        /// expire为储存时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expire"></param>
        public void Set(string key, object value,long expire)
        {
            connection.SendCommand("SET", key,value.ToString(), "EX", expire.ToString());
        }

        public void Set(string key, object value,TimeSpan expire)
        {
            if(!value.GetType().IsArray||!value.GetType().IsCollectible)
            connection.SendCommand("SET", key, value.ToString(), "EX", expire.TotalSeconds.ToString());
            else
            {
                IEnumerable<object> values = (IEnumerable<object>)value;
                foreach(var item in values)
                {
                    connection.SendCommand("RPUSH",key, item.ToString(),"EX",expire.TotalSeconds.ToString());
                }

            }
        }


        public object Get(string key)
        {
            return connection.SendCommand("GET", key);
        }

    }
}
