using E.Data;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace E
{
    public class ForTest : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod]
        private static void Create()
        {
            ForTest forTest = FindObjectOfType<ForTest>();
            if(forTest == null)
            { DontDestroyOnLoad(new GameObject("ForTest").AddComponent<ForTest>()); }
        }

        public UnityEngine.UI.Text consoleTextArea;

        private void OverridePrint(string message)
        {
            if (consoleTextArea != null)
            {
                consoleTextArea.text = message;
            }
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
            dataProcessor?.Tick();
            DrawProgress();
        }

        private readonly StringBuilder sb = new StringBuilder();

        private void DrawProgress()
        {
            if(cloneAsyncOperation != null && cloneAsyncOperation.Size > 0)
            {
                sb.Clear();
                sb.Append("progress: ");
                sb.Append(cloneAsyncOperation.Progress);
                sb.Append(System.Environment.NewLine);
                sb.Append("speed: ");
                sb.Append(Utility.FormatDataSize(cloneAsyncOperation.Speed, "<n><u>/s"));
                sb.Append(System.Environment.NewLine);
                sb.Append("reamain time: ");
                sb.Append(cloneAsyncOperation.RemainingTime);
                sb.Append(System.Environment.NewLine);
                sb.Append("size: ");
                sb.Append(Utility.FormatDataSize(cloneAsyncOperation.Size));
                sb.Append(System.Environment.NewLine);
                sb.Append("processed size: ");
                sb.Append(Utility.FormatDataSize(cloneAsyncOperation.ProcessedBytes));
                sb.Append(System.Environment.NewLine);
                OverridePrint(sb.ToString());
            }
        }

        private void OnDestroy()
        {
            dataProcessor?.Dispose();
        }

        private void Init()
        {
            DataProcessorDebug.OverrideLog((object message) =>
            {
                Debug.Log(message);
            });
            DataProcessorDebug.OverrideLogError((object message) =>
            {
                Debug.LogError(message);
            });
            DataProcessorDebug.OverrideLogException((System.Exception exception) =>
            {
                Debug.LogException(exception);
            }); 
            DataProcessorDebug.enableLog = true;
            DataProcessorDebug.enableLogError = true;
            DataProcessorDebug.enableLogException = true;
            dataProcessor = new StandaloneDataProcessor();
        }

        private void TestDownload()
        {
            string fileUri0 = "file:///E:/Downloads/jdk-8u271-windows-x64.exe";
            string fileUri1 = "file:///F:/fuckme/eatmyshit/jdk-8u271-windows-x64.exe";
            string fileUri2 = "file:///E:/Downloads/Windows10.iso";
            string fileUri3 = "file:///F:/Windows10.iso";

            string httpUri0 = "http://localhost:4322/hahoe/jdk-8u271-windows-x64.exe";

            string ftpUri0 = "ftp://localhost/";
            string ftpUri1 = "ftp://localhost/";

            //test 连续 断点
            //file to file
            //cloneAsyncOperation = dataProcessor.Clone(fileUri0, fileUri1);
            //cloneAsyncOperation.LoadData = true;
            //cloneAsyncOperation.ForceTestConnection = true;
            //cloneAsyncOperation.onClose += () =>
            //{
            //    if (cloneAsyncOperation.IsProcessingComplete)
            //    {
            //        if (cloneAsyncOperation.Data != null)
            //        { Debug.LogError(cloneAsyncOperation.Data.Length); }
            //    }
            //};

            //file to http
            cloneAsyncOperation = dataProcessor.Clone(fileUri0, httpUri0);
            cloneAsyncOperation.LoadData = false;
            cloneAsyncOperation.ForceTestConnection = true;
            cloneAsyncOperation.targetAccount = new ConnectionAsyncOperation.Account()
            { username = "admin", password = "123456" };
            cloneAsyncOperation.onClose += () =>
            {
                if (cloneAsyncOperation.IsProcessingComplete)
                {
                    if (cloneAsyncOperation.Data != null)
                    { Debug.LogError(cloneAsyncOperation.Data.Length); }
                }
            };

            //file to ftp
            //cloneAsyncOperation = dataProcessor.Clone("file:///E:/Downloads/jdk-8u271-windows-x64.exe", "file:///F:/jdk-8u271-windows-x64.exe");

            //http to file

            //http to http

            //http to ftp

            //ftp to file
            //ftp to http
            //ftp to ftp

            //username = "admin";
            //password = "123456";
            //string githubcom = "https://github.com/";
            //string localhttp = "http://localhost:4322/";
            //Debug.Log(ForceTestConnection(githubcom));
            //Debug.Log(ForceTestConnection(localhttp));
        }

        //string username;

        //string password;

        //private bool ForceTestConnection(string testUri)
        //{
        //    System.Net.HttpWebRequest testRequest = null;
        //    System.Net.HttpWebResponse testResponse = null;
        //    try
        //    {
        //        testRequest = GetRequest(testUri, System.Net.WebRequestMethods.Http.Head);
        //        testResponse = GetResponse(testRequest);
        //        return true;
        //    }
        //    catch (System.Net.WebException e)
        //    {
        //        DataProcessorDebug.LogError("cause an connection error at: " + testUri + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace);
        //        return false;
        //    }
        //    finally
        //    {
        //        testRequest?.Abort();
        //        testResponse?.Dispose();
        //    }
        //}

        //private System.Net.HttpWebRequest GetRequest(string uri, string mathod)
        //{
        //    System.Net.HttpWebRequest req = System.Net.WebRequest.Create(uri) as System.Net.HttpWebRequest;
        //    if (req == null) return null;
        //    req.Method = mathod;
        //    req.AllowAutoRedirect = true;
        //    if (username != null && password != null)
        //    { req.Credentials = new System.Net.NetworkCredential(username, password); }
        //    return req;
        //}

        //private System.Net.HttpWebResponse GetResponse(System.Net.HttpWebRequest request)
        //{ return request.GetResponse() as System.Net.HttpWebResponse; }

    }
}