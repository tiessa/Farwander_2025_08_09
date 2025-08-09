using System.Collections.Generic;
using UnityEngine;
using TJNK.Farwander.Core;
using TJNK.Farwander.Actors;

namespace TJNK.Farwander.Systems
{
    public class ActorIndex : MonoBehaviour
    {
        public static ActorIndex Instance { get; private set; }

        private readonly Dictionary<GridPosition, List<Actor>> _byCell = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Register(Actor a, GridPosition pos)
        {
            if (!_byCell.TryGetValue(pos, out var list))
            {
                list = new List<Actor>(2);
                _byCell[pos] = list;
            }
            if (!list.Contains(a)) list.Add(a);
        }

        public void Unregister(Actor a, GridPosition pos)
        {
            if (_byCell.TryGetValue(pos, out var list))
            {
                list.Remove(a);
                if (list.Count == 0) _byCell.Remove(pos);
            }
        }

        public void NotifyMove(Actor a, GridPosition from, GridPosition to)
        {
            if (from != to)
            {
                Unregister(a, from);
                Register(a, to);
            }
        }

        public Actor GetFirstAt(GridPosition pos)
        {
            return _byCell.TryGetValue(pos, out var list) && list.Count > 0 ? list[0] : null;
        }

        public T GetFirstAt<T>(GridPosition pos) where T : Component
        {
            if (_byCell.TryGetValue(pos, out var list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var c = list[i].GetComponent<T>();
                    if (c) return c;
                }
            }
            return null;
        }
    }
}