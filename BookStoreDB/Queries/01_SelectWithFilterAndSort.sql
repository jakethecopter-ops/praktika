USE BookStoreDB;
GO

SELECT 
    b.Title AS [Название книги],
    a.FirstName + ' ' + a.LastName AS [Автор],
    b.Price AS [Цена],
    b.PublishedDate AS [Год издания],
    b.StockQuantity AS [На складе]
FROM Books b
INNER JOIN Authors a ON b.AuthorID = a.AuthorID
WHERE b.IsActive = 1 AND b.Price > 500
ORDER BY b.Price DESC;