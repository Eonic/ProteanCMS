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
using System.Reflection;
using System.Web.Configuration;

using System.Xml;
using static Protean.stdTools;

namespace Protean.Providers
{
    namespace Streaming
    {

        public class BaseProvider
        {
            private const string mcModuleName = "Protean.Providers.Streaming.BaseProvider";

            private object _AdminXforms;
            private object _AdminProcess;
            private object _Activities;

            protected XmlNode moStreamingCfg;

            public object AdminXforms
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

            public object AdminProcess
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

            public object Activities
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

            public BaseProvider(ref Cms myWeb, string ProviderName)
            {
                try
                {
                    Type calledType;
                    XmlElement oProviderCfg;

                    moStreamingCfg = (XmlNode)WebConfigurationManager.GetWebApplicationSection("protean/streaming");
                    oProviderCfg = (XmlElement)moStreamingCfg.SelectSingleNode("provider[@name='" + ProviderName + "']");

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
                        ProviderClass = "Protean.Providers.Streaming.EonicProvider";
                        calledType = Type.GetType(ProviderClass, true);
                    }
                    else
                    {
                        Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)WebConfigurationManager.GetWebApplicationSection("protean/paymentProviders");
                        if (moPrvConfig.Providers[ProviderClass] != null)
                        {
                            var assemblyInstance = Assembly.Load(moPrvConfig.Providers[ProviderClass].Type);
                            calledType = assemblyInstance.GetType("Protean.Providers.Payment." + ProviderClass, true);
                        }
                        else
                        {
                            calledType = Type.GetType("Protean.Providers.Payment." + ProviderClass, true);
                        }

                    }

                    var o = Activator.CreateInstance(calledType);

                    var args = new object[5];
                    args[0] = _AdminXforms;
                    args[1] = _AdminProcess;
                    args[2] = _Activities;
                    args[3] = this;
                    args[4] = myWeb;

                    calledType.InvokeMember("Initiate", BindingFlags.InvokeMethod, null, o, args);
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "New", ex, "", ProviderName + " Could Not be Loaded", gbDebug);
                }

            }

        }

        public class EonicProvider
        {

            public EonicProvider()
            {
                // do nothing
            }

            public void Initiate(ref object _AdminXforms, ref object _AdminProcess, ref object _Activities, ref object MemProvider, ref Cms myWeb)
            {

                MemProvider.AdminXforms = new AdminXForms(ref myWeb);
                MemProvider.AdminProcess = new AdminProcess(ref myWeb);
                MemProvider.AdminProcess.oAdXfm = MemProvider.AdminXforms;
                MemProvider.Activities = new Activities();

            }

            public class AdminXForms : Cms.Admin.AdminXforms
            {
                private const string mcModuleName = "Protean.Providers.Streaming.AdminXForms";

                public AdminXForms(ref Cms aWeb) : base(aWeb)
                {
                }

            }

            public class AdminProcess : Cms.Admin
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

                public AdminProcess(ref Cms aWeb) : base(aWeb)
                {
                }
            }


            public class Activities
            {
                private const string mcModuleName = "Protean.Providers.Streaming.Activities";



            }

            public class Modules
            {

                public event OnErrorEventHandler OnError;

                public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);
                private const string mcModuleName = "Eonic.CampaignMonitorTools.Modules";

                private System.Collections.Specialized.NameValueCollection moStreamingConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/streaming");


                public Modules()
                {

                    // do nowt

                }

                public void GetSteamURL(ref Cms myWeb, ref XmlElement oContentNode)
                {

                    try
                    {
                    }

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetSteamURL", ex, ""));
                    }
                }
            }
        }


    }

}