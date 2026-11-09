USE BookStoreDB;
GO

SELECT 
    c.CategoryName AS [Категория],
    b.Title AS [Книга],
    b.Price AS [Цена]
FROM Categories c
LEFT JOIN BookCategories bc ON c.CategoryID = bc.CategoryID
LEFT JOIN Books b ON bc.BookID = b.BookID
ORDER BY c.CategoryName, b.Title;