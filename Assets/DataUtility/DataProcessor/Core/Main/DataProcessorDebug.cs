namespace E.Data
{
    public class DataProcessorDebug
    {
        public static bool enableLog = false;

        public static bool enableLogError = true;

        public static bool enableLogException = true;

        private static System.Action<object> logAction;

        private static System.Action<object> logErrorAction;

        private static System.Action<System.Exception> logExceptionAction;

        public static void OverrideLog(System.Action<object> logAction)
        {
            DataProcessorDebug.logAction = logAction;
        }

        public static void OverrideLogError(System.Action<object> logErrorAction)
        {
            DataProcessorDebug.logErrorAction = logErrorAction;
        }

        public static void OverrideLogException(System.Action<System.Exception> logExceptionAction)
        {
            DataProcessorDebug.logExceptionAction = logExceptionAction;
        }

        public static void Log(object message)
        {
            if (enableLog)
            {
                logAction?.Invoke(message);
            }
        }

        public static void LogError(object message)
        {
            if (enableLogError)
            {
                logErrorAction?.Invoke(message);
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
