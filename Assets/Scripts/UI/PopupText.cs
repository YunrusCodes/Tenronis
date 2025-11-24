using UnityEngine;
using TMPro;
using System.Collections;

namespace Tenronis.UI
{
    /// <summary>
    /// 彈出文字效果
    /// </summary>
    public class PopupText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textComponent;
        [SerializeField] private float lifetime = 1f;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        
        private float timer;
        
    /// <summary>
    /// 初始化彈出文字
    /// </summary>
    public void Initialize(string text, Color color, Vector3 worldPosition)
    {
        if (textComponent == null)
            textComponent = GetComponent<TextMeshProUGUI>();
        
        textComponent.text = text;
        textComponent.color = color;
        
        // 將世界座標轉換為螢幕座標，再轉換為 Canvas 位置
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        
        // 設置 RectTransform 位置
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.position = screenPos;
        }
        else
        {
            transform.position = screenPos;
        }
        
        timer = 0f;
        StartCoroutine(AnimatePopup());
    }
        
    private IEnumerator AnimatePopup()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector3 startPos = rectTransform != null ? rectTransform.position : transform.position;
        
        while (timer < lifetime)
        {
            timer += Time.deltaTime;
            float progress = timer / lifetime;
            
            // 向上移動（在螢幕空間中）
            Vector3 newPos = startPos + Vector3.up * (moveSpeed * timer * 50f); // 乘以 50 因為是螢幕像素
            
            if (rectTransform != null)
            {
                rectTransform.position = newPos;
            }
            else
            {
                transform.position = newPos;
            }
            
            // 淡出
            Color color = textComponent.color;
            color.a = fadeCurve.Evaluate(progress);
            textComponent.color = color;
            
            yield return null;
        }
        
        Destroy(gameObject);
    }
    }
}

