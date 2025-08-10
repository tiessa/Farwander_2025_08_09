using UnityEngine;

namespace TJNK.Farwander.Modules.Game
{
    public enum SpellKind { Heal, MagicMissile }

    [CreateAssetMenu(menuName = "Farwander/Config/Spell", fileName = "Spell")]
    public sealed class SpellSO : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public Sprite Sprite;
        public SpellKind Kind;
        public int Range = 6;
        public int ManaCost = 5;
        public int Amount = 5; // heal amount or missile damage
    }

    [CreateAssetMenu(menuName = "Farwander/Config/Spell Database", fileName = "SpellDatabase")]
    public sealed class SpellDatabaseSO : ScriptableObject
    {
        public SpellSO[] Spells;
    }
}