using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;

namespace Orion.Engine.Networking.Http
{
    public class HttpResponse
    {
        #region Fields
        private static Regex headerRegex = new Regex(@"([^:]+)\s*(.+))", RegexOptions.Compiled);

        private readonly Dictionary<string, string> responseHeaders = new Dictionary<string,string>();
        private string body;
        #endregion

        #region Constructors
        internal HttpResponse(HttpRequest request, NetworkStream stream)
        {
            StreamReader reader = new StreamReader(stream);
            string line = reader.ReadLine();
            while (line != HttpRequest.crlf)
            {
                Match result = headerRegex.Match(line.Trim());
                responseHeaders[result.Groups[0].Value] = result.Groups[1].Value;
            }

            ReadBody(stream);
        }
        #endregion

        #region Properties
        public string Body
        {
            get { return body; }
        }
        #endregion

        #region Methods
        #region Public
        public string GetResponseHeader(HttpResponseHeader header)
        {
            return GetResponseHeader(HttpEnumMethods.ToString(header));
        }

        public string GetResponseHeader(string header)
        {
            if (!responseHeaders.ContainsKey(header)) return null;
            return responseHeaders[header];
        }
        #endregion

        #region Private
        private Encoding GetEncoding()
        {
            string contentType = responseHeaders[HttpEnumMethods.ToString(HttpRequestHeader.ContentType)];
            int semicolon = contentType.IndexOf(';');
            if (semicolon == -1) return Encoding.GetEncoding("ISO-8859-1");
            return Encoding.GetEncoding(contentType.Substring(semicolon + 1).Trim());
        }

        private void ReadBody(NetworkStream stream)
        {
            StreamReader reader = new StreamReader(stream, GetEncoding());
            string transferEncodingName = HttpEnumMethods.ToString(HttpResponseHeader.TransferEncoding);
            if (responseHeaders.ContainsKey(transferEncodingName))
            {
                string encodingType = responseHeaders[transferEncodingName];
                if (encodingType.ToUpper() == "CHUNKED")
                {
                    ReadChunkedBody(reader);
                    return;
                }
            }
            body = reader.ReadToEnd();
        }

        private void ReadChunkedBody(StreamReader reader)
        {
            StringBuilder bodyBuilder = new StringBuilder();
            int length = 0;
            byte[] array = new byte[0x1000];
            do
            {
                length = int.Parse(reader.ReadLine());
                if (array.Length < length) array = new byte[length];
                reader.BaseStream.Read(array, 0, length);
                bodyBuilder.Append(reader.CurrentEncoding.GetString(array, 0, length));
            } while (length != 0);
            body = bodyBuilder.ToString();
        }
        #endregion
        #endregion
    }
}
