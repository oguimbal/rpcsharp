using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Rpcsharp;

namespace RpcTests.RemoteCall
{
    [TestFixture]
    public class PromiseTests
    {
        [Test]
        public async Task MultipleAwaits()
        {
            var r = new ClassRootClient();
            var service = new FakeService(2);
            var p = service.CallAsync(() => r.Simple());
            await p;
            await p;
            Assert.AreEqual(2, service.CallCount);
        }

        [Test]
        public void NoAwait()
        {
            var r = new ClassRootClient();
            var service = new FakeService(0);
            service.CallAsync(() => r.Simple());
            Thread.Sleep(50);
            Assert.AreEqual(0, service.CallCount);
        }

        [Test]
        public async Task ReusePromise()
        {
            IInterfaceRoot r = new InterfaceRootClientImpl();
            var service = new FakeService();
            var p = service.CallAsync(() => r.Add(1,1));
            var result = await service.CallAsync(() => r.Add(p.Execute(), 1));
            Assert.AreEqual(1, service.CallCount);
            Assert.AreEqual(3, result);
        }
    }

}
