using UnityEngine;
using System;

namespace Tenronis.ScriptableObjects
{
    /// <summary>
    /// 難度軌道類型
    /// 定義三種難度軌道，影響目標擊殺時間和敵人屬性
    /// </summary>
    public enum DifficultyTrack
    {
        Casual,     // 休閒模式：35秒目標擊殺時間
        Standard,   // 標準模式：25秒目標擊殺時間
        Expert      // 專家模式：20秒目標擊殺時間
    }
    
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
        
        [Header("難度配置")]
        [Tooltip("難度軌道：影響目標擊殺時間和敵人屬性計算")]
        public DifficultyTrack difficultyTrack = DifficultyTrack.Standard;
        
        [Tooltip("啟用自動平衡：根據 PDA 和 SP 自動計算敵人屬性")]
        public bool autoBalance = true;
        
        [Header("玩家能力參數（用於自動平衡）")]
        [Tooltip("玩家傷害可用性 (PDA) - 玩家每秒期望輸出傷害\n來源：05_Player_Model.md")]
        [Range(1f, 3000f)]
        public float playerPDA = 100f;
        
        [Tooltip("板面穩定性參數 (SP) - 影響敵人射速\n範圍：0（極危）到 1（安全）\n來源：04_Difficulty_Model.md")]
        [Range(0f, 1f)]
        public float playerSP = 0.5f;
        
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
        
        // ==================== 計算屬性（只讀，顯示於 Inspector） ====================
        
        /// <summary>
        /// 目標擊殺時間（秒）
        /// 來源：06_Balance_Analysis.md - 平衡條件
        /// </summary>
        public float TargetKillTime
        {
            get
            {
                switch (difficultyTrack)
                {
                    case DifficultyTrack.Casual:
                        return 35f;
                    case DifficultyTrack.Standard:
                        return 25f;
                    case DifficultyTrack.Expert:
                        return 20f;
                    default:
                        return 25f;
                }
            }
        }
        
        /// <summary>
        /// 計算建議的敵人 HP
        /// 公式：maxHp = PDA × TargetTime
        /// 來源：06_Balance_Analysis.md - 平衡條件
        /// </summary>
        public int CalculatedMaxHp
        {
            get
            {
                return Mathf.RoundToInt(playerPDA * TargetKillTime);
            }
        }
        
        /// <summary>
        /// 計算建議的射擊間隔
        /// 公式：shootInterval = Lerp(minInterval, maxInterval, SP)
        /// SP 越高（越穩定），敵人可以射得越快
        /// 來源：04_Difficulty_Model.md - 板面穩定性函數
        /// </summary>
        public float CalculatedShootInterval
        {
            get
            {
                float minInterval = GetMinShootInterval();
                float maxInterval = GetMaxShootInterval();
                return Mathf.Lerp(maxInterval, minInterval, playerSP);
            }
        }
        
        /// <summary>
        /// 計算建議的子彈速度
        /// 來源：04_Difficulty_Model.md - 難度指數定義
        /// </summary>
        public float CalculatedBulletSpeed
        {
            get
            {
                switch (difficultyTrack)
                {
                    case DifficultyTrack.Casual:
                        return 6f;
                    case DifficultyTrack.Standard:
                        return 8f;
                    case DifficultyTrack.Expert:
                        return 10f;
                    default:
                        return 8f;
                }
            }
        }
        
        /// <summary>
        /// 難度倍率
        /// 來源：自訂（基於難度軌道）
        /// </summary>
        public float DifficultyMultiplier
        {
            get
            {
                switch (difficultyTrack)
                {
                    case DifficultyTrack.Casual:
                        return 0.5f;
                    case DifficultyTrack.Standard:
                        return 1.0f;
                    case DifficultyTrack.Expert:
                        return 1.6f;
                    default:
                        return 1.0f;
                }
            }
        }
        
        /// <summary>
        /// 敵人子彈壓力指標 λ_bullet（發/秒）
        /// 公式：λ_bullet = 1 / shootInterval
        /// 來源：02_Combat_Formulas.md - 防空負擔模型
        /// </summary>
        public float BulletPressure
        {
            get
            {
                if (shootInterval <= 0) return 0f;
                return 1f / shootInterval;
            }
        }
        
