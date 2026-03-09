using Protean.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Protean.Cms.dbHelper;
using static Protean.stdTools;
using static Protean.Tools.Xml;

namespace Protean
{
    public partial class Cms
    {

        // Inherits dbTools
        public partial class dbHelper
        {
            public partial class utils
            {
                public class APILog
                {
                    Protean.Cms.model.APILog apiLog;
                    dbHelper myDbh;
                    Boolean isActive = false;

                    public long nAPILogKey { 
                        get {return apiLog.nAPILogKey;}
                        set { apiLog.nAPILogKey = value; }
                    }

                    public long nUserId
                    {
                        get { return apiLog.nUserId; }
                        set { apiLog.nUserId = value; }
                    }
                    public DateTime dRequestDateTime
                    {
                        get { return apiLog.dRequestDateTime; }
                        set { apiLog.dRequestDateTime = value; }
                    }
                    public long dResponseTimeDiff
                    {
                        get { return apiLog.dResponseTimeDiff; }
                        set { apiLog.dResponseTimeDiff = value; }
                    }
                    public string cRequestedUrl
                    {
                        get { return apiLog.cRequestedUrl; }
                        set { apiLog.cRequestedUrl = value; }
                    }
                    public string cMethodName
                    {
                        get { return apiLog.cMethodName; }
                        set { apiLog.cMethodName = value; }
                    }
                    public string cPayLoad
                    {
                        get { return apiLog.cPayLoad; }
                        set { apiLog.cPayLoad = value; }
                    }
                    public string cResponseData
                    {
                        get { return apiLog.cResponseData; }
                        set { apiLog.cResponseData = value; }
                    }
                    public string cResponseType
                    {
                        get { return apiLog.cResponseType; }
                        set { apiLog.cResponseType = value; }
                    }
                    public string cRequestType
                    {
                        get { return apiLog.cRequestType; }
                        set { apiLog.cRequestType = value; }
                    }

                    public string cSourceIP
                    {
                        get { return apiLog.cSourceIP; }
                        set { apiLog.cSourceIP = value; }
                    }
                    public string cUserAgent
                    {
                        get { return apiLog.cUserAgent; }
                        set { apiLog.cUserAgent = value; }
                    }

                    public APILog(dbHelper myDbh)
                    {
                        apiLog = new Protean.Cms.model.APILog();
                        this.myDbh = myDbh;
                        if (Convert.ToBoolean(myDbh.myWeb.goApp["apilog"]) == true)
                        {
                            isActive = true;
                        }
                        else
                        {
                            if (myDbh.TableExists("tblAPILog") == true)
                            {
                                myDbh.myWeb.goApp["apilog"] = true;
                                isActive = true;
                            }
                            else
                            {
                                isActive = false;
                            }
                        }
                    }


                    public void Add()
                    {
                        myDbh.PerfMonLog("DBHelper", "AddAPILog ([args])");
                        string sSql;
                        string nId;
                        string cProcessInfo = "";
                        try
                        {
                            if (isActive)
                            {
                                if (!string.IsNullOrEmpty(apiLog.cPayLoad))
                                {
                                    apiLog.cPayLoad = SqlFmt(apiLog.cPayLoad);
                                }
                                sSql = String.Format("INSERT INTO [dbo].[tblAPILog] ([nUserId],[dRequestDateTime],[cRequestedUrl],[cMethodName],[cPayLoad],[cRequestType],[cSourceIP],[cUserAgent],[cResponseData],[cResponseType]) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}')", apiLog.nUserId, apiLog.dRequestDateTime, apiLog.cRequestedUrl, apiLog.cMethodName, apiLog.cPayLoad, apiLog.cRequestType, apiLog.cSourceIP, apiLog.cUserAgent, apiLog.cResponseData, apiLog.cResponseType);

                                nId = myDbh.GetIdInsertSql(sSql);

                                if (nId == "0")
                                {

                                    throw new Exception("Api log not saved");

                                }
                                apiLog.nAPILogKey= Convert.ToInt64(nId);
                                //return Convert.ToInt64(nId);
                            }
                            //else {
                            //   // return 0;
                            //}
                        }

                        catch (Exception ex)
                        {
                            myDbh.OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AddAPILog", ex, cProcessInfo));
                            //return 0;
                        }
                    }

                    public bool Update()
                    {
                        myDbh.PerfMonLog("DBHelper", "UpdateContact ([args])");
                        string sSql;
                        string cProcessInfo = "";
                        try
                        {
                            if (isActive)
                            {
                                if (apiLog.nAPILogKey != 0)
                                {

                                    //sSql = "UPDATE [dbo].[tblAPILog]" + "SET [cResponseData] = '" + Tools.Database.SqlFmt(apiLog.cResponseData) + "', [cResponseType] = '" + Tools.Database.SqlFmt(apiLog.cResponseType) + "', [dResponseDateTime] = '" + apiLog.dResponseDateTime + "' WHERE [nAPILogKey] = " + Convert.ToString(apiLog.nAPILogKey);
                                    sSql = "UpdateAPILog";
                                    var arrParms = new System.Collections.Hashtable();
                                  

                                    arrParms.Add("@APILogKey", apiLog.nAPILogKey);
                                    arrParms.Add("@ResponseData", apiLog.cResponseData);
                                    arrParms.Add("@ResponseType", apiLog.cResponseType);
                                    myDbh.ExeProcessSql(sSql,System.Data.CommandType.StoredProcedure, arrParms);

                                  //  myDbh.ExeProcessSql(sSql);
                                    return true;
                                }
                                else
                                {
                                    throw new Exception("Invalid APILog Key");

                                }
                            }
                            else
                            {
                                return false;
                            }
                        }

                        catch (Exception ex)
                        {
                            myDbh.OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "UpdateAPILog", ex, cProcessInfo));
                            return false;
                        }
                    }
                }
            }
        }
    }
}
