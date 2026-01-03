using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using System.Collections;

public class GPSUnlocker : MonoBehaviour
{
    // ============ 1. 核心交互对象 ============
    [Header("【步骤0: 点击发现后要隐藏的界面】")]
    public GameObject functionButtons;

    [Header("【步骤4: 最终奖励】")]
    public Button mainBuildBtn;

    // ============ 2. QuizPanel UI绑定 ============
    [Header("【QuizPanel 结构绑定】")]
    public GameObject quizPanel;

    [Header("--- 阶段1: 定位显示组 ---")]
    public GameObject locationGroup;
    public Text textScanning;
    public Text textSuccess;

    [Header("--- 阶段2: 答题交互组 ---")]
    public Text questionText;
    public Image questionImage;
    public GameObject answerInputObj;
    public GameObject submitBtnObj;
    public Text feedbackText;

    public InputField inputFieldComponent;

    // ============ 3. 地块B 配置 ============
    [Header("地块B配置")]
    public double plotB_MinLat;
    public double plotB_MaxLat;
    public double plotB_MinLon;
    public double plotB_MaxLon;
    [TextArea] public string questionB_Text;
    public Sprite questionB_Image;
    public string answerB_Correct;

    [Header("场景物体")]
    public GameObject plotObject_B;

    // 内部变量
    private bool isRunning = false;
    private const double SearchRange = 0.0002;

    // 【新增】存档的钥匙名称（用于保存和读取）
    private const string SaveKey_PlotB = "IsPlotBUnlocked";

    void Start()
    {
        // 1. 初始化UI隐藏状态
        if (quizPanel) quizPanel.SetActive(false);

        // 绑定按钮事件
        if (submitBtnObj)
        {
            Button btn = submitBtnObj.GetComponent<Button>();
            if (btn)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(OnSubmitAnswer);
            }
        }

        // ========================================================
        // 【核心修改】检查存档：之前是否已经解锁过？
        // ========================================================
        // GetInt 默认返回0，如果之前存过1，说明解锁了
        bool isUnlocked = PlayerPrefs.GetInt(SaveKey_PlotB, 0) == 1;

