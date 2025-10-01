using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EVServiceCenter.Core.Domains.Shared.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public string? ErrorCode { get; set; }
        public object? ValidationErrors { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public int? StatusCode { get; set; }


        public static ApiResponse<T> WithSuccess(T? data = default, string? message = null, int statusCode = 200)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                ErrorCode = null,
                ValidationErrors = null,
                StatusCode = statusCode
            };
        }

        public static ApiResponse<T> WithError(string message, string errorCode = "ERROR" , int statusCode = 400)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default,
                ErrorCode = errorCode,
                ValidationErrors = null,
                StatusCode = statusCode
            };
        }

        public static ApiResponse<T> WithNotFound(string message, int statusCode = 404)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default,
                ErrorCode = "NotFound",
                ValidationErrors = null,
                StatusCode = statusCode
            };
        }

        public static ApiResponse<T> WithValidationError(object validationErrors, string? message = "Lỗi xác thực", int statusCode = 422)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default,
                ErrorCode = "ValidationError",
                ValidationErrors = validationErrors,
                StatusCode = statusCode
            };
        }
    }
}