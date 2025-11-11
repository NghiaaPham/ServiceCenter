using System.Text;
using System.Text.RegularExpressions;

namespace EVServiceCenter.Infrastructure.Services
{
    /// <summary>
    /// 🎨 Enhancement #1: Email Template Engine
    /// Simple but effective template rendering with variable substitution
    /// </summary>
    public class EmailTemplateEngine
    {
        private readonly string _templateBasePath;

        public EmailTemplateEngine(string templateBasePath = "EmailTemplates")
        {
            _templateBasePath = templateBasePath;
        }

        /// <summary>
        /// Render template với data
        /// </summary>
        public async Task<string> RenderAsync(
            string templateName,
            Dictionary<string, string> data,
            CancellationToken cancellationToken = default)
        {
            // Load base template
            var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _templateBasePath);
            var baseTemplatePath = Path.Combine(basePath, "BaseEmailTemplate.html");
            var contentTemplatePath = Path.Combine(basePath, $"{templateName}.html");

            if (!File.Exists(baseTemplatePath))
                throw new FileNotFoundException($"Base template not found: {baseTemplatePath}");

            if (!File.Exists(contentTemplatePath))
                throw new FileNotFoundException($"Content template not found: {contentTemplatePath}");

            var baseHtml = await File.ReadAllTextAsync(baseTemplatePath, cancellationToken);
            var contentHtml = await File.ReadAllTextAsync(contentTemplatePath, cancellationToken);

            // Process conditional blocks in content
            contentHtml = ProcessConditionals(contentHtml, data);

            // Replace variables in content
            contentHtml = ReplaceVariables(contentHtml, data);

            // Insert content into base template
            baseHtml = baseHtml.Replace("{{CONTENT}}", contentHtml);

            // Replace variables in base template
            baseHtml = ReplaceVariables(baseHtml, data);

            return baseHtml;
        }

        /// <summary>
        /// Process conditional blocks: {{#IF_REFUND}}...{{/IF_REFUND}}
        /// </summary>
        private string ProcessConditionals(string html, Dictionary<string, string> data)
        {
            var pattern = @"\{\{#IF_([A-Z_]+)\}\}(.*?)\{\{/IF_\1\}\}";
            var regex = new Regex(pattern, RegexOptions.Singleline);

            return regex.Replace(html, match =>
            {
                var conditionKey = match.Groups[1].Value;
                var content = match.Groups[2].Value;

                // Check if condition key exists in data and has value
                if (data.ContainsKey(conditionKey) && !string.IsNullOrEmpty(data[conditionKey]))
                {
                    return content; // Include content
                }

                return string.Empty; // Exclude content
            });
        }

        /// <summary>
        /// Replace {{VARIABLE}} with actual values
        /// </summary>
        private string ReplaceVariables(string html, Dictionary<string, string> data)
        {
            foreach (var kvp in data)
            {
                var placeholder = $"{{{{{kvp.Key}}}}}";
                html = html.Replace(placeholder, kvp.Value ?? string.Empty);
            }

            return html;
        }

        /// <summary>
        /// Build appointment confirmation email
        /// </summary>
        public async Task<string> BuildAppointmentConfirmationAsync(
            string customerName,
            string appointmentCode,
            DateTime appointmentDate,
            string vehicleInfo,
            string servicesList,
            decimal estimatedCost,
            string viewUrl,
            CancellationToken cancellationToken = default)
        {
            var data = new Dictionary<string, string>
            {
                ["SUBJECT"] = "Xác nhận đặt lịch thành công",
                ["CUSTOMER_NAME"] = customerName,
                ["APPOINTMENT_CODE"] = appointmentCode,
                ["APPOINTMENT_DATE"] = appointmentDate.ToString("dd/MM/yyyy"),
                ["APPOINTMENT_TIME"] = appointmentDate.ToString("HH:mm"),
                ["VEHICLE_INFO"] = vehicleInfo,
                ["SERVICES_LIST"] = servicesList,
                ["ESTIMATED_COST"] = estimatedCost.ToString("N0"),
                ["VIEW_APPOINTMENT_URL"] = viewUrl,
                ["SERVICE_CENTER_ADDRESS"] = "123 Đường ABC, Quận XYZ, TP. HCM"
            };

            return await RenderAsync("AppointmentConfirmation", data, cancellationToken);
        }

