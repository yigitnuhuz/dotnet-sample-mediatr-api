using Core.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Core.Helpers
{
    public static class ActionResultHelper
    {
        public static string Version { get; set; }

        public static ActionResult Ok<T>(T data)
        {
            return new OkObjectResult(new ApiResponseDto<T>
            {
                Version = Version,
                Data = data
            });
        }

        public static ActionResult Error(ApiErrorDto errorDto)
        {
            errorDto.Version = Version;

            return new ObjectResult(errorDto);
        }
    }
}