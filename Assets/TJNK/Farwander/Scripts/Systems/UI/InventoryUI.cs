using UnityEngine;
using UnityEngine.UI;
using TJNK.Farwander.Items;

namespace TJNK.Farwander.Systems.UI
{
    public class InventoryUI : MonoBehaviour
    {
        public Inventory inventory;
        public Transform gridRoot;    // parent for slots
        public GameObject slotPrefab; // Image + count Text + highlight image

        private void OnEnable()
        {
            if (inventory != null) inventory.OnInventoryChanged += Refresh;
            Refresh(inventory);
        }
        private void OnDisable()
        {
            if (inventory != null) inventory.OnInventoryChanged -= Refresh;
        }

        public void Bind(Inventory inv)
        {
            if (inventory != null) inventory.OnInventoryChanged -= Refresh;
            inventory = inv;
            if (inventory != null) inventory.OnInventoryChanged += Refresh;
            Refresh(inventory);
        }

        private void Clear()
        {
            for (int i = gridRoot.childCount - 1; i >= 0; i--)
                Destroy(gridRoot.GetChild(i).gameObject);
        }

        private void Refresh(Inventory inv)
        {
            if (!inv || !gridRoot || !slotPrefab) return;
            Clear();

            var slots = inv.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                var go = Instantiate(slotPrefab, gridRoot);
                go.name = $"Slot_{i}";
                var icon = go.transform.Find("Icon")?.GetComponent<Image>();
                var count = go.transform.Find("Count")?.GetComponent<Text>();
                var highlight = go.transform.Find("Highlight")?.GetComponent<Image>();
                var idx = i;

                var inst = slots[i];
                if (inst != null)
                {
                    if (icon) icon.sprite = inst.def.icon;
                    if (count) count.text = (inst.def.maxStack > 1 && inst.count > 1) ? inst.count.ToString() : "";
                }
                else
                {
                    if (icon) icon.sprite = null;
                    if (count) count.text = "";
                }

                if (highlight) highlight.enabled = (inv.SelectedIndex == i);

                var btn = go.GetComponent<Button>();
                if (btn)
                    btn.onClick.AddListener(() => { inv.ToggleSelect(idx); Refresh(inv); });
            }
        }
    }
}
