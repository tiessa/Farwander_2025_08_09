using UnityEngine;

namespace TJNK.Farwander.Modules.Game
{
    [CreateAssetMenu(menuName = "Farwander/Config/Enemy Type", fileName = "EnemyType")]
    public sealed class EnemyTypeSO : ScriptableObject
    {
        public string EnemyName = "Grunt";
        public Sprite Sprite;
        public int Count = 5;
        public StatsBlock BaseStats = new StatsBlock { HP=6, MaxHP=6, Attack=2, Defense=0, Range=4, Magic=2, Mana=5, MaxMana=5 };
        public bool Archer;
        public bool Caster;
        public float ManaRegenPerTick = 0.1f;
    }

    [CreateAssetMenu(menuName = "Farwander/Config/Enemy Roster", fileName = "EnemyRoster")]
    public sealed class EnemyRosterConfigSO : ScriptableObject
    {
        public EnemyTypeSO[] Types;
    }
}