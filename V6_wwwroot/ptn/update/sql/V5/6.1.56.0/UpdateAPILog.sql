CREATE PROCEDURE dbo.UpdateAPILog
@ApiLogKey int,
@ResponseData varchar(max),
@ResponseType nvarchar(500)
AS
BEGIN

Declare @timediff as bigint
Declare @RequestDateTime As datetime

Select @RequestDateTime=dRequestDateTime from tblAPILog where [nAPILogKey] = @ApiLogKey

Set @timediff=DATEDIFF(MILLISECOND, @RequestDateTime, getdate()) % 1000

UPDATE [dbo].[tblAPILog] SET [cResponseData] = @ResponseData, [cResponseType] = @ResponseType, [dResponseTimeDiff] =@timediff  WHERE [nAPILogKey] = @ApiLogKey


END