namespace E.Data
{
    public interface IStream
    {
        string Host { get; set; }
        int Timeout { get; set; }
        bool Exists { get; }
        bool Create();
        bool Delete();
        bool Complete { get; set; }
        string Version { get; set; }
        long Length { get; }
        long Position { get; set; }
        bool CanRead { get; }
        bool CanWrite { get; }
        void Write(byte[] buffer, int offset, int count);
        int Read(byte[] buffer, int offset, int count);
        void Close();
    }
}
