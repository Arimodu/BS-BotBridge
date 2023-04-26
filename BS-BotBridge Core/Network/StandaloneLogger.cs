namespace BSBBCore.Network
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;

    public enum LogLevel
    {
        Error,
        Warn,
        Info,
        Debug
    }

    public class StandaloneLogger
    {
        private readonly Action<string> _logAction;
        private readonly LogLevel _logLevel;

        public StandaloneLogger(Action<string> logAction, LogLevel logLevel)
        {
            _logAction = logAction;
            _logLevel = logLevel;
        }

        public void Log(LogLevel level, string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            if (level > _logLevel) return;
            string className = Path.GetFileNameWithoutExtension(filePath);
            string logLine = $"[{level} :: {className}.{memberName}] {message}";
            _logAction(logLine);
        }

        public void Debug(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "") => Log(LogLevel.Debug, message, memberName, filePath);
        public void Info(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "") => Log(LogLevel.Info, message, memberName, filePath);
        public void Warn(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "") => Log(LogLevel.Warn, message, memberName, filePath);
        public void Error(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "") => Log(LogLevel.Error, message, memberName, filePath);
    }
}
