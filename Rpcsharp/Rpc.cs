using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rpcsharp
{
    public static class RpcExtensions
    {
        public static async Task CallAsync(this IRpcServiceAsync service, Expression<Action> call)
        {
            var visitor = new RpcCallVisitor();
            var visited = visitor.Serialize(call.Body);
            var result = await service.InvokeRemoteAsync(visited);
            await RpcEvaluator.HandleResultAsync(result, service.ResolveReferenceAsync);
        }

        public static async Task<T> CallAsync<T>(this IRpcServiceAsync service, Expression<Func<T>> call)
        {
            var visitor = new RpcCallVisitor();
            var visited = visitor.Serialize(call.Body);
            var result = await service.InvokeRemoteAsync(visited);
            var handled = await RpcEvaluator.HandleResultAsync(result, service.ResolveReferenceAsync);
            return (T) handled;
        }
        public static void Call(this IRpcService service, Expression<Action> call)
        {
            var visitor = new RpcCallVisitor();
            var visited = visitor.Serialize(call.Body);
            var result = service.InvokeRemote(visited);
            RpcEvaluator.HandleResult(result,service.ResolveReference);
        }

        public static T Call<T>(this IRpcService service, Expression<Func<T>> call)
        {
            var visitor = new RpcCallVisitor();
            var visited = visitor.Serialize(call.Body);
            var result = service.InvokeRemote(visited);
            var handled = RpcEvaluator.HandleResult(result, service.ResolveReference);
            return (T) handled;
        }

    }
}
