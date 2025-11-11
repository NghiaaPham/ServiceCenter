using EVServiceCenter.Core.Domains.FinancialReports.DTOs.Requests;
using EVServiceCenter.Core.Domains.FinancialReports.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.FinancialReports;

/// <summary>
/// Financial Reports Controller
/// Provides financial reporting endpoints matching original requirements
/// </summary>
[ApiController]
[Route("api/reports")]
[ApiExplorerSettings(GroupName = "Financial Reports")]
[Authorize(Roles = "Admin,Manager")]
public class ReportController : ControllerBase
{
    private readonly IFinancialReportService _financialReportService;
    private readonly ILogger<ReportController> _logger;

    public ReportController(
        IFinancialReportService financialReportService,
        ILogger<ReportController> logger)
    {
        _financialReportService = financialReportService;
        _logger = logger;
    }

    /// <summary>
    /// [Revenue] Get revenue report for a date range
    /// Original requirement: GET /api/reports/revenue?from=&to=
    /// </summary>
    /// <param name="from">Start date (required)</param>
    /// <param name="to">End date (required)</param>
    /// <param name="centerId">Filter by service center (optional)</param>
    /// <param name="groupBy">Group by: Daily | Weekly | Monthly (default: Daily)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("revenue")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRevenueReport(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] int? centerId = null,
        [FromQuery] string groupBy = "Daily",
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (from > to)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Start date must be before end date"
                });
            }

            var query = new RevenueReportQueryDto
            {
                StartDate = from,
                EndDate = to,
                CenterId = centerId,
                GroupBy = groupBy,
                IncludePaymentMethodBreakdown = true,
                IncludeServiceCenterBreakdown = centerId == null
            };

            _logger.LogInformation(
                "Revenue report requested: {From} to {To}, GroupBy: {GroupBy}",
                from, to, groupBy);

            var report = await _financialReportService.GenerateRevenueReportAsync(query, cancellationToken);

            return Ok(new
            {
                success = true,
                data = report,
                message = "Revenue report generated successfully"
            });
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
    /// [Profit] Get profit analysis report
    /// Original requirement: GET /api/reports/profit
    /// </summary>
    /// <param name="from">Start date (required)</param>
    /// <param name="to">End date (required)</param>
    /// <param name="centerId">Filter by service center (optional)</param>
    /// <param name="includeBreakdown">Include service center breakdown (default: false)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("profit")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProfitReport(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] int? centerId = null,
        [FromQuery] bool includeBreakdown = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (from > to)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Start date must be before end date"
                });
            }

            _logger.LogInformation(
                "Profit report requested: {From} to {To}",
                from, to);

            var report = await _financialReportService.GenerateProfitReportAsync(
                from,
                to,
                centerId,
                includeBreakdown,
                cancellationToken);

            return Ok(new
            {
                success = true,
                data = report,
                message = "Profit report generated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating profit report");
            return StatusCode(500, new
            {
                success = false,
                message = "Error generating profit report"
            });
        }
    }

    /// <summary>
    /// [Popular Services] Get most popular services report
    /// Original requirement: GET /api/reports/services-popular
    /// </summary>
    /// <param name="from">Start date (required)</param>
    /// <param name="to">End date (required)</param>
    /// <param name="centerId">Filter by service center (optional)</param>
    /// <param name="topN">Number of top services to return (default: 10)</param>
    /// <param name="includeTrends">Include period-over-period trends (default: false)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("services-popular")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPopularServicesReport(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] int? centerId = null,
        [FromQuery] int topN = 10,
        [FromQuery] bool includeTrends = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (from > to)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Start date must be before end date"
                });
            }

            if (topN < 1 || topN > 50)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "topN must be between 1 and 50"
                });
            }

            _logger.LogInformation(
                "Popular services report requested: {From} to {To}, TopN: {TopN}",
                from, to, topN);

            var report = await _financialReportService.GeneratePopularServicesReportAsync(
                from,
                to,
                centerId,
                topN,
                includeTrends,
                cancellationToken);

            return Ok(new
            {
                success = true,
                data = report,
                message = "Popular services report generated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating popular services report");
            return StatusCode(500, new
            {
                success = false,
                message = "Error generating popular services report"
            });
        }
    }

    #region Quick Summaries

    /// <summary>
    /// [Quick Summary] Get today's financial summary
    /// Combines revenue, profit, and popular services for today
    /// </summary>
    [HttpGet("today")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTodaySummary(CancellationToken cancellationToken)
    {
        try
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1).AddSeconds(-1);

            var revenueQuery = new RevenueReportQueryDto
            {
                StartDate = today,
                EndDate = tomorrow,
                GroupBy = "Daily",
                IncludePaymentMethodBreakdown = true
            };

            // ✅ FIXED: Sequential execution to avoid DbContext threading issues
            var revenue = await _financialReportService.GenerateRevenueReportAsync(revenueQuery, cancellationToken);
            var profit = await _financialReportService.GenerateProfitReportAsync(today, tomorrow, null, false, cancellationToken);

            return Ok(new
            {
                success = true,
                data = new
                {
                    date = today,
                    revenue = new
                    {
                        total = revenue.TotalRevenue,
                        collectionRate = revenue.CollectionRate,
                        paymentCount = revenue.TotalPaymentCount,
                        paymentMethodBreakdown = revenue.PaymentMethodBreakdown
                    },
                    profit = new
                    {
                        netProfit = profit.NetProfit,
                        profitMargin = profit.ProfitMargin,
                        costs = profit.TotalCosts
                    }
                },
                message = "Today's financial summary"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting today's summary");
            return StatusCode(500, new
            {
                success = false,
                message = "Error retrieving today's summary"
            });
        }
    }

    /// <summary>
    /// [Quick Summary] Get this month's financial summary
    /// </summary>
    [HttpGet("this-month")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetThisMonthSummary(CancellationToken cancellationToken)
    {
        try
        {
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var revenueQuery = new RevenueReportQueryDto
            {
                StartDate = startOfMonth,
                EndDate = endOfMonth,
                GroupBy = "Daily",
                IncludePaymentMethodBreakdown = true
            };

            // ✅ FIXED: Sequential execution to avoid DbContext threading issues
            var revenue = await _financialReportService.GenerateRevenueReportAsync(revenueQuery, cancellationToken);
            var profit = await _financialReportService.GenerateProfitReportAsync(
                startOfMonth,
                endOfMonth,
                null,
                false,
                cancellationToken);

            return Ok(new
            {
                success = true,
                data = new
                {
                    month = startOfMonth.ToString("MMMM yyyy"),
                    revenue = new
                    {
                        total = revenue.TotalRevenue,
                        collectionRate = revenue.CollectionRate,
                        growthRate = revenue.GrowthRate,
                        timeSeries = revenue.TimeSeries
                    },
                    profit = new
                    {
                        netProfit = profit.NetProfit,
                        profitMargin = profit.ProfitMargin,
                        profitGrowthRate = profit.ProfitGrowthRate
                    }
                },
                message = "This month's financial summary"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting this month's summary");
            return StatusCode(500, new
            {
                success = false,
                message = "Error retrieving monthly summary"
            });
        }
    }

    #endregion
}
