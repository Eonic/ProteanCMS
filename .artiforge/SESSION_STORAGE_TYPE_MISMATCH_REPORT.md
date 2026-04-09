# Session Storage Type Mismatch Report - mnUserId Upgrade

**Issue**: `mnUserId` field was upgraded from `int` to `long`, causing `InvalidCastException` on second request due to session storage type mismatches.

**Status**: ? ROOT CAUSE IDENTIFIED

---

## Critical Issues Found

### 1. **REST.cs - Line ~124** (CRITICAL - PUBLIC API)
```csharp
mnUserId = Convert.ToInt16(oMembershipProv.Activities.GetUserId(ref myWeb));
```
**Problem**: Converting to `Int16` instead of `long`  
**Impact**: Session initialization fails on second request  
**Fix**: Change to `Convert.ToInt64()`

---

### 2. **Rest.cs - ValidateAPICall() - Line ~359** (CRITICAL)
```csharp
nUserId = Convert.ToInt32(myWeb.moSession["nUserId"]);
```
**Problem**: Converting session value to `Int32` instead of `long`  
**Impact**: Type mismatch when session stores `long` value  
**Fix**: Change to `Convert.ToInt64()`

---

### 3. **Rest.cs - ValidateAPICall() - Line ~372** (CRITICAL)
```csharp
nUserId = (int)Convert.ToInt64(sValidResponse);
```
**Problem**: Casting `Int64` to `int`  
**Impact**: Loses data for large user IDs  
**Fix**: Remove cast, keep as `long`

---

### 4. **Membership.Async.cs** (CRITICAL)
```csharp
mnUserId = Convert.ToInt16(oMembershipProv.Activities.GetUserId(ref myWeb));
```
**Problem**: Same as REST.cs issue  
**Fix**: Change to `Convert.ToInt64()`

---

### 5. **Membership.cs - SecureMembershipProcess()** (HIGH)
```csharp
int nCookieUser = Convert.ToInt16(CookieValue(UserCookieName, -1));
```
**Problem**: Converting cookie value to `Int16`  
**Impact**: Session cookie type mismatch  
**Fix**: Change to `long`

---

### 6. **Membership.cs - ActivateAccount()** (HIGH)
```csharp
long userId = Convert.ToInt64(myWeb.moDbHelper.GetDataValue(...));
```
**Status**: ? Correct - uses `long`

---

### 7. **Membership.cs - AccountResetLink()** (MEDIUM)
```csharp
public string AccountResetLink(int AccountID)  // Parameter type
```
**Problem**: Method parameter is `int` instead of `long`  
**Fix**: Change to `long`

---

### 8. **Calendar.cs - Interaction.IIf()** (LOW)
```csharp
Convert.ToInt16(oCalContent.SelectSingleNode(...).InnerText)
```
**Problem**: While not directly user ID, same pattern  
**Status**: Monitor for consistency

---

## Session Storage Patterns to Fix

### Pattern 1: Session Variable Storage
**Current (WRONG)**:
```csharp
moSession["nUserId"] = (object)nUserIdAsInt;  // Stores int
nUserId = Convert.ToInt32(moSession["nUserId"]);  // Retrieves as int
```

**Correct**:
```csharp
moSession["nUserId"] = (object)mnUserId;  // Stores long
nUserId = Convert.ToInt64(moSession["nUserId"]);  // Retrieves as long
```

### Pattern 2: Cookie Storage
**Current (WRONG)**:
```csharp
Convert.ToInt16(CookieValue(UserCookieName, -1))  // Int16
```

**Correct**:
```csharp
Convert.ToInt64(CookieValue(UserCookieName, 0L))  // Int64 with long default
```

### Pattern 3: API Parameter Conversions
**Current (WRONG)**:
```csharp
nUserId = (int)Convert.ToInt64(sValidResponse);
```

**Correct**:
```csharp
nUserId = Convert.ToInt64(sValidResponse);  // Keep as long
```

---

## Files Requiring Fixes

| File | Issues | Priority |
|------|--------|----------|
| `Assemblies\Protean.CMS\api\Rest.cs` | 3 occurrences | CRITICAL |
| `Assemblies\Protean.CMS\providers\Membership.BaseProvider.cs` | 1 occurrence | CRITICAL |
| `Assemblies\Protean.CMS\features\membership\Membership.cs` | 2 occurrences | HIGH |
| `Assemblies\Protean.CMS\core\Base.cs` | Review needed | HIGH |
| `Assemblies\Protean.CMS\tools\stdTools.cs` | Review needed | MEDIUM |

---

## Why This Causes InvalidCastException on Second Request

1. **First Request**: 
   - `mnUserId` (long) ? stored in session as `long` object
   - Code tries to convert to `int` ? may succeed with implicit boxing
   - Request completes

2. **Second Request**:
   - Session retrieves boxed `long` value
   - Code attempts: `Convert.ToInt32(longValue)`
   - **Unboxing fails**: Cannot directly unbox `long` to `int`
   - **InvalidCastException** thrown

3. **The Fix**:
   - Ensure session always stores/retrieves as `long`
   - All conversions must be `long`
   - No intermediate `int`/`short` conversions

---

## Implementation Priority

### Phase 1 (IMMEDIATE) - Session Critical Path
1. ? `DeliverPage.cs` - Already using parameterless constructor
2. `Rest.cs` - Fix lines 124, 359, 372
3. `Membership.BaseProvider.cs` - Fix user ID initialization

### Phase 2 (URGENT) - Async Path
4. Review `Cms.Async.cs` for similar issues
5. Update `OpenAsync()` method

### Phase 3 (HIGH) - Comprehensive Coverage
6. Audit all `Convert.ToInt16()` and `Convert.ToInt32()` calls
7. Update method signatures from `int userId` to `long userId`
8. Review database parameter types (should be `BigInt` for long values)

---

## Testing Strategy

After fixes, test:
1. ? First page load (already working)
2. **Second page request** (reproduce original issue)
3. **Session persistence** across multiple requests
4. **Cookie-based authentication** scenarios
5. **API calls** with user ID parameters

---

## Code Review Checklist

- [ ] All `mnUserId` references are `long`
- [ ] Session storage uses `long` type
- [ ] No `Convert.ToInt32()` or `Convert.ToInt16()` on user IDs
- [ ] Method parameters use `long` for user IDs
- [ ] Database parameters use `BigInt` SQL type
- [ ] Cookie conversions use `long`
- [ ] API return values maintain `long` type
- [ ] Async methods consistent with sync methods
