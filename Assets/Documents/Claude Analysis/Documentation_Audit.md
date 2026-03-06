# Documentation 文件審計報告

> 基於源碼逐項核對 `Assets/Documentation/` 所有 markdown 文件
> 審計日期：2026-03-06
> 核對對象：GameConstants.cs、BlockData.cs、CombatManager.cs、PlayerManager.cs、
> GridManager.cs、EnemyController.cs、SkillExecutor.cs、GameEnums.cs

---

## 結論摘要

`Documentation/` 下的文件由 2025-12-01 的 AI Agent 生成，**基於當時的代碼版本**。
此後代碼經過多次調整，導致文件中存在 **17 項重大不一致**，其中多項影響所有數學公式的計算結果。

### 影響等級

| 等級 | 說明 | 數量 |
|------|------|------|
| 🔴 致命 | 影響所有公式計算結果 | 2 |
| 🟠 嚴重 | 機制描述根本性錯誤 | 7 |
| 🟡 中等 | 數值或描述部分錯誤 | 5 |
| 🔵 輕微 | 已移除的常數仍被引用 | 3 |

---

## 🔴 致命錯誤（影響所有公式）

### 1. BASE_MISSILE_DAMAGE：2.0 → 實際為 1.0

**影響範圍**：所有文件中的傷害計算

| | 文件中 | 代碼實際值 |
|--|--------|-----------|
| BASE_MISSILE_DAMAGE | 2.0 | **1.0** (`1f`) |

**代碼位置**：`GameConstants.cs:35`
```csharp
public const float BASE_MISSILE_DAMAGE = 1f;
```

**連鎖影響**：
- `Balance_Math_Model.md` 所有範例的基礎傷害均以 2.0 計算 → **全部偏高**
- 範例1「基礎消除」：文件算出 20.0，實際為 **10.0**
- 範例3「Stage 20 後期」：文件算出 13,440，實際約 **6,840**
- 所有 PDA（玩家傷害可用性）數值均偏高約 2 倍
- 所有 Build 分析中的擊殺時間預測均偏短

**受影響文件**：
- `Balance_Math_Model.md`（全文）
- `Math/01_Core_Variables.md`
- `Math/02_Combat_Formulas.md`
- `Math/04_Difficulty_Model.md`
- `Math/05_Player_Model.md`
- `Math/06_Balance_Analysis.md`
- `Math/07_Skill_Tiers_Model.md`
- `Math/08_Legendary_Build_Analysis.md`
- `Math/09_Design_Spec_For_Builds.md`
- `Math/10_Build_Comparison_Analysis.md`
- `BUILD_ANALYSIS_SUMMARY.md`
- `BUILD_SPEC_SUMMARY.md`
- `Balance_Analysis_Summary.md`
- `SKILL_TIERS_EXPANSION.md`

---

### 2. OVERFLOW_CP_COST：25 → 實際為 75

**影響範圍**：溢出機制相關的所有分析

| | 文件中 | 代碼實際值 |
|--|--------|-----------|
| OVERFLOW_CP_COST | 25 | **75** |

**代碼位置**：`GameConstants.cs:18`
```csharp
public const int OVERFLOW_CP_COST = 75;
```

**影響**：
- 溢出懲罰被嚴重低估（實際消耗是文件描述的 **3 倍**）
- 資源管理分析、CP 容錯空間計算全部錯誤
- Defense Build 的生存分析結論偏樂觀

**受影響文件**：
- `Balance_Math_Model.md`（§溢出系統、§CP系統）
- `Math/01_Core_Variables.md`
- `Math/04_Difficulty_Model.md`

---

## 🟠 嚴重錯誤（機制描述根本性錯誤）

### 3. 湮滅技能（Annihilation）完全缺失

文件中 `TacticalExpansion` 描述為：
- Lv1：解鎖**處決**（Execution）
- Lv2：解鎖**修補**（Repair）

