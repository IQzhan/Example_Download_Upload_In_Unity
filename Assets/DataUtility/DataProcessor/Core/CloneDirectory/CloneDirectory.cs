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
                if(string.IsNullOrWhiteSpace(source)) { throw new System.ArgumentNullException("source", "can not be null."); }
                if (string.IsNullOrWhiteSpace(target)) { throw new System.ArgumentNullException("target", "can not be null."); }
                System.Uri sourceUri = null;
                System.Uri targetUri = null;
                if (!source.EndsWith("/")) sourceUri = new System.Uri(source + "/");
                if (!target.EndsWith("/")) targetUri = new System.Uri(target + "/");
                if (sourceUri == null) sourceUri = new System.Uri(source);
                if (targetUri == null) targetUri = new System.Uri(target);
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
                string sourceUri = GetOriginalString(source);
                string targetUri = GetOriginalString(target);
                DirectoryAsyncOperation sourceDirectory = GetFileSystemEntries(sourceUri);
                DirectoryAsyncOperation targetDirectory = GetFileSystemEntries(targetUri);
                if (sourceDirectory == null)
                {
                    asyncOperation.IsError = true;
                    DoClose();
                    return asyncOperation;
                }
                int completedCount = 1;
                if (targetDirectory != null) { completedCount++; }
                sourceDirectory.onClose += () => { if(--completedCount == 0) DoAction(); };
                if (targetDirectory != null)
                { targetDirectory.onClose += () => { if (--completedCount == 0) DoAction(); }; }
                void DoAction()
                {
                    if(sourceDirectory.IsProcessingComplete)
                    {
                        SortedList<string, FileSystemEntry> sourceEntries = sourceDirectory.Entries;
                        SortedList<string, FileSystemEntry> targetEntries = null;
                        if (targetDirectory != null && targetDirectory.IsProcessingComplete)
                        { targetEntries = targetDirectory.Entries; }
                        
                        if(sourceEntries != null)
                        {
                            commandHandler.AddCommand(() =>
                            {
                                Regex matchRule = new Regex(Utility.GetFileName(sourceUri) + @"[/\\](.+)");
                                List<FileSystemEntry> sourceEntryList = new List<FileSystemEntry>();
                                long totalSize = 0;
                                foreach (KeyValuePair<string, FileSystemEntry> sourceEntryKV in sourceEntries)
                                {
                                    FileSystemEntry sourceEntry = sourceEntryKV.Value;
                                    if (!sourceEntry.isFolder)
                                    {
                                        totalSize += sourceEntry.size;
                                        sourceEntryList.Add(sourceEntry);
                                    }
                                }
                                asyncOperation.Size = totalSize;
                                bool doCloneMark = false;
                                if (targetEntries != null)
                                {
                                    List<FileSystemEntry> deleteList = new List<FileSystemEntry>();
                                    Regex matchRule1 = new Regex(Utility.GetFileName(targetUri) + @"[/\\](.+)");
                                    foreach (KeyValuePair<string, FileSystemEntry> targetEntryKV in targetEntries)
                                    {
                                        FileSystemEntry targetEntry = targetEntryKV.Value;
                                        if (!targetEntry.isFolder)
                                        {
                                            string partPath = matchRule1.Match(targetEntry.uri).Groups[1].Value;
                                            string sourcePath = sourceUri + partPath;
                                            if (!sourceEntries.ContainsKey(Utility.ConvertURLSlash(sourcePath)))
                                            { deleteList.Add(targetEntry); }
                                        }
                                    }
                                    int deleteListIndex = 0;
                                    if(deleteList.Count > 0) { doCloneMark = true; deleteNext(); }
                                    void deleteNext()
                                    {
                                        DeleteAsyncOperation deleteAsync = Delete(deleteList[deleteListIndex].uri);
                                        deleteAsync.onClose += () =>
                                        {
                                            if ((++deleteListIndex) < sourceEntryList.Count) { deleteNext(); }
                                            else { doClone(); }
                                        };
                                    }
                                }
                                if (!doCloneMark) doClone();
                                void doClone()
                                {
                                    int sourceEntryListIndex = 0;
                                    if (sourceEntryList.Count > 0) { cloneNext(); } else { DoClose(); }
                                    void cloneNext()
                                    {
                                        FileSystemEntry sourceEntry = sourceEntryList[sourceEntryListIndex];
                                        string partPath = matchRule.Match(sourceEntry.uri).Groups[1].Value;
                                        string targetPath = targetUri + partPath;
                                        CloneAsyncOperation cloneAsync = Clone(sourceEntry.uri, targetPath);
                                        cloneAsync.LoadData = false;
                                        cloneAsync.onClose += () =>
                                        {
                                            if (++sourceEntryListIndex < sourceEntryList.Count) { cloneNext(); }
                                            else { asyncOperation.SetCurrentCloneAsyncOperation(null); DoClose(); }
                                        };
                                        asyncOperation.SetCurrentCloneAsyncOperation(cloneAsync);
                                    }
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