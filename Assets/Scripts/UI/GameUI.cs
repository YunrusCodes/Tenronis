using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tenronis.Data;
using Tenronis.Core;
using Tenronis.Managers;
using Tenronis.Gameplay.Enemy;

namespace Tenronis.UI
{
    /// <summary>
    /// 主遊戲UI
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [Header("主選單")]
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private Button startButton;
        
        [Header("遊戲中UI")]
        [SerializeField] private GameObject gameplayPanel;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private TextMeshProUGUI salvoText;  // 齊射提示文字
        [SerializeField] private Slider playerHpSlider;
        [SerializeField] private TextMeshProUGUI playerHpText;
        [SerializeField] private Slider playerCpSlider;
        [SerializeField] private TextMeshProUGUI playerCpText;
        [SerializeField] private Slider enemyHpSlider;
        [SerializeField] private TextMeshProUGUI enemyHpText;
        [SerializeField] private TextMeshProUGUI stageText;
        
        [Header("技能UI")]
        [SerializeField] private TextMeshProUGUI explosionDamageText;
        [SerializeField] private TextMeshProUGUI executionKeyLabelText; // 處決技能按鍵標籤（顯示"1"或"Locked"）
        [SerializeField] private TextMeshProUGUI executionCostText; // 處決技能CP消耗顯示
        [SerializeField] private TextMeshProUGUI repairKeyLabelText; // 修補技能按鍵標籤（顯示"2"或"Locked"）
        [SerializeField] private TextMeshProUGUI repairCostText; // 修補技能CP消耗顯示
        
        // 齊射文字顯示計時
        private float salvoDisplayTimer = 0f;
        private int lastClearedRows = 0;
        
        [Header("升級UI")]
        [SerializeField] private GameObject levelUpPanel;
        
