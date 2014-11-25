USE [FRSCentralStorage]
GO
/****** Object:  StoredProcedure [dbo].[spGetFileMetaList]    Script Date: 10/31/2014 10:20:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure [dbo].[spValidateDeleteLastMark]

AS

BEGIN

BEGIN TRAN

UPDATE FileValidation  SET MetaReader = NULL, MetaReadTime = NULL
WHERE MetaReadTime IS NOT NULL AND ValidateTime IS NULL

COMMIT
END