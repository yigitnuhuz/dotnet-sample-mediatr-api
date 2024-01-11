using System.ComponentModel.DataAnnotations;

namespace Core.Enums
{
    public enum ExceptionType
    {
        [Display(Name = "enum_core_exception_type_undefined")]
        Undefined = 1,

        [Display(Name = "enum_core_exception_type_validation")]
        Validation = 2,

        [Display(Name = "enum_core_exception_type_info")]
        Info = 3,
    }
}
