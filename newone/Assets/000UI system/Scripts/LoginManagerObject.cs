using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputUsername;    // 用户名输入框
    [SerializeField] private TMP_InputField inputPassword;    // 密码输入框
    [SerializeField] private TMP_InputField inputCampusName;  // 校园名称输入框
    [SerializeField] private Text txtErrorMessage;        // 错误提示文本
    [SerializeField] private Button btnLogin;             // 登录按钮

    private const string USERNAME_KEY = "Username";       // 存储的用户名的key
    private const string PASSWORD_KEY = "Password";       // 存储的密码的key

    private void Start()
    {
        // 给登录按钮绑定点击事件
        btnLogin.onClick.AddListener(OnLoginButtonClick);
    }

    // 登录按钮点击时的事件处理
    private void OnLoginButtonClick()
    {
        string username = inputUsername.text;
        string password = inputPassword.text;
        string campusName = inputCampusName.text;

        // 检查用户名和密码
        if (ValidateCredentials(username, password))
        {
            // 登录成功，保存用户信息
            PlayerPrefs.SetString(USERNAME_KEY, username);
            PlayerPrefs.SetString(PASSWORD_KEY, password);
            PlayerPrefs.Save();

            // 跳转到主界面或进入游戏
            Debug.Log("Login Successful! Welcome " + username);
            // 这里可以加载新的场景，或者进行下一步操作
            // SceneManager.LoadScene("MainGameScene");
        }
        else
        {
            // 登录失败，显示错误信息
            txtErrorMessage.text = "Invalid Username or Password!";
        }
    }

    // 验证用户名和密码
    private bool ValidateCredentials(string username, string password)
    {
        // 你可以根据需求修改验证逻辑（例如访问数据库、服务器验证等）
        // 这里是一个简单的本地验证
        if (username == "admin" && password == "1234")
        {
            return true;
        }
        return false;
    }
}