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
        readonly bool _continueOnCapturedContext = true;
        Expression IRpcPromise.Expression { get { return _call; } }

        internal RpcPromise(IRpcService service, Expression<Action> call)
        {
            _service = service;
            _call = ExpressionSimplifier.Simplify(call.Body);
        }

        RpcPromise(IRpcService service, Expression call, bool continueOnCapturedContext)
        {
            _service = service;
            _call = call;
            _continueOnCapturedContext = continueOnCapturedContext;
        }

        internal RpcPromise(IRpcServiceAsync service, Expression<Action> call)
        {
            _serviceAsync = service;
            _call = ExpressionSimplifier.Simplify(call.Body);
        }

        RpcPromise(IRpcServiceAsync service, Expression call, bool continueOnCapturedContext)
        {
            _serviceAsync = service;
            _call = call;
            _continueOnCapturedContext = continueOnCapturedContext;
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
                .ConfigureAwait(_continueOnCapturedContext)
                .GetAwaiter()
                .GetResult();
        }


        public ConfiguredTaskAwaitable.ConfiguredTaskAwaiter GetAwaiter()
        {
            if (_service != null)
            {
                return Task.Run((Action)InternalExecute)
                    .ConfigureAwait(_continueOnCapturedContext)
                    .GetAwaiter();
            }

            return InternalExecuteAsync()
                .ConfigureAwait(_continueOnCapturedContext)
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
                .ConfigureAwait(_continueOnCapturedContext);
            await RpcEvaluator
                .HandleResultAsync(result, _serviceAsync.ResolveReferenceAsync)
                .ConfigureAwait(_continueOnCapturedContext);
        }

        public RpcPromise ConfigureAwait(bool continueOnCapturedContext)
        {
            if (_service != null)
                return new RpcPromise(_service, _call, continueOnCapturedContext);
            return new RpcPromise(_serviceAsync, _call, continueOnCapturedContext);
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
        readonly bool _continueOnCapturedContext = true;

        Expression IRpcPromise.Expression { get { return _call; } }

        internal RpcPromise(IRpcService service, Expression<Func<T>> call)
        {
            _service = service;
            _call = ExpressionSimplifier.Simplify(call.Body);
        }
        RpcPromise(IRpcService service, Expression call, bool continueOnCapturedContext)
        {
            _service = service;
            _call = call;
            _continueOnCapturedContext = continueOnCapturedContext;
        }

        internal RpcPromise(IRpcServiceAsync service, Expression<Func<T>> call)
        {
            _serviceAsync = service;
            _call = ExpressionSimplifier.Simplify(call.Body);
        }

        RpcPromise(IRpcServiceAsync service, Expression call, bool continueOnCapturedContext)
        {
            _serviceAsync = service;
            _call = call;
            _continueOnCapturedContext = continueOnCapturedContext;
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
                .ConfigureAwait(_continueOnCapturedContext)
                .GetAwaiter()
                .GetResult();
        }

        public ConfiguredTaskAwaitable<T>.ConfiguredTaskAwaiter GetAwaiter()
        {
            if (_service != null)
            {
                return Task.Run((Func<T>)InternalExecute)
                    .ConfigureAwait(_continueOnCapturedContext)
                    .GetAwaiter();
            }

            return InternalExecuteAsync()
                .ConfigureAwait(_continueOnCapturedContext)
                .GetAwaiter();
        }


        T InternalExecute()
        {
            var visitor = new RpcCallVisitor();
            var visited = visitor.Serialize(_call);
            var result = _service.InvokeRemote(visited);

            // todo: This is an horrible hack to allow rpc roots arrays to be returned... 
            if (typeof(IRpcRoot[]).IsAssignableFrom(typeof(T)) && result != null)
            {
                var elt = typeof (T).GetElementType();
                // todo: what happens on a null array  ?
                var array = (IRpcRoot[]) Array.CreateInstance(elt, result.References.Length);
                for(int i=0;i< result.References.Length;i++)
                {
                    array[i] = _service.ResolveReference(result.References[i]);
                }
                return (T) (object) array;
            }

            var handled = RpcEvaluator.HandleResult(result, _service.ResolveReference);
            return (T)handled;
        }

        async Task<T> InternalExecuteAsync()
        {
            var visitor = new RpcCallVisitor();
            var visited = visitor.Serialize(_call);
            var result = await _serviceAsync
                .InvokeRemoteAsync(visited)
                .ConfigureAwait(_continueOnCapturedContext);

            // todo: This is an horrible hack to allow rpc roots arrays to be returned... 
            if (typeof(IRpcRoot[]).IsAssignableFrom(typeof(T)) && result != null)
            {
                var elt = typeof(T).GetElementType();
                // todo: what happens on a null array  ?
                var array = (IRpcRoot[])Array.CreateInstance(elt, result.References.Length);
                for (int i = 0; i < result.References.Length; i++)
                {
                    array[i] = await _serviceAsync.ResolveReferenceAsync(result.References[i])
                                                    .ConfigureAwait(_continueOnCapturedContext);
                }
                return (T)(object)array;
            }


            var handled = await RpcEvaluator
                .HandleResultAsync(result, _serviceAsync.ResolveReferenceAsync)
                .ConfigureAwait(_continueOnCapturedContext);

            return (T)handled;
        }

        public RpcPromise<T> ConfigureAwait(bool continueOnCapturedContext)
        {
            if (_service != null)
                return new RpcPromise<T>(_service, _call, continueOnCapturedContext);
            return new RpcPromise<T>(_serviceAsync, _call, continueOnCapturedContext);
        }
    }
}