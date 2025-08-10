using UnityEngine;

namespace TJNK.Farwander.Items
{
    [CreateAssetMenu(menuName = "Farwander/Item", fileName = "Item")]
    public class ItemDef : ScriptableObject
    {
        public string id; // unique, used for save/load
        public string displayName;
        [TextArea] public string description;
        public ItemKind kind = ItemKind.Material;
        public EquipSlot slot = EquipSlot.None;
        [Min(1)] public int maxStack = 99;
        public Sprite icon; // placeholder generated if null
        // phase-2+ fields (ignored now): baseDamage, range, etc.
    }
}