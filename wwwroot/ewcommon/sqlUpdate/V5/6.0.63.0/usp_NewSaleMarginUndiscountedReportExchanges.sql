/*                                                                                      
Created By : Suvarna Sonawane Date : 02-May-2016                                                                                      
Reason : To get data for new sales report.                                                                                      
*/                                                                         
                                                                    
ALTER PROC [dbo].[usp_NewSaleMarginUndiscountedReportExchanges]    --'01/1/2023','01/30/2023'                                                                            
 @dtFromDate DATETIME,                                                              
 @dtToDate DATETIME                                                                            
AS                                                                            
                                                                                        
--declare  @dtFromDate DATETIME                        
--declare @dtToDate DATETIME                        
--set @dtFromDate = '08/01/2016'                        
--set @dtToDate =  '08/30/2016'                                                                            
                                                
BEGIN                                                                 
                                                                
--GET VOUCHER GROSS AND NET                                                                
CREATE TABLE #tempSaleMargin ( strCode NVARCHAR(50),strCategory NVARCHAR(50)          
,CategoryName NVARCHAR(50)   --added by Ravi Bade for the category name                                                              
,descriptions NTEXT,intForNumPeople INT, IntNumberOfSales int                                                                
,GrossAmount FLOAT, NetAmount FLOAT                                                                
,SupplierGross FLOAT, SupplierNet FLOAT                                                                     
,VoucherNumber varchar(10)                                          
,PreTax FLOAT,AfterTax FLOAT)                                                                 
                                                                
INSERT INTO #tempSaleMargin                                                                
SELECT DISTINCT strCode, O.strCategory        
,poc.PriceOptionCategoryCode +'-' + poc.PriceOptionCategoryName as CategoryName    --added by Ravi Bade for the category name                                                                            
,ISNULL(CONVERT(NVARCHAR(600),o.strHTRTitle),'')+' '+ISNULL(CONVERT(NVARCHAR(600),o.strHTRDescriptions),'')AS descriptions                                                                
,ISNULL(intForNumPeople,1) AS  intForNumPeople ,COUNT(VO.intOrderID) AS IntNumberOfSales                                                                
,CONVERT(DECIMAL(18, 2), ISNULL((SUM(                                        
CASE WHEN O.intoptionid in (3532 ,4424)                                      
THEN (ISNULL(voi.fltUnitPrice ,0))                                        
ELSE ((ISNULL(VOI.fltUnitPrice, 0)) - (ISNULL(VOI.fltPromoAmount, 0)))                  
END )),0)) AS  GrossAmount                                        
,CONVERT(DECIMAL(18, 2),ISNULL(SUM(                                        
CASE WHEN o.intoptionid in (3532 ,4424)                                      
THEN (ISNULL(voi.fltUnitPrice ,0)/120*100)                                        
ELSE                                        
(ISNULL(O.dblAmountVatcharged, 0) + ISNULL(O.dblAmountZeroRated, 0)) * ((((ISNULL(VOI.fltUnitPrice, 0)) -                                        
(ISNULL(VOI.fltPromoAmount, 0))                   
))                       
/ CASE WHEN O.dblPrice <> 0 THEN O.dblPrice ELSE VOI.fltUnitPrice END)                                        
END ),0)) AS  NetAmount                           
--,CONVERT(DECIMAL(18, 2),ISNULL(SUM(                                         
--CASE WHEN o.intoptionid=3532                                     
--THEN (ISNULL(voi.fltUnitPrice ,0)/120*100)                                           
--ELSE                                                               
--(ISNULL(O.dblAmountVatcharged, 0) + ISNULL(O.dblAmountZeroRated, 0)) * ((((ISNULL(VOI.fltUnitPrice, 0))                 
--))                                               
--/ CASE WHEN O.dblPrice <> 0 THEN O.dblPrice ELSE VOI.fltUnitPrice END)                                                                
--END ),0)) AS  NetAmount                                                              
, 0, 0,VOI.intVoucherNumber, 0,0                                                       
FROM tblOptions O          
left join tblPriceOptionCategory poc on O.strCategory=poc.PriceOptionCategoryCode    --added by Ravi Bade for the category name                                                              
LEFT OUTER JOIN tblVoucherOrderItems VOI ON O.strCode = VOI.strOptionReference                                                                
LEFT OUTER JOIN tblVoucherOrders VO ON VOI.intOrderID = VO.intOrderID                                                                
WHERE CONVERT(DATETIME,CONVERT(VARCHAR(11),VO.datDateModified)) >= @dtFromDate                                                                
AND CONVERT(DATETIME,CONVERT(VARCHAR(11),VO.datDateModified)) <= @dtToDate                                                                
AND ISNULL(VOI.isdelete,0) = 0                           
--AND VOI.intOrderID NOT IN (                                        
--SELECT intOrderID                                        
--FROM tblvoucherorderitems                                        
--WHERE CONVERT(DATETIME,CONVERT(VARCHAR(11),VO.datDateModified)) >= @dtFromDate                                        
--AND CONVERT(DATETIME,CONVERT(VARCHAR(11),VO.datDateModified)) <= @dtToDate                                        
--AND intProductID = -3 AND fltUnitPrice <> 0                                        
--)                                                               
AND ISNULL(VO.isdelete,0)=0 AND intVoucherNumber IS NOT NULL  AND strStatus <>'failed'                            
AND (ISNULL(VO.orderletter,'') IN ('Exchange','ExchangeOther','OnlineEx','PhoneEx') OR ISNULL(VO.strpaymentmethod,'') = 'Exchange' )                                                   
--AND strCode in ( 'GLBAL1', 'GLBAL2', 'SLCLT3','SPIL30', 'WFEW30', 'WFEW60')                                  
--AND strCode in ('BGFC30')                               
--AND strCode in ('TOKANY')                                   
GROUP BY strCode, O.strCategory, poc.PriceOptionCategoryCode,poc.PriceOptionCategoryName, --added by Ravi Bade for the category name        
ISNULL(CONVERT(NVARCHAR(600),o.strHTRTitle),'')+' '+ISNULL(CONVERT(NVARCHAR(600),o.strHTRDescriptions),'')                                                  
,ISNULL(intForNumPeople,1), VOI.intVoucherNumber                                                          
ORDER BY 2, 1                                                          
                                                          
