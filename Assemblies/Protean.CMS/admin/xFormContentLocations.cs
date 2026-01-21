// ***********************************************************************
// $Library:     protean.cms.adminXforms
// $Revision:    3.1  
// $Date:        2006-03-02
// $Author:      Trevor Spink (trevor@eonic.com)
// &Website:     eonic.com
// &Licence:     Apache-2.0 license
// $Copyright:   Copyright (c) 2002 - 2022 Eonic Digital LLP.
// ***********************************************************************


using Microsoft.VisualBasic;
using Protean.Providers.CDN;
using Protean.Providers.Membership;
using Protean.Providers.Payment;
using Protean.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
//using System.Text.Json.Nodes;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using static Protean.Cms;
using static Protean.stdTools;
using static Protean.Tools.Text;
using static Protean.Tools.Xml;
using static System.Web.HttpUtility;

namespace Protean
{

    public partial class Cms
    {
        public partial class Admin
        {
            public partial class AdminXforms : xForm, IDisposable
            {
                /// <summary>
                /// 
                /// </summary>
                /// <remarks></remarks>
                private class xFormContentLocations
                {

                    #region  Declarations
                    private string _moduleName = "xFormContentLocations";


                    // Declarations
                    private AdminXforms _form;
                    //private XmlElement _locations;
                    private int _locationCount = 1;
                    private long _contentId;
                    private XmlElement _structureXml = null;
                    private Hashtable _currentLocations;
                    private Hashtable _locationsScope;
                    private XmlNodeList _selects = null;

