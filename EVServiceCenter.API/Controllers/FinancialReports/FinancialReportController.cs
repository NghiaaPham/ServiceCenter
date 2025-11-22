using EVServiceCenter.Core.Domains.FinancialReports.DTOs.Requests;
using EVServiceCenter.Core.Domains.FinancialReports.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace EVServiceCenter.API.Controllers.FinancialReports;

/// <summary>
/// Financial Reports Management
/// Provides comprehensive financial analytics and reporting
/// Admin/Manager only access
/// </summary>
[ApiController]
[Route("api/financial-reports")]
[ApiExplorerSettings(GroupName = "Financial Reports")]
[Authorize(Roles = "Admin,Manager")]
public class FinancialReportController : ControllerBase
{
    private readonly IFinancialReportService _financialReportService;
    private readonly ILogger<FinancialReportController> _logger;
    private static readonly string[] AllowedGroupBy = new[] { "Daily", "Weekly", "Monthly" };

    public FinancialReportController(
        IFinancialReportService financialReportService,
        ILogger<FinancialReportController> logger)
    {
        _financialReportService = financialReportService;
        _logger = logger;
    }

    /// <summary>
    /// [Revenue Report] Generate comprehensive revenue report
    /// Supports daily, weekly, and monthly grouping with multiple breakdowns
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/financial-reports/revenue?startDate=2025-01-01&amp;endDate=2025-01-31&amp;groupBy=Daily
    ///
    /// Query parameters:
    /// - startDate (required): Start date for analysis (ISO 8601)
    /// - endDate (required): End date for analysis (ISO 8601)
    /// - groupBy: Daily | Weekly | Monthly (default: Daily)
    /// - centerId: Filter by specific service center (optional)
    /// - paymentMethod: Cash | BankTransfer | VNPay | MoMo | Card (optional)
    /// - includePaymentMethodBreakdown: true | false (default: true)
    /// - includeServiceCenterBreakdown: true | false (default: false)
    ///
    /// Returns:
    /// - Summary metrics (total revenue, payment count, collection rate)
    /// - Time series data (daily/weekly/monthly breakdown)
    /// - Payment method breakdown (optional)
    /// - Service center breakdown (optional)
    /// - Growth rate vs previous period
    /// </remarks>
    [HttpGet("revenue")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetRevenueReport(
            [FromQuery] RevenueReportQueryDto query,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!IsValidDateRange(query.StartDate, query.EndDate, out var dateError))
                    return BadRequest(new { success = false, message = dateError });
                if (!IsValidGroupBy(query.GroupBy))
                    return BadRequest(new { success = false, message = "groupBy must be one of: Daily, Weekly, Monthly" });

                _logger.LogInformation(
                    "Revenue report requested: {StartDate} to {EndDate}, GroupBy: {GroupBy}",
                    query.StartDate, query.EndDate, query.GroupBy);

            var report = await _financialReportService.GenerateRevenueReportAsync(query, cancellationToken);

            return Ok(new
            {
                success = true,
                data = report,
                message = "Revenue report generated successfully"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid revenue report parameters: {Message}", ex.Message);
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating revenue report");
            return StatusCode(500, new
            {
                success = false,
                message = "Error generating revenue report"
            });
        }
    }

    /// <summary>
    /// [Quick Summary] Get today's revenue summary
    /// Fast endpoint for dashboard display
    /// </summary>
    [HttpGet("revenue/today")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTodayRevenueSummary(CancellationToken cancellationToken)
    {
        try
        {
            var today = DateTime.Today;
            var query = new RevenueReportQueryDto
            {
                StartDate = today,
                EndDate = today.AddDays(1).AddSeconds(-1),
                GroupBy = "Daily",
                IncludePaymentMethodBreakdown = true,
                IncludeServiceCenterBreakdown = false
            };

            var report = await _financialReportService.GenerateRevenueReportAsync(query, cancellationToken);

            return Ok(new
            {
                success = true,
                data = new
                {
                    date = today,
                    totalRevenue = report.TotalRevenue,
                    paymentCount = report.TotalPaymentCount,
                    averagePaymentAmount = report.AveragePaymentAmount,
                    collectionRate = report.CollectionRate,
                    paymentMethodBreakdown = report.PaymentMethodBreakdown
                },
                message = "Today's revenue summary"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting today's revenue summary");
            return StatusCode(500, new
            {
                success = false,
                message = "Error retrieving today's revenue"
            });
        }
    }

    /// <summary>
    /// [Quick Summary] Get current month's revenue summary
    /// Fast endpoint for monthly dashboard
    /// </summary>
    [HttpGet("revenue/this-month")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetThisMonthRevenueSummary(CancellationToken cancellationToken)
    {
        try
        {
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var query = new RevenueReportQueryDto
            {
                StartDate = startOfMonth,
                EndDate = endOfMonth,
                GroupBy = "Daily",
                IncludePaymentMethodBreakdown = true,
                IncludeServiceCenterBreakdown = true
            };

            var report = await _financialReportService.GenerateRevenueReportAsync(query, cancellationToken);

            return Ok(new
            {
                success = true,
                data = new
                {
                    month = startOfMonth.ToString("MMMM yyyy"),
                    totalRevenue = report.TotalRevenue,
                    paymentCount = report.TotalPaymentCount,
                    averagePaymentAmount = report.AveragePaymentAmount,
                    collectionRate = report.CollectionRate,
                    growthRate = report.GrowthRate,
                    timeSeries = report.TimeSeries,
                    paymentMethodBreakdown = report.PaymentMethodBreakdown,
                    serviceCenterBreakdown = report.ServiceCenterBreakdown
                },
                message = "This month's revenue summary"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting this month's revenue summary");
            return StatusCode(500, new
            {
                success = false,
                message = "Error retrieving monthly revenue"
            });
        }
    }

    /// <summary>
    /// [Comparison] Compare revenue between two periods
    /// Useful for year-over-year or quarter-over-quarter analysis
    /// </summary>
    [HttpGet("revenue/compare")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompareRevenuePeriods(
        [FromQuery] DateTime period1Start,
        [FromQuery] DateTime period1End,
        [FromQuery] DateTime period2Start,
        [FromQuery] DateTime period2End,
        [FromQuery] string groupBy = "Daily",
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!IsValidDateRange(period1Start, period1End, out var err1))
                return BadRequest(new { success = false, message = err1 });
            if (!IsValidDateRange(period2Start, period2End, out var err2))
                return BadRequest(new { success = false, message = err2 });
            if (!IsValidGroupBy(groupBy))
                return BadRequest(new { success = false, message = "groupBy must be one of: Daily, Weekly, Monthly" });

            // Validate periods have same length
            var period1Length = (period1End - period1Start).TotalDays;
            var period2Length = (period2End - period2Start).TotalDays;

            if (Math.Abs(period1Length - period2Length) > 1)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Periods must have similar lengths for meaningful comparison"
                });
            }

            var query1 = new RevenueReportQueryDto
            {
                StartDate = period1Start,
                EndDate = period1End,
                GroupBy = groupBy,
                IncludePaymentMethodBreakdown = true,
                IncludeServiceCenterBreakdown = false
            };

            var query2 = new RevenueReportQueryDto
            {
                StartDate = period2Start,
                EndDate = period2End,
                GroupBy = groupBy,
                IncludePaymentMethodBreakdown = true,
                IncludeServiceCenterBreakdown = false
            };

            // ✅ FIXED: Sequential execution to avoid DbContext threading issues
            var report1 = await _financialReportService.GenerateRevenueReportAsync(query1, cancellationToken);
            var report2 = await _financialReportService.GenerateRevenueReportAsync(query2, cancellationToken);

            var revenueDifference = report2.TotalRevenue - report1.TotalRevenue;
            var percentageChange = report1.TotalRevenue > 0
                ? Math.Round((revenueDifference / report1.TotalRevenue) * 100, 2)
                : 0;

            return Ok(new
            {
                success = true,
                data = new
                {
                    period1 = new
                    {
                        startDate = period1Start,
                        endDate = period1End,
                        totalRevenue = report1.TotalRevenue,
                        paymentCount = report1.TotalPaymentCount,
                        paymentMethodBreakdown = report1.PaymentMethodBreakdown
                    },
                    period2 = new
                    {
                        startDate = period2Start,
                        endDate = period2End,
                        totalRevenue = report2.TotalRevenue,
                        paymentCount = report2.TotalPaymentCount,
                        paymentMethodBreakdown = report2.PaymentMethodBreakdown
                    },
                    comparison = new
                    {
                        revenueDifference,
                        percentageChange,
                        trend = percentageChange > 0 ? "Up" : percentageChange < 0 ? "Down" : "Flat"
                    }
                },
                message = "Period comparison completed"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing revenue periods");
            return StatusCode(500, new
            {
                success = false,
                message = "Error comparing periods"
            });
        }
    }

    #region Payment Reports

    /// <summary>
    /// [Payment Analytics] Generate comprehensive payment analytics report
    /// Includes status distribution, gateway performance, and failure analysis
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/financial-reports/payments?startDate=2025-01-01&amp;endDate=2025-01-31
    ///
    /// Query parameters:
    /// - startDate (required): Start date for analysis
    /// - endDate (required): End date for analysis
    /// - centerId: Filter by specific service center (optional)
    /// - paymentMethod: Cash | BankTransfer | VNPay | MoMo (optional)
    /// - status: Pending | Completed | Failed (optional)
    /// - includeGatewayMetrics: true | false (default: true)
    /// - includeFailureAnalysis: true | false (default: true)
    ///
    /// Returns:
    /// - Summary metrics (total payments, success/failure rates)
    /// - Payment status distribution
    /// - Payment method breakdown
    /// - Gateway performance comparison (VNPay vs MoMo)
    /// - Failed payment analysis by error code
    /// </remarks>
    [HttpGet("payments")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaymentReport(
        [FromQuery] PaymentReportQueryDto query,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!IsValidDateRange(query.StartDate, query.EndDate, out var dateError))
                return BadRequest(new { success = false, message = dateError });

            _logger.LogInformation(
                "Payment report requested: {StartDate} to {EndDate}",
                query.StartDate, query.EndDate);

            var report = await _financialReportService.GeneratePaymentReportAsync(query, cancellationToken);

            return Ok(new
            {
                success = true,
                data = report,
                message = "Payment analytics report generated successfully"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid payment report parameters: {Message}", ex.Message);
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating payment report");
            return StatusCode(500, new
            {
                success = false,
                message = "Error generating payment report"
            });
        }
    }

    /// <summary>
    /// [Quick Summary] Get gateway comparison summary
    /// Compare VNPay vs MoMo performance
    /// </summary>
    [HttpGet("payments/gateway-comparison")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGatewayComparison(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!IsValidDateRange(startDate, endDate, out var dateError))
                return BadRequest(new { success = false, message = dateError });

            var query = new PaymentReportQueryDto
            {
                StartDate = startDate,
                EndDate = endDate,
                IncludeGatewayMetrics = true,
                IncludeFailureAnalysis = false
            };

            var report = await _financialReportService.GeneratePaymentReportAsync(query, cancellationToken);

            return Ok(new
            {
                success = true,
                data = new
                {
                    period = new { startDate, endDate },
                    overallSuccessRate = report.OverallSuccessRate,
                    gatewayPerformance = report.GatewayPerformance,
                    mostReliableGateway = report.MostReliableGateway
                },
                message = "Gateway comparison completed"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating gateway comparison");
            return StatusCode(500, new
            {
                success = false,
                message = "Error retrieving gateway comparison"
            });
        }
    }

    /// <summary>
    /// [Quick Summary] Get today's payment summary
    /// Fast endpoint for dashboard display
    /// </summary>
    [HttpGet("payments/today")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTodayPaymentSummary(CancellationToken cancellationToken)
    {
        try
        {
            var today = DateTime.Today;
            var query = new PaymentReportQueryDto
            {
                StartDate = today,
                EndDate = today.AddDays(1).AddSeconds(-1),
                IncludeGatewayMetrics = true,
                IncludeFailureAnalysis = true
            };

            var report = await _financialReportService.GeneratePaymentReportAsync(query, cancellationToken);

            return Ok(new
            {
                success = true,
                data = new
                {
                    date = today,
                    totalPayments = report.TotalPayments,
                    successfulPayments = report.SuccessfulPayments,
                    failedPayments = report.FailedPayments,
                    successRate = report.OverallSuccessRate,
                    totalAmount = report.TotalAmount,
                    mostUsedMethod = report.MostUsedPaymentMethod,
                    gatewayPerformance = report.GatewayPerformance
                },
                message = "Today's payment summary"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting today's payment summary");
            return StatusCode(500, new
            {
                success = false,
                message = "Error retrieving today's payments"
            });
        }
    }

    #endregion

    #region Invoice Reports

    /// <summary>
    /// [Invoice Analytics] Generate comprehensive invoice analytics report
    /// Includes status distribution, aging analysis, discount effectiveness, and tax summary
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/financial-reports/invoices?startDate=2025-01-01&amp;endDate=2025-01-31
    ///
    /// Query parameters:
    /// - startDate (required): Start date for analysis
    /// - endDate (required): End date for analysis
    /// - centerId: Filter by specific service center (optional)
    /// - status: Pending | Paid | Cancelled | PartiallyPaid | Overdue (optional)
    /// - includeAgingAnalysis: true | false (default: true)
    /// - includeDiscountAnalysis: true | false (default: true)
    /// - includeTaxSummary: true | false (default: false)
    ///
    /// Returns:
    /// - Summary metrics (total invoices, outstanding/paid amounts, collection rate)
    /// - Invoice status distribution
    /// - Aging analysis (0-30, 31-60, 61-90, 90+ days)
    /// - Discount effectiveness analysis (optional)
    /// - Tax collection summary (optional)
    /// - Average days to payment
    /// </remarks>
    [HttpGet("invoices")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetInvoiceReport(
        [FromQuery] InvoiceReportQueryDto query,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!IsValidDateRange(query.StartDate, query.EndDate, out var dateError))
                return BadRequest(new { success = false, message = dateError });

            _logger.LogInformation(
                "Invoice report requested: {StartDate} to {EndDate}",
                query.StartDate, query.EndDate);

            var report = await _financialReportService.GenerateInvoiceReportAsync(query, cancellationToken);

            return Ok(new
            {
                success = true,
                data = report,
                message = "Invoice analytics report generated successfully"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid invoice report parameters: {Message}", ex.Message);
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoice report");
            return StatusCode(500, new
            {
                success = false,
                message = "Error generating invoice report"
            });
        }
    }

    /// <summary>
    /// [Quick Summary] Get outstanding invoices summary
    /// Shows all unpaid/partially paid invoices with aging breakdown
    /// </summary>
    [HttpGet("invoices/outstanding")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOutstandingInvoicesSummary(
        [FromQuery] int? centerId,
        CancellationToken cancellationToken)
    {
        try
        {
            var today = DateTime.Today;
            var query = new InvoiceReportQueryDto
            {
                StartDate = today.AddYears(-1), // Last year
                EndDate = today,
                CenterId = centerId,
                IncludeAgingAnalysis = true,
                IncludeDiscountAnalysis = false,
                IncludeTaxSummary = false
            };

            var report = await _financialReportService.GenerateInvoiceReportAsync(query, cancellationToken);

            return Ok(new
            {
                success = true,
                data = new
                {
                    outstandingCount = report.OutstandingInvoicesCount,
                    outstandingAmount = report.OutstandingAmount,
                    outstandingPercentage = report.OutstandingPercentage,
                    agingAnalysis = report.AgingAnalysis,
                    statusDistribution = report.StatusDistribution
                        .Where(s => s.Status == "Pending" || s.Status == "PartiallyPaid" || s.Status == "Overdue")
                        .ToList()
                },
                message = "Outstanding invoices summary"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting outstanding invoices summary");
            return StatusCode(500, new
            {
                success = false,
                message = "Error retrieving outstanding invoices"
            });
        }
    }

    /// <summary>
    /// [Quick Summary] Get this month's invoice summary
    /// Fast endpoint for monthly dashboard
    /// </summary>
    [HttpGet("invoices/this-month")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetThisMonthInvoiceSummary(CancellationToken cancellationToken)
    {
        try
        {
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var query = new InvoiceReportQueryDto
            {
                StartDate = startOfMonth,
                EndDate = endOfMonth,
                IncludeAgingAnalysis = false,
                IncludeDiscountAnalysis = true,
                IncludeTaxSummary = true
            };

            var report = await _financialReportService.GenerateInvoiceReportAsync(query, cancellationToken);

            return Ok(new
            {
                success = true,
                data = new
                {
                    month = startOfMonth.ToString("MMMM yyyy"),
                    totalInvoices = report.TotalInvoices,
                    totalAmount = report.TotalInvoiceAmount,
                    paidAmount = report.PaidAmount,
                    outstandingAmount = report.OutstandingAmount,
                    collectionRate = report.CollectionRate,
                    averageDaysToPayment = report.AverageDaysToPayment,
                    statusDistribution = report.StatusDistribution,
                    discountAnalysis = report.DiscountAnalysis,
                    taxSummary = report.TaxSummary
                },
                message = "This month's invoice summary"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting this month's invoice summary");
            return StatusCode(500, new
            {
                success = false,
                message = "Error retrieving monthly invoices"
            });
        }
    }

    /// <summary>
    /// [Discount Analysis] Get discount effectiveness report
    /// Analyzes discount usage patterns and financial impact
    /// </summary>
    [HttpGet("invoices/discount-analysis")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDiscountAnalysis(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int? centerId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!IsValidDateRange(startDate, endDate, out var dateError))
                return BadRequest(new { success = false, message = dateError });

            var query = new InvoiceReportQueryDto
            {
                StartDate = startDate,
                EndDate = endDate,
                CenterId = centerId,
                IncludeAgingAnalysis = false,
                IncludeDiscountAnalysis = true,
                IncludeTaxSummary = false
            };

            var report = await _financialReportService.GenerateInvoiceReportAsync(query, cancellationToken);

            return Ok(new
            {
                success = true,
                data = new
                {
                    period = new { startDate, endDate },
                    totalInvoices = report.TotalInvoices,
                    totalRevenue = report.TotalInvoiceAmount,
                    discountAnalysis = report.DiscountAnalysis
                },
                message = "Discount analysis completed"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating discount analysis");
            return StatusCode(500, new
            {
                success = false,
                message = "Error retrieving discount analysis"
            });
        }
    }

    #region ValidationHelpers

    private static bool IsValidDateRange(DateTime start, DateTime end, out string error)
    {
        if (start == default || end == default)
        {
            error = "startDate and endDate are required (ISO format)";
            return false;
        }

        if (end < start)
        {
            error = "endDate must be greater than or equal to startDate";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static bool IsValidGroupBy(string groupBy)
    {
        return !string.IsNullOrWhiteSpace(groupBy) &&
               AllowedGroupBy.Any(g => g.Equals(groupBy, StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #endregion
}
