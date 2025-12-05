# Resource Disposal Audit and Fix Plan

**Project:** ProteanCMS  
**Task:** Audit and fix resource disposal issues  
**Generated:** January 2025  
**Total Steps:** 14

---

## Overview

This comprehensive plan addresses resource disposal issues throughout the ProteanCMS codebase to eliminate memory leaks, prevent connection pool exhaustion, and improve application stability.

**Target Improvements:**
- Reduce memory footprint by 50%
- Eliminate connection pool exhaustion
- Remove file lock issues
- Fix GDI+ resource leaks

---

## Step 1: Environment Setup and Baseline Metrics

**Action:** Set up the working environment and baseline metrics.

**Reasoning:** This step ensures we have a reproducible environment, a clean build, and baseline memory/connection usage to compare after fixes.

**Implementation Details:**
- Pull the latest code from the main branch to a dedicated local workspace
- Open the solution in Visual Studio 2022 with ReSharper enabled for static analysis
- Ensure NuGet packages are restored (System.Data.SqlClient, Magick.NET, AlphaFS, etc.)
- Build the solution in Release mode to catch any compilation warnings
- Capture baseline performance data:
  - Run the web application on a local IIS Express instance
  - Use dotMemory Profiler to record a snapshot after exercising typical workflows (admin page load, content publishing, cart checkout, image upload)
  - Export the snapshot summary (memory usage, live objects, connection pool stats) for later comparison

**Error Handling:**
- If the solution fails to build, resolve missing references before proceeding
- If dotMemory cannot attach, verify that the process is 64?bit and matches .NET Framework 4.8

**Testing:**
- Verify that the application starts without exceptions
- Confirm that baseline snapshot files are saved and include relevant categories (Managed Heap, Unmanaged Resources, SQL Connections)

**?? Tip:** Automate the baseline capture with a simple PowerShell script that launches IIS Express, runs a predefined set of HTTP requests (using curl or Invoke-WebRequest), and triggers dotMemory snapshot. This makes re?running the baseline fast.

---

## Step 2: Static Analysis for Undisposed IDisposable Instances

**Action:** Run static analysis to locate undisposed IDisposable instances across the codebase.

**Reasoning:** Static analysis quickly identifies patterns that violate the resource management rules, giving us a concrete list of places to fix before manual code review.

**Implementation Details:**
- Use ReSharper's "Inspect | Potential Code Issues | Dispose objects before exiting scope" inspection
- Additionally run Roslyn analyzer "IDisposableAnalyzers" (install via NuGet) to catch missing disposals
- Export the inspection results to an XML/CSV report, grouping by file and line number
- Filter the report for the high?risk resource types:
  - SqlConnection, SqlCommand, SqlDataReader
  - StreamReader/Writer/FileStream
  - WebClient, HttpClient
  - Image/Bitmap/Graphics
  - XmlReader/Writer
  - FTP/WebRequest streams
  - Temporary file handles

**Error Handling:**
- If the analyzer throws errors due to missing references, restore all NuGet packages and ensure the project targets .NET Framework 4.8

**Testing:**
- Verify that the generated report contains entries for each priority file (Cms.DBHelper.cs, FTPHelper.cs, ImageHelper.cs, etc.)

**?? Tip:** Combine the ReSharper and Roslyn reports into a single master spreadsheet. Add columns for "Current Disposal Pattern" and "Suggested Fix" to streamline the next steps.

---

## Step 3: Create Detailed Audit Document

**Action:** Create a detailed audit document summarizing findings per priority file.

**Reasoning:** The audit serves as a roadmap, ensuring that every discovered leak is addressed systematically and can be tracked to completion.

**Implementation Details:**
- Open a new Markdown file `ResourceDisposalAudit.md` in the repository root
- For each priority file, add a section with:
  - File path
  - List of undisposed objects (type, line number)
  - Current pattern (e.g., manual Close, missing using)
  - Recommended fix (using statement, try/finally, singleton HttpClient, etc.)
  - Impact rating (Critical, High, Medium)
- Add a summary table linking to each section for quick navigation

**Error Handling:**
- Ensure line numbers are accurate; if the static analysis line numbers shift after formatting, manually verify a few entries

