using System;
using System.Collections.Generic;
using System.Linq;
using StoreAnalytics.Models;

namespace StoreAnalytics.Services
{
    public static class StoreAnalyticsService
    {
        public static StoreAnalyticsReport BuildReport(
            IEnumerable<Category> categories,
            IEnumerable<Supplier> suppliers,
            IEnumerable<Product> products,
            IEnumerable<Order> orders,
            decimal? minPrice,
            decimal? maxPrice,
            int? categoryId,
            string? searchText,
            int pageNumber,
            int pageSize)
        {
            if (categories == null) throw new ArgumentNullException(nameof(categories));
            if (suppliers == null) throw new ArgumentNullException(nameof(suppliers));
            if (products == null) throw new ArgumentNullException(nameof(products));
            if (orders == null) throw new ArgumentNullException(nameof(orders));
            if (pageNumber < 1) throw new ArgumentOutOfRangeException(nameof(pageNumber), "Номер страницы должен быть >= 1");
            if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize), "Размер страницы должен быть >= 1");
            if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
                throw new ArgumentException("Минимальная цена не может быть больше максимальной");

            if (products.Any(p => p.Price < 0 || p.StockQuantity < 0))
                throw new ArgumentException("Обнаружены товары с отрицательной ценой или остатком");

            if (orders.Any(o => o.Items.Any(i => i.Quantity <= 0 || i.UnitPrice < 0)))
                throw new ArgumentException("Обнаружены некорректные строки заказов");

            var productsList = products.ToList();
            var categoriesList = categories.ToList();
            var suppliersList = suppliers.ToList();
            var ordersList = orders.ToList();

            int totalProducts = productsList.Count;
            int activeProducts = productsList.Count(p => p.IsActive);
            int outOfStockProducts = productsList.Count(p => p.StockQuantity == 0);
            decimal totalStockValue = productsList.Sum(p => p.Price * p.StockQuantity);

            decimal? averageActiveProductPrice = productsList
                .Where(p => p.IsActive)
                .Select(p => (decimal?)p.Price)
                .Average();

            bool hasOutOfStock = productsList.Any(p => p.StockQuantity == 0);
            bool allPricesValid = productsList.All(p => p.Price >= 0);
            var expensiveItem = productsList.FirstOrDefault(p => p.Price > 100_000m);
            var targetCategory = categoriesList.Single(c => c.Id == 1);

            IEnumerable<Product> filteredProducts = productsList;

            if (minPrice.HasValue)
                filteredProducts = filteredProducts.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                filteredProducts = filteredProducts.Where(p => p.Price <= maxPrice.Value);

            if (categoryId.HasValue)
                filteredProducts = filteredProducts.Where(p => p.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(searchText))
                filteredProducts = filteredProducts.Where(p => p.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase));

            filteredProducts = filteredProducts.Where(p => p.StockQuantity > 0);
            filteredProducts = filteredProducts.Where(p => p.IsActive);

            var catalogQuery =
                from p in filteredProducts
                join c in categoriesList on p.CategoryId equals c.Id into catGroup
                from c in catGroup.DefaultIfEmpty()
                join s in suppliersList on p.SupplierId equals s.Id into supGroup
                from s in supGroup.DefaultIfEmpty()
                orderby p.Price, p.Name, p.Id
                select new ProductCatalogItem(
                    p.Id,
                    p.Name,
                    c?.Name ?? "Неизвестная категория",
                    s?.Name ?? "Не указан",
                    p.Price,
                    decimal.Round(p.Price * 1.2m, 2, MidpointRounding.AwayFromZero),
                    p.StockQuantity == 0 ? "Нет в наличии" : (p.StockQuantity <= 5 ? "Мало осталось" : "В наличии"),
                    p.IsActive);
                    
            var catalogPage = catalogQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var categoryReports =
                from c in categoriesList
                join p in productsList on c.Id equals p.CategoryId into productGroup
                let prods = productGroup.ToList()
                select new CategoryStockReport(
                    c.Name,
                    prods.Count,
                    prods.Count(p => p.IsActive),
                    prods.Count(p => p.StockQuantity > 0),
                    prods.Sum(p => p.StockQuantity),
                    prods.Sum(p => p.Price * p.StockQuantity),
                    prods.Select(p => (decimal?)p.Price).Average(),
                    prods.Select(p => (decimal?)p.Price).Min(),
                    prods.Select(p => (decimal?)p.Price).Max());

            var categoriesReportList = categoryReports
                .OrderBy(r => r.CategoryName)
                .ToList();

            var priceRangeGroups = productsList
                .GroupBy(p => new
                {
                    CategoryName = categoriesList.FirstOrDefault(c => c.Id == p.CategoryId)?.Name ?? "Неизвестная",
                    PriceRange = decimal.Floor(p.Price / 500m) * 500m
                })
                .OrderBy(g => g.Key.CategoryName)
                .ThenBy(g => g.Key.PriceRange);

            Console.WriteLine("Группировка по категориям и ценовым диапазонам");
            foreach (var group in priceRangeGroups)
            {
                Console.WriteLine($"Категория: {group.Key.CategoryName}, Диапазон: {group.Key.PriceRange} - {group.Key.PriceRange + 499}, Товаров: {group.Count()}");
            }
            Console.WriteLine();

            var salesData = ordersList
                .SelectMany(o => o.Items.Select(i => new
                {
                    OrderId = o.Id,
                    ProductId = i.ProductId,
                    i.Quantity,
                    i.UnitPrice,
                    LineTotal = i.Quantity * i.UnitPrice
                }))
                .Join(productsList,
                      orderItem => orderItem.ProductId,
                      product => product.Id,
                      (orderItem, product) => new { orderItem, product })
                .GroupBy(x => x.product.Id)
                .Select(g => new ProductSalesReport(
                    g.Key,
                    g.First().product.Name,
                    g.Sum(x => x.orderItem.Quantity),
                    g.Select(x => x.orderItem.OrderId).Distinct().Count(),
                    g.Sum(x => x.orderItem.LineTotal)))
                .OrderByDescending(r => r.Revenue)
                .ThenByDescending(r => r.TotalQuantity)
                .ThenBy(r => r.ProductName)
                .ThenBy(r => r.ProductId)
                .Take(5)
                .ToList();

            return new StoreAnalyticsReport(
                totalProducts,
                activeProducts,
                outOfStockProducts,
                totalStockValue,
                averageActiveProductPrice,
                catalogPage,
                categoriesReportList,
                salesData);
        }
    }
}