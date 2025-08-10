using System;
using System.Collections.Generic;

namespace TJNK.Farwander.Core
{
    /// <summary>Simple typed event bus with Subscribe/Publish and IDisposable unsubscription.</summary>
    public sealed class EventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _subs = new Dictionary<Type, List<Delegate>>();

        public IDisposable Subscribe<T>(Action<T> handler)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            var t = typeof(T);
            List<Delegate> list;
            if (!_subs.TryGetValue(t, out list))
            {
                list = new List<Delegate>();
                _subs[t] = list;
            }
            list.Add(handler);
            return new Unsub(() => list.Remove(handler));
        }

        public int Publish<T>(T evt)
        {
            var t = typeof(T);
            List<Delegate> list;
            if (!_subs.TryGetValue(t, out list) || list.Count == 0) return 0;
            var snapshot = list.ToArray();
            foreach (var d in snapshot)
            {
                var a = d as Action<T>; if (a != null) a(evt);
            }
            return snapshot.Length;
        }

        private sealed class Unsub : IDisposable
        {
            private Action _dispose; public Unsub(Action d) { _dispose = d; }
            public void Dispose() { if (_dispose != null) { _dispose(); _dispose = null; } }
        }
    }
}