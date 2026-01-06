using System;
using UnityEngine;

namespace Tenronis.Data
{
    /// <summary>
    /// 方塊數據
    /// </summary>
    [Serializable]
    public class BlockData
    {
        public BlockColor color;
        public int hp;
        public int maxHp;
        public bool isIndestructible;
        public float createdTime; // 創建時間（用於反擊判定）
        public BlockType blockType; // 方塊特性類型
        
        public BlockData(BlockColor color, int hp, int maxHp, bool isIndestructible = false, BlockType blockType = BlockType.Normal)
        {
            this.color = color;
            this.hp = hp;
            this.maxHp = maxHp;
            this.isIndestructible = isIndestructible;
            this.createdTime = Time.time;
            this.blockType = blockType;
        }
        
        public BlockData Clone()
        {
            return new BlockData(color, hp, maxHp, isIndestructible, blockType)
            {
                createdTime = this.createdTime
            };
        }
    }
    
    /// <summary>
    /// 玩家數據
    /// </summary>
    [Serializable]
    public class PlayerStats
    {
        public int maxHp;
        public int currentHp;
        public int maxCp;              // Castle Point 最大值
        public int currentCp;          // Castle Point 當前值
        public int score;
        
        // 升級數據
        public int blockDefenseLevel;      // 方塊防禦等級（增加方塊HP）
        public int missileExtraCount;      // 協同火力等級（額外導彈數量 = 等級）
        public int explosionCharge;        // 當前爆炸充能
        public int explosionMaxCharge;     // 爆炸充能上限
        public int explosionChargeLevel;   // 爆炸充能等級（Explosion Buff等級）
        public int salvoLevel;             // 齊射等級（齊射強化）
        public int burstLevel;             // 連發等級
        public int counterFireLevel;       // 反擊等級
        public int spaceExpansionLevel;    // 空間擴充等級（解鎖儲存槽位數量）
        public int cpExpansionLevel;       // 資源擴充等級（增加CP上限）
        public int tacticalExpansionLevel; // 戰術擴展等級（解鎖技能）
        
        // 戰鬥狀態
        public int comboCount;             // 當前連擊數
        
        public PlayerStats()
        {
            maxHp = GameConstants.PLAYER_MAX_HP;
            currentHp = GameConstants.PLAYER_MAX_HP;
            maxCp = GameConstants.PLAYER_MAX_CP;
            currentCp = GameConstants.PLAYER_MAX_CP;
            score = 0;
            blockDefenseLevel = GameConstants.DEFENSE_START_LEVEL;
            missileExtraCount = GameConstants.VOLLEY_START_LEVEL; // Volley（協同火力）起始等級（基礎發射 1 發對應為 0）
            explosionCharge = 0;
            explosionMaxCharge = GameConstants.EXPLOSION_INITIAL_MAX_CHARGE;
            explosionChargeLevel = 1; // Explosion 起始等級 1
            salvoLevel = 1; // Salvo（齊射強化）起始等級 1
            burstLevel = 1;
            counterFireLevel = 1;
            spaceExpansionLevel = 1; // 初始解鎖 1 個槽位（A 鍵）
            cpExpansionLevel = 0;
            tacticalExpansionLevel = GameConstants.TACTICAL_EXPANSION_START_LEVEL; // 戰術擴展起始等級
            comboCount = 0;
        }
    }
    
}

