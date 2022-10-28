using GitAnalyzer.Modules;
using GitAnalyzer.Modules.GitObjects;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mod.ShowCommits
{
    public class ShowCommitsModule : BaseModule
    {
        public class GitResult
        {
            [Searchable]
            public string GitHash { get; set; }
            [Searchable]
            public string Description { get; set; }
            [Searchable]
            public string Author { get; set; }
            public DateTime Date { get; set; }
        }
        public override string ModuleName => "Show all commits";

        public override object ModuleParameters => new object();

        public override Func<object, object> DefaultSort => throw new NotImplementedException();

        protected override void ExecuteModule(GitRepository repo)
        {
            BackgroundWorker bgWorker = new BackgroundWorker();
            bgWorker.WorkerReportsProgress = true;
            bgWorker.DoWork += (s, e) =>
            {
                bgWorker.ReportProgress(0, $"Searching for commits...");
                var commits = repo.GetAllCommits();
                bgWorker.ReportProgress(0, $"Found {commits.Length} commits");

                foreach(var commit in commits)
                {
                    if (repo.RunGit($"git show -s --format=\"%H %an %at %s\" {commit}", out string output))
                    {

                    }
                }
                var chunked = commits.Split(10).ToArray();
                bgWorker.ReportProgress(0, $"Processing chunks...");

                var gitObjects = ProcessChunks(repo, chunked, bgWorker);
                bgWorker.ReportProgress(0, $"Found {gitObjects.Length} unique objects");

                bgWorker.ReportProgress(0, $"Sorting by size...");
                var sorted = gitObjects.OrderByDescending(x => x.Date).ToArray();

                bgWorker.ReportProgress(0, $"Ready!");
                e.Result = sorted;
            };
            bgWorker.RunWorkerCompleted += (s, e) =>
            {
                SubmitExecutionResults(e.Result as IList);
            };
            bgWorker.ProgressChanged += (s, e) =>
            {
                SubmitProgressChanged(e.UserState.ToString());
            };
            bgWorker.RunWorkerAsync();
        }

        private GitResult[] ProcessChunks(GitRepository repo, IEnumerable<string>[] chunked, BackgroundWorker bgWorker)
        {
            ConcurrentDictionary<string, GitResult> rawResults = new ConcurrentDictionary<string, GitResult>();
            Parallel.For(0, chunked.Length, chunkIndx =>
            {
                var chunk = chunked[chunkIndx];
                Parallel.ForEach(chunk, (commit) =>
                {
                    if (repo.RunGit($"git show -s --format=\"%H %an %at %s\" {commit}", out string output))
                    {
                        var parts = output.Split(new[] { ' ' }, 4);
                        var hash = parts[0];
                        var author = parts[1];
                        var stamp = int.TryParse(parts[2], out int val) ? val : -1;
                        var desc = parts[3];
                        GitResult r = new GitResult
                        {
                            GitHash = hash,
                            Author = author,
                            Date = stamp == -1 ? DateTime.MaxValue : new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(stamp).ToLocalTime(),
                            Description = desc,
                        };
                        rawResults.TryAdd(hash, r);
                    }
                });
                bgWorker.ReportProgress(0, $"Chunk {chunkIndx} of {chunked.Length}, total objects: {rawResults.Count}");
            });
            return rawResults.Values.ToArray();
        }
    }
}
