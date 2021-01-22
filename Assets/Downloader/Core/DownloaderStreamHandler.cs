namespace E.Net
{
    public abstract class DownloaderStreamHandler
    {
        public abstract class Stream 
        {
            public abstract long Length { get; }
            public abstract long Position { get; set; }
            public abstract int Read(byte[] buffer, int offset, int count);
            public abstract void Write(byte[] buffer, int offset, int count);
            public abstract void Close();
        }

        public enum FileMode
        {
            CreateNew = 1,
            Create = 2,
            Open = 3,
            OpenOrCreate = 4,
            Truncate = 5,
            Append = 6
        }

        public enum FileAccess
        {
            Read = 1,
            Write = 2,
            ReadWrite = 3
        }

        public abstract class File 
        {
            public abstract bool Exists(string path);
            public abstract Stream Open(string path, FileMode fileMode, FileAccess fileAccess);
            public abstract void Delete(string path);
            public abstract void Move(string from, string to);
            public abstract long GetLength(string path);
        }

        public abstract class Directory
        {
            public abstract bool Exists(string path);
            public abstract void Delete(string path, bool recursive);
            public abstract void CreateDirectory(string path);
            public abstract string[] GetDirectories(string path, string searchPartten, bool searchAll);
        }

        public abstract class WebRequest
        {
            public abstract bool CreateRequest();
            public abstract int Timeout { get; set; }
            public abstract void AddRange(long from, long to = -1);
            public abstract bool GetResponse();
            public abstract long TotalContentLength { get; }
            public abstract long ContentLength { get; }
            public abstract System.DateTime LastModified { get; }
            public abstract string GetResponseHeader(string headerName);
            public abstract string GetETag();
            public abstract bool StatusCodeSuccess { get; }
            public abstract int StatusCode { get; }
            public abstract string StatusDescription { get; }
            public abstract bool GetResponseStream();
            public abstract int Read(byte[] buffer, int offset, int count);
            public abstract void Close();
        }

        public abstract File GetFileImplement();

        public abstract Directory GetDirectoryImplement();

        public abstract WebRequest CreateWebRequestInstance(System.Uri uri);
    }
}
