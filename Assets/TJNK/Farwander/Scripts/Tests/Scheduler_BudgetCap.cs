using NUnit.Framework;
using TJNK.Farwander.Core;

namespace TJNK.Farwander.Tests
{
    public class Scheduler_BudgetCap
    {
        [Test]
        public void ProcessesUpToBudget_RemainderStaysQueued()
        {
            var sch = new TimedScheduler();
            int ran=0; var owner = new object();
            for (int i=0;i<300;i++) sch.Schedule(1, EventPriority.System, EventLane.Dispatch, owner, ()=> ran++);
            sch.AdvanceTo(1);
            Assert.AreEqual(TimedScheduler.DispatchBudgetPerCycle, ran);
            sch.AdvanceTo(1);
            Assert.Greater(ran, TimedScheduler.DispatchBudgetPerCycle);
        }
    }
}