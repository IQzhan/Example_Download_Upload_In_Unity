namespace E.Data
{
    public partial class DataProcessor : System.IDisposable
    {
        public int CacheSize = 1024;

        public int MaxTaskNum
        { get { return taskHandler.MaxTaskNum; } set { taskHandler.MaxTaskNum = value; } }

        public long MaxCommandDeltaTick
        { get { return commandHandler.MaxFrameMilliseconds; } set { commandHandler.MaxFrameMilliseconds = value; } }

        private TaskHandler taskHandler;

        private DataStreamFactory streamFactory;

        private CommandHandler commandHandler;

        public DataProcessor(TaskHandler taskHandler, DataStreamFactory streamFactory)
        {
            this.taskHandler = taskHandler;
            this.streamFactory = streamFactory;
            commandHandler = new CommandHandlerInstance();
        }

        private class CommandHandlerInstance : CommandHandler { }

        public void Tick()
        {
            if (!disposedValue)
            {
                commandHandler.Tick();
                taskHandler.Tick();
            }
        }

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }
                taskHandler.Dispose();
                streamFactory.Dispose();
                commandHandler.Dispose();
                taskHandler = null;
                streamFactory = null;
                commandHandler = null;
                disposedValue = true;
            }
        }

        ~DataProcessor()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }
    }
}
