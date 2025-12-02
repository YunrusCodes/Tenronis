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
        public string themeName = "New Theme";
        public Sprite themeIcon;
        public Color themeColor = Color.white;
        [TextArea(3, 5)]
        public string description = "主題描述...";

        [Header("關卡列表")]
        public List<StageDataSO> stages = new List<StageDataSO>();

        /// <summary>
        /// 獲取關卡列表
        /// </summary>
        public List<StageDataSO> GetStages()
        {
            return stages;
        }
    }
}
