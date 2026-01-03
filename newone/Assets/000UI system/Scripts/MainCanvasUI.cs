using System;
using UnityEngine;
using UnityEngine.UI;

public class MainCanvasUI : MonoBehaviour
{
    [Header("TopBar - 基础信息")]
    [SerializeField] private Text nicknameText;
    [SerializeField] private Text silverText;
    [SerializeField] private Text goldText;
    [SerializeField] private Slider vitalitySlider;

    [Header("TopBar - 挂机收益")]
    [SerializeField] private Button btnClaimAfk;
    [SerializeField] private Text afkAmountText; // 显示“可领取xx银币”（可为空）

    [Header("右下角功能按钮")]
    [SerializeField] private Button btnBackpack;
    [SerializeField] private Button btnTask;
    [SerializeField] private Button btnSocial;
    [SerializeField] private Button btnSetting;

    [Header("窗口/面板（按层级命名）")]
    [SerializeField] private GameObject backpackPanel;
    [SerializeField] private GameObject taskWindow;
    [SerializeField] private GameObject socialWindow;
    [SerializeField] private GameObject settingsWindow;

    [Header("窗口内关闭按钮（如果你做了就绑）")]
    [SerializeField] private Button btnCloseBackpack;
    [SerializeField] private Button btnCloseTask;

    // ===== 用户名：从注册存的 PlayerPrefs 里拿 =====
    private const string KEY_PLAYER_NAME = "PLAYER_NAME";

    // ===== 领取规则：每 10 分钟 80 银币 =====
    private const int REWARD_AMOUNT = 80;
    private const int REWARD_INTERVAL_SECONDS = 10 * 60; // 600秒

    // 用 PlayerPrefs 持久化（关闭游戏再打开也能继续算）
    private const string KEY_LAST_REWARD_TIME = "AFK_LAST_REWARD_TIME_UTC";   // long unix seconds
    private const string KEY_PENDING_REWARD = "AFK_PENDING_REWARD";          // int

    private float vitality = 80f; // 你原来的逻辑：0-100（先保留）
    private int pendingReward = 0; // 当前可领取银币

    // 用来减少每帧计算（1秒刷新一次）
    private float nextTick = 0f;

    private void Awake()
    {
        // 绑定按钮事件（你也可以在 Inspector 绑，这里保留你原风格）
        if (btnBackpack != null) btnBackpack.onClick.AddListener(OpenBackpack);
        if (btnTask != null) btnTask.onClick.AddListener(OpenTask);
        if (btnSocial != null) btnSocial.onClick.AddListener(OpenSocial);
        if (btnSetting != null) btnSetting.onClick.AddListener(OpenSetting);

        if (btnCloseBackpack != null) btnCloseBackpack.onClick.AddListener(CloseBackpack);
        if (btnCloseTask != null) btnCloseTask.onClick.AddListener(CloseTask);

        if (btnClaimAfk != null) btnClaimAfk.onClick.AddListener(ClaimAfkReward);
    }

    private void Start()
    {
        // 初始隐藏窗口
        if (backpackPanel != null) backpackPanel.SetActive(false);
        if (taskWindow != null) taskWindow.SetActive(false);
        if (socialWindow != null) socialWindow.SetActive(false);
        if (settingsWindow != null) settingsWindow.SetActive(false);

        // 初始化挂机收益（会根据时间自动算“可领取多少”）
        InitAfkReward();

        RefreshTopBar();
        RefreshAfkUI();
    }

    private void Update()
    {
        // 每秒刷新一次（不然每帧算也行，但没必要）
        if (Time.unscaledTime < nextTick) return;
        nextTick = Time.unscaledTime + 1f;

        UpdatePendingRewardByTime();
        RefreshAfkUI();
    }

    // =================== 顶部栏刷新 ===================
    private void RefreshTopBar()
    {
        // ✅ 不再用写死 nickname，而是从 PlayerPrefs 读
        string nickname = PlayerPrefs.GetString(KEY_PLAYER_NAME, "玩家");
        if (nicknameText != null) nicknameText.text = nickname;

        if (PlayerData.Instance != null)
        {
            if (silverText != null) silverText.text = PlayerData.Instance.Silver.ToString();
            if (goldText != null) goldText.text = PlayerData.Instance.Gold.ToString();
        }

        if (vitalitySlider != null) vitalitySlider.value = vitality;
    }
   
