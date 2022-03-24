-- =============================================
-- Author:		Trevor Spink
-- Create date: 11/03/06
-- Description:	Returns Site Structure with users access level
-- =============================================
ALTER PROCEDURE [dbo].[getContentStructure]
	-- Add the parameters for the stored procedure here
		@UserId int,
		@bAdminMode bit = 0,
		@dateNow datetime,
		@authUsersGrp int = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
IF @bAdminMode = 0
BEGIN
    -- Insert statements for procedure here
	SELECT		s.nStructKey as id, 
				s.nStructParId as parId, 
                s.cStructName as name, 
				s.cUrl as url, 
				s.cStructDescription as Description, 
				a.dPublishDate as publish, 
                a.dExpireDate as expire, 
				a.nStatus as status, 	
				dbo.fxn_checkPermission(s.nStructKey,@Userid,@authUsersGrp) as access,
				s.cStructLayout as layout
	FROM        tblContentStructure s
	INNER JOIN  tblAudit a ON s.nAuditId = a.nAuditKey
	WHERE nStatus = 1 
	and NOT (dbo.fxn_checkPermission(s.nStructKey,@Userid,@authUsersGrp) LIKE '%DENIED%')
    AND (dPublishDate is null or dPublishDate = 0 or dPublishDate <= @dateNow )
    AND (dExpireDate is null or dExpireDate = 0 or dExpireDate >= @dateNow )
    ORDER BY	s.nStructParId, s.nStructOrder 
END
ELSE
IF @UserId = -1 
BEGIN
    -- admin mode - return all pages and don't check for the permissions
	SELECT		s.nStructKey as id, 
				s.nStructParId as parId, 
                s.cStructName as name, 
				s.cUrl as url, 
				s.cStructDescription as Description, 
				a.dPublishDate as publish, 
                a.dExpireDate as expire, 
				a.nStatus as status, 	
				'ADMIN' as access,
				s.cStructLayout as layout
	FROM        tblContentStructure s
	INNER JOIN  tblAudit a ON s.nAuditId = a.nAuditKey
	ORDER BY	s.nStructParId, s.nStructOrder
END
ELSE
BEGIN
    -- admin mode, returns permissions for the user
	SELECT		s.nStructKey as id, 
				s.nStructParId as parId, 
                s.cStructName as name, 
				s.cUrl as url, 
				s.cStructDescription as Description, 
				a.dPublishDate as publish, 
                a.dExpireDate as expire, 
				a.nStatus as status, 	
				dbo.fxn_checkPermission(s.nStructKey,@Userid,@authUsersGrp) as access,
				s.cStructLayout as layout
	FROM        tblContentStructure s
	INNER JOIN  tblAudit a ON s.nAuditId = a.nAuditKey
	ORDER BY	s.nStructParId, s.nStructOrder
END
END
