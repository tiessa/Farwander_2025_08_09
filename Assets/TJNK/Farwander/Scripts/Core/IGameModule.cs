using UnityEngine;

namespace TJNK.Farwander.Core
{
    /// <summary>Basic lifecycle; no gameplay logic yet.</summary>
    public interface IGameModule
    {
        string Id { get; }
        void Initialize(GameCore core);
        void Shutdown();
    }
}