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
        public const int GARBAGE_BLOCK_HP = 2;
        public const int INDESTRUCTIBLE_BLOCK_HP = 9999;
        
        // 敵人子彈傷害
        public const int BULLET_DAMAGE = 10;
        public const int BASE_HIT_DAMAGE = 10;
        public const int OVERFLOW_DAMAGE_PERCENT = 50; // 溢出時損失50%血量
        public const int INSERT_ROW_OVERFLOW_DAMAGE = 50; // 插入行頂出方塊時的固定傷害
    }
}

