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
        
        private void OnEnable()
        {
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
            // 標題
            var titleText = optionObj.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
            if (titleText != null)
                titleText.text = buffData.buffName;
            
            // 描述
            var descText = optionObj.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
            if (descText != null)
                descText.text = buffData.description;
            
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
            GameEvents.TriggerBuffSelected(buffType);
            
            // 檢查是否還有待選Buff
            if (GameManager.Instance.PendingBuffCount > 0)
            {
                // 刷新選項
                GenerateBuffOptions();
                UpdateCurrentStats();
            }
            else
            {
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
                legendarySb.AppendLine($"裝甲強化: Lv.{stats.blockDefenseLevel} (+{stats.blockDefenseLevel} HP)");
                legendarySb.AppendLine($"協同火力: Lv.{stats.salvoLevel} ({stats.salvoLevel * 50}% 多行加成)");
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
            
            if (stats.missileExtraCount >= GameConstants.VOLLEY_MAX_LEVEL)
                buffLines.Add($"齊射強化: Lv.{stats.missileExtraCount}/{GameConstants.VOLLEY_MAX_LEVEL} (已達上限)");
            else
                buffLines.Add($"齊射強化: Lv.{stats.missileExtraCount}/{GameConstants.VOLLEY_MAX_LEVEL}");
            
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

