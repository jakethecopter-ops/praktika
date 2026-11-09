using System;
using System.Globalization;

Console.WriteLine("=Ввод данных о товаре=");
Console.Write("Наименование: ");
var name = Console.ReadLine();
Console.Write("Производитель: ");
var manufacturer = Console.ReadLine();
Console.Write("Цена: ");
var priceInput = Console.ReadLine();
Console.Write("Дата производства (ДД.ММ.ГГГГ): ");
var prodDateInput = Console.ReadLine();
Console.Write("Срок годности (ДД.ММ.ГГГГ): ");
var expDateInput = Console.ReadLine();
Console.Write("Размер скидки в % (введите 0, если скидки нет): ");
var discountInput = Console.ReadLine();

try
{
    var price = decimal.Parse(priceInput, CultureInfo.InvariantCulture);
    var prodDate = DateTime.ParseExact(prodDateInput, "dd.MM.yyyy", CultureInfo.InvariantCulture);
    var expDate = DateTime.ParseExact(expDateInput, "dd.MM.yyyy", CultureInfo.InvariantCulture);
    var discount = decimal.Parse(discountInput, CultureInfo.InvariantCulture);
    Product product;
    if (discount > 0)
    {
        product = new DiscountedProduct(name, manufacturer, price, prodDate, expDate, discount);
    }
    else
    {
        product = new Product(name, manufacturer, price, prodDate, expDate);
    }
    Console.WriteLine("\n=Информация о товаре=");
    Console.WriteLine(product.ToString());
}
catch (FormatException)
{
    Console.WriteLine("\nОшибка: Неверный формат данных. Пожалуйста, проверьте ввод чисел и дат.");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"\nОшибка валидации: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"\nНепредвиденная ошибка: {ex.Message}");
}

//Объявление классов
/// Базовый класс, описывающий товар
public class Product
{
    private readonly string _name;
    private readonly string _manufacturer;
    private readonly decimal _price;
    private readonly DateTime _productionDate;
    private readonly DateTime _expirationDate;
    public Product(string name, string manufacturer, decimal price, DateTime productionDate, DateTime expirationDate)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Наименование товара не может быть пустым.");
        }
        if (price <= 0)
        {
            throw new ArgumentException("Цена должна быть больше нуля.");
        }
        if (expirationDate <= productionDate)
        {
            throw new ArgumentException("Срок годности не может быть раньше или равен дате производства.");
        }

        _name = name;
        _manufacturer = manufacturer;
        _price = price;
        _productionDate = productionDate;
        _expirationDate = expirationDate;
    }
    public string Name => _name;
    public string Manufacturer => _manufacturer;
    public decimal Price => _price;
    public DateTime ProductionDate => _productionDate;
    public DateTime ExpirationDate => _expirationDate;

    public override string ToString()
    {
        return $"Товар: {_name}, Производитель: {_manufacturer}, Цена: {_price:C}, " +
               $"Годен до: {_expirationDate:dd.MM.yyyy}";
    }
}
/// Класс, описывающий товар со скидкой
public class DiscountedProduct : Product
{
    private readonly decimal _discountPercentage;
    public DiscountedProduct(
        string name,
        string manufacturer,
        decimal price,
        DateTime productionDate,
        DateTime expirationDate,
        decimal discountPercentage)
        : base(name, manufacturer, price, productionDate, expirationDate)
    {
        if (discountPercentage < 0 || discountPercentage > 100)
        {
            throw new ArgumentException("Размер скидки должен быть в диапазоне от 0 до 100%.");
        }
        _discountPercentage = discountPercentage;
    }
    public decimal DiscountPercentage => _discountPercentage;
    public decimal SalePrice => base.Price * (1 - _discountPercentage / 100m);
    public override string ToString()
    {
        return $"{base.ToString()}, Скидка: {_discountPercentage}%, Акционная цена: {SalePrice:C}";
    }
}