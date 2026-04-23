using UnityEngine;

public class PlantingManager : MonoBehaviour
{
    // 在 Unity 面板里把你的粒子预制体拖到这里
    public GameObject splashPrefab;

    // 这个函数用来给 UI 按钮调用
    public void PlaySplashEffect()
    {
        // 1. 获取你想让粒子出现的位置
        // 如果是点击处，或者是固定的种植点，这里以“当前鼠标在世界空间的位置”为例
        Vector3 spawnPosition = CalculatePlantPosition();

        // 2. 生成粒子特效
        if (splashPrefab != null)
        {
            // 在指定位置生成粒子，Quaternion.identity 表示不旋转
            Instantiate(splashPrefab, spawnPosition, Quaternion.identity);
        }
    }

    // 这是一个示意函数，计算树应该种在哪
    Vector3 CalculatePlantPosition()
    {
        // 这里返回你实际种树的坐标，目前先返回原点作为示例
        return Vector3.zero;
    }
}