﻿using E.Data;
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

            //test 连续 断点

            //file to file
            cloneAsyncOperation = dataProcessor.Clone("file:///E:/Downloads/Windows10.iso", "file:///F:/Windows10.iso");
            //file to http
            //file to ftp

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