        if (isUnlocked)
        {
            // === 情况A：已经解锁过 ===
            Debug.Log("检测到存档：地块B已解锁，直接开放建造功能。");

            // 1. 直接点亮建造按钮
            if (mainBuildBtn) mainBuildBtn.interactable = true;

            // 2. 让地块直接变绿
            if (plotObject_B) plotObject_B.GetComponent<Renderer>().material.color = Color.green;

            // 3. 确保功能按钮显示
            if (functionButtons) functionButtons.SetActive(true);
        }
        else
        {
            // === 情况B：还没解锁 ===
            // 锁定建造按钮
            if (mainBuildBtn) mainBuildBtn.interactable = false;

            // 确保功能按钮显示
            if (functionButtons) functionButtons.SetActive(true);
        }
    }

    // 点击“发现”按钮的入口
    public void StartDiscoveryProcess()
    {
        // 【新增优化】如果已经解锁了，点击发现按钮提示一下，或者不执行
        if (PlayerPrefs.GetInt(SaveKey_PlotB, 0) == 1)
        {
            Debug.Log("已经解锁了，不需要再定位了");
            return; // 直接返回，不执行后面的隐藏UI和定位
        }

        if (functionButtons) functionButtons.SetActive(false);
        StartCoroutine(DiscoveryRoutine());
    }

    IEnumerator DiscoveryRoutine()
    {
        // ... (这一段代码完全保持不变，为了节省篇幅省略，逻辑与上一版一致) ...
        // ... 请直接使用上一版的 DiscoveryRoutine 内容 ...

        // 为了代码完整性，我简单写出关键流程：
        // 1. 初始化面板
        quizPanel.SetActive(true);
        if (locationGroup) locationGroup.SetActive(true);
        if (textScanning) textScanning.gameObject.SetActive(true);
        if (textSuccess) textSuccess.gameObject.SetActive(false);
        if (answerInputObj) answerInputObj.SetActive(false);
        if (submitBtnObj) submitBtnObj.SetActive(false);
        if (questionText) questionText.gameObject.SetActive(false);
        if (questionImage) questionImage.gameObject.SetActive(false);
        if (feedbackText) feedbackText.text = "";

        // 2. 启动GPS
        if (!isRunning)
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                Permission.RequestUserPermission(Permission.FineLocation);
                yield return new WaitForSeconds(1);
            }
            Input.location.Start(5f, 5f);
            int waitSeconds = 15;
            while (Input.location.status == LocationServiceStatus.Initializing && waitSeconds > 0)
            {
                if (textScanning) textScanning.text = $"卫星连接中... ({waitSeconds})";
                yield return new WaitForSeconds(1); waitSeconds--;
            }
            if (waitSeconds < 1 || Input.location.status == LocationServiceStatus.Failed)
            {
                if (textScanning) textScanning.text = "GPS信号获取失败";
                yield return new WaitForSeconds(2);
                quizPanel.SetActive(false);
                if (functionButtons) functionButtons.SetActive(true);
                yield break;
            }
            isRunning = true;
        }

        // 3. 扫描
        if (textScanning) textScanning.text = "正在搜寻附近地块的信号...";
        yield return new WaitForSeconds(1.5f);

        // 4. 判定
        double curLat = Input.location.lastData.latitude;
        double curLon = Input.location.lastData.longitude;
        bool nearB = IsInsideRect(curLat, curLon, plotB_MinLat, plotB_MaxLat, plotB_MinLon, plotB_MaxLon, SearchRange);

        if (nearB)
        {
            if (textScanning) textScanning.gameObject.SetActive(false);
            if (textSuccess) textSuccess.gameObject.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            if (locationGroup) locationGroup.SetActive(false);
            ShowQuiz(questionB_Text, questionB_Image);
        }
        else
        {
            float dist = CalculateDistance(curLat, curLon, (plotB_MinLat + plotB_MaxLat) / 2, (plotB_MinLon + plotB_MaxLon) / 2);
            if (textScanning) textScanning.text = $"未在区域内\n距离目标还有: {dist:F0}米";
            yield return new WaitForSeconds(3f);
            quizPanel.SetActive(false);
            if (functionButtons) functionButtons.SetActive(true);
        }
    }

    void ShowQuiz(string qText, Sprite qImg)
    {
        if (questionText) { questionText.gameObject.SetActive(true); questionText.text = qText; }
        if (questionImage) { questionImage.sprite = qImg; questionImage.gameObject.SetActive(qImg != null); }
        if (answerInputObj) answerInputObj.SetActive(true);
        if (submitBtnObj) submitBtnObj.SetActive(true);
        if (inputFieldComponent) inputFieldComponent.text = "";
    }

    void OnSubmitAnswer()
    {
        if (inputFieldComponent == null) return;
        string playerInput = inputFieldComponent.text.Trim();

        if (playerInput == answerB_Correct)
        {
            // === 答对逻辑 ===

            // 1. 变绿
            if (plotObject_B) plotObject_B.GetComponent<Renderer>().material.color = Color.green;

            // 2. 关面板
            quizPanel.SetActive(false);

            // 3. 激活按钮
            if (mainBuildBtn) mainBuildBtn.interactable = true;

            // 4. 恢复主UI
            if (functionButtons) functionButtons.SetActive(true);

            // 5. 【核心修改】保存数据！记录“已解锁”
            // "SaveKey_PlotB" 是钥匙，1 代表 true
            PlayerPrefs.SetInt(SaveKey_PlotB, 1);
            PlayerPrefs.Save(); // 强制立即保存到硬盘

            Debug.Log("答题正确，已存档！");
        }
        else
        {
            if (feedbackText) feedbackText.text = "答案不正确！请再次尝试";
            inputFieldComponent.text = "";
        }
    }

    // 辅助函数保持不变
    bool IsInsideRect(double curLat, double curLon, double minLat, double maxLat, double minLon, double maxLon, double padding)
    {
        return (curLat >= minLat - padding) && (curLat <= maxLat + padding) &&
               (curLon >= minLon - padding) && (curLon <= maxLon + padding);
    }

    float CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var R = 6371e3; var rad = Mathf.Deg2Rad;
        var dLat = (lat2 - lat1) * rad; var dLon = (lon2 - lon1) * rad;
        var a = Mathf.Sin((float)dLat / 2) * Mathf.Sin((float)dLat / 2) +
                Mathf.Cos((float)(lat1 * rad)) * Mathf.Cos((float)(lat2 * rad)) *
                Mathf.Sin((float)dLon / 2) * Mathf.Sin((float)dLon / 2);
        var c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        return (float)(R * c);
    }

    // 【开发测试用】如果你想重置存档（比如为了测试），可以添加这个方法
    // 在Unity编辑器运行时，右键点击脚本组件标题，选择 "Reset Save Data" 即可
    [ContextMenu("Reset Save Data")]
    public void ResetSaveData()
    {
        PlayerPrefs.DeleteKey(SaveKey_PlotB);
        Debug.Log("存档已清除，回到未解锁状态");
    }
}