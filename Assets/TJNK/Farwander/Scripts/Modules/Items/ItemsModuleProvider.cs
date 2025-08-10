using TJNK.Farwander.Core;
using UnityEngine;

namespace TJNK.Farwander.Modules.Items
{
    /// <summary>Scaffolding only; no gameplay yet. TODO: Publish/Subscribe events in this module.</summary>
    public sealed class ItemsModuleProvider : ModuleProvider
    {
        public override string ModuleId { get { return "Items"; } }
        public override void Bind(GameCore core)
        {
            base.Bind(core);
            // TODO: subscribe/register via core.Bus / core.Queries
        }
    }
}