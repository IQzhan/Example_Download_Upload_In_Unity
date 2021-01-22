namespace E.Net
{
    public class StandaloneDownloader : Downloader
    {
        public StandaloneDownloader() : base(new StandaloneStreamHandler(), new StandaloneTaskHandler()) { }
    }
}
