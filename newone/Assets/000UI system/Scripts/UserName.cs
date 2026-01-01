using UnityEngine;
using UnityEngine.UI;

public class TopBarUI_Legacy : MonoBehaviour
{
    [SerializeField] private Text nicknameText;

    private const string KEY_PLAYER_NAME = "PLAYER_NAME";

    private void Start()
    {
        string username = PlayerPrefs.GetString(KEY_PLAYER_NAME, "Íæ¼Ò");
        nicknameText.text = username;
    }
}