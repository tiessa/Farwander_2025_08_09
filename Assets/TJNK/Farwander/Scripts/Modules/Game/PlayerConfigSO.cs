using UnityEngine;

namespace TJNK.Farwander.Modules.Game
{
    [System.Serializable]
    public struct StatsBlock
    {
        public int HP, MaxHP, Attack, Defense, Range, Magic, Mana, MaxMana;
    }

    [CreateAssetMenu(menuName = "Farwander/Config/Player", fileName = "PlayerConfig")]
    public sealed class PlayerConfigSO : ScriptableObject
    {
        public Sprite SpriteOverride; // optional
        public StatsBlock BaseStats = new StatsBlock { HP=10, MaxHP=10, Attack=2, Defense=1, Range=5, Magic=3, Mana=10, MaxMana=10 };
        public float ManaRegenPerTick = 0.2f;
    }
}