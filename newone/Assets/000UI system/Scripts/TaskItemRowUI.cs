using System;
using UnityEngine;
using UnityEngine.UI;

public class TaskItemRowUI : MonoBehaviour
{
    [SerializeField] private Text txtTitle;
    [SerializeField] private Text txtProgress;
    [SerializeField] private Button btnClaim;
    [SerializeField] private Text txtClaim;

    private TaskData data;
    private Action<TaskData> onClickClaim;

    public void Bind(TaskData data, Action<TaskData> onClickClaim)
    {
        this.data = data;
        this.onClickClaim = onClickClaim;

        if (txtTitle != null) txtTitle.text = data.title;
        if (txtProgress != null) txtProgress.text = $"{data.current}/{data.target}";

        // 按钮状态
        if (btnClaim != null)
        {
            btnClaim.onClick.RemoveAllListeners();
            btnClaim.onClick.AddListener(() => this.onClickClaim?.Invoke(this.data));
        }

        RefreshClaimState();
    }

    private void RefreshClaimState()
    {
        if (data == null) return;

        if (data.claimed)
        {
            if (txtClaim != null) txtClaim.text = "已领取";
            if (btnClaim != null) btnClaim.interactable = false;
            return;
        }

        if (data.IsCompleted)
        {
            if (txtClaim != null) txtClaim.text = $"领取+{data.rewardSilver}";
            if (btnClaim != null) btnClaim.interactable = true;
        }
        else
        {
            if (txtClaim != null) txtClaim.text = "未完成";
            if (btnClaim != null) btnClaim.interactable = false;
        }
    }
}
