using System;
using System.Collections.Generic;

namespace TJNK.Farwander.Core
{
    public interface IValidator<T>
    {
        bool Validate(T context, out string reason);
    }

    public sealed class ValidationResult
    {
        public bool IsValid;
        public string Reason;
        public object FailedValidator;
    }

    /// <summary>Registration + audit-only validation pipeline.</summary>
    public sealed class ValidationPipeline
    {
        private readonly Dictionary<Type, List<object>> _validators = new Dictionary<Type, List<object>>();
        private readonly List<string> _auditLog = new List<string>();
        public IList<string> AuditLog { get { return _auditLog.AsReadOnly(); } }

        public void Register<T>(IValidator<T> validator)
        {
            if (validator == null) throw new ArgumentNullException("validator");
            var key = typeof(T);
            List<object> list; if (!_validators.TryGetValue(key, out list)) { list = new List<object>(); _validators[key] = list; }
            list.Add(validator);
        }

        public ValidationResult Audit<T>(T context)
        {
            var res = new ValidationResult { IsValid = true };
            var key = typeof(T);
            List<object> list; if (!_validators.TryGetValue(key, out list)) return res;
            foreach (var vObj in list)
            {
                var v = (IValidator<T>)vObj;
                string reason; if (!v.Validate(context, out reason)) { res.IsValid = false; res.Reason = reason; res.FailedValidator = v; _auditLog.Add("FAIL: " + v.GetType().Name + ": " + reason); return res; }
            }
            _auditLog.Add("PASS: " + typeof(T).Name);
            return res;
        }
    }
}