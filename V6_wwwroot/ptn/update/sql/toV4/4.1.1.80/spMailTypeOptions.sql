
CREATE PROCEDURE [dbo].[spMailTypeOptions] 

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Select distinct a.cActivityDetail as 'name', a.cActivityDetail as 'value' from tblActivityLog a inner join tblEmailActivityLog e on e.nEmailActivityKey = a.nStructId
	
	where nActivityType = 3
END

