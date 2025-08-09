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
        
        private static readonly GridPosition[] Dirs =
        {
            new GridPosition( 1, 0), new GridPosition(-1, 0),
            new GridPosition( 0, 1), new GridPosition( 0,-1)
        };
        
        void Update()
        {
            if (!HasGrid || Runtime == null) return;                      // <-- guard
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
            if (Runtime.Generator.IsWalkable(target))
            {
                Place(target);
                Runtime.Visibility.Recompute(target, fovRadius, Runtime.Generator.BlocksSight);
            }
            // keep cadence either way
            TurnManager.Instance.EndPlayerTurn();
        }
    }
}