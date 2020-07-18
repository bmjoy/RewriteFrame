using System;
using System.Diagnostics.CodeAnalysis;

namespace Crucis.Protocol
{
    /// <summary>
    /// Used with <see cref="HttpRequestException"/> to indicate if a request is safe to retry.
    /// </summary>
    internal enum RequestRetryType
    {
        NoRetry,
        RetryOnSameOrNextProxy,
        RetryOnNextProxy
    }

    [SuppressMessage("Microsoft.Serialization", "CA2229")]
    public class RequestException : Exception
    {
        internal RequestRetryType AllowRetry { get; } = RequestRetryType.NoRetry;

        public RequestException()
            : this(null, null)
        { }

        public RequestException(string message)
            : this(message, null)
        { }

        public RequestException(string message, Exception inner)
            : base(message, inner)
        {
            if (inner != null)
            {
                HResult = inner.HResult;
            }
        }

        // This constructor is used internally to indicate that a request was not successfully sent due to an IOException,
        // and the exception occurred early enough so that the request may be retried on another connection.
        internal RequestException(string message, Exception inner, RequestRetryType allowRetry)
            : this(message, inner)
        {
            AllowRetry = allowRetry;
        }
    }
}
