using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Tenronis.Data;
using Tenronis.Gameplay.Tetromino;

namespace Tenronis.UI
{
    /// <summary>
    /// 下一個方塊預覽 UI
    /// </summary>
    public class NextPiecePreview : MonoBehaviour
    {
        [Header("預覽設定")]
        [SerializeField] private RectTransform previewContainer; // UI 容器
        [SerializeField] private float blockSize = 30f; // UI 方塊大小（像素）
        [SerializeField] private float spacing = 2f; // 方塊間距
        
        [Header("視覺設定")]
        [SerializeField] private bool useSprite = false; // 是否使用 Sprite
        [SerializeField] private Sprite blockSprite; // 方塊 Sprite（可選）
        
        [Header("腐化符號設定")]
        [SerializeField] private Sprite explosiveSymbol; // 爆炸型符紋
        [SerializeField] private Sprite voidSymbol; // 虛無型符紋
        
        private List<GameObject> previewBlocks = new List<GameObject>();
        
        private void Start()
        {
            // 訂閱下一個方塊更新事件
            Core.GameEvents.OnNextPieceChanged += UpdatePreview;
            
            // 初始更新
            Invoke(nameof(UpdatePreview), 0.2f); // 延遲更新，確保 TetrominoController 已初始化
        }
        
        private void OnDestroy()
        {
            Core.GameEvents.OnNextPieceChanged -= UpdatePreview;
        }
        
        /// <summary>
        /// 更新預覽
        /// </summary>
        private void UpdatePreview()
        {
            // 清除舊的預覽
            ClearPreview();
            
            // 檢查 TetrominoController
            if (TetrominoController.Instance == null)
            {
                return;
            }
            
            TetrominoShape nextShape = TetrominoController.Instance.NextShape;
            if (nextShape.shape == null)
            {
                return;
            }
            
            // 繪製下一個方塊
            DrawNextPiece(nextShape);
        }
        
        /// <summary>
        /// 繪製下一個方塊
        /// </summary>
        private void DrawNextPiece(TetrominoShape shape)
        {
            int rows = shape.shape.GetLength(0);
            int cols = shape.shape.GetLength(1);
            
            // 獲取腐化信息
            Dictionary<string, BlockType> corruptedBlocks = new Dictionary<string, BlockType>();
            if (TetrominoController.Instance != null)
            {
                corruptedBlocks = TetrominoController.Instance.GetNextCorruptedBlocks();
            }
            
            // 計算方塊實際佔用的範圍（忽略空白）
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
            
            // 計算中心偏移（讓方塊居中顯示）
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
                        
                        // 檢查這個格子是否被腐化
                        string key = $"{x},{y}";
                        BlockType? corruptType = null;
                        if (corruptedBlocks.ContainsKey(key))
                        {
                            corruptType = corruptedBlocks[key];
                        }
                        
                        GameObject blockObj = CreateUIBlock(relativeX, relativeY, offsetX, offsetY, blockColor, corruptType);
                        previewBlocks.Add(blockObj);
                    }
                }
            }
        }
        
        /// <summary>
        /// 建立 UI 方塊
        /// </summary>
        private GameObject CreateUIBlock(int x, int y, float offsetX, float offsetY, Color color, BlockType? corruptType = null)
        {
            GameObject blockObj = new GameObject($"PreviewBlock_{x}_{y}");
            blockObj.transform.SetParent(previewContainer, false);
            
            RectTransform rectTransform = blockObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(blockSize, blockSize);
            
            // 計算位置（UI 座標系統，Y 軸向上）
            float posX = offsetX + x * (blockSize + spacing);
            float posY = offsetY - y * (blockSize + spacing);
            rectTransform.anchoredPosition = new Vector2(posX, posY);
            
            // 添加視覺組件
            // 所有方塊（包括腐蝕方塊）都使用原本的顏色
            Color blockColor = color;
            
            if (useSprite && blockSprite != null)
            {
                Image image = blockObj.AddComponent<Image>();
                image.sprite = blockSprite;
                image.color = blockColor;
            }
            else
            {
                // 使用純色方塊
                Image image = blockObj.AddComponent<Image>();
                image.color = blockColor;
            }
            
            // 如果方塊被腐化，添加符號標記
            if (corruptType.HasValue && (corruptType.Value == BlockType.Explosive || corruptType.Value == BlockType.Void))
            {
                AddCorruptionSymbol(blockObj, corruptType.Value);
            }
            
            return blockObj;
        }
        
        /// <summary>
        /// 添加腐化符號標記（UI版本）
        /// </summary>
        private void AddCorruptionSymbol(GameObject blockObj, BlockType corruptType)
        {
            // 創建符號圖片物件
            GameObject symbolObj = new GameObject("CorruptionSymbol");
            symbolObj.transform.SetParent(blockObj.transform, false);
            
            // 添加 RectTransform
            RectTransform symbolRect = symbolObj.AddComponent<RectTransform>();
            symbolRect.anchorMin = Vector2.zero;
            symbolRect.anchorMax = Vector2.one;
            symbolRect.sizeDelta = Vector2.zero;
            symbolRect.anchoredPosition = Vector2.zero;
            
            // 添加 Image 組件
            Image symbolImage = symbolObj.AddComponent<Image>();
            
            // 設置符號 Sprite
            if (corruptType == BlockType.Explosive)
            {
                symbolImage.sprite = explosiveSymbol;
            }
            else // Void
            {
                symbolImage.sprite = voidSymbol;
            }
            
            // 設置圖片屬性
            symbolImage.preserveAspect = true;
        }
        
        /// <summary>
        /// 清除預覽
        /// </summary>
        private void ClearPreview()
        {
            foreach (GameObject block in previewBlocks)
            {
                if (block != null)
                {
                    Destroy(block);
                }
            }
            previewBlocks.Clear();
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
                case BlockColor.Gray:    return new Color(0.5f, 0.5f, 0.5f);    // 灰色（修復用）
                default: return Color.white;
            }
        }
    }
}

