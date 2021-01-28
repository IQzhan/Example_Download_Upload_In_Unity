using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace E.Data
{
    public class StandaloneStreamFactory : DataStreamFactory
    {
        protected StandaloneStreamFactory() { }

        public override DataStream GetStream(in Uri uri)
        {
            if (uri.IsFile)
            { return new FileStream(uri); }
            else if((uri.Scheme == System.Uri.UriSchemeHttp) || (uri.Scheme == System.Uri.UriSchemeHttp))
            { return new HttpStream(uri, this); }
            else if(uri.Scheme == System.Uri.UriSchemeFtp)
            { return new FtpStream(uri); }
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

        private class FileStream : DataStream
        {
            public FileStream(in System.Uri uri) : base(uri) { }

            private const string extend = @".downloading";

            private string fileName;

            private System.IO.FileInfo fileInfo;

            private System.IO.Stream stream;

            private System.IO.Stream IoStream
            {
                get
                {
                    if(stream == null)
                    { stream = System.IO.File.Open(FileName, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite); }
                    return stream;
                }
                set
                {
                    stream?.Dispose();
                    stream = null;
                    stream = value;
                }
            }

            private string FileName
            { get { if(fileName == null) { RefreshFileName(); } return fileName; } }

            private void RefreshFileName()
            {
                string localPath = uri.LocalPath;
                if (System.IO.File.Exists(localPath))
                { fileName = localPath; fileInfo = null; fileInfo = new System.IO.FileInfo(fileName); }
                else
                {
                    string name = GetFileName(localPath);
                    string dir = GetDirectoryName(localPath);
                    string[] fileNames = System.IO.Directory.GetFiles
                        (dir, name + ".*.downloading", System.IO.SearchOption.TopDirectoryOnly);
                    if (fileNames.Length > 0) 
                    { fileName = fileNames[0]; fileInfo = null; fileInfo = new System.IO.FileInfo(fileName); }
                }
            }

            private static readonly Regex fileNameRegex = new Regex(@"(?:[/\\]{0,1}(?:[^/\\\:\?\*\<\>\|]+[/\\])+([^/\\\:\?\*\<\>\|]+(?:\.[^/\\\:\?\*\<\>\|]+){0,1}))");

            public static string GetDirectoryName(string filePath)
            {
                return filePath.Substring(0, filePath.Length - GetFileName(filePath).Length - 1);
            }

            public static string GetFileName(string filePath)
            {
                return fileNameRegex.Match(filePath).Groups[1].Value;
            }

            public override void SetUser(string username, string password) { }

            public override int Timeout { get => 0; set { } }

            public override bool Exists
            { 
                get 
                {
                    if(FileName == null || (FileName != null && !System.IO.File.Exists(FileName)))
                    { RefreshFileName(); }
                    return fileName != null;
                } 
            }

            public override bool Complete
            {
                get { return FileName != null && !FileName.EndsWith(extend); }
                set
                {
                    if (value && FileName != null && FileName.EndsWith(extend))
                    {
                        System.DateTime lastTime = LastModified;
                        fileInfo.MoveTo(uri.LocalPath);
                        RefreshFileName();
                        fileInfo.LastWriteTime = lastTime;
                    }
                }
            }

            public override long Length { get { return  FileName != null ? fileInfo.Length : 0; } set { } }

            private static readonly System.Text.RegularExpressions.Regex LastModifiedRegex
                = new System.Text.RegularExpressions.Regex(@".+(?:\.([0-9]+)\.downloading)$");

            public System.DateTime lastModified = System.DateTime.MinValue;

            public override System.DateTime LastModified
            {
                get
                {
                    if(FileName != null)
                    {
                        if (FileName.EndsWith(extend))
                        {
                            System.Text.RegularExpressions.MatchCollection matchCollection = LastModifiedRegex.Matches(FileName);
                            if(matchCollection.Count > 0)
                            {
                                string val = matchCollection[0].Groups[1].Value;
                                if(long.TryParse(val, out long lastmodifiedTick))
                                { return new System.DateTime(lastmodifiedTick); }
                            }
                            return System.DateTime.MinValue;
                        }
                        else
                        { return fileInfo.LastWriteTime; }
                    }
                    return System.DateTime.MinValue;
                }
                set
                {
                    if (FileName != null)
                    {
                        if (FileName.EndsWith(extend))
                        { fileInfo.MoveTo(uri.LocalPath + value.Ticks.ToString() + extend); }
                        fileInfo.LastWriteTime = value;
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
                if (FileName != null) fileInfo.Delete();
                fileName = null;
                fileInfo = null;
                return true;
            }

            public override bool Create()
            {
                if (fileName == null && lastModified != System.DateTime.MinValue) 
                {
                    string localPath = uri.LocalPath;
                    string dir = GetDirectoryName(localPath);
                    if (!System.IO.Directory.Exists(dir)) { System.IO.Directory.CreateDirectory(dir); }
                    System.IO.File.Create(localPath + "." + lastModified.Ticks.ToString() + extend);
                    return true;
                }
                return false;
            }

            public override long Position { get => IoStream.Position; set => IoStream.Position = value; }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return IoStream.Read(buffer, offset, count);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                IoStream.Write(buffer, offset, count);
            }

            protected override void ReleaseManaged()
            {
                fileName = null;
                fileInfo = null;
            }

            protected override void ReleaseUnmanaged()
            {
                stream?.Dispose();
                stream = null;
            }
        }

        private class HttpStream : DataStream
        {
            /*GET
            通过请求URI得到资源
            POST,
            用于添加新的内容
            PUT
            用于修改某个内容
            DELETE,
            删除某个内容
            CONNECT,
            用于代理进行传输，如使用SSL
            OPTIONS
            询问可以执行哪些方法
            PATCH,
            部分文档更改
            PROPFIND, (wedav)
            查看属性
            PROPPATCH, (wedav)
            设置属性
            MKCOL, (wedav)
            创建集合（文件夹）
            COPY, (wedav)
            拷贝
            MOVE, (wedav)
            移动
            LOCK, (wedav)
            加锁
            UNLOCK (wedav)
            解锁
            TRACE
            用于远程诊断服务器*/

            public HttpStream(in System.Uri uri, StandaloneStreamFactory factory) : base(uri) { this.factory = factory; }

            private StandaloneStreamFactory factory;

            private int timeout;

            public override int Timeout { get => timeout; set => timeout = value; }

            private string username;

            private string password;

            public override void SetUser(string username, string password)
            {
                if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
                {
                    this.username = username;
                    this.password = password;
                }
            }

            public override bool Exists { get => TestConnection();}

            private bool TestConnection()
            { return TestHostConnection(uri.Host) && ForceTestConnection(uri.AbsoluteUri); }

            private bool TestHostConnection(string host)
            {
                lock (this)
                {
                    if (factory.testedHostConnection.ContainsKey(host))
                    {
                        return factory.testedHostConnection[host];
                    }
                    else
                    {
                        bool value = ForceTestConnection(host);
                        factory.testedHostConnection[host] = value;
                        return value;
                    }
                }
            }

            private bool ForceTestConnection(string host)
            {
                System.Net.HttpWebRequest testRequest = null;
                System.Net.HttpWebResponse testResponse = null;
                try
                {
                    testRequest = System.Net.WebRequest.Create(host) as System.Net.HttpWebRequest;
                    if (testRequest == null) return false;
                    testRequest.Method = System.Net.WebRequestMethods.Http.Head;
                    testRequest.Timeout = timeout;
                    testResponse = testRequest.GetResponse() as System.Net.HttpWebResponse;
                    if (testResponse == null) return false;
                    if ((int)testResponse.StatusCode / 100 == 2) return true;
                }
                catch (System.Net.WebException e)
                {
                    DataProcessorDebug.LogException(e);
                    return false;
                }
                finally
                {
                    testRequest?.Abort();
                    testResponse?.Dispose();
                }
                return true;
            }

            public override bool Complete { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            private long length = -1;

            private long contentLength;

            public override long Length 
            { get { return length; } set { contentLength = value; } }

            public override System.DateTime LastModified { get; set; }

            public override string Version
            {
                get
                {
                    System.DateTime lastTime = LastModified;
                    return lastTime != System.DateTime.MinValue ? lastTime.Ticks.ToString() : null;
                }
            }

            public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override bool Create()
            {
                return true;
            }

            public override bool Delete()
            {
                System.Net.HttpWebRequest testRequest = null;
                System.Net.HttpWebResponse testResponse = null;
                try
                {
                    testRequest = System.Net.WebRequest.Create(uri) as System.Net.HttpWebRequest;
                    if (testRequest == null) return false;
                    testRequest.Method = "DELETE";
                    testRequest.Timeout = timeout;
                    testResponse = testRequest.GetResponse() as System.Net.HttpWebResponse;
                    if (testResponse == null) return false;
                    if ((int)testResponse.StatusCode / 100 == 2) return true;
                }
                catch (System.Net.WebException e)
                {
                    DataProcessorDebug.LogException(e);
                    return false;
                }
                finally
                {
                    testRequest?.Abort();
                    testResponse?.Dispose();
                }
                return true;
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            protected override void ReleaseManaged()
            {
                factory = null;
            }

            protected override void ReleaseUnmanaged()
            {
                throw new NotImplementedException();
            }
        }

        private class FtpStream : DataStream
        {
            public FtpStream(in System.Uri uri) : base(uri) { }

            private int timeout;

            public override int Timeout { get => timeout; set => timeout = value; }

            private string username;

            private string password;

            public override void SetUser(string username, string password)
            {
                if(!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
                {
                    this.username = username;
                    this.password = password;
                }
            }

            //TODO 测试连接
            public override bool Exists
            { get { return RefreshFileName(); } }

            private const string extend = ".downloading";

            private static readonly Regex fileNameRegex = new Regex(@"(?:[/\\]{0,1}(?:[^/\\\:\?\*\<\>\|]+[/\\])+([^/\\\:\?\*\<\>\|]+(?:\.[^/\\\:\?\*\<\>\|]+){0,1}))");

            public static string GetDirectoryName(string filePath)
            {
                return filePath.Substring(0, filePath.Length - GetFileName(filePath).Length - 1);
            }

            public static string GetFileName(string filePath)
            {
                return fileNameRegex.Match(filePath).Groups[1].Value;
            }

            private string fileName;

            private string FileName
            { get { if (fileName == null) { RefreshFileName(); } return fileName; } }

            private bool RefreshFileName()
            {
                ResetFileTarget();
                string absUri = uri.AbsoluteUri;
                string fname = GetFileName(absUri);
                string dirUri = GetDirectoryName(absUri);
                string[] fileNames = GetFiles(dirUri);
                string fromArr = GetNameFromArray(fname, fileNames);
                if (fromArr != null) { fileName = dirUri + "/" + fromArr; }
                return fileName != null;
            }

            private string GetNameFromArray(string name, string[] strs)
            {
                for(int i = 0; i < strs.Length; i++)
                {
                    string comstr = strs[i];
                    if(comstr == name || (comstr.Contains(name) && comstr.EndsWith(extend)))
                    { return comstr; }
                }
                return null;
            }

            private void ResetFileTarget()
            {
                fileName = null;
                length = -1;
                position = 0;
            }

            public override bool Complete
            {
                get
                {
                    string fn = FileName;
                    return fn != null && !fn.EndsWith(extend);
                }
                set
                {
                    string fn = FileName;
                    if (value && fn != null && fn.EndsWith(extend))
                    {
                        if(!Rename(fn, GetFileName(uri.AbsoluteUri)))
                        { }
                        ResetFileTarget();
                    }
                }
            }

            private long length = -1;

            private long contentLength = 0;

            public override long Length
            {
                get
                {
                    if (length == -1) { length = GetLength(FileName); }
                    return length;
                }
                set { contentLength = value; }
            }

            private System.DateTime lastModified = System.DateTime.MinValue;

            private static readonly System.Text.RegularExpressions.Regex LastModifiedRegex
                = new System.Text.RegularExpressions.Regex(@".+(?:\.([0-9]+)\.downloading)$");

            public override System.DateTime LastModified
            {
                get
                {
                    string fn = FileName;
                    if (fn != null)
                    {
                        if (fn.EndsWith(extend))
                        {
                            System.Text.RegularExpressions.MatchCollection matchCollection = LastModifiedRegex.Matches(fn);
                            if (matchCollection.Count > 0)
                            {
                                string val = matchCollection[0].Groups[1].Value;
                                if (long.TryParse(val, out long lastmodifiedTick))
                                { return new System.DateTime(lastmodifiedTick); }
                            }
                            return System.DateTime.MinValue;
                        }
                        else
                        { return GetLastModified(fn); }
                    }
                    return System.DateTime.MinValue;
                }
                set
                {
                    string fn = FileName;
                    if (fn != null)
                    {
                        if (fn.EndsWith(extend))
                        { Rename(fn, GetFileName(uri.AbsoluteUri) + "." + value.Ticks.ToString() + extend); }
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
                if(DeleteFile(FileName))
                {
                    ResetFileTarget();
                    return true;
                }
                return false;
            }

            public override bool Create()
            {
                if(fileName == null && lastModified != System.DateTime.MinValue)
                {
                    return CreateFile(uri.AbsoluteUri + "." + lastModified.Ticks.ToString() + extend);
                }
                return false;
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return ResponseStream.Read(buffer, offset, count);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                RequestStream.Write(buffer, offset, count);
            }

            protected override void ReleaseManaged()
            {
                username = null;
                password = null;
                fileName = null;
            }

            protected override void ReleaseUnmanaged()
            {
                DisposeReqStream();
                DisposeRspStream();
                DisposeReqRsp();
            }

            private System.Net.FtpWebRequest GetRequest(string uri, string mathod)
            {
                System.Net.FtpWebRequest req = (System.Net.FtpWebRequest)System.Net.WebRequest.Create(uri);
                req.Method = mathod;
                req.Timeout = timeout;
                if(username != null && password != null)
                req.Credentials = new System.Net.NetworkCredential(username, password);
                req.KeepAlive = false;
                req.UsePassive = false;
                req.UseBinary = true;
                return req;
            }

            private System.Net.FtpWebResponse GetResponse(System.Net.FtpWebRequest request)
            { return (System.Net.FtpWebResponse)request.GetResponse(); }

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
                        webRequest = GetRequest(uri.AbsoluteUri, System.Net.WebRequestMethods.Ftp.AppendFile);
                        webRequest.ContentLength = contentLength;
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
                        webRequest = GetRequest(uri.AbsoluteUri, System.Net.WebRequestMethods.Ftp.DownloadFile);
                        webRequest.ContentOffset = position;
                        webResponse = GetResponse(webRequest);
                        responseStream = GetResponseStream(webResponse);
                    }
                    return responseStream;
                }
            }

            private string[] GetFiles(string dirUri)
            {
                if (dirUri == null) return new string[0];
                System.IO.StreamReader streamReader = null;
                System.Net.FtpWebResponse ftpWebResponse = null;
                System.Net.FtpWebRequest ftpWebRequest = null;
                try
                {
                    if (!dirUri.EndsWith("/")) dirUri += "/";
                    ftpWebRequest = GetRequest(dirUri, System.Net.WebRequestMethods.Ftp.ListDirectory);
                    ftpWebResponse = GetResponse(ftpWebRequest);
                    streamReader = new System.IO.StreamReader(ftpWebResponse.GetResponseStream(), true);
                    string line = null;
                    List<string> lines = new List<string>();
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        lines.Add(line);
                    }
                    return lines.ToArray();
                }
                catch (System.Exception e)
                {
                    DataProcessorDebug.LogException(e);
                    return new string[0];
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
                catch (System.Exception e)
                {
                    DataProcessorDebug.LogException(e);
                    return false;
                }
                finally
                {
                    ftpWebResponse?.Dispose();
                    ftpWebRequest?.Abort();
                }
            }

            private static readonly Regex hostRegex = new Regex(@"^(?:ftp://[^/\\]+)$");

            private bool CreateDir(string dirUri)
            {
                if (dirUri == null) return false;
                if (hostRegex.IsMatch(dirUri)) return true;
                string baseName = GetDirectoryName(dirUri);
                if (!CreateDir(baseName)) { return false; }
                if (!BasedDirExists(dirUri) && !BasedDirCreate(dirUri)) { return false; }
                return true;
            }

            private bool BasedDirExists(string dirUri)
            {
                if (dirUri == null) return false;
                System.Net.FtpWebRequest ftpWebRequest = null;
                System.Net.FtpWebResponse ftpWebResponse = null;
                System.IO.StreamReader streamReader = null;
                try
                {
                    string dir = GetDirectoryName(dirUri);
                    ftpWebRequest = GetRequest(dir, System.Net.WebRequestMethods.Ftp.ListDirectory);
                    ftpWebResponse = GetResponse(ftpWebRequest);
                    streamReader = new System.IO.StreamReader(GetResponseStream(ftpWebResponse));
                    string name = GetFileName(dirUri);
                    string line = null;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (line == name)
                        {
                            return true;
                        }
                    }
                    return false;
                }
                catch (System.Exception e)
                {
                    DataProcessorDebug.LogException(e);
                    return false;
                }
                finally
                {
                    streamReader?.Dispose();
                    ftpWebResponse?.Dispose();
                    ftpWebRequest?.Abort();
                }
            }

            private bool BasedDirCreate(string dirUri)
            {
                if (dirUri == null) return false;
                System.Net.FtpWebRequest ftpWebRequest = null;
                System.Net.FtpWebResponse ftpWebResponse = null;
                try
                {
                    ftpWebRequest = GetRequest(dirUri, System.Net.WebRequestMethods.Ftp.MakeDirectory);
                    ftpWebResponse = GetResponse(ftpWebRequest);
                    return true;
                }
                catch (System.Exception e)
                {
                    DataProcessorDebug.LogException(e);
                    return false;
                }
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
                {
                    DataProcessorDebug.LogException(e);
                    return false;
                }
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
                    ftpWebRequest.RenameTo = toName;
                    ftpWebResponse = GetResponse(ftpWebRequest);
                    return true;
                }
                catch (System.Exception e)
                {
                    DataProcessorDebug.LogException(e);
                    return false;
                }
                finally
                {
                    ftpWebResponse?.Dispose();
                    ftpWebRequest?.Abort();
                }
            }

            private long GetLength(string fileUri)
            {
                if (fileUri == null) return -1;
                System.Net.FtpWebRequest ftpWebRequest = null;
                System.Net.FtpWebResponse ftpWebResponse = null;
                long length = -1;
                try
                {
                    ftpWebRequest = GetRequest(fileUri, System.Net.WebRequestMethods.Ftp.GetFileSize);
                    ftpWebResponse = GetResponse(ftpWebRequest);
                    length = ftpWebResponse.ContentLength; 
                }
                catch (System.Exception e)
                {
                    DataProcessorDebug.LogException(e);
                }
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
                    ftpWebResponse = GetResponse(ftpWebRequest);
                    System.DateTime lastModified = ftpWebResponse.LastModified;
                    return lastModified;
                }
                catch (System.Exception e)
                {
                    DataProcessorDebug.LogException(e);
                    return System.DateTime.MinValue;
                }
                finally
                {
                    ftpWebResponse?.Dispose();
                    ftpWebRequest?.Abort();
                }
            }
        }
    }
}
