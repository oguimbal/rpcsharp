using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Rpcsharp;

namespace Interfaces
{
    [ServiceContract]
    public interface IWcfServer
    {
        [OperationContract]
        Task<SerializedEvaluation> Evaluate(SerializedEvaluation toEvaluate);
    }
}
