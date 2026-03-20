# Azure Auto-Scaling Architecture Assessment for ProteanCMS

## Executive Summary

ProteanCMS is a .NET Framework 4.8 ASP.NET Web Forms application with significant architectural patterns that present challenges for Azure auto-scaling. This assessment identifies critical blockers and provides a comprehensive modernization roadmap.

---

## Current Architecture Analysis

### Technology Stack
- **Framework**: .NET Framework 4.8 (ASP.NET Web Forms)
- **Target**: Full Framework (not .NET Core/5+)
- **Database**: SQL Server (via ADO.NET)
- **Session State**: In-Process (HttpContext.Session)
- **Caching**: In-Memory (HttpContext.Cache, Application state)
- **File System**: Local file system dependencies

### Critical Auto-Scaling Blockers

#### ?? **BLOCKER 1: In-Process Session State**
**Location**: Throughout codebase
**Evidence**:
```csharp
// Base.cs, Cms.cs, Cart.cs - Multiple files
public System.Web.SessionState.HttpSessionState moSession;
moSession["Logging"] = "On";
moSession["SessionRequest"] = 0;
moSession["KA_Description"] = "...";
```

**Impact**: 
- Session data stored in-memory on each web server instance
- User sessions will break when load balancer switches servers
- Shopping carts will be lost
- Authentication state will fail
- Admin workflows will break mid-process

**Azure Solution Required**:
- Migrate to Azure Redis Cache for distributed session state
- Configure `<sessionState>` to use Azure Redis provider
- Estimated effort: 40-60 hours + testing

---

#### ?? **BLOCKER 2: Application-Level In-Memory Caching**
**Location**: Multiple cache patterns throughout application
**Evidence**:
```csharp
// Base.cs
public System.Web.Caching.Cache goCache;
goCache = moCtx.Cache;

// Cms.cs - Static caching
public static bool gbSiteCacheMode;
moCtx.Cache.Insert(url, data, ...);

// CssWebClient.cs
public void ClearApplicationCache(string Serviceurl) {
    var enumerator = moCtx.Cache.GetEnumerator();
    while (enumerator.MoveNext())
        keys.Add(enumerator.Key.ToString());
    for (int i = 0; i <= loopTo; i++) {
        if (keys[i].Contains(Serviceurl.ToLower())) {
            moCtx.Cache.Remove(keys[i]);
        }
    }
}
```

**Impact**:
- Cache misses across different server instances
- Inconsistent data served to users
- Performance degradation
- Structure/menu caching (`GetStructureXML`) will fail
- Page caching ineffective

**Azure Solution Required**:
- Implement Azure Redis Cache or Azure Cache for Redis
- Replace `HttpContext.Cache` with `IDistributedCache`
- Estimated effort: 60-80 hours

---

#### ?? **BLOCKER 3: Static Application State**
**Location**: Cms.cs and multiple static field declarations
**Evidence**:
```csharp
// Cms.cs - Static fields shared across app domain
public static string gcMenuContentCountTypes;
public static string gcMenuContentBriefTypes;
public static string gcEwBaseUrl;
public static string gcBlockContentType;
public static bool gbMembership;
public static bool gbQuote;
public static bool gbReport;
public static int gnTopLevel;
public static int gnNonAuthUsers;
public static int gnAuthUsers;
public static bool gbClone;
public static bool gbMemberCodes;
public static bool gbIPLogging;
public static string gcGenerator;
public static string gcCodebase;
public static string gcReferencedAssemblies;
public static bool gbUseLanguageStylesheets;
public static string gcProjectPath;
public static bool gbUserIntegrations;
public static bool gbSingleLoginSessionPerUser;
public static short gnSingleLoginSessionTimeout;
public static bool gbSiteCacheMode;
public static bool gbCompiledTransform;

// Base.cs
public System.Web.HttpApplicationState goApp;
```

**Impact**:
- State not synchronized across instances
- Configuration changes require app restart
- Singleton pattern failures in scaled environment
- Counter/metrics inaccurate

**Azure Solution Required**:
- Move configuration to Azure App Configuration or Key Vault
- Implement distributed state management
- Estimated effort: 30-40 hours

---

