using UnityEngine;
using TJNK.Farwander.Core;
using TJNK.Farwander.Systems;

namespace TJNK.Farwander.Actors
{
    public class Actor : MonoBehaviour
    {
        public Grid grid;          // Assigned at runtime by GameBootstrap
        public bool HasGrid => grid != null;

        private GridPosition _lastPos;
        private bool _hasLastPos;

        public Vector3Int Cell => grid.WorldToCell(transform.position);
        public GridPosition Pos => GridPosition.FromV3Int(Cell);

        public void Place(GridPosition p)
        {
            // Requires grid assigned
            var newWorld = grid.CellToWorld(p.ToV3Int()) + new Vector3(0.5f, 0.5f, 0f);
            var from = _hasLastPos ? _lastPos : p;

            transform.position = newWorld;

            if (ActorIndex.Instance)
            {
                if (!_hasLastPos)
                {
                    ActorIndex.Instance.Register(this, p);
                    _hasLastPos = true;
                }
                else
                {
                    ActorIndex.Instance.NotifyMove(this, from, p);
                }
            }

            _lastPos = p;
            _hasLastPos = true;
        }
    }
}