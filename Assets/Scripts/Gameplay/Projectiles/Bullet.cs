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
        
        // 子彈數據
        private BulletType bulletType;
        private int damage;
        private float speed;
        
        public BulletType BulletType => bulletType;
        public int Damage => damage;
        
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
                    spriteRenderer.color = new Color(0.94f, 0.27f, 0.27f); // 紅色
                    transform.localScale = Vector3.one * 0.3f;
                    break;
                    
                case BulletType.AddBlock:
                    spriteRenderer.color = new Color(0.29f, 0.87f, 0.5f); // 綠色
                    transform.localScale = Vector3.one * 0.3f;
                    break;
                    
                case BulletType.AreaDamage:
                    spriteRenderer.color = new Color(0.98f, 0.45f, 0.13f); // 橙色
                    transform.localScale = Vector3.one * 0.4f;
                    break;
                    
                case BulletType.InsertRow:
                    spriteRenderer.color = new Color(0.66f, 0.33f, 0.97f); // 紫色
                    transform.localScale = Vector3.one * 0.35f;
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

