using UnityEngine;
using System.Collections.Generic;
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
        
        [Header("主題列表")]
        [Tooltip("所有可用的主題 (Stage Sets)")]
        public List<StageSetSO> allThemes = new List<StageSetSO>();
        
        [Header("普通強化")]
        [SerializeField] private BuffDataSO[] normalBuffs;
        
        [Header("傳奇強化")]
        [SerializeField] private BuffDataSO[] legendaryBuffs;
        
        // 遊戲狀態
        private GameState currentState = GameState.Menu;
        private DifficultyTrack currentDifficulty = DifficultyTrack.Standard;
        private StageSetSO currentTheme = null;
        private List<StageDataSO> currentStages = null;
        private int currentStageIndex = 0;
        private int pendingBuffCount = 0;
        private float damageAccumulator = 0f;
        private int rogueRequirement = GameConstants.INITIAL_ROGUE_REQUIREMENT;
        
        // 屬性
        public GameState CurrentState => currentState;
        public DifficultyTrack CurrentDifficulty => currentDifficulty;
        public StageSetSO CurrentTheme => currentTheme;
        public StageDataSO CurrentStage => currentStages != null && currentStageIndex < currentStages.Count ? currentStages[currentStageIndex] : null;
        public int CurrentStageIndex => currentStageIndex;
        public int TotalStages => currentStages != null ? currentStages.Count : 0;
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
        /// 開始遊戲 - 指定主題與難度
        /// </summary>
        /// <param name="themeIndex">主題索引 (在 allThemes 中的位置)</param>
        /// <param name="difficulty">難度</param>
        public void StartGame(int themeIndex, DifficultyTrack difficulty)
        {
            Debug.Log($"=== [GameManager] StartGame(Theme: {themeIndex}, Diff: {difficulty}) 開始執行 ===");
            
            if (themeIndex < 0 || themeIndex >= allThemes.Count)
            {
                Debug.LogError($"[GameManager] 無效的主題索引: {themeIndex}，範圍: 0-{allThemes.Count - 1}");
                return;
            }
            
            currentTheme = allThemes[themeIndex];
            currentDifficulty = difficulty;
            
            // 根據難度從主題中獲取關卡列表
            currentStages = currentTheme.GetStages(difficulty);
            
            if (currentStages == null || currentStages.Count == 0)
            {
                Debug.LogError($"[GameManager] 主題 {currentTheme.themeName} 在難度 {difficulty} 下沒有設定關卡！");
                return;
            }
            
            Debug.Log($"[GameManager] 選擇主題：{currentTheme.themeName}，難度：{difficulty}，關卡數：{currentStages.Count}");
            
            // 重置遊戲數據
            currentStageIndex = 0;
            pendingBuffCount = 0;
            damageAccumulator = 0f;
            rogueRequirement = GameConstants.INITIAL_ROGUE_REQUIREMENT;
            
            Debug.Log($"[GameManager] 遊戲數據已重置 - Difficulty: {difficulty}, Stage: {currentStageIndex}, Buffs: {pendingBuffCount}, Total Stages: {TotalStages}");
            
            ChangeGameState(GameState.Playing);
            
            Debug.Log("=== [GameManager] StartGame() 執行完成 ===");
        }
        
        /// <summary>
        /// 重新開始遊戲（使用當前主題與難度）
        /// </summary>
        public void RestartGame()
        {
            if (currentTheme != null)
            {
                // 找到當前主題的索引
                int themeIndex = allThemes.IndexOf(currentTheme);
                if (themeIndex != -1)
                {
                    StartGame(themeIndex, currentDifficulty);
                }
                else
                {
                    Debug.LogError("[GameManager] 無法重新開始：當前主題不在列表內");
                    ReturnToMenu();
                }
            }
            else
            {
                Debug.LogError("[GameManager] 無法重新開始：沒有選擇主題");
                ReturnToMenu();
            }
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
            if (currentStages != null && currentStageIndex < currentStages.Count)
            {
                int stageReward = currentStages[currentStageIndex].rewardBuffCount;
                if (stageReward > 0)
                {
                    pendingBuffCount += stageReward;
                    Debug.Log($"[GameManager] 關卡 {currentStageIndex + 1}/{currentStages.Count} 完成！難度: {currentDifficulty}, 獲得 {stageReward} 張升級卡牌，總計: {pendingBuffCount}");
                }
            }
            
            // 恢復CP至全滿
            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.RestoreCp();
                Debug.Log($"[GameManager] 關卡完成，CP已恢復至全滿");
            }
            
            currentStageIndex++;
            
            // 進入下一關時恢復 50% HP（非第一關）
            if (currentStageIndex > 0 && PlayerManager.Instance != null)
            {
                int healAmount = Mathf.FloorToInt(PlayerManager.Instance.Stats.maxHp * 0.5f);
                int oldHp = PlayerManager.Instance.Stats.currentHp;
                PlayerManager.Instance.Heal(healAmount);
                Debug.Log($"[GameManager] 進入關卡 {currentStageIndex + 1}，恢復 {healAmount} HP（50%），HP: {oldHp} → {PlayerManager.Instance.Stats.currentHp}/{PlayerManager.Instance.Stats.maxHp}");
            }
            
            if (currentStages == null || currentStageIndex >= currentStages.Count)
            {
                // 勝利
                Debug.Log($"[GameManager] 所有關卡完成！難度: {currentDifficulty}");
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
        /// 處理Buff選擇
        /// </summary>
        private void HandleBuffSelected(BuffType buffType)
        {
            pendingBuffCount--;
            Debug.Log($"[GameManager] 選擇Buff: {buffType}，剩餘: {pendingBuffCount}");
            
            if (pendingBuffCount <= 0)
            {
                // 檢查是否需要提供傳奇強化選擇
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
                            // 只要普通強化達到滿級，就提供傳奇強化選擇
                            shouldWaitForLegendaryBuff = true;
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
        /// 增加待選Buff數量
        /// </summary>
        public void AddPendingBuffs(int amount)
        {
            pendingBuffCount += amount;
            Debug.Log($"[GameManager] 增加待選Buff: {amount}，當前總計: {pendingBuffCount}");
        }

        /// <summary>
        /// 獲取隨機Buff選項（用於Roguelike選單）
        /// </summary>
        public BuffDataSO[] GetRandomBuffOptions(int count = 3, bool forceLegendary = false)
        {
            var options = new List<BuffDataSO>();
            
            // 過濾普通強化（排除已滿級和技能）
            var availableNormalBuffs = new List<BuffDataSO>();
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
            
            // 決定是否使用傳奇強化池
            bool useLegendaryPool = forceLegendary || availableNormalBuffs.Count == 0;
            
            if (useLegendaryPool)
            {
                if (legendaryBuffs != null && legendaryBuffs.Length > 0)
                {
                    var availableLegendaryBuffs = new List<BuffDataSO>();
                    foreach (var buff in legendaryBuffs)
                    {
                        if (buff != null)
                        {
                            if (PlayerManager.Instance != null && PlayerManager.Instance.IsBuffMaxed(buff.buffType))
                                continue;
                                
                            availableLegendaryBuffs.Add(buff);
                        }
                    }
                    
                    if (availableLegendaryBuffs.Count <= count)
                    {
                        options.AddRange(availableLegendaryBuffs);
                    }
                    else
                    {
                        var selectedLegendary = SelectRandomBuff(availableLegendaryBuffs, count);
                        options.AddRange(selectedLegendary);
                    }
                }
            }
            else
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
        private List<BuffDataSO> SelectRandomBuff(List<BuffDataSO> buffList, int count)
        {
            var selected = new List<BuffDataSO>();
            var tempList = new List<BuffDataSO>(buffList);
            
            int loopCount = Mathf.Min(count, tempList.Count);
            
            for (int i = 0; i < loopCount; i++)
            {
                float totalWeight = 0f;
                foreach (var buff in tempList)
                {
                    totalWeight += buff.spawnWeight;
                }
                
                if (totalWeight <= 0) break;
                
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
