using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tenronis.Data;
using Tenronis.Core;
using Tenronis.Managers;
using Tenronis.Gameplay.Enemy;
using Tenronis.ScriptableObjects;
using Tenronis.Utils;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

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
        [SerializeField] private Button quitButton; // 退出按鈕
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private TextMeshProUGUI salvoText;  // 齊射提示文字
        [SerializeField] private TextMeshProUGUI impactBlastText; // 衝擊爆破提示文字
        [SerializeField] private TextMeshProUGUI skillText; // 技能施放提示文字
        [SerializeField] private Slider playerHpSlider;
        [SerializeField] private TextMeshProUGUI playerHpText;
        [SerializeField] private Slider playerCpSlider;
        [SerializeField] private TextMeshProUGUI playerCpText;
        [SerializeField] private TextMeshProUGUI overflowCostText; // 溢出CP消耗提示文字
        [SerializeField] private Slider enemyHpSlider;
        [SerializeField] private TextMeshProUGUI enemyHpText;
        [SerializeField] private TextMeshProUGUI stageText;
        
        [Header("技能UI")]
        [SerializeField] private GameObject skillPanel; // 技能面板
        [SerializeField] private TextMeshProUGUI explosionDamageText;
        [SerializeField] private GameObject annihilationSkillObject; // 湮滅技能GameObject
        [SerializeField] private TextMeshProUGUI annihilationKeyLabelText;
        [SerializeField] private TextMeshProUGUI annihilationCostText;
        [SerializeField] private GameObject executionSkillObject; // 處決技能GameObject
        [SerializeField] private TextMeshProUGUI executionKeyLabelText;
        [SerializeField] private TextMeshProUGUI executionCostText;
        [SerializeField] private GameObject repairSkillObject; // 修補技能GameObject
        [SerializeField] private TextMeshProUGUI repairKeyLabelText;
        [SerializeField] private TextMeshProUGUI repairCostText;
        
        [Header("敵人受傷計數器")]
        [SerializeField] private TextMeshProUGUI enemyDamageCounterText;
        [SerializeField] private float damageCounterResetTime = 3f; // 無傷害後歸零時間
        
        // 齊射文字顯示計時
        private float salvoDisplayTimer = 0f;
        private float salvoAnimationTimer = 0f;
        private float salvoAnimationDuration = 0.3f; // 動畫時間
        private bool isVoidNullify = false; // 標記是否為虛無抵銷
        
        // 衝擊爆破文字顯示計時
        private float impactBlastDisplayTimer = 0f;
        private float impactBlastAnimationTimer = 0f;
        private float impactBlastAnimationDuration = 0.3f;
        private float lastImpactBlastDamage = 0f;
        
        // 技能文字顯示計時
        private float skillDisplayTimer = 0f;
        private float skillAnimationTimer = 0f;
        private float skillAnimationDuration = 0.3f;
        private string lastSkillName = "";
        private int lastClearedRows = 0;
        
        // 敵人受傷計數器
        private float accumulatedEnemyDamage = 0f;
        private float damageCounterTimer = 0f;
        
        // FirstPassInfo 字符對象列表
        private List<GameObject> firstPassInfoCharObjects = new List<GameObject>();
        
        // 連發文字動畫
        private Vector2 comboTextOriginalPosition;
        private int lastComboCount = 0;
        private float comboSlideTimer = 0f;
        private float comboSlideDuration = 0.2f; // 滑入動畫時間
        private float comboSlideOffset = 100f; // 從右邊偏移的距離
        
        // 連發數字推動動畫
        private float comboPushTimer = 0f;
        private float comboPushDuration = 0.15f; // 推動動畫時間
        private float comboPushOffset = 30f; // 垂直偏移距離
        private TextMeshProUGUI comboOldText; // 舊數字文字（用於淡出）
        private TextMeshProUGUI comboLabelText; // 「連發!」標籤
        private float comboLabelBaseOffset = 50f; // 標籤與數字的基本間距
        private Color comboTextOriginalColor;
        
        // 原始透明度值
        private float salvoTextOriginalAlpha = 1f;
        private float impactBlastTextOriginalAlpha = 1f;
        private float skillTextOriginalAlpha = 1f;
        private float comboTextOriginalAlpha = 1f;
        
        [Header("升級UI")]
        [SerializeField] private GameObject levelUpPanel;
        
        [Header("遊戲結束 - 失敗")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI gameOverFinalScoreText;
        [SerializeField] private Button gameOverRestartButton;
        [SerializeField] private Button gameOverMenuButton;
        
        [Header("遊戲結束 - 勝利")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private TextMeshProUGUI victoryFinalScoreText;
        [SerializeField] private Button victoryMenuButton;
        [SerializeField] private TextMeshProUGUI firstPassInfoText; // 首次通關資訊文字
        [SerializeField] private Transform firstPassInfoTextContainer; // 包含首次通關資訊文字的容器（用於整體移動）
        
        [Header("退出確認面板")]
        [SerializeField] private GameObject quitPanel;
        [SerializeField] private Button quitYesButton; // 確認退出（返回主選單）
        [SerializeField] private Button quitNoButton; // 取消退出（關閉面板）
        
        private void Start()
        {
            Debug.Log("[GameUI] Start() - 初始化GameUI");
            
            if (GameManager.Instance == null)
            {
                Debug.LogError("❌ [GameUI] 場景中找不到GameManager！");
                return;
            }
            
            // 綁定按鈕事件 - 失敗面板
            if (gameOverRestartButton != null) gameOverRestartButton.onClick.AddListener(OnRestart);
            if (gameOverMenuButton != null) gameOverMenuButton.onClick.AddListener(OnReturnToMenu);
            
            // 綁定按鈕事件 - 勝利面板
            if (victoryMenuButton != null) victoryMenuButton.onClick.AddListener(OnReturnToMenu);
            
            // 綁定按鈕事件 - 退出功能
            if (quitButton != null) quitButton.onClick.AddListener(OnQuitButtonClicked);
            if (quitYesButton != null) quitYesButton.onClick.AddListener(OnQuitYesClicked);
            if (quitNoButton != null) quitNoButton.onClick.AddListener(OnQuitNoClicked);
            
            // 訂閱遊戲事件
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
            GameEvents.OnRowsCleared += HandleRowsClearedForSalvo;
            GameEvents.OnSkillUnlocked += UpdateSkillUI;
            GameEvents.OnEnemyDamaged += HandleEnemyDamaged;
            GameEvents.OnGridOverflow += HandleGridOverflow;
            GameEvents.OnSkillUsed += HandleSkillUsed;
            
            // 訂閱語言變更事件
            UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
            
            // 保存連發文字原始位置和顏色，並創建相關文字物件
            if (comboText != null)
            {
                comboTextOriginalPosition = comboText.rectTransform.anchoredPosition;
                comboTextOriginalColor = comboText.color;
                comboTextOriginalAlpha = comboText.color.a;
                
                // 創建舊文字物件（用於推動動畫的淡出效果）
                GameObject oldTextObj = Instantiate(comboText.gameObject, comboText.transform.parent);
                oldTextObj.name = "ComboOldText";
                comboOldText = oldTextObj.GetComponent<TextMeshProUGUI>();
                comboOldText.gameObject.SetActive(false);
                
                // 創建「連發!」標籤（只參與滑入動畫，不參與推動動畫）
                GameObject labelObj = Instantiate(comboText.gameObject, comboText.transform.parent);
                labelObj.name = "ComboLabelText";
                comboLabelText = labelObj.GetComponent<TextMeshProUGUI>();
                comboLabelText.text = LocalizationHelper.GetLocalizedString("連發!");
                comboLabelText.gameObject.SetActive(false);
            }
            
            // 保存其他UI文字的原始透明度
            if (salvoText != null) salvoTextOriginalAlpha = salvoText.color.a;
            if (impactBlastText != null) impactBlastTextOriginalAlpha = impactBlastText.color.a;
            if (skillText != null) skillTextOriginalAlpha = skillText.color.a;
            
            // 初始化UI
            ShowMenu();
        }
        
        private void OnDestroy()
        {
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
            GameEvents.OnRowsCleared -= HandleRowsClearedForSalvo;
            GameEvents.OnSkillUnlocked -= UpdateSkillUI;
            GameEvents.OnEnemyDamaged -= HandleEnemyDamaged;
            GameEvents.OnGridOverflow -= HandleGridOverflow;
            GameEvents.OnSkillUsed -= HandleSkillUsed;
            
            // 取消訂閱語言變更事件
            UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocaleChanged -= OnLanguageChanged;
            
            // 解綁按鈕事件 - 失敗面板
            if (gameOverRestartButton != null) gameOverRestartButton.onClick.RemoveListener(OnRestart);
            if (gameOverMenuButton != null) gameOverMenuButton.onClick.RemoveListener(OnReturnToMenu);
            
            // 解綁按鈕事件 - 勝利面板
            if (victoryMenuButton != null) victoryMenuButton.onClick.RemoveListener(OnReturnToMenu);
            
            // 解綁按鈕事件 - 退出功能
            if (quitButton != null) quitButton.onClick.RemoveListener(OnQuitButtonClicked);
            if (quitYesButton != null) quitYesButton.onClick.RemoveListener(OnQuitYesClicked);
            if (quitNoButton != null) quitNoButton.onClick.RemoveListener(OnQuitNoClicked);
        }
        
        private void Update()
        {
            if (GameManager.Instance.CurrentState == GameState.Playing)
            {
                UpdateGameplayUI();
                UpdateSalvoDisplay();
                UpdateImpactBlastDisplay();
                UpdateSkillDisplay();
                UpdateEnemyDamageCounter();
                UpdateComboSlideAnimation();
                UpdateComboPushAnimation();
            }
        }
        
        /// <summary>
        /// 更新連發文字滑入動畫（數字和「連發!」一起滑入）
        /// </summary>
        private void UpdateComboSlideAnimation()
        {
            if (comboText == null) return;
            if (comboSlideTimer <= 0) return;
            
            comboSlideTimer -= Time.deltaTime;
            
            // 計算動畫進度 (0 -> 1)
            float progress = 1f - (comboSlideTimer / comboSlideDuration);
            progress = Mathf.Clamp01(progress);
            
            // 使用 ease-out 效果
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
            
            // 從右邊滑入到原始位置
            float currentOffset = comboSlideOffset * (1f - easedProgress);
            comboText.rectTransform.anchoredPosition = comboTextOriginalPosition + new Vector2(currentOffset, 0);
            
            // 「連發!」標籤也一起滑入
            if (comboLabelText != null && lastComboCount > 0)
            {
                UpdateComboLabelPosition(lastComboCount, currentOffset);
            }
        }
        
        /// <summary>
        /// 根據數字位數更新「連發!」標籤位置
        /// </summary>
        private void UpdateComboLabelPosition(int comboCount, float additionalOffset)
        {
            if (comboLabelText == null || comboText == null) return;
            
            // 根據數字位數計算偏移量（每多一位數增加約 25 像素）
            int digitCount = comboCount.ToString().Length;
            float digitOffset = digitCount * 25f;
            
            Vector2 labelPosition = comboTextOriginalPosition + new Vector2(digitOffset + comboLabelBaseOffset + additionalOffset, 0);
            comboLabelText.rectTransform.anchoredPosition = labelPosition;
        }
        
        /// <summary>
        /// 根據 Combo 數量獲取對應顏色
        /// </summary>
        private Color GetComboColor(int comboCount)
        {
            if (comboCount >= 20)
                return new Color(1f, 0.2f, 0.2f); // 紅色 (20+)
            else if (comboCount >= 10)
                return new Color(1f, 0.5f, 0f); // 橙色 (10-19)
            else if (comboCount >= 5)
                return new Color(1f, 0.9f, 0.2f); // 黃色 (5-9)
            else
                return comboTextOriginalColor; // 原始顏色 (2-4)
        }
        
        /// <summary>
        /// 更新連發數字推動動畫（新數字從下往上推舊數字）
        /// </summary>
        private void UpdateComboPushAnimation()
        {
            if (comboText == null) return;
            if (comboPushTimer <= 0) return;
            
            comboPushTimer -= Time.deltaTime;
            
            // 計算動畫進度 (0 -> 1)
            float progress = 1f - (comboPushTimer / comboPushDuration);
            progress = Mathf.Clamp01(progress);
            
            // 使用 ease-out 效果
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
            
            // 新文字：從下面往上移動到原始位置，同時淡入（使用新 combo 的顏色）
            float newTextOffset = comboPushOffset * (1f - easedProgress);
            comboText.rectTransform.anchoredPosition = comboTextOriginalPosition + new Vector2(0, -newTextOffset);
            Color newColor = GetComboColor(lastComboCount);
            comboText.color = new Color(newColor.r, newColor.g, newColor.b, easedProgress * comboTextOriginalAlpha);
            
            // 舊文字：往上移動，同時淡出（保持舊 combo 的顏色）
            if (comboOldText != null && comboOldText.gameObject.activeSelf)
            {
                float oldTextOffset = comboPushOffset * easedProgress;
                comboOldText.rectTransform.anchoredPosition = comboTextOriginalPosition + new Vector2(0, oldTextOffset);
                Color oldColor = comboOldText.color;
                comboOldText.color = new Color(oldColor.r, oldColor.g, oldColor.b, 1f - easedProgress);
                
                // 動畫結束後隱藏舊文字
                if (progress >= 1f)
                {
                    comboOldText.gameObject.SetActive(false);
                }
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
            SetPanelActive(quitPanel, false); // 隱藏退出確認面板
            
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
                    
                    // 檢查主題是否已解鎖
                    bool isUnlocked = IsThemeUnlocked(theme);
                    
                    // 只有已解鎖的主題才顯示按鈕
                    if (isUnlocked)
                    {
                        var btn = Instantiate(themeButtonPrefab, themeButtonContainer);
                        var btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
                        if (btnText != null) btnText.text = GetLocalizedThemeName(theme);
                        
                        btn.onClick.AddListener(() => OnThemeSelected(index));
                    }
                }
            }
        }
        
        /// <summary>
        /// 檢查主題是否已解鎖
        /// </summary>
        /// <param name="theme">主題資料</param>
        /// <returns>如果主題已解鎖則返回 true</returns>
        private bool IsThemeUnlocked(StageSetSO theme)
        {
            if (theme == null) return false;
            
            // 如果 UnlockKey 為空，表示無需解鎖，直接返回 true
            if (string.IsNullOrEmpty(theme.unlockKey))
            {
                return true;
            }
            
            // 檢查 PlayerPrefs 中是否存在對應的 PassKey
            bool hasPassKey = PlayerPrefs.HasKey(theme.unlockKey);
            return hasPassKey;
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
            SetPanelActive(quitPanel, false); // 隱藏退出確認面板
        }
        
        private void ShowLevelUp()
        {
            SetPanelActive(menuPanel, false);
            SetPanelActive(gameplayPanel, true);
            SetPanelActive(levelUpPanel, true);
            SetPanelActive(gameOverPanel, false);
            SetPanelActive(victoryPanel, false);
            SetPanelActive(quitPanel, false); // 隱藏退出確認面板
        }
        
        private void ShowGameOver()
        {
            SetPanelActive(menuPanel, false);
            SetPanelActive(gameplayPanel, false);
            SetPanelActive(levelUpPanel, false);
            SetPanelActive(gameOverPanel, true);
            SetPanelActive(victoryPanel, false);
            SetPanelActive(quitPanel, false); // 隱藏退出確認面板
            
            if (gameOverFinalScoreText != null && PlayerManager.Instance != null)
                gameOverFinalScoreText.text = $"{LocalizationHelper.GetLocalizedString("最終分數:")} {PlayerManager.Instance.Stats.score:N0}";
        }
        
        private void ShowVictory()
        {
            SetPanelActive(menuPanel, false);
            SetPanelActive(gameplayPanel, false);
            SetPanelActive(levelUpPanel, false);
            SetPanelActive(gameOverPanel, false);
            SetPanelActive(victoryPanel, true);
            SetPanelActive(quitPanel, false); // 隱藏退出確認面板
            
            if (victoryFinalScoreText != null && PlayerManager.Instance != null)
                victoryFinalScoreText.text = $"{LocalizationHelper.GetLocalizedString("最終分數:")} {PlayerManager.Instance.Stats.score:N0}";
            
            // 檢查是否為第一次通關並顯示 FirstPassInfo
            if (GameManager.Instance != null && GameManager.Instance.CurrentTheme != null)
            {
                var currentTheme = GameManager.Instance.CurrentTheme;
                
                // 檢查是否有設定 FirstPassInfo 且為第一次通關
                if (!string.IsNullOrEmpty(currentTheme.firstPassInfo) || 
                    !string.IsNullOrEmpty(currentTheme.firstPassInfoEn) || 
                    !string.IsNullOrEmpty(currentTheme.firstPassInfoJa))
                {
                    // 使用 GameManager 記錄的是否為第一次通關狀態
                    bool isFirstPass = GameManager.Instance.IsFirstPass;
                    
                    if (isFirstPass && firstPassInfoText != null)
                    {
                        // 播放首次通關資訊動畫
                        StartCoroutine(PlayFirstPassInfoAnimationCoroutine());
                    }
                    else
                    {
                        // 如果不是第一次通關，隱藏 FirstPassInfo
                        if (firstPassInfoText != null)
                            firstPassInfoText.gameObject.SetActive(false);
                        if (firstPassInfoTextContainer != null)
                            firstPassInfoTextContainer.gameObject.SetActive(false);
                    }
                }
                else
                {
                    // 如果沒有設定 FirstPassInfo，隱藏相關 UI
                    if (firstPassInfoText != null)
                        firstPassInfoText.gameObject.SetActive(false);
                    if (firstPassInfoTextContainer != null)
                        firstPassInfoTextContainer.gameObject.SetActive(false);
                }
            }
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
        
        // --- 退出功能 ---
        
        /// <summary>
        /// 點擊退出按鈕 - 顯示退出確認面板
        /// </summary>
        private void OnQuitButtonClicked()
        {
            SetPanelActive(quitPanel, true);
        }
        
        /// <summary>
        /// 點擊退出確認（Yes）- 返回主選單
        /// </summary>
        private void OnQuitYesClicked()
        {
            SetPanelActive(quitPanel, false);
            OnReturnToMenu();
        }
        
        /// <summary>
        /// 點擊取消退出（No）- 關閉退出確認面板
        /// </summary>
        private void OnQuitNoClicked()
        {
            SetPanelActive(quitPanel, false);
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
                    if (stats.comboCount > 1)
                    {
                        comboText.gameObject.SetActive(true);
                        if (comboLabelText != null)
                        {
                            comboLabelText.gameObject.SetActive(true);
                            // 每次顯示時都重新獲取本地化文字（確保語言切換時能更新）
                            comboLabelText.text = LocalizationHelper.GetLocalizedString("連發!");
                        }
                        
                        // 只有第二個 Combo（從 1 變成 2）時觸發滑入動畫
                        if (stats.comboCount == 2 && lastComboCount < 2)
                        {
                            comboText.text = $"{stats.comboCount}";
                            comboSlideTimer = comboSlideDuration;
                            // 將數字和標籤都移到右邊
                            comboText.rectTransform.anchoredPosition = comboTextOriginalPosition + new Vector2(comboSlideOffset, 0);
                            Color slideColor = GetComboColor(stats.comboCount);
                            comboText.color = new Color(slideColor.r, slideColor.g, slideColor.b, comboTextOriginalAlpha);
                            if (comboLabelText != null)
                            {
                                Color labelSlideColor = GetComboColor(stats.comboCount);
                                comboLabelText.color = new Color(labelSlideColor.r, labelSlideColor.g, labelSlideColor.b, comboTextOriginalAlpha);
                            }
                            UpdateComboLabelPosition(stats.comboCount, comboSlideOffset);
                        }
                        // combo 增加時觸發推動動畫（3連發以上）
                        else if (stats.comboCount > lastComboCount && lastComboCount >= 2)
                        {
                            // 設置舊數字（準備往上淡出）
                            if (comboOldText != null)
                            {
                                comboOldText.text = $"{lastComboCount}";
                                comboOldText.gameObject.SetActive(true);
                                comboOldText.rectTransform.anchoredPosition = comboTextOriginalPosition;
                                comboOldText.color = GetComboColor(lastComboCount);
                            }
                            
                            // 設置新數字（從下面往上淡入）
                            Color newComboColor = GetComboColor(stats.comboCount);
                            comboText.text = $"{stats.comboCount}";
                            comboText.rectTransform.anchoredPosition = comboTextOriginalPosition + new Vector2(0, -comboPushOffset);
                            comboText.color = new Color(newComboColor.r, newComboColor.g, newComboColor.b, 0f);
                            if (comboLabelText != null)
                            {
                                comboLabelText.color = new Color(newComboColor.r, newComboColor.g, newComboColor.b, comboTextOriginalAlpha);
                            }
                            
                            // 更新標籤位置（根據新數字的位數）
                            UpdateComboLabelPosition(stats.comboCount, 0);
                            
                            comboPushTimer = comboPushDuration;
                        }
                        
                        lastComboCount = stats.comboCount;
                    }
                    else
                    {
                        comboText.gameObject.SetActive(false);
                        if (comboOldText != null) comboOldText.gameObject.SetActive(false);
                        if (comboLabelText != null) comboLabelText.gameObject.SetActive(false);
                        lastComboCount = 0;
                    }
                }
                
                if (playerHpSlider != null) { playerHpSlider.maxValue = stats.maxHp; playerHpSlider.value = stats.currentHp; }
                if (playerHpText != null) playerHpText.text = $"{LocalizationHelper.GetLocalizedString("HP:")} {stats.currentHp} / {stats.maxHp}";
                if (playerCpSlider != null) { playerCpSlider.maxValue = stats.maxCp; playerCpSlider.value = stats.currentCp; }
                if (playerCpText != null) playerCpText.text = $"{LocalizationHelper.GetLocalizedString("CP:")} {stats.currentCp} / {stats.maxCp}";
                if (explosionDamageText != null) explosionDamageText.text = $"{LocalizationHelper.GetLocalizedString("衝擊炮充能 :")} {stats.explosionCharge}/{stats.explosionMaxCharge}";
                
                // 更新溢出CP消耗提示
                if (overflowCostText != null)
                {
                    if (stats.currentCp >= GameConstants.OVERFLOW_CP_COST)
                    {
                        // CP足夠：顯示 溢出代價 : CP -75
                        overflowCostText.text = $"{LocalizationHelper.GetLocalizedString("溢出代價 : CP -")}{GameConstants.OVERFLOW_CP_COST}";
                        overflowCostText.color = Color.white;
                    }
                    else
                    {
                        // CP不足：顯示 溢出代價 : HP ⇒ 1 (紅色)
                        overflowCostText.text = LocalizationHelper.GetLocalizedString("溢出代價 : HP ⇒ 1");
                        overflowCostText.color = Color.red;
                    }
                }
                
                UpdateSkillUI();
            }
            
            if (EnemyController.Instance != null)
            {
                if (enemyHpSlider != null) { enemyHpSlider.maxValue = EnemyController.Instance.MaxHp; enemyHpSlider.value = EnemyController.Instance.CurrentHp; }
                if (enemyHpText != null) enemyHpText.text = $"{Mathf.Ceil(EnemyController.Instance.CurrentHp)} / {EnemyController.Instance.MaxHp}";
            }
            
            if (stageText != null && GameManager.Instance != null)
            {
                stageText.text = $"{LocalizationHelper.GetLocalizedString("STAGE")} {GameManager.Instance.CurrentStageIndex + 1} / {GameManager.Instance.TotalStages}";
            }
        }
        
        private void HandleRowsClearedForSalvo(List<int> clearedRows, List<int> nonGarbageRows, bool hasVoid)
        {
            // 虛無抵銷處理：顯示虛無抵銷文字
            if (hasVoid)
            {
                isVoidNullify = true;
                lastClearedRows = 0; // 虛無抵銷不顯示行數
                salvoDisplayTimer = 2f;
                salvoAnimationTimer = salvoAnimationDuration; // 觸發動畫
                return;
            }
            
            // 正常齊射處理
            isVoidNullify = false;
            if (nonGarbageRows.Count >= 2)
            {
                lastClearedRows = nonGarbageRows.Count;
                salvoDisplayTimer = 2f;
                salvoAnimationTimer = salvoAnimationDuration; // 觸發動畫
                
                // 根據齊射程度觸發螢幕搖晃
                if (VFX.ScreenShake.Instance != null)
                {
                    float intensity;
                    float duration;
                    
                    if (nonGarbageRows.Count >= 4)
                    {
                        // 全彈齊射 - 最劇烈
                        intensity = 0.4f;
                        duration = 0.35f;
                    }
                    else if (nonGarbageRows.Count == 3)
                    {
                        // 三連齊射 - 中等
                        intensity = 0.25f;
                        duration = 0.25f;
                    }
                    else
                    {
                        // 雙管齊射 - 輕微
                        intensity = 0.15f;
                        duration = 0.15f;
                    }
                    
                    VFX.ScreenShake.Instance.Shake(intensity, duration);
                }
            }
        }
        
        private void UpdateSalvoDisplay()
        {
            if (salvoText == null) return;
            if (salvoDisplayTimer > 0)
            {
                salvoDisplayTimer -= Time.deltaTime;
                
                // 設置文字和基礎顏色
                Color baseColor;
                if (isVoidNullify)
                {
                    // 虛無抵銷顯示
                    salvoText.text = LocalizationHelper.GetLocalizedString("虛無抵銷!");
                    baseColor = new Color(0.5f, 0.5f, 0.5f); // 灰色
                }
                else if (lastClearedRows >= 4)
                {
                    salvoText.text = LocalizationHelper.GetLocalizedString("全彈齊射!");
                    baseColor = new Color(1f, 0.84f, 0f); // 金色
                }
                else if (lastClearedRows == 3)
                {
                    salvoText.text = LocalizationHelper.GetLocalizedString("三連齊射!");
                    baseColor = new Color(1f, 0.5f, 0f); // 橙色
                }
                else
                {
                    salvoText.text = LocalizationHelper.GetLocalizedString("雙管齊射!");
                    baseColor = new Color(0.13f, 0.83f, 0.93f); // 青色
                }
                
                // 動畫效果：從 2 倍大縮小到原始大小，同時淡入
                if (salvoAnimationTimer > 0)
                {
                    salvoAnimationTimer -= Time.deltaTime;
                    float progress = 1f - (salvoAnimationTimer / salvoAnimationDuration);
                    progress = Mathf.Clamp01(progress);
                    
                    // 使用 ease-out 效果
                    float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
                    
                    // 縮放：從 2 倍縮小到 1 倍
                    float scale = Mathf.Lerp(2f, 1f, easedProgress);
                    salvoText.transform.localScale = new Vector3(scale, scale, 1f);
                    
                    // 透明度：從 0 變成原始透明度
                    salvoText.color = new Color(baseColor.r, baseColor.g, baseColor.b, easedProgress * salvoTextOriginalAlpha);
                }
                else
                {
                    // 動畫結束，保持正常狀態
                    salvoText.transform.localScale = Vector3.one;
                    salvoText.color = new Color(baseColor.r, baseColor.g, baseColor.b, salvoTextOriginalAlpha);
                }
                
                salvoText.gameObject.SetActive(isVoidNullify || lastClearedRows >= 2);
                if (salvoDisplayTimer <= 0) salvoText.gameObject.SetActive(false);
            }
            else
            {
                salvoText.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 處理網格溢出事件（顯示衝擊爆破文字）
        /// </summary>
        private void HandleGridOverflow()
        {
            if (PlayerManager.Instance == null) return;
            
            // 獲取當前充能值（在溢出處理前的值）
            lastImpactBlastDamage = PlayerManager.Instance.Stats.explosionCharge;
            
            // 只有有傷害時才顯示
            if (lastImpactBlastDamage > 0)
            {
                impactBlastDisplayTimer = 2f;
                impactBlastAnimationTimer = impactBlastAnimationDuration;
            }
        }
        
        /// <summary>
        /// 更新衝擊爆破文字顯示
        /// </summary>
        private void UpdateImpactBlastDisplay()
        {
            if (impactBlastText == null) return;
            if (impactBlastDisplayTimer > 0)
            {
                impactBlastDisplayTimer -= Time.deltaTime;
                
                // 設置文字
                impactBlastText.text = $"{LocalizationHelper.GetLocalizedString("衝擊爆破!")}\n{lastImpactBlastDamage:0}";
                Color baseColor = new Color(1f, 0.3f, 0.1f); // 橙紅色
                
                // 動畫效果：從 2 倍大縮小到原始大小，同時淡入
                if (impactBlastAnimationTimer > 0)
                {
                    impactBlastAnimationTimer -= Time.deltaTime;
                    float progress = 1f - (impactBlastAnimationTimer / impactBlastAnimationDuration);
                    progress = Mathf.Clamp01(progress);
                    
                    // 使用 ease-out 效果
                    float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
                    
                    // 縮放：從 2 倍縮小到 1 倍
                    float scale = Mathf.Lerp(2f, 1f, easedProgress);
                    impactBlastText.transform.localScale = new Vector3(scale, scale, 1f);
                    
                    // 透明度：從 0 變成原始透明度
                    impactBlastText.color = new Color(baseColor.r, baseColor.g, baseColor.b, easedProgress * impactBlastTextOriginalAlpha);
                }
                else
                {
                    // 動畫結束，保持正常狀態
                    impactBlastText.transform.localScale = Vector3.one;
                    impactBlastText.color = new Color(baseColor.r, baseColor.g, baseColor.b, impactBlastTextOriginalAlpha);
                }
                
                impactBlastText.gameObject.SetActive(true);
                if (impactBlastDisplayTimer <= 0) impactBlastText.gameObject.SetActive(false);
            }
            else
            {
                impactBlastText.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 處理技能施放事件
        /// </summary>
        private void HandleSkillUsed(string skillName)
        {
            // 將中文技能名稱轉換為本地化版本
            if (skillName == "湮滅")
                lastSkillName = LocalizationHelper.GetLocalizedString("湮滅");
            else if (skillName == "處決")
                lastSkillName = LocalizationHelper.GetLocalizedString("處決");
            else if (skillName == "修補")
                lastSkillName = LocalizationHelper.GetLocalizedString("修補");
            else
                lastSkillName = skillName; // 如果已經是本地化版本，直接使用
            
            skillDisplayTimer = 1.5f;
            skillAnimationTimer = skillAnimationDuration;
        }
        
        /// <summary>
        /// 更新技能文字顯示
        /// </summary>
        private void UpdateSkillDisplay()
        {
            if (skillText == null) return;
            if (skillDisplayTimer > 0)
            {
                skillDisplayTimer -= Time.deltaTime;
                
                // 設置文字和顏色
                skillText.text = lastSkillName;
                Color baseColor = GetSkillColor(lastSkillName);
                
                // 動畫效果：從 2 倍大縮小到原始大小，同時淡入
                if (skillAnimationTimer > 0)
                {
                    skillAnimationTimer -= Time.deltaTime;
                    float progress = 1f - (skillAnimationTimer / skillAnimationDuration);
                    progress = Mathf.Clamp01(progress);
                    
                    // 使用 ease-out 效果
                    float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
                    
                    // 縮放：從 2 倍縮小到 1 倍
                    float scale = Mathf.Lerp(2f, 1f, easedProgress);
                    skillText.transform.localScale = new Vector3(scale, scale, 1f);
                    
                    // 透明度：從 0 變成原始透明度
                    skillText.color = new Color(baseColor.r, baseColor.g, baseColor.b, easedProgress * skillTextOriginalAlpha);
                }
                else
                {
                    // 動畫結束，保持正常狀態
                    skillText.transform.localScale = Vector3.one;
                    skillText.color = new Color(baseColor.r, baseColor.g, baseColor.b, skillTextOriginalAlpha);
                }
                
                skillText.gameObject.SetActive(true);
                if (skillDisplayTimer <= 0) skillText.gameObject.SetActive(false);
            }
            else
            {
                skillText.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 根據技能名稱獲取顏色
        /// </summary>
        private Color GetSkillColor(string skillName)
        {
            // 使用本地化後的技能名稱進行比對
            string localizedAnnihilation = LocalizationHelper.GetLocalizedString("湮滅");
            string localizedExecution = LocalizationHelper.GetLocalizedString("處決");
            string localizedRepair = LocalizationHelper.GetLocalizedString("修補");
            
            if (skillName == localizedAnnihilation || skillName == "湮滅")
                return new Color(0.8f, 0.2f, 1f); // 紫色
            else if (skillName == localizedExecution || skillName == "處決")
                return new Color(1f, 0.3f, 0.3f); // 紅色
            else if (skillName == localizedRepair || skillName == "修補")
                return new Color(0.3f, 1f, 0.5f); // 綠色
            else
                return Color.white;
        }
        
        private void UpdateSkillUI()
        {
            if (PlayerManager.Instance == null) return;
            
            // 1 -> 湮滅
            bool isAnnihilationUnlocked = PlayerManager.Instance.IsAnnihilationUnlocked();
            if (annihilationSkillObject != null) annihilationSkillObject.SetActive(isAnnihilationUnlocked);
            if (annihilationKeyLabelText != null && isAnnihilationUnlocked) annihilationKeyLabelText.text = "1";
            if (annihilationCostText != null && isAnnihilationUnlocked) annihilationCostText.text = $"{LocalizationHelper.GetLocalizedString("CP:")}{GameConstants.ANNIHILATION_CP_COST}";
            
            // 2 -> 處決
            bool isExecutionUnlocked = PlayerManager.Instance.IsExecutionUnlocked();
            if (executionSkillObject != null) executionSkillObject.SetActive(isExecutionUnlocked);
            if (executionKeyLabelText != null && isExecutionUnlocked) executionKeyLabelText.text = "2";
            if (executionCostText != null && isExecutionUnlocked) executionCostText.text = $"{LocalizationHelper.GetLocalizedString("CP:")}{GameConstants.EXECUTION_CP_COST}";
            
            // 3 -> 修補
            bool isRepairUnlocked = PlayerManager.Instance.IsRepairUnlocked();
            if (repairSkillObject != null) repairSkillObject.SetActive(isRepairUnlocked);
            if (repairKeyLabelText != null && isRepairUnlocked) repairKeyLabelText.text = "3";
            if (repairCostText != null && isRepairUnlocked) repairCostText.text = $"{LocalizationHelper.GetLocalizedString("CP:")}{GameConstants.REPAIR_CP_COST}";
            
            // 如果三個技能都未解鎖，隱藏SkillPanel
            bool hasAnySkillUnlocked = isAnnihilationUnlocked || isExecutionUnlocked || isRepairUnlocked;
            if (skillPanel != null) skillPanel.SetActive(hasAnySkillUnlocked);
        }
        
        /// <summary>
        /// 處理敵人受傷事件
        /// </summary>
        private void HandleEnemyDamaged(float damage, int intensityLevel)
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
        /// 語言變更事件處理
        /// </summary>
        private void OnLanguageChanged(UnityEngine.Localization.Locale locale)
        {
            // 更新連發標籤文字
            if (comboLabelText != null && comboLabelText.gameObject.activeSelf)
            {
                comboLabelText.text = LocalizationHelper.GetLocalizedString("連發!");
            }
            
            // 更新主題按鈕文字
            if (themeButtonContainer != null)
            {
                var themes = GameManager.Instance?.allThemes;
                if (themes != null)
                {
                    int index = 0;
                    foreach (Transform child in themeButtonContainer)
                    {
                        if (index < themes.Count)
                        {
                            var btnText = child.GetComponentInChildren<TextMeshProUGUI>();
                            if (btnText != null)
                            {
                                btnText.text = GetLocalizedThemeName(themes[index]);
                            }
                        }
                        index++;
                    }
                }
            }
        }
        
        /// <summary>
        /// 根據當前語言獲取主題名稱
        /// </summary>
        private string GetLocalizedThemeName(StageSetSO theme)
        {
            if (theme == null) return "";
            
            string languageCode = GetCurrentLanguageCode();
            
            switch (languageCode)
            {
                case "en":
                    return !string.IsNullOrEmpty(theme.themeNameEn) ? theme.themeNameEn : theme.themeName;
                case "ja":
                    return !string.IsNullOrEmpty(theme.themeNameJa) ? theme.themeNameJa : theme.themeName;
                case "zh-TW":
                default:
                    return theme.themeName;
            }
        }
        
        /// <summary>
        /// 獲取當前語言代碼
        /// </summary>
        private string GetCurrentLanguageCode()
        {
            // 優先使用 LocalizationSettings（因為它會在 SetLanguage 時立即更新，不受初始化順序影響）
            try
            {
                var locale = UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale;
                if (locale != null && !string.IsNullOrEmpty(locale.Identifier.Code))
                {
                    return locale.Identifier.Code;
                }
            }
            catch
            {
                // 如果獲取失敗，繼續嘗試 LanguageManager
            }
            
            // 如果 LocalizationSettings 不可用，使用 LanguageManager
            if (Tenronis.Managers.LanguageManager.Instance != null)
            {
                string langCode = Tenronis.Managers.LanguageManager.Instance.CurrentLanguageCode;
                if (!string.IsNullOrEmpty(langCode))
                {
                    return langCode;
                }
            }
            
            return "zh-TW"; // 默認繁體中文
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
        
        /// <summary>
        /// 播放首次通關資訊動畫（協程版本）
        /// 每個字符從透明+大 → 不透明+正常大小，然後整句向左移出
        /// </summary>
        private IEnumerator PlayFirstPassInfoAnimationCoroutine()
        {
            if (firstPassInfoText == null || GameManager.Instance == null || GameManager.Instance.CurrentTheme == null)
                yield break;
            
            var currentTheme = GameManager.Instance.CurrentTheme;
            
            // 清理之前的字符對象
            ClearFirstPassInfoChars();
            
            // 獲取本地化的 FirstPassInfo 文字
            string text = GetLocalizedFirstPassInfo(currentTheme);
            
            if (string.IsNullOrEmpty(text))
            {
                yield break;
            }
            
            // 先設置文字並強制更新，以獲取字符位置信息
            firstPassInfoText.text = text;
            firstPassInfoText.ForceMeshUpdate();
            
            // 獲取容器（如果沒有指定，使用文字本身的 Transform）
            Transform container = firstPassInfoTextContainer != null ? firstPassInfoTextContainer : firstPassInfoText.transform;
            Vector3 originalContainerPosition = container.localPosition;
            
            // 設置容器初始位置（X=0，保持 Y 和 Z 不變）
            Vector3 centerPosition = new Vector3(0f, originalContainerPosition.y, originalContainerPosition.z);
            container.localPosition = centerPosition;
            
            // 獲取容器的 RectTransform（如果有的話）來控制高度
            RectTransform containerRect = container as RectTransform;
            float originalHeight = 1f;
            if (containerRect != null)
            {
                originalHeight = containerRect.sizeDelta.y;
                // 設置初始高度為 0
                containerRect.sizeDelta = new Vector2(containerRect.sizeDelta.x, 0f);
            }
            else
            {
                // 如果不是 RectTransform，使用 scaleY
                Vector3 originalScale = container.localScale;
                container.localScale = new Vector3(originalScale.x, 0f, originalScale.z);
                originalHeight = originalScale.y;
            }
            
            // 顯示容器
            container.gameObject.SetActive(true);
            
            // 獲取字符信息（需要先顯示文字才能獲取正確的字符信息）
            firstPassInfoText.gameObject.SetActive(true);
            TMP_TextInfo textInfo = firstPassInfoText.textInfo;
            int characterCount = textInfo.characterCount;
            
            // 如果字符數量為 0，可能是文字還沒更新，強制更新一次
            if (characterCount == 0)
            {
                firstPassInfoText.ForceMeshUpdate();
                textInfo = firstPassInfoText.textInfo;
                characterCount = textInfo.characterCount;
            }
            
            // 如果還是沒有字符，直接返回
            if (characterCount == 0)
            {
                Debug.LogWarning("[GameUI] FirstPassInfo 文字沒有字符，無法播放動畫");
                yield break;
            }
            
            // 隱藏原始文字（我們將使用單獨的字符對象）
            firstPassInfoText.gameObject.SetActive(false);
            
            // 為每個字符創建單獨的 TextMeshProUGUI 對象
            List<GameObject> charObjects = new List<GameObject>();
            
            for (int i = 0; i < characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                
                // 跳過空格和不可見字符
                if (!charInfo.isVisible || charInfo.character == ' ') continue;
                
                // 創建字符對象
                GameObject charObj = new GameObject($"FirstPassInfoChar_{i}");
                charObj.transform.SetParent(container, false);
                
                // 添加 TextMeshProUGUI 組件
                TextMeshProUGUI charText = charObj.AddComponent<TextMeshProUGUI>();
                charText.text = charInfo.character.ToString();
                
                // 複製字體相關屬性
                charText.font = firstPassInfoText.font;
                charText.fontSize = firstPassInfoText.fontSize;
                charText.fontSizeMin = firstPassInfoText.fontSizeMin;
                charText.fontSizeMax = firstPassInfoText.fontSizeMax;
                charText.fontStyle = firstPassInfoText.fontStyle;
                charText.fontWeight = firstPassInfoText.fontWeight;
                charText.enableAutoSizing = firstPassInfoText.enableAutoSizing;
                
                // 複製顏色相關屬性
                charText.color = firstPassInfoText.color;
                charText.faceColor = firstPassInfoText.faceColor;
                charText.enableVertexGradient = firstPassInfoText.enableVertexGradient;
                if (firstPassInfoText.enableVertexGradient)
                {
                    charText.colorGradient = firstPassInfoText.colorGradient;
                }
                charText.colorGradientPreset = firstPassInfoText.colorGradientPreset;
                
                // 複製對齊方式
                charText.alignment = firstPassInfoText.alignment;
                charText.horizontalAlignment = firstPassInfoText.horizontalAlignment;
                charText.verticalAlignment = firstPassInfoText.verticalAlignment;
                
                // 複製間距
                charText.characterSpacing = firstPassInfoText.characterSpacing;
                charText.wordSpacing = firstPassInfoText.wordSpacing;
                charText.lineSpacing = firstPassInfoText.lineSpacing;
                charText.paragraphSpacing = firstPassInfoText.paragraphSpacing;
                
                // 複製其他屬性
                charText.textWrappingMode = firstPassInfoText.textWrappingMode;
                charText.overflowMode = firstPassInfoText.overflowMode;
                
                // 複製材質
                charText.fontSharedMaterial = firstPassInfoText.fontSharedMaterial;
                charText.fontMaterials = firstPassInfoText.fontMaterials;
                
                // 複製 Outline 效果（如果有的話）
                var originalOutline = firstPassInfoText.GetComponent<UnityEngine.UI.Outline>();
                if (originalOutline != null)
                {
                    var charOutline = charObj.AddComponent<UnityEngine.UI.Outline>();
                    charOutline.effectColor = originalOutline.effectColor;
                    charOutline.effectDistance = originalOutline.effectDistance;
                    charOutline.useGraphicAlpha = originalOutline.useGraphicAlpha;
                }
                
                // 複製 Shadow 效果（如果有的話）
                var originalShadow = firstPassInfoText.GetComponent<UnityEngine.UI.Shadow>();
                if (originalShadow != null)
                {
                    var charShadow = charObj.AddComponent<UnityEngine.UI.Shadow>();
                    charShadow.effectColor = originalShadow.effectColor;
                    charShadow.effectDistance = originalShadow.effectDistance;
                    charShadow.useGraphicAlpha = originalShadow.useGraphicAlpha;
                }
                
                // 複製其他 UI 屬性
                charText.raycastTarget = firstPassInfoText.raycastTarget;
                charText.maskable = firstPassInfoText.maskable;
                
                // 計算字符位置（相對於容器）
                Vector3 charCenterLocal = (charInfo.topLeft + charInfo.topRight + 
                                          charInfo.bottomLeft + charInfo.bottomRight) / 4f;
                
                // 轉換為世界空間
                Vector3 charCenterWorld = firstPassInfoText.transform.TransformPoint(charCenterLocal);
                
                // 轉換為容器的本地空間
                Vector3 charCenterContainerLocal = container.InverseTransformPoint(charCenterWorld);
                // 確保 Y 位置為 0
                charObj.transform.localPosition = new Vector3(charCenterContainerLocal.x, 0f, charCenterContainerLocal.z);
                
                // 初始狀態：透明+大（保留原始顏色的 RGB，只改變 alpha）
                Color originalColor = charText.color;
                charText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
                charObj.transform.localScale = Vector3.one * 2f;
                
                charObjects.Add(charObj);
                firstPassInfoCharObjects.Add(charObj);
            }
            
            // 如果沒有創建任何字符對象，直接返回
            if (charObjects.Count == 0)
            {
                Debug.LogWarning("[GameUI] FirstPassInfo 沒有可顯示的字符，無法播放動畫");
                // 恢復原始文字顯示
                firstPassInfoText.gameObject.SetActive(true);
                yield break;
            }
            
            // 創建動畫序列
            Sequence mainSequence = DOTween.Sequence();
            
            // 第一階段：容器高度從 0 擴展到原始高度（上下擴展）
            float containerExpandDuration = 1f;
            if (containerRect != null)
            {
                // 使用 RectTransform 的 sizeDelta
                mainSequence.Append(containerRect.DOSizeDelta(new Vector2(containerRect.sizeDelta.x, originalHeight), containerExpandDuration));
            }
            else
            {
                // 使用 scaleY
                Vector3 originalScale = container.localScale;
                mainSequence.Append(container.DOScaleY(originalHeight, containerExpandDuration));
            }
            
            // 第二階段：每個字符依次從透明+大 → 不透明+正常大小（在第一階段完成後才開始）
            for (int i = 0; i < charObjects.Count; i++)
            {
                GameObject charObj = charObjects[i];
                TextMeshProUGUI charText = charObj.GetComponent<TextMeshProUGUI>();
                
                float delay = i * 0.1f; // 每個字符延遲 0.1 秒
                float duration = 0.4f; // 動畫持續時間
                
                // 字符動畫（使用 Append 確保在第一階段完成後才開始）
                Sequence charSeq = DOTween.Sequence();
                charSeq.AppendInterval(delay);
                charSeq.Append(charText.DOFade(1f, duration));
                charSeq.Join(charObj.transform.DOScale(Vector3.one, duration));
                
                // 第一個字符動畫使用 Append，後續的字符動畫使用 Join（與第一個字符動畫同時進行）
                if (i == 0)
                {
                    mainSequence.Append(charSeq);
                }
                else
                {
                    mainSequence.Join(charSeq);
                }
            }
            
            // 計算字符動畫總時間
            float totalCharAnimationTime = charObjects.Count * 0.1f + 0.4f;
            
            // 確保序列等待所有字符動畫完成（如果字符動畫還沒完成）
            if (mainSequence.Duration() < containerExpandDuration + totalCharAnimationTime)
            {
                mainSequence.AppendInterval((containerExpandDuration + totalCharAnimationTime) - mainSequence.Duration());
            }
            
            // 第三階段：短暫停留
            mainSequence.AppendInterval(0.5f);
            
            // 第四階段：容器向左移出到 -2000（只改變 X 軸）
            Vector3 exitPosition = new Vector3(-2000f, originalContainerPosition.y, originalContainerPosition.z);
            mainSequence.Append(container.DOLocalMove(exitPosition, 0.8f));
            
            // 等待動畫完成
            yield return mainSequence.WaitForCompletion();
            
            // 動畫完成後清理
            ClearFirstPassInfoChars();
            if (container != null)
            {
                // 恢復原始位置和大小
                container.localPosition = originalContainerPosition;
                if (containerRect != null)
                {
                    containerRect.sizeDelta = new Vector2(containerRect.sizeDelta.x, originalHeight);
                }
                else
                {
                    Vector3 originalScale = container.localScale;
                    container.localScale = new Vector3(originalScale.x, originalHeight, originalScale.z);
                }
                container.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 清理 FirstPassInfo 字符對象
        /// </summary>
        private void ClearFirstPassInfoChars()
        {
            foreach (var charObj in firstPassInfoCharObjects)
            {
                if (charObj != null)
                {
                    Destroy(charObj);
                }
            }
            firstPassInfoCharObjects.Clear();
        }
        
        /// <summary>
        /// 根據當前語言獲取首次通關資訊
        /// </summary>
        private string GetLocalizedFirstPassInfo(StageSetSO theme)
        {
            if (theme == null) return "";
            
            string languageCode = GetCurrentLanguageCode();
            
            switch (languageCode)
            {
                case "en":
                    return !string.IsNullOrEmpty(theme.firstPassInfoEn) ? theme.firstPassInfoEn : theme.firstPassInfo;
                case "ja":
                    return !string.IsNullOrEmpty(theme.firstPassInfoJa) ? theme.firstPassInfoJa : theme.firstPassInfo;
                case "zh-TW":
                default:
                    return theme.firstPassInfo;
            }
        }
    }
}

