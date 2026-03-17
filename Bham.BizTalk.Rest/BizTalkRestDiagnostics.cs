using System;

namespace Bham.BizTalk.Rest
{
    public enum BizTalkRestLogLevel
    {
        Debug = 0,
        Information = 1,
        Warning = 2,
        Error = 3
    }

    public sealed class BizTalkRestLogEntry
    {
        public DateTime TimestampUtc { get; set; }

        public BizTalkRestLogLevel Level { get; set; }

        public string Operation { get; set; }

        public string Url { get; set; }

        public string Message { get; set; }

        public int? StatusCode { get; set; }

        public Exception Exception { get; set; }
    }

    public sealed class BizTalkRestClientException : Exception
    {
        public BizTalkRestClientException(
            string operation,
            string url,
            string message,
            Exception innerException = null,
            int? statusCode = null,
            string responseBody = null)
            : base(message, innerException)
        {
            Operation = operation;
            Url = url;
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }

        public string Operation { get; }

        public string Url { get; }

        public int? StatusCode { get; }

        public string ResponseBody { get; }
    }

    internal static class BizTalkRestLogging
    {
        public static void Write(
            Action<BizTalkRestLogEntry> logger,
            BizTalkRestLogLevel level,
            string operation,
            string url,
            string message,
            Exception exception = null,
            int? statusCode = null)
        {
            if (logger == null)
            {
                return;
            }

            try
            {
                logger(
                    new BizTalkRestLogEntry
                    {
                        TimestampUtc = DateTime.UtcNow,
                        Level = level,
                        Operation = operation,
                        Url = url,
                        Message = message,
                        StatusCode = statusCode,
                        Exception = exception
                    });
            }
            catch
            {
            }
        }
    }
}