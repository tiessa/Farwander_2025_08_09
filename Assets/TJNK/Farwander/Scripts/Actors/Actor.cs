using UnityEngine;
using TJNK.Farwander.Core;

namespace TJNK.Farwander.Actors
{
    public class Actor : MonoBehaviour
    {
        public Grid grid;          // Assigned at runtime by GameBootstrap
        public bool HasGrid => grid != null;

        public Vector3Int Cell => grid.WorldToCell(transform.position);
        public GridPosition Pos => GridPosition.FromV3Int(Cell);

        public void Place(GridPosition p)
        {
            // Requires grid; caller should ensure HasGrid is true
            transform.position = grid.CellToWorld(p.ToV3Int()) + new Vector3(0.5f, 0.5f, 0f);
        }
    }
}