using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RokuLoader
{
    public static class HttpUpload
    {
        static void PostFile(bool verbose, string hostname, string filePath)
        {
            var uri = new Uri($"http://{hostname}");
            PostFile(verbose, uri, filePath);
        }

        static void PostFile(bool verbose, string hostname, string username, string password, string filePath)
        {
            var uri = new Uri($"http://{hostname}:{username}@{password}");

        }

        static void PostFile(bool verbose, Uri uri, string filePath)
        {
            var webClient = new WebClient();
            var responseArray = webClient.UploadFile(uri, filePath);

            if (verbose)
            {
                Console.WriteLine(Encoding.ASCII.GetString(responseArray));
            }
        }
    }
}
