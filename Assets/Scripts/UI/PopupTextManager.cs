using UnityEngine;
using Tenronis.Core;

namespace Tenronis.UI
{
    /// <summary>
    /// 彈出文字管理器
    /// </summary>
    public class PopupTextManager : MonoBehaviour
    {
        [Header("設定")]
        [SerializeField] private PopupText popupTextPrefab;
        [SerializeField] private Canvas worldCanvas;
        
        private void Start()
        {
            // 訂閱事件
            GameEvents.OnShowPopupText += ShowPopupText;
        }
        
        private void OnDestroy()
        {
            GameEvents.OnShowPopupText -= ShowPopupText;
        }
        
        /// <summary>
        /// 顯示彈出文字
        /// </summary>
        private void ShowPopupText(string text, Color color, Vector2 worldPosition)
        {
            if (popupTextPrefab == null) return;
            
            PopupText popup = Instantiate(popupTextPrefab, worldCanvas.transform);
            popup.Initialize(text, color, worldPosition);
        }
    }
}

