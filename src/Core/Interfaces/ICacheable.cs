using System;
using System.Collections.Generic;
using Core.Enums;

namespace Core.Interfaces
{
    public interface ICacheable
    {
        CacheType CacheType { get; }
        KeyValuePair<object[], TimeSpan> CacheSettings { get; }
    }
}