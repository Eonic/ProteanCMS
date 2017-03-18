if not Exists(select * from sys.tables where Name = N'tblCodes')
BEGIN
	CREATE TABLE [dbo].[tblCodes](
		[nCodeKey] [int] IDENTITY(1,1) NOT NULL,
		[cCodeName] [nvarchar](50) NULL,
		[nCodeType] [int] NULL,
		[nCodeParentId] [int] NULL,
		[cCodeGroups] [nvarchar](50) NULL,
		[cCode] [nvarchar](255) NOT NULL,
		[nUseId] [int] NULL,
		[dUseDate] [datetime] NULL,
		[nAuditId] [int] NULL,
	 CONSTRAINT [PK_tblCodes] PRIMARY KEY CLUSTERED 
	(
		[nCodeKey] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

END