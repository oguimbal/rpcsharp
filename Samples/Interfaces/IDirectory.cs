namespace Interfaces
{
    public interface IDirectory
        : INode
    {
        IDirectory MoveTo(IDirectory newParent);
        IDirectory Rename(string newName);
        IDirectory CreateSubfolder(string name);
        IFile CreateFile(string name);

        INode[] ListChildNodes();
    }
}