using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Tenronis.Data;
using Tenronis.Core;
using Tenronis.Managers;
using Tenronis.ScriptableObjects;
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
        
        [Header("Stage 顯示動畫")]
        [SerializeField] private TextMeshProUGUI stageText; // Stage 文字（場景中新增的，顯示 "Stage n 怪物名稱"）
        [SerializeField] private Transform stageTextContainer; // 包含 Stage 文字的容器（用於整體移動）
        [SerializeField] private AudioClip stageCharSound; // Stage 字符動畫音效（可選）
        
        [Header("玩家強化面板")]
        [SerializeField] private GameObject playerBuffPanel;
        [SerializeField] private TextMeshProUGUI currentStatsText;
        [SerializeField] private UnityEngine.UI.Toggle detailToggle; // 顯示詳細資訊的 Toggle
        
        [Header("分頁按鈕")]
        [SerializeField] private Button normalBuffButton;
        [SerializeField] private Button legendaryBuffButton;
        
        [Header("傳奇強化說明頁面")]
        [SerializeField] private GameObject bonusPanel;
        [SerializeField] private RawImage bonusVideoImage; // 從 Image 改成 RawImage
        [SerializeField] private VideoPlayer bonusVideoPlayer; // VideoPlayer 組件
        [SerializeField] private UnityEngine.UI.AspectRatioFitter bonusAspectRatioFitter; // AspectRatioFilter 組件
        [SerializeField] private TextMeshProUGUI bonusText;
        [SerializeField] private TextMeshProUGUI bonusLevelText; // 顯示等級變化（例如：戰術擴張 Lv0 -> Lv1）
        [SerializeField] private Button bonusConfirmButton;
        
        [Header("說明影片資源")]
        [SerializeField] private VideoClip defenseVideo; // 防禦強化.mp4
        [SerializeField] private VideoClip[] volleyVideos = new VideoClip[5]; // 火力強化1-5.mp4
        [SerializeField] private VideoClip annihilationVideo; // 湮滅.mp4
        [SerializeField] private VideoClip executionVideo; // 處決.mp4
        [SerializeField] private VideoClip repairVideo; // 修補.mp4
        
        private List<GameObject> currentOptions = new List<GameObject>();
        private List<GameObject> attackPreviewItems = new List<GameObject>();
        private List<Coroutine> spriteSyncCoroutines = new List<Coroutine>();
        private List<GameObject> bossBattleCharObjects = new List<GameObject>(); // Boss Battle 字符對象列表
        private List<GameObject> stageCharObjects = new List<GameObject>(); // Stage 字符對象列表
        private bool showDetailedInfo = false; // 是否顯示詳細資訊
        private int currentInfoTab = 0; // 0=敵人資訊, 1=普通強化, 2=傳奇強化
        private bool waitingForBonusConfirm = false; // 是否正在等待說明頁面確認
        
        private void OnEnable()
        {
            currentInfoTab = 0; // 0=普通強化, 1=傳奇強化（不再有敵人資訊分頁）
            showDetailedInfo = false; // 默認不顯示詳細資訊
            waitingForBonusConfirm = false;
            
            // 確保說明頁面初始為關閉狀態
            if (bonusPanel != null)
                bonusPanel.SetActive(false);
            
            GenerateBuffOptions();
            UpdateCurrentStats();
            StartCoroutine(UpdateNextStageEnemyPreviewCoroutine());
            
            // 設置分頁按鈕
            SetupTabButtons();
            
            // 設置 Toggle
            SetupDetailToggle();
            
            // 設置說明頁面確認按鈕
            SetupBonusConfirmButton();
            
            // 設置關閉敵人面板按鈕
            SetupCloseEnemyPanelButton();
            
            // 預設顯示普通強化
            ShowInfoTab(0);
        }
        
        private void OnDisable()
        {
            ClearOptions();
            ClearAttackPreviews();
            ClearBossBattleChars();
            ClearStageChars();
            
            // 停止所有 DOTween 動畫
            if (bossBattleText != null)
            {
                bossBattleText.DOKill();
            }
            if (bossBattleTextContainer != null)
            {
                bossBattleTextContainer.DOKill();
            }
            if (stageText != null)
            {
                stageText.DOKill();
            }
            if (stageTextContainer != null)
            {
                stageTextContainer.DOKill();
            }
            
            // 移除按鈕監聽
            if (normalBuffButton != null) normalBuffButton.onClick.RemoveAllListeners();
            if (legendaryBuffButton != null) legendaryBuffButton.onClick.RemoveAllListeners();
            if (bonusConfirmButton != null) bonusConfirmButton.onClick.RemoveAllListeners();
            if (closeEnemyPanelButton != null) closeEnemyPanelButton.onClick.RemoveAllListeners();
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
        /// 設置分頁按鈕
        /// </summary>
        private void SetupTabButtons()
        {
            if (normalBuffButton != null)
            {
                normalBuffButton.onClick.RemoveAllListeners();
                normalBuffButton.onClick.AddListener(() => ShowInfoTab(0));
            }
            
            if (legendaryBuffButton != null)
            {
                legendaryBuffButton.onClick.RemoveAllListeners();
                legendaryBuffButton.onClick.AddListener(() => ShowInfoTab(1));
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
        /// 顯示指定的分頁
        /// </summary>
        /// <param name="tabIndex">0=普通強化, 1=傳奇強化</param>
        private void ShowInfoTab(int tabIndex)
        {
            currentInfoTab = tabIndex;
            
            // 敵人資訊面板固定顯示在左側
            if (enemyInfoPanel != null)
                enemyInfoPanel.SetActive(true);
            
            // 右側玩家強化面板固定顯示
            if (playerBuffPanel != null)
                playerBuffPanel.SetActive(true);
            
            // 根據分頁控制 Toggle 顯示/隱藏
            if (detailToggle != null)
            {
                // 普通強化頁面顯示 Toggle，傳奇強化頁面隱藏 Toggle
                detailToggle.gameObject.SetActive(tabIndex == 0);
            }
            
            // 更新按鈕背景色和內容
            UpdateTabButtonColors();
            UpdateCurrentStats(); // 根據當前分頁更新內容
        }
        
        /// <summary>
        /// 更新分頁按鈕樣式（選中：黃底黑字，未選中：黑底白字）
        /// </summary>
        private void UpdateTabButtonColors()
        {
            // 只更新普通和傳奇按鈕（敵人資訊按鈕已隱藏）
            SetButtonStyle(normalBuffButton, currentInfoTab == 0);
            SetButtonStyle(legendaryBuffButton, currentInfoTab == 1);
        }
        
        /// <summary>
        /// 設置按鈕樣式
        /// </summary>
        private void SetButtonStyle(Button button, bool isSelected)
        {
            if (button == null) return;
            
            // 設置背景色
            Color bgColor = isSelected ? Color.yellow : Color.black;
            var colors = button.colors;
            colors.normalColor = bgColor;
            colors.highlightedColor = bgColor;
            colors.selectedColor = bgColor;
            button.colors = colors;
            
            // 設置文字顏色
            Color textColor = isSelected ? Color.black : Color.white;
            var text = button.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.color = textColor;
            }
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
            
            if (GameManager.Instance == null) return;
            
            // 普通強化固定為二選一（不再有傳奇強化選擇階段）
            int optionCount = 2;
            BuffDataSO[] options = GameManager.Instance.GetRandomBuffOptions(optionCount, false);
            
            foreach (var buffData in options)
            {
                if (buffData == null) continue;
                
                GameObject optionObj = Instantiate(buffOptionPrefab, buffOptionsContainer);
                currentOptions.Add(optionObj);
                
                // 設置UI
                SetupBuffOption(optionObj, buffData);
            }
        }
        
        /// <summary>
        /// 設置Buff選項UI
        /// </summary>
        private void SetupBuffOption(GameObject optionObj, BuffDataSO buffData)
        {
            // 檢查是否為傳奇強化
            bool isLegendary = System.Array.IndexOf(GameConstants.LEGENDARY_BUFFS, buffData.buffType) >= 0;
            
            // 標題
            var titleText = optionObj.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
            if (titleText != null)
            {
                if (isLegendary)
                {
                    titleText.text = $"{buffData.buffName}"; // 傳奇強化標記
                }
                else
                {
                    titleText.text = buffData.buffName;
                }
            }
            
            // 描述
            var descText = optionObj.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
            if (descText != null)
            {
                if (isLegendary)
                {
                    descText.text = $"[傳奇強化]\n{buffData.description}";
                }
                else
                {
                    descText.text = buffData.description;
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
        /// 清除選項
        /// </summary>
        private void ClearOptions()
        {
            foreach (var option in currentOptions)
            {
                if (option != null)
                    Destroy(option);
            }
            currentOptions.Clear();
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
                        ShowBonusPanel(legendaryBuff, volleyLevelBefore, defenseLevelBefore, tacticalExpansionLevelBefore, 
                                      volleyUpgraded, defenseUpgraded, tacticalExpansionUpgraded, 
                                      unlockedAnnihilationAfter, unlockedExecutionAfter, unlockedRepairAfter);
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
                
                ShowBonusPanel(null, 0, 0, tacticalExpansionLevelBeforeSkill, false, false, tacticalExpansionUpgradedSkill, unlockedAnnihilation, unlockedExecution, unlockedRepair);
                waitingForBonusConfirm = true;
                return; // 等待確認按鈕，不繼續下一步
            }
            
            // 如果沒有需要顯示說明頁面的情況，繼續正常流程
            ContinueAfterBuffSelection();
        }
        
        /// <summary>
        /// 顯示傳奇強化說明頁面
        /// </summary>
        private void ShowBonusPanel(BuffDataSO legendaryBuff, int volleyLevelBefore, int defenseLevelBefore, int tacticalExpansionLevelBefore,
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
            
            // 設置文字和影片
            string descriptionText = "";
            VideoClip videoClip = null;
            
            if (volleyUpgraded && PlayerManager.Instance != null)
            {
                int volleyLevel = PlayerManager.Instance.Stats.missileExtraCount;
                descriptionText = $"現在消除方塊，會額外生成 {1 + volleyLevel} 發飛彈";
                
                // 根據等級選擇對應的影片（等級 1-5 對應索引 0-4）
                int videoIndex = Mathf.Clamp(volleyLevel - 1, 0, volleyVideos.Length - 1);
                if (videoIndex >= 0 && videoIndex < volleyVideos.Length && volleyVideos[videoIndex] != null)
                {
                    videoClip = volleyVideos[videoIndex];
                }
            }
            else if (defenseUpgraded && PlayerManager.Instance != null)
            {
                int defenseLevel = PlayerManager.Instance.Stats.blockDefenseLevel;
                descriptionText = $"方塊現在可以承受 {1 + defenseLevel} 發飛彈";
                
                if (defenseVideo != null)
                {
                    videoClip = defenseVideo;
                }
            }
            else if (unlockedAnnihilation)
            {
                descriptionText = "按 1 將當前方塊轉化為可穿透已存在方塊的湮滅方塊。按 空白鍵 將摧毀所有與湮滅方塊重疊的方塊，並生成飛彈。";
                
                if (annihilationVideo != null)
                {
                    videoClip = annihilationVideo;
                }
            }
            else if (unlockedExecution)
            {
                descriptionText = "按 2 消除每一行最上方的方塊，並生成飛彈。";
                
                if (executionVideo != null)
                {
                    videoClip = executionVideo;
                }
            }
            else if (unlockedRepair)
            {
                descriptionText = "按 3 將所有封閉區域填充為方塊。若形成整列，則視為有效消除。消除虛無方塊時不會產生任何飛彈。";
                
                if (repairVideo != null)
                {
                    videoClip = repairVideo;
                }
            }
            
            // 設置影片
            if (bonusVideoPlayer != null && bonusVideoImage != null)
            {
                if (videoClip != null)
                {
                    // 設置 VideoPlayer
                    bonusVideoPlayer.clip = videoClip;
                    bonusVideoPlayer.renderMode = VideoRenderMode.RenderTexture;
                    
                    // 獲取影片的寬高比
                    uint videoWidth = videoClip.width;
                    uint videoHeight = videoClip.height;
                    float aspectRatio = (float)videoWidth / (float)videoHeight;
                    
                    // 根據影片寬高比調整 AspectRatioFilter
                    if (bonusAspectRatioFitter != null)
                    {
                        bonusAspectRatioFitter.aspectRatio = aspectRatio;
                        bonusAspectRatioFitter.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent;
                    }
                    
                    // 創建 RenderTexture（根據影片實際尺寸）
                    int videoWidthInt = (int)videoWidth;
                    int videoHeightInt = (int)videoHeight;
                    if (bonusVideoPlayer.targetTexture == null || 
                        bonusVideoPlayer.targetTexture.width != videoWidthInt || 
                        bonusVideoPlayer.targetTexture.height != videoHeightInt)
                    {
                        // 釋放舊的 RenderTexture（如果存在）
                        if (bonusVideoPlayer.targetTexture != null)
                        {
                            bonusVideoPlayer.targetTexture.Release();
                            Destroy(bonusVideoPlayer.targetTexture);
                        }
                        
                        // 創建新的 RenderTexture（使用影片的實際尺寸）
                        RenderTexture renderTexture = new RenderTexture(videoWidthInt, videoHeightInt, 0);
                        bonusVideoPlayer.targetTexture = renderTexture;
                    }
                    
                    // 設置 RawImage 的 texture
                    bonusVideoImage.texture = bonusVideoPlayer.targetTexture;
                    bonusVideoImage.gameObject.SetActive(true);
                    
                    // 播放影片
                    bonusVideoPlayer.Play();
                }
                else
                {
                    // 如果沒有影片，隱藏 RawImage
                    bonusVideoImage.gameObject.SetActive(false);
                    bonusVideoPlayer.Stop();
                    
                    // 重置 AspectRatioFilter（可選）
                    if (bonusAspectRatioFitter != null)
                    {
                        bonusAspectRatioFitter.aspectRatio = 1f; // 重置為 1:1
                    }
                }
            }
            
            // 設置文字
            if (bonusText != null)
            {
                bonusText.text = descriptionText;
            }
            
            // 設置等級變化文字
            if (bonusLevelText != null)
            {
                string levelChangeText = "";
                
                if (tacticalExpansionUpgraded && PlayerManager.Instance != null)
                {
                    int tacticalExpansionLevel = PlayerManager.Instance.Stats.tacticalExpansionLevel;
                    levelChangeText = $"戰術擴張\n Lv{tacticalExpansionLevelBefore} -> Lv{tacticalExpansionLevel}";
                }
                else if (unlockedAnnihilation || unlockedExecution || unlockedRepair)
                {
                    // 如果解鎖了技能，顯示 TacticalExpansion 的等級變化
                    if (PlayerManager.Instance != null)
                    {
                        int tacticalExpansionLevel = PlayerManager.Instance.Stats.tacticalExpansionLevel;
                        levelChangeText = $"戰術擴張\n Lv{tacticalExpansionLevelBefore} -> Lv{tacticalExpansionLevel}";
                    }
                }
                else if (defenseUpgraded && PlayerManager.Instance != null)
                {
                    int defenseLevel = PlayerManager.Instance.Stats.blockDefenseLevel;
                    levelChangeText = $"鞏固防禦\n Lv{defenseLevelBefore} -> Lv{defenseLevel}";
                }
                else if (volleyUpgraded && PlayerManager.Instance != null)
                {
                    int volleyLevel = PlayerManager.Instance.Stats.missileExtraCount;
                    levelChangeText = $"加倍火力\n Lv{volleyLevelBefore} -> Lv{volleyLevel}";
                }
                
                bonusLevelText.text = levelChangeText;
            }
            
            // 顯示說明頁面
            bonusPanel.SetActive(true);
        }
        
        /// <summary>
        /// 說明頁面確認按鈕點擊
        /// </summary>
        private void OnBonusConfirmClicked()
        {
            // 停止影片播放
            if (bonusVideoPlayer != null)
            {
                bonusVideoPlayer.Stop();
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
            if (currentStatsText == null) return;
            
            var stats = PlayerManager.Instance.Stats;
            
            // 檢查 stats 是否已初始化（避免在 Start() 之前調用時出錯）
            if (stats == null)
            {
                Debug.LogWarning("[RoguelikeMenu] PlayerStats is null, skipping update");
                return;
            }
            
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            // 根據當前分頁顯示不同內容
            if (currentInfoTab == 0) // 普通強化
            {
                sb.AppendLine("普通增益強化項目");
                sb.AppendLine("<size=80%>(達到上限獲得一次傳奇強化)</size>");
                sb.AppendLine();
                
                // 升級進度提示區域
                sb.AppendLine("<b><i><u><color=black>升滿一個能力以獲得傳奇獎勵</color></u></i></b>");
                sb.AppendLine();
                
                // 齊射強化進度條（簡化版）
                string salvoProgress = GetSimpleProgressBar(stats.salvoLevel, GameConstants.SALVO_MAX_LEVEL);
                sb.AppendLine($"齊射強化 {salvoProgress}");
                if (showDetailedInfo)
                {
                    int maxSalvoPercent = GameConstants.SALVO_MAX_LEVEL * 50;
                    sb.AppendLine($"  當前: {stats.salvoLevel * 50}%/列 (上限 {maxSalvoPercent}%/列)");
                }
                sb.AppendLine();
                
                // 連發強化進度條（簡化版）
                string burstProgress = GetSimpleProgressBar(stats.burstLevel, GameConstants.BURST_MAX_LEVEL);
                sb.AppendLine($"連發強化 {burstProgress}");
                if (showDetailedInfo)
                {
                    int maxBurstPercent = GameConstants.BURST_MAX_LEVEL * 25;
                    sb.AppendLine($"  當前: {stats.burstLevel * 25}%/發 (上限 {maxBurstPercent}%/發)");
                }
                sb.AppendLine();
                
                // 反擊強化進度條（簡化版）
                string counterProgress = GetSimpleProgressBar(stats.counterFireLevel, GameConstants.COUNTER_MAX_LEVEL);
                sb.AppendLine($"反擊強化 {counterProgress}");
                if (showDetailedInfo)
                {
                    sb.AppendLine($"  當前: {stats.counterFireLevel}發子彈 (上限 {GameConstants.COUNTER_MAX_LEVEL}發子彈)");
                    sb.AppendLine("  <size=80%>方塊在生成後的0.2秒內受到傷害會觸發反擊</size>");
                }
                sb.AppendLine();
                
                // 爆炸充能進度條（簡化版）
                string explosionProgress = GetSimpleProgressBar(stats.explosionChargeLevel, GameConstants.EXPLOSION_BUFF_MAX_LEVEL);
                sb.AppendLine($"衝擊擴充 {explosionProgress}");
                if (showDetailedInfo)
                {
                    int maxExplosionCharge = GameConstants.EXPLOSION_BUFF_MAX_LEVEL * GameConstants.EXPLOSION_BUFF_MAX_CHARGE_INCREASE;
                    sb.AppendLine($"  當前上限: {stats.explosionMaxCharge} (最大 {maxExplosionCharge})");
                    sb.AppendLine($"  <size=80%>網格溢位時消耗 {GameConstants.OVERFLOW_CP_COST} CP，釋放衝擊炮累積的充能對敵人造成傷害</size>");
                    sb.AppendLine($"  <size=80%><color=red>若 CP 不足，自身HP 歸 1</color></size>");
                }
                sb.AppendLine();
                
                // 資源擴充進度條（簡化版）
                string cpProgress = GetSimpleProgressBar(stats.cpExpansionLevel, GameConstants.RESOURCE_EXPANSION_MAX_LEVEL);
                sb.AppendLine($"資源擴充 {cpProgress}");
                if (showDetailedInfo)
                {
                    int maxCp = GameConstants.PLAYER_MAX_CP + GameConstants.RESOURCE_EXPANSION_MAX_LEVEL * 50;
                    sb.AppendLine($"  當前 CP: {stats.maxCp} (上限 {maxCp})");
                    sb.AppendLine("  <size=80%>釋放衝擊炮和使用主動技能需消耗CP</size>");
                }
                sb.AppendLine();
                
                // 空間擴充進度條（簡化版）
                string spaceProgress = GetSimpleProgressBar(stats.spaceExpansionLevel, GameConstants.SPACE_EXPANSION_MAX_LEVEL);
                sb.AppendLine($"空間擴充 {spaceProgress}");
                if (showDetailedInfo)
                {
                    sb.AppendLine($"  當前槽位: {stats.spaceExpansionLevel} (上限 {GameConstants.SPACE_EXPANSION_MAX_LEVEL})");
                    sb.AppendLine("  <size=80%>可儲存的方塊槽位數量</size>");
                }
            }
            else // currentInfoTab == 1，傳奇強化
            {
                sb.AppendLine("傳奇增益強化項目");
                sb.AppendLine();
                
                sb.AppendLine($"方塊可承受子彈次數 : {stats.blockDefenseLevel}");
                sb.AppendLine();
                
                sb.AppendLine($"導彈傷害倍率 : x{1 + stats.missileExtraCount}");
                sb.AppendLine();
                
                // 湮滅技能
                sb.AppendLine($"湮滅 (按鍵1) - 消耗 {GameConstants.ANNIHILATION_CP_COST} CP");
                sb.AppendLine("<size=80%>進入幽靈穿透狀態，硬降時破壞重疊方塊並發射導彈</size>");
                if (!PlayerManager.Instance.IsAnnihilationUnlocked())
                {
                    sb.AppendLine("<color=red>(無法使用，獲得一次戰術擴張以解鎖)</color>");
                }
                sb.AppendLine();
                
                // 處決技能
                sb.AppendLine($"處決 (按鍵2) - 消耗 {GameConstants.EXECUTION_CP_COST} CP");
                sb.AppendLine("<size=80%>清除每列最上方的方塊並發射導彈</size>");
                if (!PlayerManager.Instance.IsExecutionUnlocked())
                {
                    sb.AppendLine("<color=red>(無法使用，獲得兩次戰術擴張以解鎖)</color>");
                }
                sb.AppendLine();
                
                // 修補技能
                sb.AppendLine($"修補 (按鍵3) - 消耗 {GameConstants.REPAIR_CP_COST} CP");
                sb.AppendLine("<size=80%>填補封閉空洞並檢查消除</size>");
                if (!PlayerManager.Instance.IsRepairUnlocked())
                {
                    sb.AppendLine("<color=red>(無法使用，獲得三次戰術擴張以解鎖)</color>");
                }
            }
            
            currentStatsText.text = sb.ToString();
        }
        
        /// <summary>
        /// 生成簡化版進度條字符串（不帶等級標記）
        /// </summary>
        private string GetSimpleProgressBar(int current, int max)
        {
            System.Text.StringBuilder progress = new System.Text.StringBuilder();
            
            for (int i = 0; i < max; i++)
            {
                if (i < current)
                {
                    progress.Append("<color=yellow>■</color> ");
                }
                else
                {
                    progress.Append("<color=grey>□</color> ");
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
            string previewText = $"HP: {currentStage.maxHp}  |  攻擊間隔: {currentStage.shootInterval}s";
            
            // 先清空文字，等動畫完成後再顯示
            if (nextStageEnemyPreviewText != null)
            {
                nextStageEnemyPreviewText.text = "";
            }
            
            // 關卡描述（先不顯示，等子彈實例化完成後用打字機效果顯示）
            string descriptionText = "";
            if (previewDescriptionText != null)
            {
                if (!string.IsNullOrEmpty(currentStage.description))
                {
                    descriptionText = currentStage.description;
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
            
            // 獲取文字內容（如果為空，使用默認值）
            string text = bossBattleText.text;
            if (string.IsNullOrEmpty(text))
            {
                text = "Boss Battle";
            }
            
            // 先設置文字並強制更新，以獲取字符位置信息
            bossBattleText.text = text;
            bossBattleText.ForceMeshUpdate();
            
            // 獲取容器（如果沒有指定，使用文字本身的 Transform）
            Transform container = bossBattleTextContainer != null ? bossBattleTextContainer : bossBattleText.transform;
            Vector3 originalContainerPosition = container.localPosition;
            
            // 設置容器初始位置（從左側屏幕外 -2000，只改變 X 軸）
            // 確保 X 軸明確為 -2000，Y 和 Z 保持原始值
            Vector3 startPosition = originalContainerPosition;
            container.localPosition = startPosition;
            
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
                charText.text = text[i].ToString();
                
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
                charObj.transform.localPosition = charCenterContainerLocal;
                
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
            
            // 第一階段：容器從 -2000 移動到 0（中心位置，只改變 X 軸）
            Vector3 centerPosition = new Vector3(0f, originalContainerPosition.y, originalContainerPosition.z);
            float containerMoveInDuration = 1f;
            mainSequence.Append(container.DOLocalMove(centerPosition, containerMoveInDuration));
            
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
            if (mainSequence.Duration() < containerMoveInDuration + totalCharAnimationTime)
            {
                mainSequence.AppendInterval((containerMoveInDuration + totalCharAnimationTime) - mainSequence.Duration());
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
                container.localPosition = originalContainerPosition;
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
            
            // 構建文字內容："Stage n 怪物名稱"
            string text = $"Stage {stageIndex + 1} {stage.stageName}";
            
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
                enabledBullets.Add((BulletType.Normal, "普通子彈", "對命中的方塊造成傷害", stageData.normalBullet.chance));
            
            if (stageData.areaBullet.enabled)
                enabledBullets.Add((BulletType.AreaDamage, "範圍傷害", "以命中的方塊為中心，對 3x3 範圍內的方塊造成傷害", stageData.areaBullet.chance));
            
            if (stageData.addBlockBullet.enabled)
                enabledBullets.Add((BulletType.AddBlock, "添加方塊", "對命中的方塊造成傷害，並上方添加垃圾方塊", stageData.addBlockBullet.chance));
            
            if (stageData.addExplosiveBlockBullet.enabled)
                enabledBullets.Add((BulletType.AddExplosiveBlock, "爆炸方塊", "對命中的方塊造成傷害，並上方添加爆炸方塊", stageData.addExplosiveBlockBullet.chance));
            
            if (stageData.addRowBullet.enabled)
                enabledBullets.Add((BulletType.InsertRow, "插入垃圾行", "對命中的方塊造成傷害，並從底部插入一整行垃圾方塊", stageData.addRowBullet.chance));
            
            if (stageData.addVoidRowBullet.enabled)
                enabledBullets.Add((BulletType.InsertVoidRow, "虛無垃圾行", "對命中的方塊造成傷害，並從底部插入一整行虛無方塊", stageData.addVoidRowBullet.chance));
            
            if (stageData.corruptExplosiveBullet.enabled)
                enabledBullets.Add((BulletType.CorruptExplosive, "腐化爆炸", "對命中的方塊造成傷害，並將下個方塊隨機一格變成爆炸方塊", stageData.corruptExplosiveBullet.chance));
            
            if (stageData.corruptVoidBullet.enabled)
                enabledBullets.Add((BulletType.CorruptVoid, "腐化虛無", "對命中的方塊造成傷害，並將下個方塊隨機一格變成虛無方塊", stageData.corruptVoidBullet.chance));
            
            // 如果沒有任何啟用的子彈，不顯示任何預覽
            if (enabledBullets.Count == 0)
            {
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
            foreach (var bullet in bullets)
            {
                float actualChance = bullet.weight / totalWeight;
                yield return StartCoroutine(CreateAttackPreviewItem(bullet.type, bullet.name, bullet.desc, actualChance));
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
        private IEnumerator CreateAttackPreviewItem(BulletType bulletType, string attackName, string description, float chance)
        {
            GameObject item = Instantiate(enemyAttackPreviewPrefab, enemyAttackPreviewContainer);
            attackPreviewItems.Add(item);
            
            // 獲取背板 Image（在根 GameObject 上）
            Image backgroundImage = item.GetComponent<Image>();
            
            // 查找 ColorImage 下的 SpriteRenderer 和 Animator
            var colorImageTransform = item.transform.Find("ColorImage");
            Image iconImage = null;
            
            if (colorImageTransform != null && bulletPrefabReference != null)
            {
                var spriteRenderer = colorImageTransform.GetComponent<SpriteRenderer>();
                var animator = colorImageTransform.GetComponent<Animator>();
                iconImage = colorImageTransform.GetComponent<Image>();
                
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
                if (bulletPrefabReference != null)
                {
                    nameText.color = bulletPrefabReference.GetColorByType(bulletType);
                }
            }
            
            if (descText != null)
            {
                descText.text = "";
            }
            
            // 第一階段：背板和 icon 一起淡入
            Sequence fadeInSequence = DOTween.Sequence();
            if (backgroundImage != null)
            {
                fadeInSequence.Join(backgroundImage.DOFade(1f, 0.5f));
            }
            if (iconImage != null)
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
    }
}

