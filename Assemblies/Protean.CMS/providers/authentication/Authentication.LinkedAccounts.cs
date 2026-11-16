using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Protean.Providers.Authentication
{
    public static class LinkedAccounts
    {
        public static void AddToForm(Cms myWeb, xForm form, long userId)
        {
            try
            {
                IEnumerable<IauthenticaitonProvider> providers = new ReturnProvider().Get(ref myWeb);
                if (providers == null) return;
                providers = providers.Where(p => p.config["scope"] == "site").ToList();
                if (!providers.Any()) return;

                string statusMessage = string.Empty;
                string unlinkName = myWeb.moRequest["UnlinkProvider"];
                string postedProvider = myWeb.moRequest["auth_provider"];

                foreach (IauthenticaitonProvider p in providers)
                {
                    string pName = p.config["name"];
                    if (!string.IsNullOrEmpty(unlinkName))
                    {
                        if (pName.Equals(unlinkName, StringComparison.OrdinalIgnoreCase))
                        {
                            p.Unlink(userId);
                            statusMessage = pName + " has been disconnected";
                        }
                    }
                    else
                    {
                        bool linked;
                        string err = p.LinkAuthenticationResponse("UserLogon", userId, out linked);
                        if (!string.IsNullOrEmpty(err)) statusMessage = err;
                        else if (linked && pName.Equals(postedProvider, StringComparison.OrdinalIgnoreCase)) statusMessage = pName + " has been connected";
                    }
                }

                XmlElement accounts = form.Instance.OwnerDocument.CreateElement("linkedAccounts");
                XmlElement parent = (XmlElement)form.moXformElmt.SelectSingleNode("group") ?? form.moXformElmt;
                XmlElement grp = form.addGroup(ref parent, "linkedAccounts", "linked-accounts", "Linked accounts");
                if (!string.IsNullOrEmpty(statusMessage)) form.addNote(ref grp, xForm.noteTypes.Hint, statusMessage, true);

                foreach (IauthenticaitonProvider p in providers)
                {
                    string pName = p.config["name"];
                    bool isLinked = p.IsLinked(userId);
                    XmlElement acc = form.Instance.OwnerDocument.CreateElement("account");
                    acc.SetAttribute("provider", pName);
                    acc.SetAttribute("linked", isLinked ? "true" : "false");
                    accounts.AppendChild(acc);

                    if (isLinked)
                    {
                        form.addSubmit(ref grp, "UnlinkProvider", "Disconnect " + pName, "UnlinkProvider", "btn-outline-danger btn-disconnect btn-" + pName.ToLower(), "", pName);
                    }
                    else
                    {
                        p.GetLogonBtn(ref grp, "link");
                    }
                }
                form.Instance.AppendChild(accounts);
            }
            catch (Exception) { }
        }
    }
}