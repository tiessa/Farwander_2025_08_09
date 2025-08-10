using NUnit.Framework;
using TJNK.Farwander.Core;

namespace TJNK.Farwander.Tests
{
    public class Scheduler_AdvanceFlooring
    {
        [Test]
        public void FloorsRealTimeToTicks_MinAdvanceOne()
        {
            var sch = new TimedScheduler();
            ulong before = sch.Now;
            sch.AdvanceTo(before); // no change
            Assert.AreEqual(before, sch.Now);
            sch.AdvanceTo(before + 1);
            Assert.AreEqual(before + 1, sch.Now);
        }
    }
}