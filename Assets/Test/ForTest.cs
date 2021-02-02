using E.Data;
using System.Collections.Generic;
using System.IO;
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

        private DataProcessor dataProcessor;

        private CloneAsyncOperation cloneAsyncOperation;

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

        private void DrawProgress()
        {
            if(cloneAsyncOperation != null)
            {
                Debug.LogError(cloneAsyncOperation.Progress);
            }
        }

        private void OnDestroy()
        {
            dataProcessor.Dispose();
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
            dataProcessor = new StandaloneDataProcessor();
        }

        private void TestDownload()
        {
            dataProcessor.CacheSize = 1024;
            cloneAsyncOperation = dataProcessor.Clone(
                @"http://localhost:4406/StreamingAssets/Data/files.data",
                @"file:///E:/Downloads/fuckme.data");
            


            //Task task = Task.Run(() => 
            //{
            //    StandaloneStreamFactory factory = null;
            //    DataStream dataStream = null;
            //    try
            //    {
            //        //System.Uri uri0 = new System.Uri(@"ftp://localhost/suckmydick/吃鸡吧.txt");
            //        //System.Uri uri1 = new System.Uri(@"ftp://localhost/吃鸡吧1.txt");
            //        //System.Uri uri2 = new System.Uri(@"file:///E:/Downloads/fuckyou.txt");
            //        //System.Uri uri3 = new System.Uri(@"http://localhost:4406/StreamingAssets/Data/files.data");
            //        //factory = new StandaloneStreamFactoryInstance();
            //        //dataStream = factory.GetStream(uri3);
            //        //dataStream.Timeout = 5 * 1000;
            //        //bool exists = dataStream.Exists;
            //        //Debug.LogError(exists);
            //        //long length = dataStream.Length;
            //        //Debug.LogError(length);

            //        //byte[] data = new byte[length];
            //        //dataStream.Read(data, 0, data.Length);
            //        //string str = System.Text.ASCIIEncoding.UTF8.GetString(data);
            //        //Debug.LogError(str);
            //        //if (exists)
            //        //{
            //        //    bool deleted = dataStream.Delete();
            //        //    Debug.LogError(deleted);
            //        //}
            //        //dataStream.LastModified = System.DateTime.Now;
            //        //bool created = dataStream.Create();
            //        //Debug.LogError(created);
            //        //dataStream.Complete = true;


            //        //string text0 = "我日你妈嗨";
            //        //byte[] data0 = System.Text.ASCIIEncoding.UTF8.GetBytes(text0);
            //        //dataStream.Write(data0, 0, data0.Length);
            //        //dataStream.Complete = true;

            //        //long 长度 = dataStream.Length;
            //        //Debug.LogError(长度);
            //        //byte[] data = new byte[长度];
            //        //dataStream.Read(data, 0, (int)长度);
            //        //string text = System.Text.ASCIIEncoding.UTF8.GetString(data);
            //        //Debug.LogError(text);

            //    }
            //    catch (System.Exception e)
            //    {
            //        Debug.LogException(e);
            //    }
            //    finally
            //    {
            //        dataStream?.Dispose();
            //        factory?.Dispose();
            //    }
            //});

        }

        private IEnumerable<string> Invoker()
        {
            yield return null;
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

        private class StandaloneStreamFactoryInstance : StandaloneStreamFactory { }

    }
}