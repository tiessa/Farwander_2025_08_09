using NUnit.Framework;
using TJNK.Farwander.Core;

namespace TJNK.Farwander.Tests
{
    public class Scheduler_CancelOwned
    {
        [Test]
        public void CancelsAllFuture_ForOwner()
        {
            var sch = new TimedScheduler();
            int ran=0; var ownerA = new object(); var ownerB=new object();
            sch.Schedule(1, EventPriority.System, EventLane.Action, ownerA, ()=> ran++);
            sch.Schedule(1, EventPriority.System, EventLane.Action, ownerB, ()=> ran++);
            sch.CancelOwned(ownerA);
            sch.AdvanceTo(10);
            Assert.AreEqual(1, ran);
        }
    }
}