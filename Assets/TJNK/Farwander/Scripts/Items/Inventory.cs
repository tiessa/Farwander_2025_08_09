using System;
using System.Collections.Generic;
using UnityEngine;

namespace TJNK.Farwander.Items
{
    [DisallowMultipleComponent]
    public class Inventory : MonoBehaviour
    {
        [Min(1)] public int capacity = 12;
        [SerializeField] private List<ItemInstance> slots = new(); // size grows to capacity
        public event Action<Inventory> OnInventoryChanged;
        public int SelectedIndex { get; private set; } = -1;

        void Awake()
        {
            EnsureSize();
        }

        private void EnsureSize()
        {
            while (slots.Count < capacity) slots.Add(null);
            if (slots.Count > capacity) slots.RemoveRange(capacity, slots.Count - capacity);
        }

        public IReadOnlyList<ItemInstance> Slots => slots;

        public void ToggleSelect(int index)
        {
            if (index < 0 || index >= capacity) return;
            SelectedIndex = (SelectedIndex == index) ? -1 : index;
            OnInventoryChanged?.Invoke(this);
        }

        public bool TryAdd(ItemInstance add)
        {
            if (add == null || add.def == null || add.count <= 0) return false;

            // 1) Merge into existing stacks
            if (add.def.maxStack > 1)
            {
                for (int i = 0; i < capacity && add.count > 0; i++)
                {
                    var s = slots[i];
                    if (s != null && s.def == add.def && s.count < s.def.maxStack)
                    {
                        add.count = s.AddInto(add.count);
                    }
                }
            }

            // 2) Place into empty slots
            for (int i = 0; i < capacity && add.count > 0; i++)
            {
                if (slots[i] == null)
                {
                    int place = (add.def.maxStack > 1) ? Mathf.Min(add.count, add.def.maxStack) : 1;
                    slots[i] = new ItemInstance(add.def, place);
                    add.count -= place;
                }
            }

            bool addedAll = add.count <= 0;
            OnInventoryChanged?.Invoke(this);
            return addedAll;
        }

        public ItemInstance TakeAllAt(int index)
        {
            if (index < 0 || index >= capacity) return null;
            var inst = slots[index];
            slots[index] = null;
            if (SelectedIndex == index) SelectedIndex = -1;
            if (inst != null) OnInventoryChanged?.Invoke(this);
            return inst;
        }

        public ItemInstance TakeSelected()
        {
            if (SelectedIndex < 0) return null;
            return TakeAllAt(SelectedIndex);
        }
    }
}