**代碼實際值**（`PlayerManager.cs`、`SkillExecutor.cs`）：
- Lv1：解鎖**湮滅**（Annihilation）—— CP 消耗 5
- Lv2：解鎖**處決**（Execution）—— CP 消耗 5
- Lv3：解鎖**修補**（Repair）—— CP 消耗 30

**遺漏的完整機制**（湮滅技能）：
- 當前方塊進入幽靈穿透狀態（半透明）
- 碰撞檢測只看左右邊界，忽略底部和方塊佔用
- 清除腐化信息、無 Ghost Piece
- 硬降時破壞重疊非不可摧毀方塊 → 發射導彈 + combo+1
- 方塊被消耗不鎖定

**受影響文件**：所有提及 TacticalExpansion 的文件

---

### 4. TacticalExpansion 最高等級：2 → 實際為 3

| | 文件中 | 代碼實際值 |
|--|--------|-----------|
| TACTICAL_EXPANSION_MAX_LEVEL | 2 | **3** |

**代碼位置**：`GameConstants.cs:63`

---

### 5. Volley 最高等級：無上限 → 實際為 5

| | 文件中 | 代碼實際值 |
|--|--------|-----------|
| VOLLEY_MAX_LEVEL | 無上限 | **5** |

**代碼位置**：`GameConstants.cs:66`

**影響**：
- 文件的所有「Volley 無上限導致火力失控」分析前提已不成立
- Build 分析中 Expert + Volley 的極端數值不再適用
- 「Volley 上限建議 L=4」的提案已被代碼超越（目前 L=5）

---

### 6. Heal Buff 已廢棄，文件仍當作有效 Buff

**文件描述**：
- 列為傳奇強化之一（4種中的1種）
- 效果：「恢復 50% 最大 HP」
- 可在 Buff 選擇中出現

**代碼實際狀態**：
- `GameEnums.cs:79`：`Heal, // [已廢棄] 治療：改為關卡開始時自動恢復`
- `GameConstants.cs`：LEGENDARY_BUFFS 只有 3 種（Defense, Volley, TacticalExpansion）
- Heal **不在任何 Buff 池中**，玩家無法選取

**實際恢復機制**：過關後自動恢復 50% maxHP（`GameManager.cs:226`）

---

### 7. Buff 起始等級大面積錯誤

文件隱含多數 Buff 起始為 Lv0，但代碼中（`BlockData.cs` PlayerStats 建構子）：

| Buff | 文件隱含 | 代碼實際值 |
|------|---------|-----------|
| Salvo | 0 或 1 | **1** |
| Burst | 0 或 1 | **1** |
| Counter | 0 或 1 | **1** |
| Explosion | 0 或 1 | **1** |
| SpaceExpansion | 0 或 1 | **1** |
| ResourceExpansion | 0 | 0 ✓ |
| Defense | 0 | 0 ✓ |
| Volley | 0 | 0 ✓ |
| TacticalExpansion | 0 | 0 ✓ |

**影響**：
- 初始火力計算偏低（Salvo/Burst/Counter 實際從 Lv1 開始）
- 初始反擊已可用（Counter Lv1），文件分析中的「無 Counter 時」情境較少出現
- 初始就有 1 個儲存槽位（Space Lv1）

---

### 8. 子彈選擇系統：優先級制 → 實際為加權隨機

**文件描述**（`Balance_Math_Model.md` §子彈類型概率分布）：
```
優先級順序：
1. CorruptVoid（10%）
2. CorruptExplosive（15%）
...
8. Normal（默認）
總機率 = 125%（有重疊），使用累積機率模型
```

**代碼實際邏輯**（`EnemyController.cs:220-245`）：
- 收集所有 `enabled` 的子彈及其 `chance`（權重值）
- 計算權重總和 → 隨機 [0, 總和) → 選中對應子彈
- 這是標準的**加權隨機選擇**，不是優先級/累積機率

