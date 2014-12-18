using Rpcsharp;

namespace Interfaces
{
    public interface INode
        : IRpcRoot
    {
        string Name { get; }
        string FullPath { get; }
        bool Exists { get; }

        void Delete();
    }
}