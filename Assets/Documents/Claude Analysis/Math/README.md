# Tenronis - 數學模型與平衡分析

> **版本**: 3.0.0 (Code-Verified)
> **日期**: 2026-03-06
> **驗證基準**: 全部公式與常數均直接取自源碼，無任何假設值

---

## 文檔索引

| # | 文檔 | 內容 | 行數(約) |
|---|------|------|----------|
| 01 | [01_Core_Variables.md](01_Core_Variables.md) | 核心常數 & 變量定義 | ~400 |
| 02 | [02_Combat_Formulas.md](02_Combat_Formulas.md) | 傷害公式 & 戰鬥機制 | ~500 |
| 03 | [03_Stage_Data.md](03_Stage_Data.md) | 全5主題×10關完整數據表 | ~600 |
| 04 | [04_Buff_System.md](04_Buff_System.md) | Buff系統 & 成長路徑 | ~400 |
| 05 | [05_Balance_Analysis.md](05_Balance_Analysis.md) | 平衡分析 & 調整建議 | ~500 |

---

## 與舊文檔的關鍵差異

本文檔（v3.0）修正了 `Assets/Documentation/Math/` 中 v2.0 的**17處錯誤**。
完整差異清單見 `Claude Analysis/Documentation_Audit.md`。

### 最嚴重的修正

| 項目 | v2.0 舊值 | v3.0 正確值 | 影響 |
|------|-----------|-------------|------|
| BASE_MISSILE_DAMAGE | 2.0 | **1.0** | 所有傷害公式結果減半 |
| OVERFLOW_CP_COST | 25 | **75** | 溢出懲罰大幅增加 |
| 遊戲結構 | 單一20關線性 | **5主題×10關** | 整個難度模型需重建 |
| Annihilation 技能 | 不存在 | **Lv1解鎖，5CP** | 技能體系缺失一環 |
| 子彈選擇邏輯 | 優先級制 | **加權隨機** | 敵人威脅模型需修正 |

---

## 符號約定

### 常數（大寫底線）
- `D_base` = BASE_MISSILE_DAMAGE = 1.0
- `τ` = TICK_RATE = 0.8s
- `W` = BOARD_WIDTH = 10
- `H` = BOARD_HEIGHT = 20

### 玩家狀態變量
- `Lv_salvo` = 齊射等級 (起始1, 上限6)
- `Lv_burst` = 連發等級 (起始1, 上限6)
- `Lv_counter` = 反擊等級 (起始1, 上限6)
- `Lv_volley` = 協同火力等級 (起始0, 上限5)
- `Lv_defense` = 裝甲等級 (起始0, 無上限)
- `combo` = 當前連擊數
- `R` = 本次消除的非垃圾行數（上限4）

### 函數
- `DMG(R, combo)` = 單發導彈傷害
- `N_missiles(R)` = 單次消除總導彈數
- `DPS_total` = 單次消除總傷害輸出

---

## 源碼對應

| 文檔中的值 | 源碼文件 | 位置 |
|-----------|---------|------|
| 所有遊戲常數 | `Scripts/Data/GameConstants.cs` | 全文件 |
| 玩家初始狀態 | `Scripts/Data/BlockData.cs` → `PlayerStats()` | 第66-85行 |
| 傷害計算 | `Scripts/Managers/CombatManager.cs` → `HandleRowsCleared()` | 第111-115行 |
| 反擊公式 | `Scripts/Managers/CombatManager.cs` → `CheckCounterFire()` | 第659-660行 |
| Buff升級邏輯 | `Scripts/Managers/PlayerManager.cs` → `HandleBuffSelected()` | 第171-305行 |
| 溢出機制 | `Scripts/Managers/GridManager.cs` → `HandleOverflow()` | 第641-695行 |
| 方塊傷害 | `Scripts/Managers/GridManager.cs` → `DamageBlock()` | 第434-484行 |
| 關卡數據 | `ScriptableObjects/StageData/Theme_*/` | 各 .asset 文件 |
