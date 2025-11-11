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
        public async Task SendCustomerEmailVerificationAsync(string email, string fullName, string verificationToken)
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
                <body style='margin:0; padding:0; background-color:#fdfdfc; font-family: Georgia, Times New Roman, serif; color:#2c2c2c;'>
                    <div style='max-width:600px; margin:40px auto; background-color:#ffffff; border-radius:12px; 
                                border:1px solid #e6e6e6; box-shadow:0 4px 12px rgba(0,0,0,0.06); overflow:hidden;'>

                        <!-- Header -->
                        <div style='background-color:#1c1c1c; padding:30px; text-align:center; color:#d4af37;'>
                            <h1 style='margin:0; font-size:24px; font-weight:bold; letter-spacing:1px;'>EV Service Center Vietnam</h1>
                            <p style='margin:8px 0 0; font-size:15px;'>Xác thực tài khoản</p>
                        </div>

                        <!-- Content -->
                        <div style='padding:35px; font-size:15px; line-height:1.7;'>
                            <p>Kính gửi <strong>{fullName}</strong>,</p>

                            <p>Chúng tôi trân trọng cảm ơn bạn đã đăng ký tài khoản tại <strong>EV Service Center Vietnam</strong>.</p>
                            <p>Để hoàn tất quá trình đăng ký, vui lòng xác thực địa chỉ email của bạn bằng cách nhấn vào nút dưới đây:</p>

                            <!-- Button -->
                            <div style='text-align:center; margin:35px 0;'>
                                <a href='{verificationApiUrl}' 
                                    style='background-color:#d4af37; color:#1c1c1c; padding:14px 40px; 
                                            text-decoration:none; border-radius:40px; font-weight:bold; font-size:15px;
                                            display:inline-block; box-shadow:0 3px 6px rgba(0,0,0,0.15);'>
                                    XÁC THỰC EMAIL
                                </a>
                            </div>

                            <!-- Alternative link -->
                            <p>Nếu nút trên không hoạt động, vui lòng sao chép liên kết sau và dán vào trình duyệt của bạn:</p>
                            <p style='background-color:#faf9f7; padding:12px; border-radius:6px; 
                                        word-break:break-all; font-size:13px; border:1px solid #e2d9c3;'>
                                {verificationApiUrl}
                            </p>

                            <!-- Frontend fallback -->
                            <p style='font-size:13px; color:#555; margin-top:20px;'>
                                Hoặc xác thực thông qua giao diện web: <br>
                                <a href='{frontendVerificationUrl}' style='color:#8b7500;'>{frontendVerificationUrl}</a>
                            </p>

                            <!-- Notice -->
                            <div style='background-color:#fffbea; border:1px solid #e2d9c3; padding:15px; 
                                        border-radius:8px; margin:25px 0; font-size:14px;'>
                                <strong>⏰ Lưu ý:</strong> Liên kết xác thực sẽ hết hạn sau <strong>24 giờ</strong>.
                            </div>

                            <p style='margin-top:30px; font-size:14px;'>
                                Nếu bạn không đăng ký tài khoản này, vui lòng bỏ qua email.<br>
                                Đây là email tự động, vui lòng không trả lời.
                            </p>

                            <p style='margin-top:20px; font-size:14px;'>
                                Trân trọng,<br>
                                <strong>Đội ngũ EV Service Center Vietnam</strong><br>
                                {_configuration["AppSettings:CompanyAddress"]}
                            </p>
                        </div>

                        <!-- Footer -->
                        <div style='background-color:#1c1c1c; padding:20px; text-align:center; font-size:12px; color:#e0e0e0;'>
                            <p style='margin:6px 0 0; font-size:11px;'>© {DateTime.UtcNow.Year} EV Service Center Vietnam. All rights reserved.</p>
                        </div>
                    </div>
                </body>
            </html>";

            await SendEmailAsync(email, subject, htmlBody);
        }

        public async Task SendPasswordResetAsync(string email, string fullName, string resetToken)
        {
            var subject = "Đặt lại mật khẩu EV Service Center";
            var frontendUrl = _configuration["AppSettings:WebsiteUrl"] ?? "http://localhost:3000";
            var resetUrl = $"{frontendUrl}/reset-password?token={resetToken}&email={Uri.EscapeDataString(email)}";

            var htmlBody = $@"
    <html>
        <body style='margin:0; padding:0; background-color:#fdfdfc; font-family: Georgia, Times New Roman, serif; color:#2c2c2c;'>
            <div style='max-width:600px; margin:40px auto; background-color:#ffffff; border-radius:12px; 
                        border:1px solid #e6e6e6; box-shadow:0 4px 12px rgba(0,0,0,0.06); overflow:hidden;'>

                <!-- Header -->
                <div style='background-color:#1c1c1c; padding:30px; text-align:center; color:#d4af37;'>
                    <h1 style='margin:0; font-size:24px; font-weight:bold; letter-spacing:1px;'>EV Service Center Vietnam</h1>
                    <p style='margin:8px 0 0; font-size:15px;'>Yêu cầu đặt lại mật khẩu</p>
                </div>

                <!-- Content -->
                <div style='padding:35px; font-size:15px; line-height:1.7;'>
                    <p>Xin chào <strong>{fullName}</strong>,</p>
                    <p>Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu cho tài khoản <strong>{email}</strong>.</p>

                    <div style='text-align:center; margin:35px 0;'>
                        <a href='{resetUrl}'
                           style='background-color:#d4af37; color:#1c1c1c; padding:14px 40px; 
                                  text-decoration:none; border-radius:40px; font-weight:bold; font-size:15px;
                                  display:inline-block; box-shadow:0 3px 6px rgba(0,0,0,0.15);'>
                            ĐẶT LẠI MẬT KHẨU
                        </a>
                    </div>

                    <p>Nếu nút trên không hoạt động, sao chép liên kết sau và dán vào trình duyệt:</p>
                    <p style='background-color:#faf9f7; padding:12px; border-radius:6px; 
                              word-break:break-all; font-size:13px; border:1px solid #e2d9c3;'>
                        {resetUrl}
                    </p>

                    <div style='background-color:#fffbea; border:1px solid #e2d9c3; padding:15px; 
                                border-radius:8px; margin:25px 0; font-size:14px;'>
                        <strong>⚠️ Bảo mật:</strong> Liên kết sẽ hết hạn sau <strong>1 giờ</strong>.<br>
                        Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.
                    </div>

                    <p style='margin-top:30px; font-size:14px;'>
                        Trân trọng,<br>
                        <strong>Đội ngũ EV Service Center Vietnam</strong>
                    </p>
                </div>

                <!-- Footer -->
                <div style='background-color:#1c1c1c; padding:20px; text-align:center; font-size:12px; color:#e0e0e0;'>
                    <p style='margin:4px 0;'>Email hỗ trợ: {_configuration["AppSettings:SupportEmail"]}</p>
                    <p style='margin:4px 0;'>Hotline: {_configuration["AppSettings:SupportPhone"]}</p>
                    <p style='margin:6px 0 0; font-size:11px;'>© {DateTime.UtcNow.Year} EV Service Center Vietnam. All rights reserved.</p>
                </div>
            </div>
        </body>
    </html>";

            await SendEmailAsync(email, subject, htmlBody);
        }


        public async Task SendWelcomeEmailAsync(string email, string fullName)
        {
            var subject = "Chào mừng đến với EV Service Center";

            var htmlBody = $@"
    <html>
    <head>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    </head>
    <body style='margin:0; padding:0; background-color:#fdfdfc; font-family: Georgia, Times New Roman, serif; color:#2c2c2c;'>
        <div style='max-width:600px; margin:40px auto; background-color:#ffffff; border-radius:12px; 
                    border:1px solid #e6e6e6; box-shadow:0 4px 12px rgba(0,0,0,0.06); overflow:hidden;'>

            <!-- Header -->
            <div style='background-color:#1c1c1c; padding:30px; text-align:center; color:#d4af37;'>
                <h1 style='margin:0; font-size:24px; font-weight:bold; letter-spacing:1px;'>EV Service Center Vietnam</h1>
                <p style='margin:8px 0 0; font-size:15px;'>Chào mừng bạn!</p>
            </div>

            <!-- Body -->
            <div style='padding:35px; font-size:15px; line-height:1.7;'>
                <p>Xin chào <strong>{fullName}</strong>,</p>
                <p>Chúc mừng bạn đã trở thành thành viên của <strong>EV Service Center Vietnam</strong>!</p>

                <div style='background-color:#faf9f7; border:1px solid #e2d9c3; padding:20px; border-radius:10px; margin:20px 0;'>
                    <h3 style='margin:0 0 8px 0; color:#8b7500;'>✅ Tài khoản của bạn đã được kích hoạt</h3>
                    <p style='margin:8px 0 0 0; color:#2c2c2c;'>Bạn có thể bắt đầu sử dụng các dịch vụ của chúng tôi ngay bây giờ.</p>
                </div>

                <h3 style='color:#8b7500;'>Những gì bạn có thể làm:</h3>
                <ul style='padding-left:20px; margin:10px 0;'>
                    <li>Đặt lịch bảo dưỡng xe điện</li>
                    <li>Theo dõi lịch sử dịch vụ</li>
                    <li>Nhận thông báo nhắc nhở bảo dưỡng</li>
                    <li>Truy cập các ưu đãi đặc biệt</li>
                    <li>Liên hệ với đội ngũ hỗ trợ 24/7</li>
                </ul>

                <div style='text-align:center; margin:30px 0;'>
                    <a href='{_configuration["AppSettings:WebsiteUrl"]}/login' 
                       style='background-color:#d4af37; color:#1c1c1c; padding:14px 40px; 
                              text-decoration:none; border-radius:40px; font-weight:bold; font-size:15px;
                              display:inline-block; box-shadow:0 3px 6px rgba(0,0,0,0.15);'>
                        🚀 BẮT ĐẦU NGAY
                    </a>
                </div>

                <div style='background-color:#fffbea; border-left:5px solid #d4af37; padding:15px; border-radius:8px; margin:20px 0;'>
                    <h4 style='margin:0 0 8px 0; color:#7a5c00;'>Mẹo hữu ích</h4>
                    <p style='margin:0;'>Thêm email <strong>{_configuration["Smtp:FromEmail"]}</strong> vào danh bạ để không bỏ lỡ thông báo quan trọng.</p>
                </div>

                <p style='margin-top:20px;'>
                    Trân trọng,<br>
                    <strong>Đội ngũ EV Service Center Vietnam</strong>
                </p>
            </div>

            <!-- Footer -->
            <div style='background-color:#1c1c1c; padding:20px; text-align:center; font-size:12px; color:#e0e0e0;'>
                <p style='margin:4px 0;'>📞 {_configuration["AppSettings:SupportPhone"]} | 📧 {_configuration["AppSettings:SupportEmail"]}</p>
                <p style='margin:4px 0;'><strong>EV Service Center Vietnam</strong><br>{_configuration["AppSettings:CompanyAddress"]}</p>
                <p style='margin:6px 0 0; font-size:11px;'>© {DateTime.UtcNow.Year} EV Service Center Vietnam. All rights reserved.</p>
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
                <body style='margin:0; padding:0; background-color:#fdfdfc; font-family: Georgia, Times New Roman, serif; color:#2c2c2c;'>
                    <div style='max-width:600px; margin:40px auto; background-color:#ffffff; border-radius:12px; 
                                border:1px solid #e6e6e6; box-shadow:0 4px 12px rgba(0,0,0,0.06); overflow:hidden; padding-bottom:20px;'>

                        <div style='background-color:#1c1c1c; padding:30px; text-align:center; color:#d4af37;'>
                            <h2 style='margin:0; font-size:20px; font-weight:bold;'>Thông báo từ EV Service Center Vietnam</h2>
                        </div>

                        <div style='padding:30px; font-size:15px; line-height:1.7;'>
                            <h3 style='color:#8b7500; margin-top:0;'>{subject}</h3>

                            <div style='background-color:#faf9f7; border:1px solid #e2d9c3; padding:20px; border-radius:8px; margin:15px 0;'>
                                {FormatMessageContent(message)}
                            </div>

                            <div style='background-color:#fffbea; border:1px solid #e2d9c3; padding:12px; border-radius:8px; margin:20px 0;'>
                                <p style='margin:0; font-size:14px; color:#7a5c00;'><strong>ℹ️ Lưu ý:</strong> Đây là thông báo tự động. Vui lòng không trả lời email này.</p>
                            </div>

                            <div style='text-align:center; margin-top:20px;'>
                                <a href='{_configuration["AppSettings:WebsiteUrl"]}' 
                                   style='background-color:#d4af37; color:#1c1c1c; padding:12px 30px; 
                                          text-decoration:none; border-radius:30px; font-weight:bold; display:inline-block; box-shadow:0 3px 6px rgba(0,0,0,0.15);'>
                                    TRUY CẬP HỆ THỐNG
                                </a>
                            </div>
                        </div>

                        <div style='background-color:#1c1c1c; padding:18px; text-align:center; font-size:12px; color:#e0e0e0;'>
                            <p style='margin:4px 0;'>📧 {_configuration["AppSettings:SupportEmail"]} | 📞 {_configuration["AppSettings:SupportPhone"]}</p>
                            <p style='margin:4px 0;'><strong>EV Service Center Vietnam</strong><br>{_configuration["AppSettings:CompanyAddress"]}</p>
                        </div>
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
                //  var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
                int.TryParse(_configuration["Smtp:Port"], out var smtpPort);
                if (smtpPort == 0) smtpPort = 587;

                var smtpUsername = _configuration["Smtp:Username"];
                var smtpPassword = _configuration["Smtp:Password"];
                var fromEmail = _configuration["Smtp:FromEmail"];
                var fromName = _configuration["Smtp:FromName"];
                // var enableSsl = bool.Parse(_configuration["Smtp:EnableSsl"] ?? "true");
                // var timeout = int.Parse(_configuration["Smtp:Timeout"] ?? "30000");
                bool.TryParse(_configuration["Smtp:EnableSsl"], out var enableSsl);
                int.TryParse(_configuration["Smtp:Timeout"], out var timeout);
                if (timeout == 0) timeout = 30000;

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

        public async Task SendInternalStaffWelcomeEmailAsync(string email, string fullName, string username, string role, string department)
        {
            var subject = "Chào mừng gia nhập đội ngũ EV Service Center";

            var roleDisplayName = role switch
            {
                "Admin" => "Quản trị viên",
                "Staff" => "Nhân viên",
                "Technician" => "Kỹ thuật viên",
                _ => role
            };

            var htmlBody = $@"
            <html>
            <head>
                 <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            </head>
            <body style='margin:0; padding:0; background-color:#fdfdfc; font-family: Georgia, Times New Roman, serif; color:#2c2c2c;'>
                <div style='max-width:600px; margin:40px auto; background-color:#ffffff; border-radius:12px; 
                            border:1px solid #e6e6e6; box-shadow:0 4px 12px rgba(0,0,0,0.06); overflow:hidden;'>

                    <!-- Header -->
                    <div style='background-color:#1c1c1c; padding:30px; text-align:center; color:#d4af37;'>
                        <h1 style='margin:0; font-size:24px; font-weight:bold; letter-spacing:1px;'>EV Service Center Vietnam</h1>
                        <p style='margin:8px 0 0; font-size:15px;'>Thư Chào Mừng Thành Viên Mới</p>
                    </div>

                    <!-- Body -->
                    <div style='padding:35px; font-size:15px; line-height:1.7;'>
                        <p>Kính gửi <strong>{fullName}</strong>,</p>
            
                        <p>Chúng tôi hân hoan chào đón bạn gia nhập <strong>EV Service Center Vietnam</strong> với vị trí <strong style='color:#b8860b;'>{roleDisplayName}</strong>. Đây là một dấu mốc quan trọng, và chúng tôi tin tưởng rằng bạn sẽ góp phần tạo nên những thành tựu nổi bật.</p>

                        <!-- Account Info -->
                        <div style='background-color:#faf9f7; border:1px solid #e2d9c3; border-radius:10px; padding:20px; margin:25px 0;'>
                            <h3 style='margin:0 0 12px 0; color:#8b7500;'>Thông tin tài khoản</h3>
                            <p><strong>Tên đăng nhập:</strong> {username}</p>
                            <p><strong>Vai trò:</strong> {roleDisplayName}</p>
                            {(string.IsNullOrEmpty(department) ? "" : $"<p><strong>Phòng ban:</strong> {department}</p>")}
                            <p><strong>Email:</strong> {email}</p>
                        </div>

                        <h3 style='color:#8b7500; margin-top:30px;'>Bước tiếp theo</h3>
                        <ol style='margin:15px 0; padding-left:20px;'>
                            <li>Đăng nhập vào hệ thống bằng thông tin được cấp</li>
                            <li>Cập nhật hồ sơ cá nhân</li>
                            <li>Làm quen với môi trường làm việc và công cụ</li>
                            <li>Liên hệ quản lý để được định hướng chi tiết</li>
                        </ol>

                        <div style='text-align:center; margin:35px 0;'>
                            <a href='{_configuration["AppSettings:WebsiteUrl"]}/login' 
                               style='background-color:#d4af37; color:#1c1c1c; padding:14px 40px; 
                                      text-decoration:none; border-radius:40px; font-weight:bold; font-size:15px; display:inline-block; box-shadow:0 3px 6px rgba(0,0,0,0.15);'>
                                ĐĂNG NHẬP NGAY
                            </a>
                        </div>

                        <div style='background-color:#fffbea; border-left:5px solid #d4af37; padding:15px; border-radius:8px; margin:25px 0;'>
                            <h4 style='margin:0 0 8px 0; color:#7a5c00;'>Lưu ý bảo mật</h4>
                            <ul style='margin:5px 0 0 20px;'>
                                <li>Không chia sẻ thông tin đăng nhập</li>
                                <li>Đổi mật khẩu định kỳ</li>
                                <li>Đăng xuất khi không sử dụng</li>
                            </ul>
                        </div>

                        <p style='margin-top:25px;'>Chúng tôi trân trọng kỳ vọng vào sự đồng hành của bạn trong hành trình phát triển chung.</p>
            
                        <p style='margin-top:20px;'>Trân trọng,<br>
                        <strong>Ban Giám đốc EV Service Center Vietnam</strong></p>
                    </div>

                    <!-- Footer -->
                    <div style='background-color:#1c1c1c; padding:20px; text-align:center; font-size:12px; color:#e0e0e0;'>
                        <p style='margin:4px 0;'>📞 {_configuration["AppSettings:SupportPhone"]} | 📧 {_configuration["AppSettings:SupportEmail"]}</p>
                        <p style='margin:4px 0;'><strong>EV Service Center Vietnam</strong><br>{_configuration["AppSettings:CompanyAddress"]}</p>
                        <p style='margin:6px 0 0; font-size:11px;'>© {DateTime.UtcNow.Year} EV Service Center Vietnam. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";
            await SendEmailAsync(email, subject, htmlBody);
        }
    }
}
