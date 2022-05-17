
CREATE PROCEDURE [dbo].[spMailDownloadOptions] 

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Select top 50 CAST(dDateTime as nvarchar(50)) + ' - ' + cActivityDetail as 'name', nActivityKey as 'value' from tblActivityLog 
	
	where nActivityType = 14
	order by dDateTime desc
END



