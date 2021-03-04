using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace E.Data
{
    public partial class DataProcessor
    {
        public CloneDirectoryAsyncOperation CloneDirectory(in string source, in string target)
        {
            try
            {
                System.Uri sourceUri = new System.Uri(source);
                System.Uri targetUri = new System.Uri(target);
                return CloneDirectory(sourceUri, targetUri);
            }
            catch (System.Exception e)
            {
                DataProcessorDebug.LogException(e);
                return null;
            }
        }

        public CloneDirectoryAsyncOperation CloneDirectory(in System.Uri source, in System.Uri target)
        {
            if(Check(source, target, out CloneDirectoryAsyncOperationImplement asyncOperation))
            {
                asyncOperation.IsWorking = true;
                string sourceUri = source.OriginalString;
                string targetUri = target.OriginalString;
                DirectoryAsyncOperation sourceDirectory = GetFileSystemEntries(sourceUri);
                DirectoryAsyncOperation targetDirectory = GetFileSystemEntries(targetUri);
                if(sourceDirectory == null || targetDirectory == null)
                {
                    asyncOperation.IsError = true;
                    DoClose();
                    return asyncOperation;
                }
                sourceDirectory.onClose += () => { DoAction(); };
                targetDirectory.onClose += () => { DoAction(); };
                void DoAction()
                {
                    if(sourceDirectory != null && sourceDirectory.IsProcessingComplete &&
                        targetDirectory != null && targetDirectory.IsProcessingComplete)
                    {
                        SortedList<string, FileSystemEntry> sourceEntries = sourceDirectory.Entries;
                        SortedList<string, FileSystemEntry> targetEntries = targetDirectory.Entries;
                        if(sourceEntries != null && targetEntries != null)
                        {
                            commandHandler.AddCommand(() =>
                            {
                                Regex matchRule = new Regex(@"StreamingAssets[/\\](.+)");
                                //TODO if b not in a then delete b
                                //
                                long totalSize = 0;
                                List<CloneAsyncOperation> cloneAs = new List<CloneAsyncOperation>();
                                foreach(KeyValuePair<string, FileSystemEntry> sourceEntryKV in sourceEntries)
                                {
                                    FileSystemEntry sourceEntry = sourceEntryKV.Value;
                                    if (!sourceEntry.isFolder)
                                    {
                                        totalSize += sourceEntry.size;
                                        string partPath = matchRule.Match(sourceEntry.uri).Groups[1].Value;
                                        string targetPath = targetUri + partPath;
                                        CloneAsyncOperation cloneAsync =
                                        Clone(sourceEntry.uri, targetPath);
                                        cloneAsync.LoadData = false;
                                        cloneAsync.onClose += () =>
                                        {
                                            //continue;
                                            
                                        };
                                        cloneAs.Add(cloneAsync);
                                    }
                                }
                                asyncOperation.Size = totalSize;
                            });
                        }
                        else
                        {
                            asyncOperation.IsError = true;
                            DoClose();
                        }
                    }
                }
                void DoClose()
                {
                    asyncOperation.Close();
                    asyncOperation.onClose?.Invoke();
                }
            }
            return asyncOperation;
        }
    }
}