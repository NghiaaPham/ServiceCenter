using System;
using System.Threading;
using System.Threading.Tasks;
using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Helpers
{
    /// <summary>
    /// Shared helper to apply customer spend & loyalty updates in a consistent way.
    /// Keeps all calculations in one place so different flows (payments, appointments)
    /// do not drift apart.
    /// </summary>
    public static class CustomerSpendHelper
    {
        public static async Task<bool> TryIncrementTotalSpentAsync(
            EVDbContext context,
            ILogger logger,
            int customerId,
            decimal amount,
            string scenario,
            CancellationToken cancellationToken,
            bool saveChanges = false)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (amount <= 0)
            {
                logger?.LogDebug(
                    "Skip spend recognition for customer {CustomerId} because amount <= 0 ({Amount}) in scenario {Scenario}",
                    customerId,
                    amount,
                    scenario);
                return false;
            }

            var customer = await context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);

            if (customer == null)
            {
                logger?.LogWarning(
                    "Cannot recognize spend for customer {CustomerId} because record not found (Scenario={Scenario})",
                    customerId,
                    scenario);
                return false;
            }

            customer.TotalSpent = (customer.TotalSpent ?? 0m) + amount;
            customer.LastVisitDate = DateOnly.FromDateTime(DateTime.UtcNow);
            customer.UpdatedDate = DateTime.UtcNow;

            var loyaltyPoints = (int)(amount / 10000);
            if (loyaltyPoints > 0)
            {
                customer.LoyaltyPoints = (customer.LoyaltyPoints ?? 0) + loyaltyPoints;
            }

            if (saveChanges)
            {
                await context.SaveChangesAsync(cancellationToken);
            }

            logger?.LogInformation(
                "Recognized spend {Amount:N0} for customer {CustomerId} (Scenario={Scenario}, Points+={Points})",
                amount,
                customerId,
                scenario,
                loyaltyPoints);

            return true;
        }
    }
}