--GET SUPPLIER GROSS, NET AND PROFIT IN SEPERATE TABLE                                            
DECLARE @tempSupplierMarginNew TABLE(strCode2 NVARCHAR(50),strCategory2 NVARCHAR(50)                                                 
,SupplierGross2 FLOAT, SupplierNet2 FLOAT, intForNumPeople2 INT                                          
,PreTax2 FLOAT, AfterTax2 FLOAT                                          
,VoucherNumber varchar(10)                                      
,GrossCost2 FLOAT, NetCost2 FLOAT                             
,VatStatus2 VARCHAR(100)             
,SupplierType VARCHAR(20)                                                               
)                             
                                            
;WITH TEMP AS                                            
(                                            
SELECT  strCode, O.strCategory                                                                
,CONVERT(decimal(10, 2),                                              
CASE WHEN (VOI.strVoucherStatus = 'Redeemed')                                                          
THEN                                                                            
AVG(ISNULL(INV.fltgrossamount,                                               
--              
(                                            
CASE WHEN(                                                
ISNULL(                                                
case when  isnull(sah.strSupplierType, case when isnull(s.strSupplierType, 'Principal') = 'Principal' then 'P' else 'A' end) = 'A' then                                                        CASE when s.strVATStatus = 'Not Registered' THEN h.fltPrice     
 
    
       
       
        
          
                        
 when s.strVATStatus = 'Partial VAT Activities' THEN                                                         
 case when (isnull(H.AmountTobeVatCharged,0)  + isnull(H.AmountZeroRated,0)) = 0                                                       
 then isnull(GrossPrice, 0)                                                        
 else (isnull(H.AmountTobeVatCharged,0) +  ((isnull(H.AmountTobeVatCharged,0))*20/100)  + isnull(H.AmountZeroRated,0))                                                         
 end                                                              
 else h.fltPrice + h.fltPrice *20/100 end                                                                 
else                                                                                             
 CASE when s.strVATStatus = 'Not Registered' THEN                                                                                          
   case when Honoured = 'Yes' then round(isnull(fltPrice,0), 2)                                                                                               
   else round((isnull(H.AmountTobeVatCharged,0) +  isnull(H.AmountZeroRated,0)), 2)                                                                                               
   end                                                                                         
 WHEN s.strVATStatus = 'Partial VAT Activities' THEN                                                                                       
   round((isnull(H.AmountTobeVatCharged,0) +  isnull(H.AmountZeroRated,0)) + ((isnull(H.AmountTobeVatCharged,0))*20/100) , 2)                                                                                        
 ELSE                                                               
   round((isnull(H.AmountTobeVatCharged,0) +  isnull(H.AmountZeroRated,0)) + ((isnull(H.AmountTobeVatCharged,0)*20/100)) , 2)                                 
 END                                                                                            
end, sa.CostIncludingVAT)) <> 0                                                 
THEN                                                 
ISNULL(                                                
case when  isnull(sah.strSupplierType, case when isnull(s.strSupplierType, 'Principal') = 'Principal' then 'P' else 'A' end) = 'A' then                                                                 
 CASE when s.strVATStatus = 'Not Registered' THEN h.fltPrice                                                             
 when s.strVATStatus = 'Partial VAT Activities' THEN                          
 case when (isnull(H.AmountTobeVatCharged,0)  + isnull(H.AmountZeroRated,0)) = 0                                                         
 then isnull(GrossPrice, 0)                                                        
 else (isnull(H.AmountTobeVatCharged,0) +  ((isnull(H.AmountTobeVatCharged,0))*20/100)  + isnull(H.AmountZeroRated,0))                                                         
 end                                                              
 else h.fltPrice + h.fltPrice *20/100 end                                                                 
else                                                      
 CASE when s.strVATStatus = 'Not Registered' THEN           
   case when Honoured = 'Yes' then round(isnull(fltPrice,0), 2)                                          
   else round((isnull(H.AmountTobeVatCharged,0) +  isnull(H.AmountZeroRated,0)), 2)                                                                                               
   end                                                                                         
 WHEN s.strVATStatus = 'Partial VAT Activities' THEN                                                                                       
  case when (isnull(H.AmountTobeVatCharged,0)  + isnull(H.AmountZeroRated,0)) = 0                                     
 then H.fltPrice                                                                                            
  else round((isnull(H.AmountTobeVatCharged,0) +  isnull(H.AmountZeroRated,0)) + ((isnull(H.AmountTobeVatCharged,0))*20/100) , 2)                                         
 end      
 ELSE                                                                   
   round((isnull(H.AmountTobeVatCharged,0) +  isnull(H.AmountZeroRated,0)) + ((isnull(H.AmountTobeVatCharged,0)*20/100)) , 2)                                                              
 END                                                                                                
end, sa.CostIncludingVAT)                                              
ELSE                                                
dbo.fn_GetGrossFromHistory(sa.intSupplierActivityID)                                                
END                                             
) * ISNULL(VOI.intForNumPeople, 1)                                            
--                                              
))                                              
ELSE                                               
--------------------------------------                                              
(CASE WHEN(                                                
ISNULL(                                                
case when  isnull(sah.strSupplierType, case when isnull(s.strSupplierType, 'Principal') = 'Principal' then 'P' else 'A' end) = 'A' then                                                        
 CASE when s.strVATStatus = 'Not Registered' THEN h.fltPrice                                                             
 when s.strVATStatus = 'Partial VAT Activities' THEN                                                         
 case when (isnull(H.AmountTobeVatCharged,0)  + isnull(H.AmountZeroRated,0)) = 0                                                         
 then  h.fltPrice --+ (h.fltPrice *20/100)--isnull(GrossPrice, 0)                          
 else (isnull(H.AmountTobeVatCharged,0) +  ((isnull(H.AmountTobeVatCharged,0))*20/100)  + isnull(H.AmountZeroRated,0))                                                         
 end                                                              
 else  h.fltPrice + (h.fltPrice *20/100)                  
 end                                                                 
else                                                                                
 CASE when s.strVATStatus = 'Not Registered' THEN                                                                                          
   case when Honoured = 'Yes' then round(isnull(fltPrice,0), 2)                       
   else round((isnull(H.AmountTobeVatCharged,0) +  isnull(H.AmountZeroRated,0)), 2)                                                                                               
   end                                                                                         
 WHEN s.strVATStatus = 'Partial VAT Activities' THEN                                                                                       
  round((isnull(H.AmountTobeVatCharged,0) +  isnull(H.AmountZeroRated,0)) + ((isnull(H.AmountTobeVatCharged,0))*20/100) , 2)                                                                                        
 ELSE                                            
   round((isnull(H.AmountTobeVatCharged,0) +  isnull(H.AmountZeroRated,0)) + ((isnull(H.AmountTobeVatCharged,0)*20/100)) , 2)                                                          
 END                                                                                            
end, sa.CostIncludingVAT)) <> 0                                                 
THEN                                                 
ISNULL(                                                
case when  isnull(sah.strSupplierType, case when isnull(s.strSupplierType, 'Principal') = 'Principal' then 'P' else 'A' end) = 'A' then                                                                 
 CASE when s.strVATStatus = 'Not Registered' THEN h.fltPrice                                                             
 when s.strVATStatus = 'Partial VAT Activities' THEN                                        
 case when (isnull(H.AmountTobeVatCharged,0)  + isnull(H.AmountZeroRated,0)) = 0                                                         
 then  h.fltPrice --+ (h.fltPrice *20/100)--isnull(GrossPrice, 0)                                                        
 else (isnull(H.AmountTobeVatCharged,0) +  ((isnull(H.AmountTobeVatCharged,0))*20/100)  + isnull(H.AmountZeroRated,0))                                                         
 end                                                              
 else  h.fltPrice + (h.fltPrice *20/100)                                            
 end                                                 
else                    
 CASE when s.strVATStatus = 'Not Registered' THEN                                                                                          
   case when Honoured = 'Yes' then round(isnull(fltPrice,0), 2)                                                       
   else round((isnull(H.AmountTobeVatCharged,0) +  isnull(H.AmountZeroRated,0)), 2)                                                                                               
   end                                                                                         
 WHEN s.strVATStatus = 'Partial VAT Activities' THEN                                           
   round((isnull(H.AmountTobeVatCharged,0) +  isnull(H.AmountZeroRated,0)) + ((isnull(H.AmountTobeVatCharged,0))*20/100) , 2)                                                                                        
 ELSE                                                               
   round((isnull(H.AmountTobeVatCharged,0) +  isnull(H.AmountZeroRated,0)) + ((isnull(H.AmountTobeVatCharged,0)*20/100)) , 2)                                                          
 END                                                                                            
end, sa.CostExcludingVAT                                            
)                                                
ELSE                                                
dbo.fn_GetGrossFromHistory(sa.intSupplierActivityID)                                                
END) * ISNULL(VOI.intForNumPeople, 1)                                             
-----------------------------------                                              
END)                                                
as SupplierGross                                         
,CONVERT(decimal(10, 2),                                  
CASE WHEN VOI.strVoucherStatus = 'Redeemed'                                                           
THEN                                                                         
AVG(ISNULL(        
 CASE WHEN (INV.fltVatchargeamount + INV.fltZeroratedamount) = 0         
 THEN INV.fltOriginalPrice ELSE (INV.fltVatchargeamount + INV.fltZeroratedamount) END,           
--                    
(                                              
CASE WHEN                                                
(                                                
CASE WHEN ISNULL(fltPrice, sa.CostExcludingVAT) = 0                                                 
THEN ISNULL(dbo.fn_GetNetFromHistory(sa.intSupplierActivityID), 0)                                                
ELSE ISNULL(fltPrice, sa.CostExcludingVAT)                                               
END                                                
) IS NULL                                                 
THEN                                                 
(                              
(                                                                            
CASE WHEN s.strSupplierType = 'Agent' THEN                                                                            
 CASE WHEN sah.strSupplierType IS NOT NULL  THEN                                                                            
  CASE WHEN s.strVATStatus = 'Not Registered' THEN                                                                
   ISNULL(dbo.fn_GetNetFromHistory(sa.intSupplierActivityID), ISNULL(sa.CostExcludingVat, 0))                                                                        
  ELSE                                                                             
   CASE WHEN (ISNULL(H.AmountTobeVatCharged,0)  + ISNULL(H.AmountZeroRated,0)) = 0                                                                           
   THEN ISNULL(H.fltPrice, 0)                                                                            
   ELSE (ISNULL(H.AmountTobeVatCharged,0)  + ISNULL(H.AmountZeroRated,0))                                                                            
   END                                                                            
  END                                                                            
 ELSE                           
   CASE WHEN (ISNULL(sa.AmountTobeVatCharged,0) + ISNULL(sa.AmountZeroRated,0)) <> 0                                                       
   THEN (ISNULL(sa.AmountTobeVatCharged,0) + ISNULL(sa.AmountZeroRated,0))                                                                            
   ELSE                     
    CASE WHEN ISNULL(sa.CostExcludingVat,0) = 0                                                                            
    THEN ISNULL(dbo.fn_GetGrossFromHistory(sa.intSupplierActivityID), isnull(sa.CostExcludingVat, 0))                                  
    ELSE ISNULL(sa.CostExcludingVat,0)                                                                      
    END                                                                            
   END                                                                             
 END                                                                     
ELSE                                                                            
 CASE WHEN sah.strSupplierType IS NULL                                            
 THEN ISNULL(sa.AmountTobeVatCharged, 0) + (ISNULL(sa.AmountZeroRated, 0))                                                                            
 ELSE (ISNULL(H.AmountTobeVatCharged,0)  + ISNULL(H.AmountZeroRated,0))                                                        
 END                                                                            
END)                                                
)                                                
ELSE                                                 
 CASE WHEN ISNULL(fltPrice, (ISNULL(sa.AmountTobeVatCharged, 0) + (ISNULL(sa.AmountZeroRated, 0)))) = 0                                                 
 THEN ISNULL(dbo.fn_GetNetFromHistory(sa.intSupplierActivityID), 0)                                                
 ELSE ISNULL(fltPrice, (ISNULL(sa.AmountTobeVatCharged, 0) + (ISNULL(sa.AmountZeroRated, 0))))                                               
 END                                                
END                                            
) * ISNULL(VOI.intForNumPeople, 1)                                             
--                                              
))                                               
ELSE                              
------------------------------------------------                                               
(                                            
CASE WHEN                                                
(                                                
CASE WHEN ISNULL(fltPrice, sa.CostExcludingVAT) = 0                      
THEN ISNULL(dbo.fn_GetNetFromHistory(sa.intSupplierActivityID), 0)                                                
ELSE ISNULL(fltPrice, sa.CostExcludingVAT)                                                 
END                                                
) IS NULL                                                 
THEN                       
(                                                
AVG(                                                                            
CASE WHEN s.strSupplierType = 'Agent' THEN                                                                            
 CASE WHEN sah.strSupplierType IS NOT NULL  THEN                                          CASE WHEN s.strVATStatus = 'Not Registered' THEN                                                                            
   ISNULL(dbo.fn_GetNetFromHistory(sa.intSupplierActivityID), ISNULL(sa.CostExcludingVat, 0))                                                                          
  ELSE                                                                             
   CASE WHEN (ISNULL(H.AmountTobeVatCharged,0)  + ISNULL(H.AmountZeroRated,0)) = 0                                                                            
   THEN ISNULL(H.fltPrice, 0)                                                                            
   ELSE (ISNULL(H.AmountTobeVatCharged,0)  + ISNULL(H.AmountZeroRated,0))                                                                            
   END                          
  END                                                                            
 ELSE                                                                           
   CASE WHEN (ISNULL(sa.AmountTobeVatCharged,0) + ISNULL(sa.AmountZeroRated,0)) <> 0                                                                             
   THEN (ISNULL(sa.AmountTobeVatCharged,0) + ISNULL(sa.AmountZeroRated,0))                                                                            
   ELSE                                                                            
    CASE WHEN ISNULL(sa.CostExcludingVat,0) = 0                                              
    THEN ISNULL(dbo.fn_GetGrossFromHistory(sa.intSupplierActivityID), isnull(sa.CostExcludingVat, 0))                                                                          
    ELSE ISNULL(sa.CostExcludingVat,0)                                                                      
    END       END                                                                    
 END                                                                     
ELSE                                                                            
 CASE WHEN sah.strSupplierType IS NULL                                                                            
 THEN ISNULL(sa.AmountTobeVatCharged, 0) + (ISNULL(sa.AmountZeroRated, 0))                                                                            
 ELSE (ISNULL(H.AmountTobeVatCharged,0)  + ISNULL(H.AmountZeroRated,0))                                    
 END                                                                          
END)                                                
)                                                
ELSE                                                 
 CASE WHEN ISNULL(fltPrice, (ISNULL(sa.AmountTobeVatCharged, 0) + (ISNULL(sa.AmountZeroRated, 0)))) = 0                                                 
 THEN ISNULL(dbo.fn_GetNetFromHistory(sa.intSupplierActivityID), 0)                                                
 ELSE ISNULL(fltPrice, (ISNULL(sa.AmountTobeVatCharged, 0) + (ISNULL(sa.AmountZeroRated, 0))))                                               
 END                                                
END                                             
)* ISNULL(VOI.intForNumPeople, 1)                                            
------------------------------------------------                                               
END                                              
) AS SupplierNet                                                                               
,ISNULL(VOI.intForNumPeople, 1)   as intForNumPeople                                       
,voi.intVoucherNumber                                       
,S.strVATStatus                                      
,CONVERT(DECIMAL(18, 2),ISNULL(SUM(              
CASE WHEN o.intoptionid in (3532 ,4424)                               
THEN (ISNULL(voi.fltUnitPrice ,0)/120*100)                                        
ELSE                                        
(ISNULL(O.dblAmountVatcharged, 0) + ISNULL(O.dblAmountZeroRated, 0)) * ((((ISNULL(VOI.fltUnitPrice, 0)) -                                        
(ISNULL(VOI.fltPromoAmount, 0))                   
))                       
/ CASE WHEN O.dblPrice <> 0 THEN O.dblPrice ELSE VOI.fltUnitPrice END)                                        
END ),0)) AS  NetAmount                   
,CONVERT(DECIMAL(18, 2), ISNULL((SUM(                                        
CASE WHEN O.intoptionid in (3532 ,4424)                                       
THEN (ISNULL(voi.fltUnitPrice ,0))                                        
ELSE ((ISNULL(VOI.fltUnitPrice, 0)) - (ISNULL(VOI.fltPromoAmount, 0)))                  
END )),0)) AS  GrossAmount                          
--,CONVERT(DECIMAL(18, 2), (SUM(ISNULL(VOI.fltUnitPrice, 0))),0) AS  GrossAmount                                   
,convert(decimal(10, 2),                                                                                
AVG(                                                                                
case when s.strSupplierType = 'Agent' then                                                                                
 case when sah.strSupplierType  IS NOT NULL  THEN                                                               
  case when s.strVATStatus = 'Partial VAT Activities' then                                                                                
   case when (isnull(H.AmountTobeVatCharged,0)  + isnull(H.AmountZeroRated ,0)) = 0                                                                                 
   then H.fltPrice                                                                                
   else (isnull(H.AmountTobeVatCharged ,0) + isnull(H.AmountZeroRated ,0))                                    
   end                                              
  when s.strVATStatus = 'Registered' then                                                                                
   case when (isnull(H.AmountTobeVatCharged,0)  + isnull(H.AmountZeroRated,0)) = 0                                                                                
   then H.fltPrice                                            
   else (isnull(H.AmountTobeVatCharged,0)  + isnull(H.AmountZeroRated,0))                                                                                
   end                                                                                
  else                                                                      
   CASE when s.strVATStatus = 'Not Registered' THEN                                                                                                    
    case when Honoured = 'Yes' then round(isnull(fltPrice,0), 2)                                                 
    else                 
     isnull(sa.AmountTobeVatCharged, isnull(H.AmountTobeVatCharged,0)) + ISNULL(sa.AmountZeroRated, isnull(H.AmountZeroRated,0))                                                                                                     
    end                                                                 
   ELSE                                                                         
   (isnull(sa.AmountTobeVatCharged, isnull(H.AmountTobeVatCharged,0)) +  ISNULL(sa.AmountZeroRated, isnull(H.AmountZeroRated,0)))                                                                   
   END                               
  end                                                                                 
 else                                       
  case when s.strVATStatus = 'Partial VAT Activities' then (isnull(sa.AmountTobeVatCharged,0) + isnull(sa.AmountZeroRated,0))                                                                                
  else  isnull(sa.CostExcludingVat, 0)                                                                                
  end                                                             
 end                                                                                
else                                                                                
 CASE when s.strVATStatus = 'Not Registered' THEN                                                                                                    
  case when Honoured = 'Yes' then round(isnull(fltPrice,0), 2)                                                                                                         
  else                                                                      
  isnull(sa.AmountTobeVatCharged, isnull(H.AmountTobeVatCharged,0)) + ISNULL(sa.AmountZeroRated, isnull(H.AmountZeroRated,0))                                                       
  end                                                            
 ELSE                                                                         
  (isnull(sa.AmountTobeVatCharged, isnull(H.AmountTobeVatCharged,0)) + ISNULL(sa.AmountZeroRated, isnull(H.AmountZeroRated,0)))                                                                   
 END                                                          
end)) AS NetCost                                           
,convert(decimal(10, 2),                                                                                
AVG(                                                                                
case when s.strSupplierType = 'Agent' then                                            
 case when sah.strSupplierType  IS NOT NULL  THEN                                                                 
  case when s.strVATStatus = 'Partial VAT Activities' then                                                       
   case when (isnull(H.AmountTobeVatCharged,0)  + isnull(H.AmountZeroRated ,0)) = 0                                                                                 
   then isnull(H.fltPrice, 0) + (isnull(H.fltPrice, 0) * 20 /100)                                                                              
 else ((isnull(H.AmountTobeVatCharged ,0) + (isnull(H.AmountTobeVatCharged, 0) * 20 /100)) + isnull(H.AmountZeroRated ,0))                                                                                
   end                                                                           
  when s.strVATStatus = 'Registered' then                                                                                
   case when (isnull(H.AmountTobeVatCharged,0)  + isnull(H.AmountZeroRated,0)) = 0                                            
   then isnull(H.fltPrice, 0) + (isnull(H.fltPrice, 0) * 20 /100)                                                                             
   else ((isnull(H.AmountTobeVatCharged,0) + (isnull(H.AmountTobeVatCharged, 0) * 20 /100) )  + isnull(H.AmountZeroRated,0))                                                                                
   end                                                     
  else                                                                      
   CASE when s.strVATStatus = 'Not Registered' THEN                                                                                                    
    case when Honoured = 'Yes' then round(isnull(fltPrice,0), 2)                                                                                                         
    else                                                                      
     isnull(sa.AmountTobeVatCharged, isnull(H.AmountTobeVatCharged,0)) + ISNULL(sa.AmountZeroRated, isnull(H.AmountZeroRated,0))             
    end                                                                 
   ELSE                                                                         
   ((isnull(sa.AmountTobeVatCharged, isnull(H.AmountTobeVatCharged,0)) + ((isnull(sa.AmountTobeVatCharged, isnull(H.AmountTobeVatCharged,0))) * 20 /100))                                           
   +  ISNULL(sa.AmountZeroRated, isnull(H.AmountZeroRated,0)))                                                                   
   END                                                                  
  end                                                                                 
 else                                                      
  case when s.strVATStatus = 'Partial VAT Activities' then                                           
  ((isnull(sa.AmountTobeVatCharged,0)  + (isnull(sa.AmountTobeVatCharged, 0) * 20 /100) ) + isnull(sa.AmountZeroRated,0))                                       
  else  isnull(sa.CostIncludingVat, 0)                              
  end                                                                                 
 end                                                                                
else              
 CASE when s.strVATStatus = 'Not Registered' THEN                                                                                                    
  case when Honoured = 'Yes' then round(isnull(fltPrice,0), 2)                                                                                                         
  else                                                                      
  isnull(sa.AmountTobeVatCharged, isnull(H.AmountTobeVatCharged,0)) + ISNULL(sa.AmountZeroRated, isnull(H.AmountZeroRated,0))                                                                                                     
  end                                            
ELSE                                                                         
  ((isnull(sa.AmountTobeVatCharged, isnull(H.AmountTobeVatCharged,0)) + ((isnull(sa.AmountTobeVatCharged, isnull(H.AmountTobeVatCharged,0))) * 20 /100))                                           
  +  ISNULL(sa.AmountZeroRated, isnull(H.AmountZeroRated,0)))                                                
 END                                                          
end)) AS GrossCost                                          
,s.strSupplierType                                            
FROM tblSupplierActivity SA                                                                
LEFT OUTER JOIN tblOptions O ON SA.intActivityID = O.intOptionID    
LEFT OUTER JOIN tblVoucherOrderItems VOI ON O.strCode = VOI.strOptionReference                                                                
LEFT OUTER JOIN tblVoucherOrders VO ON VOI.intOrderID = VO.intOrderID                                                                  
JOIN tblSupplier s ON sa.intSupplierID = s.intSupplierID                                                           
LEFT OUTER JOIN tblInvoiceLineItem INV on VOI.intOrderItemID = INV.intVoucherID                                                              
OUTER APPLY                      
(                                                                            
SELECT dtDateModified, AmountTobeVatCharged, AmountZeroRated, dtStartDate, dtEndDate, fltPrice, GrossPrice, Honoured                                                                           
FROM tblSupplierActivityPriceHistory H1                                                                          
WHERE sa.intSupplierActivityID = H1.intSupplierActivityID                                                  
AND VO.datDateModified BETWEEN H1.dtStartDate and H1.dtEndDate                                                                           
)H                                                                            
OUTER APPLY                                                                            
(                                                           
SELECT DISTINCT TOP 1 strSupplierType                                            
FROM tblSupplierAgentChangeHistory h1                                                                            
WHERE h1.intSupplierID = s.intSupplierID                                                                            
AND CONVERT(VARCHAR, H.dtDateModified, 110) >= CONVERT(VARCHAR, h1.datChange, 110)                                                                            
AND CONVERT(VARCHAR, H.dtDateModified, 110) <= CONVERT(VARCHAR, ISNULL(h1.datChangeEnd, GETDATE()), 110)                                                                            
)sah                                                              
WHERE CONVERT(DATETIME,CONVERT(VARCHAR(11),VO.datDateModified)) >= @dtFromDate                                                                
AND CONVERT(DATETIME,CONVERT(VARCHAR(11),VO.datDateModified)) <= @dtToDate                                                                
AND ISNULL(VOI.isdelete,0) = 0              
--AND VOI.intOrderID NOT IN (                                        
--SELECT intOrderID                                        
--FROM tblvoucherorderitems                                        
--WHERE CONVERT(DATETIME,CONVERT(VARCHAR(11),VO.datDateModified)) >= @dtFromDate                                        
--AND CONVERT(DATETIME,CONVERT(VARCHAR(11),VO.datDateModified)) <= @dtToDate                                        
--AND intProductID = -3 AND fltUnitPrice <> 0                                        
--)                                                            
AND ISNULL(VO.isdelete,0)=0 AND intVoucherNumber IS NOT NULL  AND strStatus <>'failed'                                                                
AND (ISNULL(VO.orderletter,'') IN ('Exchange','ExchangeOther','OnlineEx','PhoneEx') OR ISNULL(VO.strpaymentmethod,'') = 'Exchange' )                                                              
AND ISNULL(blnArchived, 0) = 0 AND ISNULL(SA.blnDelete, 0) = 0                                      
--AND strCode in ( 'GLBAL1', 'GLBAL2', 'SLCLT3','SPIL30', 'WFEW30', 'WFEW60')         --AND strCode in ('BGFC30')             
--AND strCode in ('TOKANY')                                      
GROUP BY strCode, O.strCategory, sah.strSupplierType, s.strSupplierType,SA.intSupplierActivityID, o.dblPrice, sa.CostExcludingVat, s.strVATStatus                                                         
,o.dblAmountVatcharged, o.dblAmountZeroRated, sa.AmountTobeVatCharged, sa.AmountZeroRated, sa.AmountZeroRated,sa.CostIncludingVAT                                                                
,VOI.intVoucherNumber, VOI.strVoucherStatus                                                 
,h.fltPrice, H.AmountTobeVatCharged, H.AmountZeroRated, H.GrossPrice,H.Honoured, ISNULL(VOI.intForNumPeople, 1) )                                            
                            
 INSERT INTO @tempSupplierMarginNew                                            
  SELECT strCode, strCategory                              
  ,CASE WHEN strCode = 'TOKANY' THEN AVG(GrossAmount) * 0.78  ELSE AVG(SupplierGross) END SupplierGross                              
  ,CASE WHEN strCode = 'TOKANY' THEN AVG(NetAmount) * 0.78  ELSE AVG(SupplierNet) END SupplierNet                              
  , intForNumPeople                                                
