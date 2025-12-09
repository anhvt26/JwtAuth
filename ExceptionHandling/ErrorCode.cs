using System.ComponentModel;

namespace JwtAuth.ExceptionHandling
{
    public enum ErrorCode
    {
        [Description("Lỗi hệ thống!")]
        UNKNOWN = -1,
        [Description("Thành công.")]
        SUCCESS,
        [Description("Có lỗi xảy ra.")]
        OTHER,
        [Description("Yêu cầu sai!")]
        BAD_REQUEST,
        [Description("Xác thực thất bại.")]
        UN_AUTHORIZED,
        [Description("Không có quyền truy cập.")]
        FORBIDDEN,
        [Description("Thiếu thông tin tài khoản hoặc mật khẩu.")]
        MISSING_USERNAME_OR_PASSWORD,
        [Description("Tài khoản hoặc mật khẩu không chính xác.")]
        USERNAME_OR_PASSWORD_INCORRECT,
        [Description("Không tìm thấy người dùng.")]
        USER_NOT_FOUND,
        [Description("Thiếu tham số.")]
        MISSING_PARAMETER,
        [Description("Tài khoản admin đã tồn tại.")]
        ADMIN_ACCOUNT_ALREADY_EXISTS,
        [Description("Tài khoản không tồn tại.")]
        ACCOUNT_NOT_FOUND,
        [Description("Token không hợp lệ.")]
        INVALID_TOKEN,
        [Description("Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.")]
        ACCOUNT_LOCKED,
        [Description("Không tìm thấy dữ liệu.")]
        NOT_FOUND,
        [Description("Thiếu uuid.")]
        MISSING_UUID,
        [Description("Api không tồn tại.")]
        API_ENDPOINT_NOT_FOUND,
        [Description("Api đã bị xóa.")]
        API_ENDPOINT_ALREADY_DELETED,
        [Description("Api chưa bị xóa.")]
        API_ENDPOINT_NOT_DELETED,
        [Description("Không thể xóa api đặc quyền.")]
        CANNOT_REMOVE_API_SPECIAL_PERMISSION,
        [Description("File data is required.")]
        FILE_DATA_REQUIRED,
        [Description("User đã có tài khoản.")]
        USER_ALREADY_HAS_ACCOUNT,
        [Description("Tài khoản admin đã được khởi tạo.")]
        ADMIN_ACCOUNT_ALREADY_INITIALIZED,
        [Description("Certificate không hợp lệ.")]
        INVALID_CERT,
        [Description("Định dạng tập tin không được hỗ trợ")]
        NOT_SUPPORT_FILE_FORMAT,
        [Description("Truy cập bị từ chối.")]
        PERMISSION_DENIED,
        [Description("Tham số không hợp lệ.")]
        ARGUMENT_EXCEPTION,
        [Description("Chuỗi kết nối rỗng.")]
        CONNECTION_STRING_EMPTY,
        [Description("Mật khẩu xác nhận không khớp.")]
        PASSWORD_CONFIRM_NOT_MATCH,
        [Description("Mật khẩu không chính xác.")]
        PASSWORD_INCORRECT,
        [Description("Đăng ký service bị trùng.")]
        AMBIGIUOUS_SERVICE_REGISTRATION,
        [Description("Quá nhiều file được tải lên.")]
        UPLOAD_TOO_MANY_FILES,
        [Description("Tham số không hợp lệ.")]
        INVALID_PARAM,
        [Description("Tài khoản không hoạt động.")]
        ACCOUNT_NOT_ACTIVE,
        [Description("Đã tồn tại.")]
        EXISTED,
        [Description("Số lượng file và mục đích không khớp.")]
        FILE_DATA_AND_PURPOSE_MISMATCH,
        [Description("Không hỗ trợ.")]
        NOT_SUPPORTED,
        [Description("Hành động thất bại.")]
        ACTION_FAILED,
        [Description("Lỗi API Zalo OA.")]
        ZALO_OA_API_ERROR,
        [Description("Lỗi API TUYA.")]
        TUYA_API_ERROR,
        [Description("Hành động không được phép.")]
        NOT_ALLOWED,
        [Description("Phiên đăng nhập đã hết hạn.")]
        SESSION_EXPIRED
    }
}
