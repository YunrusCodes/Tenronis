using UnityEngine;
using Tenronis.Data;

namespace Tenronis.ScriptableObjects
{
    /// <summary>
    /// Buff數據 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "BuffData", menuName = "Tenronis/Buff Data", order = 2)]
    public class BuffDataSO : ScriptableObject
    {
        [Header("Buff資訊")]
        public string buffName = "未命名增益";
        public string buffNameEn = "Unnamed Buff"; // 英文名稱
        public string buffNameJa = "未命名バフ"; // 日文名稱
        public BuffType buffType;
        
        [TextArea(3, 5)]
        public string description = "描述...";
        [TextArea(3, 5)]
        public string descriptionEn = "Description..."; // 英文描述
        [TextArea(3, 5)]
        public string descriptionJa = "説明..."; // 日文描述
        
        [Header("視覺")]
        public Sprite icon;
        public Color iconColor = Color.white;
        
        [Header("稀有度")]
        [Range(0f, 1f)]
        [Tooltip("生成權重，越高越常見")]
        public float spawnWeight = 1f;
    }
}

