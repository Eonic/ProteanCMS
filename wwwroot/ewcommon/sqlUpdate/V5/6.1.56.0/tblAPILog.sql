

CREATE TABLE [dbo].[tblAPILog](
	[nAPILogKey] [int] IDENTITY(1,1) NOT NULL,
	[nUserId] [int] NULL,
	[dRequestDateTime] [datetime] NULL,
	[dResponseDateTime] [datetime] NULL,
	[cRequestedUrl] [varchar](300) NULL,
	[cMethodName] [varchar](100) NULL,
	[cPayLoad] [varchar](max) NULL,
	[cResponseData] [varchar](max) NULL,
	[cResponseType] [nvarchar](500) NULL,
	[cRequestType] [varchar](10) NULL,
	[cSourceIP] [varchar](15) NULL,
	[cUserAgent] [varchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]



