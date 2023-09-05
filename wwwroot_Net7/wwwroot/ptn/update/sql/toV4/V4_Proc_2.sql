CREATE PROCEDURE dbo.spGetUsers (
	@nParDirId int = 0,
	@nStatus int = 99
)
AS
BEGIN

	IF @nParDirId = 0
	BEGIN
			IF @nStatus = 99 
			BEGIN
				EXEC dbo.spGetAllUsers 
			END 
			ELSE IF @nStatus = -1 or @nStatus = 1 
			BEGIN
				EXEC dbo.spGetAllUsersActive 
			END
			ELSE IF @nStatus = 0
			BEGIN
				EXEC dbo.spGetAllUsersInactive 
			END
	END
	ELSE
	BEGIN
		IF @nStatus = 99 
			BEGIN
				EXEC dbo.spGetCompanyUsers @nDirId = @nParDirId 
			END 
			ELSE IF @nStatus = -1 or @nStatus = 1 
			BEGIN
				EXEC dbo.spGetCompanyUsersActive @nDirId = @nParDirId
			END
			ELSE IF @nStatus = 0
			BEGIN
				EXEC dbo.spGetCompanyUsersInActive @nDirId = @nParDirId 
			END
	END

END