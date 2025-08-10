using NUnit.Framework;
using TJNK.Farwander.Core;

namespace TJNK.Farwander.Tests
{
    public class ValidationPipeline_Audit
    {
        private sealed class AlwaysFail : IValidator<int>
        {
            public bool Validate(int context, out string reason) { reason = "nope"; return false; }
        }
        private sealed class AlwaysPass : IValidator<int>
        {
            public bool Validate(int context, out string reason) { reason = null; return true; }
        }

        [Test]
        public void FirstFail_WinsAndAudits()
        {
            var v = new ValidationPipeline();
            v.Register<int>(new AlwaysPass());
            v.Register<int>(new AlwaysFail());
            v.Register<int>(new AlwaysPass());

            var res = v.Audit(42);
            Assert.IsFalse(res.IsValid);
            Assert.AreEqual("nope", res.Reason);
            Assert.IsNotNull(res.FailedValidator);
        }
    }
}