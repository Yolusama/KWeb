using DependencyInjection;
using KWeb;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace KRedisCaching
{
    public class RedisConnection : IDisposable
    {
        public string Host { get; set; }
        public int Port { get; set; }

        private Socket socket;
        public RedisDatabase usedDatabase;
        public RedisConnection() 
        { 
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            usedDatabase = new RedisDatabase(this);
        }
        public RedisConnection(string host, int port) : this() 
        {
            Host = host;
            Port = port;
            if (Host == "localhost")
                Host = "127.0.0.1";
            socket.Connect(new IPEndPoint(IPAddress.Parse(Host), Port));
        }

        public bool Connect()
        {
            if(socket.Connected) return true;
            if (Host == "localhost")
                Host = "127.0.0.1";
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(Host), Port);
            socket.Connect(endPoint);
            return socket.Connected;
        }

        public RedisDatabase GetDataBase(int count)
        {
            SendCommand("SELECT", count.ToString());
            return usedDatabase;
        }

        public RedisDatabase this[int count]
        {
            get { return GetDataBase(count); }
        }

        public string SendCommand(params string[] args)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"*{args.Length}\r\n");

            foreach(string arg in args)
            {
                builder.Append($"${arg.Length}\r\n");
                builder.Append($"{arg}\r\n");
            }
            using NetworkStream stream = new NetworkStream(socket);
            stream.Write(Encoding.UTF8.GetBytes(builder.ToString()));
            return GetResponse(stream);
        }

        private string GetResponse(Stream stream)
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true))
            {
                string line = reader.ReadLine();

                if (line.StartsWith("+")) // 简单字符串
                {
                    return line.Substring(1);
                }
                else if (line.StartsWith(":")) // 整数
                {
                    return line.Substring(1);
                }
                else if (line.StartsWith("$")) // Bulk字符串
                {
                    int length = int.Parse(line.Substring(1));
                    if (length == -1) return null; // nil

                    char[] buffer = new char[length];
                    reader.Read(buffer, 0, length);
                    reader.ReadLine(); // 读取末尾的 \r\n
                    return new string(buffer);
                }
                else if (line.StartsWith("*")) // 数组
                {
                    return line; // 可扩展处理
                }
                else
                {
                    throw new Exception("Unknown response format.");
                }
            }
        }

        public void Dispose()
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Dispose();
        }
    }

    public class RedisConnectionBuilder
    {
        private string host;
        private int port;

        public RedisConnectionBuilder()
        {
        }
        public RedisConnectionBuilder SetHost(string host)
        {
            this.host = host;
            return this;
        }

        public RedisConnectionBuilder SetPort(int port)
        {
            this.port = port;
            return this;
        }

        public RedisConnection Build()
        {
            RedisConnection connection = new RedisConnection
            {
                Host = host,
                Port = port
            };
            connection.Connect();
            return connection;
        }
    }

    public static class AppExpansion
    {
        public static void AddRedisConnetion(this WebApplication app,Action<RedisConnectionBuilder> builder)
        {
            RedisConnectionBuilder connctionBuilder = new RedisConnectionBuilder();
            builder(connctionBuilder);
            app.Services.AddService(connctionBuilder.Build);
        }
    }
}