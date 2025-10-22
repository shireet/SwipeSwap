using Microsoft.Extensions.DependencyInjection;
using SwipeSwap.Application.Auth.Dtos;
using SwipeSwap.Application.Profile.Dtos;

namespace SwipeSwap.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cf => cf.RegisterServicesFromAssembly(typeof(RegisterUserRequest).Assembly));
        services.AddMediatR(cf => cf.RegisterServicesFromAssembly(typeof(LoginUserRequest).Assembly));
        services.AddMediatR(cf => cf.RegisterServicesFromAssembly(typeof(RefreshTokenRequest).Assembly));
        
        services.AddMediatR(cf => cf.RegisterServicesFromAssembly(typeof(GetUserByIdRequest).Assembly));
        services.AddMediatR(cf => cf.RegisterServicesFromAssembly(typeof(UpdateUserRequest).Assembly));
        services.AddMediatR(cf => cf.RegisterServicesFromAssembly(typeof(GetCurrentUserRequest).Assembly));
        return services;
    }
}