**Testing:**
- Perform a peer review of the audit file to confirm completeness

**?? Tip:** Link each issue to a GitHub issue (or your internal tracking system) directly from the Markdown using the issue URL. This allows developers to address items individually and track progress.

---

## Step 4: Refactor Database Helper Classes

**Action:** Refactor database helper classes to enforce proper disposal using `using` statements.

**Reasoning:** Database connections are the most common source of leaks and can exhaust the connection pool if not disposed correctly.

**Implementation Details:**

**Files:** `Protean.CMS\core\Cms.DBHelper.cs`, `Protean.Tools\Database.cs`

Replace any pattern that manually opens SqlConnection/SqlCommand/SqlDataReader without a `using` block:

```csharp
using (var conn = new SqlConnection(connString))
{
    conn.Open();
    using (var cmd = new SqlCommand(query, conn))
    {
        using (var rdr = cmd.ExecuteReader())
        {
            // process data
        }
    }
}
```

**Key Changes:**
- If a method returns a `SqlDataReader` to callers, change the API to return a fully materialized data structure (e.g., DataTable, List<T>) and dispose the reader internally
- For async methods, use `await using` with `ConfigureAwait(false)` where appropriate
- Ensure any `SqlConnection` opened in a `try` block is closed in a `finally` if a `using` cannot be applied
- Add comments where a legacy public API forces the caller to manage disposal

**Error Handling:**
- Preserve existing transaction scopes
- Catch and log `SqlException` without swallowing; rethrow after disposing

**Testing:**
- Create/augment unit tests that open a connection, execute a query, and assert that `ConnectionState` is Closed after method execution
- Run integration tests that simulate high?concurrency page loads (100 parallel requests) and verify no "Timeout expired" errors

**?? Tip:** Introduce a helper method `ExecuteReader(Action<SqlDataReader> action)` that encapsulates the using pattern. This reduces repetition and centralizes disposal logic.

---

## Step 5: Update FTP Helper Classes

**Action:** Update FTP helper classes to dispose network streams and FTP connections correctly.

**Reasoning:** Improper disposal of FTP/WebRequest streams can leave sockets open, leading to resource exhaustion and unpredictable failures.

**Implementation Details:**

**Files:** `Protean.CMS\tools\FTPHelper.cs`, `Protean.Tools\FTPClient.cs`

```csharp
var request = (FtpWebRequest)WebRequest.Create(uri);
request.Method = WebRequestMethods.Ftp.UploadFile;
using (var requestStream = request.GetRequestStream())
{
    // write data
}
using (var response = (FtpWebResponse)request.GetResponse())
{
    // optionally read response.StatusDescription
}
```

**Key Changes:**
- Ensure every `FtpWebRequest` and its response stream are wrapped in `using`
- For custom `NetworkStream` usage, apply the same pattern
- If the class holds a persistent FTP client, implement `IDisposable` on the helper class

**Error Handling:**
- Catch `WebException` and ensure the response stream is disposed even when an error occurs
- Log FTP errors with enough context

**Testing:**
- Write integration tests that upload and download a temporary file
- Verify that after a failure (e.g., wrong credentials) the connection is still disposed

**?? Tip:** Wrap the FTP logic in a separate service interface (`IFileTransferService`) so future changes (e.g., SFTP) can be swapped without affecting callers.

---

## Step 6: Refactor Image Processing Utilities

**Action:** Refactor image processing utilities to dispose GDI+ objects promptly.

**Reasoning:** Image, Bitmap, and Graphics objects lock file handles and consume unmanaged memory.

**Implementation Details:**

**Files:** `Protean.CMS\tools\ImageHelper.cs`, `Protean.Tools\Image.cs`

```csharp
using (var img = Image.FromFile(path))
{
    // process image
}

using (var bitmap = new Bitmap(width, height))
using (var graphics = Graphics.FromImage(bitmap))
{
    // drawing code
}
```

**Key Changes:**
- Replace `Image.FromFile(path)` with using statements
- When creating a `Bitmap` or `Graphics` object, always wrap in `using`
- If images need to be returned to callers, return a copy and dispose the original
- Ensure any `MemoryStream` used to hold image bytes is also disposed

