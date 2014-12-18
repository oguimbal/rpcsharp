using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Rpcsharp.Parser;

namespace Rpcsharp
{
    interface IRpcPromise
    {
        Expression Expression { get; }
    }

    /// <summary>
    /// Represents the promise of server-side computation
    /// </summary>
    /// <remarks>
    /// This object is awaitable
    /// </remarks>
    public class RpcPromise :
        IRpcPromise
    {
        readonly IRpcService _service;
        readonly IRpcServiceAsync _serviceAsync;
        readonly Expression _call;
        Expression IRpcPromise.Expression { get { return _call; } }

        internal RpcPromise(IRpcService service, Expression<Action> call)
        {
            _service = service;
            _call = ExpressionSimplifier.Simplify(call.Body);
        }
        internal RpcPromise(IRpcServiceAsync service, Expression<Action> call)
        {
            _serviceAsync = service;
            _call = ExpressionSimplifier.Simplify(call.Body);
        }

        /// <summary>
        /// Executes this RPC promise synchronously. Tiggering it multiple times will execute it multiple times.
        /// </summary>
        public void Execute()
        {
            if (_service != null)
            {
                InternalExecute();
                return;
            }

            InternalExecuteAsync()
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }


        public ConfiguredTaskAwaitable.ConfiguredTaskAwaiter GetAwaiter()
        {
            if (_service != null)
            {
                return Task.Run((Action)InternalExecute)
                    .ConfigureAwait(false)
                    .GetAwaiter();
            }

            return InternalExecuteAsync()
                .ConfigureAwait(false)
                .GetAwaiter();
        }

        void InternalExecute()
        {
            var visitor = new RpcCallVisitor();
            var visited = visitor.Serialize(_call);
            var result = _service.InvokeRemote(visited);
            RpcEvaluator.HandleResult(result, _service.ResolveReference);
        }

        async Task InternalExecuteAsync()
        {
            var visitor = new RpcCallVisitor();
            var visited = visitor.Serialize(_call);
            var result = await _serviceAsync
                .InvokeRemoteAsync(visited)
                .ConfigureAwait(false);
            await RpcEvaluator
                .HandleResultAsync(result, _serviceAsync.ResolveReferenceAsync)
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Represents the promise of server-side computation, with a result
    /// </summary>
    /// <remarks>
    /// This object is awaitable
    /// </remarks>
    public class RpcPromise<T> : IRpcPromise
    {
        readonly IRpcService _service;
        readonly IRpcServiceAsync _serviceAsync;
        readonly Expression _call;
        Expression IRpcPromise.Expression { get { return _call; } }

        internal RpcPromise(IRpcService service, Expression<Func<T>> call)
        {
            _service = service;
            _call = ExpressionSimplifier.Simplify(call.Body);
        }

        internal RpcPromise(IRpcServiceAsync service, Expression<Func<T>> call)
        {
            _serviceAsync = service;
            _call = ExpressionSimplifier.Simplify(call.Body);
        }

        /// <summary>
        /// Executes this RPC promise synchronously. Tiggering it multiple times will execute it multiple times.
        /// </summary>
        public T Execute()
        {
            if (_service != null)
            {
                return InternalExecute();
            }

            return InternalExecuteAsync()
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public ConfiguredTaskAwaitable<T>.ConfiguredTaskAwaiter GetAwaiter()
        {
            if (_service != null)
            {
                return Task.Run((Func<T>)InternalExecute)
                    .ConfigureAwait(false)
                    .GetAwaiter();
            }

            return InternalExecuteAsync()
                .ConfigureAwait(false)
                .GetAwaiter();
        }


        T InternalExecute()
        {
            var visitor = new RpcCallVisitor();
            var visited = visitor.Serialize(_call);
            var result = _service.InvokeRemote(visited);
            var handled = RpcEvaluator.HandleResult(result, _service.ResolveReference);
            return (T)handled;
        }

        async Task<T> InternalExecuteAsync()
        {
            var visitor = new RpcCallVisitor();
            var visited = visitor.Serialize(_call);
            var result = await _serviceAsync
                .InvokeRemoteAsync(visited)
                .ConfigureAwait(false);
            var handled = await RpcEvaluator
                .HandleResultAsync(result, _serviceAsync.ResolveReferenceAsync)
                .ConfigureAwait(false);
            return (T)handled;
        }
    }
}