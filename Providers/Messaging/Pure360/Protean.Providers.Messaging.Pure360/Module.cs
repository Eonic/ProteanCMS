using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Configuration;
using System.Xml;


namespace Protean.Providers.Messaging
{
    
    public class Pure360Tools
    {
        public Pure360Tools()
        {
        }

        public class Modules
        {
            public event OnErrorEventHandler OnError;

            public delegate void OnErrorEventHandler(object sender, Protean.Tools.Errors.ErrorEventArgs e);

            private const string mcModuleName = "Protean.CampaignMonitorTools.Modules";

            private System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");


            public Modules()
            {

                // do nowt
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }

            public void Subscribe(ref Protean.Cms myWeb, ref XmlElement oContentElement)
            {
                try
                {
                    Protean.Cms.xForm oXform = new Protean.Cms.xForm(ref myWeb);
                    oXform.moPageXML = myWeb.moPageXml;
                    XmlNode oContentNode = (XmlElement)oContentElement;
                    oXform.load(ref oContentNode, false);
                    if (oXform.isSubmitted())
                    {
                        oXform.updateInstanceFromRequest();
                        oXform.validate();
                        if (oXform.valid)
                        {

                            // We have an Xform within this content we need to process.
                            string listId = oContentElement.GetAttribute("listID");
                            if (oXform.Instance.SelectSingleNode("Subscribe/Items/ListId") != null)
                            {
                                if (oXform.Instance.SelectSingleNode("Subscribe/Items/ListId").InnerText != "")
                                    listId = oXform.Instance.SelectSingleNode("Subscribe/Items/ListId").InnerText;
                            }
                            string apiKey = moMailConfig["ApiKey"];
                            string email = oXform.Instance.SelectSingleNode("Subscribe/Items/Email").InnerText;
                            string name = oXform.Instance.SelectSingleNode("Subscribe/Items/Name").InnerText;
                            createsend_dotnet.ApiKeyAuthenticationDetails cmAuth = new createsend_dotnet.ApiKeyAuthenticationDetails(moMailConfig["ApiKey"]);
                            createsend_dotnet.Subscriber subscriber = new createsend_dotnet.Subscriber(cmAuth, listId);
                            // lets loop through the nodes to add custom fields
                            int i = 0;

                            foreach (var oElmt in oXform.Instance.SelectNodes("Subscribe/Items/*"))
                            {//ss
                             //if ((oElmt.Name == "Name" | oElmt.Name == "Email" | oElmt.Name == "ListId"))
                                i = i + 1;
                            }
                            List<createsend_dotnet.SubscriberCustomField> customFields = new List<createsend_dotnet.SubscriberCustomField>();
                            i = 0;
                            foreach (var oElmt in oXform.Instance.SelectNodes("Subscribe/Items/*"))
                            {//ss
                                //if (!(oElmt.Name == "Name" | oElmt.Name == "Email" | oElmt.Name == "ListId")) ss
                                //{
                                //    createsend_dotnet.SubscriberCustomField customField = new createsend_dotnet.SubscriberCustomField();
                                //    customField.Key = oElmt.Name;
                                //    customField.Value = oElmt.innertext;
                                //    customFields.Add(customField);
                                //    i = i + 1;
                                //}
                            }
                            try
                            {
                                subscriber.Add(email, name, customFields, false);
                                oXform.RootGroup.InnerXml = "<div class=\"subscribed\">" + oXform.Instance.SelectSingleNode("SubscribedMessage").InnerXml + "</div>";
                            }
                            catch (Exception ex)
                            {
                                oXform.RootGroup.InnerXml = "<div class=\"error\">" + ex.Message + "</div>";
                            }
                        }
                    }
                    oXform.addValues();
                    oXform = null/* TODO Change to default(_) if this is not a reference type */;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Subscribe", ex, ""));
                }
            }

            public void UnSubscribe(ref Protean.Cms myWeb, ref XmlElement oContentNode)
            {
                try
                {
                    Protean.Cms.xForm oXform = new Protean.Cms.xForm(ref myWeb);
                    oXform.moPageXML = myWeb.moPageXml;
                    oXform.load(oContentNode.ToString());
                    if (oXform.isSubmitted())
                    {
                        oXform.updateInstanceFromRequest();
                        oXform.validate();
                        if (oXform.valid)
                        {
                            // We have an Xform within this content we need to process.
                            string listId = oContentNode.GetAttribute("listID");
                            string apiKey = moMailConfig["ApiKey"];
                            string email = oXform.Instance.SelectSingleNode("Subscribe/Items/Email").InnerText;
                            CampaignMonitorAPIWrapper.CampaignMonitorAPI.api _api = new CampaignMonitorAPIWrapper.CampaignMonitorAPI.api();
                            object subscriber; // CampaignMonitorAPIWrapper.CampaignMonitorAPI.Subscriber
                                               // lets loop through the nodes to add custom fields
                            CampaignMonitorAPIWrapper.CampaignMonitorAPI.SubscriberCustomField[] customFields = new CampaignMonitorAPIWrapper.CampaignMonitorAPI.SubscriberCustomField[1];
                            int i = 0;

                            // subscriber = _api.AddSubscriberWithCustomFields(apiKey, listId, email, name, customFields)
                            subscriber = _api.Unsubscribe(apiKey, listId, email);

                            oXform.RootGroup.InnerXml = "<div>" + oXform.Instance.SelectSingleNode("SubscribedMessage").InnerXml + "</div>";
                        }
                    }
                    oXform.addValues();
                    oXform = null/* TODO Change to default(_) if this is not a reference type */;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Subscribe", ex, ""));
                }
            }
        }
    }

}

