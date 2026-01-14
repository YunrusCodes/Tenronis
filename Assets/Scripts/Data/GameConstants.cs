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
        public const float TICK_RATE = 0.8f;

        // 玩家設定
        public const int PLAYER_MAX_HP = 100;
        public const int PLAYER_MAX_CP = 100;
        public const int OVERFLOW_CP_COST = 75;

        // 技能CP消耗
        public const int EXECUTION_CP_COST = 5;
        public const int REPAIR_CP_COST = 30;
        public const int ANNIHILATION_CP_COST = 5;

        // 導彈設定
        public const float MISSILE_SPEED = 20f;

        // Roguelike系統
        public const int INITIAL_ROGUE_REQUIREMENT = 50;
        public const int ROGUE_REQUIREMENT_INCREMENT = 5;

        // 反擊系統
        public const float COUNTER_FIRE_TIME_WINDOW = 0.2f;

        // Combo 系統
        public const float COMBO_RESET_DELAY = 0.3f;

        // 傷害計算
        public const float BASE_MISSILE_DAMAGE = 1f;
        public const float SALVO_DAMAGE_MULTIPLIER = 0.5f;
        public const float BURST_DAMAGE_MULTIPLIER = 0.25f;
        public const float EXECUTION_DAMAGE = 4f;
        public const float REPAIR_DAMAGE = 2f;

        // 方塊設定
        public const int BASE_BLOCK_HP = 1;
        public const int GARBAGE_BLOCK_HP = 1;
        public const int INDESTRUCTIBLE_BLOCK_HP = 9999;

        // 敵人子彈傷害
        public const int BULLET_DAMAGE = 10;
        public const int BASE_HIT_DAMAGE = 10;
        public const int OVERFLOW_DAMAGE_PERCENT = 50;
        public const int INSERT_ROW_OVERFLOW_DAMAGE = 50;

        // 爆炸充能系統
        public const int EXPLOSION_INITIAL_MAX_CHARGE = 200;
        public const int EXPLOSION_COUNTER_CHARGE = 5;
        public const int EXPLOSION_ROW_CLEAR_CHARGE = 50;
        public const int EXPLOSION_BUFF_MAX_CHARGE_INCREASE = 200;
        public const int EXPLOSION_BUFF_MAX_LEVEL = 4;

        // Buff 等級上限
        public const int SALVO_MAX_LEVEL = 6;
        public const int BURST_MAX_LEVEL = 6;
        public const int COUNTER_MAX_LEVEL = 6;
        public const int SPACE_EXPANSION_MAX_LEVEL = 4;
        public const int RESOURCE_EXPANSION_MAX_LEVEL = 3;

        // ⭐ 戰術擴展：最高等級 3
        public const int TACTICAL_EXPANSION_MAX_LEVEL = 3;
        
        // ⭐ 協同火力：最高等級 5
        public const int VOLLEY_MAX_LEVEL = 5;

        // Buff 起始等級
        public const int DEFENSE_START_LEVEL = 0;
        public const int VOLLEY_START_LEVEL = 0;
        public const int TACTICAL_EXPANSION_START_LEVEL = 3;

        // 🚫 普通強化（有限等級）
        public static readonly BuffType[] NORMAL_BUFFS = new BuffType[]
        {
            BuffType.Salvo,
            BuffType.Burst,
            BuffType.Counter,
            BuffType.Explosion,
            BuffType.SpaceExpansion,
            BuffType.ResourceExpansion

            // ❌ TacticalExpansion 已移除
        };

        // ⭐ 傳奇強化（無上限 or 特殊效果）
        public static readonly BuffType[] LEGENDARY_BUFFS = new BuffType[]
        {
            BuffType.Defense,    // 裝甲強化：無上限
            BuffType.Volley,     // 協同火力：上限5級
            BuffType.TacticalExpansion // ⭐ 戰術擴展：解鎖技能
        };
    }
}
