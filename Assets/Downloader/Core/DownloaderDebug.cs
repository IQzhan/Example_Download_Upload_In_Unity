﻿namespace E.Net
{
    public class DownloaderDebug
    {
        public static bool enableLog = false;

        public static bool enableLogError = true;

        public static bool enableLogWarning = false;

        public static bool enableLogException = true;

        private static System.Action<string> logAction;

        private static System.Action<string> logErrorAction;

        private static System.Action<string> logWarningAction;

        private static System.Action<System.Exception> logExceptionAction;

        public static void OverrideLog(System.Action<string> logAction)
        {
            DownloaderDebug.logAction = logAction;
        }

        public static void OverrideLogError(System.Action<string> logErrorAction)
        {
            DownloaderDebug.logErrorAction = logErrorAction;
        }

        public static void OverrideLogWarning(System.Action<string> logWarningAction)
        {
            DownloaderDebug.logWarningAction = logWarningAction;
        }

        public static void OverrideLogException(System.Action<System.Exception> logExceptionAction)
        {
            DownloaderDebug.logExceptionAction = logExceptionAction;
        }

        public static void Log(string message)
        {
            if (enableLog)
            {
                logAction?.Invoke(message);
            }
        }

        public static void LogError(string message)
        {
            if (enableLogError)
            {
                logErrorAction?.Invoke(message);
            }
        }

        public static void LogWarning(string message)
        {
            if (enableLogWarning)
            {
                logWarningAction?.Invoke(message);
            }
        }

        public static void LogException(System.Exception exception)
        {
            if (enableLogException)
            {
                logExceptionAction?.Invoke(exception);
            }
        }
    }
}