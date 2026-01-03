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

    // ================= 注册 =================
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

            // ✅ 把用户名存起来（后面TopBar显示用）
            PlayerPrefs.SetString(KEY_PLAYER_NAME, uName);
            PlayerPrefs.Save();

            // ✅ 注册成功不解锁开始按钮（必须登录才解锁）
            if (startupUI != null) startupUI.SetLoggedIn(false);
        }
        catch (LCException e)
        {
            if (statusText != null) statusText.text = "注册失败：" + e.Message;
        }
    }

    // ================= 登录 =================
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

            // ✅ 保存用户名（TopBar等地方读取）
            PlayerPrefs.SetString(KEY_PLAYER_NAME, user.Username);
            PlayerPrefs.Save();

            // ✅ 只解锁开始按钮，不跳转场景
            if (startupUI != null)
                startupUI.SetLoggedIn(true);
        }
        catch (LCException e)
        {
            if (statusText != null) statusText.text = "登录失败：" + e.Message;

            // ✅ 登录失败：锁回开始按钮
            if (startupUI != null)
                startupUI.SetLoggedIn(false);
        }
    }
}