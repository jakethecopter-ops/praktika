namespace StoreAnalytics.Models
{
    public sealed record ProductCatalogItem(
        int Id,
        string Name,
        string CategoryName,
        string SupplierName,
        decimal Price,
        decimal PriceWithTax,
        string StockStatus,
        bool IsActive);

    public sealed record StoreAnalyticsReport(
        int TotalProducts,
        int ActiveProducts,
        int OutOfStockProducts,
        decimal TotalStockValue,
        decimal? AverageActiveProductPrice,
        IReadOnlyList<ProductCatalogItem> CatalogPage,
        IReadOnlyList<CategoryStockReport> Categories,
        IReadOnlyList<ProductSalesReport> TopSellingProducts);

    public sealed record CategoryStockReport(
        string CategoryName,
        int ProductCount,
        int ActiveProductCount,
        int InStockProductCount,
        int TotalStockQuantity,
        decimal TotalStockValue,
        decimal? AveragePrice,
        decimal? MinPrice,
        decimal? MaxPrice);

    public sealed record ProductSalesReport(
        int ProductId,
        string ProductName,
        int TotalQuantity,
        int OrdersCount,
        decimal Revenue);
}