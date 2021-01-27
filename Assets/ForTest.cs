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
            //CreateDir(@"ftp://localhost/eatmyshit");
            //RenameFile(@"ftp://localhost/eatmyshit", @"suckmydick");
            //GetDirList(@"ftp://localhost/haha");
            //bool exi = DirExists(@"ftp://localhost/jiba/Postman-win64-8.0.2-Setup.exe");
            //Debug.LogError(exi);
            //string[] lists = GetFiles(@"ftp://localhost/");
            //for(int i = 0; i < lists.Length; i++)
            //{
            //    Debug.LogError(lists[i]);
            //}
            
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

        private readonly Regex dirListRegex = new Regex(@"(.+)\s+<DIR>\s+(.+)");

        private string[] GetFiles(string dirUri)
        {
            System.IO.StreamReader streamReader = null;
            System.Net.FtpWebResponse ftpWebResponse = null;
            System.Net.FtpWebRequest ftpWebRequest = null;
            try
            {
                if (!dirUri.EndsWith("/")) dirUri += "/";
                ftpWebRequest = GetRequest(dirUri, System.Net.WebRequestMethods.Ftp.ListDirectory);
                ftpWebResponse = GetResponse(ftpWebRequest);
                streamReader = new System.IO.StreamReader(ftpWebResponse.GetResponseStream(), true);
                string line = null;
                List<string> lines = new List<string>();
                while((line = streamReader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
                return lines.ToArray();
            }
            catch(System.Exception e)
            {
                DataProcessorDebug.LogException(e);
                return new string[0];
            }
            finally
            {
                streamReader?.Dispose();
                ftpWebResponse?.Dispose();
                ftpWebRequest?.Abort();
            }
        }

        private bool FileExists(string fileUri)
        {
            long length = GetLength(fileUri);
            if (length == -1) return false;
            return true;
        }

        private bool DirExists(string dirPath)
        {
            System.DateTime lastModified = GetLastModified(dirPath);
            if (lastModified == System.DateTime.MinValue) return false;
            return true;
        }

        private long GetLength(string fileUri)
        {
            System.Net.FtpWebRequest ftpWebRequest = null;
            System.Net.FtpWebResponse ftpWebResponse = null;
            long length = -1;
            try
            {
                ftpWebRequest = GetRequest(fileUri, System.Net.WebRequestMethods.Ftp.GetFileSize);
                ftpWebResponse = GetResponse(ftpWebRequest);
                length = ftpWebResponse.ContentLength;
            }
            catch (System.Exception e)
            {
                DataProcessorDebug.LogException(e);
            }
            finally
            {
                ftpWebResponse?.Dispose();
                ftpWebRequest?.Abort();
            }
            return length;
        }

        private System.DateTime GetLastModified(string fileUri)
        {
            System.Net.FtpWebRequest ftpWebRequest = null;
            System.Net.FtpWebResponse ftpWebResponse = null;
            try
            {
                ftpWebRequest = GetRequest(fileUri, System.Net.WebRequestMethods.Ftp.GetDateTimestamp);
                ftpWebResponse = GetResponse(ftpWebRequest);
                System.DateTime lastModified = ftpWebResponse.LastModified;
                return lastModified;
            }
            catch (System.Exception e)
            {
                DataProcessorDebug.LogException(e);
                return System.DateTime.MinValue;
            }
            finally
            {
                ftpWebResponse?.Dispose();
                ftpWebRequest?.Abort();
            }
        }

        private int timeout = 5 * 1000;

        private System.Net.FtpWebRequest GetRequest(string uri, string mathod)
        {
            System.Net.FtpWebRequest req = (System.Net.FtpWebRequest)System.Net.WebRequest.Create(uri);
            req.Method = mathod;
            req.Timeout = timeout;
            //req.ReadWriteTimeout = timeout;
            //req.Credentials = new System.Net.NetworkCredential(username, password);
            req.KeepAlive = false;
            req.UsePassive = false;
            req.UseBinary = true;
            return req;
        }

        private System.Net.FtpWebResponse GetResponse(System.Net.FtpWebRequest request)
        {
            return (System.Net.FtpWebResponse)request.GetResponse();
        }

        private System.IO.Stream GetRequestStream(System.Net.FtpWebRequest request)
        {
            return request.GetRequestStream();
        }

        private System.IO.Stream GetResponseStream(System.Net.FtpWebResponse response)
        {
            return response.GetResponseStream();
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