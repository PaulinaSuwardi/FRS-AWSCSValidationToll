USE [FRSCentralStorage]
GO
/****** Object:  StoredProcedure [dbo].[spGetFileMetaList]    Script Date: 10/31/2014 10:20:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER Procedure [dbo].[spValidateAWSResult]
@sha1 nvarchar(40),
@s3Result int,
@dynamoResult int

AS

BEGIN

BEGIN TRAN


UPDATE FileValidation SET [S3Result] = @s3Result, [DynamoResult] = @dynamoResult, [ValidateTime] = GETUTCDATE() WHERE [SHA1] = @sha1 

COMMIT
END