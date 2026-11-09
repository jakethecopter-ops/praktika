USE BookStoreDB;
GO

INSERT INTO BookCategories (BookID, CategoryID) VALUES
(1, 1), (1, 3),  -- Преступление и наказание
(2, 1), (2, 3),  -- Война и мир
(3, 3), (3, 4),  -- Сто лет одиночества
(4, 2), (4, 3),  -- 1984
(5, 4),          -- Норвежский лес
(6, 1), (6, 3);  -- Анна Каренина

PRINT 'Связи книг с категориями добавлены: ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' записей'
GO