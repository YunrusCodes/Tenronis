# 02 - 戰鬥公式系統

> 所有公式直接取自 `CombatManager.cs`、`GridManager.cs`、`PlayerManager.cs`

---

## 1. 核心傷害公式

### 1.1 單發導彈傷害

來源：`CombatManager.HandleRowsCleared()` 第111-115行

```
effectiveRowCount = min(nonGarbageRows, 4)

salvoBonus = (effectiveRowCount > 1)
    ? (effectiveRowCount - 1) × Lv_salvo × 0.5
    : 0

burstBonus = Lv_burst × combo × 0.25

DMG = 1.0 + salvoBonus + burstBonus
```

**公式展開：**

```
DMG(R, combo) = 1 + max(0, R-1) × Lv_salvo × 0.5 + Lv_burst × combo × 0.25
```

其中：
- `R` = min(非垃圾行數, 4)
- `combo` = 當前連擊數（消除前已+1）

### 1.2 每次消除的導彈數量

來源：`CombatManager.HandleRowsCleared()` 第118-134行

```
每行發射的導彈 = W × (1 + Lv_volley)
總導彈數 = nonGarbageRows × W × (1 + Lv_volley)
```

> **注意**：這裡用的是 `nonGarbageRows`（實際非垃圾行數，不受 cap 4 限制），
> 但傷害計算中的 `effectiveRowCount` 受 cap 4 限制。

### 1.3 單次消除總傷害

```
DPS_total = DMG(R, combo) × nonGarbageRows × W × (1 + Lv_volley)
```

---

## 2. 範例計算

### 範例 1：起始狀態消除 1 行

```
條件：Lv_salvo=1, Lv_burst=1, Lv_volley=0, combo=1（第一次消除）

DMG = 1 + max(0, 1-1)×1×0.5 + 1×1×0.25
    = 1 + 0 + 0.25
    = 1.25

導彈數 = 1 × 10 × (1+0) = 10
總傷害 = 1.25 × 10 = 12.5
```

### 範例 2：起始狀態消除 2 行

```
條件：同上, R=2

DMG = 1 + (2-1)×1×0.5 + 1×1×0.25
    = 1 + 0.5 + 0.25
    = 1.75

導彈數 = 2 × 10 × 1 = 20
總傷害 = 1.75 × 20 = 35
```

### 範例 3：中期強化消除 4 行（combo=5）

```
條件：Lv_salvo=3, Lv_burst=3, Lv_volley=2, combo=5

DMG = 1 + (4-1)×3×0.5 + 3×5×0.25
    = 1 + 4.5 + 3.75
    = 9.25

導彈數 = 4 × 10 × (1+2) = 120
總傷害 = 9.25 × 120 = 1110
```

### 範例 4：滿級消除 4 行（combo=10）

```
條件：Lv_salvo=6, Lv_burst=6, Lv_volley=5, combo=10

DMG = 1 + 3×6×0.5 + 6×10×0.25
    = 1 + 9 + 15
    = 25

導彈數 = 4 × 10 × 6 = 240
總傷害 = 25 × 240 = 6000
```

---

## 3. 反擊系統

來源：`CombatManager.CheckCounterFire()` 第644-688行

### 3.1 觸發條件

```
方塊放置後 0.2 秒內被敵人子彈命中
  → 時間判定: Time.time - block.createdTime <= 0.2
  → 需要 counterFireLevel > 0
```

### 3.2 反擊傷害

```
反擊前：combo++ (CancelComboReset + 直接遞增)
反擊導彈數 = Lv_counter（骰子點數排列）

burstBonus_counter = Lv_burst × combo_new × 0.25
DMG_counter = 1.0 + burstBonus_counter
```

> **注意**：反擊傷害**不含** salvoBonus（因為不是消除行觸發的）。

### 3.3 反擊範例

```
條件：Lv_counter=3, Lv_burst=2, 當前combo=4

反擊觸發：combo → 5
DMG_counter = 1 + 2×5×0.25 = 1 + 2.5 = 3.5
反擊導彈數 = 3
反擊傷害 = 3.5 × 3 = 10.5

額外充能：+5 爆炸充能
```

---

## 4. 虛無抵銷系統

來源：`CombatManager.HandleRowsCleared()` 第97-103行

```
if (hasVoid == true):
    不發射任何導彈（整次消除無效化）
    return
```

### 判定規則

- 消除的行中**任意一格**是 Void 方塊 → `hasVoid = true`
- 整次消除的**所有行**都不產生導彈（不是只有含 Void 的行）
- Combo 仍然正常累加（+1）
- 分數仍然正常計算（行數×100）
- 爆炸充能仍然正常增加（+50）

### 影響分析

虛無抵銷只阻止導彈發射，不影響：
- combo 計數
- 分數計算
- 爆炸充能累積

---

## 5. 溢出與爆炸充能系統

來源：`GridManager.HandleOverflow()` 第641-695行

### 5.1 溢出觸發條件

