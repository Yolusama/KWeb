using System.Text;

namespace KJSON
{
    public class KJson : IDisposable
    {
        private readonly JsonReader reader;
        private readonly Stream stream;

        public KJson(string? jsonFile)
        {
            if (jsonFile == null) return;
            stream = new FileStream(jsonFile,FileMode.Open, FileAccess.Read);
            reader = new JsonReader(stream);
        }

        public JsonInfo? Get(string key)
        {
            return reader.Read(key);
        }

        public T? Get<T>(string key)
        {
            JsonInfo? info = reader.Read(key);
            return info == null ? default : info.Get<T>();
        }

        public JsonInfo? this[string key]
        {
            get { return reader[key]; }
        }

        public void Dispose()
        {
            reader.Dispose();
            stream.Dispose();
        }
    }
}