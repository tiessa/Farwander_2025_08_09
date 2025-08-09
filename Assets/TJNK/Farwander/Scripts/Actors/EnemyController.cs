using System.Collections;
using UnityEngine;
using TJNK.Farwander.Core;
using TJNK.Farwander.Systems;
using TJNK.Farwander.Generation;

namespace TJNK.Farwander.Actors
{
    public class EnemyController : Actor, IEnemyActor
    {
        public MapRuntime Runtime; // Assigned by GameBootstrap
        public Actor Player;       // Assigned by GameBootstrap
        public int chaseRange = 30;

        void OnEnable()  => TurnManager.Instance.RegisterEnemy(this);
        void OnDisable() => TurnManager.Instance.UnregisterEnemy(this);

        public IEnumerator TakeTurn()
        {
            if (!HasGrid || Runtime == null || Player == null) { yield return null; yield break; }
            
            var p = GridPosition.FromV3Int(Player.Cell);
            var me = this.Pos;
            if (Manhattan(me, p) <= chaseRange)
            {
                var path = Pathfinder.FindPath(me, p, Runtime.Generator.IsWalkable);
                if (path.Count > 1)
                {
                    Place(path[1]); // next step
                }
            }
            yield return null;
        }

        private int Manhattan(GridPosition a, GridPosition b)
            => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}