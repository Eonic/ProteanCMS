using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Protean.Tools
{

    public class GitHelper
    {

        public string RunGitCommands(string gitUserName, string gitEmail, string ps1FilePath, string workingDirectory)
        {
            string cArguments = "";


            string cResult = "";
            if (!string.IsNullOrEmpty(workingDirectory))
            {
               
                if (Directory.Exists(workingDirectory))
                {
                    if (!string.IsNullOrEmpty(gitUserName) && !string.IsNullOrEmpty(gitEmail))
                    {
                        GitCommandExecution("git config user.name " + gitUserName, workingDirectory);
                        GitCommandExecution("git config user.email " + gitEmail, workingDirectory);
                    }
                    GitCommandExecution("git config --add safe.directory  \"" + workingDirectory.Replace("\\", "/") + "\"", workingDirectory);
                    //GitCommandExecution("git config --add safe.directory \"" + cRepositoryPath.Replace("\\", "/") + "\"", cRepositoryPath);


                    if (!string.IsNullOrEmpty(ps1FilePath))
                    {
                        cArguments = $"-ExecutionPolicy Bypass -File " + ps1FilePath;
                        if (File.Exists(ps1FilePath))
                        {
                            cResult = GitCommandExecution(cArguments, workingDirectory);
                        }
                    }
                }
            }
            return cResult;
        }

        public string GitCommandExecution(string arguments, string workingDirectory)
        //public string GitCommandExecution(string gitUserName, string gitEmail,string ps1FilePath, string workingDirectory)

        {
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
                if (exitCode == 0 && error=="")
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
