USE BookStoreDB;
GO

-- Уникальный индекс на ISBN
CREATE UNIQUE INDEX UX_Books_ISBN ON Books(ISBN);

-- Индекс для ускорения поиска по автору
CREATE INDEX IX_Books_AuthorID ON Books(AuthorID);

-- Индекс для ускорения поиска по категории
CREATE INDEX IX_BookCategories_CategoryID ON BookCategories(CategoryID);

-- Индекс для ускорения поиска заказов по email
CREATE INDEX IX_Orders_CustomerEmail ON Orders(CustomerEmail);

PRINT 'Индексы созданы'
GO