using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Tenronis.Data;
using Tenronis.Gameplay.Tetromino;

namespace Tenronis.UI
{
    /// <summary>
    /// 儲存方塊 UI 顯示（4個儲存位置）
    /// </summary>
    public class HeldPiecesUI : MonoBehaviour
    {
        [Header("儲存位置容器")]
        [SerializeField] private RectTransform[] slotContainers = new RectTransform[4]; // 4個儲存位置
        
        [Header("按鍵提示")]
        [SerializeField] private TextMeshProUGUI[] keyLabels = new TextMeshProUGUI[4]; // 顯示 A、S、D、F 或 Locked
        
        [Header("預覽設定")]
        [SerializeField] private float blockSize = 25f; // UI 方塊大小（像素）
        [SerializeField] private float spacing = 2f; // 方塊間距
        
        [Header("視覺設定")]
        [SerializeField] private bool useSprite = false; // 是否使用 Sprite
        [SerializeField] private Sprite blockSprite; // 方塊 Sprite（可選）
        [SerializeField] private Color emptySlotColor = new Color(0.3f, 0.3f, 0.3f, 0.5f); // 空位顏色
        
        private List<GameObject>[] slotPreviews = new List<GameObject>[4]; // 每個位置的預覽方塊
        private GameState previousGameState = GameState.Menu; // 用於判斷是否為真正開局
        
        private void Start()
        {
            // 初始化
            for (int i = 0; i < 4; i++)
            {
                slotPreviews[i] = new List<GameObject>();
            }
            
            // 訂閱事件
            Core.GameEvents.OnHeldPieceChanged += UpdateSlot;
            Core.GameEvents.OnGameStateChanged += HandleGameStateChanged;
            Core.GameEvents.OnHeldSlotStateChanged += UpdateLockIcons;
            
            // 初始顯示空位
            for (int i = 0; i < 4; i++)
            {
                ShowEmptySlot(i);
            }
            
            // 初始化按鍵標籤（根據解鎖狀態顯示按鍵或 Locked）
            UpdateLockIcons();
        }
        
        private void OnDestroy()
        {
            Core.GameEvents.OnHeldPieceChanged -= UpdateSlot;
            Core.GameEvents.OnGameStateChanged -= HandleGameStateChanged;
            Core.GameEvents.OnHeldSlotStateChanged -= UpdateLockIcons;
        }
        
        /// <summary>
        /// 處理遊戲狀態改變
        /// </summary>
        private void HandleGameStateChanged(GameState newState)
        {
            if (newState == GameState.Playing)
            {
                bool isReturningFromLevelUp = previousGameState == GameState.LevelUp;

                // 僅在真正開局或重新初始化時清空儲存
                if (!isReturningFromLevelUp)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        ShowEmptySlot(i);
                    }
                }
                
                // 更新鎖定狀態（根據實際解鎖數量）
                UpdateLockIcons();
            }

