IF DB_ID('BookStoreDB') IS NOT NULL
BEGIN
    PRINT 'База данных BookStoreDB уже существует'
END
ELSE
BEGIN
    CREATE DATABASE BookStoreDB;
    PRINT 'База данных BookStoreDB успешно создана'
END
GO

USE BookStoreDB;
GO