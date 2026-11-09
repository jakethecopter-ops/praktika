using System;
using System.Collections.Generic;

namespace OrderProcessingApp
{
    /// Пользовательский делегат для расчёта итоговой стоимости одной позиции
    public delegate decimal ItemTotalCalculator(OrderItem item, decimal transformedUnitPrice);
    /// Представляет товарную позицию в заказе
    public sealed class OrderItem
    {
        public OrderItem(string name, decimal price, int quantity)
        {
            Name = name;
            Price = price;
            Quantity = quantity;
        }

        public string Name { get; }
        public decimal Price { get; }
        public int Quantity { get; }
    }
    /// Аргументы события завершения обработки заказа
    public sealed class OrderProcessedEventArgs : EventArgs
    {
        public OrderProcessedEventArgs(int orderId, decimal total, int processedItemsCount, DateTimeOffset processedAt)
        {
            OrderId = orderId;
            Total = total;
            ProcessedItemsCount = processedItemsCount;
            ProcessedAt = processedAt;
        }

        public int OrderId { get; }
        public decimal Total { get; }
        public int ProcessedItemsCount { get; }
        public DateTimeOffset ProcessedAt { get; }
    }
    /// Обработчик заказов интернет-магазина
    public sealed class OrderProcessor
    {
        /// Событие, уведомляющее о завершении обработки заказа
        public event EventHandler<OrderProcessedEventArgs>? OrderProcessed;
        /// Обрабатывает заказ, применяя заданные правила фильтрации, преобразования цен и расчёта
        public decimal Process(
            int orderId,
            IEnumerable<OrderItem> items,
            Predicate<OrderItem> shouldProcess,
            Func<decimal, decimal> transformPrice,
            ItemTotalCalculator calculateItemTotal,
            Action<string>? log = null)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (shouldProcess == null) throw new ArgumentNullException(nameof(shouldProcess));
            if (transformPrice == null) throw new ArgumentNullException(nameof(transformPrice));
            if (calculateItemTotal == null) throw new ArgumentNullException(nameof(calculateItemTotal));

            if (orderId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(orderId), "Идентификатор заказа должен быть положительным");
            }
            log?.Invoke("Начало обработки заказа");
            decimal totalOrderAmount = 0;
            int processedItemsCount = 0;
            foreach (var item in items)
            {
                if (item.Price < 0)
                {
                    throw new ArgumentException($"Цена товара '{item.Name}' не может быть отрицательной");
                }
                if (item.Quantity <= 0)
                {
                    throw new ArgumentException($"Количество товара '{item.Name}' должно быть положительным");
                }

                if (!shouldProcess(item))
                {
                    log?.Invoke($"Исключение товара '{item.Name}' из расчёта");
                    continue;
                }
                decimal transformedPrice = transformPrice(item.Price);
                log?.Invoke($"Преобразование цены товара '{item.Name}': {item.Price} -> {transformedPrice}");
                decimal itemTotal = calculateItemTotal(item, transformedPrice);
                log?.Invoke($"Расчёт стоимости позиции '{item.Name}': {itemTotal}");
                totalOrderAmount += itemTotal;
                processedItemsCount++;
            }
            log?.Invoke("Завершение обработки заказа");
            var eventArgs = new OrderProcessedEventArgs(orderId, totalOrderAmount, processedItemsCount, DateTimeOffset.Now);
            OnOrderProcessed(eventArgs);

            return totalOrderAmount;
        }
        /// Вызывает событие OrderProcessed с изоляцией ошибок в цепочке подписчиков
        private void OnOrderProcessed(OrderProcessedEventArgs e)
        {
            var handlers = OrderProcessed;

            if (handlers is null)
            {
                return;
            }

            foreach (EventHandler<OrderProcessedEventArgs> handler in handlers.GetInvocationList())
            {
                try
                {
                    handler(this, e);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Ошибка в обработчике события: {ex.Message}");
                }
            }
        }
    }
    /// Точка входа в приложение
    public class Program
    {
        public static void Main()
        {
            var processor = new OrderProcessor();
            // Демонстрация замыкания
            int logCounter = 0;
            Action<string> logger = message =>
            {
                logCounter++;
                Console.WriteLine($"[{logCounter:D2}] {message}");
            };
            var items = new List<OrderItem>
            {
                new OrderItem("Ноутбук", 100000m, 1),
                new OrderItem("Бракованный чехол", 500m, 1),
                new OrderItem("Мышь", 2000m, 2)
            };
            // Определение делегатов с использованием лямбда-выражений
            Predicate<OrderItem> shouldProcess = item => !item.Name.Contains("Бракованный", StringComparison.OrdinalIgnoreCase);
            // Static-лямбда для гарантии отсутствия захвата внешнего состояния
            Func<decimal, decimal> applyDiscount = static price => price * 0.9m;
            ItemTotalCalculator calculateTotal = (item, transformedPrice) => transformedPrice * item.Quantity;
            // Подписка на события
            EventHandler<OrderProcessedEventArgs> successHandler = (sender, args) =>
            {
                Console.WriteLine($"Отлично. Заказ #{args.OrderId} обработан. Сумма: {args.Total:C}, Позиций: {args.ProcessedItemsCount}");
            };
            processor.OrderProcessed += successHandler;
            processor.OrderProcessed += (sender, args) =>
            {
                throw new InvalidOperationException("Симуляция сбоя в системе уведомлений");
            };
            processor.OrderProcessed += (sender, args) =>
            {
                Console.WriteLine($"Данные заказа #{args.OrderId} отправлены в архив ({args.ProcessedAt:HH:mm:ss})");
            };
            // Обработка первого заказа
            Console.WriteLine("Обработка первого заказа");
            
            decimal finalTotal1 = processor.Process(
                orderId: 101,
                items: items,
                shouldProcess: shouldProcess,
                transformPrice: applyDiscount,
                calculateItemTotal: calculateTotal,
                log: logger
            );
            // Демонстрация отписки
            Console.WriteLine("Выполняем отписку successHandler");
            processor.OrderProcessed -= successHandler;
            // Обработка второго заказа для подтверждения отписки
            Console.WriteLine("Обработка второго заказа");
            var items2 = new List<OrderItem>
            {
                new OrderItem("Клавиатура", 5000m, 1)
            };
            decimal finalTotal2 = processor.Process(
                orderId: 102,
                items: items2,
                shouldProcess: shouldProcess,
                transformPrice: applyDiscount,
                calculateItemTotal: calculateTotal,
                log: logger
            );
            Console.WriteLine("Итоги");
            Console.WriteLine($"Сумма заказа #101: {finalTotal1:C}");
            Console.WriteLine($"Сумма заказа #102: {finalTotal2:C}");
        }
    }
}