using UnityEngine;
using System;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; }

    public event Action OnCurrencyChanged;

    [Header("Currency")]
    [SerializeField] private int silver = 1000;
    [SerializeField] private int gold = 1000;

    public int Silver => silver;
    public int Gold => gold;

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

    public void AddSilver(int amount)
    {
        if (amount <= 0) return;
        silver += amount;
        OnCurrencyChanged?.Invoke();
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        gold += amount;
        OnCurrencyChanged?.Invoke();
    }
}