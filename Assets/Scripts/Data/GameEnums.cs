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
    /// 子彈類型
    /// </summary>
    public enum BulletType
    {
        Normal,         // 普通子彈
        AddBlock,       // 添加方塊
        AreaDamage,     // 範圍傷害
        InsertRow       // 插入不可摧毀行
    }

    /// <summary>
    /// Buff類型
    /// </summary>
    public enum BuffType
    {
        Defense,        // 防禦：增加方塊HP
        Volley,         // 齊射：每消除一行增加導彈數量
        Heal,           // 治療：恢復HP
        Explosion,      // 爆炸：溢出時造成傷害
        Salvo,          // 協同：多行消除加成
        Burst,          // 連發：連擊加成
        Counter,        // 反擊：新放置方塊被擊中時反擊
        Execution,      // 處決：清除每列最底部方塊
        Repair          // 修復：填補封閉空洞
    }
}

