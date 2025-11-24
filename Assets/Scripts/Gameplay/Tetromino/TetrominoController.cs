using UnityEngine;
using Tenronis.Data;
using Tenronis.Core;
using Tenronis.Managers;

namespace Tenronis.Gameplay.Tetromino
{
    /// <summary>
    /// 俄羅斯方塊控制器
    /// </summary>
    public class TetrominoController : MonoBehaviour
    {
        public static TetrominoController Instance { get; private set; }
        
        [Header("設定")]
        [SerializeField] private GameObject blockPreviewPrefab;
        
        // 當前方塊
        private TetrominoShape currentShape;
        private Vector2Int currentPosition;
        private int[,] currentRotation;
        
        // 下一個方塊
        private TetrominoShape nextShape;
        
        // 視覺預覽
        private GameObject[] previewBlocks;
        private GameObject[] ghostBlocks;
        
        // 遊戲狀態
        private float dropTimer;
        private bool isActive;
        
        // 屬性
        public TetrominoShape NextShape => nextShape;
        public Vector2Int CurrentPosition => currentPosition;
        
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
            // 訂閱事件
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
            
            // 預先生成下一個方塊
            nextShape = TetrominoDefinitions.GetRandomTetromino();
        }
        
        private void OnDestroy()
        {
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        }
        
        private void Update()
        {
            if (!isActive) return;
            
            // 自動下落
            dropTimer += Time.deltaTime;
            if (dropTimer >= GameConstants.TICK_RATE)
            {
                MoveDown();
                dropTimer = 0f;
            }
        }
        
        /// <summary>
        /// 生成新方塊
        /// </summary>
        public void SpawnPiece()
        {
            currentShape = nextShape;
            nextShape = TetrominoDefinitions.GetRandomTetromino();
            
            // 設置初始位置（網格中央頂部）
            currentPosition = new Vector2Int(GameConstants.BOARD_WIDTH / 2 - 1, 0);
            currentRotation = (int[,])currentShape.shape.Clone();
            
            dropTimer = 0f;
            
            // 檢查是否碰撞（Game Over）
            if (CheckCollision(currentPosition, currentRotation))
            {
                HandleOverflow();
                return;
            }
            
            isActive = true;
            UpdateVisual();
        }
        
        /// <summary>
        /// 向左移動
        /// </summary>
        public void MoveLeft()
        {
            if (!isActive) return;
            
            Vector2Int newPos = currentPosition + Vector2Int.left;
            if (!CheckCollision(newPos, currentRotation))
            {
                currentPosition = newPos;
                UpdateVisual();
            }
        }
        
        /// <summary>
        /// 向右移動
        /// </summary>
        public void MoveRight()
        {
            if (!isActive) return;
            
            Vector2Int newPos = currentPosition + Vector2Int.right;
            if (!CheckCollision(newPos, currentRotation))
            {
                currentPosition = newPos;
                UpdateVisual();
            }
        }
        
        /// <summary>
        /// 向下移動
        /// </summary>
        public void MoveDown()
        {
            if (!isActive) return;
            
            Vector2Int newPos = currentPosition + Vector2Int.up; // Unity Y軸向上
            if (!CheckCollision(newPos, currentRotation))
            {
                currentPosition = newPos;
                UpdateVisual();
            }
            else
            {
                LockPiece();
            }
        }
        
        /// <summary>
        /// 旋轉
        /// </summary>
        public void Rotate()
        {
            if (!isActive) return;
            
            int[,] rotated = RotateMatrix(currentRotation);
            
            // 嘗試旋轉
            if (!CheckCollision(currentPosition, rotated))
            {
                currentRotation = rotated;
                GameEvents.TriggerPlayRotateSound();
            }
            // 嘗試左移旋轉
            else if (!CheckCollision(currentPosition + Vector2Int.left, rotated))
            {
                currentPosition += Vector2Int.left;
                currentRotation = rotated;
                GameEvents.TriggerPlayRotateSound();
            }
            // 嘗試右移旋轉
            else if (!CheckCollision(currentPosition + Vector2Int.right, rotated))
            {
                currentPosition += Vector2Int.right;
                currentRotation = rotated;
                GameEvents.TriggerPlayRotateSound();
            }
            
            UpdateVisual();
        }
        
        /// <summary>
        /// 硬降落
        /// </summary>
        public void HardDrop()
        {
            if (!isActive) return;
            
            while (!CheckCollision(currentPosition + Vector2Int.up, currentRotation))
            {
                currentPosition += Vector2Int.up;
            }
            
            LockPiece();
        }
        
