namespace E.Data
{
    public abstract class IStream : System.IDisposable
    {
        /// <summary>
        /// host of uri
        /// </summary>
        public abstract string Host { get; }

        /// <summary>
        /// name of file, random for others
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// connection timeout
        /// </summary>
        public abstract int Timeout { get; set; }

        /// <summary>
        /// test connection and return true if successed, save the connection result
        /// </summary>
        public abstract bool Exists { get; }

        /// <summary>
        /// create file if not exists
        /// </summary>
        /// <returns></returns>
        public abstract bool Create();

        /// <summary>
        /// delete file if exists
        /// </summary>
        /// <returns></returns>
        public abstract bool Delete();

        /// <summary>
        /// true if data complete downloaded
        /// </summary>
        public abstract bool Complete { get; set; }

        /// <summary>
        /// length of data
        /// </summary>
        public abstract long Length { get; }

        /// <summary>
        /// last modified time milliseconds of data,
        /// if get nothing, use now milliseconds
        /// </summary>
        public abstract long LastModified { get; }

        /// <summary>
        /// use [LastModified] and [Length] to generate a [Version] if data complete downloaded,
        /// else use [Version] from file name like [filename.Version.downloading] if is not complete download,
        /// else null
        /// </summary>
        public abstract string Version { get; }

        /// <summary>
        /// current seek position,
        /// set: if data can be seek
        /// </summary>
        public abstract long Position { get; set; }

        /// <summary>
        /// true if data can be read
        /// </summary>
        public abstract bool CanRead { get; }

        /// <summary>
        /// true if data can be write
        /// </summary>
        public abstract bool CanWrite { get; }

        /// <summary>
        /// write data if CanWrite is true
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public abstract void Write(byte[] buffer, int offset, int count);

        /// <summary>
        /// read data if CanRead is true
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public abstract int Read(byte[] buffer, int offset, int count);

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

        ~IStream()
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
