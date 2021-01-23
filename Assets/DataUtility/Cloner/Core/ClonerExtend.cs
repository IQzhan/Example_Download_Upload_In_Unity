namespace E.Data
{
    public partial class Cloner : System.IDisposable
    {
        public int CacheSize = 1024;

        public int MaxTaskNum
        { get { return taskHandler.MaxTaskNum; } set { taskHandler.MaxTaskNum = value; } }

        public long MaxCommandDeltaTick
        { get { return commandHandler.MaxFrameMilliseconds; } set { commandHandler.MaxFrameMilliseconds = value; } }

        private System.Uri cacheUri;

        public System.Uri CacheUri
        {
            get { return cacheUri; }
            set
            {
                if (!(value != null && value.IsAbsoluteUri && value.IsFile))
                    throw new System.ArgumentException("must be absolute file uri", "CacheUri");
                cacheUri = value;
            }
        }

        private TaskHandler taskHandler;

        private StreamFactory streamFactory;

        private CommandHandler commandHandler;

        public Cloner(System.Uri CacheUri, TaskHandler taskHandler, StreamFactory streamFactory)
        {
            this.CacheUri = CacheUri;
            this.taskHandler = taskHandler;
            this.streamFactory = streamFactory;
            commandHandler = new CommandHandlerInstance();
        }

        public void Tick()
        {
            if (!disposedValue)
            {
                commandHandler.Tick();
                taskHandler.Tick();
            }
        }

        private bool Check(in byte[] data, in System.Uri target, out IStream targetStream, out ClonerAsyncOperationImplement request)
        {
            targetStream = null;
            request = null;
            try
            {
                if (data == null) 
                    throw new System.ArgumentNullException("data");
                if (!(target != null && target.IsAbsoluteUri))
                    throw new System.ArgumentException("must be absolute uri", "target");
                targetStream = streamFactory.GetStream(target);
                request = new ClonerAsyncOperationImplement();
            }
            catch (System.Exception e)
            {
                ClonerDebug.LogException(e);
                return false;
            }
            return true;
        }

        private bool Check(in System.Uri target, out IStream targetStream)
        {
            targetStream = null;
            try
            {
                if(!(target != null && target.IsAbsoluteUri))
                    throw new System.ArgumentException("must be absolute uri", "target");
                targetStream = streamFactory.GetStream(target);
            }
            catch(System.Exception e)
            {
                ClonerDebug.LogException(e);
                return false;
            }
            return true;
        }

        private bool Check(in System.Uri source, in System.Uri target,
            out IStream sourceStream, out IStream targetStream, out ClonerAsyncOperationImplement request)
        {
            sourceStream = null;
            targetStream = null;
            request = null;
            try
            {
                if (!(source != null && source.IsAbsoluteUri)) 
                    throw new System.ArgumentException("must be absolute uri", "source");
                sourceStream = streamFactory.GetStream(source);
                if (target != null)
                {
                    if (!target.IsAbsoluteUri) throw new System.ArgumentException("must be absollute uri", "target");
                    targetStream = streamFactory.GetStream(target);
                }
                request = new ClonerAsyncOperationImplement();
            }
            catch (System.Exception e)
            {
                ClonerDebug.LogException(e);
                return false;
            }
            return true;
        }

        private class CommandHandlerInstance : CommandHandler { }

        private class ClonerAsyncOperationImplement : ClonerAsyncOperation
        {
            public new bool IsWorking { get { return base.IsWorking; } set { base.IsWorking = value; } }

            public new byte[] Data { get { return base.Data; } set { base.Data = value; } }

            public new long Size { get { return base.Size; } set { base.Size = value; } }

            public new long ProcessedBytes { get { return base.ProcessedBytes; } set { base.ProcessedBytes = value; } }

            public new bool IsConnecting { get { return base.IsConnecting; } set { base.IsConnecting = value; } }

            public new bool IsProcessing { get { return base.IsProcessing; } set { base.IsProcessing = value; } }

            public new bool IsClosed { get { return base.IsClosed; } set { base.IsClosed = value; } }

            public new bool IsError { get { return base.IsError; } set { base.IsError = value; } }
        }

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    cacheUri = null;
                    streamFactory = null;
                }
                taskHandler.Dispose();
                commandHandler.Dispose();
                taskHandler = null;
                commandHandler = null;
                disposedValue = true;
            }
        }

        ~Cloner()
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
