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
            //string fileUri0 = "file:///E:/Downloads/jdk-8u271-windows-x64.exe";
            //string fileUri1 = "file:///F:/Downloads0/jdk-8u271-windows-x64.exe";
            //string fileUri2 = "file:///E:/Downloads/Windows10.iso";
            //string fileUri3 = "file:///F:/Windows10.iso";

            //string httpUri0 = "http://localhost:4322/Downloads/jdk-8u271-windows-x64.exe";
            //string httpUri1 = "http://localhost:4322/Downloads0/jdk-8u271-windows-x64.exe";

            //string ftpUri0 = "ftp://localhost/Downloads/jdk-8u271-windows-x64.exe";
            //string ftpUri1 = "ftp://localhost/Downloads1/jdk-8u271-windows-x64.exe";
            //string ftpUri2 = "ftp://localhost/Downloads/Windows10.iso";

            //test 连续 断点
            //file to file
            //byte[] bts0 = System.Text.Encoding.UTF8.GetBytes("水水水水水水水ttttttttttt水水水水水水水水水水水水");
            //System.IO.Stream btsStream0 = new System.IO.MemoryStream(bts0);
            //string fileUri4 = "file:///F:/fuckme/eatmyshit.txt";
            //cloneAsyncOperation = dataProcessor.Clone(fileUri4);

            //file to http
            //byte[] bts = System.Text.Encoding.UTF8.GetBytes("日你妈海asdajsdhas dh ash diash diasgh ");
            //System.IO.Stream btStream = new System.IO.MemoryStream(bts);
            //string httpUri1 = "http://localhost:4322/hahoe/eieiei.txt";
            //cloneAsyncOperation = dataProcessor.Clone(httpUri1);

            //file to ftp
            //byte[] bts = System.Text.Encoding.UTF8.GetBytes("sdasd aads gadgf ads gfdg ");
            //cloneAsyncOperation = dataProcessor.Clone(bts, "ftp://localhost/Downloads/nothing.txt");

            //http to file
            //cloneAsyncOperation = dataProcessor.Clone(httpUri0, fileUri1);
            //http to http
            //cloneAsyncOperation = dataProcessor.Clone(httpUri0, httpUri1);
            //http to ftp
            //cloneAsyncOperation = dataProcessor.Clone(httpUri0, ftpUri1);

            //ftp to file
            //cloneAsyncOperation = dataProcessor.Clone(ftpUri0, fileUri1);
            //ftp to http
            //cloneAsyncOperation = dataProcessor.Clone(ftpUri0, httpUri1);
            //ftp to ftp
            //cloneAsyncOperation = dataProcessor.Clone(ftpUri0, ftpUri1);

            //cloneAsyncOperation.LoadData = false;
            //cloneAsyncOperation.ForceTestConnection = true;
            //cloneAsyncOperation.sourceAccount = new ConnectionAsyncOperation.Account()
            //{ username = "admin", password = "123456" };
            //cloneAsyncOperation.targetAccount = new ConnectionAsyncOperation.Account()
            //{ username = "admin", password = "123456" };
            //cloneAsyncOperation.onClose += () =>
            //{
            //    if (cloneAsyncOperation.IsProcessingComplete)
            //    {
            //        if (cloneAsyncOperation.Data != null)
            //        //{ Debug.LogError(System.Text.Encoding.UTF8.GetString(cloneAsyncOperation.Data)); }
            //        { Debug.LogError(cloneAsyncOperation.Data.Length); }
            //    }
            //};

        }

    }
}