using System;
using StoreAnalytics.Data;
using StoreAnalytics.Services;

namespace StoreAnalytics
{
    public class Program
    {
        public static void Main()
        {
            var (categories, suppliers, products, orders) = TestDataFactory.Generate();

            Console.WriteLine("Формирование аналитического отчёта\n");

            var report = StoreAnalyticsService.BuildReport(
                categories: categories,
                suppliers: suppliers,
                products: products,
                orders: orders,
                minPrice: 500m,
                maxPrice: 100000m,
                categoryId: null,
                searchText: "мастер",
                pageNumber: 1,
                pageSize: 10);

            Console.WriteLine("Общие показатели");
            Console.WriteLine($"Всего товаров: {report.TotalProducts}");
            Console.WriteLine($"Активных товаров: {report.ActiveProducts}");
            Console.WriteLine($"Нет в наличии: {report.OutOfStockProducts}");
            Console.WriteLine($"Общая стоимость запасов: {report.TotalStockValue:C}");
            Console.WriteLine($"Средняя цена активного товара: {report.AverageActiveProductPrice:C}");
            Console.WriteLine();

            Console.WriteLine("Каталог товаров(Страница 1)");
            foreach (var item in report.CatalogPage)
            {
                Console.WriteLine($"[{item.Id}] {item.Name} | {item.CategoryName} | {item.SupplierName} | {item.PriceWithTax:C} | {item.StockStatus}");
            }
            Console.WriteLine();

            Console.WriteLine("Отчёт по категориям");
            foreach (var cat in report.Categories)
            {
                Console.WriteLine($"{cat.CategoryName}: Товаров: {cat.ProductCount}, Активных: {cat.ActiveProductCount}, " +
                                  $"На складе: {cat.InStockProductCount} (Всего: {cat.TotalStockQuantity} шт. на {cat.TotalStockValue:C}), " +
                                  $"Цены: {cat.MinPrice:C} - {cat.MaxPrice:C} (Средняя: {cat.AveragePrice:C})");
            }
            Console.WriteLine();

            Console.WriteLine("Топ-5 продаваемых товаров");
            foreach (var sale in report.TopSellingProducts)
            {
                Console.WriteLine($"#{sale.ProductId} {sale.ProductName}: Продано {sale.TotalQuantity} шт. в {sale.OrdersCount} заказах. Выручка: {sale.Revenue:C}");
            }
        }
    }
}