namespace E.Data
{
    public abstract class DataStreamFactory
    {
        public abstract DataStream GetStream(in System.Uri uri);
    }
}
