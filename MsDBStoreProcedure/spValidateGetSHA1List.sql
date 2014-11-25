USE [FRSCentralStorage]
GO
/****** Object:  StoredProcedure [dbo].[spGetFileMetaList]    Script Date: 10/31/2014 10:20:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER Procedure [dbo].[spValidateGetSHA1List]
@numberOfData int,
@readerName varchar(12)

AS

BEGIN

BEGIN TRAN

UPDATE TOP (@numberOfData) FileValidation  SET MetaReader = @readerName, MetaReadTime = GETUTCDATE()
OUTPUT deleted.SHA1
WHERE ValidateTime IS NULL AND MetaReader is NULL

COMMIT
END