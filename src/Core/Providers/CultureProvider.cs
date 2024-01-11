using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace Core.Providers
{
    public class CultureProvider(LocalizationSettings settings) : RequestCultureProvider
    {
        public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext context)
        {
            var headerName = settings.HeaderName;

            var defaultCulture = settings.DefaultCulture;

            var requestCulture = context.Request.Headers[headerName];

            var culture = settings.SupportedCultures.FirstOrDefault(x => x.Equals(requestCulture, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(culture)) return Task.FromResult(new ProviderCultureResult(culture));

            culture = defaultCulture;

            context.Request.Headers[headerName] = defaultCulture;

            return Task.FromResult(new ProviderCultureResult(culture));
        }
    }
}