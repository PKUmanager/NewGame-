using UnityEngine;

public class UIManager : MonoBehaviour
{
    // 所有界面
    public GameObject mainUI;
    public GameObject backpackUI;
    public GameObject taskUI;

    // 当前打开的界面
    private GameObject currentPanel;

    void Start()
    {
        // 初始只显示主界面
        ShowMainUI();
    }

    public void ShowMainUI()
    {
        // 隐藏所有界面
        HideAllPanels();

        // 显示主界面
        mainUI.SetActive(true);
        currentPanel = mainUI;
    }

    public void ShowBackpack()
    {
        // 隐藏当前界面，显示背包
        if (currentPanel != null)
            currentPanel.SetActive(false);

        backpackUI.SetActive(true);
        currentPanel = backpackUI;
    }

    public void ShowTask()
    {
        if (currentPanel != null)
            currentPanel.SetActive(false);

        taskUI.SetActive(true);
        currentPanel = taskUI;
    }

    void HideAllPanels()
    {
        mainUI.SetActive(false);
        backpackUI.SetActive(false);
        taskUI.SetActive(false);
    }

    // 返回按钮通用方法
    public void OnBackButton()
    {
        ShowMainUI();
    }
}