-- =============================================
-- Author:		Trevor Spink
-- Create date: <Create Date,,>
-- Description:	Gets date for order downloads
-- =============================================
CREATE PROCEDURE [dbo].[spOrderDownload]
	-- Add the parameters for the stored procedure here
		@dStartDate datetime,
		@dEndDate datetime,
		@cOrderType nvarchar(50),
		@nOrderStage int = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT co.nCartOrderKey, co.cCartXml, a.* from tblCartOrder co
		inner join tblAudit a on a.nAuditKey = co.nAuditId
	where co.nCartStatus = @nOrderStage and
	co.cCartSchemaName = @cOrderType and
	a.dInsertDate >= @dStartDate and
	a.dInsertDate <= @dEndDate 
END

