using UnityEngine;
using System.Collections.Generic;
using Tenronis.Data;
using Tenronis.Core;
using Tenronis.Gameplay.Projectiles;
using Tenronis.Utilities;

namespace Tenronis.Managers
{
    /// <summary>
    /// 戰鬥管理器 - 處理導彈、子彈、碰撞
    /// </summary>
    public class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance { get; private set; }
        
        [Header("預製體")]
        [SerializeField] private Missile missilePrefab;
        [SerializeField] private Bullet bulletPrefab;
        
        [Header("容器")]
        [SerializeField] private Transform projectileContainer;
        
        // 對象池
        private ObjectPool<Missile> missilePool;
        private ObjectPool<Bullet> bulletPool;
        
        // 活躍物件追蹤
        private List<Missile> activeMissiles = new List<Missile>();
        private List<Bullet> activeBullets = new List<Bullet>();
        
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
            // 初始化對象池
            if (projectileContainer == null)
            {
                projectileContainer = new GameObject("ProjectileContainer").transform;
                projectileContainer.SetParent(transform);
            }
            
            if (missilePrefab != null)
                missilePool = new ObjectPool<Missile>(missilePrefab, projectileContainer, 50);
            
            if (bulletPrefab != null)
                bulletPool = new ObjectPool<Bullet>(bulletPrefab, projectileContainer, 30);
            
