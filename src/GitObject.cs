using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitAnalyzer
{
    public enum GitObjectType
    {
        blob,
        commit,
        tree
    }
    internal class GitObject
    {
        public GitObject(string mode, string objecttype, string hash, string size, string path)
        {
            Mode = mode;
            switch (objecttype)
            {
                case "blob": ObjectType = GitObjectType.blob; break;
                case "commit": ObjectType = GitObjectType.commit; break;
                case "tree": ObjectType = GitObjectType.tree; break;
            }
            GitHash = hash;
            FileSizeInBytes = size == "-" ? -1 : Convert.ToInt32(size);
            Path = path;
        }
        public string Mode { get; set; }
        public string Path { get; set; }
        public int FileSizeInBytes { get; set; }
        public string GitHash { get; set; }
        public int Hits { get; set; }
        public GitObjectType ObjectType { get; set; }
    }
}
