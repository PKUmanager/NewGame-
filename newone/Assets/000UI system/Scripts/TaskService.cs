using System;
using System.Collections.Generic;
using UnityEngine;

public enum RewardType
{
    Silver = 0,
    Item = 1
}

[Serializable]
public class TaskDefinition
{
    public string id;          // 唯一ID（别改）
    public string title;       // UI标题
    public RewardType rewardType;
    public int silverAmount;   // rewardType=Silver用
    public string itemId;      // rewardType=Item用（例如 "BETA_EQUIP_001"）
}

/// <summary>
/// 任务状态：完成/已领取（都要持久化）
/// </summary>
public class TaskService : MonoBehaviour
{
    public static TaskService Instance;

    public event Action OnTaskChanged; // UI刷新用

    // 你现在要的两个任务定义（最简单：直接写死）
    private readonly List<TaskDefinition> defs = new List<TaskDefinition>()
    {
        new TaskDefinition
        {
            id = "TASK_LOGIN_CLAIM_80",
            title = "登录游戏即可领取",
            rewardType = RewardType.Silver,
            silverAmount = 80
        },
        new TaskDefinition
        {
            id = "TASK_UPLOAD_ONCE_BETA_ITEM",
            title = "成功上传作品一次（领取内测限定道具）",
            rewardType = RewardType.Item,
            itemId = "BETA_EQUIP_001"
        }
    };

    private const string KEY_DONE_PREFIX = "TASK_DONE_";
    private const string KEY_CLAIM_PREFIX = "TASK_CLAIMED_";

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

    // 给UI用：获取全部任务定义
    public List<TaskDefinition> GetAllTasks()
    {
        return defs;
    }

    public bool IsDone(string taskId) => PlayerPrefs.GetInt(KEY_DONE_PREFIX + taskId, 0) == 1;
    public bool IsClaimed(string taskId) => PlayerPrefs.GetInt(KEY_CLAIM_PREFIX + taskId, 0) == 1;

    private void SetDone(string taskId, bool done)
    {
        PlayerPrefs.SetInt(KEY_DONE_PREFIX + taskId, done ? 1 : 0);
        PlayerPrefs.Save();
        OnTaskChanged?.Invoke();
    }

    private void SetClaimed(string taskId, bool claimed)
    {
        PlayerPrefs.SetInt(KEY_CLAIM_PREFIX + taskId, claimed ? 1 : 0);
        PlayerPrefs.Save();
        OnTaskChanged?.Invoke();
    }

    
    public void MarkLoginCompleted()
    {
       
        SetDone("TASK_LOGIN_CLAIM_80", true);
    }

    
    public void MarkUploadCompleted()
    {
        SetDone("TASK_UPLOAD_ONCE_BETA_ITEM", true);
    }


    public bool Claim(string taskId)
    {
        if (!IsDone(taskId)) return false;    // ✅没完成不能领
        if (IsClaimed(taskId)) return false;  // ✅领过不能再领

        TaskDefinition def = defs.Find(d => d.id == taskId);
        if (def == null) return false;

        // 发奖励（你这里的逻辑OK）
        if (def.rewardType == RewardType.Silver)
        {
            if (PlayerData.Instance != null) PlayerData.Instance.AddSilver(def.silverAmount);
            else
            {
                int cur = PlayerPrefs.GetInt("SILVER", 0);
                PlayerPrefs.SetInt("SILVER", cur + def.silverAmount);
                PlayerPrefs.Save();
            }
        }
        else if (def.rewardType == RewardType.Item)
        {
            InventoryService inv = InventoryService.Instance;
            if (inv != null && !string.IsNullOrEmpty(def.itemId))
                inv.GrantItem(def.itemId);
        }

        SetClaimed(taskId, true); // ✅最后标记已领取
        return true;
    }
}