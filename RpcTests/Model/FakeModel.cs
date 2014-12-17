using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Rpcsharp;

namespace RpcTests.Model
{

    public interface IFakeRoot : IRpcRoot
    {
        [Rpc]
        void Simple();
    }

    class FakeService : IRpcServiceAsync
    {
        bool alreadyCalled = false;
        public Task<SerializedEvaluation> InvokeRemoteAsync(SerializedEvaluation visited)
        {
            Assert.IsFalse(alreadyCalled, "Called twice");
            alreadyCalled = true;
            var ret = RpcEvaluator.HandleIncomingRequest(visited, r =>
            {
                if (r == "ClassRootRef")
                    return new ClassRootServer();
                return new InterfaceRootServerImpl();
            });

            return Task.FromResult(ret);
        }

        public Task<IRpcRoot> ResolveReferenceAsync(string reference)
        {
            throw new NotImplementedException();
        }
    }

    public class ClassRootClient : IRpcRoot
    {
        [Rpc]
        public void Simple()
        {
            Assert.Fail("Unexpected client side call");
        }

        public string GetReference()
        {
            return "ClassRootRef";
        }
    }
    public class ClassRootServer : IRpcRoot
    {
        [Rpc]
        public void Simple()
        {
        }

        public string GetReference()
        {
            Assert.Fail("Unexpected client server side call");
            return null;
        }
    }

    public interface IInterfaceRoot : IRpcRoot
    {
        [Rpc]
        void SimpleInterface();
        [Rpc]
        string Compute(int a, int b);
        [Rpc]
        double Add(int a, double b);

        [Rpc]
        int Add(int a, int b);
    }

    class InterfaceRootClientImpl : IInterfaceRoot
    {
        // intentionally forgot the [Rpc] attribute...
        public void SimpleInterface()
        {
            Assert.Fail("Unexpected client side call");
        }

        public string Compute(int a, int b)
        {
            Assert.Fail("Unexpected client side call");
            return null;
        }

        public double Add(int a, double b)
        {
            Assert.Fail("Unexpected client side call");
            return 0;
        }

        public int Add(int a, int b)
        {
            Assert.Fail("Unexpected client side call");
            return 0;
        }

        public string GetReference()
        {
            return "InterfaceRootImplRef";
        }
    }

    class InterfaceRootServerImpl : IInterfaceRoot
    {
        // intentionally forgot the [Rpc] attribute...
        public void SimpleInterface()
        {
        }

        public string Compute(int a, int b)
        {
            return a + "+" + b + "=" + (a + b);
        }

        public double Add(int a, double b)
        {
            return a + b + 1;
        }

        public int Add(int a, int b)
        {
            return a + b ;
        }

        public string GetReference()
        {
            Assert.Fail("Unexpected client server side call");
            return null;
        }
    }
}
