USE BookStoreDB;
GO

SELECT 
    c.CategoryName AS [Категория],
    COUNT(b.BookID) AS [Количество книг],
    AVG(b.Price) AS [Средняя цена]
FROM Categories c
INNER JOIN BookCategories bc ON c.CategoryID = bc.CategoryID
INNER JOIN Books b ON bc.BookID = b.BookID
GROUP BY c.CategoryName
HAVING AVG(b.Price) > 600
ORDER BY [Средняя цена] DESC;