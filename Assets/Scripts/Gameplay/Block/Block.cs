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
            if (colorPalette != null && colorPalette.Length > (int)blockColor)
            {
                return colorPalette[(int)blockColor];
            }
            
            // 預設顏色
            switch (blockColor)
            {
                case BlockColor.Cyan: return new Color(0.13f, 0.83f, 0.93f); // #22D3EE
                case BlockColor.Blue: return new Color(0.23f, 0.51f, 0.96f); // #3B82F6
                case BlockColor.Orange: return new Color(0.98f, 0.45f, 0.13f); // #F97316
                case BlockColor.Yellow: return new Color(0.98f, 0.8f, 0.08f); // #FACC15
                case BlockColor.Green: return new Color(0.29f, 0.87f, 0.5f); // #4ADE80
                case BlockColor.Purple: return new Color(0.66f, 0.33f, 0.97f); // #A855F7
                case BlockColor.Red: return new Color(0.94f, 0.27f, 0.27f); // #EF4444
                case BlockColor.Gray: return new Color(0.39f, 0.45f, 0.55f); // #64748B
                case BlockColor.Garbage: return new Color(0.35f, 0.35f, 0.35f); // 垃圾方塊
                default: return Color.white;
            }
        }
        
        public BlockData GetBlockData() => blockData;
    }
}

