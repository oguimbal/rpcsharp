using Rpcsharp.Proxying;
using Rpcsharp.Proxying.Private;

namespace Rpcsharp
{
    public static class Proxy
    {
        public static T Stub<T>(string reference)
            where T:IRpcRoot
        {
            var instance = InterfaceImplementer.Create<T>();
            ((IReferenceSetter)instance).SetReference(reference);
            return instance;
        }
    }
}
