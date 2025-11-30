namespace Tenronis.Data
{
    /// <summary>
    /// 遊戲常數配置
    /// </summary>
    public static class GameConstants
    {
        // 遊戲板設定
        public const int BOARD_WIDTH = 10;
        public const int BOARD_HEIGHT = 20;
        
        // 遊戲節奏
        public const float TICK_RATE = 0.8f; // 方塊下落間隔（秒）
        
        // 玩家設定
        public const int PLAYER_MAX_HP = 100;
        public const int PLAYER_MAX_CP = 100; // Castle Point 最大值
        public const int OVERFLOW_CP_COST = 25; // 溢出時消耗的 CP
        
        // 技能CP消耗
        public const int EXECUTION_CP_COST = 5; // 處決技能消耗的 CP
        public const int REPAIR_CP_COST = 30; // 修復技能消耗的 CP
        
        // 導彈設定
        public const float MISSILE_SPEED = 20f; // 格子/秒
        
        // Roguelike系統
        public const int INITIAL_ROGUE_REQUIREMENT = 50; // 初始升級所需傷害
        public const int ROGUE_REQUIREMENT_INCREMENT = 5; // 每次升級後增加的要求
        
        // 反擊系統
        public const float COUNTER_FIRE_TIME_WINDOW = 0.2f; // 反擊時間窗口（秒）
        
        // Combo系統
        public const float COMBO_RESET_DELAY = 0.3f; // Combo重置延遲（秒）
        
        // 傷害計算
        public const float BASE_MISSILE_DAMAGE = 2f;
        public const float SALVO_DAMAGE_MULTIPLIER = 0.5f; // 每額外行增加50%傷害
        public const float BURST_DAMAGE_MULTIPLIER = 0.25f; // 每層Combo增加25%傷害
        public const float EXECUTION_DAMAGE = 4f;
        public const float REPAIR_DAMAGE = 2f;
        
        // 方塊設定
        public const int BASE_BLOCK_HP = 1;
        public const int GARBAGE_BLOCK_HP = 1;
        public const int INDESTRUCTIBLE_BLOCK_HP = 9999;
        
        // 敵人子彈傷害
        public const int BULLET_DAMAGE = 10;
        public const int BASE_HIT_DAMAGE = 10;
        public const int OVERFLOW_DAMAGE_PERCENT = 50; // 溢出時損失50%血量
        public const int INSERT_ROW_OVERFLOW_DAMAGE = 50; // 插入行頂出方塊時的固定傷害
        
        // 爆炸充能系統
        public const int EXPLOSION_INITIAL_MAX_CHARGE = 200; // 初始充能上限
        public const int EXPLOSION_COUNTER_CHARGE = 5; // 反擊一次增加的充能
        public const int EXPLOSION_ROW_CLEAR_CHARGE = 50; // 消排一次增加的充能
        public const int EXPLOSION_BUFF_MAX_CHARGE_INCREASE = 200; // Explosion Buff 每次增加的上限
        public const int EXPLOSION_BUFF_MAX_LEVEL = 4; // Explosion Buff 最高等級
        
        // Buff等級上限
        public const int SALVO_MAX_LEVEL = 6; // Salvo（齊射強化）最高等級
        public const int BURST_MAX_LEVEL = 6; // Burst 最高等級
        public const int COUNTER_MAX_LEVEL = 6; // Counter 最高等級
        public const int SPACE_EXPANSION_MAX_LEVEL = 4; // SpaceExpansion 最高等級
        public const int RESOURCE_EXPANSION_MAX_LEVEL = 3; // ResourceExpansion 最高等級
        
        // 強化分類
        // 普通強化：有上限等級的強化
        public static readonly BuffType[] NORMAL_BUFFS = new BuffType[]
        {
            BuffType.Salvo,              // 齊射強化：起始1，上限6
            BuffType.Burst,              // 連發強化：起始1，上限6
            BuffType.Counter,            // 反擊強化：起始1，上限6
            BuffType.Explosion,          // 過載爆破：起始1，上限4
            BuffType.SpaceExpansion,     // 空間擴充：起始1，上限4
            BuffType.ResourceExpansion   // 資源擴充：起始0，上限3
        };
        
        // 傳奇強化：無上限或特殊效果的強化
        public static readonly BuffType[] LEGENDARY_BUFFS = new BuffType[]
        {
            BuffType.Defense,            // 裝甲強化：起始0，無上限
            BuffType.Volley,             // 協同火力：起始0，無上限
            BuffType.Heal                // 緊急修復：立即效果
        };
    }
}