,CASE WHEN strCode = 'TOKANY' THEN  AVG(GrossAmount) - (AVG(GrossAmount) * 0.78)                          
  --ELSE (AVG(GrossAmount) - (AVG(CASE WHEN GrossCost = 0 THEN SupplierGross ELSE GrossCost END)* intForNumPeople))                             
  ELSE (AVG(GrossAmount) - (AVG(CASE WHEN SupplierGross = 0 THEN (GrossCost * intForNumPeople) ELSE SupplierGross END)))                            
  END PreTax                                   
  ,CASE WHEN strCode = 'TOKANY'                               
  THEN                               
 CASE WHEN strSupplierType = 'Agent'                                     
   THEN  (AVG(NetAmount) - (AVG(NetAmount) * 0.78)) /120*100                          
   ELSE  AVG(NetAmount) - (AVG(NetAmount) * 0.78)                          
 END                               
  ELSE                              
   CASE WHEN strSupplierType = 'Agent'                                     
   --THEN ((AVG(GrossAmount) - (AVG(CASE WHEN GrossCost = 0 THEN SupplierGross ELSE GrossCost END)* intForNumPeople))/120*100)                                     
   --ELSE (AVG(NetAmount) - (AVG(CASE WHEN NetCost = 0 THEN SupplierNet ELSE NetCost END)* intForNumPeople))                            
   THEN ((AVG(GrossAmount) - (AVG(CASE WHEN SupplierGross = 0 THEN (GrossCost * intForNumPeople) ELSE SupplierGross END)))/120*100)                                     
   ELSE 0--(AVG(NetAmount) - (AVG(CASE WHEN SupplierNet = 0 THEN (NetCost * intForNumPeople) ELSE SupplierNet END)))                                     
   END                               
  END as AfterTax                               
  ,0                              
  ,AVG(GrossAmount),AVG(NetAmount)                                      
  ,strVATStatus            
  ,strSupplierType                                      
  From Temp                                                      
  Group by strCode, strCategory, intForNumPeople,strVATStatus,strSupplierType  --,intVoucherNumber                                                                           
                                             
             --select * from @tempSupplierMarginNew                                
                                                   
