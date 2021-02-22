using E.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
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
            SortedList<string, DataStream.ResourceInfo> srl = GetDirectoryContents("http://localhost:4322/", true);
            //foreach(KeyValuePair<string, Resource> kvp in srl)
            //{ Debug.LogError(kvp.Value); }

        }

        private const string requestString =
            "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" +
            "<a:propfind xmlns:a=\"DAV:\">" +
            "<a:prop>" +
            "<a:displayname/>" +
            "<a:iscollection/>" +
            "<a:getlastmodified/>" +
            "</a:prop>" +
            "</a:propfind>";
        private static byte[] requestBytes = Encoding.ASCII.GetBytes(requestString);

        private const string PROPFIND = "PROPFIND";

        public static SortedList<string, DataStream.ResourceInfo> GetDirectoryContents(string url, bool deep)
        {
            HttpWebRequest webRequest = null;
            HttpWebResponse webResponse = null;
            try
            {
                webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.Credentials = CredentialCache.DefaultCredentials;
                webRequest.Method = PROPFIND;
                webRequest.Headers.Add("Translate: f");
                if (deep == true) { webRequest.Headers.Add("Depth: infinity"); }
                else { webRequest.Headers.Add("Depth: 1"); }
                webRequest.ContentLength = requestString.Length;
                webRequest.ContentType = "text/xml";
                Stream requestStream = webRequest.GetRequestStream();
                requestStream.Write(requestBytes, 0, requestBytes.Length);
                requestStream.Dispose();
                webResponse = (HttpWebResponse)webRequest.GetResponse();
                StreamReader streamReader = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8);
                string responseString = streamReader.ReadToEnd();
                streamReader.Dispose();
                XmlDocument XmlDoc = new XmlDocument();
                XmlDoc.LoadXml(responseString);
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(XmlDoc.NameTable);
                nsmgr.AddNamespace("a", "DAV:");
                XmlNodeList NameList = XmlDoc.SelectNodes("//a:prop/a:displayname", nsmgr);
                XmlNodeList isFolderList = XmlDoc.SelectNodes("//a:prop/a:iscollection", nsmgr);
                XmlNodeList LastModList = XmlDoc.SelectNodes("//a:prop/a:getlastmodified", nsmgr);
                XmlNodeList HrefList = XmlDoc.SelectNodes("//a:href", nsmgr);
                SortedList<string, DataStream.ResourceInfo> resourceList = new SortedList<string, DataStream.ResourceInfo>();
                for (int i = 0; i < NameList.Count; i++)
                {
                    string theUri = HrefList[i].InnerText;
                    resourceList.Add(theUri, new DataStream.ResourceInfo
                    {
                        uri = theUri,
                        name = NameList[i].InnerText,
                        isFolder = Convert.ToBoolean(Convert.ToInt32(isFolderList[i].InnerText)),
                        lastModified = Convert.ToDateTime(LastModList[i].InnerText)
                    });
                }
                return resourceList;
            }
            catch (WebException e)
            { Debug.LogException(e); return null; }
            finally
            {
                webResponse?.Dispose();
                webRequest?.Abort();
            }
        }

    }
}