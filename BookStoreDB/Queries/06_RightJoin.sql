USE BookStoreDB;
GO

SELECT 
    b.Title AS [Книга],
    c.CategoryName AS [Категория]
FROM BookCategories bc
RIGHT JOIN Books b ON bc.BookID = b.BookID
LEFT JOIN Categories c ON bc.CategoryID = c.CategoryID
ORDER BY b.Title;