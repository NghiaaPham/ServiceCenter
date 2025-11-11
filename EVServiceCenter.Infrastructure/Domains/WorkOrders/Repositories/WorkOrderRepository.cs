using EVServiceCenter.Core.Domains.Shared.Interfaces;
using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Domains.WorkOrders.DTOs.Requests;
using EVServiceCenter.Core.Domains.WorkOrders.DTOs.Responses;
using EVServiceCenter.Core.Domains.WorkOrders.Interfaces;
using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.Infrastructure.Domains.WorkOrders.Repositories;

/// <summary>
/// Repository for WorkOrder data access
/// Optimized queries with proper indexes and includes
/// </summary>
public class WorkOrderRepository : IWorkOrderRepository
{
    private EVDbContext _context;
    private readonly WorkOrderQueryRepository _queryRepo;

    public WorkOrderRepository(EVDbContext context, WorkOrderQueryRepository queryRepo)
    {
        _context = context;
        _queryRepo = queryRepo;
    }

    #region IRepository<WorkOrder> Implementation

    public async Task<IEnumerable<WorkOrder>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.WorkOrders
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkOrder?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.WorkOrders
            .Include(w => w.Customer)
            .Include(w => w.Vehicle)
            .Include(w => w.ServiceCenter)
            .Include(w => w.Status)
            .Include(w => w.Technician)
            .Include(w => w.Advisor)
            .Include(w => w.WorkOrderServices).ThenInclude(ws => ws.Service)
            .Include(w => w.WorkOrderParts).ThenInclude(wp => wp.Part)
            .FirstOrDefaultAsync(w => w.WorkOrderId == id, cancellationToken);
    }

    public async Task<WorkOrder> CreateAsync(WorkOrder entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedDate = DateTime.UtcNow;
        _context.WorkOrders.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<WorkOrder> UpdateAsync(WorkOrder entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedDate = DateTime.UtcNow;
        _context.WorkOrders.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var workOrder = await _context.WorkOrders.FindAsync(new object[] { id }, cancellationToken);
        if (workOrder == null) return false;

        _context.WorkOrders.Remove(workOrder);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.WorkOrders.AnyAsync(w => w.WorkOrderId == id, cancellationToken);
    }

    public async Task<IEnumerable<WorkOrder>> FindAsync(
        System.Linq.Expressions.Expression<Func<WorkOrder, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkOrders
            .Where(predicate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkOrder?> FirstOrDefaultAsync(
        System.Linq.Expressions.Expression<Func<WorkOrder, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkOrders
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.WorkOrders.CountAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        System.Linq.Expressions.Expression<Func<WorkOrder, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkOrders.CountAsync(predicate, cancellationToken);
    }

    public async Task<IEnumerable<WorkOrder>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkOrders
            .AsNoTracking()
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkOrder>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        System.Linq.Expressions.Expression<Func<WorkOrder, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkOrders
            .Where(predicate)
            .AsNoTracking()
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task CreateRangeAsync(IEnumerable<WorkOrder> entities, CancellationToken cancellationToken = default)
    {
        await _context.WorkOrders.AddRangeAsync(entities, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateRangeAsync(IEnumerable<WorkOrder> entities, CancellationToken cancellationToken = default)
    {
        _context.WorkOrders.UpdateRange(entities);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteRangeAsync(IEnumerable<WorkOrder> entities, CancellationToken cancellationToken = default)
    {
        _context.WorkOrders.RemoveRange(entities);
        await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region IWorkOrderRepository Specific Methods (Delegate to QueryRepository)

    public Task<PagedResult<WorkOrderSummaryDto>> GetWorkOrdersAsync(
        WorkOrderQueryDto query,
        CancellationToken cancellationToken = default)
    {
        return _queryRepo.GetWorkOrdersAsync(query, cancellationToken);
    }

    public Task<WorkOrderResponseDto?> GetWorkOrderDetailAsync(
        int workOrderId,
        CancellationToken cancellationToken = default)
    {
        return _queryRepo.GetWorkOrderDetailAsync(workOrderId, cancellationToken);
    }

    public Task<WorkOrder?> GetByCodeAsync(
        string workOrderCode,
        CancellationToken cancellationToken = default)
    {
        return _queryRepo.GetByCodeAsync(workOrderCode, cancellationToken);
    }

    public Task<string> GenerateWorkOrderCodeAsync(CancellationToken cancellationToken = default)
    {
        return _queryRepo.GenerateWorkOrderCodeAsync(cancellationToken);
    }

    public Task<List<WorkOrderSummaryDto>> GetWorkOrdersByTechnicianAsync(
        int technicianId,
        int? statusId = null,
        CancellationToken cancellationToken = default)
    {
        return _queryRepo.GetWorkOrdersByTechnicianAsync(technicianId, statusId, cancellationToken);
    }

    public Task<List<WorkOrderSummaryDto>> GetWorkOrdersByVehicleAsync(
        int vehicleId,
        CancellationToken cancellationToken = default)
    {
        return _queryRepo.GetWorkOrdersByVehicleAsync(vehicleId, cancellationToken);
    }

    public Task<List<WorkOrderSummaryDto>> GetWorkOrdersByCustomerAsync(
        int customerId,
        CancellationToken cancellationToken = default)
    {
        return _queryRepo.GetWorkOrdersByCustomerAsync(customerId, cancellationToken);
    }

    public Task<bool> IsTechnicianAvailableAsync(
        int technicianId,
        int maxConcurrentWorkOrders = 5,
        CancellationToken cancellationToken = default)
    {
        return _queryRepo.IsTechnicianAvailableAsync(technicianId, maxConcurrentWorkOrders, cancellationToken);
    }

    public Task<bool> UpdateStatusAsync(
        int workOrderId,
        int newStatusId,
        int updatedBy,
        CancellationToken cancellationToken = default)
    {
        return _queryRepo.UpdateStatusAsync(workOrderId, newStatusId, updatedBy, cancellationToken);
    }

    public Task<decimal> CalculateProgressAsync(
        int workOrderId,
        CancellationToken cancellationToken = default)
    {
        return _queryRepo.CalculateProgressAsync(workOrderId, cancellationToken);
    }

    #endregion
}
