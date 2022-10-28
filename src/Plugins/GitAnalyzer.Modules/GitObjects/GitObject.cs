using System;

namespace GitAnalyzer.Modules.GitObjects
{
    public enum GitObjectType
    {
        blob,
        commit,
        tree
    }
    public class GitObject
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
        [Searchable]
        public string Path { get; set; }
        public int FileSizeInBytes { get; set; }
        [Searchable]
        public string GitHash { get; set; }
        public int Hits { get; set; }
        public GitObjectType ObjectType { get; set; }
    }
}
