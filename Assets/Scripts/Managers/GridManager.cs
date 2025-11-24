using UnityEngine;
using Tenronis.Data;
using Tenronis.Core;
using System.Collections.Generic;

namespace Tenronis.Managers
{
    /// <summary>
    /// 網格管理器 - 管理遊戲網格和方塊
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }
        
        [Header("網格設定")]
        [SerializeField] private Transform gridContainer;
        [SerializeField] private GameObject blockPrefab;
        
        [Header("視覺設定")]
        [SerializeField] private float blockSize = 1f;
        [SerializeField] private Vector2 gridOffset = Vector2.zero;
        
        // 網格數據（二維陣列）
        private BlockData[,] grid;
        private GameObject[,] blockObjects; // 視覺物件
        
        // 屬性
        public BlockData[,] Grid => grid;
        public float BlockSize => blockSize;
        public Vector2 GridOffset => gridOffset;
        
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
            InitializeGrid();
            
            // 訂閱事件
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
            GameEvents.OnPieceLocked += HandlePieceLocked;
        }
        
        private void OnDestroy()
        {
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
            GameEvents.OnPieceLocked -= HandlePieceLocked;
        }
        
        /// <summary>
        /// 處理方塊鎖定事件 - 自動檢查消除
        /// </summary>
        private void HandlePieceLocked()
        {
            List<int> clearedRows = CheckAndClearRows();
            
            if (clearedRows.Count > 0)
            {
                Debug.Log($"[GridManager] 消除了 {clearedRows.Count} 行！");
                GameEvents.TriggerRowsCleared(clearedRows.Count);
            }
        }
        
        /// <summary>
        /// 初始化網格
        /// </summary>
        private void InitializeGrid()
        {
            grid = new BlockData[GameConstants.BOARD_HEIGHT, GameConstants.BOARD_WIDTH];
            blockObjects = new GameObject[GameConstants.BOARD_HEIGHT, GameConstants.BOARD_WIDTH];
            
            // 建立視覺網格容器
            if (gridContainer == null)
            {
                gridContainer = new GameObject("GridContainer").transform;
                gridContainer.SetParent(transform);
            }
        }
        
        /// <summary>
        /// 清空網格
        /// </summary>
        public void ClearGrid()
        {
            for (int y = 0; y < GameConstants.BOARD_HEIGHT; y++)
            {
                for (int x = 0; x < GameConstants.BOARD_WIDTH; x++)
                {
                    grid[y, x] = null;
                    
                    if (blockObjects[y, x] != null)
                    {
                        Destroy(blockObjects[y, x]);
                        blockObjects[y, x] = null;
                    }
                }
            }
        }
        
        /// <summary>
        /// 在網格中設置方塊
        /// </summary>
        public void SetBlock(int x, int y, BlockData blockData)
        {
            if (!IsValidPosition(x, y)) return;
            
            grid[y, x] = blockData;
            UpdateBlockVisual(x, y);
        }
        
        /// <summary>
        /// 獲取指定位置的方塊
        /// </summary>
        public BlockData GetBlock(int x, int y)
        {
            if (!IsValidPosition(x, y)) return null;
            return grid[y, x];
        }
        
        /// <summary>
        /// 移除方塊
        /// </summary>
        public void RemoveBlock(int x, int y)
        {
            if (!IsValidPosition(x, y)) return;
            
            grid[y, x] = null;
            
            if (blockObjects[y, x] != null)
            {
                Destroy(blockObjects[y, x]);
                blockObjects[y, x] = null;
            }
        }
        
        /// <summary>
        /// 檢查位置是否有效
        /// </summary>
        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < GameConstants.BOARD_WIDTH && 
                   y >= 0 && y < GameConstants.BOARD_HEIGHT;
        }
        
        /// <summary>
        /// 檢查位置是否被佔用
        /// </summary>
        public bool IsOccupied(int x, int y)
        {
            return IsValidPosition(x, y) && grid[y, x] != null;
        }
        
        /// <summary>
        /// 更新方塊視覺
        /// </summary>
        private void UpdateBlockVisual(int x, int y)
        {
            if (!IsValidPosition(x, y)) return;
            
            BlockData blockData = grid[y, x];
            
            // 如果沒有資料，移除視覺物件
            if (blockData == null)
            {
                if (blockObjects[y, x] != null)
                {
                    Destroy(blockObjects[y, x]);
                    blockObjects[y, x] = null;
                }
                return;
            }
            
            // 建立或更新視覺物件
            if (blockObjects[y, x] == null && blockPrefab != null)
            {
                blockObjects[y, x] = Instantiate(blockPrefab, gridContainer);
            }
            
            if (blockObjects[y, x] != null)
            {
                // 設置位置
                Vector3 worldPos = GridToWorldPosition(x, y);
                blockObjects[y, x].transform.position = worldPos;
                
                // 更新方塊組件
                var blockComponent = blockObjects[y, x].GetComponent<Gameplay.Block.Block>();
                if (blockComponent != null)
                {
                    blockComponent.SetBlockData(blockData);
                }
            }
        }
        
        /// <summary>
        /// 網格座標轉世界座標
        /// </summary>
        public Vector3 GridToWorldPosition(int x, int y)
        {
            return new Vector3(
                x * blockSize + gridOffset.x,
                -y * blockSize + gridOffset.y,  // Y軸反轉：Grid的y增加時，世界座標y減少（向下）
                0f
            );
        }
        
        /// <summary>
        /// 世界座標轉網格座標
        /// </summary>
        public Vector2Int WorldToGridPosition(Vector3 worldPos)
        {
            int x = Mathf.RoundToInt((worldPos.x - gridOffset.x) / blockSize);
            int y = Mathf.RoundToInt((gridOffset.y - worldPos.y) / blockSize);  // Y軸反轉：世界座標y越小，Grid的y越大
            return new Vector2Int(x, y);
        }
        
        /// <summary>
        /// 檢查並消除滿行
        /// </summary>
        public List<int> CheckAndClearRows()
        {
            List<int> normalRows = new List<int>();
            List<int> indestructibleRows = new List<int>();
            
            // 檢查所有行
            for (int y = 0; y < GameConstants.BOARD_HEIGHT; y++)
            {
                bool isFull = true;
                bool isIndestructible = true;
                
                for (int x = 0; x < GameConstants.BOARD_WIDTH; x++)
                {
                    if (grid[y, x] == null)
                    {
                        isFull = false;
                        break;
                    }
                    if (!grid[y, x].isIndestructible)
                    {
                        isIndestructible = false;
                    }
                }
                
                if (isFull)
                {
                    if (isIndestructible)
                        indestructibleRows.Add(y);
                    else
                        normalRows.Add(y);
                }
            }
            
            // 處理消除邏輯
            List<int> rowsToClear = new List<int>();
            
            if (normalRows.Count > 0)
            {
                rowsToClear.AddRange(normalRows);
                
                // 如果有不可摧毀行，也一起消除（防禦機制）
                if (indestructibleRows.Count > 0)
                {
                    rowsToClear.AddRange(indestructibleRows);
                }
            }
            
            // 消除行
            if (rowsToClear.Count > 0)
            {
                ClearRows(rowsToClear);
            }
            
            return rowsToClear;
        }
        
        /// <summary>
        /// 消除指定行
        /// </summary>
        private void ClearRows(List<int> rows)
        {
            if (rows.Count == 0) return;
            
            // 移除所有要清除的行的視覺物件
            foreach (int row in rows)
            {
                for (int x = 0; x < GameConstants.BOARD_WIDTH; x++)
                {
                    RemoveBlock(x, row);
                }
            }
            
            // 建立新網格（從下往上重建）
            // 使用臨時變量來記錄目標行索引
            int targetY = GameConstants.BOARD_HEIGHT - 1; // 從底部開始
            
            // 從底部往頂部遍歷原網格
            for (int sourceY = GameConstants.BOARD_HEIGHT - 1; sourceY >= 0; sourceY--)
            {
                // 跳過要清除的行
                if (rows.Contains(sourceY))
                {
                    continue;
                }
                
                // 如果源行和目標行不同，移動這一行
                if (sourceY != targetY)
                {
                    for (int x = 0; x < GameConstants.BOARD_WIDTH; x++)
                    {
                        grid[targetY, x] = grid[sourceY, x];
                        blockObjects[targetY, x] = blockObjects[sourceY, x];
                        
                        // 更新視覺位置
                        if (blockObjects[targetY, x] != null)
                        {
                            blockObjects[targetY, x].transform.position = GridToWorldPosition(x, targetY);
                        }
                        
                        // 清空源位置
                        grid[sourceY, x] = null;
                        blockObjects[sourceY, x] = null;
                    }
                }
                
                targetY--;
            }
            
            // 清空頂部新增的空行
            for (int y = 0; y <= targetY; y++)
            {
                for (int x = 0; x < GameConstants.BOARD_WIDTH; x++)
                {
                    grid[y, x] = null;
                    blockObjects[y, x] = null;
                }
            }
        }
        
        /// <summary>
        /// 對方塊造成傷害
        /// </summary>
        public bool DamageBlock(int x, int y, int damage)
        {
            if (!IsValidPosition(x, y)) return false;
            
            BlockData block = grid[y, x];
            if (block == null) return false;
            
            // 不可摧毀方塊不受傷害
            if (block.isIndestructible) return false;
            
            block.hp -= damage;
            
            if (block.hp <= 0)
            {
                RemoveBlock(x, y);
                return true;
            }
            else
            {
                UpdateBlockVisual(x, y);
                return false;
            }
        }
        
        /// <summary>
        /// 插入不可摧毀行（Boss技能）
        /// </summary>
        public void InsertIndestructibleRow()
        {
            // 檢查頂行是否有方塊
            bool topRowHasBlocks = false;
            for (int x = 0; x < GameConstants.BOARD_WIDTH; x++)
            {
                if (grid[0, x] != null)
                {
                    topRowHasBlocks = true;
                    break;
                }
            }
            
            if (topRowHasBlocks)
            {
                // 觸發溢出傷害
                GameEvents.TriggerPlayerDamaged(GameConstants.INSERT_ROW_OVERFLOW_DAMAGE);
            }
            
            // 所有行上移
            for (int y = 0; y < GameConstants.BOARD_HEIGHT - 1; y++)
            {
                for (int x = 0; x < GameConstants.BOARD_WIDTH; x++)
                {
                    grid[y, x] = grid[y + 1, x];
                    blockObjects[y, x] = blockObjects[y + 1, x];
                    
                    if (blockObjects[y, x] != null)
                    {
                        blockObjects[y, x].transform.position = GridToWorldPosition(x, y);
                    }
                }
            }
            
            // 在底部插入不可摧毀行
            for (int x = 0; x < GameConstants.BOARD_WIDTH; x++)
            {
                BlockData indestructibleBlock = new BlockData(
                    BlockColor.Garbage,
                    GameConstants.INDESTRUCTIBLE_BLOCK_HP,
                    GameConstants.INDESTRUCTIBLE_BLOCK_HP,
                    true
                );
                SetBlock(x, GameConstants.BOARD_HEIGHT - 1, indestructibleBlock);
            }
        }
        
        /// <summary>
        /// 處理遊戲狀態變更
        /// </summary>
        private void HandleGameStateChanged(GameState newState)
        {
            if (newState == GameState.Playing)
            {
                if (grid == null)
                {
                    InitializeGrid();
                }
            }
        }
        
        /// <summary>
        /// 刷新所有方塊視覺
        /// </summary>
        public void RefreshAllVisuals()
        {
            for (int y = 0; y < GameConstants.BOARD_HEIGHT; y++)
            {
                for (int x = 0; x < GameConstants.BOARD_WIDTH; x++)
                {
                    UpdateBlockVisual(x, y);
                }
            }
        }
        
        /// <summary>
        /// 獲取指定列的最高方塊行位置（從上往下，y越小越高）
        /// 返回-1表示該列沒有方塊
        /// </summary>
        public int GetColumnTopRow(int column)
        {
            if (column < 0 || column >= GameConstants.BOARD_WIDTH) return -1;
            
            // 從上往下（y從0開始）查找第一個方塊
            for (int y = 0; y < GameConstants.BOARD_HEIGHT; y++)
            {
                if (grid[y, column] != null)
                {
                    return y;
                }
            }
            
            return -1; // 該列沒有方塊
        }
        
        /// <summary>
        /// 獲取所有列的高度信息（返回每列的最高行位置）
        /// </summary>
        public Dictionary<int, int> GetAllColumnHeights()
        {
            Dictionary<int, int> columnHeights = new Dictionary<int, int>();
            
            for (int x = 0; x < GameConstants.BOARD_WIDTH; x++)
            {
                int topRow = GetColumnTopRow(x);
                if (topRow >= 0)
                {
                    columnHeights[x] = topRow;
                }
            }
            
            return columnHeights;
        }
        
        /// <summary>
        /// 獲取最高點（方塊堆疊最高的列），返回列索引
        /// 如果有多個相同高度，返回第一個
        /// </summary>
        public int GetHighestColumn()
        {
            Dictionary<int, int> heights = GetAllColumnHeights();
            if (heights.Count == 0) return -1;
            
            int highestColumn = -1;
            int highestRow = GameConstants.BOARD_HEIGHT; // 從最大開始（y越小越高）
            
            foreach (var kvp in heights)
            {
                if (kvp.Value < highestRow)
                {
                    highestRow = kvp.Value;
                    highestColumn = kvp.Key;
                }
            }
            
            return highestColumn;
        }
        
        /// <summary>
        /// 獲取最低點（方塊堆疊最低的列），返回列索引
        /// 如果有多個相同高度，返回第一個
        /// </summary>
        public int GetLowestColumn()
        {
            Dictionary<int, int> heights = GetAllColumnHeights();
            if (heights.Count == 0) return -1;
            
            int lowestColumn = -1;
            int lowestRow = -1; // 從最小開始（y越大越低）
            
            foreach (var kvp in heights)
            {
                if (kvp.Value > lowestRow)
                {
                    lowestRow = kvp.Value;
                    lowestColumn = kvp.Key;
                }
            }
            
            return lowestColumn;
        }
    }
}

