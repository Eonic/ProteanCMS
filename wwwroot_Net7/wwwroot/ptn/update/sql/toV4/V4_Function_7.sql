CREATE FUNCTION fxn_getUserDepts(@nDirId int, @showCompany bit = 0)
RETURNS nvarchar(128)  
BEGIN 
DECLARE @deptlist nvarchar(255), @dept varchar(255), @company varchar(255), @lastcompany varchar(255), @bFlag bit, @bDeptFlag bit
DECLARE dept CURSOR FOR 
	SELECT  DISTINCT dircompany.cDirName, dirdept.cDirName
	FROM	tblDirectoryRelation user2dept
			INNER JOIN tblDirectory dirdept 
				ON user2dept.nDirChildId = @nDirId
					AND user2dept.nDirParentId = dirdept.nDirKey 
					AND dirdept.cDirSchema = 'Department'
			INNER JOIN tblDirectoryRelation dept2company
				ON dept2company.nDirChildId = dirdept.nDirKey
			INNER JOIN tblDirectory dircompany 
				ON  dept2company.nDirParentId = dircompany.nDirKey AND dircompany.cDirSchema = 'Company'
	ORDER BY dircompany.cDirName, dirdept.cDirName
	SET @deptlist = ''
	SET @lastcompany = ''
	SET @bFlag = 0
	SET @bDeptFlag = 0
	OPEN dept 
	FETCH NEXT FROM dept INTO @company,@dept 
	WHILE @@FETCH_STATUS = 0
	BEGIN
	
		-- check for closing bracket
		IF @company <> @lastcompany AND @showCompany = 1 
		BEGIN
			IF @bFlag = 0
			BEGIN
				 SET @bFlag = 1
			END
			ELSE
			BEGIN
				SET @deptlist = @deptlist + ')'
			END
		END
		-- check for comma
		IF @bDeptFlag = 0
		BEGIN
			 SET @bDeptFlag = 1
		END
		ELSE
		BEGIN
			SET @deptlist = @deptlist + ', '
		END	
		-- check for company
		IF @company <> @lastcompany AND @showCompany = 1 
		BEGIN
			SET @lastcompany = @company
			SET @deptlist = @deptlist + @company + ' ('
		END
	   	
		SET @deptlist = @deptlist + @dept
		FETCH NEXT FROM dept INTO @company,@dept 
	END
	IF @deptlist<>'' AND @showCompany = 1 
	BEGIN
		SET @deptlist = @deptlist + ')'
	END
	
	CLOSE dept 
	DEALLOCATE dept 
	RETURN REPLACE(@deptlist,'&amp;','&') 
END