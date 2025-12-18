using UnityEngine;
using Tenronis.Data;
using Tenronis.Core;
using Tenronis.Managers;
using Tenronis.ScriptableObjects;
using Tenronis.Gameplay.Projectiles;

namespace Tenronis.Gameplay.Enemy
{
    /// <summary>
    /// 敵人控制器
    /// </summary>
    public class EnemyController : MonoBehaviour
    {
        public static EnemyController Instance { get; private set; }
        
        [Header("視覺")]
        [SerializeField] private SpriteRenderer enemySprite;
        
        [Header("受傷特效")]
        [SerializeField] private GameObject damageEffectPrefab; // 受傷時的爆炸特效
        
        // 敵人狀態
        private StageDataSO currentStageData;
        private float currentHp;
        private float shootTimer;
        private bool isDefeated = false; // 是否已被擊敗
        
        // 視覺效果
        private Vector3 originalSpritePosition;
        private Coroutine shakeCoroutine;
        private Coroutine effectSpawnCoroutine;
        private int pendingEffectCount = 0; // 待生成的特效數量
        
        // 屬性
        public float CurrentHp => currentHp;
        public float MaxHp => currentStageData != null ? currentStageData.maxHp : 100f;
        public float HpPercent => MaxHp > 0 ? currentHp / MaxHp : 0f;
        
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
            // 記錄敵人Sprite的原始位置
            if (enemySprite != null)
            {
                originalSpritePosition = enemySprite.transform.localPosition;
            }
            
            // 訂閱事件
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
            GameEvents.OnEnemyDamaged += HandleDamaged;
        }
        
        private void OnDestroy()
        {
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
            GameEvents.OnEnemyDamaged -= HandleDamaged;
        }
        
        private void Update()
        {
            if (GameManager.Instance.CurrentState != GameState.Playing) return;
            if (currentStageData == null) return;
            if (isDefeated) return; // 被擊敗後立即停止射擊
            
            // 射擊計時
            shootTimer += Time.deltaTime;
            if (shootTimer >= currentStageData.shootInterval)
            {
                Shoot();
                shootTimer = 0f;
            }
        }
        
        /// <summary>
        /// 初始化敵人
        /// </summary>
        private void InitializeEnemy()
        {
            currentStageData = GameManager.Instance.CurrentStage;
            
            if (currentStageData != null)
            {
                currentHp = currentStageData.maxHp;
                shootTimer = 0f;
                isDefeated = false; // 重置擊敗狀態
                pendingEffectCount = 0; // 清空特效隊列
                
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
                
                // 恢復Sprite位置和顏色
                if (enemySprite != null)
                {
                    enemySprite.transform.localPosition = originalSpritePosition;
                    enemySprite.color = Color.white; // 恢復完全不透明
                }
                
                // 自動設置敵人圖示
                if (enemySprite != null && currentStageData.enemyIcon != null)
                {
                    enemySprite.sprite = currentStageData.enemyIcon;
                    Debug.Log($"[EnemyController] 設置敵人圖示: {currentStageData.enemyIcon.name}");
                }
                
                Debug.Log($"[EnemyController] 初始化: {currentStageData.stageName}, HP: {currentHp}");
            }
        }
        
        /// <summary>
        /// 射擊
        /// </summary>
        private void Shoot()
        {
            if (CombatManager.Instance == null) return;
            if (GridManager.Instance == null) return;
            
            // 決定子彈類型
            BulletType bulletType = DetermineBulletType();
            
            // 決定目標列
            int column = DetermineTargetColumn(bulletType);
            
            // 發射子彈（移除 burstCount，永遠單發）
            CombatManager.Instance.FireBullet(
                column,
                bulletType,
                GameConstants.BULLET_DAMAGE,
                currentStageData.bulletSpeed
            );
        }
        
        /// <summary>
        /// 根據子彈類型智能決定目標列
        /// </summary>
        private int DetermineTargetColumn(BulletType bulletType)
        {
            // 如果啟用智能射擊
            if (currentStageData.useSmartTargeting && GridManager.Instance != null)
            {
                // AddBlock 類型子彈優先射擊高點
                if ((bulletType == BulletType.AddBlock || bulletType == BulletType.AddExplosiveBlock) 
                    && currentStageData.addBlockTargetsHigh)
                {
                    int highestColumn = GridManager.Instance.GetHighestColumn();
                    if (highestColumn >= 0)
                    {
                        Debug.Log($"[EnemyController] 智能射擊：AddBlock 目標高點列 {highestColumn}");
                        return highestColumn;
                    }
                }
                
                // AreaDamage 子彈優先射擊低點
                if (bulletType == BulletType.AreaDamage && currentStageData.areaDamageTargetsLow)
                {
                    int lowestColumn = GridManager.Instance.GetLowestColumn();
                    if (lowestColumn >= 0)
                    {
                        Debug.Log($"[EnemyController] 智能射擊：AreaDamage 目標低點列 {lowestColumn}");
                        return lowestColumn;
                    }
                }
            }
            
            // 預設：隨機列
            int randomColumn = Random.Range(0, GameConstants.BOARD_WIDTH);
            return randomColumn;
        }
        
