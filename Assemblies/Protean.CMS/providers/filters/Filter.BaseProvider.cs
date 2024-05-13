// ***********************************************************************
// $Library:     Protean.Providers.Filter.BaseProvider
// $Revision:    3.1  
// $Date:        2010-03-02
// $Author:      Trevor Spink (trevor@eonic.co.uk)
// &Website:     www.eonic.co.uk
// &Licence:     All Rights Reserved.
// $Copyright:   Copyright (c) 2002 - 2010 Eonic Ltd.
// ***********************************************************************

using Protean.Providers.Membership;
using System;
using System.Reflection;
using System.Web.Configuration;

using System.Xml;
using static Protean.stdTools;

namespace Protean.Providers
{
    namespace Filter
    {
        public interface IFilterProvider
        {
            IFilterAdminXforms AdminXforms { get; set; }
            IFilterAdminProcess AdminProcess { get; set; }
            IFilterActivities Activities { get; set; }           

        }
        public interface IFilterAdminXforms
        {
            void AddControl(ref Cms aWeb, ref XmlElement FilterConfig, ref Protean.xForm oXform, ref XmlElement oFromGroup, ref XmlElement oContentNode, string cWhereSql);
            string ApplyFilter(ref Cms aWeb, ref string cWhereSql, ref Protean.xForm oXform, ref XmlElement oFromGroup, ref XmlElement FilterConfig, ref string cFilterTarget);
            string GetFilterSQL(ref Cms aWeb);
        }
        public interface IFilterAdminProcess
        {
        }
        public interface IFilterActivities
        { 
        }

        public class ReturnProvider
        {
            private const string mcModuleName = "Protean.Providers.Filter.ReturnProvider";
            protected XmlNode moFilterCfg;

