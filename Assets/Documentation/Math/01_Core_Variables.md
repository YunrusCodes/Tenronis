# 01 - 核心變量定義
# Core Variables Definition

**文檔版本**: 2.0  
**最後更新**: 2025年12月1日  
**數據源**: GameConstants.cs, BlockData.cs, StageDataSO.cs

---

## 📋 目錄

1. [常數系統](#常數系統)
2. [狀態變量](#狀態變量)
3. [派生變量](#派生變量)
4. [變量關係圖](#變量關係圖)

---

## 🔢 常數系統

### 遊戲板常數

```
W := BOARD_WIDTH = 10                    // 網格寬度（格）
H := BOARD_HEIGHT = 20                   // 網格高度（格）
τ := TICK_RATE = 0.8                     // 遊戲節奏（秒）
```

### 玩家基礎常數

```
HP_max := PLAYER_MAX_HP = 100            // 玩家最大生命值
CP_max := PLAYER_MAX_CP = 100            // 玩家最大Castle Point基礎值
CP_overflow := OVERFLOW_CP_COST = 25     // 溢出時CP消耗
```

### 導彈系統常數

```
D_base := BASE_MISSILE_DAMAGE = 2.0      // 基礎導彈傷害
v_m := MISSILE_SPEED = 20.0              // 導彈飛行速度（格/秒）
k_salvo := SALVO_DAMAGE_MULTIPLIER = 0.5 // 齊射加成倍率
k_burst := BURST_DAMAGE_MULTIPLIER = 0.25// 連發加成倍率
```

### 方塊系統常數

```
HP_block := BASE_BLOCK_HP = 1            // 基礎方塊HP
HP_garbage := GARBAGE_BLOCK_HP = 1       // 垃圾方塊HP
HP_indestr := INDESTRUCTIBLE_BLOCK_HP = 9999 // 不可摧毀方塊HP
```

### 子彈系統常數

```
D_bullet := BULLET_DAMAGE = 10           // 子彈基礎傷害（實際造成1 HP）
D_hit := BASE_HIT_DAMAGE = 10            // 基礎命中傷害
```

### 時間常數

```
t_combo := COMBO_RESET_DELAY = 0.3       // Combo重置延遲（秒）
t_counter := COUNTER_FIRE_TIME_WINDOW = 0.2  // 反擊時間窗口（秒）
```

### 爆炸充能系統常數

```
Q_init := EXPLOSION_INITIAL_MAX_CHARGE = 200        // 初始充能上限
q_counter := EXPLOSION_COUNTER_CHARGE = 5           // 反擊獲得充能
q_clear := EXPLOSION_ROW_CLEAR_CHARGE = 50          // 消排獲得充能
ΔQ := EXPLOSION_BUFF_MAX_CHARGE_INCREASE = 200     // 每級增加充能上限
L_Q_max := EXPLOSION_BUFF_MAX_LEVEL = 4             // 爆炸充能最高等級
```

### 技能常數

```
CP_exec := EXECUTION_CP_COST = 5         // 處決技能消耗
CP_repair := REPAIR_CP_COST = 30         // 修補技能消耗
D_exec := EXECUTION_DAMAGE = 4.0         // 處決技能傷害
D_repair := REPAIR_DAMAGE = 2.0          // 修補技能傷害（未使用）
```

### Buff等級上限

```
L_salvo := SALVO_MAX_LEVEL = 6           // 齊射強化最高等級
L_burst := BURST_MAX_LEVEL = 6           // 連發強化最高等級
L_counter := COUNTER_MAX_LEVEL = 6       // 反擊強化最高等級
L_space := SPACE_EXPANSION_MAX_LEVEL = 4 // 空間擴充最高等級
L_resource := RESOURCE_EXPANSION_MAX_LEVEL = 3    // 資源擴充最高等級
L_tactical := TACTICAL_EXPANSION_MAX_LEVEL = 2    // 戰術擴展最高等級
```

---

## 📊 狀態變量

### 玩家狀態

```
HP(t) ∈ [0, HP_max]                      // 當前生命值
CP(t) ∈ [0, CP_max(L_resource)]         // 當前Castle Point
C(t) ∈ ℕ₀                                // 當前Combo數
Q(t) ∈ [0, Q_max(L_Q)]                   // 當前爆炸充能
S(t) ∈ ℤ₊                                // 當前分數
```

### Buff等級變量

```
L_defense ∈ ℕ₀                           // 裝甲強化等級（無上限）
L_volley ∈ ℕ₀                            // 協同火力等級（無上限）
L_salvo ∈ [1, L_salvo]                   // 齊射強化等級
L_burst ∈ [1, L_burst]                   // 連發強化等級
L_counter ∈ [0, L_counter]               // 反擊強化等級
L_Q ∈ [1, L_Q_max]                       // 爆炸充能等級
L_space ∈ [1, L_space]                   // 空間擴充等級
L_resource ∈ [0, L_resource]             // 資源擴充等級
L_tactical ∈ [0, L_tactical]             // 戰術擴展等級
```

### 敵人狀態（關卡n）

```
HP_enemy(n, t) ∈ [0, HP_enemy_max(n)]    // 敵人當前HP
HP_enemy_max(n)                          // 敵人最大HP
I_shoot(n)                               // 射擊間隔（秒）
v_bullet(n)                              // 子彈速度（格/秒）
B_count(n)                               // 連發數量
```

### 網格狀態

```
G(t) := [g_{x,y}(t)]_{W×H}               // 網格狀態矩陣
g_{x,y}(t) ∈ {∅, Normal, Void, Explosive}// 單格狀態
hp_{x,y}(t) ∈ [0, HP_block + L_defense]  // 單格當前HP
```

---

## 🔄 派生變量

### CP上限函數

```
CP_max(L_resource) = CP_max + L_resource × 50
                   = 100 + 50L_resource

範圍：[100, 250]（L_resource ∈ [0, 3]）
```

### 爆炸充能上限函數

```
Q_max(L_Q) = Q_init + (L_Q - 1) × ΔQ
           = 200 + (L_Q - 1) × 200
           = 200(1 + L_Q - 1)
           = 200L_Q

範圍：[200, 1000]（L_Q ∈ [1, 4]）

特例：
L_Q = 4: Q_max = 1000（特殊設定）
```

### 方塊HP函數

```
HP_block(L_defense) = HP_block + L_defense
                    = 1 + L_defense

普通垃圾方塊：
HP_garbage(L_defense) = HP_garbage + L_defense = 1 + L_defense

不可摧毀垃圾方塊：
HP_indestr(L_defense) = HP_indestr + L_defense = 9999 + L_defense
```

### 導彈數量函數

```
N_missiles(r, L_volley) = r × W × (1 + L_volley)
                        = 10r(1 + L_volley)

其中：
r := rows_cleared ∈ [1, 4]           // 消除行數
```

### 反擊導彈數量

```
N_counter(L_counter) = L_counter

條件：方塊放置後時間 ≤ t_counter
```

### 技能導彈數量

```
N_exec(L_volley) = W × (1 + L_volley)
                 = 10(1 + L_volley)

條件：消耗CP_exec
```

---

## 🔗 變量關係圖

### 傷害輸出依賴鏈

```
[r, C, L_volley, L_salvo, L_burst]
            ↓
    [DMG_single, N_missiles]
            ↓
        DMG_total
            ↓
      HP_enemy減少
```

### 生存依賴鏈

```
[L_defense, L_resource, L_tactical]
            ↓
    [HP_block, CP_max, 技能解鎖]
            ↓
        生存能力
            ↓
      承受敵人攻擊
```

### Combo循環依賴

```
消除行 → C+1 → DMG_burst增加 → 更快擊敗敵人
  ↑                                    ↓
  ←─── 更多時間消除 ←───────────────────┘
```

### 資源循環

```
關卡完成 → CP恢復至CP_max
              ↓
        技能使用（消耗CP）
              ↓
        清理方塊/填補空洞
              ↓
        繼續戰鬥 → 關卡完成
```

---

## 📐 關鍵比率與無量綱數

### 火力密度指標

```
ρ_fire := N_missiles(r, L_volley) / W
        = r(1 + L_volley)

物理意義：每列發射的平均導彈數
範圍：[1, 24]（r=4, L_volley=5時）
```

### 防禦厚度指標

```
δ_defense := HP_block(L_defense) / HP_block
           = (1 + L_defense) / 1
           = 1 + L_defense

物理意義：相對於基礎HP的防禦倍數
範圍：[1, ∞)
```

### Combo效率因子

```
η_combo := C × L_burst × k_burst / D_base
         = 0.125 × C × L_burst

物理意義：Combo對基礎傷害的相對增幅
範例：C=20, L_burst=6 → η=15（1500%增幅）
```

### 充能利用率

```
μ_charge := Q(t) / Q_max(L_Q)

物理意義：當前充能佔上限的比例
範圍：[0, 1]
戰術意義：μ接近1時考慮觸發溢出
```

---

## 🎯 變量命名約定

### 希臘字母
- τ (tau): 時間常數
- ρ (rho): 密度類指標
- δ (delta): 差異/增量
- η (eta): 效率因子
- μ (mu): 利用率
- Δ (Delta): 變化量

### 下標約定
- _max: 最大值
- _base: 基礎值
- _init: 初始值
- (t): 時間依賴
- (n): 關卡依賴
- _{x,y}: 位置索引

### 函數表示
- f(x): 標量函數
- F(t): 時間函數
- G[x]: 矩陣/網格
- L_*: 等級變量（Level）

---

## 📚 交叉引用

**相關文檔**：
- → [02_Combat_Formulas.md](02_Combat_Formulas.md) - 使用這些變量計算傷害
- → [03_Enemy_Values.md](03_Enemy_Values.md) - 敵人狀態變量的具體數值
- → [05_Player_Model.md](05_Player_Model.md) - 玩家狀態演化模型

**代碼源文件**：
- `GameConstants.cs`: 第6-94行（所有常數定義）
- `BlockData.cs`: 第10-36行（數據結構）
- `PlayerStats.cs`: 第42-86行（玩家狀態）

---

**文檔狀態**: ✅ 完整  
**數學符號**: ✅ 標準化  
**代碼驗證**: ✅ 已驗證

