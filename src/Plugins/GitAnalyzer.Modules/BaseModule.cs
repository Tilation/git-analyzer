using GitAnalyzer.Modules.GitObjects;
using System;
using System.Collections;

namespace GitAnalyzer.Modules
{
    /// <summary>
    /// All plugins or modules need to inherit this class in order to be shown and executed in the main program.
    /// </summary>
    public abstract class BaseModule
    {
        /// <summary>
        /// The display name of the Module.
        /// <para>It is also used when serialize the resulting data and saving it to a file.</para>
        /// </summary>
        public abstract string ModuleName { get; }

        /// <summary>
        /// This is the object that will hold the parameters for the module.
        /// <para>All its properties with the attribute <see cref="SearchableAttribute"/> will be shown to the user.</para>
        /// </summary>
        public virtual ModuleParameters ModuleParameters { get; } = new ModuleParameters();

        /// <summary>
        /// This is a property selector for sorting the resulting data, see <see cref="BuiltIn.GitHistoricFiles"/> for an example.
        /// </summary>
        public virtual Func<object, object> DefaultSort => null;

        /// <summary>
        /// This will hold the resulting data and be shown in the Results tab.
        /// <para>Must be set using <see cref="SubmitExecutionResults(IList)"/> at the end of the execution to show results.</para>
        /// </summary>
        public IList ModuleResult { get; private set; }

        public delegate void ExecutionProgressChanged(BaseModule sender, ExecutionLogMessageReceivedEventArgs state);
        public delegate void ExecutionResultSubmitted(BaseModule sender, ExecutionFinishedEventArgs args);

        /// <summary>
        /// This event is triggered when there is new data to be shown to the user.
        /// </summary>
        public event ExecutionProgressChanged ModuleLogMessageReceived;

        /// <summary>
        /// This event is triggered when:
        /// <list type="bullet">
        /// <item>The module finished and there is a  <see cref="BaseModule.ModuleResult"/> to be shown.</item>
        /// <item>A dump file is loaded for the module.</item>
        /// </list>
        /// </summary>
        public event ExecutionResultSubmitted ModuleExecutionFinished;

        /// <summary>
        /// This triggers when the user presses the Run button.
        /// <para>Before this is called, it switches branches to the selected one.</para>
        /// </summary>
        /// <param name="repo"></param>
        protected abstract void ExecuteModule(GitRepository repo);

        /// <summary>
        /// Sets the <see cref="ModuleResult"/> and triggers the event <see cref="ModuleExecutionFinished"/>.
        /// </summary>
        /// <param name="result"></param>
        protected void SubmitExecutionResults(IList result)
        {
            ModuleResult = result;
            ModuleExecutionFinished?.Invoke(this, new ExecutionFinishedEventArgs { Result = result });
        }

        /// <summary>
        /// Notifies the controls that there is Log data to be shown to the user.
        /// <para>Triggers the event <see cref="ModuleLogMessageReceived"/>.</para>
        /// </summary>
        /// <param name="state"></param>
        protected void SubmitLogMessage(string state)
        {
            ModuleLogMessageReceived?.Invoke(this, new ExecutionLogMessageReceivedEventArgs { State = state });
        }

        /// <summary>
        /// Switches the branch to the one in <see cref="ModuleParameters.Branch"/> and calls <see cref="BaseModule.ExecuteModule(GitRepository)"/>.
        /// </summary>
        /// <param name="repo"></param>
        public void Run(GitRepository repo)
        {
            if (repo.TryCheckOutBranch(ModuleParameters.Branch)) ExecuteModule(repo);
        }

        /// <summary>
        /// This is called when loading serialized data.
        /// <para>Sets <see cref="ModuleResult"/> and triggers the event <see cref="ModuleExecutionFinished"/>.</para>
        /// </summary>
        /// <param name="list"></param>
        public void SetResult(IList list)
        {
            ModuleResult = list;
            ModuleExecutionFinished?.Invoke(this, new ExecutionFinishedEventArgs { Result = ModuleResult });
        }
    }
}
