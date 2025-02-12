using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KJSON
{
    public class JsonInfo
    {
        private JsonElement element;
        public JsonElement Element { get { return element; } }
        public JsonInfo(JsonElement element)
        {
            this.element = element;
        }

        public T? Get<T>()
        {
            return element.Deserialize<T>();
        }

        public JsonInfo? Get(string key)
        {
            try
            {
                JsonElement element = this.element.GetProperty(key);
                return new JsonInfo(element);
            }
            catch
            {
                return null;
            }
        }

        public JsonInfo? this[string key]
        {
            get { return Get(key); }
        }
            
    }
}
