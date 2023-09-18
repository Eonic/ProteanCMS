if exists (select * from sysobjects where id = object_id(N'[#SQLSTATS]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
DROP TABLE #SQLSTATS

if exists (select * from sysobjects where id = object_id(N'[DBO].[EW_FKS]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
DROP TABLE [DBO].[EW_FKS]

Create Table [DBO].[EW_FKS](
foreign_key_name   varchar(255) , 
keycnt varchar(255),
foreign_table  varchar(255) ,
foreign_column_1 varchar(255),
foreign_column_2 varchar(255),
primary_table varchar(255),
primary_column_1 varchar(255),
primary_column_2 varchar(255)
)

Insert Into EW_FKS (foreign_key_name ,keycnt ,foreign_table ,foreign_column_1 ,foreign_column_2 ,primary_table ,primary_column_1,primary_column_2 )
select cast(f.name  as varchar(255)) as foreign_key_name
    , r.keycnt
    , cast(c.name as  varchar(255)) as foreign_table
    , cast(fc.name as varchar(255)) as  foreign_column_1
    , cast(fc2.name as varchar(255)) as foreign_column_2
    ,  cast(p.name as varchar(255)) as primary_table
    , cast(rc.name as varchar(255))  as primary_column_1
    , cast(rc2.name as varchar(255)) as  primary_column_2
    from sysobjects f
    inner join sysobjects c on  f.parent_obj = c.id
    inner join sysreferences r on f.id =  r.constid
    inner join sysobjects p on r.rkeyid = p.id
    inner  join syscolumns rc on r.rkeyid = rc.id and r.rkey1 = rc.colid
    inner  join syscolumns fc on r.fkeyid = fc.id and r.fkey1 = fc.colid
    left join  syscolumns rc2 on r.rkeyid = rc2.id and r.rkey2 = rc.colid
    left join  syscolumns fc2 on r.fkeyid = fc2.id and r.fkey2 = fc.colid
    where f.type =  'F'

Create Table #SQLSTATS(
cStatement   varchar(255) 
)

Insert Into #SQLSTATS (cStatement)
select 'ALTER TABLE ' + foreign_table + ' DROP CONSTRAINT ' + foreign_key_name 
        from EW_FKS order by foreign_table

DECLARE @parent nchar(255)
DECLARE parents CURSOR FOR  
Select cStatement from #SQLSTATS

OPEN parents

FETCH NEXT FROM parents INTO @parent 
WHILE @@FETCH_STATUS = 0

BEGIN

PRINT @Parent

EXEC sp_executesql @Parent

PRINT @Parent

FETCH NEXT FROM parents INTO @parent 

END

DROP TABLE #SQLSTATS