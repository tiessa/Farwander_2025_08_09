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
        public int meleeDamage = 2;

        void OnEnable()  => TurnManager.Instance.RegisterEnemy(this);
        void OnDisable() => TurnManager.Instance.UnregisterEnemy(this);

        public IEnumerator TakeTurn()
        {
            if (!HasGrid || Runtime == null || Player == null) { yield return null; yield break; }

            var p = GridPosition.FromV3Int(Player.Cell);
            var me = this.Pos;

            // If adjacent to player: attack
            if (Manhattan(me, p) == 1)
            {
                CombatResolver.TryMelee(this, Player, meleeDamage);
                yield return null;
                yield break;
            }

            // Otherwise chase if in range
            if (Manhattan(me, p) <= chaseRange)
            {
                var path = Pathfinder.FindPath(me, p, Runtime.Generator.IsWalkable);
                if (path.Count > 1)
                {
                    var next = path[1];
                    // If next step is the player cell, attack instead of move
                    if (next == p)
                    {
                        CombatResolver.TryMelee(this, Player, meleeDamage);
                    }
                    else
                    {
                        // If another enemy stands there, just wait (simple rule)
                        var blocker = ActorIndex.Instance ? ActorIndex.Instance.GetFirstAt<EnemyController>(next) : null;
                        if (blocker == null)
                            Place(next);
                    }
                }
            }
            yield return null;
        }

        private int Manhattan(GridPosition a, GridPosition b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}
