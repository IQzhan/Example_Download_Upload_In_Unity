using System.Collections.Generic;
using System.IO;

namespace E.Net
{
    public class Downloader
    {
        public int CacheSize = 1024;

        public int MaxTaskNum
        { get { return taskHandler.MaxTaskNum; } set { taskHandler.MaxTaskNum = value; } }

        public string[] VersionHeaderNames = { "X-COS-META-MD5" };

        private readonly DownloadHandler downloadHandler;

        private readonly DownloaderCommandHandler commandHandler;

        private readonly DownloaderStreamHandler streamHandler;

        private readonly DownloaderTaskHandler taskHandler;

        private System.Action closeActions;

        private readonly Dictionary<string, bool> checkedHosts = new Dictionary<string, bool>();

        protected Downloader(DownloaderStreamHandler streamHandler, DownloaderTaskHandler taskHandler)
        {
            this.streamHandler = streamHandler;
            this.taskHandler = taskHandler;
            downloadHandler = new DownloadHandler(ref streamHandler);
            commandHandler = DownloaderCommandHandlerFactory.GetDownloaderCommandHandler();
        }

        private class DownloaderCommandHandlerFactory : DownloaderCommandHandler
        {
            private DownloaderCommandHandlerFactory() { }

            public static DownloaderCommandHandler GetDownloaderCommandHandler()
            {
                return new DownloaderCommandHandlerFactory();
            }
        }

        public void Tick()
        {
            commandHandler.Tick();
            taskHandler.Tick();
        }

        public void Close()
        {
            closeActions?.Invoke();
        }

        public void TestConnection(System.Uri source, bool force, System.Action<bool> callback)
        {
            System.Action task = () => 
            {
                bool value = TestConnectionAync(source, force);
                commandHandler.AddCommand(() =>
                { callback?.Invoke(value); });
            };
            taskHandler.AddTask(ref task);
        }


        /// <summary>
        /// 仅加载不保存到本地
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public DownloaderRequest Download(System.Uri source, bool forceTestConnection = false)
        {
            return Download(source, null, null, forceTestConnection);
        }

        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="source">源地址: http,ftp,file,unc</param>
        /// <param name="targetBase">目标基地址: file,unc</param>
        /// <param name="targetRelative">目标相对路径包含文件名</param>
        /// <param name="forceTestConnection">true强制测试源链接，false使用上次测试结果</param>
        /// <returns></returns>
        public DownloaderRequest Download(System.Uri source, System.Uri targetBase, string targetRelative, bool forceTestConnection = false)
        {
            DownloadHandler.Request downloaderRequest = null;
            DownloadHandler.SaveInfo saveInfo = null;
            if(!CheckUri(ref source, ref targetBase, ref targetRelative, ref saveInfo, ref downloaderRequest)) { return null; }
            commandHandler.AddCommand(() =>
            {
                System.Action commandAction = () =>
                {
                    try
                    {
                        string localVersion = null;
                        string version = null;
                        bool lv = false;
                        bool sv = false;
                        bool eq = false;
                        bool end = false;
                        if(source != null)
                        {
                            if (TestConnectionAync(source, forceTestConnection))
                            { version = GetVersion(source); }
                            sv = !string.IsNullOrWhiteSpace(version);
                        }
                        if (saveInfo != null)
                        {
                            saveInfo.CleanUnused();
                            localVersion = saveInfo.Version;
                            lv = !string.IsNullOrWhiteSpace(localVersion);
                            end = !saveInfo.IsDownloading;
                        }
                        eq = localVersion == version;
                        bool loadC = (!lv && !sv) || (lv && !sv) || (lv && sv && eq && end);
                        //bool downloadC = (!lv && sv) || (lv && sv && !eq) || (lv && sv && eq && !end);
                        bool saveC = saveInfo != null;
                        bool sourceC = source != null;
                        if((saveC && sourceC && loadC) || (saveC && !sourceC))
                        {
                            LoadData(ref saveInfo, ref downloaderRequest);
                        }
                        else //if((saveC && sourceC && !loadC) || (!saveC && sourceC))
                        {
                            DownloadData(ref source, ref saveInfo, ref version, ref downloaderRequest);
                        }
                        // else (!saveC && !sourceC) { }
                    }
                    catch (System.Exception e)
                    {
                        downloaderRequest.IsError = true;
                        DownloaderDebug.LogError(e.Message + System.Environment.NewLine + e.StackTrace);
                    }
                    finally
                    {
                        downloaderRequest.Close();
                        saveInfo.Close();
                        DoRemoveCloseAction(downloaderRequest.Close);
                        DoClose(downloaderRequest);
                    }
                };
                taskHandler.AddTask(ref commandAction);
            });
            return downloaderRequest;
        }

