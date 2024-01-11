using Core.Enums;

namespace Core.Dtos
{
    public class ApiErrorDto(string message, ExceptionType type) : BaseResponseDto
    {
        public string Message { get; } = message;

        public ExceptionType ExceptionType { get; } = type;
    }
}