using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Networking.Http
{
    public enum HttpRequestMethod
    {
        Options,
        Get,
        Head,

        Post,
        Put,
        Delete,
        Trace,
        Connect
    }

    public static class HttpRquestMethodExtension
    {
        public static string ToString(this HttpRequestMethod method)
        {
            switch (method)
            {
                case HttpRequestMethod.Head: return "HEAD";
                case HttpRequestMethod.Get: return "GET";
                case HttpRequestMethod.Post: return "POST";
                default: throw new InvalidOperationException("No name defined for this request method");
            }
        }

        public static bool IsIdempotent(this HttpRequestMethod method)
        {
            return method == HttpRequestMethod.Get
                || method == HttpRequestMethod.Head
                || method == HttpRequestMethod.Options;
        }
    }
}