        private bool CheckUri(ref System.Uri source, ref System.Uri targetBase, ref string targetRelative, 
            ref DownloadHandler.SaveInfo saveInfo, ref DownloadHandler.Request downloaderRequest)
        {
            try
            {
                if (source == null || !source.IsAbsoluteUri) { throw new System.ArgumentException("uri must be absolute."); }
                if (source.IsFile)
                {
                    if(targetBase == null)
                    {
                        string path = source.LocalPath;
                        string prePath = Path.GetDirectoryName(path);
                        string relativePath = Path.GetFileName(path);
                        source = null;
                        targetBase = new System.Uri(prePath);
                        targetRelative = relativePath;
                    }
                }
                if (targetBase != null)
                {
                    if (!targetBase.IsAbsoluteUri || !targetBase.IsFile)
                    { throw new System.ArgumentException("must be absolute and a local file path.", "targetBase"); }
                    if (string.IsNullOrWhiteSpace(targetRelative))
                    { throw new System.ArgumentNullException("targetRelative"); }
                    saveInfo = downloadHandler.GetSaveInfo(targetBase, targetRelative);
                }
                downloaderRequest = downloadHandler.CreateRequest();
                DoAddCloseAction(downloaderRequest.Close);
            }
            catch (System.Exception e)
            {
                if (downloaderRequest != null) { downloaderRequest.IsError = true; }
                DownloaderDebug.LogException(e);
                return false;
            }
            return true;
        }

        private void LoadData(ref DownloadHandler.SaveInfo saveInfo, ref DownloadHandler.Request downloaderRequest)
        {
            if (!downloaderRequest.LoadAfterDownloaded) { return; }
            byte[] bytes = null;
            try
            {
                if (!saveInfo.Exists()) { return; }
                downloaderRequest.IsLoading = true;
                long length = saveInfo.Length;
                downloaderRequest.Size = length;
                downloaderRequest.DownloadedSize = length;
                bytes = new byte[length];
                byte[] temp = new byte[CacheSize];
                long currentSeek = 0;
                int i;
                while (currentSeek < length)
                {
                    if (!downloaderRequest.IsClosed)
                    {
                        long refCount = length - currentSeek;
                        int readCount = refCount > CacheSize ? CacheSize : (int)refCount;
                        saveInfo.ReadData(currentSeek, temp, 0, readCount);
                        for (i = 0; i < readCount; i++)
                        { bytes[currentSeek + i] = temp[i]; }
                        currentSeek += readCount;
                        downloaderRequest.LoadedSize = currentSeek;
                    }
                    else { break; }
                }
            }
            catch (System.Exception e)
            {
                bytes = null;
                downloaderRequest.IsError = true;
                DownloaderDebug.LogError(e.Message + System.Environment.NewLine + e.StackTrace);
            }
            finally
            {
                downloaderRequest.IsLoading = false;
                downloaderRequest.Data = bytes;
            }
        }

        private void DownloadData(ref System.Uri source, ref DownloadHandler.SaveInfo saveInfo, ref string version, ref DownloadHandler.Request downloaderRequest)
        {
            DownloaderStreamHandler.WebRequest req = null;
            try
            {
                long position = 0;
                if (saveInfo != null)
                {
                    saveInfo.Version = version;
                    position = saveInfo.Length;
                }
                downloaderRequest.IsConnecting = true;
                downloaderRequest.DownloadedSize = position;
                req = streamHandler.CreateWebRequestInstance(source);
                if (req.CreateRequest())
                {
                    if (downloaderRequest.IsClosed) { return; }
                    req.AddRange(position);
                    req.Timeout = downloaderRequest.Timeout;
                    if (req.GetResponse())
                    {
                        if (downloaderRequest.IsClosed) { return; }
                        if (req.StatusCodeSuccess)
                        {
                            downloaderRequest.IsConnecting = false;
                            long length = req.TotalContentLength;
                            if (length <= 0) { return; }
                            downloaderRequest.Size = length;
                            if (length == position) { return; }
                            if (req.GetResponseStream())
                            {
                                if (downloaderRequest.IsClosed) { return; }
                                downloaderRequest.IsDownloading = true;
                                byte[] temp = new byte[CacheSize];
                                long currentSeek = position;
                                int readCount;
                                while (currentSeek < length)
                                {
                                    if (!downloaderRequest.IsClosed)
                                    {
                                        readCount = req.Read(temp, 0, CacheSize);
                                        if (saveInfo != null) 
                                        { saveInfo.WriteData(currentSeek, temp, 0, readCount); }
                                        currentSeek += readCount;
                                        downloaderRequest.DownloadedSize = currentSeek;
                                    }
                                    else { break; }
                                }
                            }
                            else { downloaderRequest.IsError = true; }
                        }
                        else
                        {
                            downloaderRequest.IsError = true;
                            DownloaderDebug.LogError(req.StatusCode.ToString() + " " + req.StatusDescription + System.Environment.NewLine + source);
                        }
                    }
                }
                else { downloaderRequest.IsError = true; }
            }
            catch (System.Exception e)
            {
                downloaderRequest.IsError = true;
                DownloaderDebug.LogError(e.Message + System.Environment.NewLine + e.StackTrace);
            }
            finally
            {
                req?.Close();
                downloaderRequest.IsConnecting = false;
                downloaderRequest.IsDownloading = false;
                if (downloaderRequest.IsDownloadComplete && saveInfo != null)
                { saveInfo.CompleteDownload(); }
                LoadData(ref saveInfo, ref downloaderRequest);
            }
        }

