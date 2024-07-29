CREATE PROCEDURE dbo.spGetPriceRange 
@MinPrice int,
@MaxPrice int,
@Step int
AS
BEGIN
DECLARE @cnt as int
Declare @startPrice as int
Declare @PriceRange AS Table(id INT NOT NULL IDENTITY(1, 1), minPrice int,maxPrice int,ProductCount int)
Declare @stepPrice as int
Declare @ProductCount as int

SET @startPrice=@MinPrice

	
WHILE @startPrice<@MaxPrice
BEGIN
SET @stepPrice=@startPrice+@Step

Select @ProductCount=count(ci.nContentId) from tblContentIndex ci 
inner join tblContentIndexDef cid on cid.nContentIndexDefKey=ci.nContentIndexDefinitionKey 
inner join tblAudit ca on ca.nAuditKey=cid.nAuditId and nStatus=1 and cid.cDefinitionName='Price' 
inner join tblContent c on ci.nContentId=c.nContentKey and c.cContentSchemaName='Product'
inner join tblAudit ac on ac.nAuditKey=c.nAuditId and ac.nStatus=1 
where ci.nNumberValue between @startPrice and @stepPrice


Insert into @PriceRange(minPrice,maxPrice,ProductCount) values (@startPrice,@stepPrice,@ProductCount)

SET @startPrice=@stepPrice
		
END

select minPrice,maxPrice,ProductCount,convert(varchar(10),minPrice)+'-'+convert(varchar(10),maxPrice) as [value],
case when id=1 then '< £'+convert(varchar(10),maxPrice)+ ' ['+ Convert(varchar(10),ProductCount)+']' 
else '£'+convert(varchar(10),minPrice) + ' - £'+convert(varchar(10),maxPrice)+ ' ['+ Convert(varchar(10),ProductCount)+']' 

end as [name] from @PriceRange

END

