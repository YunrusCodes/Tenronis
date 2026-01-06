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
        private StageSetSO currentTheme = null;
        private List<StageDataSO> currentStages = null;
        private int currentStageIndex = 0;
        private int pendingBuffCount = 0;
        private float damageAccumulator = 0f;
        private int rogueRequirement = GameConstants.INITIAL_ROGUE_REQUIREMENT;
        
        // 屬性
        public GameState CurrentState => currentState;
        public StageSetSO CurrentTheme => currentTheme;
        public StageDataSO CurrentStage => currentStages != null && currentStageIndex < currentStages.Count ? currentStages[currentStageIndex] : null;
        public StageDataSO NextStage => currentStages != null && currentStageIndex + 1 < currentStages.Count ? currentStages[currentStageIndex + 1] : null;
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
        /// 開始遊戲 - 指定主題
        /// </summary>
        /// <param name="themeIndex">主題索引 (在 allThemes 中的位置)</param>
        public void StartGame(int themeIndex)
        {
            Debug.Log($"=== [GameManager] StartGame(Theme: {themeIndex}) 開始執行 ===");
            
            if (themeIndex < 0 || themeIndex >= allThemes.Count)
            {
                Debug.LogError($"[GameManager] 無效的主題索引: {themeIndex}，範圍: 0-{allThemes.Count - 1}");
                return;
            }
            
            currentTheme = allThemes[themeIndex];
            
            // 從主題中獲取關卡列表
            currentStages = currentTheme.GetStages();
            
            if (currentStages == null || currentStages.Count == 0)
            {
                Debug.LogError($"[GameManager] 主題 {currentTheme.themeName} 沒有設定關卡！");
                return;
            }
            
            Debug.Log($"[GameManager] 選擇主題：{currentTheme.themeName}，關卡數：{currentStages.Count}");
            
            // 重置遊戲數據
            currentStageIndex = 0;
            pendingBuffCount = 0;
            damageAccumulator = 0f;
            rogueRequirement = GameConstants.INITIAL_ROGUE_REQUIREMENT;
            
            Debug.Log($"[GameManager] 遊戲數據已重置 - Stage: {currentStageIndex}, Buffs: {pendingBuffCount}, Total Stages: {TotalStages}");
            
            // ⭐ 新增：給予第一關的獎勵卡牌
            int firstStageReward = currentStages[0].rewardBuffCount;
            if (firstStageReward > 0)
            {
                pendingBuffCount += firstStageReward;
                Debug.Log($"[GameManager] 準備挑戰第一關！獲得 {firstStageReward} 張升級卡牌");
                
                // 如果有獎勵，先進入升級介面
                ChangeGameState(GameState.LevelUp);
            }
            else
            {
                // 沒有獎勵，直接開始遊戲
                ChangeGameState(GameState.Playing);
            }
            
            Debug.Log("=== [GameManager] StartGame() 執行完成 ===");
        }
        
        /// <summary>
        /// 重新開始遊戲（使用當前主題）
        /// </summary>
        public void RestartGame()
        {
            if (currentTheme != null)
            {
                // 找到當前主題的索引
                int themeIndex = allThemes.IndexOf(currentTheme);
                if (themeIndex != -1)
                {
                    StartGame(themeIndex);
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
            // 恢復CP至全滿
            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.RestoreCp();
                Debug.Log($"[GameManager] 關卡完成，CP已恢復至全滿");
            }
            
            // ⭐ 關鍵改變：先增加關卡索引
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
                Debug.Log($"[GameManager] 所有關卡完成！");
                ChangeGameState(GameState.Victory);
            }
            else
            {
                // ⭐ 關鍵改變：獲取下一關（當前索引）的獎勵卡牌
                int stageReward = currentStages[currentStageIndex].rewardBuffCount;
                if (stageReward > 0)
                {
                    pendingBuffCount += stageReward;
                    Debug.Log($"[GameManager] 準備挑戰關卡 {currentStageIndex + 1}/{currentStages.Count}！獲得 {stageReward} 張升級卡牌，總計: {pendingBuffCount}");
                }
                
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
            
            // 傳奇強化現在是自動獎勵，說明頁面會由 RoguelikeMenu 控制
            // 不要在這裡立即改變遊戲狀態，讓 RoguelikeMenu 在說明頁面確認後再處理
            // 這樣可以確保玩家看到說明頁面後才開始遊戲
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
        
        /// <summary>
        /// 自動選擇一個傳奇強化作為獎勵（當普通強化達到滿級時）
        /// </summary>
        public BuffDataSO GetRandomLegendaryBuffReward()
        {
            if (legendaryBuffs == null || legendaryBuffs.Length == 0)
            {
                Debug.LogWarning("[GameManager] 沒有可用的傳奇強化！");
                return null;
            }
            
            // 過濾出可用的傳奇強化（排除已滿級和null）
            var availableLegendaryBuffs = new List<BuffDataSO>();
            foreach (var buff in legendaryBuffs)
            {
                if (buff != null)
                {
                    // 檢查是否已達滿級（對於有上限的傳奇強化）
                    if (PlayerManager.Instance != null && PlayerManager.Instance.IsBuffMaxed(buff.buffType))
                        continue;
                        
                    availableLegendaryBuffs.Add(buff);
                }
            }
            
            if (availableLegendaryBuffs.Count == 0)
            {
                Debug.LogWarning("[GameManager] 所有傳奇強化都已達滿級！");
                return null;
            }
            
            // 基於權重隨機選擇一個
            var selected = SelectRandomBuff(availableLegendaryBuffs, 1);
            return selected.Count > 0 ? selected[0] : null;
        }
    }
}
