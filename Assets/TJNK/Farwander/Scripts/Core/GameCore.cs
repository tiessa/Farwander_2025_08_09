using System;
using UnityEngine;

namespace TJNK.Farwander.Core
{
    /// <summary>Minimal GameCore that advances ticks at a fixed rate and exposes core services.</summary>
    public sealed class GameCore : MonoBehaviour
    {
        [Header("Clock")]
        [SerializeField] private uint ticksPerSecond = 30; // never changes at runtime
        [SerializeField] private bool paused = false;

        public ulong Now { get { return _scheduler.Now; } }

        private EventBus _bus;
        private QueryRegistry _queries;
        private TimedScheduler _scheduler;
        private ValidationPipeline _validation;

        private double _accum; // seconds
        private double _tickDuration; // seconds per tick
        private ulong _lastLoggedTick = ulong.MaxValue;

        private void Awake()
        {
            _bus = new EventBus();
            _queries = new QueryRegistry();
            _scheduler = new TimedScheduler();
            _validation = new ValidationPipeline();

            _tickDuration = 1.0 / Mathf.Max(1, (int)ticksPerSecond);

            // Expose services via QueryRegistry
            _queries.Register(() => _bus);
            _queries.Register(() => _scheduler);
            _queries.Register(() => _validation);
            _queries.Register(() => _queries); // self
            
            BindProviders();

            // TODO Save/Load hooks: serialize Now + queue state
        }
        
        private void BindProviders()
        {
            var providers = GetComponentsInChildren<ModuleProvider>(true);
            foreach (var p in providers)
            {
                try { p.Bind(this); }
                catch (System.Exception ex) { Debug.LogException(ex); }
            }
        }        

        private void Update()
        {
            if (paused) { _scheduler.Pause(true); return; }
            _scheduler.Pause(false);

            _accum += Time.deltaTime;
            ulong ticksToAdvance = 0UL;
            while (_accum >= _tickDuration)
            {
                _accum -= _tickDuration;
                ticksToAdvance++;
            }
            if (ticksToAdvance == 0 && Time.deltaTime > 0f) ticksToAdvance = 1; // min advance 1 tick when dt>0

            if (ticksToAdvance > 0)
            {
                var target = _scheduler.Now + ticksToAdvance;
                _scheduler.AdvanceTo(target);
            }
            
            if (Scheduler.Now != _lastLoggedTick)
            {
                _lastLoggedTick = Scheduler.Now;
                if ((_lastLoggedTick % 30UL) == 0UL)
                    Debug.Log($"[GameCore] Now={_lastLoggedTick}");
            }
        }

        // Expose for modules/tests
        public T Get<T>() where T : class { return _queries.Get<T>(); }
        public EventBus Bus { get { return _bus; } }
        public QueryRegistry Queries { get { return _queries; } }
        public TimedScheduler Scheduler { get { return _scheduler; } }
        public ValidationPipeline Validation { get { return _validation; } }
    }
}
