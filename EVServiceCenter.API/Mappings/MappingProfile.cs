using AutoMapper;
using EVServiceCenter.Core.Domains.Identity.DTOs.Responses;
using EVServiceCenter.Core.Domains.Identity.Entities;

namespace EVServiceCenter.API.Mappings
{
  public class MappingProfile : Profile
  {
        public MappingProfile()
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
    }
}