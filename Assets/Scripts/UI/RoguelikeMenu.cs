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
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            sb.AppendLine("【當前強化狀態】");
            sb.AppendLine();
            
            // 被動強化
            sb.AppendLine("═══ 被動強化 ═══");
            
            if (stats.blockDefenseLevel > 0)
                sb.AppendLine($"🛡️ 裝甲強化: Lv.{stats.blockDefenseLevel} (+{stats.blockDefenseLevel} HP)");
            
            if (stats.missileExtraCount > 0)
                sb.AppendLine($"🚀 多重齊射: Lv.{stats.missileExtraCount} (+{stats.missileExtraCount} 導彈/行)");
            
            if (stats.salvoLevel > 1)
                sb.AppendLine($"🎯 協同打擊: Lv.{stats.salvoLevel} ({stats.salvoLevel * 50}% 多行加成)");
            
            if (stats.burstLevel > 1)
                sb.AppendLine($"💥 連擊爆發: Lv.{stats.burstLevel} ({stats.burstLevel * 25}% 連擊加成)");
            
            if (stats.counterFireLevel > 1)
                sb.AppendLine($"⚔️ 反擊系統: Lv.{stats.counterFireLevel} ({stats.counterFireLevel} 反擊導彈)");
            
            if (stats.explosionDamage > 0)
                sb.AppendLine($"💣 爆炸充能: +{stats.explosionDamage} 溢出傷害");
            
            if (stats.spaceExpansionLevel > 1)
                sb.AppendLine($"📦 空間擴充: {stats.spaceExpansionLevel} 槽位已解鎖");
            
            if (stats.cpExpansionLevel > 0)
                sb.AppendLine($"⚡ 資源擴充: Lv.{stats.cpExpansionLevel} (CP上限: {stats.maxCp})");
            
            // 主動技能
            if (stats.executionCount > 0 || stats.repairCount > 0)
            {
                sb.AppendLine();
                sb.AppendLine("═══ 主動技能 ═══");
                
                if (stats.executionCount > 0)
                    sb.AppendLine($"✂️ 處決: x{stats.executionCount} 可用");
                
                if (stats.repairCount > 0)
                    sb.AppendLine($"🔧 修復: x{stats.repairCount} 可用");
            }
            
            // 如果沒有任何強化
            if (stats.blockDefenseLevel == 0 && stats.missileExtraCount == 0 && 
                stats.salvoLevel <= 1 && stats.burstLevel <= 1 && 
                stats.counterFireLevel <= 1 && stats.explosionDamage == 0 && 
                stats.spaceExpansionLevel <= 1 && stats.cpExpansionLevel == 0 && 
                stats.executionCount == 0 && stats.repairCount == 0)
            {
                sb.AppendLine();
                sb.AppendLine("目前尚未獲得任何強化");
                sb.AppendLine("選擇一個強化開始變強吧！");
            }
            
            currentStatsText.text = sb.ToString();
        }
    }
}

