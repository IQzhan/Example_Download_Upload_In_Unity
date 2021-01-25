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