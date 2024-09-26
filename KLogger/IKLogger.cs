namespace KLogger
{
    public interface IKLogger
    {
       
        public void Log(string message,LogLevel level);
        public void SwitchLogColor(LogLevel level);
        public void Debug(string message);
        public void Info(string message);
        public void Warn(string message);
        public void Error(string message);
        public void Fatal(string message);
        public void Trace(string message);
    }
}