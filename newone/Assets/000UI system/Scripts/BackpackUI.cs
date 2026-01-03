using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackpackUI : MonoBehaviour
{
    public enum Category { 家具, 装饰, 植物, 材料, 装扮 }

    [Header("ScrollView Content")]
    [SerializeField] private Transform contentRoot;

    [Header("ItemSlot Prefab")]
    [SerializeField] private ItemSlotUI itemSlotPrefab;

    private Category currentCategory = Category.家具;

    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += RefreshCurrent;

        RefreshCurrent();
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshCurrent;
    }

    private void RefreshCurrent()
    {
        RefreshList(currentCategory);
    }

    private void RefreshList(Category category)
    {
        ClearContent();
        if (InventoryManager.Instance == null) return;

        // Category -> ItemCategory
        ItemCategory cat = (ItemCategory)System.Enum.Parse(typeof(ItemCategory), category.ToString());

        var items = InventoryManager.Instance.GetByCategory(cat);
        for (int i = 0; i < items.Count; i++)
        {
            var (def, count) = items[i];
            var slot = Instantiate(itemSlotPrefab, contentRoot);
            slot.Bind(def.displayName, count, def.icon);
        }
    }

    private void ClearContent()
    {
        if (contentRoot == null) return;
        var slots = contentRoot.GetComponentsInChildren<ItemSlotUI>(true);
        for (int i = 0; i < slots.Length; i++)
            Destroy(slots[i].gameObject);
    }
}