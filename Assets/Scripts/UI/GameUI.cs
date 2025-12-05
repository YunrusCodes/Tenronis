using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tenronis.Data;
using Tenronis.Core;
using Tenronis.Managers;
using Tenronis.Gameplay.Enemy;
using Tenronis.ScriptableObjects;
using System.Collections.Generic;

namespace Tenronis.UI
{
    /// <summary>
    /// 主遊戲UI
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [Header("主選單")]
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private GameObject themeListPanel;
        
        [Header("按鈕與預製件")]
        [SerializeField] private Button themeButtonPrefab; // 用於生成主題按鈕
        [SerializeField] private Transform themeButtonContainer; // 主題按鈕容器
        
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
        [SerializeField] private TextMeshProUGUI executionKeyLabelText;
        [SerializeField] private TextMeshProUGUI executionCostText;
        [SerializeField] private TextMeshProUGUI repairKeyLabelText;
        [SerializeField] private TextMeshProUGUI repairCostText;
        [SerializeField] private TextMeshProUGUI annihilationKeyLabelText;
        [SerializeField] private TextMeshProUGUI annihilationCostText;
        
        [Header("敵人受傷計數器")]
        [SerializeField] private TextMeshProUGUI enemyDamageCounterText;
        [SerializeField] private float damageCounterResetTime = 3f; // 無傷害後歸零時間
        
        // 齊射文字顯示計時
        private float salvoDisplayTimer = 0f;
        private int lastClearedRows = 0;
        
        // 敵人受傷計數器
        private float accumulatedEnemyDamage = 0f;
        private float damageCounterTimer = 0f;
        
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
            
            if (GameManager.Instance == null)
            {
                Debug.LogError("❌ [GameUI] 場景中找不到GameManager！");
                return;
            }
            
            // 綁定按鈕事件
            if (restartButton != null) restartButton.onClick.AddListener(OnRestart);
            if (menuButton != null) menuButton.onClick.AddListener(OnReturnToMenu);
            
            // 訂閱遊戲事件
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
            GameEvents.OnRowsCleared += HandleRowsClearedForSalvo;
            GameEvents.OnSkillUnlocked += UpdateSkillUI;
            GameEvents.OnEnemyDamaged += HandleEnemyDamaged;
            
