CREATE NONCLUSTERED INDEX idx_Permissions_nStructId_nAccessLevel
ON [dbo].[tblDirectoryPermission] ([nStructId],[nAccessLevel])