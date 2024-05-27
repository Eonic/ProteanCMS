// ***********************************************************************
// $Library:     Protean.Providers.messaging.base
// $Revision:    3.1  
// $Date:        2010-03-02
// $Author:      Trevor Spink (trevor@eonic.co.uk)
// &Website:     www.eonic.co.uk
// &Licence:     All Rights Reserved.
// $Copyright:   Copyright (c) 2002 - 2010 Eonic Ltd.
// ***********************************************************************

using System;
using System.Collections;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web.Configuration;

using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using static Protean.Cms;
using static Protean.stdTools;
using Protean.Tools;
using System.Dynamic;
using Protean.Providers.Payment;
using Protean.Providers.Messaging;
using System.Data;
using System.Web.UI.WebControls;

namespace Protean.Providers
{
    namespace Database
    {

        public interface IDatabaseProvider
        {
            IDatabaseProvider Initiate(ref Cms myWeb);
            Boolean valid { get; set; }
            string ConnStr { get; set; }

            DataSet GetDataSet(string sSql, string tableName);
        }

        public class ReturnProvider
        {
            private const string mcModuleName = "Protean.Providers.Database.ReturnProvider";

            public IDatabaseProvider Get(ref Cms myWeb, string ProviderName)
            {
                string cProgressInfo = "";
                try
                {
                    Type calledType;
                    if (string.IsNullOrEmpty(ProviderName))
                    {
                        ProviderName = "Protean.Providers.IDatabaseProvider.DefaultProvider";
                        calledType = Type.GetType(ProviderName, true);
                    }
                    else
                    {
                        var castObject = WebConfigurationManager.GetWebApplicationSection("protean/databaseProviders");
                        Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)castObject;
                        System.Configuration.ProviderSettings ourProvider = moPrvConfig.Providers[ProviderName];
                        Assembly assemblyInstance;
                        // = [Assembly].Load(moPrvConfig.Providers(ProviderName).Type)

                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(ourProvider.Parameters["path"], "", false)))
                        {
                            cProgressInfo = goServer.MapPath(Conversions.ToString(ourProvider.Parameters["path"]));
                            assemblyInstance = Assembly.LoadFrom(goServer.MapPath(Conversions.ToString(ourProvider.Parameters["path"])));
                        }
                        else
                        {
                            assemblyInstance = Assembly.Load(ourProvider.Type);
                        }

                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(ourProvider.Parameters["className"], "", false)))
                        {
                            ProviderName = Conversions.ToString(ourProvider.Parameters["className"]);
                        }

                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(ourProvider.Parameters["rootClass"], "", false)))
                        {
                            calledType = assemblyInstance.GetType("Protean.Providers.Database." + ProviderName, true);
                        }
                        else
                        {
                            // calledType = assemblyInstance.GetType(ourProvider.parameters("rootClass") & ".Providers.Messaging", True)
                            calledType = assemblyInstance.GetType(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(ourProvider.Parameters["rootClass"], ".Providers.Database."), ProviderName)), true);
                        }
                    }

                    var o = Activator.CreateInstance(calledType);

                    var args = new object[1];
                    args[0] = myWeb;

                    return (IDatabaseProvider)calledType.InvokeMember("Initiate", BindingFlags.InvokeMethod, null, o, args);
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "New", ex, vstrFurtherInfo: cProgressInfo + " - " + ProviderName + " Could Not be Loaded", bDebug: gbDebug);
                    return null;
                }
            }
        }


        public class DefaultProvider : IDatabaseProvider
        {
            public DefaultProvider()
            {

            }

            private Boolean bValid;

            public Boolean valid
            {
                set
                {
                    bValid = value;
                }
                get
                {
                    return bValid;
                }
            }
            private string _ConnStr;

            public string ConnStr
            {
                set
                {
                    _ConnStr = value;
                }
                get
                {
                    return _ConnStr;
                }
            }
            // do nothing
            public IDatabaseProvider Initiate(ref Cms myWeb)
            {

                return this;
            }


            public DataSet GetDataSet(string sSql, string tableName)
            {

                return null;
            }
        }
    }
}