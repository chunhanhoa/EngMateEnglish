-- Thêm cột score_TTH
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TienTrinhHoc]') AND name = 'score_TTH')
BEGIN
    ALTER TABLE [dbo].[TienTrinhHoc] ADD [score_TTH] int NULL;
    PRINT 'Đã thêm cột score_TTH vào bảng TienTrinhHoc';
END
ELSE
BEGIN
    PRINT 'Cột score_TTH đã tồn tại';
END

-- Thêm cột ID_CD
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TienTrinhHoc]') AND name = 'ID_CD')
BEGIN
    ALTER TABLE [dbo].[TienTrinhHoc] ADD [ID_CD] int NULL;
    PRINT 'Đã thêm cột ID_CD vào bảng TienTrinhHoc';
END
ELSE
BEGIN
    PRINT 'Cột ID_CD đã tồn tại';
END
