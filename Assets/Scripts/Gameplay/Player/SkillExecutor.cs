using UnityEngine;
using Tenronis.Data;
using Tenronis.Core;
using Tenronis.Managers;

namespace Tenronis.Gameplay.Player
{
    /// <summary>
    /// 技能執行器 - 處決、修復技能的具體實作
    /// 這個類別被InputManager使用
    /// </summary>
    public static class SkillExecutor
    {
        /// <summary>
        /// 執行處決技能
        /// 清除每列最上面的方塊（削平表面）並發射導彈
        /// </summary>
        public static void ExecuteExecution()
        {
            if (PlayerManager.Instance == null || GridManager.Instance == null) return;
            if (!PlayerManager.Instance.UseExecution()) return;
            
            var stats = PlayerManager.Instance.Stats;
            float volleyMultiplier = 1 + stats.missileExtraCount; // Volley 傷害倍率
            
            // 計算處決的程度等級（不包含消除行數加成）
            int intensityLevel = Mathf.Min(stats.missileExtraCount + Mathf.Min(stats.comboCount / 10, 3), 8);
            
            // 清除每列最上面的方塊（削平表面）
            for (int x = 0; x < GameConstants.BOARD_WIDTH; x++)
            {
                // 從頂部往底部掃描，找到第一個方塊
                for (int y = 0; y < GameConstants.BOARD_HEIGHT; y++)
                {
                    if (GridManager.Instance.IsOccupied(x, y))
                    {
                        GridManager.Instance.RemoveBlock(x, y);
                        
                        // 發射導彈（受 Volley 傷害倍率影響）
                        Vector3 pos = GridManager.Instance.GridToWorldPosition(x, y);
                        float damage = GameConstants.EXECUTION_DAMAGE * volleyMultiplier;
                        
                        CombatManager.Instance?.FireMissile(pos, damage, intensityLevel);
                        
                        break; // 只清除最上面的一個
                    }
                }
            }
            
            GameEvents.TriggerPlayMissileSound();
        }
        
        /// <summary>
        /// 執行修復技能
        /// 填補封閉空洞並檢查消除
        /// </summary>
        public static void ExecuteRepair()
        {
            if (PlayerManager.Instance == null || GridManager.Instance == null) return;
            if (!PlayerManager.Instance.UseRepair()) return;
            
            // 填補封閉空洞
            FillClosedHoles();
            
            // 檢查並消除滿行
            // CheckAndClearRows 內部已經觸發 TriggerRowsCleared
            // 導彈發射由 CombatManager.HandleRowsCleared 統一處理（包含虛無抵銷檢查）
            var clearedRows = GridManager.Instance.CheckAndClearRows();
            
            if (clearedRows.Count > 0)
            {
                Debug.Log($"[SkillExecutor] 修復技能消除了 {clearedRows.Count} 行");
            }
            else
            {
                GameEvents.TriggerPlayImpactSound();
            }
        }
        
        /// <summary>
        /// 執行湮滅技能
        /// 將當前控制的方塊轉換為幽靈穿透狀態
        /// </summary>
        public static void ExecuteAnnihilation()
        {
            if (PlayerManager.Instance == null) return;
            if (Tenronis.Gameplay.Tetromino.TetrominoController.Instance == null) return;
            
            // 檢查是否已經在湮滅狀態
            if (Tenronis.Gameplay.Tetromino.TetrominoController.Instance.IsInAnnihilationState)
            {
                Debug.Log("[SkillExecutor] 已經處於湮滅狀態！");
                return;
            }
            
            // 檢查是否有活躍方塊
            if (!Tenronis.Gameplay.Tetromino.TetrominoController.Instance.IsActive)
            {
                Debug.Log("[SkillExecutor] 沒有活躍方塊可以進入湮滅狀態！");
                return;
            }
            
            // 消耗CP
            if (!PlayerManager.Instance.UseAnnihilation()) return;
            
            // 進入幽靈穿透狀態
            Tenronis.Gameplay.Tetromino.TetrominoController.Instance.EnterAnnihilationState();
            
            Debug.Log("[SkillExecutor] 湮滅技能啟動！方塊進入幽靈穿透狀態");
        }
        
        /// <summary>
        /// 填補封閉空洞（BFS演算法）
        /// </summary>
        private static void FillClosedHoles()
        {
            if (GridManager.Instance == null || PlayerManager.Instance == null) return;
            
            var grid = GridManager.Instance.Grid;
            int defenseLevel = PlayerManager.Instance.Stats.blockDefenseLevel;
            
            // 標記已訪問的空格
            bool[,] visited = new bool[GameConstants.BOARD_HEIGHT, GameConstants.BOARD_WIDTH];
            
            // 從邊界開始BFS標記所有連通的空格
            System.Collections.Generic.Queue<Vector2Int> queue = new System.Collections.Generic.Queue<Vector2Int>();
            
            // 從頂部所有空格開始
            for (int x = 0; x < GameConstants.BOARD_WIDTH; x++)
            {
                if (grid[0, x] == null && !visited[0, x])
                {
                    queue.Enqueue(new Vector2Int(x, 0));
                    visited[0, x] = true;
                }
            }
            
            // BFS搜尋
            Vector2Int[] directions = { 
                Vector2Int.up, 
                Vector2Int.down, 
                Vector2Int.left, 
                Vector2Int.right 
            };
            
            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                
                foreach (var dir in directions)
                {
                    Vector2Int next = current + dir;
                    
                    if (GridManager.Instance.IsValidPosition(next.x, next.y) &&
                        !visited[next.y, next.x] &&
                        grid[next.y, next.x] == null)
                    {
                        visited[next.y, next.x] = true;
                        queue.Enqueue(next);
                    }
                }
            }
            
            // 填補未訪問的空格（封閉空洞）
            bool anyFilled = false;
            for (int y = 0; y < GameConstants.BOARD_HEIGHT; y++)
            {
                for (int x = 0; x < GameConstants.BOARD_WIDTH; x++)
                {
                    if (grid[y, x] == null && !visited[y, x])
                    {
                        // 填補空洞（批量操作，先不觸發事件）
                        int blockHp = GameConstants.BASE_BLOCK_HP + defenseLevel;
                        BlockData block = new BlockData(BlockColor.Gray, blockHp, blockHp);
                        GridManager.Instance.SetBlock(x, y, block, triggerEvent: false);
                        anyFilled = true;
                    }
                }
            }
            
            // 如果有填補任何方塊，統一觸發一次事件
            if (anyFilled)
            {
                GameEvents.TriggerGridChanged();
            }
        }
    }
}

