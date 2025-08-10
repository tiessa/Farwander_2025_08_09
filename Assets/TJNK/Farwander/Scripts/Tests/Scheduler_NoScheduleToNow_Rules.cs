using NUnit.Framework;
using TJNK.Farwander.Core;

namespace TJNK.Farwander.Tests
{
    public class Scheduler_NoScheduleToNow_Rules
    {
        [Test]
        public void TickAndAction_Disallow_Now()
        {
            var sch = new TimedScheduler();
            Assert.Throws<System.InvalidOperationException>(()=> sch.Schedule(0, EventPriority.System, EventLane.Tick, null, ()=>{}));
            Assert.Throws<System.InvalidOperationException>(()=> sch.Schedule(0, EventPriority.System, EventLane.Action, null, ()=>{}));
        }

        [Test]
        public void Dispatch_Allows_Now()
        {
            var sch = new TimedScheduler();
            var id = sch.Schedule(0, EventPriority.System, EventLane.Dispatch, new object(), ()=>{});
            Assert.AreNotEqual(0, id); // not dropped (cap not hit)
        }
    }
}