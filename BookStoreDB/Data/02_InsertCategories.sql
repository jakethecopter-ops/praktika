USE BookStoreDB;
GO

INSERT INTO Categories (CategoryName, Description) VALUES
('Классика', 'Классическая литература'),
('Фантастика', 'Научная фантастика и фэнтези'),
('Роман', 'Романы и повести'),
('Современная проза', 'Современная литература');

PRINT 'Категории добавлены: ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' записей'
GO