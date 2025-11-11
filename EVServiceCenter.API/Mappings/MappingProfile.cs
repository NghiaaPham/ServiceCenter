using AutoMapper;
using EVServiceCenter.Core.Domains.Identity.DTOs.Responses;
using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Domains.Notifications.DTOs.Responses;
using EVServiceCenter.Core.Domains.ServiceRatings.DTOs.Responses;
using EVServiceCenter.Core.Domains.Chat.DTOs.Responses;
using EVServiceCenter.Core.Entities;

namespace EVServiceCenter.API.Mappings;

/// <summary>
/// AutoMapper profile for entity to DTO mappings
/// Performance: AutoMapper mappings are compiled and cached for fast execution
/// Maintainability: Centralized mapping configuration with clear intent
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        ConfigureIdentityMappings();
        ConfigureNotificationMappings();
        ConfigureServiceRatingMappings();
        ConfigureChatMappings();
    }

    /// <summary>
    /// Identity and User mappings
    /// </summary>
    private void ConfigureIdentityMappings()
    {
        CreateMap<User, UserResponseDto>()
            .ForMember(dest => dest.RoleName,
                      opt => opt.MapFrom(src => src.Role != null ? src.Role.RoleName : null))
            .ForMember(dest => dest.IsActive,
                      opt => opt.MapFrom(src => src.IsActive ?? false))
            .ForMember(dest => dest.DisplayName, opt => opt.Ignore()) // Computed property
            .ForMember(dest => dest.IsInternal, opt => opt.Ignore()) // Computed property
            .ForMember(dest => dest.IsLocked, opt => opt.Ignore()); // Computed property
    }

    /// <summary>
    /// Notification module mappings
    /// Performance: Navigation properties are loaded lazily to avoid N+1 queries
    /// Maintainability: Only explicit mappings for computed or different-named properties
    /// </summary>
    private void ConfigureNotificationMappings()
    {
        // AutoMapper will automatically map properties with matching names
        CreateMap<Notification, NotificationResponseDto>()
            .ForMember(dest => dest.IsRead,
                      opt => opt.MapFrom(src => src.ReadDate.HasValue))
            .ForMember(dest => dest.IsDelivered,
                      opt => opt.MapFrom(src => src.DeliveredDate.HasValue));
    }

    /// <summary>
    /// Service Rating module mappings
    /// Performance: Customer and technician names are loaded via Include() to prevent N+1
    /// Maintainability: AutoMapper auto-maps matching properties
    /// </summary>
    private void ConfigureServiceRatingMappings()
    {
        // AutoMapper will automatically map properties with matching names
        CreateMap<ServiceRating, ServiceRatingResponseDto>()
            .ForMember(dest => dest.CustomerName,
                      opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FullName : null))
            .ForMember(dest => dest.TechnicianName,
                      opt => opt.MapFrom(src => src.Technician != null ? src.Technician.FullName : null))
            .ForMember(dest => dest.AdvisorName,
                      opt => opt.MapFrom(src => src.Advisor != null ? src.Advisor.FullName : null));
    }

    /// <summary>
    /// Chat module mappings
    /// Performance: Message counts use COUNT(*) aggregation instead of loading all messages
    /// Scalability: Supports pagination for large chat histories
    /// Maintainability: AutoMapper auto-maps matching properties
    /// </summary>
    private void ConfigureChatMappings()
    {
        // ChatMessage -> ChatMessageResponseDto
        // AutoMapper auto-maps matching properties
        CreateMap<ChatMessage, ChatMessageResponseDto>()
            .ForMember(dest => dest.IsRead,
                      opt => opt.MapFrom(src => src.IsRead ?? false));

        // ChatChannel -> ChatChannelResponseDto
        CreateMap<ChatChannel, ChatChannelResponseDto>()
            .ForMember(dest => dest.CustomerName,
                      opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FullName : null))
            .ForMember(dest => dest.AssignedUserName,
                      opt => opt.MapFrom(src => src.AssignedUser != null ? src.AssignedUser.FullName : null))
            .ForMember(dest => dest.UnreadCount,
                      opt => opt.Ignore()); // Computed via separate query for performance
    }
}