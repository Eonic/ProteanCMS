using System;
using System.Web.Configuration;

namespace Protean
{

    /// <summary>
/// Protean.Cms.TypeExtensions extends Protean.Tools.TypeExtensions
/// allows handling of various scenarios that return a type.
/// </summary>
/// <remarks></remarks>
    public class Web
    {
        public class TypeExtensions : Tools.TypeExtensions
        {

            public static Type TypeFromProviderName(string typeFromSpecifiedAssembly, string providerName, string providerSection)
            {

                Protean.ProviderSectionHandler providerConfig = (Protean.ProviderSectionHandler)WebConfigurationManager.GetWebApplicationSection(providerSection);
                if (providerConfig is not null)
                {
                    var provider = providerConfig.Providers[providerName];
                    if (provider is not null)
                    {
                        return Tools.TypeExtensions.Type(typeFromSpecifiedAssembly, provider.Type);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }

            }

            public static Type Type(string typeName, string assemblyName, string assemblyType, string providerName, string providerSection)
            {

                if (!string.IsNullOrEmpty(providerName) & !string.IsNullOrEmpty(providerSection))
                {
                    return TypeFromProviderName(typeName, providerName, providerSection);
                }
                else if (!string.IsNullOrEmpty(assemblyName))
                {
                    return Type(typeName, assemblyName, assemblyType);
                }
                else
                {
                    // Need to avoid going back to Protean.Tools, so just repliacte here
                    return System.Type.GetType(typeName, true, true);
                }

            }

        }
    }
}