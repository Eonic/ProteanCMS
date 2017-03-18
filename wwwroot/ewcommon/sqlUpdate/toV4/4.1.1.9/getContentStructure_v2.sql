/****** Object:  StoredProcedure [dbo].[getContentStructure_v2]    Script Date: 02/05/2009 15:30:54 ******/
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[getContentStructure_v2]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
PRINT 'getContentStructure_v2 already exists which may mean that you''ve already run this upgrade script'
DROP PROCEDURE [dbo].[getContentStructure_v2]
END

/****** Object:  StoredProcedure [dbo].[getContentStructure_v2]    Script Date: 02/05/2009 15:30:54 ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[getContentStructure_v2]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'
-- =============================================
-- Author:				Trevor Spink
-- Create date:			11/03/06
-- Last Modified:		03/02/2009
-- Last modified by:	Ali Granger
-- Description:			Returns Site Structure with users access level
-- =============================================
CREATE PROCEDURE [dbo].[getContentStructure_v2]
	-- Add the parameters for the stored procedure here
		@UserId int,
		@bAdminMode bit = 0,
		@dateNow datetime,
		@authUsersGrp int = 0,
		@bReturnDenied bit = 0,
		@bShowAll bit = 0
		
AS
BEGIN
	
	SET NOCOUNT ON
	If @bAdminMode = 0 
	
		EXEC dbo.getContentStructure_Basic 
			@UserId,
			@dateNow,
			@authUsersGrp,
			@bReturnDenied,
			@bShowAll

	ELSE IF @UserId = -1

		EXEC dbo.getContentStructure_Admin

	ELSE
		
		EXEC dbo.getContentStructure_Enumerate 
			@UserId,
			@dateNow,
			@authUsersGrp,
			@bReturnDenied,
			@bShowAll		

END
' 
END

