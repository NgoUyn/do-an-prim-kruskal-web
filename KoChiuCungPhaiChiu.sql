USE [PRIM_KRUSKAL_Tour]; -- Đổi tên DB nếu cần
GO

-- =============================================
-- 1. CLEANUP & SCHEMA SETUP (Giữ nguyên cấu trúc chuẩn)
-- =============================================
PRINT '--- BẮT ĐẦU KHỞI TẠO DATABASE (200 ĐIỂM/TỈNH) ---';

IF OBJECT_ID('dbo.LOCATION', 'U') IS NOT NULL DROP TABLE dbo.LOCATION;
IF OBJECT_ID('dbo.KHOANG_CACH', 'U') IS NOT NULL DROP TABLE dbo.KHOANG_CACH;
IF OBJECT_ID('dbo.TINH_THANH', 'U') IS NOT NULL DROP TABLE dbo.TINH_THANH;
GO

-- 1.1 Bảng TINH_THANH
CREATE TABLE [dbo].[TINH_THANH](
	[ID_TINH] [int] IDENTITY(1,1) NOT NULL,
	[TEN_TINH] [nvarchar](100) NOT NULL,
    [Description] [nvarchar](500) NULL, 
    [Center_Lat] [float] NULL,
    [Center_Lon] [float] NULL,
 CONSTRAINT [PK_TINH_THANH] PRIMARY KEY CLUSTERED ([ID_TINH] ASC)
);
GO

-- 1.2 Bảng KHOANG_CACH
CREATE TABLE [dbo].[KHOANG_CACH](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ID_TINH_A] [int] NOT NULL,
	[ID_TINH_B] [int] NOT NULL,
	[KHOANG_CACH_VALUE] [float] NOT NULL,
 CONSTRAINT [PK_KHOANG_CACH] PRIMARY KEY CLUSTERED ([ID] ASC)
);
GO

ALTER TABLE [dbo].[KHOANG_CACH] WITH CHECK ADD CONSTRAINT [FK_KC_TINH_A] FOREIGN KEY([ID_TINH_A]) REFERENCES [dbo].[TINH_THANH] ([ID_TINH]);
ALTER TABLE [dbo].[KHOANG_CACH] WITH CHECK ADD CONSTRAINT [FK_KC_TINH_B] FOREIGN KEY([ID_TINH_B]) REFERENCES [dbo].[TINH_THANH] ([ID_TINH]);
GO

-- 1.3 Bảng LOCATION
CREATE TABLE [dbo].[LOCATION](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ProvinceId] [int] NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Latitude] [float] NOT NULL,
	[Longitude] [float] NOT NULL,
	[Source] [nvarchar](50) DEFAULT 'Generated',
 CONSTRAINT [PK_LOCATION] PRIMARY KEY CLUSTERED ([ID] ASC)
);
GO

ALTER TABLE [dbo].[LOCATION] WITH CHECK ADD CONSTRAINT [FK_LOC_TINH] FOREIGN KEY([ProvinceId]) REFERENCES [dbo].[TINH_THANH] ([ID_TINH]);
GO

-- =============================================
-- 2. DATA SEEDING: 34 TỈNH THÀNH SÁP NHẬP
-- =============================================
PRINT '--- ĐANG NẠP 34 TỈNH THÀNH ---';
SET IDENTITY_INSERT [dbo].[TINH_THANH] ON;

