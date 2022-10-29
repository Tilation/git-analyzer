using GitAnalyzer.Modules;
using GitAnalyzer.Modules.GitObjects;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GitAnalyzer.Modules.BuiltIn
{
    public class GitHistoricFiles : BaseModule
    {
        private readonly Regex reg = new Regex(@"(\w+)\s+(\w+)\s+(\w+)\s+(.*?)\s+(.*)", RegexOptions.Compiled);
        public override string ModuleName => "List historic files";
        public override Func<object, object> DefaultSort => x => ((GitObject)x)?.FileSizeInBytes;

        protected override void ExecuteModule(GitRepository repo)
        {
            var branches = repo.GetBranchNames();
            var branch = repo.GetCheckedOutBranch();
            BackgroundWorker bgWorker = new BackgroundWorker();
            bgWorker.WorkerReportsProgress = true;
            bgWorker.DoWork += (s, e) =>
            {
                bgWorker.ReportProgress(0, $"Searching for commits...");
                var commits = repo.GetAllCommits();
                bgWorker.ReportProgress(0, $"Found {commits.Length} commits");

                var chunked = commits.Split(10).ToArray();
                bgWorker.ReportProgress(0, $"Processing chunks...");

                var gitObjects = ProcessChunks(repo, chunked, bgWorker);
                bgWorker.ReportProgress(0, $"Found {gitObjects.Length} unique objects");

                bgWorker.ReportProgress(0, $"Sorting by size...");
                var sorted = gitObjects.OrderByDescending(x => x.FileSizeInBytes).ToArray();

                bgWorker.ReportProgress(0, $"Ready!");
                e.Result = sorted;
            };
            bgWorker.RunWorkerCompleted += (s, e) =>
            {
                SubmitExecutionResults(e.Result as IList);
            };
            bgWorker.ProgressChanged += (s, e) =>
            {
                SubmitLogMessage(e.UserState.ToString());
            };
            bgWorker.RunWorkerAsync();
        }

        private GitObject[] ProcessChunks(GitRepository repo, IEnumerable<string>[] chunked, BackgroundWorker bgWorker)
        {
            ConcurrentDictionary<string, GitObject> rawResults = new ConcurrentDictionary<string, GitObject>();
            Parallel.For(0, chunked.Length, chunkIndx =>
            {
                var chunk = chunked[chunkIndx];
                Parallel.ForEach(chunk, (commit) =>
                {
                    if (repo.RunGit($"git ls-tree -r --long {commit}", out string output))
                    {
                        foreach (Match match in reg.Matches(output))
                        {
                            var obj = new GitObject(match.Groups[1].Value,
                                match.Groups[2].Value,
                                match.Groups[3].Value,
                                match.Groups[4].Value,
                                match.Groups[5].Value);
                            if (!rawResults.TryAdd(obj.GitHash, obj))
                                if (rawResults.TryGetValue(obj.GitHash, out var obj2))
                                    obj2.Hits++;
                        }
                    }
                });
                bgWorker.ReportProgress(0, $"Chunk {chunkIndx} of {chunked.Length}, total objects: {rawResults.Count}");
            });
            return rawResults.Values.ToArray();
        }

    }
}
