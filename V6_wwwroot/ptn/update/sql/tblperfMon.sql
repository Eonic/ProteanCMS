USE [ew_PerfMon]
GO

/****** Object:  Table [dbo].[tblPerfMon]    Script Date: 12/29/2010 12:06:16 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tblPerfMon](
	[EntryID] [int] IDENTITY(1,1) NOT NULL,
	[MachineName] [nvarchar](255) NULL,
	[Website] [nvarchar](255) NULL,
	[SessionID] [nvarchar](255) NULL,
	[SessionRequest] [nvarchar](255) NULL,
	[Path] [nvarchar](255) NULL,
	[Module] [nvarchar](255) NULL,
	[Procedure] [nvarchar](255) NULL,
	[Description] [nvarchar](800) NULL,
	[Step] [int] NULL,
	[Time] [decimal](18, 3) NULL,
	[TimeAccumalative] [decimal](18, 3) NULL,
	[Requests] [int] NULL,
	[PrivateMemorySize64] [nvarchar](255) NULL,
	[PrivilegedProcessorTimeMilliseconds] [nvarchar](255) NULL
) ON [PRIMARY]

GO