--UPDATE MAIN TABLE FOR SUPPIER GROSS, NET AND MARGIN AFTER TAX                                                                
UPDATE #tempSaleMargin                                                                
SET                                             
#tempSaleMargin.SupplierGross = m.SupplierGross2                                           
,#tempSaleMargin.SupplierNet = m.SupplierNet2                                           
,#tempSaleMargin.PreTax = m.PreTax2                                          
--,#tempSaleMargin.AfterTax = m.AfterTax2            
,#tempSaleMargin.AfterTax = (CASE WHEN m.SupplierType = 'Agent' THEN m.AfterTax2 ELSE (#tempSaleMargin.NetAmount) - (m.SupplierNet2) END)                                                                      
FROM @tempSupplierMarginNew m                                                                
WHERE #tempSaleMargin.strCode COLLATE DATABASE_DEFAULT= m.strCode2 COLLATE DATABASE_DEFAULT     
AND #tempSaleMargin.strCategory COLLATE DATABASE_DEFAULT = m.strCategory2 COLLATE DATABASE_DEFAULT    
AND #tempSaleMargin.intForNumPeople = m.intForNumPeople2                                               
                             
--GET FINAL RESULT                                                                
SELECT strCode, strCategory, CategoryName, --CategoryName Added by Ravi Bade for the tweak report titles        
 ISNULL(CONVERT(NVARCHAR(600),descriptions),'') descriptions, intForNumPeople, SUM(IntNumberOfSales) AS IntNumberOfSales                                                                
