using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitAnalyzer.Modules
{
    public sealed class ExecutionLogMessageReceivedEventArgs : EventArgs
    {
        public string State { get; set; }
    }
}
