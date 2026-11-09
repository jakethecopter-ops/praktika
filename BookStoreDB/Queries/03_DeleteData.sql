USE BookStoreDB;
GO

-- Покажем, что будет удалено
SELECT 
    b.Title,
    b.IsActive,
    b.StockQuantity
FROM Books b
WHERE b.IsActive = 0 AND b.StockQuantity = 0;

-- Удаляем (сначала из связанных таблиц!)
DELETE FROM BookCategories 
WHERE BookID IN (SELECT BookID FROM Books WHERE IsActive = 0 AND StockQuantity = 0);

DELETE FROM OrderItems 
WHERE BookID IN (SELECT BookID FROM Books WHERE IsActive = 0 AND StockQuantity = 0);

DELETE FROM Books 
WHERE IsActive = 0 AND StockQuantity = 0;

PRINT 'Неактивные книги удалены'