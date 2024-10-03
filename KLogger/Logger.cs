using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLogger
{
    public class Logger : IKLogger
    {
        public Logger(string? folderPath = null)
        { 
            FolderPath = folderPath;
            if(folderPath != null)
            {
                DirectoryInfo directory = new DirectoryInfo(folderPath);
                if(!directory.Exists)
                    directory.Create();
            }
        }
        public string? FolderPath { get; init; }

        public void Log(string message, LogLevel level)
        {
            SwitchLogColor(level);
            string toWriteMsg = $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}，Log Level {level}: {message}";
            if(level == LogLevel.Error || level == LogLevel.Fatal)
                Console.Error.WriteLine(toWriteMsg);
            else
                Console.WriteLine(toWriteMsg);
            if(FolderPath != null)
            {
                string path = Path.Combine(FolderPath, $"{DateTime.Now.ToString("yyyy-MM-dd")}.log");
                using FileStream stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Write);
                stream.Write(Encoding.UTF8.GetBytes(toWriteMsg+'\n'));
            }
            Console.ResetColor();
        }
        public void Debug(string message)
        {
            Log(message,LogLevel.Debug);
        }

        public void Error(string message)
        {
            Log(message,LogLevel.Error);
        }

        public void Fatal(string message)
        {
            Log(message,LogLevel.Fatal);
        }

        public void Info(string message)
        {
            Log(message,LogLevel.Info);
        }


        public void Trace(string message)
        {
            Log(message, LogLevel.Trace);
        }

        public void Warn(string message)
        {
            Log(message, LogLevel.Warn);
        }

        public void SwitchLogColor(LogLevel level)
        {
            switch(level)
            {
                case LogLevel.Trace: Console.ForegroundColor = ConsoleColor.Green; break;
                case LogLevel.Debug: Console.ForegroundColor = ConsoleColor.Yellow; break;
                case LogLevel.Info: Console.ForegroundColor = ConsoleColor.Magenta; break;
                case LogLevel.Warn: Console.ForegroundColor = ConsoleColor.DarkYellow;break;
                case LogLevel.Error: Console.ForegroundColor = ConsoleColor.Red; break;
                case LogLevel.Fatal: Console.ForegroundColor = ConsoleColor.DarkRed; break;
            }
        }
    }
}
