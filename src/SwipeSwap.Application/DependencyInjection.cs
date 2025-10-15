using Microsoft.Extensions.DependencyInjection;
using SwipeSwap.Application.Auth;

namespace SwipeSwap.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cf => cf.RegisterServicesFromAssembly(typeof(RegisterUserRequest).Assembly));
        return services;
    }
}