        /// <summary>
        /// 數學難度等級描述
        /// 基於 CT (Comprehensive Threat) 模型
        /// 來源：04_Difficulty_Model.md - 綜合威脅指數
        /// </summary>
        public string DifficultyDescription
        {
            get
            {
                float ct = CalculateComprehensiveThreat();
                
                if (ct < 2f) return "★☆☆☆☆ 非常簡單";
                if (ct < 5f) return "★★☆☆☆ 簡單";
                if (ct < 10f) return "★★★☆☆ 中等";
                if (ct < 15f) return "★★★★☆ 困難";
                return "★★★★★ 非常困難";
            }
        }
        
        /// <summary>
        /// 綜合威脅指數 (CT)
        /// 簡化計算：基於 HP、射速、子彈速度的綜合評估
        /// 來源：04_Difficulty_Model.md - 綜合威脅指數
        /// </summary>
        private float CalculateComprehensiveThreat()
        {
            // 歸一化基準值（Stage 1）
            float baseHp = 120f;
            float baseShootInterval = 3.0f;
            float baseBulletSpeed = 5.0f;
            
            // 計算歸一化值
            float hpNorm = maxHp / baseHp;
            float shootNorm = baseShootInterval / Mathf.Max(shootInterval, 0.1f);
            float speedNorm = bulletSpeed / baseBulletSpeed;
            
            // 權重係數（來源：04_Difficulty_Model.md）
            float alpha_hp = 0.4f;
            float alpha_shoot = 0.3f;
            float alpha_speed = 0.2f;
            float alpha_bullet = 0.1f;
            
            // 計算技能威脅
            float bulletThreat = CalculateBulletThreat();
            
            // 綜合威脅
            float ct = alpha_hp * hpNorm + 
                      alpha_shoot * shootNorm + 
                      alpha_speed * speedNorm + 
                      alpha_bullet * bulletThreat;
            
            return ct;
        }
        
        /// <summary>
        /// 計算子彈技能威脅度
        /// 來源：自訂（基於技能配置）
        /// </summary>
        private float CalculateBulletThreat()
        {
            float threat = 0f;
            
            // 基礎技能權重
            if (normalBullet.enabled) threat += normalBullet.chance * 1.0f;
            if (areaBullet.enabled) threat += areaBullet.chance * 2.0f;
            if (addBlockBullet.enabled) threat += addBlockBullet.chance * 2.5f;
            if (addExplosiveBlockBullet.enabled) threat += addExplosiveBlockBullet.chance * 3.0f;
            if (addRowBullet.enabled) threat += addRowBullet.chance * 3.5f;
            if (addVoidRowBullet.enabled) threat += addVoidRowBullet.chance * 4.0f;
            if (corruptExplosiveBullet.enabled) threat += corruptExplosiveBullet.chance * 3.0f;
            if (corruptVoidBullet.enabled) threat += corruptVoidBullet.chance * 3.5f;
            
            // 智能瞄準額外威脅
            if (useSmartTargeting) threat *= 1.3f;
            
            return threat;
        }
        
        // ==================== 自動平衡方法 ====================
        
