# 01 - 核心變量定義

> 所有數值直接取自 `GameConstants.cs` 和 `BlockData.cs`（PlayerStats 建構函數）

---

## 1. 遊戲板常數

| 常數 | 值 | 代碼名稱 | 說明 |
|------|----|---------|------|
| 寬度 W | 10 | `BOARD_WIDTH` | 網格橫向格數 |
| 高度 H | 20 | `BOARD_HEIGHT` | 網格縱向格數 |
| Tick 間隔 τ | 0.8s | `TICK_RATE` | 方塊自然下落間隔 |

---

## 2. 玩家常數

| 常數 | 值 | 代碼名稱 | 說明 |
|------|----|---------|------|
| 最大 HP | 100 | `PLAYER_MAX_HP` | |
| 最大 CP | 100 | `PLAYER_MAX_CP` | Castle Point 初始上限 |
| 溢出 CP 消耗 | **75** | `OVERFLOW_CP_COST` | 溢出時消耗的 CP 量 |

### 溢出規則（GridManager.HandleOverflow）

```
if (CP >= 75):
    CP -= 75          // 消耗 75 CP
else:
    CP = 0             // CP 歸零
    HP = 1             // HP 強制降至 1（瀕死）
// 之後：清空網格 + 觸發爆炸充能傷害
```

---

## 3. 導彈常數

| 常數 | 值 | 代碼名稱 | 說明 |
|------|----|---------|------|
| 基礎傷害 D_base | **1.0** | `BASE_MISSILE_DAMAGE` | 單發導彈基礎傷害 |
| 齊射倍率 μ_salvo | 0.5 | `SALVO_DAMAGE_MULTIPLIER` | 每額外行×每級增傷 |
| 連發倍率 μ_burst | 0.25 | `BURST_DAMAGE_MULTIPLIER` | 每級×每combo增傷 |
| 導彈速度 | 20.0 | `MISSILE_SPEED` | 單位/秒 |
| 處決傷害 | 4.0 | `EXECUTION_DAMAGE` | 處決技能對敵人的傷害 |

---

## 4. 方塊常數

| 常數 | 值 | 代碼名稱 | 說明 |
|------|----|---------|------|
| 基礎方塊 HP | 1 | `BASE_BLOCK_HP` | 玩家放置的方塊 |
| 垃圾方塊 HP | 1 | `GARBAGE_BLOCK_HP` | 敵人添加的方塊 |
| 不可摧毀 HP | 9999 | `INDESTRUCTIBLE_BLOCK_HP` | 垃圾行方塊 |

### 方塊特性類型（BlockType）

| 類型 | 效果 |
|------|------|
| Normal | 無特性 |
| Void | **虛無抵銷**：消除行中含有 Void 方塊 → 整次消除不發射導彈 |
| Explosive | 被敵人子彈破壞時，對玩家造成 **5 HP** 傷害 |

### 不可摧毀方塊反傷

- 不可摧毀方塊被攻擊（包括敵人子彈命中） → 對玩家反傷 **10 HP**
- 來源：`GridManager.DamageBlock()` 第442-446行

---

## 5. 敵人子彈常數

| 常數 | 值 | 代碼名稱 | 說明 |
|------|----|---------|------|
| 子彈傷害 | 10 | `BULLET_DAMAGE` | 子彈穿透到底部打中城堡的傷害 |

### 子彈類型（BulletType）

| 類型 | 代碼名 | 效果 |
|------|--------|------|
| Normal | `Normal` | 破壞命中方塊 1 HP |
| AreaDamage | `AreaDamage` | 3×3 範圍各 1 HP |
| AddBlock | `AddBlock` | 破壞命中方塊 + 在上方添加 1 個普通垃圾方塊 |
| AddExplosiveBlock | `AddExplosiveBlock` | 破壞命中方塊 + 在上方添加 1 個爆炸垃圾方塊 |
| InsertRow | `InsertRow` | 破壞命中方塊 + 從底部插入普通不可摧毀行 |
| InsertVoidRow | `InsertVoidRow` | 破壞命中方塊 + 從底部插入虛無不可摧毀行 |
| CorruptExplosive | `CorruptExplosive` | 破壞命中方塊 + 腐化下個方塊的隨機一格為爆炸方塊 |
| CorruptVoid | `CorruptVoid` | 破壞命中方塊 + 腐化下個方塊的隨機一格為虛無方塊 |

> **子彈選擇機制**：加權隨機（Weighted Random）。
> 每個關卡配置中，啟用的子彈類型各有一個 `chance` 值，
> 實際機率 = 該子彈 chance / 所有啟用子彈 chance 之和。
> 來源：`EnemyController.DetermineBulletType()`

---

## 6. 反擊 & Combo 常數