        /// <summary>
        /// 決定子彈類型
        /// </summary>
        private BulletType DetermineBulletType()
        {
            float rand = Random.value;
            float cumulativeChance = 0f;
            
            // 腐化虛無方塊
            if (currentStageData.corruptVoidBullet.enabled)
            {
                cumulativeChance += currentStageData.corruptVoidBullet.chance;
                if (rand < cumulativeChance)
                {
                    return BulletType.CorruptVoid;
                }
            }
            
            // 腐化爆炸方塊
            if (currentStageData.corruptExplosiveBullet.enabled)
            {
                cumulativeChance += currentStageData.corruptExplosiveBullet.chance;
                if (rand < cumulativeChance)
                {
                    return BulletType.CorruptExplosive;
                }
            }
            
            // 插入虛無垃圾行
            if (currentStageData.addVoidRowBullet.enabled)
            {
                cumulativeChance += currentStageData.addVoidRowBullet.chance;
                if (rand < cumulativeChance)
                {
                    return BulletType.InsertVoidRow;
                }
            }
            
            // 插入普通垃圾行
            if (currentStageData.addRowBullet.enabled)
            {
                cumulativeChance += currentStageData.addRowBullet.chance;
                if (rand < cumulativeChance)
                {
                    return BulletType.InsertRow;
                }
            }
            
            // 範圍傷害
            if (currentStageData.areaBullet.enabled)
            {
                cumulativeChance += currentStageData.areaBullet.chance;
                if (rand < cumulativeChance)
                {
                    return BulletType.AreaDamage;
                }
            }
            
            // 添加爆炸方塊
            if (currentStageData.addExplosiveBlockBullet.enabled)
            {
                cumulativeChance += currentStageData.addExplosiveBlockBullet.chance;
                if (rand < cumulativeChance)
                {
                    return BulletType.AddExplosiveBlock;
                }
            }
            
            // 添加普通方塊
            if (currentStageData.addBlockBullet.enabled)
            {
                cumulativeChance += currentStageData.addBlockBullet.chance;
                if (rand < cumulativeChance)
                {
                    return BulletType.AddBlock;
                }
            }
            
            // 預設：普通子彈
            return BulletType.Normal;
        }
        
        /// <summary>
        /// 處理受傷
        /// </summary>
        private void HandleDamaged(float damage)
        {
            // 視覺效果：爆炸特效（即使已擊敗也要累積，確保同一批導彈的特效都能顯示）
            // 每發導彈產生 1 個特效（不受傷害加成影響）
            if (damageEffectPrefab != null && enemySprite != null)
            {
                pendingEffectCount += 1;
                
                // 如果還沒有在生成特效，啟動協程
                if (effectSpawnCoroutine == null)
                {
                    effectSpawnCoroutine = StartCoroutine(SpawnDamageEffectsSequentially());
                }
            }
            
            // 如果已經被擊敗，只累積特效，不再處理傷害和擊敗邏輯
            if (isDefeated) return;
            
            currentHp = Mathf.Max(0f, currentHp - damage);
            
            Debug.Log($"[EnemyController] 受到傷害: {damage}, 剩餘HP: {currentHp}");
            
            // 視覺效果：晃動
            if (enemySprite != null)
            {
                if (shakeCoroutine != null)
                {
                    StopCoroutine(shakeCoroutine);
                }
                shakeCoroutine = StartCoroutine(ShakeSprite());
            }
            
            // 檢查是否被擊敗
            if (currentHp <= 0f)
            {
                HandleDefeated();
            }
        }
        
        /// <summary>
        /// 敵人Sprite晃動效果（使用 unscaledDeltaTime 確保暫停時也能播放）
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
                
