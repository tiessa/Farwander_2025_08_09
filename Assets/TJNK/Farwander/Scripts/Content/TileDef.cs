using UnityEngine;
using UnityEngine.Tilemaps;

namespace TJNK.Farwander.Content
{
    [CreateAssetMenu(menuName = "Farwander/TileDef")]
    public class TileDef : ScriptableObject
    {
        public TileBase tile;
        public bool walkable = true;
        public bool blocksSight = false;
    }
}