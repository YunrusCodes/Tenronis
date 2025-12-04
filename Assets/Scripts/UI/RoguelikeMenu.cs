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
        
        [Header("普通強化面板")]
        [SerializeField] private GameObject normalBuffPanel;
        [SerializeField] private TextMeshProUGUI currentStatsText;
        
        [Header("傳奇強化面板")]
        [SerializeField] private GameObject legendaryBuffPanel;
        [SerializeField] private TextMeshProUGUI legendaryBuffText;
        
        [Header("分頁按鈕")]
        [SerializeField] private Button enemyInfoButton;
        [SerializeField] private Button normalBuffButton;
        [SerializeField] private Button legendaryBuffButton;
        
        private List<GameObject> currentOptions = new List<GameObject>();
        private List<GameObject> attackPreviewItems = new List<GameObject>();
        private List<Coroutine> spriteSyncCoroutines = new List<Coroutine>();
        private bool isLegendaryBuffSelectionPhase = false;
        private int currentInfoTab = 0; // 0=敵人資訊, 1=普通強化, 2=傳奇強化
        
        private void OnEnable()
        {
            isLegendaryBuffSelectionPhase = false;
            currentInfoTab = 0; // 預設顯示敵人資訊
            
            GenerateBuffOptions();
            UpdateCurrentStats();
            UpdateNextStageEnemyPreview();
            
            // 設置分頁按鈕
            SetupTabButtons();
            
            // 預設顯示敵人資訊
            ShowInfoTab(0);
        }
        
        private void OnDisable()
        {
            ClearOptions();
            ClearAttackPreviews();
            
            // 移除按鈕監聽
            if (enemyInfoButton != null) enemyInfoButton.onClick.RemoveAllListeners();
            if (normalBuffButton != null) normalBuffButton.onClick.RemoveAllListeners();
            if (legendaryBuffButton != null) legendaryBuffButton.onClick.RemoveAllListeners();
        }
        
        /// <summary>
        /// 設置分頁按鈕
        /// </summary>
        private void SetupTabButtons()
        {
            if (enemyInfoButton != null)
            {
                enemyInfoButton.onClick.RemoveAllListeners();
                enemyInfoButton.onClick.AddListener(() => ShowInfoTab(0));
            }
            
            if (normalBuffButton != null)
            {
                normalBuffButton.onClick.RemoveAllListeners();
                normalBuffButton.onClick.AddListener(() => ShowInfoTab(1));
            }
            
            if (legendaryBuffButton != null)
            {
                legendaryBuffButton.onClick.RemoveAllListeners();
                legendaryBuffButton.onClick.AddListener(() => ShowInfoTab(2));
            }
        }
        
        /// <summary>
        /// 顯示指定的分頁
        /// </summary>
        /// <param name="tabIndex">0=敵人資訊, 1=普通強化, 2=傳奇強化</param>
        private void ShowInfoTab(int tabIndex)
        {
            currentInfoTab = tabIndex;
            
            // 顯示/隱藏面板
            if (enemyInfoPanel != null)
                enemyInfoPanel.SetActive(tabIndex == 0);
            
            if (normalBuffPanel != null)
                normalBuffPanel.SetActive(tabIndex == 1);
            
            if (legendaryBuffPanel != null)
                legendaryBuffPanel.SetActive(tabIndex == 2);
            
            // 更新按鈕背景色
            UpdateTabButtonColors();
        }
        
        /// <summary>
        /// 更新分頁按鈕樣式（選中：黃底黑字，未選中：黑底白字）
        /// </summary>
        private void UpdateTabButtonColors()
        {
            SetButtonStyle(enemyInfoButton, currentInfoTab == 0);
            SetButtonStyle(normalBuffButton, currentInfoTab == 1);
            SetButtonStyle(legendaryBuffButton, currentInfoTab == 2);
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
                    titleText.text = $"⭐ {buffData.buffName} ⭐"; // 傳奇強化標記
                    titleText.color = new Color(1f, 0.84f, 0f); // 金色
                }
                else
                {
                    titleText.text = buffData.buffName;
                    titleText.color = Color.white;
                }
            }
            
            // 描述
            var descText = optionObj.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
            if (descText != null)
            {
                if (isLegendary)
                {
                    descText.text = $"[傳奇強化]\n{buffData.description}";
                    descText.color = new Color(1f, 0.84f, 0f); // 金色
                }
                else
                {
                    descText.text = buffData.description;
                    descText.color = Color.white;
                }
            }
            
            // 圖示
            var iconImage = optionObj.transform.Find("Icon")?.GetComponent<Image>();
            if (iconImage != null && buffData.icon != null)
            {
                iconImage.sprite = buffData.icon;
                iconImage.color = buffData.iconColor;
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
            
            var stats = PlayerManager.Instance.Stats;
            
            // 計算最大值
            int maxSalvoPercent = GameConstants.SALVO_MAX_LEVEL * 50;
            int maxBurstPercent = GameConstants.BURST_MAX_LEVEL * 25;
            int maxCounter = GameConstants.COUNTER_MAX_LEVEL;
            int maxCp = GameConstants.PLAYER_MAX_CP + GameConstants.RESOURCE_EXPANSION_MAX_LEVEL * 50;
            int maxExplosionCharge = GameConstants.EXPLOSION_INITIAL_MAX_CHARGE + GameConstants.EXPLOSION_BUFF_MAX_LEVEL * GameConstants.EXPLOSION_BUFF_MAX_CHARGE_INCREASE;
            int maxSpace = GameConstants.SPACE_EXPANSION_MAX_LEVEL;
            
            // 更新普通強化
            if (currentStatsText != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("普通增益強化項目");
                sb.AppendLine("<size=80%>(達到上限獲得一次傳奇強化)</size>");
                sb.AppendLine();
                
                sb.AppendLine($"齊射 : {stats.salvoLevel * 50}%/列");
                sb.AppendLine($"<size=80%>(上限 {maxSalvoPercent}%/列)</size>");
                sb.AppendLine();
                
                sb.AppendLine($"連發 : {stats.burstLevel * 25}%/發");
                sb.AppendLine($"<size=80%>(上限 {maxBurstPercent}%/發)</size>");
                sb.AppendLine();
                
                sb.AppendLine($"反擊 : {stats.counterFireLevel}發子彈");
                sb.AppendLine($"<size=80%>(上限 {maxCounter}發子彈)</size>");
                sb.AppendLine();
                
                sb.AppendLine($"CP : {stats.maxCp}");
                sb.AppendLine($"<size=80%>(上限 {maxCp})</size>");
                sb.AppendLine();
                
                sb.AppendLine($"衝擊充能上限 : {stats.explosionMaxCharge}");
                sb.AppendLine($"<size=80%>(上限 {maxExplosionCharge})</size>");
                sb.AppendLine($"<size=80%>網格溢位時消耗 {GameConstants.OVERFLOW_CP_COST} CP，累積充能對敵人造成傷害 <color=red>若 CP 不足，HP 歸 1</color></size>");
                sb.AppendLine();
                
                sb.AppendLine($"空間 : {stats.spaceExpansionLevel}");
                sb.AppendLine($"<size=80%>(上限 {maxSpace})</size>");
                sb.AppendLine("<size=80%>可儲存的方塊槽位數量</size>");
                
                currentStatsText.text = sb.ToString();
            }
            
            // 更新傳奇強化
            if (legendaryBuffText != null)
            {
                System.Text.StringBuilder legendarySb = new System.Text.StringBuilder();
                legendarySb.AppendLine("傳奇增益強化項目");
                legendarySb.AppendLine();
                
                legendarySb.AppendLine($"方塊可承受子彈次數 : {stats.blockDefenseLevel}");
                legendarySb.AppendLine();
                
                legendarySb.AppendLine($"每個方塊可產生的子彈 : {1 + stats.missileExtraCount}");
                legendarySb.AppendLine();
                
                // 湮滅技能
                string annihilationStatus = PlayerManager.Instance.IsAnnihilationUnlocked() ? "按鍵1" : "未解鎖";
                legendarySb.AppendLine($"湮滅 ({annihilationStatus}) - 消耗 {GameConstants.ANNIHILATION_CP_COST} CP");
                legendarySb.AppendLine("<size=80%>進入幽靈穿透狀態，硬降時破壞重疊方塊並發射導彈</size>");
                legendarySb.AppendLine();
                
                // 處決技能
                string executionStatus = PlayerManager.Instance.IsExecutionUnlocked() ? "按鍵2" : "未解鎖";
                legendarySb.AppendLine($"處決 ({executionStatus}) - 消耗 {GameConstants.EXECUTION_CP_COST} CP");
                legendarySb.AppendLine("<size=80%>清除每列最上方的方塊並發射導彈</size>");
                legendarySb.AppendLine();
                
                // 修補技能
                string repairStatus = PlayerManager.Instance.IsRepairUnlocked() ? "按鍵3" : "未解鎖";
                legendarySb.AppendLine($"修補 ({repairStatus}) - 消耗 {GameConstants.REPAIR_CP_COST} CP");
                legendarySb.AppendLine("<size=80%>填補封閉空洞並檢查消除</size>");
                
                legendaryBuffText.text = legendarySb.ToString();
            }
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