INSERT INTO [dbo].[TINH_THANH] ([ID_TINH], [TEN_TINH], [Description], [Center_Lat], [Center_Lon]) VALUES
(1, N'Thành phố Hà Nội', N'Không thay đổi', 21.0285, 105.8542),
(2, N'Thành phố Huế', N'Không thay đổi (Thừa Thiên Huế)', 16.4637, 107.5909),
(3, N'Thành phố Hồ Chí Minh', N'Hợp nhất: TP.HCM + Bình Dương + Bà Rịa Vũng Tàu', 10.8231, 106.6297),
(4, N'Thành phố Hải Phòng', N'Hợp nhất: Hải Phòng + Hải Dương', 20.8449, 106.6881),
(5, N'Thành phố Đà Nẵng', N'Hợp nhất: Đà Nẵng + Quảng Nam', 16.0544, 108.2022),
(6, N'Thành phố Cần Thơ', N'Hợp nhất: Cần Thơ + Sóc Trăng + Hậu Giang', 10.0452, 105.7469),
(7, N'Tỉnh Lai Châu', N'Không thay đổi', 22.3969, 103.4610),
(8, N'Tỉnh Điện Biên', N'Không thay đổi', 21.3877, 103.0228),
(9, N'Tỉnh Sơn La', N'Không thay đổi', 21.3273, 103.9056),
(10, N'Tỉnh Lạng Sơn', N'Không thay đổi', 21.8538, 106.7603),
(11, N'Tỉnh Quảng Ninh', N'Không thay đổi', 20.9521, 107.0862),
(12, N'Tỉnh Thanh Hoá', N'Không thay đổi', 19.8078, 105.7767),
(13, N'Tỉnh Nghệ An', N'Không thay đổi', 19.2330, 104.9438),
(14, N'Tỉnh Hà Tĩnh', N'Không thay đổi', 18.3429, 105.9058),
(15, N'Tỉnh Cao Bằng', N'Không thay đổi', 22.6708, 106.2532),
(16, N'Tỉnh Tuyên Quang', N'Hợp nhất: Tuyên Quang + Hà Giang', 21.8238, 105.2162),
(17, N'Tỉnh Lào Cai', N'Hợp nhất: Lào Cai + Yên Bái', 22.4809, 103.9758),
(18, N'Tỉnh Thái Nguyên', N'Hợp nhất: Thái Nguyên + Bắc Kạn', 21.5942, 105.8482),
(19, N'Tỉnh Phú Thọ', N'Hợp nhất: Phú Thọ + Vĩnh Phúc + Hoà Bình', 21.3233, 105.2325),
(20, N'Tỉnh Bắc Ninh', N'Hợp nhất: Bắc Ninh + Bắc Giang', 21.1861, 106.0763),
(21, N'Tỉnh Hưng Yên', N'Hợp nhất: Hưng Yên + Thái Bình', 20.8529, 106.0152),
(22, N'Tỉnh Ninh Bình', N'Hợp nhất: Ninh Bình + Hà Nam + Nam Định', 20.2506, 105.9749),
(23, N'Tỉnh Quảng Trị', N'Hợp nhất: Quảng Trị + Quảng Bình', 16.8200, 107.1000),
(24, N'Tỉnh Quảng Ngãi', N'Hợp nhất: Quảng Ngãi + Kon Tum', 15.1205, 108.7923),
(25, N'Tỉnh Gia Lai', N'Hợp nhất: Gia Lai + Bình Định', 13.8179, 108.1993),
(26, N'Tỉnh Khánh Hoà', N'Hợp nhất: Khánh Hoà + Ninh Thuận', 12.2388, 109.1967),
(27, N'Tỉnh Lâm Đồng', N'Hợp nhất: Lâm Đồng + Đắk Nông + Bình Thuận', 11.9404, 108.4583),
(28, N'Tỉnh Đắk Lắk', N'Hợp nhất: Đắk Lắk + Phú Yên', 12.6667, 108.0383),
(29, N'Tỉnh Đồng Nai', N'Hợp nhất: Đồng Nai + Bình Phước', 11.0291, 107.1628),
(30, N'Tỉnh Tây Ninh', N'Hợp nhất: Tây Ninh + Long An', 11.3667, 106.1167),
(31, N'Tỉnh Vĩnh Long', N'Hợp nhất: Vĩnh Long + Bến Tre + Trà Vinh', 10.2541, 105.9723),
(32, N'Tỉnh Đồng Tháp', N'Hợp nhất: Đồng Tháp + Tiền Giang', 10.5479, 105.6683),
(33, N'Tỉnh Cà Mau', N'Hợp nhất: Cà Mau + Bạc Liêu', 9.1769, 105.1501),
(34, N'Tỉnh An Giang', N'Hợp nhất: An Giang + Kiên Giang', 10.5231, 105.1264);

