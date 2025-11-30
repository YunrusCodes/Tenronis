# Tenronis - Unity 設置指南

## 快速開始

### 步驟 1: 建立新場景

1. 在Unity中建立新場景：`File > New Scene > Basic (Built-in)`
2. 儲存場景為 `MainGame.unity`

### 步驟 2: 建立管理器物件

在Hierarchy中建立以下空物件：

```
[右鍵 > Create Empty]
命名為: GameManagers
```

在GameManagers下建立子物件：
- GameManager (添加 `GameManager` 腳本)
- GridManager (添加 `GridManager` 腳本)
- PlayerManager (添加 `PlayerManager` 腳本)
- CombatManager (添加 `CombatManager` 腳本)
- AudioManager (添加 `AudioManager` 腳本)
- InputManager (添加 `InputManager` 腳本)

### 步驟 3: 建立遊戲控制器

```
[右鍵 > Create Empty]
命名為: GameControllers
```

在GameControllers下建立：
- TetrominoController (添加 `TetrominoController` 腳本)
- EnemyController (添加 `EnemyController` 腳本)
  - 在EnemyController下建立子物件：EnemySprite (添加 `Sprite Renderer`)
  - 將EnemySprite拖入EnemyController的 `Enemy Sprite` 欄位
- PlayerVisualController (添加 `PlayerVisualController` 腳本)
  - 在PlayerVisualController下建立子物件：PlayerSprite (添加 `Sprite Renderer`)
  - 將PlayerSprite拖入PlayerVisualController的 `Player Sprite` 欄位

### 步驟 4: 建立方塊預製體

1. 建立新物件: `GameObject > 2D Object > Sprite > Square`
2. 命名為 `Block`
3. 添加 `Block` 腳本
4. 在Block下創建子物件：
   - 右鍵 Block > `Create Empty`
   - 命名為 `SymbolText`
   - 添加組件: `Add Component > TextMeshPro - Text`
5. 設置 SymbolText：
   - Position: `(0, 0, 0)`
   - Width: `1`
   - Height: `1`
   - Alignment: 水平和垂直都居中
   - Font Size: `8`
   - Color: 白色（會由腳本控制）
   - Sorting Layer: 確保在方塊上方
6. 設置 Block：
   - Scale: (0.9, 0.9, 1) - 讓方塊之間有間隙
   - Sprite Renderer Color: 白色
   - Symbol Text: 拖入SymbolText子物件
7. 拖曳到 `Assets/Prefabs/Blocks/` 資料夾
8. 刪除場景中的實例

### 步驟 5: 建立導彈預製體

