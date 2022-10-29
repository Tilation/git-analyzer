using GitAnalyzer.Modules.GitObjects;
using System;
using System.Collections;

namespace GitAnalyzer.Modules
{
    public abstract class BaseModule
    {
        public abstract string ModuleName { get; }
        public IList ModuleResult { get; private set; }
        public virtual ModuleParameters ModuleParameters { get; } = new ModuleParameters();
        public virtual Func<object, object> DefaultSort => null;

        public delegate void ExecutionProgressChanged(BaseModule sender, ExecutionProgressChangedEventArgs state);
        public delegate void ExecutionResultSubmitted(BaseModule sender, ExecutionFinishedEventArgs args);

        public event ExecutionProgressChanged ModuleProgressChanged;
        public event ExecutionResultSubmitted ModuleExecutionFinished;



        protected abstract void ExecuteModule(GitRepository repo);

        protected void SubmitExecutionResults(IList result)
        {
            ModuleResult = result;
            ModuleExecutionFinished?.Invoke(this, new ExecutionFinishedEventArgs { Result = result });
        }

        protected void SubmitProgressChanged(string state)
        {
            ModuleProgressChanged?.Invoke(this, new ExecutionProgressChangedEventArgs { State = state });
        }

        public void Run(GitRepository repo)
        {
            if (repo.TryCheckOutBranch(ModuleParameters.Branch)) ExecuteModule(repo);
        }

        public void SetResult(IList list)
        {
            ModuleResult = list;
            ModuleExecutionFinished?.Invoke(this, new ExecutionFinishedEventArgs { Result = ModuleResult });
        }
    }
}
