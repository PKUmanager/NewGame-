using System.Collections.Generic;
using UnityEngine;
using SpaceFusion.SF_Grid_Building_System.Scripts.Core;
using SpaceFusion.SF_Grid_Building_System.Scripts.SaveSystem;
using SpaceFusion.SF_Grid_Building_System.Scripts.Scriptables;
using SpaceFusion.SF_Grid_Building_System.Scripts.Utils;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Managers
{
    public class UndoManager : MonoBehaviour
    {
        public static UndoManager Instance;

        // 历史记录栈
        private Stack<IUndoCommand> _undoStack = new Stack<IUndoCommand>();

        // 锁：防止在执行撤回操作时，再次触发“记录操作”，导致死循环
        public bool IsUndoing { get; private set; } = false;

        private void Awake()
        {
            if (Instance != null) Destroy(this);
            Instance = this;
        }

        // === 外部调用接口 ===

        // 记录：刚放置了一个物体 -> 撤回时需要删除它
        public void RecordPlaceAction(string guid)
        {
            if (IsUndoing) return;
            _undoStack.Push(new UndoPlaceCommand(guid));
            // Debug.Log($"[Undo] 记录放置: {guid}, 历史栈: {_undoStack.Count}");
        }

        // 记录：刚删除了一个物体 -> 撤回时需要重新放置它
        public void RecordRemoveAction(PlaceableObjectData data)
        {
            if (IsUndoing) return;
            _undoStack.Push(new UndoRemoveCommand(data));
            // Debug.Log($"[Undo] 记录删除: {data.assetIdentifier}, 历史栈: {_undoStack.Count}");
        }

        // 执行撤回
        public void PerformUndo()
        {
            if (_undoStack.Count == 0)
            {
                Debug.Log("没有可以撤回的操作。");
                return;
            }

            IsUndoing = true; // 上锁

            try
            {
                IUndoCommand command = _undoStack.Pop();
                command.Undo();

                // 撤回后强制保存一次，确保 SaveData 数据一致
                if (GameManager.Instance != null && GameManager.Instance.saveData != null)
                    SaveSystem.SaveSystem.Save(GameManager.Instance.saveData);
            }
            catch (System.Exception e)
            {
                Debug.LogError("撤回失败: " + e.Message);
            }
            finally
            {
                IsUndoing = false; // 解锁
            }
        }
    }

    // === 命令模式接口 ===
    public interface IUndoCommand
    {
        void Undo();
    }

    // 撤回“放置”操作 -> 执行删除
    public class UndoPlaceCommand : IUndoCommand
    {
        private string _guid;
        public UndoPlaceCommand(string guid) => _guid = guid;

        public void Undo()
        {
            var handler = Object.FindObjectOfType<PlacementHandler>();
            if (handler != null)
            {
                // 注意：这里只删除了物体，如果你有 GridData (红/绿格子占用)，
                // 可能需要重置 GridData。但在目前的架构下，删除物体通常足够。
                // 如果需要清除格子占用，可以在 PlacementHandler 中处理，或者玩家点击格子时会自动刷新。
                handler.RemoveObjectPositions(_guid);
            }
        }
    }

    // 撤回“删除”操作 -> 执行重新放置
    public class UndoRemoveCommand : IUndoCommand
    {
        private PlaceableObjectData _data;
        // 保存数据副本
        public UndoRemoveCommand(PlaceableObjectData data) => _data = data;

        public void Undo()
        {
            var handler = Object.FindObjectOfType<PlacementHandler>();
            var config = GameConfig.Instance;

            if (handler != null && config != null)
            {
                var db = config.PlaceableObjectDatabase;
                Placeable placeable = db.GetPlaceable(_data.assetIdentifier);

                if (placeable != null)
                {
                    // 获取 Grid 引用以计算世界坐标
                    var grid = Object.FindObjectOfType<PlacementGrid>();
                    Vector3 worldPos = grid.CellToWorld(_data.gridPosition);

                    // 重新生成物体
                    handler.PlaceLoadedObject(placeable, worldPos, _data, grid.CellSize);

                    // 恢复 SaveData (PlaceLoadedObject 内部通常会处理，或者 PlacementHandler 需要处理)
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.saveData.AddData(_data);
                    }
                }
            }
        }
    }
}