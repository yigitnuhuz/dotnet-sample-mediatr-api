using System.ComponentModel.DataAnnotations;

namespace Core.Enums
{
    public enum CacheType
    {
        [Display(Name = "enum_core_cache_type_none")]
        None = 1,

        [Display(Name = "enum_core_cache_type_memory")]
        Memory = 2,

        [Display(Name = "enum_core_cache_type_distributed")]
        Distributed = 3,
    }
}
