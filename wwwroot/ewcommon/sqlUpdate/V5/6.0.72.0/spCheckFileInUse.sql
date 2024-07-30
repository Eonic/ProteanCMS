CREATE PROCEDURE dbo.spCheckFileInUse 
@filePath varchar(300)
AS
BEGIN

declare @sql nvarchar(450)

If(SELECT count(object_id) FROM sys.fulltext_indexes  where object_id = object_id('dbo.tblContent'))>0
BEGIN
	set @sql='select nContentKey,cContentSchemaName,cContentName from tblContent where contains(cContentXmlBrief,''' + @filePath + ''') or contains(cContentXmlDetail,''' + @filePath + ''')'
END
ELSE

BEGIN
set @sql='select * from tblContent where cContentXmlBrief like ''%' + @filePath + '''%'''+ ' or cContentXmlDetail like ''%''' + @filePath + '''%'''

END

print(@sql)
exec(@sql)
END