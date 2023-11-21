// ***********************************************************************
// $Library:     eonic.dbhelper
// $Revision:    3.1  
// $Date:        2006-03-02
// $Author:      Trevor Spink (trevor@eonic.co.uk)
// &Website:     www.eonic.co.uk
// &Licence:     All Rights Reserved.
// $Copyright:   Copyright (c) 2002 - 2006 Eonic Ltd.
// ***********************************************************************

using Protean.Tools;
using Protean.CMS;

namespace Protean.CMS
{
    using Protean.Tools.Integration.Twitter;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Text.RegularExpressions;
    using System.Web.Configuration;
    using System.Xml;

    public partial class Cms
    {

        // Inherits dbTools
        public partial class dbHelper : Protean.Tools.Database
        {

            #region New Error Handling
            public new event OnErrorEventHandler OnError;

            public new delegate void OnErrorEventHandler(object sender, Protean.Tools.Errors.ErrorEventArgs e);

            private void _OnError(object sender, Protean.Tools.Errors.ErrorEventArgs e)
            {
                OnError?.Invoke(sender, e);
            }

            #endregion

            #region Declarations

            private new const string mcModuleName = "Eonic.dbHelper";

            private System.Web.HttpContext moCtx;

            // Private goApp As System.Web.HttpApplicationState

            public System.Web.HttpRequest goRequest;
            public System.Web.HttpResponse goResponse;
            public System.Web.SessionState.HttpSessionState goSession; // we need to pass this through from Web
            public System.Web.HttpServerUtility goServer;
            public XmlDocument moPageXml;

            public System.Collections.Specialized.NameValueCollection goConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");

            public long mnUserId;

            public bool gbAdminMode = false;
            public Cms myWeb;

            private SqlDataAdapter moDataAdpt;
            private PermissionLevel nCurrentPermissionLevel = PermissionLevel.Open;

            private Protean.Providers.Messaging.BaseProvider moMessaging;

            private bool gbVersionControl = false;


            #endregion
            #region Initialisation


            public dbHelper(ref Cms aWeb) : base()
            {
                try
                {
                    myWeb = aWeb;
                    PerfMonLog("dbHelper", "New");
                    if (moCtx is null)
                    {
                        moCtx = aWeb.moCtx;
                    }
                    // If Not (moCtx Is Nothing) Then
                    // goApp = moCtx.Application
                    goRequest = moCtx.Request;
                    goResponse = moCtx.Response;
                    goSession = moCtx.Session;
                    goServer = moCtx.Server;
                    // End If

                    ResetConnection("Data Source=" + goConfig["DatabaseServer"] + "; " + "Initial Catalog=" + goConfig["DatabaseName"] + "; " + GetDBAuth());


                    moPageXml = myWeb.moPageXml;
                    mnUserId = myWeb.mnUserId;

                    if (myWeb != null)
                    {
                        gbVersionControl = myWeb.gbVersionControl;
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""));
                }

                base.OnError += _OnError;
            }

            public void PerfMonLog(string classname, string desc, string desc2 = null)
            {
                if (myWeb != null)
                {
                    myWeb.PerfMon.Log(classname, desc, desc2);
                }

            }

            public string GetDBAuth()
            {
                // PerfMonLog("dbHelper", "getDBAuth")
                try
                {
                    string dbAuth;
                    if (!string.IsNullOrEmpty(goConfig["DatabasePassword"]))
                    {
                        dbAuth = "user id=" + goConfig["DatabaseUsername"] + "; password=" + goConfig["DatabasePassword"];
                    }
                    else if (!string.IsNullOrEmpty(goConfig["DatabaseAuth"]))
                    {
                        dbAuth = goConfig["DatabaseAuth"];
                    }
                    else
                    {
                        dbAuth = "Integrated Security=SSPI;";
                    }
                    return dbAuth;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getDBAuth", ex, ""));
                    return null;
                }
            }


            public dbHelper(string cConnectionString, long nUserId, System.Web.HttpContext moCtx = default)
            {
                // MyBase.New(cConnectionString)
                try
                {

                    if (moCtx is null)
                    {
                        moCtx = System.Web.HttpContext.Current;
                    }

                    if (moCtx != null)
                    {
                        // goApp = moCtx.Application
                        goRequest = moCtx.Request;
                        goResponse = moCtx.Response;
                        goSession = moCtx.Session;
                        goServer = moCtx.Server;
                    }


                    myWeb = null;
                    // moPageXml = myWeb.moPageXml
                    mnUserId = nUserId;

                    ResetConnection(cConnectionString);
                    base.ConnectionPooling = true;
                    base.ConnectTimeout = 15;
                    base.MinPoolSize = 0;
                    base.MaxPoolSize = 100;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""));
                }

