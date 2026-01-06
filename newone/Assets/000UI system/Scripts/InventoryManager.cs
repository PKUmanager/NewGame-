using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public event Action OnInventoryChanged;

    [Header("可选：如果不想用Resources，也可以继续在Inspector拖。")]
    [SerializeField] private List<ItemDefinition> itemDatabase = new List<ItemDefinition>();

    private const string KEY_INV_JSON = "INV_JSON";

    [Serializable] private class InvEntry { public string id; public int count; }
    [Serializable] private class InvData { public List<InvEntry> items = new List<InvEntry>(); }

    private readonly Dictionary<string, int> counts = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ✅ 关键：手机端经常因为场景/Prefab引用丢失导致 itemDatabase 为空
        // 如果Inspector没配，就自动从 Resources/Items 加载所有 ItemDefinition
        if (itemDatabase == null || itemDatabase.Count == 0)
        {
            var loaded = Resources.LoadAll<ItemDefinition>("Items");
            if (loaded != null && loaded.Length > 0)
                itemDatabase = new List<ItemDefinition>(loaded);

            Debug.Log($"[InventoryManager] Loaded ItemDefinition from Resources/Items: {(loaded == null ? 0 : loaded.Length)}");
        }
        else
        {
            Debug.Log($"[InventoryManager] ItemDatabase from Inspector: {itemDatabase.Count}");
        }

        Load();
    }

    public ItemDefinition GetDef(string id)
    {
        for (int i = 0; i < itemDatabase.Count; i++)
        {
            if (itemDatabase[i] != null && itemDatabase[i].id == id)
                return itemDatabase[i];
        }

        Debug.LogError($"[InventoryManager] GetDef NOT FOUND id={id}. 这会导致背包不显示该物品。请确认有对应 ItemDefinition 且在 Resources/Items 或 Inspector 列表里。");
        return null;
    }

    public int GetCount(string id) => counts.TryGetValue(id, out var c) ? c : 0;

    public void AddItem(string itemId, int delta)
    {
        if (string.IsNullOrEmpty(itemId) || delta == 0) return;

        if (counts.ContainsKey(itemId))
            counts[itemId] += delta;
        else
            counts[itemId] = delta;

        if (counts[itemId] < 0) counts[itemId] = 0;

        Save();
        OnInventoryChanged?.Invoke();

        Debug.Log($"[InventoryManager] AddItem: {itemId} delta={delta} now={counts[itemId]}");
    }

    public List<(ItemDefinition def, int count)> GetByCategory(ItemCategory cat)
    {
        var list = new List<(ItemDefinition, int)>();
        foreach (var kv in counts)
        {
            if (kv.Value <= 0) continue;

            var def = GetDef(kv.Key);
            if (def == null) continue;              // ✅ def找不到就不会显示（你手机端问题基本在这里）
            if (def.category != cat) continue;

            list.Add((def, kv.Value));
        }
        return list;
    }

    private void Save()
    {
        var data = new InvData();
        foreach (var kv in counts)
            data.items.Add(new InvEntry { id = kv.Key, count = kv.Value });

        PlayerPrefs.SetString(KEY_INV_JSON, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    private void Load()
    {
        counts.Clear();
        if (!PlayerPrefs.HasKey(KEY_INV_JSON)) return;

        var json = PlayerPrefs.GetString(KEY_INV_JSON, "");
        if (string.IsNullOrEmpty(json)) return;

        var data = JsonUtility.FromJson<InvData>(json);
        if (data?.items == null) return;

        for (int i = 0; i < data.items.Count; i++)
        {
            var e = data.items[i];
            if (string.IsNullOrEmpty(e.id)) continue;
            if (e.count <= 0) continue;
            counts[e.id] = e.count;
        }

        Debug.Log($"[InventoryManager] Load OK. items={counts.Count}");
    }
}