using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Protean.Tools
{

    public class GitHelper
    {

        //public string RunGitCommands()
        //{
        //    System.Collections.Specialized.NameValueCollection moConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");

        //    //string cRepositoryPath = "";
        //    //string cArguments = "";
        //    string cResult = "";
        //    if (!string.IsNullOrEmpty(moConfig["GitRepoPath"]))
        //    {
        //        cRepositoryPath = moConfig["GitRepoPath"];
        //        if (Directory.Exists(cRepositoryPath))
        //        {
        //            if (!string.IsNullOrEmpty(moConfig["GitUserName"]) && !string.IsNullOrEmpty(moConfig["GitEmail"]))
        //            {
        //                GitCommandExecution("git config user.name " + moConfig["GitUserName"], cRepositoryPath);
        //                GitCommandExecution("git config user.email" + moConfig["GitEmail"], cRepositoryPath);
        //            }
        //            GitCommandExecution("git config --add safe.directory \"" + cRepositoryPath.Replace("\\", "/") + "\"", cRepositoryPath);


        //            if (!string.IsNullOrEmpty(moConfig["GitCommandFile"]))
        //            {
        //                cArguments = "-ExecutionPolicy Bypass -File " + moConfig["GitCommandFile"];
        //                if (File.Exists(moConfig["GitCommandFile"]))
        //                {
        //                    cResult = GitCommandExecution(cArguments, cRepositoryPath);
        //                }
        //            }
        //        }
        //    }
        //    return cResult;
        //}

        public string GitCommandExecution(string arguments, string workingDirectory)
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
                if (exitCode == 0)
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
