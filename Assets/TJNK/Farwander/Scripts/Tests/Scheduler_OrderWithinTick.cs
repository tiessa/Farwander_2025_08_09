using NUnit.Framework;
using TJNK.Farwander.Core;
using System.Collections.Generic;

namespace TJNK.Farwander.Tests
{
    public class Scheduler_OrderWithinTick
    {
        [Test]
        public void SameTick_Ordering_PriorityThenSequence()
        {
            var sch = new TimedScheduler();
            int seq=0; var log = new List<string>();
            System.Action<string> Mark=(s)=>{ log.Add(s+(seq++).ToString()); };

            sch.Schedule(5, EventPriority.Actor, EventLane.Action, null, ()=>Mark("A"));
            sch.Schedule(5, EventPriority.System, EventLane.Action, null, ()=>Mark("S"));
            sch.Schedule(5, EventPriority.UI, EventLane.Action, null, ()=>Mark("U"));
            sch.Schedule(5, EventPriority.World, EventLane.Action, null, ()=>Mark("W"));

            sch.AdvanceTo(10);
            Assert.AreEqual(4, log.Count);
            Assert.AreEqual("S0", log[0]);
            Assert.AreEqual("W1", log[1]);
            Assert.AreEqual("A2", log[2]);
            Assert.AreEqual("U3", log[3]);
        }
    }
}