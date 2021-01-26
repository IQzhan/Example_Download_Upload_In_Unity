using E.Data;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace E
{
    public class ForTest : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod]
        private static void Create()
        {
            DontDestroyOnLoad(new GameObject("ForTest").AddComponent<ForTest>());
        }

        private readonly System.Uri cacheUri = new System.Uri(@"E:\Downloads\");

        private DataProcessor dataProcessor;

        private void Awake()
        {
            Init();
            TestDownload();
        }

        private void Update()
        {
            dataProcessor.Tick();
            DrawProgress();
        }

        private void Init()
        {
            DataProcessorDebug.OverrideLog((string message) =>
            {
                Debug.Log(message);
            });
            DataProcessorDebug.OverrideLogError((string message) =>
            {
                Debug.LogError(message);
            });
            DataProcessorDebug.OverrideLogException((System.Exception exception) =>
            {
                Debug.LogException(exception);
            });
            dataProcessor = new StandaloneDataProcessor(cacheUri);

            dataProcessor.StartAsyncOperationGroup();
            
            dataProcessor.EndAsyncOperationGroup();
        }

        private void TestDownload()
        {
            //Task task = Task.Run(() =>
            //{
            //    try
            //    {
            //        HttpWebRequest webRequest = WebRequest.CreateHttp(@"http://localhost:4321/files/");
            //        //System.IO.Stream reqstream = httpreq.GetRequestStream();
            //        //webRequest.WriteTimeout = 5 * 1000;
            //        if (webRequest == null) return;
            //        WebResponse webResponse = webRequest.GetResponse();
            //        if (webResponse == null) return;
            //        Debug.Log(webResponse.Headers);
            //        System.IO.StreamReader respStream = new System.IO.StreamReader(webResponse.GetResponseStream());
            //        string text = respStream.ReadToEnd();
            //        Debug.Log(text);
            //    }
            //    catch (System.Exception e)
            //    {
            //        Debug.LogException(e);
            //    }

            //});


            //WebClient webClient = new WebClient();

            //webClient.UploadFile(@"http://localhost:4321/dir/fuckyou.txt", "PUT" ,@"E:/Downloads/fuckyou.txt");
            //Delete(new System.Uri(@"http://192.168.88.6:4321/files/dir/jiji.txt"));
            DoFTP(new System.Uri(@"ftp://localhost/eatmyshit"));
        }

        public bool Delete(System.Uri uri)
        {
            System.Net.HttpWebRequest testRequest = null;
            System.Net.HttpWebResponse testResponse = null;
            try
            {
                testRequest = System.Net.WebRequest.Create(uri) as System.Net.HttpWebRequest;
                if (testRequest == null) return false;
                testRequest.PreAuthenticate = true;
                testRequest.UseDefaultCredentials = false;
                testRequest.Credentials = new System.Net.NetworkCredential("fucker", "123456");
                testRequest.AllowAutoRedirect = true;
                testRequest.Method = "PUT";
                testRequest.Timeout = 5*1000;
                System.IO.Stream stream = testRequest.GetRequestStream();
                byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes("fuckyourmother");
                stream.Write(data, 0, data.Length);
                stream.Close();

                testResponse = testRequest.GetResponse() as System.Net.HttpWebResponse;
                if (testResponse == null) return false;
                if ((int)testResponse.StatusCode / 100 == 2) return true;
            }
            catch (System.Net.WebException e)
            {
                DataProcessorDebug.LogException(e);
                return false;
            }
            finally
            {
                testRequest?.Abort();
                testResponse?.Dispose();
            }
            return true;
        }

        private bool DoFTP(System.Uri uri)
        {
            System.Net.FtpWebRequest testRequest = null;
            System.Net.FtpWebResponse testResponse = null;
            try
            {
                testRequest = System.Net.WebRequest.Create(uri) as System.Net.FtpWebRequest;
                if (testRequest == null) return false;
                //testRequest.PreAuthenticate = true;
                //testRequest.UseDefaultCredentials = false;
                //testRequest.Credentials = new System.Net.NetworkCredential("fucker", "123456");
                testRequest.Method = System.Net.WebRequestMethods.Ftp.MakeDirectory;
                testRequest.Timeout = 5 * 1000;
                //System.IO.Stream stream = testRequest.GetRequestStream();
                //byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes("fuckyourmother");
                //stream.Write(data, 0, data.Length);
                //stream.Close();

                testResponse = testRequest.GetResponse() as System.Net.FtpWebResponse;
                if (testResponse == null) return false;
                if ((int)testResponse.StatusCode / 100 == 2) return true;
            }
            catch (System.Net.WebException e)
            {
                DataProcessorDebug.LogException(e);
                return false;
            }
            finally
            {
                testRequest?.Abort();
                testResponse?.Dispose();
            }
            return true;
        }

        private PathInfo[] GetFiles(string text)
        {

            return null;
        }

        public struct PathInfo
        {
            public bool isDir;
            public string name;
            public long length;
            public long time;
        }

        private void DrawProgress()
        {

        }

        private void OnDestroy()
        {
            dataProcessor.Dispose();
        }
    }
}