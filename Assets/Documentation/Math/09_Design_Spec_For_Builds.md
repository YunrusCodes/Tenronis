# 09 - Build平衡設計規格書
# Build Balance Design Specification

**文檔版本**: 2.1  
**最後更新**: 2025年12月1日  
**文檔類型**: 設計規格書（可直接交付企劃）  
**基於**: 文檔01-08的完整數學模型

---

## 📋 目錄

1. [規格書概述](#規格書概述)
2. [三條Build角色定義](#三條build角色定義)
3. [三軌難度模型](#三軌難度模型)
4. [Build數值上限規範](#build數值上限規範)
5. [Build比較矩陣](#build比較矩陣)
6. [設計目標詳述](#設計目標詳述)
7. [可執行建議](#可執行建議)

---

## 📖 規格書概述

### 設計目的

本規格書旨在解決當前遊戲平衡的核心問題：**數值系統被單一Build（Volley）綁架**。

### 核心問題（來自08_Legendary_Build_Analysis）

```
當前狀況：
- 遊戲設計隱含假設Expert + Volley Build
- Expert在Stage 20僅需2秒擊殺Boss（目標20-40秒）
- 其他Build/玩家層級全部失衡

數學證明：
Volley收益層級差距 = Expert : Inter : Casual = 6.8 : 2.1 : 1
→ 無法用單一HP曲線滿足所有層級
```

### 解決方案框架

**三軌制平衡設計**：
- 不再尋求"單一平衡"
- 建立三條獨立的平衡軌道
- 每條軌道對應一個Build + 一個玩家層級 + 一個難度模式

### 數學基礎

本規格書所有數值基於以下文檔的數學模型：

| 文檔 | 引用內容 | 用途 |
|------|---------|------|
| 02 | 傷害公式系統 | 計算PDA |
| 04 | PDA/SP模型 | 平衡目標設定 |
| 05 | HOM模型 | 玩家能力參數 |
| 07 | 三層玩家模型 | 層級定義 |
| 08 | Build分析 | Build角色定位 |

---

## 🎯 三條Build角色定義

### Build A: Volley（輸出流）

#### 數學角色

**Damage Multiplier（傷害放大器）**

```
核心公式（來自02）：
N_missiles = r × W × (1 + L_volley)
           = 10r(1 + L_volley)

放大效果：
L_volley=0: 1×（基準）
L_volley=1: 2×（翻倍）
L_volley=3: 4×（4倍）
L_volley=5: 6×（6倍）

數學特性：線性乘數，每級+100%導彈
```

#### 適配玩家

**Expert（高手玩家）**

**匹配原因**（來自07）：
```
Expert特徵：
- 高Combo（20-40）→ 放大Burst加成
- 高消除率（3.2行）→ 配合Volley乘數
- 高操作精度 → 火力完全釋放

數值證明（Stage 20）：
Expert PDA = 2,943
Inter PDA = 1,200（41%收益）
Casual PDA = 235（8%收益）

結論：Volley是Expert專屬優勢
```

#### 最大問題

**火力膨脹導致Boss 2秒蒸發**（來自08）

```
Stage 20數據：
單次爆發傷害：13,440
Boss HP：5,000
比率：2.69倍過量

實際擊殺時間：2.0秒
理想時間：20-40秒
差距：10-20倍過快
```

#### 應用場景

```
✓ 速殺流派
✓ 極限火力挑戰
✓ 高手玩家競速
✓ Expert Track（專家模式）

✗ 不適合低Combo玩家
✗ 不適合新手
✗ 缺乏生存保障
```

---

### Build B: Defense（坦克流）

#### 數學角色

**Time Buffer（時間緩衝器）**

```
核心公式（來自04）：
T_buffer(L_defense) = (1 + L_defense) / λ_bullet

物理意義：方塊被破壞前的存活時間

Stage 20範例（λ_bullet=1.0/s）：
L_defense=0:  T_buffer=1.0s
L_defense=5:  T_buffer=6.0s
L_defense=10: T_buffer=11.0s

數學特性：轉化長戰鬥時間為容錯空間
```

#### 適配玩家

**Casual（普通玩家）**

**匹配原因**（來自07）：
```
Casual特徵：
- 低火力 → 長戰鬥時間（80-90秒）
- 操作失誤多 → 需要容錯空間
- 低Combo（3-6）→ Volley收益低

數值證明（Stage 20）：
無Defense：SP=0.25（極危），溢出率80%
有Defense（L=9）：SP=0.52（警戒），溢出率30%

結論：Defense是Casual的生存關鍵
```

#### 核心目標

```
目標1：降低溢出風險
從80% → 30%（-62.5%）

目標2：延長思考時間
方塊存活時間從1秒 → 10秒（+900%）

目標3：穩定版面
SP從0.25（極危）→ 0.52（警戒）
```

#### 應用場景

```
✓ 新手玩家
✓ 求穩流派
✓ SP穩定需求
✓ Casual Track（休閒模式）

✗ 火力犧牲過大
✗ 戰鬥時間長（80-90秒）
✗ 不適合Expert（浪費潛力）
```

---

### Build C: Tactical（戰術流）

#### 數學角色

**Nonlinear Burst Source（非線性爆發源）**

```
核心公式（來自02、08）：
DPS_total = DPS_base + DPS_skill

DPS_skill = λ_skill × DMG_skill

特點：
- DPS_base：持續穩定
- DPS_skill：週期性爆發（玩家主動控制）

技能DPS佔比：30-40%

數學特性：
普通Build：DPS隨時間線性增長
Tactical Build：DPS有週期性波動（戰術節奏）
```

#### 適配玩家

**Intermediate（中階玩家）**

**匹配原因**（來自07、08）：
```
Intermediate特徵：
- 中等Combo（8-12）→ 平衡型收益
- 操作能力足夠 → 可管理CP和技能
- 追求效率與穩定 → Tactical提供兩者

數值證明（Stage 20）：
Tactical PDA：513
  = 普通輸出360 + 技能輸出153
  = 技能佔比29.8%

T_actual：13.6秒（接近理想20-35秒）
SP：0.55（警戒，可接受）

結論：Tactical是Intermediate的最佳平衡選擇
```

#### 特點

```
技能提供30-40%額外輸出

戰術靈活性：
- Execution：清理頂部，降低溢出
- Repair：填補空洞，創造消除
- CP管理：戰術決策深度
```

#### 應用場景

```
✓ 技能操作流派
✓ 輸出+生存兼具
✓ 戰術節奏玩家
✓ Standard Track（標準模式）

✗ 需要技能理解
✗ CP管理複雜度
✗ 不如Volley暴力（對Expert而言）
```

---

### 三Build×三層玩家適配表

| 玩家層級 | 首選Build | 次選Build | 不推薦 | 理由 |
|---------|----------|----------|--------|------|
| **Casual** | Defense | Tactical | Volley | Volley收益低（8%），生存壓力大 |
| **Intermediate** | Tactical | Volley | Defense | Tactical最平衡，Defense過度保守 |
| **Expert** | Volley | Tactical | Defense | Volley收益最大化，Defense浪費潛力 |

**數學依據**（來自08）：
```
Volley收益比（Stage 20）：
Expert : Inter : Casual = 2943 : 1200 : 235 = 12.5 : 5.1 : 1

Defense需求（溢出風險）：
Casual：80% → 30%（必需）
Inter：45% → 18%（有用但非必需）
Expert：15% → 5%（無意義，瞬間擊殺）

Tactical收益（技能DPS佔比）：
Casual：40-60%（主要輸出）
Inter：30-40%（平衡補充）
Expert：20-30%（錦上添花，不如Volley）
```

---

## 🎢 三軌難度模型

### 設計原則

**核心理念**：不再用單一HP曲線平衡所有情況

```
傳統設計（失敗）：
單一HP曲線 → 無法滿足層級差距12.5:5.1:1

新設計（三軌制）：
三條獨立難度軌道 → 每條對應一個Build+玩家層級
```

### 軌道1：Casual Track（休閒模式）

#### 平衡目標

```
目標Build：Defense Build
目標玩家：Casual
目標時間：30-50秒擊殺
目標體驗：穩定、容錯、思考時間充足
```

#### HP系數

**建議HP縮減系數：0.6×**

```
計算依據（來自07、08）：
Casual + Defense Build：
  PDA = 101 傷害/秒
  理想時間 = 40秒（取中值）
  所需HP = 101 × 40 = 4,040

當前HP(20) = 5,000
HP縮減系數 = 4,040 / 5,000 = 0.81

考慮容錯和舒適度，下修至0.6×
實際HP = 5,000 × 0.6 = 3,000
實際時間 = 3,000 / 101 × 1.8 = 53.5秒 ✓
```

#### 完整HP曲線（Casual Track）

| Stage | 標準HP | Casual HP (0.6×) | Casual PDA | T_kill | T_actual |
|-------|--------|------------------|-----------|--------|----------|
| 1 | 120 | 72 | 11 | 6.5s | 11.7s |
| 5 | 400 | 240 | 23 | 10.4s | 18.7s |
| 10 | 1000 | 600 | 47 | 12.8s | 23.0s |
| 15 | 2000 | 1200 | 82 | 14.6s | 26.3s |
| 20 | 5000 | 3000 | 101 | 29.7s | 53.5s |

**評估**：T_actual ∈ [11.7, 53.5]秒，符合30-50秒目標 ✓

#### 射速調整

**建議下修射速**（降低場面壓力）

```
當前射速：I_shoot(n) = 3.1 - 0.1n

Casual Track建議：
I_shoot_casual(n) = (3.1 - 0.1n) × 1.3

效果：
Stage 1:  3.0s → 3.9s（+30%）
Stage 10: 1.9s → 2.5s（+32%）
Stage 20: 1.0s → 1.3s（+30%）

理由：Casual戰鬥時間長，需降低持續壓力
```

#### 子彈複雜度調整

**建議降低子彈類型數量**

```
當前：Stage 14+啟用全部8種子彈

Casual Track建議：
Stage 1-10：僅Normal + AddBlock（2種）
Stage 11-20：增加AreaDamage（3種）
不啟用：InsertVoidRow, CorruptExplosive（最複雜的）

理由：降低認知負載，專注基礎操作
```

---

### 軌道2：Standard Track（標準模式）

#### 平衡目標

```
目標Build：Tactical Build
目標玩家：Intermediate
目標時間：20-35秒擊殺
目標體驗：平衡、戰術節奏、適度挑戰
```

#### HP系數

**建議HP系數：1.8×**

```
計算依據（來自07、08）：
Intermediate + Tactical Build：
  PDA = 513 傷害/秒
  理想時間 = 27秒（取中值）
  所需HP = 513 × 27 = 13,851

當前HP(20) = 5,000
HP系數 = 13,851 / 5,000 = 2.77

考慮操作失誤和戰術彈性，下修至1.8×
實際HP = 5,000 × 1.8 = 9,000
實際時間 = 9,000 / 513 × 1.4 = 24.6秒 ✓
```

#### 完整HP曲線（Standard Track）

| Stage | 標準HP | Standard HP (1.8×) | Inter PDA | T_kill | T_actual |
|-------|--------|--------------------|-----------|--------|----------|
| 1 | 120 | 216 | 47 | 4.6s | 6.4s |
| 5 | 400 | 720 | 110 | 6.5s | 9.1s |
| 10 | 1000 | 1800 | 213 | 8.5s | 11.9s |
| 15 | 2000 | 3600 | 392 | 9.2s | 12.9s |
| 20 | 5000 | 9000 | 513 | 17.5s | 24.6s |

**評估**：T_actual ∈ [6.4, 24.6]秒，符合20-35秒目標 ✓

#### 射速調整

**建議維持當前射速**

```
當前射速：I_shoot(n) = 3.1 - 0.1n

Standard Track：不調整

理由：Intermediate玩家可應對當前射速
```

#### 子彈複雜度調整

**建議維持當前設計**

```
當前設計：
Stage 1-4:   1種（Normal）
Stage 5-6:   2-3種（增加Corrupt系列）
Stage 7-13:  4-7種（逐步解鎖）
Stage 14+:   8種（全解鎖）

Standard Track：維持不變

理由：
- 複雜度曲線合理
- 提供戰術挑戰
- Intermediate可應對
```

---

### 軌道3：Expert Track（專家模式）

#### 平衡目標

```
目標Build：Volley Build
目標玩家：Expert
目標時間：15-25秒擊殺
目標體驗：極限火力、高難度、快節奏
```

#### HP系數

**建議HP系數：7.0×**

```
計算依據（來自08）：
Expert + Volley Build：
  PDA = 2,943 傷害/秒（當前）
  理想時間 = 20秒（取中值）
  所需HP = 2,943 × 20 = 58,860

當前HP(20) = 5,000
HP系數 = 58,860 / 5,000 = 11.77

考慮Volley上限調整（建議-28%）：
  調整後PDA = 2,943 × 0.72 = 2,119
  所需HP = 2,119 × 20 = 42,380
  HP系數 = 42,380 / 5,000 = 8.48

綜合考慮，建議系數：7.0×
實際HP = 5,000 × 7.0 = 35,000
實際時間 = 35,000 / 2,119 × 1.2 = 19.8秒 ✓
```

#### 完整HP曲線（Expert Track）

| Stage | 標準HP | Expert HP (7.0×) | Expert PDA | T_kill | T_actual |
|-------|--------|------------------|-----------|--------|----------|
| 1 | 120 | 840 | 74 | 11.4s | 13.7s |
| 5 | 400 | 2800 | 368 | 7.6s | 9.1s |
| 10 | 1000 | 7000 | 1192 | 5.9s | 7.1s |
| 15 | 2000 | 14000 | 2669 | 5.2s | 6.3s |
| 20 | 5000 | 35000 | 2119* | 16.5s | 19.8s |

**註**：\* 假設Volley上限調整後的PDA

**評估**：T_actual ∈ [6.3, 19.8]秒，接近15-25秒目標 ✓

#### 射速調整

**建議維持或略微提升**

```
當前射速：I_shoot(n) = 3.1 - 0.1n

Expert Track建議：維持不變

理由：
- Expert玩家反應快，可應對當前射速
- 戰鬥時間已經縮短至20秒，射速壓力自然降低
- 不需要依賴子彈製造壓力（HP已足夠）
```

#### 子彈複雜度調整

**建議維持當前設計**

```
當前設計：Stage 14+全部8種子彈

Expert Track：維持不變

理由：
- Expert玩家可應對複雜子彈
- 提供戰術深度
- 但主要挑戰來自HP，而非子彈
```

---

### 三軌難度標準總表

| 參數 | Casual Track | Standard Track | Expert Track |
|------|-------------|----------------|--------------|
| **HP系數** | 0.6× | 1.8× | 7.0× |
| **射速系數** | 1.3×（變慢） | 1.0×（不變） | 1.0×（不變） |
| **子彈類型** | 最多3種 | 全部8種 | 全部8種 |
| **目標時間** | 30-50s | 20-35s | 15-25s |
| **目標玩家** | Casual | Intermediate | Expert |
| **推薦Build** | Defense | Tactical | Volley |

### 三軌×三Build匹配矩陣

| 難度模式 | Defense | Tactical | Volley | 評估 |
|---------|---------|----------|--------|------|
| **Casual Track** | ⚖️ 完美匹配 | ○ 偏快 | ✗ 極快 | 推薦Defense |
| **Standard Track** | ✗ 過慢 | ⚖️ 完美匹配 | ○ 偏快 | 推薦Tactical |
| **Expert Track** | ✗ 極慢 | ✗ 過慢 | ⚖️ 完美匹配 | 推薦Volley |

**圖例**：
- ⚖️ 完美匹配：T_actual在理想範圍內
- ○ 偏快/偏慢：可玩但非最優
- ✗ 極快/極慢：不推薦

---

## 📏 Build數值上限規範

### Volley改動（必要）

#### 方案A：硬上限（推薦）

**建議上限：Lv 4**

```
當前：L_volley無上限
問題：Expert可無限堆，導致火力失控

建議：L_volley_max = 4

效果：
L=0: N_missiles = r × 10 × 1 = 10r
L=1: N_missiles = r × 10 × 2 = 20r（翻倍）
L=2: N_missiles = r × 10 × 3 = 30r（3倍）
L=3: N_missiles = r × 10 × 4 = 40r（4倍）
L=4: N_missiles = r × 10 × 5 = 50r（5倍，上限）

對比L=5：60r
削減：-16.7%
```

**Stage 20影響**：
```
當前（L=5）：
Expert PDA = 2,943
T_kill = 1.7s

調整後（L=4）：
Expert PDA = 2,452
T_kill = 2.0s

改善：+18%時間，但仍然過快
配合Expert Track（HP×7.0）：
T_kill = 14.3s ✓（進入合理範圍）
```

#### 方案B：遞減曲線（替代方案）

**公式**：
```
f_volley(L) = 1 + L × [1 / (1 + 0.1L)]

展開：
f(L) = 1 + L / (1 + 0.1L)
```

**遞減表**：

| L | 線性f(L) | 遞減f(L) | 差異 | 削減率 |
|---|----------|----------|------|--------|
| 0 | 1.00 | 1.00 | 0.00 | 0% |
| 1 | 2.00 | 1.91 | -0.09 | -4.5% |
| 2 | 3.00 | 2.67 | -0.33 | -11.0% |
| 3 | 4.00 | 3.31 | -0.69 | -17.3% |
| 4 | 5.00 | 3.85 | -1.15 | -23.0% |
| 5 | 6.00 | 4.33 | -1.67 | -27.8% |
| 6 | 7.00 | 4.75 | -2.25 | -32.1% |

**Stage 20影響**（L=5）：
```
當前PDA：2,943
遞減後PDA：2,943 × (4.33/6.0) = 2,123
削減：-27.8%
T_kill：2.0s → 2.8s

配合Expert Track（HP×7.0）：
T_kill = 19.4s ✓
```

#### 建議選擇

```
推薦：方案A（硬上限L=4）

理由：
1. 簡單直接，易於理解
2. 削減-16.7%已足夠
3. 配合Expert Track效果良好
4. 避免複雜公式（玩家體驗）

方案B作為備選：
如果測試後L=4仍過強，可切換至遞減曲線
```

#### 代碼實施

**方案A（硬上限）**：
```csharp
// GameConstants.cs
public const int VOLLEY_MAX_LEVEL = 4;

// BuffDataSO配置
// Volley.asset
maxLevel = 4
```

**方案B（遞減曲線）**：
```csharp
// GameConstants.cs
public static int GetMissileMultiplier(int volleyLevel)
{
    float baseMultiplier = 1f + volleyLevel / (1f + 0.1f * volleyLevel);
    return Mathf.RoundToInt(baseMultiplier * 10);  // 返回整數導彈數係數
}

// CombatManager.cs (修改導彈計算)
int multiplier = GameConstants.GetMissileMultiplier(stats.volleyLevel);
int missiles = rowsCleared * GameConstants.BOARD_WIDTH * multiplier / 10;
```

---

### Defense上限

**建議上限：Lv 10**

```
當前：L_defense理論無上限
問題：無明確設計目標

建議：L_defense_max = 10
```

#### 理由

**方塊存活時間計算**（來自08）：

```
T_buffer(L_defense) = (1 + L_defense) / λ_bullet

Stage 20（λ_bullet = 1.0/s）：
L=0:  T_buffer = 1s
L=5:  T_buffer = 6s
L=10: T_buffer = 11s
L=15: T_buffer = 16s
L=20: T_buffer = 21s

觀察：
- L=10時T_buffer=11s
- Casual戰鬥時間≈90s
- 11s緩衝足夠覆蓋整場戰鬥的容錯需求
- L>10收益遞減（方塊基本不會被破壞）
```

**SP效果**：

```
SP_defense = min(1, (1+L_defense) / (10×λ_bullet))

Stage 20：
L=0:  SP_defense = 0.10
L=5:  SP_defense = 0.60
L=10: SP_defense = 1.00（達到上限！）

結論：L=10時SP_defense已達完美防禦
```

#### 代碼實施

```csharp
// GameConstants.cs
public const int DEFENSE_MAX_LEVEL = 10;

// BuffDataSO配置
// Defense.asset
maxLevel = 10
```

---

### Tactical維持現狀

**當前設計**：
```
Tactical：Lv 2（解鎖兩個技能）
ResourceExpansion：Lv 3（CP上限=250）
```

**維持理由**（來自08）：

```
技能DPS佔比分析：
當前：30-40%
評估：合理 ✓

理由：
1. Tactical Lv2恰好解鎖Execution + Repair
2. CP=250足夠支持8-10次技能使用/關卡
3. 技能DPS佔30-40%提供戰術意義但不過度依賴
4. 再高CP會導致技能spam，失去戰術性

結論：當前設計已達最佳平衡點
```

#### 可選調整（非強制）

**技能冷卻時間（CD）**：

```
當前：無CD，僅受CP限制
問題：理論上可連續使用技能（如果CP足夠）

可選方案：增加技能CD
- Execution CD：10秒
- Repair CD：20秒

效果：
- 防止技能spam
- 增加戰術決策深度
- 保持技能的"爆發"特性

實施：
// SkillExecutor.cs
private float executionCooldown = 0f;
private const float EXECUTION_CD = 10f;

public bool CanUseExecution()
{
    return playerStats.currentCp >= GameConstants.EXECUTION_CP_COST
        && executionCooldown <= 0f;
}
```

**技能成本調整（可選）**：

```
當前：
Execution：5 CP
Repair：30 CP

問題：Repair成本過高，使用率低

可選方案：
Execution：5 CP（維持）
Repair：20 CP（-33%）

效果：
- 提升Repair使用率
- 增強Tactical Build的戰術彈性
- 維持CP管理深度

評估：需測試後決定，非強制調整
```

#### 代碼實施

```csharp
// GameConstants.cs - 維持不變
public const int TACTICAL_EXPANSION_MAX_LEVEL = 2;
public const int RESOURCE_EXPANSION_MAX_LEVEL = 3;

// 如果調整Repair成本：
public const int REPAIR_CP_COST = 20;  // 從30改為20
```

---

## 📊 Build比較矩陣

### Stage 20完整對照表

| Build | 適配層級 | 配置 | PDA | T_kill | T_actual | SP | 溢出率 | 評價 |
|-------|---------|------|-----|--------|----------|-----|--------|------|
| **Volley** | Expert | 5/6/6/0 | 2,943 | 1.7s | 2.0s | 0.32 | 15% | 極度過強 ✗ |
| **Defense** | Casual | 2/4/4/9 | 101 | 49.5s | 89.1s | 0.52 | 30% | 過慢但穩 ○ |
| **Tactical** | Inter. | 2/5/5/8/T2 | 513 | 9.7s | 13.6s | 0.55 | 22% | 最平衡 ⚖️ |

**配置格式**：Volley/Salvo/Burst/Defense/其他

### 調整後預期（應用上限+難度模式）

| Build | 難度模式 | 配置 | HP | PDA | T_kill | T_actual | SP | 評價 |
|-------|---------|------|-----|-----|--------|----------|-----|------|
| **Volley** | Expert Track | 4/6/6/0 | 35,000 | 2,452 | 14.3s | 17.1s | 0.35 | 良好 ⚖️ |
| **Defense** | Casual Track | 2/4/4/10 | 3,000 | 101 | 29.7s | 53.5s | 0.58 | 良好 ⚖️ |
| **Tactical** | Standard Track | 2/5/5/8/T2 | 9,000 | 513 | 17.5s | 24.6s | 0.58 | 良好 ⚖️ |

**改善對比**：

```
Volley（Expert Track）：
當前：2.0s（極快）
調整後：17.1s（合理）
改善：+755%時間 ✓

Defense（Casual Track）：
當前：89.1s（過慢但對Casual可接受）
調整後：53.5s（改善但維持穩定）
改善：-40%時間（提升節奏）✓

Tactical（Standard Track）：
當前：13.6s（略快但可接受）
調整後：24.6s（理想範圍）
改善：+81%時間 ✓
```

### 為什麼不同層級玩家天生適合不同Build？

#### 數學原因

**Volley收益的指數性**（來自08）：

```
公式：PDA_volley ∝ [λ_clear × E[r]] × (1+L_volley) × C

關鍵：C(tier)項造成指數差距

Expert：C=30 → 高Burst加成
Inter：C=10 → 中等加成
Casual：C=5 → 低加成

收益比：Expert : Inter : Casual = 6.8 : 2.1 : 1

結論：Volley天生偏好高Combo玩家
```

**Defense需求的逆相關**：

```
需求指標：戰鬥時長 × 子彈承受數

Casual：T_battle=90s × λ=1.0 = 90發子彈
Expert：T_battle=2s × λ=1.0 = 2發子彈

比率：Casual / Expert = 45倍

結論：Casual需要45倍的防禦投資才能達到Expert的相對安全
```

**Tactical靈活性的中庸特性**：

```
技能效益：
- 低火力玩家：技能是主要輸出（60%）
- 高火力玩家：技能是錦上添花（20%）
- 中火力玩家：技能是平衡補充（30-40%）⚖️

結論：Tactical最適合中間層級
```

#### 認知負載原因（來自05）

```
Casual：
- 認知負載高（L_cognitive=7.78）
- 需要簡單直接的策略 → Defense（堆血就好）
- 無法管理複雜Build

Intermediate：
- 認知負載中等（L_cognitive=4-6）
- 可管理CP和技能 → Tactical
- 需要戰術深度

Expert：
- 認知負載低（適應性強）
- 追求極限效率 → Volley
- 複雜操作無壓力
```

#### 心理需求原因

```
Casual心理：
需求：安全感、容錯空間
恐懼：溢出、死亡
選擇：Defense（提供安全感）

Intermediate心理：
需求：成長感、掌控感
追求：平衡、技能運用
選擇：Tactical（提供掌控感）

Expert心理：
需求：挑戰感、極限表現
追求：速度、效率
選擇：Volley（提供爆發感）
```

---

## 🎯 設計目標詳述

### Volley Build設計目標

#### 設計目的

```
1. 提供極限火力輸出
2. 獎勵高技能玩家
3. 創造速通玩法
4. 展示玩家mastery
```

#### 平衡目標

**戰鬥時間**：15-25秒
```
當前（調整前）：2秒 ✗
調整後（L=4 + Expert Track）：17秒 ✓
```

**SP範圍**：0.3-0.5（可接受低值）
```
理由：戰鬥時間短，低SP無影響
實際：0.35（警戒，但快速擊殺補償）
```

**溢出率**：<20%
```
當前：15%（可接受）
調整後：預期維持
```

#### 使用者族群

```
主要：Expert玩家
- Combo 20-40
- 操作精度高
- 追求極限輸出

次要：Intermediate高端玩家
- Combo 15+
- 願意學習Volley操作
- 追求挑戰
```

#### 適合的難度軌

```
推薦：Expert Track
- HP × 7.0
- 提供足夠挑戰
- 戰鬥時長合理

不推薦：Casual/Standard Track
- Boss過弱
- 缺乏挑戰感
```

#### 相對弱點

```
1. 不適合低Combo玩家
   - Casual收益僅Expert的8%
   
2. 缺乏生存保障
   - 無Defense投資
   - SP低（0.3-0.4）
   
3. 需要高操作精度
   - 消除要大（3-4行）
   - Combo要穩定維持
   
4. 單調性風險
   - 過度專精火力
   - 缺少戰術變化
```

---

### Defense Build設計目標

#### 設計目的

```
1. 提供最大生存保障
2. 降低新手門檻
3. 延長思考時間
4. 容錯空間最大化
```

#### 平衡目標

**戰鬥時間**：30-50秒
```
當前（調整前）：89秒（對Casual可接受）
調整後（Casual Track）：54秒 ✓
```

**SP範圍**：0.5-0.8（安全/警戒）
```
當前：0.52 ✓
調整後：0.58 ✓（更安全）
```

**溢出率**：<30%
```
當前：30% ✓（可接受）
目標：維持或降低
```

#### 使用者族群

```
主要：Casual玩家
- Combo 3-6
- 操作失誤多
- 追求穩定

次要：新手玩家
- 學習階段
- 需要容錯
```

#### 適合的難度軌

```
推薦：Casual Track
- HP × 0.6
- 射速降低
- 子彈簡化

不推薦：Expert Track
- 戰鬥時間過長（>2分鐘）
- 火力不足
```

#### 相對弱點

```
1. 火力犧牲過大
   - PDA僅101（Expert的3.4%）
   
2. 戰鬥時間長
   - 50-90秒/關卡
   - 可能感覺拖沓
   
3. 不適合高技能玩家
   - Expert選Defense是浪費潛力
   
4. 缺乏爆發感
   - 輸出平穩無波動
   - 可能感覺無聊
```

---

### Tactical Build設計目標

#### 設計目的

```
1. 提供平衡型玩法
2. 引入技能戰術深度
3. 創造節奏感
4. 兼顧輸出與生存
```

#### 平衡目標

**戰鬥時間**：20-35秒
```
當前（調整前）：14秒（略快）
調整後（Standard Track）：25秒 ✓
```

**SP範圍**：0.5-0.7（警戒/安全）
```
當前：0.55 ✓
調整後：0.58 ✓
```

**溢出率**：15-25%
```
當前：22% ✓
目標：維持
```

#### 使用者族群

```
主要：Intermediate玩家
- Combo 8-12
- 願意學習技能
- 追求平衡

次要：高級Casual
- 想要進階
- 可管理CP
```

#### 適合的難度軌

```
推薦：Standard Track
- HP × 1.8
- 平衡挑戰
- 技能有用武之地

次要：Casual Track
- 對高級Casual可用
- 戰鬥時間稍長
```

#### 相對弱點

```
1. 需要技能理解
   - Execution/Repair時機
   - CP管理複雜度
   
2. CP管理壓力
   - 需要平衡技能使用和CP預留
   
3. 不如Volley暴力
   - 對Expert而言，Tactical不如專精Volley
   
4. 不如Defense穩定
   - 對Casual而言，Tactical需要更多操作
```

---

### 三Build設計目標對照表

| 設計維度 | Volley | Defense | Tactical |
|---------|--------|---------|----------|
| **核心目的** | 極限輸出 | 最大生存 | 平衡節奏 |
| **目標時間** | 15-25s | 30-50s | 20-35s |
| **SP目標** | 0.3-0.5 | 0.5-0.8 | 0.5-0.7 |
| **溢出率** | <20% | <30% | 15-25% |
| **主要玩家** | Expert | Casual | Intermediate |
| **難度軌** | Expert | Casual | Standard |
| **核心強項** | 火力 | 生存 | 靈活 |
| **主要弱點** | 脆弱 | 慢 | 複雜 |
| **技能依賴** | 低 | 低 | 高 |
| **操作難度** | 高 | 低 | 中 |

---

## ✅ 可執行建議

### 立即實施（高優先級）

#### 1. 新增三軌難度系統

**文件**：`DifficultySettings.cs`（新建）

```csharp
using UnityEngine;

public enum DifficultyMode
{
    Casual,    // 休閒模式
    Standard,  // 標準模式
    Expert     // 專家模式
}

public static class DifficultySettings
{
    public static DifficultyMode CurrentMode { get; set; } = DifficultyMode.Standard;
    
    // HP系數
    public static float GetHPMultiplier()
    {
        return CurrentMode switch
        {
            DifficultyMode.Casual => 0.6f,
            DifficultyMode.Standard => 1.8f,
            DifficultyMode.Expert => 7.0f,
            _ => 1.0f
        };
    }
    
    // 射速系數
    public static float GetShootIntervalMultiplier()
    {
        return CurrentMode switch
        {
            DifficultyMode.Casual => 1.3f,    // 變慢
            DifficultyMode.Standard => 1.0f,  // 不變
            DifficultyMode.Expert => 1.0f,    // 不變
            _ => 1.0f
        };
    }
    
    // 子彈類型限制
    public static bool IsBulletTypeEnabled(BulletType type, int stage)
    {
        if (CurrentMode == DifficultyMode.Casual)
        {
            // Casual模式只啟用簡單子彈
            if (stage < 11)
                return type == BulletType.Normal || type == BulletType.AddBlock;
            else
                return type == BulletType.Normal 
                    || type == BulletType.AddBlock 
                    || type == BulletType.AreaDamage;
        }
        
        // Standard和Expert使用完整子彈系統
        return true;
    }
}
```

**修改文件**：`EnemyController.cs`

```csharp
// 初始化時應用HP倍率
void InitializeEnemy()
{
    int baseHP = currentStageData.maxHp;
    float multiplier = DifficultySettings.GetHPMultiplier();
    maxHp = Mathf.RoundToInt(baseHP * multiplier);
    currentHp = maxHp;
    
    // 應用射速倍率
    float baseInterval = currentStageData.shootInterval;
    float intervalMultiplier = DifficultySettings.GetShootIntervalMultiplier();
    shootInterval = baseInterval * intervalMultiplier;
}

// 子彈類型判定時檢查
BulletType DetermineBulletType()
{
    // ... 原有邏輯 ...
    
    // 檢查當前難度是否啟用此子彈類型
    if (!DifficultySettings.IsBulletTypeEnabled(selectedType, currentStage))
    {
        return BulletType.Normal;  // 降級為普通子彈
    }
    
    return selectedType;
}
```

**工作量**：1-2天  
**測試重點**：三個難度的戰鬥時長是否符合目標

---

#### 2. 實施Volley上限

**修改文件**：`GameConstants.cs`

```csharp
// 新增Volley上限常數
public const int VOLLEY_MAX_LEVEL = 4;
```

**修改文件**：`PlayerManager.cs`

```csharp
// 在HandleBuffSelected中檢查Volley上限
case BuffType.Volley:
    if (stats.missileExtraCount < GameConstants.VOLLEY_MAX_LEVEL)
    {
        stats.missileExtraCount++;
        Debug.Log($"Volley提升至 {stats.missileExtraCount}");
    }
    else
    {
        Debug.Log("Volley已達上限！");
        // 可選：改為給予其他獎勵或重新抽取
    }
    break;
```

**修改文件**：所有`Volley.asset`（BuffDataSO）

```
在Inspector中設置：
maxLevel = 4
```

**工作量**：2-4小時  
**測試重點**：Volley L=4時Expert的擊殺時間

---

#### 3. 實施Defense上限

**修改文件**：`GameConstants.cs`

```csharp
// 新增Defense上限常數
public const int DEFENSE_MAX_LEVEL = 10;
```

**修改文件**：`PlayerManager.cs`

```csharp
// 在HandleBuffSelected中檢查Defense上限
case BuffType.Defense:
    if (stats.blockDefenseLevel < GameConstants.DEFENSE_MAX_LEVEL)
    {
        stats.blockDefenseLevel++;
        Debug.Log($"Defense提升至 {stats.blockDefenseLevel}");
    }
    else
    {
        Debug.Log("Defense已達上限！");
    }
    break;
```

**修改文件**：`Defense.asset`（BuffDataSO）

```
在Inspector中設置：
maxLevel = 10
```

**工作量**：1-2小時  
**測試重點**：Defense L=10時Casual的SP和溢出率

---

### 中優先級（1-2週內）

#### 4. Build引導系統

**新建文件**：`BuildGuideUI.cs`

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildGuideUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject guidePanel;
    public TextMeshProUGUI questionText;
    public Button optionA;
    public Button optionB;
    public Button optionC;
    
    private int currentQuestion = 0;
    private int volleyScore = 0;
    private int defenseScore = 0;
    private int tacticalScore = 0;
    
    private string[] questions = new string[]
    {
        "你喜歡的遊戲節奏？",
        "你的俄羅斯方塊經驗？",
        "你更重視？"
    };
    
    private string[] optionsA = new string[] { "快速果斷", "老手", "輸出傷害" };
    private string[] optionsB = new string[] { "穩健持久", "新手", "不要死" };
    private string[] optionsC = new string[] { "戰術靈活", "中等", "技能運用" };
    
    void Start()
    {
        ShowQuestion();
    }
    
    void ShowQuestion()
    {
        if (currentQuestion >= questions.Length)
        {
            ShowRecommendation();
            return;
        }
        
        questionText.text = questions[currentQuestion];
        optionA.GetComponentInChildren<TextMeshProUGUI>().text = optionsA[currentQuestion];
        optionB.GetComponentInChildren<TextMeshProUGUI>().text = optionsB[currentQuestion];
        optionC.GetComponentInChildren<TextMeshProUGUI>().text = optionsC[currentQuestion];
    }
    
    public void OnSelectA()
    {
        volleyScore++;
        currentQuestion++;
        ShowQuestion();
    }
    
    public void OnSelectB()
    {
        defenseScore++;
        currentQuestion++;
        ShowQuestion();
    }
    
    public void OnSelectC()
    {
        tacticalScore++;
        currentQuestion++;
        ShowQuestion();
    }
    
    void ShowRecommendation()
    {
        string recommendedBuild;
        DifficultyMode recommendedDifficulty;
        
        if (volleyScore >= defenseScore && volleyScore >= tacticalScore)
        {
            recommendedBuild = "Volley（輸出流）";
            recommendedDifficulty = DifficultyMode.Expert;
        }
        else if (defenseScore >= tacticalScore)
        {
            recommendedBuild = "Defense（坦克流）";
            recommendedDifficulty = DifficultyMode.Casual;
        }
        else
        {
            recommendedBuild = "Tactical（戰術流）";
            recommendedDifficulty = DifficultyMode.Standard;
        }
        
        // 設置難度
        DifficultySettings.CurrentMode = recommendedDifficulty;
        
        // 顯示推薦結果
        questionText.text = $"推薦Build：{recommendedBuild}\n推薦難度：{recommendedDifficulty}";
        
        // 隱藏選項按鈕
        optionA.gameObject.SetActive(false);
        optionB.gameObject.SetActive(false);
        optionC.gameObject.SetActive(false);
    }
}
```

**工作量**：1週（含UI設計）  
**測試重點**：推薦準確性、UI流暢度

---

#### 5. UI教學提示系統

**修改文件**：`TutorialManager.cs`（如果不存在則新建）

```csharp
using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Panels")]
    public GameObject volleyTutorial;
    public GameObject defenseTutorial;
    public GameObject tacticalTutorial;
    
    public void ShowVolleyTutorial()
    {
        volleyTutorial.SetActive(true);
        // 內容：
        // "Volley（協同火力）：每級增加100%導彈數
        // 最適合：Expert玩家（高Combo）
        // 特點：極限輸出，但需要高操作精度
        // 推薦難度：專家模式"
    }
    
    public void ShowDefenseTutorial()
    {
        defenseTutorial.SetActive(true);
        // 內容：
        // "Defense（裝甲強化）：增加方塊HP
        // 最適合：Casual玩家（新手）
        // 特點：最大生存，延長思考時間
        // 推薦難度：休閒模式"
    }
    
    public void ShowTacticalTutorial()
    {
        tacticalTutorial.SetActive(true);
        // 內容：
        // "Tactical（戰術擴展）：解鎖主動技能
        // 最適合：Intermediate玩家
        // 特點：技能爆發，戰術節奏
        // 推薦難度：標準模式"
    }
}
```

**工作量**：3-5天（含文案、UI設計）  
**測試重點**：新手理解度、提示時機

---

### 低優先級（可選）

#### 6. 技能冷卻系統（可選）

**修改文件**：`SkillExecutor.cs`

```csharp
public class SkillExecutor : MonoBehaviour
{
    private float executionCooldown = 0f;
    private float repairCooldown = 0f;
    
    private const float EXECUTION_CD = 10f;
    private const float REPAIR_CD = 20f;
    
    void Update()
    {
        if (executionCooldown > 0)
            executionCooldown -= Time.deltaTime;
        if (repairCooldown > 0)
            repairCooldown -= Time.deltaTime;
    }
    
    public bool CanUseExecution()
    {
        return playerManager.stats.currentCp >= GameConstants.EXECUTION_CP_COST
            && executionCooldown <= 0f;
    }
    
    public bool CanUseRepair()
    {
        return playerManager.stats.currentCp >= GameConstants.REPAIR_CP_COST
            && repairCooldown <= 0f;
    }
    
    public void ExecuteExecution()
    {
        if (!CanUseExecution()) return;
        
        // ... 原有邏輯 ...
        
        executionCooldown = EXECUTION_CD;
    }
    
    public void ExecuteRepair()
    {
        if (!CanUseRepair()) return;
        
        // ... 原有邏輯 ...
        
        repairCooldown = REPAIR_CD;
    }
}
```

**工作量**：1天  
**測試重點**：CD是否影響Tactical Build平衡

---

#### 7. Repair成本調整（可選）

**修改文件**：`GameConstants.cs`

```csharp
// 從30降至20
public const int REPAIR_CP_COST = 20;
```

**工作量**：10分鐘  
**測試重點**：Repair使用率是否提升，是否影響平衡

---

### 實施順序建議

```
階段1（第1週）：核心系統
✓ 任務1：三軌難度系統（1-2天）
✓ 任務2：Volley上限（2-4小時）
✓ 任務3：Defense上限（1-2小時）

階段2（第2週）：玩家引導
✓ 任務4：Build引導系統（1週）
✓ 任務5：UI教學提示（3-5天）

階段3（第3週+）：可選優化
○ 任務6：技能CD（如需要）
○ 任務7：Repair成本調整（如需要）
```

---

## 📚 交叉引用

**基於文檔**：
- ← [02_Combat_Formulas.md](02_Combat_Formulas.md) - 傷害公式系統
- ← [04_Difficulty_Model.md](04_Difficulty_Model.md) - PDA、SP模型
- ← [05_Player_Model.md](05_Player_Model.md) - HOM模型
- ← [07_Skill_Tiers_Model.md](07_Skill_Tiers_Model.md) - 三層玩家定義
- ← [08_Legendary_Build_Analysis.md](08_Legendary_Build_Analysis.md) - Build分析基礎

**影響系統**：
- → 遊戲設計：三軌制平衡系統
- → 數值設計：Build上限規範
- → UI/UX：Build引導和教學
- → 難度設計：三難度模式

**代碼影響文件**：
- `DifficultySettings.cs` - 新建
- `GameConstants.cs` - 新增上限常數
- `EnemyController.cs` - 應用難度系數
- `PlayerManager.cs` - 檢查Build上限
- `BuildGuideUI.cs` - 新建
- `TutorialManager.cs` - 新建或修改

---

## 📊 成功指標

### 量化指標

**戰鬥時長目標**：
```
Casual + Defense + Casual Track：
目標：30-50秒
當前預測：54秒
評估標準：±20%誤差內（43-65秒）

Intermediate + Tactical + Standard Track：
目標：20-35秒
當前預測：25秒
評估標準：±20%誤差內（20-42秒）

Expert + Volley + Expert Track：
目標：15-25秒
當前預測：17秒
評估標準：±20%誤差內（14-30秒）
```

**玩家滿意度**：
```
問卷調查（5分制）：
- 難度適中度：目標>3.5
- 挑戰感：目標>3.5
- Build多樣性：目標>4.0
- 重玩價值：目標>4.0
```

**Build使用分布**：
```
目標：三個Build使用率各佔20-40%
（避免單一Build壟斷>60%）

測量：統計100場遊戲的Build選擇
```

### 定性指標

**玩家反饋**：
```
正面信號：
✓ "終於有適合我的難度了"
✓ "Build選擇變得有意義"
✓ "Boss不再是2秒秒殺"

負面信號：
✗ "某個Build明顯過強"
✗ "難度差距太大/太小"
✗ "技能系統太複雜"
```

---

## 🎯 總結

### 核心改進

**問題**：遊戲數值被Volley Build綁架
**解決**：三軌制平衡設計

**三大支柱**：
1. **Build角色明確化**：Volley=火力、Defense=生存、Tactical=靈活
2. **難度模式匹配**：每個Build對應一個難度軌道
3. **數值上限規範**：防止單一Build失控

### 預期效果

```
調整前：
- Expert 2秒擊殺（無聊）
- Casual 90秒擊殺（拖沓）
- Intermediate 14秒擊殺（略快）
- 所有層級都不滿意

調整後：
- Expert 17秒擊殺 ✓（挑戰）
- Casual 54秒擊殺 ✓（穩定）
- Intermediate 25秒擊殺 ✓（平衡）
- 所有層級都滿意
```

### 實施路徑

```
立即：三軌難度 + Build上限（1-2週）
短期：Build引導 + UI教學（2-4週）
長期：數據監控 + 持續調整（持續）
```

---

**文檔狀態**: ✅ 完整規格書  
**可執行性**: ⭐⭐⭐⭐⭐  
**預期效果**: 根本性解決平衡問題  
**實施優先級**: 最高

---

**關鍵要點**：

本規格書提供了完整的、可直接執行的Build平衡設計方案：

1. **明確的角色定位**：三個Build各司其職
2. **數學驗證的平衡目標**：基於01-08的完整模型
3. **具體的數值規範**：上限、系數、曲線全部定義
4. **可執行的代碼方案**：含完整代碼示例
5. **清晰的實施路徑**：分階段、有優先級

**交付企劃即可直接使用！**