        private bool TestConnectionAync(System.Uri source, bool force)
        {
            lock (checkedHosts)
            {
                string host = source.Host;
                if (force || !checkedHosts.TryGetValue(host, out bool value))
                {
                    value = ForceTestConnection(source);
                    checkedHosts.Add(host, value);
                }
                return value;
            }
        }

        private bool ForceTestConnection(System.Uri source)
        {
            bool value = false;
            DownloaderStreamHandler.WebRequest req = null;
            try
            {
                req = streamHandler.CreateWebRequestInstance(source);
                if (req.CreateRequest() && req.GetResponse())
                {
                    value = true;
                }
            }
            catch (System.Exception e)
            {
                DownloaderDebug.LogError(e.Message + System.Environment.NewLine + e.StackTrace);
            }
            finally
            {
                req?.Close();
            }
            return value;
        }

        private string GetVersion(System.Uri source)
        {
            string version = null;
            DownloaderStreamHandler.WebRequest req = null;
            try
            {
                req = streamHandler.CreateWebRequestInstance(source);
                if (req.CreateRequest() && req.GetResponse())
                {
                    for (int i = 0; i < VersionHeaderNames.Length; i++)
                    {
                        string v = req.GetResponseHeader(VersionHeaderNames[i]);
                        if (v != null) { version = v; }
                    }
                    if (version == null) { version = req.GetETag(); }
                    if (version == null) { version = req.LastModified.Ticks.ToString(); }
                }
            }
            catch(System.Exception e)
            {
                DownloaderDebug.LogError(e.Message + System.Environment.NewLine + e.StackTrace);
            }
            finally
            {
                req?.Close();
            }
            return version;
        }

        private void DoAddCloseAction(System.Action closeAction)
        {
            commandHandler.AddCommand(() =>
            {
                closeActions -= closeAction;
                closeActions += closeAction;
            });
        }

        private void DoRemoveCloseAction(System.Action closeAction)
        {
            commandHandler.AddCommand(() =>
            {
                closeActions -= closeAction;
            });
        }

        private void DoClose(DownloadHandler.Request downloaderRequest)
        {
            commandHandler.AddCommand(() =>
            {
                downloaderRequest.onClose();
            },
            () =>
            {
                return downloaderRequest.onClose != null;
            });
        }

        private class DownloadHandler
        {
            public DownloadHandler(ref DownloaderStreamHandler streamHandler)
            {
                fileMethods = streamHandler.GetFileImplement();
                directoryMethods = streamHandler.GetDirectoryImplement();
            }

            private DownloaderStreamHandler.File fileMethods;

            private DownloaderStreamHandler.Directory directoryMethods;

            public class Request : DownloaderRequest
            {
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

            public Request CreateRequest()
            {
                return new Request();
            }

            public SaveInfo GetSaveInfo(System.Uri targetBase, string targetRelative)
            {
                return new SaveInfo(targetBase, targetRelative, ref fileMethods, ref directoryMethods);
            }

            public class SaveInfo
            {
                private const string FolderName = "a8e19009413b6d1bf00eeb89fd3083280fa5636d";

                public SaveInfo(System.Uri targetBase, string targetRelative,
                    ref DownloaderStreamHandler.File fileMethods,
                    ref DownloaderStreamHandler.Directory directoryMethods)
                {
                    this.targetBase = targetBase;
                    filePath = new System.Uri(targetBase, targetRelative);
                    downloadingPath = new System.Uri(targetBase, targetRelative + ".downloading");
                    cacheFolder = new System.Uri(targetBase, Path.Combine(FolderName, targetRelative));
                    this.fileMethods = fileMethods;
                    this.directoryMethods = directoryMethods;
                }

                private DownloaderStreamHandler.File fileMethods;

                private DownloaderStreamHandler.Directory directoryMethods;

                private readonly System.Uri targetBase;

                private readonly System.Uri filePath;

                private readonly System.Uri downloadingPath;

                private readonly System.Uri cacheFolder;

                private string FilePath
                { get { if (IsDownloading) { return downloadingPath.LocalPath; } else { return filePath.LocalPath; } } }

                public bool IsDownloading
                { get { return GetIsDownloading(); } }

