/****** Object:  FullTextIndex     Script Date: 30-07-2024 14:06:12 ******/
CREATE FULLTEXT INDEX ON [dbo].[tblContent](
[cContentXmlBrief] LANGUAGE 'English', 
[cContentXmlDetail] LANGUAGE 'English')
KEY INDEX [PK_tbl_Content]ON ([ContentIdx], FILEGROUP [PRIMARY])
WITH (CHANGE_TRACKING = AUTO, STOPLIST = SYSTEM)