SET IDENTITY_INSERT [dbo].[TINH_THANH] OFF;
GO

-- =============================================
-- 3. SINH DỮ LIỆU LOCATION (200 ĐIỂM/TỈNH)
-- =============================================
PRINT '--- ĐANG SINH DỮ LIỆU 200 ĐỊA ĐIỂM CHO MỖI TỈNH ---';

DECLARE @ProvinceID int;
DECLARE @Lat float;
DECLARE @Lon float;
DECLARE @i int;
DECLARE @RandomLat float;
DECLARE @RandomLon float;

DECLARE province_cursor CURSOR FOR 
SELECT ID_TINH, Center_Lat, Center_Lon FROM TINH_THANH;

OPEN province_cursor;
FETCH NEXT FROM province_cursor INTO @ProvinceID, @Lat, @Lon;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @i = 1;
    -- THAY ĐỔI Ở ĐÂY: Tăng lên 200 địa điểm
    WHILE @i <= 200
    BEGIN
        -- Tăng độ phân tán lên 0.5 độ (khoảng 55km) để 200 điểm không bị quá dày đặc
        SET @RandomLat = @Lat + (RAND() - 0.5) * 0.5; 
        SET @RandomLon = @Lon + (RAND() - 0.5) * 0.5;

        INSERT INTO [dbo].[LOCATION] (ProvinceId, Name, Latitude, Longitude, Source)
        VALUES (
            @ProvinceID, 
            N'Điểm tham quan ' + CAST(@i AS NVARCHAR(10)) + N' - ' + CAST(@ProvinceID AS NVARCHAR(10)), 
            @RandomLat, 
            @RandomLon, 
            'AutoGenerated'
        );

        SET @i = @i + 1;
    END
    FETCH NEXT FROM province_cursor INTO @ProvinceID, @Lat, @Lon;
END

CLOSE province_cursor;
DEALLOCATE province_cursor;
GO

-- =============================================
-- 4. SINH KHOẢNG CÁCH LIÊN TỈNH
-- =============================================
PRINT '--- ĐANG SINH DỮ LIỆU KHOẢNG CÁCH ---';

-- 4.1. Nối chuỗi
INSERT INTO KHOANG_CACH (ID_TINH_A, ID_TINH_B, KHOANG_CACH_VALUE)
SELECT 
    t1.ID_TINH, 
    t2.ID_TINH, 
    SQRT(POWER(t1.Center_Lat - t2.Center_Lat, 2) + POWER(t1.Center_Lon - t2.Center_Lon, 2)) * 100 
FROM TINH_THANH t1
JOIN TINH_THANH t2 ON t1.ID_TINH = t2.ID_TINH - 1;

-- 4.2. Random edges
INSERT INTO KHOANG_CACH (ID_TINH_A, ID_TINH_B, KHOANG_CACH_VALUE)
SELECT TOP 150 
    t1.ID_TINH, 
    t2.ID_TINH,
    SQRT(POWER(t1.Center_Lat - t2.Center_Lat, 2) + POWER(t1.Center_Lon - t2.Center_Lon, 2)) * 110
FROM TINH_THANH t1
CROSS JOIN TINH_THANH t2
WHERE t1.ID_TINH < t2.ID_TINH 
  AND t1.ID_TINH % 2 = 0 
  AND t2.ID_TINH % 3 = 0 
  AND NOT EXISTS (SELECT 1 FROM KHOANG_CACH k WHERE k.ID_TINH_A = t1.ID_TINH AND k.ID_TINH_B = t2.ID_TINH);

GO

PRINT '✅ ĐÃ HOÀN TẤT!';
PRINT '- Bảng TINH_THANH: 34 Tỉnh/Thành';
PRINT '- Bảng LOCATION: ~6800 địa điểm (200/tỉnh)';
PRINT '- Bảng KHOANG_CACH: Đã liên kết';