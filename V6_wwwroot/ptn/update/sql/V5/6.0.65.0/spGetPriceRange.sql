CREATE PROCEDURE spGetPriceRange         
          
  @MinPrice int,        
  @MaxPrice int,        
  @Step int  ,         
  @PageId varchar(100),         
  @whereSql varchar(max),         
  @FilterTarget nvarchar(10)         
  AS         
  BEGIN        
 DECLARE @sqlStr AS NVARCHAR(max) = ''        
 IF (@whereSql <> '')        
 BEGIN        
  SET @whereSql = ' AND ' + @whereSql        
 END        
        
 SET @sqlStr = '      
  
 DECLARE @MaxCount as int  
  
 DROP TABLE IF EXISTS  #PriceRangetemp;        
 create table #PriceRangetemp (id INT , minPrice int,maxPrice int,ContentCount int,MaxProductPrice int,MinProductPrice int)         
  declare  @PriceRange as table (id INT NOT NULL IDENTITY(1, 1), minPrice int,maxPrice int,ContentCount int,MaxProductPrice int,MinProductPrice int)         
  Declare @startPrice as int         
   Declare @stepPrice as int        
 Declare @ContentCount as int         
 Declare @MaxProductPrice as int        
 Declare @MinProductPrice as int         
 SET @startPrice=' + convert(VARCHAR(10), @MinPrice) + '         
          
 WHILE @startPrice<=' + convert(VARCHAR(10), @MaxPrice) + '        
 BEGIN        
 if(@startPrice='+convert(VARCHAR(10), @MinPrice)+' AND @startPrice<>0)   
 BEGIN  
  
 set @stepPrice= ' + convert(VARCHAR(10), @Step) +'  
 END  
 ELSE  
 BEGIN  
  set @startPrice=@startPrice+1  
 SET @stepPrice=@startPrice-1 + ' + convert(VARCHAR(10), @Step) +      
   
  '  END    
  Insert into @PriceRange(minPrice,maxPrice,ContentCount,MaxProductPrice,MinProductPrice) values (@startPrice,@stepPrice,@ContentCount,@MaxProductPrice,@MinProductPrice)         
  SET @startPrice=@stepPrice        
  End        
  insert into #PriceRangetemp        
select pr.id,pr.minPrice,pr.maxPrice, count(ci.nContentId)*0.1 as ContentCount,max(ci.nNumberValue) MaxProductPrice,min(ci.nNumberValue) as MaxProductPrice        
FROM @PriceRange pr        
 inner join  tblContentIndex ci on ci.nNumberValue >= pr.minPrice and ci.nNumberValue <=pr.maxPrice        
 inner join tblContentIndexDef cid on cid.nContentIndexDefKey=ci.nContentIndexDefinitionKey          
  inner join tblAudit ca on ca.nAuditKey=cid.nAuditId and nStatus=1 and cid.cDefinitionName=''Price''           
  inner join tblContent c on ci.nContentId=c.nContentKey and c.cContentSchemaName=''' + @FilterTarget +         
  '''                  
inner join tblAudit ac on ac.nAuditKey=c.nAuditId and ac.nStatus=1           
LEFT OUTER JOIN tblContentLocation AS CL ON c.nContentKey = CL.nContentId         
where  CL.nStructId IN (select nStructKey from tblContentStructure where  (nStructParId in (' + convert(VARCHAR(10), @PageId) + ') or nStructKey in (' + convert(VARCHAR(10), @PageId) + '))        
)'        
        
 IF @whereSql <> ''        
  SET @sqlStr = CONCAT (        
    @sqlStr        
    ,@whereSql        
    )        
 SET @sqlStr = CONCAT (        
   @sqlStr        
   ,'group by pr.id,pr.minPrice,pr.maxPrice       
     
    Select @MaxCount=sum(ContentCount) from #PriceRangetemp where MaxProductPrice>' + convert(VARCHAR(10), @MaxPrice) +'  
 Update #PriceRangetemp set ContentCount =ContentCount+@MaxCount, MaxPrice= MinPrice,MaxProductPrice=MinPrice where MinPrice ='+ convert(VARCHAR(10), @MaxPrice) +'+1;  
 Delete from #PriceRangetemp where MinPrice >' + convert(VARCHAR(10), @MaxPrice) +' + '+ convert(VARCHAR(10), @Step)+'  
 select * from #PriceRangetemp order by 1 asc ;'  
   )        
        
 --PRINT @sqlStr        
        
 EXEC (@sqlStr)    
  
  
END 