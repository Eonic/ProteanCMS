


/****** Object:  Index [idx_tblCartCatProductRelations_nContentId]    Script Date: 11/15/2011 19:56:13 ******/
CREATE NONCLUSTERED INDEX [idx_tblCartCatProductRelations_nContentId] ON [dbo].[tblCartCatProductRelations] 
(
	[nContentId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]



/****** Object:  Index [idx_tblCartCatProductRelations_nCatId]    Script Date: 11/15/2011 19:56:33 ******/
CREATE NONCLUSTERED INDEX [idx_tblCartCatProductRelations_nCatId] ON [dbo].[tblCartCatProductRelations] 
(
	[nCatId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]



/****** Object:  Index [idx_tblCartDiscountDirRelations_nDiscountId]    Script Date: 11/15/2011 19:57:03 ******/
CREATE NONCLUSTERED INDEX [idx_tblCartDiscountDirRelations_nDiscountId] ON [dbo].[tblCartDiscountDirRelations] 
(
	[nDiscountId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]


/****** Object:  Index [idx_tblCartDiscountDirRelations_nDirId]    Script Date: 11/15/2011 19:57:19 ******/
CREATE NONCLUSTERED INDEX [idx_tblCartDiscountDirRelations_nDirId] ON [dbo].[tblCartDiscountDirRelations] 
(
	[nDirId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]


/****** Object:  Index [idx_tblCartDiscountProdCatRelations_nDiscountId]    Script Date: 11/15/2011 19:57:39 ******/
CREATE NONCLUSTERED INDEX [idx_tblCartDiscountProdCatRelations_nDiscountId] ON [dbo].[tblCartDiscountProdCatRelations] 
(
	[nDiscountId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]


/****** Object:  Index [idx_tblCartDiscountProdCatRelations_nProductCatId]    Script Date: 11/15/2011 19:57:53 ******/
CREATE NONCLUSTERED INDEX [idx_tblCartDiscountProdCatRelations_nProductCatId] ON [dbo].[tblCartDiscountProdCatRelations] 
(
	[nProductCatId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]


/****** Object:  Index [idx_tblCartDiscountProdCatRelations_nAuditId]    Script Date: 11/15/2011 19:58:07 ******/
CREATE UNIQUE NONCLUSTERED INDEX [idx_tblCartDiscountProdCatRelations_nAuditId] ON [dbo].[tblCartDiscountProdCatRelations] 
(
	[nAuditId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]



/****** Object:  Index [idx_tblCartDiscountRules_nAuditId]    Script Date: 11/15/2011 20:04:52 ******/
CREATE UNIQUE NONCLUSTERED INDEX [idx_tblCartDiscountRules_nAuditId] ON [dbo].[tblCartDiscountRules] 
(
	[nAuditId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [idx_tblCartCatProductRelations_general] ON [dbo].[tblCartCatProductRelations] 
(
	[nCatId] ASC,
	[nCatProductRelKey] ASC,
	[nContentId] ASC
)WITH (SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [idx_tblAudit_general] ON [dbo].[tblAudit] 
(
	[nAuditKey] ASC,
	[nStatus] ASC,
	[dExpireDate] ASC,
	[dPublishDate] ASC
)WITH (SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]





