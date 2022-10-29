using System;
using System.Diagnostics;
using System.Linq;

namespace GitAnalyzer.Modules.GitObjects
{
    public class GitRepository
    {
        public event Action<string> UpdateProgress;
        public event Action Finished;
        public static string GitVersion { get; private set; }
        public static bool IsGitInstalled { get; private set; }
        public static void CollectGitInfo()
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.Arguments = $"/C git version";
            cmd.Start();
            cmd.WaitForExit();

            GitVersion = cmd.StandardOutput.ReadToEnd();
            IsGitInstalled = cmd.ExitCode == 0;
        }

        public string RepositoryPath { get; set; }
        public GitObject[] Objects { get; set; }
        public GitRepository(string path)
        {
            RepositoryPath = path;
        }

        public bool Exists()
        {
            if (RunGit("git status 2", out string output))
            {
                return !output.StartsWith("fatal:");
            }
            return false;
        }

        public bool TryCheckOutBranch(string branch)
        {
            if (RunGit($"git checkout {branch}", out string output))
            {
                return !output.StartsWith("error");
            }
            return false;
        }

        public string[] GetBranchNames()
        {
            if (RunGit("git branch", out string output))
            {
                var branches = output.Split('\n').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x =>
                {
                    x = x.Trim();
                    if (x.StartsWith("* ")) return x.Substring(2);
                    return x;
                }).ToArray();
                return branches;
            }
            return new string[0];
        }

        public string GetCheckedOutBranch()
        {
            if (RunGit("git branch", out string output))
            {
                var branch = output.Split('\n').FirstOrDefault(x => x.StartsWith("* ")).Substring(2);
                return branch;
            }
            return null;
        }

        public bool RunGit(string command, out string output)
        {
            if (!IsGitInstalled)
            {
                output = "Git is not installed.";
                return false;
            }

            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            var drive = RepositoryPath.Split(':')[0];
            cmd.StartInfo.Arguments = $"/C cd {RepositoryPath} && {drive}: && {command}";
            cmd.Start();

            output = cmd.StandardOutput.ReadToEnd().Trim();
            return cmd.ExitCode == 0;
        }

        public string[] GetAllCommits()
        {
            if (RunGit("git rev-list --all", out string output))
            {
                return output.Split('\n').Select(x => x.Trim()).ToArray();
            }
            return new string[0];
        }

        internal void Finish()
        {
            Finished?.Invoke();
        }

        internal void Report(string v)
        {
            UpdateProgress?.Invoke(v);
        }

        public string GetName()
        {
            if (RunGit("git rev-parse --show-toplevel", out string output))
            {
                return output;
            }
            return null;
        }

        public string GetSafeName()
        {
            if (RunGit("git rev-parse --show-toplevel", out string output))
            {
                if (output.IndexOf('/') >= 0)
                {
                    return output.Split('/').Last();
                }else
                {
                    return output.Split('\\').Last();
                }
            }
            return null;
        }
    }
}
