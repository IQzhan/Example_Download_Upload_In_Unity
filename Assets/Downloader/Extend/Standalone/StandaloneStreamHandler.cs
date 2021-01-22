using System;
using System.Text.RegularExpressions;

namespace E.Net
{
    public class StandaloneStreamHandler : DownloaderStreamHandler
    {
        static StandaloneStreamHandler()
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 512;
        }

        public override File GetFileImplement()
        {
            return new StandaloneFile();
        }

        public override Directory GetDirectoryImplement()
        {
            return new StandaloneDirectory();
        }

        public override WebRequest CreateWebRequestInstance(System.Uri uri)
        {
            return new StandaloneWebRequest(uri);
        }

        public class StandaloneStream : Stream
        {
            public StandaloneStream(System.IO.Stream ioStream)
            {
                this.ioStream = ioStream;
            }

            private System.IO.Stream ioStream;

            public override long Length 
            { 
                get => ioStream.Length;
            }
            
            public override long Position 
            { 
                get => ioStream.Position;
                set => ioStream.Position = value;
            }

            public override void Close()
            {
                if(ioStream != null)
                {
                    ioStream.Close();
                    ioStream = null;
                }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return ioStream.Read(buffer, offset, count);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                ioStream.Write(buffer, offset, count);
            }
        }

        public class StandaloneFile : File
        {
            public override void Delete(string path)
            {
                System.IO.File.Delete(path);
            }

            public override bool Exists(string path)
            {
                return System.IO.File.Exists(path);
            }

            public override long GetLength(string path)
            {
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(path);
                if(fileInfo != null && fileInfo.Exists)
                {
                    return fileInfo.Length;
                }
                return 0;
            }

            public override void Move(string from, string to)
            {
                System.IO.File.Move(from, to);
            }

            public override Stream Open(string path, FileMode fileMode, FileAccess fileAccess)
            {
                return new StandaloneStream(System.IO.File.Open(path, (System.IO.FileMode)fileMode, (System.IO.FileAccess)fileAccess));
            }
        }

        public class StandaloneDirectory : Directory
        {
            public override void CreateDirectory(string path)
            {
                DownloaderDebug.LogError(path);
                System.IO.Directory.CreateDirectory(path);
            }

            public override void Delete(string path, bool recursive)
            {
                System.IO.Directory.Delete(path, recursive);
            }

            public override bool Exists(string path)
            {
                return System.IO.Directory.Exists(path);
            }

            public override string[] GetDirectories(string path, string searchPattern, bool searchAll)
            {
                return System.IO.Directory.GetDirectories(path, searchPattern, 
                    searchAll ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly);
            }
        }

        public class StandaloneWebRequest : WebRequest
        {
            public StandaloneWebRequest(System.Uri uri)
            {
                this.uri = uri;
            }

            /// <summary>
            /// http file ftp
            /// </summary>
            private readonly System.Uri uri;

            private System.Net.HttpWebRequest webRequest;
            private System.Net.HttpWebResponse webResponse;
            private System.IO.Stream readStream;

            public override bool CreateRequest()
            {
                if (uri.IsFile)
                {
                    //TODO 
                    
                }
                webRequest = System.Net.WebRequest.CreateHttp(uri);
                return webRequest != null;
            }

            public override int Timeout { get => webRequest.Timeout; set => webRequest.Timeout = value; }

            public override void AddRange(long from, long to = -1)
            {
                if(to > from)
                {
                    webRequest.AddRange(from, to);
                }
                else
                {
                    webRequest.AddRange(from);
                }
            }

            public override bool GetResponse()
            {
                webResponse = webRequest.GetResponse() as System.Net.HttpWebResponse;
                return webResponse != null;
            }

            public override long ContentLength => webResponse.ContentLength;

            public override DateTime LastModified => webResponse.LastModified;

            public override int StatusCode => (int)webResponse.StatusCode;

            public override string StatusDescription => webResponse.StatusDescription;

            public override long TotalContentLength => GetTotalContentLength();

            public override bool StatusCodeSuccess => StatusCode == 200 || StatusCode == 206;

            private readonly Regex rangeRegex = new Regex(@"bytes\s[0-9]+-[0-9]+/+([0-9]+)");

            private long GetTotalContentLength()
            {
                System.Net.WebHeaderCollection header = webResponse.Headers;
                string value = header.Get("Content-Range");
                if (!string.IsNullOrWhiteSpace(value))
                {
                    MatchCollection matchCollection = rangeRegex.Matches(value);
                    if(matchCollection.Count > 0)
                    {
                        string numValue = matchCollection[0].Groups[1].Value;
                        if(long.TryParse(numValue, out long result))
                        { return result; }
                    }
                }
                return ContentLength;
            }

            public override string GetResponseHeader(string headerName)
            {
                System.Net.WebHeaderCollection header = webResponse.Headers;
                string value = header.Get(headerName);
                if (!string.IsNullOrWhiteSpace(value))
                { return value; }
                return null;
            }

            public override string GetETag()
            {
                return GetResponseHeader("E-Tag");
            }

            public override bool GetResponseStream()
            {
                readStream = webResponse.GetResponseStream();
                return readStream != null;
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return readStream.Read(buffer, offset, count);
            }

            public override void Close()
            {
                readStream?.Close();
                readStream = null;
                webResponse?.Close();
                webResponse = null;
                webRequest?.Abort();
                webRequest = null;
            }
        }
    }
}
