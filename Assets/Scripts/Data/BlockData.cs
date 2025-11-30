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
        public int missileExtraCount;      // 額外導彈數量
        public int explosionDamage;        // 爆炸充能傷害
        public int salvoLevel;             // 齊射等級
        public int burstLevel;             // 連發等級
        public int counterFireLevel;       // 反擊等級
        public int spaceExpansionLevel;    // 空間擴充等級（解鎖儲存槽位數量）
        public int cpExpansionLevel;       // 資源擴充等級（增加CP上限）
        
        // 技能充能
        public int executionCount;         // 處決次數
        public int repairCount;            // 修復次數
        
        // 戰鬥狀態
        public int comboCount;             // 當前連擊數
        
        public PlayerStats()
        {
            maxHp = GameConstants.PLAYER_MAX_HP;
            currentHp = GameConstants.PLAYER_MAX_HP;
            maxCp = GameConstants.PLAYER_MAX_CP;
            currentCp = GameConstants.PLAYER_MAX_CP;
            score = 0;
            blockDefenseLevel = 0;
            missileExtraCount = 0;
            explosionDamage = 0;
            salvoLevel = 1;
            burstLevel = 1;
            counterFireLevel = 1;
            spaceExpansionLevel = 1; // 初始解鎖 1 個槽位（A 鍵）
            cpExpansionLevel = 0;
            executionCount = 0;
            repairCount = 0;
            comboCount = 0;
        }
    }
    
}

