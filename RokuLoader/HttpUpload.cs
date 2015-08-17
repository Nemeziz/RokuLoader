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
    public static class HttpUpload
    {
        public static bool PostFile(string hostname, string username, string password, string filePath)
        {
            var success = false;

            if (!Uri.IsWellFormedUriString($"http://{hostname}/", UriKind.Absolute))
            {
                Console.WriteLine($"Invalid hostname \"{hostname}\"");
                return false;
            }

            var uri = new Uri($"http://{hostname}/plugin_install");

            Console.WriteLine($"Connecting to {hostname}...");
            if (!IsRokuReacheable(hostname)) return false;

            Console.WriteLine("Uploading {0}...", Path.GetFileName(filePath));
            try
            {
                var response = MultipartFormDataPost(uri, filePath, username, password);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseStream = response.GetResponseStream();
                    if (responseStream != null)
                        using (var reader = new StreamReader(responseStream, Encoding.UTF8))
                        {
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

        public static HttpWebResponse MultipartFormDataPost(Uri postUrl, string filePath, string username, string password)
        {
            var boundary = "-----------------------------" + Encoding.UTF8.GetBytes(DateTime.Now.Ticks.ToString());
            var contentType = "multipart/form-data; boundary=" + boundary;

            return PostForm(postUrl, username, password, contentType, GetMultipartFormData(
                new Dictionary<string, object>{
                    { "archive",new FileParameter(ReadFile(filePath), Path.GetFileName(filePath), "application/zip")},
                    {"mysubmit","Replace"}}, 
                boundary));
        }

        private static HttpWebResponse PostForm(Uri postUrl, string username, string password, string contentType, byte[] formData)
        {
            var httpWebRequest = WebRequest.Create(postUrl) as HttpWebRequest;
            if (httpWebRequest == null) throw new NullReferenceException("request is not a http request");

            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = contentType;
            httpWebRequest.UserAgent = "Mozilla/4.0 (MSIE 6.0; Windows NT 5.1)";
            httpWebRequest.CookieContainer = new CookieContainer();
            httpWebRequest.ContentLength = formData.Length;
            if(username != "none") httpWebRequest.Credentials = new NetworkCredential(username, password);

            using (var requestStream = httpWebRequest.GetRequestStream())
            {
                requestStream.Write(formData, 0, formData.Length);
                requestStream.Close();
            }
            return httpWebRequest.GetResponse() as HttpWebResponse;
        }

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

        public class FileParameter
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

        private static string StripHtml(string strHtml)
        {
            return Regex.Replace(strHtml, "<(.|\n)*?>", "");
        }

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
