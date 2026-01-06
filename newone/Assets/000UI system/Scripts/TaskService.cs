using System;
using System.Collections.Generic;
using UnityEngine;

public enum RewardType
{
    Silver,
    Item
}

[Serializable]
public class TaskDefinition
{
    public string id;
    public string title;
    public RewardType rewardType;
    public int silverAmount;
    public string itemId;
}

public class TaskService : MonoBehaviour
{
    public static TaskService Instance;

    public event Action OnTaskChanged;

    private const string KEY_DONE = "TASK_DONE_";
    private const string KEY_CLAIMED = "TASK_CLAIMED_";

    private List<TaskDefinition> defs = new List<TaskDefinition>()
    {
        new TaskDefinition {
            id = "TASK_LOGIN_CLAIM_80",
            title = "登录游戏即可领取",
            rewardType = RewardType.Silver,
            silverAmount = 80
        },
        new TaskDefinition {
            id = "TASK_UPLOAD_ONCE_BETA_ITEM",
            title = "成功上传作品一次（领取内测限定道具）",
            rewardType = RewardType.Item,
            itemId = "beta_badge"   // 花环
        }
    };

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

    public List<TaskDefinition> GetAllTasks() => defs;

    public bool IsDone(string id) => PlayerPrefs.GetInt(KEY_DONE + id, 0) == 1;
    public bool IsClaimed(string id) => PlayerPrefs.GetInt(KEY_CLAIMED + id, 0) == 1;

    private void SetDone(string id)
    {
        PlayerPrefs.SetInt(KEY_DONE + id, 1);
        PlayerPrefs.Save();
        OnTaskChanged?.Invoke();
    }

    private void SetClaimed(string id)
    {
        PlayerPrefs.SetInt(KEY_CLAIMED + id, 1);
        PlayerPrefs.Save();
        OnTaskChanged?.Invoke();
    }

    // ===== 给 AuthManager 调 =====
    public void MarkLoginCompleted()
    {
        SetDone("TASK_LOGIN_CLAIM_80");
    }

    // ===== 给 上传逻辑 调 =====
    public void MarkUploadCompleted()
    {
        SetDone("TASK_UPLOAD_ONCE_BETA_ITEM");
    }

    // ===== UI 点领取 调 =====
    public bool Claim(string taskId)
    {
        if (!IsDone(taskId)) return false;
        if (IsClaimed(taskId)) return false;

        TaskDefinition def = defs.Find(d => d.id == taskId);
        if (def == null) return false;

        if (def.rewardType == RewardType.Silver)
        {
            PlayerData.Instance.AddSilver(def.silverAmount);
        }
        else if (def.rewardType == RewardType.Item)
        {
            if (InventoryManager.Instance == null)
            {
                Debug.LogError("❌ 手机端没有 InventoryManager，奖励失败");
                return false;
            }

            InventoryManager.Instance.AddItem(def.itemId, 1);
        }

        SetClaimed(taskId);
        return true;
    }
}