USE BookStoreDB;
GO

CREATE TABLE Authors (
    AuthorID INT PRIMARY KEY IDENTITY(1,1),
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    BirthDate DATE NULL,
    Country NVARCHAR(50) NULL
);
GO

PRINT 'Таблица Authors создана'
GO