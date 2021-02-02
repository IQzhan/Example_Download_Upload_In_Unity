using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace E.Data
{
    public class WebDAVTest
    {
        //--------------WebDAV上传代码-----
        private void upload()
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://10.57.144.2/WebDAV/IMS對外教育訓練PPTnew.pdf");
            req.Credentials = new NetworkCredential("username", "password");//用户名,密码   //CredentialCache.DefaultCredentials使用默认的认证
            req.PreAuthenticate = true;
            req.Method = "PUT";
            req.AllowWriteStreamBuffering = true;

            // Retrieve request stream
            Stream reqStream = req.GetRequestStream();

            // Open the local file
            FileStream rdr = new FileStream("C://IMS對外教育訓練PPTnew.pdf", FileMode.Open);

            // Allocate byte buffer to hold file contents
            byte[] inData = new byte[4096];

            // loop through the local file reading each data block
            //  and writing to the request stream buffer
            int bytesRead = rdr.Read(inData, 0, inData.Length);
            while (bytesRead > 0)
            {
                reqStream.Write(inData, 0, bytesRead);
                bytesRead = rdr.Read(inData, 0, inData.Length);
            }

            rdr.Close();
            reqStream.Close();

            req.GetResponse();



            //也可以用以下的方式

            /*  System.Uri myURi = new System.Uri("http://10.57.144.2/WebDAV/hello.doc");
              FileStream inStream = File.OpenRead("C://timeTest.doc"); 
 
            WebRequest req = WebRequest.Create(myURi);
            req.Method = "PUT";
              req.Timeout = System.Threading.Timeout.Infinite;
 
              req.Credentials = CredentialCache.DefaultCredentials;
              Stream outStream = req.GetRequestStream();
 
              //CopyStream(inStream, outStream);
              byte[] inData = new byte[4096];
              int bytesRead = inStream.Read(inData, 0, inData.Length);
              while (bytesRead > 0)
              {
                  outStream.Write(inData, 0, bytesRead);
                  bytesRead = inStream.Read(inData, 0, inData.Length);
              }
              inStream.Close();
              outStream.Close();
              req.GetResponse();*/

        }

        //--------------WebDAV下载代码-----
        private void WebDAVGet_Click(object sender, EventArgs e)
        {
            System.Uri myURi = new System.Uri(@"http://10.57.144.2/WebDAV/hello.doc");
            string sfilePath = "C://hello.doc";

            WebRequest req = WebRequest.Create(myURi);
            req.Method = "GET";
            req.Timeout = System.Threading.Timeout.Infinite;
            req.Credentials = CredentialCache.DefaultCredentials;
            WebResponse res = req.GetResponse();
            Stream inStream = res.GetResponseStream();

            FileStream fs = new FileStream(sfilePath, FileMode.OpenOrCreate);

            byte[] inData = new byte[4096];
            int bytesRead = inStream.Read(inData, 0, inData.Length);
            while (bytesRead > 0)
            {
                fs.Write(inData, 0, bytesRead);
                bytesRead = inStream.Read(inData, 0, inData.Length);
            }

            fs.Close();

            inStream.Close();
        }


        //--------------WebDAV删除代码-----

        private void WebDAVDel_Click(object sender, EventArgs e)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(@"http://10.57.144.2/WebDAV/Powerpoint.pptx");
            //req.Credentials = new NetworkCredential("Administrator", "123456");
            req.PreAuthenticate = true;
            req.Method = "DELETE";
            //req.AllowWriteStreamBuffering = true;

            req.GetResponse();
        }


        //----新建文件夾----

        private void WebDAVNewFolder_Click(object sender, EventArgs e)
        {
            try
            {
                // Create the HttpWebRequest object.
                HttpWebRequest objRequest = (HttpWebRequest)HttpWebRequest.Create(@"http://10.57.144.2/WebDAV/new");
                // Add the network credentials to the request.
                objRequest.Credentials = new NetworkCredential("F3226142", "drm.123");//用户名,密码
                // Specify the method.
                objRequest.Method = "MKCOL";

                HttpWebResponse objResponse = (System.Net.HttpWebResponse)objRequest.GetResponse();

                // Close the HttpWebResponse object.
                objResponse.Close();

            }
            catch (Exception ex)
            {
                throw new Exception("Can't create the foder" + ex.ToString());
            }
        }
    }
}