            public IFilterProvider Get(ref Cms myWeb, string ProviderName)
            {
                try
                {
                    Type calledType;
                    XmlElement oProviderCfg;

                    moFilterCfg = (XmlNode)WebConfigurationManager.GetWebApplicationSection("protean/Filter");
                    oProviderCfg = (XmlElement)moFilterCfg.SelectSingleNode("provider[@name='" + ProviderName + "']");

                    string ProviderClass = "";
                    if (oProviderCfg != null)
                    {
                        if (oProviderCfg.HasAttribute("class"))
                        {
                            ProviderClass = oProviderCfg.GetAttribute("class");
                        }
                    }
                    else
                    {
                        // Asssume Eonic Provider
                    }

                    if (string.IsNullOrEmpty(ProviderClass))
                    {
                        ProviderClass = "Protean.Providers.Filter.DefaultProvider";
                        calledType = Type.GetType(ProviderClass, true);
                    }
                    else
                    {
                        Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)WebConfigurationManager.GetWebApplicationSection("protean/fliterProviders");
                        if (moPrvConfig.Providers[ProviderClass] != null)
                        {
                            var assemblyInstance = Assembly.Load(moPrvConfig.Providers[ProviderClass].Type);
                            calledType = assemblyInstance.GetType("Protean.Providers.Filter." + ProviderClass, true);
                        }
                        else
                        {
                            calledType = Type.GetType("Protean.Providers.Filter." + ProviderClass, true);
                        }

                    }

                    var o = Activator.CreateInstance(calledType);

                    var args = new object[5];
                    args[0] = myWeb;

                    return (IFilterProvider)calledType.InvokeMember("Initiate", BindingFlags.InvokeMethod, null, o, args);
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "New", ex, "", ProviderName + " Could Not be Loaded", gbDebug);
                    return null;
                }
            }
        }

        public class DefaultProvider : IFilterProvider
        {
            private IFilterAdminXforms _AdminXforms;
            private IFilterAdminProcess _AdminProcess;
            private IFilterActivities _Activities;
            IFilterAdminXforms IFilterProvider.AdminXforms
            {
                set
                {
                    _AdminXforms = value;
                }
                get
                {
                    return _AdminXforms;
                }
            }
            IFilterAdminProcess IFilterProvider.AdminProcess
            {
                set
                {
                    _AdminProcess = value;
                }
                get
                {
                    return _AdminProcess;
                }
            }
            IFilterActivities IFilterProvider.Activities
            {
                set
                {
                    _Activities = value;
                }
                get
                {
                    return _Activities;
                }
            }

            public DefaultProvider()
            {
                // do nothing
            }

            // IFilterProvider myFilters;

            public IFilterProvider Initiate(ref Cms myWeb)
            {
                // myFilters = new Filters();
                _AdminXforms = new AdminXForms(ref myWeb);
                _AdminProcess = new AdminProcess(ref myWeb);
                // MemProvider.AdminProcess.oAdXfm = MemProvider.AdminXforms
               // _Activities = new Activities();
                return this;
            }

            public class AdminXForms : Cms.Admin.AdminXforms, IFilterAdminXforms
            {
                private const string mcModuleName = "Providers.Providers.Eonic.AdminXForms";

                public AdminXForms(ref Cms aWeb) : base(ref aWeb)
                {
                }

                public void AddControl(ref Cms aWeb, ref XmlElement FilterConfig, ref xForm oXform, ref XmlElement oFromGroup, ref XmlElement oContentNode, string cWhereSql)
                {
                    throw new NotImplementedException();
                }

                public string ApplyFilter(ref Cms aWeb, ref string cWhereSql, ref xForm oXform, ref XmlElement oFromGroup, ref XmlElement FilterConfig, ref string cFilterTarget)
                {
                    throw new NotImplementedException();
                }

                public string GetFilterSQL(ref Cms aWeb)
                {
                    throw new NotImplementedException();
                }
            }

            public class AdminProcess : Cms.Admin, IFilterAdminProcess
            {

                private AdminXForms _oAdXfm;

                public object oAdXfm
                {
                    set
                    {
                        _oAdXfm = (AdminXForms)value;
                    }
                    get
                    {
                        return _oAdXfm;
                    }
                }

                public AdminProcess(ref Cms aWeb) : base(ref aWeb)
                {
                }
            }
            public void DoContentIndex()
            {
            }                    

            public class Filters
            {



                private const string mcModuleName = "Protean.Providers.Filter.DefaultProvider.Filters";

                // Work 3 Ways
                // 1. on page build with URL/form/session parameters
                // 2. JSON API to return products in JSON for Vue refresh.
                // 3. Once all products are rendered, we could show/hide with clientside javascript using XSLT templates only


                public string[] FilterQueries;

                // ' Create a filter module 
                // ' Product list module



                // Public Sub PageFilter(ByRef aWeb As Cms, ByRef nPageId As Integer, ByRef oXform As xForm, ByRef oFromGroup As XmlElement)
                // Try
                // Dim pageFilterSelect As XmlElement
                // Dim oDr As SqlDataReader
                // 'Create Stored procedure 
                // 'oDr
                // 'Adding controls to the form like dropdown, radiobuttons
                // pageFilterSelect = oXform.addSelect1(oFromGroup, "PageFilter", False, "Select By Page")
                // oXform.addOptionsFromSqlDataReader(pageFilterSelect, oDr)


                // 'Dim nPageId As Integer = 0
                // Dim cWhereSql As String = String.Empty
                // 'If (oFilterNode.SelectNodes("Filter/PageId") Is Nothing) Then
                // '    nPageId = oFilterNode.SelectNodes("Filter/PageId").ToString()
                // 'End If



                // Dim oMenuItem As XmlElement
                // If (nPageId <> 0) Then

                // 'Dim oMenuElmt As XmlElement = aWeb.GetStructureXML(aWeb.mnUserId, nPageId, 0, "Site", False, False, False, True, False, "MenuItem", "Menu")
                // Dim oSubMenuList As XmlNodeList = aWeb.moPageXml.SelectSingleNode("/Page/Menu/MenuItem/descendant-or-self::MenuItem[@id='" & nPageId & "']").SelectNodes("MenuItem")
                // For Each oMenuItem In oSubMenuList
                // cWhereSql = cWhereSql + oMenuItem.Attributes("id").InnerText.ToString() + ","
                // Next
                // If (cWhereSql = String.Empty) Then
                // cWhereSql = cWhereSql + nPageId.ToString() + ","
                // End If
                // 'call sp and return xml data
                // If (cWhereSql <> String.Empty) Then
                // cWhereSql = cWhereSql.Substring(0, cWhereSql.Length - 1)
                // cWhereSql = " nStructId IN (" + cWhereSql + ")"
                // aWeb.GetPageContentFromSelect(cWhereSql,,,,,,,,,,, "Product")
                // End If

                // End If

                // Catch ex As Exception

                // End Try
                // End Sub

                public void PriceRangeFilter(ref Cms aWeb, ref XmlElement oFilterNode)
                {

                    // sets a low and high price returns 

                }


                public void BrandFilter(ref Cms aWeb, ref XmlElement oFilterNode)
                {
                    // will not be required for ITB


                }

                public void InStockFilter(ref Cms aWeb, ref XmlElement oFilterNode)
                {
                    // will not be required for ITB


                }


            }

            public virtual string GetFilterOrderByClause()
            {
                return null;
            }
        }


    }

}