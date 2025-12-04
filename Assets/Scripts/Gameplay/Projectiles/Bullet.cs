using UnityEngine;
using Tenronis.Data;

namespace Tenronis.Gameplay.Projectiles
{
    /// <summary>
    /// 敵人子彈
    /// </summary>
    public class Bullet : MonoBehaviour
    {
        [Header("視覺")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;
        
        [Header("子彈類型配置")]
        [SerializeField] private RuntimeAnimatorController normalController;
        [SerializeField] private Color normalColor = new Color(0.94f, 0.27f, 0.27f); // 紅色
        
        [SerializeField] private RuntimeAnimatorController addBlockController;
        [SerializeField] private Color addBlockColor = new Color(0.29f, 0.87f, 0.5f); // 綠色
        
        [SerializeField] private RuntimeAnimatorController areaDamageController;
        [SerializeField] private Color areaDamageColor = new Color(0.98f, 0.45f, 0.13f); // 橙色
        
        [SerializeField] private RuntimeAnimatorController insertRowController;
        [SerializeField] private Color insertRowColor = new Color(0.66f, 0.33f, 0.97f); // 紫色
        
        [SerializeField] private RuntimeAnimatorController addExplosiveBlockController;
        [SerializeField] private Color addExplosiveBlockColor = new Color(1f, 0.92f, 0.016f); // 黃色
        
        [SerializeField] private RuntimeAnimatorController insertVoidRowController;
        [SerializeField] private Color insertVoidRowColor = new Color(0.2f, 0.2f, 0.2f); // 深灰色
        
        [SerializeField] private RuntimeAnimatorController corruptExplosiveController;
        [SerializeField] private Color corruptExplosiveColor = new Color(1f, 0f, 1f); // 洋紅色
        
        [SerializeField] private RuntimeAnimatorController corruptVoidController;
        [SerializeField] private Color corruptVoidColor = new Color(0f, 1f, 1f); // 青色
        
        // 子彈數據
        private BulletType bulletType;
        private int damage;
        private float speed;
        
        public BulletType BulletType => bulletType;
        public int Damage => damage;
        
        // 公開顏色配置供預覽使用
        public Color NormalColor => normalColor;
        public Color AddBlockColor => addBlockColor;
        public Color AreaDamageColor => areaDamageColor;
        public Color InsertRowColor => insertRowColor;
        public Color AddExplosiveBlockColor => addExplosiveBlockColor;
        public Color InsertVoidRowColor => insertVoidRowColor;
        public Color CorruptExplosiveColor => corruptExplosiveColor;
        public Color CorruptVoidColor => corruptVoidColor;
        
        /// <summary>
        /// 根據子彈類型獲取對應顏色
        /// </summary>
        public Color GetColorByType(BulletType type)
        {
            return type switch
            {
                BulletType.Normal => normalColor,
                BulletType.AddBlock => addBlockColor,
                BulletType.AreaDamage => areaDamageColor,
                BulletType.InsertRow => insertRowColor,
                BulletType.AddExplosiveBlock => addExplosiveBlockColor,
                BulletType.InsertVoidRow => insertVoidRowColor,
                BulletType.CorruptExplosive => corruptExplosiveColor,
                BulletType.CorruptVoid => corruptVoidColor,
                _ => Color.white
            };
        }
        
        /// <summary>
        /// 根據子彈類型獲取對應動畫控制器
        /// </summary>
        public RuntimeAnimatorController GetAnimatorByType(BulletType type)
        {
            return type switch
            {
                BulletType.Normal => normalController,
                BulletType.AddBlock => addBlockController,
                BulletType.AreaDamage => areaDamageController,
                BulletType.InsertRow => insertRowController,
                BulletType.AddExplosiveBlock => addExplosiveBlockController,
                BulletType.InsertVoidRow => insertVoidRowController,
                BulletType.CorruptExplosive => corruptExplosiveController,
                BulletType.CorruptVoid => corruptVoidController,
                _ => null
            };
        }
        
        /// <summary>
        /// 初始化子彈
        /// </summary>
        public void Initialize(Vector3 position, BulletType type, int damage, float speed)
        {
            transform.position = position;
            this.bulletType = type;
            this.damage = damage;
            this.speed = speed;
            
            UpdateVisual();
        }
        
        private void Update()
        {
            // 向下移動
            transform.position += Vector3.down * speed * Time.deltaTime;
            
            // 超出範圍時銷毀（低於Grid底部）
            if (Tenronis.Managers.GridManager.Instance != null)
            {
                float gridBottom = Tenronis.Managers.GridManager.Instance.GridOffset.y - 
                                   (GameConstants.BOARD_HEIGHT * Tenronis.Managers.GridManager.Instance.BlockSize);
                
                if (transform.position.y < gridBottom - 2f)
                {
                    ReturnToPool();
                }
            }
            else
            {
                // 備用：使用固定值
                if (transform.position.y < -10f)
                {
                    ReturnToPool();
                }
            }
        }
        
        /// <summary>
        /// 更新視覺
        /// </summary>
        private void UpdateVisual()
        {
            if (spriteRenderer == null) return;
            
            switch (bulletType)
            {
                case BulletType.Normal:
                    spriteRenderer.color = normalColor;
                    if (animator != null && normalController != null)
                        animator.runtimeAnimatorController = normalController;
                    break;
                    
                case BulletType.AddBlock:
                    spriteRenderer.color = addBlockColor;
                    if (animator != null && addBlockController != null)
                        animator.runtimeAnimatorController = addBlockController;
                    break;
                    
                case BulletType.AreaDamage:
                    spriteRenderer.color = areaDamageColor;
                    if (animator != null && areaDamageController != null)
                        animator.runtimeAnimatorController = areaDamageController;
                    break;
                    
                case BulletType.InsertRow:
                    spriteRenderer.color = insertRowColor;
                    if (animator != null && insertRowController != null)
                        animator.runtimeAnimatorController = insertRowController;
                    break;

                case BulletType.AddExplosiveBlock:
                    spriteRenderer.color = addExplosiveBlockColor;
                    if (animator != null && addExplosiveBlockController != null)
                        animator.runtimeAnimatorController = addExplosiveBlockController;
                    break;

                case BulletType.InsertVoidRow:
                    spriteRenderer.color = insertVoidRowColor;
                    if (animator != null && insertVoidRowController != null)
                        animator.runtimeAnimatorController = insertVoidRowController;
                    break;

                case BulletType.CorruptExplosive:
                    spriteRenderer.color = corruptExplosiveColor;
                    if (animator != null && corruptExplosiveController != null)
                        animator.runtimeAnimatorController = corruptExplosiveController;
                    break;

                case BulletType.CorruptVoid:
                    spriteRenderer.color = corruptVoidColor;
                    if (animator != null && corruptVoidController != null)
                        animator.runtimeAnimatorController = corruptVoidController;
                    break;
            }
        }
        
        /// <summary>
        /// 返回對象池
        /// </summary>
        private void ReturnToPool()
        {
            gameObject.SetActive(false);
        }
    }
}

