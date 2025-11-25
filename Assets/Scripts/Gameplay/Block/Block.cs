using UnityEngine;
using Tenronis.Data;

namespace Tenronis.Gameplay.Block
{
    /// <summary>
    /// 方塊視覺組件
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class Block : MonoBehaviour
    {
        [Header("視覺設定")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color[] colorPalette;
        
        private BlockData blockData;
        
        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        /// <summary>
        /// 設置方塊數據
        /// </summary>
        public void SetBlockData(BlockData data)
        {
            blockData = data;
            UpdateVisual();
        }
        
        /// <summary>
        /// 更新視覺
        /// </summary>
        private void UpdateVisual()
        {
            if (blockData == null || spriteRenderer == null) return;
            
            // 設置顏色
            Color baseColor = GetColorForBlockType(blockData.color);
            
            // Debug：輸出顏色資訊
            Debug.Log($"[Block] 更新視覺 - 顏色: {blockData.color}, RGB: ({baseColor.r:F2}, {baseColor.g:F2}, {baseColor.b:F2}), HP: {blockData.hp}/{blockData.maxHp}");
            
            // 根據HP調整透明度
            float opacity = blockData.isIndestructible ? 1f : Mathf.Max(0.3f, (float)blockData.hp / blockData.maxHp);
            baseColor.a = opacity;
            
            spriteRenderer.color = baseColor;
            
            // 不可摧毀方塊的特殊視覺
            if (blockData.isIndestructible)
            {
                // 可以添加特殊材質或效果
            }
        }
        
        /// <summary>
        /// 獲取方塊類型對應的顏色
        /// </summary>
        private Color GetColorForBlockType(BlockColor blockColor)
        {
            // 優先使用預設顏色（確保顏色正確）
            Color resultColor;
            
            switch (blockColor)
            {
                case BlockColor.Cyan: resultColor = new Color(0.13f, 0.83f, 0.93f); break; // #22D3EE
                case BlockColor.Blue: resultColor = new Color(0.23f, 0.51f, 0.96f); break; // #3B82F6
                case BlockColor.Orange: resultColor = new Color(0.98f, 0.45f, 0.13f); break; // #F97316
                case BlockColor.Yellow: resultColor = new Color(0.98f, 0.8f, 0.08f); break; // #FACC15
                case BlockColor.Green: resultColor = new Color(0.29f, 0.87f, 0.5f); break; // #4ADE80
                case BlockColor.Purple: resultColor = new Color(0.66f, 0.33f, 0.97f); break; // #A855F7
                case BlockColor.Red: resultColor = new Color(0.94f, 0.27f, 0.27f); break; // #EF4444
                case BlockColor.Gray: resultColor = new Color(0.39f, 0.45f, 0.55f); break; // #64748B
                case BlockColor.Garbage: resultColor = new Color(0.35f, 0.35f, 0.35f); break; // 垃圾方塊
                default: resultColor = Color.white; break;
            }
            
            // 如果設置了 colorPalette，可以覆蓋（但不是必須的）
            if (colorPalette != null && colorPalette.Length > (int)blockColor)
            {
                Color paletteColor = colorPalette[(int)blockColor];
                // 只有當調色板顏色不是白色時才使用
                if (paletteColor != Color.white && paletteColor.a > 0.01f)
                {
                    resultColor = paletteColor;
                }
            }
            
            return resultColor;
        }
        
        public BlockData GetBlockData() => blockData;
    }
}

