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

        // ��ʷ��¼ջ
        private Stack<IUndoCommand> _undoStack = new Stack<IUndoCommand>();

        // ������ֹ��ִ�г��ز���ʱ���ٴδ�������¼��������������ѭ��
        public bool IsUndoing { get; private set; } = false;

        private void Awake()
        {
            if (Instance != null) Destroy(this);
            Instance = this;
        }

        // === �ⲿ���ýӿ� ===

        // ��¼���շ�����һ������ -> ����ʱ��Ҫɾ����
        public void RecordPlaceAction(string guid)
        {
            if (IsUndoing) return;
            _undoStack.Push(new UndoPlaceCommand(guid));
            // Debug.Log($"[Undo] ��¼����: {guid}, ��ʷջ: {_undoStack.Count}");
        }

        // ��¼����ɾ����һ������ -> ����ʱ��Ҫ���·�����
        public void RecordRemoveAction(PlaceableObjectData data)
        {
            if (IsUndoing) return;
            _undoStack.Push(new UndoRemoveCommand(data));
            // Debug.Log($"[Undo] ��¼ɾ��: {data.assetIdentifier}, ��ʷջ: {_undoStack.Count}");
        }

        // ִ�г���
        public void PerformUndo()
        {
            if (_undoStack.Count == 0)
            {
                Debug.Log("û�п��Գ��صĲ�����");
                return;
            }

            IsUndoing = true; // ����

            try
            {
                IUndoCommand command = _undoStack.Pop();
                command.Undo();

                // ���غ�ǿ�Ʊ���һ�Σ�ȷ�� SaveData ����һ��
                if (GameManager.Instance != null && GameManager.Instance.saveData != null)
                    SaveSystem.SaveSystem.Save(GameManager.Instance.saveData);
            }
            catch (System.Exception e)
            {
                Debug.LogError("����ʧ��: " + e.Message);
            }
            finally
            {
                IsUndoing = false; // ����
            }
        }
    }

    // === ����ģʽ�ӿ� ===
    public interface IUndoCommand
    {
        void Undo();
    }

    // ���ء����á����� -> ִ��ɾ��
    public class UndoPlaceCommand : IUndoCommand
    {
        private string _guid;
        public UndoPlaceCommand(string guid) => _guid = guid;

        public void Undo()
        {
            var placementSystem = PlacementSystem.Instance;
            if (placementSystem != null && placementSystem.UndoRemovePlacedObjectByGuid(_guid))
            {
                return;
            }

            // Fallback for scenes where PlacementSystem is unavailable.
            var handler = Object.FindObjectOfType<PlacementHandler>();
            if (handler != null)
            {
                handler.RemoveObjectPositions(_guid);
            }
        }
    }

    // ���ء�ɾ�������� -> ִ�����·���
    public class UndoRemoveCommand : IUndoCommand
    {
        private PlaceableObjectData _data;
        // �������ݸ���
        public UndoRemoveCommand(PlaceableObjectData data) => _data = data;

        public void Undo()
        {
            var placementSystem = PlacementSystem.Instance;
            if (placementSystem != null)
            {
                placementSystem.InitializeLoadedObject(_data);
                if (GameManager.Instance != null && GameManager.Instance.saveData != null)
                {
                    GameManager.Instance.saveData.AddData(_data);
                }

                return;
            }

            var handler = Object.FindObjectOfType<PlacementHandler>();
            var config = GameConfig.Instance;
            if (handler == null || config == null) return;

            var db = config.PlaceableObjectDatabase;
            Placeable placeable = db.GetPlaceable(_data.assetIdentifier);
            if (placeable == null) return;

            var grid = Object.FindObjectOfType<PlacementGrid>();
            if (grid == null) return;

            Vector3 worldPos = grid.CellToWorld(_data.gridPosition);
            handler.PlaceLoadedObject(placeable, worldPos, _data, grid.CellSize);

            if (GameManager.Instance != null && GameManager.Instance.saveData != null)
            {
                GameManager.Instance.saveData.AddData(_data);
            }
        }
    }
}