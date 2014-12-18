using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Interfaces;
using Rpcsharp;

namespace Client
{
    public class SourceContext
    {
        public ObservableCollection<IRpcServiceAsync> Services { get; private set; }

        public SourceContext()
        {
            Services = new ObservableCollection<IRpcServiceAsync>();
            Init();
        }

        async void Init()
        {
            Services.Add(new MemoryService());
        }
    }

    public class MemoryService : IRpcServiceAsync
    {
        static Dictionary<string,INode> _nodes = new Dictionary<string, INode>
        {
            {"dir:/",new MemoryDir{Name = "", Reference = "dir:/"}},
        };

        public async Task<SerializedEvaluation> InvokeRemoteAsync(SerializedEvaluation visited)
        {
            // this is supposed to be handled server-side... let's fake it.
            return RpcEvaluator.HandleIncomingRequest(visited, reference =>
            {
                INode val;
                _nodes.TryGetValue(reference, out val);
                return val;
            });
        }

        public async Task<IRpcRoot> ResolveReferenceAsync(string reference)
        {
            // client-side reference solver
            if (reference.StartsWith("file:"))
                return Proxy.Stub<IFile>(reference);
            if (reference.StartsWith("dir:"))
                return Proxy.Stub<IDirectory>(reference);
            return null;
        }

        
        class MemoryDir : IDirectory
        {
            public string Reference { get; set; }
            public string Name { get; set; }
            public string FullPath { get; private set; }
            public bool Exists { get; private set; }
            public void Delete()
            {
                throw new System.NotImplementedException();
            }

            public IDirectory MoveTo(IDirectory newParent)
            {
                throw new System.NotImplementedException();
            }

            public IDirectory Rename(string newName)
            {
                throw new System.NotImplementedException();
            }

            public IDirectory CreateSubfolder(string name)
            {
                throw new System.NotImplementedException();
            }

            public IFile CreateFile(string name)
            {
                throw new System.NotImplementedException();
            }

            INode[] _chidren;
            public INode[] ListChildNodes()
            {
                if (_chidren != null)
                    return _chidren;
                var lst = new List<INode>();
                var dir = new MemoryDir { Name = "Infinite", Reference = Reference + "Infinite/" };
                lst.Add(dir);
                _nodes.Add(dir.Reference, dir);
                var file = new MemoryFile {Name = "file.txt", Reference = "file:"+ Reference.Substring("dir:".Length) + "file.txt"};
                lst.Add(file);
                _nodes.Add(file.Reference, file);
                return _chidren = lst.ToArray();
            }
        }
    }

    class MemoryFile : IFile
    {
        public string Reference { get; set; }
        public string Name { get; set; }
        public string FullPath { get; private set; }
        public bool Exists { get; private set; }
        public void Delete()
        {
            throw new System.NotImplementedException();
        }

        public IFile MoveTo(IDirectory newParent)
        {
            throw new System.NotImplementedException();
        }

        public IFile Rename(string newName)
        {
            throw new System.NotImplementedException();
        }

        string _content = "";
        public void PutContent(string content)
        {
            _content = content;
        }

        public string Read()
        {
            return _content;
        }
    }
}