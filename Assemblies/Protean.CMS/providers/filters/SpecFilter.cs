using System;
using System.Collections;
using System.Data;


using System.Data.SqlClient;
using System.Xml;
using Lucene.Net.Search;
using Microsoft.VisualBasic.CompilerServices;
using Protean.Providers.Filter;

namespace Protean.Providers
{

    namespace Filters
    {

        public class SpecFilter : DefaultFilter
        {

            public event OnErrorEventHandler OnError;

            public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);
            public override void AddControl(ref Cms aWeb, ref XmlElement FilterConfig, ref Protean.xForm oXform, ref XmlElement oFromGroup, ref XmlElement oContentNode, string cWhereSql)
            {
                string cProcessInfo = "AddControl";
                try
                {
                    string sControlDisplayName = FilterConfig.GetAttribute("specName");

                    string cFilterTarget = string.Empty;

                    //add to instance
                    Hashtable arrParams = new Hashtable();
                    XmlElement oXml = oXform.moPageXML.CreateElement("SpecFilter");
                    oXml.SetAttribute("name", sControlDisplayName);
                    oXform.Instance.AppendChild(oXml);

                    //add to binds
                    oXform.addBind("SpecFilter", "SpecFilter(@name='" + sControlDisplayName + "')", ref oXform.model, "false()", "string");

                    //add control
                    XmlElement thisSelect = oXform.addSelect(ref oFromGroup,"", false, sControlDisplayName, "specfilter", xForm.ApperanceTypes.Full);
                    // oXform.addOption(ref thisSelect, "Value1", "Value1");
                    SqlDataReader odr = aWeb.moDbHelper.getDataReader("SELECT DISTINCT CONCAT(cTextValue,' [',Count(*),']') as [Name], cTextValue as [Value] FROM [dbo].[tblContentIndex]\r\n  Where nContentIndexDefinitionKey = (Select nContentIndexDefKey from tblContentIndexDef where cDefinitionName = '" + sControlDisplayName + "') GROUP BY cTextValue Order By cTextValue");

                    if (odr != null) {
                        oXform.addOptionsFromSqlDataReader(ref thisSelect,ref odr);
                        //odr.Dispose();
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(cProcessInfo, "PriceFilter", ex, ""));
                }
            }

            public override string ApplyFilter(ref Cms aWeb, ref string cWhereSql, ref Protean.xForm oXform, ref XmlElement oFromGroup, ref XmlElement FilterConfig, ref string cFilterTarget)
            {
                string cProcessInfo = "ApplyFilter";
                string cPriceCond = string.Empty;
                try
                {
                   
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(cProcessInfo, "PriceFilter", ex, ""));
                }
                return cWhereSql;
            }

            public override string GetFilterSQL(ref Cms aWeb)
            {
                string cWhereSql = string.Empty;
                string cProcessInfo = "GetFilterSQL";
                string cIndexDefinationName = "Price";
                try
                {
                   
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(cProcessInfo, "PriceFilter", ex, ""));
                }
                return cWhereSql;
            }

            public override string GetFilterOrderByClause(ref Cms myWeb)
            {
                return " ci.nNumberValue ";
            }

            public override string ContentIndexDefinationName(ref Cms aWeb)
            {
                return "";
            }

        }

    }
}