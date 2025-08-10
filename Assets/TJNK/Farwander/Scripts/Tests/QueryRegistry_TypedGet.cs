using NUnit.Framework;
using TJNK.Farwander.Core;

namespace TJNK.Farwander.Tests
{
    public class QueryRegistry_TypedGet
    {
        [Test]
        public void ReturnsSameInstance_WhenFactoryDoes()
        {
            var q = new QueryRegistry();
            var shared = new object();
            q.Register(() => shared);
            Assert.AreSame(shared, q.Get<object>());
            Assert.AreSame(shared, q.Get<object>());
        }

        [Test]
        public void ReturnsNull_WhenMissing()
        {
            var q = new QueryRegistry();
            Assert.IsNull(q.Get<string>());
        }
    }
}