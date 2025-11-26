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
- Volley (多重齊射)
- Heal (緊急修復)
- Explosion (過載爆破)
- Salvo (協同火力)
- Burst (連發加速器)
- Counter (反擊系統)
- Execution (戰術處決)
- Repair (結構修復)

### 步驟 11: 設置GameManager

選擇 GameManager 物件：

1. **Stages**: 設置陣列大小為10，拖入所有關卡數據
2. **Available Buffs**: 設置陣列大小為9，拖入所有Buff數據

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
    │   └── PlayerHpText (TextMeshPro)
    ├── RightPanel
    │   ├── EnemyHpSlider (Slider)
    │   └── EnemyHpText (TextMeshPro)
    ├── SkillPanel
    │   ├── ExecutionButton (顯示次數)
    │   └── RepairButton (顯示次數)
    ├── ComboText (TextMeshPro)
    └── SalvoText (TextMeshPro)
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
- Enemy Hp Slider → EnemyHpSlider
- Enemy Hp Text → EnemyHpText
- Stage Text → StageText
- Execution Count Text → ExecutionCountText
- Repair Count Text → RepairCountText
- Level Up Panel → LevelUpPanel
- Game Over Panel → GameOverPanel
- Victory Panel → VictoryPanel
- Final Score Text → FinalScoreText
- Restart Button → RestartButton
- Menu Button → MenuButton

### 步驟 14: 設置RoguelikeMenu

1. 建立BuffOption預製體：
   ```
   BuffOption (添加 Button 組件)
   ├── Icon (Image)
   ├── Title (TextMeshPro)
   └── Description (TextMeshPro)
   ```
2. 拖曳到 `Assets/Prefabs/UI/`
3. 選擇LevelUpPanel上的RoguelikeMenu腳本
4. 設置：
   - Buff Options Container → BuffOptionsContainer
   - Buff Option Prefab → BuffOption預製體

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

