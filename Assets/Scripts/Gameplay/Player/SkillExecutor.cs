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
            int missileCount = 1 + stats.missileExtraCount;
            
            // 清除每列最上面的方塊（削平表面）
            for (int x = 0; x < GameConstants.BOARD_WIDTH; x++)
            {
                // 從頂部往底部掃描，找到第一個方塊
                for (int y = 0; y < GameConstants.BOARD_HEIGHT; y++)
                {
                    if (GridManager.Instance.IsOccupied(x, y))
                    {
                        GridManager.Instance.RemoveBlock(x, y);
                        
                        // 發射導彈
                        Vector3 pos = GridManager.Instance.GridToWorldPosition(x, y);
                        float damage = GameConstants.EXECUTION_DAMAGE;
                        
                        for (int i = 0; i < missileCount; i++)
                        {
                            CombatManager.Instance?.FireMissile(
                                pos + Vector3.up * (i * 0.2f), 
                                damage
                            );
                        }
                        
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
            var clearedRows = GridManager.Instance.CheckAndClearRows();
            
            if (clearedRows.Count > 0)
            {
                GameEvents.TriggerRowsCleared(clearedRows.Count);
                
                var stats = PlayerManager.Instance.Stats;
                int missileCount = 1 + stats.missileExtraCount;
                
                // 發射導彈
                foreach (int row in clearedRows)
                {
                    for (int x = 0; x < GameConstants.BOARD_WIDTH; x++)
                    {
                        Vector3 pos = GridManager.Instance.GridToWorldPosition(x, row);
                        float damage = GameConstants.REPAIR_DAMAGE;
                        
                        for (int i = 0; i < missileCount; i++)
                        {
                            CombatManager.Instance?.FireMissile(
                                pos + Vector3.up * (i * 0.2f), 
                                damage
                            );
                        }
                    }
                }
                
                GameEvents.TriggerPlayMissileSound();
            }
            else
            {
                GameEvents.TriggerPlayImpactSound();
            }
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
            for (int y = 0; y < GameConstants.BOARD_HEIGHT; y++)
            {
                for (int x = 0; x < GameConstants.BOARD_WIDTH; x++)
                {
                    if (grid[y, x] == null && !visited[y, x])
                    {
                        // 填補空洞
                        int blockHp = GameConstants.BASE_BLOCK_HP + defenseLevel;
                        BlockData block = new BlockData(BlockColor.Gray, blockHp, blockHp);
                        GridManager.Instance.SetBlock(x, y, block);
                    }
                }
            }
        }
    }
}

