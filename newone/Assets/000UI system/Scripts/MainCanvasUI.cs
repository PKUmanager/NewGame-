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
    [SerializeField] private Text afkAmountText; // 如果你有显示“可领取xx银币”的文本就绑，没有就留空

    [Header("右下角功能按钮")]
    [SerializeField] private Button btnBackpack;
    [SerializeField] private Button btnTask;
    [SerializeField] private Button btnSocial;
    [SerializeField] private Button btnSetting;

    [Header("窗口/面板（按你层级命名）")]
    [SerializeField] private GameObject backpackPanel; // 对应 BackpackPanel
    [SerializeField] private GameObject taskWindow;    // 对应 TaskWindow
    [SerializeField] private GameObject socialWindow; // 对应 SocialWindow
    [SerializeField] private GameObject settingsWindow;
   
    // [SerializeField] private GameObject socialWindow;
    // [SerializeField] private GameObject settingWindow;

    [Header("窗口内关闭按钮（如果你做了就绑）")]
    [SerializeField] private Button btnCloseBackpack;
    [SerializeField] private Button btnCloseTask;

    // ====== 这里先用“假数据”，你后面接真实存档/网络都行 ======
    private string nickname = "KWan";
    private float vitality = 80f; // 0-100
    private int afkSilver = 120;  // 当前可领取挂机银币

    private void Awake()
    {
        // 绑定按钮事件（也可以用Inspector绑，但小白更推荐脚本统一绑）
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
       
        if (backpackPanel != null) backpackPanel.SetActive(false);
        if (taskWindow != null) taskWindow.SetActive(false);
        if (socialWindow != null) socialWindow.SetActive(false);
        if (settingsWindow != null) settingsWindow.SetActive(false);
        RefreshTopBar();
        RefreshAfkUI();
    }

    // ====== 顶部栏刷新 ======
    private void RefreshTopBar()
    {
        if (nicknameText != null) nicknameText.text = nickname;

        if (PlayerData.Instance != null)
        {
            if (silverText != null) silverText.text = PlayerData.Instance.Silver.ToString();
            if (goldText != null) goldText.text = PlayerData.Instance.Gold.ToString();
        }

        if (vitalitySlider != null) vitalitySlider.value = vitality;
    }

    private void RefreshAfkUI()
    {
        if (afkAmountText != null) afkAmountText.text = afkSilver.ToString();
        if (btnClaimAfk != null) btnClaimAfk.interactable = afkSilver > 0;
    }

    // ====== 右下角按钮：打开窗口 ======
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

    // ====== 关闭按钮 ======
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
        // 未来 social/setting 也在这关
    }

    // ====== 挂机领取 ======
    private void ClaimAfkReward()
    {
        if (afkSilver <= 0) return;

        PlayerData.Instance.AddSilver(afkSilver);
        afkSilver = 0;
        RefreshAfkUI();
        afkSilver = 0;

        RefreshTopBar();
        RefreshAfkUI();
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