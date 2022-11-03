using GitAnalyzer.Modules.GitObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GitAnalyzer.Modules.BuiltIn
{
    public sealed class GitHistoricDirectories : BaseModule
    {
        private readonly Regex reg = new Regex(@"(\w+)\s+(\w+)\s+(\w+)\s+(.*?)\s+(.*)", RegexOptions.Compiled);

        public override string ModuleName => "List historic directories";

        protected override async void ExecuteModule(GitRepository repo)
        {
            var branches = await Task.Run(() => repo.GetBranchNames());
            var branch = await Task.Run(() => repo.GetCheckedOutBranch());

            SubmitLogMessage( $"Searching for commits...");
            var commits = await Task.Run(() => repo.GetAllCommits());
            SubmitLogMessage($"Found {commits.Length} commits");

            var chunked = commits.Split(10).ToArray();
            SubmitLogMessage($"Processing chunks...");

            var gitDirs = await Task.Run(() =>
            {
                ConcurrentDictionary<string, GitDirectory> dirs = new ConcurrentDictionary<string, GitDirectory>();
                Parallel.For(0, chunked.Length, i =>
                {
                    Parallel.ForEach(chunked[i], commit =>
                    {
                        if (repo.RunGit($"git ls-tree -r --name-only {commit}", out string output))
                        {
                            foreach (string file in output.Split('\n'))
                            {
                                int indxbs = file.LastIndexOf('\\');
                                int indxfs = file.LastIndexOf('/');
                                if (indxbs >= 0 || indxfs >= 0)
                                {
                                    GitDirectory obj = new GitDirectory() { Path = file.Substring(0, (indxfs >= 0 ? indxfs : indxbs) + 1) };
                                    if (!dirs.TryAdd(obj.Path, obj))
                                        if (dirs.TryGetValue(obj.Path, out var obj2))
                                            obj2.Hits++;
                                }
                            }
                        }
                    });
                    SubmitLogMessage($"Processed chunk {i} of {chunked.Length}, directories: {dirs.Count}");
                });
                return dirs.Values.ToArray();
            });
            SubmitLogMessage($"Found {gitDirs.Length} unique objects");

            SubmitLogMessage($"Sorting by size...");
            var sorted = gitDirs.OrderByDescending(x => x.Path).ToArray();

            SubmitLogMessage($"Ready!");
            SubmitExecutionResults(sorted);
        }
    }
}
