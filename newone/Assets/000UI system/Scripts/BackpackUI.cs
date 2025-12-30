using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackpackUI : MonoBehaviour
{
    public enum Category
    {
        家具, 装饰, 植物, 材料, 装扮
    }

    [Header("Category Tabs（按你层级命名拖拽）")]
    [SerializeField] private Button tab家具;
    [SerializeField] private Button tab装饰;
    [SerializeField] private Button tab植物;
    [SerializeField] private Button tab材料;
    [SerializeField] private Button tab装扮;

    [Header("ScrollView Content（Viewport/Content）")]
    [SerializeField] private Transform contentRoot;

    [Header("ItemSlot Prefab（你做的预制体）")]
    [SerializeField] private ItemSlotUI itemSlotPrefab;

    [Header("Close Button（Btn_Close）")]
    [SerializeField] private Button btnClose;

    // ====== 测试用：假背包数据（后面你接真实背包系统时替换这里） ======
    private List<ItemData> allItems = new List<ItemData>();

    private Category currentCategory = Category.家具;

    private void Awake()
    {
        // 绑定标签点击
        if (tab家具 != null) tab家具.onClick.AddListener(() => SwitchCategory(Category.家具));
        if (tab装饰 != null) tab装饰.onClick.AddListener(() => SwitchCategory(Category.装饰));
        if (tab植物 != null) tab植物.onClick.AddListener(() => SwitchCategory(Category.植物));
        if (tab材料 != null) tab材料.onClick.AddListener(() => SwitchCategory(Category.材料));
        if (tab装扮 != null) tab装扮.onClick.AddListener(() => SwitchCategory(Category.装扮));

        if (btnClose != null) btnClose.onClick.AddListener(() => gameObject.SetActive(false));

        // 初始化一些假数据
        BuildMockData();
    }

    private void OnEnable()
    {
        // 每次打开背包时刷新当前分类
        RefreshList(currentCategory);
    }

    private void SwitchCategory(Category category)
    {
        currentCategory = category;
        RefreshList(category);
        UpdateTabVisual(category);
    }

    private void RefreshList(Category category)
    {
        ClearContent();

        for (int i = 0; i < allItems.Count; i++)
        {
            if (allItems[i].category != category)
                continue;

            ItemSlotUI slot = Instantiate(itemSlotPrefab, contentRoot);
            slot.Bind(allItems[i], OnClickItem);
        }
    }

    private void ClearContent()
    {
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(contentRoot.GetChild(i).gameObject);
        }
    }

    private void OnClickItem(ItemData item)
    {
        // 这里先简单演示：点击物品时你可以打开详情面板/弹窗
        // 小白先做到能点通：你可以先在Console看不到输出（你不想print），就先留空
        // 下一步我会带你做 “ItemDetailPanel”
    }

    private void UpdateTabVisual(Category category)
    {
        // 简单高亮：把选中tab变灰/变亮（你也可以换成更漂亮的美术）
        SetTabColor(tab家具, category == Category.家具);
        SetTabColor(tab装饰, category == Category.装饰);
        SetTabColor(tab植物, category == Category.植物);
        SetTabColor(tab材料, category == Category.材料);
        SetTabColor(tab装扮, category == Category.装扮);
    }

    private void SetTabColor(Button btn, bool selected)
    {
        if (btn == null) return;
        Image img = btn.GetComponent<Image>();
        if (img == null) return;

        img.color = selected ? new Color(0.85f, 0.85f, 0.85f, 1f) : Color.white;
    }

    private void BuildMockData()
    {
        // 你后面接真实数据时，把这部分替换成从背包系统读取即可
        allItems.Clear();

        allItems.Add(new ItemData("木椅", Category.家具, 2, null));
        allItems.Add(new ItemData("书桌", Category.家具, 1, null));
        allItems.Add(new ItemData("壁画", Category.装饰, 3, null));
        allItems.Add(new ItemData("台灯", Category.装饰, 1, null));
        allItems.Add(new ItemData("绿植A", Category.植物, 5, null));
        allItems.Add(new ItemData("树苗", Category.植物, 2, null));
        allItems.Add(new ItemData("木材", Category.材料, 20, null));
        allItems.Add(new ItemData("石料", Category.材料, 15, null));
        allItems.Add(new ItemData("校服", Category.装扮, 1, null));
    }
}

[System.Serializable]
public class ItemData
{
    public string name;
    public BackpackUI.Category category;
    public int count;
    public Sprite icon;

    public ItemData(string name, BackpackUI.Category category, int count, Sprite icon)
    {
        this.name = name;
        this.category = category;
        this.count = count;
        this.icon = icon;
    }
}