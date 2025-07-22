using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Configuration;
using Microsoft.Identity.Client;
using Microsoft.AspNetCore.Http;

namespace Protean.Tools
{
    public class GitHelper
    {
        private readonly string _gitFilePath;

        public GitHelper(string gitFilePath)
        {
            if (!string.IsNullOrEmpty(gitFilePath))
            {
                _gitFilePath = gitFilePath;
            }
        }

        public string AuthenticateDevOps(string cClientId, string cTenantId, string cScope, string cSecreteValue)
        {
            string cAccessToken = string.Empty;
            try
            {
                if(string.IsNullOrEmpty(cClientId) || string.IsNullOrEmpty(cTenantId) || string.IsNullOrEmpty(cScope) || string.IsNullOrEmpty(cSecreteValue))
                {
                    return "";
                }

                var app = ConfidentialClientApplicationBuilder.Create(cClientId)
              .WithClientSecret(cSecreteValue)
              .WithAuthority($"https://login.microsoftonline.com/{cTenantId}")
             .Build();

                var tokenResult = app.AcquireTokenForClient(new[] { cScope }).ExecuteAsync().Result;
                cAccessToken = tokenResult.AccessToken;

                return cAccessToken;
            }
            catch (Exception ex) {
                return "";
            }

        }

        public string GitCommandExecution(string ps1FilePath,string cAccessToken)
        {
            string output = "";
            string error = "";
            string result = "";
            string askPassPath = string.Empty;
           
            try
            {
                string gitFilePath = _gitFilePath;
                if (string.IsNullOrEmpty(cAccessToken))
                {
                    return "Access token is not null or empty. Please provide a valid token.";
                }
                if (string.IsNullOrEmpty(ps1FilePath))
                {
                    return "PowerShell script path is not null or empty. Please provide a valid script path.";
                }
                if (string.IsNullOrEmpty(gitFilePath))
                {
                    return "Git file path is not null or empty. Please provide a valid git file path.";
                }

                if (!string.IsNullOrEmpty(gitFilePath))
                {
                    ps1FilePath = gitFilePath + ps1FilePath;
                }
                string arguments = $"-ExecutionPolicy Bypass -File \"{ps1FilePath}\"";
                
                // Create temporary askpass script
                askPassPath = Path.Combine(Path.GetTempPath(), "askpass_oauth2.bat");
                File.WriteAllText(askPassPath, $"@echo off{Environment.NewLine}echo {cAccessToken}");

                ProcessStartInfo gitPull = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                gitPull.EnvironmentVariables["GIT_ASKPASS"] = askPassPath;
                gitPull.EnvironmentVariables["GIT_TERMINAL_PROMPT"] = "0";
                gitPull.EnvironmentVariables["ACCESS_TOKEN"] = cAccessToken;

                using (var process = Process.Start(gitPull))
                {
                    output = process.StandardOutput.ReadToEnd();
                    error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    int exitCode = process.ExitCode;
                    bool hasGitError = error.Contains("fatal") || error.Contains("error");

                    if (exitCode == 0 && !hasGitError)
                    {
                        result = $"Git Pull successful.\nOutput:\n{output}";
                    }
                    else
                    {
                        result = $"Git Pull may have failed.\nExit Code: {exitCode}\nError:\n{error}\nOutput:\n{output}";
                    }
                }
            }
            catch (Exception ex)
            {
                result = $"Exception occurred while executing Git command:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
            }
            finally
            {
                // Ensure cleanup happens even if an exception occurs
                if (!string.IsNullOrEmpty(askPassPath) && File.Exists(askPassPath))
                {
                    try
                    {
                        File.Delete(askPassPath);
                    }
                    catch (Exception ex)
                    {
                        result += $"\n\nFailed to delete temporary askpass file:\n{ex.Message}";
                    }
                }
            }

            return result;
        }
    }
}
