using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Tenronis.Data;
using Tenronis.Core;
using Tenronis.Managers;
using Tenronis.ScriptableObjects;
using Tenronis.Utils;
using DG.Tweening;

namespace Tenronis.UI
{
    /// <summary>
    /// Roguelike升級選單
    /// </summary>
    public class RoguelikeMenu : MonoBehaviour
    {
        [Header("Buff選項")]
        [SerializeField] private GameObject selectPanel; // SelectPanel (包含 BuffOptionsContainer)
        [SerializeField] private Transform buffOptionsContainer;
        [SerializeField] private GameObject buffOptionPrefab;
        
        [Header("敵人資訊面板")]
        [SerializeField] private GameObject enemyInfoPanel;
        [SerializeField] private Image enemyIconImage; // 敵人頭像
        [SerializeField] private Button closeEnemyPanelButton; // 關閉敵人面板按鈕
        [SerializeField] private TextMeshProUGUI nextStageEnemyPreviewText;
        [SerializeField] private TextMeshProUGUI previewDescriptionText; // 關卡描述文字
        [SerializeField] private Transform enemyAttackPreviewContainer;
        [SerializeField] private GameObject enemyAttackPreviewPrefab;
        [SerializeField] private Tenronis.Gameplay.Projectiles.Bullet bulletPrefabReference;
        
        [Header("Boss Battle 動畫")]
        [SerializeField] private TextMeshProUGUI bossBattleText; // Boss Battle 文字（場景中新增的）
        [SerializeField] private Transform bossBattleTextContainer; // 包含 Boss Battle 文字的容器（用於整體移動）
        [SerializeField] private AudioClip bossBattleCharSound; // 字符動畫音效
        
        [Header("Bonus Enhance 動畫")]
        [SerializeField] private TextMeshProUGUI bonusEnhanceText; // Bonus Enhance 文字（場景中新增的）
        [SerializeField] private Transform bonusEnhanceTextContainer; // 包含 Bonus Enhance 文字的容器（用於整體移動）
        [SerializeField] private AudioClip bonusEnhanceCharSound; // 字符動畫音效
        
        [Header("Stage 顯示動畫")]
        [SerializeField] private TextMeshProUGUI stageText; // Stage 文字（場景中新增的，顯示 "Stage n 怪物名稱"）
        [SerializeField] private Transform stageTextContainer; // 包含 Stage 文字的容器（用於整體移動）
        [SerializeField] private AudioClip stageCharSound; // Stage 字符動畫音效（可選）
        
        [Header("工程選擇動畫")]
        [SerializeField] private TextMeshProUGUI projectSelectionText; // 工程選擇文字（場景中新增的）
        [SerializeField] private Transform projectSelectionTextContainer; // 包含工程選擇文字的容器（用於整體移動）
        [SerializeField] private AudioClip projectSelectionCharSound; // 字符動畫音效
        
        [Header("玩家強化面板")]
        [SerializeField] private GameObject playerBuffPanel;
        [SerializeField] private TextMeshProUGUI normalStatsText; // 普通強化顯示
        [SerializeField] private TextMeshProUGUI legendaryStatsText; // 傳奇強化顯示
        [SerializeField] private UnityEngine.UI.Toggle detailToggle; // 顯示詳細資訊的 Toggle
        [SerializeField] private Button showPlayerBuffButton; // 顯示玩家狀態面板按鈕
        [SerializeField] private Button hidePlayerBuffButton; // 隱藏玩家狀態面板按鈕
        
        [Header("傳奇強化說明頁面")]
        [SerializeField] private GameObject bonusPanel;
        [SerializeField] private Image bonusIconImage; // 強化圖示
        [SerializeField] private Image bonusAnimationImage; // 從 RawImage 改成 Image
        [SerializeField] private Animator bonusAnimator; // Animator 組件（用於播放動畫）
        [SerializeField] private SpriteRenderer bonusSpriteRenderer; // SpriteRenderer（用於Animator控制，從中讀取Sprite）
        [SerializeField] private UnityEngine.UI.AspectRatioFitter bonusAspectRatioFitter; // AspectRatioFilter 組件
        [SerializeField] private TextMeshProUGUI bonusText;
        [SerializeField] private TextMeshProUGUI bonusLevelText; // 顯示等級變化（例如：戰術擴張 Lv0 -> Lv1）
        [SerializeField] private Button bonusConfirmButton;
        
        [Header("說明動畫資源")]
        [SerializeField] private RuntimeAnimatorController defenseAnimator; // 防禦強化動畫
        [SerializeField] private RuntimeAnimatorController[] volleyAnimators = new RuntimeAnimatorController[5]; // 火力強化1-5動畫
        [SerializeField] private RuntimeAnimatorController annihilationAnimator; // 湮滅動畫
        [SerializeField] private RuntimeAnimatorController executionAnimator; // 處決動畫
        [SerializeField] private RuntimeAnimatorController repairAnimator; // 修補動畫
        
        [Header("提示訊息")]
        [SerializeField] private GameObject tipsPanel; // 提示訊息面板
        [SerializeField] private TextMeshProUGUI tipsText; // 提示訊息文字
        [SerializeField] private Button nextTipButton; // 下一個提示按鈕
        
        private List<GameObject> currentOptions = new List<GameObject>();
        private Dictionary<GameObject, BuffDataSO> optionBuffDataMap = new Dictionary<GameObject, BuffDataSO>();
        private List<GameObject> attackPreviewItems = new List<GameObject>();
        private List<Coroutine> spriteSyncCoroutines = new List<Coroutine>();
        private List<GameObject> bossBattleCharObjects = new List<GameObject>(); // Boss Battle 字符對象列表
        private List<GameObject> bonusEnhanceCharObjects = new List<GameObject>(); // Bonus Enhance 字符對象列表
        private List<GameObject> stageCharObjects = new List<GameObject>(); // Stage 字符對象列表
        private List<GameObject> projectSelectionCharObjects = new List<GameObject>(); // 工程選擇字符對象列表
        private List<Coroutine> bonusHintBlinkCoroutines = new List<Coroutine>(); // 儲存 bonusHintText 的閃爍動畫協程
        private List<(TextMeshProUGUI text, int currentLevel, int maxLevel)> pendingBlinkAnimations = new List<(TextMeshProUGUI, int, int)>(); // 待啟動的閃爍動畫列表
        private bool isFirstGroup = true; // 是否為第一組選項
        private bool showDetailedInfo = false; // 是否顯示詳細資訊
        private int currentInfoTab = 0; // 0=敵人資訊, 1=普通強化, 2=傳奇強化
        private bool waitingForBonusConfirm = false; // 是否正在等待說明頁面確認
        private int currentTipIndex = 0; // 當前提示訊息的索引
        private Coroutine tipDisplayCoroutine = null; // 提示訊息顯示協程
        
        private void OnEnable()
        {
            currentInfoTab = 0; // 0=普通強化, 1=傳奇強化（不再有敵人資訊分頁）
            showDetailedInfo = false; // 默認不顯示詳細資訊
            waitingForBonusConfirm = false;
            isFirstGroup = true; // 重置為第一組
            pendingBlinkAnimations.Clear(); // 清空待啟動的動畫列表
            
            // 確保說明頁面初始為關閉狀態
            if (bonusPanel != null)
                bonusPanel.SetActive(false);
            
            // 更新動畫文字為本地化版本（確保語言切換時能正確顯示）
            UpdateAnimationTexts();
            
            // 訂閱語言變更事件
            UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
            
            GenerateBuffOptions();
            UpdateCurrentStats();
            StartCoroutine(UpdateNextStageEnemyPreviewCoroutine());
            
            // 設置 Toggle
            SetupDetailToggle();
            
            // 設置說明頁面確認按鈕
            SetupBonusConfirmButton();
            
            // 設置關閉敵人面板按鈕
            SetupCloseEnemyPanelButton();
            
            // 設置玩家狀態面板按鈕
            SetupPlayerBuffPanelButtons();
            
            // 設置提示訊息按鈕
            SetupTipButton();
            
            // 初始化提示訊息變量（但不立即顯示，等待關閉敵人面板時才顯示）
            InitializeTips();
            
            // 顯示所有資訊（不再有分頁）
            if (enemyInfoPanel != null)
                enemyInfoPanel.SetActive(true);
            if (playerBuffPanel != null)
                playerBuffPanel.SetActive(false); // 預設隱藏，需通過按鈕顯示
            if (detailToggle != null)
                detailToggle.gameObject.SetActive(true);
            
            // TipsPanel 初始狀態：只有在關閉敵人面板後根據是否有 tips 來決定是否顯示
            // 這裡暫時保持激活狀態（或根據場景設置的初始狀態），實際顯示由 StartTipsDisplay() 控制
        }
        
        /// <summary>
        /// 更新動畫文字為本地化版本
        /// </summary>
        private void UpdateAnimationTexts()
        {
            if (bossBattleText != null)
            {
                bossBattleText.text = LocalizationHelper.GetLocalizedString("遭遇強敵!!!");
            }
            if (bonusEnhanceText != null)
            {
                bonusEnhanceText.text = LocalizationHelper.GetLocalizedString("傳奇工程");
            }
            if (projectSelectionText != null)
            {
                projectSelectionText.text = LocalizationHelper.GetLocalizedString("工程選擇");
            }
        }
        
        /// <summary>
        /// 語言變更事件處理
        /// </summary>
        private void OnLanguageChanged(UnityEngine.Localization.Locale locale)
        {
            // 更新動畫文字
            UpdateAnimationTexts();
            
            // 更新當前顯示的 buff 選項（如果有的話）
            UpdateBuffOptionsLocalization();
        }
        
        /// <summary>
        /// 更新當前顯示的 buff 選項的本地化文本
        /// </summary>
        private void UpdateBuffOptionsLocalization()
        {
            if (currentOptions == null || optionBuffDataMap == null) return;
            
            // 更新每個選項的本地化文本
            foreach (var optionObj in currentOptions)
            {
                if (optionObj == null || !optionObj.activeSelf) continue;
                
                // 從映射中獲取對應的 BuffDataSO
                if (!optionBuffDataMap.TryGetValue(optionObj, out BuffDataSO buffData) || buffData == null)
                {
                    continue;
                }
                
                // 更新標題
                var titleText = optionObj.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
                if (titleText != null)
                {
                    titleText.text = GetLocalizedBuffName(buffData);
                }
                
                // 更新描述
                var descText = optionObj.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
                if (descText != null)
                {
                    bool isLegendary = System.Array.IndexOf(GameConstants.LEGENDARY_BUFFS, buffData.buffType) >= 0;
                    string buffDescription = GetLocalizedBuffDescription(buffData);
                    
                    if (isLegendary)
                    {
                        string legendaryTag = LocalizationHelper.GetLocalizedString("[傳奇強化]");
                        descText.text = $"{legendaryTag}\n{buffDescription}";
                    }
                    else
                    {
                        descText.text = buffDescription;
                    }
                }
            }
        }
        
        private void OnDisable()
        {
            // 取消訂閱語言變更事件
            UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocaleChanged -= OnLanguageChanged;
            
            ClearOptions();
            ClearAttackPreviews();
            ClearBossBattleChars();
            ClearBonusEnhanceChars();
            ClearStageChars();
            ClearProjectSelectionChars();
            
            // 停止所有 DOTween 動畫
            if (bossBattleText != null)
            {
                bossBattleText.DOKill();
            }
            if (bossBattleTextContainer != null)
            {
                bossBattleTextContainer.DOKill();
            }
            if (bonusEnhanceText != null)
            {
                bonusEnhanceText.DOKill();
            }
            if (bonusEnhanceTextContainer != null)
            {
                bonusEnhanceTextContainer.DOKill();
            }
            if (stageText != null)
            {
                stageText.DOKill();
            }
            if (stageTextContainer != null)
            {
                stageTextContainer.DOKill();
            }
            if (projectSelectionText != null)
            {
                projectSelectionText.DOKill();
            }
            if (projectSelectionTextContainer != null)
            {
                projectSelectionTextContainer.DOKill();
            }
            
            // 移除按鈕監聽
            if (bonusConfirmButton != null) bonusConfirmButton.onClick.RemoveAllListeners();
            if (closeEnemyPanelButton != null) closeEnemyPanelButton.onClick.RemoveAllListeners();
            if (showPlayerBuffButton != null) showPlayerBuffButton.onClick.RemoveAllListeners();
            if (hidePlayerBuffButton != null) hidePlayerBuffButton.onClick.RemoveAllListeners();
            if (nextTipButton != null) nextTipButton.onClick.RemoveAllListeners();
            
            // 停止提示訊息顯示協程
            if (tipDisplayCoroutine != null)
            {
                StopCoroutine(tipDisplayCoroutine);
                tipDisplayCoroutine = null;
            }
            
            // 停止Sprite同步協程
            if (spriteSyncCoroutine != null)
            {
                StopCoroutine(spriteSyncCoroutine);
                spriteSyncCoroutine = null;
            }
            
            // 停止Animator
            if (bonusAnimator != null)
            {
                bonusAnimator.enabled = false;
            }
        }
        
        /// <summary>
        /// 設置關閉敵人面板按鈕
        /// </summary>
        private void SetupCloseEnemyPanelButton()
        {
            if (closeEnemyPanelButton != null)
            {
                closeEnemyPanelButton.onClick.RemoveAllListeners();
                closeEnemyPanelButton.onClick.AddListener(OnCloseEnemyPanelClicked);
            }
        }
        
        /// <summary>
        /// 關閉敵人面板按鈕點擊
        /// </summary>
        private void OnCloseEnemyPanelClicked()
        {
            if (enemyInfoPanel != null)
            {
                enemyInfoPanel.SetActive(false);
            }
            
            // 關閉敵人面板後，開始顯示提示訊息
            StartTipsDisplay();
            
            // 檢查是否有待選的 Buff
            if (GameManager.Instance != null && GameManager.Instance.PendingBuffCount > 0)
            {
                // 有獎勵，播放工程選擇動畫，然後顯示 Buff 選擇
                if (projectSelectionText != null)
                {
                    StartCoroutine(PlayProjectSelectionAnimationCoroutine());
                }
            }
            else
            {
                // 沒有獎勵，直接開始遊戲
                if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.LevelUp)
                {
                    GameManager.Instance.ChangeGameState(GameState.Playing);
                }
                
                // 關閉選單
                gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 開始顯示提示訊息（在關閉敵人面板後調用）
        /// </summary>
        private void StartTipsDisplay()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentStage == null)
            {
                // 如果沒有關卡數據，隱藏 TipsPanel
                if (tipsPanel != null)
                {
                    tipsPanel.SetActive(false);
                }
                return;
            }
            
            var currentStage = GameManager.Instance.CurrentStage;
            if (currentStage.tips == null || currentStage.tips.Count == 0)
            {
                // 如果沒有提示訊息，隱藏 TipsPanel 並返回
                if (tipsPanel != null)
                {
                    tipsPanel.SetActive(false);
                }
                return;
            }
            
