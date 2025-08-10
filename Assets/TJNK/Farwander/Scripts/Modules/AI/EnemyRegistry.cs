using System.Collections.Generic;

namespace TJNK.Farwander.Modules.AI
{
    public sealed class EnemyRegistry : IEnemyRegistry
    {
        private readonly List<int> _ids = new List<int>();
        public IReadOnlyList<int> EnemyIds => _ids;
        public void Register(int id) { if (!_ids.Contains(id)) _ids.Add(id); }
        public void Clear() => _ids.Clear();
    }
}