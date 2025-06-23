// ***********************************************************************
// $Library:     eonic.dbhelper
// $Revision:    3.1  
// $Date:        2006-03-02
// $Author:      Trevor Spink (trevor@eonic.co.uk)
// &Website:     www.eonic.co.uk
// &Licence:     All Rights Reserved.
// $Copyright:   Copyright (c) 2002 - 2024 Trevor Spink Consultants Ltd.
// ***********************************************************************

using AngleSharp.Dom;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Protean.Providers.Authentication;
using Protean.Providers.Membership;
using Protean.Providers.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.FtpClient.Extensions;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Configuration;
using System.Xml;
using static Protean.Cms.dbHelper;
using static Protean.Cms.dbImport;
using static Protean.stdTools;
using static Protean.Tools.Xml;

namespace Protean
{



    public partial class Cms
    {

        // Inherits dbTools
        public class dbHelper : Tools.Database
        {

            #region New Error Handling
            public new event OnErrorEventHandler OnError;

            public new delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);

            private void _OnError(object sender, Tools.Errors.ErrorEventArgs e)
            {
                OnError?.Invoke(sender, e);
            }

            #endregion

            #region Declarations

            private const string mcModuleName = "Eonic.dbHelper";

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

            private IMessagingProvider moMessaging;

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
                    mnUserId = (long)myWeb.mnUserId;

                    if (myWeb != null)
                    {
                        gbVersionControl = myWeb.gbVersionControl;
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""));
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
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getDBAuth", ex, ""));
                    return null;
                }
            }


            public dbHelper(string cConnectionString, long nUserId, System.Web.HttpContext moCtx = null)
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
                    ConnectionPooling = true;
                    ConnectTimeout = 15;
                    MinPoolSize = 0;
                    MaxPoolSize = 100;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""));
                }

                base.OnError += _OnError;
            }

            public dbHelper(string cDbServer, string cDbName, long nUserId, System.Web.HttpContext moCtx = null) : base()
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
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""));
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
                    DatabaseName = ocon.Database;
                    DatabaseServer = ocon.DataSource;
                    ocon.Close();
                    ocon = null;

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
                    cUsername = Tools.Text.SimpleRegexFind(cDBAuth, "user id=([^;]*)", 1, RegexOptions.IgnoreCase);
                    cPassword = Tools.Text.SimpleRegexFind(cDBAuth, "password=([^;]*)", 1, RegexOptions.IgnoreCase);

                    DatabaseUser = cUsername;
                    DatabasePassword = cPassword;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ResetConnection", ex, ""));
                }
            }


            #endregion
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
                PageCacheDeleted = 64,

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
                OrderStatusChange = 400,

                //Delete file
                DeleteFileActivity = 401

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
                catch (Exception)
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
                catch (Exception)
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
                catch (Exception)
                {
                    return false;
                }
            }

            #endregion
            #region Table Definition Procedures
            public string TableKey(TableNames cTableName)
            {
                //return getKey(Enum.Parse(typeof(TableNames), cTableName.ToString()));
                return getKey((int)cTableName);
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
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getNameByKey", ex, cProcessInfo));
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

            //protected internal string getKey(objectTypes objectType)
            protected internal string getKey(int tablekey)
            {

                string strReturn = "";
                switch (tablekey)
                {
                    case (int)TableNames.tblContent:
                        {
                            strReturn = "nContentKey";
                            break;
                        }
                    case (int)TableNames.tblContentLocation:
                        {
                            strReturn = "nContentLocationKey"; // "nContentId"
                            break;
                        }
                    case (int)TableNames.tblContentRelation:
                        {
                            strReturn = "nContentRelationKey";
                            break;
                        }
                    case 3:
                    case 29:
                        {
                            strReturn = "nStructKey";
                            break;
                        }
                    case 4:
                        {
                            strReturn = "nDirKey";
                            break;
                        }
                    case 5:
                        {
                            strReturn = "nRelKey";
                            break;
                        }
                    case 6:
                        {
                            strReturn = "nPermKey";
                            break;
                        }
                    case 7:
                        {
                            strReturn = "nQResultsKey";
                            break;
                        }
                    case 8:
                        {
                            strReturn = "nQDetailKey";
                            break;
                        }
                    case 9:
                        {
                            strReturn = "nAuditKey";
                            break;
                        }
                    case 10:
                        {
                            strReturn = "nCourseResultKey";
                            break;
                        }
                    case 11:
                        {
                            strReturn = "nActivityKey";
                            break;
                        }
                    case 12:
                        {
                            strReturn = "nCartOrderKey";
                            break;
                        }
                    case 13:
                        {
                            strReturn = "nCartItemKey";
                            break;
                        }
                    case 14:
                        {
                            strReturn = "nContactKey";
                            break;
                        }
                    case 15:
                        {
                            strReturn = "nLocationKey";
                            break;
                        }
                    case 16:
                        {
                            strReturn = "nShipOptKey";
                            break;
                        }
                    case 17:
                        {
                            strReturn = "nShpRelKey";
                            break;
                        }
                    case 18:
                        {
                            return "nCatProductRelKey";
                        }
                    case 19:
                        {
                            return "nDiscountKey";
                        }
                    case 20:
                        {
                            return "nDiscountDirRelationKey";
                        }
                    case 21:
                        {
                            return "nDiscountProdCatRelationKey";
                        }
                    case 22:
                        {
                            return "nCatKey";
                        }
                    case 23:
                        {
                            return "nActionKey";
                        }
                    case 24:
                        {
                            return "nSubKey";
                        }
                    case 25:
                        {
                            return "nPayMthdKey";
                        }
                    case 26:
                        {
                            return "nCodeKey";
                        }
                    case 27:
                        {
                            return "nContentVersionKey";
                        }
                    case 28:
                        {
                            return "nCartShippingPermissionKey";
                        }
                    // Case 29 'duplicate, but leave this
                    // Return "nStructKey" 
                    case 30:
                        {
                            return "nLkpID";
                        }
                    case 31:
                        {
                            return "nDeliveryKey";
                        }
                    case 32:
                        {
                            return "nCarrierKey";
                        }
                    case 33:
                        {
                            return "nSubRenewalKey";
                        }
                    case 34:
                        {
                            return "nCartPaymentKey";
                        }
                    // 100-199 reserved for LMS
                    case 100:
                        {
                            return "nCpdLogKey";
                        }
                    case 101:
                        {
                            return "nCertificateKey";
                        }

                    // 200- reserved for [next thing]
                    // Add new key id for Index def table by nita
                    case 200:
                        {
                            return "nContentIndexDefKey";
                        }
                    case 202:
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
                                sSql = "select " + getNameFname(objectType) + " from " + getTable(objectType) + " where " + getKey((int)objectType) + " = " + nKey;
                                using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                                {

                                    while (oDr.Read())
                                        sResult = Conversions.ToString(oDr[0]);
                                }

                                break;
                            }
                    }

                    return sResult;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getNameByKey", ex, cProcessInfo));
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
                            return "cContactForiegnRef";
                        }
                    case (objectTypes)15:
                        {
                            return "cLocationForiegnRef";
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
                        appcheckName = Conversions.ToString(moCtx.Application["getdb-" + dbObjectName]);
                        if (string.IsNullOrEmpty(appcheckName))
                        {

                            if (checkDBObjectExists("b" + dbObjectName))
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

                catch (Exception)
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

                                sSql = "select " + getKey((int)objectType) + " from " + getTable(objectType) + " where " + getNameFname(objectType) + " LIKE '" + cName + "'";
                                using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                                {
                                    if (oDr.HasRows)
                                    {
                                        while (oDr.Read())
                                            sResult = Conversions.ToString(oDr[0]);
                                    }
                                }

                                break;
                            }

                    }

                    return sResult;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getNameByKey", ex, cProcessInfo));
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
                    sSql = "select nAuditId from " + getTable(objecttype) + " where " + getKey((int)objecttype) + " = " + nId;
                    using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {

                        while (oDr.Read())
                            nAuditId = Conversions.ToLong(oDr[0]);

                    }
                    // we could test the current status to see if the change is a valid one. I.E. live can only be hidden not moved back to approval or InProgress, but the workflow should ensure this doesn't happen.
                    sSql = "select nStatus from tblAudit WHERE nAuditKey =" + nAuditId;
                    sResult = ExeProcessSqlScalar(sSql);

                    return Conversions.ToLong(sResult);
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getObjectStatus", ex, cProcessInfo));
                    return Conversions.ToLong("");
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
                    sSql = "select nAuditId from " + getTable(objecttype) + " where " + getKey((int)objecttype) + " = " + nId;
                    // we want to touch the parent table just incase we have any triggers asscoiated with it
                    // change get DateReader to getdataset and update.
                    using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {

                        while (oDr.Read())
                            nAuditId = Conversions.ToLong(oDr[0]);

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
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "setObjectStatus", ex, cProcessInfo));
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
                    sSql = "select nAuditId from " + getTable(objecttype) + " where " + getKey((int)objecttype) + " = " + nId;
                    using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {

                        while (oDr.Read())
                            nAuditId = Conversions.ToLong(oDr[0]);

                    }
                    sSql = "select nStatus from tblAudit WHERE nAuditKey =" + nAuditId;
                    sResult = Conversions.ToInteger(myWeb.moDbHelper.ExeProcessSqlScalar(sSql));

                    return sResult.ToString();
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "setObjectStatus", ex, cProcessInfo));
                    return "";
                }

            }

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
                var statusChange = ActivityType.Undefined;
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
                            logActivity(statusChange, (long)myWeb.mnUserId, 0L, 0L, Convert.ToInt16(auditId), "");
                        }

                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "logObjectStatusChange", ex, cProcessInfo));
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
                    string ItemIdPath = "";
                    sPath = sFullPath;
                    sPath = goServer.UrlDecode(sPath);

                    // We have to assume that hyphens are spaces here
                    // Nore : if this is turned on, you will have to update any pages that have hyphens in their names
                    if (myWeb.moConfig["PageURLFormat"] == "hyphens")
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

                    sProcessInfo = "split to path array";
                    aPath = Strings.Split(sPath, "/");
                    if (Information.UBound(aPath) > 0)
                    {
                        // get the last in line
                        sPath = aPath[Information.UBound(aPath)];
                        ItemIdPath = aPath[Information.UBound(aPath) - 1];
                        if (ItemIdPath.EndsWith("-"))
                        {
                            nArtId = Convert.ToInt64(ItemIdPath.Replace("-", ""));
                        }
                    }
                    else
                    {
                        sPath = sPath;
                    }



                    switch (myWeb.moConfig["DetailPathType"] ?? "")
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
                                    if (!string.IsNullOrEmpty(myWeb.moRequest["artid"]))
                                    {
                                        nArtId = (long)myWeb.GetRequestItemAsInteger("artid", 0);
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

                                // if (nArtId > 0L & !gbAdminMode)
                                if (nArtId > 0L)
                                {
                                    // article id was passed in the url so we may need to redirect

                                    sSql = "select cContentSchemaName, cContentName from tblContent c inner join tblAudit a on a.nAuditKey = c.nAuditId where nContentKey = " + nArtId;
                                    string contentType = "";
                                    string contentName = "";
                                    string redirectUrl = "";

                                    using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                                    {

                                        while (oDr.Read())
                                        {
                                            contentType = Conversions.ToString(oDr[0]);
                                            contentName = Conversions.ToString(oDr[1]);
                                        }
                                    }

                                    var loopTo1 = prefixs.Length - 1;
                                    for (i = 0; i <= loopTo1; i++)
                                    {

                                        thisPrefix = prefixs[i].Substring(0, prefixs[i].IndexOf("/"));
                                        thisContentType = prefixs[i].Substring(prefixs[i].IndexOf("/") + 1, prefixs[i].Length - prefixs[i].IndexOf("/") - 1);
                                        if ((contentType ?? "") == (thisContentType ?? ""))
                                        {
                                            if (myWeb.moConfig["addPathArtId"] == "on")
                                            {
                                                ItemIdPath = nArtId + "-/";
                                            }

                                            redirectUrl += "/" + thisPrefix + "/" + ItemIdPath + Protean.Tools.Text.CleanName(contentName).Replace(" ", "-").Trim('-');
                                            if (myWeb.moConfig["DetailPathTrailingSlash"] == "on")
                                            {
                                                redirectUrl = redirectUrl + "/";
                                            }
                                        }
                                    }
                                    char[] charsToTrim = { '/' };
                                    string originalPath = myWeb.mcOriginalURL.Split('?')[0].TrimEnd(charsToTrim);
                                    if ((originalPath.ToLower() ?? "") != (redirectUrl.ToLower() ?? ""))
                                    {
                                        myWeb.msRedirectOnEnd = redirectUrl;
                                    }

                                }
                                else
                                {
                                    // We don't have an article id so we need to find it based on the final string in the path.
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
                                        string cContentName = SqlFmt(sPath).Replace("+", "_").Replace("*", "_").Replace(" ", "_").Replace("__", "%").Replace("___", "%");

                                        if (gbAdminMode)
                                        {
                                            // replace * with wildcard as content names replace / with * in cleanname
                                            sSql = "select TOP(1) nContentKey  from tblContent c inner join tblAudit a on a.nAuditKey = c.nAuditId where cContentName like '" + cContentName + "' and cContentSchemaName like '" + thisContentType + "' order by nVersion desc";
                                        }
                                        else
                                        {
                                            sSql = "select TOP(1) nContentKey from tblContent c inner join tblAudit a on a.nAuditKey = c.nAuditId where cContentName like '" + cContentName + "' and cContentSchemaName like '" + thisContentType + "' " + myWeb.GetStandardFilterSQLForContent() + " order by nVersion desc";
                                        }
                                        ods = GetDataSet(sSql, "Content");
                                        if (ods != null)
                                        {
                                            if (ods.Tables["Content"].Rows.Count == 1)
                                            {
                                                nArtId = Conversions.ToLong(ods.Tables["Content"].Rows[Conversions.ToInteger("0")]["nContentKey"]);
                                            }
                                            else
                                            {
                                                // handling for content versions ?
                                            }
                                        }

                                        // we want to redirect to path with id
                                        if (myWeb.moConfig["addPathArtId"] == "on" && nArtId > 0)
                                        {
                                            ItemIdPath = nArtId + "-/";
                                            string redirectUrl = "/" + thisPrefix + "/" + ItemIdPath + sPath;
                                            if (myWeb.moConfig["DetailPathTrailingSlash"] == "on")
                                            {
                                                redirectUrl = redirectUrl + "/";
                                            }
                                            myWeb.msRedirectOnEnd = redirectUrl;
                                        }
                                    }
                                }
                                // now get the page id 
                                if (nArtId > 0L)
                                {
                                    sSql = "select nStructId from tblContentLocation where bPrimary = 1 and nContentId = " + nArtId;
                                    //TS Why were these two lines commented out? They are required if a product is being shown that is not on a page but its parent product is
                                    //  sSql = sSql + " union ";
                                    //  sSql = sSql + " select nStructId from tblContentLocation where bPrimary = 1 and nContentId IN(select cl.nContentParentId from tblContentRelation cl where cl.nContentChildId = " + nArtId + ")";

                                    ods = GetDataSet(sSql, "Pages");
                                    if (ods.Tables["Pages"].Rows.Count > 0)
                                    {
                                        if (bCheckPermissions)
                                        {
                                            // Check the permissions for the page - this will either return 0, the page id or a system page.
                                            long checkPermissionPageId = checkPagePermission(Conversions.ToLong(ods.Tables["Pages"].Rows[0]["nStructId"]));
                                            if (Conversions.ToBoolean(Operators.AndObject(checkPermissionPageId != 0L, Operators.OrObject(Operators.ConditionalCompareObjectEqual(ods.Tables["Pages"].Rows[Conversions.ToInteger("0")]["nStructId"], checkPermissionPageId, false), IsSystemPage(checkPermissionPageId)))))

                                            {
                                                nPageId = checkPermissionPageId;
                                            }
                                        }
                                        else
                                        {
                                            nPageId = Conversions.ToLong(ods.Tables["Pages"].Rows[0]["nStructId"]);
                                        }
                                        //nPageId = Conversions.ToLong(ods.Tables["Pages"].Rows[0]["nStructId"]);
                                    }
                                    else
                                    {
                                        // get page if product related to product

                                        sSql = " select nStructId from tblContentLocation where bPrimary = 1 and nContentId IN(select cl.nContentParentId from tblContentRelation cl where cl.nContentChildId = " + nArtId + ")";
                                        ods = GetDataSet(sSql, "Pages");
                                        if (bCheckPermissions)
                                        {
                                            // Check the permissions for the page - this will either return 0, the page id or a system page.
                                            long checkPermissionPageId = checkPagePermission(Conversions.ToLong(ods.Tables["Pages"].Rows[0]["nStructId"]));
                                            if (Conversions.ToBoolean(Operators.AndObject(checkPermissionPageId != 0L, Operators.OrObject(Operators.ConditionalCompareObjectEqual(ods.Tables["Pages"].Rows[Conversions.ToInteger("0")]["nStructId"], checkPermissionPageId, false), IsSystemPage(checkPermissionPageId)))))

                                            {
                                                nPageId = checkPermissionPageId;
                                            }
                                        }
                                        else
                                        {
                                            nPageId = Conversions.ToLong(ods.Tables["Pages"].Rows[0]["nStructId"]);
                                        }
                                    }

                                    if (checkRedirect)
                                    {
                                        string redirectUrl = "";
                                        if (myWeb.moConfig["addPathArtId"] == "on")
                                        {
                                            ItemIdPath = nArtId + "-/";
                                        }

                                        redirectUrl += "/" + thisPrefix + "/" + ItemIdPath + sPath.ToString().Replace(" ", "-").Trim('-') + "/";

                                        char[] charsToTrim = { '/' };
                                        myWeb.mcOriginalURL = myWeb.mcOriginalURL.TrimEnd(charsToTrim);
                                        if ((myWeb.mcOriginalURL.ToLower() ?? "") != (redirectUrl.ToLower() ?? ""))
                                        {
                                            myWeb.msRedirectOnEnd = redirectUrl;
                                        }

                                        if ((sFullPath ?? "") != (redirectUrl ?? ""))
                                        {
                                            myWeb.msRedirectOnEnd = redirectUrl;
                                        }
                                    }

                                }
                                break;
                            }
                    }

                    /// if (nArtId == default)
                    ///  {
                    sSql = "select nStructKey, nStructParId, nVersionParId, cVersionLang from tblContentStructure where (cStructName like '" + SqlFmt(sPath) + "' or cStructName like '" + SqlFmt(Strings.Replace(sPath, " ", "")) + "' or cStructName like '" + SqlFmt(Strings.Replace(sPath, " ", "-")) + "')";

                    ods = GetDataSet(sSql, "Pages");

                    if (ods != null)
                    {
                        if (ods.Tables["Pages"].Rows.Count == 1)
                        {
                            nPageId = Conversions.ToLong(ods.Tables["Pages"].Rows[Conversions.ToInteger("0")]["nStructKey"]);
                        }
                        // if there is just one page validate it
                        else if (ods.Tables["Pages"].Rows.Count == 0)
                        {
                        }

                        // do nothing nothing found

                        else
                        {
                            foreach (DataRow oRow in ods.Tables["Pages"].Rows)
                            {
                                // Debug.WriteLine(oRow.Item("nStructKey"))
                                if (!(Conversions.ToInteger(Operators.ConcatenateObject("0", oRow["nVersionParId"])) == 0))
                                {
                                    // we have a language verion we need to behave differently to confirm id
                                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(myWeb.mcPageLanguage, oRow["cVersionLang"], false)))
                                    {
                                        nPageId = Conversions.ToLong(oRow["nStructKey"]);
                                        break;
                                    }
                                }
                                else
                                {
                                    int argnStep = Information.UBound(aPath) - 1;
                                    if (recurseUpPathArray(Conversions.ToInteger(oRow["nStructParId"]), ref aPath, ref argnStep) == true)
                                    {
                                        if (bCheckPermissions)
                                        {

                                            // Check the permissions for the page - this will either return 0, the page id or a system page.
                                            long checkPermissionPageId = checkPagePermission(Conversions.ToLong(oRow["nStructKey"]));

                                            if (Conversions.ToBoolean(Operators.AndObject(checkPermissionPageId != 0L, Operators.OrObject(Operators.ConditionalCompareObjectEqual(oRow["nStructKey"], checkPermissionPageId, false), IsSystemPage(checkPermissionPageId)))))

                                            {
                                                nPageId = checkPermissionPageId;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            nPageId = Conversions.ToLong(oRow["nStructKey"]);
                                            break;
                                        }
                                    }
                                }

                            }
                        }
                        //   }

                        // Note : if sPath is empty the SQL call above WILL return pages, we don't want these, we want top level pgid
                        if (!(nPageId > 1L && !string.IsNullOrEmpty(sPath)))
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
                                nPageId = (long)myWeb.RootPageId;
                            }
                        }

                        // If bSetGlobalPageVariable Then gnPageId = nPageId

                        // myWeb.PerfMon.Log("dbHelper", "getPageAndArticleIdFromPath-end")

                        return (int)nPageId;
                    }
                }
                catch (Exception ex)
                {

                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getPageAndArticleIdFromPath", ex, sProcessInfo));

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
                    if (myWeb.moConfig["PageURLFormat"] == "hyphens")
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


                    if (!(ods == null) && ods.Tables["Pages"].Rows.Count == 1)
                    {
                        nPageId = Conversions.ToInteger(ods.Tables["Pages"].Rows[Conversions.ToInteger("0")]["nStructKey"]);
                    }
                    // if there is just one page validate it
                    else if (ods.Tables["Pages"].Rows.Count == 0)
                    {
                    }

                    // do nothing nothing found

                    else
                    {
                        foreach (DataRow oRow in ods.Tables["Pages"].Rows)
                        {
                            // Debug.WriteLine(oRow.Item("nStructKey"))
                            if (!(Conversions.ToInteger(Operators.ConcatenateObject("0", oRow["nVersionParId"])) == 0))
                            {
                                // we have a language verion we need to behave differently to confirm id
                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(myWeb.mcPageLanguage, oRow["cVersionLang"], false)))
                                {
                                    nPageId = Conversions.ToInteger(oRow["nStructKey"]);
                                    break;
                                }
                            }
                            else
                            {
                                int argnStep = Information.UBound(aPath) - 1;
                                if (recurseUpPathArray(Conversions.ToInteger(oRow["nStructParId"]), ref aPath, ref argnStep) == true)
                                {
                                    if (bCheckPermissions)
                                    {

                                        // Check the permissions for the page - this will either return 0, the page id or a system page.
                                        long checkPermissionPageId = checkPagePermission(Conversions.ToLong(oRow["nStructKey"]));

                                        if (Conversions.ToBoolean(Operators.AndObject(checkPermissionPageId != 0L, Operators.OrObject(Operators.ConditionalCompareObjectEqual(oRow["nStructKey"], checkPermissionPageId, false), IsSystemPage(checkPermissionPageId)))))

                                        {
                                            nPageId = (int)checkPermissionPageId;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        nPageId = Conversions.ToInteger(oRow["nStructKey"]);
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
                            nPageId = (int)myWeb.gnPageNotFoundId;
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

                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getPageIdFromPath", ex, sProcessInfo));

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
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "IsSystemPage", ex, ""));
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

                    if (Cms.gbClone)
                    {
                        // If the page is cloned then we need to look at the page that it's cloned from
                        long nClonePageId = Conversions.ToLong(GetDataValue("select nCloneStructId from tblContentStructure where nStructKey = " + nPageId, CommandType.Text, null, 0));
                        if (nClonePageId > 0L)
                            nPageId = nClonePageId;
                    }

                    cSql = "select cStructLayout from  tblContentStructure where nStructKey = " + nPageId;
                    cLayout = Conversions.ToString(GetDataValue(cSql, CommandType.Text, null, "default"));
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getPageLayout", ex, cSql));
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

                    if (Cms.gbClone)
                    {
                        // If the page is cloned then we need to look at the page that it's cloned from
                        long nClonePageId = Conversions.ToLong(GetDataValue("select nCloneStructId from tblContentStructure where nStructKey = " + nPageId));
                        if (nClonePageId > 0L)
                            nPageId = nClonePageId;
                    }

                    cSql = "select cVersionLang from  tblContentStructure where nStructKey = " + nPageId;
                    cLayout = GetDataValue(cSql, CommandType.Text, null, "default").ToString();
                    if (string.IsNullOrEmpty(cLayout))
                        cLayout = "en-gb";
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getPageLang", ex, cSql));
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
                        using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                        {

                            if (!oDr.HasRows)
                            {
                                recurseUpPathArrayRet = false;
                            }
                            else
                            {
                                oDr.Read();
                                long nParId = Conversions.ToLong(oDr["nStructParId"]);
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
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getPageIdFromPath", ex, sProcessInfo));
                }

                return recurseUpPathArrayRet;
            }

            public int checkPagePermission(long nPageId)
            {
                PerfMonLog("DBHelper", "checkPagePermission");
                string sProcessInfo = "";
                long nAuthGroup = (long)Cms.gnAuthUsers;
                try
                {
                    // if we are not in admin mode
                    if (!gbAdminMode)
                    {
                        // Check we have access to this page
                        long nAuthUserId;
                        if (mnUserId == 0L & Cms.gnNonAuthUsers != 0)
                        {

                            // Note : if we are checking permissions for a page, and we're not logged in, then we shouldn't check with the gnAuthUsers group
                            // Ratehr, we should use the gnNonAuthUsers user group if it exists.

                            nAuthUserId = (long)Cms.gnNonAuthUsers;
                            nAuthGroup = (long)Cms.gnNonAuthUsers;
                        }

                        else if (mnUserId == 0L)
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
                                if (mnUserId > 0L)
                                {
                                    nPageId = myWeb.gnPageAccessDeniedId;
                                }
                                else
                                {
                                    myWeb.moSession["LogonRedirect"] = myWeb.mcOriginalURL;
                                    nPageId = myWeb.gnPageLoginRequiredId;
                                }
                            }

                            if (Conversions.ToBoolean(Operators.AndObject(Operators.ConditionalCompareObjectEqual(oPerm, "VIEW by Authenticated Users", false), mnUserId == 0L)))
                            {
                                myWeb.moSession["LogonRedirect"] = myWeb.mcPagePath;
                                nPageId = myWeb.gnPageLoginRequiredId;
                            }

                        }
                        return (int)nPageId;
                    }
                    else
                    {
                        return (int)nPageId;
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getPageIdFromPath", ex, sProcessInfo));
                }
                PerfMonLog("DBHelper", "checkPagePermission-end");
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
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "checkPageExist", ex, sProcessInfo));
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
                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(myWeb.moSession["ewAuth"], Tools.Encryption.HashString(myWeb.moSession.SessionID + goConfig["AdminPassword"], Strings.LCase(myWeb.moConfig["MembershipEncryption"]), true), false)))
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
                        if (mnUserId == 0L & Cms.gnNonAuthUsers != 0)
                        {
                            nAuthUserId = (long)Cms.gnNonAuthUsers;
                        }
                        else
                        {
                            nAuthUserId = mnUserId;
                        }
                        string sPerm;
                        string sSql = "SELECT dbo.fxn_checkPermission(" + nPageId + ", " + nAuthUserId + "," + Cms.gnAuthUsers + ") AS perm";
                        sPerm = Conversions.ToString(GetDataValue(sSql));
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
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getPagePermissionLevel", ex, sProcessInfo));
                }

                return default;
            }


            public PermissionLevel getContentPermissionLevel(long nContentId, long nPageId)
            {

                PerfMonLog("DBHelper", "getContentPermissionLevel");


                string sProcessInfo = "";
                XmlElement oElmt;
                var oPerm = PermissionLevel.Denied;
                try
                {

                    XmlElement argContentNode = null;
                    oElmt = (XmlElement)getLocationsByContentId(nContentId, ContentNode: ref argContentNode);

                    foreach (XmlElement oElmt2 in oElmt.SelectNodes("Location"))
                    {
                        if (oElmt2.GetAttribute("primary") == "true" & Conversions.ToDouble(oElmt2.GetAttribute("pgid")) == nPageId)
                        {
                            oPerm = getPagePermissionLevel(nPageId);
                        }
                    }

                    // if the content is orphan then get permission from homepage
                    if (oElmt.SelectNodes("Location").Count == 0)
                    {
                        oPerm = getPagePermissionLevel(Conversions.ToLong(myWeb.moConfig["RootPageId"]));
                    }

                    return oPerm;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getContentPermissionLevel", ex, sProcessInfo));
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
                        case objectTypes.CartShippingLocation:
                            {
                                string cSQL = "SELECT nLocationKey FROM tblCartShippingLocations WHERE nLocationParId = " + nId;
                                using (var oDataReader = getDataReaderDisposable(cSQL))
                                {
                                    while (oDataReader.Read())
                                        DeleteObject(objectTypes.CartShippingLocation, Conversions.ToLong(oDataReader[0]));
                                }
                                cSQL = "SELECT nAuditId FROM tblCartShippingLocations WHERE nLocationKey = " + nId;
                                DeleteObject(objectTypes.Audit, Conversions.ToLong(ExeProcessSqlScalar(cSQL)));
                                break;
                            }
                        case objectTypes.CartDiscountDirRelations:
                            {
                                sSql = "Select  nAuditId From tblCartDiscountDirRelations Where nDiscountDirRelationKey = " + nId;
                                DeleteObject(objectTypes.Audit, Conversions.ToLong(ExeProcessSqlScalar(sSql)));
                                break;
                            }

                        case objectTypes.CartDiscountProdCatRelations:
                            {
                                sSql = "Select  nAuditId From tblCartDiscountProdCatRelations Where nDiscountProdCatRelationKey = " + nId;
                                DeleteObject(objectTypes.Audit, Conversions.ToLong(ExeProcessSqlScalar(sSql)));
                                break;
                            }

                        case objectTypes.CartDiscountRules:
                            {
                                sSql = "Select  nAuditId From tblCartDiscountRules Where nDiscountKey = " + nId;
                                DeleteObject(objectTypes.Audit, Conversions.ToLong(ExeProcessSqlScalar(sSql)));
                                sSql = "Select nDiscountDirRelationKey FROM tblCartDiscountDirRelations WHERE nDiscountId = " + nId;
                                using (var oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read())
                                        DeleteObject(objectTypes.CartDiscountDirRelations, Conversions.ToLong(oDr[0]));
                                }

                                sSql = "Select nDiscountProdCatRelationKey FROM tblCartDiscountProdCatRelations WHERE nDiscountId = " + nId;
                                using (var oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read())
                                        DeleteObject(objectTypes.CartDiscountProdCatRelations, Conversions.ToLong(oDr[0]));
                                }

                                break;
                            }
                        case objectTypes.CartProductCategories:
                            {
                                sSql = "Select nCatProductRelKey, nAuditId From tblCartCatProductRelations Where nCatId = " + nId;
                                using (var oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read())
                                    {
                                        DeleteObject(objectTypes.CartCatProductRelations, Conversions.ToLong(oDr.GetValue(0)));
                                        DeleteObject(objectTypes.Audit, Conversions.ToLong(oDr.GetValue(1)));
                                    }
                                }
                                sSql = "Select nDiscountProdCatRelationKey FROM tblCartDiscountProdCatRelations WHERE nProductCatId = " + nId;
                                using (var oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read())
                                        DeleteObject(objectTypes.CartDiscountProdCatRelations, Conversions.ToLong(oDr[0]));
                                }

                                break;
                            }
                        case objectTypes.CartCatProductRelations:
                            {
                                sSql = "Select nAuditId From tblCartCatProductRelations Where nCatProductRelKey = " + nId;
                                using (var oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read())
                                        DeleteObject(objectTypes.Audit, Conversions.ToLong(oDr.GetValue(0)));
                                }

                                break;
                            }
                        case objectTypes.Content:
                            {

                                // Delete ActivityLogs for content
                                sSql = "select nActivityKey from tblActivityLog where nArtId = " + nId;
                                using (var oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read())
                                        DeleteObject(objectTypes.ActivityLog, Conversions.ToLong(oDr[0]));
                                }

                                // get any locations and delete
                                sSql = "select nContentLocationKey, bPrimary from tblContentLocation where nContentId = " + nId;
                                using (var oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read())
                                        DeleteObject(objectTypes.ContentLocation, Conversions.ToLong(oDr[0]));
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
                                using (var oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read())
                                        DeleteObject(objectTypes.ContentRelation, Conversions.ToLong(oDr[0]));
                                }

                                break;
                            }

                        case objectTypes.ContentRelation:
                            {
                                sSql = "Select nAuditId, nContentParentID, nContentChildId From tblContentRelation Where nContentRelationKey = " + nId;
                                using (var oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read())
                                    {
                                        sSql = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("Select nContentRelationKey From tblContentRelation WHERE nContentParentId = ", oDr[2]), " AND nContentChildId = "), oDr[1]));
                                        DeleteObject(objectTypes.Audit, Conversions.ToLong(oDr[0]));
                                    }
                                }
                                ExeProcessSql("Delete From tblContentRelation where nContentRelationKey = " + nId);
                                using (var oDr = getDataReaderDisposable(sSql))
                                {
                                    var nOther = default(int);
                                    while (oDr.Read())
                                    {
                                        nOther = Conversions.ToInteger(oDr[0]);
                                        break;
                                    }
                                    if (nOther > 0)
                                        DeleteObject(objectTypes.ContentRelation, nOther);
                                }

                                break;
                            }
                        case objectTypes.ContentStructure:
                            {

                                // Delete ActivityLogs for page
                                sSql = "select nActivityKey from tblActivityLog where nStructId = " + nId;
                                using (var oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read())
                                        DeleteObject(objectTypes.ActivityLog, Conversions.ToLong(oDr[0]));
                                }

                                // delete any child pages

                                sSql = "select nStructKey from tblContentStructure where nStructParId = " + nId;
                                using (var oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read())
                                        DeleteObject(objectTypes.ContentStructure, Conversions.ToLong(oDr[0]));
                                }

                                // do we want to delete any content that is not also located elsewhere???? 
                                // Add Later....

                                // get any locations and delete
                                sSql = "select nContentLocationKey, bPrimary from tblContentLocation where nStructId = " + nId;
                                using (var oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read())
                                        // If oDr("bPrimary") = True Then
                                        // Else
                                        // bHaltDelete = True ' there is more than one location for this content don't delete it.
                                        // End If
                                        DeleteObject(objectTypes.ContentLocation, Conversions.ToLong(oDr[0]));
                                }

                                clearStructureCacheAll();
                                break;
                            }


                        case objectTypes.QuestionaireResult:
                            {
                                sSql = "select nQDetailKey from tblQuestionaireResultDetail where nQResultId = " + nId;
                                using (var oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read())
                                        DeleteObject(objectTypes.QuestionaireResultDetail, Conversions.ToLong(oDr[0]));
                                }

                                break;
                            }

                        case objectTypes.Directory:
                            {

                                // Delete ActivityLogs
                                sSql = "select nActivityKey from tblActivityLog where nUserDirId = " + nId;
                                using (var oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read())
                                        DeleteObject(objectTypes.ActivityLog, Conversions.ToLong(oDr[0]));
                                }

                                // Delete Permissions
                                sSql = "select nPermKey from tblDirectoryPermission where nDirId = " + nId;
                                using (var oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read())
                                        DeleteObject(objectTypes.Permission, Conversions.ToLong(oDr[0]));
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
                                using (var oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read())
                                        DeleteObject(objectTypes.Directory, Conversions.ToLong(oDr[0]));
                                }

                                // Delete Relationships
                                sSql = "select nRelKey from tblDirectoryRelation where nDirParentId = " + nId + " or nDirChildId = " + nId;
                                using (var oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read())
                                        DeleteObject(objectTypes.DirectoryRelation, Conversions.ToLong(oDr[0]));
                                }

                                break;
                            }

                        case objectTypes.DirectoryRelation:
                            {

                                // if we are linking a user to a company we need to also delete from depts / training groups that are also in that company.

                                // get the parent dir object
                                sSql = "select d.nDirKey, d.cDirSchema, dr.nDirChildId from tblDirectoryRelation dr inner join tblDirectory d on d.nDirKey = dr.nDirParentId where nRelKey=" + nId;
                                var nParentId = default(long);
                                var nChildId = default(long);
                                string cDirSchema = "";
                                using (var oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read())
                                    {
                                        nParentId = Conversions.ToLong(oDr["nDirKey"]);
                                        nChildId = Conversions.ToLong(oDr["nDirChildId"]);
                                        cDirSchema = Conversions.ToString(oDr["cDirSchema"]);
                                    }
                                }
                                if (cDirSchema == "Company")
                                {
                                    // select all of the children of nParentId that have relations with our child
                                    sSql = "select cr.nRelKey from tblDirectoryRelation dr " + "inner join tblDirectory pd on pd.nDirKey = dr.nDirParentId  " + "inner join tblDirectory cd on cd.nDirKey = dr.nDirChildId  " + "inner join tblDirectoryRelation cr on cd.nDirKey = cr.nDirParentId  " + "where dr.nDirParentId = " + nParentId + " " + "and cr.nDirChildId = " + nChildId;
                                    using (var oDr = getDataReaderDisposable(sSql))
                                    {
                                        // delete links between the target object and the children of the parent object.
                                        while (oDr.Read())
                                            DeleteObject(objectTypes.DirectoryRelation, Conversions.ToLong(oDr[0]));
                                    }

                                }

                                break;
                            }
                        case objectTypes.CartItem:
                            {
                                sSql = "Select  nAuditID from tblCartItem WHERE nCartItemKey = " + nId;

                                using (SqlDataReader oDr = getDataReaderDisposable(sSql))
                                {
                                    var nCrtItmAdtId = default(int);
                                    while (oDr.Read())
                                        // DeleteObject(objectTypes.Audit, oDr.GetValue(0))
                                        nCrtItmAdtId = Conversions.ToInteger(oDr.GetValue(0));
                                    DeleteObject(objectTypes.Audit, nCrtItmAdtId);
                                }
                                // options
                                sSql = "Select nCartItemKey from tblCartItem WHERE nParentID = " + nId;
                                using (var oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read())
                                        DeleteObject(objectTypes.CartItem, Conversions.ToLong(oDr.GetValue(0)));
                                }
                                ExeProcessSql("Delete from tblCartItem where nCartItemKey = " + nId);
                                break;
                            }


                        case objectTypes.CartOrder:
                            {
                                // cart items
                                sSql = "Select nCartItemKey from tblCartItem WHERE nCartOrderID = " + nId;
                                using (var oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read())
                                        DeleteObject(objectTypes.CartItem, Conversions.ToLong(oDr.GetValue(0)));
                                }
                                // contacts
                                sSql = "Select nContactKey from tblCartContact WHERE nContactCartID = " + nId;
                                using (var oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read())
                                        DeleteObject(objectTypes.CartContact, Conversions.ToLong(oDr.GetValue(0)));

                                }
                                sSql = "Select nAuditId from tblCartOrder WHERE nCartOrderKey = " + nId;
                                using (var oDr = getDataReaderDisposable(sSql))
                                {
                                    var nCrtOrdAdtId = default(int);
                                    while (oDr.Read())
                                        // DeleteObject(objectTypes.Audit, oDr.GetValue(0))
                                        nCrtOrdAdtId = Conversions.ToInteger(oDr.GetValue(0));
                                    ExeProcessSql("Delete from tblCartOrder where nCartOrderKey = " + nId);
                                    DeleteObject(objectTypes.Audit, nCrtOrdAdtId);
                                }

                                break;
                            }

                        case objectTypes.CartContact:
                            {
                                sSql = "Select nAuditId from tblCartContact WHERE nContactKey = " + nId;
                                using (var oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read())
                                    {
                                        if (!(oDr.GetValue(0) is DBNull))
                                        {
                                            DeleteObject(objectTypes.Audit, Conversions.ToLong(oDr.GetValue(0)));
                                        }
                                    }
                                    ExeProcessSql("Delete from tblCartContact where nContactKey = " + nId);
                                }

                                break;
                            }
                        case objectTypes.Subscription:
                            {
                                var oXML = new XmlDocument();
                                sSql = "SELECT cSubscriptionXML, nUserId FROM tblSubscription WHERE nSubKey = " + nId;
                                var nSubUserId = default(int);
                                using (var oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read())
                                    {
                                        oXML.InnerXml = Strings.Replace(Strings.Replace(Conversions.ToString(oDr[0]), "&gt;", ">"), "&lt;", "<");
                                        nSubUserId = Conversions.ToInteger(oDr[1]);
                                    }
                                }
                                XmlElement oElmt = (XmlElement)oXML.DocumentElement.SelectSingleNode("descendant-or-self::UserGroups");

                                sSql = "SELECT cSubXML FROM tblSubscription WHERE nDirId = " + nSubUserId + " AND (NOT (nSubKey = " + nId + "))";
                                var oDS = myWeb.moDbHelper.GetDataSet(sSql, "Content");
                                var oXML2 = new XmlDocument();
                                oXML2.InnerXml = Strings.Replace(Strings.Replace(oDS.GetXml(), "&gt;", ">"), "&lt;", "<");

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
                                        using (var oDr = getDataReaderDisposable(sSql))
                                        {
                                            while (oDr.Read())
                                                DeleteObject(objectTypes.DirectoryRelation, Conversions.ToLong(oDr.GetValue(0)));
                                        }

                                    }
                                }

                                sSql = "Select nAuditId From tblSubscription where nSubKey = " + nId;
                                using (var oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read())
                                        DeleteObject(objectTypes.Audit, Conversions.ToLong(oDr.GetValue(0)));
                                    ExeProcessSql("Delete from tblSubscription where nSubKey = " + nId);
                                }

                                break;
                            }
                        case objectTypes.CartShippingMethod:
                            {
                                sSql = "SELECT nShpRelKey FROM tblCartShippingRelations WHERE nShpOptId = " + nId;
                                using (var oDr = getDataReaderDisposable(sSql))
                                {
                                    while (oDr.Read())
                                        DeleteObject(objectTypes.CartShippingRelations, Conversions.ToLong(oDr.GetValue(0)));
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
                            sSql = "select nAuditId from " + getTable(objectType) + " where " + getKey((int)objectType) + " = " + nId;
                            using (var oDr = getDataReaderDisposable(sSql))
                            {
                                while (oDr.Read())
                                {
                                    if (Information.IsNumeric(oDr[0]))
                                    {
                                        nAuditId = Conversions.ToLong(oDr[0]);
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
                        sSql = "delete from " + getTable(objectType) + " where " + getKey((int)objectType) + " = " + nId;
                        ExeProcessSql(sSql);


                    }

                    return nId;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "DeleteObject", ex, cProcessInfo));
                }

                return default;

            }

            /// <summary>
            /// Delete content and associated relationships by ID.
            /// Bulk method to try and emulate DeleteObject without the bit by bit iteration
            /// The items that will be deleted are:
            /// the content, any locations for this content, any relations for this content, the related audit records for all three
            /// </summary>
            /// <param name="contentIDList">A List(Of String) of the IDs to delete</param>
            /// <remarks></remarks>
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
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, methodName, ex, processInfo));
                }
            }

            /// <summary>
            /// Delete content and associated relationships by ID.
            /// Bulk method to try and emulate DeleteObject without the bit by bit iteration
            /// The items that will be deleted are:
            /// the content, any locations for this content, any relations for this content, the related audit records for all three
            /// </summary>
            /// <param name="contentIDCSVList">The content IDs to delete as a comma separated list</param>
            /// <remarks>I'm not decided on deleting orphan related content.</remarks>
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
                        deletedRecords = ExeProcessSql("DELETE tblAudit FROM tblAudit INNER JOIN tblContentLocation ON nAuditId = nAuditKey AND nContentId IN (" + contentIDCSVList + ")");
                        if (deletedRecords > 0)
                        {
                            ExeProcessSql("DELETE FROM tblContentLocation WHERE nContentId IN (" + contentIDCSVList + ")");
                        }

                        // Delete the relatnios for this content
                        deletedRecords = ExeProcessSql("DELETE tblAudit FROM tblAudit INNER JOIN tblContentRelation ON nAuditId = nAuditKey AND (nContentChildId IN (" + contentIDCSVList + ") OR nContentParentId IN (" + contentIDCSVList + "))");
                        if (deletedRecords > 0)
                        {
                            ExeProcessSql("DELETE  FROM tblContentRelation WHERE nContentChildId IN (" + contentIDCSVList + ") OR nContentParentId IN (" + contentIDCSVList + ")");
                        }

                        // Delete the actual content
                        deletedRecords = ExeProcessSql("DELETE tblAudit FROM tblAudit INNER JOIN tblContent ON nAuditId = nAuditKey AND nContentKey IN (" + contentIDCSVList + ")");
                        if (deletedRecords > 0)
                        {
                            ExeProcessSql("DELETE  FROM tblContent WHERE nContentKey IN (" + contentIDCSVList + ")");
                        }

                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, methodName, ex, processInfo));
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

                        case (objectTypes)3: // tblContentStructure
                            {
                                DeleteAllObjects(objectTypes.ContentLocation);
                                break;
                            }
                    }

                    sSql = "select " + getKey((int)objectType) + " from " + getTable(objectType);

                    using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {
                        if (bReporting)
                        {
                            goResponse.Write("Deleting: - " + getTable(objectType));
                        }

                        while (oDr.Read())
                            DeleteObject(objectType, Conversions.ToLong(oDr[0]), bReporting);
                        while (oDr.Read())
                            DeleteObject(objectType, Conversions.ToLong(oDr[0]), bReporting);
                    }
                }


                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "DeleteAllObjects", ex, cProcessInfo));
                }

            }

            public virtual string getObjectInstance(objectTypes ObjectType, long nId = -1, string sWhere = "")
            {

                PerfMonLog("DBHelper", "getObjectInstance");

                string sSql;
                DataSet oDs;
                XmlElement oElmt;
                // Dim  As New 
                string sContent;
                string sInstance = "";
                DataRow oRow;
                string sNewVal = "";

                string cProcessInfo = "";
                try
                {
                    // Dim oXml As XmlDocument = New XmlDocument


                    if (string.IsNullOrEmpty(sWhere))
                    {
                        sSql = "select * from " + getTable(ObjectType) + " left outer join tblAudit a on nAuditId = a.nAuditKey where " + getKey((int)ObjectType) + " = " + nId;
                    }
                    else
                    {
                        sSql = "select TOP 1 * from " + getTable(ObjectType) + " inner join tblAudit a on nAuditId = a.nAuditKey " + sWhere;
                    }
                    if (ObjectType == objectTypes.ScheduledItem)
                    {
                        sSql = "select * from " + getTable(ObjectType) + " where " + getKey((int)ObjectType) + " = " + nId;
                    }


                    cProcessInfo = "running: " + sSql;

                    oDs = GetDataSet(sSql, getTable(ObjectType), "instance");
                    ReturnNullsEmpty(ref oDs);

                    XmlDocument oXml;
                    if (nId > 0L)
                    {
                        oXml = GetXml(oDs);
                    }
                    else
                    {

                        // var oNewXml = new XmlDataDocument(oDs);
                        XmlDocument oNewXml = new XmlDocument();
                        if (oDs.Tables[0].Rows.Count > 0)
                        {
                            oNewXml.LoadXml(oDs.GetXml());
                        }
                        oXml = oNewXml;

                    }


                    // 
                    oDs.EnforceConstraints = false;

                    // Convert any text to xml
                    foreach (XmlNode oNode in oXml.SelectNodes("/instance/" + getTable(ObjectType) + "/*"))
                    {

                        // ignore for passwords
                        if (!(oNode.Name == "cDirPassword"))
                        {
                            oElmt = (XmlElement)oNode;
                            sContent = oElmt.InnerText;
                            try
                            {
                                oElmt.InnerXml = sContent;
                            }
                            catch
                            {
                                // run tidy...
                                oElmt.InnerXml = stdTools.tidyXhtmlFrag(sContent, true, false);
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
                                oElmt = (XmlElement)oNode;
                                sContent = oElmt.InnerText;
                                if (sContent == "-1")
                                    oElmt.InnerText = "1";

                            }
                        }

                    }
                    if (oXml.SelectSingleNode("instance") != null)
                    {
                        sInstance = oXml.SelectSingleNode("instance").InnerXml;
                    }
                    else
                    {
                        // we could be clever here and step through the dataset to build an empty instance?
                        oRow = oDs.Tables[getTable(ObjectType)].NewRow();
                        oElmt = oXml.CreateElement(getTable(ObjectType));
                        foreach (DataColumn oColumn in oDs.Tables[getTable(ObjectType)].Columns)
                        {
                            // set a default value for status.
                            if (oColumn.ToString() == "nStatus")
                            {
                                switch (ObjectType)
                                {
                                    case objectTypes.CartShippingMethod:
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

                            else if (!(oColumn.DefaultValue is DBNull))
                            {
                                sNewVal = convertDtSQLtoXML(oColumn.DataType, oColumn.DefaultValue);
                            }
                            else
                            {
                                sNewVal = "";

                            }
                            XmlNode argoNode = oElmt;
                            addNewTextNode(oColumn.ToString(), ref argoNode, sNewVal, true, false);
                            oElmt = (XmlElement)argoNode; // always force this
                        }
                        oXml.AppendChild(oElmt);
                        sInstance = oXml.InnerXml;
                    }

                    oXml = null;
                    // oDs.Clear()
                    oDs = null;

                    return sInstance;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getObjectInstance", ex, cProcessInfo));
                    return "";
                }

            }

            public bool IsAuditedObjectLive(objectTypes objectType, long objectKey)
            {
                try
                {
                    string query = "SELECT " + getKey((int)objectType) + " FROM " + getTable(objectType) + " o INNER JOIN dbo.tblAudit a ON o.nauditId = a.nauditKey " + "WHERE " + myWeb.GetStandardFilterSQLForContent(false);
                    return Conversions.ToBoolean(Operators.ConditionalCompareObjectGreater(GetDataValue(query), 0, false));
                }
                catch (Exception)
                {
                    return false;
                }
            }

            public bool IsChildPage(objectTypes pageid, long objectKey)
            {

                try
                {
                    string query = "select * from tblcontentstructure P inner join tblcontentstructure C on p.nStructKey = C.nStructParId where p.nStructKey =" + objectKey;
                    return Conversions.ToBoolean(Operators.ConditionalCompareObjectGreater(GetDataValue(query), 0, false));
                }
                catch (Exception)
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

                    nCount = Conversions.ToLong(GetDataValue(sSql));

                    if (nCount > 0L)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "isCascade", ex, cProcessInfo));
                }

                return default;
            }

            public virtual string setObjectInstance(objectTypes ObjectType, XmlElement oInstance = null, long nKey = -1)
            {

                PerfMonLog("DBHelper", "setObjectInstance", ObjectType.ToString());


                // EonicWeb Specific Function for updating DB Entities, manages creation and updating of Audit table

                XmlDocument oXml;  // Change XmlDataDocument to XmlDocument
                XmlElement oElmt = null;
                XmlElement oElmt2;
                var nAuditId = default(long);
                XmlElement oTableNode;

                string cProcessInfo = "";
                long nVersionId = 0L;

                bool bSaveInstance = true;

                try
                {

                    // if we have not been given an instance we'll create one, this makes it easy to update the audit table by just supplying an id
                    if (oInstance is null)
                    {
                        oXml = new XmlDocument();  // Change XmlDataDocument to XmlDocument
                        oElmt = oXml.CreateElement("instance");
                        oXml.AppendChild(oElmt);
                        oTableNode = oXml.CreateElement(getTable(ObjectType));
                        oXml.FirstChild.AppendChild(oTableNode);
                        oElmt2 = oXml.CreateElement(getKey((int)ObjectType));
                        oElmt2.InnerText = nKey.ToString();
                        oElmt.AppendChild(oElmt2);
                        oInstance = oXml.DocumentElement;
                    }
                    else
                    {
                        oTableNode = (XmlElement)oInstance.SelectSingleNode(getTable(ObjectType));
                    }
                    if (getTable(ObjectType) == "tblAudit")
                    {
                        // we get the first actual element.
                        foreach (XmlElement currentOElmt in oInstance.SelectNodes("*"))
                        {
                            oElmt = currentOElmt;
                            if (oTableNode is null)
                            {
                                if (oElmt.NodeType == XmlNodeType.Element)
                                {
                                    oTableNode = oElmt;
                                }
                            }
                        }
                    }

                    // lets get the key from the instance if not supplied
                    if (nKey == -1)
                    {
                        oElmt = (XmlElement)oInstance.SelectSingleNode("*/" + getKey((int)ObjectType));
                        if (oElmt != null)
                        {
                            if (Information.IsNumeric(oElmt.InnerText))
                            {
                                nKey = Conversions.ToLong(oElmt.InnerText);
                            }
                        }
                    }

                    // also lets add the key to the instance because were going to need it for saveInstanc
                    if (nKey > 0L)
                    {
                        var argoNode = oInstance.SelectSingleNode("*");
                        addNewTextNode(getKey((int)ObjectType), ref argoNode, nKey.ToString(), true, true);
                    }

                restart:
                    ;

                    // Special case: contentversion (when setting audit)
                    if (oTableNode is null & ObjectType == objectTypes.ContentVersion)
                    {
                        oTableNode = (XmlElement)oInstance.SelectSingleNode(getTable(objectTypes.Content));
                    }



                    // Process for Updates or Inserts
                    if (nKey > 0L) // CASE FOR UPDATE ------- not a new record so lets update the audit.
                    {
                        // and now we update the audit table
                        cProcessInfo = "Updating Object Type: " + ((int)ObjectType).ToString() + "(Table: " + getTable(ObjectType) + ", Object Id: " + nKey;
                        switch (ObjectType)
                        {
                            case objectTypes.Content:
                                {
                                    XmlElement oPrimId = (XmlElement)oInstance.SelectSingleNode("tblContent/nContentPrimaryId");
                                    if (string.IsNullOrEmpty(oPrimId.InnerText))
                                    {
                                        oPrimId.InnerText = "0";
                                    }
                                    XmlElement oVerId = (XmlElement)oInstance.SelectSingleNode("tblContent/nVersion");
                                    if (string.IsNullOrEmpty(oVerId.InnerText))
                                    {
                                        oVerId.InnerText = "0";
                                    }
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
                                        if (NodeState(ref oInstance, "descendant-or-self::nAuditId") == XmlNodeState.HasContents)
                                        {
                                            XmlElement oAuditElmt = (XmlElement)oInstance.SelectSingleNode("descendant-or-self::nAuditId");
                                            if (Information.IsNumeric(oAuditElmt.InnerText))
                                            {
                                                nAuditId = Conversions.ToLong(oAuditElmt.InnerText);
                                            }
                                            if (nAuditId == 0L)
                                            {
                                                if (myWeb is null)
                                                {
                                                    nAuditId = getAuditId(1, 0L, "");
                                                }
                                                else
                                                {
                                                    nAuditId = getAuditId(1, (long)myWeb.mnUserId, "");
                                                }
                                                oInstance.SelectSingleNode("descendant-or-self::nAuditId").InnerText = nAuditId.ToString();
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
                            case var @case when @case == objectTypes.CourseResult:
                            case objectTypes.Certificate:
                            case objectTypes.CpdLog:
                            case objectTypes.QuestionaireResultDetail:
                            case objectTypes.Lookup:
                            case objectTypes.CartCarrier:
                            case objectTypes.CartDelivery:
                            case objectTypes.Subscription:
                            case objectTypes.SubscriptionRenewal:
                            case objectTypes.CartPayment:
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
                                    XmlElement updateDateNode = (XmlElement)oInstance.SelectSingleNode("*/dUpdateDate");
                                    if (updateDateNode != null)
                                    {
                                        if (updateDateNode.GetAttribute("force") == "true")
                                        {
                                            forceUpdate = true;
                                        }
                                    }
                                    if (forceUpdate == false) // always force this
                                    {
                                        XmlNode argoNode1 = oTableNode;
                                        addNewTextNode("dUpdateDate", ref argoNode1, XmlDate(DateTime.Now, true), true, true);
                                        oTableNode = (XmlElement)argoNode1;
                                    }
                                    XmlNode argoNode2 = oTableNode;
                                    addNewTextNode("nUpdateDirId", ref argoNode2, mnUserId.ToString(), true, true);
                                    oTableNode = (XmlElement)argoNode2; // always force this
                                    break;
                                }

                            default:
                                {
                                    break;
                                }
                                // do nowt
                        }

                        // Check the auditId is populated in the instnace
                        XmlElement auditIdElmt = (XmlElement)oInstance.SelectSingleNode(getTable(ObjectType) + "/nAuditId");
                        if (auditIdElmt != null)
                        {
                            if (string.IsNullOrEmpty(auditIdElmt.InnerText))
                            {
                                auditIdElmt.InnerText = nAuditId.ToString();
                            }
                        }
                    }

                    else  // CASE FOR INSERT  ------------  We have a new record lets give it an auditId if it needs one.
                    {
                        cProcessInfo = "Inserting Object Type: " + ((int)ObjectType).ToString() + "(Table: " + getTable(ObjectType) + ", New Object";
                        switch (ObjectType)
                        {

                            case objectTypes.Content:
                                {
                                    // To be readjusted
                                    if (gbVersionControl)
                                    {
                                        // out to a subroutine for versioning
                                        long argnKey = 0L;
                                        bool argbSaveInstance = true;
                                        contentVersioning(ref oInstance, ref ObjectType, nKey: ref argnKey, bSaveInstance: ref argbSaveInstance);
                                        if (ObjectType == objectTypes.ContentVersion)
                                            goto restart;
                                    }

                                    // we are using getAuditId to create a new audit record.
                                    nAuditId = Conversions.ToLong(setObjectInstance(objectTypes.Audit, oInstance));
                                    XmlNode argoNode3 = oTableNode;
                                    addNewTextNode("nAuditId", ref argoNode3, nAuditId.ToString(), true, true);
                                    oTableNode = (XmlElement)argoNode3;
                                    break;
                                }

                            case var case1 when case1 == objectTypes.Content:
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
                            case objectTypes.CartPayment:
                            case objectTypes.CartPaymentMethod:
                            case objectTypes.indexkey:
                                {

                                    // we are using getAuditId to create a new audit record.
                                    nAuditId = Conversions.ToLong(setObjectInstance(objectTypes.Audit, oInstance));
                                    XmlNode argoNode4 = oTableNode;
                                    addNewTextNode("nAuditId", ref argoNode4, nAuditId.ToString(), true, true);
                                    oTableNode = (XmlElement)argoNode4;
                                    break;
                                }


                            case objectTypes.Audit:
                                {
                                    // add logic for inserting default audit fields.
                                    // addNewTextNode("dPublishDate", oInstance.FirstChild, xmlDate(Now()), True, False)

                                    // if this is supplied we want to keep it, mostly it will be blank
                                    if (oInstance.FirstChild.SelectSingleNode("dInsertDate") is null)
                                    {
                                        XmlNode argoNode5 = oTableNode;
                                        addNewTextNode("dInsertDate", ref argoNode5, XmlDate(DateTime.Now, true), true, true);
                                        oTableNode = (XmlElement)argoNode5; // always force this
                                    }
                                    else if (string.IsNullOrEmpty(oInstance.FirstChild.SelectSingleNode("dInsertDate").InnerText))
                                    {
                                        oInstance.FirstChild.SelectSingleNode("dInsertDate").InnerText = XmlDate(DateTime.Now, true);
                                        // addNewTextNode("dInsertDate", oTableNode, Protean.Tools.Xml.XmlDate(Now(), True), True, True) 'always force this
                                    }

                                    if (oInstance.FirstChild.SelectSingleNode("nInsertDirId") is null)
                                    {
                                        XmlNode argoNode6 = oTableNode;
                                        addNewTextNode("nInsertDirId", ref argoNode6, mnUserId.ToString(), true, true);
                                        oTableNode = (XmlElement)argoNode6; // always force this
                                    }
                                    else if (string.IsNullOrEmpty(oInstance.FirstChild.SelectSingleNode("nInsertDirId").InnerText))
                                    {
                                        oInstance.FirstChild.SelectSingleNode("nInsertDirId").InnerText = mnUserId.ToString();
                                        // addNewTextNode("nInsertDirId", oTableNode, mnUserId, True, True) 'always force this
                                    }


                                    bool forceUpdate = false;
                                    XmlElement updateDateNode = (XmlElement)oInstance.SelectSingleNode("tblContent/dUpdateDate");
                                    if (updateDateNode != null)
                                    {
                                        if (updateDateNode.GetAttribute("force") == "true")
                                        {
                                            forceUpdate = true;
                                        }
                                    }
                                    if (forceUpdate == false) // always force this
                                    {
                                        XmlNode argoNode7 = oTableNode;
                                        addNewTextNode("dUpdateDate", ref argoNode7, XmlDate(DateTime.Now, true), true, true);
                                        oTableNode = (XmlElement)argoNode7;
                                    }

                                    XmlNode argoNode8 = oTableNode;
                                    addNewTextNode("nUpdateDirId", ref argoNode8, mnUserId.ToString(), true, true);
                                    oTableNode = (XmlElement)argoNode8; // always force this
                                    if (oInstance.SelectSingleNode("descendant-or-self::nStatus") is null)
                                    {
                                        XmlNode argoNode9 = oTableNode;
                                        addNewTextNode("nStatus", ref argoNode9, "1", true, true);
                                        oTableNode = (XmlElement)argoNode9;
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(oInstance.SelectSingleNode("descendant-or-self::nStatus").InnerText))
                                        {
                                            XmlNode argoNode10 = oTableNode;
                                            addNewTextNode("nStatus", ref argoNode10, "1", true, true);
                                            oTableNode = (XmlElement)argoNode10;
                                        }
                                        XmlNode argoNode11 = oTableNode;
                                        addNewTextNode("nStatus", ref argoNode11, "1", true, false);
                                        oTableNode = (XmlElement)argoNode11;
                                    }

                                    break;
                                }
                        }
                    }

                    cProcessInfo = "Saving instance";

                    PerfMonLog("DBHelper", "setObjectInstance", "startsave");


                    if (bSaveInstance)
                    {
                        nKey = saveInstance(ref oInstance, getTable(ObjectType), getKey((int)ObjectType));
                    }
                    else
                    {
                        nKey = nVersionId;
                    }

                    PerfMonLog("DBHelper", "setObjectInstance", "endsave");

                    if (ObjectType == objectTypes.ContentStructure)
                    {
                        clearStructureCacheAll();
                    }

                    return nKey.ToString();
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "setObjectInstance", ex, cProcessInfo));
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
                    nAuditId = 0L;

                    // First check the node (exists and is numeric)
                    if (NodeState(ref oInstance, "descendant-or-self::nAuditId") == XmlNodeState.HasContents)
                    {
                        XmlElement oElmt = (XmlElement)oInstance.SelectSingleNode("descendant-or-self::nAuditId");
                        if (Information.IsNumeric(oElmt.InnerText))
                        {
                            nAuditId = Conversions.ToLong(oElmt.InnerText);
                        }
                    }

                    // If not set then try getting the value from the DB
                    if (!(nAuditId > 0L))
                    {
                        nAuditId = Conversions.ToLong(GetDataValue("SELECT nAuditId FROM " + getTable(objectType) + " WHERE " + getKey((int)objectType) + "=" + nKey));
                    }
                    XmlElement oTableNode = (XmlElement)oInstance.FirstChild;

                    // if this is supplied we want to keep it, mostly it will be blank
                    if (oTableNode.SelectSingleNode("dInsertDate") is null)
                    {
                        XmlNode argoNode = oTableNode;
                        addNewTextNode("dInsertDate", ref argoNode, XmlDate(DateTime.Now, true), true, true);
                        oTableNode = (XmlElement)argoNode; // always force this
                    }
                    else if (string.IsNullOrEmpty(oInstance.FirstChild.SelectSingleNode("dInsertDate").InnerText))
                    {
                        XmlNode argoNode1 = oTableNode;
                        addNewTextNode("dInsertDate", ref argoNode1, XmlDate(DateTime.Now, true), true, true);
                        oTableNode = (XmlElement)argoNode1; // always force this
                    }

                    if (oTableNode.SelectSingleNode("nInsertDirId") is null)
                    {
                        XmlNode argoNode2 = oTableNode;
                        addNewTextNode("nInsertDirId", ref argoNode2, mnUserId.ToString(), true, true);
                        oTableNode = (XmlElement)argoNode2; // always force this
                    }
                    else if (string.IsNullOrEmpty(oInstance.FirstChild.SelectSingleNode("nInsertDirId").InnerText))
                    {

                        XmlNode argoNode3 = oTableNode;
                        addNewTextNode("nInsertDirId", ref argoNode3, mnUserId.ToString(), true, true);
                        oTableNode = (XmlElement)argoNode3; // always force this
                    }

                    // Set the Audit instance
                    if (nAuditId > 0L)
                        setObjectInstance(objectTypes.Audit, oInstance, nAuditId);

                    return nAuditId;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "tidyAuditId", ex, cProcessInfo));
                    return Conversions.ToLong("");
                }

            }

            #region DB Methods: Version Control
            protected long contentVersioning(ref XmlElement oInstance, ref objectTypes ObjectType, [Optional, DefaultParameterValue(0L)] ref long nKey, [Optional, DefaultParameterValue(true)] ref bool bSaveInstance)
            {

                string cProcessInfo = "";
                // XmlElement oElmt = null;
                XmlElement oOrigInstance;
                long nCurrentVersionNumber;
                long nNewVersionNumber = 0L;
                Status nStatus;
                string cSql = "";
                long nVersionId = 0L;

                try
                {

                    // first we look at the status of the content being sumitted.

                    XmlNode argoParent = oInstance;
                    nStatus = (Status)Conversions.ToInteger(getNodeValueByType(ref argoParent, "//nStatus", XmlDataType.TypeNumber, 1));
                    oInstance = (XmlElement)argoParent;
                    XmlNode argoParent1 = oInstance;
                    nCurrentVersionNumber = Conversions.ToLong(getNodeValueByType(ref argoParent1, "//nVersion", XmlDataType.TypeNumber, 0));
                    oInstance = (XmlElement)argoParent1;

                    // Get the maximum version number
                    if (nKey > 0L)
                    {
                        cSql = "SELECT MAX(nVersion) As MaxV " + "FROM ( " + "       SELECT nVersion FROM dbo.tblContent WHERE nContentKey = " + nKey + "       UNION " + "       SELECT nVersion FROM dbo.tblContentVersions WHERE nContentPrimaryId = " + nKey + ") VersionList";




                        nNewVersionNumber = Conversions.ToLong(Operators.AddObject(GetDataValue(cSql), 1));
                    }

                    oInstance.SelectSingleNode("//nVersion").InnerText = nNewVersionNumber.ToString();

                    // If the status is live or hidden, then check the users page permissions
                    // At the moment assume that if we are in admin mode, we can skip this
                    if (myWeb != null)
                    {

                        if (!myWeb.mbAdminMode)
                        {
                            // Check the permissions
                            // Problem at the moment ascertaining the parent page for orphan content, 
                            // This is already acheived in Web.GetAjaxXml so we pass through a value to dbHelper

                            if (!CanPublish(CurrentPermissionLevel))
                            {

                                // The permission level inspected is not a publishing level, so this needs to be set as pending.
                                nStatus = Status.Pending;

                                // Check if the status node exists
                                if (NodeState(ref oInstance, "//nStatus") == XmlNodeState.NotInstantiated)
                                {
                                    // Status node doesn't exist - we have to assume that it's going to be added to the first child of oInstance (i.e. instance/tablename/nstatus)
                                    XmlElement oTable = (XmlElement)oInstance.FirstChild;
                                    oTable.AppendChild(oTable.OwnerDocument.CreateElement("nStatus"));
                                }

                                oInstance.SelectSingleNode("//nStatus").InnerText = ((int)nStatus).ToString();

                            }
                        }

                        switch (nStatus)
                        {
                            case Status.Live:
                            case Status.Hidden:
                                {

                                    // If this is not new then get the current version and commit it to the version table
                                    if (nKey > 0L)
                                    {
                                        // create a copy of the origional in versions and save live
                                        oOrigInstance = moPageXml.CreateElement("instance");
                                        oOrigInstance.InnerXml = getObjectInstance(objectTypes.Content, nKey);

                                        nVersionId = setNewContentVersionInstance(ref oOrigInstance, nKey);

                                        // If this was a pending item, supercede the copy in the version table (ie superceded anything that's pending)
                                        string cPreviousStatus = "";
                                        if (NodeState(ref oInstance, "currentStatus", "", "", default, null, "", cPreviousStatus) == XmlNodeState.HasContents)
                                        {
                                            if (cPreviousStatus == "3" | cPreviousStatus == "4")
                                            {
                                                // Update everything with a status of Pending to be DraftSuperceded
                                                ExeProcessSql("UPDATE tblAudit SET nStatus = " + ((int)Status.Superceded).ToString() + " FROM tblAudit a INNER JOIN tblContentVersions c ON c.nAuditId = a.nAuditKey AND c.nContentPrimaryId = " + nKey + " AND (a.nStatus = " + ((int)Status.Pending).ToString() + " or a.nStatus = " + ((int)Status.InProgress).ToString() + ")");
                                            }
                                        }

                                    }

                                    break;
                                }

                            case Status.InProgress: // PREVIEW
                                {

                                    nVersionId = setNewContentVersionInstance(ref oInstance, nKey, Status.InProgress);
                                    // ?how do we stop the origional updating?
                                    bSaveInstance = false;
                                    break;
                                }

                            case Status.Pending:
                                {

                                    // Pending works in a number of ways
                                    // - New content is created in the content table, not the versions table
                                    // - Existing content is created in the versions table
                                    // - For existing content, if the current Content (nKey) is Live or Hidden then save Pending instance to versions
                                    // - If the current Content (nKey) is Pending or Rejected then move current to Versions and save Pending instance to Content.

                                    // First get the current live, if applicable.
                                    if (nKey > 0L)
                                    {

                                        long cParentId = nKey;
                                        // Get the current live
                                        oOrigInstance = moPageXml.CreateElement("instance");
                                        oOrigInstance.InnerXml = getObjectInstance(objectTypes.Content, nKey);


                                        // Assess the status
                                        XmlNode argoParent2 = oOrigInstance;
                                        Status nLiveStatus = (Status)Conversions.ToInteger(getNodeValueByType(ref argoParent2, "//nStatus", XmlDataType.TypeNumber));
                                        oOrigInstance = (XmlElement)argoParent2;
                                        switch (nLiveStatus)
                                        {
                                            case Status.Live:
                                            case Status.Hidden:
                                                {
                                                    // Leave the live content alone, set the pending content as a version
                                                    ObjectType = objectTypes.ContentVersion;
                                                    prepareContentVersionInstance(ref oInstance, nKey, nStatus);
                                                    nKey = 0L;
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
                                        ExeProcessSql("UPDATE tblAudit SET nStatus = " + ((int)Status.DraftSuperceded).ToString() + " FROM tblAudit a INNER JOIN tblContentVersions c ON c.nAuditId = a.nAuditKey AND c.nContentPrimaryId = " + cParentId + " AND (a.nStatus = " + ((int)Status.Pending).ToString() + " or a.nStatus = " + ((int)Status.InProgress).ToString() + ")");
                                    }

                                    else
                                    {

                                        // This is a new item of content - do nothing, let it be added as content, with a status of Pending.


                                    }

                                    break;
                                }


                            case Status.Rejected:
                            case Status.Superceded:
                                {
                                    // remove key
                                    ObjectType = objectTypes.ContentVersion;
                                    break;
                                }
                                // GoTo restart
                        }

                    }

                    return nVersionId;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "contentVersioning", ex, cProcessInfo));
                }

                return default;
            }

            private void prepareContentVersionInstance(ref XmlElement oInstance, long nContentPrimaryId, Status nStatus = Status.Superceded)
            {
                PerfMonLog("DBHelper", "prepareContentVersionInstance");
                string cProcessInfo = "ContentParId = " + nContentPrimaryId;

                try
                {
                    // Set the reference to the parent content
                    oInstance.SelectSingleNode("descendant-or-self::nContentPrimaryId").InnerText = nContentPrimaryId.ToString();

                    // Empty the audit references to create a new one
                    oInstance.SelectSingleNode("descendant-or-self::nAuditId").InnerText = "";

                    if (oInstance.SelectSingleNode("descendant-or-self::nAuditKey") != null)
                    {
                        // don't need this if we don't have the related audit feilds.

                        oInstance.SelectSingleNode("descendant-or-self::nAuditKey").InnerText = "";
                        // Change the publish date on the version to the last updated date for the content we are archiving
                        oInstance.SelectSingleNode("descendant-or-self::dInsertDate").InnerText = oInstance.SelectSingleNode("descendant-or-self::dUpdateDate").InnerText;

                    }

                    // Set the status
                    oInstance.SelectSingleNode("descendant-or-self::nStatus").InnerText = ((int)nStatus).ToString();
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "prepareContentVersionInstance", ex, cProcessInfo));
                }


            }

            private long setNewContentVersionInstance(ref XmlElement oInstance, long nContentPrimaryId, Status nStatus = Status.Superceded)
            {
                PerfMonLog("DBHelper", "setNewContentVersionInstance");
                string cProcessInfo = "ContentParId = " + nContentPrimaryId;
                long nVersionId = 0L;
                try
                {
                    // Prepare the instance
                    prepareContentVersionInstance(ref oInstance, nContentPrimaryId, nStatus);

                    // Save the intance
                    nVersionId = Conversions.ToLong(setObjectInstance(objectTypes.ContentVersion, oInstance, 0L));

                    return nVersionId;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "setNewContentVersionInstance", ex, cProcessInfo));
                    return default;
                }

            }

            public XmlElement GetVersionInstance(long nContentPrimaryId, long nContentVersionId)
            {

                string cProcessInfo = "ContentParId = " + nContentPrimaryId;
                var oTempInstance = moPageXml.CreateElement("instance");
                try
                {
                    // grab some values from the live version
                    oTempInstance.InnerXml = getObjectInstance(objectTypes.Content, nContentPrimaryId);
                    long nAuditId = Conversions.ToLong(oTempInstance.SelectSingleNode("tblContent/nAuditKey").InnerText);
                    long nVersion = Conversions.ToLong(oTempInstance.SelectSingleNode("tblContent/nVersion").InnerText);
                    long nStatus = Conversions.ToLong(oTempInstance.SelectSingleNode("tblContent/nStatus").InnerText);
                    string sInsertDate = oTempInstance.SelectSingleNode("tblContent/dInsertDate").InnerText;
                    string sInsertUser = oTempInstance.SelectSingleNode("tblContent/nInsertDirId").InnerText;

                    // pull the content in from the versions table
                    oTempInstance.InnerXml = getObjectInstance(objectTypes.ContentVersion, nContentVersionId);
                    // change to match
                    XmlElement argoNode = (XmlElement)oTempInstance.SelectSingleNode("tblContentVersions");
                    renameNode(ref argoNode, "tblContent");
                    XmlElement argoNode1 = (XmlElement)oTempInstance.SelectSingleNode("tblContent/nContentVersionKey");
                    renameNode(ref argoNode1, "nContentKey");
                    // update some of the values
                    oTempInstance.SelectSingleNode("tblContent/nContentKey").InnerText = nContentPrimaryId.ToString();
                    oTempInstance.SelectSingleNode("tblContent/nAuditKey").InnerText = nAuditId.ToString();
                    oTempInstance.SelectSingleNode("tblContent/nAuditId").InnerText = nAuditId.ToString();
                    oTempInstance.SelectSingleNode("tblContent/nVersion").InnerText = nVersion.ToString();
                    oTempInstance.SelectSingleNode("tblContent/dInsertDate").InnerText = sInsertDate;
                    oTempInstance.SelectSingleNode("tblContent/nInsertDirId").InnerText = sInsertUser;
                    oTempInstance.SelectSingleNode("tblContent/nStatus").InnerText = nStatus.ToString();

                    return oTempInstance;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetVersionInstance", ex, cProcessInfo));
                    return null;
                }

            }

            public long contentStatus(long nContentPrimaryId, long nContentVersionId, Status nStatus = Status.Live)
            {
                PerfMonLog("DBHelper", "setNewContentVersionInstance");
                string cProcessInfo = "ContentParId = " + nContentPrimaryId;
                long nVersionId = 0L;
                try
                {

                    var oTempInstance = GetVersionInstance(nContentPrimaryId, nContentVersionId);

                    oTempInstance.SelectSingleNode("tblContent/nStatus").InnerText = ((int)nStatus).ToString();

                    // Save the intance
                    nVersionId = Conversions.ToLong(setObjectInstance(objectTypes.Content, oTempInstance, 0L));

                    // Superceed previous versions
                    //string cPreviousStatus = "";
                    // If NodeState(oTempInstance, "currentStatus", , , , , , cPreviousStatus) = XmlNodeState.HasContents Then
                    // If cPreviousStatus = "3" Or cPreviousStatus = "4" Then
                    // Update everything with a status of Pending to be DraftSuperceded
                    ExeProcessSql("UPDATE tblAudit SET nStatus = " + ((int)Status.Superceded).ToString() + " FROM tblAudit a INNER JOIN tblContentVersions c ON c.nAuditId = a.nAuditKey AND c.nContentPrimaryId = " + nContentPrimaryId + " AND (a.nStatus = " + ((int)Status.Pending).ToString() + " or a.nStatus = " + ((int)Status.InProgress).ToString() + ")");
                    // End If
                    // End If

                    return nVersionId;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "setNewContentVersionInstance", ex, cProcessInfo));
                    return default;
                }

            }


            public XmlElement getPendingContent(bool bGetContentSinceLastLogged = false)
            {
                PerfMonLog("DBHelper", "getPendingContent");
                string cProcessInfo = "";
                XmlElement pendingList = null;

                try
                {

                    //string cSql = "";
                    // string sContent = "";
                    string dLastRun = "";
                    string cFilterSql = "";

                    var oDS = new DataSet();

                    // Add the filter
                    if (bGetContentSinceLastLogged)
                    {
                        dLastRun = Conversions.ToString(GetDataValue("SELECT TOP 1 dDateTime FROM dbo.tblActivityLog WHERE nActivityType=" + ((int)ActivityType.PendingNotificationSent).ToString() + " ORDER BY 1 DESC"));
                        if (!string.IsNullOrEmpty(dLastRun) && Information.IsDate(dLastRun))
                            cFilterSql = " WHERE Last_Updated > " + SqlDate(dLastRun, true);
                    }

                    // Get the pending content

                    oDS = myWeb.moDbHelper.GetDataSet("SELECT * FROM vw_VersionControl_GetPendingContent" + cFilterSql, "Pending", "GenericReport");

                    if (oDS.Tables.Count > 0 && oDS.Tables[0].Rows.Count > 0)
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
                            var withBlock = oDS.Tables["Pending"];
                            withBlock.Columns["status"].ColumnMapping = MappingType.Attribute;
                            withBlock.Columns["id"].ColumnMapping = MappingType.Attribute;
                            withBlock.Columns["versionid"].ColumnMapping = MappingType.Attribute;
                            withBlock.Columns["userid"].ColumnMapping = MappingType.Attribute;
                            withBlock.Columns["username"].ColumnMapping = MappingType.Attribute;
                            withBlock.Columns["currentLiveVersion"].ColumnMapping = MappingType.Attribute;
                            withBlock.Columns["pageid"].ColumnMapping = MappingType.Attribute;
                            withBlock.Columns["page"].ColumnMapping = MappingType.Attribute;
                            withBlock.Columns["ContentId"].ColumnMapping = MappingType.Attribute;
                            withBlock.Columns["type"].ColumnMapping = MappingType.Attribute;
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
                        myWeb.moDbHelper.ReturnNullsEmpty(ref oDS);

                        // convert to Xml Dom
                        // var oXml = new XmlDataDocument(oDS);
                        XmlDocument oXml = new XmlDocument();
                        if (oDS.Tables[0].Rows.Count > 0)
                        {
                            oXml.LoadXml(oDS.GetXml());
                        }
                        oXml.PreserveWhitespace = false;

                        pendingList = moPageXml.CreateElement("Content");
                        pendingList.SetAttribute("name", "Content Awaiting Approval");
                        pendingList.SetAttribute("type", "Report");
                        if (!string.IsNullOrEmpty(dLastRun) && Information.IsDate(dLastRun))
                            pendingList.SetAttribute("since", XmlDate(dLastRun, true));
                        pendingList.InnerXml = oXml.InnerXml;

                        // Tidy Up - Get rid of all that orphan content from the relations
                        foreach (XmlNode oOrphan in pendingList.FirstChild.SelectNodes("*[name()!='Pending']"))
                            pendingList.FirstChild.RemoveChild(oOrphan);


                        // Tidy Up XMl Nodes
                        foreach (XmlElement oElmt in pendingList.SelectNodes("//*[local-name()='UserXml' or local-name()='ContentXml']"))
                        {
                            XmlElement xmloElmt = oElmt;
                            SetInnerXmlThenInnerText(ref xmloElmt, oElmt.InnerText);
                        }

                        // Tidy Up - Move all Locations and Relations into a Metadata Node
                        XmlElement oLocations = null;
                        XmlElement oRelations = null;
                        XmlElement Metadata = null;

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
                    {
                        CommitLogToDB(ActivityType.PendingNotificationSent, myWeb.mnUserId, "", DateTime.Now, bOverrideLoggingChecks: true);
                    }

                    return pendingList;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getPendingContent", ex, cProcessInfo));
                    return null;
                }

            }
            #endregion



            public XmlElement createFakeInstance(objectTypes objectType, XmlElement oValueElmt = null)
            {
                PerfMonLog("DBHelper", "createFakeInstance");
                var oElmt3 = moPageXml.CreateElement(getTable(objectType));
                if (oValueElmt != null)
                {
                    oElmt3.AppendChild(oValueElmt);
                }
                var oElmt4 = moPageXml.CreateElement("instance");
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

                    if (oDs.Tables[0].Rows.Count > 0)
                    {
                        ReturnNullsEmpty(ref oDs);

                        oDs.Tables[0].Columns[0].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[1].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[2].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[3].ColumnMapping = MappingType.Attribute;

                        var oXml = GetXml(oDs);

                        oDs.EnforceConstraints = false;
                        foreach (XmlNode oNode in oXml.DocumentElement.SelectNodes("group"))
                        {
                            oElmt = (XmlElement)oNode;
                            if (Conversions.ToInteger("0" + oElmt.GetAttribute("isMember")) > 0)
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
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getGroupsInstance", ex, cProcessInfo));
                    return null;
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
                    string cTableKey = getKey((int)objectType);
                    string cTableFRef = getFRef(objectType);

                    nId = getObjectByRef(cTableName, cTableKey, cTableFRef, objectType, cForeignRef, cSchemaType).ToString();
                    return Conversions.ToLong(nId);
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, cProcName, ex, cProcessInfo));

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
                    cTableKey = getKey((int)objectType);
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
                            case objectTypes.Content:
                                {
                                    sSql = "select " + cTableKey + " from " + cTableName + " where " + cTableFRef + " = '" + SqlFmt(cForeignRef) + "' and cContentSchemaName='" + cSchemaType + "'";
                                    break;
                                }
                            // sSql = "select " & getKey(objectType) & " from " & getTable(objectType) & " where " & getFRef(objectType) & " = '" & SqlFmt(cForeignRef) & "' and cContentSchemaName='" & cSchemaType & "'"
                            case objectTypes.Directory:
                                {
                                    sSql = "select " + cTableKey + " from " + cTableName + " where " + cTableFRef + " = '" + SqlFmt(cForeignRef) + "' and cDirSchema='" + cSchemaType + "'";
                                    break;
                                }
                                // sSql = "select " & getKey(objectType) & " from " & getTable(objectType) & " where " & getFRef(objectType) & " = '" & SqlFmt(cForeignRef) & "' and cDirSchema='" & cSchemaType & "'"
                        }
                    }

                    using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {
                        if (oDr is null)
                            return 0L;
                        while (oDr.Read())
                            nId = Conversions.ToString(oDr[0]);

                        return Conversions.ToLong(nId);
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, cProcName, ex, cProcessInfo));

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
                        sSql = "select " + getKey((int)objectType) + " from " + getTable(objectType) + " where " + getFRef(objectType) + " = '" + SqlFmt(cForiegnRef) + "'";
                    }
                    else
                    {
                        switch (objectType)
                        {
                            case objectTypes.Content:
                                {
                                    sSql = "select " + getKey((int)objectType) + " from " + getTable(objectType) + " where " + getFRef(objectType) + " = '" + SqlFmt(cForiegnRef) + "' and cContentSchemaName='" + cSchemaType + "'";
                                    break;
                                }
                            case objectTypes.Directory:
                                {
                                    sSql = "select " + getKey((int)objectType) + " from " + getTable(objectType) + " where " + getFRef(objectType) + " = '" + SqlFmt(cForiegnRef) + "' and cDirSchema='" + cSchemaType + "'";
                                    break;
                                }
                        }
                    }

                    using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {
                        if (oDr is null)
                            return null;
                        while (oDr.Read())
                        {
                            if (!string.IsNullOrEmpty(nIds))
                                nIds += ",";
                            nIds = Conversions.ToString(nIds + oDr[0]);
                        }

                        return Strings.Split(nIds, ",");
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getObjectByRef", ex, cProcessInfo));
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
                    string cTableKey = getKey((int)objectType);
                    string cTableFRef = getFRef(objectType);

                    // SQL to save the fref value
                    switch (objectType)
                    {
                        case objectTypes.Content:
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
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, cProcName, ex, cProcessInfo));

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
                    sSql = "insert into tblAudit (dPublishDate, dExpireDate, dInsertDate, nInsertDirId, dUpdateDate, nUpdateDirId, nStatus, cDescription)" + " Values (" + SqlDate(dPublishDate) + ", " + SqlDate(dExpireDate) + "," + SqlDate(dInsertDate, true) + ", " + nUserId + "," + SqlDate(dUpdateDate, true) + ", " + nUserId + "," + nStatus + ", '" + cDescription + "')";
                    // Protean.Tools.Database.SqlDate
                    nId = Conversions.ToInteger(GetIdInsertSql(sSql));

                    return nId;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getAuditId", ex, cProcessInfo));

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
                    XmlElement menu = (XmlElement)moPageXml.SelectSingleNode("/Page/Menu");

                    if (locations != null & menu != null)
                    {


                        // See if any of the locations for this content exist in the current page menu
                        foreach (XmlElement location in locations.SelectNodes("//Location"))
                        {

                            if (menu.SelectSingleNode("//MenuItem[@id=" + location.GetAttribute("pgid") + "]") != null)
                            {
                                foundLocation = true;
                                break;
                            }
                        }

                        // If nothing was found and checkRelatedIfOrphan is flagged up, then find related (parent) content and search that as well
                        if (!foundLocation & checkRelatedIfOrphan)
                        {
                            XmlElement nContentNodeXmlElt = null;
                            XmlElement relations = (XmlElement)getRelationsByContentId(contentId, ref nContentNodeXmlElt, contentRelationType: RelationType.Child);
                            foreach (XmlElement relation in relations.SelectNodes("//Relation"))
                            {
                                foundLocation = checkContentLocationsInCurrentMenu(Conversions.ToLong(relation.GetAttribute("relatedContentId")));
                                if (foundLocation)
                                    break;
                            }



                        }

                    }

                    return foundLocation;
                }


                catch (Exception ex)
                {

                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "checkContentLocationsInCurrentMenu", ex, processInfo));
                    return false;

                }

            }


            public XmlNode getRelationsByContentId(long contentId, [Optional, DefaultParameterValue(null)] ref XmlElement contentNode, RelationType contentRelationType = RelationType.Child | RelationType.Parent)
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
                    ds.Tables[0].Columns["relatedContentId"].ColumnMapping = MappingType.Attribute;
                    ds.Tables[0].Columns["childRelation"].ColumnMapping = MappingType.Attribute;
                    ds.Tables[0].Columns["parentRelation"].ColumnMapping = MappingType.Attribute;
                    ds.EnforceConstraints = false;
                    //var dsXml = new XmlDataDocument(ds);
                    XmlDocument dsXml = new XmlDocument();
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        dsXml.LoadXml(ds.GetXml());
                    }
                    ds = null;


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
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getRelationsByContentId", ex, processInfo));

                    return null;
                }

            }


            public XmlNode getLocationsByContentId(long nContentId, [Optional, DefaultParameterValue(null)] ref XmlElement ContentNode)
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
                    oDs.Tables[0].Columns["pgid"].ColumnMapping = MappingType.Attribute;
                    oDs.Tables[0].Columns["primary"].ColumnMapping = MappingType.Attribute;
                    oDs.Tables[0].Columns["position"].ColumnMapping = MappingType.Attribute;

                    oDs.EnforceConstraints = false;
                    //var oXml = new XmlDataDocument(oDs);
                    XmlDocument oXml = new XmlDocument();
                    if (oDs.Tables[0].Rows.Count > 0)
                    {
                        oXml.LoadXml(oDs.GetXml());
                    }

                    oDs = null;
                    if (ContentNode is null)
                    {
                        oElmt = moPageXml.CreateElement("Content");
                    }
                    else
                    {
                        oElmt = ContentNode;
                    }
                    foreach (XmlElement oLocElmt in oXml.SelectNodes("Content/Location"))
                        oElmt.AppendChild(oElmt.OwnerDocument.ImportNode(oLocElmt, true));

                    return oElmt;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getLocationsByContentId", ex, cProcessInfo));

                    return null;
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
                        ReorderContent(nPageId, nContentId, "MoveTop", cPosition: sPosition);
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "updatePagePosition", ex, cProcessInfo));
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
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "updateLocations", ex, cProcessInfo));
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

                    foreach (DataRow oRow in oDs.Tables[0].Rows)
                    {
                        if (nLocations.Contains(oRow["nStructId"]))
                        {
                            // ignoring existing ones
                            nLocations.Remove(oRow["nStructId"]);
                        }
                        // deleting removed ones
                        else if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oRow["bPrimary"], false, false)))
                        {
                            DeleteObject(objectTypes.ContentLocation, Conversions.ToLong(oRow["nContentLocationKey"]));
                        }
                    }

                    foreach (DictionaryEntry keypair in nLocations)
                        // adding new ones
                        setContentLocation(Conversions.ToLong(keypair.Value), nContentId, false, false, false, sPosition, false);
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "updateLocations", ex, cProcessInfo));
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
                        using (var oDRE = getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
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
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "updateLocationsDetail", ex, cProcessInfo));
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
                    if (Strings.LCase(myWeb.moConfig["AllowContentLocationsSetPrimary"]) == "on")
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
                    ExeProcessSql(sSql);

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
                                setContentLocation(Conversions.ToLong(nLoc[(int)i]), nContentId, true, false);
                            }
                            else
                            {
                                setContentLocation(Conversions.ToLong(nLoc[(int)i]), nContentId, false, false);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "updateLocationsWithScope", ex, cProcessInfo));
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
                    foreach (DataRow oRow in oDs.Tables[0].Rows)
                        DeleteObject(objectTypes.CartShippingRelations, Conversions.ToLong(oRow["nShpRelKey"]));

                    oDs = null;


                    if (!string.IsNullOrEmpty(sLocations))
                    {
                        nLoc = Strings.Split(sLocations, ",", Compare: CompareMethod.Binary);
                        var loopTo = (long)(nLoc.Length - 1);
                        for (i = 0L; i <= loopTo; i++)
                            insertShippingLocation(nOptId, Conversions.ToLong(nLoc[(int)i]), false);
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "updateLocations", ex, cProcessInfo));

                }
            }

            public XmlNode getUserQuizInstance(long nQuizId)
            {
                PerfMonLog("DBHelper", "getUserQuizInstance");
                // Dim oDr As SqlDataReader
                XmlNode oQuizXml = null;
                XmlElement oElmt;
                object dDateTaken;
                long nTimeTaken;
                string cProcessInfo = "";
                try
                {
                    // oDr = getDataReader("SELECT nContentId, cQResultsXml,nQResultsTimeTaken FROM tblQuestionaireResult r WHERE nQResultsKey=" & CStr(nQuizId))
                    using (var oDr = getDataReaderDisposable("SELECT nContentId, cQResultsXml,nQResultsTimeTaken FROM tblQuestionaireResult r WHERE nQResultsKey=" + nQuizId.ToString()))  // Done by nita on 6/7/22
                    {
                        if (oDr.Read())
                        {
                            oQuizXml = moPageXml.CreateElement("container");
                            oQuizXml.InnerXml = Conversions.ToString(oDr["cQResultsXml"]);
                            oQuizXml = oQuizXml.SelectSingleNode("instance");
                            // lets add the contentId of the xFormQuiz to the results set.
                            oElmt = (XmlElement)oQuizXml.SelectSingleNode("descendant-or-self::results");
                            oElmt.SetAttribute("contentId", Conversions.ToString(oDr["nContentId"]));
                            // Add the time taken
                            oElmt = (XmlElement)oQuizXml.SelectSingleNode("//status/timeTaken");
                            if (oDr["nQResultsTimeTaken"] is DBNull)
                                nTimeTaken = 0L;
                            else
                                nTimeTaken = Conversions.ToLong(oDr["nQResultsTimeTaken"]);
                            if (oElmt is null)
                            {
                                var argoNode = oQuizXml.SelectSingleNode("//status");
                                addNewTextNode("timeTaken", ref argoNode, nTimeTaken.ToString());
                            }
                            else
                            {
                                oElmt.InnerText = nTimeTaken.ToString();
                            }
                        }

                    }

                    // let's add the date taken
                    dDateTaken = GetDataValue("SELECT a.dInsertDate FROM tblAudit a INNER JOIN tblQuestionaireResult q ON a.nAuditKey = q.nAuditId AND nQResultsKey=" + nQuizId.ToString());
                    if (!(ReferenceEquals(dDateTaken, DBNull.Value) & dDateTaken is null))
                    {
                        oElmt = (XmlElement)oQuizXml.SelectSingleNode("//status/dateTaken");
                        if (oElmt is null)
                        {
                            var argoNode1 = oQuizXml.SelectSingleNode("//status");
                            addNewTextNode("dateTaken", ref argoNode1, XmlDate(dDateTaken, true));
                        }
                        else
                        {
                            oElmt.InnerText = XmlDate(dDateTaken, true);
                        }
                    }

                    return oQuizXml;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getUserQuizInstance", ex, cProcessInfo));

                    return null;
                }
            }

            public long insertStructure(XmlElement oInstance)
            {
                PerfMonLog("DBHelper", "insertStructure (xmlElement)");
                string cProcessInfo = "";
                try
                {

                    return Conversions.ToLong(setObjectInstance(objectTypes.ContentStructure, oInstance));
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "insertStructure(xmlElement)", ex, cProcessInfo));
                    return default;
                }

            }


            public long insertStructure(long nStructParId, string cStructForiegnRef, string cStructName, string cStructDescription, string cStructLayout, long nStatus = 1L, DateTime? dPublishDate = null, DateTime? dExpireDate = null, string cDescription = "", long nOrder = 0L)
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
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getAuditId", ex, cProcessInfo));

                }

                return default;
            }

            public long insertPageVersion(long nStructParId, string cStructForiegnRef, string cStructName, string cStructDescription, string cStructLayout, long nStatus = 1L, DateTime? dPublishDate = null, DateTime? dExpireDate = null, string cDescription = "", long nOrder = 0L, long nVersionParId = default, string cVersionLang = "", string cVersionDescription = "", PageVersionType nVersionType = default)
            {
                PerfMonLog("DBHelper", "insertStructure ([args])");
                string sSql;
                string nId;
                string cProcessInfo = "";
                try
                {
                    sSql = "Insert Into tblContentStructure (nStructParId, cStructForiegnRef, cStructName,  cStructDescription, cStructLayout, nAuditId, nStructOrder, nVersionParId, cVersionLang, cVersionDescription, nVersionType)" + "values (" + nStructParId + ",'" + SqlFmt(cStructForiegnRef) + "'" + ",'" + SqlFmt(cStructName) + "'" + ",'" + SqlFmt(cStructDescription) + "'" + ",'" + SqlFmt(cStructLayout) + "'" + "," + getAuditId((int)nStatus, cDescription: cDescription, dPublishDate: dPublishDate, dExpireDate: dExpireDate) + "," + nOrder + "," + nVersionParId + ",'" + cVersionLang + "'" + ",'" + cVersionDescription + "'" + "," + ((int)nVersionType).ToString() + " )";

                    nId = GetIdInsertSql(sSql);
                    return Conversions.ToLong(nId);
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getAuditId", ex, cProcessInfo));

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

                    nId = ExeProcessSql(sSql).ToString();
                    return Conversions.ToLong(nId);
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "moveShippingLocation", ex, cProcessInfo));

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


                    nId = ExeProcessSql(sSql).ToString();
                    clearStructureCacheAll();
                    return Conversions.ToLong(nId);
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "moveStructure", ex, cProcessInfo));

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
                    int destination = Conversions.ToInteger(GetDataValue("SELECT TOP 1 nStructKey FROM tblContentStructure WHERE nStructKey=" + nNewStructParId));

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
                        using (var currentLocation = getDataReaderDisposable("SELECT TOP 1 CASE WHEN bPrimary IS NULL THEN 0 ELSE bPrimary END as PrimaryLocation, CASE WHEN bCascade IS NULL THEN 0 ELSE bCascade END AS cascadeLocation, CASE WHEN cPosition IS NULL THEN '' ELSE cPosition END AS position from tblContentLocation where nStructId = " + nStructKey + " and nContentId = " + nContentKey))  // Done by nita on 6/7/22
                        {
                            if (currentLocation.Read())
                            {
                                primary = Conversions.ToBoolean(currentLocation[0]);
                                cascade = Conversions.ToBoolean(currentLocation[1]);
                                // position = currentLocation(2)
                                // TS By DEFAULT we move to column1 because it is always on every page.
                            }
                        }

                        // Work out if the destination is primary
                        int destinationPrimary = Conversions.ToInteger(GetDataValue("SELECT TOP 1 bPrimary from tblContentLocation where nStructId = " + nNewStructParId + " and nContentId = " + nContentKey + " AND bPrimary=1"));

                        // Work out if the destination is the only primary
                        int areThereOtherPrimaries = Conversions.ToInteger(GetDataValue("SELECT TOP 1 bPrimary from tblContentLocation where nStructId <> " + nNewStructParId + " and nContentId = " + nContentKey + " AND bPrimary=1"));

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
                        nId = setContentLocation(nNewStructParId, nContentKey, primary, cascade, true, position, true).ToString();
                    }

                    else
                    {

                        nId = nStructKey.ToString();

                    }

                    return Conversions.ToLong(nId);
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "moveContent", ex, cProcessInfo));

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


                string sKeyField = getKey((int)objectType);

                string cProcessInfo = "";
                try
                {
                    // Lets firsttest the object can be ordered
                    if (!string.IsNullOrEmpty(getOrderFname(objectType)))
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
                            sSql = "Select * from " + getTable(objectType) + " where " + getKey((int)objectType) + "=" + nKey.ToString();
                            using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                            {
                                while (oDr.Read())
                                {
                                    if (!(oDr[getParIdFname(objectType)] is DBNull))
                                    {
                                        nParentid = Convert.ToInt64(oDr[getParIdFname(objectType)]);
                                    }
                                }

                            }

                            // Get Siblings
                            sSql = "Select * from " + getTable(objectType) + " where " + getParIdFname(objectType) + "=" + nParentid.ToString() + " order by " + getOrderFname(objectType);
                        }

                        oDs = getDataSetForUpdate(sSql, getTable(objectType), "results");

                        RecCount = oDs.Tables[getTable(objectType)].Rows.Count;
                        i = 1;
                        bool skipnext = false;

                        switch (ReOrderCmd ?? "")
                        {
                            case "MoveTop":
                                {
                                    foreach (DataRow currentORow in oDs.Tables[getTable(objectType)].Rows)
                                    {
                                        oRow = currentORow;
                                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oRow[sKeyField], nKey, false)))
                                        {
                                            oRow[getOrderFname(objectType)] = 1;
                                        }
                                        else
                                        {
                                            oRow[getOrderFname(objectType)] = i + 1;
                                            i = i + 1;
                                        }
                                    }

                                    break;
                                }
                            case "MoveBottom":
                                {
                                    foreach (DataRow currentORow1 in oDs.Tables[getTable(objectType)].Rows)
                                    {
                                        oRow = currentORow1;
                                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oRow[sKeyField], nKey, false)))
                                        {
                                            oRow[getOrderFname(objectType)] = RecCount;
                                        }
                                        else
                                        {
                                            oRow[getOrderFname(objectType)] = i;
                                            i = i + 1;
                                        }
                                    }

                                    break;
                                }
                            case "MoveUp":
                                {
                                    foreach (DataRow currentORow2 in oDs.Tables[getTable(objectType)].Rows)
                                    {
                                        oRow = currentORow2;
                                        if (Conversions.ToBoolean(Operators.AndObject(Operators.ConditionalCompareObjectEqual(oRow[sKeyField], nKey, false), i != 1)))
                                        {
                                            // swap with previous
                                            oDs.Tables[getTable(objectType)].Rows[i - 2][getOrderFname(objectType)] = i;
                                            oRow[getOrderFname(objectType)] = i - 1;
                                        }
                                        else
                                        {
                                            oRow[getOrderFname(objectType)] = i;
                                        }
                                        i = i + 1;
                                    }

                                    break;
                                }
                            case "MoveDown":
                                {
                                    foreach (DataRow currentORow3 in oDs.Tables[getTable(objectType)].Rows)
                                    {
                                        oRow = currentORow3;
                                        if (Conversions.ToBoolean(Operators.AndObject(Operators.ConditionalCompareObjectEqual(oRow[sKeyField], nKey, false), i != RecCount)))
                                        {
                                            // swap with next
                                            oDs.Tables[getTable(objectType)].Rows[i][getOrderFname(objectType)] = i;
                                            oRow[getOrderFname(objectType)] = i + 1;
                                            skipnext = true;
                                        }
                                        else if (!skipnext)
                                        {
                                            oRow[getOrderFname(objectType)] = i;
                                            skipnext = false;
                                        }
                                        i = i + 1;
                                    }

                                    break;
                                }

                            default:
                                {
                                    foreach (DataRow currentORow4 in oDs.Tables[getTable(objectType)].Rows)
                                    {
                                        oRow = currentORow4;
                                        oRow[getOrderFname(objectType)] = i;
                                        i = i + 1;
                                    }

                                    break;
                                }
                        }

                        updateDataset(ref oDs, getTable(objectType));

                        clearStructureCacheAll();

                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "reOrderNode", ex, cProcessInfo));

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
                    using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {
                        while (oDr.Read())
                            cSchemaName = Conversions.ToString(oDr["cContentSchemaName"]);
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

                    bool bExcludeHiddenOnOrdering = Conversions.ToBoolean(Interaction.IIf(Strings.LCase(goConfig["ExcludeHiddenOnOrdering"]) == "on", true, false));

                    if (bExcludeHiddenOnOrdering)
                    {
                        var oDt = new DataTable();
                        oDs.Tables[getTable(objectType)].DefaultView.Sort = "nStatus DESC";
                        oDt = oDs.Tables[getTable(objectType)].DefaultView.ToTable();
                        oDs.Tables[getTable(objectType)].Clear();
                        oDs.Tables[getTable(objectType)].Merge(oDt);
                        oDt.Dispose();
                        oDt = null;
                    }

                    RecCount = oDs.Tables[getTable(objectType)].Rows.Count;
                    i = 1;
                    bool skipnext = false;

                    switch (ReOrderCmd ?? "")
                    {
                        case "MoveTop":
                            {
                                foreach (DataRow currentORow in oDs.Tables[getTable(objectType)].Rows)
                                {
                                    oRow = currentORow;
                                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oRow[sKeyField], nContentId, false)))
                                    {
                                        oRow[getOrderFname(objectType)] = 1;
                                    }
                                    else
                                    {
                                        oRow[getOrderFname(objectType)] = i + 1;
                                        i = i + 1;
                                    }
                                    // non-ideal alternative for updating the entire dataset
                                    sSql = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("update " + getTable(objectType) + " Set nDisplayOrder = ", oRow[getOrderFname(objectType)]), " where "), getKey((int)objectType)), " = "), oRow[getKey((int)objectType)]));
                                    ExeProcessSql(sSql);
                                }

                                break;
                            }
                        case "MoveBottom":
                            {
                                foreach (DataRow currentORow1 in oDs.Tables[getTable(objectType)].Rows)
                                {
                                    oRow = currentORow1;
                                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oRow[sKeyField], nContentId, false)))
                                    {
                                        oRow[getOrderFname(objectType)] = RecCount;
                                    }
                                    else
                                    {
                                        oRow[getOrderFname(objectType)] = i;
                                        i = i + 1;
                                    }
                                    // non-ideal alternative for updating the entire dataset
                                    sSql = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("update " + getTable(objectType) + " Set nDisplayOrder = ", oRow[getOrderFname(objectType)]), " where "), getKey((int)objectType)), " = "), oRow[getKey((int)objectType)]));
                                    ExeProcessSql(sSql);
                                }

                                break;
                            }
                        case "MoveUp":
                            {
                                foreach (DataRow currentORow2 in oDs.Tables[getTable(objectType)].Rows)
                                {
                                    oRow = currentORow2;
                                    if (Conversions.ToBoolean(Operators.AndObject(Operators.ConditionalCompareObjectEqual(oRow[sKeyField], nContentId, false), i != 1)))
                                    {
                                        // swap with previous
                                        oDs.Tables[getTable(objectType)].Rows[i - 2][getOrderFname(objectType)] = i;
                                        sSql = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("update " + getTable(objectType) + " Set nDisplayOrder = ", oDs.Tables[getTable(objectType)].Rows[i - 2][getOrderFname(objectType)]), " where "), getKey((int)objectType)), " = "), oDs.Tables[getTable(objectType)].Rows[i - 2][getKey((int)objectType)]));
                                        ExeProcessSql(sSql);

                                        oRow[getOrderFname(objectType)] = i - 1;
                                        sSql = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("update " + getTable(objectType) + " Set nDisplayOrder = ", oRow[getOrderFname(objectType)]), " where "), getKey((int)objectType)), " = "), oRow[getKey((int)objectType)]));
                                        ExeProcessSql(sSql);
                                    }
                                    else
                                    {
                                        oRow[getOrderFname(objectType)] = i;
                                        sSql = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("update " + getTable(objectType) + " Set nDisplayOrder = ", oRow[getOrderFname(objectType)]), " where "), getKey((int)objectType)), " = "), oRow[getKey((int)objectType)]));
                                        ExeProcessSql(sSql);
                                    }
                                    i = i + 1;
                                }

                                break;
                            }
                        case "MoveDown":
                            {
                                foreach (DataRow currentORow3 in oDs.Tables[getTable(objectType)].Rows)
                                {
                                    oRow = currentORow3;
                                    if (Conversions.ToBoolean(Operators.AndObject(Operators.ConditionalCompareObjectEqual(oRow[sKeyField], nContentId, false), i != RecCount)))
                                    {
                                        // swap with next
                                        oDs.Tables[getTable(objectType)].Rows[i][getOrderFname(objectType)] = i;
                                        oRow[getOrderFname(objectType)] = i + 1;
                                        skipnext = true;
                                    }
                                    else if (!skipnext)
                                    {
                                        oRow[getOrderFname(objectType)] = i;
                                        skipnext = false;
                                    }

                                    // non-ideal alternative for updating the entire dataset
                                    sSql = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("update " + getTable(objectType) + " Set nDisplayOrder = ", oRow[getOrderFname(objectType)]), " where "), getKey((int)objectType)), " = "), oRow[getKey((int)objectType)]));
                                    ExeProcessSql(sSql);

                                    i = i + 1;
                                }

                                break;
                            }

                        default:
                            {
                                foreach (DataRow currentORow4 in oDs.Tables[getTable(objectType)].Rows)
                                {
                                    oRow = currentORow4;
                                    oRow[getOrderFname(objectType)] = i;
                                    i = i + 1;
                                }

                                break;
                            }
                    }
                    string sXml = oDs.GetXml();
                }
                // This won't work as we are drawing from 2 tables
                // updateDataset(oDs, getTable(objectType))

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "reOrderContent", ex, cProcessInfo));

                }
            }

            public void copyPageContent(long nSourcePageId, long nTargetPageId, bool bCopyDescendants, CopyContentType mode, XmlElement oMenuItem = null)
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
                        foreach (DataRow oDr in oDS.Tables["Content"].Rows)
                        {

                            int nContentId = 0;
                            bool bNewItem = false;
                            // Debug.WriteLine(oDr("bPrimary"))
                            if (Conversions.ToBoolean(Operators.AndObject(mode == CopyContentType.Copy, Operators.ConditionalCompareObjectEqual(oDr["bPrimary"], true, false))))
                            {
                                bNewItem = true;
                                nContentId = (int)createContentCopy(Conversions.ToLong(oDr["nContentId"]), null, false);
                                positionReMap[0, (int)copyCount] = Conversions.ToLong(oDr["nContentId"]);
                                positionReMap[1, (int)copyCount] = nContentId;
                                copyCount = copyCount + 1L;
                                var oldPositionReMap = positionReMap;
                                positionReMap = new long[2, (int)(copyCount + 1)];
                                if (oldPositionReMap != null)
                                    for (var i = 0; i <= oldPositionReMap.Length / oldPositionReMap.GetLength(1) - 1; ++i)
                                        Array.Copy(oldPositionReMap, i * oldPositionReMap.GetLength(1), positionReMap, i * positionReMap.GetLength(1), Math.Min(oldPositionReMap.GetLength(1), positionReMap.GetLength(1)));
                            }
                            else if (Conversions.ToBoolean(Operators.AndObject(mode == CopyContentType.CopyForce, Operators.ConditionalCompareObjectEqual(oDr["bPrimary"], true, false))))
                            {
                                bNewItem = true;
                                nContentId = (int)createContentCopy(Conversions.ToLong(oDr["nContentId"]), null, true);
                                positionReMap[0, (int)copyCount] = Conversions.ToLong(oDr["nContentId"]);
                                positionReMap[1, (int)copyCount] = nContentId;
                                copyCount = copyCount + 1L;
                                var oldPositionReMap1 = positionReMap;
                                positionReMap = new long[2, (int)(copyCount + 1)];
                                if (oldPositionReMap1 != null)
                                    for (var i1 = 0; i1 <= oldPositionReMap1.Length / oldPositionReMap1.GetLength(1) - 1; ++i1)
                                        Array.Copy(oldPositionReMap1, i1 * oldPositionReMap1.GetLength(1), positionReMap, i1 * positionReMap.GetLength(1), Math.Min(oldPositionReMap1.GetLength(1), positionReMap.GetLength(1)));
                            }
                            else if (mode == CopyContentType.Locate)
                            {
                                // just get the id
                                nContentId = Conversions.ToInteger(oDr["nContentId"]); // just need to do a locations
                            }
                            else
                            {
                                // locate with  new primaries
                                nContentId = Conversions.ToInteger(oDr["nContentId"]);
                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oDr["bPrimary"], true, false)))
                                    bNewItem = true;
                            }
                            // now set a location
                            setContentLocation(nTargetPageId, nContentId, bNewItem, Conversions.ToBoolean(Interaction.IIf(oDr["bCascade"] is DBNull, false, oDr["bCascade"])), false, Conversions.ToString(Interaction.IIf(oDr["cPosition"] is DBNull, "", oDr["cPosition"])), true);
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
                        oDS.Tables["MenuItem"].Columns["nStructKey"].ColumnMapping = MappingType.Attribute;
                        oDS.Tables["MenuItem"].Columns["nStructParId"].ColumnMapping = MappingType.Hidden;
                        oDS.Relations.Add("Rel01", oDS.Tables["MenuItem"].Columns["nStructKey"], oDS.Tables["MenuItem"].Columns["nStructParId"], false);
                        oDS.Relations["Rel01"].Nested = true;
                        var oMenuXml = new XmlDocument();
                        oMenuXml.InnerXml = oDS.GetXml();
                        // now select the menu item we need
                        oMenuItem = (XmlElement)oMenuXml.SelectSingleNode("descendant-or-self::MenuItem[@nStructKey=" + nSourcePageId + "]");
                        oMenuXml = null;
                        // we need to go through its children and create items
                    }
                    foreach (XmlElement oMenuChild in oMenuItem.ChildNodes)
                    {

                        int nMenuId = Conversions.ToInteger(oMenuChild.GetAttribute("nStructKey")); // new source page id
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
                        oMenuElmt = (XmlElement)oMenuItemXML.DocumentElement.FirstChild.SelectSingleNode("nStructParId");
                        oMenuElmt.InnerText = nTargetPageId.ToString();
                        // get the new id
                        nNewMenuID = Conversions.ToInteger(setObjectInstance(objectTypes.ContentStructure, oMenuItemXML.DocumentElement));
                        // call the function again
                        copyPageContent(nMenuId, nNewMenuID, bCopyDescendants, mode, oMenuChild);
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "copyPageContent", ex, cProcessInfo));

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
                    nContentId = Conversions.ToLong(setObjectInstance(objectTypes.Content, oInstanceXML.DocumentElement));
                    // get any child items related to the origional parent that are
                    // 'not orphan (have no page) or
                    // are related to other items.
                    // - copy relations to the new object.
                    sSql = "select nContentChildId, nDisplayOrder, cRelationtype," + "(select COUNT(nContentId) from tblContentLocation l where l.nContentId = r.nContentChildId) as nLocations, " + "(select COUNT(nContentParentId) from tblContentRelation r2 where r2.nContentChildId = r.nContentChildId) as nRelations, " + "(select COUNT(nContentParentId) from tblContentRelation r3 where r3.nContentParentId = r.nContentChildId and r3.nContentChildId = r.nContentParentId) as twoWay " + "from tblContentRelation r where nContentParentId = " + contentId;

                    oDS2 = GetDataSet(sSql, "Relations", "Relations");
                    foreach (DataRow oDr2 in oDS2.Tables["Relations"].Rows)
                    {
                        if (ForceCopy)
                        {
                            if (!copied.Contains(Conversions.ToLong(oDr2["nContentChildId"])))
                            {
                                string newRelatedContentId = createContentCopy(Conversions.ToLong(oDr2["nContentChildId"]), copied).ToString();
                                insertContentRelation((int)nContentId, newRelatedContentId, Conversions.ToBoolean(oDr2["twoWay"]), Conversions.ToString(oDr2["cRelationType"]), true);
                            }
                        }
                        else if (Conversions.ToBoolean(Operators.AndObject(Operators.ConditionalCompareObjectEqual(oDr2["nLocations"], 0, false), Operators.ConditionalCompareObjectEqual(oDr2["nRelations"], 1, false))))
                        {
                            // we copy and releate because it is orphan and only related to our item
                            string newRelatedContentId = createContentCopy(Conversions.ToLong(oDr2["nContentChildId"]), copied).ToString();
                            insertContentRelation((int)nContentId, newRelatedContentId, Conversions.ToBoolean(oDr2["twoWay"]), Conversions.ToString(oDr2["cRelationType"]), true);
                        }
                        else
                        {
                            // we simply relate because it is either a page or has multiple relations
                            insertContentRelation((int)nContentId, Conversions.ToString(oDr2["nContentChildId"]), Conversions.ToBoolean(oDr2["twoWay"]), Conversions.ToString(oDr2["cRelationType"]), true);
                        }
                    }
                    oContentElmt = null;
                    oInstanceXML = null;

                    return nContentId;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "createContentCopy", ex, cProcessInfo));
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
                    sSql = "Insert Into tblContent (nContentPrimaryId, nVersion, cContentForiegnRef, cContentName,  cContentSchemaName, cContentXmlBrief, cContentXmlDetail, nAuditId)" + "values (" + nParentID + "," + "1" + ",'" + SqlFmt(cContentForiegnRef) + "'" + ",'" + SqlFmt(cContentName) + "'" + ",'" + SqlFmt(cSchemaName) + "'" + ",'" + SqlFmt(cContentXmlBrief) + "'" + ",'" + SqlFmt(cContentXmlDetail) + "'" + "," + getAuditId(nContentStatus, dPublishDate: dPublishDate, dExpireDate: dExpireDate) + ")";

                    nId = GetIdInsertSql(sSql);

                    return Conversions.ToLong(nId);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "insertContent", ex, cProcessInfo));

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
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "insertContent", ex, cProcessInfo));
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
                    if (oDs.Tables["ContentLocation"].Rows.Count == 0)
                    {
                        oRow = oDs.Tables["ContentLocation"].NewRow();
                        oRow["nStructId"] = nStructId;
                        oRow["nContentId"] = nContentId;
                        oRow["bPrimary"] = bPrimary;
                        oRow["bCascade"] = bCascade;
                        oRow["nDisplayOrder"] = nDisplayOrder;
                        if (!string.IsNullOrEmpty(cPosition))
                        {
                            oRow["cPosition"] = cPosition;
                        }
                        oRow["nAuditId"] = getAuditId();
                        oDs.Tables["ContentLocation"].Rows.Add(oRow);
                        bReorderLocations = true;
                    }
                    else
                    {
                        oRow = oDs.Tables["ContentLocation"].Rows[0];
                        oRow.BeginEdit();
                        // if we are allready primary then leave it.. Unless we need for force it as in External Syncronisation XSLT
                        if (bOveridePrimary)
                        {
                            oRow["bPrimary"] = bPrimary;
                        }
                        if (bUpdatePosition & !string.IsNullOrEmpty(cPosition))
                        {
                            oRow["cPosition"] = cPosition;
                        }
                        oRow["bCascade"] = bCascade;
                        oRow.EndEdit();
                        // update the audit table
                    }

                    updateDataset(ref oDs, "ContentLocation", false);
                    nId = Conversions.ToInteger(ExeProcessSqlScalar(sSql)).ToString();

                    if (bReorderLocations)
                    {
                        if (myWeb != null)
                        {
                            if (!string.IsNullOrEmpty(myWeb.mcBehaviourNewContentOrder))
                            {
                                ReorderContent(nStructId, nContentId, myWeb.mcBehaviourNewContentOrder);
                            }
                        }
                    }
                    return Conversions.ToInteger(nId);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "setContentLocation", ex, cProcessInfo));
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
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "setContentLocation", ex, cProcessInfo));
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
                        object oldId = positionReMap[0, row];
                        object newId = positionReMap[1, row];
                        sSql = Conversions.ToString(Operators.ConcatenateObject("select cPosition from tblContentLocation where nStructId = " + pageId + " and  nContentId=", newId));
                        string cPosition = ExeProcessSqlScalar(sSql);
                        if (cPosition != null)
                        {
                            for (int row2 = 0, loopTo1 = positionReMap.GetUpperBound(1); row2 <= loopTo1; row2++)
                            {
                                if (cPosition.EndsWith("-" + positionReMap[0, row2].ToString()))
                                {
                                    cPosition = cPosition.Replace("-" + positionReMap[0, row2].ToString(), "-" + positionReMap[1, row2].ToString());
                                }
                            }
                            sSql = Conversions.ToString(Operators.ConcatenateObject("update tblContentLocation set cPosition = '" + cPosition + "' where nStructId = " + pageId + " and  nContentId=", newId));
                            ExeProcessSql(sSql);
                        }
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ResetContentPositions", ex, cProcessInfo));
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

                    nId = ExeProcessSql(sSql).ToString();
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "insertShippingLocation", ex, cProcessInfo));
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

                    using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {

                        while (oDr.Read())
                            nId = Conversions.ToString(oDr[0]);


                        return Conversions.ToLong(nId);
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getLocationByRef", ex, cProcessInfo));

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

                    nId = Conversions.ToString(GetDataValue(sSql));

                    return nId;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getLocationByRef", ex, cProcessInfo));
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

                    using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {

                        while (oDr.Read())
                            nId = Conversions.ToString(oDr[0]);

                        return Conversions.ToLong(nId);
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getLocationByRef", ex, cProcessInfo));

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
                    long nPageId = Conversions.ToLong("0" + oContent.GetAttribute("grabberRoot"));
                    long nTop = Conversions.ToLong("0" + oContent.GetAttribute("grabberItems"));
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
                                cWhereSql += " AND CL.nStructId=" + SqlFmt(nPageId.ToString()) + " ";
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

                        XmlElement argoPageDetail = null;
                        int nCount = 0;
                        myWeb.GetPageContentFromSelect(cWhereSql, ref nCount, bIgnorePermissionsCheck: myWeb.mbAdminMode, nReturnRows: (int)nTop, cOrderBy: cOrderBy, oContentsNode: ref oContent, oPageDetail: ref argoPageDetail);
                        // Get Related Items
                        if (Strings.LCase(goConfig["DisableGrabberRelated"]) != "on")
                        {
                            foreach (XmlElement oContentElmt in oContent.SelectNodes("Content"))
                            {
                                XmlElement xmloContentElmt = oContentElmt;
                                addRelatedContent(ref xmloContentElmt, Conversions.ToInteger(oContentElmt.GetAttribute("id")), myWeb.mbAdminMode);
                            }

                        }

                    }
                }


                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getContentFromContentGrabber", ex, cProcessInfo));
                }
            }

            public virtual void getContentFromProductGroup(ref XmlElement oContent)
            {
                PerfMonLog("DBHelper", "getContentFromModuleGrabber");

                string cProcessInfo = "";
                try
                {


                    string cOrderBy = "";
                    string cAdditionalColumn = "";

                    // Get the parameters SortDirection
                    string cSchema = oContent.GetAttribute("contentType");
                    long nGroupId = Conversions.ToLong("0" + oContent.GetAttribute("groupid"));
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
                            cAdditionalJoin = "INNER Join tblCartCatProductRelations  On c.nContentKey = tblCartCatProductRelations.nContentId and tblCartCatProductRelations.nCatId=" + nGroupId.ToString();
                        }

                        cOrderBy = "tblCartCatProductRelations.nDisplayOrder";
                        cAdditionalColumn = " ,tblCartCatProductRelations.nDisplayOrder  ";

                        // Get Related Items
                        XmlElement argoPageDetail = null;
                        int nCount = 0;
                        myWeb.GetPageContentFromSelect(cWhereSql, ref nCount, bIgnorePermissionsCheck: myWeb.mbAdminMode, nReturnRows: 0, cOrderBy: cOrderBy, oContentsNode: ref oContent, cAdditionalJoins: cAdditionalJoin, oPageDetail: ref argoPageDetail, cAdditionalColumns: cAdditionalColumn);
                        foreach (XmlElement oContentElmt in oContent.SelectNodes("Content"))
                        {
                            XmlElement xmloContentElmt = oContentElmt;
                            addRelatedContent(ref xmloContentElmt, Conversions.ToInteger(oContentElmt.GetAttribute("id")), myWeb.mbAdminMode);
                        }
                    }
                }


                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getContentFromContentGrabber", ex, cProcessInfo));
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
                    XmlNode argoParent = oGrabber;
                    string cSchema = Conversions.ToString(getNodeValueByType(ref argoParent, "Type"));
                    oGrabber = (XmlElement)argoParent;
                    XmlNode argoParent1 = oGrabber;
                    long nPageId = Conversions.ToLong(getNodeValueByType(ref argoParent1, "Page", XmlDataType.TypeNumber));
                    oGrabber = (XmlElement)argoParent1;
                    XmlNode argoParent2 = oGrabber;
                    long nTop = Conversions.ToLong(getNodeValueByType(ref argoParent2, "NumberOfItems", XmlDataType.TypeNumber));
                    oGrabber = (XmlElement)argoParent2;
                    XmlNode argoParent3 = oGrabber;
                    string cSort = Conversions.ToString(getNodeValueByType(ref argoParent3, "Sort"));
                    oGrabber = (XmlElement)argoParent3;
                    XmlNode argoParent4 = oGrabber;
                    string cSortDirection = Conversions.ToString(getNodeValueByType(ref argoParent4, "SortDirection"));
                    oGrabber = (XmlElement)argoParent4;
                    XmlNode argoParent5 = oGrabber;
                    string cIncludeChildPages = Conversions.ToString(getNodeValueByType(ref argoParent5, "IncludeChildPages"));
                    oGrabber = (XmlElement)argoParent5;
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
                                cWhereSql += " AND CL.nStructId=" + SqlFmt(nPageId.ToString()) + " ";
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
                        XmlElement argoPageDetail = null;
                        int nCount = 0;
                        XmlElement nContentNodeElmyt = null;
                        myWeb.GetPageContentFromSelect(cWhereSql, ref nCount, ref nContentNodeElmyt, oPageDetail: ref argoPageDetail, false, false, nReturnRows: (int)nTop, cOrderBy: cOrderBy, cAdditionalJoins: joinSQL);
                        PerfMonLog("DBHelper", "getContentFromContentGrabber-End");

                    }
                }


                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getContentFromContentGrabber", ex, cProcessInfo));
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
                    using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {
                        while (oDr.Read())
                            nId = Conversions.ToString(oDr[0]);

                        return Conversions.ToLong(nId);
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getContentByRef", ex, cProcessInfo));

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

                    using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {

                        while (oDr.Read())
                            cContent = Conversions.ToString(oDr[0]);

                        return cContent;
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getContentByRef", ex, cProcessInfo));

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

                    oXml.InnerXml = Conversions.ToString(oRow["cContentXmlBrief"]);
                    oPathElmt = (XmlElement)oXml.SelectSingleNode(URLXpath);
                    if (oPathElmt != null)
                    {
                        sPath = oPathElmt.InnerText;
                    }
                    if (oPathElmt is null | string.IsNullOrEmpty(sPath))
                    {
                        cProcessInfo = "No 'Path' in Brief";
                        oXml.InnerXml = Conversions.ToString(oRow["cContentXmlDetail"]);
                        oPathElmt = (XmlElement)oXml.SelectSingleNode(URLXpath);
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
                        return goServer.MapPath("/") + goServer.UrlDecode(convertEntitiesToString(convertEntitiesToString(sPath)));
                    }
                    else
                    {
                        return goServer.MapPath("/" + goServer.UrlDecode(convertEntitiesToString(convertEntitiesToString(sPath))));
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getContentFilePath", ex, cProcessInfo));
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

                    using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {

                        while (oDr.Read())
                            sContentSchemaName = Conversions.ToString(oDr[0]);

                        return sContentSchemaName;
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getContentType", ex, cProcessInfo));

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
                        var withBlock = oDs.Tables["Version"];
                        withBlock.Columns["id"].ColumnMapping = MappingType.Attribute;
                        withBlock.Columns["primaryId"].ColumnMapping = MappingType.Attribute;
                        withBlock.Columns["version"].ColumnMapping = MappingType.Attribute;
                        withBlock.Columns["ref"].ColumnMapping = MappingType.Attribute;
                        withBlock.Columns["name"].ColumnMapping = MappingType.Attribute;
                        withBlock.Columns["type"].ColumnMapping = MappingType.Attribute;
                        withBlock.Columns["status"].ColumnMapping = MappingType.Attribute;
                        withBlock.Columns["publish"].ColumnMapping = MappingType.Attribute;
                        withBlock.Columns["expire"].ColumnMapping = MappingType.Attribute;
                        withBlock.Columns["update"].ColumnMapping = MappingType.Attribute;
                        withBlock.Columns["owner"].ColumnMapping = MappingType.Attribute;
                        withBlock.Columns["updater"].ColumnMapping = MappingType.Attribute;
                    }

                    sSqlVersions = "" + "select c.nContentVersionKey as id,  c.nContentPrimaryId as primaryId,  c.nVersion as version,  cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, a.nStatus as status, a.dInsertDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], dins.cDirName as owner, dins.cDirXml as ownerDetail, dupd.cDirName as updater, dupd.cDirXml as updaterDetail " + "from tblContentVersions c inner join tblAudit a on c.nAuditId = a.nAuditKey inner join tblDirectory dins on dins.nDirKey = a.nInsertDirId inner join tblDirectory dupd on dupd.nDirKey = a.nUpdateDirId where nContentPrimaryId = " + nContentId + " ORDER BY c.nVersion DESC";

                    oDs2 = GetDataSet(sSqlVersions, "Version", "ContentVersions");
                    {
                        var withBlock1 = oDs2.Tables["Version"];
                        withBlock1.Columns["id"].ColumnMapping = MappingType.Attribute;
                        withBlock1.Columns["primaryId"].ColumnMapping = MappingType.Attribute;
                        withBlock1.Columns["version"].ColumnMapping = MappingType.Attribute;
                        withBlock1.Columns["ref"].ColumnMapping = MappingType.Attribute;
                        withBlock1.Columns["name"].ColumnMapping = MappingType.Attribute;
                        withBlock1.Columns["type"].ColumnMapping = MappingType.Attribute;
                        withBlock1.Columns["status"].ColumnMapping = MappingType.Attribute;
                        withBlock1.Columns["publish"].ColumnMapping = MappingType.Attribute;
                        withBlock1.Columns["expire"].ColumnMapping = MappingType.Attribute;
                        withBlock1.Columns["update"].ColumnMapping = MappingType.Attribute;
                        withBlock1.Columns["owner"].ColumnMapping = MappingType.Attribute;
                        withBlock1.Columns["updater"].ColumnMapping = MappingType.Attribute;
                    }

                    oDs.Merge(oDs2);

                    // tidy the user details
                    oRoot.InnerXml = oDs.GetXml();

                    foreach (XmlElement oElmt in oRoot.SelectNodes("ContentVersions/Version/ownerDetail | ContentVersions/Version/updaterDetail "))
                        oElmt.InnerXml = oElmt.InnerText;

                    return (XmlElement)oRoot.FirstChild;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getContentVersions", ex, cProcessInfo));
                    return null;
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
                        var withBlock = oDs.Tables["Version"];
                        withBlock.Columns["id"].ColumnMapping = MappingType.Attribute;
                        withBlock.Columns["primaryId"].ColumnMapping = MappingType.Attribute;
                        withBlock.Columns["description"].ColumnMapping = MappingType.Attribute;
                        withBlock.Columns["ref"].ColumnMapping = MappingType.Attribute;
                        withBlock.Columns["name"].ColumnMapping = MappingType.Attribute;
                        withBlock.Columns["type"].ColumnMapping = MappingType.Attribute;
                        withBlock.Columns["lang"].ColumnMapping = MappingType.Attribute;
                        withBlock.Columns["status"].ColumnMapping = MappingType.Attribute;
                        withBlock.Columns["publish"].ColumnMapping = MappingType.Attribute;
                        withBlock.Columns["expire"].ColumnMapping = MappingType.Attribute;
                        withBlock.Columns["update"].ColumnMapping = MappingType.Attribute;
                        withBlock.Columns["owner"].ColumnMapping = MappingType.Attribute;
                        withBlock.Columns["updater"].ColumnMapping = MappingType.Attribute;
                        withBlock.Columns["groups"].ColumnMapping = MappingType.Attribute;
                    }

                    sSqlVersions = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("" + "select p.nStructKey as id,  p.nVersionParId as primaryId,  p.cVersionDescription as description,  p.cStructForiegnRef as ref, cStructName as name, nVersionType as type, cVersionLang as lang, a.nStatus as status, a.dInsertDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], dins.cDirName as owner, dins.cDirXml as ownerDetail, dupd.cDirName as updater, dupd.cDirXml as updaterDetail, dbo.fxn_getPageGroups(p.nStructKey) as Groups " + "from tblContentStructure p inner join tblAudit a on p.nAuditId = a.nAuditKey left outer join tblDirectory dins on dins.nDirKey = a.nInsertDirId left outer join tblDirectory dupd on dupd.nDirKey = a.nUpdateDirId  where p.nVersionParId = ", ParPageId), " order by nVersionType, nStructOrder"));
                    oDs2 = GetDataSet(sSqlVersions, "Version", "PageVersions");
                    {
                        var withBlock1 = oDs2.Tables["Version"];
                        withBlock1.Columns["id"].ColumnMapping = MappingType.Attribute;
                        withBlock1.Columns["primaryId"].ColumnMapping = MappingType.Attribute;
                        withBlock1.Columns["description"].ColumnMapping = MappingType.Attribute;
                        withBlock1.Columns["ref"].ColumnMapping = MappingType.Attribute;
                        withBlock1.Columns["name"].ColumnMapping = MappingType.Attribute;
                        withBlock1.Columns["type"].ColumnMapping = MappingType.Attribute;
                        withBlock1.Columns["lang"].ColumnMapping = MappingType.Attribute;
                        withBlock1.Columns["status"].ColumnMapping = MappingType.Attribute;
                        withBlock1.Columns["publish"].ColumnMapping = MappingType.Attribute;
                        withBlock1.Columns["expire"].ColumnMapping = MappingType.Attribute;
                        withBlock1.Columns["update"].ColumnMapping = MappingType.Attribute;
                        withBlock1.Columns["owner"].ColumnMapping = MappingType.Attribute;
                        withBlock1.Columns["updater"].ColumnMapping = MappingType.Attribute;
                        withBlock1.Columns["groups"].ColumnMapping = MappingType.Attribute;
                    }

                    oDs2.Merge(oDs);
                    oRoot.InnerXml = oDs2.GetXml();

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
                        if (myWeb.goLangConfig != null)
                        {
                            if ((oElmt.GetAttribute("lang") ?? "") == (myWeb.goLangConfig.GetAttribute("code") ?? "") | string.IsNullOrEmpty(oElmt.GetAttribute("lang")))
                            {
                                oElmt.SetAttribute("langSystemName", myWeb.goLangConfig.GetAttribute("default"));
                            }
                            else
                            {
                                XmlElement langElmt = (XmlElement)myWeb.goLangConfig.SelectSingleNode("Language[@code='" + oElmt.GetAttribute("lang") + "']");
                                if (langElmt != null)
                                {
                                    oElmt.SetAttribute("langSystemName", langElmt.GetAttribute("systemName"));
                                }
                            }
                        }
                    }

                    return (XmlElement)oRoot.FirstChild;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getContentVersions", ex, cProcessInfo));
                    return null;
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

                    cDirPassword = Tools.Encryption.HashString(cDirPassword, goConfig["MembershipEncryption"], true);

                    if (nId < 1L)
                    {
                        sSql = "Insert Into tblDirectory (cDirSchema, cDirForiegnRef, cDirName, cDirPassword, cDirXml, cDirEmail, nAuditId)" + " values (" + "'" + SqlFmt(cDirSchema) + "'" + ",'" + SqlFmt(cDirForiegnRef) + "'" + ",'" + SqlFmt(cDirName) + "'" + ",'" + SqlFmt(cDirPassword) + "'" + ",'" + SqlFmt(cDirXml) + "'" + ",'" + SqlFmt(cEmail) + "'" + "," + getAuditId(nStatus) + ")";
                        nId = Conversions.ToLong(GetIdInsertSql(sSql));
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
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "insertUser", ex, cProcessInfo));

                }

                return default;
            }

            public void maintainDirectoryRelation(long nParId, long nChildId, bool bRemove = false, object dExpireDate = null, bool bIfExistsDontUpdate = false, string cEmail = null, string cGroup = null, bool isLast = true)
            {
                string sSql;
                // Dim oDr As SqlDataReader
                string cProcessInfo = "";
                // long nDelRelationId = 0L;
                XmlDocument oXml;
                bool bHasChanged = false;
                PerfMonLog(mcModuleName, "maintainMembershipsFromXForm", "start");
                try
                {
                    if (!(nParId == 0L | nChildId == 0L))
                    {
                        // Does relationship exist?
                        sSql = "select * from tblDirectoryRelation where nDirParentId = " + nParId + " and nDirChildId  = " + nChildId;
                        using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
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
                                        while (oDr.Read())
                                        {
                                            // update audit
                                            oXml = new XmlDocument();
                                            if (Information.IsDate(dExpireDate))
                                            {
                                                oXml.LoadXml("<instance><tblAudit><dExpireDate>" + XmlDate(dExpireDate) + "</dExpireDate></tblAudit></instance>");
                                            }
                                            else
                                            {
                                                // this should update the update date and user
                                                oXml.LoadXml("<instance><tblAudit/></instance>");
                                            }
                                            setObjectInstance(objectTypes.Audit, oXml.DocumentElement, Conversions.ToLong(oDr["nAuditId"]));
                                            oXml = null;
                                        }
                                    }
                                }
                                else
                                {
                                    while (oDr.Read())
                                    {
                                        DeleteObject(objectTypes.DirectoryRelation, Conversions.ToLong(oDr["nRelKey"]));
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
                                    sSql = "insert into tblDirectoryRelation(nDirParentId, nDirChildId, nAuditId) values( " + nParId + ", " + nChildId + ", " + getAuditId(dExpireDate: dExpireDate) + ")";
                                }
                                else
                                {
                                    sSql = "insert into tblDirectoryRelation(nDirParentId, nDirChildId, nAuditId) values( " + nParId + ", " + nChildId + ", " + getAuditId() + ")";
                                }
                                ExeProcessSql(sSql);
                                bHasChanged = true;
                            }

                            if (bHasChanged)
                            {
                                // Keep Mailing List In Sync.
                                // If Not cEmail !=hing Then
                                System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");
                                string sMessagingProvider = "";
                                if (moMailConfig != null)
                                {
                                    sMessagingProvider = moMailConfig["MessagingProvider"];
                                }
                                if (moMessaging is null & myWeb != null)
                                {
                                    // myWeb IsNot Nothing prevents being called from bulk imports.

                                    Protean.Providers.Messaging.ReturnProvider RetProv = new Protean.Providers.Messaging.ReturnProvider();
                                    IMessagingProvider moMessaging = RetProv.Get(ref myWeb, sMessagingProvider);
                                }
                                if (moMessaging != null && moMessaging.AdminProcess != null)
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
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "maintainDirectoryRelation", ex, cProcessInfo));
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
                    using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {
                        if (oDr.HasRows)
                        {
                            // if so check bRemove and remove it if nessesary
                            while (oDr.Read())
                            {
                                // update audit
                                // the permission level has changed... update it
                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(nLevel, oDr["nAccessLevel"], false)))
                                {
                                    sSql = Conversions.ToString(Operators.ConcatenateObject("update tblDirectoryPermission set nAccessLevel = " + nLevel + " where nPermKey=", oDr["nPermKey"]));
                                    ExeProcessSql(sSql);
                                }
                            }
                        }

                        else
                        {
                            // if not create it

                            sSql = "insert into tblDirectoryPermission(nDirId, nStructId,nAccessLevel, nAuditId) values( " + nDirId + ", " + nPageId + "," + nLevel + " ," + getAuditId() + ")";
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
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "maintainDirectoryRelation", ex, cProcessInfo));

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
                XmlDocument oXml = new XmlDocument();
                string searchterm = "";
                string sContent;

                string cProcessInfo = "";
                try
                {

                    if (Conversions.ToBoolean(Operators.OrObject(goSession["oDirList"] is null, Operators.ConditionalCompareObjectNotEqual(goSession["cDirListType"], cSchemaName, false))))
                    {
                        switch (cSchemaName ?? "")
                        {
                            case "User":
                                {
                                    searchterm = (string)myWeb.moSession["UserSearch"];
                                    if (goRequest["UserSearch"] == "Search")
                                    {
                                        searchterm = goRequest["search"];
                                        myWeb.moSession.Add("UserSearch", searchterm);
                                    }
                                    if (goRequest["UserSearch"] == "Clear")
                                    {
                                        searchterm = "";
                                        myWeb.moSession.Remove("UserSearch");
                                    }

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

                                    if (!string.IsNullOrEmpty(searchterm))
                                    {
                                        sSql = "execute spSearchUsers @cSearch='" + searchterm + "'";

                                    }

                                    break;
                                }

                            default:
                                {
                                    if (!string.IsNullOrEmpty(goRequest["search"]))
                                    {
                                        sSql = "execute spSearchDirectory @cSearch='" + goRequest["search"] + "', @cSchemaName = '" + cSchemaName + "'";
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
                        ReturnNullsEmpty(ref oDs);
                        if (oDs.Tables.Count > 0)
                        {
                            oDs.Tables[0].Columns[0].ColumnMapping = MappingType.Attribute;

                            //oXml = new XmlDataDocument(oDs);
                            if (oDs.Tables[0].Rows.Count > 0)
                            {
                                oXml.LoadXml(oDs.GetXml());
                            }
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

                            goSession["sDirListType"] = cSchemaName;
                            goSession["oDirList"] = oXml;
                        }
                    }
                    else
                    {
                        oXml = (XmlDocument)goSession["sDirListType"];
                    }
                    oElmt = moPageXml.CreateElement("directory");
                    oElmt.SetAttribute(cSchemaName + "SearchTerm", searchterm);
                    if (oXml == null)
                    {
                        oXml = new XmlDocument();
                        oXml.AppendChild(oElmt);
                    }
                    else
                    {
                        if (oXml.FirstChild != null)
                        {
                            oElmt.InnerXml = oXml.FirstChild.InnerXml;
                        }
                    }
                    // let get the details of the parent object
                    if (nParId != 0L)
                    {
                        sSql = "select * from tblDirectory where nDirKey = " + nParId;
                        using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                        {
                            while (oDr.Read())
                            {
                                oElmt.SetAttribute("parId", nParId.ToString());
                                oElmt.SetAttribute("parType", Conversions.ToString(oDr["cDirSchema"]));
                                oElmt.SetAttribute("parName", Conversions.ToString(oDr["cDirName"]));
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
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getUsers", ex, cProcessInfo));
                    return null;
                }
            }

            public XmlElement GetUserXML(long nUserId, bool bIncludeContacts = true, bool bSkipCheckPagePerm = false)
            {
                PerfMonLog("DBHelper", "GetUserXML");

                // Dim odr As SqlDataReader
                XmlElement root = null;
                XmlElement oElmt;
                string sSql = "";
                string sProcessInfo = "";
                var PermLevel = PermissionLevel.Open;
                string cOverrideUserGroups;
                string cJoinType;
                try
                {
                    cOverrideUserGroups = goConfig["SearchAllUserGroups"];
                    if (nUserId != 0L)
                    {
                        // 

                        // If nPermLevel > 2 Then
                        // myWeb.moPageXml.DocumentElement.SetAttribute("adminMode", getPermissionLevel(nPermLevel))
                        // End If
                        // odr = getDataReader("SELECT * FROM tblDirectory where nDirKey = " & nUserId)
                        using (var oDr = getDataReaderDisposable("SELECT * FROM tblDirectory inner join tblAudit on nAuditkey = nAuditId  where nDirKey = " + nUserId))  // Done by nita on 6/7/22
                        {
                            while (oDr.Read())
                            {
                                root = moPageXml.CreateElement(Conversions.ToString(oDr["cDirSchema"]));
                                root.SetAttribute("id", nUserId.ToString());
                                root.SetAttribute("name", Conversions.ToString(oDr["cDirName"]));
                                root.SetAttribute("fRef", Conversions.ToString(oDr["cDirForiegnRef"]));

                                root.SetAttribute("status", Conversions.ToString(oDr["nStatus"]));
                                // root.SetAttribute("permission", getPermissionLevel(nPermLevel))
                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(oDr["cDirXml"], "", false)))
                                {
                                    root.InnerXml = Conversions.ToString(oDr["cDirXml"]);
                                    foreach (XmlAttribute attr in root.FirstChild.Attributes)
                                        root.SetAttribute(attr.Name, attr.Value);
                                    root.InnerXml = root.SelectSingleNode("*").InnerXml;
                                }
                                // Ignore if myWeb is nothing
                                if (myWeb != null && bSkipCheckPagePerm != true)
                                {
                                    PermLevel = getPagePermissionLevel((long)myWeb.mnPageId);
                                    root.SetAttribute("pagePermission", PermLevel.ToString());
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

                        using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                        {
                            while (oDr.Read())
                            {
                                oElmt = moPageXml.CreateElement(Conversions.ToString(oDr["cDirSchema"]));
                                oElmt.SetAttribute("id", Conversions.ToString(oDr["nDirKey"]));
                                oElmt.SetAttribute("name", Conversions.ToString(oDr["cDirName"]));
                                oElmt.SetAttribute("fRef", Conversions.ToString(oDr["cDirForiegnRef"]));
                                if (!(oDr["Member"] is DBNull))
                                {
                                    oElmt.SetAttribute("isMember", "yes");
                                }
                                oElmt.InnerXml = Conversions.ToString(oDr["cDirXml"]);
                                foreach (XmlAttribute attr in oElmt.FirstChild.Attributes)
                                    oElmt.SetAttribute(attr.Name, attr.Value);
                                oElmt.InnerXml = oElmt.FirstChild.InnerXml;
                                if (!(cOverrideUserGroups == "on"))
                                {
                                    if (!(oDr["Member"] is DBNull))
                                    {
                                        if (oElmt != null & root != null)
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
                            root.SetAttribute("id", nUserId.ToString());
                            root.SetAttribute("old", "true()");
                        }

                        if (bIncludeContacts)
                        {
                            root.AppendChild(GetUserContactsXml((int)nUserId));
                            foreach (XmlElement oCompany in root.SelectNodes("Company[@id!='']"))
                                oCompany.AppendChild(GetUserContactsXml(Conversions.ToInteger(oCompany.GetAttribute("id"))));
                        }

                        if (goConfig["Subscriptions"] == "on" & myWeb != null)
                        {
                            var mySub = new Cms.Cart.Subscriptions(ref myWeb);
                            mySub.AddSubscriptionToUserXML(ref root, (int)nUserId);
                        }
                        // now we want to get the admin permissions for this page


                    }
                    if (root != null)
                    {
                        if (root.SelectSingleNode("cContactTelCountryCode") is null)
                        {
                            root.AppendChild(root.OwnerDocument.CreateElement("cContactTelCountryCode"));
                        }
                    }
                    PerfMonLog("DBHelper", "GetUserXML - END");
                    return root;
                }

                catch (Exception ex)
                {
                    if (string.IsNullOrEmpty(sProcessInfo))
                        sProcessInfo = sSql;
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetUserXML", ex, sProcessInfo));
                    return null;
                }

            }


            public XmlElement GetUserContactsXml(int nUserId)
            {
                PerfMonLog("DBHelper", "GetUserContactsXMl");
                try
                {
                    var oContacts = moPageXml.CreateElement("Contacts");
                    string cSQL = "SELECT * FROM tblCartContact where nContactCartId = 0 and nContactDirId = " + nUserId;
                    var oDS = new DataSet();
                    oDS = GetDataSet(cSQL, "Contact");
                    foreach (DataRow oDRow in oDS.Tables[0].Rows)
                    {
                        var oContact = moPageXml.CreateElement("Contact");
                        foreach (DataColumn oDC in oDS.Tables[0].Columns)
                        {
                            var oIElmt = moPageXml.CreateElement(oDC.ColumnName);
                            if (!(oDRow[oDC.ColumnName] is DBNull))
                            {
                                string cStrContent = Conversions.ToString(oDRow[oDC.ColumnName]);
                                cStrContent = encodeAllHTML(cStrContent);
                                if (cStrContent != null & !string.IsNullOrEmpty(cStrContent))
                                {
                                    if (oDC.ColumnName == "cContactXml")
                                    {
                                        oIElmt.InnerXml = Conversions.ToString(oDRow[oDC.ColumnName]);
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
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetUserContactsXMl", ex, ""));
                    return null;
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

                    return Conversions.ToLong(GetDataValue(sSql));
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getDirParentId", ex, cProcessInfo));
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
                XmlDocument oXml = new XmlDocument();
                string sSqlCompanyCol = "";
                string sSqlCompanyOrder = "";

                string sContent;

                string cProcessInfo = "";
                string cDirXml = "";
                try
                {

                    // If goSession("oDirList") Is Nothing Or goSession("cDirListType") <> cSchemaName Then

                    oElmt = moPageXml.CreateElement("directory");

                    // let get the details of the child object
                    if (nChildId != 0L)
                    {
                        sSql = "select * from tblDirectory where nDirKey = " + nChildId;
                        using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                        {
                            if (oDr.HasRows)
                            {
                                while (oDr.Read())
                                {
                                    oElmt.SetAttribute("childId", nChildId.ToString());
                                    oElmt.SetAttribute("childType", Conversions.ToString(oDr["cDirSchema"]));
                                    cChildSchema = Conversions.ToString(oDr["cDirSchema"]);
                                    oElmt.SetAttribute("childName", Conversions.ToString(oDr["cDirName"]));
                                    cDirXml = Conversions.ToString(oDr["cDirXml"]);

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
                        if (goRequest["relateAs"] == "children")
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
                    ReturnNullsEmpty(ref oDs);
                    if (string.IsNullOrEmpty(sSqlCompanyCol))
                    {
                        oDs.Tables[0].Columns[0].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[1].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[3].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[4].ColumnMapping = MappingType.Attribute;
                    }
                    else
                    {
                        oDs.Tables[0].Columns[0].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[1].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[2].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[4].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[5].ColumnMapping = MappingType.Attribute;
                    }


                    //oXml = new XmlDataDocument(oDs);
                    if (oDs.Tables[0].Rows.Count > 0)
                    {
                        oXml.LoadXml(oDs.GetXml());
                    }
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

                    if (oXml.FirstChild != null)
                    {
                        oElmt.InnerXml = cDirXml + oXml.FirstChild.InnerXml;
                    }


                    return oElmt;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getUsers", ex, cProcessInfo));
                    return null;
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
                        nChildId = Conversions.ToLong(myWeb.moRequest["childId"]);
                    }


                    if (string.IsNullOrEmpty(sParIds))
                    {
                        if (myWeb.moRequest["parentList"] != null)
                        {
                            aParId = myWeb.moRequest["parentList"].Split(',');
                        }
                    }
                    else
                    {
                        aParId = sParIds.Split(',');
                    }

                    if (aParId != null)
                    {
                        var loopTo = (long)Information.UBound(aParId);
                        for (i = 0L; i <= loopTo; i++)
                        {
                            if (Information.IsNumeric(aParId[(int)i]))
                            {
                                nParId = Conversions.ToLong(aParId[(int)i]);
                                if (myWeb.moRequest["relateAs"] == "children" | relateAs == RelationType.Child)
                                {
                                    // OK we are reversing the way this works and relating as children rather than parents.
                                    // this is done for group memberships where companyies, departments etc. are children of groups.
                                    if (string.IsNullOrEmpty(sParIds))
                                    {
                                        // handle the checkbox form
                                        if (!string.IsNullOrEmpty(myWeb.moRequest["rel_" + nParId]))
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
                                    if (!string.IsNullOrEmpty(myWeb.moRequest["rel_" + nParId]))
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
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "saveDirectoryRelations", ex, cProcessInfo));
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
                    using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {
                        while (oDr.Read())
                        {
                            // first we remove
                            DeleteObject(objectTypes.DirectoryRelation, Conversions.ToLong(oDr["nRelKey"]));
                            // then we insert new
                            saveDirectoryRelations(Conversions.ToLong(oDr["nDirChildId"]), nTargetDirId.ToString(), false, bIfExistsDontUpdate: true);
                        }
                    }

                    // Delete Department Directory Relations, Department Permissions and Department

                    if (bDeleteSource)
                    {
                        DeleteObject(objectTypes.Directory, nSourceDirId);
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "moveDirectoryRelations", ex, cProcessInfo));
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
                    using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {
                        while (oDr.Read())
                            DeleteObject(objectTypes.DirectoryRelation, Conversions.ToLong(oDr[0]));
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "moveDirectoryRelations", ex, cProcessInfo));
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

                    nParId = Conversions.ToLong(goRequest["parId"]);

                    foreach (var item in goRequest.Form)
                    {
                        if (Strings.InStr(Conversions.ToString(item), "page_") > 0)
                        {
                            nPageId = Conversions.ToLong(Strings.Replace(Conversions.ToString(item), "page_", "")).ToString();
                            switch (goRequest[Conversions.ToString(item)] ?? "")
                            {
                                case "permit":
                                    {
                                        savePermissions(Conversions.ToLong(nPageId), goRequest["parId"], PermissionLevel.View);
                                        break;
                                    }
                                case "deny":
                                    {
                                        savePermissions(Conversions.ToLong(nPageId), goRequest["parId"], PermissionLevel.Denied);
                                        break;
                                    }
                                case "remove":
                                    {
                                        savePermissions(Conversions.ToLong(nPageId), goRequest["parId"], PermissionLevel.Open);
                                        break;
                                    }
                            }
                        }
                    }
                }


                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "saveDirectoryPermissions", ex, cProcessInfo));
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
                                    maintainPermission(nPageId, nDirId, ((int)nLevel).ToString());
                                }
                                else
                                {
                                    sSql = "select * from tblDirectoryPermission where nStructId = " + nPageId + " and nDirId=" + nDirId;
                                    using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                                    {
                                        while (oDr.Read())
                                            DeleteObject(objectTypes.Permission, Conversions.ToLong(oDr["nPermKey"]));
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
                            case PermissionLevel.Open:
                                {
                                    sSql = "select * from tblDirectoryPermission where nStructId = " + nPageId;
                                    using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                                    {
                                        while (oDr.Read())
                                            DeleteObject(objectTypes.Permission, Conversions.ToLong(oDr["nPermKey"]));
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
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "saveDirectoryRelations", ex, cProcessInfo));
                }

            }

            public void clearDirectoryCache()
            {
                PerfMonLog("DBHelper", "clearDirectoryCache");
                string cProcessInfo = "";
                try
                {

                    goSession["sDirListType"] = null;
                    goSession["oDirList"] = null;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "clearDirectoryCache", ex, cProcessInfo));
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
                            sSql = "DELETE FROM dbo.tblXmlCache " + " WHERE nCacheDirId = " + Tools.Database.SqlFmt(myWeb.mnUserId.ToString());
                        }
                        else
                        {
                            // No user id - delete the cache based on session id.
                            sSql = "DELETE FROM dbo.tblXmlCache " + " WHERE cCacheSessionID = '" + Tools.Database.SqlFmt(myWeb.moSession.SessionID.ToString()) + "'";
                        }
                        ExeProcessSql(sSql);
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "clearStructureCache", ex, sSql));
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
                        if (myWeb != null)
                        {
                            myWeb.moCtx.Application["AdminStructureCache"] = (object)null;
                        }
                        ExeProcessSql(sSql);
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "clearStructureCache", ex, sSql));
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
                            ExeProcessSqlScalar(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("DELETE FROM dbo.tblXmlCache WHERE cCacheSessionId = '", Interaction.IIf(bAuth, SqlFmt(goSession.SessionID), "")), "' AND DATEDIFF(hh,dCacheDate,GETDATE()) > 12")));
                        }
                        else
                        {
                            ExeProcessSqlScalar("DELETE FROM dbo.tblXmlCache WHERE DATEDIFF(hh,dCacheDate,GETDATE()) > 12");
                        }

                        // OPTION 2 - Insert using parameter Also slow
                        string nUpdateCount;
                        sSql = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("INSERT INTO dbo.tblXmlCache (cCacheSessionID,nCacheDirId,cCacheStructure,cCacheType) " + "VALUES (" + "'", Interaction.IIf(bAuth, SqlFmt(goSession.SessionID), "")), "',"), SqlFmt(nUserId.ToString())), ","), " @XmlValue,"), "'"), cCacheType), "'"), ")"));






                        var oCmd = new SqlCommand(sSql, oConn);

                        if (oConn.State == ConnectionState.Closed)
                            oConn.Open();

                        var param = new SqlParameter("@XmlValue", SqlDbType.Xml);
                        param.Direction = ParameterDirection.Input;
                        param.Value = new XmlNodeReader(StructureXml); // StructureXml.OuterXml '
                        param.Size = StructureXml.OuterXml.Length;
                        oCmd.Parameters.Add(param);

                        nUpdateCount = oCmd.ExecuteNonQuery().ToString();

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
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "addStructureCache", ex, sSql));
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
                Tools.Security.Impersonate oImp;
                bool isValidEmailAddress;
                bool areEmailAddressesAllowed;
                int nNumberOfUsers;

                try
                {
                    //this need to be optional based on auth provider config
                    Protean.Providers.Authentication.ReturnProvider oAuthProv = new Protean.Providers.Authentication.ReturnProvider();
                    IEnumerable<IauthenticaitonProvider> oAuthProviders = oAuthProv.Get(ref myWeb);

                    // Does the configuration setting indicate that email addresses are allowed.
                    if (Strings.LCase(myWeb.moConfig["EmailUsernames"]) == "on")
                    {
                        areEmailAddressesAllowed = true;
                    }
                    else
                    {
                        areEmailAddressesAllowed = false;
                    }

                    // Returns true if cUsername is a valid email address.
                    isValidEmailAddress = EmailAddressCheck(cUsername);

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
                        nNumberOfUsers = dsUsers.Tables[0].Rows.Count;

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
                            oUserDetails = dsUsers.Tables[0].Rows[0];
                            cPasswordDatabase = Conversions.ToString(oUserDetails["cDirPassword"]);
                            nUserId = Conversions.ToLong(oUserDetails["nDirKey"]);

                            // here we are checking SAML login is from google or microsoft, if not return error message.
                            if (oAuthProviders != null)
                            {
                                if (oAuthProviders.Count() > 0)
                                {                                    
                                    foreach (IauthenticaitonProvider authProvider in oAuthProviders)
                                    {
                                        Boolean bUse = false;
                                        if (authProvider.config["scope"].ToString() == "admin")
                                        {
                                            bUse = true;
                                        }
                                        if (bUse && authProvider.name.ToLower() == cPasswordForm.ToLower())  // this extra if added because direct checking available provider.
                                        {                                           
                                            if (myWeb.moRequest["SAMLResponse"] != null && authProvider.name == cPasswordDatabase)
                                            {
                                                bValidPassword = true;
                                                break;
                                            }
                                            else
                                            {
                                                return sReturn = "<span class=\"msg-1036\">Login failed. Please use your "+ authProvider.name + " account to sign in.</span>";                                                
                                            }
                                        }                                       
                                    }
                                }
                            }
                            //End Auth Provider

                            if (!(Strings.LCase(myWeb.moConfig["MembershipEncryption"]) == "plain") & !string.IsNullOrEmpty(myWeb.moConfig["MembershipEncryption"]))
                            {
                                string cHashedPassword = Tools.Encryption.HashString(cPasswordForm, Strings.LCase(myWeb.moConfig["MembershipEncryption"]), true); // plain - md5 - sha1

                                switch (myWeb.moConfig["MembershipEncryption"])
                                {
                                    case "md5salt": // we need password from the database, as this has the salt in format: hashedpassword:salt
                                        string[] arrPasswordFromDatabase = Strings.Split(cPasswordDatabase, ":");
                                        if (arrPasswordFromDatabase.Length == 2)
                                        {
                                            // RJP 7 Nov 2012. Note leave the md5 hard coded in the line below.
                                            if ((arrPasswordFromDatabase[0] ?? "") == (Tools.Encryption.HashString(arrPasswordFromDatabase[1] + ADPassword, "md5", true) ?? ""))
                                            {
                                                bValidPassword = true;
                                            }
                                        }
                                        break;
                                    case "SHA2_512_SALT": // to replicate
                                        string salt = oUserDetails["cDirSalt"].ToString().ToUpperInvariant();
                                        string saltedPassword = salt + cPasswordForm.Trim().ToLowerInvariant();
                                        cHashedPassword = Tools.Encryption.HashString(saltedPassword, "sha2_512", true);
                                        if ((cPasswordDatabase ?? "") == cHashedPassword)
                                        {
                                            bValidPassword = true;
                                        }
                                        break;
                                    default:
                                        var oConvDoc = new XmlDocument();
                                        var oConvElmt = oConvDoc.CreateElement("PW");
                                        oConvElmt.InnerText = cHashedPassword;
                                        cHashedPassword = oConvElmt.InnerXml;
                                        cHashedPassword = Strings.Replace(cHashedPassword, "&gt;", ">");
                                        cHashedPassword = Strings.Replace(cHashedPassword, "&lt;", "<");
                                        if (cPasswordDatabase == cHashedPassword)
                                        {
                                            bValidPassword = true;
                                        }                                       
                                        break;
                                }
                            }
                            else if (cPasswordDatabase == cPasswordForm)
                            {
                                bValidPassword = true;
                            }

                            if (bValidPassword == true)
                            {

                                switch (dsUsers.Tables[0].Rows.Count)
                                {

                                    case 0:
                                        {
                                            sReturn = "<span class=\"msg-1015\">These credentials do not match a valid account</span>";
                                            break;
                                        }

                                    case 1:
                                        {
                                            sReturn = Conversions.ToString(oUserDetails["nDirKey"]);

                                            // Check user dates
                                            if (Information.IsDate(oUserDetails["dExpireDate"]))
                                            {
                                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectLess(oUserDetails["dExpireDate"], DateTime.Now, false)))
                                                {
                                                    sReturn = "<span class=\"msg-1016\">User account has expired</span>";
                                                }
                                            }
                                            if (Information.IsDate(oUserDetails["dPublishDate"]))
                                            {
                                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectGreaterEqual(oUserDetails["dPublishDate"], DateTime.Now, false)))
                                                {
                                                    sReturn = "<span class=\"msg-1012\">User account is not active</span>";
                                                }
                                            }
                                            // Check user status
                                            if ((Convert.ToInt32(oUserDetails["nStatus"]) != 1 && Convert.ToInt32(oUserDetails["nStatus"]) != -1))
                                            {
                                                XmlElement oUserXml = GetUserXML(nUserId);

                                                if (oUserXml.SelectSingleNode("ActivationKey") != null)
                                                {
                                                    if (oUserXml.SelectSingleNode("ActivationKey").InnerText != "")
                                                    {
                                                        sReturn = "<span class=\"msg-1021\">User account awaiting activation by email</span>";

                                                    }
                                                }
                                                else
                                                {
                                                    sReturn = "<span class=\"msg-1013\">User account has been disabled</span>";
                                                }


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
                                XmlElement moPolicy = (XmlElement)WebConfigurationManager.GetWebApplicationSection("protean/PasswordPolicy");
                                string retryMsg = "";
                                if (moPolicy != null)
                                {
                                    int nRetrys = Conversions.ToInteger("0" + moPolicy.FirstChild.SelectSingleNode("retrys").InnerText);
                                    if (nRetrys > 0)
                                    {
                                        myWeb.moDbHelper.logActivity(ActivityType.LogonInvalidPassword, nUserId, 0L, 0L, cPasswordForm, cForiegnRef: "");

                                        string sSql2 = "select count(nActivityKey) from tblActivityLog where nActivityType=" + Convert.ToString((int)dbHelper.ActivityType.LogonInvalidPassword) + " and nUserDirId = " + nUserId;
                                        int earlierTries = Conversions.ToInteger(ExeProcessSqlScalar(sSql2));
                                        if (earlierTries >= nRetrys)
                                        {

                                            Protean.Providers.Membership.ReturnProvider RetProv = new Protean.Providers.Membership.ReturnProvider();
                                            IMembershipProvider oMembershipProv = RetProv.Get(ref myWeb, myWeb.moConfig["MembershipProvider"]);
                                            oMembershipProv.Activities.ResetUserAcct(ref myWeb, Convert.ToInt32(nUserId));

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
                            oImp = new Tools.Security.Impersonate();
                            if (oImp.ImpersonateValidUser(cUsername, goConfig["AdminDomain"], ADPassword, true, goConfig["AdminGroup"]))
                            {
                                sReturn = 1.ToString();
                                // RJP 7 Nov 2012. Amended to use Lower Case to prevent against case sensitive entries in Protean.Cms.Config.
                                myWeb.moSession["ewAuth"] = Tools.Encryption.HashString(myWeb.moSession.SessionID + goConfig["AdminPassword"], Strings.LCase(myWeb.moConfig["MembershipEncryption"]), true);

                            }
                        }

                        // check Single User Login
                        if (Cms.gbSingleLoginSessionPerUser && Information.IsNumeric(sReturn) && !string.IsNullOrEmpty(cUsername))
                        {

                            // Find the latest activity for this user within a timeout period - if it isn't logoff then flag up an error
                            string lastSeenActivityQuery = "" + "SELECT TOP 1 nACtivityType FROM tblActivityLog l " + "WHERE nUserDirId = " + sReturn.ToString() + " " + "AND DATEDIFF(s,l.dDateTime,GETDATE()) < " + Cms.gnSingleLoginSessionTimeout.ToString() + " " + "ORDER BY dDateTime DESC ";

                            int lastSeenActivity = Conversions.ToInteger(GetDataValue(lastSeenActivityQuery, CommandType.Text, null, ActivityType.Logoff));
                            if (lastSeenActivity != (int)ActivityType.Logoff)
                            {
                                sReturn = "<span class=\"msg-9017\">This username is currently logged on.  Please wait for them to log off or try another username.</span>";
                            }

                        }

                        if (Information.IsNumeric(sReturn))
                        {
                            // delete failed logon attempts record
                            string sSql2 = "delete from tblActivityLog where nActivityType = " + Convert.ToString((int)dbHelper.ActivityType.LogonInvalidPassword) + " and nUserDirId=" + sReturn;
                            myWeb.moDbHelper.ExeProcessSql(sSql2);

                            // check mailinglist sync

                            // Keep Mailing List In Sync.
                            // If Not cEmail Is Nothing Then
                            System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");
                            string sMessagingProvider = "";
                            if (moMailConfig != null)
                            {
                                sMessagingProvider = moMailConfig["MessagingProvider"];
                            }
                            if (moMessaging is null)
                            {
                                Protean.Providers.Messaging.ReturnProvider RetProv = new Protean.Providers.Messaging.ReturnProvider();
                                IMessagingProvider moMessaging = RetProv.Get(ref myWeb, sMessagingProvider);
                            }
                            if (moMessaging != null && moMessaging.AdminProcess != null)
                            {
                                try
                                {
                                    int UserId = Convert.ToInt16(sReturn);
                                    moMessaging.AdminProcess.SyncUser(ref UserId);
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
                        string one = "1";
                        oImp = new Tools.Security.Impersonate();
                        if (oImp.ImpersonateValidUser(cUsername, goConfig["AdminDomain"], ADPassword, true, goConfig["AdminGroup"]))
                        {
                            // RJP 7 Nov 2012. Amended to use Lower Case to prevent against case sensitive entries in Protean.Cms.Config.
                            myWeb.moSession["ewAuth"] = Tools.Encryption.HashString(myWeb.moSession.SessionID + goConfig["AdminPassword"], Strings.LCase(myWeb.moConfig["MembershipEncryption"]), true);
                            return one;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "validateUser", ex, cProcessInfo));
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
                    if (Tools.Text.IsEmail(cEmail))
                    {
                        // This assumes that email addresses are unique, but in case they're not we'll select
                        // the first active user that has been most recently updated
                        cSql = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("SELECT TOP 1 nDirKey " + "FROM tblDirectory  " + "INNER JOIN tblAudit ON nauditid = nauditkey  " + "WHERE cDirXml LIKE ('%<Email>' + @email + '</Email>%') " + "AND cDirSchema='User'  ", Interaction.IIf(bIncludeInactive, " ", "AND nStatus<>0 ")), "ORDER BY ABS(nStatus) DESC, dUpdateDate DESC "));





                        nReturnId = Conversions.ToInteger(GetDataValue(cSql, CommandType.Text, Tools.Dictionary.getSimpleHashTable("email:" + SqlFmt(cEmail)), -1));
                    }

                    return nReturnId;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetUserIDFromEmail", ex, cSql));
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

                    cSql = "SELECT nDirKey " + "FROM tblDirectory  " + "INNER JOIN tblAudit ON nauditid = nauditkey  " + "WHERE nDirKey = " + SqlFmt(nUserId.ToString()) + " " + "AND cDirSchema='User'  " + "AND nStatus <> 0 " + "AND (dPublishDate is null or dPublishDate = 0 or dPublishDate <= " + SqlDate(DateTime.Now) + " ) " + "AND (dExpireDate is null or dExpireDate = 0 or dExpireDate >= " + SqlDate(DateTime.Now) + " ) ";







                    cIsValidUser = (Convert.ToInt32(GetDataValue(cSql, CommandType.Text, null, -1)) != -1);
                    return cIsValidUser;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "IsValidUser", ex, cSql));
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
                    cReturn = Conversions.ToInteger(ExeProcessSqlScalar(cSQL));
                    if (Information.IsNumeric(cReturn))
                        return cReturn;
                    else
                        return 0;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "DirParentByType", ex, ""));
                }

                return default;
            }

            public XmlElement getDirectoryParentsByType(long nId, DirectoryType oParentType)
            {
                PerfMonLog("DBHelper", "getDirectoryParentsByType");
                DataSet oDs;
                XmlDocument oXml = new XmlDocument();
                string cSql;
                string sContent;

                var oElmt = moPageXml.CreateElement("directory");
                string[] cDirectoryType = new string[] { "User", "Department", "Company", "Group", "Role" };

                cSql = "exec dbo.spGetDirParentByType @nDirId = " + nId + ", @cParentType = '" + cDirectoryType[(int)oParentType] + "'";

                oDs = GetDataSet(cSql, "item", "directory");
                ReturnNullsEmpty(ref oDs);

                //oXml = new XmlDataDocument(oDs);                
                if (oDs.Tables[0].Rows.Count > 0)
                {
                    oXml.LoadXml(oDs.GetXml());
                }
                oDs.EnforceConstraints = false;

                foreach (XmlElement oElmt2 in oXml.SelectNodes("descendant-or-self::cDirXml"))
                {
                    sContent = oElmt2.InnerText;
                    if (!string.IsNullOrEmpty(sContent))
                    {
                        oElmt2.InnerXml = sContent;
                    }
                }

                if (oXml.FirstChild != null)
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
                //bool bValid = false;

                string sProjectPath = goConfig["ProjectPath"] + "";
                string sSenderName = goConfig["SiteAdminName"];
                string sSenderEmail = goConfig["SiteAdminEmail"];


                try
                {
                    sSql = "select * from tblDirectory where cDirSchema = 'User' and cDirXml like '%<Email>" + Strings.LCase(cEmail) + "</Email>%'";

                    using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {
                        if (oDr.HasRows)
                        {
                            while (oDr.Read())
                            {
                                var oXmlDetails = new XmlDocument();
                                oXmlDetails.LoadXml(GetUserXML(Conversions.ToLong(oDr["nDirKey"]), false).OuterXml);

                                // lets add the saved password to the xml
                                var oElmtPwd = oXmlDetails.CreateElement("Password");
                                oElmtPwd.InnerText = Conversions.ToString(oDr["cDirPassword"]);
                                oXmlDetails.SelectSingleNode("User").AppendChild(oElmtPwd);
                                // now lets send the email
                                var oMsg = new Protean.Messaging(ref myWeb.msException);
                                // Set the language
                                if (moPageXml != null && moPageXml.DocumentElement != null && moPageXml.DocumentElement.HasAttribute("translang"))
                                {
                                    oMsg.Language = moPageXml.DocumentElement.GetAttribute("translang");
                                }

                                try
                                {
                                    var fsHelper = new Protean.fsHelper();
                                    string filePath = fsHelper.checkCommonFilePath("/xsl/email/passwordReminder.xsl");

                                    dbHelper argodbHelper = null;
                                    sReturn = Conversions.ToString(oMsg.emailer(oXmlDetails.DocumentElement, goConfig["ProjectPath"] + filePath, sSenderName, sSenderEmail, cEmail, "Password Reminder", odbHelper: ref argodbHelper, "Your reset link has been emailed to you"));

                                    //bValid = true;
                                }
                                catch (Exception)
                                {
                                    sReturn = "Your email failed to send from password reminder";
                                    // bValid = false;
                                }
                                oXmlDetails = null;
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
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "passwordReminder", ex, cProcessInfo));

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

                    using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
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
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "checkUserExists", ex, cProcessInfo));

                }

                return default;
            }

            public bool checkEmailUnique(string cEmail, long nCurrId = 0L)
            {
                PerfMonLog("DBHelper", "checkEmailUnique");
                string sSql = string.Empty;
                // Dim oDr As SqlDataReader
                string cProcessInfo = "";

                try
                {
                    if (Strings.LCase(myWeb.moConfig["EmailUsernames"]) == "on")
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

                    using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
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

                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "checkUserExists", ex, cProcessInfo));
                }
                PerfMonLog("DBHelper", "checkEmailUnique", sSql);
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

                    using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {
                        if (oDr != null)
                        {
                            while (oDr.Read())
                            {
                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oDr["cDirName"], cRoleName, false)))
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
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "checkUserRole", ex, cProcessInfo));

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
                    cOverrideUserGroups = goConfig["SearchAllUserGroups"];

                    root = moPageXml.CreateElement("User");
                    root.SetAttribute("id", nUserId.ToString());
                    if (nUserId != 0)
                    {
                        // odr = getDataReader("SELECT * FROM tblDirectory where nDirKey = " & nUserId)
                        using (var oDr = getDataReaderDisposable("SELECT * FROM tblDirectory where nDirKey = " + nUserId))  // Done by nita on 6/7/22
                        {
                            while (oDr.Read())
                            {
                                root.SetAttribute("name", Conversions.ToString(oDr["cDirName"]));
                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(oDr["cDirXml"], "", false)))
                                {
                                    root.InnerXml = Conversions.ToString(oDr["cDirXml"]);
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

                        using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                        {
                            while (oDr.Read())
                            {
                                oElmt = moPageXml.CreateElement(Conversions.ToString(oDr["cDirSchema"]));
                                oElmt.SetAttribute("id", Conversions.ToString(oDr["nDirKey"]));
                                oElmt.SetAttribute("name", Conversions.ToString(oDr["cDirName"]));
                                if (!(oDr["Member"] is DBNull))
                                {
                                    oElmt.SetAttribute("isMember", "yes");
                                }

                                if (!(cOverrideUserGroups == "on"))
                                {
                                    if (!(oDr["Member"] is DBNull))
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

                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getUserXMLById", ex, sProcessInfo));
                    return null;
                }


            }


            public long logActivity(ActivityType nActivityType, long nUserDirId, long nStructId, long nArtId = 0L, string cActivityDetail = "", string cForiegnRef = "")
            {
                string cSubName = "logActivity(ActivityType,Int,Int,[Int],[String])";
                try
                {
                    return logActivity(nActivityType, nUserDirId, nStructId, nArtId, 0L, cActivityDetail, false, cForiegnRef);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, cSubName, ex, ""));
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
                    if (goSession != null)
                    {
                        sessionId = goSession.SessionID;
                    }

                    if (removePreviousActivitiesFromCurrentSession & !string.IsNullOrEmpty(sessionId))
                    {
                        sSql = "DELETE FROM tblActivityLog WHERE cSessionId=" + SqlString(sessionId) + " AND nActivityType=" + SqlFmt(((int)loggedActivityType).ToString());
                        ExeProcessSql(sSql);
                    }


                    if (Cms.gbIPLogging)
                    {
                        checkForIpAddressCol();
                    }


                    // Generate the SQL statement
                    // First add the values

                    var valuesList = new List<string>();
                    valuesList.Add(userDirId.ToString());
                    valuesList.Add(structId.ToString());
                    valuesList.Add(artId.ToString());
                    valuesList.Add(SqlDate(DateTime.Now, true));
                    valuesList.Add(((int)loggedActivityType).ToString());
                    if (activityDetail != null)
                    {
                        if (activityDetail.Length >= 800)
                        {
                            activityDetail = "TRUNCATED:" + activityDetail.Substring(0, 785);
                        }
                    }
                    valuesList.Add(SqlString(activityDetail));
                    valuesList.Add(SqlString(Conversions.ToString(Interaction.IIf(string.IsNullOrEmpty(sessionId), "Service_" + DateTime.Now.ToString(), sessionId))));
                    if (otherId > 0L)
                        valuesList.Add(otherId.ToString());
                    if (Cms.gbIPLogging && myWeb != null)
                        valuesList.Add(SqlString(Strings.Left(myWeb.moRequest.ServerVariables["REMOTE_ADDR"], 15)));

                    // Now build the SQL
                    sSql = "Insert Into tblActivityLog (nUserDirId, nStructId, nArtId, dDateTime, nActivityType, cActivityDetail, cSessionId";

                    // Handle optional columns
                    if (otherId > 0L)
                        sSql += ",nOtherId";
                    if (Cms.gbIPLogging && myWeb != null)
                        sSql += ",cIPAddress";

                    if (checkTableColumnExists("tblActivityLog", "cForeignRef"))
                    {
                        sSql += ",cForeignRef";
                        valuesList.Add(SqlString(cForiegnRef));
                    }

                    sSql += ") values (" + string.Join(",", valuesList.ToArray()) + ")";

                    return Conversions.ToLong(GetIdInsertSql(sSql));
                }


                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, cSubName, ex, cProcessInfo));

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
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "updateActivity", ex, cActivityDetail));
                }
            }



            public int CommitLogToDB(ActivityType nEventType, int nUserId, string cSessionId, DateTime dDateTime, int nPrimaryId = 0, int nSecondaryId = 0, string cDetail = "", bool bOverrideLoggingChecks = false)
            {

                if (myWeb != null & !bOverrideLoggingChecks)
                {
                    if (!myWeb.Features.ContainsKey("ActivityReporting"))
                        return default;
                }

                try
                {

                    // TS 04/12/11 this logall feature has been added in by someone but seems to break functionaility 
                    // unless logAll is set, no reference to setting logAll is found anywhere else.
                    // can only assume this was added to make some kind of overload work, but breaks standard activityLogging/Reporting
                    // any logic as to wether this should be run prior to this.

                    // If Not goSession Is Nothing And Not (bOverrideLoggingChecks) Then
                    // If Not goSession("LogAll") = 1 And Not goSession("LogAll") = "true" And Not goSession("LogAll") = "on" Then
                    // 'not logging everything if no user and logall not turned on
                    // Exit Function
                    // End If
                    // End If

                    // TS 04/12/11 however have assumed an overload sets negative values too so have reversed the logic.
                    if (goSession != null & !bOverrideLoggingChecks)
                    {
                        if (Conversions.ToBoolean(Operators.OrObject(Operators.OrObject(Operators.ConditionalCompareObjectEqual(goSession["LogAll"], "0", false), Operators.ConditionalCompareObjectEqual(goSession["LogAll"], "false", false)), Operators.ConditionalCompareObjectEqual(goSession["LogAll"], "off", false))))
                        {
                            // not logging everything if no user and logall not turned on
                            return default;
                        }
                    }

                    if (myWeb is null)
                    {
                        Cms.gbIPLogging = false;
                    }

                    string cSQL = "INSERT INTO tblActivityLog (nUserDirId, nStructId, nArtId, dDateTime, nActivityType, cActivityDetail, cSessionId";
                    if (Cms.gbIPLogging)
                        cSQL += ",cIPAddress";
                    cSQL += ") VALUES (";
                    cSQL += nUserId + ",";
                    cSQL += nPrimaryId + ",";
                    cSQL += nSecondaryId + ",";
                    cSQL += SqlDate(dDateTime, true) + ",";
                    cSQL += ((int)nEventType).ToString() + ",";
                    cSQL += "'" + cDetail + "',";
                    cSQL += "'" + cSessionId + "'";
                    if (Cms.gbIPLogging)
                        cSQL += ",'" + SqlFmt(Strings.Left(myWeb.moRequest.ServerVariables["REMOTE_ADDR"], 15)) + "'";
                    cSQL += ")";

                    return Conversions.ToInteger(GetIdInsertSql(cSQL));
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "CommitLogToDB", ex, ""));
                    return 0;
                }
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
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AllowMigration", ex, "AllowMigration"));

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
                    iID = Conversions.ToInteger(ExeProcessSqlScalar(strSQL));
                    return iID;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "FindDirectoryByForiegn", ex, "AllowMigration"));
                    return -1;

                }
            }


            public void checkForIpAddressCol()
            {

                // check for gbIPLogging column
                var strSqlCheckForIpCol = new System.Text.StringBuilder();
                strSqlCheckForIpCol.Append("if NOT Exists(select * from sys.columns where Name = 'cIPAddress' and Object_ID = Object_ID('tblActivityLog')) ");
                strSqlCheckForIpCol.Append("begin ");
                strSqlCheckForIpCol.Append("alter table tblActivityLog add cIPAddress nvarchar(15) NULL  ");
                strSqlCheckForIpCol.Append("end ");

                ExeProcessSql(strSqlCheckForIpCol.ToString());


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
                    if (oDs.Tables[0].Rows.Count == 0)
                    {
                        // check for page-product relation
                        strSQL = "select * from tblcontentstructure P inner join  tblContentLocation C on p.nStructKey = C.nStructId where p.nStructKey= '" + pageId + "'";
                        oDs = GetDataSet(strSQL, "page", "Page");
                    }
                    if (oDs.Tables[0].Rows.Count > 0)
                    {
                        result = true;
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "FindDirectoryByForiegn", ex, "AllowMigration"));
                    return false;

                }
            }

            public void ListOrders(ref XmlElement oContentsXML, Cms.Cart.cartProcess ProcessId, string cSchemaName)
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
                    oRoot.SetAttribute("name", cSchemaName + "s - " + ProcessId.GetType().ToString());
                    if ((int)ProcessId == 0)
                    {
                        sSql = "SELECT nCartOrderKey as id, nCartStatus as status, c.cContactName as name, c.cContactEmail as email, a.dUpdateDate from tblCartOrder inner join tblAudit a on nAuditId = a.nAuditKey left outer join tblCartContact c on (nCartOrderKey = c.nContactCartId and cContactType = 'Billing Address') where (cCartSchemaName= '" + cSchemaName + "') order by nCartOrderKey desc";
                    }
                    else
                    {
                        sSql = "SELECT nCartOrderKey as id, nCartStatus as status, c.cContactName as name, c.cContactEmail as email, a.dUpdateDate from tblCartOrder inner join tblAudit a on nAuditId = a.nAuditKey left outer join tblCartContact c on (nCartOrderKey = c.nContactCartId and cContactType = 'Billing Address') where nCartStatus = " + ((int)ProcessId).ToString() + " and (cCartSchemaName= '" + cSchemaName + "') order by nCartOrderKey desc";
                    }


                    oDs = GetDataSet(sSql, cSchemaName, "List");

                    if (oDs.Tables[0].Rows.Count > 0)
                    {
                        oDs.Tables[0].Columns[0].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[1].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[2].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[3].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[4].ColumnMapping = MappingType.Attribute;

                        // load existing data into the instance
                        oElmt = moPageXml.CreateElement("List");
                        oElmt.InnerXml = oDs.GetXml();

                        oContentsXML.AppendChild(oElmt);
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ListShippingLocations", ex, cProcessInfo));
                }
            }

            public XmlElement DisplayCart(long nCartId, string cCartSchema)
            {
                PerfMonLog("DBHelper", "DisplayCart");
                // Content for the XML that will display all the information stored for the Cart
                // This is a list of cart items (and quantity, price ...), totals,
                // billing & delivery addressi and delivery method.

                DataSet oDs;

                string sSql;
                DataRow oRow;

                XmlElement oElmt;
                XmlDocument oXml = new XmlDocument();
                XmlElement oCartElmt;
                XmlElement oOrderElmt;

                int quant;
                double weight;
                double total;
                double vatAmt;
                double shipCost;


                string cProcessInfo = "";
                try
                {

                    System.Collections.Specialized.NameValueCollection oCartConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/cart");

                    string cSiteURL = oCartConfig["SiteURL"];
                    string cCartURL = oCartConfig["SecureURL"];
                    string nTaxRate = oCartConfig["TaxRate"];
                    string cOrderNoPrefix = oCartConfig["OrderNoPrefix"];
                    string cCurrency = oCartConfig["CurrencySymbol"];

                    oCartElmt = moPageXml.CreateElement("Cart");
                    oCartElmt.SetAttribute("type", Strings.LCase(cCartSchema)); // "order")
                    oCartElmt.SetAttribute("currency", cCurrency);

                    oOrderElmt = moPageXml.CreateElement(cCartSchema);
                    oCartElmt.AppendChild(oOrderElmt);

                    if (!(nCartId > 0L)) // no shopping
                    {
                        oOrderElmt.SetAttribute("status", "Empty"); // set CartXML attributes
                        oOrderElmt.SetAttribute("itemCount", "0"); // to nothing
                        oOrderElmt.SetAttribute("total", "0.00"); // for nothing
                    }
                    else
                    {

                        // Add Totals
                        quant = 0; // get number of items & sum of collective prices (ie. cart total) from db
                        total = 0.0d;
                        weight = 0.0d;

                        sSql = "select i.nCartItemKey as id, i.cItemRef as ref, i.cItemURL as url, i.cItemName as Name, i.nPrice as price, i.nTaxRate as taxRate, i.nQuantity as quantity, i.nShpCat as shippingLevel, i.nDiscountValue as discount,i.nWeight as weight, p.cContentXmlDetail as productDetail, i.nItemOptGrpIdx, i.nItemOptIdx, i.nParentId  from tblCartItem i left join tblContent p on i.nItemId = p.nContentKey where nCartOrderId=" + nCartId;

                        oDs = getDataSetForUpdate(sSql, "Item", "Cart");
                        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                        oDs.Relations.Add("Rel1", oDs.Tables["Item"].Columns["id"], oDs.Tables["Item"].Columns["nParentId"], false);
                        oDs.Relations["Rel1"].Nested = true;
                        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                        foreach (DataRow currentORow in oDs.Tables["Item"].Rows)
                        {
                            oRow = currentORow;


                            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                            if (Conversions.ToInteger(Operators.ConcatenateObject("0", oRow["nParentId"])) == 0)
                            {
                                decimal nOpPrices = 0m;
                                foreach (var oOpRow in oRow.GetChildRows("Rel1"))
                                    // need an option check price bit here


                                    nOpPrices = Conversions.ToDecimal(nOpPrices + Convert.ToInt32(oOpRow["price"]));

                                weight = Conversions.ToDouble(Operators.AddObject(weight, Operators.MultiplyObject(oRow["weight"], oRow["quantity"])));
                                quant = Conversions.ToInteger(Operators.AddObject(quant, oRow["quantity"]));
                                total = Conversions.ToDouble(Operators.AddObject(total, Operators.MultiplyObject(oRow["quantity"], Operators.SubtractObject(Operators.AddObject(oRow["price"], nOpPrices), Conversions.ToInteger(Operators.ConcatenateObject("0", oRow["discount"]))))));
                            }

                        }
                        updateDataset(ref oDs, "Item", true);

                        // add to Cart XML
                        oCartElmt.SetAttribute("status", "Active");
                        oCartElmt.SetAttribute("itemCount", quant.ToString());
                        oCartElmt.SetAttribute("weight", weight.ToString());
                        oCartElmt.SetAttribute("orderType", "");

                        // Add the addresses to the dataset
                        if (nCartId > 0L)
                        {
                            sSql = "select cContactType as type, cContactName as GivenName, cContactCompany as Company, cContactAddress as Street, cContactCity as City, cContactState as State, cContactZip as PostalCode, cContactCountry as Country, cContactTel as Telephone, cContactFax as Fax, cContactEmail as Email, cContactXml as Details from tblCartContact where nContactCartId=" + nCartId;
                            addTableToDataSet(ref oDs, sSql, "Contact");
                        }
                        // Add Items - note - do this AFTER we've updated the prices! 

                        if (oDs.Tables["Item"].Rows.Count > 0)
                        {
                            // cart items
                            oDs.Tables[0].Columns[0].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[0].Columns[1].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[0].Columns[2].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[0].Columns[4].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[0].Columns[5].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[0].Columns[6].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[0].Columns[7].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[0].Columns[8].ColumnMapping = MappingType.Attribute;
                            // cart contacts
                            oDs.Tables["Contact"].Columns[0].ColumnMapping = MappingType.Attribute;

                            //oXml = new XmlDataDocument(oDs);
                            if (oDs.Tables[0].Rows.Count > 0)
                            {
                                oXml.LoadXml(oDs.GetXml());
                            }
                            oDs.EnforceConstraints = false;
                            // Convert the detail to xml
                            foreach (XmlElement currentOElmt in oXml.SelectNodes("/Cart/Item/productDetail | /Cart/Contact/Detail"))
                            {
                                oElmt = currentOElmt;
                                oElmt.InnerXml = oElmt.InnerText;
                                if (oElmt.SelectSingleNode("Content") != null)
                                {
                                    oElmt.InnerXml = oElmt.SelectSingleNode("Content").InnerXml;
                                }
                            }
                            oOrderElmt.InnerXml = oXml.FirstChild.InnerXml;
                        }
                        oXml = null;
                        oDs = null;

                        sSql = "select o.*, dUpdateDate from tblCartOrder o join tblAudit a on nAuditId = a.nAuditKey where nCartOrderKey=" + nCartId;
                        oDs = GetDataSet(sSql, "Order", "Cart");
                        foreach (DataRow currentORow1 in oDs.Tables["Order"].Rows)
                        {
                            oRow = currentORow1;
                            oOrderElmt.SetAttribute("ref", Conversions.ToString(Operators.ConcatenateObject(cOrderNoPrefix, oRow["nCartOrderKey"])));
                            oOrderElmt.SetAttribute("date", Conversions.ToString(oRow["dUpdateDate"]));
                            oOrderElmt.SetAttribute("status", Conversions.ToString(oRow["nCartStatus"]));

                            shipCost = Conversions.ToDouble(Operators.ConcatenateObject("0", oRow["nShippingCost"]));
                            oOrderElmt.SetAttribute("shippingType", Conversions.ToString(Operators.ConcatenateObject(oRow["nShippingMethodId"], "")));
                            oOrderElmt.SetAttribute("shippingCost", shipCost + "");

                            if (Conversions.ToDouble(nTaxRate) > 0d)
                            {

                                vatAmt = (total + shipCost) * (Conversions.ToDouble(nTaxRate) / 100d);

                                oOrderElmt.SetAttribute("totalNet", Strings.FormatNumber(total + shipCost, 2, TriState.True, TriState.False, TriState.False));
                                oOrderElmt.SetAttribute("vatRate", nTaxRate);
                                oOrderElmt.SetAttribute("shippingType", Conversions.ToString(Operators.ConcatenateObject(oRow["nShippingMethodId"], "")));
                                oOrderElmt.SetAttribute("shippingCost", Strings.FormatNumber(shipCost, 2, TriState.True, TriState.False, TriState.False));
                                oOrderElmt.SetAttribute("vatAmt", Strings.FormatNumber(vatAmt, 2, TriState.True, TriState.False, TriState.False));
                                oOrderElmt.SetAttribute("total", Strings.FormatNumber(total + shipCost + vatAmt, 2, TriState.True, TriState.False, TriState.False));
                            }
                            else
                            {
                                oOrderElmt.SetAttribute("totalNet", Strings.FormatNumber(total + shipCost, 2, TriState.True, TriState.False, TriState.False));
                                oOrderElmt.SetAttribute("vatRate", 0.0d.ToString());
                                oOrderElmt.SetAttribute("shippingType", Conversions.ToString(Operators.ConcatenateObject(oRow["nShippingMethodId"], "")));
                                oOrderElmt.SetAttribute("shippingCost", Strings.FormatNumber(shipCost, 2, TriState.True, TriState.False, TriState.False));
                                oOrderElmt.SetAttribute("vatAmt", 0.0d.ToString());
                                oOrderElmt.SetAttribute("total", Strings.FormatNumber(total + shipCost, 2, TriState.True, TriState.False, TriState.False));
                            }



                            if (oRow["cClientNotes"] != System.DBNull.Value || oRow["cClientNotes"].ToString() != "")
                            {
                                oElmt = moPageXml.CreateElement("ClientNotes");
                                oElmt.InnerXml = Conversions.ToString(oRow["cClientNotes"]);
                                oOrderElmt.AppendChild(oElmt.FirstChild);
                            }
                            if (Conversions.ToBoolean(!Operators.ConditionalCompareObjectEqual(Operators.ConcatenateObject(oRow["cSellerNotes"], ""), "", false)))
                            {
                                oElmt = moPageXml.CreateElement("SellerNotes");
                                oElmt.InnerText = Conversions.ToString(oRow["cSellerNotes"]);
                                oOrderElmt.AppendChild(oElmt);
                            }
                        }

                    }

                    return oCartElmt;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, cProcessInfo));
                    return null;
                }

            }

            public void ListUserOrders(ref XmlElement oContentsXML, string cOrderType)
            {
                PerfMonLog("DBHelper", "ListUserOrders");
                XmlElement oRoot;
                XmlElement oElmt;
                XmlElement oElmtOrder;
                string sSql;
                DataSet oDs;

                string cProcessInfo = "";
                string cExtraWhere = "";
                long UserId = Conversions.ToLong(Operators.ConcatenateObject("0", goSession["nEwUserId"]));
                try
                {

                    if (UserId == 0L)
                        UserId = mnUserId;

                    // Basic List
                    oRoot = moPageXml.CreateElement("Content");
                    oRoot.SetAttribute("template", "default");
                    switch (cOrderType ?? "")
                    {
                        case "Order":
                            {
                                oRoot.SetAttribute("name", "Orders");
                                cExtraWhere = "(tblCartOrder.cCartSchemaName = 'Order' or tblCartOrder.cCartSchemaName = '' or tblCartOrder.cCartSchemaName is null )"; // AND (tblCartOrder.nCartStatus = 7)"
                                break;
                            }
                        case "Quote":
                            {
                                oRoot.SetAttribute("name", "Quotes");
                                cExtraWhere = "(tblCartOrder.cCartSchemaName = 'Quote')"; // AND (tblCartOrder.nCartStatus = 7)"
                                break;
                            }
                        case "GiftList":
                            {
                                oRoot.SetAttribute("name", "Gift Lists");
                                cExtraWhere = "(tblCartOrder.cCartSchemaName = 'GiftList')"; // AND (tblCartOrder.nCartStatus = 7)"
                                break;
                            }

                        default:
                            {
                                break;
                            }
                    }

                    sSql = "SELECT tblCartOrder.nCartOrderKey, tblCartOrder.cCartXml FROM tblCartOrder INNER JOIN tblAudit ON tblCartOrder.nAuditId = tblAudit.nAuditKey WHERE tblCartOrder.nCartUserDirId = " + UserId + " AND " + cExtraWhere;
                    oDs = GetDataSet(sSql, cOrderType, "OrderList");
                    foreach (DataRow oDr in oDs.Tables[0].Rows)
                    {
                        oElmt = moPageXml.CreateElement(cOrderType);
                        oElmt.InnerXml = Conversions.ToString(Operators.ConcatenateObject(oDr["cCartXml"], ""));
                        oElmtOrder = (XmlElement)oElmt.FirstChild;
                        if (oElmtOrder != null)
                        {
                            oElmtOrder.SetAttribute("id", Conversions.ToString(oDr["nCartOrderKey"]));
                            oContentsXML.AppendChild(oElmt.FirstChild);
                        }
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ListUserOrders", ex, cProcessInfo));
                }
            }


            public void ListUserVouchers(ref XmlElement oContentsXML)
            {
                PerfMonLog("DBHelper", "ListUserOrders");
                XmlElement oRoot;
                string sSql;
                DataSet oDs;

                string cProcessInfo = "";
                string cExtraWhere = string.Empty;
                long UserId = Conversions.ToLong(Operators.ConcatenateObject("0", goSession["nEwUserId"]));
                XmlDocument oXml = new XmlDocument();
                try
                {

                    if (UserId == 0L)
                        UserId = mnUserId;

                    // Basic List
                    oRoot = moPageXml.CreateElement("Content");
                    oRoot.SetAttribute("template", "default");

                    sSql = "SELECT nCodeKey as id, cCode as code, dIssuedDate as issueDate, dUseDate as usedDate, xUsageData as UsageData FROM tblCodes INNER JOIN tblAudit ON tblCodes.nAuditId = tblAudit.nAuditKey WHERE tblCodes.nIssuedDirId = " + UserId + "";
                    oDs = GetDataSet(sSql, "Voucher", "VoucherList");


                    if (oDs.Tables["Voucher"].Rows.Count > 0)
                    {
                        // cart items
                        oDs.Tables[0].Columns["id"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["issueDate"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["usedDate"].ColumnMapping = MappingType.Attribute;

                        //oXml = new XmlDataDocument(oDs);                        
                        if (oDs.Tables[0].Rows.Count > 0)
                        {
                            oXml.LoadXml(oDs.GetXml());
                        }
                        oDs.EnforceConstraints = false;

                        oContentsXML.InnerXml = oXml.FirstChild.InnerXml;
                    }
                    oXml = null;
                    oDs = null;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ListUserOrders", ex, cProcessInfo));
                }
            }


            public string exportShippingLocations()
            {
                PerfMonLog("DBHelper", "exportShippingLocations");
                string cSql;
                XmlDocument oXml = new XmlDocument();
                DataSet oDs;
                string cXml;

                // Get all the data from the table
                cSql = "select * from ";
                if (goRequest["db"] != null)
                {
                    if (!string.IsNullOrEmpty(goRequest["db"]))
                    {
                        cSql = cSql + "[" + goRequest["db"] + "].";
                    }
                }
                switch (goRequest["version"] ?? "")
                {
                    case "v2":
                        {
                            cSql = cSql + "[dbo].[tbl_ewc_shippingLocations]";
                            break;
                        }
                    case "v4":
                        {
                            cSql = cSql + "[dbo].[tblCartShippingLocations]";
                            break;
                        }
                }
                oDs = GetDataSet(cSql, "tblCartShippingLocations", "Export");
                ReturnNullsEmpty(ref oDs);

                // Put the data into an xml document
                if (oDs is null)
                {
                    cXml = "<Export/>";
                }
                else
                {
                    //oXml = new XmlDataDocument(oDs);
                    if (oDs.Tables[0].Rows.Count > 0)
                    {
                        oXml.LoadXml(oDs.GetXml());
                    }
                    cXml = oXml.InnerXml;
                }


                // Things to ponder - do we need to convert any xml blobs within a field (as their entities will have been converted to &entity;)
                return cXml;

            }

            public string importObjects(XmlElement ObjectsXml, string FeedRef = "", string ReParseXsl = "")
            {
                PerfMonLog("DBHelper", "importObjects");
                string cProcessInfo = "";
                string cContentLocationTable = "";

                string cTableName = string.Empty;
                string cTableKey = string.Empty;
                string cTableFRef = string.Empty;

                string cPreviousTableName = string.Empty;

                bool bDeleteNonEntries = false;
                string cDeleteTempTableName = "";
                string cDefiningField = "";
                string cDefiningFieldValue = "";
                string cDeleteTempType = "Content";
                string cDefiningWhereStmt = "";
                bool bSkipExisting = false;
                bool bResetLocations = true;
                long nResetLocationIfHere = 0L;
                //long ProcessedQty = 0L;
                long completeCount = 0L;
                long startNo = 0L;

                try
                {

                    // Do we allready have a feed running that is not complete ?

                    string FeedCheck = "";
                    if (!string.IsNullOrEmpty(FeedRef))
                    {
                        string sSQL = "select TOP 1 cActivityDetail from tblActivityLog where nActivityType = 44 and cActivityDetail like '" + FeedRef + "%' and not(cActivityDetail like '%Complete') and dDateTime > " + sqlDateTime(DateAndTime.DateAdd(DateInterval.Minute, -60, DateTime.Now)) + " order by dDateTime DESC";
                        FeedCheck = ExeProcessSqlScalar(sSQL);
                    }

                    if (!string.IsNullOrEmpty(FeedCheck))
                    {
                        logActivity(ActivityType.Custom1, mnUserId, 0L, 0L, "Previous Feed Still Processing:" + FeedCheck);
                        return "Previous Feed Still Processing:" + FeedCheck;
                        //return default;
                    }
                    else
                    {
                        // Get the last time the feed run and check it completed
                        string sSQL = "select TOP 1 cActivityDetail from tblActivityLog where nActivityType = 44 and cActivityDetail like '" + FeedRef + "%' order by dDateTime DESC ";
                        FeedCheck = ExeProcessSqlScalar(sSQL) + "";
                        if (FeedCheck.EndsWith(" Processed"))
                        {
                            string sProcessesQty = Strings.Mid(FeedCheck, FeedCheck.IndexOf("Objects, ") + 10, FeedCheck.IndexOf(" Processed") - FeedCheck.IndexOf("Objects, ") - 9);
                            if (Information.IsNumeric(sProcessesQty))
                            {
                                startNo = Conversions.ToLong(sProcessesQty);
                                logActivity(ActivityType.Custom1, mnUserId, 0L, 0L, "Previous Feed Restarted:" + startNo);
                            }
                            else
                            {
                                return "StartNo not found:" + FeedCheck;
                                // return default;
                            }
                        }
                    }
                    if (ObjectsXml != null)
                    {

                        cContentLocationTable = getTable(objectTypes.ContentLocation);

                        // NB NEW STUFF ------------
                        // Check that we want to delete missing objects from the spreadsheet (For Content)
                        if (ObjectsXml.SelectSingleNode("DeleteNonEntries[@enabled='true']") != null)
                        {
                            // Now look for the defining field, this allows say the content to only work with a distinct type of object, as defined by the defining field name

                            if (ObjectsXml.SelectSingleNode("DeleteNonEntries/@sqlWhere") != null)
                            {
                                cDefiningWhereStmt = ObjectsXml.SelectSingleNode("DeleteNonEntries/@sqlWhere").InnerText;
                            }

                            if (ObjectsXml.SelectSingleNode("DeleteNonEntries[@enabled='true']/cDefiningField") != null)
                            {
                                cDefiningField = ObjectsXml.SelectSingleNode("DeleteNonEntries[@enabled='true']/cDefiningField").InnerText.ToString();
                                cDefiningFieldValue = ObjectsXml.SelectSingleNode("DeleteNonEntries[@enabled='true']/cDefiningField/@value").InnerText.ToString();
                                bDeleteNonEntries = true;
                                if (ObjectsXml.SelectSingleNode("DeleteNonEntries/@type") != null)
                                {
                                    cDeleteTempType = ObjectsXml.SelectSingleNode("DeleteNonEntries/@type").InnerText.ToString();
                                }
                                cDeleteTempTableName = "temp_" + DateTime.Now.ToString();
                                cDeleteTempTableName = cDeleteTempTableName.Replace("/", "_").Replace(":", "_").Replace(" ", "_");
                                // Remember to import the SP into the database to be used
                                // The next line is currently not used, it was incase of having to use a Store Procedure, however that did not overcome the collation error
                                // Dim cSQL As String = "exec [spCreateImportTable] '" & cDeleteTempTableName & "'"
                                string cSQL = "CREATE TABLE dbo." + cDeleteTempTableName + " (cImportID nvarchar(800), cTableName nvarchar(50))";
                                ExeProcessSql(cSQL);
                            }
                        }

                        // To delete existing Directory Relations (excluding Admin ones)
                        if (ObjectsXml.SelectSingleNode("DeleteDirRelations[@enabled='true']") != null)
                        {
                            string cSql_Relation_Audits = "DELETE tblAudit from tblAudit a " + "Inner Join tblDirectoryRelation r " + "On r.nAuditId = a.nAuditKey " + "Where r.nDirChildId IN ( " + "Select nDirKey " + "From tblDirectory " + "WHERE nDirKey NOT IN (" + "Select d.nDirKey " + "From tblDirectoryRelation r " + "Inner Join tblDirectory d " + "On r.nDirChildId = d.nDirKey " + "WHERE r.nDirParentId = " + "(SELECT nDirKey From tblDirectory Where cDirForiegnRef = 'Administrator')))";

                            myWeb.moDbHelper.ExeProcessSqlorIgnore(cSql_Relation_Audits);

                            string cSql_Relations = "DELETE " + "From tblDirectoryRelation " + "Where nDirChildId IN ( " + "Select nDirKey " + "From tblDirectory " + "WHERE nDirKey NOT IN (" + "Select d.nDirKey " + "From tblDirectoryRelation r " + "Inner Join tblDirectory d " + "On r.nDirChildId = d.nDirKey " + "WHERE r.nDirParentId = " + "(SELECT nDirKey From tblDirectory Where cDirForiegnRef = 'Administrator')))";

                            myWeb.moDbHelper.ExeProcessSqlorIgnore(cSql_Relations);
                        }
                        // NB NEW STUFF ------------

                        if (ObjectsXml.SelectSingleNode("SkipExisting[@enabled='true']") != null)
                        {
                            bSkipExisting = true;
                        }

                        bool bOrphan = ObjectsXml.SelectSingleNode("NoLocations[@enabled='true']") != null;

                        if (ObjectsXml.SelectSingleNode("ResetLocations[@enabled='false']") != null)
                        {
                            bResetLocations = false;
                        }
                        else
                        {
                            bResetLocations = true;
                            XmlElement resetNode = (XmlElement)ObjectsXml.SelectSingleNode("ResetLocations");
                            if (resetNode != null)
                            {
                                if (Information.IsNumeric(resetNode.GetAttribute("enabled")))
                                {
                                    nResetLocationIfHere = Conversions.ToLong(resetNode.GetAttribute("enabled"));
                                }
                            }
                        }

                        long totalInstances = ObjectsXml.SelectNodes("Instance | instance").Count;

                        string ReturnMessage = FeedRef + " Importing " + totalInstances + " Objects";

                        long logId = logActivity(ActivityType.ContentImport, mnUserId, 0L, 0L, ReturnMessage + " Started");

                        var oTransform = new Protean.XmlHelper.Transform(ref myWeb, ReParseXsl, false);
                        // oTransform.XSLFile = ReParseXsl
                        // oTransform.Compiled = False

                        var Tasks = new dbImport(oConn.ConnectionString, mnUserId);

                        short nThreads = (short)Conversions.ToInteger("0" + myWeb.moConfig["ImportThreads"]);
                        if (nThreads == 0)
                            nThreads = 10;
                        ThreadPool.SetMaxThreads(nThreads, nThreads);


                        List<ManualResetEvent> doneEvents = new List<ManualResetEvent>();
                        // Dim eventsDoneEvt As New System.Threading.ManualResetEvent(False)

                        foreach (XmlElement oInstance in ObjectsXml.SelectNodes("Instance | instance"))
                        {
                            completeCount = completeCount + 1L;
                            if (completeCount > startNo)
                            {

                                ImportStateObj stateObj = new dbImport.ImportStateObj();
                                stateObj.oInstance = oInstance;
                                stateObj.LogId = logId;
                                stateObj.FeedRef = FeedRef;
                                stateObj.CompleteCount = completeCount;
                                stateObj.totalInstances = totalInstances;
                                stateObj.bSkipExisting = bSkipExisting;
                                stateObj.bResetLocations = bResetLocations;
                                stateObj.nResetLocationIfHere = nResetLocationIfHere;
                                stateObj.bOrphan = bOrphan;
                                stateObj.bDeleteNonEntries = bDeleteNonEntries;
                                stateObj.cDeleteTempTableName = cDeleteTempTableName;
                                stateObj.cDeleteTempType = cDeleteTempType;
                                stateObj.moTransform = oTransform;
                                stateObj.oResetEvt = new ManualResetEvent(false);
                                if (oInstance.NextSibling is null)
                                {
                                    stateObj.LastItem = true;
                                }
                                else
                                {
                                    stateObj.LastItem = false;
                                }

                                stateObj.cDefiningWhereStmt = cDefiningWhereStmt;
                                stateObj.cDefiningField = cDefiningField;
                                stateObj.cDefiningFieldValue = cDefiningFieldValue;

                                doneEvents.Add(stateObj.oResetEvt);
                                ThreadPool.QueueUserWorkItem(new WaitCallback(Tasks.ImportSingleObject), stateObj);

                                stateObj = null;
                            }
                        }

                        // ' eventsDoneEvt.WaitOne()
                        // '    If System.Threading.WaitHandle.WaitAll(doneEvents, New TimeSpan(0, 0, 5), False) Then

                        updateActivity(logId, ReturnMessage + " Complete");

                        // Clear Page Cache

                        myWeb.ClearPageCache();
                        return ReturnMessage;
                    }
                    // '       End If

                    // Me.updateActivity(logId, "Importing " & totalInstances & "Objects, " & completeCount & " Complete")

                    // 'lets get the object type from the table name.
                    // cTableName = oInstance.FirstChild.Name

                    // 'return the object type from the table name
                    // Dim oTblName As TableNames
                    // For Each oTblName In [Enum].GetValues(GetType(objectTypes))
                    // If oTblName.ToString = cTableName Then Exit For
                    // Next
                    // Dim oObjType As New objectTypes

                    // '^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                    // 'Disabled on 16/09/2008 the following, due to the incompatible assignment of Value to the Object Types 
                    // 'oTblName = oObjType
                    // oObjType = oTblName
                    // '^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

                    // ' The purpose of this is to try to reduce the amount of table name/key/fref calls
                    // ' so to optimise this for bulk use.
                    // If cTableName <> cPreviousTableName Then
                    // cTableKey = getKey(oObjType)
                    // cTableFRef = getFRef(oObjType)
                    // End If



                    // Dim fRefNode As XmlElement = oInstance.SelectSingleNode(cTableName & "/" & cTableFRef)
                    // Dim fRef As String = fRefNode.InnerText

                    // 'We absolutly do not do anything if no fRef
                    // If Not fRef = "" Then
                    // Dim nId As Long
                    // 'lets get an id if we are updating a record with a foriegn Ref

                    // nId = getObjectByRef(cTableName, cTableKey, cTableFRef, oObjType, fRef)

                    // 'nId = myWeb.moDbHelper.getObjectByRef(cTableName, cTableKey, cTableFRef, oObjType, fRef)

                    // 'if we want to replace the fRef
                    // If Not fRefNode.GetAttribute("replaceWith") = "" Then
                    // fRefNode.InnerText = fRefNode.GetAttribute("replaceWith")
                    // End If

                    // oInstance.SelectSingleNode(cTableName & "/" & cTableFRef)

                    // If Not (bSkipExisting And nId <> 0) Then
                    // nId = setObjectInstance(oObjType, oInstance, nId)
                    // End If

                    // ' PerfMonLog("DBHelper", "importObjects", "objectId=" & nId)

                    // processInstanceExtras(nId, oInstance, bResetLocations, bOrphan)

                    // 'NB NEW STUFF ------------
                    // If bDeleteNonEntries Then

                    // Dim cSQL As String = "INSERT INTO dbo." & cDeleteTempTableName & " (cImportID , cTableName) VALUES ('" & SqlFmt(fRef) & "','" & SqlFmt(cTableName) & "')"
                    // Me.ExeProcessSql(cSQL)

                    // End If
                    // 'NB NEW STUFF ------------


                    // End If



                    else
                    {
                        return "";
                    }
                }


                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ImportObjects", ex, cProcessInfo));
                    return "";
                }
            }




            public int processInstanceExtras(long savedId, XmlElement oInstance, bool bResetLocations, bool bOrphan)
            {

                PerfMonLog("DBHelper", "processInstanceExtras", "");

                string cProcessInfo = "";
                int i = 0;
                try
                {
                    string cContentLocationTable = getTable(objectTypes.ContentLocation);
                    // lets get the object type from the table name.
                    if (oInstance is null)
                    {
                        return 0;
                    }
                    else
                    {

                        string cTableName = oInstance.FirstChild.Name;

                        // return the object type from the table name
                        var oTblName = default(TableNames);
                        foreach (TableNames currentOTblName in Enum.GetValues(typeof(objectTypes)))
                        {
                            oTblName = currentOTblName;
                            if ((oTblName.ToString() ?? "") == (cTableName ?? ""))
                                break;
                        }
                        var oObjType = new objectTypes();
                        oObjType = (objectTypes)oTblName;

                        // Type specific additional processes.
                        switch (oObjType)
                        {
                            case objectTypes.Content:
                                {
                                    // now lets sort out the locations
                                    // lets delete previous locations for that content
                                    if (bResetLocations)
                                    {
                                        RemoveContentLocations((int)savedId, cContentLocationTable);
                                    }


                                    // now lets add those specificed
                                    // Process locations
                                    if (!bOrphan)
                                    {
                                        XmlElement oPrmLoc = (XmlElement)oInstance.SelectSingleNode("Location[@primary='true']");
                                        if (oPrmLoc is null)
                                        {
                                            oPrmLoc = (XmlElement)oInstance.SelectSingleNode("Location");
                                        }
                                        foreach (XmlElement oLocation in oInstance.SelectNodes("Location"))
                                        {
                                            long sPrimary = 0L;
                                            long displayOrder = Conversions.ToInteger("0" + oLocation.GetAttribute("displayOrder"));
                                            if (ReferenceEquals(oLocation, oPrmLoc))
                                                sPrimary = 1L;
                                            if (!string.IsNullOrEmpty(oLocation.GetAttribute("foriegnRef")))
                                            {
                                                string cleanFref = oLocation.GetAttribute("foriegnRef");
                                                if (Conversions.ToBoolean(Strings.InStr(cleanFref, "&")))
                                                {
                                                    cleanFref = cleanFref.Replace("&amp;", "&");
                                                }
                                                bool updateLocation = true;
                                                if (sPrimary == 1L)
                                                {
                                                    // does the item have a primary location that does not match the fRef ?
                                                    // if so we want to remove the location associated with the fRef because the client has moved the product manually to a more appropreate page/
                                                    string sSQL = "select count(*)  FROM tblContentLocation cl inner join tblContentStructure cs on cl.nStructId = cs.nStructKey where bPrimary = 1 and nContentId = " + savedId + " and cStructForiegnRef != '" + SqlFmt(cleanFref) + "'";
                                                    if (Conversions.ToDouble(ExeProcessSqlScalar(sSQL)) > 0d)
                                                    {
                                                        // this item has an alternate primary location, then make sure we don't add it 
                                                        updateLocation = false;
                                                        string[] pageids = getObjectsByRef(objectTypes.ContentStructure, cleanFref);
                                                        var loopTo = pageids.Length - 1;
                                                        for (i = 0; i <= loopTo; i++)
                                                            // and delete the existing location for that fRef
                                                            RemoveContentLocation(Conversions.ToLong(pageids[i]), savedId);


                                                    }
                                                }
                                                if (updateLocation)
                                                {
                                                    setContentLocationByRef(cleanFref, (int)savedId, (int)sPrimary, 0, oLocation.GetAttribute("position"), displayOrder);
                                                }
                                            }

                                            else if (!string.IsNullOrEmpty(oLocation.GetAttribute("id")))
                                            {
                                                setContentLocation(Conversions.ToLong(oLocation.GetAttribute("id")), savedId, Conversions.ToBoolean(sPrimary), false, false, oLocation.GetAttribute("position"), true, displayOrder);
                                            }
                                        }
                                    }


                                    // lets look for content relationships
                                    XmlElement oRelation;
                                    foreach (XmlElement currentORelation in oInstance.SelectNodes("Relation"))
                                    {
                                        oRelation = currentORelation;
                                        // Relate this content to an item by either that item's parent ID or the foreign ref
                                        if (!string.IsNullOrEmpty(oRelation.GetAttribute("foriegnRef")))
                                        {
                                            setContentRelationByRef((int)savedId, oRelation.GetAttribute("foriegnRef"), true, oRelation.GetAttribute("type"), true);
                                        }
                                        else if (!string.IsNullOrEmpty(oRelation.GetAttribute("relatedContentId")) && Tools.Number.IsReallyNumeric(oRelation.GetAttribute("relatedContentId")) && Convert.ToInt32(oRelation.GetAttribute("relatedContentId")) > 0 && string.IsNullOrEmpty(oRelation.GetAttribute("direction")))


                                        {
                                            insertContentRelation((int)savedId, Convert.ToInt32(oRelation.GetAttribute("relatedContentId")).ToString(), true, oRelation.GetAttribute("type"), true);
                                        }

                                        else if (oRelation.GetAttribute("relatedContentId").Contains(",") | !string.IsNullOrEmpty(oRelation.GetAttribute("direction")))
                                        {
                                            // remove existing content relations of type
                                            RemoveContentRelationByType(savedId, oRelation.GetAttribute("type"), oRelation.GetAttribute("direction"));

                                            foreach (var relContId in oRelation.GetAttribute("relatedContentId").Split(','))
                                            {
                                                if (Information.IsNumeric(relContId))
                                                {
                                                    if (Strings.LCase(oRelation.GetAttribute("direction")) == "child")
                                                    {
                                                        insertContentRelation(Convert.ToInt32(relContId), savedId.ToString(), false, oRelation.GetAttribute("type"), true);
                                                    }
                                                    else
                                                    {
                                                        insertContentRelation((int)savedId, Convert.ToInt32(relContId).ToString(), true, oRelation.GetAttribute("type"), true);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    foreach (XmlElement currentORelation1 in oInstance.SelectNodes("ProductGroups"))
                                    {
                                        oRelation = currentORelation1;
                                        insertProductGroupRelation((int)savedId, oRelation.GetAttribute("ids"));
                                    }
                                    foreach (XmlElement oRelatedLibraryImages in oInstance.SelectNodes("RelatedLibraryImages"))
                                    {
                                        // createGalleryImages(savedId, oRelatedGalleryImages.InnerText,oRelatedGalleryImages.attribute("skipFirst"))
                                        // function to step through each image in the array, check it exists, get width and height, open LibraryImage Xform,
                                        // Get the instance, set /images/img[@class=display] with src and width and height
                                        // then use setObjectInstance, get the new id and related to the savedId setcontentrelation(newid,savedid).
                                        if (!string.IsNullOrEmpty(oRelatedLibraryImages.GetAttribute("skipFirst")) & !string.IsNullOrEmpty(oRelatedLibraryImages.GetAttribute("type")))
                                        {
                                            CreateLibraryImages((int)savedId, oRelatedLibraryImages.InnerText, oRelatedLibraryImages.GetAttribute("skipFirst"), oRelatedLibraryImages.GetAttribute("type"));
                                        }

                                    }

                                    break;
                                }
                            case objectTypes.Directory:
                                {
                                    foreach (XmlElement oRelation in oInstance.SelectNodes("Relation"))
                                    {
                                        long nloc;

                                        if (!string.IsNullOrEmpty(oRelation.GetAttribute("foriegnRef")))
                                        {
                                            nloc = getObjectByRef(objectTypes.Directory, oRelation.GetAttribute("foriegnRef"), oRelation.GetAttribute("type"));
                                        }
                                        else
                                        {
                                            nloc = Conversions.ToInteger("0" + oRelation.GetAttribute("relatedDirId"));
                                        }

                                        if (nloc > 0L)
                                        {
                                            bool bRemove = false;
                                            if (oRelation.GetAttribute("remove") == "true")
                                            {
                                                bRemove = true;
                                            }
                                            maintainDirectoryRelation(nloc, savedId, bRemove);
                                        }

                                    }

                                    break;
                                }

                            case objectTypes.ContentStructure:
                                {
                                    var oTblName2 = default(TableNames);
                                    foreach (XmlElement oContentInstance in oInstance.SelectNodes("Contents/instance"))
                                    {
                                        // lets get an id if we are updating a record with a foriegn Ref
                                        // return the object type from the table name
                                        string cTableName2 = oContentInstance.FirstChild.Name;
                                        foreach (TableNames currentOTblName2 in Enum.GetValues(typeof(objectTypes)))
                                        {
                                            oTblName2 = currentOTblName2;
                                            if ((oTblName2.ToString() ?? "") == (cTableName2 ?? ""))
                                                break;
                                        }
                                        var oObjType2 = new objectTypes();
                                        oObjType2 = (objectTypes)oTblName2;

                                        long nContentId = 0L;
                                        XmlElement fRefElmt = (XmlElement)oContentInstance.SelectSingleNode(getTable(oObjType2) + "/" + getFRef(oObjType2));
                                        string fRef;
                                        if (fRefElmt != null)
                                        {
                                            fRef = fRefElmt.InnerText;
                                            if (!string.IsNullOrEmpty(fRef))
                                            {
                                                nContentId = myWeb.moDbHelper.getObjectByRef(getTable(oObjType2), getKey((int)oObjType2), getFRef(oObjType2), oObjType2, fRef);
                                            }
                                        }

                                        nContentId = Conversions.ToLong(setObjectInstance(oObjType2, oContentInstance, nContentId));
                                        processInstanceExtras(nContentId, oContentInstance, bResetLocations, bOrphan);

                                    }

                                    break;
                                }

                        }

                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "processInstanceExtras", ex, cProcessInfo));
                    return 0;
                }

                return default;
            }

            public string CreateLibraryImages(int savedId, string cRelatedLibraryImage, string cSkipAttribute, string cRelatedImageType = "")
            {
                try
                {
                    // myWeb.moCtx.Request.
                    XmlElement oLibraryImageInstance;

                    if (!string.IsNullOrEmpty(cRelatedLibraryImage))
                    {
                        string[] oImagePath = cRelatedLibraryImage.Split(',');
                        // If skipfirst attribute
                        if (Conversions.ToBoolean(cSkipAttribute) == true)
                        {
                            oImagePath = oImagePath.Skip(1).ToArray();
                        }

                        foreach (string cImage in oImagePath)
                        {
                            Cms.xForm moAdXfm;
                            moAdXfm = new Cms.xForm(ref myWeb);
                            string cContentSchemaName = cRelatedImageType;
                            string cXformPath = cContentSchemaName;
                            string cContentName = "New " + cRelatedImageType;


                            cContentName = System.IO.Path.GetFileNameWithoutExtension(moCtx.Server.MapPath("/") + cImage);


                            cXformPath = "/xforms/content/" + cXformPath;
                            if (myWeb.bs5)
                            {
                                cXformPath = "/modules/galleryimagelist/" + cContentSchemaName;
                            }
                            moAdXfm = (Cms.xForm)myWeb.getXform();
                            moAdXfm.load(cXformPath + ".xml", myWeb.maCommonFolders);

                            if (!string.IsNullOrEmpty(cContentName) & moAdXfm.Instance.FirstChild != null)
                            {
                                moAdXfm.Instance.SelectSingleNode("tblContent/cContentName").InnerText = cContentName;
                                moAdXfm.Instance.SelectSingleNode("tblContent/cContentXmlBrief/Content/Title").InnerText = cContentName;
                                moAdXfm.Instance.SelectSingleNode("tblContent/cContentXmlDetail/Content/Title").InnerText = cContentName;
                                moAdXfm.Instance.SelectSingleNode("tblContent/dPublishDate").InnerText = XmlDate(DateTime.Now);
                            }

                            oLibraryImageInstance = moAdXfm.Instance;
                            XmlElement imgElement = (XmlElement)oLibraryImageInstance.SelectSingleNode("tblContent/cContentXmlBrief/Content/Images/img[@class='display']");
                            XmlElement imgElementDetail = (XmlElement)oLibraryImageInstance.SelectSingleNode("tblContent/cContentXmlDetail/Content/Images/img[@class='display']");
                            // Dim oImg As System.Drawing.Bitmap = New System.Drawing.Bitmap(goServer.MapPath("/") & cImage.Trim.Replace("/", "\"))
                            System.Drawing.Bitmap oImg;

                            if (!string.IsNullOrEmpty(myWeb.moCtx.Request.Form["cImageBasePath"]) && !string.IsNullOrEmpty(myWeb.moCtx.Request.Form["cImageBasePath"]))
                            {
                                oImg = new System.Drawing.Bitmap(myWeb.moCtx.Request.Form["cImageBasePath"] + cImage.Trim().Replace("/", @"\"));
                            }
                            else
                            {
                                oImg = new System.Drawing.Bitmap(goServer.MapPath("/") + cImage.Trim().Replace("/", @"\"));
                            }

                            imgElement.SetAttribute("src", cImage.Trim());
                            imgElement.SetAttribute("height", oImg.Height.ToString());
                            imgElement.SetAttribute("width", oImg.Width.ToString());
                            imgElementDetail.SetAttribute("src", cImage.Trim());
                            imgElementDetail.SetAttribute("height", oImg.Height.ToString());
                            imgElementDetail.SetAttribute("width", oImg.Width.ToString());
                            long nContentId;
                            nContentId = Conversions.ToLong(setObjectInstance(objectTypes.Content, oLibraryImageInstance));
                            // If we have an action here we need to relate the item
                            insertContentRelation(savedId, nContentId.ToString(), false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return " Relation Error " + ex.Message;
                }

                return default;


            }


            /// <summary>
            ///   <para>This deletes all content locations for an item of content.</para>
            /// </summary>
            /// <param name="nContentId">The content ID</param>
            /// <param name="cTable">Optional - table name for content locations - useful to pass thru to save extra DB calls</param>
            /// <returns>The number of items deleted.</returns>
            /// <remarks>Note - we changed this from using DeleteObject as it's not conducive for bulk deletes.</remarks>
            public int RemoveContentLocations(int nContentId, string cTable = "")
            {
                PerfMonLog("DBHelper", "RemoveContentLocations", "nContentId=" + nContentId);
                int i = 0;
                try
                {

                    // Normally the table name will be passed thru for optimisation, but just in case...
                    if (string.IsNullOrEmpty(cTable))
                        cTable = getTable(objectTypes.ContentLocation);

                    string cSQL = "DELETE FROM " + cTable + " WHERE nContentId = " + nContentId;
                    i = ExeProcessSql(cSQL);
                    return i;
                }
                catch (Exception)
                {
                    return i;
                }
            }

            // Public Function RemoveContentLocations(ByVal nContentId As Integer) As Integer
            // PerfMonLog("DBHelper", "RemoveContentLocations", "nContentId=" & nContentId)
            // Dim i As Integer = 0
            // Try

            // Dim cSQL As String = "SELECT tblContentLocation.nContentLocationKey FROM tblContentLocation INNER JOIN tblContentStructure ON tblContentLocation.nStructId = tblContentStructure.nStructKey"
            // cSQL &= " WHERE nContentId = " & nContentId
            // Dim oDR As SqlClient.SqlDataReader = myWeb.moDbHelper.getDataReader(cSQL)
            // Do While oDR.Read
            // myWeb.moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.ContentLocation, oDR(0))
            // i += 1
            // Loop
            // oDR.Close()
            // Return i
            // Catch ex As Exception
            // Return i
            // End Try
            // End Function

            public int setContentLocationByRef(string cStructFRef, int nContentId, int bPrimary, int bCascade)
            {

                PerfMonLog("DBHelper", "setContentLocationByRef", "ref=" + cStructFRef + " nContentId=" + nContentId);
                string cProcessInfo = "";
                // Dim oDr As SqlDataReader
                try
                {
                    string nID = ""; // myWeb.moDbHelper.getKeyByNameAndSchema(Cms.dbHelper.objectTypes.ContentStructure, "", cStructName)

                    // oDr = getDataReader("select nStructKey from tblContentStructure where cStructForiegnRef like '" & SqlFmt(cStructFRef) & "'")
                    using (var oDr = getDataReaderDisposable("select nStructKey from tblContentStructure where cStructForiegnRef like '" + SqlFmt(cStructFRef) + "'"))  // Done by nita on 6/7/22
                    {
                        int lastloc = 0;

                        while (oDr.Read())
                        {
                            nID = Conversions.ToString(oDr["nStructKey"]);
                            if (string.IsNullOrEmpty(nID))
                                nID = 0.ToString();
                            lastloc = setContentLocation(Conversions.ToLong(nID), nContentId, Conversions.ToBoolean(Interaction.IIf(bPrimary == 1, true, false)), Conversions.ToBoolean(bCascade), false);
                        }
                        return lastloc;
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "setContentLocationByRef", ex, cProcessInfo));
                    return 0;
                }
                finally
                {
                    // If Not oDr Is Nothing Then
                    // oDr.Close()
                    // oDr = Nothing
                    // End If
                }
            }


            public int setContentLocationByRef(string cStructFRef, int nContentId, int bPrimary, int bCascade, string cPosition, long nDisplayOrder = 0L)
            {

                PerfMonLog("DBHelper", "setContentLocationByRef", "ref=" + cStructFRef + " nContentId=" + nContentId);
                string cProcessInfo = "";
                // Dim oDr As SqlDataReader
                try
                {
                    string nID = ""; // myWeb.moDbHelper.getKeyByNameAndSchema(Cms.dbHelper.objectTypes.ContentStructure, "", cStructName)
                                     // nID = getObjectByRef(Cms.dbHelper.objectTypes.ContentStructure, cStructFRef)
                                     // A site may have multiple pasges with the same Fref
                                     // oDr = getDataReader("select nStructKey from tblContentStructure where cStructForiegnRef like '" & SqlFmt(cStructFRef) & "'")
                    using (var oDr = getDataReaderDisposable("select nStructKey from tblContentStructure where cStructForiegnRef like '" + SqlFmt(cStructFRef) + "'"))  // Done by nita on 6/7/22
                    {
                        int lastloc = 0;
                        if (oDr != null)
                        {
                            while (oDr.Read())
                            {
                                nID = Conversions.ToString(oDr["nStructKey"]);
                                if (string.IsNullOrEmpty(nID))
                                    nID = 0.ToString();
                                lastloc = setContentLocation(Conversions.ToLong(nID), nContentId, Conversions.ToBoolean(Interaction.IIf(bPrimary == 1, true, false)), Conversions.ToBoolean(bCascade), false, cPosition, false, nDisplayOrder);
                            }
                        }
                        return lastloc;
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "setContentLocationByRef", ex, cProcessInfo));
                    return 0;
                }
                finally
                {
                    // If Not oDr Is Nothing Then
                    // oDr.Close()
                    // oDr = Nothing
                    // End If
                }
            }


            public string setContentRelationByRef(int nContentId, string cContentFRef, bool b2Way = false, string rType = "", bool bHaltRecursion = false)
            {
                try
                {

                    int nRefId = Conversions.ToInteger(GetDataValue("SELECT nContentKey, cContentForiegnRef FROM tblContent WHERE (cContentForiegnRef = '" + SqlFmt(cContentFRef) + "')"));

                    if (!(nRefId == 0))
                    {
                        return insertContentRelation(nContentId, nRefId.ToString(), b2Way, rType, bHaltRecursion);
                    }
                    else
                    {
                        return "Not Found";
                    }
                }
                catch (Exception ex)
                {
                    return " Relation Error " + ex.Message;
                }


            }



            public string importShippingLocations(ref XmlDocument oXml)
            {
                PerfMonLog("DBHelper", "importShippingLocations");
                string cSql;
                XmlElement oShipLoc;
                XmlElement oShipLocClone;
                XmlElement oInstance;
                XmlElement oElmt;
                string cKey;
                string cParKey;
                string cProcessInfo = "";
                try
                {

                    // Clear out the current shipping locations and relations
                    cSql = "DELETE tblAudit FROM tblAudit a INNER JOIN tblCartShippingLocations c ON a.nAuditKey = c.nAuditId;" + "DELETE FROM tblCartShippingLocations;" + "DELETE tblAudit FROM tblAudit a INNER JOIN tblCartShippingRelations c ON a.nAuditKey = c.nAuditId;" + "DELETE FROM tblCartShippingRelations;" + "INSERT INTO tblLookup (cLkpKey,cLkpValue,cLkpCategory) VALUES ('ShippingLocations',1,'ImportLock');";



                    ExeProcessSql(cSql);

                    // Run through each record
                    foreach (XmlElement currentOShipLoc in oXml.SelectNodes("//tblCartShippingLocations"))
                    {
                        oShipLoc = currentOShipLoc;
                        oInstance = oXml.CreateElement("instance");
                        oShipLocClone = (XmlElement)oShipLoc.CloneNode(true);

                        // Remove the Unique ID
                        oElmt = (XmlElement)oShipLocClone.SelectSingleNode("nLocationKey");
                        if (oElmt != null)
                            oShipLocClone.RemoveChild(oElmt);

                        // Remove the Audit Id
                        oElmt = (XmlElement)oShipLocClone.SelectSingleNode("nAuditId");
                        if (oElmt != null)
                            oShipLocClone.RemoveChild(oElmt);

                        oInstance.AppendChild(oShipLocClone);

                        // Save the instance
                        cKey = setObjectInstance(objectTypes.CartShippingLocation, oInstance);
                        XmlNode argoNode = oShipLoc;
                        addNewTextNode("newKey", ref argoNode, cKey);
                        oShipLoc = (XmlElement)argoNode;
                    }

                    // Now go through and reconcile the old parent keys
                    foreach (XmlElement currentOShipLoc1 in oXml.SelectNodes("//tblCartShippingLocations"))
                    {
                        oShipLoc = currentOShipLoc1;
                        // Get the parent key
                        cKey = oShipLoc.SelectSingleNode("newKey").InnerText;
                        cParKey = oShipLoc.SelectSingleNode("nLocationParId").InnerText;

                        // Get the new parentkey
                        oElmt = (XmlElement)oXml.SelectSingleNode("//tblCartShippingLocations[nLocationKey=" + cParKey + "]");
                        if (oElmt != null)
                        {
                            oElmt = (XmlElement)oElmt.SelectSingleNode("newKey");
                            if (oElmt != null)
                            {
                                cSql = "UPDATE tblCartShippingLocations SET nLocationParId=" + oElmt.InnerText + " WHERE nLocationKey=" + cKey;
                                ExeProcessSql(cSql);
                            }
                        }
                    }

                    return "";
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "importShippingLocations", ex, cProcessInfo));
                    return "";
                }
            }

            public string importShippingLocations2(ref XmlDocument oXml)
            {
                PerfMonLog("DBHelper", "importShippingLocations");
                string cSql;

                string cProcessInfo = "";
                try
                {

                    // Clear out the current shipping locations and relations
                    cSql = "DELETE tblAudit FROM tblAudit a INNER JOIN tblCartShippingLocations c ON a.nAuditKey = c.nAuditId;" + "DELETE FROM tblCartShippingLocations;" + "DELETE tblAudit FROM tblAudit a INNER JOIN tblCartShippingRelations c ON a.nAuditKey = c.nAuditId;" + "DELETE FROM tblCartShippingRelations;" + "INSERT INTO tblLookup (cLkpKey,cLkpValue,cLkpCategory) VALUES ('ShippingLocations',1,'ImportLock');";



                    ExeProcessSql(cSql);
                    int nCount = importShippingLocationDrillDown((XmlElement)oXml.DocumentElement.FirstChild, 0);

                    return nCount.ToString();
                }


                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "importShippingLocations2", ex, cProcessInfo));
                    return "";
                }
            }

            private int importShippingLocationDrillDown(XmlElement oElmt, int nParId)
            {
                var nCount = default(int);
                try
                {
                    XmlElement oCopy = (XmlElement)oElmt.CloneNode(true);
                    XmlElement oChild;
                    foreach (XmlElement currentOChild in oCopy.SelectNodes("instance"))
                    {
                        oChild = currentOChild;
                        oChild.ParentNode.RemoveChild(oChild);
                    }
                    oCopy.SelectSingleNode("tblCartShippingLocations/cLocationNameFull").InnerText = Strings.Replace(oCopy.SelectSingleNode("tblCartShippingLocations/cLocationNameFull").InnerText, "&amp;", "and");
                    oCopy.SelectSingleNode("tblCartShippingLocations/cLocationNameShort").InnerText = Strings.Replace(oCopy.SelectSingleNode("tblCartShippingLocations/cLocationNameShort").InnerText, "&amp;", "and");
                    oCopy.SelectSingleNode("tblCartShippingLocations/nLocationParId").InnerText = nParId.ToString();
                    int nId = Conversions.ToInteger(setObjectInstance(objectTypes.CartShippingLocation, oCopy));

                    if (nId > 0)
                    {
                        nCount += 1;

                        foreach (XmlElement currentOChild1 in oElmt.SelectNodes("instance"))
                        {
                            oChild = currentOChild1;

                            nCount += importShippingLocationDrillDown(oChild, nId);
                        }
                    }

                    return nCount;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "importShippingLocationDrillDown", ex, ""));
                    return nCount;
                }
            }

            public XmlElement GetContentDetailXml(long nArtId = 0L)
            {
                return GetContentDetailXml(nArtId, false);
            }

            public XmlElement GetContentDetailXml(long nArtId = 0L, Boolean noFilter = false)
            {
                PerfMonLog("Web", "GetContentDetailXml");
                XmlElement oRoot;
                XmlElement oElmt = null;
                //XmlElement retElmt = null;
                string sContent;
                string sSql;
                string sProcessInfo = "GetContentDetailXml";
                var oDs = new DataSet();
                string sFilterSql = "";
                bool bLoadAsXml;
                XmlComment oComment;

                try
                {

                    // If requested, we need to make sure that the content we are looking for doesn't belong to a page
                    // that the user is not allowed to access.
                    // We can review the current menu structure xml instead of calling the super slow permissions functions.

                    if (nArtId > 0L)
                    {
                        sProcessInfo = "loading content" + nArtId;
                        if (noFilter == false)
                        {
                            sFilterSql += GetStandardFilterSQLForContent();
                        }
                        oRoot = moPageXml.CreateElement("ContentDetail");
                        sSql = "select c.nContentKey as id, cContentForiegnRef as ref, dbo.fxn_getContentParents(c.nContentKey) as parId, cContentName as name, cContentSchemaName as type, cContentXmlDetail as content, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, a.nStatus as status from tblContent c ";
                        sSql += "inner join tblAudit a on c.nAuditId = a.nAuditKey  ";
                        // sSql &= "inner join tblContentLocation CL on c.nContentKey = CL.nContentId "
                        sSql += "where c.nContentKey = " + nArtId + sFilterSql + " ";
                        // sSql &= "and CL.nStructId = " & mnPageId

                        oDs = GetDataSet(sSql, "Content", "ContentDetail");

                        oDs.Tables[0].Columns["id"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["ref"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["name"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["type"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["publish"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["expire"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["update"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["parId"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["owner"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["status"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["content"].ColumnMapping = MappingType.SimpleContent;

                        // Need to check the content is found on the current page.


                        if (oDs.Tables[0].Rows.Count > 0)
                        {

                            oRoot.InnerXml = Strings.Replace(oDs.GetXml(), "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "");
                            foreach (XmlNode oNode in oRoot.SelectNodes("/ContentDetail/Content"))
                            {
                                oElmt = (XmlElement)oNode;
                                sContent = oElmt.InnerText;

                                // Try to convert the InnerText to InnerXml
                                // Also if the innerxml has Content as a first node, then get the innerxml of the content node.
                                try
                                {
                                    oElmt.InnerXml = sContent;
                                    bLoadAsXml = true;
                                }

                                catch (Exception)
                                {
                                    // If the load failed, then flag it in the Content node and return the InnerText as a Comment
                                    oComment = oRoot.OwnerDocument.CreateComment(oElmt.InnerText);
                                    oElmt.SetAttribute("xmlerror", "getContentBriefXml");
                                    oElmt.InnerXml = "";
                                    oElmt.AppendChild(oComment);
                                    oComment = null;
                                    bLoadAsXml = false;
                                }

                                if (bLoadAsXml)
                                {

                                    // Successfully converted to XML.
                                    // Now check if the node imported is a Content node - if so get rid of the Content node
                                    var oFirst = firstElement(ref oElmt);
                                    // NB 19-02-2010 Added to stop unsupported types falling over
                                    if (oFirst != null)
                                    {
                                        if (oFirst.LocalName == "Content")
                                        {
                                            foreach (XmlAttribute oAttr in oElmt.SelectNodes("Content/@*"))
                                            {
                                                if (string.IsNullOrEmpty(oElmt.GetAttribute(oAttr.Name)))
                                                {
                                                    oElmt.SetAttribute(oAttr.Name, oAttr.InnerText);
                                                }
                                            }

                                            oElmt.InnerXml = oFirst.InnerXml;
                                        }
                                    }

                                    // addRelatedContent(oNode, nArtId, False)

                                }

                            }

                            return oElmt;
                        }

                        else
                        {
                            // Just a page no detail requested
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }

                catch (Exception ex)
                {
                    // returnException(myWeb.msException, mcModuleName, "getContentDetailXml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetContentDetailXml", ex, sProcessInfo));
                    return null;
                }

            }

            /// <summary>
            /// This attempts to construct the standard SQL filter for getting LIVE content
            /// If version control is on it will also assess the page permissions, 
            /// and if appropriate, it will get content that is not LIVe but, say, PENDING
            /// </summary>
            /// <returns></returns>
            /// <remarks></remarks>
            public string GetStandardFilterSQLForContent(bool bPrecedingAND = true, bool bAdminMode = false, PermissionLevel PagePerm = PermissionLevel.Open)
            {

                PerfMonLog("Web", "GetStandardFilterSQLForContent");

                string sFilterSQL = "";

                try
                {

                    // Only check for permissions if not in Admin Mode
                    if (!bAdminMode)
                    {

                        // Set the default filter
                        sFilterSQL = "a.nStatus = 1 ";

                        if (gbVersionControl && mnUserId > 0L)
                        {
                            // Version control is on
                            // Check the page permission
                            if (CanAddUpdate(PagePerm))
                            {

                                // User has update permissions - now can they only have control over their own items
                                if (CanOnlyUseOwn(PagePerm))
                                {

                                    // Return everything with a status of live and anything that was created by
                                    // the user and has a status that isn't hidden
                                    sFilterSQL = "(a.nStatus = 1 OR (a.nStatus >= 1 AND a.nInsertDirId=" + mnUserId.ToString() + ")) ";
                                }

                                else
                                {

                                    // Return anything with a status that isn't hidden
                                    sFilterSQL = "a.nStatus >= 1 ";

                                }

                            }
                        }

                        if (bPrecedingAND)
                            sFilterSQL = " AND " + sFilterSQL;
                        sFilterSQL += " and (a.dPublishDate is null or a.dPublishDate = 0 or a.dPublishDate <= " + SqlDate(DateTime.Now) + " )";
                        sFilterSQL += " and (a.dExpireDate is null or a.dExpireDate = 0 or a.dExpireDate >= " + SqlDate(DateTime.Now) + " )";
                    }

                    return sFilterSQL;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetStandardFilterSQLForContent", ex, sFilterSQL));
                    return "";
                }
            }

            public void AddDataSetToContent(ref DataSet oDs, ref XmlElement oContent, ref DateTime? dExpireDate, ref DateTime? dUpdateDate, long nCurrentPageId = 0L, bool bIgnoreDuplicates = false, string cAddSourceAttribute = "", bool bAllowRecursion = true)
            {
                try
                {

                    // Calculate the maxdepth - it can be overrided by ShowRelatedBriefDepth
                    string cShowRelatedBriefDepth = goConfig["ShowRelatedBriefDepth"] + "";
                    int nMaxDepth = 1;
                    if (!string.IsNullOrEmpty(cShowRelatedBriefDepth) && Information.IsNumeric(cShowRelatedBriefDepth))
                    {
                        nMaxDepth = Conversions.ToInteger(cShowRelatedBriefDepth);
                    }
                    DateTime ExpireDate = dExpireDate ?? DateTime.Now;
                    DateTime UpdateDate = dUpdateDate ?? DateTime.Now;
                    AddDataSetToContent(ref oDs, ref oContent, nCurrentPageId, bIgnoreDuplicates, cAddSourceAttribute, ref ExpireDate, ref UpdateDate, bAllowRecursion, nMaxDepth);
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AddDataSetToContent", ex, ""));
                }
            }


            public void AddDataSetToContent(ref DataSet oDs, ref XmlElement oContent, long nCurrentPageId, bool bIgnoreDuplicates, string cAddSourceAttribute, ref DateTime dExpireDate, ref DateTime dUpdateDate, bool bAllowRecursion, int nMaxDepth, string cShowSpecificContentTypes = "")
            {
                PerfMonLog("DBHelper", "AddDataSetToContent - Start");
                string sProcessInfo = "";
                XmlElement oElmt2;
                XmlElement oElmt3;

                string sNodeName = "";
                string sContentText = string.Empty;

                try
                {

                    string[] aNodeTypes;
                    string cShowRelatedBriefContentTypes = goConfig["ShowRelatedBriefContentTypes"] + "";
                    var oXml = ContentDataSetToXml(ref oDs, ref dUpdateDate);
                    var oXml2 = oContent.OwnerDocument.ImportNode(oXml.DocumentElement, true);

                    int contentCount = 0;
                    var n = default(long);
                    long itemsSkipped = 0L;
                    foreach (XmlNode oNode in oXml2.SelectNodes("Content"))
                    {
                        n = n + 1L;
                        XmlElement argoContent = (XmlElement)oNode;
                        oElmt2 = SimpleTidyContentNode(ref argoContent, ref dExpireDate, ref dUpdateDate, cAddSourceAttribute);
                        //oNode = argoContent;

                        sNodeName = oElmt2.GetAttribute("name");
                        int nNodeId = Conversions.ToInteger(oElmt2.GetAttribute("id"));
                        string cNodeType = oElmt2.GetAttribute("type");
                        string cRelationType = oElmt2.GetAttribute("rtype");
                        string cPosition = oElmt2.GetAttribute("position");
                        string cRelationQuery = "";
                        if (!string.IsNullOrEmpty(cRelationType))
                        {
                            cRelationQuery = " and @rType='" + cRelationType + "'";
                        }
                        // ensure this items is not allready in this location....
                        if (oContent.SelectSingleNode("*[@id='" + nNodeId + "'" + cRelationQuery + "]") is null)
                        {

                            // Allow related content to be added to the specified types always (Module is default)
                            if (oElmt2.HasAttribute("showRelated"))
                            {
                                cShowSpecificContentTypes += "," + oElmt2.GetAttribute("showRelated");
                            }

                            aNodeTypes = Strings.Split(Conversions.ToString(Interaction.IIf(string.IsNullOrEmpty(cShowRelatedBriefContentTypes), "module", cShowRelatedBriefContentTypes.ToString().ToLower())), ",");

                            if (bAllowRecursion & !(myWeb.ibIndexMode & !myWeb.ibIndexRelatedContent) & (cNodeType == "Module" || Array.IndexOf(aNodeTypes, cNodeType.ToLower()) >= aNodeTypes.GetLowerBound(0) || oContent.SelectNodes("ancestor::ContentDetail").Count != 0) || !string.IsNullOrEmpty(cShowSpecificContentTypes))



                            {

                                // Related Content in Brief
                                // Avoiding recursion.
                                // We can take two approaches:
                                // 1.  Maintain a list of IDs currently visited down this branch of the node tree. (Prevents repetition)
                                // 2.  Implement depth limiter (Prevents branching).
                                // 
                                // Depth limitation is acheived by reading the number of items in the List

                                string cAvoidRecursionList = oContent.GetAttribute("avoidRecursionList");
                                int nDepth = Conversions.ToInteger(Interaction.IIf(string.IsNullOrEmpty(cAvoidRecursionList), 0, cAvoidRecursionList.Split(',').GetLength(0)));

                                if (nDepth < nMaxDepth)
                                {
                                    if (nDepth > 0)
                                    {
                                        // We need to pass on a curent list of content ids down the related content tree to avoid recursion
                                        // Because this stage happens before oElmt2 is added to the content we can't simply look at what has been added
                                        // So we use this temporary attribute and remove it later
                                        oElmt2.SetAttribute("avoidRecursionList", cAvoidRecursionList);
                                    }
                                    // Trevors Bulk Content Relations Experiment
                                    oElmt2.SetAttribute("processForRelatedContent", "true");
                                    // addRelatedContent(oElmt2, nNodeId, myWeb.mbAdminMode, cShowSpecificContentTypes)
                                }
                                oElmt2.RemoveAttribute("avoidRecursionList");
                            }

                            // Content[@name='" & sNodeName & "']"
                            // If Not (oContent.SelectSingleNode("Content[@name='" & sNodeName & "' and @type='" & cNodeType & "' and @id='" & nNodeId & "']") Is Nothing) And Not bIgnoreDuplicates Then
                            if (oContent.SelectSingleNode("Content[@name=" + GenerateConcatForXPath(sNodeName) + " and @position='" + cPosition + "']") != null & !bIgnoreDuplicates & !string.IsNullOrEmpty(sNodeName))
                            {
                                // replace node.
                                oElmt3 = (XmlElement)oContent.SelectSingleNode("Content[@name=" + GenerateConcatForXPath(sNodeName) + " and @position='" + cPosition + "']");
                                // we will only replace more "sytem" items
                                // If _
                                // LCase(oElmt3.GetAttribute("type")) = "plaintext" Or _
                                // LCase(oElmt3.GetAttribute("type")) = "formattedtext" Or _
                                // LCase(oElmt3.GetAttribute("type")) = "image" Or _
                                // LCase(oElmt3.GetAttribute("type")) = "flashmovie" Then

                                if (Conversions.ToLong("0" + oElmt3.GetAttribute("parId")) > 0L)
                                {
                                    sProcessInfo = oElmt2.OuterXml;
                                    long primaryParId;

                                    // fix for comma separated parId's
                                    if (Strings.InStr(oElmt2.GetAttribute("parId"), ",") > 0)
                                    {
                                        primaryParId = Conversions.ToLong(Strings.Left(oElmt2.GetAttribute("parId"), Strings.InStr(oElmt2.GetAttribute("parId"), ",") - 1));
                                    }
                                    else
                                    {
                                        primaryParId = Conversions.ToLong("0" + oElmt2.GetAttribute("parId"));
                                    }

                                    long primaryParId3;
                                    if (Strings.InStr(oElmt3.GetAttribute("parId"), ",") > 0)
                                    {
                                        primaryParId3 = Conversions.ToLong(Strings.Left(oElmt3.GetAttribute("parId"), Strings.InStr(oElmt3.GetAttribute("parId"), ",") - 1));
                                    }
                                    else
                                    {
                                        primaryParId3 = Conversions.ToLong("0" + oElmt3.GetAttribute("parId"));
                                    }

                                    if (CheckIfAncestorPage(oElmt3.OwnerDocument, primaryParId, primaryParId3))
                                    {
                                        oContent.ReplaceChild(oElmt2, oElmt3);
                                    }
                                    else
                                    {
                                        oContent.AppendChild(oElmt2);
                                        contentCount = contentCount + 1;
                                    }
                                }
                                else
                                {
                                    oContent.AppendChild(oElmt2);
                                    contentCount = contentCount + 1;
                                }
                            }
                            // End If
                            else
                            {
                                oContent.AppendChild(oElmt2);
                                contentCount = contentCount + 1;
                            }
                        }
                        else
                        {
                            itemsSkipped = itemsSkipped + 1L;
                        }

                    }

                    PerfMonLog("DBHelper", "AddDataSetToContent " + n + " items of content - End");

                    // Trevors Bulk Content Relations Experiment
                    if (Strings.LCase(goConfig["FinalAddBulk"]) != "on")
                    {
                        addBulkRelatedContent(ref oContent, ref dUpdateDate, nMaxDepth);
                    }
                }

                catch (Exception ex)
                {

                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AddDataSetToContent", ex, sProcessInfo));

                }
            }

            public XmlDocument ContentDataSetToXml(ref DataSet oDs, ref DateTime dUpdateDate)
            {
                PerfMonLog("DBHelper", "ContentDataSetToXml - Start");
                string sProcessInfo = "";

                string sNodeName = string.Empty;
                string sContentText = string.Empty;

                try
                {
                    // If nCurrentPageId = 0 Then nCurrentPageId = gnPageId

                    // map the feilds to columns
                    if (oDs != null)
                    {
                        if (oDs.Tables[0].Columns.Count >= 13)
                        {
                            // This is added to remove extra column for price and location filter
                            if (oDs.Tables[0].Columns.Count == 15)
                            {
                                oDs.Tables[0].Columns.RemoveAt(14);

                            }

                            if (oDs.Tables[0].Columns.Count == 14)
                            {
                                oDs.Tables[0].Columns.RemoveAt(13);

                            }

                            oDs.Tables[0].Columns.RemoveAt(12);

                        }

                        oDs.Tables[0].Columns["id"].ColumnMapping = MappingType.Attribute;

                        if (oDs.Tables[0].Columns.Contains("parID"))
                        {
                            oDs.Tables[0].Columns["parId"].ColumnMapping = MappingType.Attribute;
                        }

                        if (oDs.Tables[0].Columns.Contains("locId"))
                        {
                            oDs.Tables[0].Columns["locId"].ColumnMapping = MappingType.Attribute;
                        }

                        // Added to handle the new relationship type
                        if (oDs.Tables[0].Columns.Contains("rtype"))
                        {
                            oDs.Tables[0].Columns["rtype"].ColumnMapping = MappingType.Attribute;
                        }

                        {
                            var withBlock = oDs.Tables[0];
                            withBlock.Columns["ref"].ColumnMapping = MappingType.Attribute;
                            withBlock.Columns["name"].ColumnMapping = MappingType.Attribute;
                            withBlock.Columns["type"].ColumnMapping = MappingType.Attribute;
                            withBlock.Columns["status"].ColumnMapping = MappingType.Attribute;
                            withBlock.Columns["publish"].ColumnMapping = MappingType.Attribute;
                            withBlock.Columns["expire"].ColumnMapping = MappingType.Attribute;
                            withBlock.Columns["owner"].ColumnMapping = MappingType.Attribute;

                            if (oDs.Tables[0].Columns["position"] != null)
                            {
                                oDs.Tables[0].Columns["position"].ColumnMapping = MappingType.Attribute;
                            }
                            if (oDs.Tables[0].Columns["overall_count"] != null)
                            {
                                oDs.Tables[0].Columns["overall_count"].ColumnMapping = MappingType.Attribute;
                            }
                            if (oDs.Tables[0].Columns["update"] != null)
                            {
                                if (dUpdateDate != default)
                                {
                                    withBlock.Columns["update"].ColumnMapping = MappingType.Attribute;
                                }
                            }
                            withBlock.Columns["content"].ColumnMapping = MappingType.SimpleContent;
                        }

                        oDs.EnforceConstraints = false;
                        // convert to Xml Dom
                        var oXml = new XmlDocument();
                        oXml.LoadXml(oDs.GetXml());
                        oXml.PreserveWhitespace = false;

                        PerfMonLog("DBHelper", "ContentDataSetToXml - End");

                        return oXml;
                    }
                    else
                    {
                        return null;
                    }
                }

                catch (Exception ex)
                {

                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ContentDataSetToXml", ex, sProcessInfo));
                    return null;

                }
            }

            protected internal XmlElement SimpleTidyContentNode(ref XmlElement oContent, ref DateTime dExpireDate, ref DateTime dUpdateDate, string cAddSourceAttribute = "")
            {
                string sProcessInfo = "";
                XmlElement oElmt;
                string sNodeName = "";
                string sContentText = "";

                try
                {
                    // myWeb.PerfMon.Log("DBHelper", "SimpleTidyContentNode")

                    // oElmt = oContent.OwnerDocument.ImportNode(oContent, True)
                    oElmt = oContent;
                    sNodeName = oElmt.GetAttribute("name");
                    // myWeb.PerfMon.Log("DBHelper", "SimpleTidyContentNode - Import")

                    // make sure the page expiredate is fed back on the contents expiry
                    if (dExpireDate != default & !string.IsNullOrEmpty(oElmt.GetAttribute("expire")))
                    {
                        if (dExpireDate > Conversions.ToDate(oElmt.GetAttribute("expire")))
                        {
                            dExpireDate = Conversions.ToDate(oElmt.GetAttribute("expire"));
                        }
                    }

                    if (dUpdateDate != default & !string.IsNullOrEmpty(oElmt.GetAttribute("update")))
                    {
                        if (dUpdateDate < Conversions.ToDate(oElmt.GetAttribute("update")))
                        {
                            dUpdateDate = Conversions.ToDate(oElmt.GetAttribute("update"));
                        }
                    }

                    if (!string.IsNullOrEmpty(cAddSourceAttribute))
                        oElmt.SetAttribute("source", cAddSourceAttribute);

                    // change the xhtml string to xml
                    // myWeb.PerfMon.Log("DBHelper", "SimpleTidyContentNode - ConvertText")
                    sContentText = oElmt.InnerText;
                    try
                    {
                        oElmt.InnerXml = sContentText;
                    }
                    catch (Exception ex)
                    {
                        sProcessInfo = ex.Message;
                        // try removing the declaration
                        try
                        {
                            oElmt.InnerXml = Strings.Replace(sContentText, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>", "");
                        }
                        catch
                        {
                            oElmt.InnerText = sContentText;
                        }

                    }
                    // myWeb.PerfMon.Log("DBHelper", "SimpleTidyContentNode - EndConvertText")
                    // Draw the content node back to the main node.
                    XmlElement oContentElmt = (XmlElement)oElmt.SelectSingleNode("Content");
                    if (oContentElmt != null)
                    {
                        if (!string.IsNullOrEmpty(oContentElmt.GetAttribute("subType")))
                        {
                            oElmt.SetAttribute("subType", oContentElmt.GetAttribute("subType"));
                        }
                        // set any attributes on the content node
                        foreach (XmlAttribute oAttr in oElmt.SelectNodes("Content/@*"))
                        {
                            if (string.IsNullOrEmpty(oElmt.GetAttribute(oAttr.Name)))
                            {
                                oElmt.SetAttribute(oAttr.Name, oAttr.InnerText);
                            }
                        }
                        oElmt.InnerXml = oContentElmt.InnerXml;
                    }
                    // myWeb.PerfMon.Log("DBHelper", "SimpleTidyContentNode - Done")
                    return oElmt;
                }

                catch (Exception ex)
                {

                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "SimpleTidyContentNode", ex, sProcessInfo));
                    return null;

                }

            }

            public void addRelatedContent(ref XmlElement oContentElmt, int nParentId, bool bAdminMode, string specificTypes = "")
            {
                // TS - This is no longer used replaced by addBulkRelatedContent

                PerfMonLog("DBHelper", "addRelatedContent - Start");
                // adds related content to passed xmlNode.
                // Page/ContentDetail/Content
                if (oContentElmt is null | myWeb.ibIndexMode & !myWeb.ibIndexRelatedContent)
                    return;

                string sSql;
                string sFilterSql = "";
                //string sSql2 = "";
                //string sNodeName = "";
                //string sContent = "";
                // Dim rTypePresent As Boolean = False
                string sSepcificTypeSQL = "";
                // 'specificTypes = "SKU"

                string sProcessInfo = "building the related Content XML";

                try
                {

                    // Recursive loop avoidance
                    // Maintain a list of content ids in related content to stop it recursing for two way relationships when sending back to AddContentToDataset and vice versa.
                    // Start by adding the current id
                    string cRelatedContentList = oContentElmt.GetAttribute("avoidRecursionList");

                    // Add the id if it is not null or zero
                    if (!string.IsNullOrEmpty(oContentElmt.GetAttribute("id")) && Information.IsNumeric(oContentElmt.GetAttribute("id")) && Conversions.ToLong(oContentElmt.GetAttribute("id")) > 0L)

                    {
                        if (!string.IsNullOrEmpty(cRelatedContentList))
                            cRelatedContentList += ",";
                        cRelatedContentList += oContentElmt.GetAttribute("id");
                    }

                    // If there is a list, add a filter.
                    if (!string.IsNullOrEmpty(cRelatedContentList))
                    {
                        oContentElmt.SetAttribute("avoidRecursionList", cRelatedContentList);
                        sFilterSql += " and NOT(c.nContentKey IN (" + cRelatedContentList + "))";
                    }

                    if (!string.IsNullOrEmpty(specificTypes))
                    {
                        string[] aSpecificTypes = Strings.Split(specificTypes, ",");
                        int i;
                        var loopTo = Information.UBound(aSpecificTypes);
                        for (i = 0; i <= loopTo; i++)
                        {
                            if (i > 0)
                                sSepcificTypeSQL += ",";
                            sSepcificTypeSQL = "'" + aSpecificTypes[i] + "'";
                        }
                        sFilterSql += " and c.cContentSchemaName IN (" + sSepcificTypeSQL + ")";
                    }

                    // Build the filter SQL
                    sFilterSql += myWeb.GetStandardFilterSQLForContent();


                    // sSql = "select c.nContentKey as id, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, x.nDisplayOrder as displayorder, (select TOP 1 CL2.nStructId from tblContentLocation CL2 where CL2.nContentId=c.nContentKey and CL2.bPrimary = 1) as parId " & _

                    if (checkTableColumnExists("tblContentRelation", "cRelationType"))
                    {
                        sSql = "select c.nContentKey as id, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cRelationType as rtype, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.nInsertDirId as owner, x.nDisplayOrder as displayorder, dbo.fxn_getContentParents(c.nContentKey) as parId " + " FROM tblContent c INNER JOIN" + " tblAudit a ON c.nAuditId = a.nAuditKey INNER JOIN" + " tblContentRelation x ON c.nContentKey = x.nContentChildId" + " WHERE (x.nContentParentId = " + nParentId + ")";
                    }
                    else
                    {
                        sSql = "select c.nContentKey as id, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.nInsertDirId as owner, x.nDisplayOrder as displayorder, dbo.fxn_getContentParents(c.nContentKey) as parId " + " FROM tblContent c INNER JOIN" + " tblAudit a ON c.nAuditId = a.nAuditKey INNER JOIN" + " tblContentRelation x ON c.nContentKey = x.nContentChildId" + " WHERE (x.nContentParentId = " + nParentId + ")";
                    }


                    sSql = sSql + sFilterSql + " order by type, x.nDisplayOrder";

                    var oDs = new DataSet();
                    oDs = GetDataSet(sSql, "Content", "Contents");

                    oDs.Tables[0].Columns["displayorder"].ColumnMapping = MappingType.Attribute;

                    PerfMonLog("DBHelper", "AddDataSetToContent - AddRelated - " + sSql);


                    if (oContentElmt.ParentNode != null)
                    {
                        if (oContentElmt.ParentNode.Name == "ContentDetail")
                        {
                            // Calculate the maxdepth for contentDetial
                            string cShowRelatedBriefDepth = goConfig["ShowRelatedDetailedDepth"] + "";
                            if (Conversions.ToInteger("0" + oContentElmt.GetAttribute("relatedDepth")) > 0)
                            {
                                cShowRelatedBriefDepth = oContentElmt.GetAttribute("relatedDepth");
                            }
                            int nMaxDepth = 1;
                            if (!string.IsNullOrEmpty(cShowRelatedBriefDepth) && Information.IsNumeric(cShowRelatedBriefDepth))
                            {
                                nMaxDepth = Conversions.ToInteger(cShowRelatedBriefDepth);
                            }
                            DateTime? argdExpireDate = DateTime.Parse("0001-01-01");
                            DateTime? argdUpdateDate = DateTime.Parse("0001-01-01");
                            AddDataSetToContent(ref oDs, ref oContentElmt, ref argdExpireDate, ref argdUpdateDate, nParentId, true, nMaxDepth.ToString());
                        }
                        else
                        {
                            DateTime? argdExpireDate1 = DateTime.Parse("0001-01-01");
                            DateTime? argdUpdateDate1 = DateTime.Parse("0001-01-01");
                            AddDataSetToContent(ref oDs, ref oContentElmt, dExpireDate: ref argdExpireDate1, dUpdateDate: ref argdUpdateDate1, nParentId);
                        }
                    }
                    else
                    {
                        DateTime? argdExpireDate2 = DateTime.Parse("0001-01-01");
                        DateTime? argdUpdateDate2 = DateTime.Parse("0001-01-01");
                        AddDataSetToContent(ref oDs, ref oContentElmt, dExpireDate: ref argdExpireDate2, dUpdateDate: ref argdUpdateDate2, nParentId);
                    }

                    PerfMonLog("DBHelper", "addRelatedContent - END");
                }

                // !!! we really should not call this recursively !!!!!
                // TS. Now we don't this is not being called.

                // Nath: Warning this caused errors with Paramo's displaying of related items!
                // SKUs were getting deleted because they no longer had a discount node!
                // Dim oDiscounts As New Protean.Cms.Cart.Discount(myWeb)
                // oDiscounts.getAvailableDiscounts(oContentElmt)

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "addRelatedContent", ex, sProcessInfo));
                }

            }
            /// <summary>
            /// This adds related content to child content nodes, given a parent node.
            /// The scope of which child nodes are processed is pre-determined by the presence of
            /// the @processForRelatedContent=true attribute in the child node.
            /// Child nodes are then recursively processed down their related content to a 
            /// depth determined by nMaxDepth.
            /// Related content nodes may be filtered by type if their parent node has an
            /// attribute of showRelated.
            /// </summary>
            /// <param name="oContentParent">The initial starting node, whose child nodes will be considered for adding related content to</param>
            /// <param name="dUpdateDate">The date to get content by. E.g. find related content on such and such a day</param>
            /// <param name="nMaxDepth">The levels of related content to try to retrieve.</param>
            /// <remarks></remarks>
            public void addBulkRelatedContent(ref XmlElement oContentParent, ref DateTime dUpdateDate, int nMaxDepth = 1)
            {

                PerfMonLog("DBHelper", "addBulkRelatedContent");
                // adds related content to passed xmlNode.
                // Page/ContentDetail/Content

                string sSql;
                string sFilterSql = "";
                //string sSql2 = "";
                //string sNodeName = "";
                //string sContent = "";
                //string specificTypeSQL = "";
                string sProcessInfo = "building the related Content XML";
                string sContentLevelxPath = "";

                // This is not cleared with each depth iteration as 
                // a Content node for a given @id should have the same @showRelated 
                // value regardless of how often or at what depth that content appears.
                var contentTypeFilters = new Dictionary<string, string>();

                try
                {

                    // The iteration in inferred by an xPath of ParentNode/Content[@processForRelatedContent='true']/Content/Content/Content etc.
                    // where the iteration depth is reflected by the Content nodes.
                    // Note that the base child content nodes will have an attribute of @processForRelatedContent=true which sets the scope.
                    while (nMaxDepth > 0)
                    {

                        // First we get a list of the relations for the content that is in scope
                        // Note: we ensure that the child node is not a Module (as some two way relationships may historically exist)
                        string relationTypeColumn = Conversions.ToString(Interaction.IIf(checkTableColumnExists("tblContentRelation", "cRelationType"), ",cRelationType AS rtype ", ""));

                        sSql = "SELECT r.nContentParentId as parId, r.nContentChildId as id, r.nDisplayOrder as displayorder " + relationTypeColumn + ", parentContent.cContentSchemaName as parType, childContent.cContentSchemaName as childType " + " FROM tblContentRelation r " + " inner join tblContent parentContent on (r.nContentParentId = parentContent.nContentKey)" + " inner join tblContent childContent on (r.nContentChildId = childContent.nContentKey)" + " where childContent.cContentSchemaName <> 'Module' AND r.nContentParentId IN (";
                        string cRelatedIds = "";
                        foreach (XmlElement oElmt in oContentParent.SelectNodes("Content[@processForRelatedContent='true']" + sContentLevelxPath))
                        {

                            // Add to the related IDs
                            if (!string.IsNullOrEmpty(oElmt.GetAttribute("id")))
                            {
                                cRelatedIds += oElmt.GetAttribute("id") + ",";
                            }
                            // Build a content type filter for this content's child nodes
                            // if the content has a filter, indicated by the repsence of a showRelated attribute
                            // Note: we only build these content filters if the content is Brief content - this
                            // can be determined by looking to see if it has an ancestor of ContentDetail
                            if (oContentParent.SelectSingleNode("ancestor::ContentDetail") is null && !string.IsNullOrEmpty(oElmt.GetAttribute("showRelated").Trim()) && !contentTypeFilters.ContainsKey(oElmt.GetAttribute("id")))

                            {
                                contentTypeFilters.Add(oElmt.GetAttribute("id"), oElmt.GetAttribute("showRelated"));
                            }


                            // Cleanup the recursion attribute
                            // oElmt.RemoveAttribute("processForRelatedContent")

                        }
                        cRelatedIds = cRelatedIds.Trim(',');
                        sSql += cRelatedIds + ") order by r.nContentParentId, r.nDisplayOrder";

                        if (!string.IsNullOrEmpty(cRelatedIds))
                        {


                            // Get the relations data and transform it into XML 
                            var oDs1 = new DataSet();
                            PerfMonLog("DBHelper", "addBulkRelatedContent - GetDataSTART");
                            oDs1 = GetDataSet(sSql, "Relation");
                            PerfMonLog("DBHelper", "addBulkRelatedContent - GetDataEND", sSql);
                            {
                                var withBlock = oDs1.Tables[0];
                                withBlock.Columns["parId"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["id"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["displayorder"].ColumnMapping = MappingType.Attribute;
                                if (!string.IsNullOrEmpty(relationTypeColumn))
                                {
                                    withBlock.Columns["rtype"].ColumnMapping = MappingType.Attribute;
                                }
                                withBlock.Columns["parType"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["childType"].ColumnMapping = MappingType.Attribute;
                            }

                            var oRelationsXml = new XmlDocument();
                            oRelationsXml.LoadXml(oDs1.GetXml());
                            oRelationsXml.PreserveWhitespace = false;



                            // ==========================================================
                            // FILTER THE RELATIONS BY RESTRICTED CHILD CONTENT TYPE
                            // ==========================================================
                            foreach (string contentTypeFilterKey in contentTypeFilters.Keys)
                            {

                                string typeFilter = "";
                                string filterXPath = "";

                                // Build the Xpath filter
                                string[] filteredTypes = contentTypeFilters[contentTypeFilterKey].Split(',');
                                for (int index = 0, loopTo = filteredTypes.Length - 1; index <= loopTo; index++)
                                    filteredTypes[index] = string.Format("@childType='{0}'", filteredTypes[index]);
                                typeFilter = string.Join(" or ", filteredTypes);

                                filterXPath = "@parId=" + contentTypeFilterKey;
                                if (!string.IsNullOrEmpty(typeFilter))
                                    filterXPath += " and not(" + typeFilter + ")";

                                // Go and get the nodes to remove
                                foreach (XmlElement removableChild in oRelationsXml.SelectNodes("NewDataSet/Relation[" + filterXPath + "]"))
                                    removableChild.ParentNode.RemoveChild(removableChild);

                            }

                            // Create a content list - it doesn't matter about type 
                            var contentList = new List<string>();
                            foreach (XmlElement relation in oRelationsXml.SelectNodes("NewDataSet/Relation"))
                            {
                                if (!contentList.Contains(relation.GetAttribute("id")))
                                {
                                    contentList.Add(relation.GetAttribute("id"));
                                }
                            }


                            // Only proceed if we have some items to investigate
                            if (contentList.Count > 0)
                            {

                                sSql = "select c.nContentKey as id, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.nInsertDirId as owner, dbo.fxn_getContentParents(c.nContentKey) as parId " + " FROM tblContent c INNER JOIN" + " tblAudit a ON c.nAuditId = a.nAuditKey " + " WHERE ";

                                sSql += " c.nContentKey IN (" + string.Join(",", contentList.ToArray()) + ") ";

                                // Build the filter SQL
                                sFilterSql = myWeb.GetStandardFilterSQLForContent();

                                sSql += sFilterSql + " order by type";

                                var oDs = new DataSet();
                                PerfMonLog("DBHelper", "addBulkRelatedContent - get relations", sSql);
                                oDs = GetDataSet(sSql, "Content", "Contents");
                                PerfMonLog("DBHelper", "addBulkRelatedContent - get relations end", sSql);
                                var contents = ContentDataSetToXml(ref oDs, ref dUpdateDate);
                                var contents2 = oContentParent.OwnerDocument.ImportNode(contents.DocumentElement, true);
                                XmlElement contentChild;
                                foreach (XmlElement oContent in contents2.SelectNodes("Content"))
                                {
                                    DateTime argdExpireDate = default;
                                    XmlElement xmloContent = oContent;
                                    xmloContent = SimpleTidyContentNode(ref xmloContent, ref argdExpireDate, ref dUpdateDate, "");
                                }

                                // now lets take our xml's and do the magic
                                PerfMonLog("DBHelper", "addBulkRelatedContent - start place contents");
                                object nRelationCount = 0;
                                // Run through each Relation
                                foreach (XmlElement relation in oRelationsXml.SelectNodes("NewDataSet/Relation"))
                                {

                                    nRelationCount = Operators.AddObject(nRelationCount, 1);

                                    // For each relation, find content nodes at the current level that match the parent id
                                    // This implements the following protections:
                                    // RECURSION - Make sure that the content nodes don't have an ancestor of the relationship child id.
                                    // DUPLICATE CONTENT - Make sure that the content nodes don't already have this child
                                    // Note that sContentLevelxPath has the / at the front - we need it at the end for this
                                    string xPathToParents = "";
                                    if (!string.IsNullOrEmpty(sContentLevelxPath))
                                        xPathToParents += sContentLevelxPath.Substring(1) + "/";

                                    xPathToParents += "Content[@id='" + relation.GetAttribute("parId") + "'" + " and not(ancestor-or-self::Content[@id='" + relation.GetAttribute("id") + "'])" + " and not(Content[@id='" + relation.GetAttribute("id") + "'])]";

                                    // Dim j As Integer = 0
                                    // Dim k As Integer = 0


                                    foreach (XmlElement contentParent in oContentParent.SelectNodes(xPathToParents))
                                    {

                                        contentChild = (XmlElement)contents2.SelectSingleNode("Content[@id='" + relation.GetAttribute("id") + "']");

                                        if (contentChild != null)
                                        {

                                            if (relation.HasAttribute("rtype"))
                                                contentChild.SetAttribute("rtype", relation.GetAttribute("rtype"));
                                            // contentChild = SimpleTidyContentNode(contentChild, "", Nothing, dUpdateDate)

                                            contentParent.AppendChild(contentChild.CloneNode(true));
                                            // k = k + 1
                                        }
                                        // j = j + 1
                                    }
                                    // myWeb.PerfMon.Log("DBHelper", "addrelation[" & relation.GetAttribute("id") & "]count[" & j & "]added[" & k & "]")

                                }
                                PerfMonLog("DBHelper", Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("addBulkRelatedContent - end place contents (", nRelationCount), " relations)")));
                            }
                        }
                        sContentLevelxPath += "/Content";
                        nMaxDepth = nMaxDepth - 1;
                    }

                    // Tidy up attributes
                    foreach (XmlElement contentNode in oContentParent.SelectNodes("Content[@processForRelatedContent]"))
                        contentNode.RemoveAttribute("processForRelatedContent");

                    PerfMonLog("DBHelper", "addBulkRelatedContent - END");
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "addBulkRelatedContent", ex, sProcessInfo));

                }

            }

            public void saveContentRelations()
            {
                PerfMonLog("DBHelper", "saveContentRelations");
                string sProcessInfo = "";
                try
                {
                    foreach (var oFrmItem in goRequest.Form)
                    {
                        string cOName = Conversions.ToString(oFrmItem);
                        string cOValue = goRequest.Form.Get(cOName);
                        string rType = myWeb.moRequest["relationType"];
                        if (cOName.Contains("list"))
                        {
                            if (string.IsNullOrEmpty(rType))
                            {
                                insertContentRelation(Conversions.ToInteger(goRequest.Form.Get("id")), cOValue, Conversions.ToBoolean(Interaction.IIf(myWeb.moRequest["RelType"] == "1way", false, true)));
                            }
                            else
                            {
                                insertContentRelation(Conversions.ToInteger(goRequest.Form.Get("id")), cOValue, Conversions.ToBoolean(Interaction.IIf(myWeb.moRequest["RelType"] == "1way", false, true)), rType);
                            }
                        }
                        // If cOName.Contains("reciprocate_") Then
                        // insertContentRelation(cOValue, goRequest.Form.Get("id"))
                        // End If
                    }
                }

                catch (Exception ex)
                {

                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "saveContentRelations", ex, sProcessInfo));

                }

            }

            // New Method for sku parent change functionality
            public void ChangeParentRelation(long oldParentID, long newParentID, long childid)
            {
                PerfMonLog("DBHelper", "ChangeParentRelation");
                string sProcessInfo = "";
                if (newParentID == 0L)
                {
                    newParentID = Conversions.ToLong(goRequest.Form["updateParent"]);
                }
                try
                {
                    // single update staetment
                    string cSQl;
                    cSQl = "update tblContentRelation set nContentParentId= " + newParentID + " where nContentParentId= " + oldParentID + " and nContentChildId =" + childid + "";
                    int nID = Conversions.ToInteger(ExeProcessSqlScalar(cSQl));
                }

                catch (Exception ex)
                {

                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ChangeParentRelation", ex, sProcessInfo));

                }

            }

            public string insertContentRelation(int nParentID, string nChildIDs, bool b2Way = false, string rType = "", bool bHaltRecursion = false)
            {
                PerfMonLog("DBHelper", "insertContentRelation");
                try
                {
                    string[] nChilds = Strings.Split(nChildIDs, ",");
                    string nIDs = "";
                    int nIx;
                    var loopTo = Information.UBound(nChilds);
                    for (nIx = 0; nIx <= loopTo; nIx++)
                    {


                        string cSQl;
                        if (!string.IsNullOrEmpty(rType))
                        {
                            cSQl = "Select nContentRelationKey from tblContentRelation where nContentParentId = " + nParentID + " and nContentChildId = " + nChilds[nIx] + "and cRelationType = '" + rType + "'";
                        }
                        else
                        {
                            cSQl = "Select nContentRelationKey from tblContentRelation where nContentParentId = " + nParentID + " and nContentChildId = " + nChilds[nIx];
                        }

                        int nID = Conversions.ToInteger(ExeProcessSqlScalar(cSQl));
                        if (nID > 0)
                        {
                            nIDs += nID + ",";
                        }
                        else
                        {

                            var oDs = new DataSet();
                            oDs = GetDataSet("SELECT * FROM tblContentRelation", "Content");

                            cSQl = "SELECT Count(tblContentRelation.nContentRelationKey) FROM tblContent INNER JOIN tblContent tblContent_1 ON tblContent.cContentSchemaName = tblContent_1.cContentSchemaName INNER JOIN tblContentRelation ON tblContent_1.nContentKey = tblContentRelation.nContentChildId" + " WHERE (tblContent.nContentKey = " + nChilds[nIx] + ") AND (tblContentRelation.nContentParentId = " + nParentID + ")";

                            int nCount = (int)Math.Round(Conversions.ToDouble(ExeProcessSqlScalar(cSQl)) + 1d);

                            if (oDs.Tables["Content"].Columns.Contains("cRelationType"))
                            {
                                cSQl = "INSERT INTO tblContentRelation (nContentParentId, nContentChildId, nDisplayOrder, nAuditId, cRelationType) VALUES (" + nParentID + "," + nChilds[nIx] + "," + nCount + "," + getAuditId(cDescription: "ContentRelation") + "," + "'" + SqlFmt(rType) + "'" + ")";
                            }
                            else
                            {
                                // getAuditId(nContentStatus, , , dPublishDate, dExpireDate) 
                                cSQl = "INSERT INTO tblContentRelation (nContentParentId, nContentChildId, nDisplayOrder, nAuditId) VALUES (" + nParentID + "," + nChilds[nIx] + "," + nCount + "," + getAuditId(cDescription: "ContentRelation") + ")";
                            }

                            // AG Note: Not sure what this is (BR wrote this)
                            // It appears to force a 2way relationship if this is an orphan content
                            // Problem is if the parent is also an orphan, then this will recurse infinitely
                            // hence we pass back the bHaltRecursion parameter.

                            // TS 16/03/22 commented this out, forces 2 way releationships I don't understand why this is nessesary or was added either.

                            // Dim nIsOrphan As Long = GetDataValue("SELECT nStructId  FROM tblContentLocation WHERE nContentId = " & nChilds(nIx), , , 0)
                            // If nIsOrphan = 0 And Not b2Way And Not bHaltRecursion Then
                            // insertContentRelation(nChilds(nIx), nParentID, , , True)
                            // End If


                            nIDs += GetIdInsertSql(cSQl) + ",";
                        }
                        if (b2Way)
                        {
                            if (!string.IsNullOrEmpty(rType))
                            {
                                insertContentRelation(Conversions.ToInteger(nChilds[nIx]), nParentID.ToString(), false, rType);
                            }
                            else
                            {
                                insertContentRelation(Conversions.ToInteger(nChilds[nIx]), nParentID.ToString(), false);
                            }
                        }
                    }
                    return Strings.Left(nIDs, nIDs.Length - 1);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "addContentRelation", ex, ""));
                    return "";
                }

            }


            public void RemoveContentRelation(long nRelatedParentId, long nContentId)
            {
                PerfMonLog("DBHelper", "RemoveContentRelation");
                try
                {

                    string cSQL = "Select nContentRelationKey from tblContentRelation where nContentParentID = " + nRelatedParentId + " AND nContentChildId = " + nContentId;
                    DeleteObject(objectTypes.ContentRelation, Conversions.ToLong(ExeProcessSqlScalar(cSQL)));
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "RemoveContentRelation", ex, ""));

                }

            }

            public void RemoveContentRelationByType(long nRelatedParentId, string cRelationType, string direction)
            {
                PerfMonLog("DBHelper", "RemoveContentRelation");
                try
                {

                    string cSQL;
                    if (Strings.LCase(direction) == "child")
                    {
                        cSQL = "Select nContentRelationKey from tblContentRelation where nContentChildId = " + nRelatedParentId + " AND cRelationType = '" + cRelationType + "'";
                    }
                    else
                    {
                        cSQL = "Select nContentRelationKey from tblContentRelation where nContentParentId = " + nRelatedParentId + " AND cRelationType = '" + cRelationType + "'";
                    }

                    using (var oRead = getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                    {
                        while (oRead.Read())
                            DeleteObject(objectTypes.ContentRelation, oRead.GetInt32(0));
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "RemoveContentRelation", ex, ""));

                }

            }

            public void RemoveContentLocation(long nPageId, long nContentId)
            {
                PerfMonLog("DBHelper", "RemoveContentRelation");
                try
                {

                    string cSQL = "Select nContentLocationKey from tblContentLocation where nStructId = " + nPageId + " AND nContentId = " + nContentId;
                    DeleteObject(objectTypes.ContentLocation, Conversions.ToLong(ExeProcessSqlScalar(cSQL)));
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "RemoveContentLocation", ex, ""));

                }

            }

            public XmlElement RelatedContentSearch(int nRootNode, string cSchemaName, bool bChildren, string cSearchExpression, int nParentId, int nIgnoreID = 0, string[] oRelated = null, bool bIncRelated = false)
            {
                PerfMonLog("DBHelper", "RelatedContentSearch");
                try
                {
                    string sSearch = cSearchExpression;
                    // remove reserved words
                    sSearch = Strings.Replace(" " + sSearch + " ", " the ", "");
                    sSearch = Strings.Replace(" " + sSearch + " ", " and ", "");
                    sSearch = Strings.Replace(" " + sSearch + " ", " if ", "");
                    sSearch = Strings.Replace(" " + sSearch + " ", " then ", "");
                    sSearch = Strings.Replace(" " + sSearch + " ", " or ", "");
                    sSearch = Strings.Trim(sSearch);


                    string cSQL = "";
                    var oDs = new DataSet();
                    XmlDocument oFullData;
                    var oResults = moPageXml.CreateElement("RelatedResults");
                    string cXPath = string.Empty;
                    string cXPath2 = string.Empty;

                    // if schemaName is a comma separated string
                    string[] aSchema = cSchemaName.Split(',');
                    int i;
                    string sWhere = "";
                    var loopTo = Information.UBound(aSchema);
                    for (i = 0; i <= loopTo; i++)
                        sWhere = sWhere + "cContentSchemaName = '" + Strings.Trim(aSchema[i]) + "' or ";
                    // lets whip of the last and
                    sWhere = Strings.Left(sWhere, Strings.Len(sWhere) - 3);


                    if (nRootNode == 0)
                    {
                        // All
                        cSQL = "SELECT nContentKey as id, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content,  " + "	a.dPublishDate AS publishDate " + " FROM tblContent c " + "	INNER JOIN tblAudit a ON a.nAuditKey = c.nAuditId " + " WHERE " + sWhere + " ORDER BY cContentName";



                    }

                    else if (nRootNode == -1)
                    {
                        // Orphans only
                        cSQL = "SELECT c.nContentKey as id, c.cContentForiegnRef as ref, c.cContentName as name, c.cContentSchemaName as type, c.cContentXmlBrief as content, " + "	a.dPublishDate AS publishDate " + " FROM tblContent c" + "	INNER JOIN tblAudit a ON a.nAuditKey = c.nAuditId " + " LEFT OUTER JOIN tblContentLocation cl ON c.nContentKey = cl.nContentId WHERE ( " + sWhere + " ) AND (cl.nContentLocationKey IS NULL) ORDER BY c.cContentName";



                    }
                    else
                    {
                        // by a page

                        // Optimisation is needed to get only relevant content for processing

                        // Firstly we get a structure XML and identify the nodes in question.
                        cSQL = "SELECT * FROM tblContentStructure";
                        oDs = GetDataSet(cSQL, "Structure", "SearchRelateable");
                        oDs.Relations.Add("StructStruct", oDs.Tables["Structure"].Columns["nStructKey"], oDs.Tables["Structure"].Columns["nStructParId"], false);
                        oDs.Relations["StructStruct"].Nested = true;
                        oDs.Tables["Structure"].Columns["nStructKey"].ColumnMapping = MappingType.Attribute;

                        // DS to XML
                        var oStructure = new XmlDocument();
                        oStructure.PreserveWhitespace = false;
                        oStructure.InnerXml = oDs.GetXml().ToString().Replace("&lt;", "<").Replace("&gt;", ">");

                        // Find the root node
                        XmlElement oRootNode = (XmlElement)oStructure.SelectSingleNode("//Structure[@nStructKey='" + nRootNode + "']");

                        // Get a list of the page ids - we're going to use this to filter content
                        string cLocations = nRootNode.ToString();
                        if (bChildren)
                        {
                            foreach (XmlElement oChild in oRootNode.SelectNodes("descendant::Structure"))
                                cLocations += "," + oChild.GetAttribute("nStructKey");
                        }

                        // Destroy things
                        oRootNode = null;
                        oStructure = null;

                        // Now get content (filtered by locations)

                        // First get a subquery that gets distinct items according to the criteria
                        string cSubquerySQL = string.Empty;
                        //Modified query according to criteria for review
                        if (cSchemaName == "Review")
                        {
                            cSubquerySQL = "SELECT DISTINCT c.nContentKey AS id " + "FROM tblContent c " + " " + "Inner join tblContentRelation cr  On c.nContentKey= cr.nContentChildId INNER JOIN tblAudit cra ON cra.nAuditKey= cr.nAuditId and cra.nStatus=1 " + "	INNER JOIN tblContentLocation l " + "		ON l.nContentId = cr.nContentParentId INNER JOIN tblAudit cla ON cla.nAuditKey= l.nAuditId and cla.nStatus=1 " + "WHERE (" + sWhere + ") " + "	AND NOT(c.nContentKey IN (0," + nIgnoreID + ")) " + "	AND l.nStructId IN (" + cLocations + ") ";

                        }
                        else
                        {
                            cSubquerySQL = "SELECT DISTINCT c.nContentKey AS id " + "FROM tblContent c " + "	INNER JOIN tblContentLocation l " + "		ON c.nContentKey = l.nContentId " + "WHERE (" + sWhere + ") " + "	AND NOT(c.nContentKey IN (0," + nIgnoreID + ")) " + "	AND l.nStructId IN (" + cLocations + ") ";

                        }


                        // No get more information
                        cSQL = "SELECT " + "	c.nContentKey AS id,  " + "	c.cContentForiegnRef AS ref,  " + "	c.cContentName AS name,  " + "	c.cContentSchemaName AS type,  " + "	c.cContentXmlBrief AS content,  " + "	a.dPublishDate AS publishDate " + "FROM tblContent c " + "	INNER JOIN tblAudit a ON a.nAuditKey = c.nAuditId " + "	INNER JOIN (" + cSubquerySQL + ") distinctlist " + "		ON c.nContentKey = distinctlist.id " + "ORDER BY c.cContentName";











                    }


                    oDs = GetDataSet(cSQL, "Content", "SearchRelateable");
                    oDs.EnforceConstraints = false;
                    oDs.Tables["Content"].Columns["id"].ColumnMapping = MappingType.Attribute;
                    oDs.Tables["Content"].Columns["ref"].ColumnMapping = MappingType.Attribute;
                    oDs.Tables["Content"].Columns["name"].ColumnMapping = MappingType.Attribute;
                    oDs.Tables["Content"].Columns["type"].ColumnMapping = MappingType.Attribute;
                    oDs.Tables["Content"].Columns["publishDate"].ColumnMapping = MappingType.Attribute;
                    oDs.Tables["Content"].Columns["content"].ColumnMapping = MappingType.SimpleContent;

                    oFullData = new XmlDocument();
                    oFullData.PreserveWhitespace = false;
                    oFullData.InnerXml = oDs.GetXml().ToString();
                    foreach (XmlNode oNode in oFullData.SelectNodes("SearchRelateable/Content"))
                    {

                        XmlElement argoContent = (XmlElement)oNode;
                        DateTime argdExpireDate = DateTime.Parse("0001-01-01");
                        DateTime argdUpdateDate = DateTime.Parse("0001-01-01");
                        argoContent = SimpleTidyContentNode(ref argoContent, dExpireDate: ref argdExpireDate, dUpdateDate: ref argdUpdateDate);
                        //oNode = argoContent;

                    }

                    string[] SearchArr = Strings.Split(cSearchExpression, " ");

                    // bool bFound = false;

                    string idList = "";
                    // Get each content node and check it against the Search Array
                    foreach (XmlElement oTempNode in oFullData.SelectNodes("SearchRelateable/Content"))
                    {

                        var loopTo1 = SearchArr.GetUpperBound(0);
                        for (i = SearchArr.GetLowerBound(0); i <= loopTo1; i++)
                        {
                            if (oTempNode.InnerText.ToUpper().Contains(SearchArr[i].ToUpper()))
                            {
                                oResults.AppendChild(moPageXml.ImportNode(oTempNode, true));
                                // bFound = true;
                            }
                        }
                        if (bIncRelated)
                        {
                            idList = idList + oTempNode.GetAttribute("id") + ",";
                        }
                    }
                    idList = idList.Trim(',');

                    if (bIncRelated)
                    {
                        cSQL = "SELECT c.nContentKey AS id, distinctlist.parId, c.cContentForiegnRef AS ref, c.cContentName AS name,  	c.cContentSchemaName AS type,  " + "c.cContentXmlBrief AS content, a.dPublishDate AS publishDate FROM tblContent c INNER JOIN tblAudit a ON a.nAuditKey = c.nAuditId " + "INNER JOIN (SELECT DISTINCT c.nContentKey AS id , rel.nContentParentId AS parId FROM tblContent c INNER JOIN tblContentRelation rel ON c.nContentKey = rel.nContentChildId " + "WHERE (cContentSchemaName = 'Product' or cContentSchemaName = 'SKU' ) 	AND rel.nContentParentId IN (" + idList + ")) distinctlist ON c.nContentKey = distinctlist.id ORDER BY c.cContentName";

                        oDs = GetDataSet(cSQL, "Content", "SearchRelated");
                        oDs.EnforceConstraints = false;
                        oDs.Tables["Content"].Columns["id"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables["Content"].Columns["parId"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables["Content"].Columns["ref"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables["Content"].Columns["name"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables["Content"].Columns["type"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables["Content"].Columns["publishDate"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables["Content"].Columns["content"].ColumnMapping = MappingType.SimpleContent;

                        var oRelatedData = moPageXml.CreateElement("RelatedData");
                        oRelatedData.InnerXml = oDs.GetXml().ToString();
                        foreach (XmlElement oRelNode in oRelatedData.SelectNodes("SearchRelated/Content"))
                        {
                            DateTime argdExpireDate1 = DateTime.Parse("0001-01-01");
                            DateTime argdUpdateDate1 = DateTime.Parse("0001-01-01");
                            XmlElement xmloRelNode = oRelNode;
                            xmloRelNode = SimpleTidyContentNode(ref xmloRelNode, dExpireDate: ref argdExpireDate1, dUpdateDate: ref argdUpdateDate1);
                            string ParId = xmloRelNode.GetAttribute("parId");
                            XmlElement ParNode = (XmlElement)oResults.SelectSingleNode("Content[@id='" + ParId + "']");
                            ParNode.AppendChild(xmloRelNode);
                        }

                    }

                    oResults.SetAttribute("nParentID", nParentId.ToString());
                    oResults.SetAttribute("cSchemaName", cSchemaName);

                    int nI;
                    if (oRelated != null)
                    {
                        var loopTo2 = Information.UBound(oRelated);
                        for (nI = 0; nI <= loopTo2; nI++)
                        {
                            if (!string.IsNullOrEmpty(oRelated[nI]))
                            {

                                XmlElement oTmp = (XmlElement)oResults.SelectSingleNode("Content[@id=" + oRelated[nI] + "]");
                                if (oTmp != null)
                                {
                                    oTmp.SetAttribute("related", 1.ToString());

                                    if (checkTableColumnExists("tblContentRelation", "cRelationType"))
                                    {

                                        cSQL = "SELECT cRelationType FROM tblContentRelation WHERE nContentParentId = '" + nParentId.ToString() + "' AND nContentChildId = '" + oRelated[nI] + "'";

                                        using (var oDre = getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                                        {
                                            string cSqlResults = "";

                                            while (oDre.Read())
                                                cSqlResults = Conversions.ToString(cSqlResults + Operators.ConcatenateObject(oDre[0], ","));
                                            oDre.Close();
                                            if (!string.IsNullOrEmpty(cSqlResults))
                                                cSqlResults = Strings.Left(cSqlResults, Strings.Len(cSqlResults) - 1);

                                            if (!string.IsNullOrEmpty(cSqlResults))
                                            {
                                                oTmp.SetAttribute("sType", cSqlResults);
                                            }
                                            else
                                            {
                                                oTmp.SetAttribute("sType", "Not Specified");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        oTmp.SetAttribute("sType", "Not Specified");
                                    }

                                }
                            }
                        }
                    }

                    return oResults;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "RelatedContentSearch", ex, ""));
                    return null;
                }
            }

            public void saveProductsGroupRelations()
            {
                PerfMonLog("DBHelper", "saveProductsGroupRelations");
                try
                {
                    foreach (var oFrmItem in goRequest.Form)
                    {
                        string cOName = Conversions.ToString(oFrmItem);
                        string cOValue = goRequest.Form.Get(cOName);
                        if (cOName.Contains("list"))
                        {
                            insertGroupProductRelation(Conversions.ToInteger(goRequest.QueryString["GroupId"]), cOValue);
                        }
                        if (cOName.Contains("unrelate"))
                        {
                            deleteGroupProductRelation(Conversions.ToInteger(goRequest.QueryString["GroupId"]), cOValue);
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "saveProductsGroupRelations", ex, ""));

                }
            }


            public string insertGroupProductRelation(int nGroupId, string nContent)
            {
                PerfMonLog("DBHelper", "insertProductGroupRelation");
                try
                {
                    string[] oContentArr = Strings.Split(nContent, ",");
                    int cCount;
                    var strReturn = new System.Text.StringBuilder();


                    var loopTo = Information.UBound(oContentArr);
                    for (cCount = 0; cCount <= loopTo; cCount++)
                    {

                        int nCatProductRelKey;

                        string cSQl = "Select nCatProductRelKey from tblCartCatProductRelations where nCatId = " + nGroupId + " and nContentId = " + oContentArr[cCount];
                        nCatProductRelKey = Conversions.ToInteger(ExeProcessSqlScalar(cSQl));

                        // if there's no existing relation, make a new one
                        if (nCatProductRelKey == 0)
                        {
                            cSQl = "INSERT INTO tblCartCatProductRelations (nContentId, nCatId, nAuditId) VALUES (" + oContentArr[cCount] + "," + nGroupId + "," + getAuditId(cDescription: "ContentRelation") + ")";

                            nCatProductRelKey = Conversions.ToInteger(GetIdInsertSql(cSQl));
                        }

                        strReturn.Append(nCatProductRelKey);

                        // delimit by comma, except on last pass
                        if (cCount != Information.UBound(oContentArr))
                        {
                            strReturn.Append(",");
                        }

                    }
                    string s = strReturn.ToString();


                    return strReturn.ToString();
                }


                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "insertProductGroupRelation", ex, ""));
                    return "0";
                }


            }

            public string deleteGroupProductRelation(int nGroupId, string nContent)
            {
                PerfMonLog("DBHelper", "insertProductGroupRelation");
                try
                {
                    string[] oContentArr = Strings.Split(nContent, ",");
                    int cCount;
                    var strReturn = new System.Text.StringBuilder();


                    var loopTo = Information.UBound(oContentArr);
                    for (cCount = 0; cCount <= loopTo; cCount++)
                    {

                        int nCatProductRelKey;

                        string cSQl = "Select nCatProductRelKey from tblCartCatProductRelations where nCatId = " + nGroupId + " and nContentId = " + oContentArr[cCount];
                        nCatProductRelKey = Conversions.ToInteger(ExeProcessSqlScalar(cSQl));

                        if (nCatProductRelKey != 0)
                        {
                            DeleteObject(objectTypes.CartCatProductRelations, nCatProductRelKey);
                        }

                    }

                    return "1";
                }


                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "insertProductGroupRelation", ex, ""));
                    return "0";
                }


            }


            public string insertProductGroupRelation(int nProductId, string sGroupIds)
            {
                PerfMonLog("DBHelper", "insertProductGroupRelation");
                string cProcessInfo = "insertProductGroupRelation";
                try
                {
                    string[] oGroupArr = Strings.Split(sGroupIds.Trim(','), ",");
                    int cCount;
                    var strReturn = new System.Text.StringBuilder();

                    if (!string.IsNullOrEmpty(sGroupIds))
                    {
                        var loopTo = Information.UBound(oGroupArr);
                        for (cCount = 0; cCount <= loopTo; cCount++)
                        {

                            int nCatProductRelKey;
                            int savedId;

                            string cSQl = "Select nCatProductRelKey from tblCartCatProductRelations where nContentId = " + nProductId + " and nCatId = " + oGroupArr[cCount];
                            nCatProductRelKey = Conversions.ToInteger(ExeProcessSqlScalar(cSQl));

                            // if there's no existing relation, make a new one
                            if (nCatProductRelKey == 0)
                            {
                                cSQl = "INSERT INTO tblCartCatProductRelations (nContentId, nCatId, nAuditId) VALUES (" + nProductId + "," + oGroupArr[cCount] + "," + getAuditId(cDescription: "ContentRelation") + ")";
                                savedId = Conversions.ToInteger(GetIdInsertSql(cSQl));
                            }
                            else
                            {
                                savedId = nCatProductRelKey;
                            }
                            if (savedId > 0)
                            {
                                strReturn.Append(savedId);
                                strReturn.Append(",");
                            }
                        }

                        string s = strReturn.ToString();
                        s = s.TrimEnd(',');

                        // delete any new ones
                        string delSql = "Select nCatProductRelKey from tblCartCatProductRelations where nContentId = " + nProductId + " and nCatProductRelKey not in (" + s + ")";
                        using (var oDr = getDataReaderDisposable(delSql))  // Done by nita on 6/7/22
                        {
                            if (oDr != null)
                            {
                                while (oDr.Read())
                                    DeleteObject(objectTypes.CartCatProductRelations, Conversions.ToLong(oDr[0]));
                            }
                            else
                            {
                                cProcessInfo = nProductId + " Not Found in " + s;
                            }

                            return s;
                        }
                    }
                    else
                    {
                        // if GroupIds is empty then delete all
                        string delSql = "Select nCatProductRelKey from tblCartCatProductRelations where nContentId = " + nProductId;
                        using (var oDr = getDataReaderDisposable(delSql))  // Done by nita on 6/7/22
                        {
                            while (oDr.Read())
                                DeleteObject(objectTypes.CartCatProductRelations, Conversions.ToLong(oDr[0]));
                        }
                        return "";
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "insertProductGroupRelation", ex, cProcessInfo));
                    return "0";
                }
                finally
                {
                    CloseConnection();
                }


            }




            public string saveDiscountDirRelation(int nDiscountId, string nDirIds, bool bInsert = true, PermissionLevel Permlevel = PermissionLevel.Open)
            {
                PerfMonLog("DBHelper", "saveDiscountDirRelation");
                try
                {
                    string[] cGroups = Strings.Split(nDirIds, ",");
                    int nI;
                    int nDirId;
                    string cNewIds = "";
                    int nPermLevel = Conversions.ToInteger("1");
                    bool bDeny = false;
                    if (checkTableColumnExists("tblCartDiscountDirRelations", "nPermLevel"))
                    {
                        bDeny = true;
                        switch (Permlevel)
                        {
                            case PermissionLevel.Denied:
                                {
                                    nPermLevel = 0;
                                    break;
                                }

                            default:
                                {
                                    nPermLevel = 1;
                                    break;
                                }
                        }
                    }

                    var loopTo = Information.UBound(cGroups);
                    for (nI = 0; nI <= loopTo; nI++)
                    {
                        nDirId = Conversions.ToInteger(cGroups[nI]);
                        if (bInsert)
                        {


                            // if exists then  return the id
                            string cSQL = "Select nDiscountDirRelationKey From tblCartDiscountDirRelations Where nDiscountId = " + nDiscountId + " And nDirId = " + nDirId;
                            int nId = Conversions.ToInteger(ExeProcessSqlScalar(cSQL));
                            if (nId > 0)
                                return nId.ToString();

                            // Logic to rmove relations if an "all" entry exists, or other way round
                            if (nDirId > 0)
                            {
                                // remove any "all" record
                                cSQL = "Select nDiscountDirRelationKey From tblCartDiscountDirRelations Where nDiscountId = " + nDiscountId + " And nDirId = 0 And nPermLevel = " + nPermLevel;
                                DeleteObject(objectTypes.CartDiscountDirRelations, Conversions.ToLong(ExeProcessSqlScalar(cSQL)));
                            }
                            else
                            {
                                // remove any specific record
                                cSQL = "Select nDiscountDirRelationKey From tblCartDiscountDirRelations Where nDiscountId = " + nDiscountId + " And nDirId > 0 And nPermLevel = " + nPermLevel;
                                using (var oDre = getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                                {
                                    while (oDre.Read())
                                        DeleteObject(objectTypes.CartDiscountDirRelations, Conversions.ToLong(oDre[0]));
                                }
                            }
                            if (bDeny)
                            {
                                cSQL = "INSERT INTO tblCartDiscountDirRelations (nDiscountId, nDirId, nAuditId, nPermLevel) Values(" + nDiscountId + "," + nDirId + "," + getAuditId(1, cDescription: "DiscountDirRelation") + ", " + nPermLevel + ")";
                            }
                            else
                            {
                                cSQL = "INSERT INTO tblCartDiscountDirRelations (nDiscountId, nDirId, nAuditId) Values(" + nDiscountId + "," + nDirId + "," + getAuditId(1, cDescription: "DiscountDirRelation") + ")";
                            }

                            cNewIds += GetIdInsertSql(cSQL) + ",";
                        }
                        else
                        {
                            cNewIds += DeleteObject(objectTypes.CartDiscountDirRelations, nDirId) + ",";
                        }
                    }
                    return Strings.Left(cNewIds, cNewIds.Length - 1);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "insertDiscountDirRelation", ex, ""));
                    return 0.ToString();
                }
            }





            public string saveShippingDirRelation(int nShippingMethodId, string nDirIds, bool bInsert = true, PermissionLevel Permlevel = PermissionLevel.Open)
            {
                PerfMonLog("DBHelper", "saveShippingDirRelation");
                try
                {
                    string[] cGroups = Strings.Split(nDirIds, ",");
                    int nI;
                    int nDirId;
                    string cNewIds = "";
                    bool bDeny = false;
                    int nPermLevel = Conversions.ToInteger("1");
                    if (checkTableColumnExists("tblCartShippingPermission", "nPermLevel"))
                    {
                        bDeny = true;
                        switch (Permlevel)
                        {
                            case PermissionLevel.Denied:
                                {
                                    nPermLevel = 0;
                                    break;
                                }

                            default:
                                {
                                    nPermLevel = 1;
                                    break;
                                }
                        }
                    }

                    var loopTo = Information.UBound(cGroups);
                    for (nI = 0; nI <= loopTo; nI++)
                    {
                        nDirId = Conversions.ToInteger(cGroups[nI]);
                        if (bInsert)
                        {
                            // if exists then  return the id
                            string cSQL = "Select nCartShippingPermissionKey From tblCartShippingPermission Where nShippingMethodId = " + nShippingMethodId + " And nDirId = " + nDirId;
                            int nId = Conversions.ToInteger(ExeProcessSqlScalar(cSQL));
                            if (nId > 0)
                                return nId.ToString();

                            // Logic to rmove relations if an "all" entry exists, or other way round
                            if (nDirId > 0)
                            {
                                // remove any "all" record
                                cSQL = "Select nCartShippingPermissionKey From tblCartShippingPermission Where nShippingMethodId = " + nShippingMethodId + " And nDirId = 0";
                                DeleteObject(objectTypes.CartShippingPermission, Conversions.ToLong(ExeProcessSqlScalar(cSQL)));
                            }
                            else
                            {
                                // remove any specific record
                                cSQL = "Select nCartShippingPermissionKey From tblCartShippingPermission Where nShippingMethodId = " + nShippingMethodId + " And nDirId > 0";
                                using (var oDre = getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                                {
                                    while (oDre.Read())
                                        DeleteObject(objectTypes.CartShippingPermission, Conversions.ToLong(oDre[0]));
                                }
                            }
                            if (bDeny)
                            {
                                cSQL = "INSERT INTO tblCartShippingPermission (nShippingMethodId, nDirId, nPermLevel, nAuditId) Values(" + nShippingMethodId + "," + nDirId + "," + nPermLevel + "," + getAuditId(1, cDescription: "CartShippingPermission") + ")";
                            }

                            else
                            {
                                cSQL = "INSERT INTO tblCartShippingPermission (nShippingMethodId, nDirId, nAuditId) Values(" + nShippingMethodId + "," + nDirId + "," + getAuditId(1, cDescription: "CartShippingPermission") + ")";
                            }

                            cNewIds += GetIdInsertSql(cSQL) + ",";
                        }
                        else
                        {
                            cNewIds += DeleteObject(objectTypes.CartShippingPermission, nDirId) + ",";
                        }
                    }
                    return Strings.Left(cNewIds, cNewIds.Length - 1);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "saveShippingDirRelation", ex, ""));
                    return 0.ToString();
                }
            }

            public string saveProductShippingGroupDirRelation(int nShippingMethodId, string nCatKeys, bool bInsert = true, PermissionLevel nRuleType = PermissionLevel.Open)
            {
                PerfMonLog("DBHelper", "saveShippingDirRelation");
                try
                {
                    string[] cGroups = Strings.Split(nCatKeys, ",");
                    int nI;
                    string cNewIds = "";
                    bool bDeny = false;
                    // Dim nRuleType As Integer = "1"
                    if (checkTableColumnExists("tblCartShippingProductCategoryRelations", "nRuleType"))
                    {
                        bDeny = true;
                        switch (nRuleType)
                        {
                            case PermissionLevel.Denied:
                                {
                                    nRuleType = (PermissionLevel)2;
                                    break;
                                }

                            default:
                                {
                                    nRuleType = (PermissionLevel)1;
                                    break;
                                }
                        }
                    }

                    var loopTo = Information.UBound(cGroups);
                    for (nI = 0; nI <= loopTo; nI++)
                    {
                        nCatKeys = Conversions.ToInteger(cGroups[nI]).ToString();
                        if (bInsert)
                        {
                            // if exists then  return the id
                            string cSQL = "Select nShipProdCatRelKey From tblCartShippingProductCategoryRelations Where nShipOptId = " + nShippingMethodId + " And nCatId = " + nCatKeys;
                            int nId = Conversions.ToInteger(ExeProcessSqlScalar(cSQL));
                            if (nId > 0)
                                return nId.ToString();

                            // Logic to rmove relations if an "all" entry exists, or other way round
                            if (Conversions.ToDouble(nCatKeys) > 0d)
                            {
                                // remove any "all" record
                                cSQL = "Select nShipProdCatRelKey From tblCartShippingProductCategoryRelations Where nShipOptId = " + nShippingMethodId + " And nCatId = 0";
                                DeleteObject(objectTypes.nShipProdCatRelKey, Conversions.ToLong(ExeProcessSqlScalar(cSQL)));
                            }
                            else
                            {
                                // remove any specific record
                                cSQL = "Select nShipProdCatRelKey From tblCartShippingProductCategoryRelations Where nShipOptId = " + nShippingMethodId + " And nCatId > 0";
                                using (var oDre = getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                                {
                                    while (oDre.Read())
                                        DeleteObject(objectTypes.nShipProdCatRelKey, Conversions.ToLong(oDre[0]));
                                }
                            }
                            if (bDeny)
                            {
                                cSQL = "INSERT INTO tblCartShippingProductCategoryRelations (nShipOptId, nCatId, nRuleType, nAuditId) Values(" + nShippingMethodId + "," + nCatKeys + "," + ((int)nRuleType).ToString() + "," + getAuditId(1, cDescription: "CartShippingProductCategoryRelations") + ")";
                            }


                            else
                            {
                                cSQL = "INSERT INTO tblCartShippingProductCategoryRelations (nShipOptId, nCatId, nRuleType, nAuditId) Values(" + nShippingMethodId + "," + nCatKeys + "," + ((int)nRuleType).ToString() + "," + getAuditId(1, cDescription: "CartShippingProductCategoryRelations") + ")";
                            }

                            cNewIds += GetIdInsertSql(cSQL) + ",";
                        }
                        else
                        {
                            cNewIds += DeleteObject(objectTypes.nShipProdCatRelKey, Conversions.ToLong(nCatKeys)) + ",";
                        }
                    }
                    return Strings.Left(cNewIds, cNewIds.Length - 1);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "saveProductShippingGroupDirRelation", ex, ""));
                    return 0.ToString();
                }
            }

            public string saveDiscountProdGroupRelation(int nDiscountId, string cProductgroups, bool bInsert = true)
            {
                PerfMonLog("DBHelper", "saveDiscountProdGroupRelation");
                try
                {
                    string[] cGroups = Strings.Split(cProductgroups, ",");
                    int nI;
                    int nProdGroupId;
                    string cNewIds = "";
                    var loopTo = Information.UBound(cGroups);
                    for (nI = 0; nI <= loopTo; nI++)
                    {
                        nProdGroupId = Conversions.ToInteger(cGroups[nI]);
                        if (bInsert)
                        {
                            // if exists then  return the id
                            string cSQL = "Select nDiscountProdCatRelationKey From tblCartDiscountProdCatRelations Where nDiscountId = " + nDiscountId + " And nProductCatId = " + nProdGroupId;
                            int nId = Conversions.ToInteger(ExeProcessSqlScalar(cSQL));
                            if (nId > 0)
                                return nId.ToString();

                            // Logic to rmove relations if an "all" entry exists, or other way round
                            if (nProdGroupId > 0)
                            {
                                // remove any "all" record
                                cSQL = "Select nDiscountProdCatRelationKey From tblCartDiscountProdCatRelations Where nDiscountId = " + nDiscountId + " And nProductCatId = 0";
                                DeleteObject(objectTypes.CartDiscountProdCatRelations, Conversions.ToLong(ExeProcessSqlScalar(cSQL)));
                            }
                            else
                            {
                                // remove any specific record
                                cSQL = "Select nDiscountProdCatRelationKey From tblCartDiscountProdCatRelations Where nDiscountId = " + nDiscountId + " And nProductCatId > 0";
                                using (var oDre = getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                                {
                                    while (oDre.Read())
                                        DeleteObject(objectTypes.CartDiscountProdCatRelations, Conversions.ToLong(oDre[0]));
                                }
                            }

                            cSQL = "INSERT INTO tblCartDiscountProdCatRelations (nDiscountId, nProductCatId, nAuditId) Values(" + nDiscountId + "," + nProdGroupId + "," + getAuditId(1, cDescription: "DiscountProdGroupRelation") + ")";
                            cNewIds += GetIdInsertSql(cSQL) + ",";
                        }
                        else
                        {
                            cNewIds += DeleteObject(objectTypes.CartDiscountProdCatRelations, nProdGroupId) + ",";
                        }
                    }
                    return Strings.Left(cNewIds, cNewIds.Length - 1);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "insertDiscountProdGroupRelation", ex, ""));
                    return 0.ToString();
                }
            }

            public bool PageIsLive(int nPageId)
            {
                PerfMonLog("DBHelper", "PageIsLive");
                try
                {
                    string sSQL = "Select tblAudit.dPublishDate, tblAudit.dExpireDate, tblAudit.nStatus, tblContentStructure.nStructKey FROM tblAudit INNER JOIN tblContentStructure On tblAudit.nAuditKey = tblContentStructure.nAuditId WHERE tblContentStructure.nStructKey = " + nPageId;
                    using (var oDRe = getDataReaderDisposable(sSQL))  // Done by nita on 6/7/22
                    {
                        bool bPublish = false;
                        bool bExpire = false;
                        bool bStatus = false;

                        while (oDRe.Read())
                        {
                            // Publish
                            if (!oDRe.IsDBNull(0))
                            {
                                if (Information.IsNumeric(oDRe[0]))
                                {
                                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oDRe[0], 0, false)))
                                        bPublish = true;
                                }
                                else if (Conversions.ToDate(oDRe[0]) <= DateTime.Now)
                                    bPublish = true;
                            }
                            else
                            {
                                bPublish = true;
                            }
                            // Expire
                            if (!oDRe.IsDBNull(1))
                            {
                                if (Information.IsNumeric(oDRe[1]))
                                {
                                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oDRe[1], 0, false)))
                                        bExpire = true;
                                }
                                else if (Conversions.ToDate(oDRe[1]) <= DateTime.Now)
                                    bExpire = true;
                            }
                            else
                            {
                                bExpire = true;
                            }
                            // Status
                            if (!oDRe.IsDBNull(2))
                            {
                                if (Conversions.ToBoolean(!Operators.ConditionalCompareObjectEqual(oDRe[2], 0, false)))
                                    bStatus = true;
                            }
                            else
                            {
                                bStatus = true;
                            }
                        }


                        if (bPublish & bExpire & bStatus)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "PageIsLive", ex, ""));
                }

                return default;
            }

            public bool AddInvalidEmail(string cEmailAddress)
            {
                PerfMonLog("DBHelper", "AddInvalidEmail");
                try
                {
                    if (string.IsNullOrEmpty(cEmailAddress))
                        return false;
                    string cSQL = "Select EmailAddress FROM tblOptOutAddresses WHERE (EmailAddress = '" + cEmailAddress + "')";
                    if (!((ExeProcessSqlScalar(cSQL) ?? "") == (cEmailAddress ?? "")))
                    {
                        cSQL = "INSERT INTO tblOptOutAddresses (EmailAddress) VALUES ('" + cEmailAddress + "')";
                        ExeProcessSql(cSQL);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AddInvalidEmail", ex, ""));
                }

                return default;
            }
            public bool AddOptOutEmail(string cEmailAddress, string nContactKey, string cStatus)
            {
                PerfMonLog("DBHelper", "AddOptOutEmail");

                try
                {
                    if (string.IsNullOrEmpty(cEmailAddress))
                        return false;
                    string cSQL = "Select EmailAddress FROM tblOptOutAddresses WHERE (EmailAddress = '" + cEmailAddress + "')";
                    string cSQLStatusCheck = "Select top 1 nStatus FROM tblOptOutAddresses WHERE (EmailAddress = '" + cEmailAddress + "') order by dOptOut desc";
                    bool bstatus = Convert.ToBoolean(ExeProcessSqlScalar(cSQLStatusCheck));

                    if (cStatus == "true")
                    {
                        cSQL = "INSERT INTO tblOptOutAddresses (EmailAddress,nCartContactId,optout_reason,nStatus,dOptOut) VALUES ('" + cEmailAddress + "','" + nContactKey + "','Cart Opt Out','" + cStatus + "'," + SqlDate(DateTime.Now, true) + ")";
                        ExeProcessSql(cSQL);
                    }
                    else
                    {
                        if (((ExeProcessSqlScalar(cSQL) ?? "") == (cEmailAddress ?? "")))
                        {
                            if (bstatus)
                            {
                                cSQL = "INSERT INTO tblOptOutAddresses (EmailAddress,nCartContactId,optout_reason,nStatus,dOptOut) VALUES ('" + cEmailAddress + "','" + nContactKey + "','Cart Opt In','" + cStatus + "'," + SqlDate(DateTime.Now, true) + ")";
                                ExeProcessSql(cSQL);

                            }

                        }

                    }


                    if (cStatus == "true")
                    {
                        System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");
                        if (moMailConfig != null)
                        {
                            string sMessagingProvider = "";
                            if (moMailConfig != null)
                            {
                                sMessagingProvider = moMailConfig["MessagingProvider"];
                            }
                            if (!string.IsNullOrEmpty(sMessagingProvider))
                            {
                                Protean.Providers.Messaging.ReturnProvider RetProv = new Protean.Providers.Messaging.ReturnProvider();
                                IMessagingProvider oMessaging = RetProv.Get(ref myWeb, sMessagingProvider);
                                oMessaging.Activities.OptOutAll(cEmailAddress);
                                return true;
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AddOptOutEmail", ex, ""));
                }

                return default;
            }
            public void RemoveInvalidEmail(string cEmailAddressesCSV)
            {
                PerfMonLog("DBHelper", "RemoveInvalidEmail");
                try
                {
                    if (string.IsNullOrEmpty(cEmailAddressesCSV))
                        return;
                    cEmailAddressesCSV = "'" + cEmailAddressesCSV + "'";
                    cEmailAddressesCSV = Strings.Replace(cEmailAddressesCSV, ",", "','");
                    string cSQL = "DELETE FROM tblOptOutAddresses  WHERE (EmailAddress IN (" + cEmailAddressesCSV + "))";

                    ExeProcessSql(cSQL);
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AddInvalidEmail", ex, ""));
                }
            }

            public void ViewMailHistory(ref XmlElement oPageDetailElmt)
            {
                PerfMonLog("DBHelper", "ViewMailHistory");
                string cSQL = "";
                try
                {

                    cSQL = "SELECT tblActivityLog.nActivityKey, tblDirectory.cDirName, tblContentStructure.cStructName, tblActivityLog.dDateTime, tblActivityLog.cActivityDetail" + " FROM tblActivityLog INNER JOIN" + " tblDirectory ON tblActivityLog.nUserDirId = tblDirectory.nDirKey INNER JOIN" + " tblContentStructure ON tblActivityLog.nStructId = tblContentStructure.nStructKey" + " WHERE(tblActivityLog.nActivityType = " + ((int)ActivityType.NewsLetterSent).ToString() + ")" + " ORDER BY tblActivityLog.dDateTime DESC";
                    var oDs = GetDataSet(cSQL, "Activity", "ActivityLog");
                    cSQL = "SELECT cDirName, nDirKey FROM tblDirectory tDir WHERE (cDirSchema = N'Group')";
                    addTableToDataSet(ref oDs, cSQL, "Group");
                    oDs.Tables["Activity"].Columns["nActivityKey"].ColumnMapping = MappingType.Attribute;
                    oDs.Tables["Activity"].Columns["cDirName"].ColumnMapping = MappingType.Attribute;
                    oDs.Tables["Activity"].Columns["cStructName"].ColumnMapping = MappingType.Attribute;
                    oDs.Tables["Activity"].Columns["dDateTime"].ColumnMapping = MappingType.Attribute;
                    oDs.Tables["Activity"].Columns["cActivityDetail"].ColumnMapping = MappingType.Element;
                    oDs.Tables["Group"].Columns["cDirName"].ColumnMapping = MappingType.Attribute;
                    oDs.Tables["Group"].Columns["nDirKey"].ColumnMapping = MappingType.Attribute;

                    var oActivityElement = oPageDetailElmt.OwnerDocument.CreateElement("ActivityLog");
                    oActivityElement.InnerXml = Strings.Replace(Strings.Replace(oDs.GetXml(), "&gt;", ">"), "&lt;", "<");
                    // xmlDateTime
                    foreach (XmlElement odtElement in oActivityElement.SelectNodes("descendant-or-self::Activity"))
                        odtElement.SetAttribute("dDateTime", XmlDate(odtElement.GetAttribute("dDateTime"), true));
                    oPageDetailElmt.AppendChild(oActivityElement.FirstChild);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ViewMailHistory", ex, cSQL));
                }
            }

            public XmlElement ActivityReport(ActivityType sActivityType, long userDirId, long structId, long artId, long otherId)
            {
                PerfMonLog("DBHelper", "ViewMailHistory");
                string cSQL = "";
                try
                {
                    string cWhere = "";
                    if (otherId > 0L)
                    {
                        cWhere = " and nOtherId = " + otherId + " ";
                    }


                    cSQL = "SELECT tblActivityLog.nActivityKey, tblDirectory.cDirName, tblActivityLog.dDateTime, tblActivityLog.cActivityDetail" + " FROM tblActivityLog INNER JOIN" + " tblDirectory ON tblActivityLog.nUserDirId = tblDirectory.nDirKey " + " WHERE(tblActivityLog.nActivityType = " + ((int)sActivityType).ToString() + cWhere + ")" + " ORDER BY tblActivityLog.dDateTime DESC";

                    var oDs = GetDataSet(cSQL, "Activity", "ActivityLog");

                    oDs.Tables["Activity"].Columns["nActivityKey"].ColumnMapping = MappingType.Attribute;
                    oDs.Tables["Activity"].Columns["cDirName"].ColumnMapping = MappingType.Attribute;
                    oDs.Tables["Activity"].Columns["dDateTime"].ColumnMapping = MappingType.Attribute;
                    oDs.Tables["Activity"].Columns["cActivityDetail"].ColumnMapping = MappingType.Element;

                    var oActivityElement = myWeb.moPageXml.CreateElement("ActivityLog");
                    oActivityElement.InnerXml = Strings.Replace(Strings.Replace(oDs.GetXml(), "&gt;", ">"), "&lt;", "<");
                    // xmlDateTime
                    foreach (XmlElement odtElement in oActivityElement.SelectNodes("descendant-or-self::Activity"))
                        odtElement.SetAttribute("dDateTime", XmlDate(odtElement.GetAttribute("dDateTime"), true));
                    return (XmlElement)oActivityElement.FirstChild;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ViewMailHistory", ex, cSQL));
                    return null;
                }
            }

            public bool CheckOptOut(string nCheckAddress)
            {
                PerfMonLog("DBHelper", "CheckOptOut");
                try
                {
                    string cSQL;
                    if (!string.IsNullOrEmpty(nCheckAddress))
                    {
                        if (checkTableColumnExists("tblOptOutAddresses","status")) {
                        bool bReturn;
                        if (myWeb.moDbHelper.checkTableColumnExists("tblOptOutAddresses", "nStatus"))
                        {
                            cSQL = "SELECT top 1 EmailAddress FROM tblOptOutAddresses WHERE nStatus=1 and EmailAddress = '" + nCheckAddress + "' order by 1 desc";
                        }
                        else
                        {
                            cSQL = "SELECT EmailAddress FROM tblOptOutAddresses WHERE EmailAddress = '" + nCheckAddress + "'";
                        }

                        using (var oDRe = getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                        {
                            bReturn = oDRe.HasRows;
                            oDRe.Close();
                            return bReturn;
                        }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs("CheckOptOut", "", ex, ""));
                    return false;
                }
            }

            public XmlElement ListOptOuts()
            {
                PerfMonLog("DBHelper", "ListOptOuts");
                try
                {
                    var oElmt = moPageXml.CreateElement("Content");
                    oElmt.SetAttribute("type", "OptOut");
                    string cSQL = "SELECT EmailAddress FROM tblOptOutAddresses ORDER BY EmailAddress";
                    var oDS = GetDataSet(cSQL, "Email", "Addresses");
                    var oXML = new XmlDocument();
                    oXML.InnerXml = oDS.GetXml();
                    oElmt.SetAttribute("count", oDS.Tables["Addresses"].Rows.Count.ToString());
                    oElmt.InnerText = oXML.DocumentElement.InnerXml;
                    return oElmt;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs("ListOptOuts", "", ex, ""));
                    return null;
                }
            }

            private bool CheckIfAncestorPage(XmlDocument oPage, long nChildPageId, long nCheckPageId)
            {
                // If Not myWeb.PerfMon Is Nothing Then PerfMonLog("stdTools", "CheckIsParentPage")
                try
                {
                    // check if the pages have the same parent then return false
                    if (nChildPageId == 0L | nCheckPageId == 0L | nChildPageId == nCheckPageId)
                        return false;

                    // check if page exists on menu if return false
                    XmlElement oParentElmt;
                    oParentElmt = (XmlElement)oPage.SelectSingleNode("descendant-or-self::MenuItem[@id='" + nCheckPageId + "']");
                    if (oParentElmt is null)
                        return false;

                    // check if parent is ancestor of child then return true
                    XmlElement oChildElmt;
                    oChildElmt = (XmlElement)oParentElmt.SelectSingleNode("descendant-or-self::MenuItem[@id='" + nChildPageId + "']");
                    if (oChildElmt is null)
                        return false;
                    else
                        return true;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "CheckIsParentPage", ex, ""));
                }

                return default;
            }

            public string CleanAuditOrphans()
            {
                PerfMonLog("dbTools", "CleanAuditOrphans");
                try
                {
                    var oDs = new DataSet();
                    string cSQL;
                    DataRow oDr;
                    bool bHasChild = false;
                    int nNoDel = 0;

                    cSQL = "Select * from tblAudit";
                    oDs = getDataSetForUpdate(cSQL, "Audit", "CleanOrphans");
                    cSQL = "Select name from sysobjects where xtype='U' and type='U' and userstat=1";
                    addTableToDataSet(ref oDs, cSQL, "Tables");

                    // fill the dataset with all the used keys
                    foreach (DataRow currentODr in oDs.Tables["Tables"].Rows)
                    {
                        oDr = currentODr;
                        if (Conversions.ToBoolean(!Operators.ConditionalCompareObjectEqual(oDr["name"], "tblAudit", false)))
                        {
                            cSQL = Conversions.ToString(Operators.ConcatenateObject("Select nAuditId from ", oDr["name"]));
                            try
                            {
                                addTableToDataSet(ref oDs, cSQL, Conversions.ToString(oDr["name"]));
                                oDs.Relations.Add(Conversions.ToString(Operators.ConcatenateObject("Audit", oDr["name"])), oDs.Tables["Audit"].Columns["nAuditKey"], (DataColumn)oDs.Tables[Convert.ToInt32(oDr["name"])].Columns["nAuditId"], false);
                                oDs.Relations[Convert.ToInt32(Operators.ConcatenateObject("Audit", oDr["name"]))].Nested = true;
                            }
                            catch (Exception)
                            {
                                // clean the exception text as it is more than likely
                                // the error appears as the table does not bother with 
                                // auditing at all
                                // msException = ""
                            }
                        }
                    }

                    foreach (DataRow currentODr1 in oDs.Tables["Audit"].Rows)
                    {
                        oDr = currentODr1;
                        bHasChild = false;
                        foreach (DataRelation oDRel in oDs.Relations)
                        {
                            if (Information.UBound(oDr.GetChildRows(oDRel.RelationName)) == 0)
                            {
                                bHasChild = true;
                                break;
                            }
                        }
                        if (!bHasChild)
                        {
                            oDr.Delete();
                            nNoDel += 1;
                        }
                    }

                    if (nNoDel > 0)
                        updateDataset(ref oDs, "Audit", false);
                    string sResponse = "Audit table cleaned, " + nNoDel + " records removed.</br>";

                    // Clean Orphan RelatedContent.
                    cSQL = "Delete tblContentRelation  from tblContentRelation r left join tblContent c on r.nContentChildId = c.nContentKey where c.nContentKey is null";
                    sResponse += "Deleted Orphaned Child Content Relations - " + ExeProcessSql(cSQL) + "</br>";

                    cSQL = "Delete tblContentRelation  from tblContentRelation r left join tblContent c on r.nContentParentId = c.nContentKey where c.nContentKey is null";
                    sResponse += "Deleted Orphaned Parent Content Relations - " + ExeProcessSql(cSQL) + "</br>";

                    cSQL = "Delete tblContentLocation  from tblContentLocation l left join tblContent c on l.nContentId = c.nContentKey where c.nContentKey is null";
                    sResponse += "Deleted Orphaned Content Locations - " + ExeProcessSql(cSQL) + "</br>";


                    return sResponse;
                }



                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "Clean Audit Orphans", ex, ""));
                    return "Error cleaning Audit table";
                }
            }

            public string cleanLocations()
            {
                PerfMonLog("dbTools", "cleanLocations");
                string cSQL1;
                DataSet oDs;
                DataView oDV;
                DataRow oDr2;
                int nContentId;
                try
                {
                    cSQL1 = "Select * from tblContentLocation";
                    oDs = getDataSetForUpdate(cSQL1, "tblcontentLocation");
                    foreach (DataRow oDr1 in oDs.Tables["tblContentLocation"].Rows)
                    {
                        nContentId = Conversions.ToInteger(oDr1["nContentId"]);
                        oDV = new DataView(oDs.Tables["tblContentLocation"]);
                        oDV.RowFilter = "nContentId = " + nContentId;
                        oDV.Sort = "bPrimary DESC, nContentLocationKey";
                        int nI;
                        var loopTo = oDV.Count - 1;
                        for (nI = 0; nI <= loopTo; nI++)
                        {
                            oDr2 = oDV[nI].Row;
                            if (nI == 0)
                            {
                                oDr2["bPrimary"] = 1;
                            }
                            else
                            {
                                oDr2["bPrimary"] = 0;
                            }
                        }
                    }
                    updateDataset(ref oDs, "tblContentLocation");
                    return "Finished";
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "exeProcessSQLfromFile", ex, ""));
                    return "Error";
                }
            }

            public XmlNodeList LocateContentSearch(int nRootNode, string cSchemaName, string bChildren, string cSearchExpression, int nCurrentPage)
            {
                DataSet oDS;
                var oXML = new XmlDocument();
                XmlNodeList oResults;
                string cSQL;
                string cWhere = "";
                XmlNodeList oPages;
                PerfMonLog("DBHelper", "RelatedContentSearch");
                try
                {
                    string sSearch = cSearchExpression;
                    // remove reserved words
                    sSearch = Strings.Replace(" " + sSearch + " ", " the ", "");
                    sSearch = Strings.Replace(" " + sSearch + " ", " and ", "");
                    sSearch = Strings.Replace(" " + sSearch + " ", " if ", "");
                    sSearch = Strings.Replace(" " + sSearch + " ", " then ", "");
                    sSearch = Strings.Replace(" " + sSearch + " ", " or ", "");
                    sSearch = Strings.Trim(sSearch);
                    //int i = 0;

                    var oMenuElmt = myWeb.GetStructureXML();

                    cSQL = "SELECT nContentKey as id, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, tblContent.cContentXmlDetail as detail  FROM tblContent ";

                    if (nRootNode > 1)
                    {
                        cSQL += "INNER JOIN tblContentLocation ON tblContent.nContentKey = tblContentLocation.nContentId";
                        if (Conversions.ToBoolean(bChildren))
                        {
                            oPages = oMenuElmt.SelectNodes("descendant-or-self::MenuItem[@id='" + nRootNode + "']/descendant-or-self::MenuItem");
                            cWhere += " tblContentLocation.nStructId IN(";
                            int nPages = oPages.Count;
                            foreach (XmlElement oElmt in oPages)
                            {
                                cWhere += oElmt.GetAttribute("id");
                                nPages = nPages - 1;
                                if (nPages != 0)
                                {
                                    cWhere += ",";
                                }
                            }
                            cWhere += ") ";
                        }
                        else
                        {
                            cWhere += " tblContentLocation.nStructId=" + nRootNode;
                        }
                    }

                    // get only one type
                    if (!string.IsNullOrEmpty(cWhere))
                        cWhere += " AND ";
                    if (!string.IsNullOrEmpty(cSchemaName))
                        cWhere += " (tblContent.cContentSchemaName = '" + cSchemaName + "')";
                    if (!string.IsNullOrEmpty(cWhere))
                        cWhere += " AND ";
                    // now the search expression
                    cWhere += " ((cast(tblContent.cContentXmlBrief as varchar(max)) LIKE '%" + sSearch + "%') OR (cast(tblContent.cContentXmlDetail as varchar(max))LIKE '%" + sSearch + "%'))";
                    if (!string.IsNullOrEmpty(cWhere))
                        cWhere = " WHERE " + cWhere;

                    cSQL += cWhere + " ORDER BY cContentName";
                    oDS = GetDataSet(cSQL, "Content", "Contents");

                    foreach (DataTable oDT in oDS.Tables)
                    {
                        foreach (DataColumn oDC in oDT.Columns)
                            oDC.ColumnMapping = MappingType.Attribute;
                    }
                    oDS.Tables["Content"].Columns["content"].ColumnMapping = MappingType.Element;
                    oDS.Tables["Content"].Columns["detail"].ColumnMapping = MappingType.Element;

                    oXML.InnerXml = Strings.Replace(Strings.Replace(oDS.GetXml(), "&gt;", ">"), "&lt;", "<");
                    oResults = oXML.DocumentElement.SelectNodes("Content");
                    return oResults;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "LocateContentSearch", ex, ""));
                    return null;
                }
            }
            public long emailActivity(short nUserDirId, string cActivityFullDetail = "", string cEmailRecipient = "", string cEmailSender = "", string cActivityXml = "")
            {

                string sSql;
                try
                {
                    if (checkTableColumnExists("tblEmailActivityLog", "cActivityXml"))
                    {
                        sSql = "Insert Into tblEmailActivityLog (nUserDirId, dDateTime,  cEmailRecipient, cEmailSender, cActivityDetail, cActivityXml) " + "values (" + nUserDirId + ", " + SqlDate(DateTime.Now, true) + ", " + "'" + SqlFmt(Strings.Left(cEmailRecipient, 255)) + "', " + "'" + SqlFmt(Strings.Left(cEmailSender, 255)) + "', " + "'" + SqlFmt(cActivityFullDetail) + "', " + " '" + SqlFmt(cActivityXml) + "')";
                    }
                    else
                    {
                        sSql = "Insert Into tblEmailActivityLog (nUserDirId, dDateTime,  cEmailRecipient, cEmailSender, cActivityDetail) " + "values (" + nUserDirId + ", " + SqlDate(DateTime.Now, true) + ", " + "'" + SqlFmt(Strings.Left(cEmailRecipient, 255)) + "', " + "'" + SqlFmt(Strings.Left(cEmailSender, 255)) + "', " + "'" + SqlFmt(cActivityFullDetail) + "')";
                    }

                    return Conversions.ToLong(GetIdInsertSql(sSql));
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "emailActivity", ex, ""));
                    // Close()
                }

                return default;

            }

            public void RemoveDuplicateDirRelations()
            {
                try
                {
                    string sSQL = "SELECT nRelKey";
                    sSQL += " FROM tblDirectoryRelation";
                    sSQL += " WHERE (((SELECT COUNT(nRelKey) AS [COUNT]";
                    sSQL += " FROM tblDirectoryRelation Rel";
                    sSQL += " WHERE (nDirChildId = tblDirectoryRelation.nDirChildId) AND (nDirParentId = tblDirectoryRelation.ndirparentId) AND (nRelKey < tblDirectoryRelation.nrelkey))) > 0)";
                    using (var oDr = getDataReaderDisposable(sSQL))  // Done by nita on 6/7/22
                    {
                        while (oDr.Read())
                            DeleteObject(objectTypes.DirectoryRelation, Conversions.ToLong(oDr[0]));
                        oDr.Close();
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "RemoveDuplicateDirRelations", ex, ""));
                }
            }


            public bool CheckCode(string cCode, string cCodeSet = "")
            {

                string cSql = "";
                try
                {
                    string cCurDate = SqlDate(DateTime.Now, true);
                    cSql = " SELECT tblCodes.nCodeKey" + " FROM tblCodes INNER JOIN tblAudit ON tblCodes.nAuditId = tblAudit.nAuditKey" + " WHERE (tblCodes.cCode = '" + cCode + "')" + " AND (tblCodes.nUseId IS NULL OR tblCodes.nUseId = 0)" + " AND (tblAudit.dPublishDate <= " + cCurDate + " OR tblAudit.dPublishDate IS NULL)" + " AND (tblAudit.dExpireDate >= " + cCurDate + " OR tblAudit.dExpireDate IS NULL)" + " AND (tblAudit.nStatus = 1 OR tblAudit.nStatus = - 1 OR tblAudit.nStatus IS NULL)";
                    if (!string.IsNullOrEmpty(cCodeSet))
                    {
                        cSql += " AND nCodeParentId IN (" + cCodeSet + ")";
                    }
                    if (Convert.ToInt32(GetDataValue(cSql)) > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "CheckCode", ex, ""));
                    return false;
                }
            }

            public string IssueCode(string cCodeSet, string nUseId)
            {

                string cSql = "";
                string returnCode = "";

                try
                {
                    string cCurDate = SqlDate(DateTime.Now, true);
                    cSql = " SELECT top 1 tblCodes.cCode" + " FROM tblCodes INNER JOIN tblAudit ON tblCodes.nAuditId = tblAudit.nAuditKey" + " INNER JOIN tblCodes partlb ON tblCodes.nCodeParentId = partlb.nCodeKey" + " WHERE (tblCodes.nUseId IS NULL OR tblCodes.nUseId = 0)" + " AND (tblAudit.dPublishDate <= " + cCurDate + " OR tblAudit.dPublishDate IS NULL)" + " AND (tblAudit.dExpireDate >= " + cCurDate + " OR tblAudit.dExpireDate IS NULL)" + " AND (tblAudit.nStatus = 1 OR tblAudit.nStatus = - 1 OR tblAudit.nStatus IS NULL)";
                    cSql += " AND partlb.cCodeName like '" + cCodeSet + "'";

                    returnCode = Conversions.ToString(base.GetDataValue(cSql));

                    if (!string.IsNullOrEmpty(returnCode))
                    {
                        UseCode(returnCode, Conversions.ToInteger(nUseId));
                        return returnCode;
                    }
                    else
                    {
                        return "";
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "CheckCode", ex, ""));
                    return Conversions.ToString(false);
                }
            }

            public string IssueCode(int nCodeSet, int nUseId, bool UseNow = true, XmlElement CodeXml = null)
            {

                string cSql = "";
                string returnCode = "";

                try
                {
                    string cCurDate = SqlDate(DateTime.Now, true);
                    cSql = " SELECT top 1 tblCodes.cCode" + " FROM tblCodes INNER JOIN tblAudit ON tblCodes.nAuditId = tblAudit.nAuditKey" + " INNER JOIN tblCodes partlb ON tblCodes.nCodeParentId = partlb.nCodeKey" + " WHERE (tblCodes.nUseId IS NULL AND tblCodes.nIssuedDirId is NULL)" + " AND (tblAudit.dPublishDate <= " + cCurDate + " OR tblAudit.dPublishDate IS NULL)" + " AND (tblAudit.dExpireDate >= " + cCurDate + " OR tblAudit.dExpireDate IS NULL)" + " AND (tblAudit.nStatus = 1 OR tblAudit.nStatus = - 1 OR tblAudit.nStatus IS NULL)";
                    cSql += " AND partlb.nCodeKey = '" + nCodeSet + "'";

                    returnCode = Conversions.ToString(base.GetDataValue(cSql));
                    if (UseNow == false)
                    {
                        // If myWeb.mnUserId > 0 Then
                        int nKey = Conversions.ToInteger(base.GetDataValue(" SELECT tblCodes.nCodeKey" + " FROM tblCodes INNER JOIN tblAudit ON tblCodes.nAuditId = tblAudit.nAuditKey" + " WHERE (tblCodes.cCode = '" + returnCode + "')" + " AND (tblCodes.nUseId IS NULL OR tblCodes.nUseId = 0)" + " AND (tblAudit.dPublishDate <= " + cCurDate + " OR tblAudit.dPublishDate IS NULL)" + " AND (tblAudit.dExpireDate >= " + cCurDate + " OR tblAudit.dExpireDate IS NULL)" + " AND (tblAudit.nStatus = 1 OR tblAudit.nStatus = - 1 OR tblAudit.nStatus IS NULL)", CommandType.Text, null, 0));

                        // fix so that the cartitemkey is linked with tblCodes.nUseId
                        base.ExeProcessSql("UPDATE tblCodes SET nUseID = " + nUseId + ", nIssuedDirId = " + myWeb.mnUserId + ", dIssuedDate = " + cCurDate + " WHERE nCodeKey = " + nKey);

                        if (CodeXml != null)
                        {
                            ExeProcessSql("UPDATE tblCodes SET xUsageData = '" + SqlFmt(CodeXml.OuterXml) + "' WHERE nCodeKey = " + nKey);
                        }

                        return returnCode;
                    }
                    // Else
                    // Return ""
                    // If
                    else if (!string.IsNullOrEmpty(returnCode))
                    {
                        UseCode(returnCode, nUseId);
                        return returnCode;
                    }
                    else
                    {
                        return "";

                    }
                }



                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "CheckCode", ex, ""));
                    return Conversions.ToString(false);
                }
            }

            public bool UseCode(string cCode, int nUseID)
            {
                try
                {
                    // recheck the code
                    string cCurDate = SqlDate(DateTime.Now, true);
                    int nKey = Conversions.ToInteger(base.GetDataValue(" SELECT tblCodes.nCodeKey" + " FROM tblCodes INNER JOIN tblAudit ON tblCodes.nAuditId = tblAudit.nAuditKey" + " WHERE (tblCodes.cCode = '" + cCode + "')" + " AND (tblCodes.nUseId IS NULL OR tblCodes.nUseId = 0)" + " AND (tblAudit.dPublishDate <= " + cCurDate + " OR tblAudit.dPublishDate IS NULL)" + " AND (tblAudit.dExpireDate >= " + cCurDate + " OR tblAudit.dExpireDate IS NULL)" + " AND (tblAudit.nStatus = 1 OR tblAudit.nStatus = - 1 OR tblAudit.nStatus IS NULL)", CommandType.Text, null, (object)0));
                    if (nKey > 0)
                    {

                        ExeProcessSql("UPDATE tblCodes SET nUseID = " + nUseID + ", dUseDate = " + cCurDate + " WHERE nCodeKey = " + nKey);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "UseCode", ex, ""));
                    return false;
                }
            }

            public bool UseCode(int nCodeId, int nUseID, long nOrderId)
            {
                try
                {
                    // recheck the code
                    string cCurDate = SqlDate(DateTime.Now, true);
                    int nKey = Conversions.ToInteger(base.GetDataValue(" SELECT tblCodes.nCodeKey" + " FROM tblCodes INNER JOIN tblAudit ON tblCodes.nAuditId = tblAudit.nAuditKey" + " WHERE (tblCodes.nCodeKey = " + nCodeId + ")" + " AND (tblCodes.nUseId IS NULL OR tblCodes.nUseId = 0)" + " AND (tblAudit.dPublishDate <= " + cCurDate + " OR tblAudit.dPublishDate IS NULL)" + " AND (tblAudit.dExpireDate >= " + cCurDate + " OR tblAudit.dExpireDate IS NULL)" + " AND (tblAudit.nStatus = 1 OR tblAudit.nStatus = - 1 OR tblAudit.nStatus IS NULL)", CommandType.Text, null, (object)0));
                    if (nKey > 0)
                    {
                        // MyBase.ExeProcessSql("UPDATE tblCodes SET nUseID = " & nUseID & ", nOrderId = " & nOrderId & ", dUseDate = " & cCurDate & " WHERE nCodeKey = " & nKey)
                        // fix where the nOrderId was there previously but the field is not part of the tblCodes table
                        ExeProcessSql("UPDATE tblCodes SET nUseID = " + nUseID + ", dUseDate = " + cCurDate + " WHERE nCodeKey = " + nKey);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "UseCode", ex, ""));
                    return false;
                }
            }
            public bool RedeemCode(string cCode)
            {
                try
                {
                    // recheck the code
                    string cCurDate = SqlDate(DateTime.Now, true);
                    int nKey = Conversions.ToInteger(GetDataValue(" SELECT tblCodes.nCodeKey FROM tblCodes WHERE (tblCodes.cCode = '" + cCode + "')"));
                    if (nKey > 0)
                    {

                        ExeProcessSql("UPDATE tblCodes SET dUseDate = " + cCurDate + " WHERE nCodeKey = " + nKey);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "UseCode", ex, ""));
                    return false;
                }
            }

            public string DBN2Str(object Frm, bool useMarks = false, bool NullText = false)
            {
                // myWeb.PerfMon.Log("dbTools", "DBN2Str")
                string strNull;
                strNull = "";

                if (Frm is null)
                {
                    strNull = "Null";
                    goto ReturnMe;
                }
                if (NullText)
                    strNull = "Null";
                if (useMarks)
                    strNull = "''";
                ReturnMe:
                ;

                if (Frm is DBNull)
                    return strNull;
                else
                    return Conversions.ToString(Interaction.IIf(useMarks, "'" + Strings.Replace(Conversions.ToString(Frm), "'", "''") + "'", Conversions.ToString(Frm)));
            }
            public object DBN2int(object Frm, bool NullText = false)
            {
                // myWeb.PerfMon.Log("dbTools", "DBN2int")
                if (Frm is DBNull)
                    return Interaction.IIf(NullText, "Null", 0);
                else
                    return Conversions.ToInteger(Frm);
            }
            public object DBN2dte(object Frm, bool NullText = false)
            {
                // myWeb.PerfMon.Log("dbTools", "DBN2dte")
                if (Frm is DBNull)
                    return Interaction.IIf(NullText, "Null", DateTime.Parse("0001-01-01"));
                else
                    return Conversions.ToDate(Frm);
            }




            public DataSet getDataSetForUpdate(string sSql, string tableName, string datasetName = "", string cConn = "")
            {
                DataSet getDataSetForUpdateRet = default;
                // myWeb.PerfMon.Log("dbTools", "getDataSetForUpdate")
                DataSet oDs;
                string cProcessInfo = "Running SQL:  " + sSql;
                SqlConnection oConnection = null;
                try
                {

                    moDataAdpt = new SqlDataAdapter();
                    if (string.IsNullOrEmpty(datasetName))
                    {
                        oDs = new DataSet();
                    }
                    else
                    {
                        oDs = new DataSet(datasetName);
                    }

                    if (!string.IsNullOrEmpty(cConn))
                    {
                        oConnection = new SqlConnection(cConn);
                    }
                    else
                    {
                        oConnection = oConn;
                    }


                    if (oConnection.State == ConnectionState.Closed)
                        oConnection.Open();
                    var oSqlCmd = new SqlCommand(sSql, oConnection);
                    moDataAdpt.SelectCommand = oSqlCmd;
                    var cb = new SqlCommandBuilder(moDataAdpt);
                    moDataAdpt.TableMappings.Add(tableName, tableName);

                    moDataAdpt.Fill(oDs, tableName);
                    moDataAdpt.Update(oDs, tableName);
                    getDataSetForUpdateRet = oDs;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getDataSetForUpdate", ex, cProcessInfo));
                    return null;
                }

                return getDataSetForUpdateRet;

            }



            public bool updateDataset(ref DataSet oDs, string sTableName, bool bReUse = false)
            {
                // myWeb.PerfMon.Log("dbTools", "updateDataset")
                string cProcessInfo = "returnDataSet";

                try
                {
                    var oxx = new SqlCommandBuilder(moDataAdpt);
                    moDataAdpt.DeleteCommand = oxx.GetDeleteCommand();
                    moDataAdpt.InsertCommand = oxx.GetInsertCommand();
                    moDataAdpt.UpdateCommand = oxx.GetUpdateCommand();
                    moDataAdpt.Update(oDs, sTableName);

                    if (!bReUse)
                    {
                        // lets tidy up
                        moDataAdpt = null;
                        oDs.Clear();
                        oDs = null;
                    }
                }


                catch (Exception ex)
                {
                    //XmlDocument oXml = new XmlDataDocument(oDs);
                    XmlDocument oXml = new XmlDocument();
                    if (oDs.Tables[0].Rows.Count > 0)
                    {
                        oXml.LoadXml(oDs.GetXml());
                    }
                    cProcessInfo = oXml.OuterXml;
                    // Return False
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getDataSet", ex, cProcessInfo));
                }

                return default;

            }

            public long saveInstance(ref XmlElement instanceElmt, string targetTable, string keyField, string whereStmt = "")
            {

                PerfMonLog("dbTools", "saveInstance");

                // Generic function to save xml to a database, picking only the relevent fields out of the XML

                string keyValue = null;
                string sSql;
                string cProcessInfo = "AddRStoXMLnode";
                long nUpdateCount;
                DataRow oRow;
                DataColumn column;
                var oDs = new DataSet();
                SqlDataAdapter oDataAdpt = null;
                try
                {
                    if (oConn.State == ConnectionState.Closed)
                        oConn.Open();

                    // Identify the keyValue and build the initial SQL Statement.
                    if (string.IsNullOrEmpty(whereStmt))
                    {
                        if (instanceElmt.SelectSingleNode("*/" + keyField) != null)
                        {
                            keyValue = instanceElmt.SelectSingleNode("*/" + keyField).InnerText;
                            if (string.IsNullOrEmpty(keyValue))
                                keyValue = "-1";
                        }
                        else
                        {
                            keyValue = "-1";
                        }
                        sSql = "select * from " + targetTable + " where " + keyField + " = " + keyValue;
                    }
                    else
                    {
                        sSql = "select * from " + targetTable + " where " + whereStmt;
                    }

                    cProcessInfo = "error running SQL: " + sSql;

                    oDataAdpt = new SqlDataAdapter(sSql, oConn);

                    // autogenerate commands
                    var cmdBuilder = new SqlCommandBuilder(oDataAdpt);
                    oDataAdpt.Fill(oDs, targetTable);

                    if (oDs.Tables[targetTable].Rows.Count > 0) // CASE FOR UPDATE
                    {
                        oRow = oDs.Tables[targetTable].Rows[0];
                        oRow.BeginEdit();
                        foreach (DataColumn currentColumn in oDs.Tables[targetTable].Columns)
                        {
                            column = currentColumn;
                            cProcessInfo = targetTable + " Update: ";
                            if (instanceElmt.SelectSingleNode("*/" + column.ToString()) != null)
                            {
                                cProcessInfo += column.ToString() + " - " + instanceElmt.SelectSingleNode("*/" + column.ToString()).InnerXml;
                                // 14/05/19 ts remed out as recent change was preventing updates.
                                if (!(column.AllowDBNull & instanceElmt.SelectSingleNode("*/" + column.ToString()) is null))
                                {
                                    oRow[column] = convertDtXMLtoSQL(column.DataType, instanceElmt.SelectSingleNode("*/" + column.ToString()), Conversions.ToBoolean(Interaction.IIf(Strings.InStr(column.ToString(), "Xml") > 0, true, false)));
                                }
                            }
                        }
                        oRow.EndEdit();

                        // run the update
                        nUpdateCount = oDataAdpt.Update(oDs, targetTable);
                    }

                    else // CASE FOR INSERT
                    {
                        oRow = oDs.Tables[targetTable].NewRow();
                        foreach (DataColumn currentColumn1 in oDs.Tables[targetTable].Columns)
                        {
                            column = currentColumn1;

                            if (instanceElmt.SelectSingleNode("*/" + column.ToString()) != null)
                            {
                                // don't want to set the value on the key feild on insert
                                cProcessInfo = targetTable + " Insert: ";
                                if (!((column.ToString() ?? "") == (keyField ?? "")))
                                {
                                    cProcessInfo += column.ToString() + " - " + instanceElmt.SelectSingleNode("*/" + column.ToString()).InnerXml;

                                    //convertDtXMLtoSQL(column.DataType, instanceElmt.SelectSingleNode("*/" & column.ToString).InnerXml, IIf(InStr(column.ToString, "Xml") > 0, True, False))
                                    oRow[column] = convertDtXMLtoSQL(column.DataType, instanceElmt.SelectSingleNode("*/" + column), Strings.InStr(column.ToString(), "Xml") > 0 ? true : false);
                                }
                            }
                        }
                        cProcessInfo = targetTable + " Add Rows";
                        oDs.Tables[targetTable].Rows.Add(oRow);

                        // Run the insert and get back the new id
                        cProcessInfo = targetTable + " Get ID";
                        string ExecuteQuery = "SELECT IDENT_CURRENT('" + targetTable + "')";
                        var getid = new SqlCommand(ExecuteQuery, oConn);
                        // cProcessInfo = targetTable & " Get ID: nUpdateCount : " & oDs.GetXml
                        nUpdateCount = oDataAdpt.Update(oDs, targetTable);
                        // cProcessInfo = targetTable & " Get ID: ExecuteScalar"
                        keyValue = Conversions.ToString(getid.ExecuteScalar());
                        cProcessInfo = targetTable + " ID Retrieved: " + keyValue;
                    }

                    if (nUpdateCount == 0L)
                    {
                        Information.Err().Raise(1000, mcModuleName, "No Update");
                    }

                    PerfMonLog("dbTools", "saveInstance-End", cProcessInfo);
                    return Conversions.ToLong(keyValue);
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "saveInstance", ex, cProcessInfo));
                }
                finally
                {
                    oDs.Dispose();
                    oDs = null;
                    if (oDataAdpt != null)
                    {
                        oDataAdpt.Dispose();
                    }
                    oDataAdpt = null;
                    oConn.Close();
                }

                return default;

            }
            public int updateInstanceField(objectTypes targetTable, int keyValue, string fieldName, string value, string whereStmt = "")
            {
                string cProcessInfo = "";
                var oInstance = myWeb.moPageXml.CreateElement("Instance");

                try
                {

                    oInstance.InnerXml = getObjectInstance(targetTable, keyValue, whereStmt);
                    if (value.StartsWith("<") & value.EndsWith(">"))
                    {
                        oInstance.SelectSingleNode("*/" + fieldName).InnerXml = value;
                    }
                    else
                    {
                        oInstance.SelectSingleNode("*/" + fieldName).InnerText = value;
                    }
                    setObjectInstance(targetTable, oInstance, (long)keyValue);
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "updateInstanceField", ex, cProcessInfo));
                }

                return default;

            }

            public new Hashtable getHashTable(string sSql, string sNameField, ref string sValueField)
            {
                // myWeb.PerfMon.Log("dbTools", "getHashTable")
                string cProcessInfo = "";
                try
                {

                    return base.getHashTable(sSql, sNameField, ref sValueField);
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getDataSet", ex, cProcessInfo));
                    return null;
                }

            }

            public DataSet getDatasetAddRows(string[] sSQL, string cTableName, string cDatasetName = "")
            {
                // myWeb.PerfMon.Log("dbTools", "getDatasetAddRows")
                // Creates a Dataset and a datatable
                // with specified names
                // runs multiple sql fills to add rows
                // to the table
                if (sSQL is null)
                    return null;
                string cProcessInfo = "";
                try
                {
                    string cSQL;
                    int nI;
                    var oDs = new DataSet();


                    SqlDataAdapter oDA;

                    var loopTo = sSQL.Count();
                    for (nI = 0; nI <= loopTo; nI++)
                    {
                        cSQL = Convert.ToString(sSQL[nI]);
                        cProcessInfo = "Running SQL:  " + cSQL;
                        oDA = new SqlDataAdapter(cSQL, oConn);
                        if (!string.IsNullOrEmpty(cDatasetName))
                            oDs.DataSetName = cDatasetName;
                        oDA.Fill(oDs, cTableName);
                    }
                    return oDs;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetDatasetAddRows", ex, cProcessInfo));
                    return null;
                }
            }

            public object convertDtXMLtoSQL(Type datatype, XmlNode value, bool bKeepXml = true)
            {
                // myWeb.PerfMon.Log("dbTools", "convertDtXMLtoSQL")
                string cProcessInfo = "Converting Datatype:  " + datatype.Name;
                try
                {
                    switch (datatype.Name ?? "")
                    {
                        case "Boolean":
                            {
                                if (value.InnerText == "true" || value.InnerText == "True" || value.InnerText == "1")
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        case "DateTime":
                            {
                                if (Information.IsDate(value.InnerText) & !value.InnerText.ToString().StartsWith("0001-01-01"))
                                {
                                    return Convert.ToDateTime(value.InnerText);
                                }
                                else
                                {
                                    return DBNull.Value;
                                }
                            }
                        case "Double":
                        case "Int32":
                        case "Int16":
                        case "Decimal":
                            {
                                if (Information.IsNumeric(value.InnerText))
                                {
                                    return Convert.ToDouble(value.InnerText);
                                }
                                else
                                {
                                    return DBNull.Value;
                                }
                            }
                        case "String":
                            {

                                if (Strings.Left(Strings.Trim(value.InnerXml.ToString()), 1) == "<" & Strings.Right(Strings.Trim(value.InnerXml.ToString()), 1) == ">")
                                {
                                    // we can assume this is XML
                                    bKeepXml = true;
                                }

                                if (bKeepXml)
                                {
                                    return value.InnerXml;
                                }
                                else
                                {
                                    return convertEntitiesToString(Convert.ToString(value.InnerXml));
                                }
                            }

                        case "Xml":
                            {
                                //byte[] bytes = Encoding.UTF8.GetBytes(Convert.ToString(value.InnerXml));
                                //return Encoding.Unicode.GetString(bytes);

                                return value.InnerXml;
                            }

                        default:
                            {
                                return value.InnerXml;
                            }
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "convertDtXMLtoSQL", ex, cProcessInfo));
                    return null;
                }
            }

            public string convertDtSQLtoXML(Type datatype, object value)
            {
                // myWeb.PerfMon.Log("dbTools", "convertDtSQLtoXML")
                string cProcessInfo = "Converting Datatype:  " + datatype.Name;
                try
                {
                    if (value is DBNull)
                    {
                        return "";
                    }
                    else
                    {
                        switch (datatype.Name ?? "")
                        {
                            case "Boolean":
                                {
                                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(value, true, false)))
                                    {
                                        return "true";
                                    }
                                    else
                                    {
                                        return "false";
                                    }
                                }
                            case "DateTime":
                                {
                                    if (Information.IsDate(value) & !(value is DBNull))
                                    {
                                        return XmlDate(value);
                                    }
                                    else
                                    {
                                        return "";
                                    }
                                }
                            case "Double":
                            case "Int32":
                            case "Int16":
                                {
                                    if (Information.IsNumeric(value))
                                    {
                                        return Conversions.ToString(value);
                                    }
                                    else
                                    {
                                        return "";
                                    }
                                }
                            case "DBNull":
                                {
                                    return "";
                                }

                            default:
                                {
                                    return Conversions.ToString(value);
                                }
                        }
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "convertDtSQLtoXML", ex, cProcessInfo));
                    return null;
                }
            }

            public bool createDB(string DatabaseName)
            {
                //string cProcessInfo = "createDB";
                try
                {
                    System.Collections.Specialized.NameValueCollection oConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");
                    var myConn = new SqlConnection("Data Source=" + oConfig["DatabaseServer"] + "; Initial Catalog=master;" + GetDBAuth());
                    string sSql;
                    oConn = myConn;

                    sSql = "select db_id('" + DatabaseName + "')";
                    if (ExeProcessSqlScalar(sSql) is null)
                    {
                        ExeProcessSql("CREATE DATABASE " + DatabaseName);
                        oConn = null;
                        return true;
                    }
                    else
                    {
                        oConn = null;
                        return true;
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "createDB", ex, ""));
                    return default;
                }
            }

            public bool createDB(string DatabaseName, string DatabaseServer, string DatabaseUsername, string DatabasePassword)
            {
                //string cProcessInfo = "createDB";
                try
                {
                    // Dim oConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/web")
                    var myConn = new SqlConnection("Data Source=" + DatabaseServer + "; Initial Catalog=master;" + "user id=" + DatabaseUsername + "; password=" + DatabasePassword);
                    string sSql;
                    oConn = myConn;

                    sSql = "select db_id('" + DatabaseName + "')";
                    if (ExeProcessSqlScalar(sSql) is null)
                    {
                        ExeProcessSql("CREATE DATABASE " + DatabaseName);
                        oConn = null;
                        return true;
                    }
                    else
                    {
                        oConn = null;
                        return true;
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "createDB", ex, ""));
                    return false;
                }
            }

            public bool isClonedPage(int nPageId)
            {

                PerfMonLog("DBHelper", "isClonedPage");

                string cProcessInfo = "";
                try
                {
                    cProcessInfo = "Page ID: " + nPageId;
                    return getClonePageID(nPageId) > 0;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "isClonedPage", ex, cProcessInfo));
                    return false;
                }
            }

            public int getClonePageID(int nPageId)
            {

                PerfMonLog("DBHelper", "getClonePageID");

                string cProcessInfo = "";
                try
                {
                    cProcessInfo = "Page ID: " + nPageId;

                    return Conversions.ToInteger(GetDataValue("SELECT nCloneStructId FROM tblContentStructure WHERE nStructKey = " + nPageId, CommandType.Text, null, (object)0));
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getClonePageID", ex, cProcessInfo));
                    return 0;
                }
            }

            public Hashtable getUnorphanedAncestor(int nParentContentId, long nLevel = 99L)
            {

                PerfMonLog("DBHelper", "getUnorphanedAncestor");

                string cProcessInfo = "";
                Hashtable hReturn = null;
                long nPageId;
                string cSql = "";
                try
                {
                    cProcessInfo = "Parent Content ID: " + nParentContentId;

                    // Arbitrarily stop recursion
                    if (nLevel < 15L)
                    {

                        // Get the primary location of the parent content
                        // Assumes that there will only be one parent id.
                        cSql = "SELECT	TOP 1 nStructId " + "FROM tblContentLocation " + "WHERE	(nContentId = " + SqlFmt(nParentContentId.ToString()) + ") AND (bPrimary = 1) ";

                        nPageId = Conversions.ToLong(GetDataValue(cSql));

                        // If the page retrieved is null (0) then this is orphan content
                        // Let's see if there's any other parents to this content
                        // Assume that the content has 0 or 1 related parents
                        if (nPageId == 0L)
                        {

                            cSql = "SELECT TOP 1 nContentParentId " + "FROM dbo.tblContentRelation " + "WHERE nContentChildId = " + SqlFmt(nParentContentId.ToString());

                            nParentContentId = Conversions.ToInteger(GetDataValue(cSql));

                            // If there is a parent related to the content then try to see if that's got a location
                            if (nParentContentId > 0)
                            {
                                hReturn = getUnorphanedAncestor(nParentContentId, nLevel + 1L);
                            }
                        }

                        else
                        {

                            // This is not orphaned content, so this is what we want to find
                            // Return the request type of info
                            hReturn = new Hashtable();
                            hReturn.Add("id", nParentContentId);
                            hReturn.Add("page", nPageId);

                        }

                    }

                    return hReturn;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getUnorphanedAncestor", ex, cProcessInfo));
                    return hReturn;
                }


            }

            public XmlElement GetMostPopularSearches(int numberToReturn, string filterStartString)
            {
                PerfMonLog("dbHelper", "GetMostPopularSearches");

                XmlElement result = null;

                try
                {
                    var popularSearches = myWeb.moPageXml.CreateElement("popularSearches");

                    string filter = "";

                    if (!string.IsNullOrEmpty(filterStartString))
                    {
                        // Create the predictive filter, with wildcards taken out
                        filter = " AND cActivityDetail LIKE '" + Regex.Replace(SqlFmt(filterStartString), @"[%\*]", "") + "%'";
                    }

                    if (!(numberToReturn > 0))
                        numberToReturn = 5;

                    string popularSearchesQuery = "SELECT TOP " + numberToReturn + " cActivityDetail " + "FROM dbo.tblActivityLog " + "WHERE nActivityType=" + ((int)ActivityType.Search).ToString() + " " + filter + " " + "GROUP BY cActivityDetail " + "HAVING SUM(nOtherId) > 0 " + "ORDER BY COUNT(*) DESC, SUM(nOtherId) DESC ";






                    using (var query = getDataReaderDisposable(popularSearchesQuery))  // Done by nita on 6/7/22
                    {

                        while (query.Read())
                        {
                            result = myWeb.moPageXml.CreateElement("search");
                            result.InnerText = query[0].ToString();
                            popularSearches.AppendChild(result);
                        }

                        query.Close();

                        return popularSearches;
                    }
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "GetMostPopularSearches", ex, "", "", gbDebug);
                    return null;
                }
            }

            public int savePayment(int CartId, long nUserId, string cProviderName, string cProviderRef, string cMethodName, XmlElement oDetailXML, DateTime dExpire, bool bUserSaved, double nAmountPaid, string paymentType = "full")
            {
                string cSQL = "";
                string cRes = "";

                long nPaymentMethodKey = -1;
                if (oDetailXML != null)
                {
                    oDetailXML.SetAttribute("AmountPaid", nAmountPaid.ToString());
                }
                try
                {
                    if (bUserSaved)
                    {
                        cSQL = "SELECT tblCartPaymentMethod.nPayMthdKey FROM tblCartPaymentMethod INNER JOIN tblAudit ON tblCartPaymentMethod.nAuditId = tblAudit.nAuditKey" + " WHERE (tblAudit.dExpireDate = " + SqlDate(dExpire) + ") and (tblCartPaymentMethod.cPayMthdAcctName = '" + cMethodName + "') and (cPayMthdProviderName = '" + cProviderName + "') and (tblCartPaymentMethod.nPayMthdUserId = " + nUserId + ")";
                        cRes = ExeProcessSqlScalar(cSQL);
                        if (Information.IsNumeric(cRes))
                        {
                            if (Conversions.ToInteger(cRes) > 0)
                            {
                                dExpire = DateTime.Now;
                            }
                        }
                    }

                    // check if we allready have a payment method for this order if so we overwrite
                    // TS Disabled Sept 21 as we might have multiple payment methods per order with new deposit functionality.
                    // cSQL = "Select nPayMthdId from tblCartOrder WHERE nCartOrderKey = " & CartId
                    // cRes = ExeProcessSqlScalar(cSQL)
                    // If IsNumeric(cRes) Then
                    // nPaymentMethodKey = CLng(cRes)
                    // End If

                    // mask the credit card number
                    if (oDetailXML != null)
                    {
                        XmlElement oCcNum = (XmlElement)oDetailXML.SelectSingleNode("number");
                        if (oCcNum != null)
                        {
                            oCcNum.InnerText = MaskString(oCcNum.InnerText, "*", false, 4);
                        }

                        // mask CV2 digits
                        XmlElement oCV2 = (XmlElement)oDetailXML.SelectSingleNode("CV2");
                        if (oCV2 != null)
                        {
                            oCV2.InnerText = "";
                        }
                    }

                    var oXml = new XmlDocument();
                    var oInstance = oXml.CreateElement("Instance");
                    var oElmt = oXml.CreateElement("tblCartPaymentMethod");
                    XmlNode argoNode = oElmt;
                    addNewTextNode("nPayMthdUserId", ref argoNode, nUserId.ToString());
                    oElmt = (XmlElement)argoNode;
                    XmlNode argoNode1 = oElmt;
                    addNewTextNode("cPayMthdProviderName", ref argoNode1, cProviderName);
                    oElmt = (XmlElement)argoNode1;
                    XmlNode argoNode2 = oElmt;
                    addNewTextNode("cPayMthdProviderRef", ref argoNode2, cProviderRef);
                    oElmt = (XmlElement)argoNode2;
                    XmlNode argoNode3 = oElmt;
                    addNewTextNode("cPayMthdAcctName", ref argoNode3, cMethodName);
                    oElmt = (XmlElement)argoNode3;
                    XmlNode argoNode4 = oElmt;
                    var oElmt2 = addNewTextNode("cPayMthdDetailXml", ref argoNode4);
                    oElmt = (XmlElement)argoNode4;
                    if (oDetailXML != null)
                    {
                        oElmt2.InnerXml = oDetailXML.OuterXml;
                    }

                    // addNewTextNode("dPayMthdExpire", oElmt, xmlDate(dExpire))

                    int nPaymentId;
                    oInstance.AppendChild(oElmt);
                    if (!(nPaymentMethodKey > 0L))
                    {
                        int nAudit = getAuditId(0, (long)myWeb.mnUserId, "Payment", DateTime.Now, dExpire, DateTime.Now, DateTime.Now);

                        XmlNode argoNode5 = oElmt;
                        addNewTextNode("nAuditId", ref argoNode5, nAudit.ToString());
                        oElmt = (XmlElement)argoNode5;
                        nPaymentId = Conversions.ToInteger(setObjectInstance(objectTypes.CartPaymentMethod, oInstance, nPaymentMethodKey));
                    }
                    else
                    {
                        nPaymentId = Conversions.ToInteger(setObjectInstance(objectTypes.CartPaymentMethod, oInstance, nPaymentMethodKey));
                        cSQL = "Select nAuditId from tblCartPaymentMethod where nPayMthdKey = " + nPaymentId;
                        int nAuditId = Conversions.ToInteger(ExeProcessSqlScalar(cSQL));
                        oInstance.RemoveAll();
                        oElmt = oXml.CreateElement("tblAudit");
                        XmlNode argoNode6 = oElmt;
                        addNewTextNode("nAuditKey", ref argoNode6, nAuditId.ToString());
                        oElmt = (XmlElement)argoNode6;
                        XmlNode argoNode7 = oElmt;
                        addNewTextNode("dExpireDate", ref argoNode7, XmlDate(dExpire));
                        oElmt = (XmlElement)argoNode7;
                        XmlNode argoNode8 = oElmt;
                        addNewTextNode("dUpdateDate", ref argoNode8, XmlDate(DateTime.Now));
                        oElmt = (XmlElement)argoNode8;
                        XmlNode argoNode9 = oElmt;
                        Tools.Xml.addNewTextNode("nUpdateDirId", ref argoNode9, myWeb.mnUserId.ToString());
                        oElmt = (XmlElement)argoNode9;
                        XmlNode argoNode10 = oElmt;
                        Tools.Xml.addNewTextNode("nInsertDirId", ref argoNode10, myWeb.mnUserId.ToString());
                        oElmt = (XmlElement)argoNode10; // 
                        XmlNode argoNode11 = oElmt;
                        addNewTextNode("nStatus", ref argoNode11, 1.ToString());
                        oElmt = (XmlElement)argoNode11;
                        oInstance.AppendChild(oElmt);
                        nPaymentId = Conversions.ToInteger(setObjectInstance(objectTypes.CartPaymentMethod, oInstance, nPaymentMethodKey));
                    }

                    CartPaymentMethod(CartId, nPaymentId);

                    string argsTableName = "tblCartPayment";
                    if (doesTableExist(ref argsTableName))
                    {
                        oInstance.RemoveAll();
                        oElmt = oXml.CreateElement("tblCartPayment");
                        XmlNode argoNode12 = oElmt;
                        addNewTextNode("nCartOrderId", ref argoNode12, CartId.ToString());
                        oElmt = (XmlElement)argoNode12;
                        XmlNode argoNode13 = oElmt;
                        addNewTextNode("nCartPaymentMethodId", ref argoNode13, nPaymentId.ToString());
                        oElmt = (XmlElement)argoNode13;
                        if (paymentType == "full")
                        {
                            XmlNode argoNode14 = oElmt;
                            addNewTextNode("bFull", ref argoNode14, "true");
                            oElmt = (XmlElement)argoNode14;
                        }
                        if (paymentType == "deposit")
                        {
                            XmlNode argoNode15 = oElmt;
                            addNewTextNode("bPart", ref argoNode15, "true");
                            oElmt = (XmlElement)argoNode15;
                        }
                        if (paymentType == "settlement")
                        {
                            XmlNode argoNode16 = oElmt;
                            addNewTextNode("bSettlement", ref argoNode16, "true");
                            oElmt = (XmlElement)argoNode16;
                        }
                        XmlNode argoNode17 = oElmt;
                        addNewTextNode("nPaymentAmount", ref argoNode17, nAmountPaid.ToString());
                        oElmt = (XmlElement)argoNode17;
                        // addNewTextNode("dInsertDate", oElmt, Protean.Tools.Xml.XmlDate(Now))
                        // addNewTextNode("dUpdateDate", oElmt, Protean.Tools.Xml.XmlDate(Now))
                        // addNewTextNode("nInsertDirId", oElmt, myWeb.mnUserId) '
                        XmlNode argoNode18 = oElmt;
                        addNewTextNode("nStatus", ref argoNode18, 1.ToString());
                        oElmt = (XmlElement)argoNode18;
                        XmlNode argoNode19 = oElmt;
                        addNewTextNode("nAuditId", ref argoNode19, getAuditId().ToString());
                        oElmt = (XmlElement)argoNode19;
                        oInstance.AppendChild(oElmt);
                        setObjectInstance(objectTypes.CartPayment, oInstance);
                    }

                    return nPaymentId;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "savePayment", ex, "", "", gbDebug);
                    return 0;
                }
            }

            public void CartPaymentMethod(int CartId, int PaymentId)
            {
                try
                {
                    string cSQL = "UPDATE tblCartOrder SET nPayMthdId = " + PaymentId + " WHERE nCartOrderKey = " + CartId;
                    ExeProcessSql(cSQL);
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "CartPaymentMethod", ex, "", "", gbDebug);
                }
            }

            public void SaveCartStatus(int CartId, int StatusId)
            {
                try
                {
                    string cSQL = "UPDATE tblCartOrder SET nCartStatus = " + StatusId + " WHERE nCartOrderKey = " + CartId;
                    ExeProcessSql(cSQL);
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "SaveCartStatus", ex, "", "", gbDebug);
                }
            }

            public void UpdateSellerNotes(long CartId, string TransactionDetails)
            {
                string sSql = "";
                DataSet oDs;
                string cProcessInfo = "UpdateSellerNotes";
                try
                {

                    // Update Seller Notes:
                    sSql = "select * from tblCartOrder where nCartOrderKey = " + CartId;
                    oDs = getDataSetForUpdate(sSql, "Order", "Cart");
                    foreach (DataRow oRow in oDs.Tables["Order"].Rows)
                        oRow["cSellerNotes"] = Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(oRow["cSellerNotes"], Constants.vbLf), DateTime.Today), " "), DateAndTime.TimeOfDay), ": changed to: (Order Placed)"), Constants.vbLf), Constants.vbLf), TransactionDetails);


                    updateDataset(ref oDs, "Order");
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "UpdateSellerNotes", ex, "", cProcessInfo, gbDebug);
                }

            }

            public void SetClientNotes(long CartId, string Notes)
            {
                string sSql = "";
                DataSet oDs;
                string cProcessInfo = "SetClientNotes";
                try
                {

                    // Update Seller Notes:
                    sSql = "select * from tblCartOrder where nCartOrderKey = " + CartId;
                    oDs = getDataSetForUpdate(sSql, "Order", "Cart");
                    foreach (DataRow oRow in oDs.Tables["Order"].Rows)
                        oRow["cClientNotes"] = Notes;
                    updateDataset(ref oDs, "Order");
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "UpdateSellerNotes", ex, "", cProcessInfo, gbDebug);
                }

            }


            public void UpdateSellerNotes(long CartId, string Status, string Notes)
            {
                string sSql = "";
                DataSet oDs;
                string cProcessInfo = "SetClientNotes";
                try
                {

                    // Update Seller Notes:
                    sSql = "select * from tblCartOrder where nCartOrderKey = " + CartId;
                    oDs = getDataSetForUpdate(sSql, "Order", "Cart");
                    foreach (DataRow oRow in oDs.Tables["Order"].Rows)
                        oRow["cSellerNotes"] = Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(oRow["cSellerNotes"], @"\n"), DateTime.Today), " "), DateTime.Now.TimeOfDay.ToString()), ": changed to: ("), Status), ") "), @"\n"), "comment: "), Notes), @"\n");
                    updateDataset(ref oDs, "Order");
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "UpdateSellerNotes", ex, "", cProcessInfo, gbDebug);
                }

            }


            public void ListReports(ref XmlElement oContentsXML)
            {
                PerfMonLog("Cart", "ListReports");

                XmlElement listElement;
                XmlElement reportElement;
                string reportName = "";
                DirectoryInfo dir;
                FileInfo[] files;
                var foldersToCheck = new List<string>();
                try
                {

                    foldersToCheck.Add("/");
                    foldersToCheck.AddRange(myWeb.maCommonFolders);

                    listElement = moPageXml.CreateElement("List");


                    foreach (string folder in foldersToCheck)
                    {

                        string reportsFolder = "/xforms/Reports";
                        if (myWeb.bs5)
                            reportsFolder = "/admin/xforms/reports";
                        dir = new DirectoryInfo(myWeb.moCtx.Server.MapPath(folder) + reportsFolder);
                        if (dir.Exists)
                        {
                            files = dir.GetFiles("*.xml");

                            foreach (FileInfo reportFile in files)
                            {

                                reportName = reportFile.Name.Substring(0, reportFile.Name.LastIndexOf("."));

                                // Check if the report has already been added (which suggest that local one has been found)
                                if (listElement.SelectSingleNode("Report[@type='" + reportName + "']") is null)
                                {

                                    // Add the report
                                    XmlNode argoNode = listElement;
                                    reportElement = addNewTextNode("Report", ref argoNode, reportName.Replace("-", " "));
                                    listElement = (XmlElement)argoNode;
                                    reportElement.SetAttribute("type", reportName);

                                }

                            }
                        }

                    }

                    oContentsXML.AppendChild(listElement);
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "ListReports", ex, "", "", gbDebug);
                }
            }



            public void GetReport(ref XmlElement oContentsXML, ref XmlElement QueryXml)
            {
                PerfMonLog("Cart", "ListReports");
                string processInfo = "";
                string storedProcedure;
                DataSet ds;
                string reportName;
                var reportElmt = oContentsXML.OwnerDocument.CreateElement("Report");

                var @params = new Hashtable();
                string paramValue = "";
                string paramName = "";

                string logDetail = "";
                string outputFormat = "";

                try
                {
                    // Gather variables
                    storedProcedure = QueryXml.GetAttribute("storedProcedure");
                    reportName = QueryXml.GetAttribute("name");
                    outputFormat = QueryXml.GetAttribute("output");
                    logDetail = QueryXml.GetAttribute("logActivityDetail");
                    // If nothing has been specific for the detail field, use the SP name
                    if (string.IsNullOrEmpty(logDetail))
                        logDetail = storedProcedure;

                    // Build the query parameters
                    processInfo = "Building Parameters";
                    foreach (XmlElement param in QueryXml.SelectNodes("param[@name!='']"))
                    {
                        paramValue = param.GetAttribute("value");
                        paramName = param.GetAttribute("name");

                        // Assume empty values are NULL values - SQl can handle this.
                        if (!string.IsNullOrEmpty(paramValue) & !@params.ContainsKey(paramName))
                        {
                            if (param.GetAttribute("type") == "datetime")
                            {
                                // paramValue = Replace(SqlDate(paramValue, True), "'", "")'
                                @params.Add(paramName, Conversions.ToDate(Strings.Replace(SqlDate(paramValue, false), "'", "")));
                            }
                            else
                            {
                                @params.Add(paramName, paramValue);
                            }
                        }

                        else
                        {
                            switch (param.GetAttribute("type") ?? "")
                            {
                                case "number":
                                    {
                                        @params.Add(paramName, 0);
                                        break;
                                    }
                                case "string":
                                    {
                                        @params.Add(paramName, "");
                                        break;
                                    }
                            }

                        }
                    }

                    // Run the data
                    processInfo = "Populated Parameter Count = " + @params.Count.ToString();
                    string spWithBespokeCheck = getDBObjectNameWithBespokeCheck(storedProcedure);
                    processInfo += "; Running SP - " + spWithBespokeCheck;
                    ConnectTimeout = 180;
                    ds = GetDataSet(spWithBespokeCheck, "Item", "Report", parameters: @params, querytype: CommandType.StoredProcedure);

                    // Convert the dataset to Xml
                    var reportXml = new XmlDocument();
                    myWeb.moDbHelper.ReturnNullsEmpty(ref ds);
                    reportXml.LoadXml(ds.GetXml());

                    // Process the data for type-specific columns
                    processInfo += "; Updating Column data";
                    foreach (DataColumn column in ds.Tables["Item"].Columns)
                    {

                        // Pick up whether this is XML - by name and not by type (which will have already converted)
                        if (column.ColumnName.ToLower().Contains("xml"))
                        {
                            foreach (XmlElement dataItem in reportXml.SelectNodes("Report/Item/" + column.ColumnName))
                            {
                                XmlElement xmldataItem = dataItem;
                                SetInnerXmlThenInnerText(ref xmldataItem, dataItem.InnerText);
                            }
                        }

                        // Add metadata and format data based on type.
                        switch (column.DataType.ToString() ?? "")
                        {
                            case "System.DateTime":
                                {
                                    var dateNodes = reportXml.SelectNodes("Report/Item/" + column.ColumnName);

                                    if (dateNodes.Count > 0)
                                    {
                                        // Add a datatype info to the first item
                                        XmlElement firstDate = (XmlElement)dateNodes.Item(0);
                                        firstDate.SetAttribute("datatype", "date");

                                        // Convert other items to xml date format
                                        foreach (XmlElement dataItem in dateNodes)
                                            dataItem.InnerText = XmlDate(dataItem.InnerText.ToString(), true);
                                    }

                                    break;
                                }
                        }
                    }


                    // Organise data for contents node
                    reportXml.PreserveWhitespace = false;
                    reportElmt.SetAttribute("name", reportName);
                    reportElmt.SetAttribute("format", outputFormat);

                    foreach (XmlElement param in QueryXml.SelectNodes("param[@name!='']"))
                        reportElmt.SetAttribute("p-" + param.GetAttribute("name"), param.GetAttribute("value"));

                    reportElmt.InnerXml = reportXml.DocumentElement.InnerXml;
                    reportElmt.AppendChild(QueryXml.CloneNode(true));
                    oContentsXML.AppendChild(reportElmt);


                    // Update the filename
                    if (!string.IsNullOrEmpty(outputFormat) && outputFormat != "rawxml" && outputFormat != "html")
                    {
                        processInfo += "; Updating filename";
                        var filename = new List<string>();
                        if (!string.IsNullOrEmpty(QueryXml.GetAttribute("filePrefix")))
                            filename.Add(QueryXml.GetAttribute("filePrefix"));

                        if (QueryXml.GetAttribute("fileExcludeReportName").ToLower() != "true")
                        {
                            filename.Add(reportName);
                        }

                        switch (QueryXml.GetAttribute("fileUID").ToLower() ?? "")
                        {
                            case "log":
                                {
                                    // This is a sequential number based on the download activity log for this report.
                                    int logCount = Conversions.ToInteger(GetDataValue("SELECT COUNT(*) FROM dbo.tblActivityLog WHERE cActivityDetail=" + SqlString(logDetail) + " AND nActivityType = " + ((int)ActivityType.ReportDownloaded).ToString()));
                                    logCount += 1;
                                    filename.Add(logCount.ToString("00000"));
                                    break;
                                }
                            case "random":
                            case "guid":
                                {
                                    filename.Add(Guid.NewGuid().ToString()); // "timestamp"
                                    break;
                                }

                            default:
                                {
                                    filename.Add(DateTime.Now.ToString("yyyyMMddhhmmss"));
                                    break;
                                }
                        }

                        if (!myWeb.mbOutputXml)
                        {
                            myWeb.mcContentDisposition = "attachment;filename=" + string.Join("-", filename.ToArray()) + "." + outputFormat;
                        }

                    }


                    // Finally, log activity if it has been requested.
                    if (QueryXml.GetAttribute("logActivity") == "true")
                    {
                        processInfo += "; Logging activity";


                        logActivity(ActivityType.ReportDownloaded, (long)myWeb.mnUserId, 0L, 0L, logDetail);
                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "GetReport", ex, "", processInfo, gbDebug);
                }
            }

            public new bool checkTableColumnExists(string tableName, string columnName)
            {

                bool columnExists = false;
                string colState = "";
                try
                {
                    // this saves the result into the application object, this result will never change for an active website.

                    // Check for empty strings.
                    if (!(string.IsNullOrEmpty(tableName) | string.IsNullOrEmpty(columnName)))
                    {
                        if (myWeb is null)
                        {
                            // case is web is not instatiated and we have not context, such as when running from a webservice.
                            columnExists = base.checkTableColumnExists(tableName, columnName);
                        }
                        else
                        {
                            var oApp = myWeb.moCtx.Application;
                            colState = Conversions.ToString(oApp[tableName + "-" + columnName]);
                            switch (colState ?? "")
                            {
                                case "0":
                                    {
                                        columnExists = false;
                                        break;
                                    }
                                case "1":
                                    {
                                        columnExists = true;
                                        break;
                                    }

                                case var @case when @case == "":
                                    {
                                        if (base.checkTableColumnExists(tableName, columnName))
                                        {
                                            oApp[tableName + "-" + columnName] = (object)1;
                                            columnExists = true;
                                        }
                                        else
                                        {
                                            oApp[tableName + "-" + columnName] = (object)0;
                                            columnExists = false;
                                        }

                                        break;
                                    }
                            }
                        }

                    }

                    return columnExists;
                }

                catch (Exception)
                {
                    return false;
                }
            }

            public bool isUnique(string tableName, string schemaName, string columnName, string xPath, string ValueToTest)
            {

                bool bUnique = false;

                try
                {
                    string sSQL = "";

                    string itemId = myWeb.moRequest["id"];

                    if (tableName == "tblContent")
                    {
                        sSQL = "Select count(nContentKey) from tblContent where cContentSchemaName='" + SqlFmt(schemaName) + "' ";

                        string columnFilter = "";
                        switch (columnName ?? "")
                        {
                            case "cContentName":
                                {
                                    // cContentName is compared for uniqueness by trimming/removing all spaces (including internal). 
                                    // Ex. following query will return match for comparison of "product name" with "  product     name"
                                    columnFilter = "and replace(replace(replace(replace(lower(rtrim(ltrim(cContentName))),' ','<>'),'><',''),'<>',' '), ' ', '-') = " + "replace(replace(replace(replace(lower(rtrim(ltrim('" + ValueToTest + "'))),' ','<>'),'><',''),'<>',' '), ' ', '-')";
                                    break;
                                }
                            case "cContentXmlBrief":
                                {
                                    columnFilter = "and cast(tblContent.cContentXmlBrief as xml).value('" + xPath + "', 'nvarchar(255)') = '" + ValueToTest + "'";
                                    break;
                                }

                            default:
                                {
                                    break;
                                }
                        }
                        sSQL = sSQL + columnFilter;

                        if (!string.IsNullOrEmpty(itemId))
                        {
                            sSQL = sSQL + " and nContentKey != " + itemId;
                        }

                        long recordCount = Conversions.ToLong(ExeProcessSqlScalar(sSQL));
                        if (recordCount == 0L)
                            bUnique = true;
                    }

                    return bUnique;
                }

                catch (Exception)
                {
                    return false;
                }
            }


            /// <summary>
            /// This function is to allow content to be added to the page that blocks the automatic loading of a particular contentType, because that content has its own rules for selecting and loading that content type.
            /// </summary>
            /// <param name="nPageId"></param>
            /// <returns></returns>
            public string GetPageBlockedContent(long nPageId)
            {
                string returnValue = "";
                try
                {
                    if (checkTableColumnExists("tblContent", "cBlockedSchemaType"))
                    {
                        string sSQL = "select cBlockedSchemaType, nContentKey from tblContent c inner join tblContentLocation cl on c.nContentKey = cl.nContentId where nStructId = " + nPageId + " and cBlockedSchemaType IS NOT NULL";
                        var oDs = GetDataSet(sSQL, "ContentBlock", "Block");
                        foreach (DataRow oRow in oDs.Tables["ContentBlock"].Rows)
                        {
                            if (!(oRow["cBlockedSchemaType"] is DBNull))
                            {

                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(oRow["cBlockedSchemaType"], "", false)))
                                {
                                    if (!string.IsNullOrEmpty(returnValue))
                                    {
                                        returnValue = returnValue + ",";
                                    }
                                    returnValue = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(returnValue, oRow["cBlockedSchemaType"]), "|"), oRow["nContentKey"]));
                                }
                            }
                        }
                    }
                    return returnValue;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "CheckIsParentPage", ex, ""));
                    return null;
                }

            }

            public DataTable GetContacts(int nSupplierId, int nDirId)
            {
                PerfMonLog("dbTools", "GetContacts");
                string sSql;
                DataSet oDs;
                try
                {
                    sSql = "select c.* from tblCartContact c " + "where c.cContactForeignRef = 'SUP-" + nSupplierId.ToString() + "' " + "and nContactDirId = " + nDirId;
                    oDs = getDataSetForUpdate(sSql, "tblCartContact");
                    return oDs.Tables[0];
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "exeProcessSQLfromFile", ex, ""));
                    return null;
                }
            }

            public bool CheckDuplicateOrder(string cPayMthdProviderRef)
            {
                PerfMonLog("dbTools", "CheckDuplicateOrder");
                string sSql;
                bool bIsDuplicate = false;
                // Dim oDr As SqlDataReader
                try
                {
                    sSql = "select Count(nPayMthdKey) from tblCartPaymentMethod where cPayMthdProviderRef= '" + cPayMthdProviderRef + "'";
                    using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {
                        if (oDr.Read())
                        {
                            bIsDuplicate = Convert.ToInt32(oDr[0]) > 0;
                        }

                        return bIsDuplicate;
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "CheckDuplicateOrder", ex, ""));
                    return default;
                }
            }

            public int SetContact(ref Cms.modal.Contact contact)
            {
                if (contact.nContactKey > 0)
                {
                    UpdateContact(ref contact);
                }
                else
                {
                    AddContact(ref contact);
                }

                return default;
            }

            public int AddContact(ref Cms.modal.Contact contact)
            {
                PerfMonLog("DBHelper", "AddContact ([args])");
                string sSql;
                string nId;
                string cProcessInfo = "";
                try
                {
                    sSql = "INSERT INTO [dbo].[tblCartContact] ([nContactDirId], [nContactCartId], [cContactType], [cContactName], [cContactCompany]," + " [cContactAddress], [cContactCity], [cContactState], [cContactZip], [cContactCountry], [cContactTel], [cContactFax], [cContactEmail]," + " [cContactXml], [nAuditId], [cContactForiegnRef], [nLat], [nLong], [cContactForeignRef], [cContactAddress2])" + " VALUES (" + contact.nContactDirId + "," + contact.nContactCartId + "" + ",'" + Tools.Database.SqlFmt(contact.cContactType) + "'" + ",'" + Tools.Database.SqlFmt(contact.cContactName) + "'" + ",'" + Tools.Database.SqlFmt(contact.cContactCompany) + "'" + ",'" + Tools.Database.SqlFmt(contact.cContactAddress) + "'" + ",'" + Tools.Database.SqlFmt(contact.cContactCity) + "'" + ",'" + Tools.Database.SqlFmt(contact.cContactState) + "'" + ",'" + Tools.Database.SqlFmt(contact.cContactZip) + "'" + ",'" + Tools.Database.SqlFmt(contact.cContactCountry) + "'" + ",'" + Tools.Database.SqlFmt(contact.cContactTel) + "'" + ",'" + Tools.Database.SqlFmt(contact.cContactFax) + "'" + ",'" + Tools.Database.SqlFmt(contact.cContactEmail) + "'" + ",'<Content><LocationSummary>" + Tools.Database.SqlFmt(contact.cContactLocationSummary) + "</LocationSummary></Content>'" + "," + getAuditId() + ",'" + Tools.Database.SqlFmt(contact.cContactForiegnRef) + "'" + ",'" + contact.nLat + "'" + ",'" + contact.nLong + "'" + ",'" + Tools.Database.SqlFmt(contact.cContactForeignRef) + "'" + ",'" + Tools.Database.SqlFmt(contact.cContactAddress2) + "')";

                    nId = GetIdInsertSql(sSql);

                    if (nId == "0")
                    {

                        throw new Exception("Address not saved");

                    }

                    return Conversions.ToInteger(nId);
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AddContact", ex, cProcessInfo));
                    return Conversions.ToInteger(false);
                }
            }

            public bool UpdateContact(ref Cms.modal.Contact contact)
            {
                PerfMonLog("DBHelper", "UpdateContact ([args])");
                string sSql;
                string cProcessInfo = "";
                try
                {
                    if (contact.nContactKey == 0)
                    {
                        contact.nContactKey = (int)myWeb.moDbHelper.getObjectByRef(objectTypes.CartContact, contact.cContactForiegnRef, "");
                    }
                    //sSql = "UPDATE [dbo].[tblCartContact]" + "SET [cContactName] = '" + Tools.Database.SqlFmt(contact.cContactName) + "'" + ", [cContactAddress] = '" + Tools.Database.SqlFmt(contact.cContactAddress) + "'" + ", [cContactAddress2] = '" + Tools.Database.SqlFmt(contact.cContactAddress2) + "'" + ", [cContactCity] = '" + Tools.Database.SqlFmt(contact.cContactCity) + "'" + ", [cContactState] = '" + Tools.Database.SqlFmt(contact.cContactState) + "'" + ", [cContactZip] = '" + Tools.Database.SqlFmt(contact.cContactZip) + "'" + ", [cContactCountry] = '" + Tools.Database.SqlFmt(contact.cContactCountry) + "'" + ", [cContactTel] = '" + Tools.Database.SqlFmt(contact.cContactTel) + "'" + ", [cContactFax] = '" + Tools.Database.SqlFmt(contact.cContactFax) + "'" + ", [cContactXml] = '<Content><LocationSummary>" + Tools.Database.SqlFmt(contact.cContactLocationSummary) + "</LocationSummary></Content>'" + "WHERE [nContactKey] = " + contact.nContactKey;
                    sSql = "UPDATE [dbo].[tblCartContact]" + "SET [cContactName] = '" + Tools.Database.SqlFmt(contact.cContactName) + "'" + ", [cContactAddress] = '" + Tools.Database.SqlFmt(contact.cContactAddress) + "'" + ", [cContactCity] = '" + Tools.Database.SqlFmt(contact.cContactCity) + "'" + ", [cContactState] = '" + Tools.Database.SqlFmt(contact.cContactState) + "'" + ", [cContactZip] = '" + Tools.Database.SqlFmt(contact.cContactZip) + "'" + ", [cContactCountry] = '" + Tools.Database.SqlFmt(contact.cContactCountry) + "'" + ", [cContactTel] = '" + Tools.Database.SqlFmt(contact.cContactTel) + "'" + ", [cContactFax] = '" + Tools.Database.SqlFmt(contact.cContactFax) + "'" + ", [cContactXml] = '<Content><LocationSummary>" + Tools.Database.SqlFmt(contact.cContactLocationSummary) + "</LocationSummary></Content>'" + "WHERE [nContactKey] = " + contact.nContactKey;

                    ExeProcessSql(sSql);
                    return true;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "UpdateContact", ex, cProcessInfo));
                    return false;
                }
            }

            public bool DeleteContact(ref int nContactKey)
            {
                PerfMonLog("DBHelper", "DeleteContact ([args])");
                try
                {
                    if (nContactKey == 0)
                    {
                        throw new ArgumentException("Invalid nContactKey");
                    }
                    myWeb.moDbHelper.DeleteObject(objectTypes.CartContact, (long)nContactKey);
                    return true;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "DeleteContact", ex, string.Empty));
                    return false;
                }
            }

            public string GetCountryISOCode(string sCountry)
            {
                // Dim oDr As SqlDataReader
                string sSql;
                string strReturn = "";

                try
                {
                    sSql = "select cLocationISOa2 from tblCartShippingLocations where cLocationNameFull Like '" + sCountry + "' or cLocationNameShort Like '" + sCountry + "'";
                    using (var oDr = getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {

                        if (oDr.HasRows)
                        {

                            while (oDr.Read())
                                strReturn = oDr["cLocationISOa2"].ToString();
                        }
                        else
                        {
                            strReturn = "";
                        }

                    }
                    return strReturn;
                }
                catch (Exception)
                {
                    return null;
                }
            }

            // Public Function GetContentListByPageFilter(ByVal sPageIds As String) As SqlDataReader
            // Dim oDr As SqlDataReader
            // Dim sSql As String
            // Dim strReturn As String = ""

            // Try
            // Dim params As New Hashtable
            // params.Add("@PageIds", sPageIds)
            // ' sSql = "spGetContentListByPageFilter"
            // oDr = myWeb.moDbHelper.getDataReader(sSql, CommandType.StoredProcedure, params)
            // Return oDr
            // Catch ex As Exception
            // Return Nothing
            // End Try

            // End Function

            #region Deprecated Functions
            public bool doesTableExist(ref string sTableName)
            {
                try
                {

                    return checkDBObjectExists(sTableName, Tools.Database.objectTypes.Table);
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "doesTableExist", ex, "Checking: " + sTableName));
                    return false;
                }

            }
            public bool TableExists(string cTableName)
            {
                string cProcessInfo = "TableExists: " + cTableName;
                try
                {
                    return checkDBObjectExists(cTableName, Tools.Database.objectTypes.Table);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "doesTableExist", ex, cProcessInfo));
                    return false;
                }
            }
            #endregion

            public string getContentIdFromOrder(string orderRef, string ContentName)
            {
                // Dim oDr As SqlDataReader
                string sSql;
                string nContentID = string.Empty;

                string cProcessInfo = "";
                try
                {
                    sSql = "execute spGetContentIdFromOrderReference @orderRef=" + orderRef + ", @ProductName=" + "'" + ContentName + "'";
                    using (SqlDataReader oDr = myWeb.moDbHelper.getDataReaderDisposable(sSql))
                    {
                        if (oDr != null)
                        {
                            while (oDr.Read())
                            {
                                nContentID = Convert.ToString(oDr["nItemId"]);
                            }
                        }
                    }
                    return nContentID.ToString();
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "UpdateContact", ex, cProcessInfo));
                    return nContentID;
                }
            }

            public XmlElement GetMenuMetaTitleDescriptionDetailsXml(XmlElement oMenuElmt)
            {
                string menuid = string.Empty;
                string PageId = string.Empty;
                string PageTitle = string.Empty;
                string MetaDescription = string.Empty;
                string cContentName = string.Empty;
                string sSql = string.Empty;
                DataSet oDs;
                try
                {
                    sSql = "EXEC spGetAllMenusList";
                    // Get the dataset
                    //get all site detailed DS and then loop
                    oDs = GetDataSet(sSql, "Content");
                    if (oDs.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow oRow2 in oDs.Tables[0].Rows)
                        {
                            PageId = Convert.ToString(oRow2["parId"]);
                            foreach (XmlElement oMenuItem in oMenuElmt.SelectNodes($"descendant-or-self::MenuItem[@id='{PageId}']"))
                            {
                                XmlElement pagetitle = moPageXml.CreateElement("PageTitle");
                                XmlElement metadescription = moPageXml.CreateElement("MetaDescription");
                                cContentName = Convert.ToString(oRow2["cContentName"]);
                                if (cContentName == "PageTitle")
                                {
                                    PageTitle = Convert.ToString(oRow2["cContentXmlBrief"]);
                                    pagetitle.InnerXml = PageTitle;
                                    pagetitle.SetAttribute("id", Convert.ToString(oRow2["nContentid"]));
                                    oMenuItem.AppendChild(pagetitle);
                                }
                                if (cContentName == "MetaDescription")
                                {
                                    MetaDescription = Convert.ToString(oRow2["cContentXmlBrief"]);
                                    metadescription.InnerXml = MetaDescription;
                                    metadescription.SetAttribute("id", Convert.ToString(oRow2["nContentid"]));
                                    oMenuItem.AppendChild(metadescription);
                                }
                            }
                        }
                    }
                    return oMenuElmt;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "GetMenuMetaTitleDescriptionDetailsXml", ex, "", "", gbDebug);
                    return null;
                }
            }
            
        }


    }
}