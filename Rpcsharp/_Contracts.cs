using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Rpcsharp
{

    public interface IRpcRoot
    {
        string Reference { get; }
    }

    public interface IRpcService
    {
        SerializedEvaluation InvokeRemote(SerializedEvaluation visited);
        IRpcRoot ResolveReference(string reference);
    }

    public interface IRpcServiceAsync
    {
        Task<SerializedEvaluation> InvokeRemoteAsync(SerializedEvaluation visited);
        Task<IRpcRoot> ResolveReferenceAsync(string reference);
    }


    [DataContract]
    public class SerializedEvaluation
    {
        [DataMember(Order = 1)]
        public string Evaluation { get; set; }
        [DataMember(Order = 2)]
        public string[] References;
    }


    public class RpcAttribute : Attribute { }
}
