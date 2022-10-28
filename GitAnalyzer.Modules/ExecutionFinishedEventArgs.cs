using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitAnalyzer.Modules
{
    public sealed class ExecutionFinishedEventArgs : EventArgs
    {
        public IList Result { get; set; }
    }
}
