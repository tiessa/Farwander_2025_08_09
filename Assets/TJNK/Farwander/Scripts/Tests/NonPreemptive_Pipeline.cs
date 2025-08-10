using NUnit.Framework;
using TJNK.Farwander.Core;
using System.Text;

namespace TJNK.Farwander.Tests
{
    public class NonPreemptive_Pipeline
    {
        [Test]
        public void DispatchChain_Completes_BeforeNextTopLevel()
        {
            var sch = new TimedScheduler();
            var log = new StringBuilder();

            sch.Schedule(1, EventPriority.System, EventLane.Action, "A", ()=>
            {
                log.Append("A");
                sch.Schedule(1, EventPriority.System, EventLane.Dispatch, "A", ()=>{ log.Append("d1"); });
                sch.Schedule(1, EventPriority.System, EventLane.Dispatch, "A", ()=>{ log.Append("d2"); });
                sch.Schedule(1, EventPriority.System, EventLane.Dispatch, "A", ()=>{ log.Append("d3"); });
            });

            sch.Schedule(1, EventPriority.System, EventLane.Action, "B", ()=> { log.Append("B"); });

            sch.AdvanceTo(1);
            Assert.AreEqual("Ad1d2d3B", log.ToString());
        }
    }
}