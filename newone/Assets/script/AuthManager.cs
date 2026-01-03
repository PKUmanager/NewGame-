using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using LeanCloud;
using LeanCloud.Storage;

public class AuthManager : MonoBehaviour
{
    [Header("把UI拖进来")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text statusText;

    [Header("解锁开始按钮用（把StartupUI拖进来）")]
    public StartupUI startupUI;

    // 你之前一直用的Key（保持一致）
    private const string KEY_PLAYER_NAME = "PLAYER_NAME";

    // =========================================================
    // ★★★ 【新增】在这里插入 Start 方法 ★★★
    // 游戏一启动，它就会自动运行，检查有没有老用户
    // =========================================================
    async void Start()
    {
        // 1. 问问 LeanCloud：我现在有缓存的登录状态吗？
        LCUser currentUser = await LCUser.GetCurrent();

        if (currentUser != null)
        {
            // === 情况 A：有！(自动登录成功) ===
            Debug.Log("✅ 自动登录成功，用户是：" + currentUser.Username);

            if (statusText != null)
                statusText.text = "欢迎回来，" + currentUser.Username;

            // 2. 确保本地也存了名字 (防止之前被清过)
            PlayerPrefs.SetString(KEY_PLAYER_NAME, currentUser.Username);
            PlayerPrefs.Save();

            // 3. 直接解锁“开始游戏”按钮！
            if (startupUI != null)
                startupUI.SetLoggedIn(true);
        }
        else
        {
            // === 情况 B：没有 (需要手动登录) ===
            Debug.Log("未登录，请手动输入账号密码");

            // 锁住“开始游戏”按钮，等待玩家操作
            if (startupUI != null)
                startupUI.SetLoggedIn(false);
        }
    }
    // =========================================================


    // ================= 注册 (下面的代码不用动) =================
    public async void OnRegisterClick()
    {
        string uName = usernameInput != null ? usernameInput.text.Trim() : "";
        string pwd = passwordInput != null ? passwordInput.text : "";

        if (string.IsNullOrEmpty(uName) || string.IsNullOrEmpty(pwd))
        {
            if (statusText != null) statusText.text = "用户名和密码不能为空！";
            return;
        }

        LCUser user = new LCUser();
        user.Username = uName;
        user.Password = pwd;

        if (statusText != null) statusText.text = "正在注册...";

        try
        {
            await user.SignUp();

            if (statusText != null) statusText.text = "注册成功！请登录";

            // ✅ 把用户名存起来
            PlayerPrefs.SetString(KEY_PLAYER_NAME, uName);
            PlayerPrefs.Save();

            // 注册成功通常还需要手动点一下登录，或者你自己决定是否直接解锁
            if (startupUI != null) startupUI.SetLoggedIn(false);
            if (TaskService.Instance != null)
                TaskService.Instance.MarkLoginCompleted();
        }
        catch (LCException e)
        {
            if (statusText != null) statusText.text = "注册失败：" + e.Message;
        }
    }

    // ================= 登录 (下面的代码不用动) =================
    public async void OnLoginClick()
    {
        string uName = usernameInput != null ? usernameInput.text.Trim() : "";
        string pwd = passwordInput != null ? passwordInput.text : "";

        if (string.IsNullOrEmpty(uName) || string.IsNullOrEmpty(pwd))
        {
            if (statusText != null) statusText.text = "用户名和密码不能为空！";
            return;
        }

        if (statusText != null) statusText.text = "正在登录...";

        try
        {
            LCUser user = await LCUser.Login(uName, pwd);

            if (statusText != null) statusText.text = "登录成功！欢迎 " + user.Username;

            // ✅ 保存用户名
            PlayerPrefs.SetString(KEY_PLAYER_NAME, user.Username);
            PlayerPrefs.Save();

            // ✅ 解锁开始按钮
            if (startupUI != null)
                startupUI.SetLoggedIn(true);
        }
        catch (LCException e)
        {
            if (statusText != null) statusText.text = "登录失败：" + e.Message;

            if (startupUI != null)
                startupUI.SetLoggedIn(false);
        }
    }
}