USE [PRIM_KRUSKAL_Tour];
GO

PRINT N'🚀 BẮT ĐẦU KHỞI TẠO DỮ LIỆU QUY MÔ LỚN (FULL 6800 ĐỊA ĐIỂM)...';

-- 1. Xóa sạch bảng LOCATION để làm lại từ đầu cho sạch sẽ
DELETE FROM LOCATION;
DBCC CHECKIDENT ('LOCATION', RESEED, 0); -- Reset ID về 1

PRINT N'🧹 Đã dọn dẹp dữ liệu cũ.';

-- ================================================================
-- PHẦN 1: NẠP DỮ LIỆU "VÀNG" (ĐỊA DANH CÓ THẬT)
-- ================================================================
-- (Giữ nguyên danh sách địa danh thật của bạn để bản đồ có hồn)
PRINT N'⭐ Đang nạp các địa danh nổi tiếng...';

-- HÀ NỘI
INSERT INTO LOCATION (ProvinceId, Name, Latitude, Longitude, Source)
SELECT ID_TINH, Name, Lat, Lon, 'RealData' FROM TINH_THANH, (VALUES 
 (N'Hồ Hoàn Kiếm', 21.0285, 105.8542), (N'Lăng Chủ tịch Hồ Chí Minh', 21.0368, 105.8347),
 (N'Văn Miếu - Quốc Tử Giám', 21.0293, 105.8360), (N'Hoàng thành Thăng Long', 21.0341, 105.8423),
 (N'Chùa Một Cột', 21.0358, 105.8335), (N'Nhà hát Lớn Hà Nội', 21.0244, 105.8576),
 (N'Nhà thờ Lớn', 21.0287, 105.8490), (N'Hồ Tây', 21.0569, 105.8223),
 (N'Làng gốm Bát Tràng', 20.9769, 105.9206), (N'Vườn Quốc gia Ba Vì', 21.0930, 105.3630),
 (N'Royal City', 21.0039, 105.8155), (N'Times City', 20.9948, 105.8679),
 (N'Aeon Mall Long Biên', 21.0275, 105.9038), (N'Cầu Long Biên', 21.0427, 105.8611)
) AS T(Name, Lat, Lon) WHERE TEN_TINH LIKE N'%Hà Nội%';

-- TP. HỒ CHÍ MINH
INSERT INTO LOCATION (ProvinceId, Name, Latitude, Longitude, Source)
SELECT ID_TINH, Name, Lat, Lon, 'RealData' FROM TINH_THANH, (VALUES 
 (N'Chợ Bến Thành', 10.7725, 106.6980), (N'Dinh Độc Lập', 10.7770, 106.6953),
 (N'Nhà thờ Đức Bà', 10.7798, 106.6990), (N'Landmark 81', 10.7953, 106.7218),
 (N'Phố đi bộ Nguyễn Huệ', 10.7735, 106.7038), (N'Bưu điện Thành phố', 10.7800, 106.6995),
 (N'Thảo Cầm Viên', 10.7876, 106.7053), (N'Khu du lịch Suối Tiên', 10.8665, 106.8028),
 (N'Địa đạo Củ Chi', 11.1442, 106.4615), (N'Đầm Sen Park', 10.7645, 106.6385),
 (N'Bến Nhà Rồng', 10.7683, 106.7068), (N'Cầu Ánh Sao', 10.7286, 106.7196),
 (N'KDL Đại Nam (Bình Dương cũ)', 11.0393, 106.6019), (N'Biển Vũng Tàu (Vũng Tàu cũ)', 10.3420, 107.0891),
 (N'Tượng Chúa Kitô (Vũng Tàu cũ)', 10.3247, 107.0833)
) AS T(Name, Lat, Lon) WHERE TEN_TINH LIKE N'%Hồ Chí Minh%';

-- ĐÀ NẴNG
INSERT INTO LOCATION (ProvinceId, Name, Latitude, Longitude, Source)
SELECT ID_TINH, Name, Lat, Lon, 'RealData' FROM TINH_THANH, (VALUES 
 (N'Cầu Rồng', 16.0611, 108.2273), (N'Bà Nà Hills', 15.9962, 107.9950),
 (N'Biển Mỹ Khê', 16.0596, 108.2456), (N'Ngũ Hành Sơn', 16.0031, 108.2636),
 (N'Chùa Linh Ứng', 16.0996, 108.2769), (N'Phố cổ Hội An (Quảng Nam cũ)', 15.8774, 108.3348),
 (N'Thánh địa Mỹ Sơn (Quảng Nam cũ)', 15.7944, 108.1233), (N'Cù Lao Chàm', 15.9583, 108.5083)
) AS T(Name, Lat, Lon) WHERE TEN_TINH LIKE N'%Đà Nẵng%';