**Error Handling:**
- Catch `OutOfMemoryException` and log; ensure disposal in `finally`

**Testing:**
- Add unit tests that load an image, perform a resize operation, and assert that the source file is no longer locked
- Use dotMemory to verify that GDI+ handles are released

**?? Tip:** Consider using `ImageFactory` from the ImageProcessor library for high?level processing, which already handles disposal internally.

---

## Step 7: Fix File System Helper Classes

**Action:** Correct file system helper classes to dispose all stream objects and clean up temporary files.

**Reasoning:** Leaked streams and orphaned temp files increase disk usage and can cause file?share locks.

**Implementation Details:**

**Files:** `Protean.CMS\tools\fsHelper\FSHelper.cs`, `Protean.Tools\FileHelper.cs`

Replace any manual `stream.Close()` with `using` blocks for:
- `FileStream`
- `StreamReader`
- `StreamWriter`
- `MemoryStream`

For temporary files:
```csharp
var tempPath = Path.GetTempFileName();
try
{
    // write/read
}
finally
{
    if (File.Exists(tempPath))
        File.Delete(tempPath);
}
```

**Error Handling:**
- On IOException during delete, log warning but do not throw

**Testing:**
- Unit test that calls a method creating a temp file, then asserts the file does not exist after completion
- Stress test that opens many streams concurrently

**?? Tip:** Implement a helper `SafeDeleteFile(string path)` that retries deletion a few times with short delays to handle transient locks.

---

## Step 8: Update XML Utilities

**Action:** Update XML utilities to dispose `XmlReader` and `XmlWriter` objects correctly.

**Reasoning:** Undisposed XML readers/writers keep underlying streams alive, leading to memory and file lock leaks.

**Implementation Details:**

**Files:** `Protean.CMS\tools\xmlTools.cs`, `Protean.Tools\Xml.cs`

```csharp
using (var reader = XmlReader.Create(stream))
{
    // read
}
```

**Key Changes:**
- Wrap `XmlReader.Create(...)` and `XmlWriter.Create(...)` in `using` blocks
- If an `XmlReader` is passed to another method, transfer ownership and document disposal responsibility
- Ensure any settings objects that hold streams are also disposed

**Error Handling:**
- Capture `XmlException` and ensure disposal

**Testing:**
- Unit test that parses a large XML file, then asserts the file can be deleted immediately after
- Run memory profiling to confirm no lingering `XmlReader` objects

**?? Tip:** Leverage `XDocument.Load(stream)` for simple read scenarios; it internally disposes the stream when passed a file path.

---

## Step 9: Audit Core Cms.cs File

**Action:** Audit and refactor the core `Cms.cs` file for general disposal patterns.

**Reasoning:** `Cms.cs` is a massive class (9,401 lines) that likely mixes many resource types.

**Implementation Details:**
- Search the file for any high?risk types
- Apply the same `using`/`try?finally` patterns as in previous steps
- Split large methods into smaller private helpers
- If the class holds long?lived resources, implement `IDisposable` on `Cms`
- Add XML documentation comments indicating disposal contracts

**Error Handling:**
- Preserve existing error handling logic

**Testing:**
- Create integration tests that execute typical CMS workflows
- Compare memory usage before and after using baseline snapshot

**?? Tip:** Consider extracting database and file?system responsibilities into separate service classes.

---

## Step 10: Fix Additional Modules

**Action:** Review and fix additional modules: `Cart.cs`, `MailQueue.cs`, and HTTP handlers.

**Reasoning:** These modules interact with databases, streams, and network resources.

**Implementation Details:**
- **Cart.cs** – Ensure any DB calls use the updated DB helper; wrap file export/import streams
- **MailQueue.cs** – Verify `SmtpClient` is disposed via `using`
- **FeedHandler.cs** and **httpHandler.cs** – Ensure `WebClient` or `HttpWebRequest` objects are disposed

**Error Handling:**
- For email sending failures, log the exception without leaking the client

**Testing:**
- Unit test that processes a cart checkout
- Simulate sending an email with invalid SMTP configuration
- Run load tests on the feed handler

