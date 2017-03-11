using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace PerfTweaksLite
{
    public static class HttpTweak
    {
        private const string IfNoneMatch = "If-None-Match";
        private const string Etag = "Etag";
        private const string IfModifiedSince = "If-Modified-Since";
        private static DateTime _dateTime;

        public static void PublicCacheExtension(this HttpContext context)
        {
            _dateTime = File.GetLastWriteTime(context.Request.PhysicalPath);
            SetConditionalGetHeaders(context, context.Request.Url.AbsoluteUri);
        }

        public static void PrivateCacheExtension(this HttpContext context, string uri)
        {
            SetConditionalGetHeaders(context, uri);
        }

        public static void PublicCache(HttpContext context)
        {
            _dateTime = File.GetLastWriteTime(context.Request.PhysicalPath);
            SetConditionalGetHeaders(context, context.Request.Url.AbsoluteUri);
        }

        public static void PrivateCache(HttpContext context, string uri)
        {
            SetConditionalGetHeaders(context, uri);
        }

        private static void SetConditionalGetHeaders(HttpContext httpContext, string url)
        {
            SetConditionalGetHeaders(Hash(url), httpContext);
            SetConditionalGetHeaders(_dateTime, httpContext);
        }

        private static void SetConditionalGetHeaders(string etag, HttpContext context)
        {
            string ifNoneMatch = context.Request.Headers[IfNoneMatch];
            etag = $"\"{etag}\"";

            if (ifNoneMatch != null && ifNoneMatch.Contains(","))
            {
                ifNoneMatch = ifNoneMatch.Substring(0, ifNoneMatch.IndexOf(",", StringComparison.Ordinal));
            }

            context.Response.AppendHeader(Etag, etag);
            context.Response.Cache.VaryByHeaders[IfNoneMatch] = true;

            if (etag == ifNoneMatch)
            {
                context.Response.ClearContent();
                context.Response.StatusCode = (int)HttpStatusCode.NotModified;
                context.Response.SuppressContent = true;
            }
        }

        private static void SetConditionalGetHeaders(DateTime lastModified, HttpContext context)
        {
            HttpResponse response = context.Response;
            HttpRequest request = context.Request;
            lastModified = new DateTime(lastModified.Year, lastModified.Month, lastModified.Day, lastModified.Hour, lastModified.Minute, lastModified.Second);

            string incomingDate = request.Headers[IfModifiedSince];

            response.Cache.SetLastModified(lastModified);

            DateTime testDate;

            if (DateTime.TryParse(incomingDate, out testDate) && testDate == lastModified)
            {
                response.ClearContent();
                response.StatusCode = (int)HttpStatusCode.NotModified;
                response.SuppressContent = true;
            }
        }

        private static string Hash(string value)
        {
            MD5 md5 = new MD5Cng();

            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(value));

            var stringBuilder = new StringBuilder();

            for (int index = 0; index < hash.Length - 1; ++index)
                stringBuilder.Append(hash[index].ToString("x2"));

            return stringBuilder.ToString();
        }
    }
}
