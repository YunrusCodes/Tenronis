using UnityEngine;
using System.Collections.Generic;
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
        private int currentRotationState; // 0=0°, 1=90°, 2=180°, 3=270°
        
        // 下一個方塊
        private TetrominoShape nextShape;
        
        // 儲存方塊系統（4個位置：A、S、D、F）
        private TetrominoShape?[] heldPieces = new TetrominoShape?[4];
        private bool[] canHoldSlot = new bool[4]; // 每個槽位在當前方塊落下前可以使用一次
        
        // 視覺預覽
        private List<GameObject> previewBlocks;
        private List<GameObject> ghostBlocks;
        
        // 遊戲狀態
        private float dropTimer;
        private bool isActive;
        
        // 屬性
        public TetrominoShape NextShape => nextShape;
        public Vector2Int CurrentPosition => currentPosition;
        public bool IsActive => isActive;
        public TetrominoShape?[] HeldPieces => heldPieces;
        public bool[] CanHoldSlot => canHoldSlot;
        
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
            GameEvents.OnGridChanged += HandleGridChanged;
            
            // 預先生成下一個方塊
            nextShape = TetrominoDefinitions.GetRandomTetromino();
        }
        
        private void OnDestroy()
        {
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
            GameEvents.OnGridChanged -= HandleGridChanged;
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
            // 檢查遊戲狀態，只在 Playing 狀態下才生成方塊
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            {
                Debug.Log($"[TetrominoController] 遊戲狀態為 {GameManager.Instance.CurrentState}，取消生成方塊");
                return;
            }
            
            currentShape = nextShape;
            nextShape = TetrominoDefinitions.GetRandomTetromino();
            
            // 觸發下一個方塊已更新事件（用於 UI 預覽）
            GameEvents.TriggerNextPieceChanged();
            
            // 設置初始位置（網格中央頂部）
            currentPosition = new Vector2Int(GameConstants.BOARD_WIDTH / 2 - 1, 0);
            currentRotation = (int[,])currentShape.shape.Clone();
            currentRotationState = 0; // 初始旋轉狀態為 0
            
            dropTimer = 0f;
            
            // 檢查是否碰撞（Game Over）
            if (CheckCollision(currentPosition, currentRotation))
            {
                HandleOverflow();
                return;
            }
            
            isActive = true;
            
            // 新方塊生成後，所有槽位都可以使用一次
            for (int i = 0; i < 4; i++)
            {
                canHoldSlot[i] = true;
            }
            
            // 觸發槽位狀態更新事件
            GameEvents.TriggerHeldSlotStateChanged();
            
            UpdateVisual();
        }
        
        /// <summary>
        /// 儲存/交換方塊（每個槽位在當前方塊落下前可以使用一次）
        /// </summary>
        /// <param name="slotIndex">儲存位置索引 (0=A, 1=S, 2=D, 3=F)</param>
        public void HoldPiece(int slotIndex)
        {
            if (!isActive) return;
            if (slotIndex < 0 || slotIndex >= 4) return;
            
            // 檢查該槽位是否還可以使用
            if (!canHoldSlot[slotIndex])
            {
                Debug.Log($"[TetrominoController] 槽位 {slotIndex} 在本回合已使用過");
                return;
            }
            
            // 清除當前視覺
            ClearVisual();
            
            if (heldPieces[slotIndex] == null)
            {
                // 儲存槽為空：儲存當前方塊，取出下一個方塊
                heldPieces[slotIndex] = currentShape;
                Debug.Log($"[TetrominoController] 儲存方塊到位置 {slotIndex}: {currentShape.name}");
                
                // 觸發儲存更新事件
                GameEvents.TriggerHeldPieceChanged(slotIndex);
                
                // 取出下一個方塊作為當前方塊
                currentShape = nextShape;
                nextShape = TetrominoDefinitions.GetRandomTetromino();
                
                // 觸發下一個方塊已更新事件
                GameEvents.TriggerNextPieceChanged();
                
                // 重置方塊狀態
                currentPosition = new Vector2Int(GameConstants.BOARD_WIDTH / 2 - 1, 0);
                currentRotation = (int[,])currentShape.shape.Clone();
                currentRotationState = 0;
                
                // 檢查碰撞
                if (CheckCollision(currentPosition, currentRotation))
                {
                    HandleOverflow();
                    return;
                }
                
                // 標記該槽位已使用
                canHoldSlot[slotIndex] = false;
                GameEvents.TriggerHeldSlotStateChanged();
                UpdateVisual();
            }
            else
            {
                // 儲存槽不為空：交換當前方塊與儲存方塊
                TetrominoShape temp = currentShape;
                currentShape = heldPieces[slotIndex].Value;
                heldPieces[slotIndex] = temp;
                
                Debug.Log($"[TetrominoController] 交換方塊：{temp.name} ⟷ {currentShape.name}");
                
                // 觸發儲存更新事件
                GameEvents.TriggerHeldPieceChanged(slotIndex);
                
                // 重置方塊狀態
                currentPosition = new Vector2Int(GameConstants.BOARD_WIDTH / 2 - 1, 0);
                currentRotation = (int[,])currentShape.shape.Clone();
                currentRotationState = 0;
                
                // 檢查碰撞
                if (CheckCollision(currentPosition, currentRotation))
                {
                    // 如果交換後的方塊無法生成，遊戲結束
                    HandleOverflow();
                    return;
                }
                
                // 標記該槽位已使用
                canHoldSlot[slotIndex] = false;
                GameEvents.TriggerHeldSlotStateChanged();
                UpdateVisual();
            }
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
        /// 旋轉（使用完整 SRS 系統）
        /// </summary>
        public void Rotate()
        {
            if (!isActive) return;
            
            // O 方塊不旋轉
            if (currentShape.type == TetrominoType.O)
            {
                return;
            }
            
            int[,] rotated = RotateMatrix(currentRotation);
            int nextRotationState = (currentRotationState + 1) % 4;
            
            // 獲取 SRS 踢牆測試表
            Vector2Int[] kickTests = GetSRSKickOffsets(currentShape.type, currentRotationState, nextRotationState);
            
            // 嘗試所有踢牆位置
            foreach (var offset in kickTests)
            {
                Vector2Int testPosition = currentPosition + offset;
                if (!CheckCollision(testPosition, rotated))
                {
                    // 旋轉成功
                    currentPosition = testPosition;
                    currentRotation = rotated;
                    currentRotationState = nextRotationState;
                    GameEvents.TriggerPlayRotateSound();
                    UpdateVisual();
                    
                    Debug.Log($"[SRS] 旋轉成功！方塊: {currentShape.type}, 狀態: {currentRotationState}, 偏移: {offset}");
                    return;
                }
            }
            
            // 所有測試都失敗
            Debug.Log($"[SRS] 旋轉失敗！方塊: {currentShape.type}");
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
            
            // 播放鎖定音效
            GameEvents.TriggerPlayLockSound();
            
            // 觸發事件
            GameEvents.TriggerPieceLocked();
            
            // 生成下一個方塊
            Invoke(nameof(SpawnPiece), 0.1f);
        }
        
        /// <summary>
        /// 強制鎖定當前方塊（不生成新方塊）
        /// 用於在進入選單前保存當前方塊狀態
        /// </summary>
        public void ForceLock()
        {
            if (!isActive) return;
            
            // 檢查方塊是否浮空（下方是否有支撐）
            if (IsFloating())
            {
                Debug.Log("[TetrominoController] 方塊浮空，取消鎖定並保留形狀作為下個方塊");
                
                isActive = false;
                
                // 保留當前形狀作為下一個方塊（不讓玩家損失）
                nextShape = currentShape;
                
                // 清除視覺
                ClearVisual();
                
                // 不鎖定，不觸發事件
                return;
            }
            
            Debug.Log("[TetrominoController] 方塊有支撐，鎖定到網格");
            
            isActive = false;
            
            // 將方塊合併到網格
            MergePieceToGrid();
            
            // 清除視覺
            ClearVisual();
            
            // 觸發事件（會觸發消行檢查）
            GameEvents.TriggerPieceLocked();
            
            // 不生成新方塊，因為要進入選單了
        }
        
        /// <summary>
        /// 檢查方塊是否浮空（下方是否有支撐）
        /// </summary>
        private bool IsFloating()
        {
            if (currentRotation == null) return false;
            
            // 檢查方塊是否可以繼續下移
            // 如果可以下移，表示下方是空的（浮空）
            Vector2Int downPosition = currentPosition + Vector2Int.up; // Y軸向下是+1
            
            return !CheckCollision(downPosition, currentRotation);
        }
        
        /// <summary>
        /// 合併方塊到網格
        /// </summary>
        private void MergePieceToGrid()
        {
            int defenseLevel = PlayerManager.Instance != null ? PlayerManager.Instance.Stats.blockDefenseLevel : 0;
            int blockHp = GameConstants.BASE_BLOCK_HP + defenseLevel;
            
            Debug.Log($"[MergePieceToGrid] 方塊類型: {currentShape.type}, 顏色: {currentShape.color}, HP: {blockHp}");
            
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
                            Debug.Log($"[MergePieceToGrid] 設置方塊在 ({gridX}, {gridY}), 顏色: {currentShape.color}");
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 獲取 SRS 踢牆偏移表
        /// </summary>
        private Vector2Int[] GetSRSKickOffsets(TetrominoType type, int fromState, int toState)
        {
            return Tenronis.Data.SRSData.GetKickOffsets(type, fromState, toState);
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
            
            if (!isActive || currentRotation == null) return;
            if (GridManager.Instance == null) return;
            
            int rows = currentRotation.GetLength(0);
            int cols = currentRotation.GetLength(1);
            
            // 建立 List
            previewBlocks = new List<GameObject>();
            ghostBlocks = new List<GameObject>();
            
            // 計算幽靈方塊位置（硬降落位置）
            Vector2Int ghostPosition = currentPosition;
            while (!CheckCollision(ghostPosition + Vector2Int.up, currentRotation))
            {
                ghostPosition += Vector2Int.up;
            }
            
            // 建立視覺方塊
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    if (currentRotation[y, x] != 0)
                    {
                        int gridX = currentPosition.x + x;
                        int gridY = currentPosition.y + y;
                        
                        // 建立預覽方塊（當前位置）
                        if (GridManager.Instance.IsValidPosition(gridX, gridY))
                        {
                            GameObject previewBlock = CreateBlockVisual(gridX, gridY, currentShape.color, 1f);
                            previewBlocks.Add(previewBlock);
                        }
                        
                        // 建立幽靈方塊（硬降落位置）
                        int ghostX = ghostPosition.x + x;
                        int ghostY = ghostPosition.y + y;
                        
                        if (ghostPosition != currentPosition && 
                            GridManager.Instance.IsValidPosition(ghostX, ghostY))
                        {
                            GameObject ghostBlock = CreateBlockVisual(ghostX, ghostY, currentShape.color, 0.3f);
                            ghostBlocks.Add(ghostBlock);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 建立方塊視覺
        /// </summary>
        private GameObject CreateBlockVisual(int gridX, int gridY, BlockColor color, float alpha)
        {
            GameObject blockObj;
            SpriteRenderer spriteRenderer;
            
            // 使用 blockPreviewPrefab 或建立新的方塊
            if (blockPreviewPrefab != null)
            {
                blockObj = Instantiate(blockPreviewPrefab, GridManager.Instance.transform);
                spriteRenderer = blockObj.GetComponent<SpriteRenderer>();
                
                // 如果預製體沒有 SpriteRenderer，添加一個
                if (spriteRenderer == null)
                {
                    spriteRenderer = blockObj.AddComponent<SpriteRenderer>();
                }
            }
            else
            {
                // 建立簡單的 2D 方塊
                blockObj = new GameObject("PreviewBlock");
                blockObj.transform.SetParent(GridManager.Instance.transform);
                
                // 添加 SpriteRenderer
                spriteRenderer = blockObj.AddComponent<SpriteRenderer>();
                
                // 建立一個簡單的白色方形 Sprite
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
                spriteRenderer.sprite = sprite;
            }
            
            // 設置位置
            Vector3 worldPos = GridManager.Instance.GridToWorldPosition(gridX, gridY);
            blockObj.transform.position = worldPos;
            
            // 設置大小
            float blockSize = GridManager.Instance.BlockSize;
            blockObj.transform.localScale = new Vector3(blockSize * 0.95f, blockSize * 0.95f, 1f);
            
            // 設置顏色
            Color blockColor = GetColorFromBlockColor(color);
            blockColor.a = alpha;
            spriteRenderer.color = blockColor;
            
            // 確保方塊在正確的渲染層
            spriteRenderer.sortingOrder = 10;
            
            return blockObj;
        }
        
        /// <summary>
        /// 將 BlockColor 轉換為 Unity Color
        /// </summary>
        private Color GetColorFromBlockColor(BlockColor blockColor)
        {
            switch (blockColor)
            {
                case BlockColor.Cyan: return Color.cyan;
                case BlockColor.Yellow: return Color.yellow;
                case BlockColor.Purple: return new Color(0.5f, 0f, 0.5f);
                case BlockColor.Green: return Color.green;
                case BlockColor.Red: return Color.red;
                case BlockColor.Blue: return Color.blue;
                case BlockColor.Orange: return new Color(1f, 0.5f, 0f);
                default: return Color.white;
            }
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
                    // 停止方塊活動
                    isActive = false;
                    ClearVisual();
                    
                    // 取消所有待執行的生成方塊指令（防止延遲生成）
                    CancelInvoke(nameof(SpawnPiece));
                    break;
            }
        }
        
        /// <summary>
        /// 處理地形改變（子彈擊中方塊等）
        /// </summary>
        private void HandleGridChanged()
        {
            // 只在方塊活躍時更新視覺
            if (isActive)
            {
                UpdateVisual();
            }
        }
    }
}

