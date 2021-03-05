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
                //DirectoryAsyncOperation targetDirectory = GetFileSystemEntries(targetUri);
                if(sourceDirectory == null 
                    //|| targetDirectory == null
                    )
                {
                    asyncOperation.IsError = true;
                    DoClose();
                    return asyncOperation;
                }
                sourceDirectory.onClose += () => { DoAction(); };
                //targetDirectory.onClose += () => { DoAction(); };
                void DoAction()
                {
                    if(sourceDirectory != null && sourceDirectory.IsProcessingComplete 
                        //&& targetDirectory != null && targetDirectory.IsProcessingComplete
                        )
                    {
                        SortedList<string, FileSystemEntry> sourceEntries = sourceDirectory.Entries;
                        //SortedList<string, FileSystemEntry> targetEntries = targetDirectory.Entries;
                        if(sourceEntries != null 
                            //&& targetEntries != null
                            )
                        {
                            commandHandler.AddCommand(() =>
                            {
                                Regex matchRule = new Regex(Utility.GetFileName(sourceUri) + @"[/\\](.+)");
                                //TODO
                                //for get file from source
                                //for get file from target
                                //if target not in source then delete
                                //source to array and clone from source one by one
                                List<FileSystemEntry> sourceEntryList = new List<FileSystemEntry>();
                                long totalSize = 0;
                                foreach (KeyValuePair<string, FileSystemEntry> sourceEntryKV in sourceEntries)
                                {
                                    FileSystemEntry sourceEntry = sourceEntryKV.Value;
                                    DataProcessorDebug.LogError(sourceEntry);
                                    if (!sourceEntry.isFolder)
                                    {
                                        totalSize += sourceEntry.size;
                                        sourceEntryList.Add(sourceEntry);
                                    }
                                    
                                }

                                DataProcessorDebug.Log(sourceEntryList.Count);

                                asyncOperation.Size = totalSize;
                                int sourceEntryListIndex = 0;
                                if(sourceEntryList.Count > 0) { cloneNext(); }
                                void cloneNext()
                                {
                                    FileSystemEntry sourceEntry = sourceEntryList[sourceEntryListIndex];
                                    string partPath = matchRule.Match(sourceEntry.uri).Groups[1].Value;
                                    string targetPath = targetUri + partPath;
                                    CloneAsyncOperation cloneAsync = Clone(sourceEntry.uri, targetPath);
                                    cloneAsync.LoadData = false;
                                    cloneAsync.onClose += () =>
                                    {
                                        if(++sourceEntryListIndex < sourceEntryList.Count) { cloneNext(); }
                                        else { asyncOperation.SetCurrentCloneAsyncOperation(null); }
                                    };
                                    asyncOperation.SetCurrentCloneAsyncOperation(cloneAsync);
                                }
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