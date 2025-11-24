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
        [SerializeField] private Slider playerHpSlider;
        [SerializeField] private TextMeshProUGUI playerHpText;
        [SerializeField] private Slider enemyHpSlider;
        [SerializeField] private TextMeshProUGUI enemyHpText;
        [SerializeField] private TextMeshProUGUI stageText;
        
        [Header("技能UI")]
        [SerializeField] private TextMeshProUGUI executionCountText;
        [SerializeField] private TextMeshProUGUI repairCountText;
        
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
            // 綁定按鈕事件
            if (startButton != null)
                startButton.onClick.AddListener(OnStartGame);
            
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestart);
            
            if (menuButton != null)
                menuButton.onClick.AddListener(OnReturnToMenu);
            
            // 訂閱遊戲事件
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
            
            // 初始化UI
            ShowMenu();
        }
        
        private void OnDestroy()
        {
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
            
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
                        comboText.text = $"COMBO x{stats.comboCount}";
                        comboText.gameObject.SetActive(true);
                    }
                    else
                    {
                        comboText.gameObject.SetActive(false);
                    }
                }
                
                // 玩家HP
                if (playerHpSlider != null)
                {
                    playerHpSlider.maxValue = stats.maxHp;
                    playerHpSlider.value = stats.currentHp;
                }
                
                if (playerHpText != null)
                    playerHpText.text = $"{stats.currentHp} / {stats.maxHp}";
                
                // 技能
                if (executionCountText != null)
                    executionCountText.text = $"x{stats.executionCount}";
                
                if (repairCountText != null)
                    repairCountText.text = $"x{stats.repairCount}";
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
            GameManager.Instance?.StartGame();
        }
        
        private void OnRestart()
        {
            GameManager.Instance?.RestartGame();
        }
        
        private void OnReturnToMenu()
        {
            GameManager.Instance?.ReturnToMenu();
        }
    }
}

