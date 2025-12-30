using System;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Text nameText;   // Text (Legacy)
    [SerializeField] private Text countText;  // Text (Legacy)
    [SerializeField] private Button button;

    private ItemData data;
    private Action<ItemData> onClick;

    public void Bind(ItemData data, Action<ItemData> onClick)
    {
        this.data = data;
        this.onClick = onClick;

        if (nameText != null) nameText.text = data.name;
        if (countText != null) countText.text = data.count.ToString();

        if (icon != null)
        {
            icon.sprite = data.icon;
            icon.enabled = (data.icon != null); // Ã»Í¼±ê¾ÍÒþ²Ø
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => this.onClick?.Invoke(this.data));
        }
    }
}