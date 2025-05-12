
CREATE PROCEDURE [dbo].[spGetParentPagesForCache]          
@nContentkey int          
AS    

BEGIN   

    Declare @ParentId As Varchar(100)

    SET @ParentId = (select top 1 tcr.nContentParentId from tblContent tc
				INNER JOIN tblContentRelation tcr ON tc.nContentKey = tcr.nContentChildId
				INNER JOIN tblAudit ta ON tc.nAuditId = ta.nAuditKey
				where tc.nContentKey = @nContentkey and tc.cContentSchemaName!='Product' and ta.nstatus =1)    

	If @ParentId Is Not Null
	Begin
		SET @nContentkey = @ParentId
	END
	ELSE
	BEGIN
		SET @nContentkey = @nContentkey
	END
          
	SELECT tc.nContentKey, tc.cContentName, tc.cContentSchemaName,
		parent1.nStructKey As parent_id, parent1.cStructName As rootParentFolder, 
		parent.nStructKey AS subparent_id,
		parent.cStructName As subrootParentFolder, 
		child.nStructKey AS child_id, 
		child.cStructName As childFolder
	FROM tblcontent tc Inner Join tblContentLocation tcl On tc.nContentKey = tcl.nContentId
	Inner join tblAudit ta On tc.nAuditId = ta.nAuditKey
	Inner Join tblContentStructure child ON tcl.nStructId = child.nStructKey
	INNER JOIN tblContentStructure parent
	  ON child.nStructParId = parent.nStructKey
	INNER JOIN tblContentStructure parent1 on parent.nStructParId = parent1.nStructKey
	WHERE tcl.nContentId=@nContentkey and ta.nStatus=1


END