USE BookStoreDB;
GO

SELECT 
    o.OrderID AS [ID заказа],
    o.CustomerName AS [Клиент],
    o.OrderDate AS [Дата заказа],
    b.Title AS [Книга],
    a.LastName AS [Автор],
    c.CategoryName AS [Категория],
    oi.Quantity AS [Количество],
    oi.UnitPrice AS [Цена за единицу],
    (oi.Quantity * oi.UnitPrice) AS [Итого]
FROM Orders o
INNER JOIN OrderItems oi ON o.OrderID = oi.OrderID
INNER JOIN Books b ON oi.BookID = b.BookID
INNER JOIN Authors a ON b.AuthorID = a.AuthorID
INNER JOIN BookCategories bc ON b.BookID = bc.BookID
INNER JOIN Categories c ON bc.CategoryID = c.CategoryID
WHERE o.IsCompleted = 1
ORDER BY o.OrderDate DESC;