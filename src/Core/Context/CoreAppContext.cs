using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Core.Context
{
    public interface ICoreAppContext
    {
        #region Token Based

        bool HasToken();
        string GetAuthorizationToken();
        bool IsAuthenticated();
        Guid UserId();
        string UserName();
        Guid SessionId();

        #endregion

        #region Header

        string Culture();
        string Ip();
        Dictionary<string, string> Headers();

        #endregion
    }

    public class CoreAppContext(IOptions<CoreSettings> options, IHttpContextAccessor contextAccessor)
        : ICoreAppContext
    {
        private readonly CoreSettings _coreSettings = options.Value;

        #region Token

        public bool HasToken()
        {
            var claim = contextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "System");

            return claim != null && claim.Value == _coreSettings.Auth.System;
        }

        public string GetAuthorizationToken()
        {
            var token = contextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(token))
            {
                throw new CustomException("invalid_token", false, HttpStatusCode.Unauthorized);
            }

            return token.Substring(token.LastIndexOf(" ", StringComparison.InvariantCultureIgnoreCase) + 1);
        }

        public bool IsAuthenticated()
        {
            var claim = contextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "IsAuthenticated");

            if (claim == null)
            {
                throw new CustomException("is_authenticated_is_missing_in_claims", false, HttpStatusCode.Unauthorized);
            }

            return claim.Value == "True";
        }

        public Guid UserId()
        {
            var claim = contextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "UserId");

            if (claim == null)
            {
                throw new CustomException("user_is_missing_in_claims", false, HttpStatusCode.Unauthorized);
            }

            return new Guid(claim.Value);
        }

        public string UserName()
        {
            var claim = contextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "UserName");

            if (claim == null)
            {
                throw new CustomException("user_is_missing_in_claims", false, HttpStatusCode.Unauthorized);
            }

            return new string(claim.Value);
        }

        public Guid SessionId()
        {
            var claim = contextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "SessionId");

            if (claim == null)
            {
                throw new CustomException("session_is_missing_in_claims", false, HttpStatusCode.Unauthorized);
            }

            return new Guid(claim.Value);
        }

        #endregion

        #region Header

        public string Culture()
        {
            var headerName = _coreSettings.Localization.HeaderName;

            return string.IsNullOrEmpty(headerName)
                ? throw new CustomException("localization_header_name_is_missing_in_settings")
                : contextAccessor.HttpContext.Request.Headers.TryGetValue(headerName, out var values)
                    ? values.ToString()
                    : null;
        }

        public string Ip()
        {
            const string ipHeaderKey = "Client-IP";
            const string ipHeaderKey2 = "X-Forwarded-For";

            var ip = contextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            if (contextAccessor.HttpContext.Request.Headers.Any(x => x.Key == ipHeaderKey))
            {
                var ips = contextAccessor.HttpContext.Request.Headers[ipHeaderKey].ToString();

                return ips.Contains(",") ? ips.Split(',')[0] : ips;
            }

            if (contextAccessor.HttpContext.Request.Headers.Any(x => x.Key == ipHeaderKey2))
            {
                var ips = contextAccessor.HttpContext.Request.Headers[ipHeaderKey2].ToString();

                return ips.Contains(",") ? ips.Split(',')[0] : ips;
            }

            return ip;
        }

        public Dictionary<string, string> Headers()
        {
            return contextAccessor.HttpContext?.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
        }

        #endregion
    }
}