using JWT;
using JWT.Algorithms;
using JWT.Builder;
using KJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KWeb.HttpOption.Jwt
{
    public struct SinglePayload
    {
        public object Payload { get; set; }
    }
    public static class KJwt
    {
        public static string Generate<T>(string secretKey,string issuser,string audience,
            long expire,T payload)
        {
            DateTime time = DateTime.Now.AddSeconds(expire);
            List<KeyValuePair<string,object>> claims = new List<KeyValuePair<string,object>>();
            Type type = typeof(T);
            foreach(PropertyInfo property in type.GetProperties(BindingFlags.Instance|BindingFlags.Public))
            {
                 claims.Add(new KeyValuePair<string, object>(property.Name, property.GetValue(payload)));
            }
            return JwtBuilder.Create().WithAlgorithm(new HMACSHA256Algorithm())
                .Issuer(issuser)
                .Audience(audience)
                .AddClaims(claims.ToArray())
                .WithSecret(secretKey)
                .ExpirationTime(time)
                .Encode();
        }

        public static T? Parse<T>(string secretKey,string token)
        {
            JsonParser parser = new JsonParser(JwtBuilder.Create()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(secretKey)
                .Decode(token));
            return (T?)parser.Parse(typeof(T));
        }

        public static string GenerateSingle(string secretKey, string issuser, string audience,
            long expire,object payload)
        {
            DateTime time = DateTime.Now.AddSeconds(expire);
            return JwtBuilder.Create().WithAlgorithm(new HMACSHA256Algorithm())
                .Issuer(issuser)
                .Audience(audience)
                .AddClaim("Payload",payload)
                .WithSecret(secretKey)
                .ExpirationTime(time)
                .Encode();
        }

        public static object Parse(string secretKey, string token)
        {
            JsonParser parser = new JsonParser(JwtBuilder.Create()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(secretKey)
                .Decode(token));
            return parser.Parse<object>("Payload");
        }
    }
}
