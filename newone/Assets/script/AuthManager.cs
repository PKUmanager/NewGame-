using LeanCloud;
using LeanCloud.Storage; // 必须引用
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;  // 【新增】这一行必须加！



public class RegisterUI : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private TMP_InputField inputCampusName;

    [Header("Buttons")]
    [SerializeField] private Button btnRegister;

    private const string KEY_CAMPUS_NAME = "CAMPUS_NAME";

    private void Awake()
    {
        if (btnRegister != null)
            btnRegister.onClick.AddListener(OnClickRegister);
    }

    private void OnClickRegister()
    {
        string campusName = inputCampusName != null ? inputCampusName.text.Trim() : "";
        if (string.IsNullOrEmpty(campusName))
            campusName = "玩家"; // 你想要的默认名

        PlayerPrefs.SetString(KEY_CAMPUS_NAME, campusName);
        PlayerPrefs.Save();

        // 这里继续你原本的注册逻辑（切场景/关面板等）
    }
}
public class AuthManager : MonoBehaviour
{
    [Header("把UI拖进去")]
    // 注意：前面加了 TMP_
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;

    // 注意：这里改成 TMP_Text，这样旧版Text和新版Text都能拖
    public TMP_Text statusText;


    // --- 注册功能 (升级版) ---
    public async void OnRegisterClick()
    {
        string uName = usernameInput.text;
        string pwd = passwordInput.text;

        // 1. 基本检查：防止空账号
        if (string.IsNullOrEmpty(uName) || string.IsNullOrEmpty(pwd))
        {
            statusText.text = "账号或密码不能为空！";
            return;
        }

        LCUser user = new LCUser();
        user.Username = uName;
        user.Password = pwd;

        statusText.text = "正在注册..."; // 给个反馈

        try
        {
            // 2. 尝试注册
            await user.SignUp();

            statusText.text = "注册成功！请登录";
            Debug.Log("注册成功");
        }
        catch (LCException e)
        {
            // 3. 【核心】专门处理用户名重复
            if (e.Code == 202)
            {
                statusText.text = "注册失败：用户名 '" + uName + "' 已被占用，请换一个！";
                Debug.LogWarning("用户名重复");
            }
            else
            {
                // 其他错误（比如断网了）
                statusText.text = "注册失败: " + e.Message;
                Debug.LogError("注册报错: " + e.Code);
            }
        }
    }

    // --- 登录功能 ---
    public async void OnLoginClick()
    {
        try
        {
            // 1. 获取登录返回的用户对象 'user'
            LCUser user = await LCUser.Login(usernameInput.text, passwordInput.text);

            // 2. 【修复】直接使用 'user.Username'，而不是 LCUser.CurrentUser
            statusText.text = "登录成功！欢迎 " + user.Username;

            // 2. 【修改2】跳转场景代码
            // 这里的 "GameScene" 必须改成你真正游戏场景的名字（比如 "Main" 或 "Level1"）
            // 稍微延迟 1 秒再跳，让玩家看清“登录成功”这几个字（可选）
            Invoke("EnterGame", 1.0f);
        }
        catch (LCException e)
        {
            statusText.text = "登录失败: " + e.Message;
        }
    }

    // 专门写个函数用来跳场景，方便 Invoke 调用
    void EnterGame()
    {
        // ！！！注意：把引号里的名字改成你实际的游戏场景名！！！
        SceneManager.LoadScene("SampleScene");
    }
}