using UnityEngine;
using Tenronis.Managers;
using Tenronis.Gameplay.Tetromino;
using Tenronis.Gameplay.Enemy;
using Tenronis.Audio;

namespace Tenronis.Utilities
{
    /// <summary>
    /// 場景設置檢查器 - 在遊戲啟動時檢查所有必要組件是否存在
    /// </summary>
    public class SceneSetupChecker : MonoBehaviour
    {
        [Header("啟動時自動檢查")]
        [SerializeField] private bool checkOnStart = true;
        
        private void Start()
        {
            if (checkOnStart)
            {
                CheckSceneSetup();
            }
        }
        
        /// <summary>
        /// 檢查場景設置
        /// </summary>
        [ContextMenu("檢查場景設置")]
        public void CheckSceneSetup()
        {
            Debug.Log("=== 開始檢查場景設置 ===");
            
            int errorCount = 0;
            int warningCount = 0;
            
            // 檢查管理器
            errorCount += CheckManager<GameManager>("GameManager");
            errorCount += CheckManager<GridManager>("GridManager");
            errorCount += CheckManager<PlayerManager>("PlayerManager");
            errorCount += CheckManager<CombatManager>("CombatManager");
            errorCount += CheckManager<AudioManager>("AudioManager");
            warningCount += CheckComponent<InputManager>("InputManager", false);
            
            // 檢查控制器
            errorCount += CheckController<TetrominoController>("TetrominoController");
            errorCount += CheckController<EnemyController>("EnemyController");
            
            // 檢查EventSystem
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                Debug.LogError("❌ 找不到 EventSystem！UI無法接收事件");
                Debug.LogError("   解決方法：Hierarchy右鍵 > UI > Event System");
                errorCount++;
            }
            else
            {
                Debug.Log("✅ EventSystem 存在");
            }
            
            // 檢查Canvas
            if (FindFirstObjectByType<Canvas>() == null)
            {
                Debug.LogError("❌ 找不到 Canvas！UI無法顯示");
                Debug.LogError("   解決方法：Hierarchy右鍵 > UI > Canvas");
                errorCount++;
            }
            else
            {
                Debug.Log("✅ Canvas 存在");
            }
            
            // 總結
            Debug.Log("=== 檢查完成 ===");
            if (errorCount > 0)
            {
                Debug.LogError($"⚠️ 發現 {errorCount} 個錯誤！遊戲可能無法正常運行");
                Debug.LogError("請參考錯誤訊息修正場景設置");
                Debug.LogError("快速設置指南：Assets/快速設置管理器.md");
            }
            else if (warningCount > 0)
            {
                Debug.LogWarning($"⚠️ 發現 {warningCount} 個警告");
            }
            else
            {
                Debug.Log("✅ 場景設置完整！可以開始遊戲");
            }
        }
        
        private int CheckManager<T>(string name) where T : MonoBehaviour
        {
            if (FindFirstObjectByType<T>() == null)
            {
                Debug.LogError($"❌ 找不到 {name}！");
                Debug.LogError($"   解決方法：建立空物件命名為 '{name}'，添加 {typeof(T).Name} 腳本");
                return 1;
            }
            else
            {
                Debug.Log($"✅ {name} 存在");
                return 0;
            }
        }
        
        private int CheckController<T>(string name) where T : MonoBehaviour
        {
            if (FindFirstObjectByType<T>() == null)
            {
                Debug.LogError($"❌ 找不到 {name}！");
                Debug.LogError($"   解決方法：建立空物件命名為 '{name}'，添加 {typeof(T).Name} 腳本");
                return 1;
            }
            else
            {
                Debug.Log($"✅ {name} 存在");
                return 0;
            }
        }
        
        private int CheckComponent<T>(string name, bool isError = true) where T : Component
        {
            if (FindFirstObjectByType<T>() == null)
            {
                if (isError)
                {
                    Debug.LogError($"❌ 找不到 {name}！");
                    return 1;
                }
                else
                {
                    Debug.LogWarning($"⚠️ 找不到 {name}");
                    return 1;
                }
            }
            else
            {
                Debug.Log($"✅ {name} 存在");
                return 0;
            }
        }
    }
}