                    // Constants
                    private const string _selectsXPath = "//node()[contains(name(),'select') and contains(@class,'contentLocations')]";
                    private const string _locationInstanceNodeName = "locatomatic";
                    #endregion
                    #region  Initialisation
                    public xFormContentLocations(long ContentId, ref Cms.xForm Form)
                    {
                        // myWeb.PerfMon.Log(_moduleName, "New")
                        try
                        {
                            // Set variables
                            _contentId = ContentId;
                            _form = (AdminXforms)Form;
                            _selects = _form.RootGroup.SelectNodes(_selectsXPath);
                        }


                        catch (Exception ex)
                        {
                            stdTools.returnException(ref Form.myWeb.msException, _moduleName, "New", ex, "", "", gbDebug);
                        }
                    }
                    #endregion
                    #region  Public Methods
                    public void Refresh()
                    {
                        // myWeb.PerfMon.Log(_moduleName, "Refresh")
                        try
                        {
                            _selects = _form.RootGroup.SelectNodes(_selectsXPath);
                        }
                        catch (Exception ex)
                        {
                            stdTools.returnException(ref _form.myWeb.msException, _moduleName, "Refresh", ex, "", "", gbDebug);
                        }
                    }
                    public bool IsActive()
                    {
                        // myWeb.PerfMon.Log(_moduleName, "IsActive")
                        try
                        {
                            return _selects.Count > 0;
                        }
                        catch (Exception ex)
                        {
                            stdTools.returnException(ref _form.myWeb.msException, _moduleName, "IsActive", ex, "", "", gbDebug);
                            return false;
                        }
                    }
                    public void ProcessSelects()
                    {
                        // myWeb.PerfMon.Log(_moduleName, "ProcessSelects")

                        long menuId;
                        XmlElement bind;
                        XmlElement locations;
                        XmlNodeList menuItems;
                        xFormContentLocationsSelect selectItem;
                        string value;
                        XmlElement location;
                        string locationid;
                        string cXPath;
                        string cXPathModifier = string.Empty;
                        string menuName = "";


                        try
                        {
                            if (IsActive())
                            {

                                // Get the site structure
                                _structureXml = _form.myWeb.GetStructureXML();

                                // Get the current locations
                                _currentLocations = new Hashtable();
                                _locationsScope = new Hashtable();
                                if (_contentId > 0L)
                                {
                                    string argsValueField = "Location";
                                    _currentLocations = _form.moDbHelper.getHashTable("SELECT nStructId,1 As Location FROM tblContentLocation WHERE nContentId=" + _contentId, "nStructId", ref argsValueField);
                                }

                                // Create a bind element
                                XmlNode argoNode = (XmlNode)_form.model;
                                bind = addNewTextNode("bind", ref argoNode);
                                _form.model = (XmlElement)argoNode;
                                bind.SetAttribute("nodeset", _locationInstanceNodeName);

                                var argoNode1 = _form.moXformElmt.SelectSingleNode("//instance");
                                locations = Xml.addNewTextNode(_locationInstanceNodeName, ref argoNode1);


                                // Iterate through each select
                                foreach (XmlElement _selectItem in _selects)
                                {
                                    XmlElement xmlselectItem = _selectItem;
                                    // Get the rootid - look for class root-id
                                    selectItem = new xFormContentLocationsSelect(ref xmlselectItem);

                                    // Get the menuItems - construct an xpath
                                    // The rootmode is the path modifier
                                    cXPath = "//MenuItem[@id=" + selectItem.Root.ToString() + " and ((not(@cloneparent) or @cloneparent=0) and (not(@clone) or @clone=0))]";

                                    if (!string.IsNullOrEmpty(_selectItem.GetAttribute("locationsXpath")))
                                    {
                                        cXPath = cXPath + _selectItem.GetAttribute("locationsXpath");
                                    }
                                    else
                                    {

                                        cXPathModifier = "";

                                        switch (selectItem.RootMode)
                                        {
                                            case xFormContentLocationsSelect.RootModes.Exclude:
                                                {
                                                    cXPath = cXPath + "/descendant::MenuItem";
                                                    break;
                                                }
                                            case xFormContentLocationsSelect.RootModes.Include:
                                                {
                                                    cXPath = cXPath + "/descendant-or-self::MenuItem";
                                                    break;
                                                }
                                            case xFormContentLocationsSelect.RootModes.ChildrenOnly:
                                                {
                                                    cXPath = cXPath + "/MenuItem";
                                                    break;
                                                }
                                        }

                                    }

                                    menuItems = _structureXml.SelectNodes(cXPath);

                                    // Add the instance node - assume that there could be more then one select here.
                                    XmlNode argoNode2 = locations;
                                    location = addNewTextNode("location", ref argoNode2);
                                    locations = (XmlElement)argoNode2;
                                    locationid = "loc_idx_" + _locationCount.ToString();
                                    _locationCount = _locationCount + 1;
                                    location.SetAttribute("id", locationid);


                                    // location the bind
                                    _form.addBind(selectItem.Id, "location[@id='" + locationid + "']", oBindParent: ref bind);


                                    XmlElement proceedingParent = null;
                                    XmlElement oChoices = null;
                                    // Process the menu items
                                    // For each menuitem, check if it's already in scope.
                                    // If not add the option to the select.
                                    foreach (XmlElement menuItem in menuItems)
                                    {
                                        menuId = Convert.ToInt64(menuItem.GetAttribute("id"));

                                        // Check if we've added it already
                                        if (!_locationsScope.ContainsKey(menuId))
                                        {
                                            if (_currentLocations.ContainsKey(menuId.ToString()))
                                            {
                                                value = "true";
                                                if (!string.IsNullOrEmpty(location.InnerText))
                                                    location.InnerText += ",";
                                                location.InnerText += menuId.ToString();
                                            }
                                            else
                                            {
                                                value = "false";
                                            }

                                            // Add to in-scope location hashtable
                                            _locationsScope.Add(menuId, value);

                                            // Determine the name - NodeState effectively sets menuName as
                                            // the DisplayName node if it's populated, if not is sets it to be the
                                            // name attribute.
                                            XmlElement xmlmenuItem = menuItem;
                                            if (Xml.NodeState(ref xmlmenuItem, "DisplayName", "", "", XmlNodeState.IsEmpty, null, "", menuName, true) != XmlNodeState.HasContents)
                                            {
                                                menuName = menuItem.GetAttribute("name");
                                            }
                                            else
                                            {
                                                menuName = menuItem.SelectSingleNode("DisplayName").InnerText;
                                            }


                                            XmlElement oParentNode = (XmlElement)menuItem.ParentNode;
                                            XmlElement oParentParentNode = (XmlElement)oParentNode.ParentNode;

                                            // _form.addOption(_selectItem, menuName, menuId)

                                            // if we are only 2 levels from the root then we use choices
                                            if (oParentParentNode != null)
                                            {
                                                if ((oParentParentNode.GetAttribute("id") ?? "") == (selectItem.Root.ToString() ?? "") & Strings.LCase(_selectItem.GetAttribute("showAllLevels")) != "true")
                                                {
                                                    XmlElement xmlselect = _selectItem;
                                                    if (proceedingParent is null)
                                                    {
                                                        oChoices = _form.addChoices(ref xmlselect, oParentNode.GetAttribute("name"));
                                                    }
                                                    else if ((proceedingParent.GetAttribute("id") ?? "") != (oParentNode.GetAttribute("id") ?? ""))
                                                    {
                                                        oChoices = _form.addChoices(ref xmlselect, oParentNode.GetAttribute("name"));

                                                    }
                                                    // Add the checkbox
                                                    _form.addOption(ref oChoices, menuName, menuId.ToString());
                                                }
                                                // If oParentNode IsNot Nothing Then
                                                else if ((oParentNode.GetAttribute("id") ?? "") != (_form.myWeb.moConfig["RootPageId"] ?? ""))
                                                {
                                                    while ((oParentNode.GetAttribute("id") ?? "") != (selectItem.Root.ToString() ?? ""))
                                                    {
                                                        menuName = oParentNode.GetAttribute("name") + " / " + menuName;
                                                        oParentNode = (XmlElement)oParentNode.ParentNode;
                                                        if (oParentNode is null)
                                                            break;
                                                    }
                                                    // End If
                                                }
                                                // Add the checkbox
                                                _form.addOption(ref xmlselectItem, menuName, menuId.ToString());
                                            }
                                            else
                                            {

                                                if ((menuItem.GetAttribute("id") ?? "") != (_form.myWeb.moConfig["RootPageId"] ?? ""))
                                                {
                                                    while ((menuItem.GetAttribute("id") ?? "") != (selectItem.Root.ToString() ?? ""))
                                                    {
                                                        menuName = menuItem.GetAttribute("name") + " / " + menuName;
                                                        oParentNode = (XmlElement)menuItem.ParentNode;
                                                    }
                                                }

                                                // Add the checkbox
                                                _form.addOption(ref xmlselectItem, menuName, menuId.ToString());
                                            }
                                            proceedingParent = oParentNode;
                                        }
                                    }
                                }
                            }
                        }

                        catch (Exception ex)
                        {
                            stdTools.returnException(ref _form.myWeb.msException, _moduleName, "ProcessSelects", ex, "", "", gbDebug);

                        }
                    }