        [Header("遊戲結束")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;
        
        private void Start()
        {
            Debug.Log("[GameUI] Start() - 初始化GameUI");
            
            // 檢查GameManager是否存在（重要！）
            if (GameManager.Instance == null)
            {
                Debug.LogError("❌ [GameUI] 場景中找不到GameManager！遊戲無法運行！");
                Debug.LogError("   解決方法：在Hierarchy中建立GameManager物件並添加GameManager腳本");
                Debug.LogError("   參考文件：Assets/快速設置管理器.md");
            }
            else
            {
                Debug.Log("✅ [GameUI] GameManager已找到");
            }
            
            // 綁定按鈕事件
            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartGame);
                Debug.Log("[GameUI] StartButton已綁定事件");
            }
            else
            {
                Debug.LogError("[GameUI] StartButton參考為空！請在Inspector中設置");
            }
            
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestart);
                Debug.Log("[GameUI] RestartButton已綁定事件");
            }
            
            if (menuButton != null)
            {
                menuButton.onClick.AddListener(OnReturnToMenu);
                Debug.Log("[GameUI] MenuButton已綁定事件");
            }
            
            // 訂閱遊戲事件
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
            GameEvents.OnRowsCleared += HandleRowsClearedForSalvo;
            GameEvents.OnSkillUnlocked += UpdateSkillUI;
            
            // 初始化UI
            ShowMenu();
            Debug.Log("[GameUI] 初始化完成，顯示主選單");
        }
        
        private void OnDestroy()
        {
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
            GameEvents.OnRowsCleared -= HandleRowsClearedForSalvo;
            GameEvents.OnSkillUnlocked -= UpdateSkillUI;
            
            if (startButton != null)
                startButton.onClick.RemoveListener(OnStartGame);
            
            if (restartButton != null)
                restartButton.onClick.RemoveListener(OnRestart);
            
            if (menuButton != null)
                menuButton.onClick.RemoveListener(OnReturnToMenu);
        }
        
        private void Update()
        {
            if (GameManager.Instance.CurrentState == GameState.Playing)
            {
                UpdateGameplayUI();
                UpdateSalvoDisplay();
            }
        }
        
        /// <summary>
        /// 更新遊戲中UI
        /// </summary>
        private void UpdateGameplayUI()
        {
            if (PlayerManager.Instance != null)
            {
                var stats = PlayerManager.Instance.Stats;
                
                // 分數
                if (scoreText != null)
                    scoreText.text = stats.score.ToString("N0");
                
                // Combo
                if (comboText != null)
                {
                    if (stats.comboCount > 1)
                    {
                        comboText.text = $"{stats.comboCount}連發!";
                        comboText.gameObject.SetActive(true);
                    }
                    else
                    {
                        comboText.gameObject.SetActive(false);
                    }
                }
                
                // 齊射顯示由 UpdateSalvoDisplay() 處理
                
                // 玩家HP
                if (playerHpSlider != null)
                {
                    playerHpSlider.maxValue = stats.maxHp;
                    playerHpSlider.value = stats.currentHp;
                }
                
                if (playerHpText != null)
                    playerHpText.text = $"{stats.currentHp} / {stats.maxHp}";
                
                // 玩家CP (Castle Point)
                if (playerCpSlider != null)
                {
                    playerCpSlider.maxValue = stats.maxCp;
                    playerCpSlider.value = stats.currentCp;
                }
                
                if (playerCpText != null)
                    playerCpText.text = $"CP: {stats.currentCp} / {stats.maxCp}";
                
                // 爆炸充能
                if (explosionDamageText != null)
                {
                    explosionDamageText.text = $"{stats.explosionCharge}/{stats.explosionMaxCharge}";
                }
                
                // 更新技能UI
                UpdateSkillUI();
            }
            
            // 敵人HP
            if (EnemyController.Instance != null)
            {
                if (enemyHpSlider != null)
                {
                    enemyHpSlider.maxValue = EnemyController.Instance.MaxHp;
                    enemyHpSlider.value = EnemyController.Instance.CurrentHp;
                }
                
                if (enemyHpText != null)
                    enemyHpText.text = $"{Mathf.Ceil(EnemyController.Instance.CurrentHp)} / {EnemyController.Instance.MaxHp}";
            }
            
            // 關卡
            if (stageText != null && GameManager.Instance != null)
            {
                stageText.text = $"STAGE {GameManager.Instance.CurrentStageIndex + 1} / {GameManager.Instance.TotalStages}";
            }
        }
        
        /// <summary>
        /// 顯示主選單
        /// </summary>
        private void ShowMenu()
        {
            SetPanelActive(menuPanel, true);
            SetPanelActive(gameplayPanel, false);
            SetPanelActive(levelUpPanel, false);
            SetPanelActive(gameOverPanel, false);
            SetPanelActive(victoryPanel, false);
        }
        
        /// <summary>
        /// 顯示遊戲中UI
        /// </summary>
        private void ShowGameplay()
        {
            SetPanelActive(menuPanel, false);
            SetPanelActive(gameplayPanel, true);
            SetPanelActive(levelUpPanel, false);
            SetPanelActive(gameOverPanel, false);
            SetPanelActive(victoryPanel, false);
        }
        
        /// <summary>
        /// 顯示升級選單
        /// </summary>
        private void ShowLevelUp()
        {
            SetPanelActive(menuPanel, false);
            SetPanelActive(gameplayPanel, true);
            SetPanelActive(levelUpPanel, true);
            SetPanelActive(gameOverPanel, false);
            SetPanelActive(victoryPanel, false);
        }
        
        /// <summary>
        /// 顯示遊戲結束
        /// </summary>
        private void ShowGameOver()
        {
            SetPanelActive(menuPanel, false);
            SetPanelActive(gameplayPanel, false);
            SetPanelActive(levelUpPanel, false);
            SetPanelActive(gameOverPanel, true);
            SetPanelActive(victoryPanel, false);
            
            if (finalScoreText != null && PlayerManager.Instance != null)
                finalScoreText.text = $"最終分數: {PlayerManager.Instance.Stats.score:N0}";
        }
        
        /// <summary>
        /// 顯示勝利
        /// </summary>
        private void ShowVictory()
        {
            SetPanelActive(menuPanel, false);
            SetPanelActive(gameplayPanel, false);
            SetPanelActive(levelUpPanel, false);
            SetPanelActive(gameOverPanel, false);
            SetPanelActive(victoryPanel, true);
            
            if (finalScoreText != null && PlayerManager.Instance != null)
                finalScoreText.text = $"最終分數: {PlayerManager.Instance.Stats.score:N0}";
        }
        
        /// <summary>
        /// 設置面板活躍狀態
        /// </summary>
        private void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
                panel.SetActive(active);
        }
        
        /// <summary>
        /// 處理遊戲狀態變更
        /// </summary>
        private void HandleGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.Menu:
                    ShowMenu();
                    break;
                    
                case GameState.Playing:
                    ShowGameplay();
                    break;
                    
                case GameState.LevelUp:
                    ShowLevelUp();
                    break;
                    
                case GameState.GameOver:
                    ShowGameOver();
                    break;
                    
                case GameState.Victory:
                    ShowVictory();
                    break;
            }
        }
        
        // 按鈕回調
        private void OnStartGame()
        {
            Debug.Log("=== [GameUI] OnStartGame() 被觸發！===");
            
            if (GameManager.Instance != null)
            {
                Debug.Log("[GameUI] 調用 GameManager.StartGame()");
                GameManager.Instance.StartGame();
            }
            else
            {
                Debug.LogError("[GameUI] GameManager.Instance 為空！無法開始遊戲");
            }
        }
        
        private void OnRestart()
        {
            Debug.Log("=== [GameUI] OnRestart() 被觸發！===");
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }
            else
            {
                Debug.LogError("[GameUI] GameManager.Instance 為空！");
            }
        }
        
        private void OnReturnToMenu()
        {
            Debug.Log("=== [GameUI] OnReturnToMenu() 被觸發！===");
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReturnToMenu();
            }
            else
            {
                Debug.LogError("[GameUI] GameManager.Instance 為空！");
            }
        }
        
        /// <summary>
        /// 處理消除行事件（用於齊射顯示）
        /// </summary>
        private void HandleRowsClearedForSalvo(int totalRowCount, int nonGarbageRowCount, bool hasVoid)
        {
            // 虛無抵銷：不顯示齊射文字
            // （"虛無抵銷!"會由 PlayerManager 的 pop-up text 顯示）
            if (hasVoid)
            {
                return;
            }
            
            // 正常齊射顯示（2 排非垃圾方塊以上）
            if (nonGarbageRowCount >= 2)
            {
                lastClearedRows = nonGarbageRowCount;
                salvoDisplayTimer = 2f; // 顯示 2 秒
            }
        }
        
        /// <summary>
        /// 更新齊射文字顯示
        /// </summary>
        private void UpdateSalvoDisplay()
        {
            if (salvoText == null) return;
            
            // 如果有顯示計時器
            if (salvoDisplayTimer > 0)
            {
                salvoDisplayTimer -= Time.deltaTime;
                
                // 根據消除行數顯示不同文字和顏色
                if (lastClearedRows >= 5)
                {
                    salvoText.text = $"超級齊射 x{lastClearedRows}!";
                    salvoText.color = new Color(1f, 0.84f, 0f); // 金色
                }
                else
                {
                    salvoText.text = $"齊射 x{lastClearedRows}!";
                    salvoText.color = new Color(0.13f, 0.83f, 0.93f); // 青色
                }
                
                salvoText.gameObject.SetActive(true);
                
                // 時間到了就隱藏
                if (salvoDisplayTimer <= 0)
                {
                    salvoText.gameObject.SetActive(false);
                }
            }
            else
            {
                salvoText.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 更新技能UI顯示
        /// </summary>
        private void UpdateSkillUI()
        {
            if (PlayerManager.Instance == null) return;
            
            var stats = PlayerManager.Instance.Stats;
            
            // 處決技能UI
            if (executionKeyLabelText != null)
            {
                if (PlayerManager.Instance.IsExecutionUnlocked())
                {
                    executionKeyLabelText.text = "1";
                }
                else
                {
                    executionKeyLabelText.text = "Locked";
                }
            }
            
            if (executionCostText != null)
            {
                if (PlayerManager.Instance.IsExecutionUnlocked())
                {
                    executionCostText.text = $"CP-{GameConstants.EXECUTION_CP_COST}";
                }
                else
                {
                    executionCostText.text = "";
                }
            }
            
            // 修補技能UI
            if (repairKeyLabelText != null)
            {
                if (PlayerManager.Instance.IsRepairUnlocked())
                {
                    repairKeyLabelText.text = "2";
                }
                else
                {
                    repairKeyLabelText.text = "Locked";
                }
            }
            
            if (repairCostText != null)
            {
                if (PlayerManager.Instance.IsRepairUnlocked())
                {
                    repairCostText.text = $"CP-{GameConstants.REPAIR_CP_COST}";
                }
                else
                {
                    repairCostText.text = "";
                }
            }
        }
    }
}