                base.OnError += _OnError;
            }

            public dbHelper(string cDbServer, string cDbName, long nUserId, System.Web.HttpContext moCtx = default) : base()
            {

                try
                {
                    if (moCtx is null)
                    {
                        moCtx = System.Web.HttpContext.Current;
                    }

                    // goApp = moCtx.Application
                    goRequest = moCtx.Request;
                    goResponse = moCtx.Response;
                    goSession = moCtx.Session;
                    goServer = moCtx.Server;

                    ResetConnection("Data Source=" + cDbServer + "; " + "Initial Catalog=" + cDbName + "; " + GetDBAuth());

                    myWeb = null;
                    // moPageXml = myWeb.moPageXml
                    mnUserId = nUserId;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""));
                }

                base.OnError += _OnError;
            }

            public void ResetConnection(string cConnectionString)
            {
                string cPassword;
                string cUsername;
                string cDBAuth;
                try
                {
                    var ocon = new SqlConnection(cConnectionString);
                    base.DatabaseName = ocon.Database;
                    base.DatabaseServer = ocon.DataSource;
                    ocon.Close();
                    ocon = default;

                    // Let's work out where to get the authorisation from - ideally it should be the connection string.
                    if (cConnectionString.ToLower().Contains("user id=") & cConnectionString.ToLower().Contains("password="))
                    {
                        cDBAuth = cConnectionString;
                    }
                    else
                    {
                        // No authorisation information provided in the connection string.  
                        // We need to source it from somewhere, let's try the web.config 
                        cDBAuth = GetDBAuth();
                    }

                    // Let's find the username and password
                    cUsername = Protean.Tools.Text.SimpleRegexFind(cDBAuth, "user id=([^;]*)", 1, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    cPassword = Protean.Tools.Text.SimpleRegexFind(cDBAuth, "password=([^;]*)", 1, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                    base.DatabaseUser = cUsername;
                    base.DatabasePassword = cPassword;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ResetConnection", ex, ""));
                }
            }


            #endregion

            // Install-Package Microsoft.VisualBasic


            #region Enums
            public new enum objectTypes
            {
                Content = 0,
                ContentLocation = 1,
                ContentRelation = 2,
                ContentStructure = 3,
                Directory = 4,
                DirectoryRelation = 5,
                Permission = 6,
                QuestionaireResult = 7,
                QuestionaireResultDetail = 8,
                Audit = 9,
                CourseResult = 10,
                ActivityLog = 11,
                CartOrder = 12,
                CartItem = 13,
                CartContact = 14,
                CartShippingLocation = 15,
                CartShippingMethod = 16,
                CartShippingRelations = 17,
                CartCatProductRelations = 18,
                CartDiscountRules = 19,
                CartDiscountDirRelations = 20,
                CartDiscountProdCatRelations = 21,
                CartProductCategories = 22,
                ScheduledItem = 23,
                Subscription = 24,
                CartPaymentMethod = 25,
                Codes = 26,
                ContentVersion = 27,
                CartShippingPermission = 28,
                PageVersion = 29,
                Lookup = 30,
                CartDelivery = 31,
                CartCarrier = 32,
                SubscriptionRenewal = 33,
                CartPayment = 34,

                // 100-199 reserved for LMS
                CpdLog = 100,
                Certificate = 101,

                // 200- reserved for [next thing]

                indexkey = 200,
                // indexdefkey = 201
                nShipProdCatRelKey = 202,
                nEmailActivityKey = 203
            }

            public enum TableNames
            {
                tblContent = 0,
                tblContentLocation = 1,
                tblContentRelation = 2,
                tblContentStructure = 3, // also 29
                tblDirectory = 4,
                tblDirectoryRelation = 5,
                tblDirectoryPermission = 6,
                tblQuestionaireResult = 7,
                tblQuestionaireResultDetail = 8,
                tblAudit = 9,
                tblCourseResult = 10,
                tblActivityLog = 11,
                tblCartOrder = 12,
                tblCartItem = 13,
                tblCartContact = 14,
                tblCartShippingLocations = 15, // was tblCartShippingLocatiosn
                tblCartShippingMethods = 16, // was tblCartShippingMethod
                tblCartShippingRelations = 17,
                tblCartCatProductRelations = 18,
                tblCartDiscountRules = 19,
                tblCartDiscountDirRelations = 20,
                tblCartDiscountProdCatRelations = 21,
                tblCartProductCategories = 22,
                tblActions = 23, // was "tblActions" in GetTable
                tblSubscription = 24, // was tblDirectorySubscription
                tblCartPaymentMethod = 25,
                tblCodes = 26,
                tblContentVersions = 27,
                tblCartShippingPermission = 28,

                // tblContentStructure = 29 'duplicate, but leave this
                tblLookup = 30,
                tblCartOrderDelivery = 31,
                tblCartCarrier = 32,
                tblSubscriptionRenewal = 33,
                tblCartPayment = 34,

                // 100-199 reserved for LMS
                tblCpdLog = 100,
                tblCertificate = 101,

                // 200- reserved for [next thing]

                // tblContentIndex = 200
                tblContentIndexDef = 200,
                tblCartShippingProductCategoryRelations = 202,
                tblEmailActivityLog = 203
            }

            public enum PermissionLevel
            {
                Denied = 0, // delete
                Open = 1, // not really used for DBe
                View = 2,
                Add = 3,
                AddUpdateOwn = 4,
                UpdateAll = 5,
                Approve = 6,
                AddUpdateOwnPublish = 7,
                Publish = 8,
                Full = 9
            }

            public enum Status
            {
                Hidden = 0,
                Live = 1,
                Superceded = 2,
                Pending = 3,
                InProgress = 4, // preview
                Rejected = 5,
                DraftSuperceded = 6,
                Lead_AwaitingAuditBooking = 7,
                Lead_AuditBooked = 8,
                Lead_QuoteSupplied = 9,
                Lead_QuoteDeclined = 10,
                Lead_QuoteAccepted = 11,
                Lead_LeadRejected = 12
            }

            public enum ActivityType
            {
                Undefined = 0,
                // General
                Logon = 1,
                PageViewed = 2,
                Email = 3,
                Logoff = 4,
                Alert = 5,
                ContentDetailViewed = 6,
                SessionStart = 7,
                SessionEnd = 8,
                SessionContinuation = 9,
                Register = 10,
                DocumentDownloaded = 11,
                Search = 12,
                FuzzySearch = 13,
                ReportDownloaded = 14,
                SessionReconnectFromCookie = 15,
                LogonInvalidPassword = 16,
                HistoricPassword = 17,
                Recompile = 18,

                // Audit changes 
                StatusChangeLive = 30,
                StatusChangeHidden = 31,
                StatusChangeApproved = 32,
                StatusChangePending = 34,
                StatusChangeInProgress = 35,
                StatusChangeRejected = 36,
                StatusChangeSuperceded = 37,

                // Admin - Starting at 40
                ContentAdded = 40,
                ContentEdited = 41,
                ContentHidden = 42,
                ContentDeleted = 43,
                ContentImport = 44,

                PageAdded = 60,
                PageEdited = 61,
                PageHidden = 62,
                PageDeleted = 63,

                SetupDataUpgrade = 70,

                // Notifications
                NewsLetterSent = 80,
                PendingNotificationSent = 81,
                JobApplication = 82,

                // Custom
                Custom1 = 98,
                Custom2 = 99,

                // Polls
                SubmitVote = 110,
                VoteExcluded = 111,

                // Syndication
                SyndicationStarted = 200,
                SyndicationInProgress = 201,
                SyndicationPartialSuccess = 202,
                SyndicationFailed = 203,
                SyndicationCompleted = 204,
                ContentSyndicated = 205,

                // Subscriptions
                SubscriptionProcess = 300,
                SubscriptionProcessAttempt = 301,
                SubscriptionAlert = 302,

                // OpenQuote
                ValidationError = 255,

                // Integrations - 900+
                IntegrationTwitterPost = 901,

                // Order Status Change
                OrderStatusChange = 400

            }

            public enum DirectoryType
            {
                User = 0,
                Department = 1,
                Company = 2,
                Group = 3,
                Role = 4
            }

            public enum CopyContentType
            {
                None = 0,
                Copy = 1,
                Locate = 2,
                LocateWithPrimary = 3,
                CopyForce = 4
            }

            // Note - base 2 so they can be combined
            public enum RelationType
            {
                Parent = 1,
                Child = 2
            }

            public enum CodeType
            {
                Membership = 1,
                Discount = 2
            }

            public enum PageVersionType
            {
                Personalisation = 1,
                WorkingCopy = 2,
                Language = 3,
                SplitTest = 4
            }


            #endregion
            #region Properties
            internal PermissionLevel CurrentPermissionLevel
            {
                get
                {
                    return nCurrentPermissionLevel;
                }
                set
                {
                    nCurrentPermissionLevel = value;
                }
            }



            #endregion
            #region Shared Functions

            public static bool CanPublish(PermissionLevel nPermissionLevel)
            {
                try
                {
                    return nPermissionLevel == PermissionLevel.Publish | nPermissionLevel == PermissionLevel.AddUpdateOwnPublish | nPermissionLevel == PermissionLevel.Approve | nPermissionLevel == PermissionLevel.Full;


                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            public static bool CanAddUpdate(PermissionLevel nPermissionLevel)
            {
                try
                {
                    return nPermissionLevel == PermissionLevel.Add | nPermissionLevel == PermissionLevel.AddUpdateOwnPublish | nPermissionLevel == PermissionLevel.AddUpdateOwn | nPermissionLevel == PermissionLevel.UpdateAll | nPermissionLevel == PermissionLevel.Full;



                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            public static bool CanOnlyUseOwn(PermissionLevel nPermissionLevel)
            {
                try
                {
                    return nPermissionLevel == PermissionLevel.AddUpdateOwnPublish | nPermissionLevel == PermissionLevel.AddUpdateOwn;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            #endregion
            #region Table Definition Procedures
            public string TableKey(TableNames cTableName)
            {
                return getKey((objectTypes)cTableName);
            }

            public string getTable(objectTypes objectType)
            {

                PerfMonLog("DBHelper", "getTable");

                string cProcessInfo = "";
                string cObjectName;

                try
                {

                    objectType = (objectTypes)(int)objectType;
                    // hack for tblContentStructure = 29. It is already declared as 3.
                    if ((int)objectType == 29)
                    {
                        cObjectName = "tblContentStructure";
                    }
                    else
                    {
                        cObjectName = Enum.GetName(typeof(TableNames), objectType);
                    }

                    return cObjectName;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getNameByKey", ex, cProcessInfo));
                    return "";
                }

                // Select Case objectType
                // Case 0
                // Return "tblContent"
                // Case 1
                // Return "tblContentLocation"
                // Case 2
                // Return "tblContentRelation"
                // Case 3, 29
                // Return "tblContentStructure"
                // Case 4
                // Return "tblDirectory"
                // Case 5
                // Return "tblDirectoryRelation"
                // Case 6
                // Return "tblDirectoryPermission"
                // Case 7
                // Return "tblQuestionaireResult"
                // Case 8
                // Return "tblQuestionaireResultDetail"
                // Case 9
                // Return "tblAudit"
                // Case 10
                // Return "tblCourseResult"
                // Case 11
                // Return "tblActivityLog"
                // Case 12
                // Return "tblCartOrder"
                // Case 13
                // Return "tblCartItem"
                // Case 14
                // Return "tblCartContact"
                // Case 15
                // Return "tblCartShippingLocations"
                // Case 16
                // Return "tblCartShippingMethods"
                // Case 17
                // Return "tblCartShippingRelations"
                // Case 18
                // Return "tblCartCatProductRelations"
                // Case 19
                // Return "tblCartDiscountRules"
                // Case 20
                // Return "tblCartDiscountDirRelations"
                // Case 21
                // Return "tblCartDiscountProdCatRelations"
                // Case 22
                // Return "tblCartProductCategories"
                // Case 23
                // Return "tblActions"
                // Case 24
                // Return "tblDirectorySubscriptions"
                // Case 25
                // Return "tblCartPaymentMethod"
                // Case 26
                // Return "tblCodes"
                // Case 27
                // Return "tblContentVersions"
                // Case 28
                // Return "tblCartShippingPermission"
                // 'Case 29
                // '    Return "tblContentStructure"
                // Case Else
                // Return ""
                // End Select
            }

            protected internal string getKey(objectTypes objectType)
            {

                string strReturn = "";
                switch (objectType)
                {
                    case 0:
                        {
                            strReturn = "nContentKey";
                            break;
                        }
                    case (objectTypes)1:
                        {
                            strReturn = "nContentLocationKey"; // "nContentId"
                            break;
                        }
                    case (objectTypes)2:
                        {
                            strReturn = "nContentRelationKey";
                            break;
                        }
                    case (objectTypes)3:
                    case (objectTypes)29:
                        {
                            strReturn = "nStructKey";
                            break;
                        }
                    case (objectTypes)4:
                        {
                            strReturn = "nDirKey";
                            break;
                        }
                    case (objectTypes)5:
                        {
                            strReturn = "nRelKey";
                            break;
                        }
                    case (objectTypes)6:
                        {
                            strReturn = "nPermKey";
                            break;
                        }
                    case (objectTypes)7:
                        {
                            strReturn = "nQResultsKey";
                            break;
                        }
                    case (objectTypes)8:
                        {
                            strReturn = "nQDetailKey";
                            break;
                        }
                    case (objectTypes)9:
                        {
                            strReturn = "nAuditKey";
                            break;
                        }
                    case (objectTypes)10:
                        {
                            strReturn = "nCourseResultKey";
                            break;
                        }
                    case (objectTypes)11:
                        {
                            strReturn = "nActivityKey";
                            break;
                        }
                    case (objectTypes)12:
                        {
                            strReturn = "nCartOrderKey";
                            break;
                        }
                    case (objectTypes)13:
                        {
                            strReturn = "nCartItemKey";
                            break;
                        }
                    case (objectTypes)14:
                        {
                            strReturn = "nContactKey";
                            break;
                        }
                    case (objectTypes)15:
                        {
                            strReturn = "nLocationKey";
                            break;
                        }
                    case (objectTypes)16:
                        {
                            strReturn = "nShipOptKey";
                            break;
                        }
                    case (objectTypes)17:
                        {
                            strReturn = "nShpRelKey";
                            break;
                        }
                    case (objectTypes)18:
                        {
                            return "nCatProductRelKey";
                        }
                    case (objectTypes)19:
                        {
                            return "nDiscountKey";
                        }
                    case (objectTypes)20:
                        {
                            return "nDiscountDirRelationKey";
                        }
                    case (objectTypes)21:
                        {
                            return "nDiscountProdCatRelationKey";
                        }
                    case (objectTypes)22:
                        {
                            return "nCatKey";
                        }
                    case (objectTypes)23:
                        {
                            return "nActionKey";
                        }
                    case (objectTypes)24:
                        {
                            return "nSubKey";
                        }
                    case (objectTypes)25:
                        {
                            return "nPayMthdKey";
                        }
                    case (objectTypes)26:
                        {
                            return "nCodeKey";
                        }
                    case (objectTypes)27:
                        {
                            return "nContentVersionKey";
                        }
                    case (objectTypes)28:
                        {
                            return "nCartShippingPermissionKey";
                        }
                    // Case 29 'duplicate, but leave this
                    // Return "nStructKey" 
                    case (objectTypes)30:
                        {
                            return "nLkpID";
                        }
                    case (objectTypes)31:
                        {
                            return "nDeliveryKey";
                        }
                    case (objectTypes)32:
                        {
                            return "nCarrierKey";
                        }
                    case (objectTypes)33:
                        {
                            return "nSubRenewalKey";
                        }
                    case (objectTypes)34:
                        {
                            return "nCartPaymentKey";
                        }
                    // 100-199 reserved for LMS
                    case (objectTypes)100:
                        {
                            return "nCpdLogKey";
                        }
                    case (objectTypes)101:
                        {
                            return "nCertificateKey";
                        }

                    // 200- reserved for [next thing]
                    // Add new key id for Index def table by nita
                    case (objectTypes)200:
                        {
                            return "nContentIndexDefKey";
                        }
                    case (objectTypes)202:
                        {
                            return "nShipProdCatRelKey";
                        }

                }
                return strReturn;
            }

            private string getParIdFname(objectTypes objectType)
            {
                PerfMonLog("DBHelper", "getParIdFname");
                string strReturn = "";
                switch (objectType)
                {

                    case (objectTypes)2:
                        {
                            strReturn = "nContentParentId";
                            break;
                        }
                    case (objectTypes)3:
                        {
                            strReturn = "nStructParId";
                            break;
                        }

                    case (objectTypes)15:
                        {
                            strReturn = "nLocationParId";
                            break;
                        }
                    case (objectTypes)29:
                        {
                            strReturn = "nVersionParId";
                            break;
                        }
                    case (objectTypes)30:
                        {
                            strReturn = "nLkpParent";
                            break;
                        }
                }
                return strReturn;
            }

            private string getOrderFname(objectTypes objectType)
            {
                string strReturn = "";
                switch (objectType)
                {
                    case (objectTypes)1:
                        {
                            strReturn = "nDisplayOrder";
                            break;
                        }
                    case (objectTypes)2:
                        {
                            strReturn = "nDisplayOrder";
                            break;
                        }
                    case (objectTypes)3:
                        {
                            strReturn = "nStructOrder";
                            break;
                        }
                    case (objectTypes)18:
                        {
                            strReturn = "nDisplayOrder";
                            break;
                        }
                    case (objectTypes)29:
                        {
                            strReturn = "nStructOrder";
                            break;
                        }
                    case (objectTypes)30:
                        {
                            strReturn = "nDisplayOrder";
                            break;
                        }

                        // Case 5
                        // Return "nRelKey"
                }
                return strReturn;
            }

            private string getNameFname(objectTypes objectType)
            {
                string strReturn = "";
                switch (objectType)
                {
                    case 0:
                        {
                            strReturn = "cContentName";
                            break;
                        }
                    case (objectTypes)3:
                        {
                            strReturn = "cStructName";
                            break;
                        }
                    case (objectTypes)4:
                        {
                            strReturn = "cDirName";
                            break;
                        }

                }
                return strReturn;
            }

            private string getSchemaFname(objectTypes objectType)
            {
                string strReturn = "";
                switch (objectType)
                {
                    case 0:
                        {
                            strReturn = "cContentSchemaName";
                            break;
                        }
                    case (objectTypes)4:
                        {
                            strReturn = "cDirSchema";
                            break;
                        }
                    case (objectTypes)12:
                        {
                            strReturn = "cCartSchemaName";
                            break;
                        }
                }
                return strReturn;
            }

            public string getNameByKey(objectTypes objectType, long nKey)
            {

                string sSql;
                // Dim oDr As SqlDataReader
                string sResult = "";
                string cProcessInfo = "";
                try
                {

                    switch (objectType)
                    {

                        case 0:
                        case (objectTypes)3:
                        case (objectTypes)4:
                            {
                                sSql = "select " + getNameFname(objectType) + " from " + getTable(objectType) + " where " + getKey(objectType) + " = " + nKey;
                                using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                                {

                                    while (oDr.Read())
                                        sResult = oDr[0].ToString();
                                }

                                break;
                            }
                    }

                    return sResult;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getNameByKey", ex, cProcessInfo));
                    return "";
                }

            }

            public string getFRef(objectTypes objectType)
            {

                switch (objectType)
                {
                    case 0:
                        {
                            return "cContentForiegnRef";
                        }
                    case (objectTypes)1:
                        {
                            return "";
                        }
                    case (objectTypes)2:
                        {
                            return "";
                        }
                    case (objectTypes)3:
                        {
                            return "cStructForiegnRef";
                        }
                    case (objectTypes)4:
                        {
                            return "cDirForiegnRef";
                        }
                    case (objectTypes)5:
                        {
                            return "";
                        }
                    case (objectTypes)6:
                        {
                            return "";
                        }
                    case (objectTypes)7:
                        {
                            return "cQResultsForiegnRef";
                        }
                    case (objectTypes)8:
                        {
                            return "";
                        }
                    case (objectTypes)9:
                        {
                            return "";
                        }
                    case (objectTypes)10:
                        {
                            return "";
                        }
                    case (objectTypes)11:
                        {
                            return "";
                        }
                    case (objectTypes)12:
                        {
                            return "cCartForiegnRef";
                        }
                    case (objectTypes)13:
                        {
                            return "";
                        }
                    case (objectTypes)14:
                        {
                            return "cContactForeignRef";
                        }
                    case (objectTypes)15:
                        {
                            return "cLocationForeignRef";
                        }
                    case (objectTypes)16:
                        {
                            return "cShipOptForeignRef";
                        }
                    case (objectTypes)22:
                        {
                            return "cCatForeignRef";
                        }
                    case (objectTypes)26:
                        {
                            return "cCode";
                        }

                    default:
                        {
                            return "";
                        }
                }
            }

            /// <summary>
            /// Check if a database object has a bespoke equivalent in the database.
            /// Bespoke objects are prefixed with the letter "b".
            /// e.g. bsp_MyProcedure
            /// </summary>
            /// <param name="dbObjectName">The object name to check (without the b prefix)</param>
            /// <returns>"b" + dbObjectName if it exists, dbObjectName otherwise</returns>
            /// <remarks>Note: This doesn't check for the existence of dbObjectName</remarks>
            public string getDBObjectNameWithBespokeCheck(string dbObjectName)
            {
                string appcheckName = dbObjectName;
                try
                {
                    if (string.IsNullOrEmpty(dbObjectName))
                    {
                        appcheckName = moCtx.Application["getdb-" + dbObjectName].ToString();
                        if (string.IsNullOrEmpty(appcheckName))
                        {

                            if (base.checkDBObjectExists("b" + dbObjectName))
                            {
                                appcheckName = "b" + dbObjectName;
                            }
                            else
                            {
                                appcheckName = dbObjectName;
                            }

                            // goApp.Lock()
                            moCtx.Application["getdb-" + dbObjectName] = appcheckName;
                            // goApp.UnLock()

                        }


                    }

                    return appcheckName;
                }

                catch (Exception ex)
                {
                    return dbObjectName;
                }

            }

            #endregion

            private string getPermissionLevel(PermissionLevel oPermLevel)
            {
                switch (oPermLevel)
                {
                    case 0:
                        {
                            return "Denied";
                        }
                    case (PermissionLevel)1:
                        {
                            return "Open";
                        }
                    case (PermissionLevel)2:
                        {
                            return "View";
                        }
                    case (PermissionLevel)3:
                        {
                            return "Add";
                        }
                    case (PermissionLevel)4:
                        {
                            return "AddUpdateOwn";
                        }
                    case (PermissionLevel)5:
                        {
                            return "UpdateAll";
                        }
                    case (PermissionLevel)6:
                        {
                            return "Approve";
                        }
                    case (PermissionLevel)7:
                        {
                            return "AddUpdateOwnPublish";
                        }
                    case (PermissionLevel)8:
                        {
                            return "Publish";
                        }
                    case (PermissionLevel)9:
                        {
                            return "Full";
                        }

                    default:
                        {
                            return "Null";
                        }
                }
            }

            public string getKeyByNameAndSchema(objectTypes objectType, string cSchemaName, string cName)
            {

                PerfMonLog("DBHelper", "getKeyByNameAndSchema");


                string sSql;
                // Dim oDr As SqlDataReader
                string sResult = "";
                string cProcessInfo = "";
                try
                {

                    switch (objectType)
                    {

                        case 0:
                        case (objectTypes)4:
                        case (objectTypes)3:
                            {

                                sSql = "select " + getKey(objectType) + " from " + getTable(objectType) + " where " + getNameFname(objectType) + " LIKE '" + cName + "'";
                                using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                                {
                                    if (oDr.HasRows)
                                    {
                                        while (oDr.Read())
                                            sResult = oDr[0].ToString();
                                    }
                                }

                                break;
                            }

                    }

                    return sResult;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getNameByKey", ex, cProcessInfo));
                    return "";
                }

            }

            public long getObjectStatus(objectTypes objecttype, long nId)
            {

                PerfMonLog("DBHelper", "getObjectStatus");

                string sSql;
                var nAuditId = default(long);
                // Dim oDr As SqlDataReader
                string sResult = "";
                string cProcessInfo = "";
                try
                {
                    sSql = "select nAuditId from " + getTable(objecttype) + " where " + getKey(objecttype) + " = " + nId;
                    using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {

                        while (oDr.Read())
                            nAuditId = Convert.ToInt64(oDr[0]);

                    }
                    // we could test the current status to see if the change is a valid one. I.E. live can only be hidden not moved back to approval or InProgress, but the workflow should ensure this doesn't happen.
                    sSql = "select nStatus from tblAudit WHERE nAuditKey =" + nAuditId;
                    sResult = ExeProcessSqlScalar(sSql);

                    return Convert.ToInt64(sResult);
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getObjectStatus", ex, cProcessInfo));
                    return Convert.ToInt64("");
                }

            }


            public virtual string setObjectStatus(objectTypes objecttype, Status status, long nId)
            {

                PerfMonLog("DBHelper", "setObjectStatus");

                string sSql;
                var nAuditId = default(long);
                // Dim oDr As SqlDataReader
                string sResult = "";
                string cProcessInfo = "";
                try
                {
                    sSql = "select nAuditId from " + getTable(objecttype) + " where " + getKey(objecttype) + " = " + nId;
                    // we want to touch the parent table just incase we have any triggers asscoiated with it
                    // change get DateReader to getdataset and update.
                    using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {

                        while (oDr.Read())
                            nAuditId = Convert.ToInt64(oDr[0]);

                    }
                    // we could test the current status to see if the change is a valid one. I.E. live can only be hidden not moved back to approval or InProgress, but the workflow should ensure this doesn't happen.
                    sSql = "UPDATE tblAudit SET nStatus = " + ((int)status).ToString() + " WHERE nAuditKey =" + nAuditId;
                    sResult = ExeProcessSql(sSql).ToString();

                    // update query to fire triggers if any. This query does not change any data values.
                    if (nAuditId > 0L)
                    {
                        sSql = "UPDATE " + getTable(objecttype) + " SET nAuditId = " + nAuditId + " WHERE nAuditId =" + nAuditId;
                        sResult = ExeProcessSql(sSql).ToString();
                    }

                    if (objecttype == objectTypes.ContentStructure)
                    {
                        clearStructureCacheAll();
                    }

                    return sResult;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "setObjectStatus", ex, cProcessInfo));
                    return "";
                }

            }

            public virtual string getObjectStatus(objectTypes objecttype, string nId)
            {

                PerfMonLog("DBHelper", "setObjectStatus");

                string sSql;
                var nAuditId = default(long);
                // Dim oDr As SqlDataReader
                int sResult;
                string cProcessInfo = "";
                try
                {
                    sSql = "select nAuditId from " + getTable(objecttype) + " where " + getKey(objecttype) + " = " + nId;
                    using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {

                        while (oDr.Read())
                            nAuditId = Convert.ToInt64(oDr[0]);

                    }
                    sSql = "select nStatus from tblAudit WHERE nAuditKey =" + nAuditId;
                    sResult = Convert.ToInt32(myWeb.moDbHelper.ExeProcessSqlScalar(sSql));

                    return sResult.ToString();
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "setObjectStatus", ex, cProcessInfo));
                    return "";
                }

            }

            // Install-Package Microsoft.VisualBasic


            /// <summary>
            /// Assess an audit id, it's previous and new status and if changed logs the appropriate change
            /// </summary>
            /// <param name="auditId"></param>
            /// <param name="oldStatus"></param>
            /// <param name="newStatus"></param>
            /// <remarks></remarks>
            private void logObjectStatusChange(long auditId, Status oldStatus, Status newStatus)
            {

                PerfMonLog("DBHelper", "logObjectStatusChange");


                string cProcessInfo = "";
                ActivityType statusChange = ActivityType.Undefined;
                try
                {
                    // Check if something's changed
                    if (oldStatus != newStatus)
                    {

                        // Process the changes, start with the absolutes
                        if (newStatus == Status.Rejected)
                        {
                            statusChange = ActivityType.StatusChangeRejected;
                        }

                        else if (newStatus == Status.Superceded)
                        {
                            statusChange = ActivityType.StatusChangeSuperceded;
                        }

                        else if (newStatus == Status.Pending)
                        {
                            statusChange = ActivityType.StatusChangePending;
                        }

                        else if (newStatus == Status.InProgress)
                        {
                            statusChange = ActivityType.StatusChangeInProgress;
                        }

                        else if (newStatus == Status.Live & oldStatus == Status.Pending)
                        {
                            statusChange = ActivityType.StatusChangeApproved;
                        }

                        else if (newStatus == Status.Live)
                        {
                            statusChange = ActivityType.StatusChangeLive;
                        }

                        // If status has changed then log it
                        if (statusChange != ActivityType.Undefined)
                        {
                            logActivity(statusChange, myWeb.mnUserId, 0, 0, Convert.ToInt16(auditId), "");
                        }

                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "logObjectStatusChange", ex, cProcessInfo));
                }
            }


            internal int getPageAndArticleIdFromPath(ref long nPageId, ref long nArtId, string sFullPath, bool bSetGlobalPageVariable = true, bool bCheckPermissions = true)
            {

                PerfMonLog("dbHelper", "getPageIdFromPath");

                string[] aPath;
                string sPath;

                string sSql;
                DataSet ods;

                string sProcessInfo = "";

                try
                {

                    sPath = sFullPath;
                    sPath = goServer.UrlDecode(sPath);

                    // We have to assume that hyphens are spaces here
                    // Nore : if this is turned on, you will have to update any pages that have hyphens in their names
                    if (myWeb.moConfig["PageURLFormat"] == "hyphens")
                    {
                        sPath = sPath.Replace("-", " ");
                    }

                    sProcessInfo = "remove first and final /";
                    if (Strings.Right(sPath, 1) == "/")
                    {
                        sPath = Strings.Left(sPath, Strings.Len(sPath) - 1);
                    }
                    if (Strings.InStr(1, sPath, "/") == 1)
                    {
                        sPath = Strings.Right(sPath, Strings.Len(sPath) - 1);
                    }

                    sProcessInfo = "Strip the QueryString";
                    if (Strings.InStr(1, sPath, "?") > 0)
                        sPath = Strings.Left(sPath, Strings.InStr(1, sPath, "?") - 1);

                    sProcessInfo = "split to path array";
                    aPath = Strings.Split(sPath, "/");
                    if (Information.UBound(aPath) > 0)
                    {
                        // get the last in line
                        sPath = aPath[Information.UBound(aPath)];
                    }
                    else
                    {
                        sPath = sPath;
                    }



                    switch (myWeb.moConfig["DetailPathType"])
                    {
                        case "ContentType/ContentName":
                            {

                                string[] prefixs = myWeb.moConfig["DetailPrefix"].Split(',');
                                string thisPrefix = "";
                                string thisContentType = "";
                                // Dim oDr As SqlDataReader

                                int i;


                                if (nArtId == default)
                                {
                                    if (!(myWeb.moRequest["artid"] == ""))
                                    {
                                        nArtId = myWeb.GetRequestItemAsInteger("artid", 0);
                                    }
                                }

                                bool checkRedirect = false;

                                if (myWeb.moConfig["DetailPathTrailingSlash"] == "on")
                                {
                                    var loopTo = prefixs.Length - 1;
                                    for (i = 0; i <= loopTo; i++)
                                    {
                                        thisPrefix = prefixs[i].Substring(0, prefixs[i].IndexOf("/"));
                                        if (sFullPath.StartsWith("/" + thisPrefix) & !sFullPath.EndsWith("/"))
                                        {
                                            checkRedirect = true;
                                        }
                                    }
                                }

                                if (nArtId > 0L & !gbAdminMode)
                                {
                                    // article id was passed in the url so we may need to redirect

                                    sSql = "select cContentSchemaName, cContentName from tblContent c inner join tblAudit a on a.nAuditKey = c.nAuditId where nContentKey = " + nArtId;
                                    string contentType = "";
                                    string contentName = "";
                                    string redirectUrl = "";

                                    using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                                    {

                                        while (oDr.Read())
                                        {
                                            contentType = Convert.ToString(oDr[0]);
                                            contentName = Convert.ToString(oDr[1]);
                                        }
                                    }

                                    var loopTo1 = prefixs.Length - 1;
                                    for (i = 0; i <= loopTo1; i++)
                                    {
                                        thisPrefix = prefixs[i].Substring(0, prefixs[i].IndexOf("/"));
                                        thisContentType = prefixs[i].Substring(prefixs[i].IndexOf("/") + 1, prefixs[i].Length - prefixs[i].IndexOf("/") - 1);
                                        if ((contentType ?? "") == (thisContentType ?? ""))
                                        {
                                            redirectUrl += "/" + thisPrefix + "/" + contentName.ToString().Replace(" ", "-").Trim('-');
                                            if (myWeb.moConfig["DetailPathTrailingSlash"] == "on")
                                            {
                                                redirectUrl = redirectUrl + "/";
                                            }
                                        }
                                    }

                                    if ((sFullPath ?? "") != (redirectUrl ?? ""))
                                    {
                                        myWeb.msRedirectOnEnd = redirectUrl;
                                    }

                                }

                                var loopTo2 = prefixs.Length - 1;
                                for (i = 0; i <= loopTo2; i++)
                                {
                                    if ((prefixs[i].Substring(0, prefixs[i].IndexOf("/")) ?? "") == (aPath[0] ?? ""))
                                    {
                                        thisPrefix = prefixs[i].Substring(0, prefixs[i].IndexOf("/"));
                                        thisContentType = prefixs[i].Substring(prefixs[i].IndexOf("/") + 1, prefixs[i].Length - prefixs[i].IndexOf("/") - 1);
                                    }
                                }

                                if (!string.IsNullOrEmpty(thisPrefix))
                                {
                                    if (gbAdminMode)
                                    {
                                        sSql = "select TOP(1) nContentKey  from tblContent c inner join tblAudit a on a.nAuditKey = c.nAuditId where cContentName like '" + SqlFmt(sPath) + "' and cContentSchemaName like '" + thisContentType + "' order by nVersion desc";
                                    }
                                    else
                                    {
                                        sSql = "select TOP(1) nContentKey from tblContent c inner join tblAudit a on a.nAuditKey = c.nAuditId where cContentName like '" + SqlFmt(sPath) + "' and cContentSchemaName like '" + thisContentType + "' " + myWeb.GetStandardFilterSQLForContent() + " order by nVersion desc";
                                    }
                                    ods = GetDataSet(sSql, "Content");
                                    if (ods != null)
                                    {
                                        if (ods.Tables["Content"].Rows.Count == 1)
                                        {
                                            nArtId = Convert.ToInt64(ods.Tables["Content"].Rows[0]["nContentKey"]);
                                        }
                                        else
                                        {
                                            // handling for content versions ?
                                        }
                                    }

                                    // now get the page id 
                                    if (nArtId > 0L)
                                    {
                                        sSql = "select nStructId from tblContentLocation where bPrimary = 1 and nContentId = " + nArtId;
                                        ods = GetDataSet(sSql, "Pages");
                                        if (ods.Tables["Pages"].Rows.Count == 1)
                                        {
                                            if (bCheckPermissions)
                                            {
                                                // Check the permissions for the page - this will either return 0, the page id or a system page.
                                                long checkPermissionPageId = checkPagePermission(Convert.ToInt64(ods.Tables["Pages"].Rows[0]["nStructId"]));
                                                if (checkPermissionPageId != 0L & (ods.Tables("Pages").Rows("0").Item("nStructId") == checkPermissionPageId | IsSystemPage(checkPermissionPageId)))

                                                {
                                                    nPageId = checkPermissionPageId;
                                                }
                                            }
                                            else
                                            {
                                                nPageId = ods.Tables("Pages").Rows("0").Item("nStructId");
                                            }
                                            nPageId = ods.Tables("Pages").Rows("0").Item("nStructId");


                                            if (checkRedirect)
                                            {
                                                string redirectUrl;
                                                redirectUrl = "/" + thisPrefix + "/" + sPath.ToString().Replace(" ", "-").Trim('-') + "/";
                                                if ((sFullPath ?? "") != (redirectUrl ?? ""))
                                                {
                                                    myWeb.msRedirectOnEnd = redirectUrl;
                                                }
                                            }
                                        }


                                        else
                                        {
                                            // handling for multiple parents versions ?
                                        }
                                    }

                                }

                                break;
                            }
                    }

                    if (nArtId == default)
                    {
                        sSql = "select nStructKey, nStructParId, nVersionParId, cVersionLang from tblContentStructure where (cStructName like '" + SqlFmt(sPath) + "' or cStructName like '" + SqlFmt(Strings.Replace(sPath, " ", "")) + "' or cStructName like '" + SqlFmt(Strings.Replace(sPath, " ", "-")) + "')";

                        ods = GetDataSet(sSql, "Pages");

                        if (ods is not null)
                        {
                            if (ods.Tables("Pages").Rows.Count == 1)
                            {
                                nPageId = ods.Tables("Pages").Rows("0").Item("nStructKey");
                            }
                            // if there is just one page validate it
                            else if (ods.Tables("Pages").Rows.Count == 0)
                            {
                            }

                            // do nothing nothing found

                            else
                            {
                                foreach (DataRow oRow in ods.Tables("Pages").Rows)
                                {
                                    // Debug.WriteLine(oRow.Item("nStructKey"))
                                    if (!("0" + oRow.Item("nVersionParId") == 0))
                                    {
                                        // we have a language verion we need to behave differently to confirm id
                                        if (myWeb.mcPageLanguage == oRow.Item("cVersionLang"))
                                        {
                                            nPageId = oRow.Item("nStructKey");
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        int argnStep = Information.UBound(aPath) - 1;
                                        if (recurseUpPathArray(oRow.Item("nStructParId"), ref aPath, ref argnStep) == true)
                                        {
                                            if (bCheckPermissions)
                                            {

                                                // Check the permissions for the page - this will either return 0, the page id or a system page.
                                                long checkPermissionPageId = checkPagePermission(oRow.Item("nStructKey"));

                                                if (checkPermissionPageId != 0L & (oRow.Item("nStructKey") == checkPermissionPageId | IsSystemPage(checkPermissionPageId)))

                                                {
                                                    nPageId = checkPermissionPageId;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                nPageId = oRow.Item("nStructKey");
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Note : if sPath is empty the SQL call above WILL return pages, we don't want these, we want top level pgid
                    if (!(nPageId > 1L & !string.IsNullOrEmpty(sPath)))
                    {
                        // page path cannot be found we have an error that we raise later
                        if (sFullPath != "System+Pages/Page+Not+Found")
                        {
                            // first don't cache the page
                            myWeb.bPageCache = false;
                            nPageId = myWeb.gnPageNotFoundId;
                        }
                        else
                        {
                            nPageId = myWeb.RootPageId;
                        }
                    }

                    // If bSetGlobalPageVariable Then gnPageId = nPageId

                    // myWeb.PerfMon.Log("dbHelper", "getPageAndArticleIdFromPath-end")

                    return (int)nPageId;
                }
                catch (Exception ex)
                {

                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getPageAndArticleIdFromPath", ex, sProcessInfo));

                }

                return default;
            }

            internal int getPageIdFromPath(string sFullPath, bool bSetGlobalPageVariable = true, bool bCheckPermissions = true)
            {

                PerfMonLog("dbHelper", "getPageIdFromPath");


                string[] aPath;
                string sPath;

                string sSql;
                DataSet ods;
                var nPageId = default(int);

                string sProcessInfo = "";

                try
                {

                    sPath = sFullPath;
                    sPath = goServer.UrlDecode(sPath);

                    // We have to assume that hyphens are spaces here
                    // Nore : if this is turned on, you will have to update any pages that have hyphens in their names
                    if (myWeb.moConfig("PageURLFormat") == "hyphens")
                    {
                        sPath = Strings.Replace(sPath, "-", " ");
                    }

                    sProcessInfo = "remove first and final /";

                    if (Strings.Right(sPath, 1) == "/")
                    {
                        sPath = Strings.Left(sPath, Strings.Len(sPath) - 1);
                    }
                    if (Strings.InStr(1, sPath, "/") == 1)
                    {
                        sPath = Strings.Right(sPath, Strings.Len(sPath) - 1);
                    }

                    sProcessInfo = "Strip the QueryString";

                    if (Strings.InStr(1, sPath, "?") > 0)
                        sPath = Strings.Left(sPath, Strings.InStr(1, sPath, "?") - 1);

                    aPath = Strings.Split(sPath, "/");

                    if (Information.UBound(aPath) > 0)
                    {

                        // get the last in line
                        sPath = aPath[Information.UBound(aPath)];
                    }
                    else
                    {
                        sPath = sPath;
                    }

                    sSql = "select nStructKey, nStructParId, nVersionParId, cVersionLang from tblContentStructure where (cStructName like '" + SqlFmt(sPath) + "' or cStructName like '" + SqlFmt(Strings.Replace(sPath, " ", "")) + "' or cStructName like '" + SqlFmt(Strings.Replace(sPath, " ", "-")) + "')";

                    ods = GetDataSet(sSql, "Pages");


                    if (!(ods == null) && ods.Tables("Pages").Rows.Count == 1)
                    {
                        nPageId = ods.Tables("Pages").Rows("0").Item("nStructKey");
                    }
                    // if there is just one page validate it
                    else if (ods.Tables("Pages").Rows.Count == 0)
                    {
                    }

                    // do nothing nothing found

                    else
                    {
                        foreach (DataRow oRow in ods.Tables("Pages").Rows)
                        {
                            // Debug.WriteLine(oRow.Item("nStructKey"))
                            if (!("0" + oRow.Item("nVersionParId") == 0))
                            {
                                // we have a language verion we need to behave differently to confirm id
                                if (myWeb.mcPageLanguage == oRow.Item("cVersionLang"))
                                {
                                    nPageId = oRow.Item("nStructKey");
                                    break;
                                }
                            }
                            else
                            {
                                int argnStep = Information.UBound(aPath) - 1;
                                if (recurseUpPathArray(oRow.Item("nStructParId"), ref aPath, ref argnStep) == true)
                                {
                                    if (bCheckPermissions)
                                    {

                                        // Check the permissions for the page - this will either return 0, the page id or a system page.
                                        long checkPermissionPageId = checkPagePermission(oRow.Item("nStructKey"));

                                        if (checkPermissionPageId != 0L & (oRow.Item("nStructKey") == checkPermissionPageId | IsSystemPage(checkPermissionPageId)))

                                        {
                                            nPageId = (int)checkPermissionPageId;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        nPageId = oRow.Item("nStructKey");
                                        break;
                                    }
                                }
                            }
                        }
                    }


                    // Note : if sPath is empty the SQL call above WILL return pages, we don't want these, we want top level pgid
                    if (!(nPageId > 1 & !string.IsNullOrEmpty(sPath)))
                    {
                        // page path cannot be found we have an error that we raise later
                        if (sFullPath != "System+Pages/Page+Not+Found")
                        {
                            // first don't cache the page
                            myWeb.bPageCache = false;
                            nPageId = myWeb.gnPageNotFoundId;
                        }
                        else
                        {
                            nPageId = myWeb.RootPageId;
                        }
                    }

                    // If bSetGlobalPageVariable Then gnPageId = nPageId

                    PerfMonLog("dbHelper", "getPageIdFromPath-end");

                    return nPageId;
                }
                catch (Exception ex)
                {

                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getPageIdFromPath", ex, sProcessInfo));

                }

                return default;
            }

            public bool IsSystemPage(long pageId)
            {

                try
                {
                    return pageId != 0L & (pageId == myWeb.gnPageAccessDeniedId | pageId == myWeb.gnPageErrorId | pageId == myWeb.gnPageLoginRequiredId | pageId == myWeb.gnPageNotFoundId);


                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "IsSystemPage", ex, ""));
                    return false;
                }

            }

            // ''' <summary>
            // ''' <para>
            // ''' Check the request for certain parameters which if matched will create a 302 redirect
            // ''' For page redirects, ISAPI can't detect what we need, so we are going to need to check if PageURLFormat is set
            // ''' For content (artid) redirects, parameters are driven by the ISAPI rewrite routines which should be updated to do the following
            // ''' </para>
            // ''' <list>
            // '''   <item>
            // '''    If itemNNNN is detected then set u the redirect as follows:
            // '''    <list>
            // '''       <item>pass the path through as path</item>
            // '''       <item>pass itemNNNN through as artid=NNNN</item>
            // '''       <item>set a flag called redirect=statuscode</item>
            // '''    </list>
            // '''   </item>
            // ''' </list>
            // ''' <para>
            // ''' The redirect URL will be "redirPath/NNNN-/Product-Name
            // ''' </para>
            // ''' </summary>
            // ''' <returns></returns>
            // ''' <remarks>This may require the existing pages to be tidied up (i.e. removing hyphens and plusses)</remarks>
            // Friend Function legacyRedirection() As Boolean
            // PerfMonLog("DBHelper", "legacyRedirection")

            // Dim cProcessInfo As String = ""
            // Dim bRedirect As Boolean = False

            // Try

            // If myWeb.moConfig("LegacyRedirect") = "on" Then

            // Dim cPath As String = myWeb.moRequest("path") & ""
            // If Not (cPath.StartsWith("/")) Then cPath = "/" & cPath
            // If Not (cPath.EndsWith("/")) Then cPath &= "/"

            // ' The checks we need to make are as follows:
            // ' Is RedirPath being passed through (from ISAPI Rewrite)
            // ' Is artid being passed through

            // ' Check if an article redirect has been called
            // If Not (myWeb.moRequest("redirect") Is Nothing) _
            // AndAlso myWeb.moRequest("artid") <> "" _
            // AndAlso IsNumeric(myWeb.moRequest("artid")) Then

            // ' Try to find the product
            // Dim nArtId As Long = CLng(myWeb.moRequest("artid"))
            // Dim cSql As String = "SELECT cContentName FROM tblContent WHERE nContentKey = " & SqlFmt(nArtId.ToString)
            // Dim cName As String = GetDataValue(cSql, , , "")



            // If Not (String.IsNullOrEmpty(cName)) Then

            // ' Replace any non-alphanumeric with hyphens
            // Dim oRe As New Text.RegularExpressions.Regex("[^A-Z0-9]", Text.RegularExpressions.RegexOptions.IgnoreCase)
            // cName = oRe.Replace(cName, "-").Trim("-")


            // ' Iron out the spaces for paths
            // cPath = Replace(cPath, " ", "-")

            // ' Construct the new path
            // cPath = cPath & nArtId.ToString & "-/" & cName
            // bRedirect = True

            // End If

            // ElseIf myWeb.moConfig("PageURLFormat") = "hyphens" Then
            // ' Check the path - if hyphens is the preference then we need to analyse the path and rewrite it.
            // If cPath.Contains("+") Or cPath.Contains(" ") Then
            // cPath = Replace(cPath, "+", "-")
            // cPath = Replace(cPath, " ", "-")
            // bRedirect = True
            // End If
            // End If

            // ' Redirect
            // If bRedirect And myWeb.moSession("legacyRedirect") <> "on" Then
            // ' Stop recursive redirects
            // myWeb.moSession("legacyRedirect") = "on"

            // ' Assume status code is 301 unless instructed otherwise.
            // Dim nResponseCode As Integer = 301
            // Select Case myWeb.moRequest("redirect")
            // Case "301", "302", "303", "304", "307"
            // nResponseCode = CInt(myWeb.moRequest("redirect"))
            // End Select
            // HTTPRedirect(myWeb.moResponse, cPath, nResponseCode)

            // Else
            // myWeb.moSession("legacyRedirect") = "off"
            // End If

            // End If

            // Return bRedirect

            // Catch ex As Exception
            // RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "legacyRedirection", ex, cProcessInfo))
            // Return False
            // End Try

            // End Function

            internal string getPageLayout(long nPageId)
            {

                PerfMonLog("DBHelper", "getPageLayout");

                string cLayout = "";
                string cSql = "";

                try
                {

                    if (gbClone)
                    {
                        // If the page is cloned then we need to look at the page that it's cloned from
                        long nClonePageId = GetDataValue("select nCloneStructId from tblContentStructure where nStructKey = " + nPageId, default, default, 0);
                        if (nClonePageId > 0L)
                            nPageId = nClonePageId;
                    }

                    cSql = "select cStructLayout from  tblContentStructure where nStructKey = " + nPageId;
                    cLayout = GetDataValue(cSql, default, default, "default");
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getPageLayout", ex, cSql));
                }

                return cLayout;

            }


            internal string getPageLang(long nPageId)
            {

                PerfMonLog("DBHelper", "getPageLang");

                string cLayout = "";
                string cSql = "";

                try
                {

                    if (gbClone)
                    {
                        // If the page is cloned then we need to look at the page that it's cloned from
                        long nClonePageId = GetDataValue("select nCloneStructId from tblContentStructure where nStructKey = " + nPageId, default, default, 0);
                        if (nClonePageId > 0L)
                            nPageId = nClonePageId;
                    }

                    cSql = "select cVersionLang from  tblContentStructure where nStructKey = " + nPageId;
                    cLayout = GetDataValue(cSql, default, default, "default").ToString();
                    if (string.IsNullOrEmpty(cLayout))
                        cLayout = "en-gb";
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getPageLang", ex, cSql));
                }

                return cLayout;

            }



            /// <summary>
            ///    Given a parent id and a page name (from an array of page paths) this works out if the parent matched the page name.
            ///    It is intended to give a confirmation of page ids for ambiguous page name by working up the path.
            /// </summary>
            /// <param name="nParentid">The parent id of the page you've come from</param>
            /// <param name="aPath">An array of page names from a path</param>
            /// <param name="nStep">The level of the array to inspect</param>
            /// <returns>Returns True if the paths match the page IDs all the way up the array</returns>
            /// <remarks></remarks>
            private bool recurseUpPathArray(int nParentid, ref string[] aPath, ref int nStep)
            {
                bool recurseUpPathArrayRet = default;

                PerfMonLog("Base", "recurseUpPathArray");

                // Dim oDr As SqlDataReader
                string sSql;
                string sProcessInfo = "";

                recurseUpPathArrayRet = false;

                try
                {
                    if (nStep > -1)
                    {

                        sSql = "select nStructKey, nStructParId from tblContentStructure where nStructKey =" + nParentid + " and (cStructName like '" + SqlFmt(aPath[nStep]) + "' or cStructName like '" + SqlFmt(Strings.Replace(aPath[nStep], " ", "")) + "')";
                        using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                        {

                            if (!oDr.HasRows)
                            {
                                recurseUpPathArrayRet = false;
                            }
                            else
                            {
                                oDr.Read();
                                long nParId = oDr("nStructParId");
                                int argnStep = nStep - 1;
                                recurseUpPathArrayRet = recurseUpPathArray((int)nParId, ref aPath, ref argnStep);
                            }
                        }
                    }

                    else if (nStep == -1 & nParentid == myWeb.RootPageId)
                    {
                        recurseUpPathArrayRet = true;
                    }
                    else
                    {
                        recurseUpPathArrayRet = false;
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getPageIdFromPath", ex, sProcessInfo));
                }

                return recurseUpPathArrayRet;
            }

            public int checkPagePermission(long nPageId)
            {

                PerfMonLog("DBHelper", "checkPagePermission");


                string sProcessInfo = "";
                long nAuthGroup = gnAuthUsers;

                try
                {
                    // if we are not in admin mode
                    if (!gbAdminMode)
                    {
                        // Check we have access to this page
                        long nAuthUserId;
                        if (mnUserId == 0 & gnNonAuthUsers != 0)
                        {

                            // Note : if we are checking permissions for a page, and we're not logged in, then we shouldn't check with the gnAuthUsers group
                            // Ratehr, we should use the gnNonAuthUsers user group if it exists.

                            nAuthUserId = gnNonAuthUsers;
                            nAuthGroup = gnNonAuthUsers;
                        }

                        else if (mnUserId == 0)
                        {

                            // If no gnNonAuthUsers user group exists, then remove the auth group
                            nAuthUserId = mnUserId;
                            nAuthGroup = -1;
                        }

                        else
                        {
                            nAuthUserId = mnUserId;
                        }
                        object oPerm;

                        oPerm = GetDataValue("SELECT dbo.fxn_checkPermission (" + nPageId + ", " + nAuthUserId + "," + nAuthGroup + ") AS perm");
                        if (!(ReferenceEquals(oPerm, DBNull.Value) | oPerm is null))
                        {
                            if (Strings.InStr(Conversions.ToString(oPerm), "DENIED") > 0)
                            {
                                if (mnUserId > 0)
                                {
                                    nPageId = myWeb.gnPageAccessDeniedId;
                                }
                                else
                                {
                                    myWeb.moSession("LogonRedirect") = myWeb.mcPagePath;
                                    nPageId = myWeb.gnPageLoginRequiredId;
                                }
                            }

                            if (Operators.ConditionalCompareObjectEqual(oPerm, "VIEW by Authenticated Users", false) & mnUserId == 0)
                            {
                                myWeb.moSession("LogonRedirect") = myWeb.mcPagePath;
                                nPageId = myWeb.gnPageLoginRequiredId;
                            }

                        }
                        return (int)nPageId;
                    }
                    else
                    {
                        return (int)nPageId;
                    }

                    PerfMonLog("DBHelper", "checkPagePermission-end");
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getPageIdFromPath", ex, sProcessInfo));
                }

                return default;
            }


            public bool checkPageExist(long nPageId)
            {

                PerfMonLog("Base", "checkPageExist");

                string sProcessInfo = "";
                object nId;

                try
                {

                    nId = GetDataValue("SELECT nStructKey from tblContentStructure where nStructKey = " + nPageId);
                    if (nId is null)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "checkPageExist", ex, sProcessInfo));
                }

                return default;
            }


            public PermissionLevel getPagePermissionLevel(long nPageId)
            {

                PerfMonLog("DBHelper", "getPagePermissionLevel");

                string sProcessInfo = "";

                try
                {

                    if (myWeb.moSession is null)
                    {
                        return PermissionLevel.Denied;
                    }
                    // Check if we are Domain Super Admin
                    // RJP 7 Nov 2012. Amended to use Lower Case to prevent against case sensitive entries in Protean.Cms.Config, previously used the string "md5"
                    if (myWeb.moSession("ewAuth") == Protean.Tools.Encryption.HashString(myWeb.moSession.SessionID + goConfig("AdminPassword"), LCase(myWeb.moConfig("MembershipEncryption")), true))
                    {
                        return PermissionLevel.Full;
                    }
                    else if (checkUserRole("Administrator"))
                    {
                        return PermissionLevel.Full;
                    }
                    else
                    {
                        // Check we have access to this page
                        long nAuthUserId;
                        if (mnUserId == 0 & gnNonAuthUsers != 0)
                        {
                            nAuthUserId = gnNonAuthUsers;
                        }
                        else
                        {
                            nAuthUserId = mnUserId;
                        }
                        string sPerm;
                        string sSql = "SELECT dbo.fxn_checkPermission(" + nPageId + ", " + nAuthUserId + "," + gnAuthUsers + ") AS perm";
                        sPerm = GetDataValue(sSql);
                        if (!(ReferenceEquals(sPerm, DBNull.Value) | sPerm is null))
                        {
                            if (sPerm.Contains("ADDUPDATEOWNPUBLISH"))
                            {
                                return PermissionLevel.AddUpdateOwnPublish;
                            }
                            else if (sPerm.Contains("ADDUPDATEOWN"))
                            {
                                return PermissionLevel.AddUpdateOwn;
                            }
                            else if (sPerm.Contains("DENIED"))
                            {
                                return PermissionLevel.Denied;
                            }
                            else if (sPerm.Contains("OPEN"))
                            {
                                return PermissionLevel.Open;
                            }
                            else if (sPerm.Contains("VIEW"))
                            {
                                return PermissionLevel.View;
                            }
                            else if (sPerm.Contains("ADD"))
                            {
                                return PermissionLevel.Add;
                            }
                            else if (sPerm.Contains("UPDATEALL"))
                            {
                                return PermissionLevel.UpdateAll;
                            }
                            else if (sPerm.Contains("APPROVE"))
                            {
                                return PermissionLevel.Approve;
                            }
                            else if (sPerm.Contains("PUBLISH"))
                            {
                                return PermissionLevel.Publish;
                            }
                            else if (sPerm.Contains("FULL"))
                            {
                                return PermissionLevel.Full;
                            }
                        }
                        else
                        {
                            return default;
                        }
                    }

                    PerfMonLog("DBHelper", "getPagePermissionLevel - END");
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getPagePermissionLevel", ex, sProcessInfo));
                }

                return default;
            }


            public PermissionLevel getContentPermissionLevel(long nContentId, long nPageId)
            {

                PerfMonLog("DBHelper", "getContentPermissionLevel");


                string sProcessInfo = "";
                XmlElement oElmt;
                PermissionLevel oPerm = PermissionLevel.Denied;
                try
                {

                    oElmt = getLocationsByContentId[nContentId];

                    foreach (XmlElement oElmt2 in oElmt.SelectNodes("Location"))
                    {
                        if (oElmt2.GetAttribute("primary") == "true" & oElmt2.GetAttribute("pgid") == nPageId)
                        {
                            oPerm = getPagePermissionLevel(nPageId);
                        }
                    }

                    // if the content is orphan then get permission from homepage
                    if (oElmt.SelectNodes("Location").Count == 0)
                    {
                        oPerm = getPagePermissionLevel(myWeb.moConfig("RootPageId"));
                    }

                    return oPerm;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getContentPermissionLevel", ex, sProcessInfo));
                }

                return default;
            }


            public void DeleteXMLCache()
            {
                PerfMonLog("DBHelper", "DeleteXMLCache");
                // place holder until we get cashing going on V4
            }

            public virtual long DeleteObject(objectTypes objectType, long nId, bool bReporting = false)
            {
                PerfMonLog("DBHelper", "DeleteObject");
                string sSql;
                var nAuditId = default(long);

                bool bHaltDelete = false;


                string cProcessInfo = "";
                try
                {
                    // lets use a new  because of the cascading nature of this routine

                    // delete child items
                    switch (objectType)
                    {
                        case var @case when @case == objectTypes.CartShippingLocation:
                            {
                                string cSQL = "SELECT nLocationKey FROM tblCartShippingLocations WHERE nLocationParId = " + nId;
                                using (SqlDataReader oDataReader = getDataReaderDisposable(cSQL))
                                {
                                    while (oDataReader.Read)
                                        DeleteObject(objectTypes.CartShippingLocation, oDataReader[0]);
                                }
                                cSQL = "SELECT nAuditId FROM tblCartShippingLocations WHERE nLocationKey = " + nId;
                                DeleteObject(objectTypes.Audit, ExeProcessSqlScalar(cSQL));
                                break;
                            }
                        case var case1 when case1 == objectTypes.CartDiscountDirRelations:
                            {
                                sSql = "Select  nAuditId From tblCartDiscountDirRelations Where nDiscountDirRelationKey = " + nId;
                                DeleteObject(objectTypes.Audit, ExeProcessSqlScalar(sSql));
                                break;
                            }

                        case var case2 when case2 == objectTypes.CartDiscountProdCatRelations:
                            {
                                sSql = "Select  nAuditId From tblCartDiscountProdCatRelations Where nDiscountProdCatRelationKey = " + nId;
                                DeleteObject(objectTypes.Audit, ExeProcessSqlScalar(sSql));
                                break;
                            }

                        case var case3 when case3 == objectTypes.CartDiscountRules:
                            {
                                sSql = "Select  nAuditId From tblCartDiscountRules Where nDiscountKey = " + nId;
                                DeleteObject(objectTypes.Audit, ExeProcessSqlScalar(sSql));
                                sSql = "Select nDiscountDirRelationKey FROM tblCartDiscountDirRelations WHERE nDiscountId = " + nId;
                                using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read)
                                        DeleteObject(objectTypes.CartDiscountDirRelations, oDr[0]);
                                }

                                sSql = "Select nDiscountProdCatRelationKey FROM tblCartDiscountProdCatRelations WHERE nDiscountId = " + nId;
                                using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read)
                                        DeleteObject(objectTypes.CartDiscountProdCatRelations, oDr[0]);
                                }

                                break;
                            }
                        case var case4 when case4 == objectTypes.CartProductCategories:
                            {
                                sSql = "Select nCatProductRelKey, nAuditId From tblCartCatProductRelations Where nCatId = " + nId;
                                using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read)
                                    {
                                        DeleteObject(objectTypes.CartCatProductRelations, oDr.GetValue(0));
                                        DeleteObject(objectTypes.Audit, oDr.GetValue(1));
                                    }
                                }
                                sSql = "Select nDiscountProdCatRelationKey FROM tblCartDiscountProdCatRelations WHERE nProductCatId = " + nId;
                                using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read)
                                        DeleteObject(objectTypes.CartDiscountProdCatRelations, oDr[0]);
                                }

                                break;
                            }
                        case var case5 when case5 == objectTypes.CartCatProductRelations:
                            {
                                sSql = "Select nAuditId From tblCartCatProductRelations Where nCatProductRelKey = " + nId;
                                using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read)
                                        DeleteObject(objectTypes.Audit, oDr.GetValue(0));
                                }

                                break;
                            }
                        case var case6 when case6 == objectTypes.Content:
                            {

                                // Delete ActivityLogs for content
                                sSql = "select nActivityKey from tblActivityLog where nArtId = " + nId;
                                using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read)
                                        DeleteObject(objectTypes.ActivityLog, oDr[0]);
                                }

                                // get any locations and delete
                                sSql = "select nContentLocationKey, bPrimary from tblContentLocation where nContentId = " + nId;
                                using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read)
                                        DeleteObject(objectTypes.ContentLocation, oDr[0]);
                                }

                                // get any child results and delete
                                // sSql = "select nQResultsKey from tblQuestionaireResult where nContentId = " & nId
                                // oDr = getDataReader(sSql)
                                // While oDr.Read
                                // DeleteObject(objectTypes.QuestionaireResult, oDr(0))
                                // End While
                                // oDr.Close()
                                // oDr = Nothing

                                // delete any content relations
                                sSql = "select nContentRelationKey from tblContentRelation where nContentParentId = " + nId + " or nContentChildId = " + nId;
                                using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read)
                                        DeleteObject(objectTypes.ContentRelation, oDr[0]);
                                }

                                break;
                            }

                        case var case7 when case7 == objectTypes.ContentRelation:
                            {
                                sSql = "Select nAuditId, nContentParentID, nContentChildId From tblContentRelation Where nContentRelationKey = " + nId;
                                using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read)
                                    {
                                        sSql = "Select nContentRelationKey From tblContentRelation WHERE nContentParentId = " + oDr[2] + " AND nContentChildId = " + oDr[1];
                                        DeleteObject(objectTypes.Audit, oDr[0]);
                                    }
                                }
                                ExeProcessSql("Delete From tblContentRelation where nContentRelationKey = " + nId);
                                using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                                {
                                    var nOther = default(int);
                                    while (oDr.Read)
                                    {
                                        nOther = oDr[0];
                                        break;
                                    }
                                    if (nOther > 0)
                                        DeleteObject(objectTypes.ContentRelation, nOther);
                                }

                                break;
                            }
                        case var case8 when case8 == objectTypes.ContentStructure:
                            {

                                // Delete ActivityLogs for page
                                sSql = "select nActivityKey from tblActivityLog where nStructId = " + nId;
                                using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read)
                                        DeleteObject(objectTypes.ActivityLog, oDr[0]);
                                }

                                // delete any child pages

                                sSql = "select nStructKey from tblContentStructure where nStructParId = " + nId;
                                using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read)
                                        DeleteObject(objectTypes.ContentStructure, oDr[0]);
                                }

                                // do we want to delete any content that is not also located elsewhere???? 
                                // Add Later....

                                // get any locations and delete
                                sSql = "select nContentLocationKey, bPrimary from tblContentLocation where nStructId = " + nId;
                                using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read)
                                        // If oDr("bPrimary") = True Then
                                        // Else
                                        // bHaltDelete = True ' there is more than one location for this content don't delete it.
                                        // End If
                                        DeleteObject(objectTypes.ContentLocation, oDr[0]);
                                }

                                clearStructureCacheAll();
                                break;
                            }


                        case var case9 when case9 == objectTypes.QuestionaireResult:
                            {
                                sSql = "select nQDetailKey from tblQuestionaireResultDetail where nQResultId = " + nId;
                                using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read)
                                        DeleteObject(objectTypes.QuestionaireResultDetail, oDr[0]);
                                }

                                break;
                            }

                        case var case10 when case10 == objectTypes.Directory:
                            {

                                // Delete ActivityLogs
                                sSql = "select nActivityKey from tblActivityLog where nUserDirId = " + nId;
                                using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read)
                                        DeleteObject(objectTypes.ActivityLog, oDr[0]);
                                }

                                // Delete Permissions
                                sSql = "select nPermKey from tblDirectoryPermission where nDirId = " + nId;
                                using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read)
                                        DeleteObject(objectTypes.Permission, oDr[0]);
                                }

                                // Delete ExamResults for users
                                // sSql = "select nQResultsKey from tblQuestionaireResult where nDirId=" & nId
                                // oDr = getDataReader(sSql)
                                // While oDr.Read
                                // DeleteObject(dbHelper.objectTypes.QuestionaireResult, oDr(0))
                                // End While
                                // oDr.Close()
                                // oDr = Nothing

                                // Delete Child Directory Objects
                                sSql = "select nDirChildId from tblDirectoryRelation where nDirParentId=" + nId;
                                using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read)
                                        DeleteObject(dbHelper.objectTypes.Directory, oDr[0]);
                                }

                                // Delete Relationships
                                sSql = "select nRelKey from tblDirectoryRelation where nDirParentId = " + nId + " or nDirChildId = " + nId;
                                using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read)
                                        DeleteObject(objectTypes.DirectoryRelation, oDr[0]);
                                }

                                break;
                            }

                        case var case11 when case11 == objectTypes.DirectoryRelation:
                            {

                                // if we are linking a user to a company we need to also delete from depts / training groups that are also in that company.

                                // get the parent dir object
                                sSql = "select d.nDirKey, d.cDirSchema, dr.nDirChildId from tblDirectoryRelation dr inner join tblDirectory d on d.nDirKey = dr.nDirParentId where nRelKey=" + nId;
                                var nParentId = default(long);
                                var nChildId = default(long);
                                string cDirSchema = "";
                                using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read)
                                    {
                                        nParentId = oDr("nDirKey");
                                        nChildId = oDr("nDirChildId");
                                        cDirSchema = oDr("cDirSchema");
                                    }
                                }
                                if (cDirSchema == "Company")
                                {
                                    // select all of the children of nParentId that have relations with our child
                                    sSql = "select cr.nRelKey from tblDirectoryRelation dr " + "inner join tblDirectory pd on pd.nDirKey = dr.nDirParentId  " + "inner join tblDirectory cd on cd.nDirKey = dr.nDirChildId  " + "inner join tblDirectoryRelation cr on cd.nDirKey = cr.nDirParentId  " + "where dr.nDirParentId = " + nParentId + " " + "and cr.nDirChildId = " + nChildId;
                                    using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                                    {
                                        // delete links between the target object and the children of the parent object.
                                        while (oDr.Read)
                                            DeleteObject(objectTypes.DirectoryRelation, oDr[0]);
                                    }

                                }

                                break;
                            }
                        case var case12 when case12 == objectTypes.CartItem:
                            {
                                sSql = "Select  nAuditID from tblCartItem WHERE nCartItemKey = " + nId;

                                using (var oDr = getDataReaderDisposable(sSql))
                                {
                                    var nCrtItmAdtId = default(int);
                                    while (oDr.Read)
                                        // DeleteObject(objectTypes.Audit, oDr.GetValue(0))
                                        nCrtItmAdtId = oDr.GetValue(0);
                                    DeleteObject(objectTypes.Audit, nCrtItmAdtId);
                                }
                                // options
                                sSql = "Select nCartItemKey from tblCartItem WHERE nParentID = " + nId;
                                using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read)
                                        DeleteObject(objectTypes.CartItem, oDr.GetValue(0));
                                }
                                ExeProcessSql("Delete from tblCartItem where nCartItemKey = " + nId);
                                break;
                            }


                        case var case13 when case13 == objectTypes.CartOrder:
                            {
                                // cart items
                                sSql = "Select nCartItemKey from tblCartItem WHERE nCartOrderID = " + nId;
                                using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read)
                                        DeleteObject(objectTypes.CartItem, oDr.GetValue(0));
                                }
                                // contacts
                                sSql = "Select nContactKey from tblCartContact WHERE nContactCartID = " + nId;
                                using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read)
                                        DeleteObject(objectTypes.CartContact, oDr.GetValue(0));

                                }
                                sSql = "Select nAuditId from tblCartOrder WHERE nCartOrderKey = " + nId;
                                using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                                {
                                    var nCrtOrdAdtId = default(int);
                                    while (oDr.Read)
                                        // DeleteObject(objectTypes.Audit, oDr.GetValue(0))
                                        nCrtOrdAdtId = oDr.GetValue(0);
                                    ExeProcessSql("Delete from tblCartOrder where nCartOrderKey = " + nId);
                                    DeleteObject(objectTypes.Audit, nCrtOrdAdtId);
                                }

                                break;
                            }

                        case var case14 when case14 == objectTypes.CartContact:
                            {
                                sSql = "Select nAuditId from tblCartContact WHERE nContactKey = " + nId;
                                using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read)
                                    {
                                        if (!(oDr.GetValue(0) is DBNull))
                                        {
                                            DeleteObject(objectTypes.Audit, oDr.GetValue(0));
                                        }
                                    }
                                    ExeProcessSql("Delete from tblCartContact where nContactKey = " + nId);
                                }

                                break;
                            }
                        case var case15 when case15 == objectTypes.Subscription:
                            {
                                var oXML = new XmlDocument();
                                sSql = "SELECT cSubscriptionXML, nUserId FROM tblSubscription WHERE nSubKey = " + nId;
                                var nSubUserId = default(int);
                                using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read)
                                    {
                                        oXML.InnerXml = Strings.Replace(Strings.Replace(oDr[0], "&gt;", ">"), "&lt;", "<");
                                        nSubUserId = oDr[1];
                                    }
                                }
                                XmlElement oElmt = oXML.DocumentElement.SelectSingleNode("descendant-or-self::UserGroups");

                                sSql = "SELECT cSubXML FROM tblSubscription WHERE nDirId = " + nSubUserId + " AND (NOT (nSubKey = " + nId + "))";
                                DataSet oDS = myWeb.moDbHelper.GetDataSet(sSql, "Content");
                                var oXML2 = new XmlDocument();
                                oXML2.InnerXml = Strings.Replace(Strings.Replace(oDS.GetXml, "&gt;", ">"), "&lt;", "<");

                                foreach (XmlElement oGrpElmt in oElmt.SelectNodes("Group[@id!='']"))
                                {
                                    bool bDelete = true;
                                    foreach (XmlElement oTmpElmt in oXML2.DocumentElement.SelectNodes("Content/cSubscriptionXML/Content/UserGroups/Group[@id=" + oGrpElmt.GetAttribute("id") + "]"))
                                    {
                                        bDelete = false;
                                        break;
                                    }
                                    if (bDelete)
                                    {
                                        sSql = "SELECT nRelKey FROM tblDirectoryRelation WHERE (nDirChildId = " + nSubUserId + ") AND (nDirParentId = " + oGrpElmt.GetAttribute("id") + ")";
                                        using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                                        {
                                            while (oDr.Read)
                                                DeleteObject(objectTypes.DirectoryRelation, oDr.GetValue(0));
                                        }

                                    }
                                }

                                sSql = "Select nAuditId From tblSubscription where nSubKey = " + nId;
                                using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read)
                                        DeleteObject(objectTypes.Audit, oDr.GetValue(0));
                                    ExeProcessSql("Delete from tblSubscription where nSubKey = " + nId);
                                }

                                break;
                            }
                        case var case16 when case16 == objectTypes.CartShippingMethod:
                            {
                                sSql = "SELECT nShpRelKey FROM tblCartShippingRelations WHERE nShpOptId = " + nId;
                                using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read)
                                        DeleteObject(objectTypes.CartShippingRelations, oDr.GetValue(0));
                                    ExeProcessSql("DELETE FROM tblCartShippingMethods WHERE nShipOptKey = " + nId);
                                }

                                break;
                            }
                    }
                    if (bReporting)
                    {
                        goResponse.Write("<p>Deleted from " + getTable(objectType) + " - " + nId + "</p>");
                    }

                    if (!bHaltDelete)
                    {

                        if (!(objectType == objectTypes.QuestionaireResultDetail) & !(objectType == objectTypes.ActivityLog) & !(objectType == objectTypes.Audit))
                        {
                            // delete the audit ref
                            sSql = "select nAuditId from " + getTable(objectType) + " where " + getKey(objectType) + " = " + nId;
                            using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                            {
                                while (oDr.Read)
                                {
                                    if (Information.IsNumeric(oDr[0]))
                                    {
                                        nAuditId = oDr[0];
                                    }

                                }
                            }
                            if (nAuditId > 0L)
                            {
                                sSql = "delete from tblAudit where nAuditKey = " + nAuditId;
                                ExeProcessSql(sSql);
                            }

                        }

                        // delete the main item
                        sSql = "delete from " + getTable(objectType) + " where " + getKey(objectType) + " = " + nId;
                        ExeProcessSql(sSql);


                    }

                    return nId;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "DeleteObject", ex, cProcessInfo));
                }

                return default;

            }



            /// <summary>

            ///         ''' Delete content and associated relationships by ID.

            ///         ''' Bulk method to try and emulate DeleteObject without the bit by bit iteration

            ///         ''' The items that will be deleted are:

            ///         ''' the content, any locations for this content, any relations for this content, the related audit records for all three

            ///         ''' </summary>

            ///         ''' <param name="contentIDList">A List(Of String) of the IDs to delete</param>

            ///         ''' <remarks></remarks>
            protected internal void BulkContentDelete(List<string> contentIDList)
            {
                string methodName = "BulkContentDelete(List(Of String))";
                string processInfo = string.Empty;
                try
                {
                    BulkContentDelete(string.Join(",", contentIDList.ToArray()));
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, methodName, ex, processInfo));
                }
            }

            /// <summary>
            ///         ''' Delete content and associated relationships by ID.
            ///         ''' Bulk method to try and emulate DeleteObject without the bit by bit iteration
            ///         ''' The items that will be deleted are:
            ///         ''' the content, any locations for this content, any relations for this content, the related audit records for all three
            ///         ''' </summary>
            ///         ''' <param name="contentIDCSVList">The content IDs to delete as a comma separated list</param>
            ///         ''' <remarks>I'm not decided on deleting orphan related content.</remarks>
            protected internal void BulkContentDelete(string contentIDCSVList)
            {
                string methodName = "BulkContentDelete(String)";
                string processInfo = string.Empty;
                int deletedRecords = 0;
                PerfMonLog(mcModuleName, methodName);

                try
                {

                    // Validate the list as a list of numbers
                    if (Regex.IsMatch(contentIDCSVList, @"^\d+(,\d+)*$"))
                    {

                        // Delete the locations for this content
                        deletedRecords = this.ExeProcessSql("DELETE tblAudit FROM tblAudit INNER JOIN tblContentLocation ON nAuditId = nAuditKey AND nContentId IN (" + contentIDCSVList + ")");
                        if (deletedRecords > 0)
                            this.ExeProcessSql("DELETE FROM tblContentLocation WHERE nContentId IN (" + contentIDCSVList + ")");

                        // Delete the relatnios for this content
                        deletedRecords = this.ExeProcessSql("DELETE tblAudit FROM tblAudit INNER JOIN tblContentRelation ON nAuditId = nAuditKey AND (nContentChildId IN (" + contentIDCSVList + ") OR nContentParentId IN (" + contentIDCSVList + "))");
                        if (deletedRecords > 0)
                            this.ExeProcessSql("DELETE  FROM tblContentRelation WHERE nContentChildId IN (" + contentIDCSVList + ") OR nContentParentId IN (" + contentIDCSVList + ")");

                        // Delete the actual content
                        deletedRecords = this.ExeProcessSql("DELETE tblAudit FROM tblAudit INNER JOIN tblContent ON nAuditId = nAuditKey AND nContentKey IN (" + contentIDCSVList + ")");
                        if (deletedRecords > 0)
                            this.ExeProcessSql("DELETE  FROM tblContent WHERE nContentKey IN (" + contentIDCSVList + ")");
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, methodName, ex, processInfo));
                }
            }


            public void DeleteAllObjects(objectTypes objectType, bool bReporting = false)
            {
                PerfMonLog("DBHelper", "DeleteAllObjects");
                string sSql;

                string cProcessInfo = "";
                try
                {
                    switch (objectType)
                    {
                        case 3 // tblContentStructure
                       :
                            {
                                DeleteAllObjects(objectTypes.ContentLocation);
                                break;
                            }
                    }

                    sSql = "select " + getKey(objectType) + " from " + getTable(objectType);

                    using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {
                        if (bReporting)
                            goResponse.Write("Deleting: - " + getTable(objectType));

                        while (oDr.Read)
                            DeleteObject(objectType, oDr(0), bReporting);
                        while (oDr.Read)
                            DeleteObject(objectType, oDr(0), bReporting);
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "DeleteAllObjects", ex, cProcessInfo));
                }
            }

            public virtual string getObjectInstance(objectTypes ObjectType, long nId = -1, string sWhere = "")
            {
                PerfMonLog("DBHelper", "getObjectInstance");

                string sSql;
                DataSet oDs;
                XmlNode oNode;
                XmlElement oElmt;
                // Dim  As New 
                string sContent;
                string sInstance = "";
                DataRow oRow;
                DataColumn oColumn;
                string sNewVal = "";

                string cProcessInfo = "";
                try
                {
                    // Dim oXml As XmlDocument = New XmlDocument


                    if (sWhere == "")
                        sSql = "select * from " + getTable(ObjectType) + " left outer join tblAudit a on nAuditId = a.nAuditKey where " + getKey(ObjectType) + " = " + nId;
                    else
                        sSql = "select TOP 1 * from " + getTable(ObjectType) + " inner join tblAudit a on nAuditId = a.nAuditKey " + sWhere;
                    if (ObjectType == objectTypes.ScheduledItem)
                        sSql = "select * from " + getTable(ObjectType) + " where " + getKey(ObjectType) + " = " + nId;


                    cProcessInfo = "running: " + sSql;

                    oDs = GetDataSet(sSql, getTable(ObjectType), "instance");
                    ReturnNullsEmpty(oDs);

                    XmlDocument oXml;
                    if (nId > 0)
                        oXml = GetXml(oDs);
                    else
                    {
                        XmlDataDocument oNewXml = new XmlDataDocument(oDs);
                        oXml = oNewXml;
                    }


                    // 
                    oDs.EnforceConstraints = false;

                    // Convert any text to xml
                    foreach (var oNode in oXml.SelectNodes("/instance/" + getTable(ObjectType) + "/*"))
                    {

                        // ignore for passwords
                        if (!oNode.Name == "cDirPassword")
                        {
                            oElmt = oNode;
                            sContent = oElmt.InnerText;
                            try
                            {
                                oElmt.InnerXml = sContent;
                            }
                            catch
                            {
                                // run tidy...
                                oElmt.InnerXml = tidyXhtmlFrag(sContent, true, false);
                            }
                            // empty empty dates
                            if (oElmt.InnerXml.StartsWith("0001-01-01T00:00:00"))
                                oElmt.InnerXml = "";
                        }

                        if (ObjectType == objectTypes.Directory)
                        {
                            // fix for status of -1
                            if (oNode.Name == "nStatus")
                            {
                                oElmt = oNode;
                                sContent = oElmt.InnerText;
                                if (sContent == "-1")
                                    oElmt.InnerText = "1";
                            }
                        }
                    }
                    if (!oXml.SelectSingleNode("instance") == null)
                        sInstance = oXml.SelectSingleNode("instance").InnerXml;
                    else
                    {
                        // we could be clever here and step through the dataset to build an empty instance?
                        oRow = oDs.Tables(getTable(ObjectType)).NewRow;
                        oElmt = oXml.CreateElement(getTable(ObjectType));
                        foreach (var oColumn in oDs.Tables(getTable(ObjectType)).Columns)
                        {
                            // set a default value for status.
                            if (oColumn.ToString == "nStatus")
                            {
                                switch (ObjectType)
                                {
                                    case object _ when objectTypes.CartShippingMethod:
                                        {
                                            sNewVal = "1"; // active
                                            break;
                                        }

                                    default:
                                        {
                                            sNewVal = "0"; // in-active
                                            break;
                                        }
                                }
                            }
                            else if (!IsDBNull(oColumn.DefaultValue))
                                sNewVal = convertDtSQLtoXML(oColumn.DataType, oColumn.DefaultValue);
                            else
                                sNewVal = "";
                            addNewTextNode(oColumn.ToString, oElmt, sNewVal, true, false); // always force this
                        }
                        oXml.AppendChild(oElmt);
                        sInstance = oXml.InnerXml;
                    }

                    oXml = null/* TODO Change to default(_) if this is not a reference type */;
                    // oDs.Clear()
                    oDs = null/* TODO Change to default(_) if this is not a reference type */;

                    return sInstance;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getObjectInstance", ex, cProcessInfo));
                    return "";
                }
            }

            public bool IsAuditedObjectLive(objectTypes objectType, long objectKey)
            {
                try
                {
                    string query = "SELECT " + getKey(objectType) + " FROM " + getTable(objectType) + " o INNER JOIN dbo.tblAudit a ON o.nauditId = a.nauditKey "
                                       + "WHERE " + myWeb.GetStandardFilterSQLForContent(false);
                    return GetDataValue(query, null/* Conversion error: Set to default value for this argument */, null/* Conversion error: Set to default value for this argument */, 0) > 0;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            public bool IsChildPage(objectTypes pageid, long objectKey)
            {
                try
                {
                    string query = "select * from tblcontentstructure P inner join tblcontentstructure C on p.nStructKey = C.nStructParId where p.nStructKey =" + objectKey;
                    return GetDataValue(query, null/* Conversion error: Set to default value for this argument */, null/* Conversion error: Set to default value for this argument */, 0) > 0;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            public bool isCascade(long ContentId)
            {
                PerfMonLog("DBHelper", "isCascade");
                long nCount;
                string sSql;
                string cProcessInfo = "";
                try
                {
                    sSql = "select count(*) from tblContentLocation where nContentId=" + ContentId + " and bCascade = 1";

                    nCount = GetDataValue(sSql);

                    if (nCount > 0)
                        return true;
                    else
                        return false;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "isCascade", ex, cProcessInfo));
                }
            }

            public virtual string setObjectInstance(objectTypes ObjectType, XmlElement oInstance = null/* TODO Change to default(_) if this is not a reference type */, long nKey = -1)
            {
                PerfMonLog("DBHelper", "setObjectInstance", ObjectType.ToString);


                // EonicWeb Specific Function for updating DB Entities, manages creation and updating of Audit table

                XmlDocument oXml;  // Change XmlDataDocument to XmlDocument
                XmlElement oElmt = null/* TODO Change to default(_) if this is not a reference type */;
                XmlElement oElmt2;
                long nAuditId;
                XmlElement oTableNode;

                string cProcessInfo = "";
                long nVersionId = 0;

                bool bSaveInstance = true;

                try
                {

                    // if we have not been given an instance we'll create one, this makes it easy to update the audit table by just supplying an id
                    if (oInstance == null)
                    {
                        oXml = new XmlDocument();  // Change XmlDataDocument to XmlDocument
                        oElmt = oXml.CreateElement("instance");
                        oXml.AppendChild(oElmt);
                        oTableNode = oXml.CreateElement(getTable(ObjectType));
                        oXml.FirstChild.AppendChild(oTableNode);
                        oElmt2 = oXml.CreateElement(getKey(ObjectType));
                        oElmt2.InnerText = nKey;
                        oElmt.AppendChild(oElmt2);
                        oInstance = oXml.DocumentElement;
                    }
                    else
                        oTableNode = oInstance.SelectSingleNode(getTable(ObjectType));
                    if (getTable(ObjectType) == "tblAudit")
                    {
                        // we get the first actual element.
                        foreach (var oElmt in oInstance.SelectNodes("*"))
                        {
                            if (oTableNode == null)
                            {
                                if (oElmt.NodeType == XmlNodeType.Element)
                                    oTableNode = oElmt;
                            }
                        }
                    }

                    // lets get the key from the instance if not supplied
                    if (nKey == -1)
                    {
                        oElmt = oInstance.SelectSingleNode("*/" + getKey(ObjectType));
                        if (!oElmt == null)
                        {
                            if (IsNumeric(oElmt.InnerText))
                                nKey = System.Convert.ToInt64(oElmt.InnerText);
                        }
                    }

                    // also lets add the key to the instance because were going to need it for saveInstanc
                    if (nKey > 0)
                        addNewTextNode(getKey(ObjectType), oInstance.SelectSingleNode("*"), nKey, true, true);

                    restart:
                    ;

                    // Special case: contentversion (when setting audit)
                    if (oTableNode == null & ObjectType == objectTypes.ContentVersion)
                        oTableNode = oInstance.SelectSingleNode(getTable(objectTypes.Content));



                    // Process for Updates or Inserts
                    if (nKey > 0)
                    {
                        // and now we update the audit table
                        cProcessInfo = "Updating Object Type: " + ObjectType + "(Table: " + getTable(ObjectType) + ", Object Id: " + nKey;
                        switch (ObjectType)
                        {
                            case objectTypes.Content:
                                {
                                    XmlElement oPrimId = oInstance.SelectSingleNode("tblContent/nContentPrimaryId");
                                    if (oPrimId.InnerText == "")
                                        oPrimId.InnerText = "0";
                                    XmlElement oVerId = oInstance.SelectSingleNode("tblContent/nVersion");
                                    if (oVerId.InnerText == "")
                                        oVerId.InnerText = "0";
                                    if (gbVersionControl)
                                    {
                                        // out to a subroutine for versioning

                                        nVersionId = contentVersioning(ref oInstance, ref ObjectType, ref nKey, ref bSaveInstance);
                                        if (ObjectType == objectTypes.ContentVersion)
                                            goto restart;
                                        nAuditId = tidyAuditId(ref oInstance, ObjectType, nKey);
                                    }
                                    else
                                    {

                                        // TEST FOR 0 AUDIT ID AND RECREATE
                                        if (Tools.Xml.NodeState(oInstance, "descendant-or-self::nAuditId") == Tools.Xml.XmlNodeState.HasContents)
                                        {
                                            XmlElement oAuditElmt = oInstance.SelectSingleNode("descendant-or-self::nAuditId");
                                            if (IsNumeric(oAuditElmt.InnerText()))
                                                nAuditId = System.Convert.ToInt64(oAuditElmt.InnerText());
                                            if (nAuditId == 0)
                                            {
                                                if (myWeb == null)
                                                    nAuditId = getAuditId(1, 0, "");
                                                else
                                                    nAuditId = getAuditId(1, myWeb.mnUserId, "");
                                                oInstance.SelectSingleNode("descendant-or-self::nAuditId").InnerText = nAuditId;
                                            }
                                        }

                                        nAuditId = tidyAuditId(ref oInstance, ObjectType, nKey);
                                    }

                                    break;
                                }

                            case objectTypes.ContentVersion:
                            case objectTypes.ContentStructure:
                            case objectTypes.Directory:
                            case objectTypes.CourseResult:
                            case objectTypes.CartOrder:
                            case objectTypes.CartItem:
                            case objectTypes.CartContact:
                            case objectTypes.CartShippingLocation:
                            case objectTypes.CartShippingMethod:
                            case objectTypes.CartCatProductRelations:
                            case objectTypes.CartDiscountDirRelations:
                            case objectTypes.CartDiscountProdCatRelations:
                            case objectTypes.CartDiscountRules:
                            case objectTypes.CartProductCategories:
                            case objectTypes.Codes:
                            case objectTypes.QuestionaireResult:
                            case objectTypes.CourseResult:
                            case objectTypes.Certificate:
                            case objectTypes.CpdLog:
                            case objectTypes.QuestionaireResultDetail:
                            case objectTypes.Lookup:
                            case objectTypes.CartCarrier:
                            case objectTypes.CartDelivery:
                            case objectTypes.Subscription:
                            case objectTypes.SubscriptionRenewal:
                            case objectTypes.CartPaymentMethod:
                            case objectTypes.indexkey:
                                {

                                    // 
                                    // Check for Audit Id - if not found, we should be able to retrieve one from the database.
                                    nAuditId = tidyAuditId(ref oInstance, ObjectType, nKey);
                                    break;
                                }

                            case objectTypes.Audit:
                                {
                                    // Check for insert fields - if they exist, then we need to get rid of them
                                    // removeChildByName(oTableNode, "dInsertDate")
                                    // removeChildByName(oTableNode, "nInsertDirId")

                                    // TS not sure about this it seems the logic to set this is further on.

                                    // Update the update fields.
                                    bool forceUpdate = false;
                                    XmlElement updateDateNode = oInstance.SelectSingleNode("*/dUpdateDate");
                                    if (updateDateNode != null)
                                    {
                                        if (updateDateNode.GetAttribute("force") == "true")
                                            forceUpdate = true;
                                    }
                                    if (forceUpdate == false)
                                        addNewTextNode("dUpdateDate", oTableNode, Protean.Tools.Xml.XmlDate(DateTime.Now(), true), true, true);
                                    addNewTextNode("nUpdateDirId", oTableNode, mnUserId, true, true); // always force this
                                    break;
                                }

                            default:
                                {
                                    break;
                                }
                        }

                        // Check the auditId is populated in the instnace
                        XmlElement auditIdElmt = oInstance.SelectSingleNode(getTable(ObjectType) + "/nAuditId");
                        if (!auditIdElmt == null)
                        {
                            if (auditIdElmt.InnerText == "")
                                auditIdElmt.InnerText = nAuditId;
                        }
                    }
                    else
                    {
                        cProcessInfo = "Inserting Object Type: " + ObjectType + "(Table: " + getTable(ObjectType) + ", New Object";
                        switch (ObjectType)
                        {
                            case objectTypes.Content:
                                {
                                    // To be readjusted
                                    if (gbVersionControl)
                                    {
                                        // out to a subroutine for versioning
                                        contentVersioning(ref oInstance, ref ObjectType);
                                        if (ObjectType == objectTypes.ContentVersion)
                                            goto restart;
                                    }

                                    // we are using getAuditId to create a new audit record.
                                    nAuditId = setObjectInstance(objectTypes.Audit, oInstance);
                                    addNewTextNode("nAuditId", oTableNode, nAuditId, true, true);
                                    break;
                                }

                            case objectTypes.Content:
                            case objectTypes.ContentStructure:
                            case objectTypes.Directory:
                            case objectTypes.CartOrder:
                            case objectTypes.CartItem:
                            case objectTypes.CartContact:
                            case objectTypes.CartShippingLocation:
                            case objectTypes.CartShippingMethod:
                            case objectTypes.CartCatProductRelations:
                            case objectTypes.CartDiscountDirRelations:
                            case objectTypes.CartDiscountProdCatRelations:
                            case objectTypes.CartDiscountRules:
                            case objectTypes.CartProductCategories:
                            case objectTypes.QuestionaireResult:
                            case objectTypes.QuestionaireResultDetail:
                            case objectTypes.CourseResult:
                            case objectTypes.Codes:
                            case objectTypes.ContentVersion:
                            case objectTypes.Certificate:
                            case objectTypes.CpdLog:
                            case objectTypes.Lookup:
                            case objectTypes.CartCarrier:
                            case objectTypes.CartDelivery:
                            case objectTypes.Subscription:
                            case objectTypes.SubscriptionRenewal:
                            case objectTypes.CartPaymentMethod:
                            case objectTypes.indexkey:
                                {

                                    // we are using getAuditId to create a new audit record.
                                    nAuditId = setObjectInstance(objectTypes.Audit, oInstance);
                                    addNewTextNode("nAuditId", oTableNode, nAuditId, true, true);
                                    break;
                                }

                            case objectTypes.Audit:
                                {
                                    // add logic for inserting default audit fields.
                                    // addNewTextNode("dPublishDate", oInstance.FirstChild, xmlDate(Now()), True, False)

                                    // if this is supplied we want to keep it, mostly it will be blank
                                    if (oInstance.FirstChild.SelectSingleNode("dInsertDate") == null)
                                        addNewTextNode("dInsertDate", oTableNode, Protean.Tools.Xml.XmlDate(DateTime.Now(), true), true, true); // always force this
                                    else if (oInstance.FirstChild.SelectSingleNode("dInsertDate").InnerText == "")
                                        oInstance.FirstChild.SelectSingleNode("dInsertDate").InnerText = Protean.Tools.Xml.XmlDate(DateTime.Now(), true);

                                    if (oInstance.FirstChild.SelectSingleNode("nInsertDirId") == null)
                                        addNewTextNode("nInsertDirId", oTableNode, mnUserId, true, true); // always force this
                                    else if (oInstance.FirstChild.SelectSingleNode("nInsertDirId").InnerText == "")
                                        oInstance.FirstChild.SelectSingleNode("nInsertDirId").InnerText = mnUserId;


                                    bool forceUpdate = false;
                                    XmlElement updateDateNode = oInstance.SelectSingleNode("tblContent/dUpdateDate");
                                    if (updateDateNode != null)
                                    {
                                        if (updateDateNode.GetAttribute("force") == "true")
                                            forceUpdate = true;
                                    }
                                    if (forceUpdate == false)
                                        addNewTextNode("dUpdateDate", oTableNode, Protean.Tools.Xml.XmlDate(DateTime.Now(), true), true, true);

                                    addNewTextNode("nUpdateDirId", oTableNode, mnUserId, true, true); // always force this
                                    if (oInstance.SelectSingleNode("descendant-or-self::nStatus") == null)
                                        addNewTextNode("nStatus", oTableNode, "1", true, true);
                                    else
                                    {
                                        if (oInstance.SelectSingleNode("descendant-or-self::nStatus").InnerText == "")
                                            addNewTextNode("nStatus", oTableNode, "1", true, true);
                                        addNewTextNode("nStatus", oTableNode, "1", true, false);
                                    }

                                    break;
                                }
                        }
                    }

                    cProcessInfo = "Saving instance";

                    PerfMonLog("DBHelper", "setObjectInstance", "startsave");


                    if (bSaveInstance)
                        nKey = saveInstance(oInstance, getTable(ObjectType), getKey(ObjectType));
                    else
                        nKey = nVersionId;

                    PerfMonLog("DBHelper", "setObjectInstance", "endsave");

                    if (ObjectType == objectTypes.ContentStructure)
                        clearStructureCacheAll();

                    return nKey;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "setObjectInstance", ex, cProcessInfo));
                    return "";
                }
            }

            protected long tidyAuditId(ref XmlElement oInstance, objectTypes objectType, long nKey)
            {
                string cProcessInfo = "";
                long nAuditId;
                try
                {

                    // Check for Audit Id - if not found, we should be able to retrieve one from the database.
                    nAuditId = 0;

                    // First check the node (exists and is numeric)
                    if (Tools.Xml.NodeState(oInstance, "descendant-or-self::nAuditId") == Tools.Xml.XmlNodeState.HasContents)
                    {
                        XmlElement oElmt = oInstance.SelectSingleNode("descendant-or-self::nAuditId");
                        if (IsNumeric(oElmt.InnerText()))
                            nAuditId = System.Convert.ToInt64(oElmt.InnerText());
                    }

                    // If not set then try getting the value from the DB
                    if (!(nAuditId > 0))
                        nAuditId = this.GetDataValue("SELECT nAuditId FROM " + getTable(objectType) + " WHERE " + getKey(objectType) + "=" + nKey, null/* Conversion error: Set to default value for this argument */, null/* Conversion error: Set to default value for this argument */, 0);
                    XmlElement oTableNode = oInstance.FirstChild;

                    // if this is supplied we want to keep it, mostly it will be blank
                    if (oTableNode.SelectSingleNode("dInsertDate") == null)
                        addNewTextNode("dInsertDate", oTableNode, Protean.Tools.Xml.XmlDate(DateTime.Now(), true), true, true); // always force this
                    else if (oInstance.FirstChild.SelectSingleNode("dInsertDate").InnerText == "")
                        addNewTextNode("dInsertDate", oTableNode, Protean.Tools.Xml.XmlDate(DateTime.Now(), true), true, true);// always force this

                    if (oTableNode.SelectSingleNode("nInsertDirId") == null)
                        addNewTextNode("nInsertDirId", oTableNode, mnUserId, true, true); // always force this
                    else if (oInstance.FirstChild.SelectSingleNode("nInsertDirId").InnerText == "")
                        addNewTextNode("nInsertDirId", oTableNode, mnUserId, true, true);// always force this

                    // Set the Audit instance
                    if (nAuditId > 0)
                        setObjectInstance(objectTypes.Audit, oInstance, nAuditId);

                    return nAuditId;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "tidyAuditId", ex, cProcessInfo));
                    return "";
                }
            }
            #region "DB Methods: Version Control"
            protected void contentVersioning(ref XmlElement oInstance, ref objectTypes ObjectType, ref long nKey = 0, ref bool bSaveInstance = true)
            {
                string cProcessInfo = "";
                XmlElement oElmt = null/* TODO Change to default(_) if this is not a reference type */;
                XmlElement oOrigInstance;
                long nCurrentVersionNumber;
                long nNewVersionNumber = 0;
                Status nStatus;
                string cSql = "";
                long nVersionId = 0;

                try
                {

                    // first we look at the status of the content being sumitted.

                    nStatus = getNodeValueByType(oInstance, "//nStatus", dataType.TypeNumber, 1);
                    nCurrentVersionNumber = getNodeValueByType(oInstance, "//nVersion", dataType.TypeNumber, 0);

                    // Get the maximum version number
                    if (nKey > 0)
                    {
                        cSql = "SELECT MAX(nVersion) As MaxV "
        + "FROM ( "
        + "       SELECT nVersion FROM dbo.tblContent WHERE nContentKey = " + nKey
        + "       UNION "
        + "       SELECT nVersion FROM dbo.tblContentVersions WHERE nContentPrimaryId = " + nKey
        + ") VersionList";
                        nNewVersionNumber = GetDataValue(cSql, null/* Conversion error: Set to default value for this argument */, null/* Conversion error: Set to default value for this argument */, 0) + 1;
                    }

                    oInstance.SelectSingleNode("//nVersion").InnerText = nNewVersionNumber.ToString();

                    // If the status is live or hidden, then check the users page permissions
                    // At the moment assume that if we are in admin mode, we can skip this
                    if (!myWeb == null)
                    {
                        if (!(myWeb.mbAdminMode))
                        {
                            // Check the permissions
                            // Problem at the moment ascertaining the parent page for orphan content, 
                            // This is already acheived in Web.GetAjaxXml so we pass through a value to dbHelper

                            if (!CanPublish(CurrentPermissionLevel))
                            {

                                // The permission level inspected is not a publishing level, so this needs to be set as pending.
                                nStatus = Status.Pending;

                                // Check if the status node exists
                                if (NodeState(oInstance, "//nStatus") == Tools.Xml.XmlNodeState.NotInstantiated)
                                {
                                    // Status node doesn't exist - we have to assume that it's going to be added to the first child of oInstance (i.e. instance/tablename/nstatus)
                                    XmlElement oTable = oInstance.FirstChild;
                                    oTable.AppendChild(oTable.OwnerDocument.CreateElement("nStatus"));
                                }

                                oInstance.SelectSingleNode("//nStatus").InnerText = nStatus;
                            }
                        }

                        switch (nStatus)
                        {
                            case object _ when Status.Live:
                            case object _ when Status.Hidden:
                                {

                                    // If this is not new then get the current version and commit it to the version table
                                    if (nKey > 0)
                                    {
                                        // create a copy of the origional in versions and save live
                                        oOrigInstance = moPageXml.CreateElement("instance");
                                        oOrigInstance.InnerXml = getObjectInstance(objectTypes.Content, nKey);

                                        nVersionId = setNewContentVersionInstance(ref oOrigInstance, nKey);

                                        // If this was a pending item, supercede the copy in the version table (ie superceded anything that's pending)
                                        string cPreviousStatus = "";
                                        if (NodeState(oInstance, "currentStatus", "", "", null/* TODO Change to default(_) if this is not a reference type */, null/* TODO Change to default(_) if this is not a reference type */, "", cPreviousStatus) == XmlNodeState.HasContents)
                                        {
                                            if (cPreviousStatus == "3" | cPreviousStatus == "4")
                                                // Update everything with a status of Pending to be DraftSuperceded
                                                ExeProcessSql("UPDATE tblAudit SET nStatus = " + Status.Superceded + " FROM tblAudit a INNER JOIN tblContentVersions c ON c.nAuditId = a.nAuditKey AND c.nContentPrimaryId = " + nKey + " AND (a.nStatus = " + Status.Pending + " or a.nStatus = " + Status.InProgress + ")");
                                        }
                                    }

                                    break;
                                }

                            case object _ when Status.InProgress // PREVIEW
                     :
                                {
                                    nVersionId = setNewContentVersionInstance(ref oInstance, nKey, Status.InProgress);
                                    // ?how do we stop the origional updating?
                                    bSaveInstance = false;
                                    break;
                                }

                            case object _ when Status.Pending:
                                {

                                    // Pending works in a number of ways
                                    // - New content is created in the content table, not the versions table
                                    // - Existing content is created in the versions table
                                    // - For existing content, if the current Content (nKey) is Live or Hidden then save Pending instance to versions
                                    // - If the current Content (nKey) is Pending or Rejected then move current to Versions and save Pending instance to Content.

                                    // First get the current live, if applicable.
                                    if (nKey > 0)
                                    {
                                        long cParentId = nKey;
                                        // Get the current live
                                        oOrigInstance = moPageXml.CreateElement("instance");
                                        oOrigInstance.InnerXml = getObjectInstance(objectTypes.Content, nKey);


                                        // Assess the status
                                        Status nLiveStatus = getNodeValueByType(oOrigInstance, "//nStatus", dataType.TypeNumber);
                                        switch (nLiveStatus)
                                        {
                                            case object _ when Status.Live:
                                            case object _ when Status.Hidden:
                                                {
                                                    // Leave the live content alone, set the pending content as a version
                                                    ObjectType = objectTypes.ContentVersion;
                                                    prepareContentVersionInstance(ref oInstance, nKey, nStatus);
                                                    nKey = 0;
                                                    break;
                                                }

                                            default:
                                                {

                                                    // The LIVE content is pending, which means that this should be moved into the version 
                                                    // and the submitted content goes into the content table.
                                                    nVersionId = setNewContentVersionInstance(ref oOrigInstance, nKey);
                                                    break;
                                                }
                                        }

                                        // Update everything with a status of Pending to be DraftSuperceded
                                        ExeProcessSql("UPDATE tblAudit SET nStatus = " + Status.DraftSuperceded + " FROM tblAudit a INNER JOIN tblContentVersions c ON c.nAuditId = a.nAuditKey AND c.nContentPrimaryId = " + cParentId + " AND (a.nStatus = " + Status.Pending + " or a.nStatus = " + Status.InProgress + ")");
                                    }
                                    else
                                    {
                                    }

                                    break;
                                }

                            case object _ when Status.Rejected:
                            case object _ when Status.Superceded:
                                {
                                    // remove key
                                    ObjectType = objectTypes.ContentVersion;
                                    break;
                                }
                        }
                    }

                    return nVersionId;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "contentVersioning", ex, cProcessInfo));
                }
            }

            private void prepareContentVersionInstance(ref XmlElement oInstance, long nContentPrimaryId, Status nStatus = Status.Superceded)
            {
                PerfMonLog("DBHelper", "prepareContentVersionInstance");
                string cProcessInfo = "ContentParId = " + nContentPrimaryId;

                try
                {
                    // Set the reference to the parent content
                    oInstance.SelectSingleNode("descendant-or-self::nContentPrimaryId").InnerText = nContentPrimaryId;

                    // Empty the audit references to create a new one
                    oInstance.SelectSingleNode("descendant-or-self::nAuditId").InnerText = "";

                    if (!oInstance.SelectSingleNode("descendant-or-self::nAuditKey") == null)
                    {
                        // don't need this if we don't have the related audit feilds.

                        oInstance.SelectSingleNode("descendant-or-self::nAuditKey").InnerText = "";
                        // Change the publish date on the version to the last updated date for the content we are archiving
                        oInstance.SelectSingleNode("descendant-or-self::dInsertDate").InnerText = oInstance.SelectSingleNode("descendant-or-self::dUpdateDate").InnerText;
                    }

                    // Set the status
                    oInstance.SelectSingleNode("descendant-or-self::nStatus").InnerText = nStatus;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "prepareContentVersionInstance", ex, cProcessInfo));
                }
            }

            private long setNewContentVersionInstance(ref XmlElement oInstance, long nContentPrimaryId, Status nStatus = Status.Superceded)
            {
                PerfMonLog("DBHelper", "setNewContentVersionInstance");
                string cProcessInfo = "ContentParId = " + nContentPrimaryId;
                long nVersionId = 0;
                try
                {
                    // Prepare the instance
                    prepareContentVersionInstance(ref oInstance, nContentPrimaryId, nStatus);

                    // Save the intance
                    nVersionId = this.setObjectInstance(objectTypes.ContentVersion, oInstance, 0);

                    return nVersionId;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "setNewContentVersionInstance", ex, cProcessInfo));
                    return default(Long);
                }
            }

            public XmlElement GetVersionInstance(long nContentPrimaryId, long nContentVersionId)
            {
                string cProcessInfo = "ContentParId = " + nContentPrimaryId;
                XmlElement oTempInstance = moPageXml.CreateElement("instance");
                try
                {
                    // grab some values from the live version
                    oTempInstance.InnerXml = getObjectInstance(dbHelper.objectTypes.Content, nContentPrimaryId);
                    long nAuditId = oTempInstance.SelectSingleNode("tblContent/nAuditKey").InnerText;
                    long nVersion = oTempInstance.SelectSingleNode("tblContent/nVersion").InnerText;
                    long nStatus = oTempInstance.SelectSingleNode("tblContent/nStatus").InnerText;
                    string sInsertDate = oTempInstance.SelectSingleNode("tblContent/dInsertDate").InnerText;
                    string sInsertUser = oTempInstance.SelectSingleNode("tblContent/nInsertDirId").InnerText;

                    // pull the content in from the versions table
                    oTempInstance.InnerXml = getObjectInstance(dbHelper.objectTypes.ContentVersion, nContentVersionId);
                    // change to match
                    Protean.Tools.Xml.renameNode(oTempInstance.SelectSingleNode("tblContentVersions"), "tblContent");
                    Protean.Tools.Xml.renameNode(oTempInstance.SelectSingleNode("tblContent/nContentVersionKey"), "nContentKey");
                    // update some of the values
                    oTempInstance.SelectSingleNode("tblContent/nContentKey").InnerText = nContentPrimaryId;
                    oTempInstance.SelectSingleNode("tblContent/nAuditKey").InnerText = nAuditId;
                    oTempInstance.SelectSingleNode("tblContent/nAuditId").InnerText = nAuditId;
                    oTempInstance.SelectSingleNode("tblContent/nVersion").InnerText = nVersion;
                    oTempInstance.SelectSingleNode("tblContent/dInsertDate").InnerText = sInsertDate;
                    oTempInstance.SelectSingleNode("tblContent/nInsertDirId").InnerText = sInsertUser;
                    oTempInstance.SelectSingleNode("tblContent/nStatus").InnerText = nStatus;

                    return oTempInstance;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetVersionInstance", ex, cProcessInfo));
                    return null/* TODO Change to default(_) if this is not a reference type */;
                }
            }

            public long contentStatus(long nContentPrimaryId, long nContentVersionId, Status nStatus = Status.Live)
            {
                PerfMonLog("DBHelper", "setNewContentVersionInstance");
                string cProcessInfo = "ContentParId = " + nContentPrimaryId;
                long nVersionId = 0;
                try
                {
                    XmlElement oTempInstance = GetVersionInstance(nContentPrimaryId, nContentVersionId);

                    oTempInstance.SelectSingleNode("tblContent/nStatus").InnerText = nStatus;

                    // Save the intance
                    nVersionId = this.setObjectInstance(objectTypes.Content, oTempInstance, 0);

                    // Superceed previous versions
                    string cPreviousStatus = "";
                    // If NodeState(oTempInstance, "currentStatus", , , , , , cPreviousStatus) = XmlNodeState.HasContents Then
                    // If cPreviousStatus = "3" Or cPreviousStatus = "4" Then
                    // Update everything with a status of Pending to be DraftSuperceded
                    ExeProcessSql("UPDATE tblAudit SET nStatus = " + Status.Superceded + " FROM tblAudit a INNER JOIN tblContentVersions c ON c.nAuditId = a.nAuditKey AND c.nContentPrimaryId = " + nContentPrimaryId + " AND (a.nStatus = " + Status.Pending + " or a.nStatus = " + Status.InProgress + ")");
                    // End If
                    // End If

                    return nVersionId;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "setNewContentVersionInstance", ex, cProcessInfo));
                    return default(Long);
                }
            }


            public XmlElement getPendingContent(bool bGetContentSinceLastLogged = false)
            {
                PerfMonLog("DBHelper", "getPendingContent");
                string cProcessInfo = "";
                XmlElement pendingList = null/* TODO Change to default(_) if this is not a reference type */;

                try
                {
                    string cSql = "";
                    string sContent = "";
                    string dLastRun = "";
                    string cFilterSql = "";

                    DataSet oDS = new DataSet();

                    // Add the filter
                    if (bGetContentSinceLastLogged)
                    {
                        dLastRun = GetDataValue("SELECT TOP 1 dDateTime FROM dbo.tblActivityLog WHERE nActivityType=" + ActivityType.PendingNotificationSent + " ORDER BY 1 DESC", null/* Conversion error: Set to default value for this argument */, null/* Conversion error: Set to default value for this argument */, "");
                        if (!(string.IsNullOrEmpty(dLastRun)) && Information.IsDate(dLastRun))
                            cFilterSql = " WHERE Last_Updated > " + SqlDate(dLastRun, true);
                    }

                    // Get the pending content

                    oDS = myWeb.moDbHelper.GetDataSet("SELECT * FROM vw_VersionControl_GetPendingContent" + cFilterSql, "Pending", "GenericReport");

                    if (oDS.Tables.Count > 0 && oDS.Tables(0).Rows.Count > 0)
                    {

                        // Get the Locations content
                        // myWeb.moDbHelper.addTableToDataSet(oDS, "SELECT	nContentId AS id, nStructKey AS pageid, cStructName AS page FROM dbo.tblContentLocation l INNER JOIN dbo.tblContentStructure s ON l.nStructId = s.nStructKey WHERE bPrimary=1", "Location")
                        // oDS.Relations.Add("PendingLocations", oDS.Tables("Pending").Columns("id"), oDS.Tables("Location").Columns("id"), False)
                        // oDS.Relations("PendingLocations").Nested = True

                        // Get the Related content
                        // myWeb.moDbHelper.addTableToDataSet(oDS, "SELECT	nContentParentId AS keyid, nContentKey AS id, cContentName AS name, cContentSchemaName AS type FROM	dbo.tblContentRelation r INNER JOIN dbo.tblContent c ON c.nContentKey = r.nContentChildId", "Content")
                        // oDS.Relations.Add("PendingRelations", oDS.Tables("Pending").Columns("id"), oDS.Tables("Content").Columns("keyid"), False)
                        // oDS.Relations("PendingRelations").Nested = True

                        // Map the attributes
                        {
                            var withBlock = oDS.Tables("Pending");
                            withBlock.Columns("status").ColumnMapping = System.Data.MappingType.Attribute;
                            withBlock.Columns("id").ColumnMapping = System.Data.MappingType.Attribute;
                            withBlock.Columns("versionid").ColumnMapping = System.Data.MappingType.Attribute;
                            withBlock.Columns("userid").ColumnMapping = System.Data.MappingType.Attribute;
                            withBlock.Columns("username").ColumnMapping = System.Data.MappingType.Attribute;
                            withBlock.Columns("currentLiveVersion").ColumnMapping = System.Data.MappingType.Attribute;
                            withBlock.Columns("pageid").ColumnMapping = System.Data.MappingType.Attribute;
                            withBlock.Columns("page").ColumnMapping = System.Data.MappingType.Attribute;
                            withBlock.Columns("ContentId").ColumnMapping = System.Data.MappingType.Attribute;
                            withBlock.Columns("type").ColumnMapping = System.Data.MappingType.Attribute;
                        }

                        // With oDS.Tables("Location")
                        // .Columns("id").ColumnMapping = MappingType.Hidden
                        // .Columns("pageid").ColumnMapping = Data.MappingType.Attribute
                        // .Columns("page").ColumnMapping = Data.MappingType.Attribute
                        // End With

                        // With oDS.Tables("Content")
                        // .Columns("keyid").ColumnMapping = MappingType.Hidden
                        // .Columns("id").ColumnMapping = Data.MappingType.Attribute
                        // .Columns("type").ColumnMapping = Data.MappingType.Attribute
                        // .Columns("name").ColumnMapping = Data.MappingType.Attribute
                        // End With


                        oDS.EnforceConstraints = false;
                        myWeb.moDbHelper.ReturnNullsEmpty(oDS);

                        // convert to Xml Dom
                        XmlDataDocument oXml = new XmlDataDocument(oDS);
                        oXml.PreserveWhitespace = false;

                        pendingList = moPageXml.CreateElement("Content");
                        pendingList.SetAttribute("name", "Content Awaiting Approval");
                        pendingList.SetAttribute("type", "Report");
                        if (!(string.IsNullOrEmpty(dLastRun)) && Information.IsDate(dLastRun))
                            pendingList.SetAttribute("since", xmlDateTime(dLastRun));
                        pendingList.InnerXml = oXml.InnerXml;

                        // Tidy Up - Get rid of all that orphan content from the relations
                        foreach (XmlNode oOrphan in pendingList.FirstChild.SelectNodes("*[name()!='Pending']"))
                            pendingList.FirstChild.RemoveChild(oOrphan);


                        // Tidy Up XMl Nodes
                        foreach (XmlElement oElmt in pendingList.SelectNodes("//*[local-name()='UserXml' or local-name()='ContentXml']"))

                            Tools.Xml.SetInnerXmlThenInnerText(oElmt, oElmt.InnerText);

                        // Tidy Up - Move all Locations and Relations into a Metadata Node
                        XmlElement oLocations = null/* TODO Change to default(_) if this is not a reference type */;
                        XmlElement oRelations = null/* TODO Change to default(_) if this is not a reference type */;
                        XmlElement Metadata = null/* TODO Change to default(_) if this is not a reference type */;

                        foreach (XmlElement oPending in pendingList.SelectNodes("//Pending"))
                        {
                            Metadata = oPending.OwnerDocument.CreateElement("Metadata");

                            oLocations = oPending.OwnerDocument.CreateElement("Locations");
                            Metadata.AppendChild(oLocations);
                            foreach (XmlElement oLocation in oPending.SelectNodes("Location"))
                            {
                                // oLocations.AppendChild(pendingList.ImportNode(oLocation, True))
                                oPending.RemoveChild(oLocation);
                                oLocations.AppendChild(oLocation);
                            }

                            oRelations = oPending.OwnerDocument.CreateElement("Related");
                            Metadata.AppendChild(oRelations);
                            foreach (XmlElement oRelation in oPending.SelectNodes("Content"))
                            {
                                // oRelations.AppendChild(pendingList.ImportNode(oRelation, True))
                                oPending.RemoveChild(oRelation);
                                oRelations.AppendChild(oRelation);
                            }

                            oPending.AppendChild(Metadata);
                        }
                    }

                    // Log the activity
                    if (bGetContentSinceLastLogged)
                        CommitLogToDB(ActivityType.PendingNotificationSent, myWeb.mnUserId, "", DateTime.Now(), null/* Conversion error: Set to default value for this argument */, null/* Conversion error: Set to default value for this argument */, null/* Conversion error: Set to default value for this argument */, true);

                    return pendingList;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getPendingContent", ex, cProcessInfo));
                    return null/* TODO Change to default(_) if this is not a reference type */;
                }
            }

            #endregion


            public XmlElement createFakeInstance(objectTypes objectType, XmlElement oValueElmt = default)
            {
                PerfMonLog("DBHelper", "createFakeInstance");
                XmlElement oElmt3 = moPageXml.CreateElement(getTable(objectType));
                if (oValueElmt is not null)
                {
                    oElmt3.AppendChild(oValueElmt);
                }
                XmlElement oElmt4 = moPageXml.CreateElement("instance");
                oElmt4.AppendChild(oElmt3);
                return oElmt4;

            }

            public virtual XmlElement getGroupsInstance(long nUserId, long nParId = -1)
            {
                PerfMonLog("DBHelper", "getGroupsInstance");
                string cProcessInfo = "";
                try
                {
                    // Dim oXml As XmlDocument = New XmlDocument
                    string sSql;
                    DataSet oDs;
                    XmlElement oGrpElmt;
                    XmlElement oElmt;


                    sSql = "execute getUsersCompanyAllParents @UserId=" + nUserId + ", @ParId=" + nParId;
                    oDs = GetDataSet(sSql, "group", "instance");

                    oGrpElmt = moPageXml.CreateElement("groups");

                    if (oDs.Tables(0).Rows.Count > 0)
                    {
                        ReturnNullsEmpty(oDs);

                        oDs.Tables(0).Columns(0).ColumnMapping = System.Data.MappingType.Attribute;
                        oDs.Tables(0).Columns(1).ColumnMapping = System.Data.MappingType.Attribute;
                        oDs.Tables(0).Columns(2).ColumnMapping = System.Data.MappingType.Attribute;
                        oDs.Tables(0).Columns(3).ColumnMapping = System.Data.MappingType.Attribute;

                        XmlDocument oXml = GetXml(oDs);

                        oDs.EnforceConstraints = false;
                        foreach (XmlNode oNode in oXml.DocumentElement.SelectNodes("group"))
                        {
                            oElmt = oNode;
                            if ("0" + oElmt.GetAttribute("isMember") > 0)
                            {
                                oElmt.SetAttribute("isMember", "true");
                            }
                            else
                            {
                                oElmt.SetAttribute("isMember", "false");
                            }
                        }
                        oGrpElmt.InnerXml = oXml.SelectSingleNode("instance").InnerXml;
                    }

                    return oGrpElmt;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getGroupsInstance", ex, cProcessInfo));
                    return default;
                }

            }

            public long getObjectByRef(objectTypes objectType, string cForeignRef, string cSchemaType = "")
            {
                string cProcName = "getObjectByRef (ObjectTypes,String,[String])";
                // PerfMonLog("DBHelper", cProcName)
                string nId = 0.ToString();
                string cProcessInfo = "";
                try
                {

                    string cTableName = getTable(objectType);
                    string cTableKey = getKey(objectType);
                    string cTableFRef = getFRef(objectType);

                    nId = getObjectByRef(cTableName, cTableKey, cTableFRef, objectType, cForeignRef, cSchemaType).ToString();
                    return Conversions.ToLong(nId);
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, cProcName, ex, cProcessInfo));

                }

                return default;

            }

            public long getObjectByRef(string cTableName, string cTableKey, string cTableFRef, objectTypes objectType, string cForeignRef, string cSchemaType = "")
            {
                string cProcName = "getObjectByRef (String,String,String,ObjectTypes,String,[String])";
                // PerfMonLog("DBHelper", cProcName)
                string sSql = "";
                string nId = 0.ToString();
                // Dim oDr As SqlDataReader


                // Some failsafes
                if (string.IsNullOrEmpty(cTableName))
                {
                    cTableName = getTable(objectType);
                }
                if (string.IsNullOrEmpty(cTableKey))
                {
                    cTableKey = getKey(objectType);
                }
                if (string.IsNullOrEmpty(cTableFRef))
                {
                    cTableFRef = getFRef(objectType);
                }



                string cProcessInfo = "";
                try
                {
                    if (string.IsNullOrEmpty(cSchemaType))
                    {
                        sSql = "select " + cTableKey + " from " + cTableName + " where " + cTableFRef + " = '" + SqlFmt(cForeignRef) + "'";
                    }
                    // sSql = "select " & getKey(objectType) & " from " & getTable(objectType) & " where " & getFRef(objectType) & " = '" & SqlFmt(cForeignRef) & "'"
                    else
                    {
                        switch (objectType)
                        {
                            case var @case when @case == objectTypes.Content:
                                {
                                    sSql = "select " + cTableKey + " from " + cTableName + " where " + cTableFRef + " = '" + SqlFmt(cForeignRef) + "' and cContentSchemaName='" + cSchemaType + "'";
                                    break;
                                }
                            // sSql = "select " & getKey(objectType) & " from " & getTable(objectType) & " where " & getFRef(objectType) & " = '" & SqlFmt(cForeignRef) & "' and cContentSchemaName='" & cSchemaType & "'"
                            case var case1 when case1 == objectTypes.Directory:
                                {
                                    sSql = "select " + cTableKey + " from " + cTableName + " where " + cTableFRef + " = '" + SqlFmt(cForeignRef) + "' and cDirSchema='" + cSchemaType + "'";
                                    break;
                                }
                                // sSql = "select " & getKey(objectType) & " from " & getTable(objectType) & " where " & getFRef(objectType) & " = '" & SqlFmt(cForeignRef) & "' and cDirSchema='" & cSchemaType & "'"
                        }
                    }

                    using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {
                        if (oDr is null)
                            return 0L;
                        while (oDr.Read)
                            nId = oDr[0];

                        return Conversions.ToLong(nId);
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, cProcName, ex, cProcessInfo));

                }

                return default;

            }

            public string[] getObjectsByRef(objectTypes objectType, string cForiegnRef, string cSchemaType = "")
            {
                PerfMonLog("DBHelper", "getObjectByRef");
                string sSql = "";
                string nIds = "";
                // Dim oDr As SqlDataReader


                string cProcessInfo = "";
                try
                {
                    if (string.IsNullOrEmpty(cSchemaType))
                    {
                        sSql = "select " + getKey(objectType) + " from " + getTable(objectType) + " where " + getFRef(objectType) + " = '" + SqlFmt(cForiegnRef) + "'";
                    }
                    else
                    {
                        switch (objectType)
                        {
                            case var @case when @case == objectTypes.Content:
                                {
                                    sSql = "select " + getKey(objectType) + " from " + getTable(objectType) + " where " + getFRef(objectType) + " = '" + SqlFmt(cForiegnRef) + "' and cContentSchemaName='" + cSchemaType + "'";
                                    break;
                                }
                            case var case1 when case1 == objectTypes.Directory:
                                {
                                    sSql = "select " + getKey(objectType) + " from " + getTable(objectType) + " where " + getFRef(objectType) + " = '" + SqlFmt(cForiegnRef) + "' and cDirSchema='" + cSchemaType + "'";
                                    break;
                                }
                        }
                    }

                    using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {
                        if (oDr is null)
                            return null;
                        while (oDr.Read)
                        {
                            if (!string.IsNullOrEmpty(nIds))
                                nIds += ",";
                            nIds += oDr[0];
                        }

                        return Strings.Split(nIds, ",");
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getObjectByRef", ex, cProcessInfo));
                    return null;
                }

            }
            public long setObjectFRef(objectTypes objectType, long id, string cForeignRef)
            {
                string cProcName = "setObjectFRef (ObjectTypes,Int,[String])";
                PerfMonLog("DBHelper", cProcName);
                string sSql = "";
                long nRowAff = 0L;
                string cProcessInfo = "";

                try
                {

                    string cTableName = getTable(objectType);
                    string cTableKey = getKey(objectType);
                    string cTableFRef = getFRef(objectType);

                    // SQL to save the fref value
                    switch (objectType)
                    {
                        case var @case when @case == objectTypes.Content:
                            {
                                sSql = "update " + cTableName + " set " + cTableFRef + " = '" + cForeignRef + "' where " + cTableKey + " = " + id;
                                break;
                            }
                    }

                    nRowAff = ExeProcessSql(sSql);
                    return nRowAff;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, cProcName, ex, cProcessInfo));

                }

                return default;

            }
            public int getAuditId(int nStatus = 1, long nDirId = 0L, string cDescription = "", object dPublishDate = null, object dExpireDate = null, object dInsertDate = null, object dUpdateDate = null)
            {
                PerfMonLog("DBHelper", "getAuditId");
                string sSql;
                int nId;
                long nUserId;

                if (nDirId == 0L)
                {
                    nUserId = mnUserId;
                }
                else
                {
                    nUserId = nDirId;
                }

                string cProcessInfo = "";
                try
                {
                    if (dInsertDate is null)
                        dInsertDate = DateTime.Now;
                    if (dUpdateDate is null)
                        dUpdateDate = DateTime.Now;

                    // sSql = "insert into tblAudit (dPublishDate, dExpireDate, dInsertDate, nInsertDirId, dUpdateDate, nUpdateDirId, nStatus, cDescription)" & _
                    // " Values (" & _
                    // sqlDate(dPublishDate) & _
                    // ", " & sqlDate(dExpireDate) & _
                    // "," & sqlDateTime(dInsertDate, dInsertDate.Hour.ToString & ":" & dInsertDate.Minute.ToString & ":" & dInsertDate.Second.ToString) & ", " & _
                    // nUserId & "," & sqlDate(dUpdateDate) & ", " & nUserId & "," & nStatus & ", '" & cDescription & "')"
                    sSql = "insert into tblAudit (dPublishDate, dExpireDate, dInsertDate, nInsertDirId, dUpdateDate, nUpdateDirId, nStatus, cDescription)" + " Values (" + Protean.Tools.Database.SqlDate(dPublishDate) + ", " + Protean.Tools.Database.SqlDate(dExpireDate) + "," + Protean.Tools.Database.SqlDate(dInsertDate, true) + ", " + nUserId + "," + Protean.Tools.Database.SqlDate(dUpdateDate, true) + ", " + nUserId + "," + nStatus + ", '" + cDescription + "')";
                    // Protean.Tools.Database.SqlDate
                    nId = GetIdInsertSql(sSql);

                    return nId;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getAuditId", ex, cProcessInfo));

                }

                return default;
            }


            public bool checkContentLocationsInCurrentMenu(long contentId, bool checkRelatedIfOrphan = false)
            {

                PerfMonLog("DBHelper", "checkContentLocationsInCurrentMenu");
                string processInfo = "";

                try
                {
                    processInfo = "contentId=" + contentId;

                    bool foundLocation = false;
                    XmlElement argContentNode = null;
                    var locations = getLocationsByContentId(contentId, ContentNode: ref argContentNode);
                    XmlElement menu = moPageXml.SelectSingleNode("/Page/Menu");

                    if (locations is not null & menu is not null)
                    {


                        // See if any of the locations for this content exist in the current page menu
                        foreach (XmlElement location in locations.SelectNodes("//Location"))
                        {

                            if (menu.SelectSingleNode("//MenuItem[@id=" + location.GetAttribute("pgid") + "]") is not null)
                            {
                                foundLocation = true;
                                break;
                            }
                        }

                        // If nothing was found and checkRelatedIfOrphan is flagged up, then find related (parent) content and search that as well
                        if (!foundLocation & checkRelatedIfOrphan)
                        {

                            XmlElement relations = getRelationsByContentId(contentId, contentRelationType: RelationType.Child);
                            foreach (XmlElement relation in relations.SelectNodes("//Relation"))
                            {
                                foundLocation = checkContentLocationsInCurrentMenu(relation.GetAttribute("relatedContentId"));
                                if (foundLocation)
                                    break;
                            }



                        }

                    }

                    return foundLocation;
                }


                catch (Exception ex)
                {

                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "checkContentLocationsInCurrentMenu", ex, processInfo));
                    return false;

                }

            }


            public XmlNode getRelationsByContentId(long contentId, [Optional, DefaultParameterValue(default)] ref XmlElement contentNode, RelationType contentRelationType = RelationType.Child | RelationType.Parent)
            {
                PerfMonLog("DBHelper", "getRelationsByContentId");
                string sqlQuery = "";
                string sqlFilter = "";
                DataSet ds;
                XmlElement returnElmt;

                string processInfo = "";
                try
                {

                    // Set the relationship direction filter
                    if ((contentRelationType & RelationType.Parent) != 0)
                        sqlFilter += "nContentParentId = " + contentId;
                    if ((contentRelationType & RelationType.Parent & RelationType.Child) != 0)
                        sqlFilter += " OR ";
                    if ((contentRelationType & RelationType.Child) != 0)
                        sqlFilter += "nContentChildId = " + contentId;

                    // Creat the full statement - this gets rid of duplicates and indicates how many relationships exist in each example
                    sqlQuery = "SELECT relatedContentId, " + "SUM(CASE WHEN relatedContentRelation='child' THEN 1 ELSE 0 END) AS childRelation, " + "SUM(CASE WHEN relatedContentRelation='parent' THEN 1 ELSE 0 END) As parentRelation	 " + "FROM ( " + "SELECT	CASE WHEN nContentChildId = " + contentId + " THEN nContentParentId ELSE nContentChildId END AS relatedContentId, " + "CASE WHEN nContentChildId = " + contentId + " THEN 'parent' ELSE 'child' END As relatedContentRelation " + "FROM tblContentRelation  " + "WHERE " + sqlFilter + ") r " + "GROUP BY relatedContentId ";








                    ds = GetDataSet(sqlQuery, "Relation", "Content");
                    ds.Tables(0).Columns("relatedContentId").ColumnMapping = System.Data.MappingType.Attribute;
                    ds.Tables(0).Columns("childRelation").ColumnMapping = System.Data.MappingType.Attribute;
                    ds.Tables(0).Columns("parentRelation").ColumnMapping = System.Data.MappingType.Attribute;
                    ds.EnforceConstraints = false;
                    var dsXml = new XmlDataDocument(ds);
                    ds = default;


                    if (contentNode is null)
                    {
                        returnElmt = moPageXml.CreateElement("Content");
                    }
                    else
                    {
                        returnElmt = contentNode;
                    }

                    foreach (XmlElement relation in dsXml.SelectNodes("Content/Relation"))
                        returnElmt.AppendChild(moPageXml.ImportNode(relation, true));

                    return returnElmt;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getRelationsByContentId", ex, processInfo));

                    return default;
                }

            }


            public XmlNode getLocationsByContentId(long nContentId, [Optional, DefaultParameterValue(default)] ref XmlElement ContentNode)
            {
                PerfMonLog("DBHelper", "getLocationsByContentId");
                string sSql;
                string nId = 0.ToString();
                DataSet oDs;
                XmlElement oElmt;


                string cProcessInfo = "";
                try
                {
                    sSql = "select nStructId as [pgid], bPrimary as [primary], cPosition as [position] from tblContentLocation where nContentId = " + nContentId;

                    oDs = GetDataSet(sSql, "Location", "Content");
                    oDs.Tables(0).Columns("pgid").ColumnMapping = System.Data.MappingType.Attribute;
                    oDs.Tables(0).Columns("primary").ColumnMapping = System.Data.MappingType.Attribute;
                    oDs.Tables(0).Columns("position").ColumnMapping = System.Data.MappingType.Attribute;

                    oDs.EnforceConstraints = false;
                    var oXml = new XmlDataDocument(oDs);

                    oDs = default;
                    if (ContentNode is null)
                    {
                        oElmt = moPageXml.CreateElement("Content");
                    }
                    else
                    {
                        oElmt = ContentNode;
                    }
                    foreach (XmlElement oLocElmt in oXml.SelectNodes("Content/Location"))
                        oElmt.AppendChild(moPageXml.ImportNode(oLocElmt, true));

                    return oElmt;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getLocationsByContentId", ex, cProcessInfo));

                    return default;
                }

            }

            public virtual void updatePagePosition(long nPageId, long nContentId, string sPosition, bool reorder = true)
            {
                PerfMonLog("DBHelper", "updateLocations");
                string sSql;
                string cProcessInfo = "";
                try
                {

                    sSql = "UPDATE tblContentLocation SET cPosition = '" + SqlFmt(sPosition) + "' WHERE (nStructId = " + nPageId + ") AND (nContentId = " + nContentId + ")";
                    ExeProcessSql(sSql);

                    // sSql = "UPDATE tblContent SET cContentName = '" & SqlFmt(sPosition) & "' WHERE (nContentKey = " & nContentId & ")"
                    // ExeProcessSql(sSql)
                    if (reorder)
                    {
                        ReorderContent(nPageId, nContentId, "MoveTop", default, sPosition);
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "updatePagePosition", ex, cProcessInfo));
                }
            }

            public virtual void updateLocations(long nContentId, string sLocations)
            {
                string cProcessInfo = "";
                try
                {
                    updateLocations(nContentId, sLocations, "");
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "updateLocations", ex, cProcessInfo));
                }
            }


            public virtual void updateLocations(long nContentId, string sLocations, string sPosition)
            {
                PerfMonLog("DBHelper", "updateLocations");
                string sSql;
                string[] nLoc;
                long i;
                string cProcessInfo = "";
                DataSet oDs;
                var nLocations = new Hashtable();
                try
                {
                    // delete the historic locations
                    // sSql = "delete from tblContentLocation where nContentId = " & nContentId & " and bPrimary = 0"
                    // ExeProcessSql(sSql)

                    // If sLocations <> "" Then
                    // nLoc = Split(sLocations, ",", , CompareMethod.Binary)
                    // For i = 0 To nLoc.Length - 1
                    // Me.setContentLocation(CLng(nLoc(i)), nContentId, False, False)
                    // Next
                    // End If

                    sSql = "select * from tblContentLocation where nContentId = " + nContentId;
                    oDs = GetDataSet(sSql, "tblContentLocation");
                    if (!string.IsNullOrEmpty(sLocations))
                    {
                        nLoc = Strings.Split(sLocations, ",", Compare: CompareMethod.Binary);
                        var loopTo = (long)(nLoc.Length - 1);
                        for (i = 0L; i <= loopTo; i++)
                            nLocations.Add(Conversions.ToInteger(nLoc[(int)i]), nLoc[(int)i]);
                    }

                    foreach (DataRow oRow in oDs.Tables(0).Rows)
                    {
                        if (nLocations.Contains(oRow("nStructId")))
                        {
                            // ignoring existing ones
                            nLocations.Remove(oRow("nStructId"));
                        }
                        // deleting removed ones
                        else if (oRow("bPrimary") == false)
                        {
                            this.DeleteObject(objectTypes.ContentLocation, oRow("nContentLocationKey"));
                        }
                    }

                    foreach (DictionaryEntry keypair in nLocations)
                        // adding new ones
                        this.setContentLocation(keypair.Value, nContentId, false, false, false, sPosition, false);
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "updateLocations", ex, cProcessInfo));
                }
            }

            public bool updateLocationsDetail(long nContentId, int nLocation, bool bPrimary)
            {
                PerfMonLog("DBHelper", "updateLocationsDetail");
                string cProcessInfo = "";
                try
                {

                    string cSQL;
                    // first we need to  check wif we are removing the only primary id
                    if (!bPrimary)
                    {
                        cSQL = "SELECT nContentLocationKey FROM tblContentLocation WHERE (NOT nStructId = " + nLocation + ") AND (nContentId = " + nContentId + ") AND  (bPrimary = 1)";
                        cProcessInfo = cSQL;
                        bool bResult = false;
                        using (SqlDataReader oDRE = getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                        {
                            if (oDRE.HasRows)
                                bResult = true;
                            oDRE.Close();
                            if (!bResult)
                                return false;
                        }
                    }
                    cSQL = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("UPDATE tblContentLocation SET bPrimary = ", Interaction.IIf(bPrimary, 1, 0)), " WHERE (nStructId = "), nLocation), ") AND (nContentId = "), nContentId), ")"));
                    cProcessInfo = cSQL;
                    ExeProcessSqlScalar(cSQL);
                    return true;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "updateLocationsDetail", ex, cProcessInfo));
                    return false;
                }
            }

            public void updateLocationsWithScope(long nContentId, string sLocations, string cInscopeLocations = "")
            {
                string sSql;
                string[] nLoc;
                long i;
                string cProcessInfo = "updateLocationsWithScope";
                var bSetPrimary = default(bool);
                try
                {
                    // Delete the current locations for this piece of content.
                    // We need to preserve any existing locations that are still required as they will have ordering information.
                    if (LCase(myWeb.moConfig("AllowContentLocationsSetPrimary")) == "on")
                    {
                        bSetPrimary = true;
                    }
                    // First, set the general delete statement
                    if (bSetPrimary)
                    {
                        sSql = "delete from tblContentLocation where nContentId = " + nContentId;
                    }
                    else
                    {
                        sSql = "delete from tblContentLocation where nContentId = " + nContentId + " and bPrimary = 0";

                    }

                    // Only delete locations within a scope (e.g. if only dealing with a partial list of locations)
                    if (!string.IsNullOrEmpty(cInscopeLocations))
                    {
                        sSql += " and nStructId IN (" + cInscopeLocations + ")";
                    }

                    // Preserve any existing and required locations
                    if (!string.IsNullOrEmpty(sLocations))
                    {
                        sSql += " and NOT(nStructId IN (" + sLocations + "))";
                    }

                    // Delete the locations
                    this.ExeProcessSql(sSql);

                    // Update / add the new locations
                    if (!string.IsNullOrEmpty(sLocations))
                    {
                        nLoc = Strings.Split(sLocations, ",", Compare: CompareMethod.Binary);
                        var loopTo = (long)(nLoc.Length - 1);
                        for (i = 0L; i <= loopTo; i++)
                        {
                            if (i == 0L & bSetPrimary)
                            {
                                // First location will be the primary
                                this.setContentLocation(Conversions.ToLong(nLoc[(int)i]), nContentId, true, false);
                            }
                            else
                            {
                                this.setContentLocation(Conversions.ToLong(nLoc[(int)i]), nContentId, false, false);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "updateLocationsWithScope", ex, cProcessInfo));
                }
            }

            public void updateShippingLocations(long nOptId, string sLocations)
            {
                PerfMonLog("DBHelper", "updateShippingLocations");
                string sSql;
                string[] nLoc;
                long i;
                DataSet oDs;
                string cProcessInfo = "";
                try
                {
                    // delete the historic locations

                    sSql = "select nShpRelKey from tblCartShippingRelations where nShpOptId = " + nOptId;
                    oDs = GetDataSet(sSql, "tblCartShippingRelations");
                    foreach (DataRow oRow in oDs.Tables(0).Rows)
                        this.DeleteObject(objectTypes.CartShippingRelations, oRow("nShpRelKey"));

                    oDs = default;


                    if (!string.IsNullOrEmpty(sLocations))
                    {
                        nLoc = Strings.Split(sLocations, ",", Compare: CompareMethod.Binary);
                        var loopTo = (long)(nLoc.Length - 1);
                        for (i = 0L; i <= loopTo; i++)
                            this.insertShippingLocation(nOptId, Conversions.ToLong(nLoc[(int)i]), false);
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "updateLocations", ex, cProcessInfo));

                }
            }

            public XmlNode getUserQuizInstance(long nQuizId)
            {
                PerfMonLog("DBHelper", "getUserQuizInstance");
                // Dim oDr As SqlDataReader
                XmlNode oQuizXml = default;
                XmlElement oElmt;
                object dDateTaken;
                long nTimeTaken;
                string cProcessInfo = "";
                try
                {
                    // oDr = getDataReader("SELECT nContentId, cQResultsXml,nQResultsTimeTaken FROM tblQuestionaireResult r WHERE nQResultsKey=" & CStr(nQuizId))
                    using (SqlDataReader oDr = getDataReaderDisposable("SELECT nContentId, cQResultsXml,nQResultsTimeTaken FROM tblQuestionaireResult r WHERE nQResultsKey=" + nQuizId.ToString()))  // Done by nita on 6/7/22
                    {
                        if (oDr.Read)
                        {
                            oQuizXml = moPageXml.CreateElement("container");
                            oQuizXml.InnerXml = oDr.Item("cQResultsXml");
                            oQuizXml = oQuizXml.SelectSingleNode("instance");
                            // lets add the contentId of the xFormQuiz to the results set.
                            oElmt = oQuizXml.SelectSingleNode("descendant-or-self::results");
                            oElmt.SetAttribute("contentId", oDr.Item("nContentId"));
                            // Add the time taken
                            oElmt = oQuizXml.SelectSingleNode("//status/timeTaken");
                            if (oDr.Item("nQResultsTimeTaken") is DBNull)
                                nTimeTaken = 0L;
                            else
                                nTimeTaken = oDr.Item("nQResultsTimeTaken");
                            if (oElmt is null)
                            {
                                addNewTextNode("timeTaken", oQuizXml.SelectSingleNode("//status"), nTimeTaken);
                            }
                            else
                            {
                                oElmt.InnerText = nTimeTaken;
                            }
                        }

                    }

                    // let's add the date taken
                    dDateTaken = GetDataValue("SELECT a.dInsertDate FROM tblAudit a INNER JOIN tblQuestionaireResult q ON a.nAuditKey = q.nAuditId AND nQResultsKey=" + nQuizId.ToString());
                    if (!(ReferenceEquals(dDateTaken, DBNull.Value) & dDateTaken is null))
                    {
                        oElmt = oQuizXml.SelectSingleNode("//status/dateTaken");
                        if (oElmt is null)
                        {
                            addNewTextNode("dateTaken", oQuizXml.SelectSingleNode("//status"), Protean.Tools.Xml.XmlDate(dDateTaken, true));
                        }
                        else
                        {
                            oElmt.InnerText = Protean.Tools.Xml.XmlDate(dDateTaken, true);
                        }
                    }

                    return oQuizXml;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getUserQuizInstance", ex, cProcessInfo));

                    return default;
                }
            }

            public long insertStructure(XmlElement oInstance)
            {
                PerfMonLog("DBHelper", "insertStructure (xmlElement)");
                string cProcessInfo = "";
                try
                {

                    return setObjectInstance(Cms.dbHelper.objectTypes.ContentStructure, oInstance);
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "insertStructure(xmlElement)", ex, cProcessInfo));
                    return default;
                }

            }


            public long insertStructure(long nStructParId, string cStructForiegnRef, string cStructName, string cStructDescription, string cStructLayout, long nStatus = 1L, [Optional, DateTimeConstant(0L/* #12:00:00 AM# */)] DateTime dPublishDate, [Optional, DateTimeConstant(0L/* #12:00:00 AM# */)] DateTime dExpireDate, string cDescription = "", long nOrder = 0L)
            {
                PerfMonLog("DBHelper", "insertStructure ([args])");
                string sSql;
                string nId;
                string cProcessInfo = "";
                try
                {
                    sSql = "Insert Into tblContentStructure (nStructParId, cStructForiegnRef, cStructName,  cStructDescription, cStructLayout, nAuditId, nStructOrder)" + "values (" + nStructParId + ",'" + SqlFmt(cStructForiegnRef) + "'" + ",'" + SqlFmt(cStructName) + "'" + ",'" + SqlFmt(cStructDescription) + "'" + ",'" + SqlFmt(cStructLayout) + "'" + "," + getAuditId((int)nStatus, cDescription: cDescription, dPublishDate: dPublishDate, dExpireDate: dExpireDate) + "," + nOrder + ")";


                    nId = GetIdInsertSql(sSql);
                    clearStructureCacheAll();
                    return Conversions.ToLong(nId);
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getAuditId", ex, cProcessInfo));

                }

                return default;
            }

            public long insertPageVersion(long nStructParId, string cStructForiegnRef, string cStructName, string cStructDescription, string cStructLayout, long nStatus = 1L, [Optional, DateTimeConstant(0L/* #12:00:00 AM# */)] DateTime dPublishDate, [Optional, DateTimeConstant(0L/* #12:00:00 AM# */)] DateTime dExpireDate, string cDescription = "", long nOrder = 0L, long nVersionParId = default, string cVersionLang = "", string cVersionDescription = "", PageVersionType nVersionType = default)
            {
                PerfMonLog("DBHelper", "insertStructure ([args])");
                string sSql;
                string nId;
                string cProcessInfo = "";
                try
                {
                    sSql = "Insert Into tblContentStructure (nStructParId, cStructForiegnRef, cStructName,  cStructDescription, cStructLayout, nAuditId, nStructOrder, nVersionParId, cVersionLang, cVersionDescription, nVersionType)" + "values (" + nStructParId + ",'" + SqlFmt(cStructForiegnRef) + "'" + ",'" + SqlFmt(cStructName) + "'" + ",'" + SqlFmt(cStructDescription) + "'" + ",'" + SqlFmt(cStructLayout) + "'" + "," + getAuditId((int)nStatus, cDescription: cDescription, dPublishDate: dPublishDate, dExpireDate: dExpireDate) + "," + nOrder + "," + nVersionParId + ",'" + cVersionLang + "'" + ",'" + cVersionDescription + "'" + "," + nVersionType + " )";

                    nId = GetIdInsertSql(sSql);
                    return Conversions.ToLong(nId);
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getAuditId", ex, cProcessInfo));

                }

                return default;
            }

            public long moveShippingLocation(long nLocKey, long nNewLocParId)
            {
                PerfMonLog("DBHelper", "moveShippingLocation");
                string sSql;
                string nId;
                string cProcessInfo = "";
                try
                {
                    sSql = "UPDATE tblCartShippingLocations SET nLocationParId = " + nNewLocParId + " WHERE nLocationKey = " + nLocKey;

                    nId = ExeProcessSql(sSql);
                    return Conversions.ToLong(nId);
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "moveShippingLocation", ex, cProcessInfo));

                }

                return default;
            }

            public long moveStructure(long nStructKey, long nNewStructParId)
            {
                PerfMonLog("DBHelper", "moveStructure");
                string sSql;
                string nId;
                string cProcessInfo = "";
                try
                {


                    sSql = "UPDATE tblContentStructure SET nStructParId = " + nNewStructParId + " WHERE nStructKey = " + nStructKey;


                    nId = ExeProcessSql(sSql);
                    clearStructureCacheAll();
                    return Conversions.ToLong(nId);
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "moveStructure", ex, cProcessInfo));

                }

                return default;
            }

            public virtual long moveContent(long nContentKey, long nStructKey, long nNewStructParId)
            {
                PerfMonLog("DBHelper", "moveContent");
                string nId;
                string cProcessInfo = "moveContent";

                try
                {

                    // Don't move if destination doesn't exist.
                    int destination = GetDataValue("SELECT TOP 1 nStructKey FROM tblContentStructure WHERE nStructKey=" + nNewStructParId, default, default, 0);

                    if (destination > 0)
                    {

                        // Trying to work out what is needed for moving content to work accurately.
                        // 1. you need to delete the location on the current page.
                        // 2. if a location exists on the destination page then you need to remove that.
                        // 3. you need to persist the Primary state
                        // 4. you need to persist the Cascaded state
                        // 5. you ought to persist the position.
                        // 6. if in removing the destination location you remove the primary location record, you should make this the primary location record

                        // Get the current location info
                        bool primary = false;
                        bool cascade = false;
                        string position = "column1";

                        // Dim currentLocation As SqlDataReader = getDataReader("SELECT TOP 1 CASE WHEN bPrimary IS NULL THEN 0 ELSE bPrimary END as PrimaryLocation, CASE WHEN bCascade IS NULL THEN 0 ELSE bCascade END AS cascadeLocation, CASE WHEN cPosition IS NULL THEN '' ELSE cPosition END AS position from tblContentLocation where nStructId = " & nStructKey & " and nContentId = " & nContentKey)
                        using (SqlDataReader currentLocation = getDataReaderDisposable("SELECT TOP 1 CASE WHEN bPrimary IS NULL THEN 0 ELSE bPrimary END as PrimaryLocation, CASE WHEN bCascade IS NULL THEN 0 ELSE bCascade END AS cascadeLocation, CASE WHEN cPosition IS NULL THEN '' ELSE cPosition END AS position from tblContentLocation where nStructId = " + nStructKey + " and nContentId = " + nContentKey))  // Done by nita on 6/7/22
                        {
                            if (currentLocation.Read)
                            {
                                primary = currentLocation[0];
                                cascade = currentLocation[1];
                                // position = currentLocation(2)
                                // TS By DEFAULT we move to column1 because it is always on every page.
                            }
                        }

                        // Work out if the destination is primary
                        int destinationPrimary = GetDataValue("SELECT TOP 1 bPrimary from tblContentLocation where nStructId = " + nNewStructParId + " and nContentId = " + nContentKey + " AND bPrimary=1", default, default, 0);

                        // Work out if the destination is the only primary
                        int areThereOtherPrimaries = GetDataValue("SELECT TOP 1 bPrimary from tblContentLocation where nStructId <> " + nNewStructParId + " and nContentId = " + nContentKey + " AND bPrimary=1", default, default, 0);

                        // If destination is the only primary then we are about to delete it so make the current location (being moved) primary
                        if (destinationPrimary > 0 & areThereOtherPrimaries == 0)
                        {
                            primary = true;
                        }

                        // Delete destination locations
                        ExeProcessSqlScalar("DELETE from tblContentLocation WHERE nContentId = " + nContentKey + " and nStructId = " + nNewStructParId);

                        // Delete current location
                        ExeProcessSqlScalar("DELETE from tblContentLocation WHERE nContentId = " + nContentKey + " and nStructId = " + nStructKey);

                        // Create new location
                        nId = setContentLocation(nNewStructParId, nContentKey, primary, cascade, true, position, true);
                    }

                    else
                    {

                        nId = nStructKey.ToString();

                    }

                    return Conversions.ToLong(nId);
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "moveContent", ex, cProcessInfo));

                }

                return default;
            }

            public void ReorderNode(objectTypes objectType, long nKey, string ReOrderCmd, string sSortField = "")
            {
                PerfMonLog("DBHelper", "ReorderNode");
                string sSql;
                DataSet oDs;
                // Dim oDr As SqlDataReader
                DataRow oRow;

                var nParentid = default(long);
                long RecCount;

                int i;


                string sKeyField = getKey(objectType);

                string cProcessInfo = "";
                try
                {
                    // Lets firsttest the object can be ordered
                    if (getOrderFname(objectType) != "")
                    {
                        if ((ReOrderCmd == "SortAlphaAsc" | ReOrderCmd == "SortAlphaDesc") & !string.IsNullOrEmpty(sSortField))
                        {
                            // Load Children
                            sSql = "Select * from " + getTable(objectType) + " where " + getParIdFname(objectType) + "=" + nKey.ToString() + " order by " + sSortField;

                            if (ReOrderCmd == "SortAlphaDesc")
                                sSql = sSql + " DESC";
                        }
                        else
                        {
                            // Load Principle
                            sSql = "Select * from " + getTable(objectType) + " where " + getKey(objectType) + "=" + nKey.ToString();
                            using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                            {
                                while (oDr.Read)
                                    nParentid = oDr.Item(getParIdFname(objectType));
                            }

                            // Get Siblings
                            sSql = "Select * from " + getTable(objectType) + " where " + getParIdFname(objectType) + "=" + nParentid.ToString() + " order by " + getOrderFname(objectType);
                        }

                        oDs = getDataSetForUpdate(sSql, getTable(objectType), "results");

                        RecCount = oDs.Tables(getTable(objectType)).Rows.Count;
                        i = 1;
                        bool skipnext = false;

                        switch (ReOrderCmd ?? "")
                        {
                            case "MoveTop":
                                {
                                    foreach (DataRow currentORow in oDs.Tables(getTable(objectType)).Rows)
                                    {
                                        oRow = currentORow;
                                        if (oRow(sKeyField) == nKey)
                                        {
                                            oRow(getOrderFname(objectType)) = 1;
                                        }
                                        else
                                        {
                                            oRow(getOrderFname(objectType)) = i + 1;
                                            i = i + 1;
                                        }
                                    }

                                    break;
                                }
                            case "MoveBottom":
                                {
                                    foreach (DataRow currentORow1 in oDs.Tables(getTable(objectType)).Rows)
                                    {
                                        oRow = currentORow1;
                                        if (oRow(sKeyField) == nKey)
                                        {
                                            oRow(getOrderFname(objectType)) = RecCount;
                                        }
                                        else
                                        {
                                            oRow(getOrderFname(objectType)) = i;
                                            i = i + 1;
                                        }
                                    }

                                    break;
                                }
                            case "MoveUp":
                                {
                                    foreach (DataRow currentORow2 in oDs.Tables(getTable(objectType)).Rows)
                                    {
                                        oRow = currentORow2;
                                        if (oRow(sKeyField) == nKey & i != 1)
                                        {
                                            // swap with previous
                                            oDs.Tables(getTable(objectType)).Rows(i - 2).Item(getOrderFname(objectType)) = i;
                                            oRow(getOrderFname(objectType)) = i - 1;
                                        }
                                        else
                                        {
                                            oRow(getOrderFname(objectType)) = i;
                                        }
                                        i = i + 1;
                                    }

                                    break;
                                }
                            case "MoveDown":
                                {
                                    foreach (DataRow currentORow3 in oDs.Tables(getTable(objectType)).Rows)
                                    {
                                        oRow = currentORow3;
                                        if (oRow(sKeyField) == nKey & i != RecCount)
                                        {
                                            // swap with next
                                            oDs.Tables(getTable(objectType)).Rows(i).Item(getOrderFname(objectType)) = i;
                                            oRow(getOrderFname(objectType)) = i + 1;
                                            skipnext = true;
                                        }
                                        else if (!skipnext)
                                        {
                                            oRow(getOrderFname(objectType)) = i;
                                            skipnext = false;
                                        }
                                        i = i + 1;
                                    }

                                    break;
                                }

                            default:
                                {
                                    foreach (DataRow currentORow4 in oDs.Tables(getTable(objectType)).Rows)
                                    {
                                        oRow = currentORow4;
                                        oRow(getOrderFname(objectType)) = i;
                                        i = i + 1;
                                    }

                                    break;
                                }
                        }

                        updateDataset(oDs, getTable(objectType));

                        clearStructureCacheAll();

                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "reOrderNode", ex, cProcessInfo));

                }
            }

            public void ReorderContent(long nPgId, long nContentId, string ReOrderCmd, bool bIsRelatedContent = false, string cPosition = "", int nGroupId = 0)
            {
                PerfMonLog("DBHelper", "ReorderContent");
                string sSql;
                DataSet oDs;
                // Dim oDr As SqlDataReader
                DataRow oRow;

                string cSchemaName = "";
                long RecCount;

                int i;

                objectTypes objectType;
                string sKeyField;
                // Dim sStatusField As String

                if (nGroupId != 0)
                {
                    objectType = objectTypes.CartCatProductRelations;
                    sKeyField = "nContentId";
                }
                else if (bIsRelatedContent)
                {
                    objectType = objectTypes.ContentRelation;
                    // sKeyField = "nContentRelationKey"
                    sKeyField = "nContentChildId";
                }
                else
                {
                    objectType = objectTypes.ContentLocation;
                    sKeyField = "nContentId";
                }


                string cProcessInfo = "";
                try
                {

                    // Lets go and get the content type
                    sSql = "Select cContentSchemaName from tblContent where nContentKey = " + nContentId;
                    using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {
                        while (oDr.Read)
                            cSchemaName = oDr.Item("cContentSchemaName");
                    }

                    // If config flag is ON then add status join in queries
                    // Get all locations of similar objects on the same page.
                    sSql = "Select CL.*, a.nStatus from tblContentLocation as CL inner join tblContent as C on C.nContentKey = CL.nContentId inner join tblAudit as a on C.nAuditId= a.nAuditKey where CL.nStructId =" + nPgId + " and C.cContentSchemaName = '" + cSchemaName + "' order by nDisplayOrder";

                    if (nGroupId != 0)
                    {
                        sSql = " Select *,a.nStatus From tblContent c " + " INNER Join tblCartCatProductRelations On c.nContentKey = tblCartCatProductRelations.nContentId  inner join tblAudit as a on c.nAuditId= a.nAuditKey " + " WHERE(tblCartCatProductRelations.nCatId = " + nGroupId + ") " + " And c.cContentSchemaName = '" + cSchemaName + "'" + " order by tblCartCatProductRelations.nDisplayOrder  ";
                    }


                    else
                    {
                        if (!string.IsNullOrEmpty(cPosition))
                        {
                            if (cPosition.EndsWith("-"))
                            {
                                sSql = "Select CL.*, a.nStatus from tblContentLocation as CL inner join tblContent as C on C.nContentKey = CL.nContentId inner join tblAudit as a on C.nAuditId= a.nAuditKey where CL.nStructId =" + nPgId + " and CL.cPosition like'" + cPosition + "%' and C.cContentSchemaName = '" + cSchemaName + "' order by nDisplayOrder";
                            }
                            else
                            {
                                sSql = "Select CL.*, a.nStatus from tblContentLocation as CL inner join tblContent as C on C.nContentKey = CL.nContentId inner join tblAudit as a on C.nAuditId= a.nAuditKey where CL.nStructId =" + nPgId + " and CL.cPosition ='" + cPosition + "' and C.cContentSchemaName = '" + cSchemaName + "' order by nDisplayOrder";
                            }
                        }

                        if (bIsRelatedContent)
                        {
                            sSql = "Select tblContentRelation.*, a.nStatus FROM tblContentRelation INNER JOIN" + " tblContent On tblContentRelation.nContentChildId = tblContent.nContentKey INNER JOIN" + " tblContent tblContent_1 On tblContent.cContentSchemaName = tblContent_1.cContentSchemaName inner join tblAudit as a on tblContent_1.nAuditId= a.nAuditKey" + " WHERE (tblContentRelation.nContentParentId = " + nPgId + ") And (tblContent_1.nContentKey = " + nContentId + ")" + " ORDER BY tblContentRelation.nDisplayOrder";
                        }
                    }
                    oDs = getDataSetForUpdate(sSql, getTable(objectType), "results");

                    // Code added for active and inactive products swap accordingly.
                    // If config key is on then add status sorting and old code running as it is.

                    bool bExcludeHiddenOnOrdering = Conversions.ToBoolean(Interaction.IIf(LCase(goConfig("ExcludeHiddenOnOrdering")) == "on", true, false));

                    if (bExcludeHiddenOnOrdering)
                    {
                        var oDt = new DataTable();
                        oDs.Tables(getTable(objectType)).DefaultView.Sort = "nStatus DESC";
                        oDt = oDs.Tables(getTable(objectType)).DefaultView.ToTable;
                        oDs.Tables(getTable(objectType)).Clear();
                        oDs.Tables(getTable(objectType)).Merge(oDt);
                        oDt.Dispose();
                        oDt = default;
                    }

                    RecCount = oDs.Tables(getTable(objectType)).Rows.Count;
                    i = 1;
                    bool skipnext = false;

                    switch (ReOrderCmd ?? "")
                    {
                        case "MoveTop":
                            {
                                foreach (DataRow currentORow in oDs.Tables(getTable(objectType)).Rows)
                                {
                                    oRow = currentORow;
                                    if (oRow(sKeyField) == nContentId)
                                    {
                                        oRow(getOrderFname(objectType)) = 1;
                                    }
                                    else
                                    {
                                        oRow(getOrderFname(objectType)) = i + 1;
                                        i = i + 1;
                                    }
                                    // non-ideal alternative for updating the entire dataset
                                    sSql = "update " + getTable(objectType) + " Set nDisplayOrder = " + oRow(getOrderFname(objectType)) + " where " + getKey(objectType) + " = " + oRow(getKey(objectType));
                                    ExeProcessSql(sSql);
                                }

                                break;
                            }
                        case "MoveBottom":
                            {
                                foreach (DataRow currentORow1 in oDs.Tables(getTable(objectType)).Rows)
                                {
                                    oRow = currentORow1;
                                    if (oRow(sKeyField) == nContentId)
                                    {
                                        oRow(getOrderFname(objectType)) = RecCount;
                                    }
                                    else
                                    {
                                        oRow(getOrderFname(objectType)) = i;
                                        i = i + 1;
                                    }
                                    // non-ideal alternative for updating the entire dataset
                                    sSql = "update " + getTable(objectType) + " Set nDisplayOrder = " + oRow(getOrderFname(objectType)) + " where " + getKey(objectType) + " = " + oRow(getKey(objectType));
                                    ExeProcessSql(sSql);
                                }

                                break;
                            }
                        case "MoveUp":
                            {
                                foreach (DataRow currentORow2 in oDs.Tables(getTable(objectType)).Rows)
                                {
                                    oRow = currentORow2;
                                    if (oRow(sKeyField) == nContentId & i != 1)
                                    {
                                        // swap with previous
                                        oDs.Tables(getTable(objectType)).Rows(i - 2).Item(getOrderFname(objectType)) = i;
                                        sSql = "update " + getTable(objectType) + " Set nDisplayOrder = " + oDs.Tables(getTable(objectType)).Rows(i - 2).Item(getOrderFname(objectType)) + " where " + getKey(objectType) + " = " + oDs.Tables(getTable(objectType)).Rows(i - 2).Item(getKey(objectType));
                                        ExeProcessSql(sSql);

                                        oRow(getOrderFname(objectType)) = i - 1;
                                        sSql = "update " + getTable(objectType) + " Set nDisplayOrder = " + oRow(getOrderFname(objectType)) + " where " + getKey(objectType) + " = " + oRow(getKey(objectType));
                                        ExeProcessSql(sSql);
                                    }
                                    else
                                    {
                                        oRow(getOrderFname(objectType)) = i;
                                        sSql = "update " + getTable(objectType) + " Set nDisplayOrder = " + oRow(getOrderFname(objectType)) + " where " + getKey(objectType) + " = " + oRow(getKey(objectType));
                                        ExeProcessSql(sSql);
                                    }
                                    i = i + 1;
                                }

                                break;
                            }
                        case "MoveDown":
                            {
                                foreach (DataRow currentORow3 in oDs.Tables(getTable(objectType)).Rows)
                                {
                                    oRow = currentORow3;
                                    if (oRow(sKeyField) == nContentId & i != RecCount)
                                    {
                                        // swap with next
                                        oDs.Tables(getTable(objectType)).Rows(i).Item(getOrderFname(objectType)) = i;
                                        oRow(getOrderFname(objectType)) = i + 1;
                                        skipnext = true;
                                    }
                                    else if (!skipnext)
                                    {
                                        oRow(getOrderFname(objectType)) = i;
                                        skipnext = false;
                                    }

                                    // non-ideal alternative for updating the entire dataset
                                    sSql = "update " + getTable(objectType) + " Set nDisplayOrder = " + oRow(getOrderFname(objectType)) + " where " + getKey(objectType) + " = " + oRow(getKey(objectType));
                                    ExeProcessSql(sSql);

                                    i = i + 1;
                                }

                                break;
                            }

                        default:
                            {
                                foreach (DataRow currentORow4 in oDs.Tables(getTable(objectType)).Rows)
                                {
                                    oRow = currentORow4;
                                    oRow(getOrderFname(objectType)) = i;
                                    i = i + 1;
                                }

                                break;
                            }
                    }
                    string sXml = oDs.GetXml;
                }
                // This won't work as we are drawing from 2 tables
                // updateDataset(oDs, getTable(objectType))

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "reOrderContent", ex, cProcessInfo));

                }
            }

            public void copyPageContent(long nSourcePageId, long nTargetPageId, bool bCopyDescendants, CopyContentType mode, XmlElement oMenuItem = default)
            {
                PerfMonLog("DBHelper", "copyPageContent");
                string cProcessInfo = "";
                string sSql;

                try
                {
                    // First we will get all the content on the current page
                    // ignoring cascaded items
                    cProcessInfo = "Retreiving Original Content";

                    sSql = "Select nContentId, bPrimary, bCascade, cPosition FROM tblContentLocation WHERE (nStructId = " + nSourcePageId + ") ORDER BY nDisplayOrder";

                    var positionReMap = new long[2, 2];
                    long copyCount = 0L;

                    DataSet oDS;
                    oDS = GetDataSet(sSql, "Content", "Contents");
                    // check if we are doing anything with the content
                    if (!(mode == CopyContentType.None))
                    {
                        foreach (DataRow oDr in oDS.Tables("Content").Rows)
                        {

                            int nContentId = 0;
                            bool bNewItem = false;
                            // Debug.WriteLine(oDr("bPrimary"))
                            if (mode == CopyContentType.Copy & oDr("bPrimary") == true)
                            {
                                bNewItem = true;
                                nContentId = (int)createContentCopy(oDr("nContentId"), default, false);
                                positionReMap[0, (int)copyCount] = oDr("nContentId");
                                positionReMap[1, (int)copyCount] = nContentId;
                                copyCount = copyCount + 1L;
                                var oldPositionReMap = positionReMap;
                                positionReMap = new long[2, (int)(copyCount + 1)];
                                if (oldPositionReMap is not null)
                                    for (var i = 0; i <= oldPositionReMap.Length / oldPositionReMap.GetLength(1) - 1; ++i)
                                        Array.Copy(oldPositionReMap, i * oldPositionReMap.GetLength(1), positionReMap, i * positionReMap.GetLength(1), Math.Min(oldPositionReMap.GetLength(1), positionReMap.GetLength(1)));
                            }
                            else if (mode == CopyContentType.CopyForce & oDr("bPrimary") == true)
                            {
                                bNewItem = true;
                                nContentId = (int)createContentCopy(oDr("nContentId"), default, true);
                                positionReMap[0, (int)copyCount] = oDr("nContentId");
                                positionReMap[1, (int)copyCount] = nContentId;
                                copyCount = copyCount + 1L;
                                var oldPositionReMap1 = positionReMap;
                                positionReMap = new long[2, (int)(copyCount + 1)];
                                if (oldPositionReMap1 is not null)
                                    for (var i1 = 0; i1 <= oldPositionReMap1.Length / oldPositionReMap1.GetLength(1) - 1; ++i1)
                                        Array.Copy(oldPositionReMap1, i1 * oldPositionReMap1.GetLength(1), positionReMap, i1 * positionReMap.GetLength(1), Math.Min(oldPositionReMap1.GetLength(1), positionReMap.GetLength(1)));
                            }
                            else if (mode == CopyContentType.Locate)
                            {
                                // just get the id
                                nContentId = oDr("nContentId"); // just need to do a locations
                            }
                            else
                            {
                                // locate with  new primaries
                                nContentId = oDr("nContentId");
                                if (oDr("bPrimary") == true)
                                    bNewItem = true;
                            }
                            // now set a location
                            setContentLocation(nTargetPageId, nContentId, bNewItem, Interaction.IIf(oDr("bCascade") is DBNull, false, oDr("bCascade")), false, Interaction.IIf(oDr("cPosition") is DBNull, "", oDr("cPosition")), true);
                            // using a different one since this isnt working for some reason

                            // setContentLocation2(nTargetPageId, nContentId, bNewItem, False)


                        }
                    }

                    ResetContentPositions(nTargetPageId, positionReMap);

                    // now we have done that page we need to look at the children

                    if (!bCopyDescendants)
                        return;
                    // do we need to create the menu for this?
                    if (oMenuItem is null)
                    {
                        // get the full menu
                        sSql = "Select nStructKey, nStructParId  FROM tblContentStructure";
                        oDS = GetDataSet(sSql, "MenuItem", "Menu");
                        oDS.Tables("MenuItem").Columns("nStructKey").ColumnMapping = MappingType.Attribute;
                        oDS.Tables("MenuItem").Columns("nStructParId").ColumnMapping = MappingType.Hidden;
                        oDS.Relations.Add("Rel01", oDS.Tables("MenuItem").Columns("nStructKey"), oDS.Tables("MenuItem").Columns("nStructParId"), false);
                        oDS.Relations("Rel01").Nested = true;
                        var oMenuXml = new XmlDocument();
                        oMenuXml.InnerXml = oDS.GetXml;
                        // now select the menu item we need
                        oMenuItem = oMenuXml.SelectSingleNode("descendant-or-self::MenuItem[@nStructKey=" + nSourcePageId + "]");
                        oMenuXml = default;
                        // we need to go through its children and create items
                    }
                    foreach (XmlElement oMenuChild in oMenuItem.ChildNodes)
                    {

                        int nMenuId = oMenuChild.GetAttribute("nStructKey"); // new source page id
                        int nNewMenuID; // new target page

                        var oMenuItemXML = new XmlDocument(); // the xml of the item
                        oMenuItemXML.AppendChild(oMenuItemXML.CreateElement("instance"));
                        oMenuItemXML.DocumentElement.InnerXml = getObjectInstance(objectTypes.ContentStructure, nMenuId);

                        XmlElement oMenuElmt;
                        foreach (XmlElement currentOMenuElmt in oMenuItemXML.DocumentElement.FirstChild.SelectNodes("nStructKey | nAuditId | nAuditKey"))
                        {
                            oMenuElmt = currentOMenuElmt;
                            oMenuElmt.InnerText = "";
                        }
                        // ste the parent new id as current page
                        oMenuElmt = oMenuItemXML.DocumentElement.FirstChild.SelectSingleNode("nStructParId");
                        oMenuElmt.InnerText = nTargetPageId;
                        // get the new id
                        nNewMenuID = setObjectInstance(objectTypes.ContentStructure, oMenuItemXML.DocumentElement);
                        // call the function again
                        copyPageContent(nMenuId, nNewMenuID, bCopyDescendants, mode, oMenuChild);
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "copyPageContent", ex, cProcessInfo));

                }
            }

            public long createContentCopy(long contentId, List<long> copied = null, bool ForceCopy = false)
            {
                PerfMonLog("DBHelper", "insertContent");
                string cProcessInfo = "";
                long nContentId;
                string sSql;
                DataSet oDS2;
                try
                {

                    if (copied is null)
                    {
                        copied = new List<long>();
                    }
                    copied.Add(contentId);

                    var oInstanceXML = new XmlDocument();
                    oInstanceXML.AppendChild(oInstanceXML.CreateElement("instance"));
                    oInstanceXML.DocumentElement.InnerXml = getObjectInstance(objectTypes.Content, contentId);
                    XmlElement oContentElmt;
                    // now we need to remove ids, audits etc
                    foreach (XmlElement currentOContentElmt in oInstanceXML.DocumentElement.FirstChild.SelectNodes("nContentKey | nContentPrimaryId | nAuditId | nAuditKey"))
                    {
                        oContentElmt = currentOContentElmt;
                        oContentElmt.InnerText = "";
                    }
                    // save it as a new piece of content and get the id
                    nContentId = setObjectInstance(objectTypes.Content, oInstanceXML.DocumentElement);
                    // get any child items related to the origional parent that are
                    // 'not orphan (have no page) or
                    // are related to other items.
                    // - copy relations to the new object.
                    sSql = "select nContentChildId, nDisplayOrder, cRelationtype," + "(select COUNT(nContentId) from tblContentLocation l where l.nContentId = r.nContentChildId) as nLocations, " + "(select COUNT(nContentParentId) from tblContentRelation r2 where r2.nContentChildId = r.nContentChildId) as nRelations, " + "(select COUNT(nContentParentId) from tblContentRelation r3 where r3.nContentParentId = r.nContentChildId and r3.nContentChildId = r.nContentParentId) as twoWay " + "from tblContentRelation r where nContentParentId = " + contentId;

                    oDS2 = GetDataSet(sSql, "Relations", "Relations");
                    foreach (DataRow oDr2 in oDS2.Tables("Relations").Rows)
                    {
                        if (ForceCopy)
                        {
                            if (!copied.Contains(oDr2("nContentChildId")))
                            {
                                string newRelatedContentId = createContentCopy(oDr2("nContentChildId"), copied).ToString();
                                insertContentRelation(nContentId, newRelatedContentId, oDr2("twoWay"), oDr2("cRelationType"), true);
                            }
                        }
                        else if (oDr2("nLocations") == 0 & oDr2("nRelations") == 1)
                        {
                            // we copy and releate because it is orphan and only related to our item
                            string newRelatedContentId = createContentCopy(oDr2("nContentChildId"), copied).ToString();
                            insertContentRelation(nContentId, newRelatedContentId, oDr2("twoWay"), oDr2("cRelationType"), true);
                        }
                        else
                        {
                            // we simply relate because it is either a page or has multiple relations
                            insertContentRelation(nContentId, oDr2("nContentChildId"), oDr2("twoWay"), oDr2("cRelationType"), true);
                        }
                    }
                    oContentElmt = default;
                    oInstanceXML = default;

                    return nContentId;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "createContentCopy", ex, cProcessInfo));
                    return default;

                }
            }


            public long insertContent(string cContentForiegnRef, string cContentName, string cSchemaName, string cContentXmlBrief, string cContentXmlDetail, int nParentID = 0, object dPublishDate = null, object dExpireDate = null, int nContentStatus = 1)
            {
                PerfMonLog("DBHelper", "insertContent");
                string sSql;
                string nId;
                string cProcessInfo = "";
                try
                {
                    sSql = "Insert Into tblContent (nContentPrimaryId, nVersion, cContentForiegnRef, cContentName,  cContentSchemaName, cContentXmlBrief, cContentXmlDetail, nAuditId)" + "values (" + nParentID + "," + "1" + ",'" + SqlFmt(cContentForiegnRef) + "'" + ",'" + SqlFmt(cContentName) + "'" + ",'" + SqlFmt(cSchemaName) + "'" + ",'" + SqlFmt(cContentXmlBrief) + "'" + ",'" + SqlFmt(cContentXmlDetail) + "'" + "," + getAuditId(nContentStatus, default, default, dPublishDate, dExpireDate) + ")";

                    nId = GetIdInsertSql(sSql);

                    return Conversions.ToLong(nId);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "insertContent", ex, cProcessInfo));

                }

                return default;

            }

            public void updateContent(long nContentId, string cContentName, string cContentXmlBrief, string cContentXmlDetail)
            {
                PerfMonLog("DBHelper", "updateContent");
                string sSql;
                string cProcessInfo = "";
                try
                {
                    sSql = "update tblContent Set" + " nContentPrimaryId = " + nContentId + " , nVersion = nVersion + 1" + " , cContentName = '" + SqlFmt(cContentName) + "'" + " , cContentXmlBrief = '" + SqlFmt(cContentXmlBrief) + "'" + " , cContentXmlDetail = '" + SqlFmt(cContentXmlDetail) + "'" + " where nContentKey = " + nContentId;

                    ExeProcessSql(sSql);
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "insertContent", ex, cProcessInfo));
                }

            }

            public int setContentLocation(long nStructId, long nContentId, bool bPrimary = false, bool bCascade = false, bool bOveridePrimary = false, string cPosition = "", bool bUpdatePosition = true, long nDisplayOrder = 0L)
            {
                PerfMonLog("DBHelper", "setContentLocation");
                // this is so we can save some content without trying to change any locations
                if (nStructId == 0L | nContentId == 0L)
                    return default;

                string sSql;
                DataSet oDs;
                DataRow oRow;
                string nId;
                string cProcessInfo = "";
                bool bReorderLocations = false;
                try
                {
                    // does this content relationship exist?
                    sSql = "select * from tblContentLocation where nStructId = " + nStructId + " and nContentId=" + nContentId;
                    oDs = getDataSetForUpdate(sSql, "ContentLocation", "Location");
                    if (oDs.Tables("ContentLocation").Rows.Count == 0)
                    {
                        oRow = oDs.Tables("ContentLocation").NewRow();
                        oRow("nStructId") = nStructId;
                        oRow("nContentId") = nContentId;
                        oRow("bPrimary") = bPrimary;
                        oRow("bCascade") = bCascade;
                        oRow("nDisplayOrder") = nDisplayOrder;
                        if (!string.IsNullOrEmpty(cPosition))
                        {
                            oRow("cPosition") = cPosition;
                        }
                        oRow("nAuditId") = getAuditId();
                        oDs.Tables("ContentLocation").Rows.Add(oRow);
                        bReorderLocations = true;
                    }
                    else
                    {
                        oRow = oDs.Tables("ContentLocation").Rows(0);
                        oRow.BeginEdit();
                        // if we are allready primary then leave it.. Unless we need for force it as in External Syncronisation XSLT
                        if (bOveridePrimary)
                        {
                            oRow("bPrimary") = bPrimary;
                        }
                        if (bUpdatePosition & !string.IsNullOrEmpty(cPosition))
                        {
                            oRow("cPosition") = cPosition;
                        }
                        oRow("bCascade") = bCascade;
                        oRow.EndEdit();
                        // update the audit table
                    }

                    updateDataset(oDs, "ContentLocation", false);
                    nId = ExeProcessSqlScalar(sSql).ToString();

                    if (bReorderLocations)
                    {
                        if (myWeb is not null)
                        {
                            if (myWeb.mcBehaviourNewContentOrder != "")
                            {
                                this.ReorderContent(nStructId, nContentId, myWeb.mcBehaviourNewContentOrder);
                            }
                        }
                    }
                    return Conversions.ToInteger(nId);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "setContentLocation", ex, cProcessInfo));
                }

                return default;

            }

            public int setContentLocation2(long nStructId, long nContentId, bool bPrimary = false, bool bCascade = false)
            {
                PerfMonLog("DBHelper", "setContentLocation2");
                // this is so we can save some content without trying to change any locations
                if (nStructId == 0L | nContentId == 0L)
                    return default;

                string sSql;
                string nId;
                string cProcessInfo = "";
                try
                {
                    // does this content relationship exist?
                    sSql = "select * from tblContentLocation where nStructId = " + nStructId + " and nContentId=" + nContentId;
                    nId = ExeProcessSqlScalar(sSql);


                    if (Conversions.ToDouble(nId) == 0d)
                    {
                        sSql = "INSERT INTO tblContentLocation (nStructId, nContentId, bPrimary, bCascade, nDisplayOrder, nAuditId) VALUES (";
                        sSql += nStructId + ",";
                        sSql += nContentId + ",";
                        sSql = Conversions.ToString(sSql + Operators.ConcatenateObject(Interaction.IIf(bPrimary, 1, 0), ","));
                        sSql = Conversions.ToString(sSql + Operators.ConcatenateObject(Interaction.IIf(bCascade, 1, 0), ","));
                        sSql += "0,";
                        sSql += getAuditId() + ");select scope_identity()";
                    }
                    else
                    {
                        sSql = "UPDATE tblContentLocation  SET ";
                        sSql += "nStructId =" + nStructId + ",";
                        sSql += "nContentId =" + nContentId + ",";
                        // sSql &= "bPrimary =" & bPrimary & ","
                        sSql = Conversions.ToString(sSql + Operators.ConcatenateObject(Operators.ConcatenateObject("bCascade =", Interaction.IIf(bCascade, 1, 0)), ","));
                        sSql += "nDisplayOrder =" + "0";
                        // sSql &= "nAuditId =" & getAuditId() & ")"
                        sSql += " WHERE nContentLocationKey = " + nId;
                    }

                    nId = ExeProcessSqlScalar(sSql);
                    return Conversions.ToInteger(nId);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "setContentLocation", ex, cProcessInfo));
                }

                return default;

            }

            public object ResetContentPositions(long pageId, long[,] positionReMap)
            {
                PerfMonLog("DBHelper", "ResetContentPositions");
                string cProcessInfo = "";
                string sSql;
                try
                {
                    for (int row = 0, loopTo = positionReMap.GetUpperBound(1); row <= loopTo; row++)
                    {
                        long oldId = positionReMap[0, row];
                        long newId = positionReMap[1, row];
                        sSql = "select cPosition from tblContentLocation where nStructId = " + pageId + " and  nContentId=" + newId;
                        string cPosition = ExeProcessSqlScalar(sSql);
                        if (cPosition is not null)
                        {
                            for (int row2 = 0, loopTo1 = positionReMap.GetUpperBound(1); row2 <= loopTo1; row2++)
                            {
                                if (cPosition.EndsWith("-" + positionReMap[0, row2].ToString()))
                                {
                                    cPosition = cPosition.Replace("-" + positionReMap[0, row2].ToString(), "-" + positionReMap[1, row2].ToString());
                                }
                            }
                            sSql = "update tblContentLocation set cPosition = '" + cPosition + "' where nStructId = " + pageId + " and  nContentId=" + newId;
                            ExeProcessSql(sSql);
                        }
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ResetContentPositions", ex, cProcessInfo));
                    return null;
                }

            }

            public void insertShippingLocation(long nOptId, long nLocId, bool bPrimary = true)
            {
                PerfMonLog("DBHelper", "insertShippingLocation");
                string sSql;
                string nId;
                string cProcessInfo = "";
                try
                {
                    sSql = "Insert Into tblCartShippingRelations(nShpOptId, nShpLocId, nAuditId)" + "values (" + nOptId + "," + nLocId + "," + getAuditId() + ")";

                    nId = ExeProcessSql(sSql);
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "insertShippingLocation", ex, cProcessInfo));
                }

            }

            public long getLocationByRef(string cForiegnRef)
            {
                PerfMonLog("DBHelper", "getLocationByRef");
                string sSql;
                string nId = 0.ToString();
                // Dim oDr As SqlDataReader


                string cProcessInfo = "";
                try
                {
                    sSql = "select nStructKey from tblContentStructure where cStructForiegnRef = '" + SqlFmt(cForiegnRef) + "'";

                    using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {

                        while (oDr.Read)
                            nId = oDr[0];


                        return Conversions.ToLong(nId);
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getLocationByRef", ex, cProcessInfo));

                }

                return default;

            }

            public string getFRefFromPageId(long nPageId)
            {
                PerfMonLog("DBHelper", "getLocationByRef");
                string sSql;
                string nId = "";

                string cProcessInfo = "";
                try
                {
                    sSql = "select cStructForiegnRef from tblContentStructure where nStructKey = " + nPageId;

                    nId = this.GetDataValue(sSql);

                    return nId;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getLocationByRef", ex, cProcessInfo));
                    return 0.ToString();
                }

            }

            public long getPrimaryLocationByArtId(long nArtId)
            {
                PerfMonLog("DBHelper", "getPrimaryLocationByArtId");
                string sSql;
                string nId = 0.ToString();
                // Dim oDr As SqlDataReader


                string cProcessInfo = "";
                try
                {
                    sSql = "SELECT nStructId FROM tblContentLocation WHERE nContentId = " + nArtId + " and (bPrimary = -1 or bPrimary = 1)";

                    using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {

                        while (oDr.Read)
                            nId = oDr[0];

                        return Conversions.ToLong(nId);
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getLocationByRef", ex, cProcessInfo));

                }

                return default;

            }

            public virtual void getContentFromModuleGrabber(ref XmlElement oContent)
            {
                PerfMonLog("DBHelper", "getContentFromModuleGrabber");

                string cProcessInfo = "";
                try
                {

                    string cWhereSql = "";
                    string cOrderBy = "";

                    // Get the parameters SortDirection
                    string cSchema = oContent.GetAttribute("contentType");
                    long nPageId = "0" + oContent.GetAttribute("grabberRoot");
                    long nTop = "0" + oContent.GetAttribute("grabberItems");
                    string cSort = oContent.GetAttribute("sortBy");
                    string cSortDirection = oContent.GetAttribute("order");
                    string cIncludeChildPages = oContent.GetAttribute("grabberItterate");

                    // Validate and Build the SQL conditions that we are going to need

                    if (!string.IsNullOrEmpty(cSchema) && nTop > 0L)
                    {

                        cWhereSql = "cContentSchemaName = " + SqlString(cSchema) + " ";

                        if (nPageId > 0L)
                        {
                            if (cIncludeChildPages.ToLower() == "true")
                            {
                                // Get the page from the structure and enumerate its children
                                string cPageIds = "" + nPageId.ToString();
                                foreach (XmlElement oPage in myWeb.moPageXml.SelectNodes("/Page/Menu//MenuItem[@id=" + nPageId.ToString() + "]//MenuItem"))
                                    cPageIds += "," + oPage.GetAttribute("id");
                                cWhereSql += " AND CL.nStructId IN (" + cPageIds + ") ";
                            }
                            else
                            {
                                cWhereSql += " AND CL.nStructId=" + SqlFmt[nPageId] + " ";
                            }
                        }


                        switch (cSort ?? "")
                        {
                            case "CreationDate":
                                {
                                    cOrderBy = "dInsertDate";
                                    break;
                                }
                            case "Name":
                                {
                                    cOrderBy = "cContentName";
                                    break;
                                }
                            case "PublishDate":
                            case "publish":
                                {
                                    cOrderBy = "dPublishDate";
                                    break;
                                }
                            case "ExpireDate":
                            case "expire":
                                {
                                    cOrderBy = "dExpireDate";
                                    break;
                                }
                            case "StartDate": // to catch historic error
                                {
                                    cOrderBy = "dExpireDate";
                                    break;
                                }
                            case "EndDate": // to catch historic error
                                {
                                    cOrderBy = "dExpireDate";
                                    break;
                                }

                            default:
                                {
                                    cOrderBy = cSort;
                                    break;
                                }
                        }

                        if (!string.IsNullOrEmpty(cOrderBy) && Strings.LCase(cSortDirection) == "descending")
                            cOrderBy += " DESC";

                        myWeb.GetPageContentFromSelect(cWhereSql, default, default, myWeb.mbAdminMode, nTop, cOrderBy, oContent);
                        // Get Related Items
                        if (LCase(goConfig("DisableGrabberRelated")) != "on")
                        {
                            foreach (XmlElement oContentElmt in oContent.SelectNodes("Content"))
                                addRelatedContent(oContentElmt, oContentElmt.GetAttribute("id"), myWeb.mbAdminMode);
                        }

                    }
                }


                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getContentFromContentGrabber", ex, cProcessInfo));
                }
            }

            public virtual void getContentFromProductGroup(ref XmlElement oContent)
            {
                PerfMonLog("DBHelper", "getContentFromModuleGrabber");

                string cProcessInfo = "";
                try
                {


                    string cOrderBy = "";

                    // Get the parameters SortDirection
                    string cSchema = oContent.GetAttribute("contentType");
                    long nGroupId = "0" + oContent.GetAttribute("groupid");
                    string cSort = oContent.GetAttribute("sortBy");
                    string cSortDirection = oContent.GetAttribute("order");
                    string cAdditionalJoin = string.Empty;
                    // Validate and Build the SQL conditions that we are going to need

                    if (!string.IsNullOrEmpty(cSchema))
                    {
                        string cWhereSql = " (tblCartCatProductRelations.nCatId =" + nGroupId + ")";
                        // Dim cWhereSql As String = " nContentKey IN (Select nContentId from tblCartCatProductRelations where nCatId=" & nGroupId & ")"
                        if (nGroupId != 0L)
                        {
                            cAdditionalJoin = "INNER Join tblCartCatProductRelations On c.nContentKey = tblCartCatProductRelations.nContentId and tblCartCatProductRelations.nCatId=" + nGroupId.ToString();
                        }

                        cOrderBy = "tblCartCatProductRelations.nDisplayOrder";


                        // Get Related Items
                        myWeb.GetPageContentFromSelect(cWhereSql, default, default, myWeb.mbAdminMode, 0, cOrderBy, oContent, cAdditionalJoin);
                        foreach (XmlElement oContentElmt in oContent.SelectNodes("Content"))
                            addRelatedContent(oContentElmt, oContentElmt.GetAttribute("id"), myWeb.mbAdminMode);


                    }
                }


                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getContentFromContentGrabber", ex, cProcessInfo));
                }
            }

            public virtual void getContentFromContentGrabber(ref XmlElement oGrabber)
            {
                PerfMonLog("DBHelper", "getContentFromContentGrabber");

                string cProcessInfo = "";
                try
                {

                    string cWhereSql = "";
                    string cOrderBy = "";

                    // Get the parameters SortDirection
                    string cSchema = getNodeValueByType(oGrabber, "Type");
                    long nPageId = getNodeValueByType(oGrabber, "Page", dataType.TypeNumber);
                    long nTop = getNodeValueByType(oGrabber, "NumberOfItems", dataType.TypeNumber);
                    string cSort = getNodeValueByType(oGrabber, "Sort");
                    string cSortDirection = getNodeValueByType(oGrabber, "SortDirection");
                    string cIncludeChildPages = getNodeValueByType(oGrabber, "IncludeChildPages");
                    string joinSQL = null;

                    // Validate and Build the SQL conditions that we are going to need

                    if (!string.IsNullOrEmpty(cSchema) && nTop > 0L)
                    {

                        cWhereSql = "cContentSchemaName = " + SqlString(cSchema) + " ";

                        if (nPageId > 0L)
                        {
                            if (cIncludeChildPages.ToLower() == "true")
                            {
                                // Get the page from the structure and enumerate its children
                                string cPageIds = "(" + nPageId.ToString() + ")";
                                foreach (XmlElement oPage in myWeb.moPageXml.SelectNodes("/Page/Menu//MenuItem[@id=" + nPageId.ToString() + "]//MenuItem"))
                                    cPageIds += ", (" + oPage.GetAttribute("id") + ")";
                                joinSQL = " Join(values " + cPageIds + ") V(pageRef) on V.pageRef = CL.nStructId ";
                            }
                            else
                            {
                                cWhereSql += " AND CL.nStructId=" + SqlFmt[nPageId] + " ";
                            }
                        }


                        switch (cSort ?? "")
                        {
                            case "CreationDate":
                                {
                                    cOrderBy = "dInsertDate";
                                    break;
                                }
                            case "Name":
                                {
                                    cOrderBy = "cContentName";
                                    break;
                                }
                            case "PublishDate":
                                {
                                    cOrderBy = "dPublishDate";
                                    break;
                                }
                            case "ExpireDate":
                                {
                                    cOrderBy = "dExpireDate";
                                    break;
                                }
                            case "StartDate": // to catch historic error
                                {
                                    cOrderBy = "dExpireDate";
                                    break;
                                }
                            case "EndDate": // to catch historic error
                                {
                                    cOrderBy = "dExpireDate";
                                    break;
                                }

                            default:
                                {
                                    cOrderBy = cSort;
                                    break;
                                }
                        }

                        if (!string.IsNullOrEmpty(cOrderBy) && cSortDirection == "Descending")
                            cOrderBy += " DESC";
                        PerfMonLog("DBHelper", "getContentFromContentGrabber-Start");
                        myWeb.GetPageContentFromSelect(cWhereSql, default, default, default, nTop, cOrderBy, default, joinSQL);
                        PerfMonLog("DBHelper", "getContentFromContentGrabber-End");

                    }
                }


                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getContentFromContentGrabber", ex, cProcessInfo));
                }
            }

            public long getContentByRef(string cForiegnRef)
            {
                PerfMonLog("DBHelper", "getContentByRef");
                string sSql;
                string nId = 0.ToString();
                // Dim oDr As SqlDataReader

                string cProcessInfo = "";
                try
                {
                    sSql = "select nContentKey from tblContent where cContentForiegnRef = '" + SqlFmt(cForiegnRef) + "'";
                    using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {
                        while (oDr.Read)
                            nId = oDr[0];

                        return Conversions.ToLong(nId);
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getContentByRef", ex, cProcessInfo));

                }

                return default;

            }

            public string getContentBrief(int nId)
            {
                PerfMonLog("DBHelper", "getContentBrief");
                string sSql;
                // Dim oDr As SqlDataReader
                string cContent = "";

                string cProcessInfo = "";
                try
                {
                    sSql = "select cContentXmlBrief from tblContent where nContentKey = " + nId;

                    using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {

                        while (oDr.Read)
                            cContent = oDr[0];

                        return cContent;
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getContentByRef", ex, cProcessInfo));

                    return "";
                }
            }

            public string getContentFilePath(DataRow oRow, string URLXpath = "/Content/Path")
            {
                PerfMonLog("DBHelper", "getContentFilePath");
                string cProcessInfo = "";
                var oXml = new XmlDocument();
                XmlElement oPathElmt;
                string sPath = "";
                try
                {
                    if (string.IsNullOrEmpty(URLXpath))
                        URLXpath = "/Content/Path";

                    oXml.InnerXml = oRow("cContentXmlBrief");
                    oPathElmt = oXml.SelectSingleNode(URLXpath);
                    if (oPathElmt is not null)
                    {
                        sPath = oPathElmt.InnerText;
                    }
                    if (oPathElmt is null | string.IsNullOrEmpty(sPath))
                    {
                        cProcessInfo = "No 'Path' in Brief";
                        oXml.InnerXml = oRow("cContentXmlDetail");
                        oPathElmt = oXml.SelectSingleNode(URLXpath);
                        if (oPathElmt is null)
                        {
                            cProcessInfo = "No 'Path' in Detail";
                        }
                        else
                        {
                            sPath = oPathElmt.InnerText;
                        }
                    }
                    // if no path found return nothing
                    if (string.IsNullOrEmpty(sPath))
                    {
                        return "";
                    }
                    else if (sPath.StartsWith(".."))
                    {
                        return goServer.MapPath("/") + goServer.UrlDecode(Protean.Tools.Xml.convertEntitiesToString(Protean.Tools.Xml.convertEntitiesToString(sPath)));
                    }
                    else
                    {
                        return goServer.MapPath("/" + goServer.UrlDecode(Protean.Tools.Xml.convertEntitiesToString(Protean.Tools.Xml.convertEntitiesToString(sPath))));
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getContentFilePath", ex, cProcessInfo));
                    return "";
                }
            }

            public string getContentType(int nId)
            {

                string sSql;
                string sContentSchemaName = "";
                // Dim oDr As SqlDataReader


                string cProcessInfo = "";
                try
                {
                    sSql = "select cContentSchemaName from tblContent where nContentKey = " + nId;

                    using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {

                        while (oDr.Read)
                            sContentSchemaName = oDr[0];

                        return sContentSchemaName;
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getContentType", ex, cProcessInfo));

                    return "";
                }

            }


            public XmlElement getContentVersions(ref long nContentId)
            {
                PerfMonLog("DBHelper", "getContentVersions");
                XmlElement oRoot;
                string sSqlContent;
                string sSqlVersions;

                DataSet oDs;
                DataSet oDs2;

                string cProcessInfo = "getContentVersions";
                try
                {

                    oRoot = moPageXml.CreateElement("ContentDetail");

                    sSqlContent = "" + "select c.nContentKey as id,  c.nContentPrimaryId as primaryId,  c.nVersion as version,  cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, a.nStatus as status, a.dInsertDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], dins.cDirName as owner, dins.cDirXml as ownerDetail, dupd.cDirName as updater, dupd.cDirXml as updaterDetail " + "from tblContent c inner join tblAudit a on c.nAuditId = a.nAuditKey inner join tblDirectory dins on dins.nDirKey = a.nInsertDirId inner join tblDirectory dupd on dupd.nDirKey = a.nUpdateDirId  where nContentKey = " + nContentId;

                    oDs = GetDataSet(sSqlContent, "Version", "ContentVersions");
                    {
                        var withBlock = oDs.Tables("Version");
                        withBlock.Columns("id").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock.Columns("primaryId").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock.Columns("version").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock.Columns("ref").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock.Columns("name").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock.Columns("type").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock.Columns("status").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock.Columns("publish").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock.Columns("expire").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock.Columns("update").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock.Columns("owner").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock.Columns("updater").ColumnMapping = System.Data.MappingType.Attribute;
                    }

                    sSqlVersions = "" + "select c.nContentVersionKey as id,  c.nContentPrimaryId as primaryId,  c.nVersion as version,  cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, a.nStatus as status, a.dInsertDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], dins.cDirName as owner, dins.cDirXml as ownerDetail, dupd.cDirName as updater, dupd.cDirXml as updaterDetail " + "from tblContentVersions c inner join tblAudit a on c.nAuditId = a.nAuditKey inner join tblDirectory dins on dins.nDirKey = a.nInsertDirId inner join tblDirectory dupd on dupd.nDirKey = a.nUpdateDirId where nContentPrimaryId = " + nContentId + " ORDER BY c.nVersion DESC";

                    oDs2 = GetDataSet(sSqlVersions, "Version", "ContentVersions");
                    {
                        var withBlock1 = oDs2.Tables("Version");
                        withBlock1.Columns("id").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock1.Columns("primaryId").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock1.Columns("version").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock1.Columns("ref").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock1.Columns("name").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock1.Columns("type").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock1.Columns("status").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock1.Columns("publish").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock1.Columns("expire").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock1.Columns("update").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock1.Columns("owner").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock1.Columns("updater").ColumnMapping = System.Data.MappingType.Attribute;
                    }

                    oDs.Merge(oDs2);

                    // tidy the user details
                    oRoot.InnerXml = oDs.GetXml;

                    foreach (XmlElement oElmt in oRoot.SelectNodes("ContentVersions/Version/ownerDetail | ContentVersions/Version/updaterDetail "))
                        oElmt.InnerXml = oElmt.InnerText;

                    return oRoot.FirstChild;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getContentVersions", ex, cProcessInfo));
                    return default;
                }
            }

            public XmlElement getPageVersions(ref long PageId)
            {
                PerfMonLog("DBHelper", "getPageVersions");
                XmlElement oRoot;
                string sSqlContent;
                string sSqlVersions;
                DataSet oDs;
                DataSet oDs2;
                object ParPageId;

                string cProcessInfo = "getPageVersions";
                try
                {
                    sSqlContent = "select nVersionParId from tblContentStructure where nStructKey = " + PageId;
                    ParPageId = GetDataValue(sSqlContent);
                    if (ParPageId is DBNull)
                    {
                        ParPageId = PageId;
                    }
                    else if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(ParPageId, 0, false)))
                    {
                        ParPageId = PageId;
                    }

                    oRoot = moPageXml.CreateElement("ContentDetail");

                    sSqlContent = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("" + "select p.nStructKey as id,  p.nStructKey as primaryId,  p.cVersionDescription as description,  p.cStructForiegnRef as ref, cStructName as name, nVersionType as type, cVersionLang as lang, a.nStatus as status, a.dInsertDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], dins.cDirName as owner, dins.cDirXml as ownerDetail, dupd.cDirName as updater, dupd.cDirXml as updaterDetail,  dbo.fxn_getPageGroups(p.nStructKey) as Groups " + "from tblContentStructure p inner join tblAudit a on p.nAuditId = a.nAuditKey LEFT OUTER JOIN  tblDirectory dins on dins.nDirKey = a.nInsertDirId LEFT OUTER JOIN tblDirectory dupd on dupd.nDirKey = a.nUpdateDirId  where nStructKey = ", ParPageId), " order by nVersionType, nStructOrder"));

                    oDs = GetDataSet(sSqlContent, "Version", "PageVersions");
                    {
                        var withBlock = oDs.Tables("Version");
                        withBlock.Columns("id").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock.Columns("primaryId").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock.Columns("description").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock.Columns("ref").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock.Columns("name").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock.Columns("type").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock.Columns("lang").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock.Columns("status").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock.Columns("publish").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock.Columns("expire").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock.Columns("update").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock.Columns("owner").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock.Columns("updater").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock.Columns("groups").ColumnMapping = System.Data.MappingType.Attribute;
                    }

                    sSqlVersions = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("" + "select p.nStructKey as id,  p.nVersionParId as primaryId,  p.cVersionDescription as description,  p.cStructForiegnRef as ref, cStructName as name, nVersionType as type, cVersionLang as lang, a.nStatus as status, a.dInsertDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], dins.cDirName as owner, dins.cDirXml as ownerDetail, dupd.cDirName as updater, dupd.cDirXml as updaterDetail, dbo.fxn_getPageGroups(p.nStructKey) as Groups " + "from tblContentStructure p inner join tblAudit a on p.nAuditId = a.nAuditKey left outer join tblDirectory dins on dins.nDirKey = a.nInsertDirId left outer join tblDirectory dupd on dupd.nDirKey = a.nUpdateDirId  where p.nVersionParId = ", ParPageId), " order by nVersionType, nStructOrder"));
                    oDs2 = GetDataSet(sSqlVersions, "Version", "PageVersions");
                    {
                        var withBlock1 = oDs2.Tables("Version");
                        withBlock1.Columns("id").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock1.Columns("primaryId").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock1.Columns("description").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock1.Columns("ref").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock1.Columns("name").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock1.Columns("type").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock1.Columns("lang").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock1.Columns("status").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock1.Columns("publish").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock1.Columns("expire").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock1.Columns("update").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock1.Columns("owner").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock1.Columns("updater").ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock1.Columns("groups").ColumnMapping = System.Data.MappingType.Attribute;
                    }

                    oDs2.Merge(oDs);
                    oRoot.InnerXml = oDs2.GetXml;

                    // tidy the user details
                    XmlElement oElmt;

                    foreach (XmlElement currentOElmt in oRoot.SelectNodes("PageVersions/Version/ownerDetail | PageVersions/Version/updaterDetail "))
                    {
                        oElmt = currentOElmt;
                        oElmt.InnerXml = oElmt.InnerText;
                    }

                    foreach (XmlElement currentOElmt1 in oRoot.SelectNodes("PageVersions/Version"))
                    {
                        oElmt = currentOElmt1;
                        if (myWeb.goLangConfig is not null)
                        {
                            if (oElmt.GetAttribute("lang") == myWeb.goLangConfig.GetAttribute("code") | oElmt.GetAttribute("lang") == "")
                            {
                                oElmt.SetAttribute("langSystemName", myWeb.goLangConfig.GetAttribute("default"));
                            }
                            else
                            {
                                XmlElement langElmt = myWeb.goLangConfig.SelectSingleNode("Language[@code='" + oElmt.GetAttribute("lang") + "']");
                                if (langElmt is not null)
                                {
                                    oElmt.SetAttribute("langSystemName", langElmt.GetAttribute("systemName"));
                                }
                            }
                        }
                    }

                    return oRoot.FirstChild;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getContentVersions", ex, cProcessInfo));
                    return default;
                }
            }

            public long insertDirectory(string cDirForiegnRef, string cDirSchema, string cDirName, string cDirPassword, string cDirXml, int nStatus = 1, bool bOverwrite = false, string cEmail = "")
            {
                PerfMonLog("DBHelper", "insertDirectory");
                string sSql;
                var nId = default(long);
                string cProcessInfo = "";
                // Dim oDr As SqlDataReader

                try
                {
                    if (!string.IsNullOrEmpty(cDirForiegnRef))
                    {
                        nId = getObjectByRef(objectTypes.Directory, cDirForiegnRef, cDirSchema);
                    }

                    // We should consider setting up encrypted passwords by default.

                    cDirPassword = Protean.Tools.Encryption.HashString(cDirPassword, goConfig("MembershipEncryption"), true);

                    if (nId < 1L)
                    {
                        sSql = "Insert Into tblDirectory (cDirSchema, cDirForiegnRef, cDirName, cDirPassword, cDirXml, cDirEmail, nAuditId)" + " values (" + "'" + SqlFmt(cDirSchema) + "'" + ",'" + SqlFmt(cDirForiegnRef) + "'" + ",'" + SqlFmt(cDirName) + "'" + ",'" + SqlFmt(cDirPassword) + "'" + ",'" + SqlFmt(cDirXml) + "'" + ",'" + SqlFmt(cEmail) + "'" + "," + getAuditId[nStatus] + ")";
                        nId = GetIdInsertSql(sSql);
                    }
                    else if (bOverwrite)
                    {

                        sSql = "update tblDirectory set " + "cDirSchema = '" + SqlFmt(cDirSchema) + "'," + "cDirName = '" + SqlFmt(cDirName) + "'," + "cDirPassword = '" + SqlFmt(cDirPassword) + "'," + "cDirXML = '" + SqlFmt(cDirXml) + "'," + "cDirEmail = '" + SqlFmt(cDirXml) + "'" + " where nDirKey = " + nId;
                        ExeProcessSql(sSql);
                        // insert code to update the audit table here
                    }

                    return nId;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "insertUser", ex, cProcessInfo));

                }

                return default;
            }

            public void maintainDirectoryRelation(long nParId, long nChildId, bool bRemove = false, object dExpireDate = null, bool bIfExistsDontUpdate = false, string cEmail = null, string cGroup = null, bool isLast = true)
            {
                string sSql;
                // Dim oDr As SqlDataReader
                string cProcessInfo = "";
                long nDelRelationId = 0L;
                XmlDocument oXml;
                bool bHasChanged = false;
                PerfMonLog(mcModuleName, "maintainMembershipsFromXForm", "start");
                try
                {
                    if (!(nParId == 0L | nChildId == 0L))
                    {
                        // Does relationship exist?
                        sSql = "select * from tblDirectoryRelation where nDirParentId = " + nParId + " and nDirChildId  = " + nChildId;
                        using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                        {
                            if (oDr.HasRows)
                            {
                                // if so check bRemove and remove it if nessesary
                                if (!bRemove)
                                {
                                    // If the permission exists, then we can update it - or we can opt to ignore the option of updating it
                                    // This is a db performance savign for some bulk routines.
                                    if (!bIfExistsDontUpdate)
                                    {
                                        while (oDr.Read)
                                        {
                                            // update audit
                                            oXml = new XmlDocument();
                                            if (Information.IsDate(dExpireDate))
                                            {
                                                oXml.LoadXml("<instance><tblAudit><dExpireDate>" + Protean.Tools.Xml.XmlDate(dExpireDate) + "</dExpireDate></tblAudit></instance>");
                                            }
                                            else
                                            {
                                                // this should update the update date and user
                                                oXml.LoadXml("<instance><tblAudit/></instance>");
                                            }
                                            setObjectInstance(objectTypes.Audit, oXml.DocumentElement, oDr("nAuditId"));
                                            oXml = default;
                                        }
                                    }
                                }
                                else
                                {
                                    while (oDr.Read)
                                    {
                                        DeleteObject(objectTypes.DirectoryRelation, oDr("nRelKey"));
                                        bHasChanged = true;
                                    }
                                }
                            }

                            // if not create it

                            else if (!bRemove)
                            {
                                // Dim nAuditId As String = ""
                                if (Information.IsDate(dExpireDate))
                                {
                                    sSql = "insert into tblDirectoryRelation(nDirParentId, nDirChildId, nAuditId) values( " + nParId + ", " + nChildId + ", " + this.getAuditId(default, default, default, default, dExpireDate) + ")";
                                }
                                else
                                {
                                    sSql = "insert into tblDirectoryRelation(nDirParentId, nDirChildId, nAuditId) values( " + nParId + ", " + nChildId + ", " + this.getAuditId() + ")";
                                }
                                ExeProcessSql(sSql);
                                bHasChanged = true;
                            }

                            if (bHasChanged)
                            {
                                // Keep Mailing List In Sync.
                                // If Not cEmail Is Nothing Then
                                System.Collections.Specialized.NameValueCollection moMailConfig = WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");
                                string sMessagingProvider = "";
                                if (moMailConfig is not null)
                                {
                                    sMessagingProvider = moMailConfig["MessagingProvider"];
                                }
                                if (moMessaging is null & myWeb is not null)
                                {
                                    // myWeb IsNot Nothing prevents being called from bulk imports.
                                    moMessaging = new Protean.Providers.Messaging.BaseProvider(myWeb, sMessagingProvider);
                                }
                                if (moMessaging is not null && moMessaging.AdminProcess is not null)
                                {
                                    try
                                    {
                                        moMessaging.AdminProcess.maintainUserInGroup(nChildId, nParId, bRemove, cEmail, cGroup, isLast);
                                    }
                                    catch (Exception ex)
                                    {
                                        cProcessInfo = ex.StackTrace;
                                    }
                                }
                                // End If
                            }
                        }
                    }

                    CloseConnection();
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "maintainDirectoryRelation", ex, cProcessInfo));
                    // Close()
                }
            }

            public void maintainPermission(long nPageId, long nDirId, string nLevel = "1")
            {
                PerfMonLog("DBHelper", "maintainPermission");
                string sSql;
                // Dim oDr As SqlDataReader
                string cProcessInfo = "";

                try
                {

                    // Does relationship exist?
                    sSql = "select * from tblDirectoryPermission where nDirId = " + nDirId + " and nStructId  = " + nPageId;
                    using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {
                        if (oDr.HasRows)
                        {
                            // if so check bRemove and remove it if nessesary
                            while (oDr.Read)
                            {
                                // update audit
                                // the permission level has changed... update it
                                if (nLevel != oDr("nAccessLevel"))
                                {
                                    sSql = "update tblDirectoryPermission set nAccessLevel = " + nLevel + " where nPermKey=" + oDr("nPermKey");
                                    ExeProcessSql(sSql);
                                }
                            }
                        }

                        else
                        {
                            // if not create it

                            sSql = "insert into tblDirectoryPermission(nDirId, nStructId,nAccessLevel, nAuditId) values( " + nDirId + ", " + nPageId + "," + nLevel + " ," + this.getAuditId + ")";
                            ExeProcessSql(sSql);
                        }

                        // If nLevel = 0 Then
                        // sSql = "delete tblDirectoryPermission where nDirId = " & nDirId & " and nStructId  = " & nPageId
                        // exeProcessSQL(sSql)
                        // End If
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "maintainDirectoryRelation", ex, cProcessInfo));

                }
            }

            public virtual XmlElement listDirectory(string cSchemaName, long nParId = 0L, int nStatus = 99)
            {
                PerfMonLog("DBHelper", "listDirectory");
                string sSql;
                DataSet oDs;
                // Dim oDr As SqlDataReader
                XmlElement oElmt;
                XmlElement oElmt2;
                XmlDataDocument oXml;

                string sContent;

                string cProcessInfo = "";
                try
                {

                    if (goSession("oDirList") is null | goSession("cDirListType") != cSchemaName)
                    {
                        switch (cSchemaName ?? "")
                        {
                            case "User":
                                {
                                    sSql = "execute spGetUsers";
                                    if (nParId != 0L)
                                    {
                                        sSql = sSql + " @nParDirId= " + nParId;
                                        if (nStatus != 99)
                                        {
                                            sSql = sSql + ", @nStatus= " + nStatus;
                                        }
                                    }
                                    else if (nStatus != 99)
                                    {
                                        sSql = sSql + " @nStatus= " + nStatus;
                                    }

                                    if (goRequest("search") != "")
                                    {
                                        sSql = "execute spSearchUsers @cSearch='" + goRequest("search") + "'";
                                    }

                                    break;
                                }

                            default:
                                {
                                    if (goRequest("search") != "")
                                    {
                                        sSql = "execute spSearchDirectory @cSearch='" + goRequest("search") + "', @cSchemaName = '" + cSchemaName + "'";
                                    }
                                    else
                                    {
                                        sSql = "execute spGetDirectoryItems @cSchemaName = '" + cSchemaName + "'";

                                    }
                                    if (nParId != 0L)
                                    {
                                        sSql = sSql + ", @nParDirId= " + nParId;
                                    }

                                    break;
                                }
                        }

                        // DataSet Method
                        oDs = GetDataSet(sSql, Strings.LCase(cSchemaName), "directory");
                        ReturnNullsEmpty(oDs);

                        oDs.Tables(0).Columns(0).ColumnMapping = System.Data.MappingType.Attribute;

                        oXml = new XmlDataDocument(oDs);
                        oDs.EnforceConstraints = false;

                        // Convert any text to xml
                        switch (cSchemaName ?? "")
                        {
                            case "User":
                                {
                                    foreach (XmlElement currentOElmt2 in oXml.SelectNodes("descendant-or-self::UserXml"))
                                    {
                                        oElmt2 = currentOElmt2;
                                        sContent = oElmt2.InnerText;
                                        if (!string.IsNullOrEmpty(sContent))
                                        {
                                            oElmt2.InnerXml = sContent;
                                        }
                                        oElmt2.ParentNode.ReplaceChild(oElmt2.FirstChild, oElmt2);
                                    }

                                    break;
                                }

                            default:
                                {
                                    foreach (XmlElement currentOElmt21 in oXml.SelectNodes("descendant-or-self::Details"))
                                    {
                                        oElmt2 = currentOElmt21;
                                        sContent = oElmt2.InnerText;
                                        if (!string.IsNullOrEmpty(sContent))
                                        {
                                            oElmt2.InnerXml = sContent;
                                        }
                                        oElmt2.ParentNode.ReplaceChild(oElmt2.FirstChild, oElmt2);
                                    }

                                    break;
                                }
                        }

                        goSession("sDirListType") = cSchemaName;
                        goSession("oDirList") = oXml;
                    }
                    else
                    {
                        oXml = goSession("sDirListType");
                    }
                    oElmt = moPageXml.CreateElement("directory");

                    if (oXml.FirstChild is not null)
                    {
                        oElmt.InnerXml = oXml.FirstChild.InnerXml;
                    }

                    // let get the details of the parent object
                    if (nParId != 0L)
                    {
                        sSql = "select * from tblDirectory where nDirKey = " + nParId;
                        using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                        {
                            while (oDr.Read)
                            {
                                oElmt.SetAttribute("parId", nParId);
                                oElmt.SetAttribute("parType", oDr("cDirSchema"));
                                oElmt.SetAttribute("parName", oDr("cDirName"));
                            }
                        }
                    }
                    else
                    {
                        oElmt.SetAttribute("parType", cSchemaName);
                        oElmt.SetAttribute("parName", "All");
                    }
                    oElmt.SetAttribute("itemType", cSchemaName);
                    switch (cSchemaName ?? "")
                    {
                        case "User":
                            {
                                oElmt.SetAttribute("displayName", "Users");
                                break;
                            }
                        case "Role":
                            {
                                oElmt.SetAttribute("displayName", "Roles");
                                break;
                            }
                        case "Group":
                            {
                                oElmt.SetAttribute("displayName", "Groups");
                                break;
                            }
                        case "Company":
                            {
                                oElmt.SetAttribute("displayName", "Companies");
                                break;
                            }
                        case "Department":
                            {
                                oElmt.SetAttribute("displayName", "Departments");
                                break;
                            }

                        default:
                            {
                                oElmt.SetAttribute("displayName", cSchemaName + "s");
                                break;
                            }
                    }

                    return oElmt;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getUsers", ex, cProcessInfo));
                    return default;
                }

            }

            public XmlElement GetUserXML(long nUserId, bool bIncludeContacts = true)
            {
                PerfMonLog("DBHelper", "GetUserXML");

                // Dim odr As SqlDataReader
                XmlElement root = default;
                XmlElement oElmt;
                string sSql = "";
                string sProcessInfo = "";
                PermissionLevel PermLevel = PermissionLevel.Open;
                string cOverrideUserGroups;
                string cJoinType;
                try
                {
                    cOverrideUserGroups = goConfig("SearchAllUserGroups");
                    if (nUserId != 0L)
                    {
                        // 

                        // If nPermLevel > 2 Then
                        // myWeb.moPageXml.DocumentElement.SetAttribute("adminMode", getPermissionLevel(nPermLevel))
                        // End If
                        // odr = getDataReader("SELECT * FROM tblDirectory where nDirKey = " & nUserId)
                        using (SqlDataReader oDr = getDataReaderDisposable("SELECT * FROM tblDirectory where nDirKey = " + nUserId))  // Done by nita on 6/7/22
                        {
                            while (oDr.Read)
                            {
                                root = moPageXml.CreateElement(oDr("cDirSchema"));
                                root.SetAttribute("id", nUserId);
                                root.SetAttribute("name", oDr("cDirName"));
                                root.SetAttribute("fRef", oDr("cDirForiegnRef"));
                                // root.SetAttribute("permission", getPermissionLevel(nPermLevel))
                                if (oDr("cDirXml") != "")
                                {
                                    root.InnerXml = oDr("cDirXml");
                                    foreach (XmlAttribute attr in root.FirstChild.Attributes)
                                        root.SetAttribute(attr.Name, attr.Value);
                                    root.InnerXml = root.SelectSingleNode("*").InnerXml;
                                }
                                // Ignore if myWeb is nothing
                                if (myWeb is not null)
                                {
                                    PermLevel = getPagePermissionLevel(myWeb.mnPageId);
                                    root.SetAttribute("pagePermission", PermLevel.ToString);
                                }
                            }
                        }

                        // get group memberships


                        if (cOverrideUserGroups == "on")
                        {
                            cJoinType = "LEFT";
                        }
                        else
                        {
                            cJoinType = "INNER";
                        }

                        sSql = "SELECT	d.*," + " r.nRelKey As Member" + " FROM	tblDirectory d" + " " + cJoinType + " JOIN tblDirectoryRelation r" + " ON r.nDirParentid = d.nDirKey " + " AND r.nDirChildId = " + nUserId + " WHERE   d.cDirSchema <> 'User'" + " ORDER BY d.cDirName";

                        using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                        {
                            while (oDr.Read)
                            {
                                oElmt = moPageXml.CreateElement(oDr("cDirSchema"));
                                oElmt.SetAttribute("id", oDr("nDirKey"));
                                oElmt.SetAttribute("name", oDr("cDirName"));
                                oElmt.SetAttribute("fRef", oDr("cDirForiegnRef"));
                                if (!(oDr("Member") is DBNull))
                                {
                                    oElmt.SetAttribute("isMember", "yes");
                                }
                                oElmt.InnerXml = oDr("cDirXml");
                                foreach (XmlAttribute attr in oElmt.FirstChild.Attributes)
                                    oElmt.SetAttribute(attr.Name, attr.Value);
                                oElmt.InnerXml = oElmt.FirstChild.InnerXml;
                                if (!(cOverrideUserGroups == "on"))
                                {
                                    if (!(oDr("Member") is DBNull))
                                    {
                                        if (oElmt is not null & root is not null)
                                        {
                                            root.AppendChild(oElmt);
                                        }
                                    }
                                }
                                else
                                {
                                    root.AppendChild(oElmt);
                                }
                                // Sync User with Mail Provider


                            }
                        }
                        if (root is null)
                        {
                            root = moPageXml.CreateElement("User");
                            root.SetAttribute("id", nUserId);
                            root.SetAttribute("old", "true()");
                        }

                        if (bIncludeContacts)
                        {
                            root.AppendChild(GetUserContactsXml((int)nUserId));
                            foreach (XmlElement oCompany in root.SelectNodes("Company[@id!='']"))
                                oCompany.AppendChild(GetUserContactsXml(oCompany.GetAttribute("id")));
                        }

                        if (goConfig("Subscriptions") == "on" & myWeb is not null)
                        {
                            var mySub = new Protean.Cms.Cart.Subscriptions(myWeb);
                            mySub.AddSubscriptionToUserXML(root, nUserId);
                        }
                        // now we want to get the admin permissions for this page


                    }
                    if (root is not null)
                    {
                        if (root.SelectSingleNode("cContactTelCountryCode") is null)
                        {
                            root.AppendChild(root.OwnerDocument.CreateElement("cContactTelCountryCode"));
                        }
                    }
                    return root;
                }

                catch (Exception ex)
                {
                    if (string.IsNullOrEmpty(sProcessInfo))
                        sProcessInfo = sSql;
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetUserXML", ex, sProcessInfo));
                    return default;
                }

            }


            public XmlElement GetUserContactsXml(int nUserId)
            {
                PerfMonLog("DBHelper", "GetUserContactsXMl");
                try
                {
                    XmlElement oContacts = moPageXml.CreateElement("Contacts");
                    string cSQL = "SELECT * FROM tblCartContact where nContactCartId = 0 and nContactDirId = " + nUserId;
                    var oDS = new DataSet();
                    oDS = GetDataSet(cSQL, "Contact");
                    foreach (DataRow oDRow in oDS.Tables(0).Rows)
                    {
                        XmlElement oContact = moPageXml.CreateElement("Contact");
                        foreach (DataColumn oDC in oDS.Tables(0).Columns)
                        {
                            XmlElement oIElmt = moPageXml.CreateElement(oDC.ColumnName);
                            if (!(oDRow(oDC.ColumnName) is DBNull))
                            {
                                string cStrContent = oDRow(oDC.ColumnName);
                                cStrContent = encodeAllHTML(cStrContent);
                                if (cStrContent is not null & !string.IsNullOrEmpty(cStrContent))
                                {
                                    if (oDC.ColumnName == "cContactXml")
                                    {
                                        oIElmt.InnerXml = oDRow(oDC.ColumnName);
                                    }
                                    else
                                    {
                                        oIElmt.InnerText = cStrContent;
                                    }
                                }
                            }
                            oContact.AppendChild(oIElmt);
                        }
                        oContacts.AppendChild(oContact);
                    }
                    return oContacts;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetUserContactsXMl", ex, ""));
                    return default;
                }
            }

            public long getDirParentId(long nChildId)
            {
                PerfMonLog("DBHelper", "getDirParentId");
                // only to be used on departments because they can only have 1 company
                string sSql;
                string cProcessInfo = "";
                try
                {
                    sSql = "select d.nDirKey as id " + "FROM tblDirectory d " + "INNER JOIN tblDirectoryRelation dept2company " + "ON d.nDirKey = dept2company.nDirParentId " + "WHERE dept2company.nDirChildId=" + nChildId;

                    return GetDataValue(sSql);
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getDirParentId", ex, cProcessInfo));
                }

                return default;
            }

            public virtual XmlElement listDirRelations(long nChildId, string cSchemaName, long nParId = 0L)
            {
                PerfMonLog("DBHelper", "listDirRelations");
                string sSql;
                DataSet oDs;
                // Dim oDr As SqlDataReader
                XmlElement oElmt;
                string cChildSchema = "";
                XmlDataDocument oXml;
                string sSqlCompanyCol = "";
                string sSqlCompanyOrder = "";

                string sContent;

                string cProcessInfo = "";
                try
                {

                    // If goSession("oDirList") Is Nothing Or goSession("cDirListType") <> cSchemaName Then

                    oElmt = moPageXml.CreateElement("directory");

                    // let get the details of the child object
                    if (nChildId != 0L)
                    {
                        sSql = "select * from tblDirectory where nDirKey = " + nChildId;
                        using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                        {
                            if (oDr.HasRows)
                            {
                                while (oDr.Read)
                                {
                                    oElmt.SetAttribute("childId", nChildId);
                                    oElmt.SetAttribute("childType", oDr("cDirSchema"));
                                    cChildSchema = oDr("cDirSchema");
                                    oElmt.SetAttribute("childName", oDr("cDirName"));
                                }
                            }
                        }
                    }
                    else
                    {
                        oElmt.SetAttribute("parType", cSchemaName);
                        oElmt.SetAttribute("parName", "All");
                    }

                    oElmt.SetAttribute("itemType", cSchemaName);
                    switch (cSchemaName ?? "")
                    {
                        case "User":
                            {
                                oElmt.SetAttribute("displayName", "Users");
                                break;
                            }
                        case "Role":
                            {
                                oElmt.SetAttribute("displayName", "Roles");
                                break;
                            }
                        case "Group":
                            {
                                oElmt.SetAttribute("displayName", "Groups");
                                break;
                            }
                        case "Company":
                            {
                                oElmt.SetAttribute("displayName", "Companies");
                                break;
                            }
                        case "Department":
                            {
                                oElmt.SetAttribute("displayName", "Departments");
                                break;
                            }

                        default:
                            {
                                break;
                            }
                    }

                    if (cSchemaName == "Department" & cChildSchema == "User")
                    {
                        // get only the departments which are in the users companies
                        // this is not required if the parent is not a user

                        sSql = "select d.nDirKey as id, d.cDirName as name, d.cDirXml as details, a.nStatus as status, dr.nDirChildId as related " + "FROM (((tblDirectory d " + "inner join tblAudit a on nAuditId = a.nAuditKey " + "INNER JOIN tblDirectoryRelation dept2company " + "ON d.nDirKey = dept2company.nDirChildId) " + "INNER JOIN tblDirectory company " + "ON company.nDirKey = dept2company.nDirParentId) " + "INNER JOIN tblDirectoryRelation user2company " + "ON company.nDirKey = user2company.nDirParentId) " + "INNER JOIN tblDirectory users " + "ON users.nDirKey = user2company.nDirChildId " + " left outer join tblDirectoryRelation dr on d.nDirKey = dr.nDirParentId and dr.nDirChildId = " + nChildId + "WHERE d.cDirSchema = 'Department' AND company.cDirSchema = 'Company' AND users.cDirSchema = 'User' " + "AND users.nDirKey=" + nChildId + " order by d.cDirName";
                    }
                    else
                    {
                        // if we are dealing with departments then lets list the companies they are in and sort by companyies first
                        if (cSchemaName == "Department")
                        {
                            sSqlCompanyCol = "dbo.fxn_getUserCompanies(d.nDirKey) as Company,";
                            sSqlCompanyOrder = "Company, ";
                        }
                        if (goRequest("relateAs") == "children")
                        {
                            sSql = "select d.nDirKey as id, " + sSqlCompanyCol + " d.cDirName as name, d.cDirXml as details, a.nStatus as status, dr.nDirChildId as related" + " from tblDirectory d" + " inner join tblAudit a on nAuditId = a.nAuditKey " + " left outer join tblDirectoryRelation dr on nDirKey = dr.nDirChildId and dr.nDirParentId = " + nChildId + " where cDirSchema = '" + cSchemaName + "' " + " and a.nStatus <> 0 order by " + sSqlCompanyOrder + " d.cDirName";




                        }

                        else
                        {
                            sSql = "select d.nDirKey as id, d.cDirName as name, d.cDirXml as details, a.nStatus as status, dr.nDirChildId as related from tblDirectory d" + " inner join tblAudit a on nAuditId = a.nAuditKey " + " left outer join tblDirectoryRelation dr on nDirKey = dr.nDirParentId and dr.nDirChildId = " + nChildId + " where cDirSchema = '" + cSchemaName + "' " + " and a.nStatus <> 0 order by d.cDirName";



                        }

                    }

                    // DataSet Method

                    oDs = GetDataSet(sSql, Strings.LCase(cSchemaName), "directory");
                    ReturnNullsEmpty(oDs);
                    if (string.IsNullOrEmpty(sSqlCompanyCol))
                    {
                        oDs.Tables(0).Columns(0).ColumnMapping = System.Data.MappingType.Attribute;
                        oDs.Tables(0).Columns(1).ColumnMapping = System.Data.MappingType.Attribute;
                        oDs.Tables(0).Columns(3).ColumnMapping = System.Data.MappingType.Attribute;
                        oDs.Tables(0).Columns(4).ColumnMapping = System.Data.MappingType.Attribute;
                    }
                    else
                    {
                        oDs.Tables(0).Columns(0).ColumnMapping = System.Data.MappingType.Attribute;
                        oDs.Tables(0).Columns(1).ColumnMapping = System.Data.MappingType.Attribute;
                        oDs.Tables(0).Columns(2).ColumnMapping = System.Data.MappingType.Attribute;
                        oDs.Tables(0).Columns(4).ColumnMapping = System.Data.MappingType.Attribute;
                        oDs.Tables(0).Columns(5).ColumnMapping = System.Data.MappingType.Attribute;
                    }


                    oXml = new XmlDataDocument(oDs);
                    oDs.EnforceConstraints = false;

                    // Convert any text to xml
                    foreach (XmlElement oElmt2 in oXml.SelectNodes("descendant-or-self::details"))
                    {
                        sContent = oElmt2.InnerText;
                        if (!string.IsNullOrEmpty(sContent))
                        {
                            oElmt2.ParentNode.InnerXml = sContent;
                        }
                    }

                    // goSession("sDirListType") = cSchemaName
                    // goSession("oDirList") = oXml
                    // Else
                    // oXml = goSession("sDirListType")
                    // End If

                    if (oXml.FirstChild is not null)
                    {
                        oElmt.InnerXml = oXml.FirstChild.InnerXml;
                    }



                    return oElmt;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getUsers", ex, cProcessInfo));
                    return default;
                }

            }

            public void saveDirectoryRelations(long nChildId = 0L, string sParIds = "", bool bRemove = false, RelationType relateAs = RelationType.Parent, bool bIfExistsDontUpdate = false)
            {

                string[] aParId = null;
                long nParId;
                var i = default(long);
                PerfMonLog("DBHelper", "saveDirectoryRelations", "Start");
                string cProcessInfo = "";
                try
                {
                    if (nChildId == 0L)
                    {
                        nChildId = myWeb.moRequest("childId");
                    }


                    if (string.IsNullOrEmpty(sParIds))
                    {
                        if (myWeb.moRequest("parentList") is not null)
                        {
                            aParId = myWeb.moRequest("parentList").Split(",");
                        }
                    }
                    else
                    {
                        aParId = sParIds.Split(",");
                    }

                    if (aParId is not null)
                    {
                        var loopTo = (long)Information.UBound(aParId);
                        for (i = 0L; i <= loopTo; i++)
                        {
                            if (Information.IsNumeric(aParId[(int)i]))
                            {
                                nParId = Conversions.ToLong(aParId[(int)i]);
                                if (myWeb.moRequest("relateAs") == "children" | relateAs == RelationType.Child)
                                {
                                    // OK we are reversing the way this works and relating as children rather than parents.
                                    // this is done for group memberships where companyies, departments etc. are children of groups.
                                    if (string.IsNullOrEmpty(sParIds))
                                    {
                                        // handle the checkbox form
                                        if (myWeb.moRequest("rel_" + nParId) != "")
                                        {
                                            maintainDirectoryRelation(nChildId, nParId, bIfExistsDontUpdate: bIfExistsDontUpdate);
                                        }
                                        else
                                        {
                                            maintainDirectoryRelation(nChildId, nParId, true, bIfExistsDontUpdate: bIfExistsDontUpdate);
                                        }
                                    }
                                    else if (bRemove == false)
                                    {
                                        maintainDirectoryRelation(nChildId, nParId, bIfExistsDontUpdate: bIfExistsDontUpdate);
                                    }
                                    else
                                    {
                                        maintainDirectoryRelation(nChildId, nParId, true, bIfExistsDontUpdate: bIfExistsDontUpdate);
                                    }
                                }
                                else if (string.IsNullOrEmpty(sParIds))
                                {
                                    // handle the checkbox form
                                    if (myWeb.moRequest("rel_" + nParId) != "")
                                    {
                                        maintainDirectoryRelation(nParId, nChildId, bIfExistsDontUpdate: bIfExistsDontUpdate);
                                    }
                                    else
                                    {
                                        maintainDirectoryRelation(nParId, nChildId, true, bIfExistsDontUpdate: bIfExistsDontUpdate);
                                    }
                                }
                                else if (bRemove == false)
                                {
                                    maintainDirectoryRelation(nParId, nChildId, bIfExistsDontUpdate: bIfExistsDontUpdate);
                                }
                                else
                                {
                                    maintainDirectoryRelation(nParId, nChildId, true, bIfExistsDontUpdate: bIfExistsDontUpdate);
                                }
                            }
                        }
                    }

                    PerfMonLog("DBHelper", "saveDirectoryRelations", "End" + i);
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "saveDirectoryRelations", ex, cProcessInfo));
                }

            }


            public void moveDirectoryRelations(long nSourceDirId, long nTargetDirId, bool bDeleteSource = false)
            {

                string sSql = "";
                // Dim oDr As SqlDataReader
                string cProcessInfo = "";
                try
                {

                    // Transfer all the relations to another department
                    sSql = "select nRelKey, nDirChildId from tblDirectoryRelation where nDirParentId = " + nSourceDirId;
                    using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {
                        while (oDr.Read)
                        {
                            // first we remove
                            DeleteObject(dbHelper.objectTypes.DirectoryRelation, oDr("nRelKey"));
                            // then we insert new
                            saveDirectoryRelations(oDr("nDirChildId"), nTargetDirId, false, bIfExistsDontUpdate: true);
                        }
                    }

                    // Delete Department Directory Relations, Department Permissions and Department

                    if (bDeleteSource)
                    {
                        DeleteObject(dbHelper.objectTypes.Directory, nSourceDirId);
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "moveDirectoryRelations", ex, cProcessInfo));
                }

            }

            public void deleteChildDirectoryRelations(long nDirId)
            {

                string sSql = "";
                // Dim oDr As SqlDataReader
                string cProcessInfo = "";
                try
                {

                    // remove all child relations so child objects don't get deleted
                    sSql = "select nRelKey from tblDirectoryRelation where nDirParentId = " + nDirId;
                    using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {
                        while (oDr.Read)
                            DeleteObject(dbHelper.objectTypes.DirectoryRelation, oDr[0]);
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "moveDirectoryRelations", ex, cProcessInfo));
                }

            }


            public void saveDirectoryPermissions()
            {
                PerfMonLog("DBHelper", "saveDirectoryPermissions");
                long nParId;
                string nPageId;



                string cProcessInfo = "";
                try
                {

                    nParId = goRequest("parId");

                    foreach (var item in goRequest.Form)
                    {
                        if (Strings.InStr(Conversions.ToString(item), "page_") > 0)
                        {
                            nPageId = Conversions.ToLong(Strings.Replace(Conversions.ToString(item), "page_", "")).ToString();
                            switch (goRequest(item))
                            {
                                case "permit":
                                    {
                                        savePermissions(nPageId, goRequest("parId"), PermissionLevel.View);
                                        break;
                                    }
                                case "deny":
                                    {
                                        savePermissions(nPageId, goRequest("parId"), PermissionLevel.Denied);
                                        break;
                                    }
                                case "remove":
                                    {
                                        savePermissions(nPageId, goRequest("parId"), PermissionLevel.Open);
                                        break;
                                    }
                            }
                        }
                    }
                }


                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "saveDirectoryPermissions", ex, cProcessInfo));
                }

            }

            public void savePermissions(long nPageId, string csDirId, PermissionLevel nLevel = PermissionLevel.View)
            {
                PerfMonLog("DBHelper", "savePermissions");
                string[] aDirId;
                long nDirId;
                long i;
                string sSql;
                // Dim oDr As SqlDataReader

                string cProcessInfo = "";
                try
                {

                    if (!string.IsNullOrEmpty(csDirId))
                    {

                        aDirId = Strings.Split(csDirId, ",");

                        var loopTo = (long)Information.UBound(aDirId);
                        for (i = 0L; i <= loopTo; i++)
                        {
                            if (!string.IsNullOrEmpty(aDirId[(int)i]))
                            {
                                nDirId = Conversions.ToLong(aDirId[(int)i]);
                                if (nLevel != PermissionLevel.Open)
                                {
                                    this.maintainPermission(nPageId, nDirId, nLevel);
                                }
                                else
                                {
                                    sSql = "select * from tblDirectoryPermission where nStructId = " + nPageId + " and nDirId=" + nDirId;
                                    using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                                    {
                                        while (oDr.Read)
                                            DeleteObject(objectTypes.Permission, oDr("nPermKey"));
                                    }
                                }

                            }
                        }
                    }
                    else
                    {
                        // reset all permissions for page
                        switch (nLevel)
                        {
                            case var @case when @case == PermissionLevel.Open:
                                {
                                    sSql = "select * from tblDirectoryPermission where nStructId = " + nPageId;
                                    using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                                    {
                                        while (oDr.Read)
                                            DeleteObject(objectTypes.Permission, oDr("nPermKey"));
                                    }

                                    break;
                                }

                            default:
                                {
                                    break;
                                }
                                // do nothing why would you want to reset all permissions to the same level?
                        }

                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "saveDirectoryRelations", ex, cProcessInfo));
                }

            }

            public void clearDirectoryCache()
            {
                PerfMonLog("DBHelper", "clearDirectoryCache");
                string cProcessInfo = "";
                try
                {

                    goSession("sDirListType") = (object)null;
                    goSession("oDirList") = (object)null;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "clearDirectoryCache", ex, cProcessInfo));
                }
            }

            public virtual void clearStructureCacheUser()
            {

                string sSql = "";
                try
                {
                    if (checkDBObjectExists("tblXmlCache", Tools.Database.objectTypes.Table))
                    {
                        if (myWeb.mnUserId > 0)
                        {
                            // user id exists
                            sSql = "DELETE FROM dbo.tblXmlCache " + " WHERE nCacheDirId = " + Protean.SqlFmt(myWeb.mnUserId);
                        }
                        else
                        {
                            // No user id - delete the cache based on session id.
                            sSql = "DELETE FROM dbo.tblXmlCache " + " WHERE cCacheSessionID = '" + SqlFmt(myWeb.moSession.SessionID.ToString) + "'";
                        }
                        base.ExeProcessSql(sSql);
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "clearStructureCache", ex, sSql));
                }

            }

            public virtual void clearStructureCacheAll()
            {

                string sSql = "";
                try
                {
                    if (checkDBObjectExists("tblXmlCache", Tools.Database.objectTypes.Table))
                    {
                        // user id exists
                        sSql = "DELETE FROM dbo.tblXmlCache ";
                        // clear from app level too
                        if (myWeb is not null)
                        {
                            myWeb.moCtx.Application("AdminStructureCache") = (object)null;
                        }
                        base.ExeProcessSql(sSql);
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "clearStructureCache", ex, sSql));
                }

            }

            public void addStructureCache(bool bAuth, long nUserId, ref string cCacheType, ref XmlElement StructureXml)
            {
                string sSql = "";


                try
                {
                    if (checkDBObjectExists("tblXmlCache", Tools.Database.objectTypes.Table))
                    {

                        // OPTION 1 - Insert using dataset update because insert statement vSlow.
                        // Dim oDs As DataSet
                        // Dim oRow As DataRow
                        // sSql = "select * from tblXmlCache where nCacheKey = 0"
                        // oDs = getDataSetForUpdate(sSql, "tblXmlCache", "Cache")
                        // If oDs.Tables("tblXmlCache").Rows.Count = 0 Then
                        // oRow = oDs.Tables("tblXmlCache").NewRow()
                        // oRow("cCacheType") = cCacheType
                        // oRow("cCacheSessionId") = IIf(bAuth, Eonic.SqlFmt(goSession.SessionID), "")
                        // oRow("nCacheDirId") = mnUserId
                        // oRow("cCacheStructure") = StructureXml.OuterXml 'New XmlNodeReader(StructureXml)
                        // oDs.Tables("tblXmlCache").Rows.Add(oRow)
                        // End If
                        // updateDataset(oDs, "tblXmlCache", False)

                        // Automatically clear up historical caches
                        if (bAuth)
                        {
                            ExeProcessSqlScalar(Operators.ConcatenateObject(Operators.ConcatenateObject("DELETE FROM dbo.tblXmlCache WHERE cCacheSessionId = '", Interaction.IIf(bAuth, Protean.SqlFmt(goSession.SessionID), "")), "' AND DATEDIFF(hh,dCacheDate,GETDATE()) > 12"));
                        }
                        else
                        {
                            ExeProcessSqlScalar("DELETE FROM dbo.tblXmlCache WHERE DATEDIFF(hh,dCacheDate,GETDATE()) > 12");
                        }

                        // OPTION 2 - Insert using parameter Also slow
                        string nUpdateCount;
                        sSql = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("INSERT INTO dbo.tblXmlCache (cCacheSessionID,nCacheDirId,cCacheStructure,cCacheType) " + "VALUES (" + "'", Interaction.IIf(bAuth, Protean.SqlFmt(goSession.SessionID), "")), "',")) + Protean.SqlFmt(nUserId) + "," + " @XmlValue," + "'" + cCacheType + "'" + ")";






                        var oCmd = new SqlCommand(sSql, oConn);

                        if (oConn.State == ConnectionState.Closed)
                            oConn.Open();

                        var param = new SqlParameter("@XmlValue", SqlDbType.Xml);
                        param.Direction = ParameterDirection.Input;
                        param.Value = new XmlNodeReader(StructureXml); // StructureXml.OuterXml '
                        param.Size = StructureXml.OuterXml.Length;
                        oCmd.Parameters.Add(param);

                        nUpdateCount = oCmd.ExecuteNonQuery;

                        // OPTION 3 User SQLBULKCOPY because supposed to be fast

                        // Dim dt As New DataTable()
                        // dt.Columns.Add("nCacheKey", Type.GetType("System.Int64"))
                        // dt.Columns.Add("cCacheType", Type.GetType("System.String"))
                        // dt.Columns.Add("cCacheSessionID", Type.GetType("System.String"))
                        // dt.Columns.Add("nCacheDirId", Type.GetType("System.Int64"))
                        // dt.Columns.Add("cCacheDate", Type.GetType("System.DateTime"))
                        // 'dt.Columns.Add("cCacheStructure", Type.GetType("System.Byte[]"))
                        // dt.Columns.Add("cCacheStructure", Type.GetType("System.String"))
                        // Dim oRow As DataRow = dt.NewRow
                        // oRow("cCacheSessionID") = IIf(bAuth, Eonic.SqlFmt(goSession.SessionID), "")
                        // oRow("nCacheDirId") = nUserId
                        // 'oRow("cCacheStructure") = Text.Encoding.Default.GetBytes(StructureXml.OuterXml)
                        // oRow("cCacheStructure") = StructureXml.OuterXml
                        // oRow("cCacheType") = cCacheType
                        // dt.Rows.Add(oRow)

                        // Dim oCopy As New SqlBulkCopy(oConn)
                        // If oConn.State = ConnectionState.Closed Then oConn.Open()
                        // oCopy.DestinationTableName = "dbo.tblXmlCache"
                        // oCopy.WriteToServer(dt)

                        // ExeProcessSql(sSql)

                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "addStructureCache", ex, sSql));
                }
            }
            public virtual string validateUser(long nUserId, string cPassword)
            {
                return validateUser(nUserId, "", cPassword);
            }

            public virtual string validateUser(string cUsername, string cPassword)
            {
                return validateUser(0L, cUsername, cPassword);
            }


            private string validateUser(long nUserId, string cUsername, string cPasswordForm)
            {

                // Username can be an email address if specified in the web config setting.

                PerfMonLog("DBHelper", "validateUser");
                string sSql = "";
                // Dim oDr As SqlDataReader
                DataSet dsUsers;
                DataRow oUserDetails;
                string cPasswordDatabase = "";
                string sReturn = "";
                string cProcessInfo = "";
                string ADPassword = cPasswordForm;
                bool bValidPassword = false;
                Protean.Tools.Security.Impersonate oImp;
                bool isValidEmailAddress;
                bool areEmailAddressesAllowed;
                int nNumberOfUsers;

                try
                {

                    // Does the configuration setting indicate that email addresses are allowed.
                    if (LCase(myWeb.moConfig("EmailUsernames")) == "on")
                    {
                        areEmailAddressesAllowed = true;
                    }
                    else
                    {
                        areEmailAddressesAllowed = false;
                    }

                    // Returns true if cUsername is a valid email address.
                    isValidEmailAddress = Tools.Xml.EmailAddressCheck(cUsername);

                    // first we validate against active directory
                    if (checkDBObjectExists("tblDirectory", Tools.Database.objectTypes.Table))
                    {

                        // If validating the user based on their User Id
                        if (nUserId > 0L)
                        {
                            sSql = "select d.*, a.* from tblDirectory d inner join tblAudit a on a.nAuditkey = nAuditId where " + "cDirSchema = 'User' and nDirKey = " + nUserId;
                            sReturn = "<span class=\"msg-1023\">The user Id is does not exist. Please contact your administrator.</span>";
                        }
                        // If the config setting indicates that email addresses are allowed and it is a valid email address.
                        else if (areEmailAddressesAllowed == true & isValidEmailAddress == true)
                        {
                            // Log in using an email address.
                            // If validating the user by their username which can be an email address.
                            sSql = "select d.*, a.* from tblDirectory d inner join tblAudit a on a.nAuditkey = nAuditId where " + "cDirSchema = 'User' and cDirEmail = '" + SqlFmt(cUsername) + "'";
                            sReturn = "<span class=\"msg-1024\">The email address was not found. Please contact your administrator.</span>";
                        }
                        else if (areEmailAddressesAllowed == true & isValidEmailAddress == false)
                        {
                            // It is not an email address so try to log in using a user name.
                            // If validating the user by their username which can be an email address.
                            sSql = "select d.*, a.* from tblDirectory d inner join tblAudit a on a.nAuditkey = nAuditId where " + "cDirSchema = 'User' and cDirName = '" + SqlFmt(cUsername) + "'";
                            sReturn = "<span class=\"msg-1015\">These credentials do not match a valid account</span>";
                        }
                        else
                        {
                            // Log in using a user name.
                            // If validating the user by their username which can be an email address.
                            sSql = "select d.*, a.* from tblDirectory d inner join tblAudit a on a.nAuditkey = nAuditId where " + "cDirSchema = 'User' and cDirName = '" + SqlFmt(cUsername) + "'";
                            sReturn = "<span class=\"msg-1015\">These credentials do not match a valid account</span>";
                        }

                        dsUsers = GetDataSet(sSql, "tblTemp");
                        nNumberOfUsers = dsUsers.Tables(0).Rows.Count;

                        if (nNumberOfUsers == 0)
                        {
                            sReturn = sReturn; // "<span class=""msg-1015"">The username was not found</span>"
                        }
                        // Return sReturn
                        else if (nNumberOfUsers > 1)
                        {
                            sReturn = "<span class=\"msg-1025\">Sorry that email address is ambiguous. Please use your username instead.</span>";
                        }
                        // Return sReturn
                        else
                        {
                            oUserDetails = dsUsers.Tables(0).Rows(0);
                            cPasswordDatabase = oUserDetails("cDirPassword");
                            nUserId = oUserDetails("nDirKey");

                            if (!(LCase(myWeb.moConfig("MembershipEncryption")) == "plain") & !(myWeb.moConfig("MembershipEncryption") == ""))
                            {
                                cPasswordForm = Protean.Tools.Encryption.HashString(cPasswordForm, LCase(myWeb.moConfig("MembershipEncryption")), true); // plain - md5 - sha1

                                if (LCase(myWeb.moConfig("MembershipEncryption")) == "md5salt")
                                {
                                    // we need password from the database, as this has the salt in format: hashedpassword:salt
                                    string[] arrPasswordFromDatabase = Strings.Split(cPasswordDatabase, ":");
                                    if (arrPasswordFromDatabase.Length == 2)
                                    {
                                        // RJP 7 Nov 2012. Note leave the md5 hard coded in the line below.
                                        if (arrPasswordFromDatabase[0] == Protean.Tools.Encryption.HashString(arrPasswordFromDatabase[1] + ADPassword, "md5", true))
                                        {
                                            bValidPassword = true;
                                        }
                                    }
                                }

                                else
                                {
                                    var oConvDoc = new XmlDocument();
                                    XmlElement oConvElmt = oConvDoc.CreateElement("PW");
                                    oConvElmt.InnerText = cPasswordForm;
                                    cPasswordForm = oConvElmt.InnerXml;
                                    cPasswordForm = Strings.Replace(cPasswordForm, "&gt;", ">");
                                    cPasswordForm = Strings.Replace(cPasswordForm, "&lt;", "<");
                                    if ((cPasswordDatabase ?? "") == (cPasswordForm ?? ""))
                                        bValidPassword = true;

                                }
                            }
                            else if ((cPasswordDatabase ?? "") == (cPasswordForm ?? ""))
                                bValidPassword = true;

                            if (bValidPassword == true)
                            {

                                switch (dsUsers.Tables(0).Rows.Count)
                                {

                                    case 0:
                                        {
                                            sReturn = "<span class=\"msg-1015\">These credentials do not match a valid account</span>";
                                            break;
                                        }

                                    case 1:
                                        {
                                            sReturn = oUserDetails("nDirKey");

                                            // Check user dates
                                            if (Information.IsDate(oUserDetails("dExpireDate")))
                                            {
                                                if (oUserDetails("dExpireDate") < DateTime.Now)
                                                {
                                                    sReturn = "<span class=\"msg-1016\">User account has expired</span>";
                                                }
                                            }
                                            if (Information.IsDate(oUserDetails("dPublishDate")))
                                            {
                                                if (oUserDetails("dPublishDate") >= DateTime.Now)
                                                {
                                                    sReturn = "<span class=\"msg-1012\">User account is not active</span>";
                                                }
                                            }
                                            // Check user status
                                            if (!(oUserDetails("nStatus") == 1 | oUserDetails("nStatus") == -1))
                                            {
                                                sReturn = "<span class=\"msg-1013\">User account has been disabled</span>";
                                            }

                                            break;
                                        }

                                    default:
                                        {
                                            // RJP 7 Nov 2012. Modified the message below to reflect the problem encountered.
                                            sReturn = "<span class=\"msg-1033\">Could not identify the account from the details supplied. Please contact the System Administrator.</span>";
                                            break;
                                        }
                                }
                            }

                            else
                            {
                                XmlElement moPolicy = WebConfigurationManager.GetWebApplicationSection("protean/PasswordPolicy");
                                string retryMsg = "";
                                if (moPolicy is not null)
                                {
                                    int nRetrys = "0" + moPolicy.FirstChild.SelectSingleNode("retrys").InnerText;
                                    if (nRetrys > 0)
                                    {
                                        myWeb.moDbHelper.logActivity(dbHelper.ActivityType.LogonInvalidPassword, nUserId, 0, default, cPasswordForm);

                                        string sSql2 = "select count(nActivityKey) from tblActivityLog where nActivityType=" + dbHelper.ActivityType.LogonInvalidPassword + " and nUserDirId = " + nUserId;
                                        int earlierTries = base.ExeProcessSqlScalar(sSql2);
                                        if (earlierTries >= nRetrys)
                                        {
                                            var oMembershipProv = new Providers.Membership.BaseProvider(myWeb, myWeb.moConfig("MembershipProvider"));

                                            oMembershipProv.Activities.ResetUserAcct(myWeb, nUserId);

                                            sReturn = "<span class=\"msg-1032\">Your account is blocked, an email has been sent to you to reset your password.</span>";
                                        }

                                        else
                                        {
                                            int trysLeft = nRetrys - earlierTries;
                                            retryMsg = "<br/><span class=\"msg-1031\">You have <span class=\"retryCount\">" + trysLeft + "</span> attempts before you account is blocked</span>";
                                            sReturn = "<span class=\"msg-1014\">The Password is not valid</span>" + retryMsg;
                                        }
                                    }
                                    else
                                    {
                                        sReturn = "<span class=\"msg-1015\">These credentials do not match a valid account</span>";
                                    }
                                }
                                else
                                {
                                    sReturn = "<span class=\"msg-1014\">The Password is not valid</span>" + retryMsg;
                                }


                            }


                        }

                        // If we get to here and have passed all the validation than sReturn will have been set to a userId and therefore numeric.

                        // check AD Login
                        if (!Information.IsNumeric(sReturn) & !string.IsNullOrEmpty(cUsername))
                        {
                            oImp = new Protean.Tools.Security.Impersonate();
                            if (oImp.ImpersonateValidUser(cUsername, goConfig("AdminDomain"), ADPassword, true, goConfig("AdminGroup")))
                            {
                                sReturn = 1.ToString();
                                // RJP 7 Nov 2012. Amended to use Lower Case to prevent against case sensitive entries in Protean.Cms.Config.
                                myWeb.moSession("ewAuth") = Protean.Tools.Encryption.HashString(myWeb.moSession.SessionID + goConfig("AdminPassword"), LCase(myWeb.moConfig("MembershipEncryption")), true);

                            }
                        }

                        // check Single User Login
                        if (gbSingleLoginSessionPerUser && Information.IsNumeric(sReturn) && !string.IsNullOrEmpty(cUsername))
                        {

                            // Find the latest activity for this user within a timeout period - if it isn't logoff then flag up an error
                            string lastSeenActivityQuery = "" + "SELECT TOP 1 nACtivityType FROM tblActivityLog l " + "WHERE nUserDirId = " + sReturn.ToString() + " " + "AND DATEDIFF(s,l.dDateTime,GETDATE()) < " + gnSingleLoginSessionTimeout.ToString + " " + "ORDER BY dDateTime DESC ";




                            int lastSeenActivity = GetDataValue(lastSeenActivityQuery, default, default, ActivityType.Logoff);
                            if (lastSeenActivity != ActivityType.Logoff)
                            {
                                sReturn = "<span class=\"msg-9017\">This username is currently logged on.  Please wait for them to log off or try another username.</span>";
                            }

                        }

                        if (Information.IsNumeric(sReturn))
                        {
                            // delete failed logon attempts record
                            string sSql2 = "delete from tblActivityLog where nActivityType = " + Cms.dbHelper.ActivityType.LogonInvalidPassword + " and nUserDirId=" + sReturn;
                            myWeb.moDbHelper.ExeProcessSql(sSql2);

                            // check mailinglist sync

                            // Keep Mailing List In Sync.
                            // If Not cEmail Is Nothing Then
                            System.Collections.Specialized.NameValueCollection moMailConfig = WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");
                            string sMessagingProvider = "";
                            if (moMailConfig is not null)
                            {
                                sMessagingProvider = moMailConfig["MessagingProvider"];
                            }
                            if (moMessaging is null)
                            {
                                moMessaging = new Protean.Providers.Messaging.BaseProvider(myWeb, sMessagingProvider);
                            }
                            if (moMessaging is not null && moMessaging.AdminProcess is not null)
                            {
                                try
                                {
                                    moMessaging.AdminProcess.SyncUser(Conversions.ToInteger(sReturn));
                                }
                                catch (Exception ex)
                                {
                                    cProcessInfo = ex.StackTrace;
                                }
                            }
                            // End If

                        }



                        return sReturn;
                    }
                    // table doesn't exist
                    else if (!string.IsNullOrEmpty(cUsername))
                    {
                        oImp = new Protean.Tools.Security.Impersonate();
                        if (oImp.ImpersonateValidUser(cUsername, goConfig("AdminDomain"), ADPassword, true, goConfig("AdminGroup")))
                        {
                            return 1.ToString();
                            // RJP 7 Nov 2012. Amended to use Lower Case to prevent against case sensitive entries in Protean.Cms.Config.
                            myWeb.moSession("ewAuth") = Protean.Tools.Encryption.HashString(myWeb.moSession.SessionID + goConfig("AdminPassword"), LCase(myWeb.moConfig("MembershipEncryption")), true);
                        }
                        else
                        {
                            return null;
                        }
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "validateUser", ex, cProcessInfo));
                    return null;
                }

                return default;
            }

            public virtual int GetUserIDFromEmail(string cEmail, bool bIncludeInactive = false)
            {

                PerfMonLog("DbHelper", "GetUserIDFromEmail");

                string cSql = "";
                int nReturnId = -1;

                try
                {
                    cEmail = Strings.Trim(cEmail);
                    if (Protean.Tools.Text.IsEmail(cEmail))
                    {
                        // This assumes that e-mail addresses are unique, but in case they're not we'll select
                        // the first active user that has been most recently updated
                        cSql = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("SELECT TOP 1 nDirKey " + "FROM tblDirectory  " + "INNER JOIN tblAudit ON nauditid = nauditkey  " + "WHERE cDirXml LIKE ('%<Email>' + @email + '</Email>%') " + "AND cDirSchema='User'  ", Interaction.IIf(bIncludeInactive, " ", "AND nStatus<>0 ")), "ORDER BY ABS(nStatus) DESC, dUpdateDate DESC "));





                        nReturnId = GetDataValue(cSql, default, Protean.Tools.Dictionary.getSimpleHashTable("email:" + SqlFmt(cEmail)), -1);
                    }

                    return nReturnId;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetUserIDFromEmail", ex, cSql));
                    return nReturnId;
                }
            }


            public bool IsValidUser(int nUserId)
            {
                PerfMonLog("DbHelper", "IsValidUser");

                string cSql = "";
                bool cIsValidUser = false;

                try
                {

                    cSql = "SELECT nDirKey " + "FROM tblDirectory  " + "INNER JOIN tblAudit ON nauditid = nauditkey  " + "WHERE nDirKey = " + SqlFmt(nUserId.ToString()) + " " + "AND cDirSchema='User'  " + "AND nStatus <> 0 " + "AND (dPublishDate is null or dPublishDate = 0 or dPublishDate <= " + Protean.Tools.Database.SqlDate(DateTime.Now) + " ) " + "AND (dExpireDate is null or dExpireDate = 0 or dExpireDate >= " + Protean.Tools.Database.SqlDate(DateTime.Now) + " ) ";







                    cIsValidUser = this.GetDataValue(cSql, default, default, -1) != -1;
                    return cIsValidUser;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "IsValidUser", ex, cSql));
                    return cIsValidUser;
                }
            }


            public int DirParentByType(int nChildId, string nParentType)
            {
                try
                {
                    int cReturn;
                    string cSQL = "SELECT top 1 tblDirectory.nDirKey FROM tblDirectory RIGHT OUTER JOIN tblDirectoryRelation ON tblDirectory.nDirKey = tblDirectoryRelation.nDirParentId";
                    cSQL += " WHERE tblDirectory.cDirSchema = '" + nParentType + "' AND tblDirectoryRelation.nDirChildId = " + nChildId;
                    cReturn = ExeProcessSqlScalar(cSQL);
                    if (Information.IsNumeric(cReturn))
                        return cReturn;
                    else
                        return 0;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "DirParentByType", ex, ""));
                }

                return default;
            }

            public XmlElement getDirectoryParentsByType(long nId, DirectoryType oParentType)
            {
                PerfMonLog("DBHelper", "getDirectoryParentsByType");
                DataSet oDs;
                XmlDocument oXml;
                string cSql;
                string sContent;

                XmlElement oElmt = moPageXml.CreateElement("directory");
                string[] cDirectoryType = new string[] { "User", "Department", "Company", "Group", "Role" };

                cSql = "exec dbo.spGetDirParentByType @nDirId = " + nId + ", @cParentType = '" + cDirectoryType[oParentType] + "'";

                oDs = GetDataSet(cSql, "item", "directory");
                ReturnNullsEmpty(oDs);

                oXml = new XmlDataDocument(oDs);
                oDs.EnforceConstraints = false;


                foreach (XmlElement oElmt2 in oXml.SelectNodes("descendant-or-self::cDirXml"))
                {
                    sContent = oElmt2.InnerText;
                    if (!string.IsNullOrEmpty(sContent))
                    {
                        oElmt2.InnerXml = sContent;
                    }
                }

                if (oXml.FirstChild is not null)
                {
                    oElmt.InnerXml = oXml.FirstChild.InnerXml;
                }

                return oElmt;

            }

            public string passwordReminder(string cEmail)
            {

                PerfMonLog("DBHelper", "passwordReminder");
                string sSql;
                // Dim oDr As SqlDataReader
                string sReturn = "";
                string cProcessInfo = "";
                bool bValid;

                string sProjectPath = goConfig("ProjectPath") + "";
                string sSenderName = goConfig("SiteAdminName");
                string sSenderEmail = goConfig("SiteAdminEmail");


                try
                {
                    sSql = "select * from tblDirectory where cDirSchema = 'User' and cDirXml like '%<Email>" + Strings.LCase(cEmail) + "</Email>%'";

                    using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {
                        if (oDr.HasRows)
                        {
                            while (oDr.Read)
                            {
                                var oXmlDetails = new XmlDataDocument();
                                oXmlDetails.LoadXml(this.GetUserXML(oDr("nDirKey"), false).OuterXml);

                                // lets add the saved password to the xml
                                XmlElement oElmtPwd = oXmlDetails.CreateElement("Password");
                                oElmtPwd.InnerText = oDr("cDirPassword");
                                oXmlDetails.SelectSingleNode("User").AppendChild(oElmtPwd);






                                // now lets send the email
                                var oMsg = new Messaging(myWeb.msException);



                                // Set the language
                                if (moPageXml is not null && moPageXml.DocumentElement is not null && moPageXml.DocumentElement.HasAttribute("translang"))

                                {
                                    oMsg.Language = moPageXml.DocumentElement.GetAttribute("translang");
                                }




                                try
                                {
                                    var fsHelper = new Protean.fsHelper();
                                    string filePath = fsHelper.checkCommonFilePath("/xsl/email/passwordReminder.xsl");

                                    sReturn = oMsg.emailer(oXmlDetails.DocumentElement, goConfig("ProjectPath") + filePath, sSenderName, sSenderEmail, cEmail, "Password Reminder", "Your password has been emailed to you");




                                    bValid = true;
                                }
                                catch (Exception ex)
                                {
                                    sReturn = "Your email failed to send from password reminder";
                                    bValid = false;
                                }
                                oXmlDetails = default;

                            }
                        }
                        else
                        {
                            sReturn = "This user was not found";
                        }
                        return sReturn;
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "passwordReminder", ex, cProcessInfo));

                    return null;
                }
            }

            public bool checkUserUnique(string cUsername, long nCurrId = 0L)
            {
                PerfMonLog("DBHelper", "checkUserUnique");
                string sSql;
                // Dim oDr As SqlDataReader
                string cProcessInfo = "";

                try
                {
                    if (nCurrId > 0L)
                    {
                        sSql = "select * from tblDirectory where cDirSchema = 'User' and cDirName = '" + SqlFmt(cUsername) + "' and nDirKey <> " + nCurrId;
                    }
                    else
                    {
                        sSql = "select * from tblDirectory where cDirSchema = 'User' and cDirName = '" + SqlFmt(cUsername) + "'";
                    }

                    using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {
                        if (oDr.HasRows)
                        {
                            oDr.Close();

                            return false;
                        }
                        else
                        {
                            oDr.Close();

                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "checkUserExists", ex, cProcessInfo));

                }

                return default;
            }

            public bool checkEmailUnique(string cEmail, long nCurrId = 0L)
            {
                PerfMonLog("DBHelper", "checkEmailUnique");
                string sSql;
                // Dim oDr As SqlDataReader
                string cProcessInfo = "";

                try
                {

                    if (LCase(myWeb.moConfig("EmailUsernames")) == "on")
                    {
                        if (nCurrId > 0L)
                        {
                            sSql = "select * from tblDirectory where cDirSchema = 'User' and cDirEmail like '" + SqlFmt(cEmail) + "' and nDirKey <> " + nCurrId;
                        }
                        else
                        {
                            sSql = "select * from tblDirectory where cDirSchema = 'User' and cDirEmail like '" + SqlFmt(cEmail) + "'";
                        }
                    }
                    else if (nCurrId > 0L)
                    {
                        sSql = "select * from tblDirectory where cDirSchema = 'User' and cDirXml like '%<Email>" + SqlFmt(cEmail) + "</Email>%' and nDirKey <> " + nCurrId;
                    }
                    else
                    {
                        sSql = "select * from tblDirectory where cDirSchema = 'User' and cDirXml like '%<Email>" + SqlFmt(cEmail) + "</Email>%'";
                    }

                    using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {
                        if (oDr.HasRows)
                        {
                            oDr.Close();

                            if (nCurrId > 0L)
                            {
                                // fix for duplicate emails allready on the system
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            oDr.Close();

                            return true;
                        }
                    }
                    PerfMonLog("DBHelper", "checkEmailUnique", sSql);
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "checkUserExists", ex, cProcessInfo));

                }

                return default;
            }

            public bool checkUserRole(string cRoleName, string cSchemaName = "Role", long userId = 0L)
            {
                PerfMonLog("DBHelper", "checkUserRole");
                string sSql;
                // Dim oDr As SqlDataReader
                string cProcessInfo = "";
                var bValid = default(bool);
                if (userId == 0L)
                    userId = mnUserId;


                try
                {
                    // get group memberships
                    sSql = "SELECT d.* FROM tblDirectory d " + "inner join tblDirectoryRelation r on r.nDirParentId = d.nDirKey " + "where r.nDirChildId = " + userId + " and d.cDirSchema='" + cSchemaName + "'";

                    using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {
                        if (oDr is not null)
                        {
                            while (oDr.Read)
                            {
                                if (oDr("cDirName") == cRoleName)
                                {
                                    bValid = true;
                                }

                            }
                            oDr.Close();
                        }

                        return bValid;
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "checkUserRole", ex, cProcessInfo));

                }

                return default;
            }

            public XmlElement getUserXMLById(ref int nUserId)
            {
                PerfMonLog("DBHelper", "getUserXMLById");
                // Dim oDs As Data.DataSet
                // Dim odr As SqlDataReader
                XmlElement root;
                XmlElement oElmt;
                string sSql;

                string sProcessInfo = "";
                string cOverrideUserGroups;
                string cJoinType;
                try
                {
                    cOverrideUserGroups = goConfig("SearchAllUserGroups");

                    root = moPageXml.CreateElement("User");
                    root.SetAttribute("id", nUserId);
                    if (nUserId != 0)
                    {
                        // odr = getDataReader("SELECT * FROM tblDirectory where nDirKey = " & nUserId)
                        using (SqlDataReader oDr = getDataReaderDisposable("SELECT * FROM tblDirectory where nDirKey = " + nUserId))  // Done by nita on 6/7/22
                        {
                            while (oDr.Read)
                            {
                                root.SetAttribute("name", oDr("cDirName"));
                                if (oDr("cDirXml") != "")
                                {
                                    root.InnerXml = oDr("cDirXml");
                                    root.InnerXml = root.SelectSingleNode("*").InnerXml;
                                }
                            }
                        }
                        // get parent directory item memberships
                        if (cOverrideUserGroups == "on")
                        {
                            cJoinType = "LEFT";
                        }
                        else
                        {
                            cJoinType = "INNER";
                        }

                        sSql = "SELECT	d.*," + " r.nRelKey As Member" + " FROM	tblDirectory d" + " " + cJoinType + " JOIN tblDirectoryRelation r" + " ON r.nDirParentid = d.nDirKey " + " AND r.nDirChildId = " + nUserId + " WHERE   d.cDirSchema <> 'User'" + " ORDER BY d.cDirName";

                        using (SqlDataReader oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                        {
                            while (oDr.Read)
                            {
                                oElmt = moPageXml.CreateElement(oDr("cDirSchema"));
                                oElmt.SetAttribute("id", oDr("nDirKey"));
                                oElmt.SetAttribute("name", oDr("cDirName"));
                                if (!(oDr("Member") is DBNull))
                                {
                                    oElmt.SetAttribute("isMember", "yes");
                                }

                                if (!(cOverrideUserGroups == "on"))
                                {
                                    if (!(oDr("Member") is DBNull))
                                    {
                                        root.AppendChild(oElmt);
                                    }
                                }
                                else
                                {
                                    root.AppendChild(oElmt);
                                }
                            }
                        }
                    }

                    return root;
                }

                catch (Exception ex)
                {

                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getUserXMLById", ex, sProcessInfo));
                    return default;
                }


            }


            public long logActivity(ActivityType nActivityType, long nUserDirId, long nStructId, long nArtId = 0L, string cActivityDetail = "", string cForiegnRef = "")
            {
                string cSubName = "logActivity(ActivityType,Int,Int,[Int],[String])";
                try
                {
                    return logActivity(nActivityType, nUserDirId, nStructId, nArtId, 0, cActivityDetail, false, cForiegnRef);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, cSubName, ex, ""));
                }

                return default;

            }


            public long logActivity(ActivityType nActivityType, long nUserDirId, long nStructId, long nArtId, long nOtherId, string cActivityDetail)
            {
                return logActivity(nActivityType, nUserDirId, nStructId, nArtId, nOtherId, cActivityDetail, false);
            }

            public long logActivity(ActivityType loggedActivityType, long userDirId, long structId, long artId, long otherId, string activityDetail, bool removePreviousActivitiesFromCurrentSession, string cForiegnRef = null)
            {

                string cSubName = "logActivity(ActivityType,Int,Int,Int,Int,String,Boolean)";
                string sSql;
                string cProcessInfo = "";
                string sessionId = "";
                try
                {

                    // Find the Session ID
                    if (goSession is not null)
                    {
                        sessionId = goSession.SessionID;
                    }

                    if (removePreviousActivitiesFromCurrentSession & !string.IsNullOrEmpty(sessionId))
                    {
                        sSql = "DELETE FROM tblActivityLog WHERE cSessionId=" + SqlString(sessionId) + " AND nActivityType=" + SqlFmt(loggedActivityType);
                        ExeProcessSql(sSql);
                    }


                    if (gbIPLogging)
                    {
                        checkForIpAddressCol();
                    }


                    // Generate the SQL statement
                    // First add the values

                    var valuesList = new List<string>();
                    valuesList.Add(userDirId.ToString());
                    valuesList.Add(structId.ToString());
                    valuesList.Add(artId.ToString());
                    valuesList.Add(Protean.Tools.Database.SqlDate(DateTime.Now, true));
                    valuesList.Add(loggedActivityType);
                    valuesList.Add(SqlString(activityDetail));
                    valuesList.Add(SqlString(Interaction.IIf(string.IsNullOrEmpty(sessionId), "Service_" + DateTime.Now.ToString(), sessionId)));
                    if (otherId > 0L)
                        valuesList.Add(otherId.ToString());
                    if (gbIPLogging)
                        valuesList.Add(SqlString(Strings.Left(myWeb.moRequest.ServerVariables("REMOTE_ADDR"), 15)));

                    // Now build the SQL
                    sSql = "Insert Into tblActivityLog (nUserDirId, nStructId, nArtId, dDateTime, nActivityType, cActivityDetail, cSessionId";

                    // Handle optional columns
                    if (otherId > 0L)
                        sSql += ",nOtherId";
                    if (gbIPLogging)
                        sSql += ",cIPAddress";

                    if (checkTableColumnExists("tblActivityLog", "cForeignRef"))
                    {
                        sSql += ",cForeignRef";
                        valuesList.Add(SqlString(cForiegnRef));
                    }

                    sSql += ") values (" + string.Join(",", valuesList.ToArray()) + ")";

                    return GetIdInsertSql(sSql);
                }


                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, cSubName, ex, cProcessInfo));

                }

                return default;

            }

            public void updateActivity(long activityID, string cActivityDetail)
            {
                try
                {

                    string sSql;

                    sSql = "Update tblActivityLog set cActivityDetail = '" + cActivityDetail + "', dDateTime = " + sqlDateTime(DateTime.Now) + " where nActivityKey = " + activityID;

                    ExeProcessSql(sSql);
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "updateActivity", ex, cActivityDetail));
                }
            }


            public void checkForIpAddressCol()
            {

                // check for gbIPLogging column
                var strSqlCheckForIpCol = new StringBuilder();
                strSqlCheckForIpCol.Append("if NOT Exists(select * from sys.columns where Name = 'cIPAddress' and Object_ID = Object_ID('tblActivityLog')) ");
                strSqlCheckForIpCol.Append("begin ");
                strSqlCheckForIpCol.Append("alter table tblActivityLog add cIPAddress nvarchar(15) NULL  ");
                strSqlCheckForIpCol.Append("end ");

                ExeProcessSql(strSqlCheckForIpCol.ToString());


            }



            public bool AllowMigration()
            {
                PerfMonLog("DBHelper", "AllowMigration");
                object oMigration;
                try
                {
                    oMigration = GetDataValue("SELECT cLkpValue from tblLookup where cLkpKey = 'MigrationDone'");
                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oMigration, "1", false)))
                    {
                        return false;
                    }
                    else
                    {
                        ExeProcessSql("INSERT INTO tblLookup (cLkpValue,cLkpKey,cLkpCategory) VALUES (1,'MigrationDone','MigrationDone')");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "AllowMigration", ex, "AllowMigration"));

                }

                return default;
            }

            public int FindDirectoryByForiegn(string ForiegnRef) // returns the id of the Dir Entry with the Foreign Ref
            {
                PerfMonLog("DBHelper", "FindDirectoryByForiegn");
                try
                {

                    string strSQL = "Select nDirKey FROM tblDirectory WHERE cDirForiegnRef = '" + SqlFmt(ForiegnRef) + "'";
                    int iID;
                    iID = ExeProcessSqlScalar(strSQL);
                    return iID;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "FindDirectoryByForiegn", ex, "AllowMigration"));
                    return -1;

                }
            }

            public bool isParent(int pageId)
            {
                PerfMonLog("DBHelper", "FindpageIsParent");
                try
                {
                    DataSet oDs;
                    bool result = false;
                    // check for page-subpage relation
                    string strSQL = "select * from tblcontentstructure P inner join  tblcontentstructure C on p.nStructKey = C.nStructParId where p.nStructKey= '" + pageId + "'";
                    oDs = GetDataSet(strSQL, "page", "Page");
                    if (oDs.Tables(0).Rows.Count == 0)
                    {
                        // check for page-product relation
                        strSQL = "select * from tblcontentstructure P inner join  tblContentLocation C on p.nStructKey = C.nStructId where p.nStructKey= '" + pageId + "'";
                        oDs = GetDataSet(strSQL, "page", "Page");
                    }
                    if (oDs.Tables(0).Rows.Count > 0)
                    {
                        result = true;
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "FindDirectoryByForiegn", ex, "AllowMigration"));
                    return false;

                }
            }

            public void ListOrders(ref XmlElement oContentsXML, Cart.cartProcess ProcessId, string cSchemaName)
            {
                PerfMonLog("DBHelper", "ListOrders");
                XmlElement oRoot;
                XmlElement oElmt;
                string sSql;
                DataSet oDs;
                string cProcessInfo = "";
                try
                {

                    oRoot = moPageXml.CreateElement("Content");
                    oRoot.SetAttribute("type", "listTree");
                    oRoot.SetAttribute("template", "default");
                    oRoot.SetAttribute("name", cSchemaName + "s - " + ProcessId.GetType.ToString);
                    if (ProcessId == 0)
                    {
                        sSql = "SELECT nCartOrderKey as id, nCartStatus as status, c.cContactName as name, c.cContactEmail as email, a.dUpdateDate from tblCartOrder inner join tblAudit a on nAuditId = a.nAuditKey left outer join tblCartContact c on (nCartOrderKey = c.nContactCartId and cContactType = 'Billing Address') where (cCartSchemaName= '" + cSchemaName + "') order by nCartOrderKey desc";
                    }
                    else
                    {
                        sSql = "SELECT nCartOrderKey as id, nCartStatus as status, c.cContactName as name, c.cContactEmail as email, a.dUpdateDate from tblCartOrder inner join tblAudit a on nAuditId = a.nAuditKey left outer join tblCartContact c on (nCartOrderKey = c.nContactCartId and cContactType = 'Billing Address') where nCartStatus = " + ProcessId + " and (cCartSchemaName= '" + cSchemaName + "') order by nCartOrderKey desc";
                    }


                    oDs = GetDataSet(sSql, cSchemaName, "List");

                    if (oDs.Tables(0).Rows.Count > 0)
                    {
                        oDs.Tables(0).Columns(0).ColumnMapping = System.Data.MappingType.Attribute;
                        oDs.Tables(0).Columns(1).ColumnMapping = System.Data.MappingType.Attribute;
                        oDs.Tables(0).Columns(2).ColumnMapping = System.Data.MappingType.Attribute;
                        oDs.Tables(0).Columns(3).ColumnMapping = System.Data.MappingType.Attribute;
                        oDs.Tables(0).Columns(4).ColumnMapping = System.Data.MappingType.Attribute;

                        // load existing data into the instance
                        oElmt = moPageXml.CreateElement("List");
                        oElmt.InnerXml = oDs.GetXml;

                        oContentsXML.AppendChild(oElmt);
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ListShippingLocations", ex, cProcessInfo));
                }
            }



        }
    }
}
