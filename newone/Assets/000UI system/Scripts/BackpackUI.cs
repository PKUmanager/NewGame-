using System;
using System.Collections.Generic;
using UnityEngine;

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

        if (contentRoot == null || itemSlotPrefab == null) return;
        if (InventoryManager.Instance == null) return;

        // BackpackUI.Category -> ItemCategory
        ItemCategory cat = (ItemCategory)Enum.Parse(typeof(ItemCategory), category.ToString());

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

    // ✅ 给按钮绑定用（Unity Button OnClick 能传 int）
    public void SelectCategory(int index)
    {
        index = Mathf.Clamp(index, 0, Enum.GetValues(typeof(Category)).Length - 1);
        currentCategory = (Category)index;
        RefreshCurrent();
    }
}