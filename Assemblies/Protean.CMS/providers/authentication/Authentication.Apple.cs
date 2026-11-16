using System;
using System.Xml;

namespace Protean.Providers
{
    namespace Authentication
    {

        public class Apple : Authentication.Default, IauthenticaitonProvider
        {

            public Apple()
            {
                // do nothing
            }

            protected override string DefaultJwksUri
            {
                get { return "https://appleid.apple.com/auth/keys"; }
            }

            protected override bool IsValidDefaultIssuer(string issuer)
            {
                return issuer == "https://appleid.apple.com";
            }

            public XmlElement GetLogonBtn(ref XmlElement oContextNode, string mode = "signin")
            {
                XmlElement thisButton = base.GetLogonBtn(ref oContextNode, mode);

                // clientId is the Apple Services ID
                thisButton.SetAttribute("data-clientid", base.config["clientId"]);

                // Must exactly match a Return URL registered against the Services ID
                string redirectUri = base.config["redirectUri"];
                if (string.IsNullOrEmpty(redirectUri))
                {
                    redirectUri = "https://" + _myWeb.moRequest.ServerVariables["HTTP_HOST"] + "/";
                }
                thisButton.SetAttribute("data-redirecturi", redirectUri);

                return thisButton;
            }

        }
    }
}