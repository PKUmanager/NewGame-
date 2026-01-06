using UnityEngine;
using UnityEngine.UI;

public class TopBarUI_Legacy : MonoBehaviour
{
    [SerializeField] private Text nicknameText;

    private const string KEY_PLAYER_NAME = "PLAYER_NAME";

    private void Start()
    {
        string username = PlayerPrefs.GetString(KEY_PLAYER_NAME, "玩家");
        nicknameText.text = username;
    }
    // ★★★ 【新增】 公开的方法，让外部脚本调用修改名字 ★★★
    public void UpdateName(string newName)
    {
        if (nicknameText != null)
        {
            nicknameText.text = newName;
        }
    }
}