**?? Tip:** Create a base class `DisposableBase` implementing the standard dispose pattern.

---

## Step 11: Standardize HttpClient Usage

**Action:** Standardize `HttpClient` usage across the solution.

**Reasoning:** Creating/disposal of `HttpClient` per request leads to socket exhaustion; a singleton is the recommended pattern.

**Implementation Details:**

```csharp
public static class HttpClientProvider
{
    public static readonly HttpClient Instance = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(30)
    };
}
```

- Replace all `new HttpClient()` or `using (var client = new HttpClient())` with `HttpClientProvider.Instance`
- Configure the static `HttpClient` only once during application start
- Do **not** dispose the singleton

**Error Handling:**
- Update any code that relied on disposing to reset handlers

**Testing:**
- Run functional tests under high concurrency
- Verify no `SocketException: Too many open files`

**?? Tip:** Wrap the singleton in a `Lazy<HttpClient>` to ensure thread?safe lazy initialization.

---

## Step 12: Run Full Test Suite and Memory Profiling

**Action:** Run the full suite of unit and integration tests, then perform memory profiling after changes.

**Reasoning:** Verification that all leaks are resolved and no regression has been introduced.

**Implementation Details:**
- Execute all existing tests
- Add new tests created in earlier steps
- Deploy to staging IIS instance
- Use dotMemory Profiler to take a snapshot after realistic workload
- Compare new snapshot against baseline:
  - Look for reductions in live objects
  - Verify no increase in unmanaged memory

**Error Handling:**
- If tests fail, isolate, revert, and re-apply with corrections

**Testing:**
- All tests must pass 100%
- Memory usage should drop by at least 50%

**?? Tip:** Automate the profiling run with a script for future CI comparison.

---

## Step 13: Update Documentation

**Action:** Update documentation and coding guidelines to reflect the new disposal standards.

**Reasoning:** Ensuring future developers follow the same patterns prevents regressions.

**Implementation Details:**
- Add section "Resource Disposal Best Practices" to `CONTRIBUTING.md`
- Include examples for each resource type
- Reference static analysis tool configuration
- Update code comments
- Create code review checklist

**Error Handling:**
- Create documentation files manually if needed

**Testing:**
- Peer review of updated docs

**?? Tip:** Link documentation to CI pipeline so PRs fail if guidelines are not followed.

---

## Step 14: Commit and Create Pull Request

**Action:** Commit changes, create a pull request, and schedule a code?review session.

**Reasoning:** Version control ensures traceability and allows team members to validate the fixes.

**Implementation Details:**
- Stage all modified files
- Write descriptive commit message: "Fix resource disposal leaks – DB, FTP, Image, Streams, XML, HttpClient, Temp files"
- Push to new branch `resource-disposal-fix`
- Open PR targeting `main` with summary, audit markdown, and profiling results
- Assign reviewers

**Error Handling:**
- If push is rejected, create draft PR and request maintainer assistance

**Testing:**
- Ensure CI pipeline runs linting, unit tests, and static analysis

**?? Tip:** Include baseline and post?fix dotMemory snapshots as PR attachments.

---

## Success Criteria

? **Zero CA2000 warnings** in code analysis  
? **Memory growth <5%** over 24 hour test  
? **No connection pool timeout errors** under load  
? **All temporary files cleaned up** after operations  
? **50% reduction** in memory footprint  

---

## Priority Files

1. **Cms.DBHelper.cs** - Database connections (CRITICAL)
2. **FTPHelper.cs / FTPClient.cs** - Network streams (CRITICAL)
3. **ImageHelper.cs / Image.cs** - GDI+ objects (CRITICAL)
4. **FSHelper.cs / FileHelper.cs** - File streams (HIGH)
5. **xmlTools.cs / Xml.cs** - XML readers (HIGH)
6. **Cms.cs** - Mixed resources (HIGH)
7. **Cart.cs** - Database and file operations (MEDIUM)
8. **MailQueue.cs** - SMTP connections (MEDIUM)

---

**Plan Generated By:** Artiforge Orchestrator  
**Estimated Timeline:** 2-3 weeks  
**Team Size:** 1-2 developers
