using UnityEngine;

namespace TJNK.Farwander.Modules.AI.Config
{
    [CreateAssetMenu(menuName = "Farwander/Enemies/Enemy Type", fileName = "EnemyType")]
    public sealed class EnemyTypeSO : ScriptableObject
    {
        public string Id = "Enemy";
        public Color32 Tint = new Color32(200, 60, 60, 255);
        // Future: stats, AI flags, speed, ranged/caster, etc.
    }
}