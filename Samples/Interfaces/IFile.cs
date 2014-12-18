using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IFile : INode
    {
        IFile MoveTo(IDirectory newParent);
        IFile Rename(string newName);
        void PutContent(string content);
        string Read();
    }
}
