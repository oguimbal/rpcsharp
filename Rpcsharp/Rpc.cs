using System;
using System.Linq.Expressions;

namespace Rpcsharp
{
    public static class RpcExtensions
    {
        /// <summary>
        /// Builds a reusable and awaitable RPC
        /// </summary>
        /// <remarks>
        /// This server call is not started until awaited.
        /// Unlike tasks, awaiting it multiple times will trigger the same server action multiple times.
        /// It can also be reused in children RPCs definitions as a "sub-procedure"
        /// </remarks>
        /// <returns>An awaitable object that represents this remote call</returns>
        public static RpcPromise CallAsync(this IRpcServiceAsync service, Expression<Action> call)
        {
            return new RpcPromise(service, call);
        }

        /// <summary>
        /// Builds a reusable and awaitable RPC that returns an object
        /// </summary>
        /// <remarks>
        /// This server call is not started until awaited.
        /// Unlike tasks, awaiting it multiple times will trigger the same server action multiple times.
        /// It can also be reused in children RPCs definitions as a "sub-procedure"
        /// </remarks>
        /// <returns>An awaitable object that represents this remote call</returns>
        public static RpcPromise<T> CallAsync<T>(this IRpcServiceAsync service, Expression<Func<T>> call)
        {
            return new RpcPromise<T>(service, call);
        }


        /// <summary>
        /// Build a synchronous RPC
        /// </summary>
        /// <remarks>
        /// This server call is started immediatly, and will return when operation is finished.
        /// To reuse operations a synchronous call in other procedures, see CallPromise() function.
        /// </remarks>
        public static void Call(this IRpcService service, Expression<Action> call)
        {
            new RpcPromise(service, call).Execute();
        }

        /// <summary>
        /// Build a synchronous RPC that returns an object
        /// </summary>
        /// <remarks>
        /// This server call is started immediatly, and will return when operation is finished.
        /// To reuse operations a synchronous call in other procedures, see CallPromise() function.
        /// </remarks>
        public static T Call<T>(this IRpcService service, Expression<Func<T>> call)
        {
            return new RpcPromise<T>(service, call).Execute();
        }

        /// <summary>
        /// Builds a reusable and awaitable RPC
        /// </summary>
        /// <remarks>
        /// This server call is not started until awaited.
        /// Unlike tasks, awaiting it multiple times will trigger the same server action multiple times.
        /// It can also be reused in children RPCs definitions as a "sub-procedure"
        /// </remarks>
        public static RpcPromise CallPromise(this IRpcService service, Expression<Action> call)
        {
            return new RpcPromise(service, call);
        }

        /// <summary>
        /// Builds a reusable and awaitable RPC that returns an object
        /// </summary>
        /// <remarks>
        /// This server call is not started until awaited.
        /// Unlike tasks, awaiting it multiple times will trigger the same server action multiple times.
        /// It can also be reused in children RPCs definitions as a "sub-procedure"
        /// </remarks>
        public static RpcPromise<T> CallPromise<T>(this IRpcService service, Expression<Func<T>> call)
        {
            return new RpcPromise<T>(service, call);
        }
    }
}
