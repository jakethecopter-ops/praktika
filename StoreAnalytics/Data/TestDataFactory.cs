using System;
using System.Collections.Generic;
using StoreAnalytics.Models;

namespace StoreAnalytics.Data
{
    public static class TestDataFactory
    {
        public static (List<Category> Categories, List<Supplier> Suppliers, List<Product> Products, List<Order> Orders) Generate()
        {
            var categories = new List<Category>
            {
                new(1, "Электроника"),
                new(2, "Книги"),
                new(3, "Одежда"),
                new(4, "Пустая категория")
            };

            var suppliers = new List<Supplier>
            {
                new(1, "ТехноМир", 4.5m),
                new(2, "Книжный дом", 4.8m),
                new(3, "МодаСтиль", 4.2m),
                new(4, "Поставщик без товаров", 3.0m),
                new(5, "ГлобалТорг", 4.9m)
            };

            var products = new List<Product>
            {
                new(1, "Ноутбук Pro", 1, 1, 150000m, 10, true, new DateTime(2026, 1, 15)),
                new(2, "Смартфон X", 1, 1, 80000m, 5, true, new DateTime(2026, 1, 16)),
                new(3, "Наушники", 1, null, 15000m, 0, true, new DateTime(2026, 1, 17)),
                new(4, "Планшет", 1, 5, 40000m, 20, false, new DateTime(2026, 1, 18)),
                new(5, "Роман Мастер", 2, 2, 800m, 50, true, new DateTime(2026, 1, 19)),
                new(6, "Роман Мастер Переиздание", 2, 2, 800m, 30, true, new DateTime(2026, 1, 20)),
                new(7, "Учебник C#", 2, 2, 2500m, 15, true, new DateTime(2026, 1, 21)),
                new(8, "Футболка", 3, 3, 1500m, 100, true, new DateTime(2026, 1, 22)),
                new(9, "Джинсы", 3, 3, 3500m, 2, true, new DateTime(2026, 1, 23)),
                new(10, "Куртка", 3, 3, 8000m, 0, true, new DateTime(2026, 1, 24)),
                new(11, "Монитор 4K", 1, 1, 35000m, 8, true, new DateTime(2026, 1, 25)),
                new(12, "Клавиатура", 1, 1, 5000m, 25, true, new DateTime(2026, 1, 26)),
                new(13, "Мышь", 1, 1, 2500m, 40, true, new DateTime(2026, 1, 27)),
                new(14, "Коврик", 1, 1, 1000m, 60, true, new DateTime(2026, 1, 28)),
                new(15, "Детектив", 2, 2, 600m, 12, true, new DateTime(2026, 1, 29)),
                new(16, "Фантастика", 2, 2, 700m, 18, true, new DateTime(2026, 1, 30)),
                new(17, "Носки", 3, 3, 300m, 200, true, new DateTime(2026, 1, 31)),
                new(18, "Ремень", 3, 3, 1200m, 15, true, new DateTime(2026, 2, 1)),
                new(19, "Чехол для телефона", 1, 5, 500m, 3, true, new DateTime(2026, 2, 2)),
                new(20, "Нераспродаваемый товар", 2, 2, 900m, 10, true, new DateTime(2026, 2, 3))
            };

            var orders = new List<Order>
            {
                new(1, 101, new DateTime(2026, 2, 10), new List<OrderItem>
                {
                    new(1, 1, 150000m),
                    new(5, 2, 800m)
                }),
                new(2, 102, new DateTime(2026, 2, 11), new List<OrderItem>
                {
                    new(2, 1, 80000m),
                    new(8, 3, 1500m)
                }),
                new(3, 103, new DateTime(2026, 2, 12), new List<OrderItem>
                {
                    new(1, 1, 150000m),
                    new(12, 1, 5000m)
                }),
                new(4, 104, new DateTime(2026, 2, 13), new List<OrderItem>
                {
                    new(7, 1, 2500m),
                    new(9, 1, 3500m)
                }),
                new(5, 105, new DateTime(2026, 2, 14), new List<OrderItem>
                {
                    new(11, 1, 35000m),
                    new(13, 2, 2500m)
                }),
                new(6, 106, new DateTime(2026, 2, 15), new List<OrderItem>
                {
                    new(15, 1, 600m),
                    new(17, 5, 300m)
                }),
                new(7, 107, new DateTime(2026, 2, 16), new List<OrderItem>
                {
                    new(2, 1, 80000m),
                    new(18, 1, 1200m)
                }),
                new(8, 108, new DateTime(2026, 2, 17), new List<OrderItem>
                {
                    new(5, 1, 800m),
                    new(19, 2, 500m)
                })
            };

            return (categories, suppliers, products, orders);
        }
    }
}