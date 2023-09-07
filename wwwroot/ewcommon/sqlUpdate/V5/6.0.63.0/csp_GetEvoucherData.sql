ALTER PROCEDURE [dbo].[csp_GetEvoucherData]     
(@intOrderId int)            
as            
begin            
            
 SELECT    top(1)      
   v.strDeliveryOption,v.strTitle,v.strFirstName,v.strLastName,VI.strOptionReference     
  FROM tblvoucherorders v    
 join tblvoucherorderitems VI on v.intOrderID=VI.intOrderID    
  where v.intOrderID=@intOrderId      
end 