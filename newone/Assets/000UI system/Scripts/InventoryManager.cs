using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public event Action OnInventoryChanged;

    [SerializeField] private List<ItemDefinition> itemDatabase = new List<ItemDefinition>();

    private const string KEY_INV_JSON = "INV_JSON";

    [Serializable]
    private class InvEntry { public string id; public int count; }

    [Serializable]
    private class InvData { public List<InvEntry> items = new List<InvEntry>(); }

    private readonly Dictionary<string, int> counts = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();
    }

    public ItemDefinition GetDef(string id)
    {
        for (int i = 0; i < itemDatabase.Count; i++)
            if (itemDatabase[i] != null && itemDatabase[i].id == id)
                return itemDatabase[i];
        return null;
    }

    public int GetCount(string id) => counts.TryGetValue(id, out var c) ? c : 0;

    public void Add(string id, int amount)
    {
        if (amount <= 0) return;

        if (counts.ContainsKey(id)) counts[id] += amount;
        else counts[id] = amount;

        Save();
        OnInventoryChanged?.Invoke();
    }

    public List<(ItemDefinition def, int count)> GetByCategory(ItemCategory cat)
    {
        var list = new List<(ItemDefinition, int)>();
        foreach (var kv in counts)
        {
            if (kv.Value <= 0) continue;
            var def = GetDef(kv.Key);
            if (def == null) continue;
            if (def.category != cat) continue;
            list.Add((def, kv.Value));
        }
        return list;
    }
    public void AddItem(string itemId, int delta)
    {
        if (string.IsNullOrEmpty(itemId) || delta == 0) return;

        if (counts.ContainsKey(itemId))
            counts[itemId] += delta;
        else
            counts[itemId] = delta;

        if (counts[itemId] < 0) counts[itemId] = 0;

        Save();

        // 如果你有事件刷新UI（你Inspector里显示有 InventoryManager (Script)，大概率有）
        OnInventoryChanged?.Invoke();
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
    }
}