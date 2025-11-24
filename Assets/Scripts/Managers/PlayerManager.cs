using UnityEngine;
using Tenronis.Data;
using Tenronis.Core;

namespace Tenronis.Managers
{
    /// <summary>
    /// 玩家管理器
    /// </summary>
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance { get; private set; }
        
        // 玩家數據
        private PlayerStats stats;
        
        // Combo系統
        private float comboResetTimer;
        private bool comboResetPending;
        
        // 屬性
        public PlayerStats Stats => stats;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void Start()
        {
            // 初始化數據
            ResetStats();
            
            // 訂閱事件
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
            GameEvents.OnPlayerDamaged += HandlePlayerDamaged;
            GameEvents.OnBuffSelected += HandleBuffSelected;
            GameEvents.OnPieceLocked += HandlePieceLocked;
            GameEvents.OnRowsCleared += HandleRowsCleared;
        }
        
        private void OnDestroy()
        {
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
            GameEvents.OnPlayerDamaged -= HandlePlayerDamaged;
            GameEvents.OnBuffSelected -= HandleBuffSelected;
            GameEvents.OnPieceLocked -= HandlePieceLocked;
            GameEvents.OnRowsCleared -= HandleRowsCleared;
        }
        
        private void Update()
        {
            // Combo重置計時器
            if (comboResetPending)
            {
                comboResetTimer -= Time.deltaTime;
                if (comboResetTimer <= 0f)
                {
                    ResetCombo();
                    comboResetPending = false;
                }
            }
        }
        
        /// <summary>
        /// 重置玩家數據
        /// </summary>
        public void ResetStats()
        {
            stats = new PlayerStats();
            comboResetPending = false;
        }
        
        /// <summary>
        /// 增加分數
        /// </summary>
        public void AddScore(int amount)
        {
            stats.score += amount;
        }
        
        /// <summary>
        /// 治療
        /// </summary>
        public void Heal(int amount)
        {
            stats.currentHp = Mathf.Min(stats.maxHp, stats.currentHp + amount);
        }
        
        /// <summary>
        /// 受到傷害
        /// </summary>
        private void HandlePlayerDamaged(int damage)
        {
            stats.currentHp = Mathf.Max(0, stats.currentHp - damage);
            
            if (stats.currentHp <= 0)
            {
                GameManager.Instance.GameOver();
            }
        }
        
        /// <summary>
        /// 處理Buff選擇
        /// </summary>
        private void HandleBuffSelected(BuffType buffType)
        {
            switch (buffType)
            {
                case BuffType.Defense:
                    stats.blockDefenseLevel++;
                    break;
                    
                case BuffType.Volley:
                    stats.missileExtraCount++;
                    break;
                    
                case BuffType.Heal:
                    Heal(Mathf.FloorToInt(stats.maxHp * 0.5f));
                    break;
                    
                case BuffType.Explosion:
                    stats.explosionDamage += 50;
                    break;
                    
                case BuffType.Salvo:
                    stats.salvoLevel++;
                    break;
                    
                case BuffType.Burst:
                    stats.burstLevel++;
                    break;
                    
                case BuffType.Counter:
                    stats.counterFireLevel++;
                    break;
                    
                case BuffType.Execution:
                    stats.executionCount++;
                    break;
                    
                case BuffType.Repair:
                    stats.repairCount++;
                    break;
            }
            
            Debug.Log($"[PlayerManager] 應用Buff: {buffType}");
        }
        
        /// <summary>
        /// 處理方塊鎖定
        /// </summary>
        private void HandlePieceLocked()
        {
            // 如果沒有消除行，開始Combo重置計時
            if (stats.counterFireLevel > 0)
            {
                StartComboResetTimer();
            }
            else
            {
                ResetCombo();
            }
        }
        
        /// <summary>
        /// 處理消除行
        /// </summary>
        private void HandleRowsCleared(int rowCount)
        {
            if (rowCount > 0)
            {
                // 增加Combo
                stats.comboCount++;
                GameEvents.TriggerComboChanged(stats.comboCount);
                
                // 取消Combo重置
                comboResetPending = false;
                
                // 增加分數
                AddScore(rowCount * 100);
                
                // 消除行數的資訊顯示在固定 UI (SalvoText)，不需要彈出文字
            }
        }
        
        /// <summary>
        /// 開始Combo重置計時器
        /// </summary>
        private void StartComboResetTimer()
        {
            comboResetTimer = GameConstants.COMBO_RESET_DELAY;
            comboResetPending = true;
        }
        
        /// <summary>
        /// 取消Combo重置
        /// </summary>
        public void CancelComboReset()
        {
            comboResetPending = false;
        }
        
        /// <summary>
        /// 重置Combo
        /// </summary>
        private void ResetCombo()
        {
            if (stats.comboCount > 0)
            {
                stats.comboCount = 0;
                GameEvents.TriggerComboReset();
            }
        }
        
        /// <summary>
        /// 消耗爆炸充能
        /// </summary>
        public void ConsumeExplosionCharge()
        {
            stats.explosionDamage = 0;
        }
        
        /// <summary>
        /// 使用處決技能
        /// </summary>
        public bool UseExecution()
        {
            if (stats.executionCount <= 0) return false;
            
            stats.executionCount--;
            CancelComboReset();
            stats.comboCount++;
            GameEvents.TriggerComboChanged(stats.comboCount);
            
            return true;
        }
        
        /// <summary>
        /// 使用修復技能
        /// </summary>
        public bool UseRepair()
        {
            if (stats.repairCount <= 0) return false;
            
            stats.repairCount--;
            return true;
        }
        
        /// <summary>
        /// 處理遊戲狀態變更
        /// </summary>
        private void HandleGameStateChanged(GameState newState)
        {
            if (newState == GameState.Playing)
            {
                // 每次進入 Playing 狀態時檢查是否需要重置
                // 如果當前關卡索引是 0，表示是新遊戲開始
                if (GameManager.Instance != null && GameManager.Instance.CurrentStageIndex == 0)
                {
                    Debug.Log("[PlayerManager] 新遊戲開始，重置玩家數據");
                    ResetStats();
                }
            }
        }
    }
}

