namespace E.Data
{
    public abstract class StreamFactory
    {
        public abstract IStream GetStream(in System.Uri uri);
    }
}
