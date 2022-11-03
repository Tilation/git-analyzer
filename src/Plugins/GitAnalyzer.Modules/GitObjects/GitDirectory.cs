using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitAnalyzer.Modules.GitObjects
{
    public sealed class GitDirectory
    {
        [Searchable]
        public string Path { get; set; }
        [Searchable]
        public int Hits { get; set; } = 1;
    }
}