-- CẦN THƠ
INSERT INTO LOCATION (ProvinceId, Name, Latitude, Longitude, Source)
SELECT ID_TINH, Name, Lat, Lon, 'RealData' FROM TINH_THANH, (VALUES 
 (N'Chợ nổi Cái Răng', 10.0051, 105.7452), (N'Bến Ninh Kiều', 10.0364, 105.7855),
 (N'Nhà cổ Bình Thủy', 10.0644, 105.7597), (N'Vườn cò Bằng Lăng', 10.1833, 105.5500),
 (N'Thiền viện Trúc Lâm Phương Nam', 10.0000, 105.7167), (N'Chùa Dơi (Sóc Trăng cũ)', 9.5878, 105.9708)
) AS T(Name, Lat, Lon) WHERE TEN_TINH LIKE N'%Cần Thơ%';

-- LÂM ĐỒNG (Đà Lạt)
INSERT INTO LOCATION (ProvinceId, Name, Latitude, Longitude, Source)
SELECT ID_TINH, Name, Lat, Lon, 'RealData' FROM TINH_THANH, (VALUES 
 (N'Hồ Xuân Hương', 11.9416, 108.4425), (N'Quảng trường Lâm Viên', 11.9398, 108.4435),
 (N'Langbiang', 12.0167, 108.4333), (N'Thung lũng Tình Yêu', 11.9817, 108.4500),
 (N'Thiền viện Trúc Lâm', 11.9036, 108.4336), (N'Đồi chè Cầu Đất', 11.9167, 108.5500),
 (N'Mũi Né (Bình Thuận cũ)', 10.9333, 108.2833), (N'Đồi cát bay (Bình Thuận cũ)', 10.9450, 108.2950)
) AS T(Name, Lat, Lon) WHERE TEN_TINH LIKE N'%Lâm Đồng%';

-- KHÁNH HÒA (Nha Trang)
INSERT INTO LOCATION (ProvinceId, Name, Latitude, Longitude, Source)
SELECT ID_TINH, Name, Lat, Lon, 'RealData' FROM TINH_THANH, (VALUES 
 (N'Vinpearl Land', 12.2167, 109.2333), (N'Tháp Bà Ponagar', 12.2667, 109.1833),
 (N'Viện Hải dương học', 12.2075, 109.2136), (N'Chùa Long Sơn', 12.2500, 109.1833),
 (N'Hòn Chồng', 12.2833, 109.2000), (N'Vịnh Vĩnh Hy (Ninh Thuận cũ)', 11.7167, 109.2000)
) AS T(Name, Lat, Lon) WHERE TEN_TINH LIKE N'%Khánh Hoà%';

-- QUẢNG NINH
INSERT INTO LOCATION (ProvinceId, Name, Latitude, Longitude, Source)
SELECT ID_TINH, Name, Lat, Lon, 'RealData' FROM TINH_THANH, (VALUES 
 (N'Vịnh Hạ Long', 20.9101, 107.1839), (N'Sun World Hạ Long', 20.9500, 107.0500),
 (N'Bảo tàng Quảng Ninh', 20.9519, 107.0903), (N'Yên Tử', 21.1583, 106.7167),
 (N'Đảo Tuần Châu', 20.9333, 106.9833), (N'Biển Trà Cổ', 21.4833, 108.0167)
) AS T(Name, Lat, Lon) WHERE TEN_TINH LIKE N'%Quảng Ninh%';

-- HUẾ
INSERT INTO LOCATION (ProvinceId, Name, Latitude, Longitude, Source)
SELECT ID_TINH, Name, Lat, Lon, 'RealData' FROM TINH_THANH, (VALUES 
 (N'Đại Nội Huế', 16.4696, 107.5779), (N'Chùa Thiên Mụ', 16.4534, 107.5448),
 (N'Lăng Khải Định', 16.3990, 107.5904), (N'Cầu Tràng Tiền', 16.4688, 107.5943),
 (N'Chợ Đông Ba', 16.4709, 107.5951), (N'Biển Lăng Cô', 16.2333, 107.9667)
) AS T(Name, Lat, Lon) WHERE TEN_TINH LIKE N'%Huế%';


-- ================================================================
-- PHẦN 2: "SMART FILLER" - SINH DỮ LIỆU LỚN (6800 ĐIỂM)
-- ================================================================
PRINT N'🤖 Đang kích hoạt AI để sinh đủ 200 địa điểm/tỉnh...';

DECLARE @CurrentProvinceID int;
DECLARE @ProvName nvarchar(100);
DECLARE @CenterLat float;
DECLARE @CenterLon float;
DECLARE @ExistingCount int;
DECLARE @TargetCount int = 200; -- 🎯 MỤC TIÊU: 200 ĐIỂM/TỈNH
DECLARE @i int;

