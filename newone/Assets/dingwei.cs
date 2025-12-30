using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using System.Collections;

public class GPSUnlocker : MonoBehaviour
{
    // ============ 1. 核心交互对象 ============
    [Header("【步骤0: 点击发现后要隐藏的界面】")]
    public GameObject functionButtons; // 主界面那一排按钮

    [Header("【步骤4: 最终奖励】")]
    public Button mainBuildBtn;        // 建造按钮

    // ============ 2. QuizPanel UI绑定 ============
    [Header("【QuizPanel 结构绑定】")]
    public GameObject quizPanel;          // 总面板

    [Header("--- 阶段1: 定位显示组 ---")]
    public GameObject locationGroup;      // Location父物体
    public Text textScanning;             // "Text_Location"
    public Text textSuccess;              // "Text_Location success"

    [Header("--- 阶段2: 答题交互组 ---")]
    public Text questionText;             // 题目文字
    public Image questionImage;           // 题目图片
    public GameObject answerInputObj;     // 输入框物体
    public GameObject submitBtnObj;       // 提交按钮物体
    public Text feedbackText;             // 错误提示

    public InputField inputFieldComponent; // 输入框组件

    // ============ 3. 地块B 配置 (只保留B) ============
    [Header("地块B配置 (唯一目标)")]
    public double plotB_MinLat;
    public double plotB_MaxLat;
    public double plotB_MinLon;
    public double plotB_MaxLon;
    [TextArea] public string questionB_Text;
    public Sprite questionB_Image;
    public string answerB_Correct;

    [Header("场景物体")]
    public GameObject plotObject_B; // 场景里的3D方块 B

    // 内部变量
    private bool isRunning = false;

    // 【判定范围】 0.0002度 ≈ 20-22米
    private const double SearchRange = 0.0002;

    void Start()
    {
        // 初始化状态
        if (mainBuildBtn) mainBuildBtn.interactable = false;
        if (quizPanel) quizPanel.SetActive(false);
        if (functionButtons) functionButtons.SetActive(true);

        // 绑定按钮
        if (submitBtnObj)
        {
            Button btn = submitBtnObj.GetComponent<Button>();
            if (btn)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(OnSubmitAnswer);
            }
        }
    }

    // 点击“发现”按钮的入口
    public void StartDiscoveryProcess()
    {
        if (functionButtons) functionButtons.SetActive(false);
        StartCoroutine(DiscoveryRoutine());
    }

    IEnumerator DiscoveryRoutine()
    {
        // === 1. 初始化UI ===
        quizPanel.SetActive(true);
        if (locationGroup) locationGroup.SetActive(true);
        if (textScanning) textScanning.gameObject.SetActive(true);
        if (textSuccess) textSuccess.gameObject.SetActive(false);

        // 隐藏答题部分
        if (answerInputObj) answerInputObj.SetActive(false);
        if (submitBtnObj) submitBtnObj.SetActive(false);
        if (questionText) questionText.gameObject.SetActive(false);
        if (questionImage) questionImage.gameObject.SetActive(false);
        if (feedbackText) feedbackText.text = "";

        // === 2. 启动GPS ===
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
                if (textScanning) textScanning.text = $"正在为您定位... ({waitSeconds})";
                yield return new WaitForSeconds(1);
                waitSeconds--;
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

        // === 3. 扫描动画 ===
        if (textScanning) textScanning.text = "正在搜寻地块附近的信号...";
        yield return new WaitForSeconds(1.5f);

        // === 4. 获取坐标并判定 (只看B) ===
        double curLat = Input.location.lastData.latitude;
        double curLon = Input.location.lastData.longitude;

        // 【关键逻辑】使用 0.0002 (20米) 判定是否在地块 B 范围内
        bool nearB = IsInsideRect(curLat, curLon, plotB_MinLat, plotB_MaxLat, plotB_MinLon, plotB_MaxLon, SearchRange);

        if (nearB)
        {
            // ---> 成功 <---
            if (textScanning) textScanning.gameObject.SetActive(false);
            if (textSuccess) textSuccess.gameObject.SetActive(true);

            yield return new WaitForSeconds(1.5f); // 停留展示成功

            if (locationGroup) locationGroup.SetActive(false);

            // 显示 B 的题目
            ShowQuiz(questionB_Text, questionB_Image);
        }
        else
        {
            // ---> 失败 <---
            // 计算距离B中心的距离
            float dist = CalculateDistance(curLat, curLon, (plotB_MinLat + plotB_MaxLat) / 2, (plotB_MinLon + plotB_MaxLon) / 2);

            if (textScanning) textScanning.text = $"未在区域内\n距离目标还有: {dist:F0}米";
            yield return new WaitForSeconds(3f);

            quizPanel.SetActive(false);
            if (functionButtons) functionButtons.SetActive(true);
        }
    }

    void ShowQuiz(string qText, Sprite qImg)
    {
        if (questionText)
        {
            questionText.gameObject.SetActive(true);
            questionText.text = qText;
        }
        if (questionImage)
        {
            questionImage.sprite = qImg;
            questionImage.gameObject.SetActive(qImg != null);
        }

        if (answerInputObj) answerInputObj.SetActive(true);
        if (submitBtnObj) submitBtnObj.SetActive(true);
        if (inputFieldComponent) inputFieldComponent.text = "";
    }

    void OnSubmitAnswer()
    {
        if (inputFieldComponent == null) return;
        string playerInput = inputFieldComponent.text.Trim();

        // 直接比对 B 的答案
        if (playerInput == answerB_Correct)
        {
            // 答对：变绿、关面板、开建造、回主页
            if (plotObject_B) plotObject_B.GetComponent<Renderer>().material.color = Color.green;
            quizPanel.SetActive(false);

            if (mainBuildBtn) mainBuildBtn.interactable = true;
            if (functionButtons) functionButtons.SetActive(true);
        }
        else
        {
            if (feedbackText) feedbackText.text = "答案不正确";
            inputFieldComponent.text = "";
        }
    }

    // 保持你的定位方法不变
    bool IsInsideRect(double curLat, double curLon, double minLat, double maxLat, double minLon, double maxLon, double padding)
    {
        return (curLat >= minLat - padding) && (curLat <= maxLat + padding) &&
               (curLon >= minLon - padding) && (curLon <= maxLon + padding);
    }

    // 仅用于失败时计算显示距离
    float CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var R = 6371e3;
        var rad = Mathf.Deg2Rad;
        var dLat = (lat2 - lat1) * rad;
        var dLon = (lon2 - lon1) * rad;
        var a = Mathf.Sin((float)dLat / 2) * Mathf.Sin((float)dLat / 2) +
                Mathf.Cos((float)(lat1 * rad)) * Mathf.Cos((float)(lat2 * rad)) *
                Mathf.Sin((float)dLon / 2) * Mathf.Sin((float)dLon / 2);
        var c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        return (float)(R * c);
    }
}