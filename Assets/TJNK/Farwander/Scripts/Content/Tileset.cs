using UnityEngine;

namespace TJNK.Farwander.Content
{
    [CreateAssetMenu(menuName = "Farwander/Tileset")]
    public class Tileset : ScriptableObject
    {
        public TileDef floor;
        public TileDef wall;
    }
}