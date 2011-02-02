using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace Orion.Engine.Networking.Http
{
    public class HttpRequest
    {
        #region Fields
        private const string formMimeType = "application/x-www-form-urlencoded";
        private static readonly Dictionary<string, string> emptyFields = new Dictionary<string, string>();
        private static readonly Regex headerValueValidator = new Regex(@"^([^\r\n]+)$", RegexOptions.Compiled);
        private static readonly Regex headerNameValidator = new Regex(@"^([a-zA-Z\-_]+)$", RegexOptions.Compiled);
        private static readonly Regex charsToEscape = new Regex(@"[$&+,/:;=?@ <>#%{}|\\^~\[\]`]", RegexOptions.Compiled);
        private static readonly string crlf = "\r\n";

        private readonly Dictionary<string, string> headers = new Dictionary<string, string>();
        private readonly IPv4EndPoint hostEndPoint;
        private readonly Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        #endregion

        #region Constructors
        public HttpRequest(string hostNameOrAddress)
            : this(hostNameOrAddress, 80)
        { }

        public HttpRequest(string hostNameOrAddress, ushort port)
        {
            IPAddress[] addresses = Dns.GetHostAddresses(hostNameOrAddress);
            if (addresses.Length == 0) throw new SocketException((int)SocketError.HostNotFound);

            hostEndPoint = new IPv4EndPoint((IPv4Address)addresses[0], port);
            socket.Connect(hostEndPoint);
            
            headers[HttpEnumMethods.ToString(HttpRequestHeader.Host)] = hostNameOrAddress.Trim();
            headers[HttpEnumMethods.ToString(HttpRequestHeader.Connection)] = "close";
        }

        public HttpRequest(IPv4Address host)
            : this(new IPv4EndPoint(host, 80))
        { }

        public HttpRequest(IPv4EndPoint endPoint)
        {
            hostEndPoint = endPoint;
            socket.Connect(hostEndPoint);
            headers[HttpEnumMethods.ToString(HttpRequestHeader.Connection)] = "close";
        }
        #endregion

        #region Properties
        public IPv4EndPoint HostEndPoint
        {
            get { return hostEndPoint; }
        }
        #endregion

        #region Methods
        private static string Escape(string valueToEscape)
        {
            return charsToEscape.Replace(valueToEscape, match => string.Format("%{0:X}", (int)match.Value[0]));
        }

        public string GetRequestHeaderValue(string header)
        {
            string value = null;
            headers.TryGetValue(header, out value);
            return value;
        }

        public string GetRequestHeaderValue(HttpRequestHeader header)
        {
            return GetRequestHeaderValue(HttpEnumMethods.ToString(header));
        }

        public void SetRequestHeaderValue(HttpRequestHeader header, string value)
        {
            SetRequestHeaderValue(HttpEnumMethods.ToString(header), value);
        }

        public void SetRequestHeaderValue(string header, string value)
        {
            if (!headerValueValidator.IsMatch(value))
                throw new ArgumentException("Header value contains newline character");
            if (!headerNameValidator.IsMatch(header))
                throw new ArgumentException("Header name contains an illegal character");

            if (header == HttpEnumMethods.ToString(HttpRequestHeader.ContentLength))
                throw new ArgumentException("Cannot set unsafe header Content-Length");
            if (header == HttpEnumMethods.ToString(HttpRequestHeader.Connection))
                throw new ArgumentException("Cannot set unsupported header Connexion");

            headers[header] = value;
        }

        public HttpResponse Execute(HttpRequestMethod method, string path)
        {
            return Execute(method, path, emptyFields);
        }

        public HttpResponse Execute(HttpRequestMethod method, string path, IDictionary<string, string> fields)
        {
            if (method != HttpRequestMethod.Get &&
                method != HttpRequestMethod.Head &&
                method != HttpRequestMethod.Post)
                throw new NotImplementedException("Only GET, HEAD and POST (single-boundary) methods are supported");

            using (MemoryStream fieldsStream = new MemoryStream())
            {
                if (fields.Count != 0)
                {
                    if (method.IsIdempotent())
                    {
                        string exceptionString = string.Format("{0} method is idempotent and cannot have a request body", HttpEnumMethods.ToString(method));
                        throw new InvalidOperationException(exceptionString);
                    }

                    StreamWriter fieldWriter = new StreamWriter(fieldsStream);
                    foreach (KeyValuePair<string, string> field in fields)
                        fieldWriter.Write("{0}={1}&", Escape(field.Key), Escape(field.Value));
                    fieldWriter.Flush();
                }

                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    socket.Connect(hostEndPoint);
                    using (NetworkStream socketStream = new NetworkStream(socket))
                    {
                        StreamWriter writer = new StreamWriter(socketStream, new UTF8Encoding(false));
                        // this is kinda cheap, but most servers can cope with receiving non-ascii chars
                        // (we can't escape the whole path because we need to keep slashes as-is)
                        writer.Write("{0} {1} HTTP/1.1" + crlf, HttpEnumMethods.ToString(method), path.Replace(" ", "%20"));
                        foreach (KeyValuePair<string, string> header in headers)
                            writer.Write("{0}: {1}" + crlf, header.Key, header.Value);

                        if (fieldsStream.Length > 0)
                        {
                            string contentTypeName = HttpEnumMethods.ToString(HttpRequestHeader.ContentType);
                            if (!headers.ContainsKey(contentTypeName))
                                writer.Write("{0}: {1}; charset={2}" + crlf, contentTypeName, formMimeType, writer.Encoding.HeaderName);
                            writer.Write("{0}: {1}" + crlf, HttpEnumMethods.ToString(HttpRequestHeader.ContentLength), fieldsStream.Length);
                            writer.Write(crlf);
                            writer.Flush();
                            writer.BaseStream.Write(fieldsStream.GetBuffer(), 0, (int)fieldsStream.Length);
                        }

                        writer.Write(crlf);
                        writer.Flush();
                        socketStream.Flush();
                        return new HttpResponse(this, socket);
                    }
                }
            }
        }

        public void ExecuteAsync(HttpRequestMethod method, string path, Action<HttpResponse> onReceive)
        {
            ThreadPool.QueueUserWorkItem(obj => onReceive(Execute(method, path)));
        }

        public void ExecuteAsync(HttpRequestMethod method, string path,
            IDictionary<string, string> fields)
        {
            ThreadPool.QueueUserWorkItem(obj => Execute(method, path, fields));
        }

        public void ExecuteAsync(HttpRequestMethod method, string path,
            IDictionary<string, string> fields, Action<HttpResponse> onReceive)
        {
            ThreadPool.QueueUserWorkItem(obj => onReceive(Execute(method, path, fields)));
        }
        #endregion
    }
}
