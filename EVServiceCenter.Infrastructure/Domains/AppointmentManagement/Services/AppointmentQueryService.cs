using System.Collections.Generic;
using System.Linq;
using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Query;
using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response;
using EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Domains.Payments.Interfaces.Services;
using EVServiceCenter.Infrastructure.Domains.AppointmentManagement.Mappers;
using Microsoft.Extensions.Logging;


namespace EVServiceCenter.Infrastructure.Domains.AppointmentManagement.Services
{
    public class AppointmentQueryService : IAppointmentQueryService
    {
        private readonly IAppointmentRepository _repository;
        private readonly IAppointmentQueryRepository _queryRepository;
        private readonly IPaymentIntentService _paymentIntentService;
        private readonly ILogger<AppointmentQueryService> _logger;

        public AppointmentQueryService(
            IAppointmentRepository repository,
            IAppointmentQueryRepository queryRepository,
            IPaymentIntentService paymentIntentService,
            ILogger<AppointmentQueryService> logger)
        {
            _repository = repository;
            _queryRepository = queryRepository;
            _paymentIntentService = paymentIntentService;
            _logger = logger;
        }

        public async Task<PagedResult<AppointmentResponseDto>> GetPagedAsync(
            AppointmentQueryDto query,
            CancellationToken cancellationToken = default)
        {
            var pagedResult = await _queryRepository.GetPagedAsync(query, cancellationToken);

            var dtos = pagedResult.Items.Select(AppointmentMapper.ToResponseDto);

            return new PagedResult<AppointmentResponseDto>
            {
                Items = dtos,
                TotalCount = pagedResult.TotalCount,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize,
                TotalPages = pagedResult.TotalPages
            };
        }

        public async Task<AppointmentDetailResponseDto?> GetByIdAsync(
            int appointmentId,
            CancellationToken cancellationToken = default)
        {
            var appointment = await _repository.GetByIdWithDetailsAsync(appointmentId, cancellationToken);

            try
            {
                return appointment == null ? null : AppointmentMapper.ToDetailResponseDto(appointment);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Appointment null khi mapping ở GetByIdAsync");
                return null;
            }
        }

        public async Task<AppointmentResponseDto?> GetByCodeAsync(
            string appointmentCode,
            CancellationToken cancellationToken = default)
        {
            var appointment = await _repository.GetByCodeAsync(appointmentCode, cancellationToken);

            return appointment == null ? null : AppointmentMapper.ToResponseDto(appointment);
        }

        public async Task<IEnumerable<AppointmentResponseDto>> GetByCustomerIdAsync(
            int customerId,
            CancellationToken cancellationToken = default)
        {
            var appointments = await _queryRepository.GetByCustomerIdAsync(customerId, cancellationToken);

            return appointments.Select(AppointmentMapper.ToResponseDto);
        }

        public async Task<IEnumerable<AppointmentResponseDto>> GetUpcomingByCustomerAsync(
            int customerId,
            int limit = 5,
            CancellationToken cancellationToken = default)
        {
            var appointments = await _queryRepository.GetUpcomingByCustomerAsync(
                customerId, limit, cancellationToken);

            return appointments.Select(AppointmentMapper.ToResponseDto);
        }

        public async Task<IEnumerable<AppointmentResponseDto>> GetByServiceCenterAndDateAsync(
            int serviceCenterId,
            DateOnly date,
            CancellationToken cancellationToken = default)
        {
            var appointments = await _queryRepository.GetByServiceCenterAndDateAsync(
                serviceCenterId, date, cancellationToken);

            return appointments.Select(AppointmentMapper.ToResponseDto);
        }

        public async Task<int> GetCountByStatusAsync(
            int statusId,
            CancellationToken cancellationToken = default)
        {
            return await _queryRepository.GetCountByStatusAsync(statusId, cancellationToken);
        }

        public async Task<IReadOnlyList<PaymentIntentResponseDto>> GetPaymentIntentsAsync(
            int appointmentId,
            CancellationToken cancellationToken = default)
        {
            var intents = await _paymentIntentService.GetByAppointmentAsync(appointmentId, cancellationToken);
            return intents
                .Select(AppointmentMapper.ToPaymentIntentResponseDto)
                .ToList();
        }

        public async Task<IEnumerable<AppointmentResponseDto>> GetUpcomingByCustomerDtosAsync(
            int customerId,
            int limit = 5,
            CancellationToken cancellationToken = default)
        {
            var dtos = await _queryRepository.GetUpcomingDtosByCustomerAsync(customerId, limit, cancellationToken);
            return dtos;
        }
    }
}
