# Tenronis - 俄羅斯加農砲

Unity 6 實作的俄羅斯方塊 + 射擊遊戲混合體

## 專案架構

### 📁 資料夾結構

```
Assets/
├── Scripts/
│   ├── Core/               # 核心系統（事件、初始化）
│   ├── Data/               # 資料定義（Enums, Constants, Data Classes）
│   ├── Managers/           # 管理器（Game, Grid, Player, Combat, Input）
│   ├── Gameplay/
│   │   ├── Block/          # 方塊組件
│   │   ├── Tetromino/      # 俄羅斯方塊控制
│   │   ├── Projectiles/    # 導彈、子彈
│   │   ├── Enemy/          # 敵人控制器
│   │   └── Player/         # 玩家技能
│   ├── UI/                 # 使用者介面
│   ├── Audio/              # 音效管理
│   ├── VFX/                # 視覺特效
│   └── Utilities/          # 工具類（對象池）
├── ScriptableObjects/
│   ├── Stages/             # 關卡數據
│   └── Buffs/              # Buff數據
├── Prefabs/                # 預製體
├── Materials/              # 材質
├── Sprites/                # 精靈圖
└── Audio/                  # 音效資源

```

## 核心系統

### 🎮 GameManager
- 單例模式遊戲主管理器
- 管理遊戲狀態（Menu, Playing, LevelUp, GameOver, Victory）
- 處理關卡進度和Roguelike系統
- 支援多主題系統，每個主題包含三種難度軌道（Casual, Standard, Expert）

### 📐 GridManager
- 管理遊戲網格（10x20）
- 方塊放置、消除、碰撞檢測
- 網格座標轉換

### 👤 PlayerManager
- 管理玩家數據（HP, Score, Upgrades）
- Combo系統
- 技能管理

### ⚔️ CombatManager
- 導彈和子彈的對象池管理
- 碰撞檢測
- 反擊系統

### 🎵 AudioManager
- 音效和音樂管理
- BGM切換（Normal / Boss）

### 🎯 TetrominoController
- 俄羅斯方塊控制
- 移動、旋轉、硬降
- 自動下落

### 👹 EnemyController
- 敵人AI
- 子彈發射
- 關卡數據讀取

## 遊戲機制

### 方塊系統
- 7種經典俄羅斯方塊形狀（I, J, L, O, S, T, Z）
- 方塊HP系統（可升級防禦）
- 3種方塊類型（Normal, Void, Explosive）
  - Void方塊：不可被子彈摧毀，但可通過消行清除

### 戰鬥系統
- **消除行 → 發射導彈**
- **齊射系統**：多行消除增加傷害
- **連發系統**：連續消除增加Combo和傷害
- **反擊系統**：新放置方塊被擊中時反擊

### 敵人系統
- 支援多主題系統，每個主題包含多個關卡
- 三種難度軌道（Casual, Standard, Expert）
- 8種子彈類型：
  - Normal（普通子彈）
  - AreaDamage（範圍傷害子彈）
  - AddBlock（添加普通方塊）
  - AddExplosiveBlock（添加爆炸方塊）
  - InsertRow（插入普通垃圾行）
  - InsertVoidRow（插入虛無垃圾行）
  - CorruptExplosive（腐化：爆炸）
  - CorruptVoid（腐化：虛無）

### Roguelike升級
- 關卡完成後獲得升級點數（由關卡的rewardBuffCount決定）
- 12種Buff類型（6種普通強化 + 4種傳奇強化 + 2種技能）：

**普通強化（有上限等級）**：
  1. **齊射強化（Salvo）**：起始1，上限6，增加多排消除時的子彈傷害
  2. **連發強化（Burst）**：起始1，上限6，Combo加成
  3. **反擊強化（Counter）**：起始1，上限6，新方塊被擊中時反擊
  4. **過載爆破（Explosion）**：起始1，上限4，增加充能上限（每次+200，最多1000）
  5. **空間擴充（SpaceExpansion）**：起始1，上限4，解鎖儲存槽位
  6. **資源擴充（ResourceExpansion）**：起始0，上限3，增加CP上限（每次+50，最多250）

**傳奇強化（無上限或特殊效果）**：
  1. **裝甲強化（Defense）**：起始0，無上限，增加方塊HP（+1 HP/等級）
  2. **協同火力（Volley）**：起始0，無上限，每個位置發射更多導彈（+1導彈/等級）
  3. **戰術擴展（TacticalExpansion）**：起始0，上限2，解鎖技能（Lv1解鎖處決，Lv2解鎖修補）
  4. **緊急修復（Heal）**：立即效果，恢復50% HP

