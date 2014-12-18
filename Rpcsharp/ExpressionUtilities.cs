using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rpcsharp
{
    static class ExpressionUtilities
    {
        public static Expression Unwrap(this Expression expr)
        {
            if (expr.NodeType == ExpressionType.Convert || expr.NodeType == ExpressionType.Quote)
                expr = ((UnaryExpression)expr).Operand;
            return expr;
        }

        public static MethodInfo GetCalledMethod(this Expression expr, bool unwrapGeneric = true)
        {
            var meth = expr.Unwrap() as MethodCallExpression;
            if (unwrapGeneric && meth.Method.IsGenericMethod && !meth.Method.IsGenericMethodDefinition)
                return meth.Method.GetGenericMethodDefinition();
            return meth.Method;
        }
        public static MethodInfo GetCalledMethod<T>(this Expression<Func<T, object>> expr, bool unwrapGeneric = true)
        {
            return expr.Body.GetCalledMethod(unwrapGeneric);
        }
        public static MethodInfo GetCalledMethod<T>(this Expression<Action<T>> expr, bool unwrapGeneric = true)
        {
            return expr.Body.GetCalledMethod(unwrapGeneric);
        }
        public static MethodInfo GetCalledMethod(this Expression<Action> expr, bool unwrapGeneric = true)
        {
            return expr.Body.GetCalledMethod(unwrapGeneric);
        }

        public static PropertyInfo GetProperty<T>(Expression<Func<T, object>> func)
        {
            var body = (MemberExpression) func.Body.Unwrap();
            return (PropertyInfo) body.Member;
        }
    }
}