                    public void ProcessRequest(long ContentId)
                    {
                        // myWeb.PerfMon.Log(_moduleName, "ProcessRequest")

                        string InclusionList = "";
                        string ScopeList = "";

                        try
                        {
                            if (IsActive())
                            {

                                // Go through the location binds and deal with them.
                                // _locationInstanceNodeName

                                foreach (XmlElement location in _form.moXformElmt.SelectNodes("//instance/" + _locationInstanceNodeName + "/location"))
                                {

                                    // The inner text will be a comma separated list, we need to add this to the inclusion list.
                                    if (!string.IsNullOrEmpty(location.InnerText))
                                    {
                                        if (!string.IsNullOrEmpty(InclusionList))
                                        {
                                            InclusionList += ",";
                                        }
                                        InclusionList += location.InnerText;
                                    }
                                }

                                // Convert the Scope to a CSV
                                ScopeList = Dictionary.hashtableToCSV(ref _locationsScope, Dictionary.Dimension.Key);

                                // manage the locations
                                _form.moDbHelper.updateLocationsWithScope(ContentId, InclusionList, ScopeList);

                            }
                        }

                        catch (Exception ex)
                        {
                            stdTools.returnException(ref _form.myWeb.msException, _moduleName, "ProcessRequest", ex, "", "", gbDebug);

                        }
                    }


