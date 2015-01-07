using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Rpcsharp.Proxying;

namespace Rpcsharp
{
    public static class Proxy
    {
        public static T Stub<T>(string reference)
            where T:IRpcRoot
        {
            var instance = InterfaceImplementer.Create<T>();
            ((IProxy)instance).SetReference(reference);
            return instance;
        }

        public static TProxy Set<TProxy, TValue>(this TProxy proxy, Expression<Func<TProxy, TValue>> property, TValue value)
        {
            // get property to assign
            var propExpression = property.Body.Unwrap() as MemberExpression;
            if (propExpression == null || !(propExpression.Member is PropertyInfo))
                throw new ArgumentException("Expecting a property");
            var prop = propExpression.Member as PropertyInfo;

            // check that given proxy is really a proxy
            var p = proxy as IProxy;
            if (p == null)
                throw new NullReferenceException("Cannot set property " + prop.Name + ". Given object is not a proxy.");

            // set the property
            SetCache<TProxy, TValue>.Setter(prop)(proxy, value);

            return proxy;
        }

        static class SetCache<TProxy,TValue>
        {
            static readonly Dictionary<PropertyInfo, Action<TProxy, TValue>> Setters = new Dictionary<PropertyInfo, Action<TProxy, TValue>>();

            public static Action<TProxy, TValue> Setter(PropertyInfo prop)
            {
                // use cache
                Action<TProxy, TValue> ret;
                lock (Setters)
                {
                    if (Setters.TryGetValue(prop, out ret))
                        return ret;
                }

                // get the concreete property associated with the given interface property
                var proxyType = InterfaceImplementer.Implement<TProxy>();
                var map = proxyType.GetInterfaceMap(typeof (TProxy));
                var i = Array.IndexOf(map.InterfaceMethods, prop.GetGetMethod(true));
                if (i < 0)
                    throw new ArgumentException("Property " + prop + " is not part of interface " + typeof (TProxy));
                var targetGetter = map.TargetMethods[i];
                var proxyProp = proxyType.GetProperties().First(x => x.GetGetMethod(true) == targetGetter);

                // create an assignment expression
                var p = Expression.Parameter(typeof(TProxy));
                var v = Expression.Parameter(typeof(TValue));
                ret = Expression.Lambda<Action<TProxy, TValue>>(
                    Expression.Assign(Expression.MakeMemberAccess(Expression.Convert(p, proxyType), proxyProp), v)
                    , p
                    , v)
                    .Compile();

                // cache result
                lock (Setters)
                {
                    Setters[prop] = ret;
                    return ret;
                }
            }
        }
    }
}
