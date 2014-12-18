using System.IO;
using System.Linq;
using Interfaces;

namespace Server.Model
{
    class ServerDirectory : IDirectory
    {
        public ServerDirectory(string newPath)
        {
            if (newPath == "/")
                newPath = "C:/";
            else
                newPath = newPath.Replace('\\', '/').TrimEnd('/');
            FullPath = newPath;
            Name = FullPath.Substring(FullPath.LastIndexOf('/') + 1);
        }

        public string Reference { get { return "dir:"+ FullPath; }}
        public string Name { get; private set; }
        public string FullPath { get; private set; }
        public bool Exists { get { return Directory.Exists(FullPath); } }

        public IDirectory MoveTo(IDirectory newParent)
        {
            var newPath = Path.Combine(newParent.FullPath, Name);
            Directory.Move(FullPath, newPath);
            return new ServerDirectory(newPath);
        }

        public IDirectory Rename(string newName)
        {
            var path = FullPath.Substring(0, FullPath.Length - Name.Length);
            var newPath = Path.Combine(path, newName);
            Directory.Move(FullPath,newPath);
            return new ServerDirectory(newPath);
        }

        public void Delete()
        {
            Directory.Delete(FullPath);
        }

        public INode[] ListChildNodes()
        {
            return  Directory.EnumerateDirectories(FullPath)
                    .Select(d => (INode) new ServerDirectory(d))
                .Concat(
                    Directory.EnumerateFiles(FullPath)
                    .Select(f => (INode) new ServerFile(f)))
                .ToArray();
        }

        public IDirectory CreateSubfolder(string name)
        {
            var newPath = Path.Combine(FullPath, name);
            Directory.CreateDirectory(newPath);
            return new ServerDirectory(newPath);
        }

        public IFile CreateFile(string name)
        {
            return new ServerFile(Path.Combine(FullPath, name));
        }
    }
}
