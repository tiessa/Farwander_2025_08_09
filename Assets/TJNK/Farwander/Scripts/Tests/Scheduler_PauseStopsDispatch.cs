using NUnit.Framework;
using TJNK.Farwander.Core;

namespace TJNK.Farwander.Tests
{
    public class Scheduler_PauseStopsDispatch
    {
        [Test]
        public void Pause_HaltsAdvance_ResumeProcesses()
        {
            var sch = new TimedScheduler();
            int ran=0; var owner = new object();
            sch.Schedule(1, EventPriority.System, EventLane.Dispatch, owner, ()=> ran++);
            sch.Pause(true);
            sch.AdvanceTo(10);
            Assert.AreEqual(0, ran);
            sch.Pause(false);
            sch.AdvanceTo(10);
            Assert.AreEqual(1, ran);
        }
    }
}