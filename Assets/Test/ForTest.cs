using E.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
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
        private DeleteAsyncOperation deleteAsyncOperation;
        private DirectoryAsyncOperation directoryAsyncOperation;

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
            sb.Clear();
            if (cloneAsyncOperation != null && cloneAsyncOperation.Size > 0)
            {
                sb.Append("clone:");
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
            }
            if (deleteAsyncOperation != null)
            {
                sb.Append("delete:");
                sb.Append("progress: ");
                sb.Append(deleteAsyncOperation.Progress);
                sb.Append(System.Environment.NewLine);
            }
            if(directoryAsyncOperation != null)
            {
                sb.Append("directory");
                sb.Append("progress: ");
                sb.Append(directoryAsyncOperation.Progress);
                sb.Append(System.Environment.NewLine);
            }
            OverridePrint(sb.ToString());
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

            //SortedList<string, DataStream.FileSystemEntry> dirs = ListDirectory("ftp://localhost/Downloads");
            //foreach (KeyValuePair<string, DataStream.FileSystemEntry> kv in dirs)
            //{ Debug.LogError(kv.Value); }

            //TODO test delete
            //file
            //deleteAsyncOperation = dataProcessor.Delete("E://Downloads/新建文件夹");
            //http
            //deleteAsyncOperation = dataProcessor.Delete("http://localhost:4322/新建文件夹");
            //deleteAsyncOperation.targetAccount = new ConnectionAsyncOperation.Account() { username = "admin", password = "123456" };
            //ftp
            //deleteAsyncOperation = dataProcessor.Delete("ftp://localhost/Downloads/新建文本文档.txt");
            //deleteAsyncOperation.targetAccount = new ConnectionAsyncOperation.Account() { username = "admin", password = "123456" };
            //TODO test Directory
            //directoryAsyncOperation = dataProcessor.GetFileSystemEntries(@"E://Downloads/");
            //directoryAsyncOperation = dataProcessor.GetFileSystemEntries(@"ftp://localhost/Downloads/");
            //directoryAsyncOperation = dataProcessor.GetFileSystemEntries(@"ftp://localhost/");
            //directoryAsyncOperation = dataProcessor.GetFileSystemEntries(@"G://");
            //directoryAsyncOperation = dataProcessor.GetFileSystemEntries(@"http://localhost:4322/Downloads0");
            //directoryAsyncOperation.onClose += (() =>
            //{
            //    if (directoryAsyncOperation.IsClosed)
            //    {
            //        SortedList<string, FileSystemEntry> fe = directoryAsyncOperation.Entries;
            //        if (fe != null)
            //        {
            //            foreach (KeyValuePair<string, FileSystemEntry> kv in fe) { Debug.LogError(kv.Key); }
            //        }
            //    }
            //});

            //TODO Compare

            //TODO group

            //TODO encode and decode

            //TODO android

            //Debug.LogError(Refresh("http://localhost:4322/Downloads"));

            //DeleteDir("ftp://localhost/suckmydick/新建文件夹/");

            

        }

        private bool DeleteDir(string dirUri)
        {
            if (dirUri == null) return false;
            System.Net.FtpWebRequest ftpWebRequest = null;
            System.Net.FtpWebResponse ftpWebResponse = null;
            try
            {
                ftpWebRequest = System.Net.WebRequest.Create(dirUri) as System.Net.FtpWebRequest;
                ftpWebRequest.Method = System.Net.WebRequestMethods.Ftp.RemoveDirectory;
                ftpWebRequest.Credentials = new System.Net.NetworkCredential("admin", "123456");
                ftpWebRequest.KeepAlive = false;

                ftpWebResponse = ftpWebRequest.GetResponse() as System.Net.FtpWebResponse;
                long size = ftpWebResponse.ContentLength;
                Stream datastream = ftpWebResponse.GetResponseStream();
                StreamReader sr = new StreamReader(datastream);
                string result = sr.ReadToEnd();
                sr.Dispose();
                Debug.LogError(result);
                return true;
            }
            catch (System.Exception e)
            { throw new System.IO.IOException("delete faild." + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace); }
            finally
            {
                ftpWebResponse?.Dispose();
                ftpWebRequest?.Abort();
            }
        }

        private bool Refresh(string uri)
        {
            System.Net.HttpWebRequest testRequest = null;
            System.Net.HttpWebResponse testResponse = null;
            try
            {
                testRequest = WebRequest.CreateHttp(uri);
                testRequest.Method = WebRequestMethods.Http.Head;
                testRequest.AllowAutoRedirect = true;
                testRequest.Credentials = new System.Net.NetworkCredential("admin", "123456");
                testResponse = testRequest.GetResponse() as System.Net.HttpWebResponse;
                long length = testResponse.ContentLength;
                Debug.LogError(length);
                DateTime lastModified = testResponse.LastModified;
                Debug.LogError(lastModified);
                return true;
            }
            catch (System.Net.WebException e)
            {
                if (testResponse == null)
                    testResponse = e.Response as System.Net.HttpWebResponse;
                switch (testResponse.StatusCode)
                {
                    case System.Net.HttpStatusCode.NotFound:
                        break;
                    default:
                        throw new System.IO.IOException("cause an connection error at: " + uri + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace);
                }
                return false;
            }
            finally
            {
                testRequest?.Abort();
                testResponse?.Dispose();
            }
        }

        //private static readonly Regex fileNameRegex = new Regex(@"(?:[/\\]+([^/\\]+)[/\\]*)$");

        //private static string GetDirectoryName(string filePath)
        //{ return fileNameRegex.Replace(filePath, string.Empty); }

        //private static string GetFileName(string filePath)
        //{ return fileNameRegex.Match(filePath).Groups[1].Value; }

        //public SortedList<string, DataStream.FileSystemEntry> GetFileSystemEntries(string fileName, bool topOnly = false)
        //{
        //    string[] entries = System.IO.Directory.GetFileSystemEntries(fileName, "*", topOnly ? System.IO.SearchOption.TopDirectoryOnly : System.IO.SearchOption.AllDirectories);
        //    if (entries != null && entries.Length > 0)
        //    {
        //        SortedList<string, DataStream.FileSystemEntry> infos = new SortedList<string, DataStream.FileSystemEntry>();
        //        for (int i = 0; i < entries.Length; i++)
        //        {
        //            string entry = entries[i];
        //            string name = GetFileName(entry);
        //            bool isFolder = System.IO.Directory.Exists(entry);
        //            infos[entry] = new DataStream.FileSystemEntry()
        //            {
        //                uri = entry,
        //                name = name,
        //                isFolder = isFolder,
        //                lastModified = System.IO.Directory.GetLastWriteTime(entry)
        //            };
        //        }
        //        return infos;
        //    }
        //    return null;
        //}

        //public SortedList<string, DataStream.FileSystemEntry> ListDirectory(string uri, bool topOnly = false)
        //{
        //    SortedList<string, DataStream.FileSystemEntry> result = null;
        //    ListDirectory0(ref result, uri, topOnly);
        //    return result;
        //}

        //private const string slash = "/";

        //private readonly Regex MSDOSRegex = new Regex(@"([0-9]+-[0-9]+-[0-9]+\s+[0-9]+:[0-9]+[AP]M)\s+(\S+)\s+(.+)");

        //private const string DIRMark = "<DIR>";

        //private void ListDirectory0(ref SortedList<string, DataStream.FileSystemEntry> result, string uri, bool topOnly)
        //{
        //    System.IO.StreamReader streamReader = null;
        //    System.Net.FtpWebResponse ftpWebResponse = null;
        //    System.Net.FtpWebRequest ftpWebRequest = null;
        //    try
        //    {
        //        if (!uri.EndsWith(slash)) uri += slash;
        //        ftpWebRequest = GetRequest(uri, System.Net.WebRequestMethods.Ftp.ListDirectoryDetails);
        //        ftpWebResponse = GetResponse(ftpWebRequest);
        //        System.IO.Stream responseStream = ftpWebResponse.GetResponseStream();
        //        streamReader = new System.IO.StreamReader(responseStream, true);
        //        string line = null;
        //        if (result == null) result = new SortedList<string, DataStream.FileSystemEntry>();
        //        while ((line = streamReader.ReadLine()) != null)
        //        {
        //            MatchCollection matchCollection = MSDOSRegex.Matches(line);
        //            if (matchCollection.Count > 0)
        //            {
        //                GroupCollection groupCollection = matchCollection[0].Groups;
        //                if (groupCollection.Count == 4)
        //                {
        //                    string mLastModified = groupCollection[1].Value;
        //                    string mMark = groupCollection[2].Value;
        //                    string mName = groupCollection[3].Value;
        //                    string mUri = uri + mName;
        //                    bool isFolder = false;
        //                    if (mMark.Equals(DIRMark)) { isFolder = true; }
        //                    result[mUri] = new DataStream.FileSystemEntry()
        //                    {
        //                        uri = mUri,
        //                        name = mName,
        //                        lastModified = Convert.ToDateTime(mLastModified),
        //                        isFolder = isFolder
        //                    };
        //                    if (isFolder && !topOnly) { ListDirectory0(ref result, mUri, topOnly); }
        //                }
        //            }
        //            else { throw new System.Exception("ftp LIST only support MS-DOS(M) style."); }
        //        }
        //        return;
        //    }
        //    catch { throw; }
        //    finally
        //    {
        //        streamReader?.Dispose();
        //        ftpWebResponse?.Dispose();
        //        ftpWebRequest?.Abort();
        //    }
        //}

        //private string username;

        //private string password;

        private System.Net.FtpWebRequest GetRequest(string uri, string mathod)
        {
            System.Net.FtpWebRequest req = System.Net.WebRequest.Create(uri) as System.Net.FtpWebRequest;
            req.Method = mathod;
            req.Credentials = new System.Net.NetworkCredential("admin", "123456");
            req.KeepAlive = false;
            req.UsePassive = false;
            req.UseBinary = true;
            return req;
        }

        private System.Net.FtpWebResponse GetResponse(System.Net.FtpWebRequest request)
        { return request.GetResponse() as System.Net.FtpWebResponse; }
    }
}