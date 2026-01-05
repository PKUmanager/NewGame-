using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text countText;

    public void Bind(string displayName, int count, Sprite sprite)
    {
        if (nameText != null) nameText.text = displayName ?? "";
        if (countText != null) countText.text = count.ToString();

        if (icon != null)
        {
            icon.sprite = sprite;
            icon.enabled = (sprite != null);
        }
    }
}