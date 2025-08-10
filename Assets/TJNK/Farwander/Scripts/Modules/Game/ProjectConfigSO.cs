using UnityEngine;

namespace TJNK.Farwander.Modules.Game
{
    [CreateAssetMenu(menuName = "Farwander/Config/Project Config", fileName = "ProjectConfig")]
    public sealed class ProjectConfigSO : ScriptableObject
    {
        public DungeonConfigSO Dungeon;
        public PlayerConfigSO Player;
        public EnemyRosterConfigSO Enemies;
        public ItemDatabaseSO ItemDB;
        public SpellDatabaseSO SpellDB;
        public CombatConfigSO Combat;
        public UIConfigSO UI;
    }
}