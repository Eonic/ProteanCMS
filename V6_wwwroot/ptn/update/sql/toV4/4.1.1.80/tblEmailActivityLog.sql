if not Exists(select * from sys.tables where Name = N'tblEmailActivityLog')
BEGIN
	CREATE TABLE [dbo].[tblEmailActivityLog](
		[nEmailActivityKey] [int] IDENTITY(1,1) NOT NULL,
		[nUserDirId] [int] NULL,
		[dDateTime] [datetime] NULL,
		[cEmailRecipient] [nvarchar](255) NULL,
		[cEmailSender] [nvarchar](255) NULL,
		[cActivityDetail] [ntext] NULL,
	 CONSTRAINT [PK_tblEmailActivityLog] PRIMARY KEY CLUSTERED 
	(
		[nEmailActivityKey] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END

if not Exists(select * from sys.columns where Name = N'cActivityXml' and Object_ID = Object_ID(N'tblEmailActivityLog')) 
BEGIN
	ALTER TABLE dbo.tblEmailActivityLog ADD	cActivityXml ntext NULL
END


