using System;

namespace E.Data
{
    /// <summary>
    /// file or folder info
    /// </summary>
    public struct FileSystemEntry
    {
        public string uri;
        public string name;
        public bool isFolder;
        public DateTime lastModified;
        public long size;
        public override string ToString()
        { return "{\"uri\": " + uri + ", \"name\": " + name + (isFolder ? ", \"type\": folder" : ", \"type\": file") + ", \"size\": " + size + ", \"lastModified\": " + lastModified + "}"; }
    }
}