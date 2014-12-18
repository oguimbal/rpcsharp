using System.Threading.Tasks;
using Interfaces;
using Rpcsharp;
using Server.Model;

namespace Server
{
    class WcfServer : IWcfServer
    {
        public async Task<SerializedEvaluation> Evaluate(SerializedEvaluation toEvaluate)
        {
            return RpcEvaluator.HandleIncomingRequest(toEvaluate, reference =>
            {
                if (reference.StartsWith("dir:"))
                    return new ServerDirectory(reference.Substring("dir:".Length));
                if (reference.StartsWith("file:"))
                    return new ServerFile(reference.Substring("file:".Length));
                return null;
            });
        }
    }
}