    // =================== 挂机收益逻辑 ===================
    private static long UtcNowUnix()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    private void InitAfkReward()
    {
        // 读取“上次计算时间”
        long last = 0;
        if (PlayerPrefs.HasKey(KEY_LAST_REWARD_TIME))
        {
            // 注意：PlayerPrefs 没有 long，咱用 string 存
            long.TryParse(PlayerPrefs.GetString(KEY_LAST_REWARD_TIME, "0"), out last);
        }
        else
        {
            // 第一次进入：把当前时间写进去，避免一进来就给一堆奖励
            last = UtcNowUnix();
            PlayerPrefs.SetString(KEY_LAST_REWARD_TIME, last.ToString());
        }

        // 读取已经累计但未领取的奖励
        pendingReward = PlayerPrefs.GetInt(KEY_PENDING_REWARD, 0);

        // 启动时先根据时间补算一次
        UpdatePendingRewardByTime();
        SaveAfkState();
    }

    private void UpdatePendingRewardByTime()
    {
        long last;
        long.TryParse(PlayerPrefs.GetString(KEY_LAST_REWARD_TIME, "0"), out last);

        long now = UtcNowUnix();
        if (last <= 0) last = now;

        long elapsed = now - last;
        if (elapsed < REWARD_INTERVAL_SECONDS) return;

        long times = elapsed / REWARD_INTERVAL_SECONDS; // 过去了几个“10分钟”
        int add = (int)times * REWARD_AMOUNT;

        pendingReward += add;

        // 推进 last 到最近一次“结算点”
        long newLast = last + times * REWARD_INTERVAL_SECONDS;
        PlayerPrefs.SetString(KEY_LAST_REWARD_TIME, newLast.ToString());

        SaveAfkState();
    }

    private void SaveAfkState()
    {
        PlayerPrefs.SetInt(KEY_PENDING_REWARD, pendingReward);
        PlayerPrefs.Save();
    }

    private void RefreshAfkUI()
    {
        if (afkAmountText != null)
        {
            // 你可以改成“可领取 80”或“+80”
            afkAmountText.text = pendingReward > 0 ? $"可领取 {pendingReward}" : "";
        }

        if (btnClaimAfk != null)
        {
            btnClaimAfk.interactable = pendingReward > 0;
        }
    }

    private void ClaimAfkReward()
    {
        // 先补算一次，避免刚好够10分钟但没刷新到
        UpdatePendingRewardByTime();

        if (pendingReward <= 0) return;

        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.AddSilver(pendingReward);
        }

        pendingReward = 0;
        SaveAfkState();

        RefreshTopBar();
        RefreshAfkUI();
    }

    // =================== 打开/关闭窗口 ===================
    public void OpenBackpack()
    {
        CloseAllWindows();
        if (backpackPanel != null) backpackPanel.SetActive(true);
    }

    public void OpenTask()
    {
        CloseAllWindows();
        if (taskWindow != null) taskWindow.SetActive(true);
    }

    public void OpenSocial()
    {
        CloseAllWindows();
        if (socialWindow != null) socialWindow.SetActive(true);
    }

    public void OpenSetting()
    {
        CloseAllWindows();
        if (settingsWindow != null) settingsWindow.SetActive(true);
    }

    public void CloseBackpack()
    {
        if (backpackPanel != null) backpackPanel.SetActive(false);
    }

    public void CloseTask()
    {
        if (taskWindow != null) taskWindow.SetActive(false);
    }

    private void CloseAllWindows()
    {
        if (backpackPanel != null) backpackPanel.SetActive(false);
        if (taskWindow != null) taskWindow.SetActive(false);
        if (socialWindow != null) socialWindow.SetActive(false);
        if (settingsWindow != null) settingsWindow.SetActive(false);
    }

    private void OnEnable()
    {
        if (PlayerData.Instance != null)
            PlayerData.Instance.OnCurrencyChanged += RefreshTopBar;
    }

    private void OnDisable()
    {
        if (PlayerData.Instance != null)
            PlayerData.Instance.OnCurrencyChanged -= RefreshTopBar;
    }
}