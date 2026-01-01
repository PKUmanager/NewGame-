using LeanCloud;
using LeanCloud.Storage;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RegisterUI : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private TMP_InputField inputUsername;

    [Header("Buttons")]
    [SerializeField] private Button btnRegister;

    private const string KEY_PLAYER_NAME = "PLAYER_NAME";

    private void Awake()
    {
        if (btnRegister != null)
            btnRegister.onClick.AddListener(OnClickRegister);
    }

    private void OnClickRegister()
    {
        string username = inputUsername != null ? inputUsername.text.Trim() : "";
        if (string.IsNullOrEmpty(username)) username = "玩家";

        PlayerPrefs.SetString(KEY_PLAYER_NAME, username);
        PlayerPrefs.Save();

        // 这里写你原本注册UI要做的事（不影响 AuthManager）
    }
} // ✅ 注意：RegisterUI 到这里必须结束（这一行非常关键）

public class AuthManager : MonoBehaviour
{
    [Header("把UI拖出去")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text statusText;

    private const string KEY_PLAYER_NAME = "PLAYER_NAME";

    public async void OnRegisterClick()
    {
        string uName = usernameInput.text.Trim();
        string pwd = passwordInput.text;

        if (string.IsNullOrEmpty(uName) || string.IsNullOrEmpty(pwd))
        {
            statusText.text = "用户名和密码不能为空！";
            return;
        }

        LCUser user = new LCUser();
        user.Username = uName;
        user.Password = pwd;

        statusText.text = "正在注册...";

        try
        {
            await user.SignUp();
            statusText.text = "注册成功！请登录";
        }
        catch (LCException e)
        {
            statusText.text = "注册失败：" + e.Message;
        }
    }

    public async void OnLoginClick()
    {
        try
        {
            LCUser user = await LCUser.Login(usernameInput.text, passwordInput.text);
            statusText.text = "登录成功！欢迎 " + user.Username;
            Invoke(nameof(EnterGame), 1.0f);
        }
        catch (LCException e)
        {
            statusText.text = "登录失败：" + e.Message;
        }
    }

    private void EnterGame()
    {
        SceneManager.LoadScene("SampleScene");
    }
} // ✅ AuthManager 也必须结束