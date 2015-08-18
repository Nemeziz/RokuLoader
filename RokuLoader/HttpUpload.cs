// ------------------------------------------------------------------------------
// RokuLoader 1.0
// Copyright (C) 2015 Patrick Fournier
// http://github.com/patrick0xf/RokuLoader
// Under MIT License
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace RokuLoader
{
    /// <summary>
    /// This class provides helper function for uploading a file to the Roku device's developer web server interface
    /// </summary>
    public static class HttpUpload
    {
        /// <summary>
        /// Posts the specified file to the Roku device's developer web server interface using the credentials provided
        /// </summary>
        /// <param name="hostname">The host name or IP address of the Roku device</param>
        /// <param name="username">The username for authenticating to the web server interface</param>
        /// <param name="password">The password for authenticating to the web interface</param>
        /// <param name="filePath">The full local path to the file</param>
        /// <returns>Returns true if the file was succesfully posted; otherwise returns false</returns>
        public static bool PostFile(string hostname, string username, string password, string filePath)
        {
            var success = false;

            //Check whether the hostname is valid
            if (!Uri.IsWellFormedUriString($"http://{hostname}/", UriKind.Absolute))
            {
                Console.WriteLine($"Invalid hostname \"{hostname}\"");
                return false;
            }

            var uri = new Uri($"http://{hostname}/plugin_install");

            //Check whether there is a web port opened at that address
            Console.WriteLine($"Connecting to {hostname}...");
            if (!IsRokuReacheable(hostname)) return false;

            //Uploads the file as part of a multi-part form data post operation
            Console.WriteLine("Uploading {0}...", Path.GetFileName(filePath));
            try
            {
                var response = MultipartFormDataPost(uri, username, password, filePath);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseStream = response.GetResponseStream();
                    if (responseStream != null)
                        using (var reader = new StreamReader(responseStream, Encoding.UTF8))
                        {
                            //Parse the body of the web response for display at the console (strip header and HTML tags)
                            var responseText = reader.ReadToEnd();
                            if (responseText.Contains("<center>"))
                            {
                                var shortResponse =
                                    responseText.Split(new[] {"<center>"}, StringSplitOptions.RemoveEmptyEntries)[1];
                                shortResponse = shortResponse.Replace("&nbsp;", " ");
                                var strippedResponse = StripHtml(shortResponse);
                                strippedResponse = strippedResponse.Replace("\n\n", "\n").Replace("\n\n", "\n");
                                strippedResponse = strippedResponse.Replace("\n\n", "\n");
                                Console.WriteLine(StripHtml(strippedResponse));
                            }
                            else
                            {
                                //Unknown message, display entire body instead
                                Console.WriteLine(responseText);
                            }

                        }
                    success = true;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
            return success;

        }

        /// <summary>
        /// Prepares the multi-part form data and post to the Roku device's developer web server interace
        /// </summary>
        /// <param name="postUrl">The Uri of the installation form on the Roku device</param>
        /// <param name="username">The username for authenticating to the web server interface</param>
        /// <param name="password">The password for authenticating to the web interface</param>
        /// <param name="filePath">The full local path to the file</param>
        /// <returns>Returns the http web response</returns>
        private static HttpWebResponse MultipartFormDataPost(Uri postUrl, string username, string password, string filePath)
        {
            var boundary = "-----------------------------" + Encoding.UTF8.GetBytes(DateTime.Now.Ticks.ToString());
            var contentType = "multipart/form-data; boundary=" + boundary;

            return PostForm(postUrl, username, password, contentType, GetMultipartFormData(
                new Dictionary<string, object>{
                    { "archive",new FileParameter(ReadFile(filePath), Path.GetFileName(filePath), "application/zip")},
                    {"mysubmit","Replace"}}, 
                boundary));
        }


        /// <summary>
        /// Post pre-constructed form data the Roku device's developer web server interace
        /// </summary>
        /// <param name="postUrl">The Uri of the installation form on the Roku device</param>
        /// <param name="username">The username for authenticating to the web server interface</param>
        /// <param name="password">The password for authenticating to the web interface</param>
        /// <param name="contentType">The content type associated with the file to post</param>
        /// <param name="formData">The pre-constructed form data</param>
        /// <returns></returns>
        private static HttpWebResponse PostForm(Uri postUrl, string username, string password, string contentType, byte[] formData)
        {
            var httpWebRequest = WebRequest.Create(postUrl) as HttpWebRequest;
            if (httpWebRequest == null) throw new NullReferenceException("request is not a http request");

            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = contentType;
            httpWebRequest.UserAgent = "Mozilla/4.0 (MSIE 6.0; Windows NT 5.1)";
            httpWebRequest.CookieContainer = new CookieContainer();
            httpWebRequest.ContentLength = formData.Length;

            //Older Roku firmware may not require a username and password
            //Disable by calling the   --username="none"   option
            if(username != "none") httpWebRequest.Credentials = new NetworkCredential(username, password);

            using (var requestStream = httpWebRequest.GetRequestStream())
            {
                requestStream.Write(formData, 0, formData.Length);
                requestStream.Close();
            }
            return httpWebRequest.GetResponse() as HttpWebResponse;
        }


        /// <summary>
        /// Constructs a form data byte array given the parameters to post to the form as a key/value pairs dictionary
        /// </summary>
        /// <param name="postParameters">A key/value pairs dictionary representing the form field name and values</param>
        /// <param name="boundary">A random but static form boundary</param>
        /// <returns></returns>
        private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
        {
            var stream = new MemoryStream();
            foreach (var keyValuePair in postParameters)
            {
                var value = keyValuePair.Value as FileParameter;

                if (value != null)
                {
                    var fileParameter = value;
                    var s = $"--{boundary}\r\nContent-Disposition: form-data; name=\"{keyValuePair.Key}\"; " + 
                            $"filename=\"{(fileParameter.FileName ?? keyValuePair.Key)}\";\r\n" + 
                            $"Content-Type: {(fileParameter.ContentType ?? "application/octet-stream")}\r\n\r\n";
                    stream.Write(Encoding.UTF8.GetBytes(s), 0, s.Length);
                    stream.Write(fileParameter.File, 0, fileParameter.File.Length);
                    stream.Write(Encoding.UTF8.GetBytes("\r\n"), 0, 2);
                }
                else
                {
                    var s = $"--{boundary}\r\nContent-Disposition: form-data; " + 
                            $"name=\"{keyValuePair.Key}\"\r\n\r\n{keyValuePair.Value}\r\n";
                    stream.Write(Encoding.UTF8.GetBytes(s), 0, s.Length);
                }
            }
            var s1 = "\r\n--" + boundary + "--\r\n";
            stream.Write(Encoding.UTF8.GetBytes(s1), 0, s1.Length);
            stream.Position = 0L;
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            stream.Close();
            return buffer;
        }

        /// <summary>
        /// Holds file parameter information
        /// </summary>
        private class FileParameter
        {
            public byte[] File { get; }
            public string FileName { get; }
            public string ContentType { get; }
            
            public FileParameter(byte[] file, string filename = null, string contenttype = null)
            {
                File = file;
                FileName = filename;
                ContentType = contenttype;
            }
        }

        /// <summary>
        /// Returns the content of a file as a byte array
        /// </summary>
        /// <param name="filePath">The full local path to the file</param>
        /// <returns></returns>
        private static byte[] ReadFile(string filePath)
        {
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            byte[] buffer;
            try
            {
                var length = (int)fileStream.Length;
                buffer = new byte[length];
                var offset = 0;
                int num;
                while ((num = fileStream.Read(buffer, offset, length - offset)) > 0) offset += num;
            }
            finally
            {
                fileStream.Close();
            }
            return buffer;
        }

        /// <summary>
        /// Strips HTML tags from a string
        /// </summary>
        /// <param name="strHtml"></param>
        /// <returns></returns>
        private static string StripHtml(string strHtml)
        {
            return Regex.Replace(strHtml, "<(.|\n)*?>", "");
        }

        /// <summary>
        /// Checks if a device at the host name specified is listening to port 80
        /// </summary>
        /// <param name="hostname">The host name or IP address of the Roku device</param>
        /// <returns></returns>
        private static bool IsRokuReacheable(string hostname)
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    socket.Connect(hostname, 80);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode != SocketError.ConnectionRefused &&
                        ex.SocketErrorCode != SocketError.TimedOut)
                        return true;
                    Console.WriteLine($"No Roku installer port responding at {hostname}");
                    return false;
                }
                finally
                {
                    socket.Close();
                }
            }

            return true;
        }
    }
}
