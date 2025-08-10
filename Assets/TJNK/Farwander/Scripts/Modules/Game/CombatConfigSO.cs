using UnityEngine;

namespace TJNK.Farwander.Modules.Game
{
    [CreateAssetMenu(menuName = "Farwander/Config/Combat", fileName = "CombatConfig")]
    public sealed class CombatConfigSO : ScriptableObject
    {
        [Tooltip("Ticks consumed by a step or cast.")] public int StepTicks = 1;
        public bool EnemyDropsOnDeath = true;
        public bool PlayerKeepsItemsOnDeath = true;
    }
}