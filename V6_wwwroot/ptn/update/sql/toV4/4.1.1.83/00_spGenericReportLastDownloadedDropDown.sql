

CREATE PROCEDURE [dbo].[spGenericReportLastDownloadedDropDown](
	@activityDetailFilter nvarchar(255),
	@activityType int = 14,
	@includeDetailInName bit = 0,
	@top int = 25
)
AS
BEGIN

	IF @activityType = 0 OR @activityType IS NULL SET @activityType = 14

	IF @top > 0  SET ROWCOUNT @top

	SELECT
			CASE @includeDetailInName
				WHEN 1
					THEN alog.cActivityDetail + ' - ' + CAST(alog.dDateTime as nvarchar(255))
				ELSE
					CAST(alog.dDateTime as nvarchar(255))
			END AS name, 
			alog.nActivityKey AS value
	FROM
			dbo.tblActivityLog alog
	WHERE
			alog.cActivityDetail=@activityDetailFilter
			AND nActivityType = @activityType
	ORDER BY
			alog.dDateTime DESC
		
END
