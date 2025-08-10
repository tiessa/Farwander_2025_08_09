using System;
using System.Collections.Generic;

namespace TJNK.Farwander.Core
{
    /// <summary>Binary-heap scheduler with lanes, priorities, and guardrails.</summary>
    public sealed class TimedScheduler
    {
        public const int SameTickDispatchCapPerOwner = 8;
        public const int DispatchBudgetPerCycle = 256;

        private struct Key : IComparable<Key>
        {
            public ulong Tick; public int Priority; public ulong Seq;
            public Key(ulong t, int p, ulong s) { Tick=t; Priority=p; Seq=s; }
            public int CompareTo(Key other)
            { var c = Tick.CompareTo(other.Tick); if (c!=0) return c; c = Priority.CompareTo(other.Priority); if (c!=0) return c; return Seq.CompareTo(other.Seq); }
        }

        private class Item
        {
            public long Id; public Key Key; public EventLane Lane; public object Owner; public Action Callback; public bool Cancelled;
        }

        private readonly List<Item> _heap = new List<Item>();
        private readonly Dictionary<object, int> _sameTickDispatchCount = new Dictionary<object, int>();
        private ulong _sequence; private long _nextId = 1;
        private bool _paused; private bool _processingTopLevel;

        public ulong Now { get; private set; }

        public void Pause(bool pause) { _paused = pause; }

        public long Schedule(ulong tick, EventPriority priority, EventLane lane, object owner, Action callback)
        {
            if (callback == null) throw new ArgumentNullException("callback");
            if (lane != EventLane.Dispatch && tick == Now) throw new InvalidOperationException("Tick/Action may not schedule to Now");
            if (lane == EventLane.Dispatch && tick == Now && owner != null)
            {
                int count; _sameTickDispatchCount.TryGetValue(owner, out count);
                if (count >= SameTickDispatchCapPerOwner) return 0; // dropped per policy
                _sameTickDispatchCount[owner] = count + 1;
            }
            var it = new Item { Id = _nextId++, Key = new Key(tick, (int)priority, _sequence++), Lane = lane, Owner = owner, Callback = callback, Cancelled = false };
            HeapPush(it); return it.Id;
        }

        public void Cancel(long id) { for (int i=0;i<_heap.Count;i++) if (_heap[i].Id==id) { _heap[i].Cancelled=true; return; } }
        public int CancelOwned(object owner) { int n=0; for (int i=0;i<_heap.Count;i++) if (object.Equals(_heap[i].Owner, owner)) { _heap[i].Cancelled=true; n++; } return n; }

        public void AdvanceTo(ulong targetTick)
        {
            if (_paused) return;
            if (targetTick < Now) targetTick = Now;

            while (_heap.Count > 0)
            {
                var top = _heap[0];
                if (top.Key.Tick > targetTick) break;

                int budget = DispatchBudgetPerCycle;
                top = HeapPop();
                if (top.Cancelled) continue;

                Now = top.Key.Tick;

                if (top.Lane == EventLane.Dispatch)
                {
                    budget = ProcessDispatchChain(top, budget);
                }
                else
                {
                    _processingTopLevel = true;
                    SafeInvoke(top);
                    _processingTopLevel = false;
                    budget = ProcessSameTickDispatches(budget);
                }
            }

            _sameTickDispatchCount.Clear();

            // 👇 ensure time moves forward even if no jobs were due
            Now = targetTick;
        }

        private int ProcessDispatchChain(Item first, int budget)
        { _processingTopLevel = true; SafeInvoke(first); budget--; if (budget<=0) { _processingTopLevel=false; return 0; } budget = ProcessSameTickDispatches(budget); _processingTopLevel=false; return budget; }

        private int ProcessSameTickDispatches(int budget)
        {
            while (budget>0 && _heap.Count>0)
            { var p = _heap[0]; if (p.Key.Tick != Now || p.Lane != EventLane.Dispatch) break; p = HeapPop(); if (p.Cancelled) continue; SafeInvoke(p); budget--; }
            return budget;
        }

        private void SafeInvoke(Item it) { try { if (it.Callback!=null) it.Callback(); } catch (Exception e) { UnityEngine.Debug.LogException(e); } }

        private void HeapPush(Item it)
        {
            _heap.Add(it); int i = _heap.Count - 1; while (i>0) { int p=(i-1)>>1; if (_heap[p].Key.CompareTo(it.Key) <= 0) break; _heap[i]=_heap[p]; i=p; } _heap[i]=it;
        }

        private Item HeapPop()
        {
            int n=_heap.Count-1; var ret=_heap[0]; var x=_heap[n]; _heap.RemoveAt(n); if (n==0) return ret; int i=0; while (true) { int l=(i<<1)+1; if (l>=n) break; int r=l+1; int m=(r<n && _heap[r].Key.CompareTo(_heap[l].Key)<0)? r : l; if (_heap[m].Key.CompareTo(x.Key) >= 0) break; _heap[i]=_heap[m]; i=m; } _heap[i]=x; return ret;
        }
    }
}