            // 訂閱事件
            GameEvents.OnRowsCleared += HandleRowsCleared;
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
        }
        
        private void OnDestroy()
        {
            GameEvents.OnRowsCleared -= HandleRowsCleared;
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        }
        
        private void Update()
        {
            if (GameManager.Instance.CurrentState != GameState.Playing) return;
            
            // 處理碰撞
            ProcessCollisions();
        }
        
        /// <summary>
        /// 發射導彈（消除行時觸發）
        /// </summary>
        private void HandleRowsCleared(int rowCount)
        {
            if (rowCount <= 0) return;
            
            var stats = PlayerManager.Instance.Stats;
            
            // 計算傷害加成
            float salvoBonus = rowCount > 1 ? (rowCount - 1) * (stats.salvoLevel * GameConstants.SALVO_DAMAGE_MULTIPLIER) : 0f;
            float burstBonus = stats.burstLevel * stats.comboCount * GameConstants.BURST_DAMAGE_MULTIPLIER;
            float totalDamage = (GameConstants.BASE_MISSILE_DAMAGE + salvoBonus + burstBonus);
            
            // 每個方塊位置發射導彈
            int missileCountPerBlock = 1 + stats.missileExtraCount;
            
            // 簡化：每消除一行，在隨機列發射導彈群
            for (int row = 0; row < rowCount; row++)
            {
                for (int x = 0; x < GameConstants.BOARD_WIDTH; x++)
                {
                    for (int i = 0; i < missileCountPerBlock; i++)
                    {
                        Vector3 spawnPos = GridManager.Instance.GridToWorldPosition(x, GameConstants.BOARD_HEIGHT - 1 - row);
                        spawnPos.y += i * 0.2f; // 錯開發射
                        
                        FireMissile(spawnPos, totalDamage);
                    }
                }
            }
            
            GameEvents.TriggerPlayMissileSound();
        }
        
        /// <summary>
        /// 發射單個導彈
        /// </summary>
        public void FireMissile(Vector3 position, float damage, int pierce = 0)
        {
            if (missilePool == null) return;
            
            Missile missile = missilePool.Get();
            missile.Initialize(position, damage, pierce);
            activeMissiles.Add(missile);
            
            GameEvents.TriggerMissileFired(damage);
        }
        
        /// <summary>
        /// 發射子彈（敵人攻擊）
        /// </summary>
        public void FireBullet(int column, BulletType type, int damage, float speed)
        {
            if (bulletPool == null) return;
            
            Vector3 spawnPos = GridManager.Instance.GridToWorldPosition(column, -1);
            
            Bullet bullet = bulletPool.Get();
            bullet.Initialize(spawnPos, type, damage, speed);
            activeBullets.Add(bullet);
        }
        
        /// <summary>
        /// 處理碰撞
        /// </summary>
        private void ProcessCollisions()
        {
            // 導彈 vs 子彈
            ProcessMissileVsBulletCollisions();
            
            // 導彈擊中敵人（超出上邊界）
            ProcessMissileHitEnemy();
            
            // 子彈擊中方塊或玩家
            ProcessBulletHitGrid();
        }
        
        /// <summary>
        /// 處理導彈vs子彈碰撞
        /// </summary>
        private void ProcessMissileVsBulletCollisions()
        {
            List<Missile> missilesToRemove = new List<Missile>();
            List<Bullet> bulletsToRemove = new List<Bullet>();
            
            foreach (var missile in activeMissiles)
            {
                if (!missile.gameObject.activeInHierarchy) continue;
                
                foreach (var bullet in activeBullets)
                {
                    if (!bullet.gameObject.activeInHierarchy) continue;
                    
                    float distance = Vector3.Distance(missile.transform.position, bullet.transform.position);
                    if (distance < 0.5f)
                    {
                        // 碰撞！
                        bulletsToRemove.Add(bullet);
                        
                        if (!missile.ConsumePierce())
                        {
                            missilesToRemove.Add(missile);
                        }
                        
                        break;
                    }
                }
            }
            
            // 清理
            foreach (var missile in missilesToRemove)
            {
                activeMissiles.Remove(missile);
                missilePool.Return(missile);
            }
            
            foreach (var bullet in bulletsToRemove)
            {
                activeBullets.Remove(bullet);
                bulletPool.Return(bullet);
            }
        }
        
        /// <summary>
        /// 處理導彈擊中敵人
        /// </summary>
        private void ProcessMissileHitEnemy()
        {
            List<Missile> missilesToRemove = new List<Missile>();
            
            foreach (var missile in activeMissiles)
            {
                if (!missile.gameObject.activeInHierarchy) continue;
                
                // 超出上邊界代表擊中敵人
                if (missile.transform.position.y > GameConstants.BOARD_HEIGHT)
                {
                    GameEvents.TriggerEnemyDamaged(missile.Damage);
                    missilesToRemove.Add(missile);
                }
            }
            
            foreach (var missile in missilesToRemove)
            {
                activeMissiles.Remove(missile);
                missilePool.Return(missile);
            }
        }
        
        /// <summary>
        /// 處理子彈擊中網格
        /// </summary>
        private void ProcessBulletHitGrid()
        {
            List<Bullet> bulletsToRemove = new List<Bullet>();
            
            foreach (var bullet in activeBullets)
            {
                if (!bullet.gameObject.activeInHierarchy) continue;
                
                Vector2Int gridPos = GridManager.Instance.WorldToGridPosition(bullet.transform.position);
                
                // 擊中玩家基地
                if (gridPos.y >= GameConstants.BOARD_HEIGHT)
                {
                    GameEvents.TriggerPlayerDamaged(bullet.Damage);
                    GameEvents.TriggerPlayImpactSound();
                    bulletsToRemove.Add(bullet);
                    continue;
                }
                
                // 擊中方塊
                if (GridManager.Instance.IsValidPosition(gridPos.x, gridPos.y))
                {
                    BlockData block = GridManager.Instance.GetBlock(gridPos.x, gridPos.y);
                    
                    if (block != null)
                    {
                        ProcessBulletEffect(bullet, gridPos, block);
                        bulletsToRemove.Add(bullet);
                    }
                }
            }
            
            foreach (var bullet in bulletsToRemove)
            {
                activeBullets.Remove(bullet);
                bulletPool.Return(bullet);
            }
        }
        
        /// <summary>
        /// 處理子彈效果
        /// </summary>
        private void ProcessBulletEffect(Bullet bullet, Vector2Int hitPos, BlockData hitBlock)
        {
            GameEvents.TriggerPlayImpactSound();
            
            switch (bullet.BulletType)
            {
                case BulletType.Normal:
                    GridManager.Instance.DamageBlock(hitPos.x, hitPos.y, 1);
                    CheckCounterFire(hitPos, hitBlock);
                    break;
                    
                case BulletType.AddBlock:
                    GridManager.Instance.DamageBlock(hitPos.x, hitPos.y, 1);
                    CheckCounterFire(hitPos, hitBlock);
                    
                    // 在上方添加垃圾方塊
                    if (hitPos.y - 1 >= 0)
                    {
                        BlockData garbageBlock = new BlockData(BlockColor.Garbage, GameConstants.GARBAGE_BLOCK_HP, GameConstants.GARBAGE_BLOCK_HP);
                        GridManager.Instance.SetBlock(hitPos.x, hitPos.y - 1, garbageBlock);
                    }
                    break;
                    
                case BulletType.AreaDamage:
                    // 3x3範圍傷害
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int x = hitPos.x + dx;
                            int y = hitPos.y + dy;
                            GridManager.Instance.DamageBlock(x, y, 1);
                        }
                    }
                    break;
                    
                case BulletType.InsertRow:
                    GridManager.Instance.DamageBlock(hitPos.x, hitPos.y, 1);
                    CheckCounterFire(hitPos, hitBlock);
                    
                    // 插入不可摧毀行
                    GridManager.Instance.InsertIndestructibleRow();
                    break;
            }
        }
        
        /// <summary>
        /// 檢查反擊
        /// </summary>
        private void CheckCounterFire(Vector2Int hitPos, BlockData hitBlock)
        {
            var stats = PlayerManager.Instance.Stats;
            if (stats.counterFireLevel <= 0) return;
            
            // 檢查方塊是否在反擊時間窗口內
            float blockAge = Time.time - hitBlock.createdTime;
            if (blockAge <= GameConstants.COUNTER_FIRE_TIME_WINDOW)
            {
                // 觸發反擊
                PlayerManager.Instance.CancelComboReset();
                stats.comboCount++;
                GameEvents.TriggerComboChanged(stats.comboCount);
                
                // 發射反擊導彈
                float burstBonus = stats.burstLevel * stats.comboCount * GameConstants.BURST_DAMAGE_MULTIPLIER;
                float damage = (GameConstants.BASE_MISSILE_DAMAGE + burstBonus);
                
                for (int i = 0; i < stats.counterFireLevel; i++)
                {
                    Vector3 firePos = GridManager.Instance.GridToWorldPosition(hitPos.x, hitPos.y);
                    firePos.y -= i * 0.2f;
                    FireMissile(firePos, damage);
                }
                
                GameEvents.TriggerShowPopupText("反擊!", new Color(0.29f, 0.87f, 0.5f), GridManager.Instance.GridToWorldPosition(hitPos.x, hitPos.y));
            }
        }
        
        /// <summary>
        /// 清理所有彈藥
        /// </summary>
        public void ClearAllProjectiles()
        {
            foreach (var missile in activeMissiles)
            {
                missilePool.Return(missile);
            }
            activeMissiles.Clear();
            
            foreach (var bullet in activeBullets)
            {
                bulletPool.Return(bullet);
            }
            activeBullets.Clear();
        }
        
        /// <summary>
        /// 處理遊戲狀態變更
        /// </summary>
        private void HandleGameStateChanged(GameState newState)
        {
            if (newState != GameState.Playing)
            {
                ClearAllProjectiles();
            }
        }
    }
}

