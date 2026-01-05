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

    // =========================================================
    // ★★★ 【新增 1】 这里加一个变量，用来绑定你的登录弹窗 ★★★
    [Header("登录弹窗（把LoginPanel拖进来）")]
    public GameObject loginWindow;
    // =========================================================

    private const string KEY_PLAYER_NAME = "PLAYER_NAME";

    async void Start()
    {
        LCUser currentUser = await LCUser.GetCurrent();

        if (currentUser != null)
        {
            Debug.Log("✅ 自动登录成功，用户是：" + currentUser.Username);

            if (statusText != null)
                statusText.text = "欢迎回来，" + currentUser.Username;

            PlayerPrefs.SetString(KEY_PLAYER_NAME, currentUser.Username);
            PlayerPrefs.Save();

            if (startupUI != null)
                startupUI.SetLoggedIn(true);

            // =========================================================
            // ★★★ 【新增 2】 如果自动登录成功，也直接把弹窗关掉 ★★★
            // =========================================================
            if (loginWindow != null)
                loginWindow.SetActive(false);
        }
        else
        {
            Debug.Log("未登录，请手动输入账号密码");
            if (startupUI != null)
                startupUI.SetLoggedIn(false);
        }
    }

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

            PlayerPrefs.SetString(KEY_PLAYER_NAME, uName);
            PlayerPrefs.Save();

            if (startupUI != null) startupUI.SetLoggedIn(false);
            if (TaskService.Instance != null)
                TaskService.Instance.MarkLoginCompleted();
        }
        catch (LCException e)
        {
            if (statusText != null) statusText.text = "注册失败：" + e.Message;
        }
    }

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

            PlayerPrefs.SetString(KEY_PLAYER_NAME, user.Username);
            PlayerPrefs.Save();

            if (startupUI != null)
                startupUI.SetLoggedIn(true);

            // =========================================================
            // ★★★ 【新增 3】 登录成功后，立刻关闭弹窗！ ★★★
            // =========================================================
            if (loginWindow != null)
            {
                loginWindow.SetActive(false);
            }
        }
        catch (LCException e)
        {
            if (statusText != null) statusText.text = "登录失败：" + e.Message;

            if (startupUI != null)
                startupUI.SetLoggedIn(false);
        }
    }
}