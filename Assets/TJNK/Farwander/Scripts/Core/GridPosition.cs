using UnityEngine;

namespace TJNK.Farwander.Core
{
    [System.Serializable]
    public struct GridPosition
    {
        public int x, y;
        public GridPosition(int x, int y) { this.x = x; this.y = y; }

        public static GridPosition operator +(GridPosition a, GridPosition b) => new GridPosition(a.x + b.x, a.y + b.y);
        public static bool operator ==(GridPosition a, GridPosition b) => a.x == b.x && a.y == b.y;
        public static bool operator !=(GridPosition a, GridPosition b) => !(a == b);
        public override bool Equals(object obj) => obj is GridPosition p && p == this;
        public override int GetHashCode() => (x * 397) ^ y;

        public Vector3Int ToV3Int() => new Vector3Int(x, y, 0);
        public static GridPosition FromV3Int(Vector3Int v) => new GridPosition(v.x, v.y);
    }
}