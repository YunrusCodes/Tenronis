# 04 - Buff 系統與成長路徑

> 數據取自 `PlayerManager.HandleBuffSelected()` 和 `GameConstants.cs`

---

## 1. Buff 分類

### 1.1 普通強化（6種）

通關後從隨機池中選擇。有等級上限。

| Buff | 效果 | 起始 | 上限 | 每級效果 |
|------|------|------|------|---------|
| **Salvo** 齊射 | 多行消除增傷 | 1 | 6 | 每額外行 +0.5 DMG/Lv |
| **Burst** 連發 | combo 增傷 | 1 | 6 | 每 combo +0.25 DMG/Lv |
| **Counter** 反擊 | 反擊導彈數 | 1 | 6 | 每級 +1 發反擊導彈 |
| **Explosion** 爆炸 | 充能上限 | 1 | 4 | 每級上限 +200 |
| **SpaceExpansion** 空間 | 儲存槽位 | 1 | 4 | 每級 +1 個儲存槽 |
| **ResourceExpansion** 資源 | CP上限 | 0 | 3 | 每級 CP上限 +50 |

### 1.2 傳奇強化（3種）

更稀有的 Buff，特殊效果或無上限。

| Buff | 效果 | 起始 | 上限 | 每級效果 |
|------|------|------|------|---------|
| **Defense** 裝甲 | 方塊HP增加 | 0 | **∞** | 所有方塊 HP +1 |
| **Volley** 協同火力 | 額外導彈 | 0 | 5 | 每格 +1 發導彈 |
| **TacticalExpansion** 戰術 | 解鎖技能 | 0 | 3 | Lv1/2/3 各解鎖1技能 |

---

## 2. 各 Buff 效果量化

### 2.1 Salvo（齊射強化）

```
額外傷害 = max(0, R-1) × Lv_salvo × 0.5

Lv1: R=2 → +0.5,  R=4 → +1.5
Lv3: R=2 → +1.5,  R=4 → +4.5
Lv6: R=2 → +3.0,  R=4 → +9.0
```

**特性**：消除 1 行時完全無效；消除 4 行時效果最大化。
**邊際收益**：每級在 R=4 時增加 1.5 DMG/發。

### 2.2 Burst（連發強化）

```
額外傷害 = Lv_burst × combo × 0.25

combo=1:  Lv1=0.25  Lv3=0.75  Lv6=1.5
combo=5:  Lv1=1.25  Lv3=3.75  Lv6=7.5
combo=10: Lv1=2.5   Lv3=7.5   Lv6=15.0
```

**特性**：combo 越高，等級收益越大。呈 **Lv × combo 二次增長**。
**邊際收益**：每級在 combo=10 時增加 2.5 DMG/發（極高）。

### 2.3 Counter（反擊強化）

```
反擊導彈數 = Lv_counter（骰子排列）
反擊傷害 = (1 + Lv_burst × combo × 0.25) × Lv_counter

Lv1: 1 發，骰子「中心」
Lv3: 3 發，骰子「對角線」
Lv6: 6 發，骰子「左右兩列各三」
```

**特性**：同時增加導彈數和啟用 0.3s combo 延遲。
**隱藏效果**：Counter Lv > 0 是維持 combo 的前提條件。

### 2.4 Explosion（爆炸充能）

```
充能上限 = 200 + (Lv - 1) × 200

Lv1: 200   Lv2: 400   Lv3: 600   Lv4: 800

充能速率（假設穩定消行）:
  每消除 1 次: +50
  每反擊 1 次: +5

  →消排 4 次 = 200 充能（Lv1 滿）
  →消排 16 次 = 800 充能（Lv4 滿）
```

**特性**：溢出時才觸發，屬於防禦型 Buff。
**與 Gravity 主題的協同**：Theme_4 頻繁溢出，充能使用頻率更高。

### 2.5 Volley（協同火力）

```
每格導彈數 = 1 + Lv_volley

Lv0: 1 發/格（基礎）
Lv1: 2 發/格
Lv3: 4 發/格
Lv5: 6 發/格

消除 1 行的導彈數 = 10 × (1 + Lv_volley)
Lv0: 10    Lv1: 20    Lv3: 40    Lv5: 60
```

**特性**：**乘法加成**，直接倍增所有導彈（消除和反擊都受益）。
**邊際收益**：Lv0→1 = +100%，Lv4→5 = +20%（遞減但仍為乘法）。

### 2.6 Defense（裝甲強化）

```
方塊 HP = 1 + Lv_defense

Lv0: HP=1（一擊即毀）
Lv1: HP=2（需2擊）
Lv5: HP=6（需6擊）
Lv10: HP=11
```

**特性**：無等級上限，但收益遞減（敵人射擊間隔固定，多 HP 只延遲破壞）。
**對垃圾方塊的影響**：敵人添加的垃圾方塊也受 Defense 加成。

### 2.7 SpaceExpansion（空間擴充）

```
可用儲存槽位 = Lv_space（初始 = 1）

Lv1: 1 個槽位（可暫存 1 個方塊）
Lv2: 2 個槽位
Lv3: 3 個槽位
Lv4: 4 個槽位
```

