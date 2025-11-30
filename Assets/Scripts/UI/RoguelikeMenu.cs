using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
        
        [Header("當前強化狀態")]
        [SerializeField] private TextMeshProUGUI currentStatsText;
        [SerializeField] private TextMeshProUGUI legendaryBuffText;
        
        private List<GameObject> currentOptions = new List<GameObject>();
        private bool isLegendaryBuffSelectionPhase = false; // 標記是否處於傳奇強化選擇階段
        
        private void OnEnable()
        {
            isLegendaryBuffSelectionPhase = false; // 重置標記
            GenerateBuffOptions();
            UpdateCurrentStats();
        }
        
        private void OnDisable()
        {
            ClearOptions();
        }
        
        /// <summary>
        /// 生成Buff選項
        /// </summary>
        private void GenerateBuffOptions()
        {
            ClearOptions();
            
            if (GameManager.Instance == null) return;
            
            BuffDataSO[] options = GameManager.Instance.GetRandomBuffOptions(3);
            
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
            bool hasMaxedNormalBuffAfter = PlayerManager.Instance != null && PlayerManager.Instance.HasMaxedNormalBuff();
            
            // 如果選擇的普通強化使其達到滿級，且之前沒有普通強化滿級，則提供一次傳奇強化選擇
            bool shouldOfferLegendaryBuff = isNormalBuff && 
                                            !hadMaxedNormalBuffBefore && 
                                            hasMaxedNormalBuffAfter;
            
            // 如果這是最後一個Buff，且應該提供傳奇強化，繼續顯示選單
            if (isLastBuff && shouldOfferLegendaryBuff)
            {
                Debug.Log("[RoguelikeMenu] 普通強化已達滿級，提供傳奇強化選擇機會！");
                
                // 標記進入傳奇強化選擇階段
                isLegendaryBuffSelectionPhase = true;
                
                // 刷新選項（會顯示傳奇強化）
                GenerateBuffOptions();
                UpdateCurrentStats();
                
                // 保持選單打開，不關閉
                return;
            }
            
            // 檢查是否還有待選Buff
            if (GameManager.Instance.PendingBuffCount > 0)
            {
                // 刷新選項
                GenerateBuffOptions();
                UpdateCurrentStats();
            }
            else
            {
                // 如果選擇的是傳奇強化，且這是傳奇強化選擇階段，則恢復遊戲狀態
                bool isLegendaryBuff = System.Array.IndexOf(GameConstants.LEGENDARY_BUFFS, buffType) >= 0;
                if (isLegendaryBuff && isLegendaryBuffSelectionPhase)
                {
                    // 傳奇強化選擇完成，恢復遊戲狀態
                    Debug.Log("[RoguelikeMenu] 傳奇強化選擇完成，恢復遊戲狀態");
                    if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.LevelUp)
                    {
                        GameManager.Instance.ChangeGameState(GameState.Playing);
                    }
                    isLegendaryBuffSelectionPhase = false; // 重置標記
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
            if (currentStatsText == null) return;
            if (PlayerManager.Instance == null) return;
            
            var stats = PlayerManager.Instance.Stats;
            
            // 更新傳奇強化（裝甲強化、協同火力）
            if (legendaryBuffText != null)
            {
                System.Text.StringBuilder legendarySb = new System.Text.StringBuilder();
                legendarySb.AppendLine("【傳奇強化】");
                legendarySb.AppendLine($"裝甲強化 Defense: Lv.{stats.blockDefenseLevel} (+{stats.blockDefenseLevel} HP)");
                legendarySb.AppendLine($"協同火力 Volley: Lv.{stats.missileExtraCount} (每個位置 +{stats.missileExtraCount} 導彈)");

                // 傳奇：戰術擴展
                string tacticalInfo = "";
                if (stats.tacticalExpansionLevel >= 1) tacticalInfo = "處決";
                if (stats.tacticalExpansionLevel >= 2) tacticalInfo = "處決、修補";

                if (stats.tacticalExpansionLevel > 0)
                {
                    legendarySb.AppendLine($"戰術擴展 Tactical Expansion: Lv.{stats.tacticalExpansionLevel}/{GameConstants.TACTICAL_EXPANSION_MAX_LEVEL} (技能：{tacticalInfo})");
                }
                else
                {
                    legendarySb.AppendLine($"戰術擴展 Tactical Expansion: 未解鎖");
                }
                legendaryBuffText.text = legendarySb.ToString();
            }
            
            // 更新普通強化（其他6個，每行顯示3個）
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("【當前強化狀態】");
            sb.AppendLine();
            
            // 被動強化 - 每行顯示3個
            sb.AppendLine("═══ 被動強化 ═══");
            
            // 收集其他強化信息（排除裝甲強化和協同火力）
            var buffLines = new List<string>();
            
            if (stats.salvoLevel >= GameConstants.SALVO_MAX_LEVEL)
                buffLines.Add($"齊射強化: Lv.{stats.salvoLevel}/{GameConstants.SALVO_MAX_LEVEL} (已達上限) ({stats.salvoLevel * 50}% 多行加成)");
            else
                buffLines.Add($"齊射強化: Lv.{stats.salvoLevel}/{GameConstants.SALVO_MAX_LEVEL} ({stats.salvoLevel * 50}% 多行加成)");
            
            if (stats.burstLevel >= GameConstants.BURST_MAX_LEVEL)
                buffLines.Add($"連發強化: Lv.{stats.burstLevel}/{GameConstants.BURST_MAX_LEVEL} (已達上限) ({stats.burstLevel * 25}% 連擊加成)");
            else
                buffLines.Add($"連發強化: Lv.{stats.burstLevel}/{GameConstants.BURST_MAX_LEVEL} ({stats.burstLevel * 25}% 連擊加成)");
            
            if (stats.counterFireLevel >= GameConstants.COUNTER_MAX_LEVEL)
                buffLines.Add($"反擊強化: Lv.{stats.counterFireLevel}/{GameConstants.COUNTER_MAX_LEVEL} (已達上限) ({stats.counterFireLevel} 反擊導彈)");
            else
                buffLines.Add($"反擊強化: Lv.{stats.counterFireLevel}/{GameConstants.COUNTER_MAX_LEVEL} ({stats.counterFireLevel} 反擊導彈)");
            
            if (stats.explosionChargeLevel >= GameConstants.EXPLOSION_BUFF_MAX_LEVEL)
                buffLines.Add($"過載爆破: Lv.{stats.explosionChargeLevel}/{GameConstants.EXPLOSION_BUFF_MAX_LEVEL} (已達上限) (充能: {stats.explosionCharge}/{stats.explosionMaxCharge})");
            else
                buffLines.Add($"過載爆破: Lv.{stats.explosionChargeLevel}/{GameConstants.EXPLOSION_BUFF_MAX_LEVEL} (充能: {stats.explosionCharge}/{stats.explosionMaxCharge})");
            
            if (stats.spaceExpansionLevel >= GameConstants.SPACE_EXPANSION_MAX_LEVEL)
                buffLines.Add($"空間擴充: Lv.{stats.spaceExpansionLevel}/{GameConstants.SPACE_EXPANSION_MAX_LEVEL} (已達上限，{stats.spaceExpansionLevel} 槽位)");
            else
                buffLines.Add($"空間擴充: Lv.{stats.spaceExpansionLevel}/{GameConstants.SPACE_EXPANSION_MAX_LEVEL} ({stats.spaceExpansionLevel} 槽位)");
            
            if (stats.cpExpansionLevel >= GameConstants.RESOURCE_EXPANSION_MAX_LEVEL)
                buffLines.Add($"資源擴充: Lv.{stats.cpExpansionLevel}/{GameConstants.RESOURCE_EXPANSION_MAX_LEVEL} (已達上限，CP: {stats.maxCp})");
            else
                buffLines.Add($"資源擴充: Lv.{stats.cpExpansionLevel}/{GameConstants.RESOURCE_EXPANSION_MAX_LEVEL} (CP: {stats.maxCp})");
            // 每行顯示3個
            for (int i = 0; i < buffLines.Count; i += 3)
            {
                var line = new System.Text.StringBuilder();
                for (int j = 0; j < 3 && (i + j) < buffLines.Count; j++)
                {
                    if (j > 0) line.Append("  |  ");
                    line.Append(buffLines[i + j]);
                }
                sb.AppendLine(line.ToString());
            }
            
            currentStatsText.text = sb.ToString();
        }
    }
}

