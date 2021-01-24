using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E.Data
{
    public class StandaloneStreamFactory : DataStreamFactory
    {
        protected StandaloneStreamFactory() { }

        public override DataStream GetStream(in Uri uri)
        {
            if (uri.IsFile)
            {
                return new FileStream();
            }
            else
            {
                return new WebStream();
            }
        }

        private class FileStream : DataStream
        {
            public override string Host => throw new NotImplementedException();

            public override string Name => throw new NotImplementedException();

            public override int Timeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override bool Exists => throw new NotImplementedException();

            public override bool Complete { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override long Length => throw new NotImplementedException();

            public override long LastModified => throw new NotImplementedException();

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

        private class HttpStream : DataStream
        {
            public override string Host => throw new NotImplementedException();

            public override string Name => throw new NotImplementedException();

            public override int Timeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override bool Exists => throw new NotImplementedException();

            public override bool Complete { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override long Length => throw new NotImplementedException();

            public override long LastModified => throw new NotImplementedException();

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
            public override string Host => throw new NotImplementedException();

            public override string Name => throw new NotImplementedException();

            public override int Timeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override bool Exists => throw new NotImplementedException();

            public override bool Complete { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override long Length => throw new NotImplementedException();

            public override long LastModified => throw new NotImplementedException();

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
            public override string Host => throw new NotImplementedException();

            public override string Name => throw new NotImplementedException();

            public override int Timeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override bool Exists => throw new NotImplementedException();

            public override bool Complete { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override long Length => throw new NotImplementedException();

            public override long LastModified => throw new NotImplementedException();

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
