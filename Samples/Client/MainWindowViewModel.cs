using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class MainWindowViewModel
    {
        public MainWindowViewModel()
        {
            Root = new DirectoryPresenter();
        }

        public DirectoryPresenter Root { get; private set; }
    }
}
