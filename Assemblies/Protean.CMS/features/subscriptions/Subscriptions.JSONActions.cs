using System;
using Newtonsoft.Json.Linq;

namespace Protean
{


    public partial class Cms
    {

        public partial class Subscriptions
        {

            #region JSON Actions

            public class JSONActions
            {
                public event OnErrorEventHandler OnError;

                public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);
                private const string mcModuleName = "Protean.Subscription.JSONActions";
                private const string cContactType = "Venue";
                private Cms myWeb;
                private Cart myCart;

                public JSONActions()
                {
                    string ctest = "this constructor is being hit"; // for testing
                    myWeb = new Cms();
                    myWeb.InitializeVariables();
                    myWeb.Open();
                    myCart = new Cart(ref myWeb);

                }


                public string SubscriptionsProcess(ref Protean.Rest myApi, ref JObject jObj)
                {
                    try
                    {
                        string cProcessInfo = "";

                        // does nothing yet
                        return null;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""));
                        return ex.Message;
                    }


                }


            }

            #endregion
        }

    }
}