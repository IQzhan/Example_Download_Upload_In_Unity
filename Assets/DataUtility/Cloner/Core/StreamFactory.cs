namespace E.Data
{
    public class StreamFactory
    {
        public static IStream GetStream(System.Uri uri)
        {
            if (uri.IsFile)
            {
                //use IO
                string path = uri.LocalPath;
            }
            else
            {
                //use web request
                //if http https
                //else if ftp
                //else
            }
            return null;
        }
    }
}