        /// <summary>
        /// 鎖定方塊
        /// </summary>
        private void LockPiece()
        {
            isActive = false;
            
            // 將方塊合併到網格
            MergePieceToGrid();
            
            // 清除視覺
            ClearVisual();
            
            // 觸發事件
            GameEvents.TriggerPieceLocked();
            
            // 生成下一個方塊
            Invoke(nameof(SpawnPiece), 0.1f);
        }
        
        /// <summary>
        /// 合併方塊到網格
        /// </summary>
        private void MergePieceToGrid()
        {
            int defenseLevel = PlayerManager.Instance != null ? PlayerManager.Instance.Stats.blockDefenseLevel : 0;
            int blockHp = GameConstants.BASE_BLOCK_HP + defenseLevel;
            
            for (int y = 0; y < currentRotation.GetLength(0); y++)
            {
                for (int x = 0; x < currentRotation.GetLength(1); x++)
                {
                    if (currentRotation[y, x] != 0)
                    {
                        int gridX = currentPosition.x + x;
                        int gridY = currentPosition.y + y;
                        
                        if (GridManager.Instance.IsValidPosition(gridX, gridY))
                        {
                            BlockData block = new BlockData(currentShape.color, blockHp, blockHp);
                            GridManager.Instance.SetBlock(gridX, gridY, block);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 檢查碰撞
        /// </summary>
        private bool CheckCollision(Vector2Int position, int[,] shape)
        {
            for (int y = 0; y < shape.GetLength(0); y++)
            {
                for (int x = 0; x < shape.GetLength(1); x++)
                {
                    if (shape[y, x] != 0)
                    {
                        int gridX = position.x + x;
                        int gridY = position.y + y;
                        
                        // 邊界檢查
                        if (gridX < 0 || gridX >= GameConstants.BOARD_WIDTH ||
                            gridY < 0 || gridY >= GameConstants.BOARD_HEIGHT)
                        {
                            return true;
                        }
                        
                        // 佔用檢查
                        if (GridManager.Instance.IsOccupied(gridX, gridY))
                        {
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 旋轉矩陣（順時針90度）
        /// </summary>
        private int[,] RotateMatrix(int[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int[,] rotated = new int[cols, rows];
            
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    rotated[x, rows - 1 - y] = matrix[y, x];
                }
            }
            
            return rotated;
        }
        
        /// <summary>
        /// 更新視覺
        /// </summary>
        private void UpdateVisual()
        {
            ClearVisual();
            
            if (!isActive || currentShape == null) return;
            
            // TODO: 建立預覽方塊和幽靈方塊視覺
            // 這部分需要實例化方塊預覽物件
        }
        
        /// <summary>
        /// 清除視覺
        /// </summary>
        private void ClearVisual()
        {
            if (previewBlocks != null)
            {
                foreach (var block in previewBlocks)
                {
                    if (block != null) Destroy(block);
                }
            }
            
            if (ghostBlocks != null)
            {
                foreach (var block in ghostBlocks)
                {
                    if (block != null) Destroy(block);
                }
            }
        }
        
        /// <summary>
        /// 處理溢出
        /// </summary>
        private void HandleOverflow()
        {
            GameEvents.TriggerGridOverflow();
            GameEvents.TriggerPlayExplosionSound();
            
            // 清空網格
            GridManager.Instance.ClearGrid();
            
            // 計算傷害
            int damage = PlayerManager.Instance != null 
                ? Mathf.FloorToInt(PlayerManager.Instance.Stats.currentHp * GameConstants.OVERFLOW_DAMAGE_PERCENT / 100f)
                : GameConstants.OVERFLOW_DAMAGE_PERCENT;
            
            GameEvents.TriggerPlayerDamaged(damage);
            
            // 觸發爆炸傷害（如果有）
            if (PlayerManager.Instance != null && PlayerManager.Instance.Stats.explosionDamage > 0)
            {
                float explosionDamage = PlayerManager.Instance.Stats.explosionDamage;
                GameEvents.TriggerEnemyDamaged(explosionDamage);
                PlayerManager.Instance.ConsumeExplosionCharge();
            }
            
            // 重新生成方塊
            Invoke(nameof(SpawnPiece), 0.5f);
        }
        
        /// <summary>
        /// 處理遊戲狀態變更
        /// </summary>
        private void HandleGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.Playing:
                    SpawnPiece();
                    break;
                    
                case GameState.LevelUp:
                case GameState.GameOver:
                case GameState.Victory:
                    isActive = false;
                    ClearVisual();
                    break;
            }
        }
    }
}