            previousGameState = newState;
        }
        
        /// <summary>
        /// 更新所有槽位的鎖定狀態
        /// </summary>
        private void UpdateLockIcons()
        {
            if (TetrominoController.Instance == null) return;
            
            int unlockedSlots = TetrominoController.Instance.UnlockedSlots;
            string[] keys = { "A", "S", "D", "F" };
            
            for (int i = 0; i < 4; i++)
            {
                // 如果槽位未解鎖，顯示 Locked；否則顯示按鍵
                bool isUnlocked = i < unlockedSlots;
                UpdateKeyLabel(i, isUnlocked ? keys[i] : "Locked");
            }
        }
        
        /// <summary>
        /// 更新特定槽位的按鍵標籤
        /// </summary>
        private void UpdateKeyLabel(int slotIndex, string text)
        {
            if (slotIndex < 0 || slotIndex >= 4) return;
            if (keyLabels[slotIndex] == null) return;
            
            keyLabels[slotIndex].text = text;
        }
        
        /// <summary>
        /// 更新特定儲存位置
        /// </summary>
        private void UpdateSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= 4) return;
            if (TetrominoController.Instance == null) return;
            
            TetrominoShape?[] heldPieces = TetrominoController.Instance.HeldPieces;
            
            if (heldPieces[slotIndex] == null)
            {
                ShowEmptySlot(slotIndex);
            }
            else
            {
                ShowHeldPiece(slotIndex, heldPieces[slotIndex].Value);
            }
        }
        
        /// <summary>
        /// 顯示空位
        /// </summary>
        private void ShowEmptySlot(int slotIndex)
        {
            ClearSlot(slotIndex);
            
            if (slotContainers[slotIndex] == null) return;
            
            // 創建空位提示
            GameObject emptyObj = new GameObject("EmptySlot");
            emptyObj.transform.SetParent(slotContainers[slotIndex], false);
            
            RectTransform rectTransform = emptyObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(blockSize * 2, blockSize * 2);
            rectTransform.anchoredPosition = Vector2.zero;
            
            Image image = emptyObj.AddComponent<Image>();
            image.color = emptySlotColor;
            
            slotPreviews[slotIndex].Add(emptyObj);
        }
        
        /// <summary>
        /// 顯示儲存的方塊
        /// </summary>
        private void ShowHeldPiece(int slotIndex, TetrominoShape shape)
        {
            ClearSlot(slotIndex);
            
            if (slotContainers[slotIndex] == null) return;
            if (shape.shape == null) return;
            
            int rows = shape.shape.GetLength(0);
            int cols = shape.shape.GetLength(1);
            
            // 計算方塊實際佔用的範圍
            int minX = cols, maxX = 0, minY = rows, maxY = 0;
            bool hasBlock = false;
            
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    if (shape.shape[y, x] != 0)
                    {
                        hasBlock = true;
                        minX = Mathf.Min(minX, x);
                        maxX = Mathf.Max(maxX, x);
                        minY = Mathf.Min(minY, y);
                        maxY = Mathf.Max(maxY, y);
                    }
                }
            }
            
            if (!hasBlock) return;
            
            // 計算中心偏移
            int actualWidth = maxX - minX + 1;
            int actualHeight = maxY - minY + 1;
            float offsetX = -(actualWidth - 1) * (blockSize + spacing) * 0.5f;
            float offsetY = (actualHeight - 1) * (blockSize + spacing) * 0.5f;
            
            // 繪製方塊
            Color blockColor = GetColorFromBlockColor(shape.color);
            
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (shape.shape[y, x] != 0)
                    {
                        int relativeX = x - minX;
                        int relativeY = y - minY;
                        
                        GameObject blockObj = CreateUIBlock(slotIndex, relativeX, relativeY, offsetX, offsetY, blockColor);
                        slotPreviews[slotIndex].Add(blockObj);
                    }
                }
            }
        }
        
        /// <summary>
        /// 創建 UI 方塊
        /// </summary>
        private GameObject CreateUIBlock(int slotIndex, int x, int y, float offsetX, float offsetY, Color color)
        {
            GameObject blockObj = new GameObject($"Block_{x}_{y}");
            blockObj.transform.SetParent(slotContainers[slotIndex], false);
            
            RectTransform rectTransform = blockObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(blockSize, blockSize);
            
            // 計算位置
            float posX = offsetX + x * (blockSize + spacing);
            float posY = offsetY - y * (blockSize + spacing);
            rectTransform.anchoredPosition = new Vector2(posX, posY);
            
            // 添加視覺組件
            if (useSprite && blockSprite != null)
            {
                Image image = blockObj.AddComponent<Image>();
                image.sprite = blockSprite;
                image.color = color;
            }
            else
            {
                Image image = blockObj.AddComponent<Image>();
                image.color = color;
            }
            
            return blockObj;
        }
        
        /// <summary>
        /// 清除特定儲存位置的視覺
        /// </summary>
        private void ClearSlot(int slotIndex)
        {
            foreach (GameObject block in slotPreviews[slotIndex])
            {
                if (block != null)
                {
                    Destroy(block);
                }
            }
            slotPreviews[slotIndex].Clear();
        }
        
        /// <summary>
        /// 將 BlockColor 轉換為 Unity Color
        /// </summary>
        private Color GetColorFromBlockColor(BlockColor blockColor)
        {
            switch (blockColor)
            {
                case BlockColor.Cyan:    return new Color(0.13f, 0.83f, 0.93f); // I
                case BlockColor.Yellow:  return new Color(0.93f, 0.88f, 0.22f); // O
                case BlockColor.Purple:  return new Color(0.65f, 0.22f, 0.93f); // T
                case BlockColor.Blue:    return new Color(0.13f, 0.37f, 0.93f); // J
                case BlockColor.Orange:  return new Color(0.93f, 0.52f, 0.13f); // L
                case BlockColor.Green:   return new Color(0.37f, 0.93f, 0.22f); // S
                case BlockColor.Red:     return new Color(0.93f, 0.22f, 0.22f); // Z
                case BlockColor.Gray:    return new Color(0.5f, 0.5f, 0.5f);    // 灰色
                default: return Color.white;
            }
        }
    }
}

