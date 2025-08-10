using UnityEngine;

namespace TJNK.Farwander.Core
{
    /// <summary>Per-module scriptable configuration (empty for now).</summary>
    [CreateAssetMenu(menuName = "Farwander/Module Config", fileName = "ModuleConfig")]
    public sealed class ModuleConfig : ScriptableObject
    {
        [TextArea] public string notes;
    }
}