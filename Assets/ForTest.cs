using E.Data;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
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
            Task task = Task.Run(() => 
            {
                StandaloneStreamFactory factory = null;
                DataStream dataStream = null;
                try
                {
                    System.Uri uri0 = new System.Uri(@"ftp://localhost/suckmydick/吃鸡吧.txt");
                    System.Uri uri1 = new System.Uri(@"ftp://localhost/吃鸡吧1.txt");
                    factory = new StandaloneStreamFactoryInstance();
                    dataStream = factory.GetStream(uri0);
                    dataStream.Timeout = 5 * 1000;
                    bool exists = dataStream.Exists;
                    Debug.LogError(exists);
                    //if (exists)
                    //{
                    //    bool deleted = dataStream.Delete();
                    //    Debug.LogError(deleted);
                    //}
                    dataStream.LastModified = System.DateTime.Now;
                    bool created = dataStream.Create();
                    Debug.LogError(created);
                    dataStream.Complete = true;
                    //string text = "我日你妈嗨";
                    //byte[] data = System.Text.ASCIIEncoding.UTF8.GetBytes(text);
                    //dataStream.Length = data.Length;
                    //dataStream.Write(data, 0, data.Length);
                    //dataStream.Complete = true;
                }
                catch(System.Exception e)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    dataStream?.Dispose();
                    factory?.Dispose();
                }
            });

        }

        private static readonly Regex fileNameRegex = new Regex(@"(?:[/\\]{0,1}(?:[^/\\\:\?\*\<\>\|]+[/\\])+([^/\\\:\?\*\<\>\|]+(?:\.[^/\\\:\?\*\<\>\|]+){0,1}))");

        public static string GetDirectoryName(string filePath)
        {
            return filePath.Substring(0, filePath.Length - GetFileName(filePath).Length - 1);
        }

        public static string GetFileName(string filePath)
        {
            return fileNameRegex.Match(filePath).Groups[1].Value;
        }

        private System.Net.FtpWebRequest GetRequest(string uri, string mathod)
        {
            System.Net.FtpWebRequest req = (System.Net.FtpWebRequest)System.Net.WebRequest.Create(uri);
            if (req == null) return null;
            req.Method = mathod;
            req.KeepAlive = false;
            req.UsePassive = false;
            req.UseBinary = true;
            return req;
        }

        private System.Net.FtpWebResponse GetResponse(System.Net.FtpWebRequest request)
        { return (System.Net.FtpWebResponse)request.GetResponse(); }

        private System.IO.Stream GetRequestStream(System.Net.FtpWebRequest request)
        { return request.GetRequestStream(); }

        private System.IO.Stream GetResponseStream(System.Net.FtpWebResponse response)
        { return response.GetResponseStream(); }

        private class StandaloneStreamFactoryInstance : StandaloneStreamFactory
        { }

        private void DrawProgress()
        {

        }

        private void OnDestroy()
        {
            dataProcessor.Dispose();
        }
    }
}