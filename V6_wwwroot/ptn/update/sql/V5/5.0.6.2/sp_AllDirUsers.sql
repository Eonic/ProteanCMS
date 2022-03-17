ALTER  PROCEDURE [dbo].[sp_AllDirUsers](@ParentDir nvarchar(255),@CurDate datetime , @UserOwners int = 0)
AS
BEGIN


DECLARE @CurDir nvarchar(255), @CurDirType nvarchar(255)
DECLARE @cDirForiegnRef nvarchar(255),@cDirName nvarchar(255),@cDirXml nvarchar(800)
DECLARE @Users TABLE(
nDirKey int,
cDirSchema nvarchar(255),
cDirForiegnRef nvarchar(255),
cDirName nvarchar(255),
cDirXml	ntext
)
DECLARE @Directories nvarchar(255)
DECLARE @Groups TABLE(nGroupId int, nComplete int)
DECLARE @CurGroup int
DECLARE @SubGroups bit

INSERT INTO @Groups (nGroupId, nComplete) VALUES (@ParentDir,0)

NextGroup:
SET @CurGroup = (SELECT TOP 1 nGroupId FROM @Groups WHERE nComplete = 0)
IF @CurGroup Is Null
GOTO FINISHED

DECLARE DirsCursor CURSOR FOR 

SELECT    tblDirectory.nDirKey, tblDirectory.cDirSchema,tblDirectory.cDirForiegnRef,tblDirectory.cDirName,tblDirectory.cDirXml
FROM         tblDirectory INNER JOIN 
             tblDirectoryRelation ON tblDirectory.nDirKey = tblDirectoryRelation.nDirChildId INNER JOIN
             tblAudit ON tblDirectory.nAuditId = tblAudit.nAuditKey
WHERE     (tblDirectoryRelation.nDirParentId = @CurGroup) AND 
	(tblAudit.dPublishDate >= @CurDate OR tblAudit.dPublishDate IS NULL) AND 
	(tblAudit.dExpireDate <= @CurDate OR tblAudit.dexpireDate IS NULL) AND 
	(tblAudit.nStatus = 1 OR tblAudit.nStatus = -1)

OPEN DirsCursor
	FETCH NEXT FROM DirsCursor INTO @CurDir, @CurDirType, @cDirForiegnRef,@cDirName,@cDirXml 
	WHILE @@FETCH_STATUS = 0
		BEGIN	
			IF @CurDirType = 'User'
				BEGIN
					IF (SELECT nDirKey FROM @Users WHERE nDirKey = @CurDir) IS NULL 
						INSERT INTO @Users (nDirKey, cDirSchema, cDirForiegnRef, cDirName, cDirXml) VALUES (@CurDir, @CurDirType, @cDirForiegnRef, @cDirName, @cDirXml)
				END
			IF  NOT @CurDirType = 'User' OR @UserOwners = 1  
				BEGIN
					INSERT INTO @Groups (nGroupId, nComplete) VALUES (@CurDir, 0)
				END 

			FETCH NEXT FROM DirsCursor INTO @CurDir, @CurDirType, @cDirForiegnRef,@cDirName,@cDirXml
		END

	CLOSE DirsCursor
DEALLOCATE DirsCursor

UPDATE @Groups SET nComplete =1 WHERE nGroupID  = @CurGroup

GOTO NextGroup

FINISHED:
SELECT * FROM @Users ORDER BY nDirKey
END
