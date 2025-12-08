using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Tenronis.Data;
using Tenronis.Core;
using Tenronis.Managers;
using Tenronis.ScriptableObjects;

namespace Tenronis.UI
{
    /// <summary>
    /// Roguelike升級選單
    /// </summary>
    public class RoguelikeMenu : MonoBehaviour
    {
        [Header("Buff選項")]
        [SerializeField] private Transform buffOptionsContainer;
        [SerializeField] private GameObject buffOptionPrefab;
        
        [Header("敵人資訊面板")]
        [SerializeField] private GameObject enemyInfoPanel;
        [SerializeField] private TextMeshProUGUI nextStageEnemyPreviewText;
        [SerializeField] private Transform enemyAttackPreviewContainer;
        [SerializeField] private GameObject enemyAttackPreviewPrefab;
        [SerializeField] private Tenronis.Gameplay.Projectiles.Bullet bulletPrefabReference;
        
        [Header("玩家強化面板")]
        [SerializeField] private GameObject playerBuffPanel;
        [SerializeField] private TextMeshProUGUI currentStatsText;
        [SerializeField] private UnityEngine.UI.Toggle detailToggle; // 顯示詳細資訊的 Toggle
        
        [Header("分頁按鈕")]
        [SerializeField] private Button normalBuffButton;
        [SerializeField] private Button legendaryBuffButton;
        
        private List<GameObject> currentOptions = new List<GameObject>();
        private List<GameObject> attackPreviewItems = new List<GameObject>();
        private List<Coroutine> spriteSyncCoroutines = new List<Coroutine>();
        private bool isLegendaryBuffSelectionPhase = false;
        private bool showDetailedInfo = false; // 是否顯示詳細資訊
        private int currentInfoTab = 0; // 0=敵人資訊, 1=普通強化, 2=傳奇強化
        
        private void OnEnable()
        {
            isLegendaryBuffSelectionPhase = false;
            currentInfoTab = 0; // 0=普通強化, 1=傳奇強化（不再有敵人資訊分頁）
            showDetailedInfo = false; // 默認不顯示詳細資訊
            
            GenerateBuffOptions();
            UpdateCurrentStats();
            UpdateNextStageEnemyPreview();
            
            // 設置分頁按鈕
            SetupTabButtons();
            
            // 設置 Toggle
            SetupDetailToggle();
            
            // 預設顯示普通強化
            ShowInfoTab(0);
        }
        
        private void OnDisable()
        {
            ClearOptions();
            ClearAttackPreviews();
            
            // 移除按鈕監聽
            if (normalBuffButton != null) normalBuffButton.onClick.RemoveAllListeners();
            if (legendaryBuffButton != null) legendaryBuffButton.onClick.RemoveAllListeners();
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
            
            int optionCount = isLegendaryBuffSelectionPhase ? 3 : 2; // 傳奇強化三選一，普通強化二選一
            BuffDataSO[] options = GameManager.Instance.GetRandomBuffOptions(optionCount, isLegendaryBuffSelectionPhase);
            
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
            // 記錄選擇前的狀態（是否有普通強化已滿級）
            bool hadMaxedNormalBuffBefore = PlayerManager.Instance != null && PlayerManager.Instance.HasMaxedNormalBuff();
            
            // 檢查選擇的Buff是否為普通強化
            bool isNormalBuff = System.Array.IndexOf(GameConstants.NORMAL_BUFFS, buffType) >= 0;
            
            // 檢查這是否會是最後一個Buff（在觸發事件前檢查）
            bool isLastBuff = GameManager.Instance.PendingBuffCount <= 1;
            
            // 觸發Buff選擇事件（這會應用Buff效果）
            GameEvents.TriggerBuffSelected(buffType);
            
            // 檢查選擇後的狀態（是否有新的普通強化達到滿級）
            bool isSelectedBuffMaxed = false;
            if (isNormalBuff && PlayerManager.Instance != null)
            {
                isSelectedBuffMaxed = PlayerManager.Instance.IsBuffMaxed(buffType);
            }
            
            // 如果選擇的普通強化使其達到滿級，則提供一次傳奇強化選擇
            bool shouldOfferLegendaryBuff = isNormalBuff && isSelectedBuffMaxed;
            
            // 如果應該提供傳奇強化（Bonus Pick）
            if (shouldOfferLegendaryBuff)
            {
                Debug.Log("[RoguelikeMenu] 普通強化已達滿級，提供額外傳奇強化選擇機會！");
                
                // 標記進入傳奇強化選擇階段
                isLegendaryBuffSelectionPhase = true;
                
                // 增加一次待選次數（因為這是額外的獎勵）
                GameManager.Instance.AddPendingBuffs(1);
                
                // 刷新選項（會顯示傳奇強化）
                GenerateBuffOptions();
                UpdateCurrentStats();
                
                // 保持選單打開，不關閉
                return;
            }
            
            // 如果不是 Bonus Pick，或者 Bonus Pick 已經選完了
            // 重置傳奇強化選擇階段標記（這樣下次刷新就會回到普通池，除非所有普通強化都滿了）
            isLegendaryBuffSelectionPhase = false;
            
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
        private void UpdateNextStageEnemyPreview()
        {
            ClearAttackPreviews();
            
            if (nextStageEnemyPreviewText == null) return;
            if (GameManager.Instance == null) return;
            
            var currentStage = GameManager.Instance.CurrentStage;
            
            // 如果沒有當前關卡
            if (currentStage == null)
            {
                nextStageEnemyPreviewText.text = "";
                return;
            }
            
            // 顯示基本資訊
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"Stage {GameManager.Instance.CurrentStageIndex + 1}: {currentStage.stageName}");
            sb.AppendLine($"HP: {currentStage.maxHp}  |  攻擊間隔: {currentStage.shootInterval}s");
            
            if (currentStage.useSmartTargeting)
            {
                sb.AppendLine("⚠ 智能射擊已啟用");
            }
            
            nextStageEnemyPreviewText.text = sb.ToString();
            
            // 生成視覺攻擊預覽
            GenerateAttackPreviews(currentStage);
        }
        
