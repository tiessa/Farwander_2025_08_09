using UnityEngine;

namespace TJNK.Farwander.Core
{
    /// <summary>Base provider Mono to host a module instance (scaffolding only).</summary>
    public abstract class ModuleProvider : MonoBehaviour
    {
        public ModuleConfig config;
        public abstract string ModuleId { get; }
        protected IGameModule _instance;
        public virtual void Bind(GameCore core) { }
    }
}