/****** Object:  Index [idx_nParentId]    Script Date: 19/06/2026 08:06:32 ******/
CREATE NONCLUSTERED INDEX [idx_nParentId] ON [dbo].[tblCartItem]
(
	[nParentId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO


