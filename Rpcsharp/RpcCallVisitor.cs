using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Rpcsharp
{
    interface IEvaluatesToString
    {
        void AsString(StringBuilder build);
    }

    class RpcCallVisitor : ExpressionVisitor
    {
        class ConstEval : Expression, IEvaluatesToString
        {
            readonly string _value;

            public ConstEval(string value)
            {
                _value = value;
            }

            public void AsString(StringBuilder build)
            {
                build.Append(_value);
            }
        }

        List<IRpcRoot> rpcRootsParameters = new List<IRpcRoot>();

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if(typeof(IRpcRoot).IsAssignableFrom(node.Type))
            { 
                // this constant is a rpc root => add a parameter 
                var rpc = (IRpcRoot) node.Value;
                var r = rpcRootsParameters.IndexOf(rpc) + 1;
                if (r <= 0)
                {
                    rpcRootsParameters.Add(rpc);
                    r = rpcRootsParameters.Count;
                }
                return new ConstEval("r" + r);
            }

            // todo: two ways implicit converions ?

            switch (Type.GetTypeCode(node.Type))
            {
                case TypeCode.Boolean:
                    return new ConstEval(((bool)node.Value) ? "true" : "false");
                case TypeCode.Char:
                    var ch = (char) node.Value;
                    if (ch == '\'')
                        return new ConstEval(@"'\''");
                    return new ConstEval(string.Format("'{0}'", ch));
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    return new ConstEval(string.Format(CultureInfo.InvariantCulture, "{0}", node.Value));
                case TypeCode.Int64:
                    return new ConstEval(string.Format(CultureInfo.InvariantCulture, "{0}L", node.Value));
                case TypeCode.UInt64:
                    return new ConstEval(string.Format(CultureInfo.InvariantCulture, "{0}UL", node.Value));
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return new ConstEval(string.Format(CultureInfo.InvariantCulture, "{0:0.0##################}", node.Value));
                case TypeCode.DateTime:
                    return new ConstEval("#" + ((DateTime)node.Value).ToUniversalTime() + "#");
                case TypeCode.String:
                {

                    var str = (string) node.Value;
                    if (str == null)
                        return new ConstEval("null");
                    // escape string.
                    return new ConstEval("'"+str.Replace(@"\", @"\\").Replace("\"", "\\\"")+"'");
                }
                default:
                    throw new InvalidConstantInRpcCallException(node);
            }
        }


        protected override Expression VisitBinary(BinaryExpression node)
        {
            return VisitOperator(node, node.Left, node.Right);
        }
        protected override Expression VisitUnary(UnaryExpression node)
        {
            return VisitOperator(node, node.Operand);
        }

        
        enum Priority
        { //http://msdn.microsoft.com/fr-fr/library/ms173145%28v=vs.80%29.aspx
            Primary = 1,
            Unary = 2,
            Mult = 3,
            Add = 4,
            Shift = 5,
            Relational = 6,
            Equal = 7,
            Bit_And = 8,
            Bit_Xor = 9,
            Bit_Or = 10,
            Bool_And = 11,
            Bool_Or = 12,
            Bool_If = 13,
            Assign = 15,

            None = 20
        }

        static readonly Dictionary<ExpressionType, Tuple<string, Priority>> ops = new Dictionary<ExpressionType, Tuple<string, Priority>>
        {

            //binaries
            {ExpressionType.Add, Tuple.Create("+", Priority.Add)},
            {ExpressionType.AddChecked, Tuple.Create("+", Priority.Add)},
            {ExpressionType.And, Tuple.Create("&", Priority.Bool_And)},
            {ExpressionType.AndAlso, Tuple.Create("&&", Priority.Bool_And)},
            //{ExpressionType.ArrayIndex, Tuple.Create("[]", Priority.Primary)},
            {ExpressionType.Coalesce, Tuple.Create("??", Priority.Primary)}, // primary ?
            {ExpressionType.Divide, Tuple.Create("/", Priority.Mult)},
            {ExpressionType.Equal, Tuple.Create("==", Priority.Equal)},
            {ExpressionType.ExclusiveOr, Tuple.Create("^", Priority.Bit_Xor)},
            {ExpressionType.GreaterThan, Tuple.Create(">", Priority.Equal)},
            {ExpressionType.GreaterThanOrEqual, Tuple.Create(">=", Priority.Equal)},
            {ExpressionType.LeftShift, Tuple.Create("<<", Priority.Shift)},
            {ExpressionType.LessThan, Tuple.Create("<", Priority.Shift)},
            {ExpressionType.LessThanOrEqual, Tuple.Create("<=", Priority.Equal)},
            {ExpressionType.Modulo, Tuple.Create("%", Priority.Mult)},
            {ExpressionType.Multiply, Tuple.Create("*", Priority.Mult)},
            {ExpressionType.MultiplyChecked, Tuple.Create("*", Priority.Mult)},
            {ExpressionType.NotEqual, Tuple.Create("!=", Priority.Equal)},
            {ExpressionType.Or, Tuple.Create("|", Priority.Bit_Or)},
            {ExpressionType.OrElse, Tuple.Create("||", Priority.Bool_Or)},
            {ExpressionType.RightShift, Tuple.Create(">>", Priority.Shift)},
            {ExpressionType.Subtract, Tuple.Create("-", Priority.Add)},
            {ExpressionType.SubtractChecked, Tuple.Create("-", Priority.Add)},

            // unaries
            {ExpressionType.Negate, Tuple.Create("-",Priority.Unary)},
            {ExpressionType.NegateChecked, Tuple.Create("-",Priority.Unary)},
            {ExpressionType.Not, Tuple.Create("-",Priority.Unary)},
            {ExpressionType.Quote, Tuple.Create("",Priority.Unary)},
            {ExpressionType.UnaryPlus, Tuple.Create("",Priority.Unary)},
        };
        

        class OpExpression : Expression, IEvaluatesToString
        {
            readonly Priority _priority;
            readonly string _op;
            readonly IEvaluatesToString _left;
            readonly IEvaluatesToString _right;

            public OpExpression(Priority priority, string op, Expression left, Expression right=null)
            {
                _priority = priority;
                _op = op;
                _left = left as IEvaluatesToString;
                if (_left == null)
                    throw new InvalidExpressionInRpcCallException(left, "Unsupported expression");
                _right = right as IEvaluatesToString;
                if (_right == null && right !=null)
                    throw new InvalidExpressionInRpcCallException(right, "Unsupported expression");
            }

            public void AsString(StringBuilder builder)
            {
                if (_right == null)
                    builder.Append(_op);

                var asOp = _left as OpExpression;
                bool needsParenthesis = asOp != null && asOp._priority > _priority;
                if (needsParenthesis)
                    builder.Append("(");
                _left.AsString(builder);
                if (needsParenthesis)
                    builder.Append(")");

                if (_right != null)
                {
                    builder.Append(_op);
                    asOp = _right as OpExpression;
                    needsParenthesis = asOp != null && asOp._priority > _priority;
                    if (needsParenthesis)
                        builder.Append("(");
                    _right.AsString(builder);
                    if (needsParenthesis)
                        builder.Append(")");
                }
            }
        }
        Expression VisitOperator(Expression node, Expression left, Expression right=null)
        {
            Tuple<string, Priority> op;
            if(!ops.TryGetValue(node.NodeType,out op))
                throw new InvalidExpressionInRpcCallException(node, (right==null?"Unary":"Binary") + " operator not supported: " + node.NodeType);
            return right == null ? new OpExpression(op.Item2, op.Item1, Visit(left)) : new OpExpression(op.Item2, op.Item1, Visit(left), Visit(right));
        }

        class ArgumentsWrap : Expression, IEvaluatesToString
        {
            readonly IEvaluatesToString[] _args;

            public ArgumentsWrap(Expression[] args)
            {
                var invalid = args.FirstOrDefault(x => !(x is IEvaluatesToString));
                if (invalid != null)
                    throw new InvalidExpressionInRpcCallException(invalid, "Not supported expression");
                _args = args.Cast<IEvaluatesToString>().ToArray();
            }

            public void AsString(StringBuilder build)
            {
                build.Append("(");
                bool next = false;
                foreach (var arg in _args)
                {
                    if (next)
                        build.Append(",");
                    next = true;
                    arg.AsString(build);
                }
                build.Append(")");
            }
        }
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (!node.Method.IsStatic && typeof (IRpcPromise).IsAssignableFrom(node.Object.Type))
            {
                // promise evaluation ?
                if (node.Method.Name != "Execute")
                    throw new InvalidExpressionInRpcCallException(node, "Cannot execute method '" + node.Method.Name + "' on promise. Only 'Execute()' is alowed");

                var objAsConst = node.Object as ConstantExpression;
                if (objAsConst == null)
                    throw new InvalidExpressionInRpcCallException(node, "Cannot execute child RPC call, because it seems that it does not match the pattern 'myPromise.Execute()'");
                var promise = (IRpcPromise) objAsConst.Value;
                if (promise == null)
                    throw new InvalidExpressionInRpcCallException(node, "Cannot execute 'null' child RPC call");
                return Visit(promise.Expression);
            }

            var args = node.Arguments
                .Select(Visit)
                .ToArray();
            return new OpExpression(Priority.Primary, "."+node.Method.Name, Visit(node.Object), new ArgumentsWrap(args));
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            return new OpExpression(Priority.Primary, ".", node.Expression, new ConstEval(node.Member.Name));
        }

        public SerializedEvaluation Serialize(Expression simplified)
        {
            var visited = Visit(simplified);
            var asEval = visited as IEvaluatesToString;
            if (asEval == null)
                throw new InvalidExpressionInRpcCallException(simplified, "Expression not supported");
            var sb = new StringBuilder();
            asEval.AsString(sb);
            var serialized = sb.ToString();

            return new SerializedEvaluation
            {
                Evaluation = serialized,
                References = rpcRootsParameters.Select(x=>x.Reference).ToArray(),
            };
        }
    }
}
