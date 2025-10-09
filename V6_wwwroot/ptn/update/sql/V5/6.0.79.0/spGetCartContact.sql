-- =============================================  
-- Created by:    Sonali Sonwane  
-- Create date:   25 Jul 2025  
-- Description:   Get Cart Details by Email or ProteanCartId(s)  
-- =============================================  
-- Example:  
-- EXEC [dbo].[spGetCartContact] 'santosh@infysion.com', '967336,967337,967337,967341'  
  
-- =============================================  
CREATE PROCEDURE [dbo].[spGetCartContact]  
    @EmailAddress NVARCHAR(100)  
AS  
BEGIN  
      
    select * from tblCartContact where ncontactCartId in(  
SELECT ncontactCartId  
    FROM tblCartContact   
   WHERE cContactEmail = @EmailAddress  
    
 )  
     order by 1 desc  
END;  
  
  