- 方塊鎖定時頂行有方塊（標準俄羅斯方塊溢出）
- 插入垃圾行時頂行有方塊
- AddBlock 子彈在 y=0 位置嘗試添加方塊

### 5.2 溢出處理流程

```
1. 清空整個網格
2. CP 判定:
   if (CP >= 75): CP -= 75
   else:          CP = 0, HP = 1
3. 爆炸充能判定:
   if (explosionCharge > 0):
     對敵人造成 explosionCharge 點傷害
     explosionCharge = 0
```

### 5.3 爆炸充能數值表

| 等級 | 充能上限 | 充能來源 | 滿充傷害 |
|------|---------|---------|---------|
| Lv1 | 200 | 消排×50 + 反擊×5 | 200 |
| Lv2 | 400 | 同上 | 400 |
| Lv3 | 600 | 同上 | 600 |
| Lv4 | 800 | 同上 | 800 |

---

## 6. 技能傷害系統

### 6.1 湮滅 (Annihilation)

```
消耗: 5 CP
效果: 當前方塊進入「幽靈穿透」狀態，落下時摧毀路徑上的方塊
觸發: combo+1（每破壞一次觸發一次 OnAnnihilationDestroy）
```

### 6.2 處決 (Execution)

```
消耗: 5 CP
效果: 清除每列最頂部的非垃圾方塊（不是最底部）
傷害: EXECUTION_DAMAGE = 4（直接對敵人造成 4 傷害）
觸發: combo+1
前提: 場上必須有可被處決的方塊（HasExecutableBlocks 檢查）
```

### 6.3 修補 (Repair)

```
消耗: 30 CP
效果: 填補網格中的封閉空洞（BFS 檢測不與頂部連通的空格）
前提: 必須存在封閉空洞（HasClosedHoles 檢查）
```

---

## 7. 分數系統

來源：`PlayerManager.HandleRowsCleared()` 第338行

```
分數增加 = clearedRows.Count × 100
```

> 極為簡單：每消除1行+100分，無 combo 加成、無行數加成。

---

## 8. 敵人受傷來源匯總

| 來源 | 傷害計算 | 觸發條件 |
|------|---------|---------|
| 消除行導彈 | DMG × R × W × (1+Lv_volley) | 消除非垃圾行且無虛無 |
| 反擊導彈 | DMG_counter × Lv_counter | 0.2s內方塊被擊中 |
| 處決技能 | 4.0（固定） | 使用處決技能 |
| 爆炸充能 | 當前充能值（最大800） | 溢出時觸發 |

---

## 9. 玩家受傷來源匯總

| 來源 | 傷害 | 觸發條件 |
|------|------|---------|
| 子彈穿透到底 | 10 HP | 子彈未被方塊攔截 |
| 爆炸方塊被破壞 | 5 HP | Explosive 方塊被敵人子彈摧毀 |
| 不可摧毀方塊反傷 | 10 HP | 任何攻擊命中不可摧毀方塊 |
| 溢出（CP不足） | HP→1 | 溢出時 CP < 75 |

---

## 10. 傷害公式偏導數分析

### 對 Lv_salvo 的偏導

```
∂DMG/∂Lv_salvo = max(0, R-1) × 0.5
  R=1: 0（齊射無效果）
  R=2: 0.5
  R=3: 1.0
  R=4: 1.5
```

→ 齊射只在消除 2+ 行時有效，且效果隨行數線性增長。

### 對 Lv_burst 的偏導

```
∂DMG/∂Lv_burst = combo × 0.25
  combo=1: 0.25
  combo=5: 1.25
  combo=10: 2.5
```

→ 連發效果隨 combo 線性增長，高 combo 時收益極高。

### 對 Lv_volley 的偏導

```
∂N_missiles/∂Lv_volley = R × W = R × 10
  R=1: 10
  R=4: 40
```

→ 協同火力直接倍增導彈數量，乘法效果。

### 敏感度比較（起始 R=2, combo=3, 各Lv=2）

```
基礎 DMG = 1 + 1×2×0.5 + 2×3×0.25 = 1 + 1 + 1.5 = 3.5

Lv_salvo +1: DMG = 1 + 1×3×0.5 + 1.5 = 4.0 (+14%)
Lv_burst +1: DMG = 1 + 1 + 3×3×0.25 = 4.25 (+21%)
Lv_volley +1: 導彈數 ×4/3 (+33%)
```

→ **Lv_volley 的邊際效益最高**（乘法加成），其次是 Lv_burst（高 combo 回報），最後是 Lv_salvo。

---

## 交叉引用

### 引用來源
- ← `01_Core_Variables.md` (常數定義)
- ← `CombatManager.cs` (傷害計算、反擊)
- ← `GridManager.cs` (溢出機制、方塊傷害)
- ← `PlayerManager.cs` (技能、combo 管理)

### 被以下文檔使用
- → `03_Stage_Data.md` (套用公式計算擊殺時間)
- → `05_Balance_Analysis.md` (平衡分析)
