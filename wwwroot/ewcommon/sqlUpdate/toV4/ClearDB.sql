if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[fxn_SearchXML]') and xtype in (N'FN', N'IF', N'TF'))
drop function [dbo].[fxn_SearchXML]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[fxn_addAudit]') and xtype in (N'FN', N'IF', N'TF'))
drop function [dbo].[fxn_addAudit]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[fxn_checkPermission]') and xtype in (N'FN', N'IF', N'TF'))
drop function [dbo].[fxn_checkPermission]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[fxn_getStatus]') and xtype in (N'FN', N'IF', N'TF'))
drop function [dbo].[fxn_getStatus]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[fxn_getUserCompanies]') and xtype in (N'FN', N'IF', N'TF'))
drop function [dbo].[fxn_getUserCompanies]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[fxn_getUserDepts]') and xtype in (N'FN', N'IF', N'TF'))
drop function [dbo].[fxn_getUserDepts]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[fxn_getUserRoles]') and xtype in (N'FN', N'IF', N'TF'))
drop function [dbo].[fxn_getUserRoles]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[fxn_shippingTotal]') and xtype in (N'FN', N'IF', N'TF'))
drop function [dbo].[fxn_shippingTotal]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[fxn_getContentParents]') and xtype in (N'FN', N'IF', N'TF'))
drop function [dbo].[fxn_getContentParents]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[fxn_getMembers]') and xtype in (N'FN', N'IF', N'TF'))
drop function [dbo].[fxn_getMembers]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[fxn_getMembers_Sub]') and xtype in (N'FN', N'IF', N'TF'))
drop function [dbo].[fxn_getMembers_Sub]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[getContentStructure]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[getContentStructure]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[getUsersCompanyAllParents]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[getUsersCompanyAllParents]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spGetAllUsers]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spGetAllUsers]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spGetAllUsersActive]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spGetAllUsersActive]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spGetAllUsersInActive]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spGetAllUsersInActive]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spGetCompanyUsers]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spGetCompanyUsers]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spGetCompanyUsersActive]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spGetCompanyUsersActive]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spGetCompanyUsersInActive]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spGetCompanyUsersInActive]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spGetDirectoryItems]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spGetDirectoryItems]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spGetUsers]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spGetUsers]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spSearchUsers]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spSearchUsers]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spGetUserSessionActivity]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spGetUserSessionActivity]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spGetUserSessionActivityDetail]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spGetUserSessionActivityDetail]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spGetCodes]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spGetCodes]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spGetCodeDirectoryGroups]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spGetCodeDirectoryGroups]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spGetContentStructure_v2]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spGetContentStructure_v2]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spGetContentStructure_Admin]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spGetContentStructure_Admin]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spOrderDownload]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spOrderDownload]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblActivityLog]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblActivityLog]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblAudit]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblAudit]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblXmlCache]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblXmlCache]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[EW_FKS]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[EW_FKS]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblCartCatProductRelations]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblCartCatProductRelations]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblCartContact]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblCartContact]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblCartDiscountDirRelations]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblCartDiscountDirRelations]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblCartDiscountProdCatRelations]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblCartDiscountProdCatRelations]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblCartDiscountRules]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblCartDiscountRules]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblCartItem]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblCartItem]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblCartOrder]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblCartOrder]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblCartPaymentMethod]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblCartPaymentMethod]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblCartProductCategories]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblCartProductCategories]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblCartShippingLocations]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblCartShippingLocations]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblCartShippingMethods]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblCartShippingMethods]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblCartShippingRelations]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblCartShippingRelations]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblContent]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblContent]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblContentLocation]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblContentLocation]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblContentRelation]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblContentRelation]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblContentStructure]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblContentStructure]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblDirectory]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblDirectory]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblDirectoryPermission]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblDirectoryPermission]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblDirectoryRelation]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblDirectoryRelation]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblCartDiscountDirRelations]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblCartDiscountDirRelations]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblCartDiscountProdCatRelations]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblCartDiscountProdCatRelations]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblLookup]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblLookup]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblOptOutAddresses]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblOptOutAddresses]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblDirectorySubscriptions]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblDirectorySubscriptions]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblCodes]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblCodes]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblAlerts]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblAlerts]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblEmailActivityLog]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblEmailActivityLog]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblContentVersions]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblContentVersions]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblCartShippingPermission]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblCartShippingPermission]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblSchemaVersion]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblSchemaVersion]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblSubscription]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblSubscription]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblSubscriptionQuota]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblSubscriptionQuota]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblSubscriptionRenewal]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblSubscriptionRenewal]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[fxn_CSVTableINTEGERS]') and xtype in (N'FN', N'IF', N'TF'))
drop function [dbo].[fxn_CSVTableINTEGERS]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[fxn_CSVTableSTRINGS]') and xtype in (N'FN', N'IF', N'TF'))
drop function [dbo].[fxn_CSVTableSTRINGS]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[fxn_GetActivityDetail]') and xtype in (N'FN', N'IF', N'TF'))
drop function [dbo].[fxn_GetActivityDetail]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[fxn_SessionUserTable]') and xtype in (N'FN', N'IF', N'TF'))
drop function [dbo].[fxn_SessionUserTable]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[fxn_SessionsInDateRange]') and xtype in (N'FN', N'IF', N'TF'))
drop function [dbo].[fxn_SessionsInDateRange]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[fxn_SessionsLength]') and xtype in (N'FN', N'IF', N'TF'))
drop function [dbo].[fxn_SessionsLength]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[fxn_SessionsStart]') and xtype in (N'FN', N'IF', N'TF'))
drop function [dbo].[fxn_SessionsStart]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[fxn_SessionsUser]') and xtype in (N'FN', N'IF', N'TF'))
drop function [dbo].[fxn_SessionsUser]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[fxn_getPageGroups]') and xtype in (N'FN', N'IF', N'TF'))
drop function [dbo].[fxn_getPageGroups]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[fxn_checkDiscountCode]') and xtype in (N'FN', N'IF', N'TF'))
drop function [dbo].[fxn_checkDiscountCode]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[fxn_getContentLocations]') and xtype in (N'FN', N'IF', N'TF'))
drop function [dbo].[fxn_getContentLocations]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spGetSessionActivity]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spGetSessionActivity]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spSearchUsers]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spSearchUsers]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[getContentStructure_Admin]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[getContentStructure_Admin]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[getAllPageVersions_sub]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[getAllPageVersions_sub]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spGetUsersByGroup]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spGetUsersByGroup]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[getContentStructure_Basic]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[getContentStructure_Basic]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[getContentStructure_v2]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[getContentStructure_v2]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[getContentStructure_Enumerate]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[getContentStructure_Enumerate]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[vw_SessionNonNullSummary]') and OBJECTPROPERTY(id, N'IsView') = 1)
drop view [dbo].[vw_SessionNonNullSummary]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[vw_SessionPageCount]') and OBJECTPROPERTY(id, N'IsView') = 1)
drop view [dbo].[vw_SessionPageCount]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[vw_CartOverView]') and OBJECTPROPERTY(id, N'IsView') = 1)
drop view [dbo].[vw_CartOverView]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[vw_CartOverViewGroups]') and OBJECTPROPERTY(id, N'IsView') = 1)
drop view [dbo].[vw_CartOverViewGroups]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[vw_CartOverViewPages]') and OBJECTPROPERTY(id, N'IsView') = 1)
drop view [dbo].[vw_CartOverViewPages]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[vw_VersionControl_GetPendingContent]') and OBJECTPROPERTY(id, N'IsView') = 1)
drop view [dbo].[vw_VersionControl_GetPendingContent]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[vw_CurrentLiveContentIds]') and OBJECTPROPERTY(id, N'IsView') = 1)
drop view [dbo].[vw_CurrentLiveContentIds]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[vw_CurrentLiveContentIdsAndSchemas]') and OBJECTPROPERTY(id, N'IsView') = 1)
drop view [dbo].[vw_CurrentLiveContentIdsAndSchemas]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spCartActivityGroupsPages]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spCartActivityGroupsPages]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spCartActivityLowLevel]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spCartActivityLowLevel]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spCartActivityMedLevel]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spCartActivityMedLevel]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spCartActivityPagesPeriod]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spCartActivityPagesPeriod]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spCartActivityTopLevel]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spCartActivityTopLevel]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[sp_AllDirUsers]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[sp_AllDirUsers]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[getAllPageVersions]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[getAllPageVersions]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[getUserPageVersions]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[getUserPageVersions]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spGenericReportLastDownloadedDropDown]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spGenericReportLastDownloadedDropDown]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spMailDownloadOptions]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spMailDownloadOptions]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spMailFormDownload]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spMailFormDownload]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spMailFormSubmissions]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spMailFormSubmissions]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spMailTypeOptions]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spMailTypeOptions]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblSubscription]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblSubscription]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblSubscriptionQuota]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblSubscriptionQuota]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblSubscriptionRenewal]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblSubscriptionRenewal]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spPurchasersLast2Years]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spPurchasersLast2Years]