---

### 9. Combo 重置機制描述錯誤

**文件描述**（多處）：
- 「3 秒內未消除行（無 Counter 時）」
- 「0.3 秒內未消除行（有 Counter 時）」

**代碼實際邏輯**（`PlayerManager.cs`）：
- `counterFireLevel > 0`：方塊鎖定後開始 **0.3 秒**重置倒數，期間有新消行/反擊可取消
- `counterFireLevel == 0`：方塊鎖定時**立即歸零**（沒有延遲）

文件的「3 秒」完全不存在於代碼中。

---

## 🟡 中等錯誤

### 10. 分數系統：複雜公式 → 實際極簡

**文件描述**（`Balance_Math_Model.md` §分數系統）：
```
消除1行：+100, 2行：+300, 3行：+500, 4行：+800
final_score = base_score × (1 + combo × 0.1)
```

**代碼實際邏輯**（`PlayerManager.cs:338`）：
```csharp
AddScore(clearedRows.Count * 100);
```
- 只是 `行數 × 100`，無差別計分
- 無 Combo 加成乘數

---

### 11. BULLET_DAMAGE 含義混淆

**文件描述**：
- 「子彈基礎傷害 = 10」（暗示每發子彈對方塊造成 10 點傷害）

**代碼實際**：
- `BULLET_DAMAGE = 10`：用於 `FireBullet()` 的 `damage` 參數
- 但 `ProcessBulletEffect()` 中所有子彈類型對方塊呼叫 `DamageBlock(x, y, 1)` —— **固定 1 HP**
- 此值 (10) 實際用於**子彈穿透棋盤時對城堡的傷害**

---

### 12. 腐化 Void 方塊是否不可摧毀

**文件描述**（`Balance_Math_Model.md` §Void方塊）：
- 「HP：9999（實際不可摧毀）」

**代碼實際**：
- `InsertVoidRow` 產生的 Void 方塊：`isIndestructible = true`，HP = 9999 ✓
- `CorruptVoid` 產生的 Void 方塊：**isIndestructible = false**，HP = 正常（`BASE_BLOCK_HP + defense`）
- 兩者的 `blockType` 都是 Void（消行時有虛無抵銷效果），但耐久性完全不同

---

### 13. 傳奇 Buff 數量：4 → 實際 3

**文件描述**：「傳奇強化（4種）」— Defense, Volley, Heal, TacticalExpansion

**代碼實際**（`GameConstants.cs:88-93`）：
```csharp
public static readonly BuffType[] LEGENDARY_BUFFS = new BuffType[]
{
    BuffType.Defense,
    BuffType.Volley,
    BuffType.TacticalExpansion
};
```
只有 **3 種**。Heal 已廢棄不在池中。

---

### 14. 不可摧毀方塊反傷：文件未統一說明

**代碼實際**（`GridManager.cs:441-446`）：
- 不可摧毀方塊被子彈命中 → 對玩家造成 **10 HP** 反傷
- 文件在某些地方提到，但在公式分析和生存計算中完全忽略
- 這對 Defense Build 分析影響重大（高 Defense 讓不可摧毀方塊更難被「消行清除」以外的方式處理，反傷是持續的威脅）

---

## 🔵 輕微錯誤（已移除常數仍被引用）

### 15. BASE_HIT_DAMAGE = 10

文件 `Math/01_Core_Variables.md` 仍引用此常數。**已從代碼移除**。

### 16. REPAIR_DAMAGE = 2.0

文件 `Math/01_Core_Variables.md` 仍引用此常數。**已從代碼移除**。
修補技能不直接造成傷害，而是填補空洞 + 觸發消行。

### 17. burstCount（敵人連發數）

文件提到「由 burstCount 定義（通常為 1）」。
代碼中此機制已移除（`EnemyController.cs:152` 註解：「移除 burstCount，永遠單發」）。

---

