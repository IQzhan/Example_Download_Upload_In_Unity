using System;
using System.Collections.Generic;

namespace E.Data
{
    public abstract class DataStream : System.IDisposable
    {
        protected DataStream(in System.Uri uri)
        { this.uri = uri; }

        public System.Uri uri;

        /// <summary>
        /// set username and password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public abstract void SetAccount(string username, string password);

        /// <summary>
        /// connection timeout
        /// </summary>
        public abstract int Timeout { get; set; }

        /// <summary>
        /// test host connection and return true if successed, save the connection result
        /// </summary>
        public abstract bool TestConnection(bool force);

        /// <summary>
        /// test connection and return true if successed.
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
        public abstract System.DateTime LastModified { get; set; }

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

        /// <summary>
        /// Get files and folders info of this directory
        /// </summary>
        /// <param name="topOnly"></param>
        /// <returns></returns>
        public abstract SortedList<string, FileSystemEntry> GetFileSystemEntries(bool topOnly);

        /// <summary>
        /// file or folder
        /// </summary>
        public struct FileSystemEntry
        {
            public string uri;
            public string name;
            public bool isFolder;
            public DateTime lastModified;
            public override string ToString()
            { return "uri: " + uri + ", name: " + name + (isFolder ? ", type: folder" : ", type: file") + ", lastModified: " + lastModified; }
        }

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
                    uri = null;
                }
                ReleaseUnmanaged();
                disposedValue = true;
            }
        }

        ~DataStream() { Dispose(disposing: false); }

        public void Dispose()
        {
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }
    }
}
