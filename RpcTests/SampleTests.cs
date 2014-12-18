using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client;
using Interfaces;
using NUnit.Framework;

namespace RpcTests
{
    [TestFixture]
    public class SampleTests
    {
        [Test]
        public async Task MemorySource()
        {
            var vm = new DirectoryPresenter();
            await vm.SetServiceAsync(new MemoryService());
            Assert.AreEqual(2,vm.Nodes.Count);
            var dir = vm.Nodes.OfType<DirectoryPresenter>().Single();
            var file = vm.Nodes.OfType<IFile>().Single();
            Assert.AreEqual(dir.Name,"Infinite");
            Assert.AreEqual(file.Name,"file.txt");
        }
    }
}
