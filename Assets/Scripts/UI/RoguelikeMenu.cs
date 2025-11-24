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
        
        private List<GameObject> currentOptions = new List<GameObject>();
        
        private void OnEnable()
        {
            GenerateBuffOptions();
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
            }
            else
            {
                // 關閉選單
                gameObject.SetActive(false);
            }
        }
    }
}