#### ?? **BLOCKER 4: Local File System Dependencies**
**Location**: Multiple file handling classes
**Evidence**:
```csharp
// fsHelper.cs - Extensive local file operations
public class fsHelper {
    public string mcStartFolder;
    public string CreateFolder(string cFolderName, string cFolderPath);
    public string DeleteFolder(string cFolderName, string cFolderPath);
    public FileStream GetFileStream(string FilePath);
    public List<string> EnumerateFolders(string path);
    public string FindFilePathInCommonFolders(string pathToCheck, string[] foldersToCheck);
}

// ImageHelper.cs - Image manipulation requires local access
public string ResizeImage(string cVirtualPath, long maxWidth, long maxHeight, ...);
public string CreateWebP(string cVirtualPath, string sForceCheck);
public string Watermark(string cVirtualPath, ...);

// Indexer.cs - Lucene indexes stored locally
private string mcIndexReadFolder;
private string mcIndexWriteFolder;
private string mcIndexCopyFolder;
```

**Impact**:
- Uploaded files only exist on one server
- Image resizing creates files locally
- Search index fragmentation across instances
- File uploads lost when instance scales down
- CDN invalidation issues

**Azure Solution Required**:
- Migrate to Azure Blob Storage for file storage
- Implement Azure CDN for static assets
- Use Azure Search or maintain Lucene in blob storage
- Estimated effort: 80-120 hours

---

#### ?? **CONCERN 5: Database Connection Pooling**
**Location**: Cms.DBHelper.cs, Database.cs
**Evidence**:
```csharp
// Database.cs - Connection management
public class dbHelper {
    private SqlDataAdapter moDataAdpt;
    private string GetDBAuth();
    public void ResetConnection(string cConnectionString);
}

// Multiple direct connection instantiations
var oCon = new System.Data.SqlClient.SqlConnection(ConStr);
oCon.Open();
```

**Impact**:
- Connection exhaustion under load
- Potential connection leaks
- No connection retry logic
- Not optimized for Azure SQL elastic pools

**Azure Solution Required**:
- Implement connection resilience with Polly
- Use Azure SQL connection pooling best practices
- Add retry logic for transient faults
- Estimated effort: 20-30 hours

---

#### ?? **CONCERN 6: Performance Monitoring Without Distributed Tracing**
**Location**: PerfLog.cs
**Evidence**:
```csharp
// PerfLog.cs - Custom performance logging
public class PerfLog {
    private string cDataConn = "";
    private bool bLoggingOn;
    private PerformanceCounter _workingSetPrivateMemoryCounter;
    private PerformanceCounter _workingSetMemoryCounter;
    
    public void Log(string cModuleName, string cProcessName, string cDescription = "");
    public void Write(); // Writes to database
}
```

**Impact**:
- No correlation across instances
- Difficult to trace requests through scaled environment
- No Azure-native monitoring integration

**Azure Solution Required**:
- Integrate Azure Application Insights
- Implement distributed tracing
- Estimated effort: 40-50 hours

---

## Azure Architecture Recommendations

### Phase 1: Enable Basic Auto-Scaling (3-4 months)

#### Step 1.1: External Session State Migration
**Priority**: CRITICAL
**Effort**: 40-60 hours

**Implementation**:
```xml
<!-- Web.config -->
<configuration>
  <system.web>
    <sessionState mode="Custom" customProvider="RedisSessionStateProvider">
      <providers>
        <add name="RedisSessionStateProvider" 
             type="Microsoft.Web.Redis.RedisSessionStateProvider"
             host="{your-redis-cache-name}.redis.cache.windows.net" 
             accessKey="{your-access-key}" 
             ssl="true" 
             port="6380" />
      </providers>
    </sessionState>
  </system.web>
</configuration>
```

**Azure Resources**:
- Azure Cache for Redis (Standard or Premium tier)
- Estimated cost: $80-400/month depending on tier

**Testing Required**:
- Shopping cart persistence across instances
- Admin session workflows
- User authentication state
- File upload sessions

---

#### Step 1.2: Distributed Caching Implementation
**Priority**: CRITICAL
**Effort**: 60-80 hours

**Code Changes Required**:
```csharp
// Replace throughout codebase
// OLD:
HttpContext.Current.Cache.Insert(key, value, ...);

// NEW:
public class DistributedCacheService {
    private readonly IDistributedCache _cache;
    
    public async Task SetAsync<T>(string key, T value, TimeSpan expiration) {
        var serialized = JsonSerializer.Serialize(value);
        var options = new DistributedCacheEntryOptions {
            AbsoluteExpirationRelativeToNow = expiration
        };
        await _cache.SetStringAsync(key, serialized, options);
    }
    
    public async Task<T> GetAsync<T>(string key) {
        var data = await _cache.GetStringAsync(key);
        return data == null ? default(T) : JsonSerializer.Deserialize<T>(data);
    }
}
```

**Azure Resources**:
- Same Redis instance can serve both session and cache
- Consider Redis Premium for clustering if high volume

---

#### Step 1.3: Blob Storage Migration
**Priority**: CRITICAL
**Effort**: 80-120 hours

**Implementation Strategy**:
```csharp
// Create abstraction layer
public interface IFileStorageService {
    Task<string> UploadFileAsync(Stream fileStream, string path, string fileName);
    Task<Stream> DownloadFileAsync(string path, string fileName);
    Task<bool> DeleteFileAsync(string path, string fileName);
    Task<List<string>> ListFilesAsync(string path);
}

public class AzureBlobStorageService : IFileStorageService {
    private readonly BlobServiceClient _blobServiceClient;
    
    public async Task<string> UploadFileAsync(Stream fileStream, string path, string fileName) {
        var containerClient = _blobServiceClient.GetBlobContainerClient("uploads");
        var blobClient = containerClient.GetBlobClient($"{path}/{fileName}");
        await blobClient.UploadAsync(fileStream, overwrite: true);
        return blobClient.Uri.ToString();
    }
    // ... implement other methods
}
```

**Refactor Required**:
- `fsHelper.cs` - Replace all File I/O operations
- `ImageHelper.cs` - Modify image processing to use blob storage
- Upload handlers - Change destination to blob storage
- Ensure Azure CDN configured for blob container

**Azure Resources**:
- Azure Blob Storage (Hot tier for active files)
- Azure CDN (Standard Microsoft tier)
- Estimated cost: $20-100/month + bandwidth

---

#### Step 1.4: Configuration Externalization
**Priority**: HIGH
**Effort**: 30-40 hours

**Implementation**:
```csharp
// Replace static configuration
public class AzureConfigurationService {
    private readonly IConfiguration _configuration;
    
    public AzureConfigurationService(IConfiguration configuration) {
        _configuration = configuration;
    }
    
    public string GetSetting(string key) {
        // Reads from Azure App Configuration with fallback to local config
        return _configuration[key];
    }
}

// Inject throughout application
public class Cms {
    private readonly IConfigurationService _configService;
    
    // Replace all static fields with service calls
    public bool Membership => _configService.GetBool("Membership");
    public bool Cart => _configService.GetBool("Cart");
}
```

**Azure Resources**:
- Azure App Configuration (Standard tier)
- Azure Key Vault for secrets
- Estimated cost: $1-10/month

---

### Phase 2: Azure App Service Configuration (1-2 weeks)

#### Step 2.1: App Service Plan Setup
**Recommended Configuration**:
- **Tier**: Standard S2 or Premium P1V2 minimum
  - S2: 2 cores, 3.5GB RAM, $146/month
  - P1V2: 1 core, 3.5GB RAM, $96/month (better for auto-scale)
- **Auto-scale rules**:
  ```yaml
  Scale Out When:
    - CPU > 70% for 10 minutes -> +1 instance
    - Memory > 80% for 10 minutes -> +1 instance
    - HTTP Queue Length > 25 for 5 minutes -> +1 instance
  
  Scale In When:
    - CPU < 30% for 20 minutes -> -1 instance
    - Minimum instances: 2 (for high availability)
    - Maximum instances: 10
  ```

#### Step 2.2: Application Settings
**Web.config transformations**:
```xml
<configuration>
  <connectionStrings>
    <add name="ProteanCMS" 
         connectionString="#{ConnectionString}#" />
  </connectionStrings>
  
  <appSettings>
    <add key="RedisConnection" value="#{RedisConnection}#" />
    <add key="BlobStorageConnection" value="#{BlobStorageConnection}#" />
    <add key="ApplicationInsightsKey" value="#{AppInsightsKey}#" />
    <add key="CDNEndpoint" value="#{CDNEndpoint}#" />
  </appSettings>
</configuration>
```

#### Step 2.3: Health Probes
**Implementation**:
```csharp
// HealthCheck.ashx
public class HealthCheck : IHttpHandler {
    public void ProcessRequest(HttpContext context) {
        try {
            // Check database connectivity
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ProteanCMS"].ConnectionString)) {
                conn.Open();
            }
            
            // Check Redis connectivity
            var redis = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["RedisConnection"]);
            redis.GetDatabase().Ping();
            
            // Check blob storage
            var blobClient = new BlobServiceClient(ConfigurationManager.AppSettings["BlobStorageConnection"]);
            await blobClient.GetAccountInfoAsync();
            
            context.Response.StatusCode = 200;
            context.Response.Write("Healthy");
        }
        catch (Exception ex) {
            context.Response.StatusCode = 503;
            context.Response.Write($"Unhealthy: {ex.Message}");
        }
    }
}
```

**Azure Configuration**:
- Health check path: `/healthcheck.ashx`
- Probe interval: 30 seconds
- Unhealthy threshold: 3 consecutive failures

---

### Phase 3: Database Optimization for Scale (2-3 weeks)

#### Step 3.1: Azure SQL Configuration
**Recommended Setup**:
- **Tier**: Standard S3 or Premium P2
- **Features to Enable**:
  - Read Scale-Out (for reporting queries)
  - Auto-tuning
  - Intelligent Insights
  - Connection resiliency

#### Step 3.2: Connection Resilience Implementation
```csharp
// Update Database.cs
public class Database {
    private static readonly IAsyncPolicy _retryPolicy = Policy
        .Handle<SqlException>(ex => IsTransient(ex))
        .WaitAndRetryAsync(3, 
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            (exception, timeSpan, retryCount, context) => {
                // Log retry attempts
            });
    
    public async Task<SqlConnection> GetConnectionAsync() {
        return await _retryPolicy.ExecuteAsync(async () => {
            var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync();
            return connection;
        });
    }
    
    private static bool IsTransient(SqlException ex) {
        // Check for transient error codes
        var transientErrors = new[] { 
            -2, -1, 4060, 40197, 40501, 40613, 49918, 49919, 49920 
        };
        return transientErrors.Contains(ex.Number);
    }
}
```

#### Step 3.3: Query Optimization
**Critical Areas**:
```sql
-- Add missing indexes (from DBHelper.cs analysis)
CREATE NONCLUSTERED INDEX IX_tblContent_nStatus_dExpireDate 
ON tblContent(nStatus, dExpireDate) 
INCLUDE (nContentKey, cContentName);

CREATE NONCLUSTERED INDEX IX_tblContentLocation_nStructId_nContentId
ON tblContentLocation(nStructId, nContentId)
INCLUDE (bPrimary, nDisplayOrder);

-- Enable Query Store
ALTER DATABASE [ProteanCMS] SET QUERY_STORE = ON;
```

---

### Phase 4: Search Index Distribution (2-3 weeks)

#### Step 4.1: Lucene Index Strategy
**Current Problem**: Local file-based Lucene indexes
**Options**:

**Option A: Azure Blob Storage for Lucene (Faster implementation)**
```csharp
// Update Indexer.cs and IndexerAsync.cs
public class AzureBlobLuceneDirectory : Directory {
    private readonly BlobContainerClient _containerClient;
    
    public AzureBlobLuceneDirectory(string connectionString, string containerName) {
        _containerClient = new BlobServiceClient(connectionString)
            .GetBlobContainerClient(containerName);
        _containerClient.CreateIfNotExists();
    }
    
    // Implement Lucene Directory methods to read/write from blob storage
}
```
- **Pros**: Minimal code changes, keeps existing Lucene infrastructure
- **Cons**: Slower than local file system, requires blob storage sync
- **Effort**: 40-60 hours

**Option B: Migrate to Azure Cognitive Search (Recommended long-term)**
```csharp
public class AzureSearchService : ISearchService {
    private readonly SearchClient _searchClient;
    
    public async Task IndexDocumentAsync(SearchDocument document) {
        await _searchClient.IndexDocumentsAsync(
            IndexDocumentsBatch.Upload(new[] { document })
        );
    }
    
    public async Task<SearchResults<T>> SearchAsync<T>(string query) {
        var options = new SearchOptions {
            IncludeTotalCount = true,
            Filter = "status eq 'live'"
        };
        return await _searchClient.SearchAsync<T>(query, options);
    }
}
```
- **Pros**: Azure-native, scalable, managed service
- **Cons**: Complete rewrite of search logic
- **Effort**: 120-160 hours
- **Cost**: $75-250/month for Basic tier

**Recommendation**: Start with Option A for Phase 1, plan Option B for Phase 2

---

### Phase 5: Monitoring & Observability (1-2 weeks)