            // 如果有提示訊息，確保 TipsPanel 是激活的
            if (tipsPanel != null)
            {
                tipsPanel.SetActive(true);
            }
            
            // 重置索引為第一個
            currentTipIndex = 0;
            
            // 使用協程顯示第一個提示訊息（帶打字動畫）
            ShowTipWithTypewriter();
        }
        
        /// <summary>
        /// 設置玩家狀態面板按鈕
        /// </summary>
        private void SetupPlayerBuffPanelButtons()
        {
            if (showPlayerBuffButton != null)
            {
                showPlayerBuffButton.onClick.RemoveAllListeners();
                showPlayerBuffButton.onClick.AddListener(OnShowPlayerBuffPanelClicked);
            }
            
            if (hidePlayerBuffButton != null)
            {
                hidePlayerBuffButton.onClick.RemoveAllListeners();
                hidePlayerBuffButton.onClick.AddListener(OnHidePlayerBuffPanelClicked);
            }
        }
        
        /// <summary>
        /// 顯示玩家狀態面板按鈕點擊
        /// </summary>
        private void OnShowPlayerBuffPanelClicked()
        {
            if (playerBuffPanel != null)
            {
                playerBuffPanel.SetActive(true);
            }
        }
        
        /// <summary>
        /// 隱藏玩家狀態面板按鈕點擊
        /// </summary>
        private void OnHidePlayerBuffPanelClicked()
        {
            if (playerBuffPanel != null)
            {
                playerBuffPanel.SetActive(false);
            }
        }
        
        /// <summary>
        /// 設置提示訊息按鈕
        /// </summary>
        private void SetupTipButton()
        {
            if (nextTipButton != null)
            {
                nextTipButton.onClick.RemoveAllListeners();
                nextTipButton.onClick.AddListener(OnNextTipClicked);
            }
        }
        
        /// <summary>
        /// 初始化提示訊息（僅初始化變量，不顯示內容）
        /// </summary>
        private void InitializeTips()
        {
            currentTipIndex = 0;
            
            // 初始狀態：清空文字，隱藏按鈕
            if (tipsText != null)
            {
                tipsText.text = "";
            }
            if (nextTipButton != null)
            {
                nextTipButton.gameObject.SetActive(false);
            }
            
            // TipsPanel 初始狀態由 StartTipsDisplay() 根據是否有 tips 來決定
        }
        
        /// <summary>
        /// 使用打字動畫顯示提示訊息（協程版本）
        /// 流程：1.隱藏按鈕 2.打字動畫顯示 "Tips\n\n提示內容" 3.顯示按鈕
        /// </summary>
        private void ShowTipWithTypewriter()
        {
            // 停止之前的協程（如果有）
            if (tipDisplayCoroutine != null)
            {
                StopCoroutine(tipDisplayCoroutine);
            }
            
            tipDisplayCoroutine = StartCoroutine(ShowTipWithTypewriterCoroutine());
        }
        
