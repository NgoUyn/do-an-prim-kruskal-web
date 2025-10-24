CREATE DATABASE PRIM_KRUSKAL_Tour;
USE PRIM_KRUSKAL_Tour;
-- BẢNG TỈNH THÀNH
CREATE TABLE TINH_THANH (
    ID_TINH INT PRIMARY KEY,
    TEN_TINH NVARCHAR(100) NOT NULL
);

-- BẢNG KHOẢNG CÁCH
CREATE TABLE KHOANG_CACH (
    ID_TINH_A INT,
    ID_TINH_B INT,
    KHOANG_CACH_KM FLOAT CHECK (KHOANG_CACH_KM > 0),
    CONSTRAINT PK_KHOANG_CACH PRIMARY KEY (ID_TINH_A, ID_TINH_B),
    CONSTRAINT FK_TINH_A FOREIGN KEY (ID_TINH_A) REFERENCES TINH_THANH(ID_TINH),
    CONSTRAINT FK_TINH_B FOREIGN KEY (ID_TINH_B) REFERENCES TINH_THANH(ID_TINH)
);

INSERT INTO TINH_THANH VALUES 
(1, N'Hồ Chí Minh'),
(2, N'Bình Dương'),
(3, N'Đồng Nai'),
(4, N'Bà Rịa - Vũng Tàu'),
(5, N'Tây Ninh'),
(6, N'Long An'),
(7, N'Bến Tre'),
(8, N'Vĩnh Long'),
(9, N'Cần Thơ'),
(10, N'An Giang');
delete KHOANG_CACH
INSERT INTO KHOANG_CACH VALUES
(1, 2, 30),   -- HCM đến Bình Dương
(1, 3, 35),   -- HCM đến Đồng Nai
(1, 4, 95),   -- HCM đến  Bà Rịa - Vũng Tàu
(1, 5, 99),   -- HCM đến Tây Ninh
(1, 6, 50),   -- HCM đến Long An
(6, 7, 82),   -- Long An đến Bến Tre
(7, 8, 65),   -- Bến Tre đến Vĩnh Long
(8, 9, 33),   -- Vĩnh Long đến Cần Thơ
(9, 10, 64),  -- Cần Thơ đến An Giang
(3, 4, 72),   -- Đồng Nai đến Bà Rịa - Vũng Tàu
(2, 5, 90),   -- Bình Dương đến Tây Ninh
(2, 3, 48);   -- Bình Dương đến Đồng Nai

select *from TINH_THANH;
select *from KHOANG_CACH;