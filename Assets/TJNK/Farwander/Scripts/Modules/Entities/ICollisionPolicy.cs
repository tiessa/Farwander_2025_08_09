using UnityEngine;
using TJNK.Farwander.Modules.Generation;

namespace TJNK.Farwander.Modules.Game.Runtime.Movement
{
    public interface ICollisionPolicy
    {
        bool CanEnter(Vector2Int cell);
    }
}