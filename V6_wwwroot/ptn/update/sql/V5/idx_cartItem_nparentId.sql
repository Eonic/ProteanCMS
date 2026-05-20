/****** Object:  Index [idx_nParentId]    Script Date: 18/05/2026 11:10:28 ******/
CREATE NONCLUSTERED INDEX [idx_nParentId] ON [dbo].[tblCartItem]
(
	[nParentId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO


