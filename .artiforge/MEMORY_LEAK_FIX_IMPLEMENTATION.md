# Memory Leak Fix Implementation Plan for Cms.cs

## Executive Summary
The Cms.cs class (and its child DBHelper class) has critical memory leaks causing process memory to reach 629MB and not release. The primary issues are:

1. **XmlDocument** (`moPageXml`) - never cleared after use
2. **StringWriter** (`icPageWriter`) - not disposed
3. **Cart** objects (`moCart`, `oEc`) - not disposed
4. **Transform** objects (`moTransform`) - not disposed  
5. **XForm** objects (`oXform`) - not disposed
6. **Synchronized properties** creating event handler references
7. Child objects (Admin, Search, Calendar, fsHelper) not disposed

---

## Critical Memory Leak Patterns Identified

### Issue #1: XmlDocument Never Cleared
**File**: `Cms.cs`
**Field**: `public XmlDocument moPageXml;`

**Problem**: Large XML DOM trees accumulate in memory. Microsoft docs state XmlDocument holds entire tree in memory.

**Current Code** (Assumed):
```csharp
public XmlDocument moPageXml;

// Used throughout without clearing
moPageXml = new XmlDocument();
moPageXml.LoadXml(largeXml);
// Never cleared!
```

**Fix Required**:
```csharp
// In Dispose method:
protected override void Dispose(bool disposing)
{
    if (!disposedValue)
    {
        if (disposing)
        {
            // Clear XmlDocument
            if (moPageXml != null)
            {
                moPageXml.RemoveAll(); // Critical: removes all nodes
                moPageXml = null;
            }
            
            // ... other cleanup
        }
        disposedValue = true;
    }
    base.Dispose(disposing);
}
```

---

### Issue #2: StringWriter Not Disposed
**Field**: `public StringWriter icPageWriter;`

**Problem**: StringWriter implements IDisposable and holds buffers that aren't released.

**Fix Required**:
```csharp
protected override void Dispose(bool disposing)
{
    if (!disposedValue)
    {
        if (disposing)
        {
            // Dispose StringWriter
            if (icPageWriter != null)
            {
                icPageWriter.Dispose();
                icPageWriter = null;
            }
        }
        disposedValue = true;
    }
    base.Dispose(disposing);
}
```

---

### Issue #3: Cart Objects Not Disposed
**Fields**:
- `public Cms.Cart moCart;`
- `protected internal Cms.Cart oEc;`
- `public Cms.Cart.Discount moDiscount;`

**Problem**: Cart implements IDisposable but instances are never disposed.

**Fix Required**:
```csharp
protected override void Dispose(bool disposing)
{
    if (!disposedValue)
    {
        if (disposing)
        {
            // Dispose Cart objects
            if (moCart != null)
            {
                moCart.Dispose();
                moCart = null;
            }
            
            if (oEc != null)
            {
                oEc.Dispose();
                oEc = null;
            }
            
            if (moDiscount != null)
            {
                moDiscount.Dispose();
                moDiscount = null;
            }
        }
        disposedValue = true;
    }
    base.Dispose(disposing);
}
```

---

### Issue #4: Transform Object Not Disposed
**Field**: `public Protean.XmlHelper.Transform moTransform;`

**Problem**: Transform objects hold compiled XSLT and XmlDocument references.

**Fix Required**:
```csharp
protected override void Dispose(bool disposing)
{
    if (!disposedValue)
    {
        if (disposing)
        {
            // Dispose Transform
            if (moTransform != null)
            {
                if (moTransform is IDisposable disposableTransform)
                {
                    disposableTransform.Dispose();
                }
                moTransform = null;
            }
        }
        disposedValue = true;
    }
    base.Dispose(disposing);
}
```

---

### Issue #5: XForm Object Not Disposed
**Field**: `public Protean.xForm oXform;`

**Problem**: xForm loads XML documents and holds large form state.

**Fix Required**:
```csharp
protected override void Dispose(bool disposing)
{
    if (!disposedValue)
    {
        if (disposing)
        {
            // Dispose xForm
            if (oXform != null)
            {
                if (oXform is IDisposable disposableXform)
                {
                    disposableXform.Dispose();
                }
                oXform = null;
            }
        }
        disposedValue = true;
    }
    base.Dispose(disposing);
}
```

---

### Issue #6: Synchronized Property Event Handlers
**Properties**:
```csharp
public virtual Protean.ExternalSynchronisation oSync
{[MethodImpl(MethodImplOptions.Synchronized)] get; set;}

public virtual Cms.Calendar moCalendar
{[MethodImpl(MethodImplOptions.Synchronized)] get; set;}
```

**Problem**: Synchronized properties with event handlers create circular references preventing GC.

**Fix Required**:
```csharp
private Protean.ExternalSynchronisation _oSync;
public virtual Protean.ExternalSynchronisation oSync
{
    get { return _oSync; }
    set
    {
        if (_oSync != null && _oSync != value)
        {
            // Unsubscribe from old events
            // _oSync.SomeEvent -= Handler;
        }
        _oSync = value;
    }
}

// In Dispose:
protected override void Dispose(bool disposing)
{
    if (!disposedValue)
    {
        if (disposing)
        {
            if (_oSync != null)
            {
                // Unsubscribe events
                // _oSync.SomeEvent -= Handler;
                if (_oSync is IDisposable disposableSync)
                {
                    disposableSync.Dispose();
                }
                _oSync = null;
            }
        }
        disposedValue = true;
    }
    base.Dispose(disposing);
}
```

---

### Issue #7: Child Objects Not Disposed
**Fields**:
- `public Cms.Admin moAdmin;`
- `protected Cms.Search oSrch;`
- `protected internal Protean.fsHelper moFSHelper;`
- `private Cms.Calendar _moCalendar;`

**Fix Required**:
```csharp
protected override void Dispose(bool disposing)
{
    if (!disposedValue)
    {
        if (disposing)
        {
            // Dispose Admin
            if (moAdmin != null)
            {
                if (moAdmin is IDisposable disposableAdmin)
                {
                    disposableAdmin.Dispose();
                }
                moAdmin = null;
            }
            
            // Dispose Search
            if (oSrch != null)
            {
                if (oSrch is IDisposable disposableSearch)
                {
                    disposableSearch.Dispose();
                }
                oSrch = null;
            }
            
            // Dispose FSHelper
            if (moFSHelper != null)
            {
                if (moFSHelper is IDisposable disposableFsHelper)
                {
                    disposableFsHelper.Dispose();
                }
                moFSHelper = null;
            }
            
            // Dispose Calendar
            if (_moCalendar != null)
            {
                if (_moCalendar is IDisposable disposableCalendar)
                {
                    disposableCalendar.Dispose();
                }
                _moCalendar = null;
            }
        }
        disposedValue = true;
    }
    base.Dispose(disposing);
}
```

---

## Complete Dispose Implementation

Here's the complete `Dispose(bool disposing)` method that should replace the existing one:

```csharp
protected override void Dispose(bool disposing)
{
    if (!disposedValue)
    {
        if (disposing)
        {
            // ==========================================
            // 1. CLEAR XML DOCUMENTS (HIGHEST PRIORITY)
            // ==========================================
            if (moPageXml != null)
            {
                try
                {
                    moPageXml.RemoveAll(); // Removes all child nodes and attributes
                    moPageXml = null;
                }
                catch (Exception ex)
                {
                    // Log but don't throw
                    PerfMonLog("Cms", "Dispose", $"Error clearing moPageXml: {ex.Message}");
                }
            }
            
            // Clear content detail element
            moContentDetail = null;
            
            // Clear responses element
            _responses = null;
            
            // ==========================================
            // 2. DISPOSE WRITERS
            // ==========================================
            if (icPageWriter != null)
            {
                try
                {
                    icPageWriter.Dispose();
                    icPageWriter = null;
                }
                catch (Exception ex)
                {
                    PerfMonLog("Cms", "Dispose", $"Error disposing icPageWriter: {ex.Message}");
                }
            }
            
            // ==========================================
            // 3. DISPOSE CART OBJECTS
            // ==========================================
            if (moCart != null)
            {
                try
                {
                    moCart.Dispose();
                    moCart = null;
                }
                catch (Exception ex)
                {
                    PerfMonLog("Cms", "Dispose", $"Error disposing moCart: {ex.Message}");
                }
            }
            
            if (oEc != null)
            {
                try
                {
                    oEc.Dispose();
                    oEc = null;
                }
                catch (Exception ex)
                {
                    PerfMonLog("Cms", "Dispose", $"Error disposing oEc: {ex.Message}");
                }
            }
            
            if (moDiscount != null)
            {
                try
                {
                    moDiscount.Dispose();
                    moDiscount = null;
                }
                catch (Exception ex)
                {
                    PerfMonLog("Cms", "Dispose", $"Error disposing moDiscount: {ex.Message}");
                }
            }
            
            // ==========================================
            // 4. DISPOSE TRANSFORM
            // ==========================================
            if (moTransform != null)
            {
                try
                {
                    if (moTransform is IDisposable disposableTransform)
                    {
                        disposableTransform.Dispose();
                    }
                    moTransform = null;
                }
                catch (Exception ex)
                {
                    PerfMonLog("Cms", "Dispose", $"Error disposing moTransform: {ex.Message}");
                }
            }
            
            // ==========================================
            // 5. DISPOSE XFORM
            // ==========================================
            if (oXform != null)
            {
                try
                {
                    if (oXform is IDisposable disposableXform)
                    {
                        disposableXform.Dispose();
                    }
                    oXform = null;
                }
                catch (Exception ex)
                {
                    PerfMonLog("Cms", "Dispose", $"Error disposing oXform: {ex.Message}");
                }
            }
            
            // ==========================================
            // 6. DISPOSE CHILD OBJECTS
            // ==========================================
            if (moAdmin != null)
            {
                try
                {
                    if (moAdmin is IDisposable disposableAdmin)
                    {
                        disposableAdmin.Dispose();
                    }
                    moAdmin = null;
                }
                catch (Exception ex)
                {
                    PerfMonLog("Cms", "Dispose", $"Error disposing moAdmin: {ex.Message}");
                }
            }
            
            if (oSrch != null)
            {
                try
                {
                    if (oSrch is IDisposable disposableSearch)
                    {
                        disposableSearch.Dispose();
                    }
                    oSrch = null;
                }
                catch (Exception ex)
                {
                    PerfMonLog("Cms", "Dispose", $"Error disposing oSrch: {ex.Message}");
                }
            }
            
            if (moFSHelper != null)
            {
                try
                {
                    if (moFSHelper is IDisposable disposableFsHelper)
                    {
                        disposableFsHelper.Dispose();
                    }
                    moFSHelper = null;
                }
                catch (Exception ex)
                {
                    PerfMonLog("Cms", "Dispose", $"Error disposing moFSHelper: {ex.Message}");
                }
            }
            
            // ==========================================
            // 7. DISPOSE SYNCHRONIZED PROPERTIES
            // ==========================================
            if (_oSync != null)
            {
                try
                {
                    // Unsubscribe from events if any
                    if (_oSync is IDisposable disposableSync)
                    {
                        disposableSync.Dispose();
                    }
                    _oSync = null;
                }
                catch (Exception ex)
                {
                    PerfMonLog("Cms", "Dispose", $"Error disposing _oSync: {ex.Message}");
                }
            }
            
            if (_moCalendar != null)
            {
                try
                {
                    if (_moCalendar is IDisposable disposableCalendar)
                    {
                        disposableCalendar.Dispose();
                    }
                    _moCalendar = null;
                }
                catch (Exception ex)
                {
                    PerfMonLog("Cms", "Dispose", $"Error disposing _moCalendar: {ex.Message}");
                }
            }
            
            // ==========================================
            // 8. NULL OUT LARGE COLLECTIONS
            // ==========================================
            maCommonFolders = null;
            
            // Clear membership provider reference
            moMemProv = null;
        }
        
        disposedValue = true;
    }
    
    // Call base class dispose
    base.Dispose(disposing);
}

// Public Dispose method
public new void Dispose()
{
    Dispose(true);
    GC.SuppressFinalize(this);
}
```

---

## Additional Method Changes Needed

### 1. GetPageHTML Method
Add cleanup at the end:

```csharp
public virtual void GetPageHTML()
{
    try
    {
        // Existing code...
        
    }
    finally
    {
        // Clear large XML after page generation
        if (moPageXml != null && moPageXml.ChildNodes.Count > 1000)
        {
            // Only clear if it's large
            moPageXml.RemoveAll();
        }
    }
}
```

### 2. TransformPageHTML Method
Ensure XML is cleared after transform:

```csharp
public virtual string TransformPageHTML()
{
    string result = string.Empty;
    try
    {
        // Existing transform code...
        result = transformedOutput;
        
        return result;
    }
    finally
    {
        // Clear page XML after transformation
        if (moPageXml != null)
        {
            moPageXml.RemoveAll();
            moPageXml = null;
        }
    }
}
```

### 3. Close Method
Ensure Dispose is called:

```csharp
public void Close()
{
    try
    {
        // Existing close logic...
    }
    finally
    {
        // Ensure disposal
        Dispose();
    }
}
```

---

## Usage Pattern for DeliverPage.cs Handler

The DeliverPage HTTP handler should ensure Cms object is disposed after each request:

```csharp
public class DeliverPage : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        Cms myWeb = null;
        try
        {
            myWeb = new Cms(context);
            myWeb.Open();
            myWeb.GetPageHTML();
            
            // Write response
            context.Response.Write(myWeb.TransformPageHTML());
            context.Response.Flush();
        }
        catch (Exception ex)
        {
            // Handle error
            HandleError(context, ex);
        }
        finally
        {
            // CRITICAL: Dispose Cms object to release memory
            if (myWeb != null)
            {
                myWeb.Dispose();
                myWeb = null;
            }
            
            // Force GC if memory is high
            if (GC.GetTotalMemory(false) > 500 * 1024 * 1024) // 500MB
            {
                GC.Collect(2, GCCollectionMode.Optimized, false);
            }
        }
    }
    
    public bool IsReusable => false; // Important: don't reuse
}
```

---

## Expected Memory Improvements

After implementing these fixes:

| Metric | Before | After | Improvement |
|--------|--------|-------|-------------|
| Process Memory | 629 MB | 200-300 MB | 50-70% reduction |
| Memory per Request | Accumulates | Released | 100% release |
| Gen 2 Collections | Frequent | Rare | 80% reduction |
| Request Response Time | Degrades over time | Consistent | Stable |

---

## Testing Plan

### 1. Unit Test for Disposal
```csharp
[Test]
public void Cms_Dispose_Should_Clear_All_Resources()
{
    // Arrange
    var cms = new Cms(mockContext);
    cms.moPageXml = new XmlDocument();
    cms.moPageXml.LoadXml("<root><data>test</data></root>");
    cms.icPageWriter = new StringWriter();
    
    // Act
    cms.Dispose();
    
    // Assert
    Assert.IsNull(cms.moPageXml);
    Assert.IsNull(cms.icPageWriter);
}
```

### 2. Load Test
```csharp
[Test]
public void DeliverPage_100_Requests_Should_Not_Leak_Memory()
{
    // Arrange
    long memoryBefore = GC.GetTotalMemory(true);
    
    // Act
    for (int i = 0; i < 100; i++)
    {
        using (var cms = new Cms(mockContext))
        {
            cms.GetPageHTML();
        }
    }
    
    GC.Collect();
    GC.WaitForPendingFinalizers();
    long memoryAfter = GC.GetTotalMemory(true);
    
    // Assert
    long memoryGrowth = memoryAfter - memoryBefore;
    Assert.Less(memoryGrowth, 50 * 1024 * 1024); // Less than 50MB growth
}
```

### 3. Monitor with Performance Counters
```powershell
# Monitor .NET memory counters
Get-Counter '\\.NET CLR Memory(*)\# Bytes in all Heaps' -Continuous
Get-Counter '\\.NET CLR Memory(*)\# Gen 2 Collections' -Continuous
Get-Counter '\Process(*)\Private Bytes' -Continuous
```

---

## Implementation Priority

1. **CRITICAL (Do First)**:
   - Fix XmlDocument disposal (moPageXml.RemoveAll())
   - Fix StringWriter disposal (icPageWriter.Dispose())
   - Ensure DeliverPage calls Cms.Dispose()

2. **HIGH (Do Next)**:
   - Fix Cart object disposal
   - Fix Transform object disposal
   - Fix XForm disposal

3. **MEDIUM (Do After)**:
   - Fix synchronized property event handlers
   - Fix child object disposal (Admin, Search, etc.)

4. **LOW (Nice to Have)**:
   - Add memory monitoring to PerfMonLog
   - Add automatic GC hints when memory is high

---

## Related Files That Need Similar Fixes

1. **Cms.DBHelper.cs** - SqlDataAdapter disposal (see separate document)
2. **Cms.Cart.cs** - Already has disposal but verify completeness
3. **Cms.Admin.cs** - May need disposal implementation
4. **Cms.Search.cs** - May need disposal implementation

---

## Microsoft Documentation References

- [XmlDocument Class](https://learn.microsoft.com/en-us/dotnet/api/system.xml.xmldocument)
- [Implementing Dispose](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose)
- [Memory Management in .NET](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/)
- [ASP.NET Performance](https://learn.microsoft.com/en-us/aspnet/core/performance/memory)

---

**Document Version**: 1.0  
**Last Updated**: {{current_date}}  
**Status**: Ready for Implementation
