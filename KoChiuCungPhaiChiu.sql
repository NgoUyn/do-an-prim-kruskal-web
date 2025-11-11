USE PRIM_KRUSKAL_Tour;
GO


IF OBJECT_ID('KHOANG_CACH', 'U') IS NOT NULL
    DROP TABLE KHOANG_CACH;
GO


CREATE TABLE KHOANG_CACH (
    ID INT PRIMARY KEY IDENTITY(1,1), 
    ID_TINH_A INT NOT NULL,
    ID_TINH_B INT NOT NULL,
    KHOANG_CACH_VALUE FLOAT NOT NULL CHECK (KHOANG_CACH_VALUE > 0),
    
    CONSTRAINT FK_KHOANG_CACH_TINH_A 
        FOREIGN KEY (ID_TINH_A) REFERENCES TINH_THANH(ID_TINH),
    CONSTRAINT FK_KHOANG_CACH_TINH_B 
        FOREIGN KEY (ID_TINH_B) REFERENCES TINH_THANH(ID_TINH)
);
GO


INSERT INTO KHOANG_CACH (ID_TINH_A, ID_TINH_B, KHOANG_CACH_VALUE) VALUES
(1, 2, 30),   -- HCM ↔ Bình Dương
(1, 3, 35),   -- HCM ↔ Đồng Nai
(1, 4, 95),   -- HCM ↔ Vũng Tàu
(1, 5, 99),   -- HCM ↔ Tây Ninh
(1, 6, 50),   -- HCM ↔ Long An
(2, 3, 48),   -- Bình Dương ↔ Đồng Nai
(2, 5, 90),   -- Bình Dương ↔ Tây Ninh
(3, 4, 72),   -- Đồng Nai ↔ Vũng Tàu
(6, 7, 82),   -- Long An ↔ Bến Tre
(7, 8, 65),   -- Bến Tre ↔ Vĩnh Long
(8, 9, 33),   -- Vĩnh Long ↔ Cần Thơ
(9, 10, 64);  -- Cần Thơ ↔ An Giang
GO


SELECT 'TINH_THANH' AS TableName, COUNT(*) AS RecordCount FROM TINH_THANH
UNION ALL
SELECT 'KHOANG_CACH', COUNT(*) FROM KHOANG_CACH;
GO


SELECT 
    kc.ID,
    t1.TEN_TINH AS [Từ],
    t2.TEN_TINH AS [Đến],
    kc.KHOANG_CACH_VALUE AS [Khoảng Cách (km)]
FROM KHOANG_CACH kc
INNER JOIN TINH_THANH t1 ON kc.ID_TINH_A = t1.ID_TINH
INNER JOIN TINH_THANH t2 ON kc.ID_TINH_B = t2.ID_TINH
ORDER BY kc.ID;
GO

PRINT '✅ Database đã được sửa thành công!';
PRINT '✅ Schema hiện tại khớp với code C#';
PRINT '✅ Có thể chạy ứng dụng ngay bây giờ';
