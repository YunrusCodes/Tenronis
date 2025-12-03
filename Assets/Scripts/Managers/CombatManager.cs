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
        
        [Header("視覺特效")]
        [SerializeField] private GameObject explosionEffectPrefab; // 飛彈攔截子彈的爆炸特效
        
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
        private void HandleRowsCleared(int totalRowCount, int nonGarbageRowCount, bool hasVoid)
        {
            if (nonGarbageRowCount <= 0) return;
            
            // 虛無抵銷：不發射導彈
            if (hasVoid)
            {
                Debug.Log($"[CombatManager] 虛無抵銷！消除了 {totalRowCount} 行但不發射導彈");
                GameEvents.TriggerPlayVoidNullifySound();
                return;
            }
            
            // 只為非垃圾方塊行發射導彈
            Debug.Log($"[CombatManager] 消除了 {totalRowCount} 行（其中 {nonGarbageRowCount} 行非垃圾方塊），發射 {nonGarbageRowCount} 行導彈");
            
            var stats = PlayerManager.Instance.Stats;
            
            // 計算傷害加成（基於非垃圾方塊行數，超過4行以4計算）
            int effectiveRowCount = Mathf.Min(nonGarbageRowCount, 4);
            float salvoBonus = effectiveRowCount > 1 ? (effectiveRowCount - 1) * (stats.salvoLevel * GameConstants.SALVO_DAMAGE_MULTIPLIER) : 0f;
            float burstBonus = stats.burstLevel * stats.comboCount * GameConstants.BURST_DAMAGE_MULTIPLIER;
            float totalDamage = (GameConstants.BASE_MISSILE_DAMAGE + salvoBonus + burstBonus);
            
            // 每個方塊位置發射導彈
            int missileCountPerBlock = 1 + stats.missileExtraCount;
            
            // 簡化：每消除一行非垃圾方塊，在隨機列發射導彈群
            for (int row = 0; row < nonGarbageRowCount; row++)
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
        /// 發射子彈(敵人攻擊)
        /// </summary>
        public void FireBullet(int column, BulletType type, int damage, float speed)
        {
            if (bulletPool == null) return;
            
            // 在 Grid 頂部（y=0）上方 2 個單位生成子彈
            Vector3 spawnPos = GridManager.Instance.GridToWorldPosition(column, 0);
            spawnPos.y += 2f;
            
            Debug.Log($"[CombatManager] 發射子彈 - 列: {column}, 類型: {type}, 位置: {spawnPos}, 速度: {speed}");
            
            // 播放敵人射擊音效
            GameEvents.TriggerPlayEnemyShootSound(type);
            
            // 發射單發子彈
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
            
            // 使用 for 循環從後往前遍歷，避免集合修改錯誤
            for (int i = activeMissiles.Count - 1; i >= 0; i--)
            {
                if (i >= activeMissiles.Count) continue; // 安全檢查
                var missile = activeMissiles[i];
                
                if (!missile.gameObject.activeInHierarchy)
                {
                    // 清理不活躍的導彈
                    missilesToRemove.Add(missile);
                    continue;
                }
                
                for (int j = activeBullets.Count - 1; j >= 0; j--)
                {
                    if (j >= activeBullets.Count) continue; // 安全檢查
                    var bullet = activeBullets[j];
                    
                    if (!bullet.gameObject.activeInHierarchy)
                    {
                        if (!bulletsToRemove.Contains(bullet))
                        {
                            bulletsToRemove.Add(bullet);
                        }
                        continue;
                    }
                    
                    float distance = Vector3.Distance(missile.transform.position, bullet.transform.position);
                    if (distance < 0.5f)
                    {
                        // 碰撞！記錄碰撞位置
                        Vector3 collisionPosition = (missile.transform.position + bullet.transform.position) / 2f;
                        
                        // 播放爆炸特效
                        if (explosionEffectPrefab != null)
                        {
                            GameObject explosion = Instantiate(explosionEffectPrefab, collisionPosition, Quaternion.identity);
                            Destroy(explosion, 2f); // 2秒後銷毀特效
                        }
                        
                        // 播放爆炸音效
                        GameEvents.TriggerPlayExplosionSound();
                        
                        // 輕微屏幕震動
                        if (VFX.ScreenShake.Instance != null)
                        {
                            VFX.ScreenShake.Instance.Shake(0.08f, 0.05f);
                        }
                        
                        if (!bulletsToRemove.Contains(bullet))
                        {
                            bulletsToRemove.Add(bullet);
                        }
                        
                        if (!missile.ConsumePierce())
                        {
                            if (!missilesToRemove.Contains(missile))
                            {
                                missilesToRemove.Add(missile);
                            }
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
            
            // 計算 Grid 頂部的世界座標
            float gridTop = GridManager.Instance.GridOffset.y + 1f;
            
            // 使用 for 循環從後往前遍歷，避免集合修改錯誤
            for (int i = activeMissiles.Count - 1; i >= 0; i--)
            {
                if (i >= activeMissiles.Count) continue; // 安全檢查
                var missile = activeMissiles[i];
                
                if (!missile.gameObject.activeInHierarchy)
                {
                    // 清理不活躍的導彈
                    missilesToRemove.Add(missile);
                    continue;
                }
                
                // 超出 Grid 頂部代表擊中敵人
                if (missile.transform.position.y > gridTop)
                {
                    Debug.Log($"[CombatManager] 導彈擊中敵人！傷害: {missile.Damage}");
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
            
            // 使用 for 循環從後往前遍歷，避免集合修改錯誤
            for (int i = activeBullets.Count - 1; i >= 0; i--)
            {
                if (i >= activeBullets.Count) continue; // 安全檢查
                var bullet = activeBullets[i];
                
                if (!bullet.gameObject.activeInHierarchy)
                {
                    // 清理不活躍的子彈
                    bulletsToRemove.Add(bullet);
                    continue;
                }
                
                Vector2Int gridPos = GridManager.Instance.WorldToGridPosition(bullet.transform.position);
                
                // 擊中玩家基地（子彈超過Grid底部）
                if (gridPos.y >= GameConstants.BOARD_HEIGHT)
                {
                    Debug.Log($"[CombatManager] 子彈擊中基地！Grid位置: ({gridPos.x}, {gridPos.y}), 傷害: {bullet.Damage}");
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
                        Debug.Log($"[CombatManager] 子彈擊中方塊！Grid位置: ({gridPos.x}, {gridPos.y}), 類型: {bullet.BulletType}");
                        ProcessBulletEffect(bullet, gridPos, block);
                        bulletsToRemove.Add(bullet);
                    }
                }
            }
            
            // 清理（去重）
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
                    
                    // 在上方添加普通垃圾方塊
                    if (hitPos.y - 1 >= 0)
                    {
                        int defenseLevel = PlayerManager.Instance != null ? PlayerManager.Instance.Stats.blockDefenseLevel : 0;
                        int garbageHp = GameConstants.GARBAGE_BLOCK_HP + defenseLevel;
                        
                        BlockData garbageBlock = new BlockData(
                            BlockColor.Garbage,
                            garbageHp,
                            garbageHp,
                            false,
                            BlockType.Normal
                        );
                        GridManager.Instance.SetBlock(hitPos.x, hitPos.y - 1, garbageBlock);
                        
                        Debug.Log($"[CombatManager] 敵人添加普通垃圾方塊 HP: {garbageHp}");
                        GameEvents.TriggerPlayEnemyAddBlockSound();
                        CheckRowsAfterAddBlock();
                    }
                    else if (hitPos.y == 0)
                    {
                        // 頂部溢出：清空網格並造成傷害（50% 當前HP）
                        Debug.Log("[CombatManager] AddBlock 溢出！方塊堆到頂部，清空網格");
                        GridManager.Instance.HandleOverflow();
                    }
                    break;
                    
                case BulletType.AddExplosiveBlock:
                    GridManager.Instance.DamageBlock(hitPos.x, hitPos.y, 1);
                    CheckCounterFire(hitPos, hitBlock);
                    
                    // 在上方添加爆炸垃圾方塊
                    if (hitPos.y - 1 >= 0)
                    {
                        int defenseLevel = PlayerManager.Instance != null ? PlayerManager.Instance.Stats.blockDefenseLevel : 0;
                        int garbageHp = GameConstants.GARBAGE_BLOCK_HP + defenseLevel;
                        
                        BlockData explosiveBlock = new BlockData(
                            BlockColor.Garbage,
                            garbageHp,
                            garbageHp,
                            false,
                            BlockType.Explosive
                        );
                        GridManager.Instance.SetBlock(hitPos.x, hitPos.y - 1, explosiveBlock);
                        
                        Debug.Log($"[CombatManager] 敵人添加爆炸垃圾方塊 HP: {garbageHp}");
                        GameEvents.TriggerPlayEnemyAddBlockSound();
                        CheckRowsAfterAddBlock();
                    }
                    else if (hitPos.y == 0)
                    {
                        // 頂部溢出：清空網格並造成傷害（50% 當前HP）
                        Debug.Log("[CombatManager] AddExplosiveBlock 溢出！方塊堆到頂部，清空網格");
                        GridManager.Instance.HandleOverflow();
                    }
                    break;
                    
                case BulletType.AreaDamage:
                    // 檢查中心方塊是否能觸發彈反
                    CheckCounterFire(hitPos, hitBlock);
                    
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
                    
                    // 插入普通不可摧毀行
                    GridManager.Instance.InsertIndestructibleRow(BlockType.Normal);
                    break;
                    
                case BulletType.InsertVoidRow:
                    GridManager.Instance.DamageBlock(hitPos.x, hitPos.y, 1);
                    CheckCounterFire(hitPos, hitBlock);
                    
                    // 插入虛無不可摧毀行
                    GridManager.Instance.InsertIndestructibleRow(BlockType.Void);
                    break;
                    
                case BulletType.CorruptExplosive:
                    GridManager.Instance.DamageBlock(hitPos.x, hitPos.y, 1);
                    CheckCounterFire(hitPos, hitBlock);
                    
                    // 腐化下個方塊的隨機一格為爆炸方塊
                    if (Gameplay.Tetromino.TetrominoController.Instance != null)
                    {
                        Gameplay.Tetromino.TetrominoController.Instance.CorruptNextPiece(BlockType.Explosive);
                        GameEvents.TriggerPlayCorruptSound(BlockType.Explosive);
                        Debug.Log("[CombatManager] 下個方塊被腐化為爆炸方塊！");
                    }
                    break;
                    
                case BulletType.CorruptVoid:
                    GridManager.Instance.DamageBlock(hitPos.x, hitPos.y, 1);
                    CheckCounterFire(hitPos, hitBlock);
                    
                    // 腐化下個方塊的隨機一格為虛無方塊
                    if (Gameplay.Tetromino.TetrominoController.Instance != null)
                    {
                        Gameplay.Tetromino.TetrominoController.Instance.CorruptNextPiece(BlockType.Void);
                        GameEvents.TriggerPlayCorruptSound(BlockType.Void);
                        Debug.Log("[CombatManager] 下個方塊被腐化為虛無方塊！");
                    }
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
                
                // 播放反擊音效
                GameEvents.TriggerPlayCounterFireSound();
                
                // 觸發玩家反擊視覺特效
                if (Gameplay.Player.PlayerVisualController.Instance != null)
                {
                    Gameplay.Player.PlayerVisualController.Instance.TriggerCounterFireEffect();
                }
                
                GameEvents.TriggerShowPopupText("反擊!", new Color(0.29f, 0.87f, 0.5f), GridManager.Instance.GridToWorldPosition(hitPos.x, hitPos.y));
                
                // 增加爆炸充能（反擊一次增加5充能）
                if (PlayerManager.Instance != null)
                {
                    PlayerManager.Instance.AddExplosionCharge(GameConstants.EXPLOSION_COUNTER_CHARGE);
                }
            }
        }
        
        /// <summary>
        /// 檢查敵人添加方塊後是否形成完整行
        /// </summary>
        private void CheckRowsAfterAddBlock()
        {
            if (GridManager.Instance == null) return;
            
            List<int> clearedRows = GridManager.Instance.CheckAndClearRows();
            
            // 注意：CheckAndClearRows 內部已經觸發 TriggerRowsCleared
            // 這裡不需要再次觸發，只需要記錄日誌
            if (clearedRows.Count > 0)
            {
                Debug.Log($"[CombatManager] 敵人添加方塊形成完整行！消除了 {clearedRows.Count} 行");
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

