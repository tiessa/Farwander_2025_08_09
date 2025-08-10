using UnityEngine;
using TJNK.Farwander.Core;
using TJNK.Farwander.Modules.Game.Runtime.Entities;

namespace TJNK.Farwander.Modules.Game.Runtime.Movement
{
    /// <summary>
    /// Subscribes to Move_Request (Action). Checks collision, updates IEntityLocations,
    /// and Dispatches Move_Resolved at Now.
    /// </summary>
    public sealed class MovementService
    {
        private readonly EventBus _bus; private readonly TimedScheduler _sch;
        private readonly IEntityLocations _locs; private readonly ICollisionPolicy _policy;

        public MovementService(EventBus bus, TimedScheduler sch, IEntityLocations locs, ICollisionPolicy policy)
        {
            _bus = bus; _sch = sch; _locs = locs; _policy = policy;
            _bus.Subscribe<TJNK.Farwander.Modules.Game.Move_Request>(OnMoveRequest);
        }

        private static UnityEngine.Vector2Int Step(UnityEngine.Vector2Int p, TJNK.Farwander.Modules.Game.Direction8 d)
        {
            switch (d)
            {
                case TJNK.Farwander.Modules.Game.Direction8.Left: return new Vector2Int(p.x-1, p.y);
                case TJNK.Farwander.Modules.Game.Direction8.Right: return new Vector2Int(p.x+1, p.y);
                case TJNK.Farwander.Modules.Game.Direction8.Up: return new Vector2Int(p.x, p.y+1);
                case TJNK.Farwander.Modules.Game.Direction8.Down: return new Vector2Int(p.x, p.y-1);
                case TJNK.Farwander.Modules.Game.Direction8.UpLeft: return new Vector2Int(p.x-1, p.y+1);
                case TJNK.Farwander.Modules.Game.Direction8.UpRight: return new Vector2Int(p.x+1, p.y+1);
                case TJNK.Farwander.Modules.Game.Direction8.DownLeft: return new Vector2Int(p.x-1, p.y-1);
                case TJNK.Farwander.Modules.Game.Direction8.DownRight: return new Vector2Int(p.x+1, p.y-1);
                default: return p;
            }
        }

        private void OnMoveRequest(TJNK.Farwander.Modules.Game.Move_Request req)
        {
            if (!_locs.TryGet(req.EntityId, out var from)) return;
            var to = Step(from, req.Dir);
            bool ok = _policy.CanEnter(to);
            if (ok) _locs.Set(req.EntityId, to);

            // Dispatch result at Now
            _sch.Schedule(_sch.Now, EventPriority.Actor, EventLane.Dispatch, this, () =>
            {
                _bus.Publish(new TJNK.Farwander.Modules.Game.Move_Resolved { EntityId = req.EntityId, From = from, To = to, Succeeded = ok });
            });
        }
    }
}
