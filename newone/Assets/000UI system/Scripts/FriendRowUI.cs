using System;
using UnityEngine;
using UnityEngine.UI;

public class FriendRowUI : MonoBehaviour
{
    [SerializeField] private Image avatarImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Button btnVisit;

    private FriendData data;
    private Action<FriendData> onVisit;

    public void Bind(FriendData data, Action<FriendData> onVisit)
    {
        this.data = data;
        this.onVisit = onVisit;

        if (nameText != null) nameText.text = data.name;

        if (avatarImage != null)
        {
            avatarImage.sprite = data.avatar;
            avatarImage.enabled = (data.avatar != null); // Ã»Í·Ïñ¾ÍÒþ²Ø
        }

        if (btnVisit != null)
        {
            btnVisit.onClick.RemoveAllListeners();
            btnVisit.onClick.AddListener(() => this.onVisit?.Invoke(this.data));
        }
    }
}