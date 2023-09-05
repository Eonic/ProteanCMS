/****** Object:  StoredProcedure [dbo].[spMailFormDownload]    Script Date: 01/17/2011 14:39:14 ******/

ALTER PROCEDURE [dbo].[spMailFormDownload]
	-- Add the parameters for the stored procedure here
(
	@lastDownload int = NULL,
	@formType nvarchar(50) = ''
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	IF @lastDownload > 0 
	BEGIN
		-- Insert statements for procedure here
		Select a.dDateTime as 'DateTime', e.cActivityXml 
		from tblActivityLog a 
		inner join tblEmailActivityLog e 
		on e.nEmailActivityKey = a.nStructId
		where nActivityType = 3
		and a.cActivityDetail = @formType
		and a.dDateTime >= 
		(select dDateTime from tblActivityLog where nActivityKey = @lastDownload and cActivityDetail = @formType)
	END
	ELSE
		Select a.dDateTime as 'DateTime', 
		e.cActivityXml from tblActivityLog a 
		inner join tblEmailActivityLog e 
		on e.nEmailActivityKey = a.nStructId
		where nActivityType = 3
		and a.cActivityDetail = @formType

END


