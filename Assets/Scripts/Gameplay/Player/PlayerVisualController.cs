using UnityEngine;
using Tenronis.Data;
using Tenronis.Core;
using Tenronis.Managers;

namespace Tenronis.Gameplay.Player
{
    /// <summary>
    /// 玩家視覺控制器
    /// </summary>
    public class PlayerVisualController : MonoBehaviour
    {
        public static PlayerVisualController Instance { get; private set; }
        
        [Header("視覺")]
        [SerializeField] private SpriteRenderer playerSprite;
        [SerializeField] private Sprite defaultSprite; // 默認圖片
        [SerializeField] private Sprite damagedSprite; // 受傷時的圖片（可選）
        [SerializeField] private Sprite lowHpSprite;   // 低HP時的圖片（可選）
        
        [Header("受傷特效")]
        [SerializeField] private GameObject damageEffectPrefab; // 受傷時的爆炸特效
        
        [Header("HP閾值")]
        [SerializeField] private float lowHpThreshold = 0.3f; // 30% HP以下顯示低HP圖片
        
        // 視覺效果
        private Vector3 originalSpritePosition;
        private Coroutine shakeCoroutine;
        private Coroutine effectSpawnCoroutine;
        private Coroutine spriteFlashCoroutine;
        private int pendingEffectCount = 0; // 待生成的特效數量
        
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
            // 記錄玩家Sprite的原始位置
            if (playerSprite != null)
            {
                originalSpritePosition = playerSprite.transform.localPosition;
                
                // 設置默認圖片
                if (defaultSprite != null)
                {
                    playerSprite.sprite = defaultSprite;
                }
            }
            
            // 訂閱事件
            GameEvents.OnPlayerDamaged += HandlePlayerDamaged;
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
        }
        
        private void OnDestroy()
        {
            GameEvents.OnPlayerDamaged -= HandlePlayerDamaged;
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        }
        
        private void Update()
        {
            // 根據HP更新圖片
            UpdateSpriteBasedOnHp();
        }
        
        /// <summary>
        /// 根據HP百分比更新玩家圖片
        /// </summary>
        private void UpdateSpriteBasedOnHp()
        {
            if (playerSprite == null || PlayerManager.Instance == null) return;
            if (shakeCoroutine != null || spriteFlashCoroutine != null) return; // 正在晃動或閃爍時不更新
            
            float hpPercent = (float)PlayerManager.Instance.Stats.currentHp / PlayerManager.Instance.Stats.maxHp;
            
            // 低HP時顯示低HP圖片
            if (hpPercent <= lowHpThreshold && lowHpSprite != null)
            {
                if (playerSprite.sprite != lowHpSprite)
                {
                    playerSprite.sprite = lowHpSprite;
                }
            }
            // 正常HP時顯示默認圖片
            else if (defaultSprite != null)
            {
                if (playerSprite.sprite != defaultSprite && playerSprite.sprite != damagedSprite)
                {
                    playerSprite.sprite = defaultSprite;
                }
            }
        }
        
        /// <summary>
        /// 處理玩家受傷
        /// </summary>
        private void HandlePlayerDamaged(int damage)
        {
            // 視覺效果：晃動
            if (playerSprite != null)
            {
                if (shakeCoroutine != null)
                {
                    StopCoroutine(shakeCoroutine);
                }
                shakeCoroutine = StartCoroutine(ShakeSprite());
            }
            
            // 視覺效果：短暫顯示受傷圖片
            if (damagedSprite != null && playerSprite != null)
            {
                if (spriteFlashCoroutine != null)
                {
                    StopCoroutine(spriteFlashCoroutine);
                }
                spriteFlashCoroutine = StartCoroutine(FlashDamagedSprite());
            }
            
            // 視覺效果：爆炸特效（累積到隊列）
            if (damageEffectPrefab != null && playerSprite != null)
            {
                pendingEffectCount++;
                
                // 如果還沒有在生成特效，啟動協程
                if (effectSpawnCoroutine == null)
                {
                    effectSpawnCoroutine = StartCoroutine(SpawnDamageEffectsSequentially());
                }
            }
        }
        
        /// <summary>
        /// 玩家Sprite晃動效果
        /// </summary>
        private System.Collections.IEnumerator ShakeSprite()
        {
            float duration = 0.3f;
            float intensity = 0.2f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                float progress = elapsed / duration;
                float strength = (1f - progress) * intensity;
                
                Vector3 offset = new Vector3(
                    Random.Range(-strength, strength),
                    Random.Range(-strength, strength),
                    0f
                );
                
                playerSprite.transform.localPosition = originalSpritePosition + offset;
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            playerSprite.transform.localPosition = originalSpritePosition;
            shakeCoroutine = null;
        }
        
        /// <summary>
        /// 短暫顯示受傷圖片
        /// </summary>
        private System.Collections.IEnumerator FlashDamagedSprite()
        {
            Sprite originalSprite = playerSprite.sprite;
            playerSprite.sprite = damagedSprite;
            
            yield return new WaitForSeconds(0.2f); // 顯示0.2秒
            
            // 恢復原圖片（根據當前HP）
            float hpPercent = (float)PlayerManager.Instance.Stats.currentHp / PlayerManager.Instance.Stats.maxHp;
            if (hpPercent <= lowHpThreshold && lowHpSprite != null)
            {
                playerSprite.sprite = lowHpSprite;
            }
            else if (defaultSprite != null)
            {
                playerSprite.sprite = defaultSprite;
            }
            
            spriteFlashCoroutine = null;
        }
        
        /// <summary>
        /// 依序生成多個受傷特效（協程）
        /// </summary>
        private System.Collections.IEnumerator SpawnDamageEffectsSequentially()
        {
            while (pendingEffectCount > 0)
            {
                // 生成一個特效
                SpawnSingleDamageEffect();
                pendingEffectCount--;
                
                // 等待一小段時間再生成下一個
                yield return new WaitForSeconds(0.05f); // 50毫秒延遲
            }
            
            effectSpawnCoroutine = null;
        }
        
        /// <summary>
        /// 在玩家Sprite隨機位置生成單個受傷特效
        /// </summary>
        private void SpawnSingleDamageEffect()
        {
            if (playerSprite == null || damageEffectPrefab == null) return;
            
            // 獲取Sprite的邊界
            Bounds spriteBounds = playerSprite.bounds;
            
            // 在Sprite範圍內隨機位置
            Vector3 randomPosition = new Vector3(
                Random.Range(spriteBounds.min.x, spriteBounds.max.x),
                Random.Range(spriteBounds.min.y, spriteBounds.max.y),
                playerSprite.transform.position.z
            );
            
            // 實例化爆炸特效
            GameObject effect = Instantiate(damageEffectPrefab, randomPosition, Quaternion.identity);
            Destroy(effect, 2f); // 2秒後銷毀
        }
        
        /// <summary>
        /// 處理遊戲狀態變更
        /// </summary>
        private void HandleGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.Playing:
                    // 遊戲開始時重置圖片
                    if (playerSprite != null && defaultSprite != null)
                    {
                        playerSprite.sprite = defaultSprite;
                    }
                    break;
                    
                case GameState.GameOver:
                    // 遊戲結束時可以添加特殊效果
                    break;
            }
        }
        
        /// <summary>
        /// 設置玩家圖片（可從外部調用）
        /// </summary>
        public void SetPlayerSprite(Sprite sprite)
        {
            if (playerSprite != null && sprite != null)
            {
                defaultSprite = sprite;
                playerSprite.sprite = sprite;
            }
        }
    }
}

