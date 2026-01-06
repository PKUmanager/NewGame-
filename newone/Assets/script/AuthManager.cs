using LeanCloud;
using LeanCloud.Storage;
using TMPro;
using UnityEngine;
using System.Threading.Tasks; // 【新增】必须有这个才能用 Delay

public class AuthManager : MonoBehaviour
{
    [Header("把UI拖进来")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text statusText;

    [Header("解锁开始按钮用（把StartupUI拖进来）")]
    public StartupUI startupUI;

    [Header("登录弹窗（把LoginPanel拖进来）")]
    public GameObject loginWindow;

    private const string KEY_PLAYER_NAME = "PLAYER_NAME";

    async void Start()
    {
        LCUser currentUser = await LCUser.GetCurrent();

        if (currentUser != null)
        {
            // 自动登录成功
            Debug.Log("☑ 自动登录成功，用户是：" + currentUser.Username);

            if (statusText != null)
                statusText.text = "欢迎回来，" + currentUser.Username;

            PlayerPrefs.SetString(KEY_PLAYER_NAME, currentUser.Username);
            PlayerPrefs.Save();

            if (startupUI != null)
                startupUI.SetLoggedIn(true);

            // 自动登录也要算“登录完成”
            MarkLoginTaskDoneSafe();

            // ★★★ 【修改】 延迟 1 秒后关闭窗口，让字显示一会儿 ★★★
            await Task.Delay(1000);

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

        if (statusText != null) statusText.text = "正在注册...";

        try
        {
            LCUser user = new LCUser();
            user.Username = uName;
            user.Password = pwd;

            await user.SignUp();

            if (statusText != null) statusText.text = "注册成功！请登录";

            PlayerPrefs.SetString(KEY_PLAYER_NAME, uName);
            PlayerPrefs.Save();

            if (startupUI != null)
                startupUI.SetLoggedIn(false);
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

            MarkLoginTaskDoneSafe();

            // ★★★ 【修改】 延迟 1.5 秒后关闭窗口，让玩家看清提示 ★★★
            await Task.Delay(1500);

            if (loginWindow != null)
                loginWindow.SetActive(false);
        }
        catch (LCException e)
        {
            if (statusText != null) statusText.text = "登录失败：" + e.Message;
            if (startupUI != null) startupUI.SetLoggedIn(false);
        }
    }

    private void MarkLoginTaskDoneSafe()
    {
        TaskService ts = TaskService.Instance;
        if (ts == null) ts = FindObjectOfType<TaskService>();

        if (ts != null)
            ts.MarkLoginCompleted();
        else
            Debug.LogError("TaskService not found：无法标记登录任务");
    }
}