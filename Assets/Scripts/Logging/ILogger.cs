using System.Runtime.CompilerServices;

namespace Logging
{
    public interface ILogger
    {
        void Log(object message,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0);

        void LogWarning(object message,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
        }

        void LogError(object message,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
        }
    }
}