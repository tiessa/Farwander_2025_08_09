using System.Collections.Generic;

namespace TJNK.Farwander.Modules.AI
{
    public interface IEnemyRegistry
    {
        IReadOnlyList<int> EnemyIds { get; }
        void Register(int id);
        void Clear();
    }
}