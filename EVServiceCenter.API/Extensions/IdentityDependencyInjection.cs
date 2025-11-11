using EVServiceCenter.Core.Domains.Identity.DTOs.Requests;
using EVServiceCenter.Core.Domains.Identity.Interfaces;
using EVServiceCenter.Core.Domains.Identity.Validators;
using EVServiceCenter.Infrastructure.Domains.Identity.Services;
using FluentValidation;

namespace EVServiceCenter.API.Extensions
{
  public static class IdentityDependencyInjection
  {
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
      // AccountRecoveryController Validators
      services.AddScoped<IValidator<ForgotPasswordRequestDto>, ForgotPasswordRequestDtoValidator>();
      services.AddScoped<IValidator<ResetPasswordSubmitRequestDto>, ResetPasswordSubmitRequestDtoValidator>();

      // Customer Registration Validator
      services.AddScoped<IValidator<CustomerRegistrationDto>, CustomerRegistrationValidator>();

      return services;
    }
  }
}
