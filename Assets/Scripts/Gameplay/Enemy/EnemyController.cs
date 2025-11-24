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
        
        // 敵人狀態
        private StageDataSO currentStageData;
        private float currentHp;
        private float shootTimer;
        
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
                
                Debug.Log($"[EnemyController] 初始化: {currentStageData.stageName}, HP: {currentHp}");
            }
        }
        
        /// <summary>
        /// 射擊
        /// </summary>
        private void Shoot()
        {
            if (CombatManager.Instance == null) return;
            
            // 隨機列
            int column = Random.Range(0, GameConstants.BOARD_WIDTH);
            
            // 決定子彈類型
            BulletType bulletType = DetermineBulletType();
            
            // 發射子彈
            CombatManager.Instance.FireBullet(
                column,
                bulletType,
                GameConstants.BULLET_DAMAGE,
                currentStageData.bulletSpeed
            );
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
            
            if (currentHp <= 0f)
            {
                HandleDefeated();
            }
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

