--spGetPriceRange 1,5000,500,64 ,''     
      
CREATE PROCEDURE spGetPriceRange           
@MinPrice int,          
@MaxPrice int,          
@Step int  ,      
@PageId int   ,  
@whereSql varchar(max)     
AS          
BEGIN          
DECLARE @cnt as int          
Declare @startPrice as int          
  
CREATE TABLE #PriceRange(id INT NOT NULL IDENTITY(1, 1), minPrice int,maxPrice int,ProductCount int,MaxProductPrice int)          
Declare @stepPrice as int          
Declare @ProductCount as int         
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
      
   set @sqlStr='Declare @ProductCount as int; Declare @MaxProductPrice as int;  Select @ProductCount=count(ci.nContentId),@MaxProductPrice=max(ci.nNumberValue) from tblContentIndex ci          
inner join tblContentIndexDef cid on cid.nContentIndexDefKey=ci.nContentIndexDefinitionKey           
inner join tblAudit ca on ca.nAuditKey=cid.nAuditId and nStatus=1 and cid.cDefinitionName=''Price''           
inner join tblContent c on ci.nContentId=c.nContentKey and c.cContentSchemaName=''Product''          
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
       
          
SET  @sqlStr=@sqlStr +' Insert into #PriceRange(minPrice,maxPrice,ProductCount,MaxProductPrice) values ('+convert(varchar(10),@startPrice)+','+convert(varchar(10),@stepPrice)+',@ProductCount,@MaxProductPrice) '         
      exec(@sqlStr)       
SET @startPrice=@stepPrice          
            
END          
          
select minPrice,maxPrice,ProductCount, MaxProductPrice ,convert(varchar(10),minPrice)+'-'+convert(varchar(10),maxPrice) as [value],          
case when id=1 then '< �'+convert(varchar(10),maxPrice)+ ' ['+ Convert(varchar(10),ProductCount)+']'           
else '�'+convert(varchar(10),minPrice) + ' - �'+convert(varchar(10),maxPrice)+ ' ['+ Convert(varchar(10),ProductCount)+']'           
          
end as [name] from #PriceRange          
where ProductCount>0      
          
       
      
END 