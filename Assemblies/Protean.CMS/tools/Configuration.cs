using System;
using System.Configuration;
using System.Web.Configuration;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.VisualBasic.CompilerServices;

namespace Protean
{


    /// <summary>
/// Generic web configuration section retrieval handler.
/// Methods are all shared.
/// The idea is that Protean.Config can be called by individual Eonic classes that offer shared config retrieval.
/// Config doens't have to be part of instantiated classes, meaning that it can be used without the other usual dependencies
/// that might not be there - e.g. HttpContext.Current
/// </summary>
/// <remarks></remarks>
    public class Config
    {

        /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="sectionName"></param>
    /// <returns></returns>
    /// <remarks></remarks>
        public static string Value(string key, string sectionName)
        {

            string returnValue = "";
            try
            {
                var section = ConfigSection(sectionName);
                if (section != null)
                {
                    returnValue = section[key];
                }
            }
            catch (Exception)
            {
                returnValue = "";
            }
            return returnValue;
        }

        public static System.Collections.Specialized.NameValueCollection ConfigSection(string section)
        {
            return (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection(section);
        }

        public static bool UpdateConfigValue(ref Protean.Cms myWeb, string configPath, string name, string value)
        {

            try
            {


                // update config values
                var oCfg = WebConfigurationManager.OpenWebConfiguration("/");
                DefaultSection oCgfSect = (DefaultSection)oCfg.GetSection(configPath);
                Tools.Security.Impersonate oImp = null;

                if (Conversions.ToBoolean(myWeb.impersonationMode))
                {
                    oImp = new Tools.Security.Impersonate();
                    oImp.ImpersonateValidUser(myWeb.moConfig["AdminAcct"], myWeb.moConfig["AdminDomain"], myWeb.moConfig["AdminPassword"], cInGroup: myWeb.moConfig["AdminGroup"]);
                }

                if (string.IsNullOrEmpty(configPath))
                {
                    if (oCfg.AppSettings.Settings[name] != null)
                    {
                        oCfg.AppSettings.Settings[name].Value = value;
                    }
                    else
                    {
                        oCfg.AppSettings.Settings.Add(name, value);
                    }
                }

                else
                {

                    var oConfigDoc = new XmlDocument();
                    oConfigDoc.LoadXml(oCgfSect.SectionInformation.GetRawXml());
                    XmlElement oelmt;
                    oelmt = (XmlElement)oConfigDoc.DocumentElement.SelectSingleNode("add[@key='" + name + "']");
                    if (oelmt != null)
                    {
                        oelmt.SetAttribute("value", value);
                    }
                    else
                    {
                        oelmt = oConfigDoc.CreateElement("add");
                        oelmt.SetAttribute("key", name);
                        oelmt.SetAttribute("value", value);
                        oConfigDoc.DocumentElement.AppendChild(oelmt);
                    }
                    oCgfSect.SectionInformation.RestartOnExternalChanges = false;
                    oCgfSect.SectionInformation.SetRawXml(oConfigDoc.DocumentElement.OuterXml);
                }
                oCfg.Save();

                if (Conversions.ToBoolean(myWeb.impersonationMode))
                {
                    oImp.UndoImpersonation();
                    oImp = null;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }




    }


    public class XmlSectionHandler : IConfigurationSectionHandler
    {


        public object Create(object parent, object configContext, XmlNode section)
        {
            // PerfMon.Log("XmlSectionHandler", "Create")
            return section;

        }

    }

    public class ProviderSectionHandler : ConfigurationSection
    {
        [ConfigurationProperty("providers")]
        public ProviderSettingsCollection Providers
        {
            get
            {
                return (ProviderSettingsCollection)base["providers"];
            }
        }

        [ConfigurationProperty("default", DefaultValue = "DefaultProvider")]
        public string Default
        {
            get
            {
                return (string)base["default"];
            }
            set
            {
                base["default"] = value;
            }
        }
    }



    /// <summary>
/// Configuration section handler that deserializes configuration settings to an object.
/// </summary>
/// <remarks>The root node must have a type attribute defining the type to deserialize to.</remarks>

    public class XmlSerializerSectionHandler : IConfigurationSectionHandler
    {

        public object Create(object parent, object configContext, XmlNode section)
        {
            // PerfMon.Log("XmlSerializerSectionHandler", "Create")
            // -- get the name of the type from the type= attribute on the root node
            var xpn = section.CreateNavigator();
            string TypeName = xpn.Evaluate("string(@type)").ToString();
            if (string.IsNullOrEmpty(TypeName))
            {
                throw new ConfigurationErrorsException("The type attribute is not present on the root node of " + "the <" + section.Name + "> configuration section ", section);


            }

            string rootClass = xpn.Evaluate("string(@rootClass)").ToString();

            string path = xpn.Evaluate("string(@path)").ToString();

            // -- make sure this string evaluates to a valid type
            var t = Type.GetType(TypeName);
            if (t is null)
            {
                throw new ConfigurationErrorsException("The type attribute '" + TypeName + "' specified in the root node of the " + "the <" + section.Name + "> configuration section " + "is not a valid type.", section);


            }
            var xs = new XmlSerializer(t);

            // -- attempt to deserialize an object of this type from the provided XML section
            var xnr = new XmlNodeReader(section);
            try
            {
                return xs.Deserialize(xnr);
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                var innerException = ex.InnerException;
                while (innerException != null)
                {
                    s += " " + innerException.Message;
                    innerException = innerException.InnerException;
                }
                throw new ConfigurationErrorsException("Unable to deserialize an object of type '" + TypeName + "' from " + "the <" + section.Name + "> configuration section: " + s, ex, section);


            }
        }

    }
}