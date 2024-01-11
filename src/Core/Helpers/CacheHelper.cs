using System.Collections.Generic;
using System.Text;

namespace Core.Helpers
{
    public static class CacheHelper
    {
        public static string CreateCacheKey(this IEnumerable<object> values, string name)
        {
            const string separator = ":";
            
            var builder = new StringBuilder(name);
            
            foreach (var value in values)
            {
                builder.Append($"{separator}{value}");
            }

            return builder.ToString();
        }
    }
}