                enemySprite.transform.localPosition = originalSpritePosition + offset;
                
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            
            enemySprite.transform.localPosition = originalSpritePosition;
            shakeCoroutine = null;
        }
        
        /// <summary>
        /// 依序生成多個受傷特效（協程）
        /// 每幀最多生成2個特效
        /// </summary>
        private System.Collections.IEnumerator SpawnDamageEffectsSequentially()
        {
            while (pendingEffectCount > 0)
            {
                // 每次生成最多2個特效
                int effectsThisFrame = Mathf.Min(2, pendingEffectCount);
                
                for (int i = 0; i < effectsThisFrame; i++)
                {
                    SpawnSingleDamageEffect();
                    pendingEffectCount--;
                }
                
                // 等待一小段時間再生成下一批（使用 Realtime 確保暫停時也能執行）
                yield return new WaitForSecondsRealtime(0.05f);
            }
            
            effectSpawnCoroutine = null;
        }
        
        /// <summary>
        /// 在敵人Sprite隨機位置生成單個受傷特效
        /// </summary>
        private void SpawnSingleDamageEffect()
        {
            if (enemySprite == null || damageEffectPrefab == null) return;
            
            // 獲取Sprite的邊界
            Bounds spriteBounds = enemySprite.bounds;
            
            // 在Sprite範圍內隨機位置
            Vector3 randomPosition = new Vector3(
                Random.Range(spriteBounds.min.x, spriteBounds.max.x),
                Random.Range(spriteBounds.min.y, spriteBounds.max.y),
                enemySprite.transform.position.z
            );
            
            // 實例化爆炸特效
            GameObject effect = Instantiate(damageEffectPrefab, randomPosition, Quaternion.identity);
            
            // 設置特效在 Time.timeScale = 0 時也能播放
            var animator = effect.GetComponent<Animator>();
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }
            
            var particleSystems = effect.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                var main = ps.main;
                main.useUnscaledTime = true;
            }
            
            // 使用協程銷毀，確保在 Time.timeScale = 0 時也能正常計時
            StartCoroutine(DestroyAfterRealtime(effect, 2f));
        }
        
        /// <summary>
        /// 延遲銷毀物件（使用真實時間，不受 Time.timeScale 影響）
        /// </summary>
        private System.Collections.IEnumerator DestroyAfterRealtime(GameObject obj, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        
        /// <summary>
        /// 處理被擊敗
        /// </summary>
        private void HandleDefeated()
        {
            isDefeated = true;
            
            // 不停止晃動效果和爆炸特效，讓最後一擊的所有特效都能完整播放
            
            Debug.Log($"[EnemyController] 敵人被擊敗！開始淡化過渡...");
            
            // 啟動淡化過渡（不立即暫停，讓特效先播放）
            StartCoroutine(DefeatTransitionCoroutine());
        }
        
        /// <summary>
        /// 擊敗過渡協程：淡化敵人圖片後進入升級畫面
        /// 同時執行淡化、左右搖晃、往下移動效果
        /// </summary>
        private System.Collections.IEnumerator DefeatTransitionCoroutine()
        {
            Color originalColor = enemySprite != null ? enemySprite.color : Color.white;
            Vector3 startPosition = enemySprite != null ? enemySprite.transform.localPosition : Vector3.zero;
            
            // ========== 第一階段：快速搖晃 + 開始下降（1秒） ==========
            float phase1Duration = 1f;
            float phase1Elapsed = 0f;
            
            // 搖晃參數
            float shakeFrequency = 20f;  // 搖晃頻率（每秒震動次數）
            float shakeIntensity = 0.3f; // 搖晃強度
            
            // 第一階段下移距離
            float phase1FallDistance = 2f;
            
            while (phase1Elapsed < phase1Duration)
            {
                phase1Elapsed += Time.deltaTime;
                float progress = phase1Elapsed / phase1Duration;
                
                // 1. 淡化效果（1.0 → 0.3）
                float alpha = Mathf.Lerp(1f, 0.3f, progress);
                
                // 2. 左右搖晃效果（使用 Sin 波形，隨時間衰減）
                float shakeAmount = Mathf.Sin(phase1Elapsed * shakeFrequency) * shakeIntensity * (1f - progress);
                
                // 3. 往下移動效果（使用緩出曲線）
                float fallAmount = Mathf.Lerp(0f, phase1FallDistance, progress * progress);
                
                if (enemySprite != null)
                {
                    // 更新顏色（淡化）
                    enemySprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                    
                    // 更新位置（搖晃 + 下移）
                    enemySprite.transform.localPosition = new Vector3(
                        startPosition.x + shakeAmount,  // X軸：左右搖晃
                        startPosition.y - fallAmount,   // Y軸：往下移動
                        startPosition.z
                    );
                }
                
                yield return null;
            }
            
            // ========== 第二階段：繼續下降 + 完全淡出（2秒） ==========
            float phase2Duration = 2f;
            float phase2Elapsed = 0f;
            
            // 第二階段額外下移距離
            float phase2FallDistance = 3f;
            
            // 記錄第一階段結束時的位置
            Vector3 phase2StartPosition = enemySprite != null ? enemySprite.transform.localPosition : startPosition;
            
            while (phase2Elapsed < phase2Duration)
            {
                phase2Elapsed += Time.deltaTime;
                float progress = phase2Elapsed / phase2Duration;
                
                // 1. 繼續淡化（0.3 → 0，完全透明）
                float alpha = Mathf.Lerp(0.3f, 0f, progress);
                
                // 2. 繼續往下移動（緩慢勻速）
                float fallAmount = Mathf.Lerp(0f, phase2FallDistance, progress);
                
                if (enemySprite != null)
                {
                    // 更新顏色（淡化）
                    enemySprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                    
                    // 更新位置（只有下移，不搖晃）
                    enemySprite.transform.localPosition = new Vector3(
                        phase2StartPosition.x,           // X軸：保持不動
                        phase2StartPosition.y - fallAmount,  // Y軸：繼續往下移動
                        phase2StartPosition.z
                    );
                }
                
                yield return null;
            }
            
            Debug.Log($"[EnemyController] 淡化過渡完成，進入升級畫面");
            GameEvents.TriggerEnemyDefeated();
        }
        
        /// <summary>
        /// 處理遊戲狀態變更
        /// </summary>
        private void HandleGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.Playing:
                    InitializeEnemy();
                    break;
                    
                case GameState.LevelUp:
                    // 關卡切換時重新初始化
                    Invoke(nameof(InitializeEnemy), 0.1f);
                    break;
            }
        }
    }
}

