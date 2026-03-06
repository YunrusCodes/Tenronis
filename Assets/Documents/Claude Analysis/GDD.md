# Tenronis — 遊戲設計文件 (GDD)

> **文件版本**：1.0（現狀記錄）
> **更新日期**：2026-03-06
> **定位**：記錄目前已實作完成的所有系統與機制
> **數據來源**：全部取自源碼，無假設值

---

## 目錄

1. [遊戲概覽](#1-遊戲概覽)
2. [核心遊戲循環](#2-核心遊戲循環)
3. [遊戲板與方塊系統](#3-遊戲板與方塊系統)
4. [方塊特性系統](#4-方塊特性系統)
5. [戰鬥系統](#5-戰鬥系統)
6. [敵人系統](#6-敵人系統)
7. [溢出與爆炸充能](#7-溢出與爆炸充能)
8. [技能系統](#8-技能系統)
9. [Buff / 升級系統](#9-buff--升級系統)
10. [關卡系統](#10-關卡系統)
11. [玩家資源系統](#11-玩家資源系統)
12. [UI 系統](#12-ui-系統)
13. [音效系統](#13-音效系統)
14. [視覺特效](#14-視覺特效)
15. [在地化系統](#15-在地化系統)
16. [操作方式](#16-操作方式)
17. [資料架構](#17-資料架構)

---

## 1. 遊戲概覽

### 1.1 基本資訊

| 項目 | 內容 |
|------|------|
| **遊戲名稱** | Tenronis |
| **類型** | 俄羅斯方塊 × 即時戰鬥 × Roguelike |
| **引擎** | Unity 6 (6000.2.6f2) |
| **平台** | PC / Standalone |
| **語言支援** | 繁體中文、英文、日文 |
| **場景架構** | 單場景（SampleScene.unity） |

### 1.2 核心概念

Tenronis 是一款將經典俄羅斯方塊玩法與即時戰鬥及 Roguelike 升級系統深度融合的 2D 遊戲。

玩家在 10×20 的網格上操作標準方塊（Tetromino），透過消除完整行來發射導彈攻擊畫面上方的敵人。敵人會定時向下發射各種類型的子彈，破壞玩家的方塊或施加負面效果。每擊敗一個敵人後，玩家可從隨機的 Buff 選項中選擇升級強化，逐步建構自己的戰鬥流派。

遊戲的核心張力來自三個維度的同時管理：
- **方塊管理**：像傳統俄羅斯方塊一樣保持版面整潔、消除行數
- **戰鬥決策**：在消除行數的同時考慮導彈傷害最大化（多行消除、combo 維持）
- **資源平衡**：管理 HP、CP、技能使用時機，應對不同敵人的攻擊模式

### 1.3 遊戲特色

- **雙向戰鬥**：玩家消除行 → 導彈上飛攻擊敵人；敵人射擊 → 子彈下落破壞方塊
- **方塊即城牆**：玩家的方塊堆疊同時是進攻資源和防禦屏障
- **Roguelike 成長**：每場遊戲的 Buff 選擇不同，鼓勵多種流派
- **五大主題世界**：每個主題有獨特的敵人攻擊風格和視覺主題
- **三語支援**：全面支援繁體中文、英文、日文

---

## 2. 核心遊戲循環

### 2.1 遊戲狀態

遊戲共有 5 個狀態，由 `GameManager` 統一管理：

```
┌────────┐
│  Menu  │ ← 啟動遊戲 / 返回主選單
└───┬────┘
    │ 選擇主題
    ▼
┌─────────┐     ┌──────────┐
│ LevelUp │────→│ Playing  │
│ (升級)  │←────│ (戰鬥中) │
└─────────┘     └────┬─────┘
  ▲ 擊殺敵人         │
  │                   │ HP ≤ 0
  │              ┌────▼─────┐
  │              │ GameOver │
  │              └──────────┘
  │
  │ 最終關卡通過
  ▼
┌──────────┐
│ Victory  │
└──────────┘
```

### 2.2 完整流程

1. **Menu**：玩家從主題列表中選擇一個已解鎖的主題
2. **LevelUp（首次）**：預覽第一關敵人資訊，若該關有獎勵 Buff 則進行選擇
3. **Playing**：即時戰鬥，操作方塊消除行來攻擊敵人
4. **敵人擊殺**：
   - CP 恢復至全滿
   - HP 恢復 50%（首關除外）
   - 進入下一個 LevelUp 狀態
5. **重複 LevelUp → Playing**，直到所有關卡通過或玩家 HP 歸零
6. **Victory**：通過最終關卡，顯示分數。首次通關顯示特殊標記並解鎖下個主題
7. **GameOver**：HP ≤ 0，顯示最終分數，可重試或返回主選單

### 2.3 單場景架構

整個遊戲在單一 Unity 場景（SampleScene.unity）中運行。所有 UI 面板透過啟用/停用（SetActive）來切換，不使用場景載入。這確保了：
- 零載入時間的狀態轉換
- 所有 Singleton Manager 始終存在
- 遊戲狀態透過 `GameEvents.OnGameStateChanged` 事件廣播

---

## 3. 遊戲板與方塊系統

### 3.1 網格規格

| 參數 | 值 |
|------|----|
| 寬度 | 10 格 |
| 高度 | 20 格 |
| Tick 間隔 | 0.8 秒（方塊自然下落速度） |

網格採用 `BlockData[20, 10]` 二維陣列儲存，y=0 為頂部，y=19 為底部。每格可為空或包含一個 `BlockData` 實例。

### 3.2 標準方塊（Tetromino）

使用標準 7 種俄羅斯方塊：

| 方塊 | 顏色 | 矩陣 | 特性 |
|------|------|------|------|
| **I** | 青色 (Cyan) | 4×4 | 特殊踢牆規則 |
| **O** | 黃色 (Yellow) | 2×2 | 不旋轉 |
| **T** | 紫色 (Purple) | 3×3 | 標準踢牆 |
| **J** | 藍色 (Blue) | 3×3 | 標準踢牆 |
| **L** | 橙色 (Orange) | 3×3 | 標準踢牆 |
| **S** | 綠色 (Green) | 3×3 | 標準踢牆 |
| **Z** | 紅色 (Red) | 3×3 | 標準踢牆 |

方塊透過 `GetRandomTetromino()` 隨機選取。

### 3.3 SRS 旋轉系統

實作完整的 **Super Rotation System (SRS)**，包含踢牆機制：

- **JLSTZ 方塊**：每次旋轉嘗試 5 組偏移（基礎位置 + 4 組踢牆偏移）
- **I 方塊**：使用獨立的偏移表（較大偏移量，因 4×4 矩陣）
- **O 方塊**：不旋轉（返回 `{(0,0)}`）

旋轉流程：
1. 計算旋轉後的方塊形狀
2. 依序測試 5 組偏移位置
3. 第一個不與現有方塊或邊界碰撞的位置即為最終位置
4. 若 5 組全部失敗，旋轉不生效

### 3.4 DAS / ARR 操控系統

為了提供專業俄羅斯方塊手感，實作了 DAS（延遲自動移位）與 ARR（自動重複率）：

| 參數 | 值 | 說明 |
|------|----|------|
| DAS Delay | 0.15 秒 | 長按到開始自動移動的延遲 |
| ARR Interval | 0.03 秒 | 自動移動的重複間隔 |
| Soft Drop Interval | 0.05 秒 | 軟降的重複間隔 |

左右移動各自獨立追蹤狀態，支援同時按壓。

### 3.5 方塊儲存系統（Hold）

玩家擁有最多 **4 個儲存槽位**，透過 Space Expansion Buff 逐步解鎖：

| 等級 | 可用槽位 |
|------|---------|
| Lv1（初始） | 1 個 |
| Lv2 | 2 個 |
| Lv3 | 3 個 |
| Lv4（滿級） | 4 個 |

操作規則：
- 按 A/S/D/F 鍵存入/取出對應槽位
- 每個槽位每次落子只能使用一次（鎖定機制）
- 方塊鎖定後所有槽位解鎖
- 儲存的方塊保留腐化狀態

### 3.6 幽靈方塊（Ghost Piece）

在當前方塊的正下方投影顯示落地預覽位置，幫助玩家判斷硬降（Hard Drop）的落點。當網格狀態變化（`OnGridChanged`）時自動更新。

---

## 4. 方塊特性系統

### 4.1 方塊類型（BlockType）

每個方塊除了顏色外，還有特性類型：

| 類型 | 視覺 | 效果 |
|------|------|------|
| **Normal** | 標準外觀 | 無特殊效果 |
| **Void** | 虛無標記符號 | 消除行中含有任何 Void 方塊 → **整次消除不產生導彈**（虛無抵銷） |
| **Explosive** | 爆炸標記符號 | 被敵人子彈破壞時，對玩家造成 **5 HP** 傷害 |

方塊特性透過 `Block.cs` 中的 `symbolRenderer` 顯示對應的視覺標記。

### 4.2 垃圾方塊

敵人的部分攻擊會在場上添加垃圾方塊：

| 類型 | HP | 顏色 | 來源 |
|------|----|------|------|
| 普通垃圾 | 1 + Defense 等級 | Garbage（灰色） | AddBlock 子彈 |
| 爆炸垃圾 | 1 + Defense 等級 | Garbage + Explosive | AddExplosiveBlock 子彈 |

### 4.3 不可摧毀方塊（垃圾行）

敵人的 InsertRow / InsertVoidRow 子彈會從底部插入整行不可摧毀方塊：

| 屬性 | 值 |
|------|----|
| HP | 9999 + Defense 等級 |
| isIndestructible | true |
| 寬度 | 滿行（10 格） |
| 反傷 | 被任何攻擊命中 → 對玩家反傷 **10 HP** |

垃圾行分為普通型（Normal）和虛無型（Void）。虛無型在被消除時會觸發虛無抵銷。

### 4.4 腐化系統

敵人的 CorruptExplosive / CorruptVoid 子彈會腐化**下一個即將出現的方塊**：

- 腐化效果：將方塊中隨機一格的 BlockType 改為 Explosive 或 Void
- 腐化會保留在儲存槽中的方塊上
- 視覺上透過 `NextPiecePreview` 顯示腐化警告
- 每次腐化子彈只影響一格

---

## 5. 戰鬥系統

### 5.1 導彈系統

當玩家消除完整行時，從被消除的位置向上發射導彈攻擊敵人。

**發射規則：**
- 只有**非垃圾方塊行**會發射導彈
- 每個被消除的格子位置發射 `1 + Lv_volley` 發導彈
- 多發導彈以骰子點數方式排列（1發=中心、2發=對角、3發=三角...）
- 導彈速度：20 單位/秒
- 導彈飛出網格頂部即命中敵人

**虛無抵銷：**
- 若消除的行中**任意一格**含有 Void 方塊 → **整次消除不發射任何導彈**
- 但 combo、分數、爆炸充能仍然正常計算

### 5.2 傷害公式

每發導彈的傷害：

```
DMG = 1.0 + salvoBonus + burstBonus

其中：
  salvoBonus = max(0, R-1) × Lv_salvo × 0.5
  burstBonus = Lv_burst × combo × 0.25
  R = min(非垃圾行數, 4)
```

| 要素 | 公式項 | 效果 |
|------|--------|------|
| 基礎傷害 | 1.0 | 每發固定 |
| 齊射加成 | (R-1) × Lv × 0.5 | 多行消除時增傷，1行無效 |
| 連發加成 | Lv × combo × 0.25 | combo 越高傷害越大 |

### 5.3 每次消除的總導彈數

```
總導彈數 = 非垃圾行數 × 10 × (1 + Lv_volley)
```

### 5.4 單次消除總傷害

```
總傷害 = DMG × 總導彈數
```

**範例（起始狀態消 1 行，combo=1）：**
```
DMG = 1 + 0 + 1×1×0.25 = 1.25
導彈 = 1 × 10 × 1 = 10
總傷害 = 12.5
```

### 5.5 反擊系統（Counter Fire）

當剛放置的方塊在 **0.2 秒內**被敵人子彈命中時觸發反擊：

1. combo + 1（取消 combo 重置計時器）
2. 從被命中位置發射 `Lv_counter` 發反擊導彈
3. 反擊導彈傷害 = `1.0 + Lv_burst × combo × 0.25`（無 salvo 加成）
4. 額外獲得 5 點爆炸充能
5. 顯示「反擊!」浮動文字

**前提**：`counterFireLevel > 0`

### 5.6 Combo 機制

- 每次消除行或觸發反擊：combo + 1
- Combo 重置：
  - 若 `counterFireLevel > 0`：方塊鎖定後啟動 0.3 秒計時器，到期重置為 0
  - 若 `counterFireLevel = 0`：方塊鎖定後立即重置為 0
- 消除行或反擊會取消正在倒數的重置計時器

### 5.7 導彈 vs 子彈碰撞

導彈和敵人子彈在飛行中可以互相碰撞：
- 碰撞距離：< 0.5 單位
- 碰撞後：子彈消滅，導彈消耗 1 點穿透值（無穿透則同歸於盡）
- 產生爆炸特效 + 輕微螢幕震動（0.08 強度，0.05 秒）

---

## 6. 敵人系統

### 6.1 基本行為

每關有一個敵人，根據 `StageDataSO` 配置行動：

| 參數 | 說明 | 範圍 |
|------|------|------|
| maxHp | 敵人血量 | 60 ~ 2500 |
| shootInterval | 射擊間隔 | 1.0s ~ 1.9s |
| bulletSpeed | 子彈速度 | 8 ~ 12 |
| isBossStage | Boss 標記 | true/false |
| useSmartTargeting | 智慧瞄準 | true/false |

### 6.2 八種子彈類型

| 子彈類型 | 視覺顏色 | 命中方塊效果 | 附加效果 |
|---------|---------|------------|---------|
| **Normal** | 紅色 #EF4444 | 1 HP 傷害 | 無 |
| **AreaDamage** | 橙色 #F97316 | 3×3 範圍各 1 HP | 無 |
| **AddBlock** | 綠色 #4ADE80 | 1 HP 傷害 | 在命中位置上方添加 1 個垃圾方塊 |
| **AddExplosiveBlock** | 黃色 #FFE804 | 1 HP 傷害 | 在命中位置上方添加 1 個爆炸垃圾方塊 |
| **InsertRow** | 紫色 #A855F7 | 1 HP 傷害 | 從底部插入普通不可摧毀行 |
| **InsertVoidRow** | 深灰 #333333 | 1 HP 傷害 | 從底部插入虛無不可摧毀行 |
| **CorruptExplosive** | 洋紅 #FF00FF | 1 HP 傷害 | 腐化下個方塊的隨機一格為爆炸方塊 |
| **CorruptVoid** | 青藍 #00FFFF | 1 HP 傷害 | 腐化下個方塊的隨機一格為虛無方塊 |

所有子彈都會先對命中的方塊造成 1 HP 傷害，然後執行附加效果。若子彈未命中任何方塊直接到達底部，對城堡（玩家）造成 **10 HP** 傷害。

### 6.3 子彈選擇：加權隨機

每關配置中，各子彈類型有 `enabled`（是否啟用）和 `chance`（權重值）：

```
實際選中機率 = 該子彈 chance / Σ(所有啟用子彈的 chance)
```

**範例**：某關啟用 Normal(chance=0.6) + AddBlock(chance=0.3) + CorrExp(chance=0.15)
```
Normal 機率 = 0.6 / (0.6+0.3+0.15) = 57%
AddBlock 機率 = 0.3 / 1.05 = 29%
CorrExp 機率 = 0.15 / 1.05 = 14%
```

### 6.4 目標列選擇

| 模式 | 條件 | 行為 |
|------|------|------|
| 隨機 | 預設 | 隨機選擇 0~9 列 |
| 瞄準最高列 | `addBlockTargetsHigh = true` | AddBlock 類子彈瞄準方塊堆最高的列 |
| 瞄準最低列 | `areaDamageTargetsLow = true` | AreaDamage 子彈瞄準方塊堆最低的列 |
| 智慧瞄準 | `useSmartTargeting = true` | 所有子彈瞄準最高列 |

> 全遊戲僅 Theme_3 Stage 9 啟用 `useSmartTargeting`。

### 6.5 敵人擊殺動畫

敵人 HP 歸零後的擊殺序列：
1. **Phase 1**（1 秒）：精靈震動 + 透明度 1.0→0.3 + 向下墜落 2 單位
2. **Phase 2**（2 秒）：透明度 0.3→0 + 繼續墜落 3 單位
3. 清除所有殘留的敵人子彈
4. 觸發 `EnemyDefeated` 事件

### 6.6 傷害視覺回饋

導彈命中敵人時，根據傷害強度（intensityLevel 0~8）產生不同數量的命中特效。每波最多 4 個特效，排隊系統動態調整處理器數量避免效果堆積。

---

## 7. 溢出與爆炸充能

### 7.1 溢出觸發

以下情況觸發溢出：
- 方塊鎖定時，頂行已有方塊（標準俄羅斯方塊溢出）
- InsertRow/InsertVoidRow 子彈嘗試插入行時，頂行已有方塊
- AddBlock 子彈在 y=0（頂行）嘗試添加方塊時

### 7.2 溢出處理

```
1. 清空整個網格
2. CP 判定：
   CP ≥ 75 → CP -= 75
   CP < 75 → CP = 0, HP = 1（瀕死狀態）
3. 爆炸充能：
   若當前充能 > 0 → 對敵人造成 = 充能值 的傷害，充能歸零
```

### 7.3 爆炸充能系統

| 參數 | 值 |
|------|----|
| 初始充能上限 | 200 |
| 每級增加上限 | +200 |
| 最高等級 | 4（上限 800） |
| 消除行充能 | +50 / 次 |
| 反擊充能 | +5 / 次 |
| 觸發時機 | 溢出時 |
| 傷害 | = 當前充能值 |

**充能上限表：**

| Explosion 等級 | 充能上限 |
|---------------|---------|
| Lv1（初始） | 200 |
| Lv2 | 400 |
| Lv3 | 600 |
| Lv4（滿級） | 800 |

---

## 8. 技能系統

技能透過 TacticalExpansion（戰術擴展）Buff 逐級解鎖：

### 8.1 湮滅（Annihilation）

| 屬性 | 值 |
|------|----|
| 解鎖 | TacticalExpansion Lv1 |
| CP 消耗 | 5 |
| 按鍵 | 1 |

**效果**：當前方塊進入「幽靈穿透」狀態，落下時不與任何方塊碰撞，會摧毀路徑上的方塊。適合在方塊堆積過高時清出空間。

### 8.2 處決（Execution）

| 屬性 | 值 |
|------|----|
| 解鎖 | TacticalExpansion Lv2 |
| CP 消耗 | 5 |
| 按鍵 | 2 |

**效果**：
1. 清除每列**最頂部**的非垃圾方塊（削平表面）
2. 從每個清除位置發射導彈
3. 對敵人造成固定 **4 點傷害**
4. combo + 1

**前提**：場上必須有可被處決的方塊（`HasExecutableBlocks` 檢查）。

### 8.3 修補（Repair）

| 屬性 | 值 |
|------|----|
| 解鎖 | TacticalExpansion Lv3 |
| CP 消耗 | 30 |
| 按鍵 | 3 |

**效果**：
1. 使用 BFS 從頂部邊界開始搜索
2. 找出所有「封閉空洞」（無法從頂部到達的空格）
3. 填補這些空洞為灰色方塊（HP = 1 + Defense 等級）
4. 若填補後形成完整行，自動消除

**前提**：必須存在封閉空洞（`HasClosedHoles` 檢查）。

---

## 9. Buff / 升級系統

### 9.1 Buff 分類

#### 普通強化（Normal Buffs）— 6 種

從通用池中隨機出現，有明確等級上限。

| Buff | 中文名 | 起始等級 | 上限 | 效果 |
|------|--------|---------|------|------|
| **Salvo** | 齊射強化 | 1 | 6 | 多行消除時，每額外行增加 `Lv × 0.5` 導彈傷害 |
| **Burst** | 連發強化 | 1 | 6 | 每點 combo 增加 `Lv × 0.25` 導彈傷害 |
| **Counter** | 反擊強化 | 1 | 6 | 反擊時發射 `Lv` 發導彈；啟用 0.3s combo 延遲 |
| **Explosion** | 爆炸充能 | 1 | 4 | 每級充能上限 +200（溢出時釋放傷害） |
| **SpaceExpansion** | 空間擴充 | 1 | 4 | 每級 +1 個方塊儲存槽位 |
| **ResourceExpansion** | 資源擴充 | 0 | 3 | 每級 CP 上限 +50 |

#### 傳奇強化（Legendary Buffs）— 3 種

較稀有，特殊效果或無上限。

| Buff | 中文名 | 起始等級 | 上限 | 效果 |
|------|--------|---------|------|------|
| **Defense** | 裝甲強化 | 0 | **無上限** | 所有方塊（含垃圾方塊）HP +1/級 |
| **Volley** | 協同火力 | 0 | 5 | 每格額外發射 +1 導彈/級（乘法加成） |
| **TacticalExpansion** | 戰術擴展 | 0 | 3 | Lv1=湮滅、Lv2=處決、Lv3=修補 |

> **注意**：`Heal`（治療）在枚舉中存在但已**廢棄**，不出現在任何 Buff 池中。

### 9.2 Buff 獎勵機制

每擊敗一個敵人後，根據該關卡的 `rewardBuffCount` 獎勵 Buff 選擇機會：

1. 從可用的 Buff 池中（排除已滿級的）以加權隨機抽取 3 個選項
2. 玩家從中選擇 1 個進行升級
3. 若有多個獎勵（rewardBuffCount > 1），重複選擇流程
4. 若普通池已全部滿級，自動切換為傳奇池
5. 傳奇 Buff 出現時，顯示「加碼強化」特殊動畫

### 9.3 Buff 加權

每個 Buff 在 `BuffDataSO` 中有 `spawnWeight`（0~1），數值越高越容易被抽到。

---

## 10. 關卡系統

### 10.1 結構概覽

遊戲包含 **5 個主題**，每個主題約 **10 關**：

| 主題 | 代碼 | 名稱 | 敵人HP範圍 | 最終Boss HP | 特色 |
|------|------|------|-----------|-------------|------|
| 0 | Tutorial | 教學 演習之夜 | 60~900 | 900 | 逐步引入機制 |
| 1 | Nightmare | 夢襲之夜 | 120~2000 | 2000 | 標準難度，全面子彈 |
| 2 | Inferno | 煉獄主題 | 140~1800 | 1800 | 腐化為主（CorrExp/CorrVoid） |
| 3 | Resentment | 怨念主題 | 120~2500 | 2500 | Boss 最多，唯一智慧瞄準 |
| 4 | Gravity | 重力主題 | 150~1400 | 1400 | **無直接傷害子彈**，純空間壓迫 |

### 10.2 主題解鎖機制

- 每個主題有 `unlockKey`（前置條件）和 `passKey`（通關標記）
- 通關後將 `passKey` 存入 `PlayerPrefs`
- 下個主題的 `unlockKey` 檢查上個主題的 `passKey`
- Theme 0 無 `unlockKey`，始終可用

### 10.3 各主題攻擊風格

| 主題 | 核心子彈類型 | 威脅維度 | 建議策略 |
|------|------------|---------|---------|
| **Tutorial** | Normal → Area → AddBlock | 漸進式學習 | 任意 |
| **Nightmare** | 全面混合 | 均衡威脅 | Salvo + Burst |
| **Inferno** | CorrVoid（前期）、CorrExp（後期） | 方塊腐化 | Defense + Counter |
| **Resentment** | Area + AddExp + CorrExp | 高爆發傷害 | Burst + Volley |
| **Gravity** | InsertRow + InsertVoidRow + CorrExp | 空間壓迫 | Explosion + SpaceExpansion |

### 10.4 共通數值遞增模式

所有主題的射擊間隔和子彈速度遵循相同的遞增模式：

**射擊間隔**（秒，越小越快）：
```
Stage 1: 1.8~1.9 → Stage 5: 1.0~1.2 → Stage 6: 1.6（重置）→ Stage 10: 1.0
```

**子彈速度**（越大越快）：
```
Stage 1: 8.0 → Stage 5: 10.0 → Stage 10: 12.0
```

### 10.5 Boss 分布

| 主題 | Boss 關卡 | Boss 數量 |
|------|----------|----------|
| Theme 0 | 5, 6, 10 | 3 |
| Theme 1 | 5, 10 | 2 |
| Theme 2 | 5, 9, 10 | 3 |
| Theme 3 | 3, 5, 6, 9, 10 | **5** |
| Theme 4 | 5, 8, 10 | 3 |

Boss 關卡使用專屬 BGM，並在 LevelUp 畫面顯示「遭遇強敵!!!」動畫。

### 10.6 Buff 獎勵分布

各主題提供的總 Buff 獎勵數：

| 主題 | 總 Buff 數 |
|------|-----------|
| Theme 0 | 16 |
| Theme 1 | **21** |
| Theme 2 | 19 |
| Theme 3 | 19 |
| Theme 4 | 18 |

---

## 11. 玩家資源系統

### 11.1 HP（城堡耐久）

| 參數 | 值 |
|------|----|
| 初始 / 最大 | 100 |
| 通關恢復 | 50%（首關除外） |
| 歸零 | 遊戲結束（GameOver） |

**受傷來源：**

| 來源 | 傷害 |
|------|------|
| 子彈穿透到底 | 10 HP |
| 爆炸方塊被摧毀 | 5 HP |
| 不可摧毀方塊反傷 | 10 HP |
| 溢出（CP不足） | HP → 1 |

### 11.2 CP（城堡點數）

| 參數 | 值 |
|------|----|
| 初始 / 基礎上限 | 100 |
| ResourceExpansion 加成 | +50 / 級（最大 250） |
| 通關恢復 | 全滿 |

**消耗來源：**

| 用途 | 消耗 |
|------|------|
| 溢出（CP足夠） | 75 |
| 湮滅 | 5 |
| 處決 | 5 |
| 修補 | 30 |

### 11.3 分數

```
每消除 1 行 = +100 分
```

極為簡單的計分規則，無 combo 加成或特殊乘數。

---

## 12. UI 系統

### 12.1 總覽

所有 UI 在單一場景中以面板啟停方式管理。主要面板：

| 面板 | 對應狀態 | 說明 |
|------|---------|------|
| `menuPanel` | Menu | 主題選擇列表 |
| `levelUpPanel` | LevelUp | 敵人預覽 + Buff 選擇 |
| `gameplayPanel` | Playing | 遊戲 HUD |
| `gameOverPanel` | GameOver | 結算 + 重試/返回 |
| `victoryPanel` | Victory | 通關結算 + 首通標記 |
| `quitPanel` | Playing | 退出確認對話框 |

### 12.2 主選單（Menu）

- 動態生成主題按鈕列表
- 鎖定的主題按鈕不可互動
- 點擊主題按鈕開始遊戲
- 語言切換由 `LanguageManager` 控制

### 12.3 LevelUp 選單

由 `RoguelikeMenu.cs` 管理，包含多個子區域：

**敵人資訊面板**：
- 敵人圖示
- 攻擊模式預覽（顯示啟用的子彈類型）
- 敵人名稱和關卡編號
- 可收合以節省空間

**Buff 選擇區**：
- 顯示 3 個隨機 Buff 選項
- 每個選項包含：名稱、描述、等級變化
- 分為普通 / 傳奇兩種分頁

**玩家狀態顯示**：
- 可切換詳細 / 簡易檢視
- 顯示所有已獲得的強化及等級

**教學提示系統**：
- 首次遊玩時自動顯示提示
- 帶有逐字動畫和精靈示意圖
- 7 個內建提示：
  1. 先觀察再落子
  2. 工程等級上限差異
  3. 腐化爆炸警告
  4. 反擊亦為連發
  5. 溢出必有代價
  6. 火力不足即恐懼
  7. 戰術性溢出

**特殊動畫**：
- 「遭遇強敵!!!」Boss 戰文字動畫
- 「加碼強化」傳奇 Buff 動畫
- 關卡名稱逐字淡入

### 12.4 遊戲 HUD（Playing）

| 元素 | 位置 | 說明 |
|------|------|------|
| 玩家 HP 條 | 左側 | Slider + 數值文字 |
| 玩家 CP 條 | 左側 | Slider + 數值文字 |
| 敵人 HP 條 | 上方 | Slider + 數值文字 |
| Combo 計數 | 右側 | 帶滑入/推出動畫 |
| 分數 | 右上 | 純文字 |
| 爆炸充能 | 下方 | 「衝擊炮充能: X/Y」 |
| 溢出成本 | 下方 | CP ≥ 75 顯示成本 |
| 關卡進度 | 上方 | 「STAGE X / Y」 |
| 技能面板 | 右下 | 3 個技能按鈕 + CP 消耗 |
| 儲存槽位 | 左下 | 4 個預覽格（鎖定/解鎖） |
| 下一塊預覽 | 右上 | 含腐化警告 |

**Combo 動畫系統**：
- 2x+ 時從右側滑入
- 3x+ 時新數字從上推入、舊數字向下淡出
- 顏色隨 combo 變化：2~4 原色 → 5~9 黃色 → 10~19 橙色 → 20+ 紅色

**特殊通知**：
- 「全彈齊射!」— 消除 4 行時顯示
- 「衝擊爆破!」— 爆炸充能釋放時顯示
- 「虛無抵銷!」— 虛無方塊消除時顯示
- 技能名稱 — 使用技能時顯示

### 12.5 GameOver 面板

- 最終分數顯示
- 「重試」按鈕（重新開始同主題）
- 「返回主選單」按鈕

### 12.6 Victory 面板

- 最終分數顯示
- 「返回主選單」按鈕
- 首次通關標記（解鎖新主題的提示文字）

### 12.7 浮動文字系統

`PopupText` + `PopupTextManager` 實現世界空間浮動文字：
- 將世界座標轉換為 UI Canvas 座標
- 文字向上飄動並淡出（1 秒壽命）
- 用於傷害數字、「反擊!」「城堡受損」等

---

## 13. 音效系統

### 13.1 架構

`AudioManager` 為 Singleton，使用 `DontDestroyOnLoad` 持久存在。
所有音效透過 `GameEvents` 事件驅動播放。

| 音源 | 預設音量 | 用途 |
|------|---------|------|
| sfxSource | 0.7 | 音效 |
| musicSource | 0.5 | 背景音樂（循環） |

### 13.2 背景音樂

| BGM | 觸發 |
|-----|------|
| normalBGM | 一般關卡 |
| bossBGM | Boss 關卡 |

### 13.3 音效列表

**遊戲操作音效：**
- 導彈發射音（missileSound）
- 爆炸音（explosionSound）
- 方塊旋轉音（rotateSound）
- 碰撞音（impactSound）
- 方塊鎖定音（lockSound）
- 反擊觸發音（counterFireSound）

**敵人射擊音效（8種，對應各子彈類型）：**
- Normal / AddBlock / AreaDamage / InsertRow
- AddExplosiveBlock / InsertVoidRow / CorruptExplosive / CorruptVoid

**特殊事件音效：**
- 敵人添加方塊音（enemyAddBlockSound）
- 敵人添加爆炸方塊音（enemyAddExplosiveBlockSound）
- 虛無抵銷音（voidNullifySound）
- 腐化爆炸/虛無音（corruptExplosiveSound / corruptVoidSound）
- 插入垃圾行音（insertRowSound / insertVoidRowSound）
- 溢出音（overflowSound）
- 爆炸方塊被摧毀音（explosiveBlockDestroyedSound）

---

## 14. 視覺特效

### 14.1 螢幕震動（ScreenShake）

| 模式 | 強度 | 持續時間 | 觸發 |
|------|------|---------|------|
| 標準震動 | 0.3 | 0.3 秒 | 溢出 |
| 輕微震動 | 0.1 | 0.15 秒 | 玩家受傷 |
| 碰撞震動 | 0.08 | 0.05 秒 | 導彈攔截子彈 |

使用 `AnimationCurve.EaseInOut` 實現平滑衰減，以 `Random.insideUnitSphere`（Z=0）產生隨機偏移。

### 14.2 玩家精靈系統（PlayerVisualController）

**多層精靈結構：**

| 層 | 用途 |
|----|------|
| playerSprite | 主體精靈（隨主題切換） |
| overlaySpriteRenderer | 狀態覆蓋層 |

**覆蓋精靈狀態：**

| 精靈 | 觸發條件 |
|------|---------|
| damagedOverlaySprite | 受傷閃爍（0.2秒）或 HP=1 持續顯示 |
| lowHpOverlaySprite | HP ≤ 30% 持續顯示 |
| overflowSprite | 溢出閃爍（0.2秒） |

**視覺動畫：**
- 受傷震動：強度 ±0.2，持續 0.3 秒，線性衰減
- 攻擊特效：消除行時從 4 個固定位置噴出特效粒子
- 受傷特效：在精靈隨機位置生成傷害粒子（0.05s 間隔，2s 壽命）

### 14.3 網格邊框（GridBorder）

使用 `LineRenderer` 繪製遊戲場邊框：
- 白色，寬度 0.1
- SortingOrder = 20
- 根據 GridManager 的 BlockSize 和 GridOffset 動態計算位置
- Playing 時顯示，Menu/GameOver/Victory 時隱藏

### 14.4 方塊視覺

`Block.cs` 控制方塊外觀：
- 顏色根據 `BlockColor` 設定
- 透明度隨 HP 變化：`opacity = Lerp(0.3, 1.0, hp/maxHp)`（不可摧毀方塊固定 1.0）
- 腐化方塊透過 `symbolRenderer` 顯示對應圖標（爆炸/虛無符號）

### 14.5 子彈視覺

每種子彈類型有：
- 獨立的 `RuntimeAnimatorController`（8 套動畫）
- 獨立的顏色標識（見 6.2 節色碼表）
- 命中方塊時產生類型專屬的命中特效（0.5 秒壽命）

### 14.6 導彈視覺

- 白色精靈
- 帶有 `TrailRenderer` 尾跡效果
- 支援穿透值（ConsumePierce）

---

## 15. 在地化系統

### 15.1 架構

| 元件 | 功能 |
|------|------|
| `LanguageManager` | 管理語言切換，持久化偏好 |
| `LocalizationHelper` | 靜態工具類，提供翻譯取值 |
| Unity Localization Package | 底層框架 |

### 15.2 支援語言

| 代碼 | 語言 | 顯示名稱 |
|------|------|---------|
| zh-TW | 繁體中文 | 繁體中文 |
| en | 英文 | English |
| ja | 日文 | 日本語 |

### 15.3 翻譯資源

- 翻譯表：`Assets/Localization/UI_Text.csv`
- 包含 **150+ 翻譯鍵值**
- 涵蓋：UI 文字、Buff 名稱/描述、技能名稱、提示訊息、戰鬥通知
- 編譯資源：`UI_Text_zh-TW.asset`、`UI_Text_en.asset`、`UI_Text_ja.asset`

### 15.4 多語言支援範圍

| 項目 | 支援方式 |
|------|---------|
| UI 文字 | CSV 翻譯表 |
| 關卡名稱 | StageDataSO 三語欄位 |
| 主題名稱/描述 | StageSetSO 三語欄位 |
| Buff 名稱/描述 | BuffDataSO 三語欄位 |
| 首通提示 | StageSetSO.firstPassInfo 三語欄位 |
| 戰鬥浮動文字 | LocalizationHelper 即時查詢 |

### 15.5 語言切換

- 玩家在主選單切換語言
- 偏好存入 `PlayerPrefs`（key: `"SelectedLanguage"`）
- 切換後重新載入場景以套用所有文字
- 啟動時根據系統語系或已保存偏好自動選擇

---

## 16. 操作方式

### 16.1 鍵盤配置

目前僅支援**鍵盤操作**（無觸控/手柄支援）。

| 操作 | 按鍵 | 說明 |
|------|------|------|
| 左移 | ← | 支援 DAS/ARR |
| 右移 | → | 支援 DAS/ARR |
| 軟降 | ↓ | 加速落下，支援 ARR |
| 硬降 | Space | 直接落到底部並鎖定 |
| 順時針旋轉 | ↑ / X | SRS 踢牆 |
| 逆時針旋轉 | Z | SRS 踢牆 |
| 儲存槽 1 | A | 存入/取出第 1 個儲存槽 |
| 儲存槽 2 | S | 存入/取出第 2 個儲存槽 |
| 儲存槽 3 | D | 存入/取出第 3 個儲存槽 |
| 儲存槽 4 | F | 存入/取出第 4 個儲存槽 |
| 湮滅 | 1 / C | TacticalExpansion Lv1+ |
| 處決 | 2 / V | TacticalExpansion Lv2+ |
| 修補 | 3 / B | TacticalExpansion Lv3+ |

### 16.2 操控參數

| 參數 | 值 | 說明 |
|------|----|------|
| DAS Delay | 150ms | 長按延遲 |
| ARR Interval | 30ms | 自動重複間隔 |
| Soft Drop Interval | 50ms | 軟降重複間隔 |
| Tick Rate | 800ms | 自然下落間隔 |

---

## 17. 資料架構

### 17.1 ScriptableObject 結構

| SO 類型 | 用途 | 路徑 |
|---------|------|------|
| `StageDataSO` | 單關卡配置（HP、子彈、獎勵） | ScriptableObjects/StageData/Theme_*/ |
| `StageSetSO` | 主題配置（關卡列表、背景、解鎖） | ScriptableObjects/Theme_*/ |
| `BuffDataSO` | Buff 展示資料（名稱、描述、圖示、權重） | ScriptableObjects/Buffs/ |

**StageDataSO 欄位：**
- 基本：stageName(3語)、stageIndex、isBossStage
- 數值：maxHp、shootInterval、bulletSpeed、rewardBuffCount
- 子彈：8 個 EnemyAbility（enabled + chance）
- 瞄準：useSmartTargeting、addBlockTargetsHigh、areaDamageTargetsLow
- 提示：hintKey

**StageSetSO 欄位：**
- 展示：themeName(3語)、themeIcon、themeColor、description(3語)
- 視覺：battleBackgroundSprite、enemyIntroBackgroundSprite、engineerBackgroundSprite、playerSprite
- 關卡：stages（List\<StageDataSO\>）
- 進度：passKey、unlockKey、firstPassInfo(3語)

**BuffDataSO 欄位：**
- 展示：buffName(3語)、description(3語)、icon、iconColor
- 資料：buffType、spawnWeight

### 17.2 事件系統（GameEvents）

採用**靜態事件匯流排**模式，所有系統間通訊透過 `GameEvents` 靜態類：

**遊戲狀態事件：**
- `OnGameStateChanged(GameState)` — 狀態切換
- `OnEnemyDefeated` — 敵人擊殺
- `OnGridOverflow` — 溢出

**戰鬥事件：**
- `OnRowsCleared(List<int>, List<int>, bool)` — 消除行（所有行、非垃圾行、是否含虛無）
- `OnEnemyDamaged(float, int)` — 敵人受傷（傷害、強度等級）
- `OnPlayerDamaged(int)` — 玩家受傷
- `OnMissileFired(float)` — 導彈發射
- `OnComboChanged(int)` — combo 變化

**UI / 視覺事件：**
- `OnGridChanged` — 網格更新（幽靈方塊重算）
- `OnPieceLocked` — 方塊鎖定
- `OnShowPopupText(string, Color, Vector2)` — 浮動文字
- `OnSkillUsed(string)` — 技能使用

**音效事件：**
- `OnPlayMissileSound`、`OnPlayExplosionSound`、`OnPlayRotateSound` 等 17+ 音效事件
- `OnPlayEnemyShootSound(BulletType)` — 依子彈類型播放不同音效

### 17.3 Manager Singleton 架構

所有核心系統使用 Singleton 模式，透過 `GameInitializer` 確保啟動時存在：

| Manager | 職責 |
|---------|------|
| `GameManager` | 遊戲狀態機、關卡進度、Buff 獎勵 |
| `GridManager` | 網格操作、行消除、溢出處理 |
| `CombatManager` | 導彈/子彈管理、碰撞檢測、傷害計算 |
| `PlayerManager` | 玩家數據、Buff 升級、combo 管理 |
| `InputManager` | 鍵盤輸入、DAS/ARR |
| `LanguageManager` | 語言切換、偏好持久化 |
| `AudioManager` | 音效/BGM 播放 |

**對象池系統**：
- CombatManager 管理導彈池（50 個）和子彈池（30 個）
- 使用自定義 `ObjectPool<T>` 實現

### 17.4 場景物件

由 `GameInitializer` 確保以下物件存在：
- 所有 Manager（從 prefab 實例化或場景中尋找）
- `TetrominoController`（方塊操控）
- `EnemyController`（敵人行為）
- `PlayerVisualController`（玩家視覺）

---

## 附錄 A：數值速查表

### 核心常數

| 常數 | 值 | 說明 |
|------|----|------|
| BOARD_WIDTH | 10 | 網格寬度 |
| BOARD_HEIGHT | 20 | 網格高度 |
| TICK_RATE | 0.8s | 自然下落間隔 |
| PLAYER_MAX_HP | 100 | 玩家初始最大HP |
| PLAYER_MAX_CP | 100 | 玩家初始最大CP |
| OVERFLOW_CP_COST | 75 | 溢出CP消耗 |
| BASE_MISSILE_DAMAGE | 1.0 | 基礎導彈傷害 |
| SALVO_DAMAGE_MULTIPLIER | 0.5 | 齊射加成倍率 |
| BURST_DAMAGE_MULTIPLIER | 0.25 | 連發加成倍率 |
| EXECUTION_DAMAGE | 4.0 | 處決固定傷害 |
| MISSILE_SPEED | 20 | 導彈飛行速度 |
| BULLET_DAMAGE | 10 | 子彈穿透傷害 |
| COUNTER_FIRE_TIME_WINDOW | 0.2s | 反擊判定窗口 |
| COMBO_RESET_DELAY | 0.3s | Combo重置延遲 |
| BASE_BLOCK_HP | 1 | 基礎方塊HP |
| GARBAGE_BLOCK_HP | 1 | 垃圾方塊HP |
| INDESTRUCTIBLE_BLOCK_HP | 9999 | 不可摧毀方塊HP |

### Buff 等級表

| Buff | 起始 | 上限 | 類型 |
|------|------|------|------|
| Salvo | 1 | 6 | 普通 |
| Burst | 1 | 6 | 普通 |
| Counter | 1 | 6 | 普通 |
| Explosion | 1 | 4 | 普通 |
| SpaceExpansion | 1 | 4 | 普通 |
| ResourceExpansion | 0 | 3 | 普通 |
| Defense | 0 | ∞ | 傳奇 |
| Volley | 0 | 5 | 傳奇 |
| TacticalExpansion | 0 | 3 | 傳奇 |

---

## 附錄 B：相關文件索引

| 文件 | 路徑 | 內容 |
|------|------|------|
| 核心常數 | `Scripts/Data/GameConstants.cs` | 所有遊戲常數 |
| 遊戲枚舉 | `Scripts/Data/GameEnums.cs` | 狀態/類型定義 |
| 機制詳述 | `Documents/Claude Analysis/mechanics.md` | 完整機制文件 |
| 數學模型 | `Documents/Claude Analysis/Math/` | 5 篇數值分析 |
| 文檔審計 | `Documents/Claude Analysis/Documentation_Audit.md` | 舊文檔勘誤 |
