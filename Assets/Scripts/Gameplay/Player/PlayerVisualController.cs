using UnityEngine;
using System.Collections.Generic;
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
        [SerializeField] private SpriteRenderer playerSprite; // 主玩家圖片（由 GameManager 控制）
        [SerializeField] private SpriteRenderer overlaySpriteRenderer; // 覆蓋層圖片（用於顯示狀態效果）
        
        [Header("覆蓋層圖片")]
        [SerializeField] private Sprite damagedOverlaySprite; // 受傷時的覆蓋層圖片（可選）
        [SerializeField] private Sprite lowHpOverlaySprite;   // 低HP時的覆蓋層圖片（可選）
        
        [Header("受傷特效")]
        [SerializeField] private GameObject damageEffectPrefab; // 受傷時的爆炸特效
        
        [Header("攻擊特效點")]
        [SerializeField] private Transform[] effectPoints = new Transform[4]; // 4個固定特效點
        [SerializeField] private GameObject attackEffectPrefab; // 攻擊/反擊時的特效
        
        [Header("HP閾值")]
        [SerializeField] private float lowHpThreshold = 0.3f; // 30% HP以下顯示低HP圖片
        
        // 視覺效果
        private Vector3 originalSpritePosition;
        private Vector3 originalOverlayPosition;
        private Coroutine shakeCoroutine;
        private Coroutine effectSpawnCoroutine;
        private Coroutine overlayFlashCoroutine;
        private int pendingEffectCount = 0; // 待生成的特效數量
        private bool isGameOver = false; // 遊戲是否結束
        
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
            }
            
            // 記錄覆蓋層的原始位置
            if (overlaySpriteRenderer != null)
            {
                originalOverlayPosition = overlaySpriteRenderer.transform.localPosition;
                // 初始時隱藏覆蓋層
                overlaySpriteRenderer.sprite = null;
            }
            
            // 訂閱事件
            GameEvents.OnPlayerDamaged += HandlePlayerDamaged;
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
            GameEvents.OnRowsCleared += HandleRowsCleared;
        }
        
        private void OnDestroy()
        {
            GameEvents.OnPlayerDamaged -= HandlePlayerDamaged;
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
            GameEvents.OnRowsCleared -= HandleRowsCleared;
        }
        
        private void Update()
        {
            // 根據HP更新覆蓋層圖片
            UpdateOverlayBasedOnHp();
        }
        
        /// <summary>
        /// 根據HP百分比更新覆蓋層圖片
        /// </summary>
        private void UpdateOverlayBasedOnHp()
        {
            if (overlaySpriteRenderer == null || PlayerManager.Instance == null) return;
            if (shakeCoroutine != null || overlayFlashCoroutine != null) return; // 正在晃動或閃爍時不更新
            
            float hpPercent = (float)PlayerManager.Instance.Stats.currentHp / PlayerManager.Instance.Stats.maxHp;
            
            // 低HP時顯示低HP覆蓋層
            if (hpPercent <= lowHpThreshold && lowHpOverlaySprite != null)
            {
                if (overlaySpriteRenderer.sprite != lowHpOverlaySprite)
                {
                    overlaySpriteRenderer.sprite = lowHpOverlaySprite;
                }
            }
            // 正常HP時隱藏覆蓋層
            else
            {
                if (overlaySpriteRenderer.sprite != null)
                {
                    overlaySpriteRenderer.sprite = null;
                }
            }
        }
        
        /// <summary>
        /// 處理玩家受傷
        /// </summary>
        private void HandlePlayerDamaged(int damage)
        {
            // 如果遊戲已結束，不再顯示受傷效果
            if (isGameOver) return;
            
            // 檢查玩家是否死亡
            if (PlayerManager.Instance != null && PlayerManager.Instance.Stats.currentHp <= 0)
            {
                HandlePlayerDeath();
                return;
            }
            
            // 視覺效果：晃動
            if (playerSprite != null)
            {
                if (shakeCoroutine != null)
                {
                    StopCoroutine(shakeCoroutine);
                }
                shakeCoroutine = StartCoroutine(ShakeSprite());
            }
            
            // 視覺效果：短暫顯示受傷覆蓋層
            if (damagedOverlaySprite != null && overlaySpriteRenderer != null)
            {
                if (overlayFlashCoroutine != null)
                {
                    StopCoroutine(overlayFlashCoroutine);
                }
                overlayFlashCoroutine = StartCoroutine(FlashDamagedOverlay());
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
        /// 處理玩家死亡
        /// </summary>
        private void HandlePlayerDeath()
        {
            isGameOver = true;
            
            // 停止所有視覺效果協程
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                shakeCoroutine = null;
            }
            
            if (effectSpawnCoroutine != null)
            {
                StopCoroutine(effectSpawnCoroutine);
                effectSpawnCoroutine = null;
            }
            
            if (overlayFlashCoroutine != null)
            {
                StopCoroutine(overlayFlashCoroutine);
                overlayFlashCoroutine = null;
            }
            
            // 清空特效隊列
            pendingEffectCount = 0;
            
            // 恢復Sprite位置
            if (playerSprite != null)
            {
                playerSprite.transform.localPosition = originalSpritePosition;
            }
            
            // 恢復覆蓋層位置並隱藏
            if (overlaySpriteRenderer != null)
            {
                overlaySpriteRenderer.transform.localPosition = originalOverlayPosition;
                overlaySpriteRenderer.sprite = null;
            }
            
            Debug.Log("[PlayerVisualController] 玩家死亡，停止所有視覺效果");
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
                
                Vector3 shakeOffset = originalSpritePosition + offset;
                playerSprite.transform.localPosition = shakeOffset;
                
                // 覆蓋層跟隨主圖片位置
                if (overlaySpriteRenderer != null)
                {
                    overlaySpriteRenderer.transform.localPosition = originalOverlayPosition + offset;
                }
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            playerSprite.transform.localPosition = originalSpritePosition;
            
            // 恢復覆蓋層位置
            if (overlaySpriteRenderer != null)
            {
                overlaySpriteRenderer.transform.localPosition = originalOverlayPosition;
            }
            
            shakeCoroutine = null;
        }
        
        /// <summary>
        /// 短暫顯示受傷覆蓋層
        /// </summary>
        private System.Collections.IEnumerator FlashDamagedOverlay()
        {
            if (overlaySpriteRenderer == null) yield break;
            
            overlaySpriteRenderer.sprite = damagedOverlaySprite;
            
            yield return new WaitForSeconds(0.2f); // 顯示0.2秒
            
            // 恢復覆蓋層（根據當前HP）
            float hpPercent = (float)PlayerManager.Instance.Stats.currentHp / PlayerManager.Instance.Stats.maxHp;
            if (hpPercent <= lowHpThreshold && lowHpOverlaySprite != null)
            {
                overlaySpriteRenderer.sprite = lowHpOverlaySprite;
            }
            else
            {
                overlaySpriteRenderer.sprite = null;
            }
            
            overlayFlashCoroutine = null;
        }
        
        /// <summary>
        /// 依序生成多個受傷特效（協程）
        /// </summary>
        private System.Collections.IEnumerator SpawnDamageEffectsSequentially()
        {
            while (pendingEffectCount > 0 && !isGameOver)
            {
                // 生成一個特效
                SpawnSingleDamageEffect();
                pendingEffectCount--;
                
                // 等待一小段時間再生成下一個
                yield return new WaitForSeconds(0.05f); // 50毫秒延遲
            }
            
            // 如果遊戲結束，清空剩餘特效隊列
            if (isGameOver)
            {
                pendingEffectCount = 0;
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
                    // 遊戲開始時重置狀態
                    isGameOver = false;
                    pendingEffectCount = 0;
                    
                    // 停止所有進行中的協程
                    if (shakeCoroutine != null)
                    {
                        StopCoroutine(shakeCoroutine);
                        shakeCoroutine = null;
                    }
                    if (effectSpawnCoroutine != null)
                    {
                        StopCoroutine(effectSpawnCoroutine);
                        effectSpawnCoroutine = null;
                    }
                    if (overlayFlashCoroutine != null)
                    {
                        StopCoroutine(overlayFlashCoroutine);
                        overlayFlashCoroutine = null;
                    }
                    
                    // 恢復Sprite位置（不修改主圖片，主圖片由 GameManager 控制）
                    if (playerSprite != null)
                    {
                        playerSprite.transform.localPosition = originalSpritePosition;
                    }
                    
                    // 恢復覆蓋層位置並隱藏
                    if (overlaySpriteRenderer != null)
                    {
                        overlaySpriteRenderer.transform.localPosition = originalOverlayPosition;
                        overlaySpriteRenderer.sprite = null;
                    }
                    
                    Debug.Log("[PlayerVisualController] 遊戲開始，重置所有視覺狀態");
                    break;
                    
                case GameState.GameOver:
                    // 遊戲結束時停止所有特效
                    HandlePlayerDeath();
                    break;
            }
        }
        
        /// <summary>
        /// 處理消除行事件
        /// </summary>
        private void HandleRowsCleared(List<int> clearedRows, List<int> nonGarbageRows, bool hasVoid)
        {
            if (isGameOver) return;
            if (attackEffectPrefab == null) return;
            
            // 虛無抵銷時不生成攻擊特效
            if (hasVoid)
            {
                Debug.Log("[PlayerVisualController] 虛無抵銷，不生成攻擊特效");
                return;
            }
            
            // 根據非垃圾方塊行數生成攻擊特效
            StartCoroutine(SpawnAttackEffectsForRows(nonGarbageRows.Count));
        }
        
        /// <summary>
        /// 為消除的每一行在所有特效點生成特效
        /// </summary>
        private System.Collections.IEnumerator SpawnAttackEffectsForRows(int rowCount)
        {
            for (int i = 0; i < rowCount; i++)
            {
                // 在所有4個特效點生成特效
                SpawnAttackEffectsAtAllPoints();
                
                // 等待一小段時間再處理下一行（確保不在同一幀）
                yield return new WaitForSeconds(0.08f); // 80毫秒延遲
            }
        }
        
        /// <summary>
        /// 在所有特效點生成攻擊特效
        /// </summary>
        private void SpawnAttackEffectsAtAllPoints()
        {
            for (int i = 0; i < effectPoints.Length; i++)
            {
                if (effectPoints[i] != null)
                {
                    SpawnEffectAtPoint(i);
                }
            }
        }
        
        /// <summary>
        /// 在指定特效點生成特效
        /// </summary>
        private void SpawnEffectAtPoint(int pointIndex)
        {
            if (pointIndex < 0 || pointIndex >= effectPoints.Length) return;
            if (effectPoints[pointIndex] == null) return;
            if (attackEffectPrefab == null) return;
            
            Vector3 spawnPosition = effectPoints[pointIndex].position;
            GameObject effect = Instantiate(attackEffectPrefab, spawnPosition, Quaternion.identity);
            Destroy(effect, 2f); // 2秒後銷毀
        }
        
        /// <summary>
        /// 觸發反擊特效（在隨機特效點）
        /// </summary>
        public void TriggerCounterFireEffect()
        {
            if (isGameOver) return;
            if (attackEffectPrefab == null) return;
            if (effectPoints.Length == 0) return;
            
            // 隨機選擇一個特效點
            int randomIndex = Random.Range(0, effectPoints.Length);
            
            // 確保選擇的特效點有效
            if (effectPoints[randomIndex] != null)
            {
                SpawnEffectAtPoint(randomIndex);
            }
        }
    }
}

