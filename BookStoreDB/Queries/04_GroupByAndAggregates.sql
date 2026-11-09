USE BookStoreDB;
GO

SELECT 
    a.FirstName + ' ' + a.LastName AS [Автор],
    COUNT(b.BookID) AS [Количество книг],
    AVG(b.Price) AS [Средняя цена],
    MIN(b.Price) AS [Минимальная цена],
    MAX(b.Price) AS [Максимальная цена],
    SUM(b.Price * b.StockQuantity) AS [Общая стоимость запасов]
FROM Authors a
LEFT JOIN Books b ON a.AuthorID = b.AuthorID
GROUP BY a.FirstName, a.LastName
ORDER BY [Общая стоимость запасов] DESC;