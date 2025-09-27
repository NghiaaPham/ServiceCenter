namespace EVServiceCenter.Core.Constants
{
    public static class ErrorMessages
    {
        public const string USER_NOT_FOUND = "Người dùng không tồn tại";
        public const string CUSTOMER_NOT_FOUND = "Khách hàng không tồn tại";
        public const string VEHICLE_NOT_FOUND = "Xe không tồn tại";
        public const string SERVICE_NOT_FOUND = "Dịch vụ không tồn tại";
        public const string APPOINTMENT_NOT_FOUND = "Lịch hẹn không tồn tại";
        public const string WORKORDER_NOT_FOUND = "Phiếu công việc không tồn tại";

        public const string INVALID_USERNAME_PASSWORD = "Tên đăng nhập hoặc mật khẩu không đúng";
        public const string ACCOUNT_LOCKED = "Tài khoản đã bị khóa";
        public const string INVALID_TOKEN = "Token không hợp lệ";
        public const string TOKEN_EXPIRED = "Token đã hết hạn";

        public const string DUPLICATE_USERNAME = "Tên đăng nhập đã tồn tại";
        public const string DUPLICATE_EMAIL = "Email đã được sử dụng";
        public const string DUPLICATE_PHONE = "Số điện thoại đã được sử dụng";
        public const string DUPLICATE_LICENSE_PLATE = "Biển số xe đã tồn tại";

        public const string INVALID_EMAIL_FORMAT = "Định dạng email không hợp lệ";
        public const string INVALID_PHONE_FORMAT = "Định dạng số điện thoại không hợp lệ";
        public const string INVALID_DATE_RANGE = "Khoảng thời gian không hợp lệ";

        public const string INSUFFICIENT_STOCK = "Không đủ hàng tồn kho";
        public const string INVALID_PAYMENT_AMOUNT = "Số tiền thanh toán không hợp lệ";
        public const string APPOINTMENT_CONFLICT = "Lịch hẹn bị trung thời gian";

        public const string ACCESS_DENIED = "Không có quyền truy cập";
        public const string OPERATION_FAILED = "Thao tác không thành công";
        public const string DATABASE_ERROR = "Lỗi cơ sở dữ liệu";
        public const string VALIDATION_ERROR = "Dữ liệu không hợp lệ";

        public const string EMAIL_NOT_VERIFIED = "Email chưa được xác thực. Vui lòng kiểm tra email và xác thực tài khoản trước khi đăng nhập.";
        public const string EMAIL_VERIFICATION_REQUIRED = "Tài khoản của bạn cần được xác thực email";
        public const string EMAIL_VERIFICATION_TOKEN_INVALID = "Link xác thực không hợp lệ hoặc đã hết hạn";
        public const string EMAIL_ALREADY_VERIFIED = "Email đã được xác thực";
    }
}
