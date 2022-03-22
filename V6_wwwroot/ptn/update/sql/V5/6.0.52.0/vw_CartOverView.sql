CREATE VIEW [dbo].[vw_CartOverView]
AS
SELECT        dbo.tblCartOrder.nCartOrderKey, dbo.tblCartOrder.cCartSchemaName, dbo.tblCartOrder.nCartStatus, dbo.tblCartOrder.cCurrency, Items.nCartItemKey, Items.nQuantity,
                             (SELECT        CASE WHEN SUM(Options.nPrice) IS NULL THEN 0 WHEN SUM(Options.nQuantity) IS NULL THEN 0 ELSE SUM(Options.nPrice * Options.nQuantity) END AS nOptionsPrice
                               FROM            dbo.tblCartItem AS Options
                               WHERE        (nParentId = Items.nCartItemKey)) + Items.nPrice AS nLinePrice, dbo.tblContent.nContentKey, dbo.tblContent.cContentSchemaName, dbo.tblContent.cContentName, dbo.tblAudit.dInsertDate AS dCartDate, 
                         dbo.tblCartOrder.nTaxRate
FROM            dbo.tblCartItem AS Items INNER JOIN
                         dbo.tblCartOrder ON Items.nCartOrderId = dbo.tblCartOrder.nCartOrderKey INNER JOIN
                         dbo.tblContent ON Items.nItemId = dbo.tblContent.nContentKey INNER JOIN
                         dbo.tblAudit ON dbo.tblCartOrder.nAuditId = dbo.tblAudit.nAuditKey
WHERE        (Items.nParentId IS NULL) OR
                         (Items.nParentId = 0)
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "Items"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 218
            End
            DisplayFlags = 280
            TopColumn = 14
         End
         Begin Table = "tblCartOrder"
            Begin Extent = 
               Top = 6
               Left = 256
               Bottom = 136
               Right = 451
            End
            DisplayFlags = 280
            TopColumn = 13
         End
         Begin Table = "tblContent"
            Begin Extent = 
               Top = 138
               Left = 38
               Bottom = 308
               Right = 250
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "tblAudit"
            Begin Extent = 
               Top = 138
               Left = 288
               Bottom = 268
               Right = 458
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vw_CartOverView'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vw_CartOverView'
GO

