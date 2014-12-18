using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Interfaces;
using Rpcsharp;

namespace Client
{
    public class DirectoryPresenter
    {
        IDirectory _directory;
        IRpcServiceAsync _service;
        public ICommand Load { get; private set; }

        ObservableCollection<object> _nodes;
        public ObservableCollection<object> Nodes
        {
            get
            {
                if (_nodes == null)
                {
                    _nodes = new ObservableCollection<object>();
                    if(_service!=null)
                        DoLoad();
                }
                return _nodes;
            }
        }

        public string Name
        {
            get { return _directory==null?"/" : (_directory.Name??""); }
        }

        public IRpcServiceAsync Service
        {
            get { return _service; }
            set
            {
                SetServiceAsync(value);
            }
        }

        public async Task SetServiceAsync(IRpcServiceAsync value)
        {
            _service = value;
            Nodes.Clear();
            if(_service==null)
                return;
            _directory = (IDirectory)await _service.ResolveReferenceAsync("dir:/");
            await DoLoad();
        }


        public DirectoryPresenter()
        {
        }

        public DirectoryPresenter(IRpcServiceAsync service, IDirectory directory)
        {
            _service = service;
            _directory = directory;
        }

        async Task DoLoad()
        {
            var subNodes = await _service.CallAsync(() => _directory.ListChildNodes());
            foreach (var n in subNodes.OfType<IDirectory>())
                _nodes.Add(new DirectoryPresenter(_service, n));
            foreach (var n in subNodes.OfType<IFile>())
                _nodes.Add(new FilePresenter(_service,n));
        }
    }
}
