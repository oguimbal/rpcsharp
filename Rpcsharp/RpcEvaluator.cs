using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ExpressionEvaluator;

namespace Rpcsharp
{
    public static class RpcEvaluator
    {
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
                reg.RegisterSymbol("r" + (i + 1), await referenceResolver(evaluation.References[i]));
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
