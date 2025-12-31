using UnityEngine;
using UnityEngine.UI;

public class TopBarUI_Legacy : MonoBehaviour
{
    [SerializeField] private Text nicknameText;

    private const string KEY_CAMPUS_NAME = "CAMPUS_NAME";

    private void Start()
    {
        string campusName = PlayerPrefs.GetString(KEY_CAMPUS_NAME, "Íæ¼Ò");
        if (nicknameText != null)
            nicknameText.text = campusName;
    }
}