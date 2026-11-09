USE master;
GO

IF DB_ID('BookStoreDB') IS NOT NULL
BEGIN
    ALTER DATABASE BookStoreDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE BookStoreDB;
    PRINT 'База данных BookStoreDB удалена'
END
ELSE
BEGIN
    PRINT 'База данных BookStoreDB не существует'
END
GO