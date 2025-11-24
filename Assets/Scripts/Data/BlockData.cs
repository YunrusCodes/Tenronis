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
        
        public BlockData(BlockColor color, int hp, int maxHp, bool isIndestructible = false)
        {
            this.color = color;
            this.hp = hp;
            this.maxHp = maxHp;
            this.isIndestructible = isIndestructible;
            this.createdTime = Time.time;
        }
        
        public BlockData Clone()
        {
            return new BlockData(color, hp, maxHp, isIndestructible)
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
        public int score;
        
        // 升級數據
        public int blockDefenseLevel;      // 方塊防禦等級（增加方塊HP）
        public int missileExtraCount;      // 額外導彈數量
        public int explosionDamage;        // 爆炸充能傷害
        public int salvoLevel;             // 齊射等級
        public int burstLevel;             // 連發等級
        public int counterFireLevel;       // 反擊等級
        
        // 技能充能
        public int executionCount;         // 處決次數
        public int repairCount;            // 修復次數
        
        // 戰鬥狀態
        public int comboCount;             // 當前連擊數
        
        public PlayerStats()
        {
            maxHp = GameConstants.PLAYER_MAX_HP;
            currentHp = GameConstants.PLAYER_MAX_HP;
            score = 0;
            blockDefenseLevel = 0;
            missileExtraCount = 0;
            explosionDamage = 0;
            salvoLevel = 1;
            burstLevel = 1;
            counterFireLevel = 1;
            executionCount = 0;
            repairCount = 0;
            comboCount = 0;
        }
    }
    
    /// <summary>
    /// 俄羅斯方塊形狀定義
    /// </summary>
    [Serializable]
    public class TetrominoShape
    {
        public string name;
        public BlockColor color;
        public int[,] shape;
        
        public TetrominoShape(string name, BlockColor color, int[,] shape)
        {
            this.name = name;
            this.color = color;
            this.shape = shape;
        }
    }
}

