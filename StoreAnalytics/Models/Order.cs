namespace StoreAnalytics.Models
{
    public sealed record OrderItem(int ProductId, int Quantity, decimal UnitPrice);

    public sealed record Order(
        int Id,
        int CustomerId,
        DateTime CreatedAt,
        IReadOnlyList<OrderItem> Items);
}