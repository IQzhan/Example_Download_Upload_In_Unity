using E.Data;
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
            string fileUri0 = "file:///E:/Downloads/jdk-8u271-windows-x64.exe";
            string fileUri1 = "file:///F:/jdk-8u271-windows-x64.exe";
            string fileUri2 = "file:///E:/Downloads/Windows10.iso";
            string fileUri3 = "file:///F:/Windows10.iso";

            string ftpUri0 = "ftp://localhost/";
            string ftpUri1 = "ftp://localhost/";

            //test 连续 断点
            //file to file
            cloneAsyncOperation = dataProcessor.Clone(fileUri2, fileUri3);
            cloneAsyncOperation.LoadData = false;
            cloneAsyncOperation.ForceTestConnection = true;
            cloneAsyncOperation.onClose += () =>
            {
                if (cloneAsyncOperation.IsProcessingComplete)
                {
                    if(cloneAsyncOperation.Data != null)
                    {
                        Debug.LogError(cloneAsyncOperation.Data.Length);
                    }
                }
            };
            
            //file to http
            //cloneAsyncOperation = dataProcessor.Clone("file:///E:/Downloads/jdk-8u271-windows-x64.exe", "file:///F:/jdk-8u271-windows-x64.exe");
            //file to ftp
            //cloneAsyncOperation = dataProcessor.Clone("file:///E:/Downloads/jdk-8u271-windows-x64.exe", "file:///F:/jdk-8u271-windows-x64.exe");

            //http to file

            //http to http

            //http to ftp

            //ftp to file
            //ftp to http
            //ftp to ftp


        }


        //private static readonly Regex fileNameRegex = new Regex(@"(?:[/\\]+([^/\\]+)[/\\]*)$");

        //public static string GetDirectoryName(string filePath)
        //{ return fileNameRegex.Replace(filePath, string.Empty); }

        //public static string GetFileName(string filePath)
        //{ return fileNameRegex.Match(filePath).Groups[1].Value; }

        //private bool FileExists(string fileUri)
        //{
        //    if (fileUri == null) throw new System.ArgumentNullException("fileUri");
        //    System.Net.FtpWebRequest ftpWebRequest = null;
        //    System.Net.FtpWebResponse ftpWebResponse = null;
        //    try
        //    {
        //        ftpWebRequest = GetRequest(fileUri, System.Net.WebRequestMethods.Ftp.GetFileSize);
        //        ftpWebResponse = GetResponse(ftpWebRequest);
        //        return true;
        //    }
        //    catch (System.Net.WebException e)
        //    {
        //        if (ftpWebResponse == null)
        //            ftpWebResponse = e.Response as System.Net.FtpWebResponse;
        //        switch (ftpWebResponse.StatusCode)
        //        {
        //            case System.Net.FtpStatusCode.ActionNotTakenFileUnavailable:
        //            case System.Net.FtpStatusCode.ActionNotTakenFilenameNotAllowed:
        //                return false;
        //            case System.Net.FtpStatusCode.ActionNotTakenFileUnavailableOrBusy:
        //                return true;
        //            default: throw e;
        //        }
        //    }
        //    finally
        //    {
        //        ftpWebResponse?.Dispose();
        //        ftpWebRequest?.Abort();
        //    }
        //}

        //private System.Net.FtpWebRequest GetRequest(string uri, string mathod)
        //{
        //    System.Net.FtpWebRequest req = (System.Net.FtpWebRequest)System.Net.WebRequest.Create(uri);
        //    if (req == null) return null;
        //    req.Method = mathod;
        //    req.KeepAlive = false;
        //    req.UsePassive = false;
        //    req.UseBinary = true;
        //    return req;
        //}

        //private System.Net.FtpWebResponse GetResponse(System.Net.FtpWebRequest request)
        //{ return (System.Net.FtpWebResponse)request.GetResponse(); }

        //private System.IO.Stream GetRequestStream(System.Net.FtpWebRequest request)
        //{ return request.GetRequestStream(); }

        //private System.IO.Stream GetResponseStream(System.Net.FtpWebResponse response)
        //{ return response.GetResponseStream(); }

        //private class StandaloneStreamFactoryInstance : StandaloneStreamFactory { }


    }
}