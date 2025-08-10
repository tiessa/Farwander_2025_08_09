using NUnit.Framework;
using TJNK.Farwander.Core;
using System.Collections.Generic;

namespace TJNK.Farwander.Tests
{
    public class Scheduler_CancelAndMarkDead
    {
        [Test]
        public void CancelledItems_AreSkipped_NoSurgery()
        {
            var sch = new TimedScheduler();
            int ran=0; var ids = new System.Collections.Generic.List<long>();
            for (int i=0;i<5;i++) ids.Add(sch.Schedule(1, EventPriority.System, EventLane.Dispatch, null, ()=> ran++));
            sch.Cancel(ids[1]); sch.Cancel(ids[3]);
            sch.AdvanceTo(1);
            Assert.AreEqual(3, ran);
        }
    }
}