        /// <summary>
        /// Build appointment cancellation email
        /// </summary>
        public async Task<string> BuildAppointmentCancellationAsync(
            string customerName,
            string appointmentCode,
            DateTime appointmentDate,
            string cancellationReason,
            DateTime cancellationTime,
            decimal? paidAmount = null,
            decimal? refundAmount = null,
            string? refundMethod = null,
            string? bookAgainUrl = null,
            CancellationToken cancellationToken = default)
        {
            var data = new Dictionary<string, string>
            {
                ["SUBJECT"] = "Xác nhận hủy lịch hẹn",
                ["CUSTOMER_NAME"] = customerName,
                ["APPOINTMENT_CODE"] = appointmentCode,
                ["APPOINTMENT_DATE"] = appointmentDate.ToString("dd/MM/yyyy"),
                ["APPOINTMENT_TIME"] = appointmentDate.ToString("HH:mm"),
                ["CANCELLATION_REASON"] = cancellationReason,
                ["CANCELLATION_TIME"] = cancellationTime.ToString("dd/MM/yyyy HH:mm"),
                ["BOOK_AGAIN_URL"] = bookAgainUrl ?? "#",
                ["SERVICE_CENTER_ADDRESS"] = "123 Đường ABC, Quận XYZ, TP. HCM"
            };

            // Add refund info if applicable
            if (paidAmount.HasValue && refundAmount.HasValue)
            {
                data["IF_REFUND"] = "true";
                data["PAID_AMOUNT"] = paidAmount.Value.ToString("N0");
                data["REFUND_AMOUNT"] = refundAmount.Value.ToString("N0");
                data["REFUND_METHOD"] = refundMethod ?? "Chuyển khoản ngân hàng";
            }

            return await RenderAsync("AppointmentCancellation", data, cancellationToken);
        }

        /// <summary>
        /// Build appointment reminder email
        /// </summary>
        public async Task<string> BuildAppointmentReminderAsync(
            string customerName,
            string appointmentCode,
            DateTime appointmentDate,
            string vehicleInfo,
            string servicesList,
            int hoursUntil,
            string viewUrl,
            string rescheduleUrl,
            CancellationToken cancellationToken = default)
        {
            var data = new Dictionary<string, string>
            {
                ["SUBJECT"] = "Nhắc nhở: Lịch hẹn sắp tới",
                ["CUSTOMER_NAME"] = customerName,
                ["APPOINTMENT_CODE"] = appointmentCode,
                ["APPOINTMENT_DATE"] = appointmentDate.ToString("dd/MM/yyyy"),
                ["APPOINTMENT_TIME"] = appointmentDate.ToString("HH:mm"),
                ["HOURS_UNTIL"] = hoursUntil.ToString(),
                ["VEHICLE_INFO"] = vehicleInfo,
                ["SERVICES_LIST"] = servicesList,
                ["VIEW_APPOINTMENT_URL"] = viewUrl,
                ["RESCHEDULE_URL"] = rescheduleUrl,
                ["SERVICE_CENTER_ADDRESS"] = "123 Đường ABC, Quận XYZ, TP. HCM"
            };

            return await RenderAsync("AppointmentReminder", data, cancellationToken);
        }

        /// <summary>
        /// Build appointment completion email
        /// </summary>
        public async Task<string> BuildAppointmentCompletionAsync(
            string customerName,
            string appointmentCode,
            DateTime completionDate,
            string technicianName,
            string? invoiceNumber = null,
            decimal? totalAmount = null,
            decimal? paidAmount = null,
            decimal? outstandingAmount = null,
            string? workCompletedList = null,
            string? recommendationsList = null,
            string? invoicePdfUrl = null,
            string? rateServiceUrl = null,
            string? nextServiceDate = null,
            CancellationToken cancellationToken = default)
        {
            var data = new Dictionary<string, string>
            {
                ["SUBJECT"] = "Hoàn thành dịch vụ",
                ["CUSTOMER_NAME"] = customerName,
                ["APPOINTMENT_CODE"] = appointmentCode,
                ["COMPLETION_DATE"] = completionDate.ToString("dd/MM/yyyy HH:mm"),
                ["TECHNICIAN_NAME"] = technicianName,
                ["WORK_COMPLETED_LIST"] = workCompletedList ?? "Đã hoàn thành các dịch vụ theo yêu cầu",
                ["RECOMMENDATIONS_LIST"] = recommendationsList ?? "Xe của bạn đang trong tình trạng tốt",
                ["RATE_SERVICE_URL"] = rateServiceUrl ?? "#",
                ["NEXT_SERVICE_DATE"] = nextServiceDate ?? "Sẽ thông báo sau",
                ["SERVICE_CENTER_ADDRESS"] = "123 Đường ABC, Quận XYZ, TP. HCM"
            };

            // Add invoice info if applicable
            if (!string.IsNullOrEmpty(invoiceNumber) && totalAmount.HasValue)
            {
                data["IF_INVOICE"] = "true";
                data["INVOICE_NUMBER"] = invoiceNumber;
                data["TOTAL_AMOUNT"] = totalAmount.Value.ToString("N0");
                data["PAID_AMOUNT"] = (paidAmount ?? 0).ToString("N0");
                data["INVOICE_PDF_URL"] = invoicePdfUrl ?? "#";

                if (outstandingAmount.HasValue && outstandingAmount > 0)
                {
                    data["IF_OUTSTANDING"] = "true";
                    data["OUTSTANDING_AMOUNT"] = outstandingAmount.Value.ToString("N0");
                }
            }

            return await RenderAsync("AppointmentCompletion", data, cancellationToken);
        }
    }
}
