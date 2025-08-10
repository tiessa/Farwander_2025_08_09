using UnityEngine;
using TJNK.Farwander.Core;
using TJNK.Farwander.Modules.Game;

namespace TJNK.Farwander.Modules.Input
{
    /// <summary>
    /// Reads keyboard and schedules Input_* events on the Action lane at Now+1 tick.
    /// Pure input provider; gameplay logic remains in services.
    /// </summary>
    public sealed class InputRouterModuleProvider : ModuleProvider
    {
        public override string ModuleId { get { return "Input.Router"; } }
        private EventBus _bus; private TimedScheduler _sch;

        public override void Bind(GameCore core)
        {
            base.Bind(core);
            _bus = core.Bus; _sch = core.Scheduler;
        }

        private void Update()
        {
            if (_bus == null || _sch == null) return;
            var dir = ReadDirectionKeyDown();
            if (dir.HasValue)
            {
                var d = dir.Value;
                _sch.Schedule(_sch.Now + 1, EventPriority.Actor, EventLane.Action, this, () =>
                {
                    _bus.Publish(new Input_Move { Dir = d });
                });
                return;
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Space) || UnityEngine.Input.GetKeyDown(KeyCode.Period))
            {
                _sch.Schedule(_sch.Now + 1, EventPriority.Actor, EventLane.Action, this, () =>
                {
                    _bus.Publish(new Input_Wait { });
                });
            }
        }

        private Direction8? ReadDirectionKeyDown()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.A) || UnityEngine.Input.GetKeyDown(KeyCode.H)) return Direction8.Left;
            if (UnityEngine.Input.GetKeyDown(KeyCode.D) || UnityEngine.Input.GetKeyDown(KeyCode.L)) return Direction8.Right;
            if (UnityEngine.Input.GetKeyDown(KeyCode.W) || UnityEngine.Input.GetKeyDown(KeyCode.K)) return Direction8.Up;
            if (UnityEngine.Input.GetKeyDown(KeyCode.S) || UnityEngine.Input.GetKeyDown(KeyCode.J)) return Direction8.Down;
            if (UnityEngine.Input.GetKeyDown(KeyCode.Y) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad7)) return Direction8.UpLeft;
            if (UnityEngine.Input.GetKeyDown(KeyCode.U) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad9)) return Direction8.UpRight;
            if (UnityEngine.Input.GetKeyDown(KeyCode.B) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad1)) return Direction8.DownLeft;
            if (UnityEngine.Input.GetKeyDown(KeyCode.N) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad3)) return Direction8.DownRight;
            return null;
        }
    }
}
