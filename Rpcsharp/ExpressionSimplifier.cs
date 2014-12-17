using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rpcsharp
{

    public static class ExpressionSimplifier
    {
        public static Expression Simplify(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Constant)
                return expression;
            var nominator = new Nominator();
            nominator.Visit(expression);
            if (nominator.mayBeEvaluated.Count == 0) // no possible evaluations
                return expression;
            var ret = new Evaluator(nominator.mayBeEvaluated).Visit(expression);
            return ret;
        }
        
        class Nominator : ExpressionVisitor
        {
            public readonly HashSet<Expression> mayBeEvaluated = new HashSet<Expression>();
            bool evaluate = true;

            public override Expression Visit(Expression node)
            {
                if (node == null)
                    return null;
                // here 'evaluate' means nothing for this stack.
                var oldEvaluate = evaluate;

                evaluate = true;
                var visited = base.Visit(node);
                // when arriving here, 'evaluate' is true only if sub-evaluations can be evaluated
                if (evaluate
                    && node.NodeType != ExpressionType.Lambda
                    && node.NodeType != ExpressionType.Parameter
                    && node.NodeType != ExpressionType.Constant)
                    mayBeEvaluated.Add(node);

                evaluate = oldEvaluate && evaluate;
                return visited;
            }

            protected override Expression VisitNew(NewExpression node)
            {
                evaluate = false;
                return base.VisitNew(node);
            }

            readonly HashSet<ParameterExpression> evaluableParameters = new HashSet<ParameterExpression>();

            public Nominator()
            {
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                evaluate &= evaluableParameters.Contains(node);
                return base.VisitParameter(node);
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (!node.Method.IsStatic)
                {// potential rpc call
                    
                    evaluate = evaluate && node.Method.GetCustomAttribute<RpcAttribute>(true) == null;

                    if (evaluate && !node.Object.Type.IsInterface && typeof (IRpcRoot).IsAssignableFrom(node.Object.Type))
                    {
                        // lets get suspicious: No RpcAttribute on an rpc root object ? That may be a 'fake' implementation of a rpc method
                        var rpcIntefaces = node.Object.Type
                            .GetInterfaces()
                            .Where(i => i != typeof (IRpcRoot) && typeof (IRpcRoot).IsAssignableFrom(i))
                            .ToArray();

                        foreach (var rpci in rpcIntefaces)
                        {
                            var map = node.Object.Type.GetInterfaceMap(rpci);
                            var i = Array.IndexOf(map.TargetMethods, node.Method);
                            if (i >= 0)
                            {
                                // this is a method implementation !
                                evaluate = evaluate && map.InterfaceMethods[i].GetCustomAttribute<RpcAttribute>() == null;
                                break;
                            }
                        }
                    }

                    // explore object simplifications
                    Visit(node.Object);

                    // explore arguments simplifications
                    foreach (var arg in node.Arguments)
                        Visit(arg);

                    return node;
                }
                // todo: Extension methods/static methods calls on rcs => security ?

                return base.VisitMethodCall(node);
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Expression != null)
                    Visit(node.Expression);
                evaluate = evaluate && node.Member.GetCustomAttribute<RpcAttribute>() == null;
                return node;
            }
        }

        class Evaluator : ExpressionVisitor
        {
            readonly HashSet<Expression> mayBeEvaluated;

            public Evaluator(HashSet<Expression> mayBeEvaluated)
            {
                this.mayBeEvaluated = mayBeEvaluated;
            }
            public override Expression Visit(Expression node)
            {
                if (mayBeEvaluated.Contains(node))
                {
                    var value = Expression.Lambda<Func<object>>(Expression.Convert(node, typeof(object))).Compile()();
                    return Expression.Constant(value, node.Type);
                }
                else
                    return base.Visit(node);
            }
        }
    }

}
