using UnityEngine;
using UnityEngine.UI;
using System;

public class TaskItemRowUI : MonoBehaviour
{
    [SerializeField] private Text txtTitle;
    [SerializeField] private Text txtProgress; // 这里我们用最简单：显示 完成/未完成
    [SerializeField] private Button btnClaim;
    [SerializeField] private Text txtClaim;

    private TaskDefinition def;

    private void OnEnable()
    {
        if (TaskService.Instance != null)
            TaskService.Instance.OnTaskChanged += RefreshUI;

        RefreshUI();
    }

    private void OnDisable()
    {
        if (TaskService.Instance != null)
            TaskService.Instance.OnTaskChanged -= RefreshUI;
    }

    public void Bind(TaskDefinition def)
    {
        this.def = def;

        if (txtTitle != null) txtTitle.text = def.title;

        if (btnClaim != null)
        {
            btnClaim.onClick.RemoveAllListeners();
            btnClaim.onClick.AddListener(OnClickClaim);
        }

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (def == null || TaskService.Instance == null) return;

        bool done = TaskService.Instance.IsDone(def.id);
        bool claimed = TaskService.Instance.IsClaimed(def.id);

        if (txtProgress != null)
            txtProgress.text = done ? "已完成" : "未完成";

        if (claimed)
        {
            if (txtClaim != null) txtClaim.text = "已领取";
            if (btnClaim != null) btnClaim.interactable = false;
            return;
        }

        if (done)
        {
            // 可领取
            if (txtClaim != null)
            {
                if (def.rewardType == RewardType.Silver)
                    txtClaim.text = $"领取+{def.silverAmount}银币";
                else
                    txtClaim.text = $"领取道具";
            }
            if (btnClaim != null) btnClaim.interactable = true;
        }
        else
        {
            // 未完成不可领
            if (txtClaim != null) txtClaim.text = "未完成";
            if (btnClaim != null) btnClaim.interactable = false;
        }
    }

    private void OnClickClaim()
    {
        if (def == null || TaskService.Instance == null) return;

        bool ok = TaskService.Instance.Claim(def.id);
        if (!ok)
        {
            RefreshUI();
            return;
        }

        // 领取成功后UI会被 OnTaskChanged 刷新，这里也可以立即刷新
        RefreshUI();
    }
}