
CREATE PROCEDURE [dbo].[spMailFormSubmissions]
	-- Add the parameters for the stored procedure here
	@startDate datetime,
	@endDate datetime,
	@formType nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Select a.dDateTime as 'DateTime', e.cActivityXml from tblActivityLog a inner join tblEmailActivityLog e on e.nEmailActivityKey = a.nStructId
	
	where nActivityType = 3
	and a.cActivityDetail = @formType
	and a.dDateTime >= @startDate
	and a.dDateTime <= @endDate
	
END


