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

### 步驟 9A: 建立主題數據（StageSetSO）

1. 在Project視窗: `Assets/ScriptableObjects/StageSets`
2. 右鍵 > `Create > Tenronis > Stage Set (Theme)`
3. 命名為 `Theme_1_Abyss`（或你想要的主題名稱）
4. 設置主題資訊：
   - **Theme Name**: 深淵主題
   - **Theme Icon**: 拖入主題圖示（可選）
   - **Theme Color**: 選擇代表顏色（例如深藍色）
   - **Description**: 主題描述文字

**注意**：暫時不要設置關卡列表，我們將在步驟9B建立關卡後再拖入

### 步驟 9B: 建立關卡數據（三軌難度）

**專案現況**：已建立10個主題，每主題3種難度×10關，共300個關卡配置。關卡文件命名格式：`主題編號_關卡編號_難度.asset`（例如：`1_1_Easy.asset`、`1_1_Normal.asset`、`1_1_Hard.asset`）

如需新增主題或關卡：

1. 在Project視窗: `Assets/ScriptableObjects/Stages`
2. 右鍵 > `Create > Tenronis > Stage Data`
3. 為新主題建立30個關卡（例如主題11）：
   - **11_1_Easy ~ 11_10_Easy**（Casual 軌道）
   - **11_1_Normal ~ 11_10_Normal**（Standard 軌道）
   - **11_1_Hard ~ 11_10_Hard**（Expert 軌道）

**範例設定 - Easy1（Casual 軌道）:**
```
Stage Name: 深淵窺視者
Stage Index: 1
Is Boss Stage: false
Difficulty Track: Casual
Auto Balance: true ← 啟用自動平衡
Player PDA: 7
Player SP: 0.7
Reward Buff Count: 1
Max Hp: 245
Shoot Interval: 2.9
Bullet Speed: 6
Burst Count: 1
[技能設置會由 Auto Balance 自動計算]
Enemy Icon: [拖入敵人圖片Sprite]
Theme Color: 淡藍色
```

**範例設定 - Normal1（Standard 軌道）:**
```
Stage Name: 深淵全能者
Stage Index: 1
Is Boss Stage: false
Difficulty Track: Standard
Auto Balance: true
Player PDA: 7
Player SP: 0.6
Reward Buff Count: 1
Max Hp: 175
Shoot Interval: 1.9
Bullet Speed: 8
Burst Count: 1
Enemy Icon: [拖入敵人圖片Sprite]
Theme Color: 藍色
```

**範例設定 - Hard10（Expert 軌道）:**
```
Stage Name: 深淵主宰
Stage Index: 10
Is Boss Stage: true
Difficulty Track: Expert
Auto Balance: true
Player PDA: 3500
Player SP: 0.2
Reward Buff Count: 2
Max Hp: 70000
Shoot Interval: 0.7
Bullet Speed: 10
Burst Count: 5
Use Smart Targeting: true ← Expert 模式啟用
Enemy Icon: [拖入Boss圖片Sprite]
Theme Color: 深紅色
```

> **重要**：
> - 敵人圖片會在關卡開始時自動顯示在 `EnemySprite` 上，無需手動設置！
> - 啟用 `Auto Balance` 後，數值會根據 PDA 和 SP 自動計算
> - 詳細數值規格請參考 `Assets/Documentation/Math/11_Difficulty_Tracks_Model.md`

### 步驟 9C: 連接關卡到主題

1. 選擇剛建立的 `Theme_1_Abyss`
2. 在 Inspector 中：
   - **Easy Stages**（Casual 軌道）: 設置陣列大小為10，拖入 Theme1_Easy1 ~ Easy10
   - **Normal Stages**（Standard 軌道）: 設置陣列大小為10，拖入 Theme1_Normal1 ~ Normal10
   - **Hard Stages**（Expert 軌道）: 設置陣列大小為10，拖入 Theme1_Hard1 ~ Hard10

**提示**：
- 可以建立多個主題（Theme_2_Void、Theme_3_Fire等）
- 每個主題都需要有自己的關卡集合
- 主題系統讓遊戲內容更豐富，可以逐步擴展

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

建議建立的Buff（共12種）：

**傳奇強化（4種）**：
- Defense (裝甲強化，起始0，無上限，+1 HP/等級)
- Volley (協同火力，起始0，無上限，每個位置+1導彈/等級)
- TacticalExpansion (戰術擴展，起始0，上限2，解鎖技能)
- Heal (緊急修復，立即效果，恢復50% HP)

