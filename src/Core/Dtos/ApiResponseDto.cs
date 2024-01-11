namespace Core.Dtos
{
    public class ApiResponseDto<T> : BaseResponseDto
    {
        public T Data { get; set; }
    }
}