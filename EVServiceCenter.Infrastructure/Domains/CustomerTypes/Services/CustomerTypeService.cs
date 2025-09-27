using EVServiceCenter.Core.Domains.CustomerTypes.DTOs.Requests;
using EVServiceCenter.Core.Domains.CustomerTypes.DTOs.Responses;
using EVServiceCenter.Core.Domains.CustomerTypes.Interfaces;
using EVServiceCenter.Core.Domains.Shared.Models;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.CustomerTypes.Services
{
    public class CustomerTypeService : ICustomerTypeService
    {
        private readonly ICustomerTypeRepository _repository;
        private readonly ILogger<CustomerTypeService> _logger;

        public CustomerTypeService(
            ICustomerTypeRepository repository,
            ILogger<CustomerTypeService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> CanDeleteAsync(int typeId, CancellationToken cancellationToken = default)
        {
            try
            {
                return !await _repository.HasCustomersAsync(typeId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if customer type can be deleted: {TypeId}", typeId);
                throw;
            }
        }
        public async Task<CustomerTypeResponseDto> CreateAsync(
           CreateCustomerTypeRequestDto request,
           CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Creating customer type: {TypeName}", request.TypeName);
                var result = await _repository.CreateAsync(request, cancellationToken);

                _logger.LogInformation("Successfully created customer type: {TypeName} with ID: {TypeId}",
                    result.TypeName, result.TypeId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in service layer creating customer type: {TypeName}", request.TypeName);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int typeId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (typeId <= 0)
                {
                    throw new ArgumentException("Customer type ID must be greater than 0", nameof(typeId));
                }

                // Check if can delete
                var canDelete = await CanDeleteAsync(typeId, cancellationToken);
                if (!canDelete)
                {
                    throw new InvalidOperationException("Cannot delete customer type that has associated customers");
                }

                _logger.LogDebug("Deleting customer type: {TypeId}", typeId);
                var result = await _repository.DeleteAsync(typeId, cancellationToken);

                if (result)
                {
                    _logger.LogInformation("Successfully deleted customer type: {TypeId}", typeId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in service layer deleting customer type: {TypeId}", typeId);
                throw;
            }
        }
        public async Task<IEnumerable<CustomerTypeResponseDto>> GetActiveTypesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Retrieving active customer types");
                return await _repository.GetActiveAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in service layer retrieving active customer types");
                throw;
            }
        }

        public async Task<PagedResult<CustomerTypeResponseDto>> GetAllAsync(
           CustomerTypeQueryDto query,
           CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Retrieving customer types with query: {@Query}", query);
                return await _repository.GetAllAsync(query, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in service layer retrieving customer types");
                throw;
            }
        }
        public async Task<CustomerTypeResponseDto?> GetByIdAsync(
            int typeId,
            bool includeStats = true,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (typeId <= 0)
                {
                    throw new ArgumentException("Customer type ID must be greater than 0", nameof(typeId));
                }

                _logger.LogDebug("Retrieving customer type by ID: {TypeId}, includeStats: {IncludeStats}", typeId, includeStats);
                return await _repository.GetByIdAsync(typeId, includeStats, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in service layer retrieving customer type by ID: {TypeId}", typeId);
                throw;
            }
        }

        public async Task<CustomerTypeResponseDto> UpdateAsync(
            UpdateCustomerTypeRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Updating customer type: {TypeId}", request.TypeId);
                var result = await _repository.UpdateAsync(request, cancellationToken);

                _logger.LogInformation("Successfully updated customer type: {TypeName} with ID: {TypeId}",
                    result.TypeName, result.TypeId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in service layer updating customer type: {TypeId}", request.TypeId);
                throw;
            }
        }
    }
}