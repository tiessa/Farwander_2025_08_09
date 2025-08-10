using NUnit.Framework;
using TJNK.Farwander.Core;

namespace TJNK.Farwander.Tests
{
    public class Scheduler_PerOwnerSameTickCap
    {
        [Test]
        public void DispatchBeyondCap_Dropped()
        {
            var sch = new TimedScheduler();
            var owner = new object();
            int ran=0;
            for (int i=0;i<10;i++)
            {
                var id = sch.Schedule(0, EventPriority.System, EventLane.Dispatch, owner, ()=> ran++);
                if (i >= TimedScheduler.SameTickDispatchCapPerOwner)
                {
                    Assert.AreEqual(0, id); // dropped
                }
                else Assert.AreNotEqual(0, id);
            }
            sch.AdvanceTo(0);
            Assert.AreEqual(TimedScheduler.SameTickDispatchCapPerOwner, ran);
        }
    }
}