using LeanCloud;
using LeanCloud.Storage; // 必须引用
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;  // 【新增】这一行必须加！

public class AuthManager : MonoBehaviour
{
    [Header("把UI拖进去")]
    // 注意：前面加了 TMP_
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;

    // 注意：这里改成 TMP_Text，这样旧版Text和新版Text都能拖
    public TMP_Text statusText;

    // --- 注册功能 ---
    public async void OnRegisterClick()
    {
        LCUser user = new LCUser();
        user.Username = usernameInput.text;
        user.Password = passwordInput.text;

        try
        {
            await user.SignUp(); // 发送到云端
            statusText.text = "注册成功！请登录";
            Debug.Log("注册成功");
        }
        catch (LCException e)
        {
            statusText.text = "注册失败: " + e.Message;
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