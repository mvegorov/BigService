using GrpcGeneratedClasses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Gateway.Grpc;

public static class GrpcExtensions
{
    public static IServiceCollection AddGrpcClients(this IServiceCollection services)
    {
        services.AddGrpcClient<GrpcGeneratedClasses.Orders.OrdersClient>((serviceProvider, client) =>
        {
            ServiceConnectionSettings settings = serviceProvider.GetRequiredService<IOptions<ServiceConnectionSettings>>().Value;
            client.Address = new Uri(settings.Address);
        });
        services.AddGrpcClient<Products.ProductsClient>((serviceProvider, client) =>
        {
            ServiceConnectionSettings settings = serviceProvider.GetRequiredService<IOptions<ServiceConnectionSettings>>().Value;
            client.Address = new Uri(settings.Address);
        });
        services.AddGrpcClient<Orders.ProcessingService.Contracts.OrderService.OrderServiceClient>((serviceProvider, client) =>
        {
            Lab5ToolsConnectionSettings settings = serviceProvider.GetRequiredService<IOptions<Lab5ToolsConnectionSettings>>().Value;
            client.Address = new Uri(settings.Address);
        });

        return services;
    }
}
