﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.IO;

namespace Orion.Engine.Networking.Http
{
    public class HttpRequest
    {
        #region Fields
        private static readonly Dictionary<string, string> emptyFields = new Dictionary<string, string>();
        private static readonly Regex headerValueValidator = new Regex(@"^([^\r\n]+)$", RegexOptions.Compiled);
        private static readonly Regex headerNameValidator = new Regex(@"^([a-zA-Z\-_]+)$", RegexOptions.Compiled);
        private static readonly Regex charsToEscape = new Regex(@"[^a-zA-Z0-9$\-@.!*""'()]", RegexOptions.Compiled);
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
            if (addresses.Length == 0) throw new ArgumentException("Could not resolve hostname " + hostNameOrAddress);
            byte[] address = addresses[0].GetAddressBytes();

            hostEndPoint = new IPv4EndPoint(address[0], address[1], address[2], address[3], port);
            socket.Connect(hostEndPoint);
            
            headers[HttpRequestHeader.Host.ToString()] = hostNameOrAddress.Trim();
            headers[HttpRequestHeader.Connection.ToString()] = "close";
        }

        public HttpRequest(IPv4Address host)
            : this(new IPv4EndPoint(host, 80))
        { }

        public HttpRequest(IPv4EndPoint endPoint)
        {
            hostEndPoint = endPoint;
            socket.Connect(hostEndPoint);
            headers[HttpRequestHeader.Connection.ToString()] = "close";
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
            return charsToEscape.Replace(valueToEscape, match => string.Format("%{0:X}", match.Value[0]));
        }

        public string GetRequestHeaderValue(string header)
        {
            string value = null;
            headers.TryGetValue(header, out value);
            return value;
        }

        public string GetRequestHeaderValue(HttpRequestHeader header)
        {
            return GetRequestHeaderValue(header.ToString());
        }

        public void SetRequestHeaderValue(HttpRequestHeader header, string value)
        {
            SetRequestHeaderValue(header.ToString(), value);
        }

        public void SetRequestHeaderValue(string header, string value)
        {
            if (!headerValueValidator.IsMatch(value))
                throw new ArgumentException("Header value contains newline character");
            if (!headerNameValidator.IsMatch(header))
                throw new ArgumentException("Header name contains an illegal character");

            if (header == HttpRequestHeader.ContentLength.ToString())
                throw new ArgumentException("Cannot set unsafe header Content-Length");
            if (header == HttpRequestHeader.Connection.ToString())
                throw new ArgumentException("Cannot set unsupported header Connexion");

            headers[header] = value;
        }

        public HttpResponse Execute(HttpRequestMethod method, string path)
        {
            return Execute(method, path, emptyFields);
        }

        public HttpResponse Execute(HttpRequestMethod method, string path, IDictionary<string, string> fields)
        {
            using (MemoryStream fieldsStream = new MemoryStream())
            {
                if (fields.Count != 0)
                {
                    if (method.IsIdempotent())
                    {
                        string exceptionString = string.Format("{0} method is idempotent and cannot have a request body", method.ToString());
                        throw new InvalidOperationException(exceptionString);
                    }

                    using (StreamWriter fieldWriter = new StreamWriter(fieldsStream))
                    foreach (KeyValuePair<string, string> field in fields)
                        fieldWriter.Write("{0}={1}&", Escape(field.Key), Escape(field.Value));
                }

                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    socket.Connect(hostEndPoint);
                    using (NetworkStream socketStream = new NetworkStream(socket))
                    {
                        using (StreamWriter writer = new StreamWriter(socketStream))
                        {
                            // this is kinda cheap, but most servers can cope with receiving non-ascii chars
                            // (we can't escape the whole path because we need to keep slashes as-is)
                            writer.Write("{0} {1} HTTP/1.1" + crlf, method.ToString(), path.Replace(" ", "%20"));
                            foreach (KeyValuePair<string, string> header in headers)
                                writer.Write("{0}: {1}" + crlf, header.Key, header.Value);
                            writer.Write(crlf);
                            writer.Write(crlf);

                            if (fieldsStream.Length > 0)
                            {
                                writer.BaseStream.Write(fieldsStream.GetBuffer(), 0, (int)fieldsStream.Length);
                                writer.Write(crlf);
                            }
                        }

                        return new HttpResponse(this, socketStream);
                    }
                }
            }
        }
        #endregion
    }
}
