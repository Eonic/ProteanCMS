-----------------------------------------------------------    
ALTER PROCEDURE [dbo].[csp_GetVouchersByOrderId] --587834

@OrderId INT            
AS            
BEGIN            
 SELECT vo.intOrderId            
  ,intVoucherNumber            
  ,PinCode            
  ,isnull(strRecipientToName,0) AS strRecipientToName           
  ,strOptionReference            
  ,isnull(strVoucherPack, 'Standard') AS strVoucherPack            
   
 FROM tblVoucherOrderItems voi            
 INNER JOIN tblVoucherOrders vo ON vo.intOrderID = voi.intOrderID            
 WHERE ProteanCartId = (            
   SELECT ProteanCartId            
   FROM tblOrders            
   WHERE intOrderID = @Orderid   and intOptionID<>-3         
   )            
END 