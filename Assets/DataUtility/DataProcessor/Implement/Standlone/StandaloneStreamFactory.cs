using System;
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
            { return new HttpStream(uri); }
            else if(uri.Scheme == System.Uri.UriSchemeFtp)
            { return new FtpStream(uri); }
            else { return new WebStream(uri); }
        }
        
        private class FileStream : DataStream
        {
            public FileStream(in System.Uri uri) : base(uri) { }

            private string fileName;

            private System.IO.FileInfo fileInfo;

            private System.IO.Stream stream;

            private System.IO.Stream IoStream
            {
                get
                {
                    if(stream == null)
                    {
                        stream = System.IO.File.Open(FileName, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite);
                    }
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
                get { return FileName != null && !FileName.EndsWith(".downloading"); }
                set
                {
                    if (value && FileName != null && FileName.EndsWith(".downloading"))
                    {
                        System.DateTime lastTime = LastModified;
                        System.IO.File.Move(FileName, uri.LocalPath);
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
                        if (FileName.EndsWith(".downloading"))
                        {
                            System.Text.RegularExpressions.MatchCollection matchCollection = LastModifiedRegex.Matches(FileName);
                            if(matchCollection.Count > 0)
                            {
                                string val = matchCollection[0].Groups[1].Value;
                                if(long.TryParse(val, out long lastmodifiedTick))
                                {
                                    return new System.DateTime(lastmodifiedTick);
                                }
                            }
                        }
                        return fileInfo.LastWriteTime;
                    }
                    return System.DateTime.MinValue;
                }
                set
                {
                    if (FileName != null)
                    {
                        if (FileName.EndsWith(".downloading"))
                        {
                            fileInfo.MoveTo(uri.LocalPath + value.Ticks.ToString() + ".downloading");
                        }
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
            public HttpStream(in System.Uri uri) : base(uri) { }

            public override int Timeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override bool Exists => throw new NotImplementedException();

            public override bool Complete { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override long Length => throw new NotImplementedException();

            public override System.DateTime LastModified { get; set; }

            public override string Version => throw new NotImplementedException();

            public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
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

        private class FtpStream : DataStream
        {
            public FtpStream(in System.Uri uri) : base(uri) { }

            public override int Timeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override bool Exists => throw new NotImplementedException();

            public override bool Complete { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override long Length => throw new NotImplementedException();

            public override System.DateTime LastModified { get; set; }

            public override string Version => throw new NotImplementedException();

            public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
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

        private class WebStream : DataStream
        {
            public WebStream(in System.Uri uri) : base(uri) { }

            public override int Timeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override bool Exists => throw new NotImplementedException();

            public override bool Complete { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override long Length => throw new NotImplementedException();

            public override System.DateTime LastModified { get; set; }

            public override string Version => throw new NotImplementedException();

            public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
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
