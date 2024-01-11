using System;
using System.Collections.Generic;

namespace Core.Models
{
    public class LogDetail
    {
        #region Request

        public string Host { get; set; }
        public string Protocol { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
        public string PathAndQuery { get; set; }
        public int StatusCode { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public string Ip { get; set; }

        #endregion

        #region Request Detail

        public string Body { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        #endregion

        #region Core Data

        public bool IsAuthenticated { get; set; }
        public Guid UserId { get; set; }
        public Guid SessionId { get; set; }
        public string Culture { get; set; }
        public string MachineName { get; set; }
        
        #endregion

        #region Exception Data

        public Exception Exception { get; set; }
        public string ExceptionType { get; set; }
        public Dictionary<string, string> Errors { get; set; }

        #endregion
    }
}