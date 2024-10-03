using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KWeb.HttpOption.RequestHandle
{
    public class CorsVerifier
    {
        private string[] allowedOrigins;
        private string[] allowedHeaders;
        private HttpMethod[] allowedMethods;
        private bool withCredentials;
        private long maxAge;

        public CorsVerifier() { withCredentials = false;maxAge = 3600; }
        public CorsVerifier AllowAnyOrigins()
        {
            allowedOrigins = new string[] { "*" };
            return this; 
        }

        public CorsVerifier AllowOrigins(params string[] origins)
        {
            allowedOrigins = origins;
            return this;
        }

        public CorsVerifier WithCredentials()
        {
            withCredentials = true;
            return this;
        }

        public CorsVerifier MaxAge(long expire)
        {
            maxAge = expire;
            return this;
        }

        public CorsVerifier AllowAnyMethods()
        {
            allowedMethods = new HttpMethod[] {HttpMethod.Get,
                HttpMethod.Post,HttpMethod.Put,HttpMethod.Patch,HttpMethod.Delete,HttpMethod.Head,HttpMethod.Options};
            return this;
        }
        public CorsVerifier AllowMethods(params string[] methods)
        {
            allowedMethods = new HttpMethod[methods.Length];
            for(int i = 0;i<methods.Length;i++)
                allowedMethods[i] = GetMehtod(methods[i]);
            return this;
        }
        public CorsVerifier AllowMethods(params HttpMethod[] methods)
        {
            allowedMethods = methods;
            return this;
        }
        public CorsVerifier AllowAnyHeaders()
        {
            allowedHeaders = new string[] { "*" };
            return this;
        }
        
        public CorsVerifier AllowHeaders(params string[] headers)
        {
            allowedHeaders = headers;
            return this;
        }
        public void Verify(HttpRequest request,HttpResponse response)
        {
            StringBuilder builder = new StringBuilder();
            string origin = request.Headers["Origin"];
            if(allowedOrigins.SingleOrDefault(o=>o==origin)!=null || allowedOrigins[0] == "*")
                response.Headers.Add("Access-Control-Allow-Origin",origin);
            foreach(string header in allowedHeaders)
            {
                builder.Append(header + ", ");
            }
            if (builder.Length >= 2)
                builder.Remove(builder.Length-2, 2);
            if (allowedHeaders.Length > 0)
            response.Headers.Add("Access-Control-Allow-Headers", builder.ToString());
            builder.Clear();
            foreach(HttpMethod method in allowedMethods)
            {
                builder.Append(method.ToString() + ", ");
            }
            if (builder.Length >= 2)
                builder.Remove(builder.Length - 2, 2);
            if (allowedMethods.Length > 0)
                response.Headers.Add("Access-Control-Allow-Methods", builder.ToString());
            if (withCredentials)
            {
                response.Headers.Add("Access-Control-Allow-Credentials", "true");
            }
            response.Headers.Add("Access-Control-Max-Age",maxAge.ToString());  
          


        }
        private HttpMethod GetMehtod(string method)
        {
            string[] patterns = new string[] { "get", "post", "put", "patch", "delete", "head","Options" };
            for (int i = 0; i < patterns.Length; i++)
            {
                Regex regex = new Regex(patterns[i], RegexOptions.IgnoreCase);
                if (regex.IsMatch(method))
                    switch (i)
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
    }

    public class CorsVerifierRegister
    {
        public Dictionary<string, CorsVerifier> CorsVerifiers { get; init; }
        public CorsVerifierRegister()
        {
            CorsVerifiers = new Dictionary<string, CorsVerifier>();
        }
        public CorsVerifier AddCorVerifier(string name)
        {
            CorsVerifiers[name] = new CorsVerifier();
            return CorsVerifiers[name];
        }
    }

    public class CorsVerifierRegisterBuilder
    {
        private readonly CorsVerifierRegister register;
        public CorsVerifierRegisterBuilder()
        {
            register = new CorsVerifierRegister();
        }
        public CorsVerifier AddCorVerifier(string name)
        {
            register.CorsVerifiers[name] = new CorsVerifier();
            return register.CorsVerifiers[name];
        }
        public CorsVerifierRegister Build()
        {
            return register;
        }
    }
}
