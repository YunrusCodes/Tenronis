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
        
        [Header("Buff數據")]
        [SerializeField] private BuffDataSO[] availableBuffs;
        
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
        public BuffDataSO[] AvailableBuffs => availableBuffs;
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
                // 恢復遊戲
                ChangeGameState(GameState.Playing);
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
            if (availableBuffs == null || availableBuffs.Length == 0)
                return new BuffDataSO[0];
            
            // 基於權重的隨機選擇
            var options = new System.Collections.Generic.List<BuffDataSO>();
            var tempBuffs = new System.Collections.Generic.List<BuffDataSO>(availableBuffs);
            
            for (int i = 0; i < Mathf.Min(count, tempBuffs.Count); i++)
            {
                float totalWeight = 0f;
                foreach (var buff in tempBuffs)
                {
                    totalWeight += buff.spawnWeight;
                }
                
                float randomValue = Random.Range(0f, totalWeight);
                float currentWeight = 0f;
                
                BuffDataSO selectedBuff = tempBuffs[0];
                foreach (var buff in tempBuffs)
                {
                    currentWeight += buff.spawnWeight;
                    if (randomValue <= currentWeight)
                    {
                        selectedBuff = buff;
                        break;
                    }
                }
                
                options.Add(selectedBuff);
                tempBuffs.Remove(selectedBuff);
            }
            
            return options.ToArray();
        }
    }
}

