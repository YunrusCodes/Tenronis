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
        public int SpaceExpansionLevel => stats.spaceExpansionLevel;
        
        /// <summary>
        /// 檢查是否有普通強化已達滿級
        /// </summary>
        public bool HasMaxedNormalBuff()
        {
            // 檢查所有普通強化是否已達上限
            if (stats.salvoLevel >= GameConstants.SALVO_MAX_LEVEL) return true;
            if (stats.burstLevel >= GameConstants.BURST_MAX_LEVEL) return true;
            if (stats.counterFireLevel >= GameConstants.COUNTER_MAX_LEVEL) return true;
            if (stats.explosionChargeLevel >= GameConstants.EXPLOSION_BUFF_MAX_LEVEL) return true;
            if (stats.spaceExpansionLevel >= GameConstants.SPACE_EXPANSION_MAX_LEVEL) return true;
            if (stats.cpExpansionLevel >= GameConstants.RESOURCE_EXPANSION_MAX_LEVEL) return true;

            // ❌ 已移除 TacticalExpansion（因為它現在是傳奇）
            return false;
        }

        /// <summary>
        /// 檢查指定Buff是否已達滿級
        /// </summary>
        public bool IsBuffMaxed(BuffType buffType)
        {
            switch (buffType)
            {
                case BuffType.Salvo:
                    return stats.salvoLevel >= GameConstants.SALVO_MAX_LEVEL;
                case BuffType.Burst:
                    return stats.burstLevel >= GameConstants.BURST_MAX_LEVEL;
                case BuffType.Counter:
                    return stats.counterFireLevel >= GameConstants.COUNTER_MAX_LEVEL;
                case BuffType.Explosion:
                    return stats.explosionChargeLevel >= GameConstants.EXPLOSION_BUFF_MAX_LEVEL;
                case BuffType.SpaceExpansion:
                    return stats.spaceExpansionLevel >= GameConstants.SPACE_EXPANSION_MAX_LEVEL;
                case BuffType.ResourceExpansion:
                    return stats.cpExpansionLevel >= GameConstants.RESOURCE_EXPANSION_MAX_LEVEL;

                // ⭐ TacticalExpansion 是傳奇強化，但有等級上限
                case BuffType.TacticalExpansion:
                    return stats.tacticalExpansionLevel >= GameConstants.TACTICAL_EXPANSION_MAX_LEVEL;

                default:
                    return false; 
            }
        }

        
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
        /// 恢復CP至全滿
        /// </summary>
        public void RestoreCp()
        {
            stats.currentCp = stats.maxCp;
            Debug.Log($"[PlayerManager] CP已恢復至全滿: {stats.currentCp} / {stats.maxCp}");
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
                    Debug.Log($"[PlayerManager] 協同火力等級提升至: {stats.missileExtraCount}");
                    break;
                    
                case BuffType.Heal:
                    Heal(Mathf.FloorToInt(stats.maxHp * 0.5f));
                    break;
                    
                case BuffType.Explosion:
                    if (stats.explosionChargeLevel < GameConstants.EXPLOSION_BUFF_MAX_LEVEL)
                    {
                        stats.explosionChargeLevel++;
                        stats.explosionMaxCharge += GameConstants.EXPLOSION_BUFF_MAX_CHARGE_INCREASE;
                        Debug.Log($"[PlayerManager] 爆炸充能上限增加200，當前上限: {stats.explosionMaxCharge} (等級: {stats.explosionChargeLevel}/{GameConstants.EXPLOSION_BUFF_MAX_LEVEL})");
                    }
                    else
                    {
                        Debug.Log($"[PlayerManager] 爆炸充能已達最高等級 {GameConstants.EXPLOSION_BUFF_MAX_LEVEL}");
                    }
                    break;
                    
                case BuffType.Salvo:
                    if (stats.salvoLevel < GameConstants.SALVO_MAX_LEVEL)
                    {
                        stats.salvoLevel++;
                        Debug.Log($"[PlayerManager] 齊射強化等級提升至: {stats.salvoLevel}/{GameConstants.SALVO_MAX_LEVEL}");
                    }
                    else
                    {
                        Debug.Log($"[PlayerManager] 齊射強化已達最高等級 {GameConstants.SALVO_MAX_LEVEL}");
                    }
                    break;
                    
                case BuffType.Burst:
                    if (stats.burstLevel < GameConstants.BURST_MAX_LEVEL)
                    {
                        stats.burstLevel++;
                        Debug.Log($"[PlayerManager] 連發強化等級提升至: {stats.burstLevel}/{GameConstants.BURST_MAX_LEVEL}");
                    }
                    else
                    {
                        Debug.Log($"[PlayerManager] 連發強化已達最高等級 {GameConstants.BURST_MAX_LEVEL}");
                    }
                    break;
                    
                case BuffType.Counter:
                    if (stats.counterFireLevel < GameConstants.COUNTER_MAX_LEVEL)
                    {
                        stats.counterFireLevel++;
                        Debug.Log($"[PlayerManager] 反擊強化等級提升至: {stats.counterFireLevel}/{GameConstants.COUNTER_MAX_LEVEL}");
                    }
                    else
                    {
                        Debug.Log($"[PlayerManager] 反擊強化已達最高等級 {GameConstants.COUNTER_MAX_LEVEL}");
                    }
                    break;
                    
                case BuffType.SpaceExpansion:
                    if (stats.spaceExpansionLevel < GameConstants.SPACE_EXPANSION_MAX_LEVEL)
                    {
                        stats.spaceExpansionLevel++;
                        Debug.Log($"[PlayerManager] 解鎖儲存槽位！當前已解鎖: {stats.spaceExpansionLevel}/{GameConstants.SPACE_EXPANSION_MAX_LEVEL}");
                        
                        // 通知 TetrominoController 解鎖槽位
                        if (Tenronis.Gameplay.Tetromino.TetrominoController.Instance != null)
                        {
                            Tenronis.Gameplay.Tetromino.TetrominoController.Instance.UnlockSlot();
                        }
                    }
                    else
                    {
                        Debug.Log($"[PlayerManager] 空間擴充已達最高等級 {GameConstants.SPACE_EXPANSION_MAX_LEVEL}");
                    }
                    break;
                    
                case BuffType.ResourceExpansion:
                    if (stats.cpExpansionLevel < GameConstants.RESOURCE_EXPANSION_MAX_LEVEL)
                    {
                        int oldMaxCp = stats.maxCp;
                        stats.cpExpansionLevel++;
                        stats.maxCp += 50;
                        // 同時增加當前CP（保持比例）
                        if (oldMaxCp > 0)
                        {
                            float cpRatio = (float)stats.currentCp / oldMaxCp;
                            stats.currentCp = Mathf.RoundToInt(stats.maxCp * cpRatio);
                        }
                        else
                        {
                            stats.currentCp = stats.maxCp;
                        }
                        Debug.Log($"[PlayerManager] 資源擴充！CP上限增加50，當前上限: {stats.maxCp} (等級: {stats.cpExpansionLevel}/{GameConstants.RESOURCE_EXPANSION_MAX_LEVEL})");
                    }
                    else
                    {
                        Debug.Log($"[PlayerManager] 資源擴充已達最高等級 {GameConstants.RESOURCE_EXPANSION_MAX_LEVEL}");
                    }
                    break;
                    
                case BuffType.TacticalExpansion:
                    if (stats.tacticalExpansionLevel < GameConstants.TACTICAL_EXPANSION_MAX_LEVEL)
                    {
                        stats.tacticalExpansionLevel++;
                        string unlockedSkill = stats.tacticalExpansionLevel == 1 ? "處決" : "修補";
                        Debug.Log($"[PlayerManager] 戰術擴展等級提升至: {stats.tacticalExpansionLevel}/{GameConstants.TACTICAL_EXPANSION_MAX_LEVEL}，解鎖技能: {unlockedSkill}");
                        // 觸發UI更新事件
                        GameEvents.TriggerSkillUnlocked();
                    }
                    else
                    {
                        Debug.Log($"[PlayerManager] 戰術擴展已達最高等級 {GameConstants.TACTICAL_EXPANSION_MAX_LEVEL}");
                    }
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
        private void HandleRowsCleared(int totalRowCount, int nonGarbageRowCount, bool hasVoid)
        {
            if (totalRowCount > 0)
            {
                // 增加Combo
                stats.comboCount++;
                GameEvents.TriggerComboChanged(stats.comboCount);
                
                // 取消Combo重置
                comboResetPending = false;
                
                // 增加分數（按總行數計算）
                AddScore(totalRowCount * 100);
                
                // 增加爆炸充能（消排增加50充能）
                AddExplosionCharge(GameConstants.EXPLOSION_ROW_CLEAR_CHARGE);
                
                // 虛無抵銷處理
                if (hasVoid)
                {
                    Debug.Log("[PlayerManager] 虛無抵銷！不產生導彈");
                    // 顯示"虛無抵銷!"彈出文字
                    GameEvents.TriggerShowPopupText(
                        "虛無抵銷!",
                        new Color(0.5f, 0.5f, 0.5f), // 灰色
                        Vector2.zero
                    );
                }
                
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
        /// 增加爆炸充能
        /// </summary>
        public void AddExplosionCharge(int amount)
        {
            stats.explosionCharge = Mathf.Min(stats.explosionMaxCharge, stats.explosionCharge + amount);
            Debug.Log($"[PlayerManager] 爆炸充能增加 {amount}，當前: {stats.explosionCharge}/{stats.explosionMaxCharge}");
        }
        
        /// <summary>
        /// 消耗爆炸充能
        /// </summary>
        public void ConsumeExplosionCharge()
        {
            stats.explosionCharge = 0;
        }
        
        /// <summary>
        /// 檢查處決技能是否已解鎖
        /// </summary>
        public bool IsExecutionUnlocked()
        {
            return stats.tacticalExpansionLevel >= 1;
        }
        
        /// <summary>
        /// 檢查修補技能是否已解鎖
        /// </summary>
        public bool IsRepairUnlocked()
        {
            return stats.tacticalExpansionLevel >= 2;
        }
        
        /// <summary>
        /// 使用處決技能（消耗CP）
        /// </summary>
        public bool UseExecution()
        {
            if (!IsExecutionUnlocked())
            {
                Debug.Log("[PlayerManager] 處決技能尚未解鎖！");
                return false;
            }
            
            int cost = GameConstants.EXECUTION_CP_COST;
            if (stats.currentCp < cost) return false;
            
            stats.currentCp -= cost;
            CancelComboReset();
            stats.comboCount++;
            GameEvents.TriggerComboChanged(stats.comboCount);
            GameEvents.TriggerCpChanged(stats.currentCp, stats.maxCp);
            
            Debug.Log($"[PlayerManager] 使用處決技能，消耗 {cost} CP，剩餘: {stats.currentCp}/{stats.maxCp}");
            return true;
        }
        
        /// <summary>
        /// 使用修復技能（消耗CP）
        /// </summary>
        public bool UseRepair()
        {
            if (!IsRepairUnlocked())
            {
                Debug.Log("[PlayerManager] 修補技能尚未解鎖！");
                return false;
            }
            
            int cost = GameConstants.REPAIR_CP_COST;
            if (stats.currentCp < cost) return false;
            
            stats.currentCp -= cost;
            GameEvents.TriggerCpChanged(stats.currentCp, stats.maxCp);
            
            Debug.Log($"[PlayerManager] 使用修復技能，消耗 {cost} CP，剩餘: {stats.currentCp}/{stats.maxCp}");
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

