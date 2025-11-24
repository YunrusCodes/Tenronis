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
            transform.position = worldPosition;
            
            timer = 0f;
            StartCoroutine(AnimatePopup());
        }
        
        private IEnumerator AnimatePopup()
        {
            Vector3 startPos = transform.position;
            
            while (timer < lifetime)
            {
                timer += Time.deltaTime;
                float progress = timer / lifetime;
                
                // 向上移動
                transform.position = startPos + Vector3.up * (moveSpeed * timer);
                
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

