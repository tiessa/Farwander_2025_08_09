using UnityEngine;

namespace TJNK.Farwander.Modules.Game
{
    [System.Flags]
    public enum EquipSlotsAllowed { None = 0, Head = 1<<0, Body = 1<<1, MainHand = 1<<2 }

    [CreateAssetMenu(menuName = "Farwander/Config/Item", fileName = "Item")]
    public sealed class ItemSO : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public Sprite Sprite;
        public bool Stackable;
        public EquipSlotsAllowed EquipSlots;
        [Header("Stat Mods")] public int AttackMod, DefenseMod, RangeMod, MagicMod, HpMod, MaxHpMod, ManaMod, MaxManaMod;
    }

    [CreateAssetMenu(menuName = "Farwander/Config/Item Database", fileName = "ItemDatabase")]
    public sealed class ItemDatabaseSO : ScriptableObject
    {
        public ItemSO[] Items;
    }
}