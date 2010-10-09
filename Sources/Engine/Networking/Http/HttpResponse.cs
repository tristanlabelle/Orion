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
        private static Regex headerRegex = new Regex(@"([^:]+):\s*(.+)", RegexOptions.Compiled);
        private static Regex responseCodeRegex = new Regex(@"HTTP/\d\.\d\s*(\d+)\s*(.+)", RegexOptions.Compiled);

        private int resultCode;
        private string resultString;
        private readonly Dictionary<string, string> responseHeaders = new Dictionary<string,string>();
        private string body;
        #endregion

        #region Constructors
        internal HttpResponse(HttpRequest request, Socket socket)
        {
            using (MemoryStream streamBuffer = new MemoryStream())
            {
                byte[] array = new byte[0x100];
                int read;
                bool hasData;
                do
                {
                    hasData = socket.Poll(1000, SelectMode.SelectRead);
                    read = socket.Receive(array);
                    streamBuffer.Write(array, 0, read);
                } while (!(hasData && read == 0));
                streamBuffer.Seek(0, SeekOrigin.Begin);

                StreamReader reader = new StreamReader(streamBuffer);
                string line = reader.ReadLine();
                int readChars = line.Length + 2;

                Match response = responseCodeRegex.Match(line.Trim());
                resultCode = int.Parse(response.Groups[1].Value);
                resultString = response.Groups[2].Value;

                while (true)
                {
                    line = reader.ReadLine();
                    readChars += line.Length + 2;
                    Match result = headerRegex.Match(line.Trim());
                    if (!result.Success) break;
                    responseHeaders[result.Groups[1].Value] = result.Groups[2].Value;
                }

                streamBuffer.Seek(readChars, SeekOrigin.Begin);
                ReadBody(streamBuffer);
            }
        }
        #endregion

        #region Properties
        public int ResultCode
        {
            get { return resultCode; }
        }

        public string ResultString
        {
            get { return resultString; }
        }

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
            int semicolon = contentType.IndexOf('=');
            if (semicolon == -1) return Encoding.GetEncoding("ISO-8859-1");
            return Encoding.GetEncoding(contentType.Substring(semicolon + 1).Trim());
        }

        private void ReadBody(MemoryStream stream)
        {
            string transferEncodingName = HttpEnumMethods.ToString(HttpResponseHeader.TransferEncoding);
            if (responseHeaders.ContainsKey(transferEncodingName))
            {
                string transferEncoding = responseHeaders[transferEncodingName];
                if (transferEncoding.ToUpper() == "CHUNKED")
                    ReadChunkedBody(stream);
            }
            else
            {
                StreamReader reader = new StreamReader(stream);
                body = reader.ReadToEnd();
            }
        }

        private void ReadChunkedBody(MemoryStream stream)
        {
            Encoding encoding = GetEncoding();
            byte[] charBuffer = new byte[0x100];
            BinaryReader reader = new BinaryReader(stream);
            StringBuilder bodyBuilder = new StringBuilder();
            int chunkSize;
            do
            {
                chunkSize = 0;
                byte currentByte = reader.ReadByte();
                while (!Char.IsWhiteSpace((char)currentByte))
                {
                    if (currentByte >= 'a') currentByte -= 'a' - 10;
                    if (currentByte >= 'A') currentByte -= 'A' - 10;
                    if (currentByte >= '0') currentByte -= (byte)'0';
                    chunkSize *= 16;
                    chunkSize += currentByte;
                    currentByte = reader.ReadByte();
                }
                if (chunkSize > charBuffer.Length) charBuffer = new byte[chunkSize];

                reader.ReadByte(); // skip one byte
                if (chunkSize > 0)
                {
                    reader.Read(charBuffer, 0, chunkSize);
                    reader.ReadInt16(); // skip 2 bytes
                    bodyBuilder.Append(encoding.GetString(charBuffer, 0, chunkSize));
                }
            } while (chunkSize != 0);
            body = bodyBuilder.ToString();
        }
        #endregion
        #endregion
    }
}
