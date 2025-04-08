namespace Gateway.Grpc;

public static class ConvertExtensions
{
    public static Google.Type.Money ToGrpcMoney(this decimal money)
    {
        long units = (long)money;

        int nanos = (int)((money - units) * 10_000_000_000m);

        return new Google.Type.Money
        {
            CurrencyCode = "USD",
            Units = units,
            Nanos = nanos,
        };
    }

    public static GrpcGeneratedClasses.OrderState ToGrpcOrderState(this string state) => state.ToLowerInvariant() switch
    {
        "created" => GrpcGeneratedClasses.OrderState.Created,
        "processing" => GrpcGeneratedClasses.OrderState.Processing,
        "cancelled" => GrpcGeneratedClasses.OrderState.Cancelled,
        "completed" => GrpcGeneratedClasses.OrderState.Completed,
        _ => throw new InvalidCastException($"Wrong state: {state}"),
    };
}