using System;
using UnityEngine;

namespace TJNK.Farwander.Items
{
    [Serializable]
    public class ItemInstance
    {
        public ItemDef def;
        public int count;

        public ItemInstance() {}
        public ItemInstance(ItemDef def, int count) { this.def = def; this.count = Mathf.Max(1, count); }

        public ItemInstance Clone() => new ItemInstance(def, count);
        public bool CanStackWith(ItemInstance other) => other != null && other.def == def && def.maxStack > 1;
        public int AddInto(int add)
        {
            if (def.maxStack <= 1) return add; // cannot stack
            int space = def.maxStack - count;
            int toAdd = Mathf.Clamp(add, 0, space);
            count += toAdd;
            return add - toAdd; // overflow
        }
    }
}