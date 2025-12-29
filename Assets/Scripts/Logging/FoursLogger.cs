using System.IO;
using System.Runtime.CompilerServices;
using Singletons;
using UnityEngine;

namespace Logging
{
    public sealed class FoursLogger: Singleton<FoursLogger>, ILogger
    {
        public void Log(object message,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            Debug.Log($"[{Path.GetFileName(file)}:{line} - {member}] {message}");
        }

        public void LogWarning(object message,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            Debug.LogWarning($"[{Path.GetFileName(file)}:{line} - {member}] {message}");
        }

        public void LogError(object message,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            Debug.LogError($"[{Path.GetFileName(file)}:{line} - {member}] {message}");
        }
    }
}