,CONVERT(DECIMAL(18, 2), SUM(GrossAmount)) GrossAmount, CONVERT(DECIMAL(18, 2), SUM(NetAmount)) NetAmount                                                                
,CONVERT(DECIMAL(18, 2), SUM(SupplierGross)) SupplierGross, CONVERT(DECIMAL(18, 2), SUM(SupplierNet)) SupplierNet                                
,CONVERT(DECIMAL(18, 2),AVG(PreTax) * SUM(IntNumberOfSales)) as MarginPreTax                                            
,CONVERT(DECIMAL(18, 2), AVG(AfterTax) * SUM(IntNumberOfSales)) MarginAfterTax              
--,CONVERT(DECIMAL(18, 2), SUM(NetAmount)) - (CONVERT(DECIMAL(18, 2), SUM(SupplierNet))) MarginAfterTax                                                        
,CONVERT(DECIMAL(18, 1),CASE WHEN SUM(NetAmount) <> 0 THEN ((AVG(AfterTax) * SUM(IntNumberOfSales) / SUM(NetAmount)) * 100) ELSE 0 END) AS MarginPercentage                                           
FROM #tempSaleMargin                                                 
Group by strCode, strCategory,CategoryName, --CategoryName Added by Ravi Bade for the tweak report titles         
 ISNULL(CONVERT(NVARCHAR(600),descriptions),'')                                                
, intForNumPeople                                          
ORDER BY  strCategory, strCode                                       
                                                           
DROP TABLE #tempSaleMargin                                                                
                                                                
END 