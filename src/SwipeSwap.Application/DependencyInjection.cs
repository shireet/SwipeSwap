using Microsoft.Extensions.DependencyInjection;
using SwipeSwap.Application.Auth.Dtos;

namespace SwipeSwap.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cf => cf.RegisterServicesFromAssembly(typeof(RegisterUserRequest).Assembly));
        services.AddMediatR(cf => cf.RegisterServicesFromAssembly(typeof(LoginUserRequest).Assembly));
        services.AddMediatR(cf => cf.RegisterServicesFromAssembly(typeof(RefreshTokenRequest).Assembly));
        return services;
    }
}
