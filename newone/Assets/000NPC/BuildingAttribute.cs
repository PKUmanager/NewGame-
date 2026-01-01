using UnityEngine;

// 定义四大分类（对应你文件夹的名字）
public enum BuildingType
{
    None,
    Terrains,    // 对应 Terrains 文件夹 (铺地)
    Facilities,  // 对应 Facilities 文件夹 (家具/设施)
    Nature,      // 对应 Nature 文件夹 (景观)
    Decorations  // 对应 Decorations 文件夹 (装饰)
}

public class BuildingAttribute : MonoBehaviour
{
    // 在 Inspector 里选
    public BuildingType type;
}