        /// <summary>
        /// 應用自動平衡
        /// 根據 PDA 和 SP 重新計算所有敵人屬性
        /// </summary>
        /// <param name="pda">玩家傷害可用性（傷害/秒）</param>
        /// <param name="sp">板面穩定性參數（0-1）</param>
        public void ApplyAutoBalance(float pda, float sp)
        {
            playerPDA = Mathf.Clamp(pda, 1f, 3000f);
            playerSP = Mathf.Clamp01(sp);
            
            // 1. 計算 maxHp（公式來源：06_Balance_Analysis.md）
            maxHp = CalculatedMaxHp;
            
            // 2. 計算射擊間隔（公式來源：04_Difficulty_Model.md）
            shootInterval = CalculatedShootInterval;
            
            // 3. 計算子彈速度（公式來源：04_Difficulty_Model.md）
            bulletSpeed = CalculatedBulletSpeed;
            
            // 4. 計算技能機率（基於難度倍率）
            ApplySkillDensity();
            
            // 6. 智能瞄準（Expert 難度啟用）
            useSmartTargeting = (difficultyTrack == DifficultyTrack.Expert) && (stageIndex >= 15);
            
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
        

        
        /// <summary>
        /// 應用技能密度
        /// 公式：chance = BaseChance × DifficultyMultiplier
        /// 來源：自訂（基於難度軌道）
        /// </summary>
        private void ApplySkillDensity()
        {
            float multiplier = DifficultyMultiplier;
            
            // 基礎機率（Standard 難度）
            float baseAreaChance = 0.25f;
            float baseAddBlockChance = 0.30f;
            float baseAddExplosiveChance = 0.20f;
            float baseAddRowChance = 0.15f;
            float baseAddVoidRowChance = 0.10f;
            float baseCorruptExplosiveChance = 0.15f;
            float baseCorruptVoidChance = 0.10f;
            
            // 應用難度倍率
            areaBullet.chance = Mathf.Clamp01(baseAreaChance * multiplier);
            addBlockBullet.chance = Mathf.Clamp01(baseAddBlockChance * multiplier);
            addExplosiveBlockBullet.chance = Mathf.Clamp01(baseAddExplosiveChance * multiplier);
            addRowBullet.chance = Mathf.Clamp01(baseAddRowChance * multiplier);
            addVoidRowBullet.chance = Mathf.Clamp01(baseAddVoidRowChance * multiplier);
            corruptExplosiveBullet.chance = Mathf.Clamp01(baseCorruptExplosiveChance * multiplier);
            corruptVoidBullet.chance = Mathf.Clamp01(baseCorruptVoidChance * multiplier);
            
            // 根據關卡進度啟用技能
            EnableSkillsByStageProgression();
        }
        
        /// <summary>
        /// 根據關卡進度啟用技能
        /// </summary>
        private void EnableSkillsByStageProgression()
        {
            // Stage 1-5: 僅普通子彈
            normalBullet.enabled = true;
            
            // Stage 6+: 範圍傷害
            areaBullet.enabled = (stageIndex >= 6);
            
            // Stage 8+: 添加方塊
            addBlockBullet.enabled = (stageIndex >= 8);
            
            // Stage 10+: 添加爆炸方塊
            addExplosiveBlockBullet.enabled = (stageIndex >= 10);
            
            // Stage 12+: 插入垃圾行
            addRowBullet.enabled = (stageIndex >= 12);
            
            // Stage 15+: 高級技能
            addVoidRowBullet.enabled = (stageIndex >= 15);
            corruptExplosiveBullet.enabled = (stageIndex >= 15);
            corruptVoidBullet.enabled = (stageIndex >= 17);
        }
        
        /// <summary>
        /// 獲取最小射擊間隔（基於難度）
        /// </summary>
        private float GetMinShootInterval()
        {
            switch (difficultyTrack)
            {
                case DifficultyTrack.Casual:
                    return 1.2f;
                case DifficultyTrack.Standard:
                    return 0.9f;
                case DifficultyTrack.Expert:
                    return 0.7f;
                default:
                    return 0.9f;
            }
        }
        
        /// <summary>
        /// 獲取最大射擊間隔（基於難度）
        /// </summary>
        private float GetMaxShootInterval()
        {
            switch (difficultyTrack)
            {
                case DifficultyTrack.Casual:
                    return 3.5f;
                case DifficultyTrack.Standard:
                    return 2.5f;
                case DifficultyTrack.Expert:
                    return 2.0f;
                default:
                    return 2.5f;
            }
        }
        
        // ==================== Unity 回調 ====================
        
        private void OnValidate()
        {
            // 當 Inspector 值改變時，如果啟用 autoBalance，自動重新計算
            if (autoBalance)
            {
                ApplyAutoBalance(playerPDA, playerSP);
            }
        }
    }
}
