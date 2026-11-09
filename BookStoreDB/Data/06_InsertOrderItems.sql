USE BookStoreDB;
GO

DECLARE @Order1 UNIQUEIDENTIFIER = (SELECT OrderID FROM Orders WHERE CustomerEmail = 'ivan@example.com');
DECLARE @Order2 UNIQUEIDENTIFIER = (SELECT OrderID FROM Orders WHERE CustomerEmail = 'maria@example.com');
DECLARE @Order3 UNIQUEIDENTIFIER = (SELECT OrderID FROM Orders WHERE CustomerEmail = 'alex@example.com');

INSERT INTO OrderItems (OrderID, BookID, Quantity, UnitPrice) VALUES
(@Order1, 1, 2, 450.00),
(@Order1, 4, 1, 380.00),
(@Order2, 3, 1, 620.00),
(@Order3, 2, 1, 890.00),
(@Order3, 4, 1, 380.00);

PRINT 'Позиции заказов добавлены: ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' записей'
GO