using EVServiceCenter.Core.Exceptions;
using EVServiceCenter.Core.Domains.Shared.Models;

namespace EVServiceCenter.API.Middleware
{
    public class GlobalExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            ApiResponse<object> response;
            int statusCode;

            switch (ex)
            {
                case BusinessRuleException businessEx:
                    statusCode = 400;
                    response = ApiResponse<object>.WithError(
                        businessEx.Message,
                        businessEx.ErrorCode,
                        statusCode);
                    _logger.LogWarning(ex, "Business rule violation: {Message}", businessEx.Message);
                    break;

                case InvalidOperationException invalidEx:
                    statusCode = 400;
                    response = ApiResponse<object>.WithError(
                        invalidEx.Message,
                        "INVALID_OPERATION",
                        statusCode);
                    _logger.LogWarning(ex, "Invalid operation: {Message}", invalidEx.Message);
                    break;

                case KeyNotFoundException notFoundEx:
                    statusCode = 404;
                    response = ApiResponse<object>.WithNotFound(
                        notFoundEx.Message,
                        statusCode);
                    _logger.LogWarning(ex, "Resource not found: {Message}", notFoundEx.Message);
                    break;

                case UnauthorizedAccessException unauthorizedEx:
                    statusCode = 403;
                    response = ApiResponse<object>.WithError(
                        "Bạn không có quyền truy cập tài nguyên này",
                        "FORBIDDEN",
                        statusCode);
                    _logger.LogWarning(ex, "Unauthorized access: {Message}", unauthorizedEx.Message);
                    break;

                default:
                    statusCode = 500;
                    response = ApiResponse<object>.WithError(
                        "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau",
                        "INTERNAL_ERROR",
                        statusCode);
                    _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                    break;
            }

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}