        /// <summary>
        /// 使用打字動畫顯示提示訊息的協程
        /// </summary>
        private IEnumerator ShowTipWithTypewriterCoroutine()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentStage == null)
            {
                yield break;
            }
            
            var currentStage = GameManager.Instance.CurrentStage;
            if (currentStage.tips == null || currentStage.tips.Count == 0)
            {
                yield break;
            }
            
            // 確保索引在有效範圍內
            if (currentTipIndex < 0 || currentTipIndex >= currentStage.tips.Count)
            {
                currentTipIndex = 0;
            }
            
            // 第一階段：隱藏按鈕
            if (nextTipButton != null)
            {
                nextTipButton.gameObject.SetActive(false);
            }
            
            // 第二階段：使用打字動畫顯示 "Tips\n\n提示內容"
            if (tipsText != null)
            {
                string tipContent = currentStage.tips[currentTipIndex];
                string fullText = $"Tips\n\n{tipContent}";
                
                // 使用打字動畫效果
                yield return StartCoroutine(TypewriterEffect(tipsText, fullText, 0.01f));
            }
            
            // 第三階段：顯示按鈕
            if (nextTipButton != null)
            {
                nextTipButton.gameObject.SetActive(true);
            }
            
            tipDisplayCoroutine = null;
        }
        
        /// <summary>
        /// 下一個提示按鈕點擊
        /// </summary>
        private void OnNextTipClicked()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentStage == null)
            {
                return;
            }
            
            var currentStage = GameManager.Instance.CurrentStage;
            if (currentStage.tips == null || currentStage.tips.Count == 0)
            {
                return;
            }
            
            // 移到下一個索引
            currentTipIndex++;
            
            // 如果已經超出範圍，回到第一個
            if (currentTipIndex >= currentStage.tips.Count)
            {
                currentTipIndex = 0;
            }
            
            // 重新執行打字動畫流程
            ShowTipWithTypewriter();
        }
        
        /// <summary>
        /// 設置說明頁面確認按鈕
        /// </summary>
        private void SetupBonusConfirmButton()
        {
            if (bonusConfirmButton != null)
            {
                bonusConfirmButton.onClick.RemoveAllListeners();
                bonusConfirmButton.onClick.AddListener(OnBonusConfirmClicked);
            }
        }
        
        /// <summary>
        /// 設置詳細資訊 Toggle
        /// </summary>
        private void SetupDetailToggle()
        {
            if (detailToggle != null)
            {
                detailToggle.isOn = showDetailedInfo;
                detailToggle.onValueChanged.RemoveAllListeners();
                detailToggle.onValueChanged.AddListener(OnDetailToggleChanged);
            }
        }
        
        /// <summary>
        /// 處理詳細資訊 Toggle 變更
        /// </summary>
        private void OnDetailToggleChanged(bool isOn)
        {
            showDetailedInfo = isOn;
            UpdateCurrentStats(); // 重新更新顯示內容
        }
        
        /// <summary>
        /// 清除攻擊預覽項目
        /// </summary>
        private void ClearAttackPreviews()
        {
            // 停止所有 Sprite 同步協程
            foreach (var coroutine in spriteSyncCoroutines)
            {
                if (coroutine != null) StopCoroutine(coroutine);
            }
            spriteSyncCoroutines.Clear();
            
            // 銷毀所有預覽項目
            foreach (var item in attackPreviewItems)
            {
                if (item != null) Destroy(item);
            }
            attackPreviewItems.Clear();
        }
        
        /// <summary>
        /// 生成Buff選項
        /// </summary>
        private void GenerateBuffOptions()
        {
            ClearOptions();
            
            // 如果不是第一組，重置標記
            bool isFirst = isFirstGroup;
            if (isFirstGroup)
            {
                isFirstGroup = false; // 標記為已處理第一組
            }
            
            if (GameManager.Instance == null) return;
            
            // 普通強化固定為三選一（不再有傳奇強化選擇階段）
            int optionCount = 3;
            BuffDataSO[] options = GameManager.Instance.GetRandomBuffOptions(optionCount, false);
            
            foreach (var buffData in options)
            {
                if (buffData == null) continue;
                
                GameObject optionObj = Instantiate(buffOptionPrefab, buffOptionsContainer);
                currentOptions.Add(optionObj);
                
                // 保存選項與 BuffDataSO 的映射關係
                optionBuffDataMap[optionObj] = buffData;
                
                // 設置UI（傳入是否為第一組的標記）
                SetupBuffOption(optionObj, buffData, isFirst);
            }
        }
        
        /// <summary>
        /// 設置Buff選項UI
        /// </summary>
        private void SetupBuffOption(GameObject optionObj, BuffDataSO buffData, bool isFirstGroup = false)
        {
            // 檢查是否為傳奇強化
            bool isLegendary = System.Array.IndexOf(GameConstants.LEGENDARY_BUFFS, buffData.buffType) >= 0;
            
            // 標題
            var titleText = optionObj.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
            if (titleText != null)
            {
                titleText.text = GetLocalizedBuffName(buffData);
            }
            
            // 描述
            var descText = optionObj.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
            if (descText != null)
            {
                string buffDescription = GetLocalizedBuffDescription(buffData);
                if (isLegendary)
                {
                    string legendaryTag = LocalizationHelper.GetLocalizedString("[傳奇強化]");
                    descText.text = $"{legendaryTag}\n{buffDescription}";
                }
                else
                {
                    descText.text = buffDescription;
                }
            }
            
            // Bonus 提示文字（顯示進度條，帶閃爍動畫）
            var bonusHintText = optionObj.transform.Find("BonusHintText")?.GetComponent<TextMeshProUGUI>();
            if (bonusHintText != null)
            {
                
                if (isLegendary)
                {
                    // 傳奇強化不顯示提示
                    bonusHintText.text = "";
                    bonusHintText.gameObject.SetActive(false);
                }
                else
                {
                    // 獲取當前等級和最大等級
                    var (currentLevel, maxLevel) = GetBuffLevelInfo(buffData.buffType);
                    if (maxLevel > 0 && currentLevel < maxLevel)
                    {
                        // 初始顯示當前狀態
                        string currentState = GetProgressBarOnly(currentLevel, maxLevel);
                        bonusHintText.text = currentState;
                        bonusHintText.gameObject.SetActive(true);
                        
                        // 如果是第一組，延遲到工程選擇動畫結束後才啟動閃爍動畫
                        if (isFirstGroup)
                        {
                            // 將動畫加入待啟動列表
                            pendingBlinkAnimations.Add((bonusHintText, currentLevel, maxLevel));
                        }
                        else
                        {
                            // 立即啟動閃爍動畫
                            StartBonusHintBlinkAnimation(bonusHintText, currentLevel, maxLevel);
                        }
                    }
                    else if (currentLevel >= maxLevel)
                    {
                        // 已滿級，顯示最終狀態
                        string progressBar = GetProgressBarOnly(maxLevel, maxLevel);
                        bonusHintText.text = progressBar;
                        bonusHintText.gameObject.SetActive(true);
                    }
                    else
                    {
                        bonusHintText.text = "";
                        bonusHintText.gameObject.SetActive(false);
                    }
                }
            }
            
            // 圖示
            var iconImage = optionObj.transform.Find("Icon")?.GetComponent<Image>();
            if (iconImage != null && buffData.icon != null)
            {
                iconImage.sprite = buffData.icon;
            }
            
            // 背板顏色
            var backgroundImage = optionObj.GetComponent<Image>();
            if (backgroundImage != null)
            {
                backgroundImage.color = buffData.iconColor;
            }
            
            // 按鈕
            var button = optionObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnSelectBuff(buffData.buffType));
            }
        }
        

        
        /// <summary>
        /// 獲取Buff的等級資訊（當前等級和最大等級）
        /// </summary>
        private (int currentLevel, int maxLevel) GetBuffLevelInfo(BuffType buffType)
        {
            if (PlayerManager.Instance == null) return (0, 0);
            
            var stats = PlayerManager.Instance.Stats;
            if (stats == null) return (0, 0);
            
            int currentLevel = 0;
            int maxLevel = 0;
            
            switch (buffType)
            {
                case BuffType.Salvo:
                    currentLevel = stats.salvoLevel;
                    maxLevel = GameConstants.SALVO_MAX_LEVEL;
                    break;
                case BuffType.Burst:
                    currentLevel = stats.burstLevel;
                    maxLevel = GameConstants.BURST_MAX_LEVEL;
                    break;
                case BuffType.Counter:
                    currentLevel = stats.counterFireLevel;
                    maxLevel = GameConstants.COUNTER_MAX_LEVEL;
                    break;
                case BuffType.Explosion:
                    currentLevel = stats.explosionChargeLevel;
                    maxLevel = GameConstants.EXPLOSION_BUFF_MAX_LEVEL;
                    break;
                case BuffType.SpaceExpansion:
                    currentLevel = stats.spaceExpansionLevel;
                    maxLevel = GameConstants.SPACE_EXPANSION_MAX_LEVEL;
                    break;
                case BuffType.ResourceExpansion:
                    currentLevel = stats.cpExpansionLevel;
                    maxLevel = GameConstants.RESOURCE_EXPANSION_MAX_LEVEL;
                    break;
                default:
                    return (0, 0); // 傳奇強化或其他類型不顯示
            }
            
            return (currentLevel, maxLevel);
        }
        
        /// <summary>
        /// 生成僅進度條符號（不包含等級標記）
        /// </summary>
        private string GetProgressBarOnly(int current, int max)
        {
            System.Text.StringBuilder progress = new System.Text.StringBuilder();
            
            for (int i = 0; i < max; i++)
            {
                bool isCompleted = i < current;
                bool isLast = (i == max - 1);
                
                if (isCompleted)
                {
                    if (isLast)
                    {
                        // 最後一個已完成：★（大一點）
                        progress.Append("<color=yellow><size=120%>★</size></color> ");
                    }
                    else
                    {
                        // 已完成：●（正常大小）
                        progress.Append("<color=yellow>●</color> ");
                    }
                }
                else
                {
                    if (isLast)
                    {
                        // 最後一個未完成：☆（大一點）
                        progress.Append("<color=grey><size=120%>☆</size></color> ");
                    }
                    else
                    {
                        // 未完成：○（小一點）
                        progress.Append("<color=grey><size=80%>○</size></color> ");
                    }
                }
            }
            
            return progress.ToString().TrimEnd(); // 移除最後的空格
        }
        
        /// <summary>
        /// 生成顯示強化後等級的進度條（用於閃爍動畫）
        /// </summary>
        private string GetProgressBarWithBlink(int current, int max)
        {
            // 顯示強化後的等級（current + 1）
            int nextLevel = current + 1;
            System.Text.StringBuilder progress = new System.Text.StringBuilder();
            
            for (int i = 0; i < max; i++)
            {
                bool willBeCompleted = i < nextLevel;
                bool isLast = (i == max - 1);
                bool isBlinkPosition = (i == current); // 需要閃爍的位置
                
                if (willBeCompleted)
                {
                    if (isLast)
                    {
                        // 最後一個已完成：★（大一點）
                        progress.Append("<color=yellow><size=120%>★</size></color> ");
                    }
                    else
                    {
                        // 已完成：●（正常大小）
                        progress.Append("<color=yellow>●</color> ");
                    }
                }
                else
                {
                    if (isLast)
                    {
                        // 最後一個未完成：☆（大一點）
                        progress.Append("<color=grey><size=120%>☆</size></color> ");
                    }
                    else
                    {
                        // 未完成：○（小一點）
                        progress.Append("<color=grey><size=80%>○</size></color> ");
                    }
                }
            }
            
            return progress.ToString().TrimEnd();
        }
        
        /// <summary>
        /// 啟動 BonusHint 的閃爍動畫
        /// </summary>
        private void StartBonusHintBlinkAnimation(TextMeshProUGUI bonusHintText, int currentLevel, int maxLevel)
        {
            if (bonusHintText == null) return;
            
            // 生成兩個版本的進度條（當前和下一個狀態）
            string currentState = GetProgressBarOnly(currentLevel, maxLevel);
            string nextState = GetProgressBarWithBlink(currentLevel, maxLevel);
            
            // 使用協程實現閃爍動畫
            Coroutine blinkCoroutine = StartCoroutine(BonusHintBlinkCoroutine(bonusHintText, currentState, nextState));
            bonusHintBlinkCoroutines.Add(blinkCoroutine);
        }
        
        /// <summary>
        /// BonusHint 閃爍動畫協程
        /// </summary>
        private IEnumerator BonusHintBlinkCoroutine(TextMeshProUGUI bonusHintText, string currentState, string nextState)
        {
            if (bonusHintText == null) yield break;
            
            bool showNext = false;
            float blinkInterval = 0.5f; // 閃爍間隔（秒）
            
            while (bonusHintText != null && bonusHintText.gameObject.activeInHierarchy)
            {
                // 交替顯示當前狀態和下一個狀態
                bonusHintText.text = showNext ? nextState : currentState;
                showNext = !showNext;
                
                yield return new WaitForSeconds(blinkInterval);
            }
        }
        
        /// <summary>
        /// 清除選項
        /// </summary>
        private void ClearOptions()
        {
            // 停止所有 BonusHint 閃爍動畫協程
            foreach (var coroutine in bonusHintBlinkCoroutines)
            {
                if (coroutine != null) StopCoroutine(coroutine);
            }
            bonusHintBlinkCoroutines.Clear();
            
            foreach (var option in currentOptions)
            {
                if (option != null)
                    Destroy(option);
            }
            currentOptions.Clear();
            optionBuffDataMap.Clear();
        }
        
        /// <summary>
        /// 選擇Buff
        /// </summary>
        private void OnSelectBuff(BuffType buffType)
        {
            // 檢查選擇的Buff是否為普通強化
            bool isNormalBuff = System.Array.IndexOf(GameConstants.NORMAL_BUFFS, buffType) >= 0;
            
            // 記錄選擇前的狀態（用於檢測技能解鎖）
            bool wasAnnihilationUnlocked = PlayerManager.Instance != null && PlayerManager.Instance.IsAnnihilationUnlocked();
            bool wasExecutionUnlocked = PlayerManager.Instance != null && PlayerManager.Instance.IsExecutionUnlocked();
            bool wasRepairUnlocked = PlayerManager.Instance != null && PlayerManager.Instance.IsRepairUnlocked();
            int tacticalExpansionLevelBeforeSkill = PlayerManager.Instance != null ? PlayerManager.Instance.Stats.tacticalExpansionLevel : 0;
            
            // 觸發Buff選擇事件（這會應用Buff效果）
            GameEvents.TriggerBuffSelected(buffType);
            
            // 檢查選擇後的狀態（是否有新的普通強化達到滿級）
            bool isSelectedBuffMaxed = false;
            if (isNormalBuff && PlayerManager.Instance != null)
            {
                isSelectedBuffMaxed = PlayerManager.Instance.IsBuffMaxed(buffType);
            }
            
            // 檢查是否解鎖了新技能
            bool unlockedAnnihilation = PlayerManager.Instance != null && !wasAnnihilationUnlocked && PlayerManager.Instance.IsAnnihilationUnlocked();
            bool unlockedExecution = PlayerManager.Instance != null && !wasExecutionUnlocked && PlayerManager.Instance.IsExecutionUnlocked();
            bool unlockedRepair = PlayerManager.Instance != null && !wasRepairUnlocked && PlayerManager.Instance.IsRepairUnlocked();
            
            // 如果選擇的普通強化使其達到滿級，則自動給予傳奇強化獎勵
            if (isSelectedBuffMaxed)
            {
                Debug.Log("[RoguelikeMenu] 普通強化已達滿級，自動給予傳奇強化獎勵！");
                
                // 自動選擇一個傳奇強化作為獎勵
                var legendaryBuff = GameManager.Instance.GetRandomLegendaryBuffReward();
                if (legendaryBuff != null)
                {
                    // 記錄選擇前的傳奇強化等級和技能解鎖狀態
                    int volleyLevelBefore = PlayerManager.Instance != null ? PlayerManager.Instance.Stats.missileExtraCount : 0;
                    int defenseLevelBefore = PlayerManager.Instance != null ? PlayerManager.Instance.Stats.blockDefenseLevel : 0;
                    int tacticalExpansionLevelBefore = PlayerManager.Instance != null ? PlayerManager.Instance.Stats.tacticalExpansionLevel : 0;
                    bool wasAnnihilationUnlockedAfter = PlayerManager.Instance != null && PlayerManager.Instance.IsAnnihilationUnlocked();
                    bool wasExecutionUnlockedAfter = PlayerManager.Instance != null && PlayerManager.Instance.IsExecutionUnlocked();
                    bool wasRepairUnlockedAfter = PlayerManager.Instance != null && PlayerManager.Instance.IsRepairUnlocked();
                
                    // 應用傳奇強化
                GameManager.Instance.AddPendingBuffs(1);
                    GameEvents.TriggerBuffSelected(legendaryBuff.buffType);
                    
                    // 重新檢查技能解鎖狀態（因為 TacticalExpansion 可能會解鎖技能）
                    bool unlockedAnnihilationAfter = PlayerManager.Instance != null && !wasAnnihilationUnlockedAfter && PlayerManager.Instance.IsAnnihilationUnlocked();
                    bool unlockedExecutionAfter = PlayerManager.Instance != null && !wasExecutionUnlockedAfter && PlayerManager.Instance.IsExecutionUnlocked();
                    bool unlockedRepairAfter = PlayerManager.Instance != null && !wasRepairUnlockedAfter && PlayerManager.Instance.IsRepairUnlocked();
                    
                    // 檢查是否升級了 Volley、Defense 或 TacticalExpansion
                    bool volleyUpgraded = legendaryBuff.buffType == BuffType.Volley && 
                                         PlayerManager.Instance != null && 
                                         PlayerManager.Instance.Stats.missileExtraCount > volleyLevelBefore;
                    bool defenseUpgraded = legendaryBuff.buffType == BuffType.Defense && 
                                          PlayerManager.Instance != null && 
                                          PlayerManager.Instance.Stats.blockDefenseLevel > defenseLevelBefore;
                    bool tacticalExpansionUpgraded = legendaryBuff.buffType == BuffType.TacticalExpansion && 
                                                     PlayerManager.Instance != null && 
                                                     PlayerManager.Instance.Stats.tacticalExpansionLevel > tacticalExpansionLevelBefore;
                    
                    // 如果升級了 Volley、Defense、TacticalExpansion 或解鎖了技能，顯示說明頁面
                    if (volleyUpgraded || defenseUpgraded || tacticalExpansionUpgraded || unlockedAnnihilationAfter || unlockedExecutionAfter || unlockedRepairAfter)
                    {
                        // 先準備 Bonus Panel 的內容（但不顯示）
                        PrepareBonusPanel(legendaryBuff, volleyLevelBefore, defenseLevelBefore, tacticalExpansionLevelBefore, 
                                         volleyUpgraded, defenseUpgraded, tacticalExpansionUpgraded, 
                                         unlockedAnnihilationAfter, unlockedExecutionAfter, unlockedRepairAfter);
                        
                        // 播放 Bonus Enhance 動畫，動畫結束後顯示 Bonus Panel 內容
                        StartCoroutine(PlayBonusEnhanceAnimationAndShowPanel());
                        waitingForBonusConfirm = true;
                        return; // 等待確認按鈕，不繼續下一步
                    }
                }
                else
                {
                    Debug.LogWarning("[RoguelikeMenu] 無法獲得傳奇強化獎勵（所有傳奇強化已達滿級或無可用選項）");
                }
            }
            // 如果只是解鎖了技能（沒有獲得傳奇強化），也顯示說明頁面
            else if (unlockedAnnihilation || unlockedExecution || unlockedRepair)
            {
                // 檢查 TacticalExpansion 是否升級了
                int tacticalExpansionLevelAfter = PlayerManager.Instance != null ? PlayerManager.Instance.Stats.tacticalExpansionLevel : 0;
                bool tacticalExpansionUpgradedSkill = tacticalExpansionLevelAfter > tacticalExpansionLevelBeforeSkill;
                
                // 先準備 Bonus Panel 的內容（但不顯示）
                PrepareBonusPanel(null, 0, 0, tacticalExpansionLevelBeforeSkill, false, false, tacticalExpansionUpgradedSkill, unlockedAnnihilation, unlockedExecution, unlockedRepair);
                
                // 播放 Bonus Enhance 動畫，動畫結束後顯示 Bonus Panel 內容
                StartCoroutine(PlayBonusEnhanceAnimationAndShowPanel());
                waitingForBonusConfirm = true;
                return; // 等待確認按鈕，不繼續下一步
            }
            
            // 如果沒有需要顯示說明頁面的情況，繼續正常流程
            ContinueAfterBuffSelection();
        }
        
        // 保存 Bonus Panel 的內容，用於動畫結束後顯示
        private string savedDescriptionText = "";
        private string savedLevelChangeText = "";
        private RuntimeAnimatorController savedAnimatorController = null;
        private Coroutine spriteSyncCoroutine = null; // 用於同步Sprite的協程
        
        /// <summary>
        /// 準備傳奇強化說明頁面內容（但不顯示）
        /// </summary>
        private void PrepareBonusPanel(BuffDataSO legendaryBuff, int volleyLevelBefore, int defenseLevelBefore, int tacticalExpansionLevelBefore,
                                      bool volleyUpgraded, bool defenseUpgraded, bool tacticalExpansionUpgraded,
                                      bool unlockedAnnihilation, bool unlockedExecution, bool unlockedRepair)
        {
            if (bonusPanel == null)
            {
                Debug.LogError("[RoguelikeMenu] BonusPanel 未設置！");
                return;
            }
            
            // 關閉選擇界面
            if (selectPanel != null)
                selectPanel.SetActive(false);
            
            // 設置文字和動畫
            string descriptionText = "";
            RuntimeAnimatorController animatorController = null;
            
            if (volleyUpgraded && PlayerManager.Instance != null)
            {
                int volleyLevel = PlayerManager.Instance.Stats.missileExtraCount;
                descriptionText = LocalizationHelper.GetLocalizedStringFormat("現在消除方塊，會生成 {0} 發飛彈", 1 + volleyLevel);
                
                // 根據等級選擇對應的動畫（等級 1-5 對應索引 0-4）
                int animIndex = Mathf.Clamp(volleyLevel - 1, 0, volleyAnimators.Length - 1);
                if (animIndex >= 0 && animIndex < volleyAnimators.Length && volleyAnimators[animIndex] != null)
                {
                    animatorController = volleyAnimators[animIndex];
                }
            }
            else if (defenseUpgraded && PlayerManager.Instance != null)
            {
                int defenseLevel = PlayerManager.Instance.Stats.blockDefenseLevel;
                descriptionText = LocalizationHelper.GetLocalizedStringFormat("方塊現在可以承受 {0} 發飛彈", 1 + defenseLevel);
                
                if (defenseAnimator != null)
                {
                    animatorController = defenseAnimator;
                }
            }
            else if (unlockedAnnihilation)
            {
                descriptionText = LocalizationHelper.GetLocalizedString("按 1 將當前方塊轉化為可穿透已存在方塊的湮滅方塊。按 空白鍵 將摧毀所有與湮滅方塊重疊的方塊，並生成飛彈。");
                
                if (annihilationAnimator != null)
                {
                    animatorController = annihilationAnimator;
                }
            }
            else if (unlockedExecution)
            {
                descriptionText = LocalizationHelper.GetLocalizedString("按 2 消除每一行最上方的方塊，並生成飛彈。");
                
                if (executionAnimator != null)
                {
                    animatorController = executionAnimator;
                }
            }
            else if (unlockedRepair)
            {
                descriptionText = LocalizationHelper.GetLocalizedString("按 3 將所有封閉區域填充為方塊。若形成整列，則視為有效消除。消除虛無方塊時不會產生任何飛彈。");
                
                if (repairAnimator != null)
                {
                    animatorController = repairAnimator;
                }
            }
            
            // 保存內容供後續動畫使用
            savedDescriptionText = descriptionText;
            savedAnimatorController = animatorController;
            
            // 獲取並設置 Icon
            Sprite iconSprite = null;
            if (legendaryBuff != null)
            {
                iconSprite = legendaryBuff.icon;
            }
            else
            {
                // 如果 legendaryBuff 為 null，根據解鎖的技能或升級的強化來獲取對應的 BuffDataSO
                BuffType? targetBuffType = null;
                if (unlockedAnnihilation || unlockedExecution || unlockedRepair || tacticalExpansionUpgraded)
                {
                    targetBuffType = BuffType.TacticalExpansion;
                }
                else if (defenseUpgraded)
                {
                    targetBuffType = BuffType.Defense;
                }
                else if (volleyUpgraded)
                {
                    targetBuffType = BuffType.Volley;
                }
                
                if (targetBuffType.HasValue && GameManager.Instance != null)
                {
                    // 從傳奇強化列表中查找
                    var legendaryBuffs = GameManager.Instance.LegendaryBuffs;
                    if (legendaryBuffs != null)
                    {
                        foreach (var buff in legendaryBuffs)
                        {
                            if (buff != null && buff.buffType == targetBuffType.Value)
                            {
                                iconSprite = buff.icon;
                                break;
                            }
                        }
                    }
                    
                    // 如果沒找到，從普通強化列表中查找
                    if (iconSprite == null)
                    {
                        var normalBuffs = GameManager.Instance.NormalBuffs;
                        if (normalBuffs != null)
                        {
                            foreach (var buff in normalBuffs)
                            {
                                if (buff != null && buff.buffType == targetBuffType.Value)
                                {
                                    iconSprite = buff.icon;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            
            // 設置 IconImage（初始狀態：透明）
            if (bonusIconImage != null)
            {
                if (iconSprite != null)
                {
                    bonusIconImage.sprite = iconSprite;
                    Color iconColor = bonusIconImage.color;
                    iconColor.a = 0f;
                    bonusIconImage.color = iconColor;
                    bonusIconImage.gameObject.SetActive(true);
                }
                else
                {
                    bonusIconImage.gameObject.SetActive(false);
                }
            }
            
            // 設置等級變化文字
            string levelChangeText = "";
            
            if (tacticalExpansionUpgraded && PlayerManager.Instance != null)
            {
                int tacticalExpansionLevel = PlayerManager.Instance.Stats.tacticalExpansionLevel;
                levelChangeText = $"{LocalizationHelper.GetLocalizedString("戰術擴張")}\n {LocalizationHelper.GetLocalizedStringFormat("Lv{0} -> Lv{1}", tacticalExpansionLevelBefore, tacticalExpansionLevel)}";
            }
            else if (unlockedAnnihilation || unlockedExecution || unlockedRepair)
            {
                // 如果解鎖了技能，顯示 TacticalExpansion 的等級變化
                if (PlayerManager.Instance != null)
                {
                    int tacticalExpansionLevel = PlayerManager.Instance.Stats.tacticalExpansionLevel;
                    levelChangeText = $"{LocalizationHelper.GetLocalizedString("戰術擴張")}\n {LocalizationHelper.GetLocalizedStringFormat("Lv{0} -> Lv{1}", tacticalExpansionLevelBefore, tacticalExpansionLevel)}";
                }
            }
            else if (defenseUpgraded && PlayerManager.Instance != null)
            {
                int defenseLevel = PlayerManager.Instance.Stats.blockDefenseLevel;
                levelChangeText = $"{LocalizationHelper.GetLocalizedString("鞏固防禦")}\n {LocalizationHelper.GetLocalizedStringFormat("Lv{0} -> Lv{1}", defenseLevelBefore, defenseLevel)}";
            }
            else if (volleyUpgraded && PlayerManager.Instance != null)
            {
                int volleyLevel = PlayerManager.Instance.Stats.missileExtraCount;
                levelChangeText = $"{LocalizationHelper.GetLocalizedString("加倍火力")}\n {LocalizationHelper.GetLocalizedStringFormat("Lv{0} -> Lv{1}", volleyLevelBefore, volleyLevel)}";
            }
            
            savedLevelChangeText = levelChangeText;
            
            // 設置動畫（但不顯示）
            if (bonusAnimator != null && bonusAnimationImage != null && bonusSpriteRenderer != null)
            {
                if (animatorController != null)
                {
                    // 停止之前的同步協程
                    if (spriteSyncCoroutine != null)
                    {
                        StopCoroutine(spriteSyncCoroutine);
                        spriteSyncCoroutine = null;
                    }
                    
                    // 設置 Animator
                    bonusAnimator.runtimeAnimatorController = animatorController;
                    
                    // 確保SpriteRenderer在開始時是啟用的
                    if (!bonusSpriteRenderer.gameObject.activeSelf)
                    {
                        bonusSpriteRenderer.gameObject.SetActive(true);
                    }
                    
                    // 重置Animator狀態
                    bonusAnimator.enabled = true;
                    bonusAnimator.Rebind();
                    
                    // 設置初始狀態：透明
                    Color imageColor = bonusAnimationImage.color;
                    imageColor.a = 0f;
                    bonusAnimationImage.color = imageColor;
                    bonusAnimationImage.gameObject.SetActive(true);
                    
                    // 開始同步Sprite的協程
                    spriteSyncCoroutine = StartCoroutine(SyncSpriteFromRendererToImage());
                }
                else
                {
                    // 如果沒有動畫，隱藏Image
                    bonusAnimationImage.gameObject.SetActive(false);
                    
                    // 停止Animator
                    if (bonusAnimator != null)
                    {
                        bonusAnimator.enabled = false;
                    }
                    
                    // 重置 AspectRatioFitter（可選）
                    if (bonusAspectRatioFitter != null)
                    {
                        bonusAspectRatioFitter.aspectRatio = 1f; // 重置為 1:1
                    }
                }
            }
            
            // 設置文字初始狀態：不顯示
            if (bonusText != null)
            {
                bonusText.text = "";
                bonusText.gameObject.SetActive(false);
            }
            
            // 設置等級變化文字初始狀態：不顯示
            if (bonusLevelText != null)
            {
                bonusLevelText.text = "";
                bonusLevelText.gameObject.SetActive(false);
            }
            
            // 隱藏確認按鈕
            if (bonusConfirmButton != null)
            {
                bonusConfirmButton.gameObject.SetActive(false);
            }
            
            // 設置 bonusPanel 的 Image 為透明
            Image bonusPanelImage = bonusPanel != null ? bonusPanel.GetComponent<Image>() : null;
            if (bonusPanelImage != null)
            {
                Color panelColor = bonusPanelImage.color;
                panelColor.a = 0f;
                bonusPanelImage.color = panelColor;
            }
            
            // 顯示說明頁面（但內容還未顯示）
            bonusPanel.SetActive(true);
        }
        
        /// <summary>
        /// 播放 Bonus Enhance 動畫並顯示 Bonus Panel 內容
        /// </summary>
        private IEnumerator PlayBonusEnhanceAnimationAndShowPanel()
        {
            // 播放 Bonus Enhance 動畫
            yield return StartCoroutine(PlayBonusEnhanceAnimationCoroutine());
            
            // 動畫結束後，顯示 Bonus Panel 內容
            yield return StartCoroutine(ShowBonusPanelContent());
        }
        
        /// <summary>
        /// 從 SpriteRenderer 同步 Sprite 到 Image 的協程
        /// </summary>
        private IEnumerator SyncSpriteFromRendererToImage()
        {
            if (bonusSpriteRenderer == null || bonusAnimationImage == null)
            {
                yield break;
            }
            
            while (bonusAnimationImage.gameObject.activeSelf && bonusAnimator != null && bonusAnimator.enabled)
            {
                // 從 SpriteRenderer 讀取當前 Sprite
                Sprite currentSprite = bonusSpriteRenderer.sprite;
                if (currentSprite != null)
                {
                    // 將 Sprite 應用到 Image
                    bonusAnimationImage.sprite = currentSprite;
                    
                    // 如果AspectRatioFitter存在，根據Sprite尺寸設置寬高比
                    if (bonusAspectRatioFitter != null && currentSprite != null)
                    {
                        float aspectRatio = (float)currentSprite.texture.width / (float)currentSprite.texture.height;
                        bonusAspectRatioFitter.aspectRatio = aspectRatio;
                        bonusAspectRatioFitter.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent;
                    }
                }
                
                // 每幀更新
                yield return null;
            }
        }
        
        /// <summary>
        /// 顯示 Bonus Panel 內容的動畫序列
        /// </summary>
        private IEnumerator ShowBonusPanelContent()
        {
            // 第一階段：bonusIconImage 淡入
            if (bonusIconImage != null && bonusIconImage.gameObject.activeSelf)
            {
                yield return bonusIconImage.DOFade(1f, 0.5f).WaitForCompletion();
            }
            
            // 第二階段：bonusLevelText 用打字機顯示
            if (bonusLevelText != null && !string.IsNullOrEmpty(savedLevelChangeText))
            {
                bonusLevelText.gameObject.SetActive(true);
                yield return StartCoroutine(TypewriterEffect(bonusLevelText, savedLevelChangeText, 0.01f));
            }
            
            // 第三階段：bonusText 用打字機顯示
            if (bonusText != null && !string.IsNullOrEmpty(savedDescriptionText))
            {
                bonusText.gameObject.SetActive(true);
                yield return StartCoroutine(TypewriterEffect(bonusText, savedDescriptionText, 0.01f));
            }
            
            // 第四階段：bonusAnimationImage 淡入
            if (bonusAnimationImage != null && bonusAnimationImage.gameObject.activeSelf && savedAnimatorController != null)
            {
                yield return bonusAnimationImage.DOFade(1f, 0.5f).WaitForCompletion();
            }
            
            // 第五階段：bonusConfirmButton 出現
            if (bonusConfirmButton != null)
            {
                bonusConfirmButton.gameObject.SetActive(true);
            }
        }
        
        /// <summary>
        /// 說明頁面確認按鈕點擊
        /// </summary>
        private void OnBonusConfirmClicked()
        {
            // 停止動畫播放
            if (bonusAnimator != null)
            {
                bonusAnimator.enabled = false;
            }
            
            // 停止同步協程
            if (spriteSyncCoroutine != null)
            {
                StopCoroutine(spriteSyncCoroutine);
                spriteSyncCoroutine = null;
            }
            
            if (bonusPanel != null)
                bonusPanel.SetActive(false);
            
            waitingForBonusConfirm = false;
            
            // 繼續正常流程
            ContinueAfterBuffSelection();
        }
        
        /// <summary>
        /// 繼續Buff選擇後的流程
        /// </summary>
        private void ContinueAfterBuffSelection()
        {
            // 恢復選擇界面
            if (selectPanel != null)
                selectPanel.SetActive(true);
            
            // 檢查是否還有待選Buff
            if (GameManager.Instance.PendingBuffCount > 0)
            {
                // 刷新選項
                GenerateBuffOptions();
                UpdateCurrentStats();
            }
            else
            {
                // 恢復遊戲狀態
                if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.LevelUp)
                {
                    GameManager.Instance.ChangeGameState(GameState.Playing);
                }
                
                // 關閉選單
                gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 更新當前強化狀態顯示
        /// </summary>
        private void UpdateCurrentStats()
        {
            if (PlayerManager.Instance == null) return;
            
            var stats = PlayerManager.Instance.Stats;
            
            // 檢查 stats 是否已初始化（避免在 Start() 之前調用時出錯）
            if (stats == null)
            {
                Debug.LogWarning("[RoguelikeMenu] PlayerStats is null, skipping update");
                return;
            }
            
            // 構建普通強化內容
            System.Text.StringBuilder normalSb = new System.Text.StringBuilder();
            
            normalSb.AppendLine(LocalizationHelper.GetLocalizedString("普通增益強化項目"));
            normalSb.AppendLine();
            
            // 齊射強化進度條（簡化版）
            string salvoProgress = GetSimpleProgressBar(stats.salvoLevel, GameConstants.SALVO_MAX_LEVEL);
            normalSb.AppendLine($"{LocalizationHelper.GetLocalizedString("齊射強化")} {salvoProgress}");
            if (showDetailedInfo)
            {
                int maxSalvoPercent = GameConstants.SALVO_MAX_LEVEL * 50;
                normalSb.AppendLine($"  當前: {stats.salvoLevel * 50}%/列 (上限 {maxSalvoPercent}%/列)");
            }
            normalSb.AppendLine();
            
            // 連發強化進度條（簡化版）
            string burstProgress = GetSimpleProgressBar(stats.burstLevel, GameConstants.BURST_MAX_LEVEL);
            normalSb.AppendLine($"{LocalizationHelper.GetLocalizedString("連發強化")} {burstProgress}");
            if (showDetailedInfo)
            {
                int maxBurstPercent = GameConstants.BURST_MAX_LEVEL * 25;
                normalSb.AppendLine($"  當前: {stats.burstLevel * 25}%/發 (上限 {maxBurstPercent}%/發)");
            }
            normalSb.AppendLine();
            
            // 反擊強化進度條（簡化版）
            string counterProgress = GetSimpleProgressBar(stats.counterFireLevel, GameConstants.COUNTER_MAX_LEVEL);
            normalSb.AppendLine($"{LocalizationHelper.GetLocalizedString("反擊強化")} {counterProgress}");
            if (showDetailedInfo)
            {
                normalSb.AppendLine($"  當前: {stats.counterFireLevel}發子彈 (上限 {GameConstants.COUNTER_MAX_LEVEL}發子彈)");
                normalSb.AppendLine($"  <size=80%>{LocalizationHelper.GetLocalizedString("方塊在生成後的0.2秒內受到傷害會觸發反擊")}</size>");
            }
            normalSb.AppendLine();
            
            // 爆炸充能進度條（簡化版）
            string explosionProgress = GetSimpleProgressBar(stats.explosionChargeLevel, GameConstants.EXPLOSION_BUFF_MAX_LEVEL);
            normalSb.AppendLine($"{LocalizationHelper.GetLocalizedString("衝擊擴充")} {explosionProgress}");
            if (showDetailedInfo)
            {
                int maxExplosionCharge = GameConstants.EXPLOSION_BUFF_MAX_LEVEL * GameConstants.EXPLOSION_BUFF_MAX_CHARGE_INCREASE;
                normalSb.AppendLine($"  當前上限: {stats.explosionMaxCharge} (最大 {maxExplosionCharge})");
                normalSb.AppendLine($"  <size=80%>{LocalizationHelper.GetLocalizedStringFormat("網格溢位時消耗 {0} CP，釋放衝擊炮累積的充能對敵人造成傷害", GameConstants.OVERFLOW_CP_COST)}</size>");
                normalSb.AppendLine($"  <size=80%><color=red>{LocalizationHelper.GetLocalizedString("若 CP 不足，自身HP 歸 1")}</color></size>");
            }
            normalSb.AppendLine();
            
            // 資源擴充進度條（簡化版）
            string cpProgress = GetSimpleProgressBar(stats.cpExpansionLevel, GameConstants.RESOURCE_EXPANSION_MAX_LEVEL);
            normalSb.AppendLine($"{LocalizationHelper.GetLocalizedString("資源擴充")} {cpProgress}");
            if (showDetailedInfo)
            {
                int maxCp = GameConstants.PLAYER_MAX_CP + GameConstants.RESOURCE_EXPANSION_MAX_LEVEL * 50;
                normalSb.AppendLine($"  當前 CP: {stats.maxCp} (上限 {maxCp})");
                normalSb.AppendLine($"  <size=80%>{LocalizationHelper.GetLocalizedString("釋放衝擊炮和使用主動技能需消耗CP")}</size>");
            }
            normalSb.AppendLine();
            
            // 空間擴充進度條（簡化版）
            string spaceProgress = GetSimpleProgressBar(stats.spaceExpansionLevel, GameConstants.SPACE_EXPANSION_MAX_LEVEL);
            normalSb.AppendLine($"{LocalizationHelper.GetLocalizedString("空間擴充")} {spaceProgress}");
            if (showDetailedInfo)
            {
                normalSb.AppendLine($"  當前槽位: {stats.spaceExpansionLevel} (上限 {GameConstants.SPACE_EXPANSION_MAX_LEVEL})");
                normalSb.AppendLine($"  <size=80%>{LocalizationHelper.GetLocalizedString("可儲存的方塊槽位數量")}</size>");
            }
            
            // 設置普通強化文本
            if (normalStatsText != null)
            {
                normalStatsText.text = normalSb.ToString();
            }
            
            // 構建傳奇強化內容
            System.Text.StringBuilder legendarySb = new System.Text.StringBuilder();
            
            legendarySb.AppendLine(LocalizationHelper.GetLocalizedString("傳奇增益強化項目"));
            legendarySb.AppendLine();
            
            legendarySb.AppendLine($"{LocalizationHelper.GetLocalizedString("方塊可承受子彈次數 :")} {stats.blockDefenseLevel}");
            legendarySb.AppendLine();
            
            legendarySb.AppendLine($"{LocalizationHelper.GetLocalizedString("導彈傷害倍率 :")} x{1 + stats.missileExtraCount}");
            legendarySb.AppendLine();
            
            // 湮滅技能
            legendarySb.AppendLine($"{LocalizationHelper.GetLocalizedString("湮滅 (按鍵1) - 消耗")} {GameConstants.ANNIHILATION_CP_COST} CP");
            legendarySb.AppendLine($"<size=80%>{LocalizationHelper.GetLocalizedString("進入幽靈穿透狀態，硬降時破壞重疊方塊並發射導彈")}</size>");
            if (!PlayerManager.Instance.IsAnnihilationUnlocked())
            {
                legendarySb.AppendLine($"<color=red>({LocalizationHelper.GetLocalizedString("無法使用，獲得一次戰術擴張以解鎖")})</color>");
            }
            legendarySb.AppendLine();
            
            // 處決技能
            legendarySb.AppendLine($"{LocalizationHelper.GetLocalizedString("處決 (按鍵2) - 消耗")} {GameConstants.EXECUTION_CP_COST} CP");
            legendarySb.AppendLine($"<size=80%>{LocalizationHelper.GetLocalizedString("清除每列最上方的方塊並發射導彈")}</size>");
            if (!PlayerManager.Instance.IsExecutionUnlocked())
            {
                legendarySb.AppendLine($"<color=red>({LocalizationHelper.GetLocalizedString("無法使用，獲得兩次戰術擴張以解鎖")})</color>");
            }
            legendarySb.AppendLine();
            
            // 修補技能
            legendarySb.AppendLine($"{LocalizationHelper.GetLocalizedString("修補 (按鍵3) - 消耗")} {GameConstants.REPAIR_CP_COST} CP");
            legendarySb.AppendLine($"<size=80%>{LocalizationHelper.GetLocalizedString("填補封閉空洞並檢查消除")}</size>");
            if (!PlayerManager.Instance.IsRepairUnlocked())
            {
                legendarySb.AppendLine($"<color=red>({LocalizationHelper.GetLocalizedString("無法使用，獲得三次戰術擴張以解鎖")})</color>");
            }
            
            // 設置傳奇強化文本
            if (legendaryStatsText != null)
            {
                legendaryStatsText.text = legendarySb.ToString();
            }
        }
        
        /// <summary>
        /// 生成簡化版進度條字符串（不帶等級標記）
        /// </summary>
        private string GetSimpleProgressBar(int current, int max)
        {
            System.Text.StringBuilder progress = new System.Text.StringBuilder();
            
            for (int i = 0; i < max; i++)
            {
                bool isCompleted = i < current;
                bool isLast = (i == max - 1);
                
                if (isCompleted)
                {
                    if (isLast)
                    {
                        // 最後一個已完成：★（大一點）
                        progress.Append("<color=yellow><size=120%>★</size></color> ");
                    }
                    else
                    {
                        // 已完成：●（正常大小）
                        progress.Append("<color=yellow>●</color> ");
                    }
                }
                else
                {
                    if (isLast)
                    {
                        // 最後一個未完成：☆（大一點）
                        progress.Append("<color=grey><size=120%>☆</size></color> ");
                    }
                    else
                    {
                        // 未完成：○（小一點）
                        progress.Append("<color=grey><size=80%>○</size></color> ");
                    }
                }
            }
            
            progress.Append($"({current}/{max})");
            
            return progress.ToString();
        }
        
        /// <summary>
        /// 生成進度條字符串（使用方塊符號）
        /// </summary>
        /// <param name="current">當前等級</param>
        /// <param name="max">最大等級</param>
        /// <returns>進度條字符串，例如 "■ ■ ■ □ □"</returns>
        private string GetProgressBar(int current, int max)
        {
            System.Text.StringBuilder progress = new System.Text.StringBuilder();
            
            for (int i = 0; i < max; i++)
            {
                if (i < current)
                {
                    progress.Append("<color=yellow>■</color> "); // 已完成：黃色實心方塊
                }
                else
                {
                    progress.Append("<color=grey>□</color> "); // 未完成：灰色空心方塊
                }
            }
            
            // 添加等級標記
            progress.Append($"({current}/{max})");
            
            return progress.ToString();
        }
        
        /// <summary>
        /// 更新當前關卡敵人攻擊預覽
        /// </summary>
        private IEnumerator UpdateNextStageEnemyPreviewCoroutine()
        {
            ClearAttackPreviews();
            
            if (nextStageEnemyPreviewText == null) yield break;
            if (GameManager.Instance == null) yield break;
            
            var currentStage = GameManager.Instance.CurrentStage;
            
            // 如果沒有當前關卡
            if (currentStage == null)
            {
                if (nextStageEnemyPreviewText != null)
                {
                    nextStageEnemyPreviewText.text = "";
                }
                yield break;
            }
            
            // 設置敵人頭像
            if (enemyIconImage != null)
            {
                if (currentStage.enemyIcon != null)
                {
                    enemyIconImage.sprite = currentStage.enemyIcon;
                    enemyIconImage.gameObject.SetActive(true);
                }
                else
                {
                    enemyIconImage.gameObject.SetActive(false);
                }
            }
            
            // 構建基本資訊文字（先不顯示，等動畫完成後用打字機效果顯示）
            string attackIntervalLabel = LocalizationHelper.GetLocalizedString("攻擊間隔:");
            string previewText = $"HP: {currentStage.maxHp}  |  {attackIntervalLabel} {currentStage.shootInterval}s";
            
            // 先清空文字，等動畫完成後再顯示
            if (nextStageEnemyPreviewText != null)
            {
                nextStageEnemyPreviewText.text = "";
            }
            
            // 關卡描述（先不顯示，等子彈實例化完成後用打字機效果顯示）
            string descriptionText = "";
            if (previewDescriptionText != null)
            {
                string description = GetLocalizedDescription(currentStage);
                if (!string.IsNullOrEmpty(description))
                {
                    descriptionText = description;
                    previewDescriptionText.text = ""; // 先清空，等動畫完成後再顯示
                    previewDescriptionText.gameObject.SetActive(true);
                }
                else
                {
                    previewDescriptionText.text = "";
                    previewDescriptionText.gameObject.SetActive(false);
                }
            }
            
            // 如果是 Boss 關卡，同時顯示 Boss Battle 動畫和 Stage 動畫
            bool bossBattleCompleted = false;
            bool stageCompleted = false;
            
            if (currentStage.isBossStage && bossBattleText != null)
            {
                StartCoroutine(WaitForBossBattleAnimation(() => bossBattleCompleted = true));
            }
            else if (bossBattleText != null)
            {
                // 如果不是 Boss 關卡，隱藏文字
                bossBattleText.gameObject.SetActive(false);
                bossBattleCompleted = true; // 標記為已完成（因為不需要播放）
            }
            else
            {
                bossBattleCompleted = true; // 標記為已完成（因為沒有 bossBattleText）
            }
            
            // 每關都顯示 Stage 動畫（傳遞預覽文字、關卡數據和描述文字，讓動畫完成後顯示）
            if (stageText != null)
            {
                StartCoroutine(WaitForStageAnimation(currentStage, GameManager.Instance.CurrentStageIndex, previewText, descriptionText, () => stageCompleted = true));
            }
            else
            {
                // 如果沒有 Stage 動畫，在第一階段開始時禁用關閉按鈕
                if (closeEnemyPanelButton != null)
                {
                    closeEnemyPanelButton.gameObject.SetActive(false);
                }
                
                // 如果沒有 Stage 動畫，直接顯示文字，然後生成攻擊預覽
                if (nextStageEnemyPreviewText != null)
                {
                    nextStageEnemyPreviewText.text = previewText;
                }
                // 沒有動畫時，直接生成攻擊預覽（傳遞描述文字）
                GenerateAttackPreviews(currentStage, descriptionText);
                stageCompleted = true; // 標記為已完成
            }
            
            // 等待所有動畫完成
            yield return new WaitUntil(() => bossBattleCompleted && stageCompleted);
        }
        
        /// <summary>
        /// 等待 Boss Battle 動畫完成
        /// </summary>
        private IEnumerator WaitForBossBattleAnimation(System.Action onComplete)
        {
            yield return StartCoroutine(PlayBossBattleAnimationCoroutine());
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// 等待 Stage 動畫完成
        /// </summary>
        private IEnumerator WaitForStageAnimation(StageDataSO stage, int stageIndex, string previewText, string descriptionText, System.Action onComplete)
        {
            yield return StartCoroutine(PlayStageAnimationCoroutine(stage, stageIndex, previewText, descriptionText));
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// 播放 Boss Battle 動畫（協程版本）
        /// 每個字符從透明+大 → 不透明+正常大小，然後整句向左移出
        /// </summary>
        private IEnumerator PlayBossBattleAnimationCoroutine()
        {
            if (bossBattleText == null) yield break;
            
            // 清理之前的字符對象
            ClearBossBattleChars();
            
            // 直接使用本地化文字（不從 TextMeshProUGUI.text 讀取，確保語言切換時能更新）
            string text = LocalizationHelper.GetLocalizedString("遭遇強敵!!!");
            
            // 先設置文字並強制更新，以獲取字符位置信息
            bossBattleText.text = text;
            bossBattleText.ForceMeshUpdate();
            
            // 獲取容器（如果沒有指定，使用文字本身的 Transform）
            Transform container = bossBattleTextContainer != null ? bossBattleTextContainer : bossBattleText.transform;
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
            bossBattleText.gameObject.SetActive(true);
            TMP_TextInfo textInfo = bossBattleText.textInfo;
            int characterCount = textInfo.characterCount;
            
            // 如果字符數量為 0，可能是文字還沒更新，強制更新一次
            if (characterCount == 0)
            {
                bossBattleText.ForceMeshUpdate();
                textInfo = bossBattleText.textInfo;
                characterCount = textInfo.characterCount;
            }
            
            // 如果還是沒有字符，直接返回
            if (characterCount == 0)
            {
                Debug.LogWarning("[RoguelikeMenu] Boss Battle 文字沒有字符，無法播放動畫");
                yield break;
            }
            
            // 隱藏原始文字（我們將使用單獨的字符對象）
            bossBattleText.gameObject.SetActive(false);
            
            // 為每個字符創建單獨的 TextMeshProUGUI 對象
            List<GameObject> charObjects = new List<GameObject>();
            
            for (int i = 0; i < characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                
                // 跳過空格和不可見字符
                if (!charInfo.isVisible || charInfo.character == ' ') continue;
                
                // 創建字符對象
                GameObject charObj = new GameObject($"BossChar_{i}");
                charObj.transform.SetParent(container, false);
                
                // 添加 TextMeshProUGUI 組件
                TextMeshProUGUI charText = charObj.AddComponent<TextMeshProUGUI>();
                charText.text = charInfo.character.ToString(); // 使用 charInfo.character 而不是 text[i]，避免索引不匹配
                
                // 複製字體相關屬性
                charText.font = bossBattleText.font;
                charText.fontSize = bossBattleText.fontSize;
                charText.fontSizeMin = bossBattleText.fontSizeMin;
                charText.fontSizeMax = bossBattleText.fontSizeMax;
                charText.fontStyle = bossBattleText.fontStyle;
                charText.fontWeight = bossBattleText.fontWeight;
                charText.enableAutoSizing = bossBattleText.enableAutoSizing;
                
                // 複製顏色相關屬性
                charText.color = bossBattleText.color;
                charText.faceColor = bossBattleText.faceColor;
                charText.enableVertexGradient = bossBattleText.enableVertexGradient;
                if (bossBattleText.enableVertexGradient)
                {
                    charText.colorGradient = bossBattleText.colorGradient;
                }
                charText.colorGradientPreset = bossBattleText.colorGradientPreset;
                
                // 複製對齊方式
                charText.alignment = bossBattleText.alignment;
                charText.horizontalAlignment = bossBattleText.horizontalAlignment;
                charText.verticalAlignment = bossBattleText.verticalAlignment;
                
                // 複製間距
                charText.characterSpacing = bossBattleText.characterSpacing;
                charText.wordSpacing = bossBattleText.wordSpacing;
                charText.lineSpacing = bossBattleText.lineSpacing;
                charText.paragraphSpacing = bossBattleText.paragraphSpacing;
                
                // 複製其他屬性
                charText.textWrappingMode = bossBattleText.textWrappingMode;
                charText.overflowMode = bossBattleText.overflowMode;
                // enableKerning 已過時，使用 fontFeatures
                // charText.enableKerning = bossBattleText.enableKerning;
                
                // 複製材質
                charText.fontSharedMaterial = bossBattleText.fontSharedMaterial;
                charText.fontMaterials = bossBattleText.fontMaterials;
                
                // 複製 Outline 效果（如果有的話）
                var originalOutline = bossBattleText.GetComponent<UnityEngine.UI.Outline>();
                if (originalOutline != null)
                {
                    var charOutline = charObj.AddComponent<UnityEngine.UI.Outline>();
                    charOutline.effectColor = originalOutline.effectColor;
                    charOutline.effectDistance = originalOutline.effectDistance;
                    charOutline.useGraphicAlpha = originalOutline.useGraphicAlpha;
                }
                
                // 複製 Shadow 效果（如果有的話）
                var originalShadow = bossBattleText.GetComponent<UnityEngine.UI.Shadow>();
                if (originalShadow != null)
                {
                    var charShadow = charObj.AddComponent<UnityEngine.UI.Shadow>();
                    charShadow.effectColor = originalShadow.effectColor;
                    charShadow.effectDistance = originalShadow.effectDistance;
                    charShadow.useGraphicAlpha = originalShadow.useGraphicAlpha;
                }
                
                // 複製其他 UI 屬性
                charText.raycastTarget = bossBattleText.raycastTarget;
                charText.maskable = bossBattleText.maskable;
                
                // 計算字符位置（相對於容器）
                // 字符的中心位置（相對於文字對象）
                Vector3 charCenterLocal = (charInfo.topLeft + charInfo.topRight + 
                                          charInfo.bottomLeft + charInfo.bottomRight) / 4f;
                
                // 轉換為世界空間
                Vector3 charCenterWorld = bossBattleText.transform.TransformPoint(charCenterLocal);
                
                // 轉換為容器的本地空間
                Vector3 charCenterContainerLocal = container.InverseTransformPoint(charCenterWorld);
                // 確保 Y 位置為 0
                charObj.transform.localPosition = new Vector3(charCenterContainerLocal.x, 0f, charCenterContainerLocal.z);
                
                // 初始狀態：透明+大（保留原始顏色的 RGB，只改變 alpha）
                Color originalColor = charText.color;
                charText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
                charObj.transform.localScale = Vector3.one * 2f;
                
                charObjects.Add(charObj);
                bossBattleCharObjects.Add(charObj);
            }
            
            // 如果沒有創建任何字符對象，直接返回
            if (charObjects.Count == 0)
            {
                Debug.LogWarning("[RoguelikeMenu] Boss Battle 沒有可顯示的字符，無法播放動畫");
                // 恢復原始文字顯示
                bossBattleText.gameObject.SetActive(true);
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
                // 在動畫開始時播放音效（delay 結束後）
                charSeq.AppendCallback(() => {
                    PlayBossBattleCharSound();
                });
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
            ClearBossBattleChars();
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
        /// 清理 Boss Battle 字符對象
        /// </summary>
        private void ClearBossBattleChars()
        {
            foreach (var charObj in bossBattleCharObjects)
            {
                if (charObj != null)
                {
                    Destroy(charObj);
                }
            }
            bossBattleCharObjects.Clear();
        }
        
        /// <summary>
        /// 播放 Boss Battle 字符動畫音效
        /// </summary>
        private void PlayBossBattleCharSound()
        {
            if (bossBattleCharSound != null)
            {
                // 嘗試使用 AudioManager（如果存在）
                if (Tenronis.Audio.AudioManager.Instance != null)
                {
                    AudioSource audioSource = Tenronis.Audio.AudioManager.Instance.GetComponent<AudioSource>();
                    if (audioSource != null)
                    {
                        audioSource.PlayOneShot(bossBattleCharSound);
                        return;
                    }
                }
                
                // 如果沒有 AudioManager，創建臨時的 AudioSource
                GameObject tempAudio = new GameObject("TempAudio");
                AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
                tempSource.PlayOneShot(bossBattleCharSound);
                Destroy(tempAudio, bossBattleCharSound.length + 0.1f);
            }
        }
        
        /// <summary>
        /// 播放工程選擇動畫（協程版本）
        /// 每個字符從透明+大 → 不透明+正常大小，然後整句向左移出
        /// </summary>
        private IEnumerator PlayProjectSelectionAnimationCoroutine()
        {
            if (projectSelectionText == null) yield break;
            
            // 在動畫開始時，隱藏 BuffOptionsContainer
            if (buffOptionsContainer != null)
            {
                buffOptionsContainer.gameObject.SetActive(false);
            }
            
            // 清理之前的字符對象
            ClearProjectSelectionChars();
            
            // 直接使用本地化文字（不從 TextMeshProUGUI.text 讀取，確保語言切換時能更新）
            string text = LocalizationHelper.GetLocalizedString("工程選擇");
            
            // 先設置文字並強制更新，以獲取字符位置信息
            projectSelectionText.text = text;
            projectSelectionText.ForceMeshUpdate();
            
            // 獲取容器（如果沒有指定，使用文字本身的 Transform）
            Transform container = projectSelectionTextContainer != null ? projectSelectionTextContainer : projectSelectionText.transform;
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
            projectSelectionText.gameObject.SetActive(true);
            TMP_TextInfo textInfo = projectSelectionText.textInfo;
            int characterCount = textInfo.characterCount;
            
            // 如果字符數量為 0，可能是文字還沒更新，強制更新一次
            if (characterCount == 0)
            {
                projectSelectionText.ForceMeshUpdate();
                textInfo = projectSelectionText.textInfo;
                characterCount = textInfo.characterCount;
            }
            
            // 如果還是沒有字符，直接返回
            if (characterCount == 0)
            {
                Debug.LogWarning("[RoguelikeMenu] 工程選擇文字沒有字符，無法播放動畫");
                yield break;
            }
            
            // 隱藏原始文字（我們將使用單獨的字符對象）
            projectSelectionText.gameObject.SetActive(false);
            
            // 為每個字符創建單獨的 TextMeshProUGUI 對象
            List<GameObject> charObjects = new List<GameObject>();
            
            for (int i = 0; i < characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                
                // 跳過空格和不可見字符
                if (!charInfo.isVisible || charInfo.character == ' ') continue;
                
                // 創建字符對象
                GameObject charObj = new GameObject($"ProjectSelectionChar_{i}");
                charObj.transform.SetParent(container, false);
                
                // 添加 TextMeshProUGUI 組件
                TextMeshProUGUI charText = charObj.AddComponent<TextMeshProUGUI>();
                charText.text = charInfo.character.ToString();
                
                // 複製字體相關屬性
                charText.font = projectSelectionText.font;
                charText.fontSize = projectSelectionText.fontSize;
                charText.fontSizeMin = projectSelectionText.fontSizeMin;
                charText.fontSizeMax = projectSelectionText.fontSizeMax;
                charText.fontStyle = projectSelectionText.fontStyle;
                charText.fontWeight = projectSelectionText.fontWeight;
                charText.enableAutoSizing = projectSelectionText.enableAutoSizing;
                
                // 複製顏色相關屬性
                charText.color = projectSelectionText.color;
                charText.faceColor = projectSelectionText.faceColor;
                charText.enableVertexGradient = projectSelectionText.enableVertexGradient;
                if (projectSelectionText.enableVertexGradient)
                {
                    charText.colorGradient = projectSelectionText.colorGradient;
                }
                charText.colorGradientPreset = projectSelectionText.colorGradientPreset;
                
                // 複製對齊方式
                charText.alignment = projectSelectionText.alignment;
                charText.horizontalAlignment = projectSelectionText.horizontalAlignment;
                charText.verticalAlignment = projectSelectionText.verticalAlignment;
                
                // 複製間距
                charText.characterSpacing = projectSelectionText.characterSpacing;
                charText.wordSpacing = projectSelectionText.wordSpacing;
                charText.lineSpacing = projectSelectionText.lineSpacing;
                charText.paragraphSpacing = projectSelectionText.paragraphSpacing;
                
                // 複製其他屬性
                charText.textWrappingMode = projectSelectionText.textWrappingMode;
                charText.overflowMode = projectSelectionText.overflowMode;
                
                // 複製材質
                charText.fontSharedMaterial = projectSelectionText.fontSharedMaterial;
                charText.fontMaterials = projectSelectionText.fontMaterials;
                
                // 複製 Outline 效果（如果有的話）
                var originalOutline = projectSelectionText.GetComponent<UnityEngine.UI.Outline>();
                if (originalOutline != null)
                {
                    var charOutline = charObj.AddComponent<UnityEngine.UI.Outline>();
                    charOutline.effectColor = originalOutline.effectColor;
                    charOutline.effectDistance = originalOutline.effectDistance;
                    charOutline.useGraphicAlpha = originalOutline.useGraphicAlpha;
                }
                
                // 複製 Shadow 效果（如果有的話）
                var originalShadow = projectSelectionText.GetComponent<UnityEngine.UI.Shadow>();
                if (originalShadow != null)
                {
                    var charShadow = charObj.AddComponent<UnityEngine.UI.Shadow>();
                    charShadow.effectColor = originalShadow.effectColor;
                    charShadow.effectDistance = originalShadow.effectDistance;
                    charShadow.useGraphicAlpha = originalShadow.useGraphicAlpha;
                }
                
                // 複製其他 UI 屬性
                charText.raycastTarget = projectSelectionText.raycastTarget;
                charText.maskable = projectSelectionText.maskable;
                
                // 計算字符位置（相對於容器）
                // 字符的中心位置（相對於文字對象）
                Vector3 charCenterLocal = (charInfo.topLeft + charInfo.topRight + 
                                          charInfo.bottomLeft + charInfo.bottomRight) / 4f;
                
                // 轉換為世界空間
                Vector3 charCenterWorld = projectSelectionText.transform.TransformPoint(charCenterLocal);
                
                // 轉換為容器的本地空間
                Vector3 charCenterContainerLocal = container.InverseTransformPoint(charCenterWorld);
                // 確保 Y 位置為 0
                charObj.transform.localPosition = new Vector3(charCenterContainerLocal.x, 0f, charCenterContainerLocal.z);
                
                // 初始狀態：透明+大（保留原始顏色的 RGB，只改變 alpha）
                Color originalColor = charText.color;
                charText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
                charObj.transform.localScale = Vector3.one * 2f;
                
                charObjects.Add(charObj);
                projectSelectionCharObjects.Add(charObj);
            }
            
            // 如果沒有創建任何字符對象，直接返回
            if (charObjects.Count == 0)
            {
                Debug.LogWarning("[RoguelikeMenu] 工程選擇沒有可顯示的字符，無法播放動畫");
                // 恢復原始文字顯示
                projectSelectionText.gameObject.SetActive(true);
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
                // 在動畫開始時播放音效（delay 結束後）
                charSeq.AppendCallback(() => {
                    PlayProjectSelectionCharSound();
                });
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
            ClearProjectSelectionChars();
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
            
            // 動畫完成後，顯示 BuffOptionsContainer
            if (buffOptionsContainer != null)
            {
                buffOptionsContainer.gameObject.SetActive(true);
            }
            
            // 啟動第一組的閃爍動畫
            StartPendingBlinkAnimations();
        }
        
        /// <summary>
        /// 啟動待啟動的閃爍動畫（第一組選項）
        /// </summary>
        private void StartPendingBlinkAnimations()
        {
            foreach (var (text, currentLevel, maxLevel) in pendingBlinkAnimations)
            {
                if (text != null && text.gameObject.activeInHierarchy)
                {
                    StartBonusHintBlinkAnimation(text, currentLevel, maxLevel);
                }
            }
            pendingBlinkAnimations.Clear();
        }
        
        /// <summary>
        /// 清理工程選擇字符對象
        /// </summary>
        private void ClearProjectSelectionChars()
        {
            foreach (var charObj in projectSelectionCharObjects)
            {
                if (charObj != null)
                {
                    Destroy(charObj);
                }
            }
            projectSelectionCharObjects.Clear();
        }
        
        /// <summary>
        /// 播放工程選擇字符動畫音效
        /// </summary>
        private void PlayProjectSelectionCharSound()
        {
            if (projectSelectionCharSound != null)
            {
                // 嘗試使用 AudioManager（如果存在）
                if (Tenronis.Audio.AudioManager.Instance != null)
                {
                    AudioSource audioSource = Tenronis.Audio.AudioManager.Instance.GetComponent<AudioSource>();
                    if (audioSource != null)
                    {
                        audioSource.PlayOneShot(projectSelectionCharSound);
                        return;
                    }
                }
                
                // 如果沒有 AudioManager，創建臨時的 AudioSource
                GameObject tempAudio = new GameObject("TempAudio");
                AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
                tempSource.PlayOneShot(projectSelectionCharSound);
                Destroy(tempAudio, projectSelectionCharSound.length + 0.1f);
            }
        }
        
        /// <summary>
        /// 播放 Bonus Enhance 動畫（協程版本）
        /// 每個字符從透明+大 → 不透明+正常大小，然後整句向左移出
        /// </summary>
        private IEnumerator PlayBonusEnhanceAnimationCoroutine()
        {
            if (bonusEnhanceText == null) yield break;
            
            // 清理之前的字符對象
            ClearBonusEnhanceChars();
            
            // 直接使用本地化文字（不從 TextMeshProUGUI.text 讀取，確保語言切換時能更新）
            string text = LocalizationHelper.GetLocalizedString("傳奇工程");
            
            // 先設置文字並強制更新，以獲取字符位置信息
            bonusEnhanceText.text = text;
            bonusEnhanceText.ForceMeshUpdate();
            
            // 獲取容器（如果沒有指定，使用文字本身的 Transform）
            Transform container = bonusEnhanceTextContainer != null ? bonusEnhanceTextContainer : bonusEnhanceText.transform;
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
            bonusEnhanceText.gameObject.SetActive(true);
            TMP_TextInfo textInfo = bonusEnhanceText.textInfo;
            int characterCount = textInfo.characterCount;
            
            // 如果字符數量為 0，可能是文字還沒更新，強制更新一次
            if (characterCount == 0)
            {
                bonusEnhanceText.ForceMeshUpdate();
                textInfo = bonusEnhanceText.textInfo;
                characterCount = textInfo.characterCount;
            }
            
            // 如果還是沒有字符，直接返回
            if (characterCount == 0)
            {
                Debug.LogWarning("[RoguelikeMenu] Bonus Enhance 文字沒有字符，無法播放動畫");
                yield break;
            }
            
            // 隱藏原始文字（我們將使用單獨的字符對象）
            bonusEnhanceText.gameObject.SetActive(false);
            
            // 為每個字符創建單獨的 TextMeshProUGUI 對象
            List<GameObject> charObjects = new List<GameObject>();
            
            for (int i = 0; i < characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                
                // 跳過空格和不可見字符
                if (!charInfo.isVisible || charInfo.character == ' ') continue;
                
                // 創建字符對象
                GameObject charObj = new GameObject($"BonusEnhanceChar_{i}");
                charObj.transform.SetParent(container, false);
                
                // 添加 TextMeshProUGUI 組件
                TextMeshProUGUI charText = charObj.AddComponent<TextMeshProUGUI>();
                charText.text = charInfo.character.ToString(); // 使用 charInfo.character 而不是 text[i]，避免索引不匹配
                
                // 複製字體相關屬性
                charText.font = bonusEnhanceText.font;
                charText.fontSize = bonusEnhanceText.fontSize;
                charText.fontSizeMin = bonusEnhanceText.fontSizeMin;
                charText.fontSizeMax = bonusEnhanceText.fontSizeMax;
                charText.fontStyle = bonusEnhanceText.fontStyle;
                charText.fontWeight = bonusEnhanceText.fontWeight;
                charText.enableAutoSizing = bonusEnhanceText.enableAutoSizing;
                
                // 複製顏色相關屬性
                charText.color = bonusEnhanceText.color;
                charText.faceColor = bonusEnhanceText.faceColor;
                charText.enableVertexGradient = bonusEnhanceText.enableVertexGradient;
                if (bonusEnhanceText.enableVertexGradient)
                {
                    charText.colorGradient = bonusEnhanceText.colorGradient;
                }
                charText.colorGradientPreset = bonusEnhanceText.colorGradientPreset;
                
                // 複製對齊方式
                charText.alignment = bonusEnhanceText.alignment;
                charText.horizontalAlignment = bonusEnhanceText.horizontalAlignment;
                charText.verticalAlignment = bonusEnhanceText.verticalAlignment;
                
                // 複製間距
                charText.characterSpacing = bonusEnhanceText.characterSpacing;
                charText.wordSpacing = bonusEnhanceText.wordSpacing;
                charText.lineSpacing = bonusEnhanceText.lineSpacing;
                charText.paragraphSpacing = bonusEnhanceText.paragraphSpacing;
                
                // 複製其他屬性
                charText.textWrappingMode = bonusEnhanceText.textWrappingMode;
                charText.overflowMode = bonusEnhanceText.overflowMode;
                
                // 複製材質
                charText.fontSharedMaterial = bonusEnhanceText.fontSharedMaterial;
                charText.fontMaterials = bonusEnhanceText.fontMaterials;
                
                // 複製 Outline 效果（如果有的話）
                var originalOutline = bonusEnhanceText.GetComponent<UnityEngine.UI.Outline>();
                if (originalOutline != null)
                {
                    var charOutline = charObj.AddComponent<UnityEngine.UI.Outline>();
                    charOutline.effectColor = originalOutline.effectColor;
                    charOutline.effectDistance = originalOutline.effectDistance;
                    charOutline.useGraphicAlpha = originalOutline.useGraphicAlpha;
                }
                
                // 複製 Shadow 效果（如果有的話）
                var originalShadow = bonusEnhanceText.GetComponent<UnityEngine.UI.Shadow>();
                if (originalShadow != null)
                {
                    var charShadow = charObj.AddComponent<UnityEngine.UI.Shadow>();
                    charShadow.effectColor = originalShadow.effectColor;
                    charShadow.effectDistance = originalShadow.effectDistance;
                    charShadow.useGraphicAlpha = originalShadow.useGraphicAlpha;
                }
                
                // 複製其他 UI 屬性
                charText.raycastTarget = bonusEnhanceText.raycastTarget;
                charText.maskable = bonusEnhanceText.maskable;
                
                // 計算字符位置（相對於容器）
                Vector3 charCenterLocal = (charInfo.topLeft + charInfo.topRight + 
                                          charInfo.bottomLeft + charInfo.bottomRight) / 4f;
                
                // 轉換為世界空間
                Vector3 charCenterWorld = bonusEnhanceText.transform.TransformPoint(charCenterLocal);
                
                // 轉換為容器的本地空間
                Vector3 charCenterContainerLocal = container.InverseTransformPoint(charCenterWorld);
                // 確保 Y 位置為 0
                charObj.transform.localPosition = new Vector3(charCenterContainerLocal.x, 0f, charCenterContainerLocal.z);
                
                // 初始狀態：透明+大（保留原始顏色的 RGB，只改變 alpha）
                Color originalColor = charText.color;
                charText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
                charObj.transform.localScale = Vector3.one * 2f;
                
                charObjects.Add(charObj);
                bonusEnhanceCharObjects.Add(charObj);
            }
            
            // 如果沒有創建任何字符對象，直接返回
            if (charObjects.Count == 0)
            {
                Debug.LogWarning("[RoguelikeMenu] Bonus Enhance 沒有可顯示的字符，無法播放動畫");
                // 恢復原始文字顯示
                bonusEnhanceText.gameObject.SetActive(true);
                yield break;
            }
            
            // 獲取 bonusPanel 的 Image 組件
            Image bonusPanelImage = bonusPanel != null ? bonusPanel.GetComponent<Image>() : null;
            
            // 創建動畫序列
            Sequence mainSequence = DOTween.Sequence();
            
            // 第一階段：容器高度從 0 擴展到原始高度（上下擴展），同時 bonusPanel Image 淡入
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
            
            // bonusPanel Image 同時淡入
            if (bonusPanelImage != null)
            {
                mainSequence.Join(bonusPanelImage.DOFade(1f, containerExpandDuration));
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
                // 在動畫開始時播放音效（delay 結束後）
                charSeq.AppendCallback(() => {
                    PlayBonusEnhanceCharSound();
                });
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
            ClearBonusEnhanceChars();
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
        /// 清理 Bonus Enhance 字符對象
        /// </summary>
        private void ClearBonusEnhanceChars()
        {
            foreach (var charObj in bonusEnhanceCharObjects)
            {
                if (charObj != null)
                {
                    Destroy(charObj);
                }
            }
            bonusEnhanceCharObjects.Clear();
        }
        
        /// <summary>
        /// 播放 Bonus Enhance 字符動畫音效
        /// </summary>
        private void PlayBonusEnhanceCharSound()
        {
            if (bonusEnhanceCharSound != null)
            {
                // 嘗試使用 AudioManager（如果存在）
                if (Tenronis.Audio.AudioManager.Instance != null)
                {
                    AudioSource audioSource = Tenronis.Audio.AudioManager.Instance.GetComponent<AudioSource>();
                    if (audioSource != null)
                    {
                        audioSource.PlayOneShot(bonusEnhanceCharSound);
                        return;
                    }
                }
                
                // 如果沒有 AudioManager，創建臨時的 AudioSource
                GameObject tempAudio = new GameObject("TempAudio");
                AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
                tempSource.PlayOneShot(bonusEnhanceCharSound);
                Destroy(tempAudio, bonusEnhanceCharSound.length + 0.1f);
            }
        }
        
        /// <summary>
        /// 播放 Stage 動畫（協程版本）
        /// 顯示 "Stage n 怪物名稱"，每個字符從透明+大 → 不透明+正常大小，然後整句向左移出
        /// </summary>
        /// <param name="stage">關卡數據</param>
        /// <param name="stageIndex">關卡索引</param>
        /// <param name="previewText">敵人預覽文字（動畫完成後用打字機效果顯示）</param>
        /// <param name="descriptionText">描述文字（子彈實例化完成後用打字機效果顯示）</param>
        private IEnumerator PlayStageAnimationCoroutine(StageDataSO stage, int stageIndex, string previewText = "", string descriptionText = "")
        {
            if (stageText == null || stage == null) yield break;
            
            // 清理之前的字符對象
            ClearStageChars();
            
            // 設置敵人頭像為透明（動畫開始時）
            if (enemyIconImage != null && enemyIconImage.gameObject.activeSelf)
            {
                Color iconColor = enemyIconImage.color;
                iconColor.a = 0f;
                enemyIconImage.color = iconColor;
            }
            
            // 構建文字內容："Stage n 怪物名稱"（根據當前語言選擇）
            string localizedStageName = GetLocalizedStageName(stage);
            string text = $"Stage {stageIndex + 1} {localizedStageName}";
            
            // 獲取容器（如果沒有指定，使用文字本身的 Transform）
            Transform container = stageTextContainer != null ? stageTextContainer : stageText.transform;
            Vector3 originalContainerPosition = container.localPosition;
            
            // 設置容器位置為中心（不需要移入移出）
            container.localPosition = originalContainerPosition;
            
            // 顯示容器
            container.gameObject.SetActive(true);
            
            // 實例化一個新的 TextMeshProUGUI 物件來進行操作
            GameObject tempTextObj = new GameObject("TempStageText");
            tempTextObj.transform.SetParent(container, false);
            TextMeshProUGUI tempStageText = tempTextObj.AddComponent<TextMeshProUGUI>();
            
            // 複製 stageText 的所有屬性到新物件
            tempStageText.text = text;
            tempStageText.font = stageText.font;
            tempStageText.fontSize = stageText.fontSize;
            tempStageText.fontSizeMin = stageText.fontSizeMin;
            tempStageText.fontSizeMax = stageText.fontSizeMax;
            tempStageText.fontStyle = stageText.fontStyle;
            tempStageText.fontWeight = stageText.fontWeight;
            tempStageText.enableAutoSizing = stageText.enableAutoSizing;
            tempStageText.color = stageText.color;
            tempStageText.faceColor = stageText.faceColor;
            tempStageText.enableVertexGradient = stageText.enableVertexGradient;
            if (stageText.enableVertexGradient)
            {
                tempStageText.colorGradient = stageText.colorGradient;
            }
            tempStageText.colorGradientPreset = stageText.colorGradientPreset;
            tempStageText.alignment = stageText.alignment;
            tempStageText.textWrappingMode = TextWrappingModes.NoWrap;
            tempStageText.characterSpacing = stageText.characterSpacing;
            tempStageText.wordSpacing = stageText.wordSpacing;
            tempStageText.lineSpacing = stageText.lineSpacing;
            tempStageText.paragraphSpacing = stageText.paragraphSpacing;
            if (stageText.fontMaterial != null)
            {
                tempStageText.fontMaterial = stageText.fontMaterial;
            }
            tempStageText.outlineWidth = stageText.outlineWidth;
            tempStageText.outlineColor = stageText.outlineColor;
            if (stageText.fontSharedMaterial != null)
            {
                tempStageText.fontSharedMaterial = stageText.fontSharedMaterial;
            }
            tempStageText.raycastTarget = stageText.raycastTarget;
            tempStageText.maskable = stageText.maskable;
            
            // 設置位置與 stageText 相同（相對於容器）
            tempTextObj.transform.localPosition = stageText.transform.localPosition;
            tempTextObj.transform.localRotation = stageText.transform.localRotation;
            tempTextObj.transform.localScale = stageText.transform.localScale;
            
            // 啟用物件並強制更新以獲取字符位置信息
            tempTextObj.SetActive(true);
            tempStageText.ForceMeshUpdate();

            // 獲取字符信息
            TMP_TextInfo textInfo = tempStageText.textInfo;
            int characterCount = textInfo.characterCount;

            // 如果字符數量為 0，可能是文字還沒更新，強制更新並再等待一幀
            if (characterCount == 0)
            {
                tempStageText.ForceMeshUpdate();
                yield return null;
                textInfo = tempStageText.textInfo;
                characterCount = textInfo.characterCount;
            }

            // 如果還是沒有字符，清理並返回
            if (characterCount == 0)
            {
                Debug.LogWarning($"[RoguelikeMenu] Stage 文字沒有字符，無法播放動畫。文字內容: '{text}'");
                Destroy(tempTextObj);
                yield break;
            }
            
            // 為每個字符創建單獨的 TextMeshProUGUI 對象
            List<GameObject> charObjects = new List<GameObject>();
            for (int i = 0; i < characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                
                // 跳過空格和不可見字符
                if (!charInfo.isVisible || charInfo.character == ' ') continue;
                
                // 創建字符對象
                GameObject charObj = new GameObject($"StageChar_{i}");
                charObj.transform.SetParent(container, false);
                
                // 添加 TextMeshProUGUI 組件
                TextMeshProUGUI charText = charObj.AddComponent<TextMeshProUGUI>();
                charText.text = charInfo.character.ToString();
                
                // 複製字體相關屬性
                charText.font = tempStageText.font;
                charText.fontSize = tempStageText.fontSize;
                charText.fontSizeMin = tempStageText.fontSizeMin;
                charText.fontSizeMax = tempStageText.fontSizeMax;
                charText.fontStyle = tempStageText.fontStyle;
                charText.fontWeight = tempStageText.fontWeight;
                charText.enableAutoSizing = tempStageText.enableAutoSizing;
                
                // 複製顏色相關屬性
                charText.color = tempStageText.color;
                charText.faceColor = tempStageText.faceColor;
                charText.enableVertexGradient = tempStageText.enableVertexGradient;
                if (tempStageText.enableVertexGradient)
                {
                    charText.colorGradient = tempStageText.colorGradient;
                }
                charText.colorGradientPreset = tempStageText.colorGradientPreset;
                
                // 複製對齊方式
                charText.alignment = tempStageText.alignment;
                charText.textWrappingMode = TextWrappingModes.NoWrap;
                
                // 複製間距
                charText.characterSpacing = tempStageText.characterSpacing;
                charText.wordSpacing = tempStageText.wordSpacing;
                charText.lineSpacing = tempStageText.lineSpacing;
                charText.paragraphSpacing = tempStageText.paragraphSpacing;
                
                // 複製材質
                if (tempStageText.fontMaterial != null)
                {
                    charText.fontMaterial = tempStageText.fontMaterial;
                }
                
                // 複製外框和陰影
                charText.outlineWidth = tempStageText.outlineWidth;
                charText.outlineColor = tempStageText.outlineColor;
                if (tempStageText.fontSharedMaterial != null)
                {
                    charText.fontSharedMaterial = tempStageText.fontSharedMaterial;
                }
                
                // 其他屬性
                charText.raycastTarget = tempStageText.raycastTarget;
                charText.maskable = tempStageText.maskable;
                
                // 設置初始狀態：透明 + 放大
                Color initialColor = charText.color;
                initialColor.a = 0f; // 透明
                charText.color = initialColor;
                charObj.transform.localScale = Vector3.one * 2f; // 放大 2 倍
                
                // 字符的中心位置（相對於文字對象）
                Vector3 charCenterLocal = (charInfo.topLeft + charInfo.topRight + 
                                          charInfo.bottomLeft + charInfo.bottomRight) / 4f;
                
                // 轉換為世界空間
                Vector3 charCenterWorld = tempStageText.transform.TransformPoint(charCenterLocal);
                
                // 轉換為容器的本地空間
                Vector3 charCenterContainerLocal = container.InverseTransformPoint(charCenterWorld);
                charObj.transform.localPosition = charCenterContainerLocal;
                
                charObjects.Add(charObj);
                stageCharObjects.Add(charObj);
            }
            
            // 如果沒有創建任何字符對象，清理並返回
            if (charObjects.Count == 0)
            {
                Debug.LogWarning("[RoguelikeMenu] Stage 沒有可顯示的字符，無法播放動畫");
                Destroy(tempTextObj);
                yield break;
            }
            
            // 隱藏臨時文字物件（字符對象已經創建完成）
            tempTextObj.SetActive(false);
            
            // 在動畫開始時，禁用關閉按鈕
            if (closeEnemyPanelButton != null)
            {
                closeEnemyPanelButton.gameObject.SetActive(false);
            }
            
            // 創建動畫序列
            Sequence mainSequence = DOTween.Sequence();
            
            // 延遲 0.2 秒後開始動畫
            mainSequence.AppendInterval(0.2f);
            
            // 字符動畫（每個字符從透明+大 → 不透明+正常大小）
            for (int i = 0; i < charObjects.Count; i++)
            {
                GameObject charObj = charObjects[i];
                TextMeshProUGUI charText = charObj.GetComponent<TextMeshProUGUI>();
                
                float delay = i * 0.1f; // 每個字符延遲 0.1 秒
                float duration = 0.4f; // 動畫持續時間
                
                // 字符動畫
                Sequence charSeq = DOTween.Sequence();
                charSeq.AppendInterval(delay);
                // 在動畫開始時播放音效（delay 結束後）
                if (stageCharSound != null)
                {
                    charSeq.AppendCallback(() => {
                        PlayStageCharSound();
                    });
                }
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
            
            // 字符動畫結束時，讓 Enemy Icon 淡入
            mainSequence.AppendCallback(() => {
                if (enemyIconImage != null && enemyIconImage.gameObject.activeSelf)
                {
                    enemyIconImage.DOFade(1f, 0.5f);
                }
            });
            
            // 短暫停頓
            mainSequence.AppendInterval(0.5f);
            
            // 等待動畫完成
            yield return mainSequence.WaitForCompletion();
            
            // 清理臨時文字物件
            if (tempTextObj != null)
            {
                Destroy(tempTextObj);
            }
            
            // 動畫完成後，使用打字機效果顯示預覽文字
            if (nextStageEnemyPreviewText != null && !string.IsNullOrEmpty(previewText))
            {
                yield return StartCoroutine(TypewriterEffectAndGeneratePreviews(nextStageEnemyPreviewText, previewText, 0.01f, stage, descriptionText));
            }
        }
        
        /// <summary>
        /// 清理 Stage 字符對象
        /// </summary>
        private void ClearStageChars()
        {
            foreach (var charObj in stageCharObjects)
            {
                if (charObj != null)
                {
                    Destroy(charObj);
                }
            }
            stageCharObjects.Clear();
        }
        
        /// <summary>
        /// 打字機效果：逐字符顯示文字
        /// </summary>
        /// <param name="textComponent">文字組件</param>
        /// <param name="fullText">完整文字</param>
        /// <param name="delayPerChar">每個字符的延遲時間（秒）</param>
        private IEnumerator TypewriterEffect(TextMeshProUGUI textComponent, string fullText, float delayPerChar)
        {
            if (textComponent == null || string.IsNullOrEmpty(fullText)) yield break;
            
            textComponent.text = "";
            
            for (int i = 0; i < fullText.Length; i++)
            {
                textComponent.text = fullText.Substring(0, i + 1);
                yield return new WaitForSeconds(delayPerChar);
            }
        }
        
        /// <summary>
        /// 打字機效果並生成攻擊預覽：先顯示文字，完成後生成攻擊預覽
        /// </summary>
        /// <param name="textComponent">文字組件</param>
        /// <param name="fullText">完整文字</param>
        /// <param name="delayPerChar">每個字符的延遲時間（秒）</param>
        /// <param name="stage">關卡數據</param>
        /// <param name="descriptionText">描述文字（子彈實例化完成後顯示）</param>
        private IEnumerator TypewriterEffectAndGeneratePreviews(TextMeshProUGUI textComponent, string fullText, float delayPerChar, StageDataSO stage, string descriptionText = "")
        {
            // 先執行打字機效果
            yield return StartCoroutine(TypewriterEffect(textComponent, fullText, delayPerChar));
            
            // 打字機效果完成後，生成攻擊預覽（傳遞描述文字）
            if (stage != null)
            {
                GenerateAttackPreviews(stage, descriptionText);
            }
        }
        
        /// <summary>
        /// 播放 Stage 字符動畫音效
        /// </summary>
        private void PlayStageCharSound()
        {
            if (stageCharSound != null)
            {
                // 嘗試使用 AudioManager（如果存在）
                if (Tenronis.Audio.AudioManager.Instance != null)
                {
                    AudioSource audioSource = Tenronis.Audio.AudioManager.Instance.GetComponent<AudioSource>();
                    if (audioSource != null)
                    {
                        audioSource.PlayOneShot(stageCharSound);
                        return;
                    }
                }
                
                // 如果沒有 AudioManager，創建臨時的 AudioSource
                GameObject tempAudio = new GameObject("TempAudio");
                AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
                tempSource.PlayOneShot(stageCharSound);
                Destroy(tempAudio, stageCharSound.length + 0.1f);
            }
        }
        
        /// <summary>
        /// 生成攻擊預覽視覺項目（權重系統）
        /// 計算方式：(彈種的權重) / (所有 enable 彈種的權重和)
        /// 所有子彈是否會發射，只看 enabled（包括普通子彈）
        /// </summary>
        private void GenerateAttackPreviews(StageDataSO stageData, string descriptionText = "")
        {
            if (enemyAttackPreviewContainer == null || enemyAttackPreviewPrefab == null) return;
            
            // 收集所有啟用的子彈類型及其權重
            var enabledBullets = new List<(BulletType type, string name, string desc, float weight)>();
            
            if (stageData.normalBullet.enabled)
                enabledBullets.Add((BulletType.Normal, LocalizationHelper.GetLocalizedString("普通子彈"), LocalizationHelper.GetLocalizedString("對命中的方塊造成傷害"), stageData.normalBullet.chance));
            
            if (stageData.areaBullet.enabled)
                enabledBullets.Add((BulletType.AreaDamage, LocalizationHelper.GetLocalizedString("範圍傷害"), LocalizationHelper.GetLocalizedString("以命中的方塊為中心，對 3x3 範圍內的方塊造成傷害"), stageData.areaBullet.chance));
            
            if (stageData.addBlockBullet.enabled)
                enabledBullets.Add((BulletType.AddBlock, LocalizationHelper.GetLocalizedString("添加方塊"), LocalizationHelper.GetLocalizedString("對命中的方塊造成傷害，並上方添加垃圾方塊"), stageData.addBlockBullet.chance));
            
            if (stageData.addExplosiveBlockBullet.enabled)
                enabledBullets.Add((BulletType.AddExplosiveBlock, LocalizationHelper.GetLocalizedString("爆炸方塊"), LocalizationHelper.GetLocalizedString("對命中的方塊造成傷害，並上方添加爆炸方塊"), stageData.addExplosiveBlockBullet.chance));
            
            if (stageData.addRowBullet.enabled)
                enabledBullets.Add((BulletType.InsertRow, LocalizationHelper.GetLocalizedString("插入垃圾行"), LocalizationHelper.GetLocalizedString("對命中的方塊造成傷害，並從底部插入一整行垃圾方塊"), stageData.addRowBullet.chance));
            
            if (stageData.addVoidRowBullet.enabled)
                enabledBullets.Add((BulletType.InsertVoidRow, LocalizationHelper.GetLocalizedString("虛無垃圾行"), LocalizationHelper.GetLocalizedString("對命中的方塊造成傷害，並從底部插入一整行虛無方塊"), stageData.addVoidRowBullet.chance));
            
            if (stageData.corruptExplosiveBullet.enabled)
                enabledBullets.Add((BulletType.CorruptExplosive, LocalizationHelper.GetLocalizedString("腐化爆炸"), LocalizationHelper.GetLocalizedString("對命中的方塊造成傷害，並將下個方塊隨機一格變成爆炸方塊"), stageData.corruptExplosiveBullet.chance));
            
            if (stageData.corruptVoidBullet.enabled)
                enabledBullets.Add((BulletType.CorruptVoid, LocalizationHelper.GetLocalizedString("腐化虛無"), LocalizationHelper.GetLocalizedString("對命中的方塊造成傷害，並將下個方塊隨機一格變成虛無方塊"), stageData.corruptVoidBullet.chance));
            
            // 如果沒有任何啟用的子彈，顯示"無"的欄位
            if (enabledBullets.Count == 0)
            {
                StartCoroutine(CreateAttackPreviewItemsSequentially(new List<(BulletType type, string name, string desc, float weight)>(), 0f, descriptionText));
                return;
            }
            
            // 計算總權重
            float totalWeight = 0f;
            foreach (var bullet in enabledBullets)
            {
                totalWeight += bullet.weight;
            }
            
            // 如果總權重為 0，不顯示任何預覽
            if (totalWeight <= 0f)
            {
                return;
            }
            
            // 為每個啟用的子彈生成預覽項目，顯示實際機率 = 權重 / 總權重
            // 使用協程依序顯示動畫
            StartCoroutine(CreateAttackPreviewItemsSequentially(enabledBullets, totalWeight, descriptionText));
        }
        
        /// <summary>
        /// 依序創建攻擊預覽項目，每個項目都有動畫效果
        /// </summary>
        private IEnumerator CreateAttackPreviewItemsSequentially(List<(BulletType type, string name, string desc, float weight)> bullets, float totalWeight, string descriptionText = "")
        {
            // 如果沒有子彈，顯示"無"的欄位
            if (bullets.Count == 0)
            {
                yield return StartCoroutine(CreateAttackPreviewItem(BulletType.Normal, LocalizationHelper.GetLocalizedString("無"), LocalizationHelper.GetLocalizedString("敵人不會發射任何子彈"), 0f, isNoneItem: true));
            }
            else
            {
                foreach (var bullet in bullets)
                {
                    float actualChance = totalWeight > 0f ? bullet.weight / totalWeight : 0f;
                    yield return StartCoroutine(CreateAttackPreviewItem(bullet.type, bullet.name, bullet.desc, actualChance));
                }
            }
            
            // 所有子彈實例化完成後，顯示描述文字（如果有）
            if (previewDescriptionText != null && !string.IsNullOrEmpty(descriptionText))
            {
                // 顯示描述文字
                yield return StartCoroutine(TypewriterEffect(previewDescriptionText, descriptionText, 0.01f));
            }
            
            // 描述文字顯示完成後（或沒有描述文字時），啟用關閉按鈕
            if (closeEnemyPanelButton != null)
            {
                closeEnemyPanelButton.gameObject.SetActive(true);
            }
        }
        
        /// <summary>
        /// 創建單個攻擊預覽項目（協程版本，帶動畫效果）
        /// </summary>
        /// <param name="bulletType">子彈類型</param>
        /// <param name="attackName">攻擊名稱（已本地化）</param>
        /// <param name="description">描述（已本地化）</param>
        /// <param name="chance">機率</param>
        /// <param name="isNoneItem">是否為"無"的情況（敵人不會攻擊）</param>
        private IEnumerator CreateAttackPreviewItem(BulletType bulletType, string attackName, string description, float chance, bool isNoneItem = false)
        {
            GameObject item = Instantiate(enemyAttackPreviewPrefab, enemyAttackPreviewContainer);
            attackPreviewItems.Add(item);
            
            // 獲取背板 Image（在根 GameObject 上）
            Image backgroundImage = item.GetComponent<Image>();
            
            // 查找 ColorImage 下的 SpriteRenderer 和 Animator
            var colorImageTransform = item.transform.Find("ColorImage");
            Image iconImage = null;
            
            if (colorImageTransform != null)
            {
                var spriteRenderer = colorImageTransform.GetComponent<SpriteRenderer>();
                var animator = colorImageTransform.GetComponent<Animator>();
                iconImage = colorImageTransform.GetComponent<Image>();
                
                if (isNoneItem)
                {
                    // 如果是"無"，隱藏 icon
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.gameObject.SetActive(false);
                    }
                    if (iconImage != null)
                    {
                        iconImage.gameObject.SetActive(false);
                    }
                    if (animator != null)
                    {
                        animator.gameObject.SetActive(false);
                    }
                }
                else if (bulletPrefabReference != null)
                {
                    // 設置顏色
                    Color bulletColor = bulletPrefabReference.GetColorByType(bulletType);
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.color = bulletColor;
                    }
                    
                    // 設置動畫控制器
                    RuntimeAnimatorController animController = bulletPrefabReference.GetAnimatorByType(bulletType);
                    if (animator != null && animController != null)
                    {
                        animator.runtimeAnimatorController = animController;
                    }
                    
                    // 啟動協程同步 SpriteRenderer 到 Image
                    if (spriteRenderer != null && iconImage != null)
                    {
                        var coroutine = StartCoroutine(SyncSpriteToImage(spriteRenderer, iconImage, bulletColor));
                        spriteSyncCoroutines.Add(coroutine);
                    }
                    else if (iconImage != null)
                    {
                        // 如果沒有 SpriteRenderer，直接設置 Image 顏色
                        iconImage.color = bulletColor;
                    }
                }
            }
            
            // 獲取文字組件
            var nameText = item.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            var descText = item.transform.Find("DescText")?.GetComponent<TextMeshProUGUI>();
            
            // 設置初始狀態：背板和 icon 透明，文字為空
            if (backgroundImage != null)
            {
                Color bgColor = backgroundImage.color;
                bgColor.a = 0f;
                backgroundImage.color = bgColor;
            }
            
            if (iconImage != null)
            {
                Color iconColor = iconImage.color;
                iconColor.a = 0f;
                iconImage.color = iconColor;
            }
            
            if (nameText != null)
            {
                nameText.text = "";
                if (isNoneItem)
                {
                    // 如果是"無"，使用灰色
                    nameText.color = Color.grey;
                }
                else if (bulletPrefabReference != null)
                {
                    nameText.color = bulletPrefabReference.GetColorByType(bulletType);
                }
            }
            
            if (descText != null)
            {
                descText.text = "";
            }
            
            // 第一階段：背板和 icon 一起淡入（如果是"無"則不淡入 icon）
            Sequence fadeInSequence = DOTween.Sequence();
            if (backgroundImage != null)
            {
                fadeInSequence.Join(backgroundImage.DOFade(1f, 0.5f));
            }
            if (iconImage != null && !isNoneItem)
            {
                fadeInSequence.Join(iconImage.DOFade(1f, 0.5f));
            }
            yield return fadeInSequence.WaitForCompletion();
            
            // 第二階段：子彈名稱打字機效果
            if (nameText != null && !string.IsNullOrEmpty(attackName))
            {
                yield return StartCoroutine(TypewriterEffect(nameText, attackName, 0.01f));
            }
            
            // 第三階段：子彈描述打字機效果
            if (descText != null && !string.IsNullOrEmpty(description))
            {
                yield return StartCoroutine(TypewriterEffect(descText, description, 0.01f));
            }
        }
        
        /// <summary>
        /// 協程：實時同步 SpriteRenderer 的 Sprite 到 UI Image
        /// </summary>
        private IEnumerator SyncSpriteToImage(SpriteRenderer spriteRenderer, Image image, Color tintColor)
        {
            while (spriteRenderer != null && image != null)
                {
                if (spriteRenderer.sprite != null)
                {
                    image.sprite = spriteRenderer.sprite;
                    image.color = tintColor;
            }
                yield return null; // 每幀更新
            }
        }
        
        /// <summary>
        /// 根據當前語言獲取關卡名稱
        /// </summary>
        private string GetLocalizedStageName(StageDataSO stage)
        {
            if (stage == null) return "";
            
            string languageCode = GetCurrentLanguageCode();
            
            switch (languageCode)
            {
                case "en":
                    return !string.IsNullOrEmpty(stage.stageNameEn) ? stage.stageNameEn : stage.stageName;
                case "ja":
                    return !string.IsNullOrEmpty(stage.stageNameJa) ? stage.stageNameJa : stage.stageName;
                case "zh-TW":
                default:
                    return stage.stageName;
            }
        }
        
        /// <summary>
        /// 根據當前語言獲取關卡描述
        /// </summary>
        private string GetLocalizedDescription(StageDataSO stage)
        {
            if (stage == null) return "";
            
            string languageCode = GetCurrentLanguageCode();
            
            switch (languageCode)
            {
                case "en":
                    return !string.IsNullOrEmpty(stage.descriptionEn) ? stage.descriptionEn : stage.description;
                case "ja":
                    return !string.IsNullOrEmpty(stage.descriptionJa) ? stage.descriptionJa : stage.description;
                case "zh-TW":
                default:
                    return stage.description;
            }
        }
        
        /// <summary>
        /// 根據當前語言獲取Buff名稱
        /// </summary>
        private string GetLocalizedBuffName(BuffDataSO buff)
        {
            if (buff == null) return "";
            
            string languageCode = GetCurrentLanguageCode();
            
            switch (languageCode)
            {
                case "en":
                    return !string.IsNullOrEmpty(buff.buffNameEn) ? buff.buffNameEn : buff.buffName;
                case "ja":
                    return !string.IsNullOrEmpty(buff.buffNameJa) ? buff.buffNameJa : buff.buffName;
                case "zh-TW":
                default:
                    return buff.buffName;
            }
        }
        
        /// <summary>
        /// 根據當前語言獲取Buff描述
        /// </summary>
        private string GetLocalizedBuffDescription(BuffDataSO buff)
        {
            if (buff == null) return "";
            
            string languageCode = GetCurrentLanguageCode();
            
            switch (languageCode)
            {
                case "en":
                    return !string.IsNullOrEmpty(buff.descriptionEn) ? buff.descriptionEn : buff.description;
                case "ja":
                    return !string.IsNullOrEmpty(buff.descriptionJa) ? buff.descriptionJa : buff.description;
                case "zh-TW":
                default:
                    return buff.description;
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
            if (LanguageManager.Instance != null)
            {
                string langCode = LanguageManager.Instance.CurrentLanguageCode;
                if (!string.IsNullOrEmpty(langCode))
                {
                    return langCode;
                }
            }
            
            return "zh-TW"; // 默認繁體中文
        }
    }
}