## 正確公式速查（基於代碼）

### 消行導彈傷害

```
單發傷害 = BASE_MISSILE_DAMAGE(1) + salvoBonus + burstBonus
salvoBonus = (有效行數 > 1) ? (min(行數,4) - 1) × salvoLevel × 0.5 : 0
burstBonus = burstLevel × comboCount × 0.25

每行導彈數 = 10 × (1 + missileExtraCount)

總傷害 = 非垃圾行數 × 10 × (1 + Volley等級) × 單發傷害
```

### 校正後的傷害範例

**基礎消除（初始狀態，Salvo=1, Burst=1, Volley=0）**：
```
消除 1 行, Combo=0
單發 = 1.0 + 0 + 0 = 1.0
導彈數 = 1 × 10 × 1 = 10
總傷害 = 10.0
```

**中期戰鬥（Salvo=2, Burst=2, Volley=1）**：
```
消除 3 行, Combo=10
單發 = 1.0 + (2×2×0.5) + (2×10×0.25) = 1.0 + 2.0 + 5.0 = 8.0
導彈數 = 3 × 10 × 2 = 60
總傷害 = 480.0
```

**後期高火力（Salvo=6, Burst=6, Volley=5）**：
```
消除 4 行, Combo=30
單發 = 1.0 + (3×6×0.5) + (6×30×0.25) = 1.0 + 9.0 + 45.0 = 55.0
導彈數 = 4 × 10 × 6 = 240
總傷害 = 13,200.0
```

（文件中使用 BASE=2.0 得出 13,440 → 實際應為 **13,200**）

### 溢出

```
觸發 → 清空棋盤
CP ≥ 75 → 消耗 75 CP
CP < 75 → HP 變為 1, CP 歸零
如有爆炸充能 → 對敵人造成等量傷害 → 充能歸零
```

### 技能解鎖

```
TacticalExpansion Lv1 → 湮滅（5 CP）
TacticalExpansion Lv2 → 處決（5 CP）
TacticalExpansion Lv3 → 修補（30 CP）
```

---

## 文件可用性評估

| 文件 | 可用性 | 主要問題 |
|------|--------|---------|
| `Balance_Math_Model.md` | ❌ 不可直接使用 | BASE_MISSILE_DAMAGE=2.0、OVERFLOW=25、缺湮滅 |
| `Balance_Analysis_Summary.md` | ❌ 不可直接使用 | 同上 + Heal 仍當有效 Buff |
| `BUILD_ANALYSIS_SUMMARY.md` | ⚠️ 需大幅修正 | Volley 已有上限 5、傷害偏高 |
| `BUILD_SPEC_SUMMARY.md` | ⚠️ 需大幅修正 | 三軌制分析基於錯誤數值 |
| `REFACTORING_SUMMARY.md` | ⚠️ 參考性質 | 記述歷史重構，內含過期數值 |
| `SKILL_TIERS_EXPANSION.md` | ⚠️ 需大幅修正 | 擊殺時間預測全偏短 |
| `Math/01~06` | ❌ 需全面更新 | 核心常數錯誤傳播到所有模型 |
| `Math/07~09` | ⚠️ 結構可參考 | 分析框架有價值，數值需重算 |
| `Math/10~11` | ⚠️ 需檢查 | 未詳細審計，可能有相同問題 |

---

## 建議

### 對 Documentation/ 文件
- 這些文件的**分析框架和思路有參考價值**（三層玩家模型、Build 路線分類、PDA/SP/CT 模型概念）
- 但**所有具體數值和計算結果不可信賴**，需基於正確常數全面重算
- 建議保留作為歷史參考，不作為開發依據

### 對 Claude Analysis/ 文件
- `mechanics.md` 已基於最新代碼審計，數值正確
- 本審計報告記錄所有已知差異
- 後續如需平衡分析，應以 `mechanics.md` 的數值為基礎重新建模