                    #endregion
                    #region  Private Class: xFormContentLocationsSelect
                    private class xFormContentLocationsSelect
                    {

                        #region  Declarations
                        private string _moduleName = "xFormContentLocationsSelect";

                        private XmlElement _selectItem;
                        private long _rootId;
                        private RootModes _rootMode;

                        public enum RootModes
                        {
                            Include,
                            Exclude,
                            RootOnly,
                            ChildrenOnly
                        }
                        #endregion
                        #region  Initialisation
                        public xFormContentLocationsSelect(ref XmlElement selectItem)
                        {
                            try
                            {
                                _rootMode = RootModes.Exclude;
                                Item = selectItem;
                            }
                            catch (Exception)
                            {
                                // returnException(Form.myWeb.msException, _moduleName, "New", ex, "", "", gbDebug)
                            }
                        }

                        #endregion
                        #region  Private Properties
                        private string ClassName
                        {
                            get
                            {
                                return _selectItem.GetAttribute("class");
                            }

                        }

                        #endregion
                        #region  Public Properties
                        public XmlElement Item
                        {
                            get
                            {
                                return _selectItem;
                            }
                            set
                            {

                                // Set the Item
                                _selectItem = value;

                                // Determine its Root Id
                                string argpropertyName = "root";
                                string rootId = getPropertyFromClass(ref argpropertyName);
                                _rootId = Convert.ToInt64(Interaction.IIf(!string.IsNullOrEmpty(rootId) & Tools.Number.IsNumeric(rootId), Convert.ToInt64(rootId), 0));

                                // Determine the root mode
                                string argpropertyName1 = "rootMode";
                                string rootModeParameter = "" + getPropertyFromClass(ref argpropertyName1);
                                switch (rootModeParameter.ToLower() ?? "")
                                {
                                    case "exclude":
                                        {
                                            _rootMode = RootModes.Exclude;
                                            break;
                                        }
                                    case "include":
                                        {
                                            _rootMode = RootModes.Include;
                                            break;
                                        }
                                    case "rootonly":
                                        {
                                            _rootMode = RootModes.RootOnly;
                                            break;
                                        }
                                    case "childrenonly":
                                        {
                                            _rootMode = RootModes.ChildrenOnly;
                                            break;
                                        }
                                }

                            }
                        }
                        public long Root
                        {
                            get
                            {
                                return _rootId;
                            }
                        }
                        public RootModes RootMode
                        {
                            get
                            {
                                return _rootMode;
                            }
                        }
                        public string Id
                        {
                            get
                            {
                                if (!string.IsNullOrEmpty(_selectItem.GetAttribute("bind")))
                                {
                                    return _selectItem.GetAttribute("bind");
                                }
                                else if (!string.IsNullOrEmpty(_selectItem.GetAttribute("ref")))
                                {
                                    return _selectItem.GetAttribute("ref");
                                }
                                else
                                {
                                    return "";
                                }
                            }
                        }
                        #endregion
                        #region  Private Methods
                        private string getPropertyFromClass(ref string propertyName)
                        {
                            try
                            {

                                string pattern = @"^.*\s" + propertyName + @"-([\S]*)\s.*$";
                                return "" + Text.SimpleRegexFind(" " + ClassName + " ", pattern, 1);
                            }

                            catch (Exception)
                            {
                                // returnException(myWeb.msException, _moduleName, "getPropertyFromClass", ex, "", "", gbDebug)
                                return "";
                            }

                        }
                        #endregion

                    }

                    #endregion


                }
            }
        }
    }
}