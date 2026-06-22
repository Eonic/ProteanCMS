
CREATE function [dbo].[getPWHash] (@password nvarchar(100), @salt uniqueidentifier) 
returns BINARY (64)
as 
begin
declare @hashed BINARY (64)
set @hashed = HashBytes('SHA2_512', cast(@salt as varchar(36)) + lower(trim(@password)))
return @hashed
end
GO