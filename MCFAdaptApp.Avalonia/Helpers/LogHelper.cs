using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace MCFAdaptApp.Avalonia.Helpers
{
    public static class LogHelper
    {
        public static void Log(string message, 
            [CallerFilePath] string filePath = "", 
            [CallerLineNumber] int lineNumber = 0)
        {
            string fileName = Path.GetFileName(filePath);
            Console.WriteLine($"[{fileName}:{lineNumber}] {message}");
        }
        
        public static void LogWarning(string message, 
            [CallerFilePath] string filePath = "", 
            [CallerLineNumber] int lineNumber = 0)
        {
            string fileName = Path.GetFileName(filePath);
            Console.WriteLine($"[{fileName}:{lineNumber}] WARNING: {message}");
        }
        
        public static void LogError(string message, 
            [CallerFilePath] string filePath = "", 
            [CallerLineNumber] int lineNumber = 0)
        {
            string fileName = Path.GetFileName(filePath);
            Console.WriteLine($"[{fileName}:{lineNumber}] ERROR: {message}");
        }
        
        public static void LogException(Exception ex, 
            [CallerFilePath] string filePath = "", 
            [CallerLineNumber] int lineNumber = 0)
        {
            string fileName = Path.GetFileName(filePath);
            Console.WriteLine($"[{fileName}:{lineNumber}] EXCEPTION: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
} 