using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Rpcsharp;
using Rpcsharp.Private;

namespace RpcTests
{
    [TestFixture]
    public class InterfaceImplementerTests
    {
        public interface ITests : IRpcRoot, IEquatable<ITests>
        {
            string PublicSet { get; set; }
            string PrivateSet { get; }
            string PrivateGet { set; }

            void VoidMethod();

            string NonVoidMethod();
        }

        [Test]
        public void JustCreate()
        {
            InterfaceImplementer.Create<ITests>();
        }

        [Test]
        public void Set()
        {
            var x = InterfaceImplementer.Create<ITests>();
            x.PublicSet = "test";
            Assert.AreEqual("test", x.PublicSet);
        }

        [Test, ExpectedException(typeof(CannotCallRemoteMethodException))]
        public void CallVoid()
        {
            var x = InterfaceImplementer.Create<ITests>();
            x.NonVoidMethod();
        }

        [Test, ExpectedException(typeof(CannotCallRemoteMethodException))]
        public void CallNonVoid()
        {
            var x = InterfaceImplementer.Create<ITests>();
            x.VoidMethod();
        }

        [Test]
        public void Equatable()
        {
            var x = InterfaceImplementer.Create<ITests>();
            var xbis = InterfaceImplementer.Create<ITests>();
            var y = InterfaceImplementer.Create<ITests>();
            ((IReferenceSetter) x).SetReference("x");
            ((IReferenceSetter) xbis).SetReference("x");
            ((IReferenceSetter) y).SetReference("y");

            // check default compararer (used by dictionaries, and co)
            Assert.IsTrue(EqualityComparer<ITests>.Default.Equals(x, xbis));
            Assert.IsFalse(EqualityComparer<ITests>.Default.Equals(x, y));

            // check 'equals' (ITest implements IEquatable<ITest>)
            Assert.True(x.Equals(xbis));
            Assert.True(xbis.Equals(x));
            Assert.True(!x.Equals(y));
            Assert.True(!y.Equals(x));

            // check 'object.Equals' override
            Assert.True(((object)x).Equals(xbis));
            Assert.False(((object)x).Equals(y));

            Assert.False(((object)x).Equals(null));
            Assert.False(((object)x).Equals(new object()));
        }
        
    }
}
