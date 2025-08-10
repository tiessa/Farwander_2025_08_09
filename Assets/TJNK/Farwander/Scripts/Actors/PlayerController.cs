using UnityEngine;
using TJNK.Farwander.Core;
using TJNK.Farwander.Systems;
using TJNK.Farwander.Generation;

namespace TJNK.Farwander.Actors
{
    public class PlayerController : Actor
    {
        public MapRuntime Runtime; // Assigned by GameBootstrap
        public int fovRadius = 8;
        public int meleeDamage = 3;

        private static readonly GridPosition[] Dirs =
        {
            new GridPosition( 1, 0), new GridPosition(-1, 0),
            new GridPosition( 0, 1), new GridPosition( 0,-1)
        };

        void Update()
        {
            if (!HasGrid || Runtime == null) return;
            var tm = TJNK.Farwander.Systems.TurnManager.Instance;
            if (tm == null || !tm.IsPlayerTurn()) return;

            GridPosition move = default;
            bool pressed = false;

            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) { move = Dirs[0]; pressed = true; }
            else if (Input.GetKeyDown(KeyCode.LeftArrow)  || Input.GetKeyDown(KeyCode.A)) { move = Dirs[1]; pressed = true; }
            else if (Input.GetKeyDown(KeyCode.UpArrow)    || Input.GetKeyDown(KeyCode.W)) { move = Dirs[2]; pressed = true; }
            else if (Input.GetKeyDown(KeyCode.DownArrow)  || Input.GetKeyDown(KeyCode.S)) { move = Dirs[3]; pressed = true; }

            if (!pressed) return;

            var target = Pos + move;

            // If an enemy occupies the target cell, attack instead of moving
            var enemyAtTarget = ActorIndex.Instance ? ActorIndex.Instance.GetFirstAt<EnemyController>(target) : null;
            if (enemyAtTarget)
            {
                CombatResolver.TryMelee(this, enemyAtTarget.GetComponent<Actor>(), meleeDamage);
                tm.EndPlayerTurn();
                return;
            }

            // Otherwise try to move
            if (Runtime.Generator.IsWalkable(target))
            {
                Place(target);
                // Recompute FOV if you added Phase 1
                if (Runtime.Visibility != null)
                    Runtime.Visibility.Recompute(target, fovRadius, Runtime.Generator.BlocksSight);
            } 
            
            
            // PICKUP (G) on current cell only
            if (Input.GetKeyDown(KeyCode.G))
            {
                var cell = Pos;
                var piles = TJNK.Farwander.Systems.ItemIndex.Instance?.GetAt(cell);
                var inv = GetComponent<TJNK.Farwander.Items.Inventory>();
                if (piles != null && inv != null)
                {
                    // pick up all piles on this cell, merging stacks
                    for (int i = piles.Count - 1; i >= 0; i--)
                    {
                        var pile = piles[i];
                        var inst = pile.TakeAll();
                        TJNK.Farwander.Systems.ItemIndex.Instance.Unregister(pile);
                        inv.TryAdd(inst);
                    }
                }
            }
            
            // DROP selected (X)
            if (Input.GetKeyDown(KeyCode.X))
            {
                var inv = GetComponent<TJNK.Farwander.Items.Inventory>();
                if (inv != null)
                {
                    var take = inv.TakeSelected();
                    if (take != null)
                    {
                        var index = TJNK.Farwander.Systems.ItemIndex.Instance;
                        var merge = index?.GetMergeable(Pos, take.def);
                        if (merge != null && take.def.maxStack > 1)
                        {
                            int overflow = take.count - (merge.stack.def.maxStack - merge.stack.count);
                            merge.stack.AddInto(take.count); // returns internal overflow, but we already merged all possible
                            merge.AddCount(0); // refresh label
                            if (overflow > 0)
                            {
                                // spawn another pile for overflow
                                var extra = new TJNK.Farwander.Items.ItemInstance(take.def, overflow);
                                SpawnPile(extra, Pos);
                            }
                        }
                        else
                        {
                            SpawnPile(take, Pos);
                        }
                    }
                }
            }

            void SpawnPile(TJNK.Farwander.Items.ItemInstance inst, TJNK.Farwander.Core.GridPosition cell)
            {
                var go = new GameObject($"Pile_{inst.def.displayName}");
                var pile = go.AddComponent<TJNK.Farwander.World.ItemPile>();
                pile.Init(inst, grid, Runtime?.Visibility);
                pile.SetCell(cell);
                TJNK.Farwander.Systems.ItemIndex.Instance?.Register(pile);
            }            

            tm.EndPlayerTurn();
        }
    }
}
