using Newtonsoft.Json;

namespace MedSecureSystem.Application.Commons
{
    public class ApiResult<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; }
        [JsonIgnore]
        public ApiResultStatus ApiResultStatus { get; set; }

        public ApiResult(T data, bool success = true, string message = null)
        {
            Success = success;
            Data = data;
            Message = message;
            Errors = new List<string>();
            ApiResultStatus = ApiResultStatus.Success;
        }

        public ApiResult(List<string>? errors, bool success = false, string message = null,
            ApiResultStatus status = ApiResultStatus.BadRequest
            )
        {
            Success = success;
            Errors = errors ?? Enumerable.Empty<string>().ToList();
            Message = message;
            ApiResultStatus = status;
        }

        public static ApiResult<T> SuccessResult(T data, string message = null)
        {
            return new ApiResult<T>(data, true, message);
        }

        public static ApiResult<T> FailureResult(List<string>? errors, string message = null)
        {
            return new ApiResult<T>(errors, false, message);
        }

        public static ApiResult<T> NotFoundResult(string message = null)
        {
            return new ApiResult<T>(null, false, message, ApiResultStatus.NotFound);
        }
    }

    public enum ApiResultStatus
    {
        Success,
        BadRequest,
        NotFound
    }
}