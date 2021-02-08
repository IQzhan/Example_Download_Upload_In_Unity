using E.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
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

            WebClient webClient = new WebClient();

            //System.IO.Stream ss = webClient.OpenWrite("http://localhost:4322/haha.txt");
            //byte[] data = System.Text.Encoding.UTF8.GetBytes("食大雕");
            //ss.Write(data, 0, data.Length);
            //webClient.Dispose();
            //webClient.Credentials = new System.Net.NetworkCredential("admin", "123456");
            //webClient.UploadFile(@"http://localhost:4322/fuck.txt", "PUT", @"F:/fuck.txt");
            //try
            //{
            //    WriteFile("http://localhost:4322/haha.txt");
            //}
            //catch(System.Exception e)
            //{
            //    Debug.LogError(e.Message + System.Environment.NewLine + e.StackTrace);
            //}
            
            UploadFile("http://localhost:4322/fuck.txt", "F:/fuck.txt");
        }

        private string UploadFile(string url, string path)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.CookieContainer = new CookieContainer();
            request.AllowAutoRedirect = true;
            request.Method = "PUT";
            request.Credentials = new System.Net.NetworkCredential("admin", "123456");

            string boundary = DateTime.Now.Ticks.ToString("X"); // 随机分隔线
            request.ContentType = "multipart/form-data;charset=utf-8;boundary=" + boundary;
            byte[] itemBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");
            byte[] endBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");

            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] bArr = new byte[fs.Length];
            fs.Read(bArr, 0, bArr.Length);
            fs.Close();

            Stream postStream = request.GetRequestStream();
            postStream.Write(itemBoundaryBytes, 0, itemBoundaryBytes.Length);
            postStream.Write(bArr, 0, bArr.Length);
            postStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
            postStream.Close();

            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            Stream instream = response.GetResponseStream();
            StreamReader sr = new StreamReader(instream, Encoding.UTF8);
            string content = sr.ReadToEnd();
            return content;
        }

        private bool CreateFile(string fileUri)
        {
            System.Net.HttpWebRequest httpWebRequest = null;
            System.Net.HttpWebResponse httpWebResponse = null;
            try
            {
                httpWebRequest = System.Net.WebRequest.CreateHttp(fileUri);
                httpWebRequest.AllowAutoRedirect = true;
                httpWebRequest.Method = System.Net.WebRequestMethods.Http.Put;
                httpWebRequest.Credentials = new System.Net.NetworkCredential("admin", "123456");
                httpWebRequest.ContentLength = 0;
                httpWebResponse = httpWebRequest.GetResponse() as System.Net.HttpWebResponse;
                return true;
            }
            catch (System.Exception e)
            { throw new System.IO.IOException("create faild." + System.Environment.NewLine + e.Message + System.Environment.NewLine + e.StackTrace); }
            finally
            {
                httpWebResponse?.Dispose();
                httpWebRequest?.Abort();
            }
        }

        private bool WriteFile(string fileUri)
        {
            System.Net.HttpWebRequest httpWebRequest = null;
            System.Net.HttpWebResponse httpWebResponse = null;
            try
            {
                
                httpWebRequest = System.Net.WebRequest.CreateHttp(fileUri);
                httpWebRequest.AllowAutoRedirect = true;
                httpWebRequest.Method = "PUT";
                httpWebRequest.Credentials = new System.Net.NetworkCredential("admin", "123456");
                byte[] data = System.Text.Encoding.UTF8.GetBytes("食大雕");
                httpWebRequest.ContentLength = data.Length;
                System.IO.Stream stream = httpWebRequest.GetRequestStream();
                //stream.Write(data, 0, data.Length);
                //Debug.LogError(stream.Length);
                stream.Dispose();
                //httpWebResponse = httpWebRequest.GetResponse() as System.Net.HttpWebResponse;
                return true;
            }
            catch (System.Exception e)
            { throw new System.IO.IOException("create faild." + System.Environment.NewLine + e.Message + System.Environment.NewLine + e.StackTrace); }
            finally
            {
                httpWebResponse?.Dispose();
                httpWebRequest?.Abort();
            }
        }

        IEnumerator<int> Invoker()
        {
            
            yield return 0;
            yield return 1;
            yield return 2;
            yield break;
        }

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