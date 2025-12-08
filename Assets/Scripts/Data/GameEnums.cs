using System;

namespace Tenronis.Data
{
    /// <summary>
    /// 遊戲狀態
    /// </summary>
    public enum GameState
    {
        Menu,
        Playing,
        LevelUp,
        GameOver,
        Victory
    }
    
    /// <summary>
    /// 方塊類型（用於 SRS 踢牆）
    /// </summary>
    public enum TetrominoType
    {
        I,  // I 方塊（特殊踢牆規則）
        O,  // O 方塊（不旋轉）
        T,  // T 方塊
        J,  // J 方塊
        L,  // L 方塊
        S,  // S 方塊
        Z   // Z 方塊
    }

    /// <summary>
    /// 方塊顏色類型
    /// </summary>
    public enum BlockColor
    {
        Cyan,
        Blue,
        Orange,
        Yellow,
        Green,
        Purple,
        Red,
        Gray,
        Garbage
    }
    
    /// <summary>
    /// 方塊特性類型
    /// </summary>
    public enum BlockType
    {
        Normal,      // 普通方塊：無特性
        Void,        // 虛無方塊：消除時不產生導彈（垃圾行）
        Explosive    // 爆炸方塊：被敵人射擊破壞時對玩家造成傷害
    }

    /// <summary>
    /// 子彈類型
    /// </summary>
    public enum BulletType
    {
        Normal,              // 普通子彈
        AreaDamage,          // 範圍傷害
        AddBlock,            // 添加普通方塊
        AddExplosiveBlock,   // 添加爆炸方塊
        InsertRow,           // 插入普通垃圾行
        InsertVoidRow,       // 插入虛無垃圾行
        CorruptExplosive,    // 腐化：將下個方塊的隨機一格變成爆炸方塊
        CorruptVoid          // 腐化：將下個方塊的隨機一格變成虛無方塊
    }

    /// <summary>
    /// Buff類型
    /// </summary>
    public enum BuffType
    {
        Defense,        // 防禦：增加方塊HP
        Volley,         // 協同火力：增加導彈傷害倍率（*Volley等級+1）
        Heal,           // [已廢棄] 治療：改為關卡開始時自動恢復
        Explosion,      // 爆炸：溢出時造成傷害
        Salvo,          // 齊射強化：多行消除時增加導彈傷害
        Burst,          // 連發：連擊加成
        Counter,        // 反擊：新放置方塊被擊中時反擊
        Execution,      // 處決：清除每列最底部方塊
        Repair,         // 修復：填補封閉空洞
        SpaceExpansion, // 空間擴充：解鎖儲存槽位
        ResourceExpansion, // 資源擴充：增加CP上限
        TacticalExpansion // 戰術擴展：解鎖技能（Lv1湮滅，Lv2處決，Lv3修補）
    }
}