DECLARE @Prefix nvarchar(50);
DECLARE @Suffix nvarchar(50);
DECLARE @NewName nvarchar(255);
DECLARE @NewLat float;
DECLARE @NewLon float;
DECLARE @RandNum int;

DECLARE cursor_smart_fill CURSOR FOR 
SELECT ID_TINH, TEN_TINH, Center_Lat, Center_Lon FROM TINH_THANH;

OPEN cursor_smart_fill;
FETCH NEXT FROM cursor_smart_fill INTO @CurrentProvinceID, @ProvName, @CenterLat, @CenterLon;

WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT @ExistingCount = COUNT(*) FROM LOCATION WHERE ProvinceId = @CurrentProvinceID;
    SET @i = @ExistingCount;
    
    DECLARE @ShortProvName nvarchar(100) = REPLACE(REPLACE(@ProvName, N'Thành phố ', ''), N'Tỉnh ', '');

    WHILE @i < @TargetCount
    BEGIN
        -- A. RANDOM LOẠI HÌNH (Mở rộng danh sách để đỡ trùng lặp)
        SET @RandNum = ABS(CHECKSUM(NEWID()) % 16); -- Tăng lên 16 loại
        SET @Prefix = CASE @RandNum
            WHEN 0 THEN N'Khách sạn ' WHEN 1 THEN N'Nhà hàng '
            WHEN 2 THEN N'Quán Cà phê ' WHEN 3 THEN N'Công viên '
            WHEN 4 THEN N'Khu vui chơi ' WHEN 5 THEN N'Resort '
            WHEN 6 THEN N'Homestay ' WHEN 7 THEN N'Siêu thị '
            WHEN 8 THEN N'Trung tâm TM ' WHEN 9 THEN N'Bảo tàng '
            WHEN 10 THEN N'Chùa ' WHEN 11 THEN N'Đền '
            WHEN 12 THEN N'Nhà hát ' WHEN 13 THEN N'Quảng trường '
            WHEN 14 THEN N'Cầu ' ELSE N'Khu du lịch '
        END;

        -- B. RANDOM TÊN RIÊNG (Mở rộng danh sách)
        SET @RandNum = ABS(CHECKSUM(NEWID()) % 20); -- Tăng lên 20 tên
        SET @Suffix = CASE @RandNum
            WHEN 0 THEN N'Hoàng Gia' WHEN 1 THEN N'Bình Minh'
            WHEN 2 THEN N'Sông Xanh' WHEN 3 THEN N'Mùa Thu'
            WHEN 4 THEN N'Hương Biển' WHEN 5 THEN N'Phố Cổ'
            WHEN 6 THEN N'Đại Dương' WHEN 7 THEN N'Thanh Xuân'
            WHEN 8 THEN N'Hạnh Phúc' WHEN 9 THEN N'Hòa Bình'
            WHEN 10 THEN N'Ngôi Sao' WHEN 11 THEN N'Thiên Đường'
            WHEN 12 THEN N'Ánh Sao' WHEN 13 THEN N'Rạng Đông'
            WHEN 14 THEN N'Thành Đạt' WHEN 15 THEN N'Hưng Thịnh'
            WHEN 16 THEN N'Cát Tường' WHEN 17 THEN N'Phú Quý'
            WHEN 18 THEN N'An Khang' ELSE N'Thịnh Vượng'
        END;

        -- Ghép tên độc nhất
        SET @NewName = @Prefix + @Suffix + N' ' + @ShortProvName + N' (' + CAST((@i + 1) AS NVARCHAR) + N')';

        -- C. RANDOM TỌA ĐỘ (Bán kính 40-50km)
        SET @NewLat = @CenterLat + (RAND() - 0.5) * 0.4;
        SET @NewLon = @CenterLon + (RAND() - 0.5) * 0.4;

        INSERT INTO LOCATION (ProvinceId, Name, Latitude, Longitude, Source)
        VALUES (@CurrentProvinceID, @NewName, @NewLat, @NewLon, 'SmartGenerated');

        SET @i = @i + 1;
    END

    PRINT N'   + Đã bổ sung đủ ' + CAST(@TargetCount AS NVARCHAR) + N' điểm cho: ' + @ShortProvName;

    FETCH NEXT FROM cursor_smart_fill INTO @CurrentProvinceID, @ProvName, @CenterLat, @CenterLon;
END

CLOSE cursor_smart_fill;
DEALLOCATE cursor_smart_fill;
GO

DECLARE @TotalCount int;
SELECT @TotalCount = COUNT(*) FROM LOCATION;
PRINT N'=====================================================';
PRINT N'🎉 HOÀN TẤT! TỔNG CỘNG ĐÃ CÓ: ' + CAST(@TotalCount AS NVARCHAR) + N' ĐỊA ĐIỂM.';
PRINT N'=====================================================';