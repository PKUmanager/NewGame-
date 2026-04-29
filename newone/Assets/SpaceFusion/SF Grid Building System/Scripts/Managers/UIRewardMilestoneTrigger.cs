using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Managers
{
    public class UIRewardMilestoneTrigger : MonoBehaviour
    {
        private enum StatType
        {
            Safety,
            Aesthetics,
            Environment,
            Comfort
        }

        [System.Serializable]
        private class MilestoneAnimation
        {
            [SerializeField] private StatType _statType;
            [SerializeField] private int _threshold;
            [SerializeField] private Animator _animator;
            [SerializeField] private string _triggerName = "Play";

            [HideInInspector] public bool Triggered;

            public bool ShouldTrigger(int previousValue, int currentValue)
            {
                // 只有没被触发过，且数值从低于阈值变为高于阈值时才返回 true
                return !Triggered && previousValue < _threshold && currentValue >= _threshold;
            }

            public int GetStatValue(int safety, int aesthetics, int environment, int comfort)
            {
                switch (_statType)
                {
                    case StatType.Safety: return safety;
                    case StatType.Aesthetics: return aesthetics;
                    case StatType.Environment: return environment;
                    case StatType.Comfort: return comfort;
                    default: return 0;
                }
            }

            public void Play()
            {
                if (_animator == null) return;

                GameObject targetObject = _animator.gameObject;
                // 必须激活物体，否则 Animator 指令无效
                if (targetObject != null && !targetObject.activeSelf)
                {
                    targetObject.SetActive(true);
                }

                if (!string.IsNullOrWhiteSpace(_triggerName))
                {
                    _animator.SetTrigger(_triggerName);
                }
            }
        }

        [Header("分数来源")]
        [SerializeField] private GameManager _gameManager;

        [Header("触发规则")]
        [SerializeField] private List<MilestoneAnimation> _milestones = new List<MilestoneAnimation>();

        private int _previousSafety;
        private int _previousAesthetics;
        private int _previousEnvironment;
        private int _previousComfort;

        private bool _isReady = false; // 标记：是否已度过初始加载期

        private void Awake()
        {
            if (_gameManager == null)
            {
                _gameManager = GameManager.Instance;
            }
        }

        // 核心：延迟启动，跳过 HomeLoader 的加载期
        private IEnumerator Start()
        {
            // 等待 2.5 秒，确保云端数据恢复完成，数值已稳定
            yield return new WaitForSeconds(2.5f);

            // 同步当前数值为“上一次数值”，确保比较时是从当前基数开始
            SyncValues();

            _isReady = true;
            Debug.Log("<color=green>【奖励系统】已就绪，开始监测实时搭建行为。</color>");
        }

        private void OnEnable()
        {
            if (_gameManager != null)
            {
                _gameManager.OnTotalStatsChanged += HandleTotalStatsChanged;
            }
        }

        private void OnDisable()
        {
            if (_gameManager != null)
            {
                _gameManager.OnTotalStatsChanged -= HandleTotalStatsChanged;
            }
        }

        private void SyncValues()
        {
            if (_gameManager == null) return;
            _previousSafety = _gameManager.TotalSafety;
            _previousAesthetics = _gameManager.TotalAesthetics;
            _previousEnvironment = _gameManager.TotalEnvironment;
            _previousComfort = _gameManager.TotalComfort;
        }

        private void HandleTotalStatsChanged(
            int totalCost,
            int totalSafety,
            int totalAesthetics,
            int totalEnvironment,
            int totalComfort)
        {
            // 如果还处于加载保护期，只默默同步数值，绝不触发动画
            if (!_isReady)
            {
                _previousSafety = totalSafety;
                _previousAesthetics = totalAesthetics;
                _previousEnvironment = totalEnvironment;
                _previousComfort = totalComfort;
                return;
            }

            // 实时监测逻辑
            for (int i = 0; i < _milestones.Count; i++)
            {
                MilestoneAnimation milestone = _milestones[i];
                if (milestone == null || milestone.Triggered) continue;

                int prev = milestone.GetStatValue(_previousSafety, _previousAesthetics, _previousEnvironment, _previousComfort);
                int curr = milestone.GetStatValue(totalSafety, totalAesthetics, totalEnvironment, totalComfort);

                if (milestone.ShouldTrigger(prev, curr))
                {
                    milestone.Triggered = true;
                    milestone.Play();
                }
            }

            // 更新历史数值
            _previousSafety = totalSafety;
            _previousAesthetics = totalAesthetics;
            _previousEnvironment = totalEnvironment;
            _previousComfort = totalComfort;
        }
    }
}