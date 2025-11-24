using UnityEngine;

namespace Tenronis.ScriptableObjects
{
    /// <summary>
    /// 關卡數據 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "StageData", menuName = "Tenronis/Stage Data", order = 1)]
    public class StageDataSO : ScriptableObject
    {
        [Header("關卡資訊")]
        public string stageName = "未命名威脅";
        public int stageIndex = 0;
        
        [Header("敵人屬性")]
        public int maxHp = 100;
        
        [Header("攻擊模式")]
        public float shootInterval = 2f;      // 射擊間隔（秒）
        public float bulletSpeed = 8f;        // 子彈速度（格子/秒）
        
        [Header("特殊能力解鎖")]
        [Tooltip("關卡5+：可以發射添加方塊子彈")]
        public bool canUseAddBlock = false;
        
        [Tooltip("關卡8+：可以發射範圍傷害子彈")]
        public bool canUseAreaDamage = false;
        
        [Tooltip("關卡10：可以發射插入行子彈")]
        public bool canUseInsertRow = false;
        
        [Header("特殊能力機率")]
        [Range(0f, 1f)]
        public float addBlockChance = 0.35f;
        
        [Range(0f, 1f)]
        public float areaDamageChance = 0.25f;
        
        [Range(0f, 1f)]
        public float insertRowChance = 0.15f;
        
        [Header("智能射擊系統")]
        [Tooltip("啟用智能射擊：根據網格狀態選擇目標列")]
        public bool useSmartTargeting = false;
        
        [Tooltip("智能射擊時，AddBlock 子彈是否優先射擊高點")]
        public bool addBlockTargetsHigh = true;
        
        [Tooltip("智能射擊時，AreaDamage 子彈是否優先射擊低點")]
        public bool areaDamageTargetsLow = true;
        
        [Tooltip("連發數量（1 = 單發，3 = 三聯發）")]
        [Range(1, 5)]
        public int burstCount = 1;
        
        [Header("視覺")]
        public Sprite enemyIcon;
        public Color themeColor = Color.red;
    }
}

