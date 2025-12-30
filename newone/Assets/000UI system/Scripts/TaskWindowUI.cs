using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskWindowUI : MonoBehaviour
{
    [Header("TaskList")]
    [SerializeField] private Transform contentRoot;       // TaskList/Viewport/Content
    [SerializeField] private TaskItemRowUI rowPrefab;     // TaskItemRow Prefab

    [Header("Buttons")]
    [SerializeField] private Button btnClose;             // TaskWindow/Btn_Close

    // ====== 假数据：后面你接真实任务系统再替换 ======
    private List<TaskData> tasks = new List<TaskData>();

    private void Awake()
    {
        if (btnClose != null) btnClose.onClick.AddListener(() => gameObject.SetActive(false));
        BuildMockTasks();
    }

    private void OnEnable()
    {
        RefreshList();
    }

    private void RefreshList()
    {
        ClearContent();

        for (int i = 0; i < tasks.Count; i++)
        {
            TaskData t = tasks[i];
            TaskItemRowUI row = Instantiate(rowPrefab, contentRoot);
            row.Bind(t, OnClickClaim);
        }
    }

    private void ClearContent()
    {
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(contentRoot.GetChild(i).gameObject);
        }
    }

    private void OnClickClaim(TaskData task)
    {
        if (!task.IsCompleted) return;
        if (task.claimed) return;

        // 发放奖励：加银币
        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.AddSilver(task.rewardSilver);
        }

        task.claimed = true;

        // 刷新任务列表（按钮变成已领取）
        RefreshList();
    }

    private void BuildMockTasks()
    {
        tasks.Clear();
        tasks.Add(new TaskData("在校园放置1个家具", 1, 0, 100));    // 未完成
        tasks.Add(new TaskData("完成1次互动对话", 1, 1, 80));      // 已完成可领取
        tasks.Add(new TaskData("收取一次挂机收益", 1, 1, 60));     // 已完成可领取
        tasks.Add(new TaskData("累计获得500银币", 500, 120, 150)); // 未完成
    }
}

[System.Serializable]
public class TaskData
{
    public string title;
    public int target;
    public int current;
    public int rewardSilver;
    public bool claimed;

    public TaskData(string title, int target, int current, int rewardSilver)
    {
        this.title = title;
        this.target = target;
        this.current = current;
        this.rewardSilver = rewardSilver;
        this.claimed = false;
    }

    public bool IsCompleted => current >= target;
}
