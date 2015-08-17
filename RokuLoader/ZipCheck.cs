// ------------------------------------------------------------------------------
// RokuLoader 1.0
// Copyright (C) 2015 Patrick Fournier
// http://github.com/patrick0xf/RokuLoader
// Under MIT License
// ------------------------------------------------------------------------------

using System;
using System.IO;

namespace RokuLoader
{
    public class ZipCheck
    {
        public static bool CheckSignature(string filepath)
        {
            try
            {
                const string signatureZip = "50-4B-03-04";
                const int signatureLength = 4;

                if (string.IsNullOrEmpty(filepath)) return false;
                if (string.IsNullOrEmpty(signatureZip)) return false;
                using (var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    if (fs.Length < signatureLength)
                        return false;
                    var signature = new byte[signatureLength];
                    var bytesRequired = signatureLength;
                    var index = 0;
                    while (bytesRequired > 0)
                    {
                        var bytesRead = fs.Read(signature, index, bytesRequired);
                        bytesRequired -= bytesRead;
                        index += bytesRead;
                    }
                    var actualSignature = BitConverter.ToString(signature);
                    return actualSignature == signatureZip;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
