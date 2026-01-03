using UnityEngine;

public enum ItemCategory
{
    家具, 装饰, 植物, 材料, 装扮
}

[CreateAssetMenu(menuName = "Game/Item Definition", fileName = "Item_")]
public class ItemDefinition : ScriptableObject
{
    public string id;              // 唯一ID：例如 "beta_badge"
    public string displayName;     // 显示名：例如 "内测限定道具"
    public ItemCategory category;  // 分类：装扮
    public Sprite icon;            // 你的图片拖这里
}