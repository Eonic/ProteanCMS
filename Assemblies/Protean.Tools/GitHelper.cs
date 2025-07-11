using DocumentFormat.OpenXml.Office.CustomXsn;
using iTextSharp.text.pdf;
using Microsoft.Identity.Client;
using Protean.Tools.Errors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Web;
using TinifyAPI;


namespace Protean.Tools
{

    public class GitHelper
    {


        public string GitCommandExecution(string cClientId, string cTenantId, string ps1FilePath, string cScopeFile, string cSecreteValue,string cAccessToken)
        {
           
            string output = "";
            string error = "";
            string result = "";
           
            string arguments = $"-ExecutionPolicy Bypass -File \"{ps1FilePath}\" -ClientId \"{cClientId}\" -TenantId \"{cTenantId}\" -accessToken \"{cAccessToken}\"";

            // Create temporary askpass script
            string askPassPath = Path.Combine(Path.GetTempPath(), "askpass_oauth2.bat");
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

                File.Delete(askPassPath); // cleanup temp file
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
                return result ;


            }
        }
    }
}
