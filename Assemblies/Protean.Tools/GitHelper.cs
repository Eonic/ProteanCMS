using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Protean.Tools
{

    public class GitHelper
    {

        public string GitCommandExecution(string gitUserName, string gitPassword, string ps1FilePath, string gitRepoUrl, string workingDirectory)
        {
            
            string arguments = "";
            if (!string.IsNullOrEmpty(workingDirectory))
            {

                if (Directory.Exists(workingDirectory))
                {
                    if (!string.IsNullOrEmpty(gitUserName) && !string.IsNullOrEmpty(gitPassword)&& !string.IsNullOrEmpty(ps1FilePath) && !string.IsNullOrEmpty(gitRepoUrl))
                    {
                         arguments = $"-ExecutionPolicy Bypass -File \"{ps1FilePath}\" -RepoUrl \"{gitRepoUrl}\" -TargetPath \"{workingDirectory}\" -Username \"{gitUserName}\" -Password \"{gitPassword}\"";

                    }
                }
            }

            string result = "";
            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = arguments,
                RedirectStandardOutput = true,
                WorkingDirectory = workingDirectory,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true


            };
            // startInfo.EnvironmentVariables["GIT_CONFIG_GLOBAL"] = @"D:\temp\app_gitconfig";
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.CreateNoWindow = true;
            using (var process = new Process { StartInfo = startInfo })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                int exitCode = process.ExitCode;
                if (exitCode == 0 )
                {
                    result = "Git Pulled successfully";
                }
                else
                {
                    result = "Git failed -" + error;
                }
                return result;
            }

        }
    }
}