#### Step 5.1: Application Insights Integration
```csharp
// Global.asax.cs
public class Global : HttpApplication {
    protected void Application_Start() {
        TelemetryConfiguration.Active.InstrumentationKey = 
            ConfigurationManager.AppSettings["ApplicationInsightsKey"];
        
        // Add custom telemetry initializer
        TelemetryConfiguration.Active.TelemetryInitializers.Add(
            new CustomTelemetryInitializer());
    }
}

// Replace PerfLog.cs usage
public class TelemetryService {
    private readonly TelemetryClient _telemetryClient;
    
    public void TrackRequest(string name, TimeSpan duration, bool success) {
        _telemetryClient.TrackRequest(name, DateTimeOffset.Now, duration, 
            success ? "200" : "500", success);
    }
    
    public void TrackDependency(string dependencyType, string target, 
        string name, TimeSpan duration, bool success) {
        _telemetryClient.TrackDependency(dependencyType, target, name, 
            null, DateTimeOffset.Now, duration, 
            success ? "200" : "500", success);
    }
}
```

#### Step 5.2: Custom Metrics
```csharp
// Track business metrics
_telemetryClient.TrackMetric("ShoppingCartValue", cartTotal);
_telemetryClient.TrackMetric("CheckoutCompletionRate", completionRate);
_telemetryClient.TrackEvent("ProductPurchased", properties);
```

---

## Implementation Timeline

### Phase 1: Foundation (Months 1-3)
- **Week 1-4**: Session state migration to Redis
- **Week 5-8**: Distributed caching implementation
- **Week 9-12**: Blob storage migration

### Phase 2: Azure Deployment (Month 4)
- **Week 13-14**: App Service setup and configuration
- **Week 15-16**: Load testing and optimization

### Phase 3: Database & Search (Months 5-6)
- **Week 17-20**: Database optimization
- **Week 21-24**: Search index distribution

### Phase 4: Monitoring (Month 7)
- **Week 25-28**: Application Insights integration and testing

---

## Cost Estimation

### Azure Resources (Monthly, USD)

| Resource | Tier | Estimated Cost |
|----------|------|---------------|
| App Service Plan | Premium P1V2 (2-10 instances) | $192 - $960 |
| Azure Cache for Redis | Standard C2 (2.5GB) | $123 |
| Azure SQL Database | Standard S3 (100 DTU) | $304 |
| Azure Blob Storage | Hot tier + CDN | $50 - $150 |
| Application Insights | Basic (5GB/day) | $14.50 |
| Azure App Configuration | Standard | $1.20 |
| **Total Monthly** | | **$685 - $1,553** |

### One-Time Development Costs

| Phase | Hours | Rate | Estimated Cost |
|-------|-------|------|---------------|
| Session State Migration | 50 | $150/hr | $7,500 |
| Distributed Caching | 70 | $150/hr | $10,500 |
| Blob Storage Migration | 100 | $150/hr | $15,000 |
| Configuration Externalization | 35 | $150/hr | $5,250 |
| Database Optimization | 60 | $150/hr | $9,000 |
| Search Index Migration | 50 | $150/hr | $7,500 |
| Monitoring Integration | 45 | $150/hr | $6,750 |
| Testing & QA | 80 | $150/hr | $12,000 |
| **Total Development** | **490 hours** | | **$73,500** |

---

## Risk Assessment

### High-Risk Areas

1. **Shopping Cart Session Loss**
   - **Risk**: Cart data loss during migration
   - **Mitigation**: Dual-write pattern during transition, extensive testing
   - **Rollback**: Keep session fallback mechanism

2. **File Upload Data Loss**
   - **Risk**: Files uploaded during migration might be lost
   - **Mitigation**: Implement sync mechanism during transition
   - **Rollback**: Keep local file system temporarily

3. **Search Index Corruption**
   - **Risk**: Index corruption during blob migration
   - **Mitigation**: Maintain backup of local index, rebuild capability
   - **Rollback**: Revert to local index

4. **Performance Degradation**
   - **Risk**: Distributed resources slower than local
   - **Mitigation**: Extensive load testing, caching optimization
   - **Rollback**: Vertical scaling initially

### Medium-Risk Areas

1. **Database Connection Exhaustion**
   - **Risk**: Connection pool exhaustion under load
   - **Mitigation**: Connection pooling configuration, monitoring
   
2. **Static Configuration Issues**
   - **Risk**: Configuration inconsistency across instances
   - **Mitigation**: Gradual migration with validation

---

## Testing Strategy

