using GitAnalyzer.Modules.GitObjects;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GitAnalyzer.Modules.BuiltIn
{
    public sealed class GitCommitsPerAuthor : BaseModule
    {
        public class GitAuthorCounter
        {
            [Searchable]
            public string Author { get; set; }
            [Searchable]
            public int CommitAmount { get; set; }
        }

        public override string ModuleName => "Commits per Author";
        public override Func<object, object> DefaultSort => x => ((GitAuthorCounter)x).CommitAmount;
        
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
                var sorted = gitObjects.OrderByDescending(x => x.CommitAmount).ToArray();

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

        private GitAuthorCounter[] ProcessChunks(GitRepository repo, IEnumerable<string>[] chunked, BackgroundWorker bgWorker)
        {
            ConcurrentDictionary<string, int> rawResults = new ConcurrentDictionary<string, int>();
            Parallel.For(0, chunked.Length, chunkIndx =>
            {
                var chunk = chunked[chunkIndx];
                Parallel.ForEach(chunk, (commit) =>
                {
                    if (repo.RunGit($" git log -1 --format='%ae' {commit}", out string output))
                    {
                        rawResults.AddOrUpdate(output, 1, (k, v) => v + 1);
                    }
                });
                bgWorker.ReportProgress(0, $"Chunk {chunkIndx} of {chunked.Length}, total authors: {rawResults.Count}");
            });
            return rawResults.Select(x=> new GitAuthorCounter { Author = x.Key, CommitAmount = x.Value}).ToArray();
        }
    }
}
