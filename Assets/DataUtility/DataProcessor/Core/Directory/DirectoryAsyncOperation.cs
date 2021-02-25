using System.Collections.Generic;

namespace E.Data
{
    public abstract class DirectoryAsyncOperation : ConnectionAsyncOperation
    {
        public SortedList<string, FileSystemEntry> Entries { get; protected set; }
    }
}
