using UnityEngine;

namespace TJNK.Farwander.Modules.Game
{
    [CreateAssetMenu(menuName = "Farwander/Config/UI", fileName = "UIConfig")]
    public sealed class UIConfigSO : ScriptableObject
    {
        public Texture2D Atlas; // optional placeholder atlas
        public int TileSize = 32;
        public int InventoryPageSize = 10;
        [Header("Colors (optional)")]
        public Color HealthBarFG = new Color(0, 1, 0, 1);
        public Color HealthBarBG = new Color(0.2f, 0.2f, 0.2f, 1);
    }
}