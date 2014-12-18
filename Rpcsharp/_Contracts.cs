using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Rpcsharp
{

    public interface IRpcRoot
    {
        string Reference { get; }
    }

    /// <summary>
    /// Minimum interface to implement a basic synchronous RCP# client
    /// </summary>
    public interface IRpcService
    {
        /// <summary>
        /// Invoked when RPC# needs to evaluate an expression server-side
        /// </summary>
        /// <remarks>
        /// When called, it is your responsibility to handle serialization, and call your server (whatever transport you want to use)
        /// </remarks>
        /// <param name="visited">Serializable class to send to the server</param>
        /// <returns>Server results <see cref="RpcEvaluator.HandleIncomingRequest"/></returns>
        SerializedEvaluation InvokeRemote(SerializedEvaluation visited);
        /// <summary>
        /// Resolve client-side references. Will be called to get object stubs from reference client-side, if a RPC has returned an IRpcRoot object.
        /// </summary>
        /// <remarks>
        /// You are recomanded to return 'stubs'... i.e. not-yet-loaded objects. 
        /// </remarks>
        /// <param name="reference">The reference to resolve</param>
        /// <returns>Resolved object</returns>
        IRpcRoot ResolveReference(string reference);
    }

    /// <summary>
    /// Minimum interface to implement a basic asynchronous RCP# client
    /// </summary>
    public interface IRpcServiceAsync
    {
        /// <summary>
        /// Invoked when RPC# needs to evaluate an expression server-side
        /// </summary>
        /// <remarks>
        /// When called, it is your responsibility to handle serialization, and call your server (whatever transport you want to use)
        /// </remarks>
        /// <param name="visited">Serializable class to send to the server</param>
        /// <returns>Server results <see cref="RpcEvaluator.HandleIncomingRequest"/></returns>
        Task<SerializedEvaluation> InvokeRemoteAsync(SerializedEvaluation visited);

        /// <summary>
        /// Resolve client-side references. Will be called to get object stubs from reference client-side, if a RPC has returned an IRpcRoot object.
        /// </summary>
        /// <remarks>
        /// You are recomanded to return 'stubs'... i.e. not-yet-loaded objects. 
        /// </remarks>
        /// <param name="reference">The reference to resolve</param>
        /// <returns>Resolved object</returns>
        Task<IRpcRoot> ResolveReferenceAsync(string reference);
    }


    public interface IRpcLoadService
    {
        void Load<T>(string reference);
    }


    /// <summary>
    /// Represents an evaluation: Either a request, or a return value.
    /// </summary>
    [DataContract]
    public class SerializedEvaluation
    {
        [DataMember(Order = 1)]
        public string Evaluation { get; set; }
        [DataMember(Order = 2)]
        public string[] References { get; set; }
    }

    /// <summary>
    /// Attribute to be put on members that are 'remote'
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RpcAttribute : Attribute { }


    

}
