
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Configuration;
using System.Xml;

namespace Protean.Providers.Filters
{
    /// <summary>
    /// Factory for filter provider instantiation with caching to eliminate reflection overhead
    /// PERFORMANCE: Caches Type and instances, reducing reflection calls from 2+ per request to zero
    /// THREAD-SAFETY: Uses ConcurrentDictionary for safe multi-threaded access
    /// </summary>
    public static class FilterProviderFactory
    {
        // Cache provider Types (eliminates repeated Type.GetType and Assembly.Load calls)
        private static readonly ConcurrentDictionary<string, Type> _typeCache
            = new ConcurrentDictionary<string, Type>();

        // Cache provider instances (most filters are stateless and can be reused)
        private static readonly ConcurrentDictionary<string, IContentFilter> _instanceCache
            = new ConcurrentDictionary<string, IContentFilter>();

        /// <summary>
        /// Get or create a filter provider instance with caching
        /// </summary>
        /// <param name="className">Filter class name (e.g., "BrandFilter")</param>
        /// <param name="providerName">Provider name from config or "default"</param>
        /// <param name="myWeb">CMS context for path resolution</param>
        /// <returns>Cached or newly created filter instance</returns>
        public static IContentFilter GetProvider(string className, string providerName, Cms myWeb)
        {
            // Create cache key combining className and providerName
            string cacheKey = $"{providerName ?? "default"}:{className}";

            // Try to get from instance cache first (HOT PATH - zero reflection)
            if (_instanceCache.TryGetValue(cacheKey, out IContentFilter cachedInstance))
            {
                return cachedInstance;
            }

            // Not in cache - resolve Type (with Type caching)
            Type filterType = ResolveFilterType(className, providerName, myWeb);

            // Create new instance
            IContentFilter instance = (IContentFilter)Activator.CreateInstance(filterType);

            // Cache for future requests (most filters are stateless)
            _instanceCache.TryAdd(cacheKey, instance);

            return instance;
        }

        /// <summary>
        /// Resolve filter Type with caching (replaces repetitive reflection in ContentFilter)
        /// </summary>
        private static Type ResolveFilterType(string className, string providerName, Cms myWeb)
        {
            string typeCacheKey = $"{providerName ?? "default"}:{className}";

            // Check Type cache
            if (_typeCache.TryGetValue(typeCacheKey, out Type cachedType))
            {
                return cachedType;
            }

            Type resolvedType;

            // STRATEGY 1: Default namespace loading
            if (string.IsNullOrEmpty(providerName) || providerName.ToLower() == "default")
            {
                string fullTypeName = "Protean.Providers.Filters." + className;
                resolvedType = Type.GetType(fullTypeName, true);
            }
            // STRATEGY 2: Custom provider from web.config
            else
            {
                var castObject = WebConfigurationManager.GetWebApplicationSection("protean/filterProviders");
                Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)castObject;
                System.Configuration.ProviderSettings ourProvider = moPrvConfig.Providers[providerName];
                Assembly assemblyInstance;

                if (ourProvider.Parameters["path"] != "" && ourProvider.Parameters["path"] != null)
                {
                    assemblyInstance = Assembly.LoadFrom(myWeb.goServer.MapPath(Convert.ToString(ourProvider.Parameters["path"])));
                }
                else
                {
                    assemblyInstance = Assembly.Load(ourProvider.Type);
                }

                if (ourProvider.Parameters["rootClass"] == "")
                {
                    resolvedType = assemblyInstance.GetType("Protean.Providers.Filters." + providerName, true);
                }
                else
                {
                    string fullTypeName = ourProvider.Parameters["rootClass"] + "." + className;
                    resolvedType = assemblyInstance.GetType(fullTypeName, true);
                }
            }

            // Cache the resolved Type
            _typeCache.TryAdd(typeCacheKey, resolvedType);

            return resolvedType;
        }

        /// <summary>
        /// Clear all caches (useful for development/debugging or when assemblies are reloaded)
        /// </summary>
        public static void ClearCache()
        {
            _typeCache.Clear();
            _instanceCache.Clear();
        }
    }
}