**特性**：提高操作靈活性，間接降低溢出風險。

### 2.8 ResourceExpansion（資源擴充）

```
CP 上限 = 100 + Lv_resource × 50

Lv0: 100 CP
Lv1: 150 CP
Lv2: 200 CP
Lv3: 250 CP
```

**特性**：更多 CP = 更多技能使用次數 + 溢出保險更充足。
**CP 恢復**：每關開始時 CP 恢復至全滿。

### 2.9 TacticalExpansion（戰術擴展）

```
Lv0: 無技能
Lv1: 解鎖「湮滅」(5 CP) — 幽靈穿透落下
Lv2: 解鎖「處決」(5 CP) — 削平頂部方塊 + 4 傷害
Lv3: 解鎖「修補」(30 CP) — 填補封閉空洞
```

**特性**：質變型 Buff，每級解鎖全新能力。

---

## 3. Buff 成長組合分析

### 3.1 傷害型路線（Speed Kill）

**優先**：Burst > Volley > Salvo

```
目標：最大化 DPS
核心循環：消除 → combo 累積 → 傷害指數增長

Lv_burst=6, Lv_volley=5, Lv_salvo=6, combo=10, R=4:
  DMG = 1 + 9 + 15 = 25
  導彈 = 4 × 10 × 6 = 240
  總傷害 = 6000（單次消除！）
```

### 3.2 防禦型路線（Fortress）

**優先**：Defense > Counter > Explosion

```
目標：方塊不被破壞，持續反擊
核心循環：方塊承受攻擊 → 反擊 → combo 維持

Lv_defense=5, Lv_counter=6:
  方塊 HP = 6（敵人需 6 次攻擊才摧毀）
  每次反擊 = 6 發導彈
  高 combo 反擊傷害可觀
```

### 3.3 控制型路線（Controller）

**優先**：TacticalExpansion > SpaceExpansion > ResourceExpansion

```
目標：技能管理板面
核心循環：湮滅清路 → 處決輸出 → 修補修復

技能組合：
  湮滅(5 CP): 快速穿透堆積方塊
  處決(5 CP): 削平表面 + 4 固定傷害
  修補(30 CP): 封閉空洞恢復（保險）
```

---

## 4. Buff 獲取經濟

### 4.1 單主題可獲 Buff 數

以 Theme_1 為例（最多 Buff）：

| 關卡 | 累計 Buff | 假設分配 |
|------|----------|---------|
| 1 | 1 | Burst Lv2 |
| 2 | 2 | Counter Lv2 |
| 3 | 3 | Salvo Lv2 |
| 4 | 4 | Burst Lv3 |
| 5 | 7 | Volley Lv1 + Salvo Lv3 + Burst Lv4 |
| 6 | 9 | Defense Lv1 + Explosion Lv2 |
| 7 | 12 | Counter Lv3 + Volley Lv2 + Salvo Lv4 |
| 8 | 15 | Burst Lv5 + Counter Lv4 + Defense Lv2 |
| 9 | 16 | Burst Lv6 |
| 10 | 21 | 5個 Buff（全面強化） |

### 4.2 普通 vs 傳奇 Buff 全滿需求

```
普通 Buff 全滿:
  Salvo(5) + Burst(5) + Counter(5) + Explosion(3) + Space(3) + Resource(3)
  = 24 次升級

傳奇 Buff 全滿（有上限的）:
  Volley(5) + TacticalExpansion(3)
  = 8 次升級
  + Defense(∞)

總計（不含 Defense）: 32 次升級
典型單主題提供: 16~21 次
```

→ **單主題不可能全滿所有 Buff**，必須策略性選擇。

---

## 5. Buff 升級優先級建議

### 通用優先級（不分主題）

| 優先 | Buff | 原因 |
|------|------|------|
| **S** | Counter (到 Lv2-3) | 啟用 combo 延遲 + 反擊傷害 |
| **A** | Burst (到 Lv4-6) | combo 傷害指數增長 |
| **A** | TacticalExpansion Lv1 | 解鎖湮滅（5CP的穿透技能） |
| **B** | Volley (到 Lv2-3) | 導彈數乘法加成 |
| **B** | Salvo (到 Lv3-4) | 多行消除增傷 |
| **C** | SpaceExpansion | 操作靈活性 |
| **C** | Explosion | 溢出保險傷害 |
| **D** | ResourceExpansion | CP 上限增加 |
| **D** | Defense | 前期價值低 |

### Theme 特化建議

| 主題 | 提升優先級 | 降低優先級 |
|------|-----------|-----------|
| Theme_2 (Inferno) | Defense → B | - |
| Theme_3 (Resentment) | Burst → S, Volley → A | Defense → D |
| Theme_4 (Gravity) | Explosion → A, Space → B | Salvo → C |

---

## 交叉引用

### 引用來源
- ← `01_Core_Variables.md` (常數)
- ← `02_Combat_Formulas.md` (傷害公式)
- ← `PlayerManager.cs` (Buff 升級邏輯)

### 被以下文檔使用
- → `05_Balance_Analysis.md` (Buff 對平衡的影響)
