USE BookStoreDB;
GO

CREATE TABLE BookCategories (
    BookID INT NOT NULL,
    CategoryID INT NOT NULL,
    AddedDate DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT PK_BookCategories PRIMARY KEY (BookID, CategoryID),
    CONSTRAINT FK_BookCategories_Books FOREIGN KEY (BookID) REFERENCES Books(BookID),
    CONSTRAINT FK_BookCategories_Categories FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID)
);
GO

PRINT 'Таблица BookCategories создана'
GO