        /// <summary>
        /// 生成攻擊預覽視覺項目
        /// </summary>
        private void GenerateAttackPreviews(StageDataSO stageData)
        {
            if (enemyAttackPreviewContainer == null || enemyAttackPreviewPrefab == null) return;
            
            // 收集所有啟用的攻擊
            var attacks = new List<(BulletType type, string name, string desc, float chance)>();
            
            if (stageData.normalBullet.enabled)
                attacks.Add((BulletType.Normal, "普通子彈", "造成 1 點方塊傷害", stageData.normalBullet.chance));
            
            if (stageData.areaBullet.enabled)
                attacks.Add((BulletType.AreaDamage, "範圍傷害", "3x3 範圍傷害", stageData.areaBullet.chance));
            
            if (stageData.addBlockBullet.enabled)
                attacks.Add((BulletType.AddBlock, "添加方塊", "在擊中位置上方添加垃圾方塊", stageData.addBlockBullet.chance));
            
            if (stageData.addExplosiveBlockBullet.enabled)
                attacks.Add((BulletType.AddExplosiveBlock, "爆炸方塊", "添加爆炸方塊，被擊中時傷害玩家", stageData.addExplosiveBlockBullet.chance));
            
            if (stageData.addRowBullet.enabled)
                attacks.Add((BulletType.InsertRow, "插入垃圾行", "從底部插入一整行方塊", stageData.addRowBullet.chance));
            
            if (stageData.addVoidRowBullet.enabled)
                attacks.Add((BulletType.InsertVoidRow, "虛無垃圾行", "插入虛無行，消除不產生導彈", stageData.addVoidRowBullet.chance));
            
            if (stageData.corruptExplosiveBullet.enabled)
                attacks.Add((BulletType.CorruptExplosive, "腐化爆炸", "將下個方塊隨機一格變成爆炸方塊", stageData.corruptExplosiveBullet.chance));
            
            if (stageData.corruptVoidBullet.enabled)
                attacks.Add((BulletType.CorruptVoid, "腐化虛無", "將下個方塊隨機一格變成虛無方塊", stageData.corruptVoidBullet.chance));
            
            // 為每個攻擊生成預覽項目
            foreach (var attack in attacks)
            {
                CreateAttackPreviewItem(attack.type, attack.name, attack.desc, attack.chance);
            }
        }
        
        /// <summary>
        /// 創建單個攻擊預覽項目
        /// </summary>
        private void CreateAttackPreviewItem(BulletType bulletType, string attackName, string description, float chance)
        {
            GameObject item = Instantiate(enemyAttackPreviewPrefab, enemyAttackPreviewContainer);
            attackPreviewItems.Add(item);
            
            // 查找 ColorImage 下的 SpriteRenderer 和 Animator
            var colorImageTransform = item.transform.Find("ColorImage");
            if (colorImageTransform != null && bulletPrefabReference != null)
            {
                var spriteRenderer = colorImageTransform.GetComponent<SpriteRenderer>();
                var animator = colorImageTransform.GetComponent<Animator>();
                var image = colorImageTransform.GetComponent<Image>();
                
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
                if (spriteRenderer != null && image != null)
                {
                    var coroutine = StartCoroutine(SyncSpriteToImage(spriteRenderer, image, bulletColor));
                    spriteSyncCoroutines.Add(coroutine);
                }
                else if (image != null)
                {
                    // 如果沒有 SpriteRenderer，直接設置 Image 顏色
                    image.color = bulletColor;
                }
            }
            
            // 設置攻擊名稱
            var nameText = item.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = attackName;
                if (bulletPrefabReference != null)
                {
                    nameText.color = bulletPrefabReference.GetColorByType(bulletType);
                }
            }
            
            // 設置描述文字
            var descText = item.transform.Find("DescText")?.GetComponent<TextMeshProUGUI>();
            if (descText != null)
            {
                descText.text = $"{description} ({chance * 100:F0}%)";
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