### Load Testing Requirements
```yaml
Test Scenarios:
  - Concurrent users: 100, 500, 1000, 2000
  - Shopping cart operations: 50 req/sec sustained
  - Content delivery: 200 req/sec sustained
  - File uploads: 20 concurrent
  - Search queries: 100 req/sec sustained
  
Tools:
  - Azure Load Testing service
  - JMeter for custom scenarios
  - Application Insights for monitoring
  
Success Criteria:
  - 99.9% availability
  - < 2 second page load time (95th percentile)
  - < 500ms API response time (95th percentile)
  - Zero data loss during scale operations
```

### Acceptance Criteria
? Session state persists across instance switches
? Shopping carts maintain integrity during scaling
? File uploads accessible from all instances
? Search results consistent across instances
? No performance degradation under load
? Auto-scaling triggers appropriately
? Graceful degradation if Redis fails
? Complete audit trail in Application Insights

---

## Recommendations Priority Matrix

| Priority | Item | Impact | Effort | Timeline |
|----------|------|--------|--------|----------|
| ?? P0 | Session State Migration | Critical | High | Month 1 |
| ?? P0 | Distributed Caching | Critical | High | Month 2 |
| ?? P0 | Blob Storage Migration | Critical | Very High | Month 3 |
| ?? P1 | Configuration Externalization | High | Medium | Month 4 |
| ?? P1 | Database Optimization | High | Medium | Month 5 |
| ?? P1 | Search Index Distribution | High | High | Month 6 |
| ?? P2 | Application Insights | Medium | Medium | Month 7 |
| ?? P2 | Advanced Monitoring | Low | Low | Month 8 |

---

## Long-Term Modernization Path

### Phase 2 (12-18 months): Full Modernization
1. **Migrate to .NET 8**
   - Rewrite as ASP.NET Core application
   - Move to Azure App Service for Linux (cheaper)
   - Enable WebSocket support for real-time features
   
2. **Microservices Architecture**
   - Separate Cart into Azure Function
   - Extract Search into dedicated service
   - API Gateway for unified entry point
   
3. **Event-Driven Architecture**
   - Azure Event Grid for order processing
   - Azure Service Bus for async operations
   - Event sourcing for audit trail
   
4. **Container-Based Deployment**
   - Docker containerization
   - Azure Kubernetes Service for orchestration
   - Further improve scalability and cost

---

## Conclusion

**Can ProteanCMS auto-scale in Azure today?** 
**No** - Critical architectural blockers prevent effective auto-scaling.

**Can it be made to auto-scale?** 
**Yes** - With 3-4 months of focused development effort (~490 hours) and investment of ~$75K in development costs, the application can be refactored to support auto-scaling.

**Key Success Factors**:
1. Executive commitment to 3-4 month project
2. Dedicated development team (2-3 developers)
3. Budget for Azure resources ($700-1,500/month ongoing)
4. Comprehensive testing strategy
5. Phased rollout with rollback capability

**Immediate Next Steps**:
1. Validate cost estimates and timeline with stakeholders
2. Set up Azure environment (Redis, SQL, Blob Storage)
3. Begin Phase 1: Session State Migration
4. Establish monitoring baselines before changes
5. Create comprehensive rollback procedures

---

## Appendix: Code Patterns to Replace

### Pattern 1: Session State Access
```csharp
// ? OLD - Will break in auto-scale
if (moSession["UserId"] != null) {
    userId = (int)moSession["UserId"];
}

// ? NEW - Works across instances
var userId = await _sessionService.GetAsync<int>("UserId");
```

### Pattern 2: Application Cache
```csharp
// ? OLD - Instance-specific cache
var menuXml = (XmlDocument)HttpContext.Current.Cache["MenuXml"];
if (menuXml == null) {
    menuXml = BuildMenuXml();
    HttpContext.Current.Cache.Insert("MenuXml", menuXml, ...);
}

// ? NEW - Distributed cache
var menuXml = await _cache.GetAsync<XmlDocument>("MenuXml");
if (menuXml == null) {
    menuXml = BuildMenuXml();
    await _cache.SetAsync("MenuXml", menuXml, TimeSpan.FromMinutes(30));
}
```

### Pattern 3: File Operations
```csharp
// ? OLD - Local file system
var filePath = Server.MapPath("/uploads/" + fileName);
File.WriteAllBytes(filePath, fileData);

// ? NEW - Azure Blob Storage
var blobClient = _blobContainer.GetBlobClient($"uploads/{fileName}");
using (var stream = new MemoryStream(fileData)) {
    await blobClient.UploadAsync(stream, overwrite: true);
}
```

---

**Document Version**: 1.0  
**Last Updated**: 2024  
**Author**: Azure Solutions Architect  
**Status**: For Review and Approval
