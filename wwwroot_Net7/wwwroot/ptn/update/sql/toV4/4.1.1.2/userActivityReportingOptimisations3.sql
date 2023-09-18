CREATE   FUNCTION [dbo].[fxn_SessionUserTable] ()
RETURNS @SessionUser TABLE 
	(	
		cSessionId nvarchar(255),
		nUserDirId int
	)
AS  
BEGIN

	--- Create the sessions table
	DECLARE @SessionUserTemp table 
		(	
			id int IDENTITY (1, 1),
			cSessionId nvarchar(255),
			nUserDirId int, 
			nCount int
		)


	INSERT INTO @SessionUserTemp
		SELECT  cSessionId,nUserDirId,nCount
		FROM	dbo.vw_SessionNonNullSummary

	INSERT INTO @SessionUser
		SELECT  cSessionId,nUserDirId
		FROM	@SessionUserTemp s
				INNER JOIN (
						SELECT	MIN(id) AS minid
						FROM	@SessionUserTemp
						GROUP BY cSessionId
				) m
				ON s.id = m.minid

	RETURN
END
