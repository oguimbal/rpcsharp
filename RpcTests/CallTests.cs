using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Rpcsharp;
using RpcTests.Model;

namespace RpcTests
{
    [TestFixture]
    public class CallTests
    {
        [Test]
        public async Task SimpleOnServiceInterface()
        {
            var r = new ClassRootClient();
            IRpcServiceAsync service = new FakeService();
            await service.CallAsync(() => r.Simple());
        }

        [Test]
        public async Task SimpleOnClass()
        {
            ClassRootClient r = new ClassRootClient();
            var service = new FakeService();
            await service.CallAsync(() => r.Simple());
        }

        [Test]
        public async Task SimpleOnInterface()
        {
            IInterfaceRoot r = new InterfaceRootClientImpl();
            var service = new FakeService();
            await service.CallAsync(() => r.SimpleInterface());
        }

        [Test]
        public async Task SimpleOnInterfaceImplementation()
        {
            InterfaceRootClientImpl r = new InterfaceRootClientImpl();
            var service = new FakeService();
            await service.CallAsync(() => r.SimpleInterface());
        }


        [Test]
        public async Task Add()
        {
            IInterfaceRoot r = new InterfaceRootClientImpl();
            var service = new FakeService();
            var result = await service.CallAsync(() => r.Add(1,1.5));
            Assert.AreEqual(3.5, result);
            service = new FakeService();
            var result2 = await service.CallAsync(() => r.Add(1,2));
            Assert.AreEqual(3, result2);
        }

        [Test]
        public async Task Compute()
        {
            IInterfaceRoot r = new InterfaceRootClientImpl();
            var service = new FakeService();
            var result = await service.CallAsync(() => r.Compute(1,2));
            Assert.AreEqual("1+2=3", result);
        }

        [Test]
        public async Task Compose()
        {
            IInterfaceRoot r = new InterfaceRootClientImpl();
            var service = new FakeService();
            var result = await service.CallAsync(() => r.Compute(r.Add(1,1+1),3));
            Assert.AreEqual("3+3=6", result);
        }

        [Test]
        public async Task BinaryTests()
        {
            IInterfaceRoot r = new InterfaceRootClientImpl();
            var service = new FakeService();
            var result = await service.CallAsync(() => (r.Add(1, 1) + r.Add(1, 1)) * r.Add(2, 2) / r.Add(2,0));
            Assert.AreEqual(8, result);
        }

    }

}
