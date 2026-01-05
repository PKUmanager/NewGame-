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
    public string id;            // 唯一ID（别改）
    public string title;         // UI标题
    public RewardType rewardType;
    public int silverAmount;     // rewardType=Silver用
    public string itemId;        // rewardType=Item用（例如 "beta_badge"）
}

public class TaskService : MonoBehaviour
{
    public static TaskService Instance;

    public event Action OnTaskChanged;

    // 任务定义（你现在是写死的，后续可改成ScriptableObject）
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
            itemId = "beta_badge"
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

    // 给UI用：获取全部任务
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

    // ✅ 登录完成：解锁登录奖励任务
    public void MarkLoginCompleted()
    {
        SetDone("TASK_LOGIN_CLAIM_80", true);
    }

    // ✅ 上传完成：解锁上传奖励任务
    public void MarkUploadCompleted()
    {
        SetDone("TASK_UPLOAD_ONCE_BETA_ITEM", true);
    }

    // ✅ 领取奖励（UI按钮点这个）
    public bool Claim(string taskId)
    {
        if (!IsDone(taskId)) return false;
        if (IsClaimed(taskId)) return false;

        TaskDefinition def = defs.Find(d => d.id == taskId);
        if (def == null) return false;

        // 发奖励
        if (def.rewardType == RewardType.Silver)
        {
            if (PlayerData.Instance != null)
            {
                PlayerData.Instance.AddSilver(def.silverAmount);
            }
            else
            {
                int cur = PlayerPrefs.GetInt("SILVER", 0);
                PlayerPrefs.SetInt("SILVER", cur + def.silverAmount);
                PlayerPrefs.Save();
            }
        }
        else if (def.rewardType == RewardType.Item)
        {
            if (string.IsNullOrEmpty(def.itemId)) return false;

            InventoryManager im = InventoryManager.Instance;
            if (im == null) im = FindObjectOfType<InventoryManager>();

            if (im != null)
            {
                im.AddItem(def.itemId, 1);
            }
            else
            {
                Debug.LogError("InventoryManager not found：无法发放道具奖励（请确认场景里有 InventoryManager 且启用）");
                return false;
            }
        }

        // 标记已领取
        SetClaimed(taskId, true);
        return true;
    }
}