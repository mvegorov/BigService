using Application.Abstractions.Service;
using Application.Ports.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Services;

public static class ServiceExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IProductService, ProductService>();

        return services;
    }
}