1. 建立新物件: `GameObject > 2D Object > Sprite > Circle`
2. 命名為 `Missile`
3. 設置：
   - Scale: (0.15, 0.4, 1) - 細長形狀
   - Color: 青色 (#22D3EE)
4. 添加 `Trail Renderer` 組件：
   - Time: 0.3
   - Width: 0.1 → 0.05
   - Color: 青色漸變到透明
5. 添加 `Missile` 腳本
6. 拖曳到 `Assets/Prefabs/Projectiles/`
7. 刪除場景實例

### 步驟 6: 建立子彈預製體

1. 建立新物件: `GameObject > 2D Object > Sprite > Circle`
2. 命名為 `Bullet`
3. 設置：
   - Scale: (0.2, 0.2, 1)
   - Color: 紅色 (#EF4444)
4. 添加 `Bullet` 腳本
5. 拖曳到 `Assets/Prefabs/Projectiles/`
6. 刪除場景實例

### 步驟 7: 設置GridManager

選擇 GridManager 物件：

1. **Block Prefab**: 拖入剛建立的Block預製體
2. **Grid Container**: 建立新空物件命名為"Grid"，拖入
3. **Block Size**: 1
4. **Grid Offset**: (-5, -10) - 讓網格置中

### 步驟 8: 設置CombatManager

選擇 CombatManager 物件：

1. **Missile Prefab**: 拖入Missile預製體
2. **Bullet Prefab**: 拖入Bullet預製體
3. **Explosion Effect Prefab**: 拖入爆炸特效預製體
   - 推薦使用: `Assets/GabrielAguiarProductions/FreeQuickEffectsVol1/Prefabs/vfx_Explosion_01.prefab`
   - 或使用: `vfx_Impact_01.prefab` (冲击效果)
4. **Projectile Container**: 建立新空物件命名為"Projectiles"，拖入

### 步驟 8.5: 設置EnemyController

選擇 EnemyController 物件：

1. **Enemy Sprite**: 拖入EnemySprite子物件
2. **Damage Effect Prefab**: 拖入受傷特效預製體
   - 推薦使用: `Assets/GabrielAguiarProductions/FreeQuickEffectsVol1/Prefabs/vfx_Impact_01.prefab`
   - 或使用: `vfx_Explosion_01.prefab` (爆炸效果)
   - 或使用: `vfx_Sparks_01.prefab` (火花效果)

### 步驟 8.6: 設置PlayerVisualController

#### 8.6.1 創建特效點

1. 在 PlayerVisualController 下創建 4 個特效點：
   - 右鍵 PlayerVisualController > `Create Empty`
   - 命名為：`EffectPoint_1`, `EffectPoint_2`, `EffectPoint_3`, `EffectPoint_4`
2. 調整特效點位置（推薦配置）：
   - EffectPoint_1: Position `(-1, 0.5, 0)` （左上）
   - EffectPoint_2: Position `(1, 0.5, 0)` （右上）
   - EffectPoint_3: Position `(-1, -0.5, 0)` （左下）
   - EffectPoint_4: Position `(1, -0.5, 0)` （右下）

#### 8.6.2 配置PlayerVisualController

選擇 PlayerVisualController 物件：

1. **Player Sprite**: 拖入PlayerSprite子物件
2. **Default Sprite**: 拖入玩家默認圖片（必需）
3. **Damaged Sprite**: 拖入受傷時的圖片（可選，受傷時短暫顯示）
4. **Low Hp Sprite**: 拖入低HP時的圖片（可選，HP < 30%時顯示）
5. **Damage Effect Prefab**: 拖入受傷特效預製體
   - 推薦使用: `Assets/GabrielAguiarProductions/FreeQuickEffectsVol1/Prefabs/vfx_Impact_01.prefab`
   - 或使用: `vfx_Explosion_01.prefab` (爆炸效果)
6. **Effect Points** (陣列大小: 4): 依序拖入4個特效點
   - Element 0: EffectPoint_1
   - Element 1: EffectPoint_2
   - Element 2: EffectPoint_3
   - Element 3: EffectPoint_4
7. **Attack Effect Prefab**: 拖入攻擊/反擊特效預製體
   - 推薦使用: `Assets/GabrielAguiarProductions/FreeQuickEffectsVol1/Prefabs/vfx_Projectile_01.prefab`
   - 或使用: `vfx_MuzzleFlash_01.prefab` (槍口火光效果)
   - 或使用: `vfx_Lightning_01.prefab` (閃電效果)
8. **Low Hp Threshold**: 設置低HP閾值（默認0.3 = 30%）

### 步驟 9: 建立關卡數據

1. 在Project視窗: `Assets/ScriptableObjects/Stages`
2. 右鍵 > `Create > Tenronis > Stage Data`
3. 建立10個關卡，命名為 `Stage_01` 到 `Stage_10`

**範例設定 - Stage_01:**
```
Stage Name: 偵察無人機
Stage Index: 0
Reward Buff Count: 1
Max Hp: 100
Shoot Interval: 2
Bullet Speed: 8
Can Use Add Block: false
Can Use Area Damage: false
Can Use Insert Row: false
Use Explosive Blocks: false
Use Void Row: false
Enemy Icon: [拖入敵人圖片Sprite]
Theme Color: 紅色
```

**範例設定 - Stage_10:**
```
Stage Name: 終焉機械神
Stage Index: 9
Reward Buff Count: 3
Max Hp: 2000
Shoot Interval: 0.8
Bullet Speed: 12
Can Use Add Block: true
Can Use Area Damage: true
Can Use Insert Row: true
Add Block Chance: 0.35
Area Damage Chance: 0.25
Insert Row Chance: 0.15
Use Explosive Blocks: true
Use Void Row: true
Enemy Icon: [拖入Boss圖片Sprite]
Theme Color: 紫色
```

> **重要**：敵人圖片會在關卡開始時自動顯示在 `EnemySprite` 上，無需手動設置！

### 步驟 10: 建立Buff數據

1. 在Project視窗: `Assets/ScriptableObjects/Buffs`
2. 右鍵 > `Create > Tenronis > Buff Data`
3. 建立9種Buff

**範例 - Defense Buff:**
```
Buff Name: 裝甲強化
Buff Type: Defense
Description: 增加方塊耐久度 +1
Spawn Weight: 1.0
```

建議建立的Buff：
- Defense (裝甲強化)
- Volley (齊射強化)
- Heal (緊急修復)
- Explosion (過載爆破)
- Salvo (協同火力)
- Burst (連發強化)
- Counter (反擊強化)
- SpaceExpansion (空間擴充)
- ResourceExpansion (資源擴充)

**注意**：Execution 和 Repair 已改為消耗CP的技能，不再作為Buff出現。

### 步驟 11: 設置GameManager

選擇 GameManager 物件：

1. **Stages**: 設置陣列大小為10，拖入所有關卡數據
2. **Normal Buffs** (普通強化): 設置陣列大小為6，拖入以下Buff：
   - Salvo (齊射強化)
   - Burst (連發強化)
   - Counter (反擊強化)
   - Explosion (過載爆破)
   - SpaceExpansion (空間擴充)
   - ResourceExpansion (資源擴充)
3. **Legendary Buffs** (傳奇強化): 設置陣列大小為3，拖入以下Buff：
   - Defense (裝甲強化)
   - Volley (協同火力)
   - Heal (緊急修復)

**傳奇強化選擇機制說明**：
- 當有普通強化達到滿級時，會自動提供傳奇強化選擇機會
- 傳奇強化選擇時，只從 Legendary Buffs 陣列中選擇
- 如果傳奇強化數量 ≤ 3，直接顯示全部（不隨機選擇）
- 如果傳奇強化數量 > 3，隨機選擇3個（根據權重）
- 不會過濾傳奇強化（除了null），保留所有內容（包括Execution和Repair，如果它們在陣列中）

### 步驟 12: 建立UI

1. 建立Canvas: `GameObject > UI > Canvas`
2. Canvas設置：
   - Render Mode: Screen Space - Overlay
   - Canvas Scaler: Scale With Screen Size
   - Reference Resolution: 1920 x 1080

#### 主選單面板
```
Canvas
└── MenuPanel (添加 VerticalLayoutGroup)
    ├── Title (TextMeshPro)
    └── StartButton (Button)
```

#### 遊戲中UI
```
Canvas
└── GameplayPanel
    ├── TopBar
    │   ├── ScoreText (TextMeshPro)
    │   └── StageText (TextMeshPro)
    ├── LeftPanel
    │   ├── PlayerHpSlider (Slider)
    │   ├── PlayerHpText (TextMeshPro)
    │   ├── PlayerCpSlider (Slider) ← 🏰 Castle Point 條
    │   └── PlayerCpText (TextMeshPro) ← 🏰 CP 數值
    ├── RightPanel
    │   ├── EnemyHpSlider (Slider)
    │   └── EnemyHpText (TextMeshPro)
    ├── SkillPanel
    │   └── ExplosionDamageText (TextMeshPro) ← 💣 爆炸充能顯示
    │   （注意：Execution 和 Repair 已改為消耗CP的技能，不再顯示次數）
    ├── ComboText (TextMeshPro)
    └── SalvoText (TextMeshPro)
```

**🏰 Castle Point (CP) 設置詳細步驟：**

1. **創建 CP Slider**：
   - 在 LeftPanel 下：`右鍵 > UI > Slider`
   - 命名為：`PlayerCpSlider`
   - 位置：放在 PlayerHpSlider 下方
   - 設置 Slider：
     - Fill Rect → Fill 的顏色：金色/黃色 (#FFD700) 或藍色 (#3B82F6)
     - Background → 深灰色半透明
     - Min Value: 0
     - Max Value: 100
     - Whole Numbers: 勾選（整數顯示）

2. **創建 CP Text**：
   - 在 LeftPanel 下：`右鍵 > UI > TextMeshPro - Text`
   - 命名為：`PlayerCpText`
   - 位置：放在 PlayerCpSlider 旁邊或下方
   - 設置文字：
     - Font Size: 18-24
     - Alignment: 居中
     - Color: 金色/黃色或白色
     - 範例文字：`"CP: 100 / 100"`

**推薦的 LeftPanel 佈局**：
```
LeftPanel (VerticalLayoutGroup 可選)
├── PlayerHpSlider (紅色條)
├── PlayerHpText (顯示 "100 / 100")
├── PlayerCpSlider (金色/藍色條) ← 新增
└── PlayerCpText (顯示 "CP: 100 / 100") ← 新增
```

#### 下一個方塊預覽 (Next Piece Preview)
```
Canvas
└── NextPiecePreview (添加 NextPiecePreview 腳本)
    ├── Background (Image - 可選，半透明背景)
    ├── Title (TextMeshPro - 顯示 "下一個")
    └── PreviewContainer (空物件 - 用於容納預覽方塊)
```

**設定步驟**：
1. 在 Canvas 下創建空物件：`NextPiecePreview`
2. 設置位置：右上角（例如：Anchor: Top-Right, Position X: -150, Y: -150）
3. 添加 `NextPiecePreview` 腳本
4. 創建子物件 `PreviewContainer`（RectTransform, Anchor: Center）
5. 在 Inspector 中設置：
   - Preview Container: 拖入 PreviewContainer
   - Block Size: 30
   - Spacing: 2
   - Use Sprite: false（使用純色方塊）

#### 儲存方塊 UI (Held Pieces - 按鍵 A、S、D、F)
```
Canvas
└── HeldPiecesPanel (添加 HeldPiecesUI 腳本)
    ├── Slot1 (空物件 - 儲存位置 A)
    │   ├── Background (Image - 可選)
    │   ├── KeyLabel (TextMeshPro - 顯示 "A")
    │   ├── LockIcon (Image 或 TextMeshPro - 鎖定圖示 🔒)
    │   └── Container (空物件 - 用於容納方塊預覽)
    ├── Slot2 (空物件 - 儲存位置 S)
    │   ├── Background (Image - 可選)
    │   ├── KeyLabel (TextMeshPro - 顯示 "S")
    │   ├── LockIcon (Image 或 TextMeshPro - 鎖定圖示 🔒)
    │   └── Container (空物件 - 用於容納方塊預覽)
    ├── Slot3 (空物件 - 儲存位置 D)
    │   ├── Background (Image - 可選)
    │   ├── KeyLabel (TextMeshPro - 顯示 "D")
    │   ├── LockIcon (Image 或 TextMeshPro - 鎖定圖示 🔒)
    │   └── Container (空物件 - 用於容納方塊預覽)
    └── Slot4 (空物件 - 儲存位置 F)
        ├── Background (Image - 可選)
        ├── KeyLabel (TextMeshPro - 顯示 "F")
        ├── LockIcon (Image 或 TextMeshPro - 鎖定圖示 🔒)
        └── Container (空物件 - 用於容納方塊預覽)
```

**設定步驟**：
1. 在 Canvas 下創建空物件：`HeldPiecesPanel`
2. 設置位置：左上角（例如：Anchor: Top-Left, Position X: 150, Y: -150）
3. 添加 `HorizontalLayoutGroup` 組件（可選，自動排列）
4. 為每個儲存位置創建：
   - 空物件 `Slot1` ~ `Slot4`
   - 每個 Slot 大小：Width: 100, Height: 120
   - 在每個 Slot 下創建：
     - `Background`（Image，半透明背景）
     - `KeyLabel`（TextMeshPro，顯示按鍵提示 A/S/D/F）
     - `LockIcon`（Image 或 TextMeshPro）
       - **使用 Image**：拖入鎖定圖示 Sprite（🔒）
       - **使用 TextMeshPro**：Text 設為 "🔒" 或 "LOCKED"
       - 位置：右上角（Anchor: Top-Right）
       - 顏色：半透明紅色或灰色
     - `Container`（空物件，Anchor: Center）
5. 在 `HeldPiecesPanel` 上添加 `HeldPiecesUI` 腳本
6. 在 Inspector 中設置：
   - Slot Containers (Size: 4): 拖入 Slot1/Container ~ Slot4/Container
   - Key Labels (Size: 4): 拖入 Slot1/KeyLabel ~ Slot4/KeyLabel
   - **Lock Icons (Size: 4)**: 拖入 Slot1/LockIcon ~ Slot4/LockIcon
   - Block Size: 25
   - Spacing: 2
   - Empty Slot Color: 灰色半透明 (0.3, 0.3, 0.3, 0.5)

#### 升級面板
```
Canvas
└── LevelUpPanel (添加 RoguelikeMenu 腳本)
    └── BuffOptionsContainer (HorizontalLayoutGroup)
```

#### 遊戲結束面板
```
Canvas
├── GameOverPanel
│   ├── TitleText
│   ├── FinalScoreText
│   └── RestartButton
└── VictoryPanel
    ├── TitleText
    ├── FinalScoreText
    └── MenuButton
```

### 步驟 13: 設置GameUI

選擇Canvas，添加 `GameUI` 腳本，連接所有UI引用：

- Menu Panel → MenuPanel物件
- Start Button → StartButton
- Gameplay Panel → GameplayPanel
- Score Text → ScoreText
- Combo Text → ComboText
- Player Hp Slider → PlayerHpSlider
- Player Hp Text → PlayerHpText
- **Player Cp Slider → PlayerCpSlider** ← 🏰 新增
- **Player Cp Text → PlayerCpText** ← 🏰 新增
- Enemy Hp Slider → EnemyHpSlider
- Enemy Hp Text → EnemyHpText
- Stage Text → StageText
- Execution Count Text → （已移除，改為消耗CP技能）
- Repair Count Text → （已移除，改為消耗CP技能）
- **Explosion Damage Text → ExplosionDamageText** ← 💣 新增（顯示爆炸充能）
- Salvo Text → SalvoText
- Level Up Panel → LevelUpPanel
- Game Over Panel → GameOverPanel
- Victory Panel → VictoryPanel
- Final Score Text → FinalScoreText
- Restart Button → RestartButton
- Menu Button → MenuButton

**🏰 Castle Point (CP) 系統說明**：
- CP 起始值：100
- 溢出時消耗：25 CP
- CP 不足時：HP 降至 1（瀕死狀態）
- 最多可承受：4 次溢出（100 / 25 = 4）
- **技能消耗**：
  - Execution（處決）：消耗 5 CP
  - Repair（修復）：消耗 30 CP
- **資源擴充Buff**：每次選擇CP上限+50，起始等級0，最高等級3（最多可提升至250）

**💣 爆炸充能系統說明**：
- **初始充能上限**：200
- **充能獲得方式**：
  - 反擊一次：+5充能
  - 消排一次：+50充能
- **Explosion Buff效果**：
  - 每次選擇充能上限+200
  - 起始等級1，最高等級4
  - 最多可達1000充能上限
- **溢出傷害**：
  - 溢出時對敵人造成當前充能值的傷害
  - 傷害後充能歸零

**💣 爆炸充能（Explosion Damage）UI 設置**：
1. **創建 ExplosionDamageText**：
   - 在 SkillPanel 下：`右鍵 > UI > TextMeshPro - Text`
   - 命名為：`ExplosionDamageText`
   - 位置：放在技能按鈕區域（與 Execution/Repair 並列）
   - 設置文字：
     - Font Size: 16-20
     - Alignment: 居中
     - Color: 橘紅色 (#FF6B35) 或金色 (#FFD700)
     - 範例文字：`"50"`
   
2. **工作原理**：
   - 始終顯示爆炸充能數值
   - 顯示格式：純數字（例如：`"0"`、`"50"`、`"100"`、`"150"`）
   - 初始值為 `0`（未獲得 Buff 時）
   - 每選一次「爆炸充能」Buff，數值 +50

### 步驟 14: 設置RoguelikeMenu

#### 升級面板結構：
```
LevelUpPanel (添加 RoguelikeMenu 腳本)
├── LegendaryBuffText (TextMeshPro) ← 📊 新增：顯示傳奇強化（裝甲強化、協同火力）
├── CurrentStatsText (TextMeshPro) ← 📊 顯示當前強化狀態（其他6個，每行3個）
└── BuffOptionsContainer (HorizontalLayoutGroup)
    └── （動態生成 BuffOption）
```

#### 設置步驟：

1. **建立 BuffOption 預製體**：
   ```
   BuffOption (添加 Button 組件)
   ├── Icon (Image)
   ├── Title (TextMeshPro)
   └── Description (TextMeshPro)
   ```
   - 拖曳到 `Assets/Prefabs/UI/`

2. **創建傳奇強化顯示（LegendaryBuffText）**：
   - 在 LevelUpPanel 下：`右鍵 > UI > TextMeshPro - Text`
   - 命名為：`LegendaryBuffText`
   - 位置：放在左側或上方
   - 設置：
     - Font Size: 16-18
     - Alignment: 左上對齊
     - Color: 金色或特殊顏色（區分傳奇強化）
     - Width: 300-400
     - Height: 100-150
     - 範例文字：
       ```
       【傳奇強化】
       裝甲強化: Lv.0 (+0 HP)
       協同火力: Lv.0 (0% 多行加成)
       ```

3. **創建當前狀態顯示（CurrentStatsText）**：
   - 在 LevelUpPanel 下：`右鍵 > UI > TextMeshPro - Text`
   - 命名為：`CurrentStatsText`
   - 位置：放在 LegendaryBuffText 下方或右側
   - 設置：
     - Font Size: 16-18
     - Alignment: 左上對齊
     - Color: 白色或淡藍色
     - Width: 400-600
     - Height: 400-600（自動調整）
     - 啟用「Vertical Overflow」→ Overflow
     - 範例文字（每行3個）：
       ```
       【當前強化狀態】
       
       ═══ 被動強化 ═══
       齊射強化: Lv.1/6  |  連發強化: Lv.1/6 (25% 連擊加成)  |  反擊強化: Lv.1/6 (1 反擊導彈)
       過載爆破: Lv.1/4 (充能: 0/200)  |  空間擴充: Lv.1/4 (1 槽位)  |  資源擴充: Lv.0/3 (CP: 100)
       ```

4. **連接 RoguelikeMenu 腳本**：
   - 選擇 LevelUpPanel 上的 RoguelikeMenu 腳本
   - 設置：
     - Buff Options Container → BuffOptionsContainer
     - Buff Option Prefab → BuffOption 預製體
     - **Legendary Buff Text → LegendaryBuffText** ← 📊 新增
     - **Current Stats Text → CurrentStatsText** ← 📊 新增

#### 功能說明：
- **LegendaryBuffText** 會自動顯示：
  - 裝甲強化（Defense）
  - 協同火力（Salvo）
  - 不論等級是否為0，一律顯示
- **CurrentStatsText** 會自動顯示：
  - 其他6個被動強化（每行3個）
  - 顯示格式：`強化名稱: Lv.當前/上限`
  - 不論等級是否為0，一律顯示
- 每次選擇 Buff 後，狀態會自動更新

#### 推薦的升級面板佈局：
```
LevelUpPanel (全螢幕半透明背景)
├── LeftPanel (當前狀態)
│   ├── LegendaryBuffText
│   │   - Position: 左上
│   │   - Width: 300-400px
│   │   - 顯示傳奇強化（裝甲強化、協同火力）
│   └── CurrentStatsText
│       - Position: 左側，LegendaryBuffText下方
│       - Width: 400-600px
│       - 顯示其他6個強化（每行3個）
├── RightPanel (選擇新增益)
│   ├── Title (TextMeshPro - "選擇一個強化")
│   └── BuffOptionsContainer
│       ├── BuffOption 1 (動態生成)
│       ├── BuffOption 2 (動態生成)
│       └── BuffOption 3 (動態生成)
```

**設計建議**：
- 左側顯示「你已經有什麼」
  - 上方：傳奇強化（特殊顯示）
  - 下方：其他強化（每行3個，整齊排列）
- 右側顯示「你可以選什麼」
- 讓玩家清楚看到強化的累積效果

### 步驟 15: 設置攝影機

選擇Main Camera：
- Position: (0, 0, -10)
- Size: 12 (Orthographic)
- Background: 深色 (#0F172A)

### 步驟 16: 測試遊戲

1. 按下Play
2. 應該看到主選單
3. 點擊開始按鈕
4. 方塊開始下落
5. 測試控制：
   - 左右鍵移動
   - 上鍵旋轉
   - 空白鍵硬降
   - 消除行觀察導彈發射

## 常見問題

### Q: 方塊無法顯示？
A: 檢查GridManager的Block Prefab是否正確連接

### Q: 導彈/子彈無法發射？
A: 檢查CombatManager的預製體引用

### Q: 遊戲無法開始？
A: 檢查GameManager是否有關卡數據

### Q: UI不顯示？
A: 檢查GameUI腳本的所有引用是否連接正確

### Q: 音效無法播放？
A: 需要準備音效檔案並在AudioManager中設置

## 進階優化

### 添加視覺特效
1. 安裝 Visual Effect Graph package
2. 建立爆炸粒子特效
3. 建立導彈軌跡特效

### 添加音效
1. 準備音效檔案（.wav 或 .mp3）
2. 放入 `Assets/Audio/` 資料夾
3. 在AudioManager中連接引用

### 優化方塊視覺
1. 建立方塊材質
2. 添加發光效果
3. 使用Shader實現HP顏色漸變

## 6. 視覺效果設置

### 6.1 ScreenShake（螢幕震動）

#### 添加到 Camera

1. 在 Hierarchy 選中 `Main Camera`
2. 在 Inspector 中點擊 `Add Component`
3. 搜尋 `ScreenShake` → 添加組件

#### 設置參數

在 Inspector 中配置：

```
ScreenShake (Script)
├── Shake Intensity: 0.3     ← 震動強度（推薦 0.3-0.5）
├── Shake Duration: 0.3      ← 震動持續時間（推薦 0.3-0.5 秒）
└── Shake Curve              ← 震動衰減曲線
```

**設置 Shake Curve（動畫曲線）：**

1. 點擊 `Shake Curve` 右側的曲線圖示
2. 預設曲線通常已經設置為 `EaseInOut(0,1,1,0)`
3. 如果需要自訂：
   - 左下角關鍵幀：`Time: 0, Value: 1`（震動開始，強度最大）
   - 右上角關鍵幀：`Time: 1, Value: 0`（震動結束，強度為 0）
   - 選擇曲線為 `EaseInOut` 讓震動平滑衰減

**效果觸發時機：**
- ✅ 方塊溢出時（`OnGridOverflow`）→ 強烈震動
- ✅ 玩家受傷時（`OnPlayerDamaged`）→ 輕微震動

**測試震動效果：**
1. 啟動遊戲
2. 讓方塊堆到頂部溢出 → 應該看到明顯震動
3. 被敵人子彈擊中 → 應該看到輕微震動

#### 進階調整

**不同場景的震動強度：**

| 場景 | Shake Intensity | Shake Duration | 效果 |
|------|----------------|----------------|------|
| 輕微震動 | 0.1 - 0.15 | 0.15 - 0.2 | 受傷時的輕微回饋 |
| 一般震動 | 0.3 - 0.4 | 0.3 - 0.4 | 方塊溢出、消除 |
| 強烈震動 | 0.5 - 0.8 | 0.5 - 0.7 | Boss 攻擊、大量消除 |

**如果震動太強：**
- 降低 `Shake Intensity` 至 `0.2` 或更低
- 縮短 `Shake Duration` 至 `0.2` 秒

**如果震動太弱：**
- 提高 `Shake Intensity` 至 `0.5` 或更高
- 延長 `Shake Duration` 至 `0.5` 秒

---

## 完成！

現在你有一個完整的Tenronis遊戲了！

建議下一步：
- 調整遊戲平衡
- 添加更多視覺特效
- 建立更多關卡
- 添加音效和音樂

