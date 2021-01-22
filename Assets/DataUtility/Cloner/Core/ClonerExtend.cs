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

        private readonly TaskHandler taskHandler;

        private readonly CommandHandler commandHandler;

        public Cloner(System.Uri CacheUri, TaskHandler taskHandler)
        {
            this.CacheUri = CacheUri;
            this.taskHandler = taskHandler;
            commandHandler = new CommandHandlerInstance();
        }

        public void Tick()
        {
            commandHandler.Tick();
            taskHandler.Tick();
        }

        private bool Check(in byte[] data, in System.Uri target, out IStream targetStream, out Request request)
        {
            targetStream = null;
            request = null;
            try
            {
                if (data == null) 
                    throw new System.ArgumentNullException("data");
                if (!(target != null && target.IsAbsoluteUri))
                    throw new System.ArgumentException("must be absolute uri", "target");
                targetStream = StreamFactory.GetStream(target);
                request = new Request();
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
                targetStream = StreamFactory.GetStream(target);
            }
            catch(System.Exception e)
            {
                ClonerDebug.LogException(e);
                return false;
            }
            return true;
        }

        private bool Check(in System.Uri source, in System.Uri target,
            out IStream sourceStream, out IStream targetStream, out Request request)
        {
            sourceStream = null;
            targetStream = null;
            request = null;
            try
            {
                if (!(source != null && source.IsAbsoluteUri)) 
                    throw new System.ArgumentException("must be absolute uri", "source");
                sourceStream = StreamFactory.GetStream(source);
                if (target != null)
                {
                    if (!target.IsAbsoluteUri) throw new System.ArgumentException("must be absollute uri", "target");
                    targetStream = StreamFactory.GetStream(target);
                }
                request = new Request();
            }
            catch (System.Exception e)
            {
                ClonerDebug.LogException(e);
                return false;
            }
            return true;
        }

        private class CommandHandlerInstance : CommandHandler { }

        private class Request : ClonerRequest
        {
            public new bool IsWorking { get { return base.IsWorking; } set { base.IsWorking = value; } }

            public new byte[] Data { get { return base.Data; } set { base.Data = value; } }

            public new long Size { get { return base.Size; } set { base.Size = value; } }

            public new long DownloadedSize { get { return base.DownloadedSize; } set { base.DownloadedSize = value; } }

            public new long LoadedSize { get { return base.LoadedSize; } set { base.LoadedSize = value; } }

            public new bool IsConnecting { get { return base.IsConnecting; } set { base.IsConnecting = value; } }

            public new bool IsDownloading { get { return base.IsDownloading; } set { base.IsDownloading = value; } }

            public new bool IsLoading { get { return base.IsLoading; } set { base.IsLoading = value; } }

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
                    // TODO: 释放托管状态(托管对象)
                }

                // TODO: 释放未托管的资源(未托管的对象)并替代终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~Cloner()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Close()
        {
            Dispose();
        }

        void System.IDisposable.Dispose()
        {
            Dispose();
        }

        private void Dispose()
        {
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }
    }
}