            // 初始化UI
            ShowMenu();
        }
        
        private void OnDestroy()
        {
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
            GameEvents.OnRowsCleared -= HandleRowsClearedForSalvo;
            GameEvents.OnSkillUnlocked -= UpdateSkillUI;
            GameEvents.OnEnemyDamaged -= HandleEnemyDamaged;
            
            if (restartButton != null) restartButton.onClick.RemoveListener(OnRestart);
            if (menuButton != null) menuButton.onClick.RemoveListener(OnReturnToMenu);
        }
        
        private void Update()
        {
            if (GameManager.Instance.CurrentState == GameState.Playing)
            {
                UpdateGameplayUI();
                UpdateSalvoDisplay();
                UpdateEnemyDamageCounter();
            }
        }
        
        // --- UI 狀態控制 ---
        
        private void ShowMenu()
        {
            SetPanelActive(menuPanel, true);
            SetPanelActive(gameplayPanel, false);
            SetPanelActive(levelUpPanel, false);
            SetPanelActive(gameOverPanel, false);
            SetPanelActive(victoryPanel, false);
            
            // 預設顯示主題選擇
            ShowThemeSelection();
        }
        
        private void ShowThemeSelection()
        {
            SetPanelActive(themeListPanel, true);
            
            // 動態生成主題按鈕
            if (themeButtonContainer != null && themeButtonPrefab != null)
            {
                // 清除舊按鈕
                foreach (Transform child in themeButtonContainer)
                {
                    Destroy(child.gameObject);
                }
                
                // 生成新按鈕
                var themes = GameManager.Instance.allThemes;
                for (int i = 0; i < themes.Count; i++)
                {
                    int index = i; // 閉包捕獲
                    var theme = themes[i];
                    var btn = Instantiate(themeButtonPrefab, themeButtonContainer);
                    var btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
                    if (btnText != null) btnText.text = theme.themeName;
                    
                    btn.onClick.AddListener(() => OnThemeSelected(index));
                }
            }
        }
        
        private void OnThemeSelected(int index)
        {
            // 直接開始遊戲
            GameManager.Instance.StartGame(index);
        }
        
        private void ShowGameplay()
        {
            SetPanelActive(menuPanel, false);
            SetPanelActive(gameplayPanel, true);
            SetPanelActive(levelUpPanel, false);
            SetPanelActive(gameOverPanel, false);
            SetPanelActive(victoryPanel, false);
        }
        
        private void ShowLevelUp()
        {
            SetPanelActive(menuPanel, false);
            SetPanelActive(gameplayPanel, true);
            SetPanelActive(levelUpPanel, true);
            SetPanelActive(gameOverPanel, false);
            SetPanelActive(victoryPanel, false);
        }
        
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
        
        private void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null) panel.SetActive(active);
        }
        
        // --- 遊戲邏輯 ---
        
        private void OnRestart()
        {
            GameManager.Instance.RestartGame();
        }
        
        private void OnReturnToMenu()
        {
            GameManager.Instance.ReturnToMenu();
        }
        
        private void HandleGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.Menu: ShowMenu(); break;
                case GameState.Playing: ShowGameplay(); break;
                case GameState.LevelUp: ShowLevelUp(); break;
                case GameState.GameOver: ShowGameOver(); break;
                case GameState.Victory: ShowVictory(); break;
            }
        }
        
        // --- UI 更新 (保持原有邏輯) ---
        
        private void UpdateGameplayUI()
        {
            if (PlayerManager.Instance != null)
            {
                var stats = PlayerManager.Instance.Stats;
                if (scoreText != null) scoreText.text = stats.score.ToString("N0");
                if (comboText != null)
                {
                    comboText.text = stats.comboCount > 1 ? $"{stats.comboCount}連發!" : "";
                    comboText.gameObject.SetActive(stats.comboCount > 1);
                }
                
                if (playerHpSlider != null) { playerHpSlider.maxValue = stats.maxHp; playerHpSlider.value = stats.currentHp; }
                if (playerHpText != null) playerHpText.text = $"HP: {stats.currentHp} / {stats.maxHp}";
                if (playerCpSlider != null) { playerCpSlider.maxValue = stats.maxCp; playerCpSlider.value = stats.currentCp; }
                if (playerCpText != null) playerCpText.text = $"CP: {stats.currentCp} / {stats.maxCp}";
                if (explosionDamageText != null) explosionDamageText.text = $"衝擊炮充能 : {stats.explosionCharge}/{stats.explosionMaxCharge}";
                
                UpdateSkillUI();
            }
            
            if (EnemyController.Instance != null)
            {
                if (enemyHpSlider != null) { enemyHpSlider.maxValue = EnemyController.Instance.MaxHp; enemyHpSlider.value = EnemyController.Instance.CurrentHp; }
                if (enemyHpText != null) enemyHpText.text = $"{Mathf.Ceil(EnemyController.Instance.CurrentHp)} / {EnemyController.Instance.MaxHp}";
            }
            
            if (stageText != null && GameManager.Instance != null)
            {
                stageText.text = $"STAGE {GameManager.Instance.CurrentStageIndex + 1} / {GameManager.Instance.TotalStages}";
            }
        }
        
        private void HandleRowsClearedForSalvo(List<int> clearedRows, int nonGarbageRowCount, bool hasVoid)
        {
            if (hasVoid) return;
            if (nonGarbageRowCount >= 2)
            {
                lastClearedRows = nonGarbageRowCount;
                salvoDisplayTimer = 2f;
            }
        }
        
        private void UpdateSalvoDisplay()
        {
            if (salvoText == null) return;
            if (salvoDisplayTimer > 0)
            {
                salvoDisplayTimer -= Time.deltaTime;
                if (lastClearedRows >= 4)
                {
                    salvoText.text = "全彈齊射!";
                    salvoText.color = new Color(1f, 0.84f, 0f); // 金色
                }
                else if (lastClearedRows == 3)
                {
                    salvoText.text = "三連齊射!";
                    salvoText.color = new Color(1f, 0.5f, 0f); // 橙色
                }
                else if (lastClearedRows == 2)
                {
                    salvoText.text = "雙管齊射!";
                    salvoText.color = new Color(0.13f, 0.83f, 0.93f); // 青色
                }
                salvoText.gameObject.SetActive(lastClearedRows >= 2);
                if (salvoDisplayTimer <= 0) salvoText.gameObject.SetActive(false);
            }
            else
            {
                salvoText.gameObject.SetActive(false);
            }
        }
        
        private void UpdateSkillUI()
        {
            if (PlayerManager.Instance == null) return;
            // 1 -> 湮滅
            if (annihilationKeyLabelText != null) annihilationKeyLabelText.text = PlayerManager.Instance.IsAnnihilationUnlocked() ? "1" : "Locked";
            if (annihilationCostText != null) annihilationCostText.text = PlayerManager.Instance.IsAnnihilationUnlocked() ? $"CP-{GameConstants.ANNIHILATION_CP_COST}" : "";
            // 2 -> 處決
            if (executionKeyLabelText != null) executionKeyLabelText.text = PlayerManager.Instance.IsExecutionUnlocked() ? "2" : "Locked";
            if (executionCostText != null) executionCostText.text = PlayerManager.Instance.IsExecutionUnlocked() ? $"CP-{GameConstants.EXECUTION_CP_COST}" : "";
            // 3 -> 修補
            if (repairKeyLabelText != null) repairKeyLabelText.text = PlayerManager.Instance.IsRepairUnlocked() ? "3" : "Locked";
            if (repairCostText != null) repairCostText.text = PlayerManager.Instance.IsRepairUnlocked() ? $"CP-{GameConstants.REPAIR_CP_COST}" : "";
        }
        
        /// <summary>
        /// 處理敵人受傷事件
        /// </summary>
        private void HandleEnemyDamaged(float damage)
        {
            accumulatedEnemyDamage += damage;
            damageCounterTimer = damageCounterResetTime; // 重置計時器
            
            // 立即更新顯示（格式：<傷害>）
            if (enemyDamageCounterText != null)
            {
                enemyDamageCounterText.gameObject.SetActive(true);
                enemyDamageCounterText.text = $"{accumulatedEnemyDamage:0.#}";
                enemyDamageCounterText.color = new Color(1f, 0.3f, 0.3f); // 紅色
            }
        }
        
        /// <summary>
        /// 更新敵人受傷計數器（3秒無傷害後歸零隱藏）
        /// </summary>
        private void UpdateEnemyDamageCounter()
        {
            if (enemyDamageCounterText == null) return;
            
            if (damageCounterTimer > 0)
            {
                damageCounterTimer -= Time.deltaTime;
                
                // 計時器結束，歸零並隱藏
                if (damageCounterTimer <= 0)
                {
                    accumulatedEnemyDamage = 0f;
                    enemyDamageCounterText.gameObject.SetActive(false);
                }
            }
        }
    }
}

