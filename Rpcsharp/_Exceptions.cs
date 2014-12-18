using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rpcsharp
{

    public class CannotCallRemoteMethodException : Exception
    {
        public CannotCallRemoteMethodException()
            : base("A remote method cannot be called directly. Use service.Call(()=>...) or service.CallAsync(()=>...)")
        {
        }
    }
    public class InvalidExpressionInRpcCallException : Exception
    {
        public Expression InvalidExpression { get; private set; }

        public InvalidExpressionInRpcCallException(Expression exp, string message)
            : base(message + " in " + exp)
        {
            InvalidExpression = exp;
        }
    }
    public class InvalidConstantInRpcCallException : InvalidExpressionInRpcCallException
    {
        public InvalidConstantInRpcCallException(ConstantExpression constant)
            : base(constant, "Cannot convert type " + constant.Type + " into a compile-time literal value, nor into a IRpcRoot.")
        {
        }
    }
}
