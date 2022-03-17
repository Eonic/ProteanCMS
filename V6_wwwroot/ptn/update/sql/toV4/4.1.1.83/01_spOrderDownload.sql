ALTER PROCEDURE [dbo].[spOrderDownload]
	-- Add the parameters for the stored procedure here
		@dStartDate datetime = NULL,
		@dEndDate datetime = NULL,
		@cOrderType nvarchar(50),
		@nOrderStage int = NULL,
		@lastDownloadID int = NULL
AS
BEGIN

	SET NOCOUNT ON;

	-- Process download ID, if supplied
	DECLARE @downloadDate datetime
	IF @lastDownloadID IS NOT NULL 
	BEGIN
		SELECT @downloadDate =  dDateTime FROM dbo.tblActivityLog WHERE nActivityKey= @lastDownloadID
	END

	-- Negate other settings if downDate has been processed.
	IF @downloadDate IS NOT NULL 
	BEGIN
		SET @dStartDate = NULL
		SET @dEndDate = NULL
	END
	
	SELECT 
			co.nCartOrderKey, 
			co.cCartXml, 
			a.* 
	FROM
			dbo.tblCartOrder co
			INNER JOIN dbo.tblAudit a 
				ON a.nAuditKey = co.nAuditId
	WHERE 
			(co.nCartStatus = @nOrderStage OR @nOrderStage IS NULL) 
			AND	co.cCartSchemaName = @cOrderType
			AND (a.dInsertDate >= @downloadDate OR @downloadDate IS NULL)
			AND (a.dInsertDate >= @dStartDate OR @dStartDate IS NULL)
			AND (a.dInsertDate <= @dEndDate OR @dEndDate IS NULL)
END