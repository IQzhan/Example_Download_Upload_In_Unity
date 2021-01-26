using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
                    string name = System.IO.Path.GetFileName(localPath);
                    string dir = System.IO.Path.GetDirectoryName(localPath);
                    string[] fileNames = System.IO.Directory.GetFiles
                        (dir, name + ".*.downloading", System.IO.SearchOption.TopDirectoryOnly);
                    if (fileNames.Length > 0) 
                    { fileName = fileNames[0]; fileInfo = null; fileInfo = new System.IO.FileInfo(fileName); }
                }
            }

            public override int Timeout { get => 0; set => _ = value; }

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

            public override long Length => FileName != null ? fileInfo.Length : 0;

            private static readonly System.Text.RegularExpressions.Regex LastModifiedRegex
                = new System.Text.RegularExpressions.Regex(@".+(?:\.([0-9]+)\.downloading)$");

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

            public override bool CanRead => true;

            public override bool CanWrite => FileName != null && !fileInfo.IsReadOnly;

            public override bool Delete()
            {
                if (FileName != null) fileInfo.Delete();
                return true;
            }

            public override bool Create()
            {
                if (FileName != null) 
                {
                    if (!fileInfo.Directory.Exists)
                    {
                        fileInfo.Directory.Create();
                    }
                    IoStream = fileInfo.Create();
                }
                return true;
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

            public override long Length => throw new NotImplementedException();

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

            public override bool CanRead => true;

            public override bool CanWrite => throw new NotImplementedException();

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

            public override bool Exists => throw new NotImplementedException();

            private bool TestConnection()
            {
                
                return true;
            }

            private string username;

            private string password;

            private System.Net.FtpWebRequest GetRequest(string uri, string mathod)
            {
                System.Net.FtpWebRequest req = System.Net.WebRequest.Create(uri) as System.Net.FtpWebRequest;
                req.Method = mathod;
                req.Credentials = new System.Net.NetworkCredential(username, password);
                req.KeepAlive = false;
                req.UsePassive = false;
                req.UseBinary = true;
                return req;
            }

            private System.IO.Stream requestStream;

            private System.IO.Stream RequestStream
            {
                get
                {
                    responseStream?.Dispose();
                    if (requestStream == null)
                    {

                    }
                    return requestStream;
                }
            }

            private System.IO.Stream responseStream;

            private System.IO.Stream ResponseStream
            {
                get
                {
                    requestStream?.Dispose();
                    if (responseStream == null)
                    {

                    }
                    return responseStream;
                }
            }

            public override bool Complete { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override long Length => throw new NotImplementedException();

            public override System.DateTime LastModified { get; set; }

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

            public override bool CanRead => throw new NotImplementedException();

            public override bool CanWrite => throw new NotImplementedException();

            public override bool Create()
            {
                throw new NotImplementedException();
            }

            public override bool Delete()
            {
                throw new NotImplementedException();
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
                throw new NotImplementedException();
            }

            protected override void ReleaseUnmanaged()
            {
                throw new NotImplementedException();
            }
        }
    }
}
