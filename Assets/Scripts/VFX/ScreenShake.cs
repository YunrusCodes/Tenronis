using UnityEngine;
using System.Collections;
using Tenronis.Core;
using Tenronis.Data;

namespace Tenronis.VFX
{
    /// <summary>
    /// 螢幕震動效果
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class ScreenShake : MonoBehaviour
    {
        [Header("震動設定")]
        [SerializeField] private float shakeIntensity = 0.3f;
        [SerializeField] private float shakeDuration = 0.3f;
        [SerializeField] private AnimationCurve shakeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        
        private Camera cam;
        private Vector3 originalPosition;
        private Coroutine shakeCoroutine;
        
        private void Awake()
        {
            cam = GetComponent<Camera>();
            originalPosition = transform.localPosition;
        }
        
        private void Start()
        {
            // 訂閱事件
            GameEvents.OnGridOverflow += TriggerShake;
            GameEvents.OnPlayerDamaged += OnPlayerDamaged;
        }
        
        private void OnDestroy()
        {
            GameEvents.OnGridOverflow -= TriggerShake;
            GameEvents.OnPlayerDamaged -= OnPlayerDamaged;
        }
        
        /// <summary>
        /// 觸發震動
        /// </summary>
        public void TriggerShake()
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
            }
            shakeCoroutine = StartCoroutine(Shake());
        }
        
        /// <summary>
        /// 震動協程
        /// </summary>
        private IEnumerator Shake()
        {
            float elapsed = 0f;
            
            while (elapsed < shakeDuration)
            {
                float progress = elapsed / shakeDuration;
                float strength = shakeCurve.Evaluate(progress) * shakeIntensity;
                
                Vector3 offset = Random.insideUnitSphere * strength;
                offset.z = 0; // 保持2D
                
                transform.localPosition = originalPosition + offset;
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            transform.localPosition = originalPosition;
            shakeCoroutine = null;
        }
        
        /// <summary>
        /// 玩家受傷時觸發輕微震動
        /// </summary>
        private void OnPlayerDamaged(int damage)
        {
            // 輕微震動
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
            }
            shakeCoroutine = StartCoroutine(ShakeMild());
        }
        
        /// <summary>
        /// 輕微震動
        /// </summary>
        private IEnumerator ShakeMild()
        {
            float duration = 0.15f;
            float intensity = 0.1f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                float progress = elapsed / duration;
                float strength = (1f - progress) * intensity;
                
                Vector3 offset = Random.insideUnitSphere * strength;
                offset.z = 0;
                
                transform.localPosition = originalPosition + offset;
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            transform.localPosition = originalPosition;
            shakeCoroutine = null;
        }
    }
}