**普通強化（6種）**：
- Salvo (齊射強化，起始1，上限6)
- Burst (連發強化，起始1，上限6)
- Counter (反擊強化，起始1，上限6)
- Explosion (過載爆破，起始1，上限4)
- SpaceExpansion (空間擴充，起始1，上限4)
- ResourceExpansion (資源擴充，起始0，上限3)

**技能（2種，通過TacticalExpansion解鎖）**：
- Execution (處決技能，消耗5 CP，清除每列底部方塊)
- Repair (修補技能，消耗30 CP，填補封閉空洞)

**注意**：
- Execution和Repair不是獨立的Buff，而是通過TacticalExpansion解鎖的技能
- 這兩個技能在升級選單中不會出現，只能通過TacticalExpansion解鎖使用

### 步驟 11: 設置GameManager

選擇 GameManager 物件：

1. **主題列表（All Themes）**：
   - 設置陣列大小為1（或你建立的主題數量）
   - 拖入 Theme_1_Abyss（和其他主題，如果有）
   - 主題順序決定UI顯示順序

2. **Normal Buffs** (普通強化): 設置陣列大小為6，拖入以下Buff：
   - Salvo (齊射強化)
   - Burst (連發強化)
   - Counter (反擊強化)
   - Explosion (過載爆破)
   - SpaceExpansion (空間擴充)
   - ResourceExpansion (資源擴充)

3. **Legendary Buffs** (傳奇強化): 設置陣列大小為4，拖入以下Buff：
   - Defense (裝甲強化)
   - Volley (協同火力)
   - TacticalExpansion (戰術擴展)
   - Heal (緊急修復)

**主題系統說明**：
- 玩家先選擇主題，再選擇難度
- 每個主題包含三種難度軌道（Casual, Standard, Expert）
- 支援多個主題，提供更豐富的遊戲內容
- UI會根據 All Themes 列表自動生成主題選擇按鈕

**三軌難度系統說明**：
- **Casual（休閒）**: 35秒目標擊殺時間，較慢的子彈速度（6格/秒）
- **Standard（標準）**: 25秒目標擊殺時間，中等子彈速度（8格/秒）
- **Expert（專家）**: 20秒目標擊殺時間，快速子彈（10格/秒），啟用智能瞄準

**傳奇強化選擇機制說明**：
- 當有普通強化達到滿級時，會自動提供傳奇強化選擇機會
- 傳奇強化選擇時，只從 Legendary Buffs 陣列中選擇
- 如果傳奇強化數量 ≤ 3，直接顯示全部（不隨機選擇）
- 如果傳奇強化數量 > 3，隨機選擇3個（根據權重）
- 不會過濾傳奇強化（除了null），保留所有內容

### 步驟 12: 建立UI

1. 建立Canvas: `GameObject > UI > Canvas`
2. Canvas設置：
   - Render Mode: Screen Space - Overlay
   - Canvas Scaler: Scale With Screen Size
   - Reference Resolution: 1920 x 1080

#### 主選單面板（主題選擇系統）
```
Canvas
└── MenuPanel
    ├── ThemeListPanel (主題選擇面板)
    │   ├── Title (TextMeshPro - "選擇主題")
    │   └── ThemeButtonContainer (ScrollView > Content，存放動態生成的主題按鈕)
    └── DifficultySelectPanel (難度選擇面板，初始隱藏)
        ├── SelectedThemeTitle (TextMeshPro - 顯示選中的主題名稱)
        ├── EasyButton (Button - "簡單模式 (Casual)")
        ├── NormalButton (Button - "標準模式 (Standard)")
        ├── HardButton (Button - "專家模式 (Expert)")
        └── BackToThemeButton (Button - "返回")
```

**Theme Button Prefab 設置**：
1. 在 Hierarchy 創建 Button
2. 設置為 Prefab（拖入 Assets/Prefabs/UI/）
3. 添加 TextMeshProUGUI 子物件顯示主題名稱
4. 調整大小和樣式

**UI流程**：
1. 遊戲啟動 → 顯示 ThemeListPanel（主題選擇）
2. GameUI 根據 GameManager.allThemes 動態生成主題按鈕
3. 玩家點擊主題 → 隱藏 ThemeListPanel，顯示 DifficultySelectPanel
4. 玩家點擊難度 → 呼叫 `GameManager.StartGame(themeIndex, difficulty)`
5. 點擊返回 → 返回 ThemeListPanel

