using System.IO;
using Interfaces;

namespace Server.Model
{
    class ServerFile : IFile
    {
        public ServerFile(string path)
        {
            FullPath = path.Replace('\\', '/');
            Name = FullPath.Substring(FullPath.LastIndexOf('/') + 1);
        }

        public string Reference { get { return "file:" + FullPath; } }
        public string Name { get; private set; }
        public string FullPath { get; private set; }
        public bool Exists { get { return File.Exists(FullPath); } }

        public void Delete()
        {
            File.Delete(FullPath);
        }

        public IFile MoveTo(IDirectory newParent)
        {
            var newPath = Path.Combine(newParent.FullPath, Name);
            File.Move(FullPath, newPath);
            return new ServerFile(newPath);
        }

        public IFile Rename(string newName)
        {
            var path = FullPath.Substring(0, FullPath.Length - Name.Length);
            var newPath = Path.Combine(path, newName);
            File.Move(FullPath, newPath);
            return new ServerFile(newPath);
        }

        public void PutContent(string content)
        {
            File.WriteAllText(FullPath, content);
        }

        public string Read()
        {
            return File.ReadAllText(FullPath);
        }
    }
}