**技能（需通過戰術擴展解鎖）**：
  1. **處決（Execution）**：消耗5 CP，清除每列底部方塊
  2. **修補（Repair）**：消耗30 CP，填補封閉空洞

### 爆炸充能系統
- **初始上限**：200
- **充能獲得**：反擊+5，消排+50
- **溢出傷害**：溢出時造成當前充能值傷害，然後歸零

### 技能系統（消耗CP）
- **處決技能**：消耗5 CP，清除每列底部方塊
- **修復技能**：消耗30 CP，填補封閉空洞

## 設置步驟

### 1. 建立場景物件

在場景中建立以下物件：

```
Scene
├── GameInitializer (空物件 + GameInitializer腳本)
├── GameManager (空物件 + GameManager腳本)
├── GridManager (空物件 + GridManager腳本)
├── PlayerManager (空物件 + PlayerManager腳本)
├── CombatManager (空物件 + CombatManager腳本)
├── AudioManager (空物件 + AudioManager腳本)
├── InputManager (空物件 + InputManager腳本)
├── TetrominoController (空物件 + TetrominoController腳本)
├── EnemyController (空物件 + EnemyController腳本)
├── Canvas (UI)
│   ├── GameUI (GameUI腳本)
│   ├── RoguelikeMenu (RoguelikeMenu腳本)
│   └── PopupTextManager (PopupTextManager腳本)
└── Main Camera
```

### 2. 建立預製體

#### Block Prefab
- Sprite Renderer（方塊視覺）
- Block腳本

#### Missile Prefab
- Sprite Renderer（青色）
- Trail Renderer（軌跡）
- Missile腳本

#### Bullet Prefab
- Sprite Renderer（紅色）
- Bullet腳本

### 3. 建立ScriptableObjects

#### 主題數據（StageSetSO）
在`Assets/ScriptableObjects/StageSets/`建立主題：

```
Theme 1: 深淵
- Easy Stages: 10個關卡（Casual難度）
- Normal Stages: 10個關卡（Standard難度）
- Hard Stages: 10個關卡（Expert難度）
```

#### 關卡數據（StageDataSO）
在`Assets/ScriptableObjects/Stages/`建立關卡數據：

```
範例：Theme1_Stage1_Easy
- Stage Name: 深淵窺視者
- Difficulty Track: Casual
- Auto Balance: true
- Player PDA: 7
- Player SP: 0.7
- Max Hp: 245
- Shoot Interval: 2.9s
- Bullet Speed: 6
```

#### Buff數據範例
在`Assets/ScriptableObjects/Buffs/`建立10種BuffData（6種普通強化 + 4種傳奇強化）

### 4. 連接引用

在各管理器上設置引用：
- **GameManager**: 
  - All Themes（主題列表，StageSetSO）
  - Normal Buffs（普通強化陣列）
  - Legendary Buffs（傳奇強化陣列）
- **GridManager**: Block預製體、Grid容器
- **CombatManager**: Missile預製體、Bullet預製體
- **AudioManager**: 音效Clips、BGM Clips

## 控制

### 鍵盤
- **← / →**: 左右移動
- **↑**: 旋轉
- **↓**: 加速下落
- **Space**: 硬降
- **1**: 使用處決技能
- **2**: 使用修復技能

## 最佳實踐要點

### Unity 6 API使用
- 使用 `FindFirstObjectByType<T>()` 取代舊的 `FindObjectOfType`
- 使用現代化的組件架構

### 架構模式
- **單例模式**：管理器使用單例確保唯一性
- **事件系統**：透過GameEvents解耦各系統
- **對象池**：導彈和子彈使用對象池優化性能
- **ScriptableObjects**：關卡和Buff數據使用SO配置

### 性能優化
- 使用對象池減少實例化開銷
- 視覺更新使用髒標記
- 碰撞檢測優化（空間劃分）

## 擴展建議

### 視覺效果
- 添加粒子特效（爆炸、導彈軌跡）
- 屏幕震動效果（已有triggerShake系統）
- 方塊破碎動畫

### 音效
- 添加音效資源
- 實作音效淡入淡出

### UI增強
- 添加方塊儲存槽UI（A/S/D/F）
- 升級統計面板
- 下一個方塊預覽

## 授權

此專案基於原React版本改寫為Unity版本，遵循Unity最佳實踐。

