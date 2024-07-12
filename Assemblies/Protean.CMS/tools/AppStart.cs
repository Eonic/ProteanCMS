using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JavaScriptEngineSwitcher.Core;

namespace Protean
{
    public class AppStart
    {
        public static void InitialiseJSEngine()
        {
            string sProcessInfo = "V8JsEngine";
            System.Web.HttpApplicationState moApp = System.Web.HttpContext.Current.Application;
            try
            {
                // ensures we have configured JSEngine for Bundle Transformer
                if (moApp["JSEngineEnabled"] == null)
                {

                    IJsEngineSwitcher engineSwitcher = JsEngineSwitcher.Current;
                    engineSwitcher.EngineFactories.Add(new JavaScriptEngineSwitcher.V8.V8JsEngineFactory());
                    string sJsEngine = "V8JsEngine";

                    engineSwitcher.DefaultEngineName = sJsEngine;
                    moApp["JSEngineEnabled"] = sJsEngine;
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
