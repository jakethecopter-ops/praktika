USE BookStoreDB;
GO

INSERT INTO Orders (CustomerName, CustomerEmail, TotalAmount, IsCompleted) VALUES
('Иван Петров', 'ivan@example.com', 1340.00, 1),
('Мария Сидорова', 'maria@example.com', 620.00, 0),
('Алексей Иванов', 'alex@example.com', 1270.00, 1);

PRINT 'Заказы добавлены: ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' записей'
GO