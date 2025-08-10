using System.Collections.Generic;
using UnityEngine;
using TJNK.Farwander.Core;
using TJNK.Farwander.World;

namespace TJNK.Farwander.Systems
{
    public class ItemIndex : MonoBehaviour
    {
        public static ItemIndex Instance { get; private set; }
        private readonly Dictionary<GridPosition, List<ItemPile>> byCell = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Register(ItemPile p)
        {
            var cell = p.Cell;
            if (!byCell.TryGetValue(cell, out var list)) { list = new List<ItemPile>(); byCell[cell] = list; }
            if (!list.Contains(p)) list.Add(p);
        }

        public void Unregister(ItemPile p)
        {
            var cell = p.Cell;
            if (byCell.TryGetValue(cell, out var list))
            {
                list.Remove(p);
                if (list.Count == 0) byCell.Remove(cell);
            }
        }

        public List<ItemPile> GetAt(GridPosition cell)
            => byCell.TryGetValue(cell, out var list) ? list : null;

        public ItemPile GetMergeable(GridPosition cell, Items.ItemDef def)
        {
            if (!byCell.TryGetValue(cell, out var list)) return null;
            for (int i = 0; i < list.Count; i++)
                if (list[i].stack != null && list[i].stack.def == def && def.maxStack > 1)
                    return list[i];
            return null;
        }
    }
}