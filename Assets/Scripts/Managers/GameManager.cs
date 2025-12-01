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
        
        [Header("關卡數據 - 三軌難度")]
        [SerializeField] private StageDataSO[] easyStages;
        [SerializeField] private StageDataSO[] normalStages;
        [SerializeField] private StageDataSO[] hardStages;
        
        [Header("普通強化")]
        [SerializeField] private BuffDataSO[] normalBuffs;
        
        [Header("傳奇強化")]
        [SerializeField] private BuffDataSO[] legendaryBuffs;
        
        // 遊戲狀態
        private GameState currentState = GameState.Menu;
        private DifficultyTrack currentDifficulty = DifficultyTrack.Standard;
        private StageDataSO[] currentStages = null;
        private int currentStageIndex = 0;
        private int pendingBuffCount = 0;
        private float damageAccumulator = 0f;
        private int rogueRequirement = GameConstants.INITIAL_ROGUE_REQUIREMENT;
        
        // 屬性
        public GameState CurrentState => currentState;
        public DifficultyTrack CurrentDifficulty => currentDifficulty;
        public StageDataSO CurrentStage => currentStages != null && currentStageIndex < currentStages.Length ? currentStages[currentStageIndex] : null;
        public int CurrentStageIndex => currentStageIndex;
        public int TotalStages => currentStages != null ? currentStages.Length : 0;
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
        /// 開始遊戲 - Easy 難度（Casual 軌道）
        /// </summary>
        public void StartGameEasy()
        {
            StartGame(DifficultyTrack.Casual);
        }
        
        /// <summary>
        /// 開始遊戲 - Normal 難度（Standard 軌道）
        /// </summary>
        public void StartGameNormal()
        {
            StartGame(DifficultyTrack.Standard);
        }
        
        /// <summary>
        /// 開始遊戲 - Hard 難度（Expert 軌道）
        /// </summary>
        public void StartGameHard()
        {
            StartGame(DifficultyTrack.Expert);
        }
        
        /// <summary>
        /// 開始遊戲（內部方法）
        /// </summary>
        private void StartGame(DifficultyTrack difficulty)
        {
            Debug.Log($"=== [GameManager] StartGame({difficulty}) 開始執行 ===");
            
            currentDifficulty = difficulty;
            
            // 根據難度選擇對應的關卡陣列
            switch (difficulty)
            {
                case DifficultyTrack.Casual:
                    currentStages = easyStages;
                    Debug.Log("[GameManager] 選擇難度：Casual（Easy）");
                    break;
                case DifficultyTrack.Standard:
                    currentStages = normalStages;
                    Debug.Log("[GameManager] 選擇難度：Standard（Normal）");
                    break;
                case DifficultyTrack.Expert:
                    currentStages = hardStages;
                    Debug.Log("[GameManager] 選擇難度：Expert（Hard）");
                    break;
                default:
                    currentStages = normalStages;
                    Debug.LogWarning($"[GameManager] 未知難度 {difficulty}，使用 Standard 作為預設");
                    break;
            }
            
            currentStageIndex = 0;
            pendingBuffCount = 0;
            damageAccumulator = 0f;
            rogueRequirement = GameConstants.INITIAL_ROGUE_REQUIREMENT;
            
            Debug.Log($"[GameManager] 遊戲數據已重置 - Difficulty: {difficulty}, Stage: {currentStageIndex}, Buffs: {pendingBuffCount}, Total Stages: {TotalStages}");
            
            ChangeGameState(GameState.Playing);
            
            Debug.Log("=== [GameManager] StartGame() 執行完成 ===");
        }
        
        /// <summary>
        /// 重新開始遊戲（使用當前難度）
        /// </summary>
        public void RestartGame()
        {
            StartGame(currentDifficulty);
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
            if (currentStages != null && currentStageIndex < currentStages.Length)
            {
                int stageReward = currentStages[currentStageIndex].rewardBuffCount;
                if (stageReward > 0)
                {
                    pendingBuffCount += stageReward;
                    Debug.Log($"[GameManager] 關卡 {currentStageIndex + 1}/{currentStages.Length} 完成！難度: {currentDifficulty}, 獲得 {stageReward} 張升級卡牌，總計: {pendingBuffCount}");
                }
            }
            
            // 恢復CP至全滿
            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.RestoreCp();
                Debug.Log($"[GameManager] 關卡完成，CP已恢復至全滿");
            }
            
            currentStageIndex++;
            
            if (currentStages == null || currentStageIndex >= currentStages.Length)
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
        /// <param name="count">選項數量</param>
        /// <param name="forceLegendary">是否強制只顯示傳奇強化</param>
        public BuffDataSO[] GetRandomBuffOptions(int count = 3, bool forceLegendary = false)
        {
            var options = new System.Collections.Generic.List<BuffDataSO>();
            
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
            
            // 決定是否使用傳奇強化池
            // 條件1: 強制顯示傳奇強化 (forceLegendary = true)
            // 條件2: 沒有可用的普通強化 (availableNormalBuffs.Count == 0)
            bool useLegendaryPool = forceLegendary || availableNormalBuffs.Count == 0;
            
            if (useLegendaryPool)
            {
                if (legendaryBuffs != null && legendaryBuffs.Length > 0)
                {
                    // 過濾掉null，但保留所有其他內容（包括Execution和Repair）
                    var availableLegendaryBuffs = new System.Collections.Generic.List<BuffDataSO>();
                    foreach (var buff in legendaryBuffs)
                    {
                        if (buff != null)
                        {
                            // 檢查是否已達滿級（針對有上限的傳奇強化，如 TacticalExpansion）
                            if (PlayerManager.Instance != null && PlayerManager.Instance.IsBuffMaxed(buff.buffType))
                                continue;
                                
                            availableLegendaryBuffs.Add(buff);
                        }
                    }
                    
                    // 如果不足 count 個就顯示全部，否則顯示 count 個
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
                }
            }
            else
            {
                // 使用普通強化池
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
            
            // 必須先計算要跑幾次，因為 tempList.Count 會在迴圈中改變
            int loopCount = Mathf.Min(count, tempList.Count);
            
            for (int i = 0; i < loopCount; i++)
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

