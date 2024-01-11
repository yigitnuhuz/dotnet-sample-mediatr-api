using System;
using System.Net;

namespace Core.Models
{
    public class CustomException(
        string message,
        bool isFriendly = false,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        : Exception(message)
    {
        public readonly bool IsFriendly = isFriendly;
        public readonly HttpStatusCode StatusCode = statusCode;
    }
}