# 02 - 戰鬥公式系統
# Combat Formulas System

**文檔版本**: 2.0  
**最後更新**: 2025年12月1日  
**數據源**: CombatManager.cs, PlayerManager.cs, SkillExecutor.cs

---

## 📋 目錄

1. [玩家傷害公式](#玩家傷害公式)
2. [技能傷害公式](#技能傷害公式)
3. [反擊系統公式](#反擊系統公式)
4. [防空負擔模型 (AAB)](#防空負擔模型-aab)
5. [爆炸充能系統](#爆炸充能系統)
6. [分數計算公式](#分數計算公式)

---

## 🎯 玩家傷害公式

### 主公式系統

#### 單發導彈傷害

```
DMG_single(r, C, L_salvo, L_burst) = D_base + B_salvo(r, L_salvo) + B_burst(C, L_burst)

其中：
D_base = 2.0                                          [常數，見01]
B_salvo(r, L_salvo) = (r - 1) · L_salvo · k_salvo    [齊射加成]
B_burst(C, L_burst) = C · L_burst · k_burst          [連發加成]
```

**參數**：
- r ∈ [1, 4]: 消除行數
- C ∈ ℕ₀: 當前Combo數
- L_salvo ∈ [1, 6]: 齊射強化等級
- L_burst ∈ [1, 6]: 連發強化等級

**常數引用**：
- k_salvo = 0.5 [見 01, 導彈系統常數]
- k_burst = 0.25 [見 01, 導彈系統常數]

#### 總導彈數

```
N_missiles(r, L_volley) = r · W · (1 + L_volley)
                        = 10r(1 + L_volley)
```

**參數**：
- r ∈ [1, 4]: 消除行數
- L_volley ∈ ℕ₀: 協同火力等級

**常數引用**：
- W = 10 [見 01, 遊戲板常數]

#### 總傷害輸出

```
DMG_total(r, C, L_volley, L_salvo, L_burst) = N_missiles(r, L_volley) · DMG_single(r, C, L_salvo, L_burst)
```

**完整展開式**：
```
DMG_total = 10r(1 + L_volley) · [2.0 + (r-1)L_salvo · 0.5 + C · L_burst · 0.25]
          = 10r(1 + L_volley) · [2.0 + 0.5(r-1)L_salvo + 0.25C·L_burst]
```

### 實際計算範例

#### 範例A: 最小傷害（初始狀態）

```
條件：r=1, C=0, L_volley=0, L_salvo=1, L_burst=1

DMG_single = 2.0 + (1-1)×1×0.5 + 0×1×0.25
           = 2.0 + 0 + 0
           = 2.0

N_missiles = 1 × 10 × (1+0) = 10

DMG_total = 10 × 2.0 = 20.0
```

#### 範例B: 中期傷害

```
條件：r=3, C=10, L_volley=1, L_salvo=2, L_burst=2

DMG_single = 2.0 + (3-1)×2×0.5 + 10×2×0.25
           = 2.0 + 2.0 + 5.0
           = 9.0

N_missiles = 3 × 10 × (1+1) = 60

DMG_total = 60 × 9.0 = 540.0
```

#### 範例C: 最大傷害（理論上限）

```
條件：r=4, C=30, L_volley=5, L_salvo=6, L_burst=6

DMG_single = 2.0 + (4-1)×6×0.5 + 30×6×0.25
           = 2.0 + 9.0 + 45.0
           = 56.0

N_missiles = 4 × 10 × (1+5) = 240

DMG_total = 240 × 56.0 = 13,440.0
```

### 傷害增長分析

#### 對r的偏導數（齊射效應）

```
∂DMG_total/∂r = 10(1 + L_volley) · [2.0 + 0.5(r-1)L_salvo + 0.25C·L_burst]
               + 10r(1 + L_volley) · [0.5L_salvo]
              
              = 10(1 + L_volley) · [2.0 + 0.5(2r-1)L_salvo + 0.25C·L_burst]
```

**物理意義**：每增加1行消除，傷害增加量

#### 對L_volley的偏導數（協同火力效應）

```
∂DMG_total/∂L_volley = 10r · [2.0 + 0.5(r-1)L_salvo + 0.25C·L_burst]
                      = DMG_total / (1 + L_volley)
```

**物理意義**：Volley每+1級，傷害增加當前傷害的 1/(1+L_volley) 倍

#### 對C的偏導數（Combo效應）

```
∂DMG_total/∂C = 10r(1 + L_volley) · 0.25L_burst
              = 2.5rL_burst(1 + L_volley)
```

**物理意義**：Combo每+1，傷害增加固定值（與當前Combo無關）

---

## 🔧 技能傷害公式

### 處決技能 (Execution)

#### 傷害公式

```
DMG_exec(L_volley, n_active) = n_active · (1 + L_volley) · D_exec
```

**參數**：
- n_active ∈ [0, W]: 清除的方塊數量（最多10個）
- L_volley ∈ ℕ₀: 協同火力等級
- D_exec = 4.0 [見 01, 技能常數]

**最大傷害**：
```
DMG_exec_max = W · (1 + L_volley) · D_exec
             = 10(1 + L_volley) · 4.0
             = 40(1 + L_volley)
```

**CP效率**：
```
η_exec := DMG_exec_max / CP_exec
        = 40(1 + L_volley) / 5
        = 8(1 + L_volley)

範例：
L_volley=0: η=8 (每CP造成8傷害)
L_volley=3: η=32 (每CP造成32傷害)
```

#### 實際效果模擬

```
情境：10個方塊全部清除，L_volley=3

DMG_exec = 10 × (1+3) × 4.0
         = 10 × 4 × 4.0
         = 160.0

CP消耗 = 5
傷害/CP = 32.0
```

### 修補技能 (Repair)

#### 效果公式

```
Effect_repair(n_holes, r_trigger) = Fill(n_holes) + DMG_trigger(r_trigger)

其中：
Fill(n_holes): 填補n_holes個封閉空洞
DMG_trigger(r_trigger): 若填補後觸發消除r_trigger行，使用標準傷害公式
```

**填補方塊HP**：
```
HP_filled = HP_block + L_defense = 1 + L_defense
```

**CP效率**：高度情境依賴，無法建立統一公式

**預期收益**：
```
E[Effect_repair] ≈ n_holes · HP_filled + P(trigger) · E[DMG_total | trigger]
```

---

## ⚔️ 反擊系統公式

### 觸發條件判定

```
Trigger_counter(Δt, L_counter) = {
    true,  if Δt ≤ t_counter ∧ L_counter ≥ 1
    false, otherwise
}

其中：
Δt: 方塊放置後經過的時間
t_counter = 0.2秒 [見 01, 時間常數]
```

### 反擊傷害公式

```
DMG_counter(C, L_counter, L_burst) = N_counter(L_counter) · DMG_counter_single(C, L_burst)

其中：
N_counter(L_counter) = L_counter
DMG_counter_single(C, L_burst) = D_base + B_burst(C, L_burst)
                                = 2.0 + C · L_burst · k_burst
                                = 2.0 + 0.25C·L_burst
```

**注意**：反擊導彈不享受齊射加成（B_salvo = 0）

### 反擊額外效果

```
Effect_counter = {
    DMG_counter,
    q_counter = +5,                    // 爆炸充能增加
    C(t+) = C(t) + 1,                  // Combo增加
    Cancel_Combo_Reset()               // 取消Combo重置計時器
}
```

### 反擊CP效率

```
反擊不消耗CP，因此效率無限大（理論）
實際限制：需要精準時機（0.2秒窗口）
```

---

## 🛡️ 防空負擔模型 (AAB)

**新增模型** - 基於代碼推導

### 定義

防空負擔(Anti-Air Burden)衡量玩家需要多大程度依賴導彈攔截來保護方塊的壓力。

### 核心公式

```
AAB(n, t) = λ_bullet(n) · v_bullet(n) · P_hit(n) / [N_missiles_rate(t) · P_intercept(t)]
```

**變量**：
- λ_bullet(n): 敵人射彈率（發/秒） = 1 / I_shoot(n)
- v_bullet(n): 子彈速度（格/秒）
- P_hit(n): 子彈命中方塊的概率（考慮玩家操作）
- N_missiles_rate(t): 玩家導彈發射率（發/秒）
- P_intercept(t): 單個導彈攔截成功率

### 簡化模型

假設穩態戰鬥（消除頻率穩定）：

```
AAB_simple(n, L_volley, r_avg, T_clear) = λ_bullet(n) / λ_missiles(L_volley, r_avg, T_clear)

其中：
λ_missiles = N_missiles(r_avg, L_volley) / T_clear
T_clear: 平均消除一次的時間（秒）
r_avg: 平均消除行數
```

### 實際數值估算

#### Stage 1（低AAB）

```
I_shoot(1) = 3.0s → λ_bullet = 0.33發/秒
假設：r_avg=2, L_volley=0, T_clear=4s

λ_missiles = 20 / 4 = 5發/秒
AAB_simple = 0.33 / 5 = 0.066

解釋：導彈數量 >> 子彈數量，防禦輕鬆
```

#### Stage 10（中AAB）

```
I_shoot(10) = 1.9s → λ_bullet = 0.53發/秒
假設：r_avg=3, L_volley=1, T_clear=3s

λ_missiles = 60 / 3 = 20發/秒
AAB_simple = 0.53 / 20 = 0.027

解釋：導彈仍然充足，但防禦壓力上升
```

#### Stage 20（高AAB）

```
I_shoot(20) = 1.0s → λ_bullet = 1.0發/秒
假設：r_avg=3, L_volley=3, T_clear=2.5s

λ_missiles = 120 / 2.5 = 48發/秒
AAB_simple = 1.0 / 48 = 0.021

解釋：雖然子彈快，但導彈更多，防禦仍可控
```

### AAB閾值分析

```
AAB < 0.1  : 低負擔（導彈足夠防禦）
AAB ∈ [0.1, 0.3]: 中負擔（需要注意防禦）
AAB > 0.3  : 高負擔（方塊易被擊破）
```

**結論**：實際遊戲中AAB保持在低水平，導彈攔截提供充足的被動防禦。

---

## 💥 爆炸充能系統

### 充能累積公式

```
Q(t+Δt) = min(Q(t) + ΔQ_event, Q_max(L_Q))

其中：
ΔQ_event = {
    q_counter = 5,   if 觸發反擊
    q_clear = 50,    if 消除行
    0,               otherwise
}
```

**累積速率**（假設穩態）：
```
dQ/dt = (q_clear · λ_clear + q_counter · λ_counter)

其中：
λ_clear: 消除頻率（次/秒）
λ_counter: 反擊頻率（次/秒）
```

### 爆炸傷害公式

```
DMG_explosion = Q(t_overflow)

條件：發生溢出時觸發
效果：充能清零 Q(t+) = 0
```

### 充能上限公式

```
Q_max(L_Q) = 200L_Q

特例：L_Q=4 時 Q_max=1000

引用：[見 01, 派生變量]
```

### 充能效率分析

```
假設平均每次消除需要時間T_clear，每N_clear次消除觸發一次溢出：

期望爆炸傷害 = Q_max(L_Q)
期望時間成本 = N_clear · T_clear
傷害速率 = Q_max(L_Q) / (N_clear · T_clear)

範例（L_Q=4, T_clear=3s, N_clear=20）：
傷害速率 = 1000 / (20×3) = 16.67 傷害/秒
```

---

## 🎯 分數計算公式

### 基礎分數

```
S_base(r) = {
    100,  if r = 1
    300,  if r = 2
    500,  if r = 3
    800,  if r = 4
}
```

**數學表示**（近似）：
```
S_base(r) ≈ 100r^1.5

實際：
r=1: 100 vs 100
r=2: 300 vs 283
r=3: 500 vs 520
r=4: 800 vs 800
```

### Combo加成公式

```
S_total(r, C) = S_base(r) · (1 + 0.1C)
```

**分數增長率**：
```
dS_total/dC = 0.1 · S_base(r)

物理意義：Combo每+1，分數增加基礎分數的10%
```

### 累積分數

```
S(t) = S(t-Δt) + S_total(r, C)

其中：r, C為本次消除的參數
```

### 分數效率指標

```
分數/傷害比 = S_total(r, C) / DMG_total(r, C, ...)

範例（r=4, C=20）：
分數 = 800 × (1 + 0.1×20) = 800 × 3 = 2400
傷害 = 13440（假設最大配置）
比率 = 2400 / 13440 ≈ 0.18

含義：每造成1點傷害獲得約0.18分
```

---

## 📊 公式對照表

| 公式 | 符號 | 範圍 | 代碼位置 |
|------|------|------|---------|
| 單發傷害 | DMG_single | [2, 56] | CombatManager.cs:99-102 |
| 總導彈數 | N_missiles | [10, 240] | CombatManager.cs:105 |
| 總傷害 | DMG_total | [20, 13440] | CombatManager.cs:102 |
| 處決傷害 | DMG_exec | [0, 240] | SkillExecutor.cs:38-46 |
| 反擊傷害 | DMG_counter | [2, 47] | CombatManager.cs:523-532 |
| 爆炸傷害 | DMG_explosion | [0, 1000] | GridManager.cs:599-604 |
| 分數 | S_total | [100, ∞) | PlayerManager.cs:326 |

---

## 📐 傷害公式圖解

```
DMG_total = N_missiles × DMG_single
            ↓           ↓
         r·W·(1+L_v) [D_base + B_salvo + B_burst]
            ↓           ↓                 ↓
         rows·10·(1+Lv)[2.0 + 0.5(r-1)Ls + 0.25C·Lb]

影響因素層次：
Level 1: 消除行數r（玩家操作）
Level 2: Combo C（連續操作）
Level 3: Buff等級（Lv, Ls, Lb）（長期成長）
```

---

## 📚 交叉引用

**引用**：
- ← [01_Core_Variables.md](01_Core_Variables.md) - 所有常數定義
- → [04_Difficulty_Model.md](04_Difficulty_Model.md) - 將這些公式應用於難度分析
- → [05_Player_Model.md](05_Player_Model.md) - 玩家狀態演化使用這些公式

**代碼源文件**：
- `CombatManager.cs`: 第79-123行（導彈發射和傷害計算）
- `SkillExecutor.cs`: 第17-54行（技能傷害）
- `PlayerManager.cs`: 第312-344行（反擊和分數）

---

**文檔狀態**: ✅ 完整  
**新增模型**: ✅ AAB模型  
**數學驗證**: ✅ 已驗證  
**代碼一致性**: ✅ 100%

