﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GitAnalyzer
{
    internal class GitRepo
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
        public GitRepo(string path)
        {
            RepositoryPath = path;
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

            output = cmd.StandardOutput.ReadToEnd();
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
    }
}
