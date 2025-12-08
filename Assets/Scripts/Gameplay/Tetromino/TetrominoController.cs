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
        
        [Header("腐蝕符號圖片")]
        [SerializeField] private Sprite explosiveSymbol; // 爆炸型符紋
        [SerializeField] private Sprite voidSymbol; // 虛無型符紋
        
        [Header("湮滅方塊貼圖")]
        [SerializeField] private Sprite annihilationBlockSprite; // 湮滅狀態專用貼圖
        
        // 當前方塊
        private TetrominoShape currentShape;
        private Vector2Int currentPosition;
        private int[,] currentRotation;
        private int currentRotationState; // 0=0°, 1=90°, 2=180°, 3=270°
        
        // 下一個方塊
        private TetrominoShape nextShape;
        
        // 方塊腐化系統：記錄被腐化的格子及其類型
        // Key: "x,y" 格式，Value: BlockType
        private Dictionary<string, BlockType> currentCorruptedBlocks = new Dictionary<string, BlockType>();
        private Dictionary<string, BlockType> nextCorruptedBlocks = new Dictionary<string, BlockType>();
        
        // 儲存方塊系統（4個位置：A、S、D、F）
        private TetrominoShape?[] heldPieces = new TetrominoShape?[4];
        private bool[] canHoldSlot = new bool[4]; // 每個槽位在當前方塊落下前可以使用一次
        private int unlockedSlots = 0; // 已解鎖的槽位數量（初始為0）
        
        // 視覺預覽
        private List<GameObject> previewBlocks;
        private List<GameObject> ghostBlocks;
        
        // 遊戲狀態
        private float dropTimer;
        private bool isActive;
        
        // 湮滅技能狀態
        private bool isInAnnihilationState = false;
        
        // Lock Delay 機制
        [Header("Lock Delay 設定")]
        [SerializeField] private float lockDelay = 0.5f;      // 鎖定延遲（秒）
        [SerializeField] private int maxLockResets = 15;      // 最大重置次數
        
        private bool isGrounded = false;   // 是否觸地
        private float lockTimer = 0f;      // 鎖定計時器
        private int lockResetCount = 0;    // 已重置次數
        
        // 屬性
        public TetrominoShape NextShape => nextShape;
        public Vector2Int CurrentPosition => currentPosition;
        public bool IsActive => isActive;
        public TetrominoShape?[] HeldPieces => heldPieces;
        public bool[] CanHoldSlot => canHoldSlot;
        public int UnlockedSlots => unlockedSlots;
        public bool IsInAnnihilationState => isInAnnihilationState;
        
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
            
            // Lock Delay 計時
            if (isGrounded)
            {
                lockTimer += Time.deltaTime;
                if (lockTimer >= lockDelay)
                {
                    LockPiece();
                }
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
        
        // 將下個方塊的腐化信息轉移到當前方塊
        currentCorruptedBlocks.Clear();
        foreach (var kvp in nextCorruptedBlocks)
        {
            currentCorruptedBlocks[kvp.Key] = kvp.Value;
        }
        nextCorruptedBlocks.Clear();
        
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
            
            // 重置 Lock Delay 狀態
            isGrounded = false;
            lockTimer = 0f;
            lockResetCount = 0;
            
            // 重置湮滅狀態
            isInAnnihilationState = false;
            
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
            
            // 檢查該槽位是否已解鎖
            if (slotIndex >= unlockedSlots)
            {
                Debug.Log($"[TetrominoController] 槽位 {slotIndex} 尚未解鎖");
                return;
            }
            
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
                
                // 重置 Lock Delay 狀態
                isGrounded = false;
                lockTimer = 0f;
                lockResetCount = 0;
                
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
                
                // 重置 Lock Delay 狀態
                isGrounded = false;
                lockTimer = 0f;
                lockResetCount = 0;
                
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
                
                // 成功移動後，重置 Lock Delay（如果在觸地狀態）
                ResetLockDelay();
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
                
                // 成功移動後，重置 Lock Delay（如果在觸地狀態）
                ResetLockDelay();
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
                
                // 湮滅狀態下檢查是否觸地（任何格子超出底部）
                if (isInAnnihilationState && IsAnnihilationTouchingGround())
                {
                    // 觸地時往上移一格再觸發效果
                    currentPosition += Vector2Int.down; // Unity Y軸：down = -1 = 往上移
                    Debug.Log("[TetrominoController] 湮滅方塊觸地，往上移一格後觸發湮滅效果");
                    ExecuteAnnihilationHardDrop();
                    return;
                }
                
                // 離開地面，重置 Lock Delay 狀態
                if (isGrounded)
                {
                    isGrounded = false;
                    lockTimer = 0f;
                    lockResetCount = 0;
                }
            }
            else
            {
                // 方塊觸地，進入 Lock Delay
                if (!isGrounded)
                {
                    isGrounded = true;
                    lockTimer = 0f;
                    lockResetCount = 0;
                    Debug.Log($"[Lock Delay] 方塊觸地，開始鎖定計時（{lockDelay}秒）");
                }
            }
        }
        
        /// <summary>
        /// 檢查湮滅狀態下方塊是否觸地（任何格子超出網格底部）
        /// </summary>
        private bool IsAnnihilationTouchingGround()
        {
            if (currentRotation == null) return false;
            
            for (int y = 0; y < currentRotation.GetLength(0); y++)
            {
                for (int x = 0; x < currentRotation.GetLength(1); x++)
                {
                    if (currentRotation[y, x] != 0)
                    {
                        int gridY = currentPosition.y + y;
                        // 如果任何格子超出網格底部，視為觸地
                        if (gridY >= GameConstants.BOARD_HEIGHT)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        
        /// <summary>
        /// 旋轉（使用完整 SRS 系統）
        /// </summary>
        public void Rotate()
        {
            if (!isActive) return;
            
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
                    
                    // 旋轉腐化信息的座標
                    RotateCorruptedBlocks();
                    
                    GameEvents.TriggerPlayRotateSound();
                    UpdateVisual();
                    
                    // 成功旋轉後，重置 Lock Delay（如果在觸地狀態）
                    ResetLockDelay();
                    
                    Debug.Log($"[SRS] 旋轉成功！方塊: {currentShape.type}, 狀態: {currentRotationState}, 偏移: {offset}");
                    return;
                }
            }
            
            // 所有測試都失敗
            Debug.Log($"[SRS] 旋轉失敗！方塊: {currentShape.type}");
            UpdateVisual();
        }
        
        /// <summary>
        /// 硬降落（立即鎖定，忽略 Lock Delay）
        /// </summary>
        public void HardDrop()
        {
            if (!isActive) return;
            
            // 湮滅狀態下的硬降：執行破壞邏輯
            if (isInAnnihilationState)
            {
                ExecuteAnnihilationHardDrop();
                return;
            }
            
            while (!CheckCollision(currentPosition + Vector2Int.up, currentRotation))
            {
                currentPosition += Vector2Int.up;
            }
            
            // 硬降立即鎖定，不使用 Lock Delay
            LockPiece();
        }
        
        /// <summary>
        /// 湮滅狀態下的硬降：破壞重疊區域的方塊
        /// </summary>
        private void ExecuteAnnihilationHardDrop()
        {
            if (currentRotation == null) return;
            
            int destroyedCount = 0;
            var stats = PlayerManager.Instance?.Stats;
            if (stats == null) return;
            
            float volleyMultiplier = 1 + stats.missileExtraCount; // Volley 傷害倍率
            float burstBonus = stats.burstLevel * stats.comboCount * GameConstants.BURST_DAMAGE_MULTIPLIER;
            float damage = (GameConstants.BASE_MISSILE_DAMAGE + burstBonus) * volleyMultiplier;
            
            // 遍歷方塊的每個格子，檢查重疊
            for (int y = 0; y < currentRotation.GetLength(0); y++)
            {
                for (int x = 0; x < currentRotation.GetLength(1); x++)
                {
                    if (currentRotation[y, x] != 0)
                    {
                        int gridX = currentPosition.x + x;
                        int gridY = currentPosition.y + y;
                        
                        // 檢查是否有重疊的方塊
                        if (GridManager.Instance.IsValidPosition(gridX, gridY) &&
                            GridManager.Instance.IsOccupied(gridX, gridY))
                        {
                            // 破壞該方塊
                            GridManager.Instance.RemoveBlock(gridX, gridY);
                            destroyedCount++;
                            
                            // 發射導彈（受 Volley 傷害倍率影響）
                            Vector3 pos = GridManager.Instance.GridToWorldPosition(gridX, gridY);
                            CombatManager.Instance?.FireMissile(pos, damage);
                        }
                    }
                }
            }
            
            // 如果破壞了至少一格，計為一次 Combo
            if (destroyedCount > 0)
            {
                PlayerManager.Instance?.OnAnnihilationDestroy();
                GameEvents.TriggerPlayMissileSound();
                Debug.Log($"[TetrominoController] 湮滅硬降破壞了 {destroyedCount} 個方塊，發射 {destroyedCount} 發導彈（傷害倍率 x{volleyMultiplier}）");
            }
            
            // 退出湮滅狀態
            ExitAnnihilationState();
            
            // 清除視覺並生成新方塊（方塊被消耗）
            isActive = false;
            ClearVisual();
            
            // 播放特殊音效
            GameEvents.TriggerPlayLockSound();
            
            // 生成下一個方塊
            Invoke(nameof(SpawnPiece), 0.1f);
        }
        
        /// <summary>
        /// 鎖定方塊
        /// </summary>
        private void LockPiece()
        {
            if (!isActive) return;
            
            isActive = false;
            isGrounded = false;
            lockTimer = 0f;
            lockResetCount = 0;
            
            Debug.Log($"[Lock Delay] 方塊已鎖定");
            
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
        /// 進入湮滅幽靈穿透狀態
        /// </summary>
        public void EnterAnnihilationState()
        {
            if (!isActive) return;
            if (isInAnnihilationState) return;
            
            isInAnnihilationState = true;
            
            // 取消腐化（湮滅狀態可完全無效化下一次腐化）
            currentCorruptedBlocks.Clear();
            
            Debug.Log("[TetrominoController] 進入湮滅幽靈穿透狀態");
            
            // 更新視覺（降低透明度）
            UpdateVisual();
        }
        
        /// <summary>
        /// 退出湮滅狀態
        /// </summary>
        private void ExitAnnihilationState()
        {
            isInAnnihilationState = false;
            Debug.Log("[TetrominoController] 退出湮滅狀態");
        }
        
        /// <summary>
        /// 重置 Lock Delay（在成功移動或旋轉後調用）
        /// </summary>
        private void ResetLockDelay()
        {
            if (isGrounded && lockResetCount < maxLockResets)
            {
                lockTimer = 0f;
                lockResetCount++;
                Debug.Log($"[Lock Delay] 重置計時器（第 {lockResetCount}/{maxLockResets} 次）");
            }
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
                            // 檢查這個格子是否被腐化
                            string key = $"{x},{y}";
                            BlockType blockType = BlockType.Normal;
                            
                            if (currentCorruptedBlocks.ContainsKey(key))
                            {
                                blockType = currentCorruptedBlocks[key];
                                Debug.Log($"[MergePieceToGrid] 格子 ({x}, {y}) 被腐化為 {blockType}");
                            }
                            
                            // 創建方塊數據（可能是腐化的）
                            BlockData block = new BlockData(currentShape.color, blockHp, blockHp, false, blockType);
                            GridManager.Instance.SetBlock(gridX, gridY, block);
                            Debug.Log($"[MergePieceToGrid] 設置方塊在 ({gridX}, {gridY}), 顏色: {currentShape.color}, 類型: {blockType}");
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
            return CheckCollision(position, shape, false);
        }
        
        /// <summary>
        /// 檢查碰撞（可選擇是否忽略方塊碰撞，用於湮滅狀態）
        /// </summary>
        private bool CheckCollision(Vector2Int position, int[,] shape, bool ignoreBlocks)
        {
            for (int y = 0; y < shape.GetLength(0); y++)
            {
                for (int x = 0; x < shape.GetLength(1); x++)
                {
                    if (shape[y, x] != 0)
                    {
                        int gridX = position.x + x;
                        int gridY = position.y + y;
                        
                        // 邊界檢查（湮滅狀態下只檢查左右邊界，允許穿透底部）
                        if (isInAnnihilationState)
                        {
                            if (gridX < 0 || gridX >= GameConstants.BOARD_WIDTH)
                            {
                                return true;
                            }
                            // 湮滅狀態下不檢查底部和方塊佔用
                            continue;
                        }
                        
                        // 正常狀態：完整邊界檢查
                        if (gridX < 0 || gridX >= GameConstants.BOARD_WIDTH ||
                            gridY < 0 || gridY >= GameConstants.BOARD_HEIGHT)
                        {
                            return true;
                        }
                        
                        // 佔用檢查（除非忽略）
                        if (!ignoreBlocks && GridManager.Instance.IsOccupied(gridX, gridY))
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
        /// 旋轉腐化方塊的座標（順時針90度）
        /// </summary>
        private void RotateCorruptedBlocks()
        {
            if (currentCorruptedBlocks.Count == 0) return;
            
            // 注意：此時 currentRotation 已經是旋轉後的矩陣
            // 旋轉後矩陣尺寸：newRows x newCols
            // 旋轉前矩陣尺寸：oldRows x oldCols，其中 oldRows = newCols, oldCols = newRows
            int newRows = currentRotation.GetLength(0);
            int newCols = currentRotation.GetLength(1);
            int oldRows = newCols;  // 旋轉前的行數 = 旋轉後的列數
            
            // 創建新的腐化信息字典
            Dictionary<string, BlockType> rotatedCorrupted = new Dictionary<string, BlockType>();
            
            foreach (var kvp in currentCorruptedBlocks)
            {
                // 解析原座標（基於旋轉前的矩陣）
                string[] parts = kvp.Key.Split(',');
                int x = int.Parse(parts[0]);
                int y = int.Parse(parts[1]);
                
                // 計算旋轉後的座標（順時針90度）
                // 規則：(x, y) -> (oldRows - 1 - y, x)
                int newX = oldRows - 1 - y;
                int newY = x;
                
                string newKey = $"{newX},{newY}";
                rotatedCorrupted[newKey] = kvp.Value;
                
                Debug.Log($"[RotateCorrupted] ({x},{y}) -> ({newX},{newY}), Type: {kvp.Value}");
            }
            
            // 更新腐化信息
            currentCorruptedBlocks = rotatedCorrupted;
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
            // 湮滅狀態下不計算幽靈位置（避免無限迴圈，且方塊本身已是幽靈形態）
            Vector2Int ghostPosition = currentPosition;
            if (!isInAnnihilationState)
            {
                while (!CheckCollision(ghostPosition + Vector2Int.up, currentRotation))
                {
                    ghostPosition += Vector2Int.up;
                }
            }
            
            // 建立視覺方塊
            // 湮滅狀態下使用較低透明度顯示幽靈形態
            float previewAlpha = isInAnnihilationState ? 0.50f : 1f;
            
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    if (currentRotation[y, x] != 0)
                    {
                        int gridX = currentPosition.x + x;
                        int gridY = currentPosition.y + y;
                        
                        // 檢查這個格子是否被腐化（湮滅狀態下不顯示腐化）
                        string key = $"{x},{y}";
                        BlockType? corruptType = null;
                        if (!isInAnnihilationState && currentCorruptedBlocks.ContainsKey(key))
                        {
                            corruptType = currentCorruptedBlocks[key];
                        }
                        
                        // 建立預覽方塊（當前位置）
                        // 湮滅狀態下可以超出邊界顯示
                        if (isInAnnihilationState || GridManager.Instance.IsValidPosition(gridX, gridY))
                        {
                            GameObject previewBlock = CreateBlockVisual(gridX, gridY, currentShape.color, previewAlpha, corruptType, isInAnnihilationState);
                            previewBlocks.Add(previewBlock);
                        }
                        
                        // 湮滅狀態下不顯示幽靈方塊（因為整個方塊已經是幽靈形態）
                        if (!isInAnnihilationState)
                        {
                            // 建立幽靈方塊（硬降落位置）
                            int ghostX = ghostPosition.x + x;
                            int ghostY = ghostPosition.y + y;
                            
                            if (ghostPosition != currentPosition && 
                                GridManager.Instance.IsValidPosition(ghostX, ghostY))
                            {
                                GameObject ghostBlock = CreateBlockVisual(ghostX, ghostY, currentShape.color, 0.3f, corruptType);
                                ghostBlocks.Add(ghostBlock);
                            }
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 建立方塊視覺
        /// </summary>
        private GameObject CreateBlockVisual(int gridX, int gridY, BlockColor color, float alpha, BlockType? corruptType = null, bool isAnnihilation = false)
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
            Color blockColor;
            if (isAnnihilation)
            {
                // 湮滅狀態：使用專用貼圖或黑色方塊白色外框
                if (annihilationBlockSprite != null)
                {
                    // 使用自定義貼圖
                    spriteRenderer.sprite = annihilationBlockSprite;
                    blockColor = new Color(1f, 1f, 1f, alpha); // 白色（不改變貼圖顏色）
                    spriteRenderer.color = blockColor;
                    spriteRenderer.sortingOrder = 11;
                }
                else
                {
                    // 預設：黑色方塊白色外框
                    blockColor = new Color(0f, 0f, 0f, alpha); // 黑色
                    spriteRenderer.color = blockColor;
                    spriteRenderer.sortingOrder = 11; // 黑色在上層
                    
                    // 創建白色外框（稍大的白色方塊在下層）
                    CreateAnnihilationBorder(blockObj, blockSize, alpha);
                }
            }
            else
            {
                // 所有方塊（包括腐蝕方塊）都使用原本的顏色
                blockColor = GetColorFromBlockColor(color);
                blockColor.a = alpha;
                spriteRenderer.color = blockColor;
                spriteRenderer.sortingOrder = 10;
            }
            
            // 確保方塊在正確的渲染層（非湮滅狀態）
            if (!isAnnihilation)
            {
                spriteRenderer.sortingOrder = 10;
            }
            
            // 如果方塊被腐化，添加符號標記
            if (corruptType.HasValue && (corruptType.Value == BlockType.Explosive || corruptType.Value == BlockType.Void))
            {
                AddCorruptionSymbol(blockObj, corruptType.Value, blockSize, alpha);
            }
            
            return blockObj;
        }
        
        /// <summary>
        /// 創建湮滅方塊的白色外框
        /// </summary>
        private void CreateAnnihilationBorder(GameObject blockObj, float blockSize, float alpha)
        {
            // 創建白色外框物件（作為子物件）
            GameObject borderObj = new GameObject("AnnihilationBorder");
            borderObj.transform.SetParent(blockObj.transform);
            borderObj.transform.localPosition = Vector3.zero;
            
            // 添加 SpriteRenderer
            SpriteRenderer borderRenderer = borderObj.AddComponent<SpriteRenderer>();
            
            // 建立白色方形 Sprite
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            borderRenderer.sprite = sprite;
            
            // 設置白色外框顏色
            borderRenderer.color = new Color(1f, 1f, 1f, alpha);
            
            // 外框比方塊稍大（1.15倍），形成邊框效果
            borderObj.transform.localScale = new Vector3(1.15f, 1.15f, 1f);
            
            // 外框在下層
            borderRenderer.sortingOrder = 10;
        }
        
        /// <summary>
        /// 添加腐化符號標記
        /// </summary>
        private void AddCorruptionSymbol(GameObject blockObj, BlockType corruptType, float blockSize, float alpha)
        {
            // 創建符號圖片物件
            GameObject symbolObj = new GameObject("CorruptionSymbol");
            symbolObj.transform.SetParent(blockObj.transform);
            symbolObj.transform.localPosition = Vector3.zero;
            
            // 添加 SpriteRenderer 組件
            SpriteRenderer symbolRenderer = symbolObj.AddComponent<SpriteRenderer>();
            
            // 設置符號 Sprite
            if (corruptType == BlockType.Explosive)
            {
                symbolRenderer.sprite = explosiveSymbol;
            }
            else // Void
            {
                symbolRenderer.sprite = voidSymbol;
            }
            
            // 設置透明度
            Color color = Color.white;
            color.a = alpha;
            symbolRenderer.color = color;
            
            // 設置渲染順序和大小
            symbolRenderer.sortingOrder = 11; // 確保在方塊上方
            symbolObj.transform.localScale = new Vector3(0.8f, 0.8f, 1f); // 符號稍小於方塊
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
            // 使用統一的溢出處理（50% 當前HP 傷害）
            GridManager.Instance.HandleOverflow();
            
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
                    // 檢查是否是新遊戲開始
                    if (GameManager.Instance != null && GameManager.Instance.CurrentStageIndex == 0)
                    {
                        Debug.Log("[TetrominoController] 新遊戲開始，重置儲存槽位");
                        // 清空所有儲存槽
                        for (int i = 0; i < heldPieces.Length; i++)
                        {
                            heldPieces[i] = null;
                            canHoldSlot[i] = true;
                            GameEvents.TriggerHeldPieceChanged(i);
                        }
                    }
                    
                    // 重置解鎖槽位數量
                    if (PlayerManager.Instance != null)
                    {
                        unlockedSlots = PlayerManager.Instance.SpaceExpansionLevel;
                        Debug.Log($"[TetrominoController] 已解鎖 {unlockedSlots} 個儲存槽位");
                        GameEvents.TriggerHeldSlotStateChanged();
                    }
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
        /// 解鎖槽位（由升級系統調用）
        /// </summary>
        public void UnlockSlot()
        {
            if (unlockedSlots < 4)
            {
                unlockedSlots++;
                Debug.Log($"[TetrominoController] 解鎖槽位 {unlockedSlots}，總計已解鎖: {unlockedSlots}");
                GameEvents.TriggerHeldSlotStateChanged();
            }
        }
        
        /// <summary>
        /// 腐化下個方塊的隨機一格
        /// </summary>
        /// <param name="blockType">要腐化成的方塊類型（Explosive 或 Void）</param>
        public void CorruptNextPiece(BlockType blockType)
        {
            if (blockType != BlockType.Explosive && blockType != BlockType.Void)
            {
                Debug.LogWarning($"[TetrominoController] 無效的腐化類型: {blockType}");
                return;
            }
            
            // 獲取下個方塊的所有非空格子
            List<string> availableBlocks = new List<string>();
            int[,] shape = nextShape.shape;
            
            for (int y = 0; y < shape.GetLength(0); y++)
            {
                for (int x = 0; x < shape.GetLength(1); x++)
                {
                    if (shape[y, x] != 0)
                    {
                        string key = $"{x},{y}";
                        // 只腐化尚未被腐化的格子
                        if (!nextCorruptedBlocks.ContainsKey(key))
                        {
                            availableBlocks.Add(key);
                        }
                    }
                }
            }
            
            // 如果有可腐化的格子，隨機選擇一個
            if (availableBlocks.Count > 0)
            {
                string targetKey = availableBlocks[Random.Range(0, availableBlocks.Count)];
                nextCorruptedBlocks[targetKey] = blockType;
                
                Debug.Log($"[TetrominoController] 下個方塊格子 {targetKey} 被腐化為 {blockType}");
                
                // 觸發下一個方塊已更新事件（用於 UI 預覽更新）
                GameEvents.TriggerNextPieceChanged();
            }
            else
            {
                Debug.LogWarning($"[TetrominoController] 下個方塊沒有可腐化的格子");
            }
        }
        
        /// <summary>
        /// 獲取下個方塊的腐化信息（用於 UI 顯示）
        /// </summary>
        public Dictionary<string, BlockType> GetNextCorruptedBlocks()
        {
            return new Dictionary<string, BlockType>(nextCorruptedBlocks);
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

