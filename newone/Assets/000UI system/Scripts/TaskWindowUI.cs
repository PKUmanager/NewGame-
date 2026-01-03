using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TaskWindowUI : MonoBehaviour
{
    [Header("TaskList")]
    [SerializeField] private Transform contentRoot;   // TaskList/Viewport/Content
    [SerializeField] private TaskItemRowUI rowPrefab; // TaskItemRowUI Prefab

    [Header("Buttons")]
    [SerializeField] private Button btnClose;

    private readonly List<TaskItemRowUI> spawned = new List<TaskItemRowUI>();

    private void Awake()
    {
        if (btnClose != null)
            btnClose.onClick.AddListener(() => gameObject.SetActive(false));
    }

    private void OnEnable()
    {
        if (TaskService.Instance != null)
            TaskService.Instance.OnTaskChanged += RefreshList;

        RefreshList();
    }

    private void OnDisable()
    {
        if (TaskService.Instance != null)
            TaskService.Instance.OnTaskChanged -= RefreshList;
    }

    private void RefreshList()
    {
        ClearContent();

        if (TaskService.Instance == null) return;

        List<TaskDefinition> tasks = TaskService.Instance.GetAllTasks();
        foreach (var def in tasks)
        {
            TaskItemRowUI row = Instantiate(rowPrefab, contentRoot);
            row.Bind(def);
            spawned.Add(row);
        }
    }

    private void ClearContent()
    {
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(contentRoot.GetChild(i).gameObject);
        }
        spawned.Clear();
    }
}