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
        
        // 導彈數據
        private float damage;
        private int pierce; // 穿透次數
        private Vector2 velocity;
        
        public float Damage => damage;
        public int Pierce => pierce;
        
        /// <summary>
        /// 初始化導彈
        /// </summary>
        public void Initialize(Vector3 position, float damage, int pierce = 0, int volleyLevel = 0)
        {
            transform.position = position;
            this.damage = damage;
            this.pierce = pierce;
            
            // 向上飛行
            velocity = Vector2.up * GameConstants.MISSILE_SPEED;
            
            // 根據 Volley 等級設置顏色
            Color missileColor = GetVolleyColor(volleyLevel);
            
            if (spriteRenderer != null)
            {
                spriteRenderer.color = missileColor;
            }
        }
        
        /// <summary>
        /// 根據 Volley 等級獲取顏色
        /// </summary>
        public static Color GetVolleyColor(int volleyLevel)
        {
            if (volleyLevel <= 0)
                return Color.white;                              // Lv0: 白色
            else if (volleyLevel == 1)
                return new Color(1f, 0.5f, 0f);                  // Lv1: 橘色
            else if (volleyLevel == 2)
                return new Color(1f, 0.2f, 0.2f);                // Lv2: 紅色
            else
                return new Color(0.3f, 0.5f, 1f);                // Lv3+: 藍色
        }
        
        private void Update()
        {
            // 移動
            transform.position += (Vector3)velocity * Time.deltaTime;
            
            // 超出範圍時銷毀（高於Grid頂部）
            if (Tenronis.Managers.GridManager.Instance != null)
            {
                float gridTop = Tenronis.Managers.GridManager.Instance.GridOffset.y + 5f;
                
                if (transform.position.y > gridTop)
                {
                    ReturnToPool();
                }
            }
            else
            {
                // 備用：使用固定值
                if (transform.position.y > 20f)
                {
                    ReturnToPool();
                }
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

