CREATE PROCEDURE [dbo].[spOrderDownload]
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
			co.nCartStatus,
			isnull(s.dStartDate, sra.dPublishDate)as PolicyStartDate,
			sra.dExpireDate as RenewalEndDate,
			cast(co.cCartXml as xml) as cCartXml, 
			a.*,
			s.cRenewalStatus AS cSubRenewalStatus,
			sa.dUpdateDate AS cSubUpdateDate
	FROM
			dbo.tblCartOrder co
			INNER JOIN dbo.tblAudit a 
				ON a.nAuditKey = co.nAuditId
			LEFT OUTER JOIN dbo.tblSubscription s on s.nOrderId = co.nCartOrderKey 
			LEFT OUTER JOIN dbo.tblSubscriptionRenewal sr on sr.nOrderId = co.nCartOrderKey 
			LEFT OUTER JOIN dbo.tblAudit sa ON sa.nAuditKey = s.nAuditId
			LEFT OUTER JOIN dbo.tblAudit sra ON sra.nAuditKey = sr.nAuditId
	WHERE 
			(co.nCartStatus = @nOrderStage OR @nOrderStage IS NULL) 
			AND	co.cCartSchemaName = @cOrderType
			AND (s.dStartDate >= @downloadDate OR @downloadDate IS NULL)
			AND ((
				(s.dStartDate >= @dStartDate OR @dStartDate IS NULL)
				AND (s.dStartDate <= DATEADD(s, -1, DATEADD(s, 86400, @dEndDate)) OR @dEndDate IS NULL))
				or s.dStartDate is null)
			AND ((
				(sra.dPublishDate >= @dStartDate OR @dStartDate IS NULL)
				AND (sra.dPublishDate <= DATEADD(s, -1, DATEADD(s, 86400, @dEndDate)) OR @dEndDate IS NULL))
				or sra.dPublishDate is null)
				AND not(s.dStartDate is null and sra.dPublishDate is null)
	Order by PolicyStartDate
END