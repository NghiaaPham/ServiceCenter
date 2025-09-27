using EVServiceCenter.Core.Domains.Identity.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace EVServiceCenter.Infrastructure.Domains.Identity.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        //public async Task SendEmailVerificationAsync(string email, string fullName, string verificationToken)
        //{
        //    var subject = "Xác thực tài khoản EV Service Center";

        //    // Lấy API URL từ config (có thể là ngrok URL)
        //    var apiUrl = _configuration["AppSettings:ApiUrl"] ?? "https://localhost:7077";
        //    var frontendUrl = _configuration["AppSettings:WebsiteUrl"] ?? "http://localhost:3000";

        //    // URL trực tiếp đến API endpoint để xác thực
        //    var verificationApiUrl = $"{apiUrl}/api/verification/verify-email?token={verificationToken}&email={Uri.EscapeDataString(email)}";

        //    // URL để redirect về frontend sau khi xác thực
        //    var frontendVerificationUrl = $"{frontendUrl}/verify-email?token={verificationToken}&email={Uri.EscapeDataString(email)}";

        //    var htmlBody = $@"
        //        <html>
        //        <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
        //            <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        //                <h2 style='color: #007bff;'>Chào {fullName},</h2>
        //                <p>Cảm ơn bạn đã đăng ký tài khoản tại <strong>EV Service Center</strong>.</p>
        //                <p>Để hoàn tất việc đăng ký, vui lòng xác thực địa chỉ email của bạn bằng cách click vào nút bên dưới:</p>

        //                <div style='text-align: center; margin: 30px 0;'>
        //                    <a href='{verificationApiUrl}' 
        //                       style='background-color: #007bff; color: white; padding: 12px 30px; 
        //                              text-decoration: none; border-radius: 5px; display: inline-block;
        //                              font-weight: bold;'>
        //                        XÁC THỰC EMAIL
        //                    </a>
        //                </div>

        //                <p>Hoặc copy link sau vào trình duyệt:</p>
        //                <p style='background-color: #f8f9fa; padding: 10px; border-radius: 5px; word-break: break-all;'>
        //                    {verificationApiUrl}
        //                </p>

        //                <p style='font-size: 12px; color: #999; margin-top: 20px;'>
        //                    Nếu bạn muốn xác thực thông qua giao diện web, vui lòng truy cập:
        //                    <a href='{frontendVerificationUrl}'>{frontendVerificationUrl}</a>
        //                </p>

        //                <div style='background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 5px; margin: 20px 0;'>
        //                    <strong>⏰ Lưu ý:</strong> Link xác thực này sẽ hết hạn sau 24 giờ.
        //                </div>

        //                <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
        //                <p style='font-size: 12px; color: #666;'>
        //                    Nếu bạn không đăng ký tài khoản này, vui lòng bỏ qua email này.<br>
        //                    Đây là email tự động, vui lòng không trả lời email này.
        //                </p>
        //                <p style='font-size: 12px; color: #666;'>
        //                    <strong>EV Service Center Team</strong><br>
        //                    {_configuration["AppSettings:CompanyAddress"]}
        //                </p>
        //            </div>
        //        </body>
        //        </html>";

        //    await SendEmailAsync(email, subject, htmlBody);
        //}

        public async Task SendEmailVerificationAsync(string email, string fullName, string verificationToken)
        {
            var subject = "Xác thực tài khoản EV Service Center";

            // Lấy API URL từ config (có thể là ngrok URL)
            var apiUrl = _configuration["AppSettings:ApiUrl"] ?? "https://localhost:7077";
            var frontendUrl = _configuration["AppSettings:WebsiteUrl"] ?? "http://localhost:3000";

            // URL trực tiếp đến API endpoint để xác thực
            var verificationApiUrl = $"{apiUrl}/api/verification/verify-email?token={verificationToken}&email={Uri.EscapeDataString(email)}";

            // URL để redirect về frontend sau khi xác thực
            var frontendVerificationUrl = $"{frontendUrl}/verify-email?token={verificationToken}&email={Uri.EscapeDataString(email)}";

            var htmlBody = $@"
    <html>
    <body style='margin:0; padding:0; background-color:#f4f6f9; font-family: Arial, sans-serif; color:#333;'>
        <div style='max-width:600px; margin:40px auto; background-color:#ffffff; border-radius:10px; 
                    box-shadow:0 4px 12px rgba(0,0,0,0.08); overflow:hidden;'>

            <!-- Header -->
            <div style='background-color:#007bff; padding:20px; text-align:center;'>
                <h1 style='margin:0; font-size:22px; color:#fff;'>EV Service Center</h1>
            </div>

            <!-- Content -->
            <div style='padding:30px;'>
                <h2 style='color:#007bff; font-size:20px; margin-top:0;'>👋 Xin chào {fullName},</h2>
                <p>Cảm ơn bạn đã đăng ký tài khoản tại <strong>EV Service Center</strong>.</p>
                <p>Để hoàn tất đăng ký, vui lòng xác thực địa chỉ email của bạn bằng cách nhấn vào nút bên dưới:</p>

                <!-- Button -->
                <div style='text-align:center; margin:35px 0;'>
                    <a href='{verificationApiUrl}' 
                       style='background-color:#007bff; color:white; padding:14px 35px; 
                              text-decoration:none; border-radius:6px; font-weight:bold;
                              font-size:15px; display:inline-block;'>
                        XÁC THỰC EMAIL
                    </a>
                </div>

                <!-- Alternative link -->
                <p>Nếu nút trên không hoạt động, vui lòng sao chép liên kết sau và dán vào trình duyệt:</p>
                <p style='background-color:#f8f9fa; padding:12px; border-radius:6px; 
                          word-break:break-all; font-size:13px; border:1px solid #e1e4e8;'>
                    {verificationApiUrl}
                </p>

                <!-- Frontend fallback -->
                <p style='font-size:13px; color:#555; margin-top:20px;'>
                    Hoặc xác thực thông qua giao diện web: <br>
                    <a href='{frontendVerificationUrl}' style='color:#007bff;'>{frontendVerificationUrl}</a>
                </p>

                <!-- Notice -->
                <div style='background-color:#fff3cd; border:1px solid #ffeeba; padding:15px; 
                            border-radius:6px; margin:25px 0; font-size:14px;'>
                    <strong>⏰ Lưu ý:</strong> Liên kết xác thực sẽ hết hạn sau <strong>24 giờ</strong>.
                </div>

                <p style='margin-top:30px; font-size:14px; color:#555;'>
                    Nếu bạn không đăng ký tài khoản này, vui lòng bỏ qua email.<br>
                    Đây là email tự động, vui lòng không trả lời.
                </p>
                <p style='margin-top:20px; font-size:14px; color:#555;'>
                    Trân trọng,<br>
                    <strong>Đội ngũ EV Service Center</strong><br>
                    {_configuration["AppSettings:CompanyAddress"]}
                </p>
            </div>

            <!-- Footer -->
            <div style='background-color:#f4f6f9; padding:20px; text-align:center; font-size:12px; color:#777;'>
                <p style='margin:6px 0 0; font-size:11px;'>© {DateTime.UtcNow.Year} EV Service Center. All rights reserved.</p>
            </div>
        </div>
    </body>
    </html>";

            await SendEmailAsync(email, subject, htmlBody);
        }


        //public async Task SendPasswordResetAsync(string email, string fullName, string resetToken)
        //{
        //    var subject = "Đặt lại mật khẩu EV Service Center";

        //    var apiUrl = _configuration["AppSettings:ApiUrl"] ?? "https://localhost:7077";
        //    var frontendUrl = _configuration["AppSettings:WebsiteUrl"] ?? "http://localhost:3000";

        //    // Direct API validation URL - sẽ redirect về frontend sau khi validate
        //    var directResetUrl = $"{apiUrl}/api/account/reset-password-page?token={resetToken}&email={Uri.EscapeDataString(email)}";

        //    // Frontend URL for manual access
        //    var frontendResetUrl = $"{frontendUrl}/reset-password?token={resetToken}&email={Uri.EscapeDataString(email)}";

        //    var htmlBody = $@"
        //<html>
        //<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
        //    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        //        <h2 style='color: #dc3545;'>🔐 Đặt lại mật khẩu</h2>
        //        <p>Chào {fullName},</p>
        //        <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản <strong>{email}</strong>.</p>

        //        <div style='text-align: center; margin: 30px 0;'>
        //            <a href='{directResetUrl}' 
        //               style='background-color: #dc3545; color: white; padding: 12px 30px; 
        //                      text-decoration: none; border-radius: 5px; display: inline-block;
        //                      font-weight: bold;'>
        //                ĐẶT LẠI MẬT KHẨU
        //            </a>
        //        </div>

        //        <p>Hoặc copy link sau vào trình duyệt:</p>
        //        <p style='background-color: #f8f9fa; padding: 10px; border-radius: 5px; word-break: break-all;'>
        //            {directResetUrl}
        //        </p>

        //        <p style='font-size: 12px; color: #999; margin-top: 20px;'>
        //            Nếu bạn muốn truy cập thông qua giao diện web, vui lòng truy cập:
        //            <a href='{frontendResetUrl}'>{frontendResetUrl}</a>
        //        </p>

        //        <div style='background-color: #f8d7da; border: 1px solid #f5c6cb; padding: 15px; border-radius: 5px; margin: 20px 0;'>
        //            <strong>⚠️ Bảo mật:</strong> Link này sẽ hết hạn sau 1 giờ.<br>
        //            Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.
        //        </div>

        //        <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
        //        <p style='font-size: 12px; color: #666;'>
        //            <strong>EV Service Center Team</strong><br>
        //            Email hỗ trợ: {_configuration["AppSettings:SupportEmail"]}
        //        </p>
        //    </div>
        //</body>
        //</html>";

        //    await SendEmailAsync(email, subject, htmlBody);
        //}

        // ✅ IMPROVED UI VERSION - Modern Layout
        public async Task SendPasswordResetAsync(string email, string fullName, string resetToken)
        {
            var subject = "Đặt lại mật khẩu EV Service Center";
            var frontendUrl = _configuration["AppSettings:WebsiteUrl"] ?? "http://localhost:3000";
            var resetUrl = $"{frontendUrl}/reset-password?token={resetToken}&email={Uri.EscapeDataString(email)}";

            var htmlBody = $@"
    <html>
    <body style='margin:0; padding:0; background-color:#f4f6f9; font-family: Arial, sans-serif; color: #333;'>
        <div style='max-width:600px; margin:40px auto; background-color:#ffffff; border-radius:10px; 
                    box-shadow:0 4px 12px rgba(0,0,0,0.08); overflow:hidden;'>
            
            <!-- Header -->
            <div style='background-color:#dc3545; padding:20px; text-align:center;'>
                <h1 style='margin:0; font-size:22px; color:#fff;'>EV Service Center</h1>
            </div>

            <!-- Content -->
            <div style='padding:30px;'>
                <h2 style='color:#dc3545; font-size:20px; margin-top:0;'>🔐 Đặt lại mật khẩu</h2>
                <p>Xin chào <strong>{fullName}</strong>,</p>
                <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản <strong>{email}</strong>.</p>

                <!-- Button -->
                <div style='text-align:center; margin:35px 0;'>
                    <a href='{resetUrl}'
                       style='background-color:#dc3545; color:white; padding:14px 35px; 
                              text-decoration:none; border-radius:6px; font-weight:bold;
                              font-size:15px; display:inline-block;'>
                        ĐẶT LẠI MẬT KHẨU
                    </a>
                </div>

                <!-- Alternative link -->
                <p>Nếu nút trên không hoạt động, vui lòng sao chép liên kết sau và dán vào trình duyệt:</p>
                <p style='background-color:#f8f9fa; padding:12px; border-radius:6px; 
                          word-break:break-all; font-size:13px; border:1px solid #e1e4e8;'>
                    {resetUrl}
                </p>

                <!-- Security Note -->
                <div style='background-color:#fff3cd; border:1px solid #ffeeba; padding:15px; 
                            border-radius:6px; margin:25px 0; font-size:14px;'>
                    <strong>⚠️ Bảo mật:</strong> Liên kết sẽ hết hạn sau <strong>1 giờ</strong>.<br>
                    Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.
                </div>

                <p style='margin-top:30px; font-size:14px; color:#555;'>
                    Trân trọng,<br>
                    <strong>Đội ngũ EV Service Center</strong>
                </p>
            </div>

            <!-- Footer -->
            <div style='background-color:#f4f6f9; padding:20px; text-align:center; font-size:12px; color:#777;'>
                <p style='margin:4px 0;'>Email hỗ trợ: {_configuration["AppSettings:SupportEmail"]}</p>
                <p style='margin:4px 0;'>Hotline: {_configuration["AppSettings:SupportPhone"]}</p>
                <p style='margin:6px 0 0; font-size:11px;'>© {DateTime.UtcNow.Year} EV Service Center. All rights reserved.</p>
            </div>
        </div>
    </body>
    </html>";

            await SendEmailAsync(email, subject, htmlBody);
        }


        //public async Task SendWelcomeEmailAsync(string email, string fullName)
        //{
        //    var subject = "Chào mừng đến với EV Service Center";

        //    var htmlBody = $@"
        //        <html>
        //        <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
        //            <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        //                <div style='text-align: center; margin-bottom: 30px;'>
        //                    <h1 style='color: #28a745; margin-bottom: 10px;'>🎉 Chào mừng bạn!</h1>
        //                    <h2 style='color: #007bff; margin-top: 0;'>EV Service Center</h2>
        //                </div>

        //                <p>Chào {fullName},</p>
        //                <p>Chúc mừng bạn đã trở thành thành viên của <strong>EV Service Center</strong>! 🚗⚡</p>

        //                <div style='background-color: #d4edda; border: 1px solid #c3e6cb; padding: 20px; border-radius: 8px; margin: 25px 0;'>
        //                    <h3 style='color: #155724; margin-top: 0;'>✅ Tài khoản của bạn đã được kích hoạt thành công!</h3>
        //                    <p style='margin-bottom: 0; color: #155724;'>Bây giờ bạn có thể sử dụng đầy đủ các dịch vụ của chúng tôi.</p>
        //                </div>

        //                <h3 style='color: #007bff;'>🌟 Những gì bạn có thể làm:</h3>
        //                <ul style='padding-left: 20px;'>
        //                    <li>Đặt lịch bảo dưỡng xe điện</li>
        //                    <li>Theo dõi lịch sử dịch vụ</li>
        //                    <li>Nhận thông báo nhắc nhở bảo dưỡng</li>
        //                    <li>Truy cập các ưu đãi đặc biệt</li>
        //                    <li>Liên hệ với đội ngũ hỗ trợ 24/7</li>
        //                </ul>

        //                <div style='text-align: center; margin: 35px 0;'>
        //                    <a href='{_configuration["AppSettings:WebsiteUrl"]}/login' 
        //                       style='background-color: #28a745; color: white; padding: 15px 35px; 
        //                              text-decoration: none; border-radius: 5px; display: inline-block;
        //                              font-weight: bold; font-size: 16px;'>
        //                        BẮT ĐẦU SỬ DỤNG NGAY
        //                    </a>
        //                </div>

        //                <div style='background-color: #cce5ff; border: 1px solid #b3d9ff; padding: 15px; border-radius: 5px; margin: 25px 0;'>
        //                    <h4 style='color: #004085; margin-top: 0;'>💡 Mẹo hữu ích:</h4>
        //                    <p style='margin-bottom: 0; color: #004085;'>
        //                        Thêm email <strong>{_configuration["Smtp:FromEmail"]}</strong> vào danh sách liên hệ để không bỏ lỡ thông báo quan trọng!
        //                    </p>
        //                </div>

        //                <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
        //                <p style='font-size: 12px; color: #666;'>
        //                    Cần hỗ trợ? Liên hệ với chúng tôi:<br>
        //                    📞 Hotline: {_configuration["AppSettings:SupportPhone"]}<br>
        //                    📧 Email: {_configuration["AppSettings:SupportEmail"]}
        //                </p>
        //                <p style='font-size: 12px; color: #666;'>
        //                    <strong>EV Service Center Team</strong><br>
        //                    {_configuration["AppSettings:CompanyAddress"]}
        //                </p>
        //            </div>
        //        </body>
        //        </html>";

        //    await SendEmailAsync(email, subject, htmlBody);
        //}

        public async Task SendWelcomeEmailAsync(string email, string fullName)
        {
            var subject = "Chào mừng đến với EV Service Center";

            var htmlBody = $@"
    <html>
    <head>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    </head>
    <body style='font-family: Arial, sans-serif; background-color: #f4f6f8; margin: 0; padding: 0;'>
        <div style='max-width: 600px; margin: 30px auto; background: #fff; border-radius: 10px; 
                    box-shadow: 0 4px 12px rgba(0,0,0,0.08); overflow: hidden;'>

            <!-- Header -->
            <div style='background: linear-gradient(90deg, #28a745, #20c997); padding: 25px; text-align: center; color: white;'>
                <h1 style='margin: 0; font-size: 28px;'>🎉 Chào mừng bạn!</h1>
                <p style='margin: 5px 0 0; font-size: 18px;'>EV Service Center</p>
            </div>

            <!-- Body -->
            <div style='padding: 30px; color: #333; line-height: 1.6;'>
                <p>Xin chào <strong>{fullName}</strong>,</p>
                <p>Chúc mừng bạn đã trở thành thành viên của <strong>EV Service Center</strong>! 🚗⚡</p>

                <div style='background-color: #e8f8f0; border-left: 5px solid #28a745; padding: 20px; 
                            border-radius: 6px; margin: 25px 0;'>
                    <h3 style='margin: 0; color: #155724;'>✅ Tài khoản của bạn đã được kích hoạt thành công!</h3>
                    <p style='margin: 8px 0 0; color: #155724;'>Bạn có thể bắt đầu sử dụng đầy đủ các dịch vụ của chúng tôi ngay bây giờ.</p>
                </div>

                <h3 style='color: #007bff;'>🌟 Những gì bạn có thể làm:</h3>
                <ul style='padding-left: 20px; margin: 15px 0;'>
                    <li>Đặt lịch bảo dưỡng xe điện</li>
                    <li>Theo dõi lịch sử dịch vụ</li>
                    <li>Nhận thông báo nhắc nhở bảo dưỡng</li>
                    <li>Truy cập các ưu đãi đặc biệt</li>
                    <li>Liên hệ với đội ngũ hỗ trợ 24/7</li>
                </ul>

                <div style='text-align: center; margin: 35px 0;'>
                    <a href='{_configuration["AppSettings:WebsiteUrl"]}/login' 
                       style='background: linear-gradient(90deg, #28a745, #20c997); color: white; 
                              padding: 15px 40px; text-decoration: none; border-radius: 30px;
                              font-weight: bold; font-size: 16px; box-shadow: 0 3px 6px rgba(0,0,0,0.2); 
                              display: inline-block;'>
                        🚀 BẮT ĐẦU NGAY
                    </a>
                </div>

                <div style='background-color: #f1f7ff; border-left: 5px solid #007bff; padding: 15px; 
                            border-radius: 6px; margin: 25px 0;'>
                    <h4 style='margin: 0; color: #004085;'>💡 Mẹo hữu ích:</h4>
                    <p style='margin: 8px 0 0; color: #004085;'>
                        Thêm email <strong>{_configuration["Smtp:FromEmail"]}</strong> vào danh bạ để không bỏ lỡ thông báo quan trọng!
                    </p>
                </div>
            </div>

            <!-- Footer -->
            <div style='background-color: #f9f9f9; padding: 20px; text-align: center; font-size: 13px; color: #666;'>
                <p style='margin: 0 0 8px;'>📞 {_configuration["AppSettings:SupportPhone"]} | 
                   📧 {_configuration["AppSettings:SupportEmail"]}</p>
                <p style='margin: 0;'><strong>EV Service Center Team</strong><br>{_configuration["AppSettings:CompanyAddress"]}</p>
            </div>
        </div>
    </body>
    </html>";

            await SendEmailAsync(email, subject, htmlBody);
        }


        public async Task SendNotificationAsync(string email, string subject, string message)
        {
            var htmlBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <div style='background-color: #007bff; color: white; padding: 20px; border-radius: 8px 8px 0 0; text-align: center;'>
                            <h2 style='margin: 0; font-size: 24px;'>📢 Thông báo từ EV Service Center</h2>
                        </div>
                        
                        <div style='background-color: #f8f9fa; padding: 30px; border-radius: 0 0 8px 8px; border: 1px solid #dee2e6; border-top: none;'>
                            <h3 style='color: #007bff; margin-top: 0;'>{subject}</h3>
                            
                            <div style='background-color: white; padding: 20px; border-radius: 5px; border-left: 4px solid #007bff; margin: 20px 0;'>
                                {FormatMessageContent(message)}
                            </div>
                            
                            <div style='background-color: #e7f3ff; border: 1px solid #b3d9ff; padding: 15px; border-radius: 5px; margin: 25px 0;'>
                                <p style='margin: 0; color: #004085; font-size: 14px;'>
                                    <strong>ℹ️ Lưu ý:</strong> Đây là thông báo tự động từ hệ thống EV Service Center. 
                                    Vui lòng không trả lời trực tiếp email này.
                                </p>
                            </div>
                        </div>
                        
                        <div style='text-align: center; margin: 25px 0;'>
                            <a href='{_configuration["AppSettings:WebsiteUrl"]}' 
                               style='background-color: #28a745; color: white; padding: 12px 25px; 
                                      text-decoration: none; border-radius: 5px; display: inline-block;
                                      font-weight: bold;'>
                                TRUY CẬP HỆ THỐNG
                            </a>
                        </div>
                        
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
                        <p style='font-size: 12px; color: #666; text-align: center;'>
                            <strong>EV Service Center</strong><br>
                            📧 {_configuration["AppSettings:SupportEmail"]} | 📞 {_configuration["AppSettings:SupportPhone"]}<br>
                            {_configuration["AppSettings:CompanyAddress"]}
                        </p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(email, subject, htmlBody);
        }

        private string FormatMessageContent(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return "<p>Không có nội dung.</p>";

            var paragraphs = message.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            var formattedContent = new StringBuilder();

            foreach (var paragraph in paragraphs)
            {
                var trimmedParagraph = paragraph.Trim();
                if (!string.IsNullOrEmpty(trimmedParagraph))
                {
                    var escapedParagraph = System.Net.WebUtility.HtmlEncode(trimmedParagraph);
                    formattedContent.AppendLine($"<p>{escapedParagraph}</p>");
                }
            }

            return formattedContent.ToString();
        }

        private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                var smtpHost = _configuration["Smtp:Host"];
                var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
                var smtpUsername = _configuration["Smtp:Username"];
                var smtpPassword = _configuration["Smtp:Password"];
                var fromEmail = _configuration["Smtp:FromEmail"];
                var fromName = _configuration["Smtp:FromName"];
                var enableSsl = bool.Parse(_configuration["Smtp:EnableSsl"] ?? "true");
                var timeout = int.Parse(_configuration["Smtp:Timeout"] ?? "30000");

                using var client = new SmtpClient(smtpHost, smtpPort);
                client.EnableSsl = enableSsl;
                client.UseDefaultCredentials = false;

                if (!string.IsNullOrEmpty(smtpUsername) && !string.IsNullOrEmpty(smtpPassword))
                {
                    client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                }

                client.Timeout = timeout;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail!, fromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true,
                    BodyEncoding = Encoding.UTF8,
                    SubjectEncoding = Encoding.UTF8
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email} with subject: {Subject}", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw new InvalidOperationException($"Email sending failed: {ex.Message}");
            }
        }
    }
}