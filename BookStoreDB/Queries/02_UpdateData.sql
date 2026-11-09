USE BookStoreDB;
GO

-- Сначала посмотрим, что обновится
SELECT 
    b.Title,
    b.Price AS [Старая цена],
    b.Price * 1.1 AS [Новая цена]
FROM Books b
INNER JOIN BookCategories bc ON b.BookID = bc.BookID
INNER JOIN Categories c ON bc.CategoryID = c.CategoryID
WHERE c.CategoryName = 'Классика';

-- Выполним обновление
UPDATE b
SET b.Price = b.Price * 1.1
FROM Books b
INNER JOIN BookCategories bc ON b.BookID = bc.BookID
INNER JOIN Categories c ON bc.CategoryID = c.CategoryID
WHERE c.CategoryName = 'Классика';

PRINT 'Цены обновлены'