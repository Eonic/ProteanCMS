using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Caching;
using static Protean.Env;

namespace Protean.Framework.Adapters
{
    public class FrameworkApplicationStateAdapter : IHttpApplicationState
    {
        private readonly MemoryCache _cache;
        private readonly string _sitePrefix;
        private readonly CacheItemPolicy _defaultPolicy;

        // Static cache manager for site-specific MemoryCache instances
        private static readonly ConcurrentDictionary<string, MemoryCache> _siteCaches =
            new ConcurrentDictionary<string, MemoryCache>();

        /// <summary>
        /// Initializes a new instance with site-isolated caching.
        /// </summary>
        /// <param name="siteName">Unique site identifier for cache isolation (e.g., domain name).</param>
        /// <param name="defaultExpiration">Default cache expiration. Defaults to 24 hours.</param>
        public FrameworkApplicationStateAdapter(string siteName, TimeSpan? defaultExpiration = null)
        {
            if (string.IsNullOrWhiteSpace(siteName))
                throw new ArgumentException("Site name cannot be null or empty", nameof(siteName));

            _sitePrefix = siteName;

            // Create or get site-specific MemoryCache instance
            _cache = _siteCaches.GetOrAdd(siteName, name =>
                new MemoryCache($"ProteanCMS_{name}"));

            _defaultPolicy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.Now.Add(defaultExpiration ?? TimeSpan.FromHours(24)),
                Priority = CacheItemPriority.Default,
                RemovedCallback = OnCacheItemRemoved
            };
        }

        // ===== INDEXER =====

        public object this[string key]
        {
            get => Get(key);
            set => Set(key, value);
        }

        // ===== CORE GET/SET METHODS =====

        public object Get(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            string prefixedKey = GetPrefixedKey(key);
            return _cache.Get(prefixedKey);
        }

        public T Get<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                return default(T);

            object value = Get(key);

            if (value == null)
                return default(T);

            try
            {
                return (T)value;
            }
            catch (InvalidCastException)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[ApplicationCache:{_sitePrefix}] Failed to cast key '{key}' to type {typeof(T).Name}");
                return default(T);
            }
        }

        public void Set(string key, object value, TimeSpan? absoluteExpiration = null)
        {
            if (string.IsNullOrEmpty(key))
                return;

            string prefixedKey = GetPrefixedKey(key);

            var policy = absoluteExpiration.HasValue
                ? new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.Add(absoluteExpiration.Value),
                    Priority = CacheItemPriority.Default,
                    RemovedCallback = OnCacheItemRemoved
                }
                : _defaultPolicy;

            _cache.Set(prefixedKey, value, policy);
        }

        public void Add(string key, object value)
        {
            Set(key, value);
        }

        // ===== CONTAINS/REMOVE/CLEAR =====

        public bool Contains(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            string prefixedKey = GetPrefixedKey(key);
            return _cache.Contains(prefixedKey);
        }

        public void Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;

            string prefixedKey = GetPrefixedKey(key);
            _cache.Remove(prefixedKey);
        }

        public void Clear()
        {
            // Get all keys for this site
            var keysToRemove = _cache
                .Where(kvp => kvp.Key.StartsWith(_sitePrefix + ":", StringComparison.Ordinal))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (string key in keysToRemove)
            {
                _cache.Remove(key);
            }
        }

        // ===== KEYS ENUMERATION =====

        public IEnumerable<string> Keys
        {
            get
            {
                return _cache
                    .Where(kvp => kvp.Key.StartsWith(_sitePrefix + ":", StringComparison.Ordinal))
                    .Select(kvp => RemovePrefix(kvp.Key));
            }
        }

        public string[] AllKeys
        {
            get { return Keys.ToArray(); }
        }

        public int Count
        {
            get
            {
                return _cache
                    .Count(kvp => kvp.Key.StartsWith(_sitePrefix + ":", StringComparison.Ordinal));
            }
        }

        // ===== LOCKING (No-op for MemoryCache - thread-safe by design) =====

        public void Lock()
        {
            // MemoryCache is thread-safe by design, no explicit locking needed
            // This is here for interface compatibility
        }

        public void UnLock()
        {
            // MemoryCache is thread-safe by design, no explicit locking needed
            // This is here for interface compatibility
        }

        // ===== HELPER METHODS =====

        private string GetPrefixedKey(string key)
        {
            return $"{_sitePrefix}:{key}";
        }

        private string RemovePrefix(string prefixedKey)
        {
            string prefix = _sitePrefix + ":";
            if (prefixedKey.StartsWith(prefix, StringComparison.Ordinal))
            {
                return prefixedKey.Substring(prefix.Length);
            }
            return prefixedKey;
        }

        private void OnCacheItemRemoved(CacheEntryRemovedArguments args)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[ApplicationCache:{_sitePrefix}] {args.CacheItem.Key} removed: {args.RemovedReason}");

            // Optional: Add telemetry or logging here
            if (args.RemovedReason == CacheEntryRemovedReason.Expired)
            {
                // Log expiration event
            }
        }

  

        /// <summary>
        /// Disposes the site-specific cache instance.
        /// </summary>
        public void Dispose()
        {
            if (_siteCaches.TryRemove(_sitePrefix, out MemoryCache cache))
            {
                cache.Dispose();
            }
        }
    }
}