                public long Length
                { get { return GetLength(); } }

                public string Version
                { get { return GetVersion(); } set { SetVersion(value); } }

                private bool GetIsDownloading()
                { if (!fileMethods.Exists(filePath.LocalPath)) { return true; } return false; }

                private long GetLength()
                { return fileMethods.GetLength(FilePath); }

                public void CompleteDownload()
                { if (IsDownloading) { Close(); if (fileMethods.Exists(downloadingPath.LocalPath))
                        { fileMethods.Move(downloadingPath.LocalPath, filePath.LocalPath); } } }

                public bool Exists()
                { if(fileMethods.Exists(filePath.LocalPath) || fileMethods.Exists(downloadingPath.LocalPath)) 
                    { return true; } return false; }

                public void WriteData(long position, byte[] bytes, int offset, int count)
                { WriteStream(DataWriteStream, position, bytes, offset, count); }

                public void ReadData(long position, byte[] bytes, int offset, int count)
                { ReadStream(DataReadStream, position, bytes, offset, count); }

                public void CleanUnused()
                { if (!fileMethods.Exists(filePath.LocalPath) && !fileMethods.Exists(downloadingPath.LocalPath))
                    { DeleteOldVersion(); } }

                public void DeleteOldVersion()
                {
                    Close();
                    if (directoryMethods.Exists(cacheFolder.LocalPath))
                    { directoryMethods.Delete(cacheFolder.LocalPath, true); }
                    if (fileMethods.Exists(filePath.LocalPath))
                    { fileMethods.Delete(filePath.LocalPath); }
                    if (fileMethods.Exists(downloadingPath.LocalPath))
                    { fileMethods.Delete(downloadingPath.LocalPath); }
                }

                private string GetVersion()
                {
                    string value = null;
                    if (directoryMethods.Exists(cacheFolder.LocalPath))
                    {
                        string[] folderNames = directoryMethods.GetDirectories(cacheFolder.LocalPath, "*", false);
                        if (folderNames.Length > 0)
                        { value = Path.GetFileName(folderNames[0]); }
                    }
                    return value;
                }

                private void SetVersion(string value)
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        string oriVersion = GetVersion();
                        CreateBaseDir(targetBase.LocalPath);
                        if (value != oriVersion)
                        {
                            DeleteOldVersion();
                            string versionPath = Path.Combine(cacheFolder.LocalPath, value);
                            directoryMethods.CreateDirectory(versionPath);
                        }
                    }
                    else { DeleteOldVersion(); }
                }

                private void CreateBaseDir(string baseSavePath)
                {
                    string basePath = Path.Combine(baseSavePath, FolderName);
                    if (!directoryMethods.Exists(basePath))
                    { directoryMethods.CreateDirectory(basePath); }
                }

                private DownloaderStreamHandler.Stream dataWriteStream;

                private DownloaderStreamHandler.Stream dataReadStream;

                private DownloaderStreamHandler.Stream DataWriteStream
                {
                    get
                    {
                        if (dataWriteStream == null)
                        {
                            CloseStream(ref dataReadStream);
                            string path = FilePath;
                            string dir = Path.GetDirectoryName(path);
                            if (!directoryMethods.Exists(dir))
                            { directoryMethods.CreateDirectory(dir); }
                            dataWriteStream = OpenStream(path, DownloaderStreamHandler.FileAccess.Write);
                        }
                        return dataWriteStream;
                    }
                }

                private DownloaderStreamHandler.Stream DataReadStream
                {
                    get
                    {
                        if (dataReadStream == null)
                        {
                            CloseStream(ref dataWriteStream);
                            string path = FilePath;
                            string dir = Path.GetDirectoryName(path);
                            if (!directoryMethods.Exists(dir))
                            { directoryMethods.CreateDirectory(dir); }
                            dataReadStream = OpenStream(path, DownloaderStreamHandler.FileAccess.Read);
                        }
                        return dataReadStream;
                    }
                }

                private DownloaderStreamHandler.Stream OpenStream(string path, DownloaderStreamHandler.FileAccess fileAccess)
                { DownloaderStreamHandler.Stream stream = fileMethods.Open(path, DownloaderStreamHandler.FileMode.OpenOrCreate, fileAccess); return stream;}

                private void WriteStream(DownloaderStreamHandler.Stream stream, long position, byte[] bytes, int offset, int count)
                { stream.Position = position; stream.Write(bytes, offset, count); }

                private void ReadStream(DownloaderStreamHandler.Stream stream, long position, byte[] bytes, int offset, int count)
                { stream.Position = position; stream.Read(bytes, offset, count); }

                private void CloseStream(ref DownloaderStreamHandler.Stream stream) { stream?.Close(); stream = null; }

                public void Close() { CloseStream(ref dataWriteStream); CloseStream(ref dataReadStream); }
            }
        }
    }
}