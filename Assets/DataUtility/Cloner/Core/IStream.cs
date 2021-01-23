namespace E.Data
{
    public interface IStream
    {
        /// <summary>
        /// host of uri
        /// </summary>
        string Host { get; }

        /// <summary>
        /// name of file, random for others
        /// </summary>
        string Name { get; }

        /// <summary>
        /// connection timeout
        /// </summary>
        int Timeout { get; set; }

        /// <summary>
        /// test connection and return true if successed, save the connection result
        /// </summary>
        bool Exists { get; }

        /// <summary>
        /// create file if not exists
        /// </summary>
        /// <returns></returns>
        bool Create();

        /// <summary>
        /// delete file if exists
        /// </summary>
        /// <returns></returns>
        bool Delete();

        /// <summary>
        /// true if data complete downloaded
        /// </summary>
        bool Complete { get; set; }

        /// <summary>
        /// length of data
        /// </summary>
        long Length { get; }

        /// <summary>
        /// last modified time milliseconds of data,
        /// if get nothing, use now milliseconds
        /// </summary>
        long LastModified { get; }

        /// <summary>
        /// use [LastModified] and [Length] to generate a [Version] if data complete downloaded,
        /// else use [Version] from file name like [filename.Version.downloading] if is not complete download,
        /// else null
        /// </summary>
        string Version { get; set; }
        
        /// <summary>
        /// current seek position,
        /// set: if data can be seek
        /// </summary>
        long Position { get; set; }
        
        /// <summary>
        /// true if data can be read
        /// </summary>
        bool CanRead { get; }

        /// <summary>
        /// true if data can be write
        /// </summary>
        bool CanWrite { get; }

        /// <summary>
        /// write data if CanWrite is true
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        void Write(byte[] buffer, int offset, int count);

        /// <summary>
        /// read data if CanRead is true
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        int Read(byte[] buffer, int offset, int count);

        /// <summary>
        /// close connection
        /// </summary>
        void Close();
    }
}
