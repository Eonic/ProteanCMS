/*-------------------------------------------------------------------------------------------------------------
	Created By			:	Nita Dubal
	Created On			:	07 AUG, 2023
	Last Modified By	:	Nita Dubal
	Last Modified On	:	
	Input Variables		:										
	Output Parameters	:	None
	Resultsets			:	-
	Sample Execution	:	                        
	
	Purpose				:	
	Revision			:	None
----------------------------------------------------------------------------------------------------------------*/

CREATE PROCEDURE [dbo].[spSendEmailAfterSubmitReview] 
	-- Add the parameters for the stored procedure here
	@ReviewId int=null
AS
BEGIN	
	
	select top 1 c.nContentKey,  c.cContentXmlDetail from tblContent c 	
	where cContentSchemaName ='Review' and c.nContentKey= isnull(@ReviewId,c.nContentKey)
	order by 1 desc
END


