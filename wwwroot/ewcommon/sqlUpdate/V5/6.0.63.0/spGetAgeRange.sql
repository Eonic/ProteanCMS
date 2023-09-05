--  spGetAgeRange 0,100,1,'Product','' ,64   
  
CREATE PROCEDURE [dbo].[spGetAgeRange]   
@MinAge int,  
@MaxAge int,   
@Step int,   
@FilterTarget nvarchar(10)  ,  
@whereSql varchar(max)  ,  
@PageId int   
AS            
BEGIN         
DECLARE @cnt as int       
BEGIN          
if (@whereSql<>'')     
BEGIN             
SET @whereSql=' AND ' + @whereSql     
END                        
--Declare @stepAge as int     
Declare @ProductCount as int   
Declare @MaxProductAge as int    
  
   
 Declare @sqlStr as nvarchar(max)  =''        
    
   
set @sqlStr='  
  
 DROP TABLE IF EXISTS  #AgeTemp;  
create table #AgeTemp  (id INT , minAge int,maxAge int,ContentCount int, MaxTotalAge int,MinTotalAge int)   
Declare @startAge as int   
Declare @stepAge as int     
SET @startAge='+convert(varchar(10),@MinAge)+'          
  
  
DECLARE @AgeRange AS TABLE (id INT NOT NULL IDENTITY(1, 1), minAge int,maxAge int,ContentCount int)        
  
 WHILE @startAge<'+convert(varchar(10),@MaxAge)+'                    
 BEGIN      
 SET @stepAge=@startAge+'+convert(varchar(10),@Step)+'     
 Insert into @AgeRange(minAge,maxAge,ContentCount) values (@startAge,@stepAge,0)                   
   SET @startAge=@stepAge   
 End  
 insert into #AgeTemp   
 select ar.id,ar.minAge,ar.maxAge, count(ci.nContentId) as ContentCount, max(ci.nNumberValue),min(ci.nNumberValue)  
FROM @AgeRange ar  
inner join tblContentIndex ci   on ci.nNumberValue >= ar.minAge and ci.nNumberValue <=ar.maxAge  
inner join tblContentIndexDef cid on cid.nContentIndexDefKey=ci.nContentIndexDefinitionKey and cid.cDefinitionName in (''Max Age'',''Min Age'')   
 inner join tblContent c on ci.nContentId=c.nContentKey and c.cContentSchemaName=''Product''    
   LEFT OUTER JOIN tblContentLocation AS CL ON c.nContentKey = CL.nContentId       
 and  CL.nStructId IN (select nStructKey from tblContentStructure where nStructParId in ('+convert(varchar(10),@PageId)+') )   
  
inner join tblAudit ac on ac.nAuditKey=cid.nAuditId and ac.nStatus=1    
inner join tblAudit cc on cc.nAuditKey=cid.nAuditId and cc.nStatus=1   
WHERE  ci.nNumberValue!=0 '  
 set @sqlStr= CONCAT ( @sqlStr  ,@whereSql )    
 set @sqlStr= CONCAT ( @sqlStr  ,' group by ar.id,ar.minAge,ar.maxAge select * from #AgeTemp   
select Min(MinTotalAge),max(MaxTotalAge) from #AgeTemp' )   
 print(@sqlStr)  
 exec(@sqlStr)  
   
 end  
  end