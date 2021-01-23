namespace E.Data
{
    public abstract class StreamFactory
    {
        public abstract IStream GetStream(System.Uri uri);
    }
}
