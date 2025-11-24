using UnityEngine;
using Tenronis.Data;

namespace Tenronis.Gameplay.Projectiles
{
    /// <summary>
    /// 玩家導彈
    /// </summary>
    public class Missile : MonoBehaviour
    {
        [Header("視覺")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private TrailRenderer trailRenderer;
        
        // 導彈數據
        private float damage;
        private int pierce; // 穿透次數
        private Vector2 velocity;
        
        public float Damage => damage;
        public int Pierce => pierce;
        
        /// <summary>
        /// 初始化導彈
        /// </summary>
        public void Initialize(Vector3 position, float damage, int pierce = 0)
        {
            transform.position = position;
            this.damage = damage;
            this.pierce = pierce;
            
            // 向上飛行
            velocity = Vector2.up * GameConstants.MISSILE_SPEED;
            
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(0.13f, 0.83f, 0.93f); // 青色
            }
        }
        
        private void Update()
        {
            // 移動
            transform.position += (Vector3)velocity * Time.deltaTime;
            
            // 超出範圍時銷毀
            if (transform.position.y > GameConstants.BOARD_HEIGHT + 5)
            {
                ReturnToPool();
            }
        }
        
        /// <summary>
        /// 減少穿透次數
        /// </summary>
        public bool ConsumePierce()
        {
            if (pierce > 0)
            {
                pierce--;
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 返回對象池
        /// </summary>
        private void ReturnToPool()
        {
            // 由CombatManager處理
            gameObject.SetActive(false);
        }
    }
}

