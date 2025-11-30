using UnityEngine;
using Tenronis.Data;
using Tenronis.Core;
using Tenronis.ScriptableObjects;

namespace Tenronis.Managers
{
    /// <summary>
    /// 遊戲主管理器 - 單例模式
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        [Header("關卡數據")]
        [SerializeField] private StageDataSO[] stages;
        
        [Header("普通強化")]
        [SerializeField] private BuffDataSO[] normalBuffs;
        
        [Header("傳奇強化")]
        [SerializeField] private BuffDataSO[] legendaryBuffs;
        
        // 遊戲狀態
        private GameState currentState = GameState.Menu;
        private int currentStageIndex = 0;
        private int pendingBuffCount = 0;
        private float damageAccumulator = 0f;
        private int rogueRequirement = GameConstants.INITIAL_ROGUE_REQUIREMENT;
        
        // 屬性
        public GameState CurrentState => currentState;
        public StageDataSO CurrentStage => stages != null && currentStageIndex < stages.Length ? stages[currentStageIndex] : null;
        public int CurrentStageIndex => currentStageIndex;
        public int TotalStages => stages != null ? stages.Length : 0;
        public BuffDataSO[] NormalBuffs => normalBuffs;
        public BuffDataSO[] LegendaryBuffs => legendaryBuffs;
        public int PendingBuffCount => pendingBuffCount;
        public int RogueRequirement => rogueRequirement;
        
