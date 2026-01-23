using UnityEngine;
using System.Collections.Generic;

namespace Tenronis.ScriptableObjects
{
    /// <summary>
    /// 關卡套組數據 (Theme)
    /// 定義一個主題下的所有關卡配置
    /// </summary>
    [CreateAssetMenu(fileName = "NewStageSet", menuName = "Tenronis/Stage Set (Theme)", order = 2)]
    public class StageSetSO : ScriptableObject
    {
        [Header("主題設定")]
        [Tooltip("主題名稱（繁體中文）")]
        public string themeName = "New Theme";
        
        [Tooltip("主題名稱（英文）")]
        public string themeNameEn = "New Theme";
        
        [Tooltip("主題名稱（日文）")]
        public string themeNameJa = "新テーマ";
        
        public Sprite themeIcon;
        public Color themeColor = Color.white;
        [TextArea(3, 5)]
        public string description = "主題描述...";
        
        [Header("背景圖片")]
        [Tooltip("戰鬥背景圖片 (用於 SpriteRenderer)")]
        public Sprite battleBackgroundSprite;
        
        [Tooltip("敵人介紹背景圖片 (用於 Image)")]
        public Sprite enemyIntroBackgroundSprite;
        
        [Tooltip("工程師背景圖片 (用於 Image)")]
        public Sprite engineerBackgroundSprite;
        
        [Header("玩家圖片")]
        [Tooltip("玩家圖片 (用於 Image)")]
        public Sprite playerSprite;

        [Header("關卡列表")]
        public List<StageDataSO> stages = new List<StageDataSO>();
        
        [Header("解鎖系統")]
        [Tooltip("通關後獲得的通關鑰匙，會保存在 PlayerPrefs 中")]
        public string passKey = "";
        
        [Tooltip("解鎖此主題所需的通關鑰匙（需在 PlayerPrefs 中存在對應的 PassKey）。若為空則表示無需解鎖")]
        public string unlockKey = "";

        /// <summary>
        /// 獲取關卡列表
        /// </summary>
        public List<StageDataSO> GetStages()
        {
            return stages;
        }
    }
}
