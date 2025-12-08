using UnityEngine;
using System;

namespace Tenronis.ScriptableObjects
{
    /// <summary>
    /// 敵人技能配置
    /// </summary>
    [Serializable]
    public class EnemyAbility
    {
        public bool enabled = false;
        
        [Range(0f, 1f)]
        [Tooltip("使用此技能的機率（0 = 不使用，1 = 總是使用）")]
        public float chance = 0.2f;
        
        public EnemyAbility(bool enabled = false, float chance = 0.2f)
        {
            this.enabled = enabled;
            this.chance = chance;
        }
    }
    
    /// <summary>
    /// 關卡數據 ScriptableObject
    /// 基於數學平衡模型：01_Core_Variables, 02_Combat_Formulas, 04_Difficulty_Model, 05_Player_Model, 06_Balance_Analysis
    /// </summary>
    [CreateAssetMenu(fileName = "StageData", menuName = "Tenronis/Stage Data", order = 1)]
    public class StageDataSO : ScriptableObject
    {
        [Header("關卡資訊")]
        public string stageName = "未命名威脅";
        public int stageIndex = 0;
        
        [Tooltip("是否為Boss關卡（影響BGM和特殊效果）")]
        public bool isBossStage = false;
        
        [Header("過關獎勵")]
        [Tooltip("過關後獲得的升級卡牌數量")]
        [Range(0, 5)]
        public int rewardBuffCount = 1;
        
        [Header("敵人屬性")]
        public int maxHp = 100;
        
        [Header("攻擊模式")]
        public float shootInterval = 2f;      // 射擊間隔（秒）
        public float bulletSpeed = 8f;        // 子彈速度（格子/秒）
        
        [Header("=== 敵人技能配置 ===")]
        [Space(10)]
        
        [Tooltip("普通子彈：造成 1 點傷害")]
        public EnemyAbility normalBullet = new EnemyAbility(true, 1f);
        
        [Tooltip("範圍傷害子彈：3x3 範圍傷害")]
        public EnemyAbility areaBullet = new EnemyAbility(false, 0.25f);
        
        [Tooltip("添加普通方塊：在擊中方塊上方添加垃圾方塊")]
        public EnemyAbility addBlockBullet = new EnemyAbility(false, 0.3f);
        
        [Tooltip("添加爆炸方塊：添加的方塊被擊中時對玩家造成 5 點傷害")]
        public EnemyAbility addExplosiveBlockBullet = new EnemyAbility(false, 0.2f);
        
        [Tooltip("插入普通垃圾行：從底部插入一整行普通方塊（BlockType.Normal），消除時正常發射導彈")]
        public EnemyAbility addRowBullet = new EnemyAbility(false, 0.15f);
        
        [Tooltip("插入虛無垃圾行：從底部插入一整行虛無方塊（BlockType.Void），消除時不產生導彈")]
        public EnemyAbility addVoidRowBullet = new EnemyAbility(false, 0.1f);
        
        [Tooltip("腐化爆炸方塊：將下個方塊的隨機一格變成爆炸方塊")]
        public EnemyAbility corruptExplosiveBullet = new EnemyAbility(false, 0.15f);
        
        [Tooltip("腐化虛無方塊：將下個方塊的隨機一格變成虛無方塊")]
        public EnemyAbility corruptVoidBullet = new EnemyAbility(false, 0.1f);
        
        [Header("智能射擊系統")]
        [Tooltip("啟用智能射擊：根據網格狀態選擇目標列")]
        public bool useSmartTargeting = false;
        
        [Tooltip("智能射擊時，AddBlock 子彈是否優先射擊高點")]
        public bool addBlockTargetsHigh = true;
        
        [Tooltip("智能射擊時，AreaDamage 子彈是否優先射擊低點")]
        public bool areaDamageTargetsLow = true;
        
        [Header("視覺")]
        public Sprite enemyIcon;
        public Color themeColor = Color.red;
    }
}