        private void Awake()
        {
            // 單例模式
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[GameManager] 場景中已存在GameManager實例，銷毀重複物件");
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[GameManager] 初始化完成 - 單例已建立");
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                GameEvents.ClearAllEvents();
            }
        }
        
        private void Start()
        {
            // 訂閱事件
            GameEvents.OnEnemyDefeated += HandleEnemyDefeated;
            // GameEvents.OnEnemyDamaged += HandleEnemyDamaged; // 已停用：改為關卡固定獎勵
            GameEvents.OnBuffSelected += HandleBuffSelected;
        }
        
        /// <summary>
        /// 改變遊戲狀態
        /// </summary>
        public void ChangeGameState(GameState newState)
        {
            if (currentState == newState)
            {
                Debug.LogWarning($"[GameManager] 嘗試切換到相同狀態: {newState}，已忽略");
                return;
            }
            
            Debug.Log($"[GameManager] 狀態變更: {currentState} → {newState}");
            currentState = newState;
            GameEvents.TriggerGameStateChanged(newState);
        }
        
        /// <summary>
        /// 開始遊戲
        /// </summary>
        public void StartGame()
        {
            Debug.Log("=== [GameManager] StartGame() 開始執行 ===");
            
            currentStageIndex = 0;
            pendingBuffCount = 0;
            damageAccumulator = 0f;
            rogueRequirement = GameConstants.INITIAL_ROGUE_REQUIREMENT;
            
            Debug.Log($"[GameManager] 遊戲數據已重置 - Stage: {currentStageIndex}, Buffs: {pendingBuffCount}");
            
            ChangeGameState(GameState.Playing);
            
            Debug.Log("=== [GameManager] StartGame() 執行完成 ===");
        }
        
        /// <summary>
        /// 重新開始遊戲
        /// </summary>
        public void RestartGame()
        {
            StartGame();
        }
        
        /// <summary>
        /// 返回主選單
        /// </summary>
        public void ReturnToMenu()
        {
            ChangeGameState(GameState.Menu);
        }
        
        /// <summary>
        /// 處理敵人被擊敗
        /// </summary>
        private void HandleEnemyDefeated()
        {
            // 獲取當前關卡的獎勵卡牌數量
            if (stages != null && currentStageIndex < stages.Length)
            {
                int stageReward = stages[currentStageIndex].rewardBuffCount;
                if (stageReward > 0)
                {
                    pendingBuffCount += stageReward;
                    Debug.Log($"[GameManager] 關卡 {currentStageIndex + 1} 完成！獲得 {stageReward} 張升級卡牌，總計: {pendingBuffCount}");
                }
            }
            
            // 恢復CP至全滿
            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.RestoreCp();
                Debug.Log($"[GameManager] 關卡完成，CP已恢復至全滿");
            }
            
            currentStageIndex++;
            
            if (currentStageIndex >= stages.Length)
            {
                // 勝利
                ChangeGameState(GameState.Victory);
            }
            else
            {
                // 進入下一關
                if (pendingBuffCount > 0)
                {
                    // 先鎖定當前方塊再進入選單
                    if (Tenronis.Gameplay.Tetromino.TetrominoController.Instance != null && 
                        Tenronis.Gameplay.Tetromino.TetrominoController.Instance.IsActive)
                    {
                        Tenronis.Gameplay.Tetromino.TetrominoController.Instance.ForceLock();
                    }
                    
                    ChangeGameState(GameState.LevelUp);
                }
                else
                {
                    // 直接繼續遊戲
                    ChangeGameState(GameState.Playing);
                }
            }
        }
        
        /// <summary>
        /// 處理敵人受傷 - 累積Roguelike點數
        /// 【已停用】改為關卡固定獎勵機制
        /// </summary>
        private void HandleEnemyDamaged(float damage)
        {
            // 累積傷害升級機制已停用
            // 現在改為每關固定獎勵 (StageDataSO.rewardBuffCount)
            
            /* 原始代碼（已註解）
            damageAccumulator += damage;
            
            // 檢查是否達到升級要求
            int buffsEarned = 0;
            while (damageAccumulator >= rogueRequirement)
            {
                damageAccumulator -= rogueRequirement;
                buffsEarned++;
                rogueRequirement += GameConstants.ROGUE_REQUIREMENT_INCREMENT;
            }
            
            if (buffsEarned > 0)
            {
                pendingBuffCount += buffsEarned;
                GameEvents.TriggerBuffAvailable();
                Debug.Log($"[GameManager] 獲得 {buffsEarned} 個升級點數！當前待選: {pendingBuffCount}");
            }
            */
        }
        
        /// <summary>
        /// 處理Buff選擇
        /// </summary>
        private void HandleBuffSelected(BuffType buffType)
        {
            pendingBuffCount--;
            Debug.Log($"[GameManager] 選擇Buff: {buffType}，剩餘: {pendingBuffCount}");
            
            if (pendingBuffCount <= 0)
            {
                // 檢查是否需要提供傳奇強化選擇
                // 如果選擇的普通強化使其達到滿級，且之前沒有普通強化滿級，則不立即改變狀態
                // 讓RoguelikeMenu決定是否繼續顯示選單
                bool shouldWaitForLegendaryBuff = false;
                
                if (PlayerManager.Instance != null)
                {
                    // 檢查選擇的Buff是否為普通強化
                    bool isNormalBuff = System.Array.IndexOf(GameConstants.NORMAL_BUFFS, buffType) >= 0;
                    
                    if (isNormalBuff)
                    {
                        // 檢查是否使普通強化達到滿級
                        bool isNowMaxed = PlayerManager.Instance.IsBuffMaxed(buffType);
                        
                        if (isNowMaxed)
                        {
                            // 檢查之前是否有其他普通強化已滿級
                            // 如果這是第一個達到滿級的普通強化，則需要提供傳奇強化選擇
                            // 注意：這裡我們無法直接知道之前的狀態，所以我們假設
                            // 如果這個Buff剛好達到滿級，且沒有其他普通強化滿級，則需要提供傳奇強化
                            bool hasOtherMaxedNormalBuff = false;
                            foreach (var normalBuffType in GameConstants.NORMAL_BUFFS)
                            {
                                if (normalBuffType != buffType && PlayerManager.Instance.IsBuffMaxed(normalBuffType))
                                {
                                    hasOtherMaxedNormalBuff = true;
                                    break;
                                }
                            }
                            
                            // 如果沒有其他普通強化滿級，則需要提供傳奇強化選擇
                            shouldWaitForLegendaryBuff = !hasOtherMaxedNormalBuff;
                        }
                    }
                }
                
                if (!shouldWaitForLegendaryBuff)
                {
                    // 恢復遊戲
                    ChangeGameState(GameState.Playing);
                }
                else
                {
                    Debug.Log("[GameManager] 檢測到普通強化達到滿級，等待傳奇強化選擇...");
                    // 不改變狀態，讓RoguelikeMenu處理傳奇強化選擇
                }
            }
        }
        
        /// <summary>
        /// 遊戲結束
        /// </summary>
        public void GameOver()
        {
            ChangeGameState(GameState.GameOver);
        }
        
        /// <summary>
        /// 獲取隨機Buff選項（用於Roguelike選單）
        /// </summary>
        public BuffDataSO[] GetRandomBuffOptions(int count = 3)
        {
            var options = new System.Collections.Generic.List<BuffDataSO>();
            
            // 檢查是否有普通強化已達滿級
            bool hasMaxedNormalBuff = PlayerManager.Instance != null && PlayerManager.Instance.HasMaxedNormalBuff();
            
            // 過濾普通強化（排除已滿級和技能）
            var availableNormalBuffs = new System.Collections.Generic.List<BuffDataSO>();
            if (normalBuffs != null)
            {
                foreach (var buff in normalBuffs)
                {
                    if (buff == null) continue;
                    
                    // 排除技能（Execution和Repair）
                    if (buff.buffType == BuffType.Execution || buff.buffType == BuffType.Repair)
                        continue;
                    
                    // 檢查是否已達滿級
                    if (PlayerManager.Instance != null && PlayerManager.Instance.IsBuffMaxed(buff.buffType))
                        continue; // 已滿級的普通強化不再出現
                    
                    availableNormalBuffs.Add(buff);
                }
            }
            
            // 如果有普通強化滿級，直接使用legendaryBuffs陣列中的所有內容（不過濾）
            if (hasMaxedNormalBuff && legendaryBuffs != null && legendaryBuffs.Length > 0)
            {
                // 過濾掉null，但保留所有其他內容（包括Execution和Repair）
                var availableLegendaryBuffs = new System.Collections.Generic.List<BuffDataSO>();
                foreach (var buff in legendaryBuffs)
                {
                    if (buff != null)
                    {
                        availableLegendaryBuffs.Add(buff);
                    }
                }
                
                // 如果不足 count 個就顯示全部，否則顯示 count 個
                // 如果傳奇強化數量 <= count，直接顯示全部（不隨機選擇）
                if (availableLegendaryBuffs.Count <= count)
                {
                    // 直接添加所有傳奇強化
                    options.AddRange(availableLegendaryBuffs);
                }
                else
                {
                    // 如果超過 count 個，才隨機選擇 count 個
                    var selectedLegendary = SelectRandomBuff(availableLegendaryBuffs, count);
                    options.AddRange(selectedLegendary);
                }
                // 不補充普通強化，即使選項不足 count 個
            }
            // 如果沒有普通強化滿級，全部從普通強化中選擇
            else if (!hasMaxedNormalBuff && availableNormalBuffs.Count > 0)
            {
                int normalCount = Mathf.Min(count, availableNormalBuffs.Count);
                var selectedNormal = SelectRandomBuff(availableNormalBuffs, normalCount);
                options.AddRange(selectedNormal);
            }
            
            return options.ToArray();
        }
        
        /// <summary>
        /// 從指定列表中基於權重隨機選擇Buff
        /// </summary>
        private System.Collections.Generic.List<BuffDataSO> SelectRandomBuff(
            System.Collections.Generic.List<BuffDataSO> buffList, 
            int count)
        {
            var selected = new System.Collections.Generic.List<BuffDataSO>();
            var tempList = new System.Collections.Generic.List<BuffDataSO>(buffList);
            
            for (int i = 0; i < Mathf.Min(count, tempList.Count); i++)
            {
                // 計算總權重
                float totalWeight = 0f;
                foreach (var buff in tempList)
                {
                    totalWeight += buff.spawnWeight;
                }
                
                if (totalWeight <= 0) break;
                
                // 基於權重隨機選擇
                float randomValue = Random.Range(0f, totalWeight);
                float currentWeight = 0f;
                
                BuffDataSO selectedBuff = tempList[0];
                foreach (var buff in tempList)
                {
                    currentWeight += buff.spawnWeight;
                    if (randomValue <= currentWeight)
                    {
                        selectedBuff = buff;
                        break;
                    }
                }
                
                selected.Add(selectedBuff);
                tempList.Remove(selectedBuff);
            }
            
            return selected;
        }
    }
}

