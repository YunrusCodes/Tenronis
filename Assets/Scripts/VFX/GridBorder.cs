using UnityEngine;
using Tenronis.Data;
using Tenronis.Managers;
using Tenronis.Core;

namespace Tenronis.VFX
{
    /// <summary>
    /// 遊戲網格邊框 - 使用 LineRenderer 在世界空間繪製
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class GridBorder : MonoBehaviour
    {
        [Header("邊框設定")]
        [SerializeField] private Color borderColor = Color.white;
        [SerializeField] private float borderWidth = 0.1f;
        [SerializeField] private int sortingOrder = 20;
        
        private LineRenderer lineRenderer;
        private bool hasDrawn = false;
        
        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            
            // 初始隱藏
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }
        }
        
        private void Start()
        {
            // 訂閱遊戲狀態改變事件
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
            
            // 如果遊戲已經在 Playing 狀態，立即繪製
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                Invoke(nameof(DrawBorder), 0.1f);
            }
        }
        
        private void OnDestroy()
        {
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        }
        
        /// <summary>
        /// 處理遊戲狀態改變
        /// </summary>
        private void HandleGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.Playing:
                    if (!hasDrawn)
                    {
                        DrawBorder();
                    }
                    ShowBorder();
                    break;
                    
                case GameState.Menu:
                case GameState.GameOver:
                case GameState.Victory:
                    HideBorder();
                    break;
                    
                case GameState.LevelUp:
                    // 升級選單時保持顯示
                    break;
            }
        }
        
        /// <summary>
        /// 顯示邊框
        /// </summary>
        private void ShowBorder()
        {
            if (lineRenderer != null)
            {
                lineRenderer.enabled = true;
            }
        }
        
        /// <summary>
        /// 隱藏邊框
        /// </summary>
        private void HideBorder()
        {
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }
        }
        
        /// <summary>
        /// 繪製邊框
        /// </summary>
        private void DrawBorder()
        {
            if (hasDrawn) return;
            
            if (GridManager.Instance == null)
            {
                Debug.LogWarning("[GridBorder] GridManager 未找到！");
                return;
            }
            
            float blockSize = GridManager.Instance.BlockSize;
            Vector2 offset = GridManager.Instance.GridOffset;
            
            // 計算邊界的四個角（包含半個方塊的邊距）
            float halfBlock = blockSize * 0.5f;
            
            Vector3 topLeft = new Vector3(
                offset.x - halfBlock, 
                offset.y + halfBlock, 
                0
            );
            
            Vector3 topRight = new Vector3(
                offset.x + GameConstants.BOARD_WIDTH * blockSize - halfBlock, 
                offset.y + halfBlock, 
                0
            );
            
            Vector3 bottomRight = new Vector3(
                offset.x + GameConstants.BOARD_WIDTH * blockSize - halfBlock, 
                offset.y - GameConstants.BOARD_HEIGHT * blockSize + halfBlock, 
                0
            );
            
            Vector3 bottomLeft = new Vector3(
                offset.x - halfBlock, 
                offset.y - GameConstants.BOARD_HEIGHT * blockSize + halfBlock, 
                0
            );
            
            // 設置 LineRenderer
            lineRenderer.positionCount = 5;
            lineRenderer.SetPosition(0, bottomLeft);
            lineRenderer.SetPosition(1, topLeft);
            lineRenderer.SetPosition(2, topRight);
            lineRenderer.SetPosition(3, bottomRight);
            lineRenderer.SetPosition(4, bottomLeft); // 閉合矩形
            
            lineRenderer.startColor = borderColor;
            lineRenderer.endColor = borderColor;
            lineRenderer.startWidth = borderWidth;
            lineRenderer.endWidth = borderWidth;
            lineRenderer.loop = true;
            
            // 設置材質和渲染層級
            if (lineRenderer.material == null || lineRenderer.material.shader.name != "Sprites/Default")
            {
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            }
            lineRenderer.sortingOrder = sortingOrder;
            
            hasDrawn = true;
            Debug.Log($"[GridBorder] 邊框已繪製 - Offset: {offset}, BlockSize: {blockSize}");
        }
        
        /// <summary>
        /// 在 Inspector 中預覽邊框設定
        /// </summary>
        private void OnValidate()
        {
            if (lineRenderer != null && Application.isPlaying)
            {
                lineRenderer.startColor = borderColor;
                lineRenderer.endColor = borderColor;
                lineRenderer.startWidth = borderWidth;
                lineRenderer.endWidth = borderWidth;
                lineRenderer.sortingOrder = sortingOrder;
            }
        }
    }
}

