using UnityEngine;
using Tenronis.Managers;
using Tenronis.Gameplay.Tetromino;
using Tenronis.Gameplay.Enemy;
using Tenronis.UI;
using Tenronis.Audio;

namespace Tenronis.Core
{
    /// <summary>
    /// 遊戲初始化器 - 確保所有必要系統都被創建
    /// </summary>
    public class GameInitializer : MonoBehaviour
    {
        [Header("管理器預製體（如果場景中沒有）")]
        [SerializeField] private GameObject gameManagerPrefab;
        [SerializeField] private GameObject gridManagerPrefab;
        [SerializeField] private GameObject playerManagerPrefab;
        [SerializeField] private GameObject combatManagerPrefab;
        [SerializeField] private GameObject audioManagerPrefab;
        [SerializeField] private GameObject inputManagerPrefab;
        
        [Header("遊戲物件預製體")]
        [SerializeField] private GameObject tetrominoControllerPrefab;
        [SerializeField] private GameObject enemyControllerPrefab;
        
        private void Awake()
        {
            // 確保所有必要的單例管理器存在
            EnsureManagerExists<GameManager>(gameManagerPrefab);
            EnsureManagerExists<GridManager>(gridManagerPrefab);
            EnsureManagerExists<PlayerManager>(playerManagerPrefab);
            EnsureManagerExists<CombatManager>(combatManagerPrefab);
            EnsureManagerExists<AudioManager>(audioManagerPrefab);
            
            // InputManager不需要是單例
            if (FindFirstObjectByType<InputManager>() == null && inputManagerPrefab != null)
            {
                Instantiate(inputManagerPrefab);
            }
            
            // 遊戲控制器
            if (TetrominoController.Instance == null && tetrominoControllerPrefab != null)
            {
                Instantiate(tetrominoControllerPrefab);
            }
            
            if (EnemyController.Instance == null && enemyControllerPrefab != null)
            {
                Instantiate(enemyControllerPrefab);
            }
        }
        
        /// <summary>
        /// 確保管理器存在
        /// </summary>
        private void EnsureManagerExists<T>(GameObject prefab) where T : MonoBehaviour
        {
            if (FindFirstObjectByType<T>() == null && prefab != null)
            {
                Instantiate(prefab);
            }
        }
    }
}

