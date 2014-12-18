using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ExpressionEvaluator;
using Rpcsharp.Parser;

namespace Rpcsharp
{
    /// <summary>
    /// Server-side handler of RPC# requests
    /// </summary>
    public static class RpcEvaluator
    {
        /// <summary>
        /// Function to call when a server request is received
        /// </summary>
        /// <param name="evaluation">The incoming request</param>
        /// <param name="referenceResolver">Your reference loader: Resolves references to server-side objects</param>
        /// <returns></returns>
        public static SerializedEvaluation HandleIncomingRequest(SerializedEvaluation evaluation, Func<string, IRpcRoot> referenceResolver)
        {
            var reg = new TypeRegistry();
            for (int i = 0; i < evaluation.References.Length; i++)
            {
                reg.RegisterSymbol("r" + (i + 1), referenceResolver(evaluation.References[i]));
            }
            var p = new CompiledExpression(evaluation.Evaluation)
            {
                TypeRegistry = reg
            };
            var ret = p.Eval();
            if (ret == null)
                return null;
            return new RpcCallVisitor().Serialize(Expression.Constant(ret));
        }

        internal static async Task<object> HandleResultAsync(SerializedEvaluation evaluation, Func<string, Task<IRpcRoot>> referenceResolver)
        {
            if (evaluation == null || string.IsNullOrEmpty(evaluation.Evaluation))
                return null;
            
            var reg = new TypeRegistry();

            for (int i = 0; i < evaluation.References.Length; i++)
            {
                reg.RegisterSymbol("r" + (i + 1), await referenceResolver(evaluation.References[i]).ConfigureAwait(false));
            }
            var p = new CompiledExpression(evaluation.Evaluation)
            {
                TypeRegistry = reg
            };
            return p.Eval();
        }

        internal static object HandleResult(SerializedEvaluation evaluation, Func<string, IRpcRoot> referenceResolver)
        {
            if (evaluation == null || string.IsNullOrEmpty(evaluation.Evaluation))
                return null;
            
            var reg = new TypeRegistry();
            for (int i = 0; i < evaluation.References.Length; i++)
            {
                reg.RegisterSymbol("r" + (i + 1), referenceResolver(evaluation.References[i]));
            }
            var p = new CompiledExpression(evaluation.Evaluation)
            {
                TypeRegistry = reg
            };
            return p.Eval();
        }
    }
}
