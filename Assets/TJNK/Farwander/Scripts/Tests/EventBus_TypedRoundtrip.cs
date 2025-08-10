using NUnit.Framework;
using TJNK.Farwander.Core;

namespace TJNK.Farwander.Tests
{
    public class EventBus_TypedRoundtrip
    {
        private struct Foo { public readonly int X; public Foo(int x){X=x;} }

        [Test]
        public void PublishSubscribe_Unsubscribe_Works()
        {
            var bus = new EventBus();
            int a=0,b=0;
            var d1 = bus.Subscribe<Foo>(f=> a+=f.X);
            var d2 = bus.Subscribe<Foo>(f=> b+=f.X*2);

            Assert.AreEqual(2, bus.Publish(new Foo(3)));
            Assert.AreEqual(3, a); Assert.AreEqual(6, b);

            d2.Dispose();
            Assert.AreEqual(1, bus.Publish(new Foo(1)));
            Assert.AreEqual(4, a); Assert.AreEqual(6, b);
        }
    }
}