if exists (select * from dbo.sysobjects where id = object_id(N'spPurchasersLast2Years') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure spPurchasersLast2Years

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spEmailOptOuts]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[spEmailOptOuts]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblCartCarrier]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblCartCarrier]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblCartOrderDelivery]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblCartOrderDelivery]


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblSchemaVersion]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblSchemaVersion]

if exists (select * from dbo.sysobjects where id = object_id(N'sp_GetProductGroups') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure sp_GetProductGroups

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[fxn_getPagePath]') and xtype in (N'FN', N'IF', N'TF'))
drop function [dbo].[fxn_getPagePath]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblCartPayment]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblCartPayment]

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblContentIndex]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblContentIndex]
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblContentIndexDef]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblContentIndexDef]
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblSchemaVersion]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblSchemaVersion]


if exists (select * from dbo.sysobjects where id = object_id(N'spCheckDiscounts') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure spCheckDiscounts

if exists (select * from dbo.sysobjects where id = object_id(N'spGetEventsTicketsCartsPrice') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure spGetEventsTicketsCartsPrice

if exists (select * from dbo.sysobjects where id = object_id(N'spGetEventsTicketsCartsRefund') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure spGetEventsTicketsCartsRefund

if exists (select * from dbo.sysobjects where id = object_id(N'spTicketsRefundSummary') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure spTicketsRefundSummary

if exists (select * from dbo.sysobjects where id = object_id(N'spTicketsSoldSummary') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure spTicketsSoldSummary

if exists (select * from dbo.sysobjects where id = object_id(N'spUpdateContentIndexData') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure spUpdateContentIndexData

if exists (select * from dbo.sysobjects where id = object_id(N'spSearchDirectory') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure spSearchDirectory

if exists (select * from dbo.sysobjects where id = object_id(N'spScheduleToUpdateIndexTable') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure spScheduleToUpdateIndexTable

if exists (select * from dbo.sysobjects where id = object_id(N'spGetPagesByParentPageId') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure spGetPagesByParentPageId

if exists (select * from dbo.sysobjects where id = object_id(N'spGetPriceRange') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure spGetPriceRange

if exists (select * from dbo.sysobjects where id = object_id(N'spRemoveDuplicateAuditKey') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure spRemoveDuplicateAuditKey

if exists (select * from dbo.sysobjects where id = object_id(N'spGetValidShippingOptions') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure spGetValidShippingOptions



if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[vw_GetDistinctEvents]') and OBJECTPROPERTY(id, N'IsView') = 1)
drop view [dbo].[vw_GetDistinctEvents]
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[vw_GetDistinctEventsTickets]') and OBJECTPROPERTY(id, N'IsView') = 1)
drop view [dbo].[vw_GetDistinctEventsTickets]
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[vw_GetDistinctTickets]') and OBJECTPROPERTY(id, N'IsView') = 1)
drop view [dbo].[vw_GetDistinctTickets]
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[vw_GetEventsTicketWithCartContact]') and OBJECTPROPERTY(id, N'IsView') = 1)
drop view [dbo].[vw_GetEventsTicketWithCartContact]
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[vw_GetEventsTicketWithCartContactRefund]') and OBJECTPROPERTY(id, N'IsView') = 1)
drop view [dbo].[vw_GetEventsTicketWithCartContactRefund]
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[vw_ticketsSalesReport]') and OBJECTPROPERTY(id, N'IsView') = 1)
drop view [dbo].[vw_ticketsSalesReport]