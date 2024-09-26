using KJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace KWeb.HttpOption
{
    public class HttpRequest
    {
        public Dictionary<string,object> Query {  get; init; }
        public Dictionary<string,object> PathVarible { get; init; }
        public Dictionary<string,string> Headers { get; init; }
        public FormCollection? Form { get; private set; }
        public string? Body {  get; private set; }
        public HttpMethod Method { get; private set; }
        public string Version { get; private set; }

        public string Url { get; private set; }
        public string NoQueryUrl ()
        {
            int querySignIndex = Url.IndexOf('?');
            string res = Url;
            if(querySignIndex>0)
            {
                res = Url.Substring(0, querySignIndex);
            }

            return res;
        }
        private HttpMethod GetMehtod(string method)
        {
            string[] patterns = new string[] { "get", "post", "put","patch","delete", "head" ,"options" };
            for(int i=0;i<patterns.Length;i++)
            {
                Regex regex = new Regex(patterns[i], RegexOptions.IgnoreCase);
                if (regex.IsMatch(method))
                    switch(i)
                    {
                        case 0: return HttpMethod.Get;
                        case 1: return HttpMethod.Post;
                        case 2: return HttpMethod.Put;
                        case 3: return HttpMethod.Patch;
                        case 4: return HttpMethod.Delete;
                        case 5: return HttpMethod.Head;
                        case 6: return HttpMethod.Options;
                    }

            }
            return HttpMethod.Request;
        }

        public HttpRequest(byte[] requestBytes,int length)
        {
            Body = null;
            Query = new Dictionary<string, object>();
            PathVarible = new Dictionary<string, object>();
            Headers = new Dictionary<string, string>();
            Init(requestBytes,length);
        }

        private void Init(byte[] requestBytes,int length)
        {
            string requestStr = Encoding.UTF8.GetString(requestBytes,0,length); 
            StringReader reader = new StringReader(requestStr);
            FormCollectionBuilder? formBuilder = null;
            string? line;
            StringBuilder? buider = null;
            int i = 0;
            while ((line = reader.ReadLine()) != null)
            {
                if (i == 0)
                {
                    string[] infos = line.Split(' ');
                    Method = GetMehtod(infos[0]);
                    Url = infos[1];
                    InitParams(Url);
                    Version = infos[2];
                    i++;
                }
                else
                {
                    if (line == "") continue;
                    if (line.Contains('{') || buider != null)
                    {
                        if (buider == null)
                            buider = new StringBuilder(line);
                        else buider.AppendLine(line);
                    }
                    else if (line.Contains("form-data"))
                    {
                        if (formBuilder == null)
                            formBuilder = new FormCollectionBuilder(line.Substring(line.IndexOf('=') + 1));
                    }
                    else if (formBuilder != null && line.Contains(formBuilder.Sign))
                        break;
                    else
                    {
                        string[] header = line.Split(": ");
                        Headers.Add(header[0], header[1]);
                    }
                }
               
            }
            if (buider != null)
            {
                Body = buider.ToString();
            }
            if(formBuilder != null)
            {
                int index = requestStr.IndexOf(formBuilder.Sign);
                if(index>0)
                Form = formBuilder.Build(requestBytes);
            }
            reader.Dispose();
        }

        private void InitParams(string url)
        {
            int paramIndex = url.IndexOf('?');
            if (paramIndex > 0)
            {
                string queryString = url.Substring(paramIndex + 1);
                if (queryString.Contains('&'))
                {
                    string[] paramKV = queryString.Split('&');
                    foreach (string paran in paramKV)
                    {
                        string[] kv = paran.Split('=');
                        Query[kv[0]] = HttpUtility.UrlDecode(kv[1]);
                    }
                }
                else
                {
                    string[] kv = queryString.Split('=');
                    Query[kv[0]] = HttpUtility.UrlDecode(kv[1]);
                }
            }

        }
    }
}
