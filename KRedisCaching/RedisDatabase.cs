using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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
            Type valueType = value.GetType();
            string valueStr = IsBaseType(valueType) ? value.ToString() : $"{valueType.FullName}:${value.ToString()}";
            connection.SendCommand("SET", key, valueStr);
        }
        /// <summary>
        /// expire为储存时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expire"></param>
        public void Set(string key, object value,long expire)
        {
            if (!value.GetType().IsArray || !value.GetType().IsCollectible)
            {
                Type valueType = value.GetType();
                string valueStr = IsBaseType(valueType) ? value.ToString() : $"{valueType.FullName}\':\'{JsonSerializer.Serialize(value)}";
                connection.SendCommand("SET", key, valueStr, "EX", expire.ToString());
            }
            else
            {
                IEnumerable<object> values = (IEnumerable<object>)value;
                foreach (var item in values)
                {
                    Type valueType = item.GetType();
                    string valueStr = IsBaseType(valueType) ? value.ToString() : $"{valueType.FullName}\':\'{JsonSerializer.Serialize(value)}";
                    connection.SendCommand("RPUSH", key, valueStr, "EX", expire.ToString());
                }
            }
        }

        public void Set(string key, object value,TimeSpan expire)
        {
            Set(key, value,(long)expire.TotalSeconds);
        }

        public bool Has(string key)
        {
            string res = connection.SendCommand("EXISTS", key);
            int judgeRes = int.Parse(res);
            bool result = judgeRes == 1;
            return result;
        }

        public object Get(string key)
        {
            string res = connection.SendCommand("GET", key);
            int index = res.IndexOf("\':\'");
            object result;
            if (index < 0)
                result = res;
            else
            {
                string csType = res.Substring(0, index);
                string value = res.Substring(index + 3);
                Type type = Type.GetType(csType);   
                result = JsonSerializer.Deserialize(value, type);
            }
            return result;
        }
        public T? Get<T>(string key)
        {
            string res = connection.SendCommand("GET", key);
            int index = res.IndexOf("\':\'");
            T? result;
            if (index < 0)
                result = default;
            else
            {
                string csType = res.Substring(0, index);
                Type type = typeof(T);
                if (!type.IsAssignableFrom(Type.GetType(csType)))
                    result = default;
                else
                {
                    string value = res.Substring(index + 3);
                    result = JsonSerializer.Deserialize<T>(value);
                }
            }
            return result;
        }

        private bool IsBaseType(Type type)
        {
            return type == typeof(string) ||
                type == typeof(int) || type == typeof(long)
                || type == typeof(float) || type == typeof(double)
                || type == typeof(uint) || type == typeof(ulong);
        }
    }
}
