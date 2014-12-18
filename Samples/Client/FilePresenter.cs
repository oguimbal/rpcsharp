using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Interfaces;
using Rpcsharp;

namespace Client
{
    public class FilePresenter : INotifyPropertyChanged
    {
        readonly IRpcServiceAsync _service;
        readonly IFile _file;

        public FilePresenter(IRpcServiceAsync service, IFile file)
        {
            _service = service;
            _file = file;
        }

        public string Name { get; private set; }

        bool got = false;
        string _text;
        public string Text
        {
            get
            {
                if (!got)
                    Get();
                return _text;
            }
            set
            {
                _text = value;
                _service.CallAsync(() => _file.PutContent(value));
                OnPropertyChanged();
            }
        }

        async Task Get()
        {
            got = true;
            _text = await _service.CallAsync(() => _file.Read());
            OnPropertyChanged("Text");
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            (PropertyChanged?? delegate { })(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}