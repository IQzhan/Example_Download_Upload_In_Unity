using E.Data;
using System.Collections;
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
            dataProcessor?.Tick();
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
            
        }

        private void TestDownload()
        {
            //cloneAsyncOperation = dataProcessor.Clone(
            //    @"http://localhost:4406/StreamingAssets/Data/files.data",
            //    @"file:///E:/Downloads/fuckme.data");

            //cloneAsyncOperation = dataProcessor.Clone(
            //    @"file:///E:/Downloads/Postman-win64-8.0.2-Setup.exe",
            //    @"file:///D:/Postman-win64-8.0.2-Setup.exe");

            //byte[] data = System.Text.Encoding.UTF8.GetBytes("草泥马傻逼吧？");
            //CloneAsyncOperation cloneAsyncOperation1 = dataProcessor.Clone(data, "file:///D:/eatshit.txt");

            //bool exi = System.IO.Directory.Exists("D:/");
            //Debug.LogError(exi);
            //string[] fileNames = System.IO.Directory.GetFiles
            //            (@"D:/", "*.*.downloading", System.IO.SearchOption.TopDirectoryOnly);
            //Debug.LogError(fileNames.Length);


            //IEnumerator<int> iv = Invoker();
            //while (iv.MoveNext())
            //{
            //    Debug.LogError(iv.Current);
            //}

            Debug.LogError(Data.Utility.BytesLengthToString(153134545454, "<n> <u>/s"));
        }

        IEnumerator<int> Invoker()
        {
            
            yield return 0;
            yield return 1;
            yield return 2;
            yield break;
        }

        private Stack<IEnumerable<int>> st = new Stack<IEnumerable<int>>();

        private SortedDictionary<int, IEnumerable<int>> dd = new SortedDictionary<int, IEnumerable<int>>();
        
        int count = 0;

        private static readonly Regex fileNameRegex = new Regex(@"(?:[/\\]+([^/\\]+)[/\\]*)$");

        public static string GetDirectoryName(string filePath)
        { return fileNameRegex.Replace(filePath, string.Empty); }

        public static string GetFileName(string filePath)
        { return fileNameRegex.Match(filePath).Groups[1].Value; }

        private bool FileExists(string fileUri)
        {
            if (fileUri == null) throw new System.ArgumentNullException("fileUri");
            System.Net.FtpWebRequest ftpWebRequest = null;
            System.Net.FtpWebResponse ftpWebResponse = null;
            try
            {
                ftpWebRequest = GetRequest(fileUri, System.Net.WebRequestMethods.Ftp.GetFileSize);
                ftpWebResponse = GetResponse(ftpWebRequest);
                return true;
            }
            catch (System.Net.WebException e)
            {
                if (ftpWebResponse == null)
                    ftpWebResponse = e.Response as System.Net.FtpWebResponse;
                switch (ftpWebResponse.StatusCode)
                {
                    case System.Net.FtpStatusCode.ActionNotTakenFileUnavailable:
                    case System.Net.FtpStatusCode.ActionNotTakenFilenameNotAllowed:
                        return false;
                    case System.Net.FtpStatusCode.ActionNotTakenFileUnavailableOrBusy:
                        return true;
                    default: throw e;
                }
            }
            finally
            {
                ftpWebResponse?.Dispose();
                ftpWebRequest?.Abort();
            }
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