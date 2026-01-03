using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryService : MonoBehaviour
{
    public static InventoryService Instance;

    public event Action OnInventoryChanged;

    // 我们只做“装备栏是否拥有某个道具”的最简方案
    // 用 PlayerPrefs 存：OWN_ITEM_<itemId> = 1
    private const string KEY_OWN_PREFIX = "OWN_ITEM_";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool HasItem(string itemId)
    {
        return PlayerPrefs.GetInt(KEY_OWN_PREFIX + itemId, 0) == 1;
    }

    public void GrantItem(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return;
        if (HasItem(itemId)) return;

        PlayerPrefs.SetInt(KEY_OWN_PREFIX + itemId, 1);
        PlayerPrefs.Save();

        OnInventoryChanged?.Invoke();
    }
}