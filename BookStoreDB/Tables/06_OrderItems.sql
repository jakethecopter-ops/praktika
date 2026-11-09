USE BookStoreDB;
GO

CREATE TABLE OrderItems (
    OrderItemID INT PRIMARY KEY IDENTITY(1,1),
    OrderID UNIQUEIDENTIFIER NOT NULL,
    BookID INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderID) REFERENCES Orders(OrderID),
    CONSTRAINT FK_OrderItems_Books FOREIGN KEY (BookID) REFERENCES Books(BookID)
);
GO

PRINT 'Таблица OrderItems создана'
GO