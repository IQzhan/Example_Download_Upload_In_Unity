namespace E.Data
{
    public abstract class DataStreamFactory : System.IDisposable
    {
        public abstract DataStream GetStream(in System.Uri uri);

        protected abstract void ReleaseManaged();

        protected abstract void ReleaseUnmanaged();

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ReleaseManaged();
                }
                ReleaseUnmanaged();
                disposedValue = true;
            }
        }

        ~DataStreamFactory()
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