**難度按鈕設置**：
- EasyButton: 呼叫 `GameManager.StartGame(selectedThemeIndex, DifficultyTrack.Casual)`
- NormalButton: 呼叫 `GameManager.StartGame(selectedThemeIndex, DifficultyTrack.Standard)`
- HardButton: 呼叫 `GameManager.StartGame(selectedThemeIndex, DifficultyTrack.Expert)`

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
- **Easy Button → EasyButton** ← 🎮 新增
- **Normal Button → NormalButton** ← 🎮 新增
- **Hard Button → HardButton** ← 🎮 新增
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



# Tenronis Setup Guide

## 1. 關卡與主題設置 (Themes & Stages)

### 1.1 建立關卡數據 (StageDataSO)
1. 在 Project 視窗中，右鍵點擊 `Create -> Tenronis -> Stage Data`。
2. 命名為 `T_S_Difficulty` (例如 `1_1_Easy`)。
3. 設定關卡參數：
   - **Stage Name**: 顯示名稱 (例如 "Theme 1 - Stage 1")
   - **Difficulty Track**: Casual / Standard / Expert
   - **Bullet Config**: 設定子彈生成機率

### 1.2 建立主題套組 (StageSetSO)
1. 在 Project 視窗中，右鍵點擊 `Create -> Tenronis -> Stage Set (Theme)`。
2. 命名為 `Theme_X` (例如 `Theme_1`)。
3. 在 Inspector 中設定：
   - **Theme Name**: 主題名稱 (例如 "Basic Shooter")
   - **Easy Stages**: 拖入該主題的 5 個 Easy 關卡
   - **Normal Stages**: 拖入該主題的 5 個 Normal 關卡
   - **Hard Stages**: 拖入該主題的 5 個 Hard 關卡

### 1.3 註冊主題到 GameManager
1. 選擇場景中的 `GameManager` 物件。
2. 找到 `All Themes` 列表。
3. 將建立好的 `StageSetSO` 拖入列表中。
4. 列表順序決定了 UI 顯示順序。

## 2. UI 設置流程

### 2.1 設置 GameUI
1. 確保場景中有 `GameUI` 物件。
2. 檢查 `GameUI` Inspector 中的參考：
   - **Menu Panel**: 主選單容器
   - **Theme List Panel**: 主題選擇頁面 (需包含 ScrollView 或 Grid)
   - **Difficulty Select Panel**: 難度選擇頁面
   - **Theme Button Prefab**: 用於生成主題按鈕的 Prefab
   - **Theme Button Container**: 主題按鈕的父物件 (Content)

### 2.2 設置按鈕事件
- **Theme Button Prefab**: 需包含 `Button` 組件和 `TextMeshProUGUI` 子物件。
- **Difficulty Buttons**: 在 `Difficulty Select Panel` 中，分別對應 `Easy`, `Normal`, `Hard` 按鈕。

## 3. 擴充指南

### 3.1 新增主題 (Theme 11+)
1. 依照 1.2 步驟建立新的 `StageSetSO`。
2. 依照 1.3 步驟將其加入 `GameManager` 的 `All Themes` 列表。
3. UI 會自動根據列表長度生成對應按鈕。

### 3.2 新增難度 (Expert+)
1. 修改 `DifficultyTrack` enum (在 `StageDataSO.cs`)。
2. 修改 `StageSetSO.cs` 增加對應的 List。
3. 修改 `GameManager.StartGame` 邏輯。
4. 修改 `GameUI` 增加對應按鈕。

## 4. 遊戲流程圖

```mermaid
graph TD
    Start[啟動遊戲] --> Menu[主選單 (Theme Selection)]
    Menu -->|選擇主題| Difficulty[難度選擇]
    Difficulty -->|Back| Menu
    Difficulty -->|選擇難度| Playing[遊戲進行中]
    Playing -->|通關| LevelUp[升級選單]
    LevelUp -->|選擇Buff| Playing
    Playing -->|失敗| GameOver[遊戲結束]
    Playing -->|全部通關| Victory[勝利]
    GameOver -->|Restart| Playing
    GameOver -->|Menu| Menu
    Victory -->|Menu| Menu
```
