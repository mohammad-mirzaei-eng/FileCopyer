using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCopyer.Models
{
    public class DirectoryNode
    {
        public DirectoryInfo Directory { get; }
        public List<FileInfo> Files { get; }
        public List<DirectoryNode> SubDirectories { get; }

        public DirectoryNode(DirectoryInfo directory)
        {
            Directory = directory;
            Files = new List<FileInfo>();
            SubDirectories = new List<DirectoryNode>();
        }

        public string FullName => Directory.FullName;
    }
}
