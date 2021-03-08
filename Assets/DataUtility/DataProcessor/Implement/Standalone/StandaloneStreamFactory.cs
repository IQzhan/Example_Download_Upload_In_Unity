using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace E.Data
{
    public class StandaloneStreamFactory : DataStreamFactory
    {
        protected StandaloneStreamFactory()
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 512;
        }

        public override DataStream GetStream(in Uri uri)
        {
            if (uri.IsFile)
            { return new FileStream(uri); }
            else if ((uri.Scheme == System.Uri.UriSchemeHttp) || (uri.Scheme == System.Uri.UriSchemeHttps))
            { return new HttpStream(uri, this); }
            else if (uri.Scheme == System.Uri.UriSchemeFtp)
            { return new FtpStream(uri, this); }
            else { throw new System.ArgumentException("Unsupported uri scheme.", "uri"); }
        }

        private ConcurrentDictionary<string, bool> testedHostConnection = new ConcurrentDictionary<string, bool>();

        protected override void ReleaseManaged()
        {
            testedHostConnection.Clear();
            testedHostConnection = null;
        }

        protected override void ReleaseUnmanaged()
        {
        }

        private static readonly Regex LastModifiedRegex = new Regex(@".+(?:\.([0-9]+)\.downloading)$");

        private static readonly Regex fileNameRegex = new Regex(@"(?:[/\\]+([^/\\]+)[/\\]*)$");

        private static string GetDirectoryName(string filePath)
        { return fileNameRegex.Replace(filePath, string.Empty); }

        private static string GetFileName(string filePath)
        { return fileNameRegex.Match(filePath).Groups[1].Value; }

        private class FileStream : DataStream
        {
            public FileStream(in System.Uri uri) : base(uri) { }

            private const string extend = @".downloading";

            private const string slash = "/";

            public override void SetAccount(string username, string password) { }

            public override int Timeout { get; set; }

            public override bool TestConnection(bool force) { return true; }

            public override bool AsCollection { get; set; } = false;

            public override bool Exists
            { get { return RefreshFileName(); } }

            private string fileName;

            private System.IO.FileInfo fileInfo;

            private System.IO.DirectoryInfo directoryInfo;

            private string FileName
            { get { if (fileName == null) { RefreshFileName(); } return fileName; } }

            int setCount1 = 0;

            string strbbs = null;

            private bool RefreshFileName()
            {
                ResetFileTarget();
                string localPath = uri.LocalPath;
                if (System.IO.Directory.Exists(localPath))
                { fileName = localPath; directoryInfo = new System.IO.DirectoryInfo(localPath); AsCollection = true; }
                else if (System.IO.File.Exists(localPath))
                { fileName = localPath; fileInfo = null; fileInfo = new System.IO.FileInfo(fileName); AsCollection = false; }
                else
                {
                    string name = GetFileName(localPath);
                    string dir = GetDirectoryName(localPath) + slash;
                    if (!System.IO.Directory.Exists(dir)) return false;
                    string[] fileNames = System.IO.Directory.GetFiles
                        (dir, name + ".*.downloading", System.IO.SearchOption.TopDirectoryOnly);
                    if (fileNames.Length > 0)
                    {
                        setCount1++;
                        strbbs = fileNames.Length.ToString();
                        fileName = fileNames[0]; fileInfo = null; fileInfo = new System.IO.FileInfo(fileName); AsCollection = false;
                    }
                }
                return fileName != null;
            }

            private void ResetFileTarget()
            { fileName = null; fileInfo = null; directoryInfo = null; }

            public override bool Complete
            {
                get { string fn = FileName; return fn != null && (AsCollection || (!AsCollection && !fn.EndsWith(extend))); }
                set
                {
                    if (!AsCollection)
                    {
                        string fn = FileName;
                        if (value && fn != null && fn.EndsWith(extend))
                        {
                            DisposeReadStream();
                            DisposeWriteStream();
                            System.DateTime lastTime = LastModified;
                            fileInfo.MoveTo(uri.LocalPath);
                            fileInfo.LastWriteTime = lastTime;
                            ResetFileTarget();
                        }
                    }
                }
            }

            public override long Length
            { get { return FileName != null ? GetLength() : 0; } }

            private long GetLength()
            { return !AsCollection ? ReadStream.Length : 0; }

            private System.DateTime lastModified = System.DateTime.MinValue;

            public override System.DateTime LastModified
            {
                get
                {
                    string fn = FileName;
                    if (fn != null)
                    {
                        if (!AsCollection)
                        {
                            if (fn.EndsWith(extend))
                            {
                                MatchCollection matchCollection = LastModifiedRegex.Matches(fn);
                                if (matchCollection.Count > 0)
                                {
                                    string val = matchCollection[0].Groups[1].Value;
                                    if (long.TryParse(val, out long lastmodifiedTick))
                                    { lastModified = new System.DateTime(lastmodifiedTick); }
                                }
                            }
                            else
                            { lastModified = fileInfo.LastWriteTime; }
                        }
                        else
                        {
                            lastModified = directoryInfo.LastWriteTime;
                        }
                    }
                    return lastModified;
                }
                set
                {
                    if (!AsCollection)
                    {
                        string fn = FileName;
                        if (fn != null)
                        {
                            if (fn.EndsWith(extend))
                            { fileInfo.MoveTo(uri.LocalPath + "." + value.Ticks.ToString() + extend); }
                            fileInfo.LastWriteTime = value;
                        }
                    }
                    lastModified = value;
                }
            }

            public override string Version
            {
                get
                {
                    System.DateTime lastTime = LastModified;
                    return lastTime != System.DateTime.MinValue ? lastTime.Ticks.ToString() : null;
                }
            }

            public override bool Delete()
            {
                if (FileName != null)
                {
                    if (!AsCollection) { fileInfo.Delete(); }
                    else { directoryInfo.Delete(true); }
                }
                ResetFileTarget();
                return true;
            }

            public override bool Create()
            {
                bool result = false;
                if (!AsCollection)
                {
                    System.DateTime lasttime = LastModified;
                    if (fileName == null && lasttime != System.DateTime.MinValue)
                    {
                        string localPath = uri.LocalPath;
                        string dir = GetDirectoryName(localPath);
                        if (!System.IO.Directory.Exists(dir)) { System.IO.Directory.CreateDirectory(dir); }

                        System.IO.File.Create(localPath + "." + lasttime.Ticks.ToString() + extend).Dispose();
                        ResetFileTarget();
                        result = true;
                    }
                    else
                    {
                        //if works here then print rested times an get times

                        DataProcessorDebug.Log("create fff " + strbbs + " " + setCount1 + " " + fileName + " " + System.IO.File.Exists(fileName));
                    }
                }
                else
                {
                    string localPath = uri.LocalPath;
                    System.IO.Directory.CreateDirectory(localPath);
                    ResetFileTarget();
                    result = true;
                }
                return result;
            }

            private long position;

            public override long Position
            {
                get
                {
                    if (!AsCollection)
                    {
                        System.IO.Stream stream = GetStream();
                        if (stream != null) position = stream.Position;
                    }
                    return position;
                }
                set
                {
                    if (!AsCollection)
                    {
                        System.IO.Stream stream = GetStream();
                        if (stream != null) stream.Position = position = value;
                    }
                }
            }

            private System.IO.Stream GetStream()
            {
                if (readStream != null) return readStream;
                if (writeStream != null) return writeStream;
                return null;
            }

            private void DisposeReadStream()
            {
                if (readStream != null)
                { readStream.Dispose(); readStream = null; }
            }

            private System.IO.Stream readStream;

            private System.IO.Stream ReadStream
            {
                get
                {
                    DisposeWriteStream();
                    if (readStream == null)
                    {
                        readStream = System.IO.File.Open(FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
                        readStream.Position = position;
                    }
                    return readStream;
                }
            }

            private void DisposeWriteStream()
            {
                if (writeStream != null)
                { writeStream.Dispose(); writeStream = null; }
            }

            private System.IO.Stream writeStream;

            private System.IO.Stream WriteStream
            {
                get
                {
                    DisposeReadStream();
                    if (writeStream == null)
                    {
                        writeStream = System.IO.File.Open(FileName, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
                        writeStream.Position = position;
                    }
                    return writeStream;
                }
            }

            public override int Read(byte[] buffer, int offset, int count)
            { if (!AsCollection) { return ReadStream.Read(buffer, offset, count); } else return 0; }

            public override void Write(byte[] buffer, int offset, int count)
            { if (!AsCollection) { WriteStream.Write(buffer, offset, count); } }

            protected override void ReleaseManaged()
            { ResetFileTarget(); }

            protected override void ReleaseUnmanaged()
            {
                DisposeReadStream();
                DisposeWriteStream();
            }

            public override SortedList<string, FileSystemEntry> GetFileSystemEntries(bool topOnly)
            {
                if (!AsCollection) { return null; }
                string[] entries = System.IO.Directory.GetFileSystemEntries(FileName, "*", topOnly ? System.IO.SearchOption.TopDirectoryOnly : System.IO.SearchOption.AllDirectories);
                if (entries != null && entries.Length > 0)
                {
                    SortedList<string, FileSystemEntry> infos = new SortedList<string, FileSystemEntry>();
                    for (int i = 0; i < entries.Length; i++)
                    {
                        string entry = entries[i];
                        string name = GetFileName(entry);
                        bool isFolder = System.IO.Directory.Exists(entry);
                        System.IO.DirectoryInfo df = null;
                        System.IO.FileInfo ff = null;
                        if (isFolder)
                        { df = new System.IO.DirectoryInfo(entry); }
                        else
                        { ff = new System.IO.FileInfo(entry); }
                        infos[entry] = new FileSystemEntry()
                        {
                            uri = entry,
                            name = name,
                            isFolder = isFolder,
                            size = isFolder ? 0 : ff.Length,
                            lastModified = isFolder ? df.LastWriteTime : ff.LastWriteTime
                        };
                    }
                    return infos;
                }
                return null;
            }
        }

        private class HttpStream : DataStream
        {
            public HttpStream(in System.Uri uri, StandaloneStreamFactory factory) : base(uri) { this.factory = factory; }

            private StandaloneStreamFactory factory;

            private int timeout;

            public override int Timeout { get => timeout; set => timeout = value; }

            private string username;

            private string password;

            public override void SetAccount(string username, string password)
            {
                if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
                { this.username = username; this.password = password; }
            }

            public override bool AsCollection { get; set; } = false;

            public override bool Exists
            { get { return TestConnection(false) && Refresh(); } }

            private static readonly Regex HostUriRegex = new Regex(@"http[s]{0,1}://[^/\\]+");

            public override bool TestConnection(bool force)
            { return TestHostConnection(HostUriRegex.Match(uri.AbsoluteUri).Value, force); }

            private bool TestHostConnection(string hostUri, bool force)
            {
                lock (string.Intern(hostUri))
                {
                    if (!force && factory.testedHostConnection.TryGetValue(hostUri, out bool val)) { return val; }
                    else { return factory.testedHostConnection[hostUri] = ForceTestConnection(hostUri); }
                }
            }

            private bool ForceTestConnection(string testUri)
            {
                System.Net.HttpWebRequest testRequest = null;
                System.Net.HttpWebResponse testResponse = null;
                try
                {
                    testRequest = GetRequest(testUri, System.Net.WebRequestMethods.Http.Head);
                    testResponse = GetResponse(testRequest);
                    return true;
                }
                catch (System.Net.WebException e)
                {
                    DataProcessorDebug.LogError("cause an connection error at: " + testUri + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace);
                    return false;
                }
                finally
                {
                    testRequest?.Abort();
                    testResponse?.Dispose();
                }
            }

            private bool Refresh()
            {
                ResetFileTarget();
                System.Net.HttpWebRequest testRequest = null;
                System.Net.HttpWebResponse testResponse = null;
                try
                {
                    testRequest = GetRequest(uri.AbsoluteUri, System.Net.WebRequestMethods.Http.Head);
                    testResponse = GetResponse(testRequest);
                    if (AsCollection) length = 0;
                    else length = testResponse.ContentLength;
                    lastModified = testResponse.LastModified;
                    return true;
                }
                catch (System.Net.WebException e)
                {
                    if (testResponse == null)
                        testResponse = e.Response as System.Net.HttpWebResponse;
                    switch (testResponse.StatusCode)
                    {
                        case System.Net.HttpStatusCode.NotFound:
                            break;
                        default:
                            throw new System.IO.IOException("cause an connection error at: " + uri.AbsoluteUri + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace);
                    }
                    return false;
                }
                finally
                {
                    testRequest?.Abort();
                    testResponse?.Dispose();
                }
            }

            private void ResetFileTarget()
            {
                length = -1;
                position = 0;
                lastModified = System.DateTime.MinValue;
            }

            public override bool Complete
            { get { return false; } set { } }

            private long length = -1;

            public override long Length
            { get { return length; } }

            private System.DateTime lastModified = System.DateTime.MinValue;

            public override System.DateTime LastModified
            { get { return lastModified; } set { } }

            public override string Version
            {
                get
                {
                    System.DateTime lastTime = LastModified;
                    return lastTime != System.DateTime.MinValue ? lastTime.Ticks.ToString() : null;
                }
            }

            private long position;

            public override long Position { get => position; set => position = value; }

            public override bool Delete()
            {
                if (DeleteFile(uri.AbsoluteUri)) { ResetFileTarget(); return true; }
                return false;
            }

            public override bool Create()
            { return ((!AsCollection && CreateFile(uri.AbsoluteUri)) || (AsCollection && CreateDir(uri.AbsoluteUri))) && Refresh(); }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (!AsCollection) { return ResponseStream.Read(buffer, offset, count); }
                else return 0;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                if (!AsCollection)
                { RequestStream.Write(buffer, offset, count); }
            }

            protected override void ReleaseManaged()
            {
                username = null;
                password = null;
                factory = null;
            }

            protected override void ReleaseUnmanaged()
            {
                DisposeReqStream();
                DisposeRspStream();
                DisposeReqRsp();
            }

            private System.Net.HttpWebRequest GetRequest(string uri, string mathod)
            {
                System.Net.HttpWebRequest req = System.Net.WebRequest.Create(uri) as System.Net.HttpWebRequest;
                if (req == null) return null;
                req.Method = mathod;
                req.Timeout = timeout;
                req.AllowAutoRedirect = true;
                if (username != null && password != null)
                { req.Credentials = new System.Net.NetworkCredential(username, password); }
                return req;
            }

            private System.Net.HttpWebResponse GetResponse(System.Net.HttpWebRequest request)
            { return request.GetResponse() as System.Net.HttpWebResponse; }

            private System.IO.Stream GetRequestStream(System.Net.HttpWebRequest request)
            { return request.GetRequestStream(); }

            private System.IO.Stream GetResponseStream(System.Net.HttpWebResponse response)
            { return response.GetResponseStream(); }

            private System.Net.HttpWebRequest webRequest;

            private System.Net.HttpWebResponse webResponse;

            private void DisposeReqRsp()
            {
                if (webRequest != null && webResponse == null)
                { webResponse = GetResponse(webRequest); }
                webResponse?.Dispose();
                webResponse = null;
                webRequest?.Abort();
                webRequest = null;
            }

            private System.IO.Stream requestStream;

            private void DisposeReqStream()
            {
                requestStream?.Dispose();
                requestStream = null;
            }

            private System.IO.Stream RequestStream
            {
                get
                {
                    if (responseStream != null)
                    {
                        DisposeRspStream();
                        DisposeReqRsp();
                    }
                    if (requestStream == null)
                    {
                        webRequest = GetRequest(uri.AbsoluteUri, System.Net.WebRequestMethods.Http.Put);
                        webRequest.AllowWriteStreamBuffering = true;
                        requestStream = GetRequestStream(webRequest);
                    }
                    return requestStream;
                }
            }

            private System.IO.Stream responseStream;

            private void DisposeRspStream()
            {
                responseStream?.Dispose();
                responseStream = null;
            }

            private System.IO.Stream ResponseStream
            {
                get
                {
                    if (requestStream != null)
                    {
                        DisposeReqStream();
                        DisposeReqRsp();
                    }
                    if (responseStream == null)
                    {
                        webRequest = GetRequest(uri.AbsoluteUri, System.Net.WebRequestMethods.Http.Get);
                        webRequest.AddRange(Position);
                        webResponse = GetResponse(webRequest);
                        responseStream = GetResponseStream(webResponse);
                    }
                    return responseStream;
                }
            }

            private bool CreateFile(string fileUri)
            {
                if (fileUri == null) return false;
                string dirname = GetDirectoryName(fileUri);
                if (!CreateDir(dirname)) { return false; }
                System.Net.HttpWebRequest httpWebRequest = null;
                System.Net.HttpWebResponse httpWebResponse = null;
                try
                {
                    httpWebRequest = GetRequest(fileUri, System.Net.WebRequestMethods.Http.Put);
                    httpWebRequest.ContentLength = 0;
                    httpWebResponse = GetResponse(httpWebRequest);
                    return true;
                }
                catch (System.Exception e)
                { throw new System.IO.IOException("create file faild." + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace); }
                finally
                {
                    httpWebResponse?.Dispose();
                    httpWebRequest?.Abort();
                }
            }

            private static readonly Regex hostRegex = new Regex(@"^(?:http[s]{0,1}://[^/\\]+)$");

            private bool CreateDir(string dirUri)
            {
                if (dirUri == null) return false;
                if (hostRegex.IsMatch(dirUri)) return TestConnection(false);
                System.Net.HttpWebRequest httpWebRequest = null;
                System.Net.HttpWebResponse httpWebResponse = null;
                try
                {
                    httpWebRequest = GetRequest(dirUri, System.Net.WebRequestMethods.Http.MkCol);
                    httpWebResponse = GetResponse(httpWebRequest);
                    return true;
                }
                catch (System.Net.WebException e)
                {
                    if (httpWebResponse == null)
                    { httpWebResponse = e.Response as System.Net.HttpWebResponse; }
                    switch (httpWebResponse.StatusCode)
                    {
                        case System.Net.HttpStatusCode.Conflict:
                            return CreateDir(GetDirectoryName(dirUri)) && CreateDir(dirUri);
                        case System.Net.HttpStatusCode.MethodNotAllowed:
                            return true;
                        default:
                            throw new System.IO.IOException("create dir faild." + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace);
                    }
                }
                finally
                {
                    httpWebResponse?.Dispose();
                    httpWebRequest?.Abort();
                }
            }

            private bool DeleteFile(string fileUri)
            {
                if (fileUri == null) return false;
                System.Net.HttpWebRequest httpWebRequest = null;
                System.Net.HttpWebResponse httpWebResponse = null;
                try
                {
                    httpWebRequest = GetRequest(fileUri, "DELETE");
                    httpWebResponse = GetResponse(httpWebRequest);
                    return true;
                }
                catch (System.Net.WebException e)
                {
                    if (httpWebResponse == null)
                    { httpWebResponse = e.Response as System.Net.HttpWebResponse; }
                    if (httpWebResponse.StatusCode == System.Net.HttpStatusCode.NotFound) return true;
                    throw new System.IO.IOException("delete file faild." + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace);
                }
                finally
                {
                    httpWebResponse?.Dispose();
                    httpWebRequest?.Abort();
                }
            }

            public override SortedList<string, FileSystemEntry> GetFileSystemEntries(bool topOnly)
            {
                if (!AsCollection) { return null; }
                string responseString = GetListDirectoryString(uri.AbsoluteUri, topOnly);
                if (string.IsNullOrWhiteSpace(responseString)) return null;
                MatchCollection matchCollection = ListDirectoryHelper.XmlFormatRegex.Matches(responseString);
                int count = matchCollection.Count;
                if (count > 0)
                {
                    SortedList<string, FileSystemEntry> resourceList = new SortedList<string, FileSystemEntry>();
                    if (count > 1)
                    {
                        for (int i = 1; i < count; i++)
                        {
                            GroupCollection groupCollection = matchCollection[i].Groups;
                            if (groupCollection.Count == 6)
                            {
                                string mUri = groupCollection[1].Value;
                                string mLastModified = groupCollection[2].Value;
                                string mName = groupCollection[3].Value;
                                string mLength = groupCollection[4].Value;
                                string mIsFolder = groupCollection[5].Value;
                                resourceList.Add(mUri, new FileSystemEntry
                                {
                                    uri = mUri,
                                    name = mName,
                                    isFolder = Convert.ToBoolean(Convert.ToInt32(mIsFolder)),
                                    size = long.Parse(mLength),
                                    lastModified = Convert.ToDateTime(mLastModified)
                                });
                            }
                        }
                    }
                    return resourceList;
                }
                return null;
            }

            private static class ListDirectoryHelper
            {
                static ListDirectoryHelper()
                {
                    string RequestString =
                    "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" +
                    "<a:propfind xmlns:a=\"DAV:\">" +
                    "<a:prop>" +
                    "<a:displayname/>" +
                    "<a:iscollection/>" +
                    "<a:getcontentlength/>" +
                    "<a:getlastmodified/>" +
                    "</a:prop>" +
                    "</a:propfind>";
                    RequestStringLength = RequestString.Length;
                    RequestBytes = System.Text.Encoding.ASCII.GetBytes(RequestString);
                }

                public static Regex XmlFormatRegex = new Regex(@"<D:response><D:href>([^<>]+)</D:href><D:propstat><D:status>[^<>]+</D:status><D:prop><D:getlastmodified>([^<>]+)</D:getlastmodified><D:displayname>([^<>]+)</D:displayname><D:getcontentlength>([0-9]+)</D:getcontentlength><D:iscollection>([01])</D:iscollection></D:prop></D:propstat></D:response>");

                public static int RequestStringLength;

                public static byte[] RequestBytes;

                public const string PROPFIND = "PROPFIND";

                public const string HEADER_Translate_f = "Translate: f";

                public const string HEADER_Depth_infinity = "Depth: infinity";

                public const string HEADER_Depth_1 = "Depth: 1";

                public const string ContentType = "text/xml";
            }

            private string GetListDirectoryString(string uri, bool topOnly)
            {
                System.Net.HttpWebRequest webRequest = null;
                System.Net.HttpWebResponse webResponse = null;
                System.IO.Stream requestStream = null;
                System.IO.Stream responseStream = null;
                try
                {
                    webRequest = GetRequest(uri, ListDirectoryHelper.PROPFIND);
                    webRequest.Headers.Add(ListDirectoryHelper.HEADER_Translate_f);
                    if (!topOnly) { webRequest.Headers.Add(ListDirectoryHelper.HEADER_Depth_infinity); }
                    else { webRequest.Headers.Add(ListDirectoryHelper.HEADER_Depth_1); }
                    webRequest.ContentLength = ListDirectoryHelper.RequestStringLength;
                    webRequest.ContentType = ListDirectoryHelper.ContentType;
                    requestStream = webRequest.GetRequestStream();
                    requestStream.Write(ListDirectoryHelper.RequestBytes, 0, ListDirectoryHelper.RequestBytes.Length);
                    requestStream.Dispose(); requestStream = null;
                    webResponse = GetResponse(webRequest);
                    responseStream = webResponse.GetResponseStream();
                    long currentPosition = 0;
                    long length = responseStream.Length;
                    byte[] data = new byte[length];
                    byte[] temp = new byte[1024];
                    while (currentPosition < length)
                    {
                        int readCount = responseStream.Read(temp, 0, 1024);
                        for (int i = 0; i < readCount; i++)
                        { data[currentPosition + i] = temp[i]; }
                        currentPosition += readCount;
                    }
                    responseStream.Dispose(); responseStream = null;
                    webResponse?.Dispose(); webResponse = null;
                    webRequest?.Abort(); webRequest = null;
                    return System.Text.Encoding.UTF8.GetString(data);
                }
                catch { throw; }
                finally
                {
                    responseStream?.Dispose();
                    requestStream?.Dispose();
                    webResponse?.Dispose();
                    webRequest?.Abort();
                }
            }
        }

        private class FtpStream : DataStream
        {
            public FtpStream(in System.Uri uri, StandaloneStreamFactory factory) : base(uri) { this.factory = factory; }

            private StandaloneStreamFactory factory;

            private int timeout;

            public override int Timeout { get => timeout; set => timeout = value; }

            private string username;

            private string password;

            public override void SetAccount(string username, string password)
            {
                if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
                { this.username = username; this.password = password; }
            }

            public override bool AsCollection { get; set; } = false;

            public override bool Exists
            { get { return TestConnection(false) && RefreshFileName(); } }

            private static readonly Regex HostUriRegex = new Regex(@"ftp://[^/\\]+");

            public override bool TestConnection(bool force)
            { return TestHostConnection(HostUriRegex.Match(uri.AbsoluteUri).Value, force); }

            private bool TestHostConnection(string hostUri, bool force)
            {
                lock (string.Intern(hostUri))
                {
                    if (!force && factory.testedHostConnection.TryGetValue(hostUri, out bool val)) { return val; }
                    else { return factory.testedHostConnection[hostUri] = ForceTestConnection(hostUri); }
                }
            }

            private bool ForceTestConnection(string testUri)
            {
                System.Net.FtpWebRequest testRequest = null;
                System.Net.FtpWebResponse testResponse = null;
                try
                {
                    testRequest = GetRequest(testUri, System.Net.WebRequestMethods.Ftp.ListDirectory);
                    if (testRequest == null) return false;
                    testResponse = GetResponse(testRequest);
                    if (testResponse == null) return false;
                    return true;
                }
                catch (System.Net.WebException e)
                {
                    DataProcessorDebug.LogError("cause an connection error at: " + testUri + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace);
                    return false;
                }
                finally
                {
                    testRequest?.Abort();
                    testResponse?.Dispose();
                }
            }

            private const string extend = ".downloading";

            private const string slash = "/";

            private static readonly Regex hostRegex = new Regex(@"^(?:ftp://[^/\\]+[/\\]?)$");

            private string fileName;

            private string FileName
            { get { if (fileName == null) { RefreshFileName(); } return fileName; } }

            private bool RefreshFileName()
            {
                ResetFileTarget();
                string absUri = uri.OriginalString;
                if (hostRegex.IsMatch(absUri)) { fileName = absUri; }
                else
                {
                    string fname = GetFileName(absUri);
                    string dirUri = GetDirectoryName(absUri);
                    string tempfname = GetFiles(dirUri, fname, out bool isFolder);
                    if (tempfname != null) fileName = dirUri + slash + tempfname;
                    AsCollection = isFolder;
                }
                return fileName != null;
            }

            private void ResetFileTarget()
            {
                fileName = null;
                length = -1;
                position = 0;
            }

            public override bool Complete
            {
                get { string fn = FileName; return fn != null && (AsCollection || (!AsCollection && !fn.EndsWith(extend))); }
                set
                {
                    if (AsCollection) return;
                    string fn = FileName;
                    if (value && fn != null && fn.EndsWith(extend))
                    {
                        if (!Rename(fn, GetFileName(uri.OriginalString)))
                        { throw new System.IO.IOException("rename faild."); }
                        ResetFileTarget();
                    }
                }
            }

            private long length = -1;

            public override long Length
            { get { if (length == -1) { length = !AsCollection ? GetLength(FileName) : 0; } return length; } }

            private System.DateTime lastModified = System.DateTime.MinValue;

            public override System.DateTime LastModified
            {
                get
                {
                    string fn = FileName;
                    if (fn != null)
                    {
                        if (!AsCollection && fn.EndsWith(extend))
                        {
                            System.Text.RegularExpressions.MatchCollection matchCollection = LastModifiedRegex.Matches(fn);
                            if (matchCollection.Count > 0)
                            {
                                string val = matchCollection[0].Groups[1].Value;
                                if (long.TryParse(val, out long lastmodifiedTick))
                                { lastModified = new System.DateTime(lastmodifiedTick); }
                            }
                        }
                        else
                        { lastModified = GetLastModified(fn); }
                    }
                    return lastModified;
                }
                set
                {
                    if (!AsCollection)
                    {
                        string fn = FileName;
                        if (fn != null)
                        {
                            if (fn.EndsWith(extend))
                            {
                                if (!Rename(fn, GetFileName(uri.OriginalString) + "." + value.Ticks.ToString() + extend))
                                { throw new System.IO.IOException("rename faild."); }
                            }
                        }
                    }
                    lastModified = value;
                }
            }

            public override string Version
            {
                get
                {
                    System.DateTime lastTime = LastModified;
                    return lastTime != System.DateTime.MinValue ? lastTime.Ticks.ToString() : null;
                }
            }

            private long position;

            public override long Position { get => position; set => position = value; }

            public override bool Delete()
            {
                if ((!AsCollection && DeleteFile(FileName)) || (AsCollection && DeleteDir(FileName))) { ResetFileTarget(); return true; }
                return false;
            }

            public override bool Create()
            {
                return
                    fileName != null ||
                    (fileName == null &&
                    ((!AsCollection &&
                    LastModified != System.DateTime.MinValue &&
                    CreateFile(uri.AbsoluteUri + "." + lastModified.Ticks.ToString() + extend)) ||
                    (AsCollection && CreateDir(uri.AbsoluteUri))
                    ));
            }

            public override int Read(byte[] buffer, int offset, int count)
            { return !AsCollection ? ResponseStream.Read(buffer, offset, count) : 0; }

            public override void Write(byte[] buffer, int offset, int count)
            { if (!AsCollection) RequestStream.Write(buffer, offset, count); }

            protected override void ReleaseManaged()
            {
                username = null;
                password = null;
                fileName = null;
                factory = null;
            }

            protected override void ReleaseUnmanaged()
            {
                DisposeReqStream();
                DisposeRspStream();
                DisposeReqRsp();
            }

            private System.Net.FtpWebRequest GetRequest(string uri, string mathod)
            {
                System.Net.FtpWebRequest req = System.Net.WebRequest.Create(uri) as System.Net.FtpWebRequest;
                if (req == null) return null;
                req.Method = mathod;
                req.Timeout = timeout;
                if (username != null && password != null)
                { req.Credentials = new System.Net.NetworkCredential(username, password); }
                req.KeepAlive = false;
                req.UsePassive = false;
                req.UseBinary = true;
                return req;
            }

            private System.Net.FtpWebResponse GetResponse(System.Net.FtpWebRequest request)
            { return request.GetResponse() as System.Net.FtpWebResponse; }

            private System.IO.Stream GetRequestStream(System.Net.FtpWebRequest request)
            { return request.GetRequestStream(); }

            private System.IO.Stream GetResponseStream(System.Net.FtpWebResponse response)
            { return response.GetResponseStream(); }

            private System.Net.FtpWebRequest webRequest;

            private System.Net.FtpWebResponse webResponse;

            private void DisposeReqRsp()
            {
                webResponse?.Dispose();
                webResponse = null;
                webRequest?.Abort();
                webRequest = null;
            }

            private System.IO.Stream requestStream;

            private void DisposeReqStream()
            {
                requestStream?.Dispose();
                requestStream = null;
            }

            private System.IO.Stream RequestStream
            {
                get
                {
                    if (responseStream != null)
                    {
                        DisposeRspStream();
                        DisposeReqRsp();
                    }
                    if (requestStream == null)
                    {
                        webRequest = GetRequest(FileName, System.Net.WebRequestMethods.Ftp.AppendFile);
                        if (webRequest == null) return null;
                        requestStream = GetRequestStream(webRequest);
                    }
                    return requestStream;
                }
            }

            private System.IO.Stream responseStream;

            private void DisposeRspStream()
            {
                responseStream?.Dispose();
                responseStream = null;
            }

            private System.IO.Stream ResponseStream
            {
                get
                {
                    if (requestStream != null)
                    {
                        DisposeReqStream();
                        DisposeReqRsp();
                    }
                    if (responseStream == null)
                    {
                        webRequest = GetRequest(FileName, System.Net.WebRequestMethods.Ftp.DownloadFile);
                        if (webRequest == null) return null;
                        webRequest.ContentOffset = Position;
                        webResponse = GetResponse(webRequest);
                        if (webResponse == null) return null;
                        responseStream = GetResponseStream(webResponse);
                    }
                    return responseStream;
                }
            }

            private string GetFiles(string dirUri, string matchName, out bool isFolder)
            {
                isFolder = false;
                if (dirUri == null) return null;
                System.IO.StreamReader streamReader = null;
                System.Net.FtpWebResponse ftpWebResponse = null;
                System.Net.FtpWebRequest ftpWebRequest = null;
                try
                {
                    if (!dirUri.EndsWith(slash)) dirUri += slash;
                    ftpWebRequest = GetRequest(dirUri, System.Net.WebRequestMethods.Ftp.ListDirectoryDetails);
                    ftpWebResponse = GetResponse(ftpWebRequest);
                    System.IO.Stream responseStream = ftpWebResponse.GetResponseStream();
                    streamReader = new System.IO.StreamReader(responseStream, true);
                    string mStr = null;
                    string line = null;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        MatchCollection matchCollection = MSDOSRegex.Matches(line);
                        if (matchCollection.Count > 0)
                        {
                            GroupCollection groupCollection = matchCollection[0].Groups;
                            if (groupCollection.Count == 4)
                            {
                                string mMark = groupCollection[2].Value;
                                string mName = groupCollection[3].Value;
                                if (mMark.Equals(DIRMark)) { isFolder = true; }
                                if (mName == matchName || (mName.Contains(matchName) && mName.EndsWith(extend)))
                                { mStr = mName; }
                            }
                        }
                        else { throw new System.Exception("ftp LIST only support MS-DOS(M) style."); }
                    }
                    return mStr;
                }
                catch (System.Net.WebException e)
                {
                    if (ftpWebResponse == null)
                        ftpWebResponse = e.Response as System.Net.FtpWebResponse;
                    switch (ftpWebResponse.StatusCode)
                    {
                        case System.Net.FtpStatusCode.ActionNotTakenFileUnavailable:
                        case System.Net.FtpStatusCode.ActionNotTakenFileUnavailableOrBusy:
                            return null;
                        default: throw new System.IO.IOException("get files faild." + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace);
                    }
                }
                finally
                {
                    streamReader?.Dispose();
                    ftpWebResponse?.Dispose();
                    ftpWebRequest?.Abort();
                }
            }

            private bool CreateFile(string fileUri)
            {
                if (fileUri == null) return false;
                string dirname = GetDirectoryName(fileUri);
                if (!CreateDir(dirname)) { return false; }
                System.Net.FtpWebRequest ftpWebRequest = null;
                System.Net.FtpWebResponse ftpWebResponse = null;
                try
                {
                    ftpWebRequest = GetRequest(fileUri, System.Net.WebRequestMethods.Ftp.AppendFile);
                    ftpWebResponse = GetResponse(ftpWebRequest);
                    return true;
                }
                catch (System.Net.WebException e)
                {
                    if (ftpWebResponse == null)
                        ftpWebResponse = e.Response as System.Net.FtpWebResponse;
                    switch (ftpWebResponse.StatusCode)
                    {
                        case System.Net.FtpStatusCode.ActionNotTakenFileUnavailable:
                        case System.Net.FtpStatusCode.ActionNotTakenFileUnavailableOrBusy:
                            return false;
                        default: throw new System.IO.IOException("create file faild." + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace);
                    }
                }
                finally
                {
                    ftpWebResponse?.Dispose();
                    ftpWebRequest?.Abort();
                }
            }

            private bool CreateDir(string dirUri)
            {
                if (dirUri == null) return false;
                if (hostRegex.IsMatch(dirUri)) return TestConnection(false);
                if (DirExists(dirUri)) return true;
                return CreateDir0(dirUri);
            }

            private bool CreateDir0(string dirUri)
            {
                System.Net.FtpWebRequest ftpWebRequest = null;
                System.Net.FtpWebResponse ftpWebResponse = null;
                try
                {
                    ftpWebRequest = GetRequest(dirUri, System.Net.WebRequestMethods.Ftp.MakeDirectory);
                    ftpWebResponse = GetResponse(ftpWebRequest);
                    return true;
                }
                catch (System.Net.WebException e)
                {
                    if (ftpWebResponse == null)
                        ftpWebResponse = e.Response as System.Net.FtpWebResponse;
                    switch (ftpWebResponse.StatusCode)
                    {
                        case System.Net.FtpStatusCode.ActionNotTakenFileUnavailable:
                            return CreateDir0(GetDirectoryName(dirUri)) && CreateDir0(dirUri);
                        default: break;
                    }
                    throw new System.IO.IOException("create dirctory faild." + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace);
                }
                finally
                {
                    ftpWebResponse?.Dispose();
                    ftpWebRequest?.Abort();
                }
            }

            private bool DirExists(string dirUri)
            {
                if (dirUri == null) return false;
                System.Net.FtpWebRequest ftpWebRequest = null;
                System.Net.FtpWebResponse ftpWebResponse = null;
                System.IO.StreamReader streamReader = null;
                try
                {
                    string dir = GetDirectoryName(dirUri);
                    ftpWebRequest = GetRequest(dir + slash, System.Net.WebRequestMethods.Ftp.ListDirectory);
                    ftpWebResponse = GetResponse(ftpWebRequest);
                    System.IO.Stream respStream = GetResponseStream(ftpWebResponse);
                    streamReader = new System.IO.StreamReader(respStream);
                    string name = GetFileName(dirUri);
                    string line = null;
                    while ((line = streamReader.ReadLine()) != null)
                    { if (line == name) { return true; } }
                    return false;
                }
                catch (System.Net.WebException e)
                {
                    if (ftpWebResponse == null)
                        ftpWebResponse = e.Response as System.Net.FtpWebResponse;
                    switch (ftpWebResponse.StatusCode)
                    {
                        case System.Net.FtpStatusCode.ActionNotTakenFileUnavailable:
                        case System.Net.FtpStatusCode.ActionNotTakenFileUnavailableOrBusy:
                            return false;
                        default: throw new System.IO.IOException("find dirctory faild." + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace);
                    }
                }
                finally
                {
                    streamReader?.Dispose();
                    ftpWebResponse?.Dispose();
                    ftpWebRequest?.Abort();
                }
            }

            private bool DeleteDir(string dirUri)
            {
                if (dirUri == null) return false;
                System.Net.FtpWebRequest ftpWebRequest = null;
                System.Net.FtpWebResponse ftpWebResponse = null;
                try
                {
                    ftpWebRequest = GetRequest(dirUri, System.Net.WebRequestMethods.Ftp.RemoveDirectory);
                    ftpWebResponse = GetResponse(ftpWebRequest);
                    return true;
                }
                catch (System.Exception e)
                { throw new System.IO.IOException("delete faild." + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace); }
                finally
                {
                    ftpWebResponse?.Dispose();
                    ftpWebRequest?.Abort();
                }
            }

            private bool DeleteFile(string fileUri)
            {
                if (fileUri == null) return false;
                System.Net.FtpWebRequest ftpWebRequest = null;
                System.Net.FtpWebResponse ftpWebResponse = null;
                try
                {
                    ftpWebRequest = GetRequest(fileUri, System.Net.WebRequestMethods.Ftp.DeleteFile);
                    ftpWebResponse = GetResponse(ftpWebRequest);
                    return true;
                }
                catch (System.Exception e)
                { throw new System.IO.IOException("delete faild." + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace); }
                finally
                {
                    ftpWebResponse?.Dispose();
                    ftpWebRequest?.Abort();
                }
            }

            private bool Rename(string fileUri, string toName)
            {
                if (fileUri == null || toName == null) return false;
                System.Net.FtpWebRequest ftpWebRequest = null;
                System.Net.FtpWebResponse ftpWebResponse = null;
                try
                {
                    ftpWebRequest = GetRequest(fileUri, System.Net.WebRequestMethods.Ftp.Rename);
                    if (ftpWebRequest == null) return false;
                    ftpWebRequest.RenameTo = toName;
                    ftpWebResponse = GetResponse(ftpWebRequest);
                    if (ftpWebResponse == null) return false;
                    return true;
                }
                catch (System.Exception e)
                { throw new System.IO.IOException("rename faild." + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace); }
                finally
                {
                    ftpWebResponse?.Dispose();
                    ftpWebRequest?.Abort();
                }
            }

            private long GetLength(string fileUri)
            {
                long length = 0;
                if (fileUri == null) return length;
                System.Net.FtpWebRequest ftpWebRequest = null;
                System.Net.FtpWebResponse ftpWebResponse = null;
                try
                {
                    ftpWebRequest = GetRequest(fileUri, System.Net.WebRequestMethods.Ftp.GetFileSize);
                    if (ftpWebRequest == null) return length;
                    ftpWebResponse = GetResponse(ftpWebRequest);
                    if (ftpWebResponse == null) return length;
                    length = ftpWebResponse.ContentLength;
                }
                catch (System.Exception e)
                { throw new System.IO.IOException("get length faild." + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace); }
                finally
                {
                    ftpWebResponse?.Dispose();
                    ftpWebRequest?.Abort();
                }
                return length;
            }

            private System.DateTime GetLastModified(string fileUri)
            {
                if (fileUri == null) return System.DateTime.MinValue;
                System.Net.FtpWebRequest ftpWebRequest = null;
                System.Net.FtpWebResponse ftpWebResponse = null;
                try
                {
                    ftpWebRequest = GetRequest(fileUri, System.Net.WebRequestMethods.Ftp.GetDateTimestamp);
                    if (ftpWebRequest == null) return System.DateTime.MinValue;
                    ftpWebResponse = GetResponse(ftpWebRequest);
                    if (ftpWebResponse == null) return System.DateTime.MinValue;
                    System.DateTime lastModified = ftpWebResponse.LastModified;
                    return lastModified;
                }
                catch (System.Exception e)
                { throw new System.IO.IOException("get last modified faild." + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace); }
                finally
                {
                    ftpWebResponse?.Dispose();
                    ftpWebRequest?.Abort();
                }
            }

            public override SortedList<string, FileSystemEntry> GetFileSystemEntries(bool topOnly)
            {
                if (!AsCollection) return null;
                SortedList<string, FileSystemEntry> result = null;
                GetFileSystemEntries0(ref result, FileName, topOnly);
                return result;
            }

            private readonly Regex MSDOSRegex = new Regex(@"([0-9]+-[0-9]+-[0-9]+\s+[0-9]+:[0-9]+[AP]M)\s+(\S+)\s+(.+)");

            private const string DIRMark = "<DIR>";

            private void GetFileSystemEntries0(ref SortedList<string, FileSystemEntry> result, string uri, bool topOnly)
            {
                System.IO.StreamReader streamReader = null;
                System.Net.FtpWebResponse ftpWebResponse = null;
                System.Net.FtpWebRequest ftpWebRequest = null;
                try
                {
                    if (!uri.EndsWith(slash)) uri += slash;
                    ftpWebRequest = GetRequest(uri, System.Net.WebRequestMethods.Ftp.ListDirectoryDetails);
                    ftpWebResponse = GetResponse(ftpWebRequest);
                    System.IO.Stream responseStream = ftpWebResponse.GetResponseStream();
                    streamReader = new System.IO.StreamReader(responseStream, true);
                    string line = null;
                    if (result == null) result = new SortedList<string, FileSystemEntry>();
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        MatchCollection matchCollection = MSDOSRegex.Matches(line);
                        if (matchCollection.Count > 0)
                        {
                            GroupCollection groupCollection = matchCollection[0].Groups;
                            if (groupCollection.Count == 4)
                            {
                                string mLastModified = groupCollection[1].Value;
                                string mMark = groupCollection[2].Value;
                                string mName = groupCollection[3].Value;
                                string mUri = uri + mName;
                                bool isFolder = false;
                                if (mMark.Equals(DIRMark)) { isFolder = true; }
                                result[mUri] = new FileSystemEntry()
                                {
                                    uri = mUri,
                                    name = mName,
                                    lastModified = Convert.ToDateTime(mLastModified),
                                    size = isFolder ? 0 : long.Parse(mMark),
                                    isFolder = isFolder
                                };
                                if (isFolder && !topOnly) { GetFileSystemEntries0(ref result, mUri, topOnly); }
                            }
                        }
                        else { throw new System.Exception("ftp LIST only support MS-DOS(M) style."); }
                    }
                    return;
                }
                catch { throw; }
                finally
                {
                    streamReader?.Dispose();
                    ftpWebResponse?.Dispose();
                    ftpWebRequest?.Abort();
                }
            }
        }
    }
}