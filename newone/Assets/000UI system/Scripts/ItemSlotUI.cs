using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Text nameText;   // legacy
    [SerializeField] private Text countText;  // legacy

    public void Bind(string displayName, int count, Sprite sprite)
    {
        if (nameText != null) nameText.text = displayName;
        if (countText != null) countText.text = count.ToString();

        if (icon != null)
        {
            icon.sprite = sprite;
            icon.enabled = (sprite != null);
        }
    }
}