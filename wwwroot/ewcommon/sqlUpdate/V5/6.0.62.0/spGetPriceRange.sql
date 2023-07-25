IF OBJECT_ID('spGetPriceRange', 'P') IS NOT NULL
DROP PROC spGetPriceRange
GO

-- spGetPriceRange 1,5000,500,64 ,' nContentId in (select nContentId from tblCartCatProductRelations c inner join tblAudit a on a.nAuditKey=c.nAuditId and nStatus=1 where c.nCatId in (9)) ','Product'   
          
            
CREATE PROCEDURE [dbo].[spGetPriceRange]                 
@MinPrice int,                
@MaxPrice int,                
@Step int  ,            
@PageId int   ,        
@whereSql varchar(max),        
@FilterTarget nvarchar(10)        
AS                
BEGIN                
DECLARE @cnt as int                
Declare @startPrice as int                
        
  if (@whereSql<>'')         
 BEGIN        
 SET @whereSql=' AND ' + @whereSql        
 END        
CREATE TABLE #PriceRange(id INT NOT NULL IDENTITY(1, 1), minPrice int,maxPrice int,ContentCount int,MaxProductPrice int)                
Declare @stepPrice as int                
Declare @ContentCount as int               
Declare @MaxProductPrice as int               
                
SET @startPrice=@MinPrice                
    Declare @sqlStr as nvarchar(max)  =''           
WHILE @startPrice<@MaxPrice                
BEGIN                
SET @stepPrice=@startPrice+@Step                
            
            
-- Select @ProductCount=count(ci.nContentId),@MaxProductPrice=max(ci.nNumberValue) from tblContentIndex ci                
--inner join tblContentIndexDef cid on cid.nContentIndexDefKey=ci.nContentIndexDefinitionKey                 
--inner join tblAudit ca on ca.nAuditKey=cid.nAuditId and nStatus=1 and cid.cDefinitionName='Price'                 
--inner join tblContent c on ci.nContentId=c.nContentKey and c.cContentSchemaName='Product'                
--inner join tblAudit ac on ac.nAuditKey=c.nAuditId and ac.nStatus=1                
--LEFT OUTER JOIN tblContentLocation AS CL ON c.nContentKey = CL.nContentId             
--where  CL.nStructId IN (select nStructKey from tblContentStructure where nStructParId in (@PageId)) or cl.nStructId=@PageId            
--and ci.nNumberValue between @startPrice and @stepPrice                
            
   set @sqlStr='Declare @ContentCount as int; Declare @MaxProductPrice as int;  Select @ContentCount=count(ci.nContentId),@MaxProductPrice=max(ci.nNumberValue) from tblContentIndex ci                
inner join tblContentIndexDef cid on cid.nContentIndexDefKey=ci.nContentIndexDefinitionKey                 
inner join tblAudit ca on ca.nAuditKey=cid.nAuditId and nStatus=1 and cid.cDefinitionName=''Price''                 
inner join tblContent c on ci.nContentId=c.nContentKey and c.cContentSchemaName='''+@FilterTarget+'''                
inner join tblAudit ac on ac.nAuditKey=c.nAuditId and ac.nStatus=1                
LEFT OUTER JOIN tblContentLocation AS CL ON c.nContentKey = CL.nContentId             
where  CL.nStructId IN (select nStructKey from tblContentStructure where nStructParId in ('+convert(varchar(10),@PageId)+'))           
and ci.nNumberValue between '+convert(varchar(10),@startPrice)+' and '+convert(varchar(10),@stepPrice)+' '          
       
    if @whereSql<>''              
              
set @sqlStr= CONCAT (                          
   @sqlStr                          
   ,@whereSql                          
   )                
SET  @sqlStr=@sqlStr+';'            
       print (@sqlStr)      
                
SET  @sqlStr=@sqlStr +' Insert into #PriceRange(minPrice,maxPrice,ContentCount,MaxProductPrice) values ('+convert(varchar(10),@startPrice)+','+convert(varchar(10),@stepPrice)+',@ContentCount,@MaxProductPrice) '               
      exec(@sqlStr)             
SET @startPrice=@stepPrice                
                  
              
  END                
              
select minPrice,maxPrice,ContentCount, MaxProductPrice ,convert(varchar(10),minPrice)+'-'+convert(varchar(10),maxPrice) as [value]                
from #PriceRange                
where ContentCount>0            
                
             
            
END 