| 常數 | 值 | 代碼名稱 | 說明 |
|------|----|---------|------|
| 反擊時間窗口 | 0.2s | `COUNTER_FIRE_TIME_WINDOW` | 方塊放置後的可彈反窗口 |
| Combo 重置延遲 | 0.3s | `COMBO_RESET_DELAY` | 有反擊時的重置延遲 |

### Combo 重置規則（PlayerManager.HandlePieceLocked）

```
if (counterFireLevel > 0):
    啟動 0.3s 計時器，到期後重置 combo = 0
    （期間如果消除行或觸發反擊，計時器取消）
else:
    立即 combo = 0
```

---

## 7. 爆炸充能常數

| 常數 | 值 | 代碼名稱 | 說明 |
|------|----|---------|------|
| 初始充能上限 | 200 | `EXPLOSION_INITIAL_MAX_CHARGE` | |
| 反擊充能量 | 5 | `EXPLOSION_COUNTER_CHARGE` | 每次反擊 +5 |
| 消排充能量 | 50 | `EXPLOSION_ROW_CLEAR_CHARGE` | 每次消除行 +50 |
| Buff 充能增量 | +200 | `EXPLOSION_BUFF_MAX_CHARGE_INCREASE` | 每升一級上限 +200 |
| Buff 最高等級 | 4 | `EXPLOSION_BUFF_MAX_LEVEL` | |

### 爆炸充能計算

```
充能上限 = 200 + (explosionChargeLevel - 1) × 200
  Lv1: 200 (初始)
  Lv2: 400
  Lv3: 600
  Lv4: 800 (滿級)

充能來源：
  每次消除行: +50
  每次反擊:   +5

觸發：溢出時，將當前充能值作為傷害直擊敵人
```

---

## 8. 技能 CP 消耗

| 技能 | CP消耗 | 解鎖條件 | 代碼名稱 |
|------|--------|---------|---------|
| 湮滅 Annihilation | 5 | TacticalExpansion Lv1 | `ANNIHILATION_CP_COST` |
| 處決 Execution | 5 | TacticalExpansion Lv2 | `EXECUTION_CP_COST` |
| 修補 Repair | 30 | TacticalExpansion Lv3 | `REPAIR_CP_COST` |

---

## 9. Buff 等級上限

### 普通強化（Normal Buffs）

| Buff | 起始 | 上限 | 代碼常數 |
|------|------|------|---------|
| Salvo 齊射 | **1** | 6 | `SALVO_MAX_LEVEL` |
| Burst 連發 | **1** | 6 | `BURST_MAX_LEVEL` |
| Counter 反擊 | **1** | 6 | `COUNTER_MAX_LEVEL` |
| Explosion 爆炸 | **1** | 4 | `EXPLOSION_BUFF_MAX_LEVEL` |
| SpaceExpansion 空間擴充 | **1** | 4 | `SPACE_EXPANSION_MAX_LEVEL` |
| ResourceExpansion 資源擴充 | 0 | 3 | `RESOURCE_EXPANSION_MAX_LEVEL` |

### 傳奇強化（Legendary Buffs）

| Buff | 起始 | 上限 | 代碼常數 |
|------|------|------|---------|
| Defense 裝甲 | 0 | **無上限** | `DEFENSE_START_LEVEL` |
| Volley 協同火力 | 0 | 5 | `VOLLEY_MAX_LEVEL` |
| TacticalExpansion 戰術擴展 | 0 | 3 | `TACTICAL_EXPANSION_MAX_LEVEL` |

> **注意**：Heal（治療）已廢棄，代碼中 BuffType.Heal 存在但不在任何 Buff 池中。
> 傳奇 Buff 共 **3 種**（非4種）。

---

## 10. 派生變量

### 實際方塊 HP

```
玩家方塊 HP = BASE_BLOCK_HP + Lv_defense = 1 + Lv_defense
垃圾方塊 HP = GARBAGE_BLOCK_HP + Lv_defense = 1 + Lv_defense
```

### CP 上限

```
CP_max = PLAYER_MAX_CP + cpExpansionLevel × 50
  Lv0: 100
  Lv1: 150
  Lv2: 200
  Lv3: 250 (滿級)
```

### 儲存槽位

```
已解鎖槽位數 = spaceExpansionLevel
  Lv1: 1 個槽位 (初始)
  Lv2: 2 個槽位
  Lv3: 3 個槽位
  Lv4: 4 個槽位 (滿級)
```

---

## 交叉引用

### 引用來源
- `GameConstants.cs` → 所有常數定義
- `BlockData.cs:PlayerStats()` → 起始等級定義
- `GameEnums.cs` → 所有枚舉定義

### 被以下文檔使用
- → `02_Combat_Formulas.md` (傷害公式)
- → `03_Stage_Data.md` (關卡數據分析)
- → `04_Buff_System.md` (Buff效果量化)
- → `05_Balance_Analysis.md` (平衡分析)
