using UnityEngine;
using Tenronis.Data;
using Tenronis.Core;
using Tenronis.Managers;
using Tenronis.ScriptableObjects;

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
            
            // 獲取連發數量
            int burstCount = currentStageData.burstCount;
            
            // 發射子彈
            CombatManager.Instance.FireBullet(
                column,
                bulletType,
                GameConstants.BULLET_DAMAGE,
                currentStageData.bulletSpeed,
                burstCount
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
                // AddBlock 子彈優先射擊高點
                if (bulletType == BulletType.AddBlock && currentStageData.addBlockTargetsHigh)
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
            
            // Stage 10: 可以使用插入行
            if (currentStageData.canUseInsertRow && rand < currentStageData.insertRowChance)
            {
                return BulletType.InsertRow;
            }
            
            // Stage 8+: 可以使用範圍傷害
            if (currentStageData.canUseAreaDamage && rand < currentStageData.areaDamageChance + currentStageData.insertRowChance)
            {
                return BulletType.AreaDamage;
            }
            
            // Stage 5+: 可以使用添加方塊
            if (currentStageData.canUseAddBlock && rand < currentStageData.addBlockChance + currentStageData.areaDamageChance + currentStageData.insertRowChance)
            {
                return BulletType.AddBlock;
            }
            
            // 預設：普通子彈
            return BulletType.Normal;
        }
        
        /// <summary>
        /// 處理受傷
        /// </summary>
        private void HandleDamaged(float damage)
        {
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
            
            // 視覺效果：爆炸特效（累積到隊列）
            if (damageEffectPrefab != null && enemySprite != null)
            {
                pendingEffectCount++;
                
                // 如果還沒有在生成特效，啟動協程
                if (effectSpawnCoroutine == null)
                {
                    effectSpawnCoroutine = StartCoroutine(SpawnDamageEffectsSequentially());
                }
            }
            
            if (currentHp <= 0f)
            {
                HandleDefeated();
            }
        }
        
        /// <summary>
        /// 敵人Sprite晃動效果
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
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            enemySprite.transform.localPosition = originalSpritePosition;
            shakeCoroutine = null;
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
            Destroy(effect, 2f); // 2秒後銷毀
        }
        
        /// <summary>
        /// 處理被擊敗
        /// </summary>
        private void HandleDefeated()
        {
            Debug.Log($"[EnemyController] 敵人被擊敗！");
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

