using SpaceFusion.SF_Grid_Building_System.Scripts.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.UI
{
    /// <summary>
    /// 挂载在UI按钮上。
    /// 当按下这个按钮时，强制结束当前的建造/删除状态。
    /// </summary>
    public class StopPlacementTrigger : MonoBehaviour, IPointerDownHandler
    {
        // 使用 OnPointerDown 而不是 OnClick，可以确保手指/鼠标按下去的瞬间，建造虚影就立刻消失，体验更好
        public void OnPointerDown(PointerEventData eventData)
        {
            if (PlacementSystem.Instance != null)
            {
                // 强制停止当前的建造状态
                PlacementSystem.Instance.StopState();
                Debug.Log("UI Action triggered: Placement State Stopped.");
            }
        }
    }
}