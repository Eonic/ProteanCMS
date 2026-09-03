
CREATE TABLE [dbo].[tblPerfMon](
	[EntryID] [int] IDENTITY(1,1) NOT NULL,
	[MachineName] [nvarchar](255) NULL,
	[Website] [nvarchar](255) NULL,
	[SessionID] [nvarchar](255) NULL,
	[SessionRequest] [nvarchar](255) NULL,
	[Path] [nvarchar](255) NULL,
	[Module] [nvarchar](255) NULL,
	[Procedure] [nvarchar](255) NULL,
	[Description] [nvarchar](max) NULL,
	[Step] [int] NULL,
	[Time] [decimal](18, 3) NULL,
	[TimeAccumalative] [decimal](18, 3) NULL,
	[Requests] [int] NULL,
	[PrivateMemorySize64] [nvarchar](255) NULL,
	[PrivilegedProcessorTimeMilliseconds] [nvarchar](255) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


