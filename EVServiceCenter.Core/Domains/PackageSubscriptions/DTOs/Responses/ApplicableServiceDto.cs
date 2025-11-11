using System;

namespace EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Responses;

/// <summary>
/// Represents a service that is already covered by the customer's active package subscription.
/// Helps frontend mark services as free (remaining usages & package info).
/// </summary>
public class ApplicableServiceDto
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;

    public int RemainingQuantity { get; set; }
    public int TotalQuantity { get; set; }

    public int SubscriptionId { get; set; }
    public string SubscriptionCode { get; set; } = string.Empty;

    public int PackageId { get; set; }
    public string PackageName { get; set; } = string.Empty;

    public int VehicleId { get; set; }

    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// Convenience flag for UI – true when remaining usages > 0.
    /// </summary>
    public bool IsFree => RemainingQuantity > 0;
}
