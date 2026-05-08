<%@ WebHandler Language="C#" Class="RuntimeDiagnostics" %>
<%@ Assembly Name="netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51" %>
<%@ Assembly Name="System.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" %>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

public class RuntimeDiagnostics : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        // Security check - require query parameter
        if (context.Request.QueryString["key"] != "diagnostics")
        {
            context.Response.StatusCode = 403;
            context.Response.Write("Access denied. Use ?key=diagnostics");
            return;
        }

        context.Response.ContentType = "text/html";
        
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html><head><title>Runtime Diagnostics</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; margin: 20px; background: #f5f5f5; }");
        sb.AppendLine(".container { max-width: 1200px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }");
        sb.AppendLine("h1 { color: #0078d4; border-bottom: 3px solid #0078d4; padding-bottom: 10px; }");
        sb.AppendLine("h2 { color: #333; margin-top: 30px; border-left: 4px solid #0078d4; padding-left: 10px; }");
        sb.AppendLine(".success { color: #107c10; font-weight: bold; }");
        sb.AppendLine(".error { color: #d13438; font-weight: bold; }");
        sb.AppendLine(".warning { color: #ff8c00; font-weight: bold; }");
        sb.AppendLine(".info { background: #e6f3ff; padding: 10px; border-left: 4px solid #0078d4; margin: 10px 0; }");
        sb.AppendLine("table { width: 100%; border-collapse: collapse; margin: 15px 0; }");
        sb.AppendLine("th { background: #0078d4; color: white; padding: 10px; text-align: left; }");
        sb.AppendLine("td { padding: 8px; border-bottom: 1px solid #ddd; }");
        sb.AppendLine("tr:hover { background: #f5f5f5; }");
        sb.AppendLine(".path { font-family: 'Consolas', monospace; background: #f0f0f0; padding: 2px 6px; border-radius: 3px; }");
        sb.AppendLine(".recommendation { background: #fff4ce; border-left: 4px solid #ff8c00; padding: 15px; margin: 15px 0; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head><body>");
        sb.AppendLine("<div class='container'>");
        sb.AppendLine("<h1>🔍 Runtime Diagnostics - Magick.NET</h1>");
        sb.AppendLine("<p>Generated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "</p>");

        bool isAzureCheck = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));
        if (isAzureCheck)
        {
            sb.AppendLine("<div class='info'>");
            sb.AppendLine("<h3>Azure Configuration Checklist</h3>");
            sb.AppendLine("<p><strong>For Magick.NET to work on Azure, you need:</strong></p>");
            sb.AppendLine("<ol>");
            sb.AppendLine("<li>✅ Platform set to <strong>64 Bit</strong> (Configuration → General Settings)</li>");
            sb.AppendLine("<li>✅ <strong>WEBSITE_LOAD_USER_PROFILE = 1</strong> (Configuration → Application Settings)</li>");
            sb.AppendLine("<li>✅ Native DLL files in <strong>bin\\x64\\</strong> folder</li>");
            sb.AppendLine("<li>✅ Probing paths in <strong>web.config</strong>: <code>bin;bin\\x64;bin\\x86;runtimes\\win-x64\\native</code></li>");
            sb.AppendLine("<li>✅ <strong>Restart App Service</strong> after changes</li>");
            sb.AppendLine("</ol>");
            sb.AppendLine("<p><em>Note: You cannot set PATH on Azure - it uses web.config probing paths instead.</em></p>");
            sb.AppendLine("</div>");
        }

        // System Information
        sb.AppendLine("<h2>System Information</h2>");
        sb.AppendLine("<table>");
        sb.AppendLine("<tr><th>Property</th><th>Value</th><th>Status</th></tr>");
        
        bool is64Bit = Environment.Is64BitProcess;
        sb.AppendLine("<tr><td>Process Architecture</td><td>" + (is64Bit ? "64-bit" : "32-bit") + "</td><td class='" + (is64Bit ? "success'>✓ Correct" : "error'>✗ WRONG - Must be 64-bit") + "</td></tr>");
        sb.AppendLine("<tr><td>OS Architecture</td><td>" + (Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit") + "</td><td>-</td></tr>");
        sb.AppendLine("<tr><td>CLR Version</td><td>" + Environment.Version + "</td><td>-</td></tr>");
        sb.AppendLine("<tr><td>Working Directory</td><td class='path'>" + Environment.CurrentDirectory + "</td><td>-</td></tr>");
        sb.AppendLine("<tr><td>App Domain Base</td><td class='path'>" + AppDomain.CurrentDomain.BaseDirectory + "</td><td>-</td></tr>");
        sb.AppendLine("</table>");

        // Environment Variables
        sb.AppendLine("<h2>Azure/IIS Environment Variables</h2>");
        sb.AppendLine("<table>");
        sb.AppendLine("<tr><th>Variable</th><th>Value</th><th>Status</th></tr>");
        
        var envVars = new Dictionary<string, string>
        {
            { "WEBSITE_LOAD_USER_PROFILE", Environment.GetEnvironmentVariable("WEBSITE_LOAD_USER_PROFILE") },
            { "WEBSITE_SITE_NAME", Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME") },
            { "WEBSITE_INSTANCE_ID", Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID") },
            { "PATH", Environment.GetEnvironmentVariable("PATH") },
            { "MAGICK_TMPDIR", Environment.GetEnvironmentVariable("MAGICK_TMPDIR") },
            { "MAGICK_TEMPORARY_PATH", Environment.GetEnvironmentVariable("MAGICK_TEMPORARY_PATH") }
        };

        bool loadUserProfile = envVars["WEBSITE_LOAD_USER_PROFILE"] == "1";
        sb.AppendLine("<tr><td>WEBSITE_LOAD_USER_PROFILE</td><td>" + (envVars["WEBSITE_LOAD_USER_PROFILE"] ?? "<span class='error'>NOT SET</span>") + "</td><td class='" + (loadUserProfile ? "success'>✓ Enabled" : "error'>✗ MUST BE SET TO 1") + "</td></tr>");
        
        bool isAzure = !string.IsNullOrEmpty(envVars["WEBSITE_SITE_NAME"]);
        sb.AppendLine("<tr><td>Running on Azure</td><td>" + (isAzure ? "Yes (" + envVars["WEBSITE_SITE_NAME"] + ")" : "No (Local IIS)") + "</td><td>-</td></tr>");
        
        if (isAzure)
        {
            sb.AppendLine("<tr><td>Instance ID</td><td>" + (envVars["WEBSITE_INSTANCE_ID"] ?? "N/A") + "</td><td>-</td></tr>");
        }

        bool hasMagickTemp = !string.IsNullOrEmpty(envVars["MAGICK_TMPDIR"]) || !string.IsNullOrEmpty(envVars["MAGICK_TEMPORARY_PATH"]);
        sb.AppendLine("<tr><td>Magick Temp Path</td><td>" + (envVars["MAGICK_TMPDIR"] ?? envVars["MAGICK_TEMPORARY_PATH"] ?? "<span class='warning'>Not set (recommended)</span>") + "</td><td class='" + (hasMagickTemp ? "success'>✓ Set" : "warning'>⚠ Recommended") + "</td></tr>");

        sb.AppendLine("</table>");

        // PATH Analysis (informational only - Azure doesn't allow PATH modification)
        if (!string.IsNullOrEmpty(envVars["PATH"]))
        {
            sb.AppendLine("<h2>PATH Directories (Informational)</h2>");
            sb.AppendLine("<div class='info'>Note: Azure relies on web.config probing paths, not PATH environment variable.</div>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Path</th><th>Status</th></tr>");

            var pathDirs = envVars["PATH"].Split(';');
            var requiredPaths = new[] { "bin", "bin\\x64", "runtimes\\win-x64\\native" };

            foreach (var required in requiredPaths)
            {
                bool found = pathDirs.Any(p => p.Contains(required));
                sb.AppendLine("<tr><td class='path'>" + required + "</td><td class='" + (found ? "success'>✓ In PATH" : "warning'>⚠ Not in PATH (OK - uses probing paths)") + "</td></tr>");
            }

            sb.AppendLine("</table>");
        }

        // Web.config Probing Paths Check
        sb.AppendLine("<h2>Web.config Probing Paths</h2>");
        sb.AppendLine("<div class='info'>Azure uses web.config &lt;probing privatePath&gt; to locate native DLLs.</div>");
        try
        {
            string webConfigPath = Path.Combine("", "web.config");
            if (File.Exists(webConfigPath))
            {
                string webConfigContent = File.ReadAllText(webConfigPath);
                bool hasProbing = webConfigContent.Contains("<probing privatePath");
                bool hasX64 = webConfigContent.Contains("bin\\x64") || webConfigContent.Contains("bin/x64");
                bool hasRuntimes = webConfigContent.Contains("runtimes\\win-x64\\native") || webConfigContent.Contains("runtimes/win-x64/native");

                sb.AppendLine("<table>");
                sb.AppendLine("<tr><th>Setting</th><th>Status</th></tr>");
                sb.AppendLine("<tr><td>Probing element exists</td><td class='" + (hasProbing ? "success'>✓ Found" : "error'>✗ MISSING") + "</td></tr>");
                sb.AppendLine("<tr><td>bin\\x64 in probing path</td><td class='" + (hasX64 ? "success'>✓ Configured" : "error'>✗ MISSING") + "</td></tr>");
                sb.AppendLine("<tr><td>runtimes\\win-x64\\native in probing path</td><td class='" + (hasRuntimes ? "success'>✓ Configured" : "warning'>⚠ Recommended") + "</td></tr>");
                sb.AppendLine("</table>");
            }
            else
            {
                sb.AppendLine("<p class='warning'>⚠ web.config not found at expected location</p>");
            }
        }
        catch (Exception ex)
        {
            sb.AppendLine("<p class='warning'>⚠ Could not read web.config: " + HttpUtility.HtmlEncode(ex.Message) + "</p>");
        }

        // DLL File Checks
        sb.AppendLine("<h2>Magick.NET DLL Files</h2>");
        sb.AppendLine("<table>");
        sb.AppendLine("<tr><th>File</th><th>Location</th><th>Exists</th><th>Size</th><th>Modified</th></tr>");

        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        var dllChecks = new[]
        {
            new { Name = "Magick.NET-Q8-x64.dll (Managed)", Path = Path.Combine(basePath, "bin", "Magick.NET-Q8-x64.dll"), Required = true },
            new { Name = "Magick.NET.Core.dll (Managed)", Path = Path.Combine(basePath, "bin", "Magick.NET.Core.dll"), Required = true },
            new { Name = "Magick.Native-Q8-x64.dll", Path = Path.Combine(basePath, "bin", "Magick.Native-Q8-x64.dll"), Required = true },
            new { Name = "Magick.Native-Q8-x64.dll", Path = Path.Combine(basePath, "bin", "x64", "Magick.Native-Q8-x64.dll"), Required = true },
            new { Name = "Magick.Native-Q8-x64.dll", Path = Path.Combine(basePath, "runtimes", "win-x64", "native", "Magick.Native-Q8-x64.dll"), Required = false },
            new { Name = "vcruntime140.dll (VC++ Runtime)", Path = Path.Combine(basePath, "bin", "x64", "vcruntime140.dll"), Required = true },
            new { Name = "vcruntime140_1.dll (VC++ Runtime)", Path = Path.Combine(basePath, "bin", "x64", "vcruntime140_1.dll"), Required = true },
            new { Name = "msvcp140.dll (VC++ Runtime)", Path = Path.Combine(basePath, "bin", "x64", "msvcp140.dll"), Required = true }
        };

        int missingCount = 0;
        foreach (var check in dllChecks)
        {
            bool exists = File.Exists(check.Path);
            if (!exists && check.Required) missingCount++;
            
            string sizeStr = "-";
            string modifiedStr = "-";
            if (exists)
            {
                try
                {
                    var fileInfo = new FileInfo(check.Path);
                    sizeStr = (fileInfo.Length / 1024.0 / 1024.0).ToString("F2") + " MB";
                    modifiedStr = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm");
                }
                catch { }
            }

            string statusClass = exists ? "success" : (check.Required ? "error" : "warning");
            string statusText = exists ? "✓ Found" : (check.Required ? "✗ MISSING" : "⚠ Optional");
            
            sb.AppendLine("<tr>");
            sb.AppendLine("<td>" + check.Name + "</td>");
            sb.AppendLine("<td class='path'>" + check.Path.Replace(basePath, "~\\") + "</td>");
            sb.AppendLine("<td class='" + statusClass + "'>" + statusText + "</td>");
            sb.AppendLine("<td>" + sizeStr + "</td>");
            sb.AppendLine("<td>" + modifiedStr + "</td>");
            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</table>");

        // Assembly Loading Test
        sb.AppendLine("<h2>Assembly Loading Test</h2>");
        
        try
        {
            string managedDllPath = Path.Combine(basePath, "bin", "Magick.NET-Q8-x64.dll");
            if (File.Exists(managedDllPath))
            {
                var assembly = Assembly.LoadFrom(managedDllPath);
                sb.AppendLine("<div class='info'>");
                sb.AppendLine("<p class='success'>✓ Managed Assembly Loaded Successfully</p>");
                sb.AppendLine("<p><strong>Assembly:</strong> " + assembly.FullName + "</p>");
                sb.AppendLine("<p><strong>Location:</strong> " + assembly.Location + "</p>");
                sb.AppendLine("<p><strong>Runtime Version:</strong> " + assembly.ImageRuntimeVersion + "</p>");
                sb.AppendLine("</div>");
                
                // Try to instantiate Magick class
                try
                {
                    Type magickImageType = assembly.GetType("ImageMagick.MagickImage");
                    if (magickImageType != null)
                    {
                        object instance = Activator.CreateInstance(magickImageType);
                        sb.AppendLine("<div class='info'>");
                        sb.AppendLine("<p class='success'>✓✓ MagickImage Instance Created Successfully!</p>");
                        sb.AppendLine("<p><strong>This means the native DLL is loading correctly.</strong></p>");
                        sb.AppendLine("</div>");
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine("<div class='recommendation'>");
                    sb.AppendLine("<p class='error'>✗ Failed to Create MagickImage Instance</p>");
                    sb.AppendLine("<p><strong>Error:</strong> " + HttpUtility.HtmlEncode(ex.Message) + "</p>");
                    if (ex.InnerException != null)
                    {
                        sb.AppendLine("<p><strong>Inner Error:</strong> " + HttpUtility.HtmlEncode(ex.InnerException.Message) + "</p>");
                    }
                    sb.AppendLine("<p><strong>This indicates the native DLL (Magick.Native-Q8-x64.dll) cannot be loaded.</strong></p>");
                    sb.AppendLine("</div>");
                }
            }
            else
            {
                sb.AppendLine("<p class='error'>✗ Magick.NET-Q8-x64.dll not found at: " + managedDllPath + "</p>");
            }
        }
        catch (Exception ex)
        {
            sb.AppendLine("<p class='error'>✗ Error loading assembly: " + HttpUtility.HtmlEncode(ex.Message) + "</p>");
        }

        // Recommendations
        sb.AppendLine("<h2>Recommendations</h2>");
        
        if (missingCount > 0)
        {
            sb.AppendLine("<div class='recommendation'>");
            sb.AppendLine("<p class='error'><strong>⚠ " + missingCount + " Required Files Missing!</strong></p>");
            sb.AppendLine("<p>Copy missing DLLs to the indicated locations before proceeding.</p>");
            sb.AppendLine("</div>");
        }

        if (!is64Bit)
        {
            sb.AppendLine("<div class='recommendation'>");
            sb.AppendLine("<p class='error'><strong>⚠ Application Pool is 32-bit!</strong></p>");
            sb.AppendLine("<p><strong>Azure Fix:</strong> Configuration → General Settings → Platform → Change to <strong>64 Bit</strong></p>");
            sb.AppendLine("<p><strong>Local IIS Fix:</strong> Application Pool → Advanced Settings → Enable 32-Bit Applications → Set to <strong>False</strong></p>");
            sb.AppendLine("</div>");
        }

        if (isAzure && !loadUserProfile)
        {
            sb.AppendLine("<div class='recommendation'>");
            sb.AppendLine("<p class='error'><strong>🔥 CRITICAL: WEBSITE_LOAD_USER_PROFILE Not Set!</strong></p>");
            sb.AppendLine("<p><strong>This is the #1 cause of native DLL loading failures on Azure.</strong></p>");
            sb.AppendLine("<p><strong>Azure Fix:</strong> Configuration → Application Settings → New application setting:</p>");
            sb.AppendLine("<p><span class='path'>Name: WEBSITE_LOAD_USER_PROFILE</span><br /><span class='path'>Value: 1</span></p>");
            sb.AppendLine("<p>Then <strong>restart the App Service</strong>.</p>");
            sb.AppendLine("</div>");
        }

        if (!hasMagickTemp && isAzure)
        {
            sb.AppendLine("<div class='recommendation'>");
            sb.AppendLine("<p class='info'><strong>💡 Recommended: Set Magick Temp Directory</strong></p>");
            sb.AppendLine("<p><strong>Azure:</strong> Configuration → Application Settings → New application setting:</p>");
            sb.AppendLine("<p><span class='path'>Name: MAGICK_TMPDIR</span><br /><span class='path'>Value: D:\\home\\site\\temp</span></p>");
            sb.AppendLine("<p>This improves performance and prevents temp file issues.</p>");
            sb.AppendLine("</div>");
        }

        if (missingCount == 0 && is64Bit && (!isAzure || loadUserProfile))
        {
            sb.AppendLine("<div class='info'>");
            sb.AppendLine("<p class='success'><strong>✓ All Checks Passed!</strong></p>");
            sb.AppendLine("<p>If you're still experiencing errors, restart the App Service or IIS Application Pool.</p>");
            sb.AppendLine("</div>");
        }

        sb.AppendLine("</div>");
        sb.AppendLine("</body></html>");

        context.Response.Write(sb.ToString());
    }

    public bool IsReusable
    {
        get { return false; }
    }
}