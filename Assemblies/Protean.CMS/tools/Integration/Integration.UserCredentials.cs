
using System;
using System.Collections;
using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Protean.Integration
{

    /// <summary>
    /// Credentials are a simple hashtable of settings that indicate string settings intended to
    /// represent tokens or authorisation for a user and that provider
    /// Ideally this would be serializable but I didn't have time to make the serialization work for inheritted classes
    /// </summary>
    /// <remarks></remarks>
    public class UserCredentials
    {

        private string _name = "Generic";

        private Hashtable _settings;

        private XmlElement _permissions;


        public UserCredentials(string providerName)
        {
            _name = providerName;
            _settings = new Hashtable();
        }

        public UserCredentials(string providerName, XmlElement directoryInstance)
        {
            _name = providerName;
            _settings = new Hashtable();
            try
            {
                if (directoryInstance.SelectSingleNode("//cDirXml/Credentials[@provider='" + providerName + "']") != null)
                {
                    Deserialize((XmlElement)directoryInstance.SelectSingleNode("//cDirXml/Credentials[@provider='" + providerName + "']"));
                }
            }
            catch (Exception ex)
            {

            }
        }


        public string Provider
        {
            get
            {
                return _name;
            }
        }

        public void AddSetting(string key, string value)
        {
            if (_settings.ContainsKey(key))
                _settings.Remove(key);
            _settings.Add(key, value);
        }

        public string GetSetting(string key)
        {
            return Conversions.ToString(Interaction.IIf(_settings.ContainsKey(key), _settings[key].ToString(), ""));
        }

        public XmlElement Serialize()
        {
            var doc = new XmlDocument();
            XmlElement element;
            var root = doc.CreateElement("Credentials");
            root.SetAttribute("provider", Provider);
            foreach (object key in _settings.Keys)
            {
                element = doc.CreateElement(key.ToString());
                element.InnerText = Conversions.ToString(_settings[key]);
                root.AppendChild(element);
            }
            // Add the permissions node
            if (_permissions != null)
            {
                root.AppendChild(doc.ImportNode(_permissions, true));
            }

            return root;
        }

        /// <summary>
        /// Serializes the UserCredentials to an XML node under the directory instance.
        /// Replaces any existing node for this provider
        /// </summary>
        /// <param name="directoryInstance"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool SerializeToDirectoryInstance(ref XmlElement directoryInstance)
        {

            bool serializationComplete = false;

            XmlElement directoryExtendedXml = (XmlElement)directoryInstance.SelectSingleNode("//cDirXml");

            if (directoryExtendedXml is null)
            {
                throw new Exception("Could not seriliaze the credentials as there is no directory instance loaded.");
            }
            else
            {
                var serializedCredentials = Serialize();
                XmlElement existingCredentials = (XmlElement)directoryExtendedXml.SelectSingleNode("Credentials[@provider='" + Provider + "']");

                if (existingCredentials != null)
                {
                    directoryExtendedXml.ReplaceChild(directoryExtendedXml.OwnerDocument.ImportNode(serializedCredentials, true), existingCredentials);
                }
                else
                {
                    directoryExtendedXml.AppendChild(directoryExtendedXml.OwnerDocument.ImportNode(serializedCredentials, true));
                }

                serializationComplete = true;
            }

            return serializationComplete;

        }

        public bool RemoveFromDirectoryInstance(ref XmlElement directoryInstance)
        {

            bool removalComplete = false;

            XmlElement directoryExtendedXml = (XmlElement)directoryInstance.SelectSingleNode("//cDirXml");

            if (directoryExtendedXml is null)
            {
                throw new Exception("Could not remove the credentials as there is no directory instance loaded.");
            }
            else
            {
                // Get the credentials
                XmlElement existingCredentials = (XmlElement)directoryExtendedXml.SelectSingleNode("Credentials[@provider='" + Provider + "']");
                if (existingCredentials != null)
                {
                    directoryExtendedXml.RemoveChild(existingCredentials);
                }
                removalComplete = true;
            }

            return removalComplete;

        }

        public bool Deserialize(XmlElement credentialsNode)
        {
            try
            {
                foreach (XmlElement setting in credentialsNode.ChildNodes)
                {

                    // Capture everything as a hashtable
                    // Except permissions which can be saved as a node.
                    if (setting.Name == "Permissions")
                    {
                        _permissions = setting;
                    }
                    else if (setting.NodeType == XmlNodeType.Element)
                    {
                        AddSetting(setting.LocalName, setting.InnerText);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }


}