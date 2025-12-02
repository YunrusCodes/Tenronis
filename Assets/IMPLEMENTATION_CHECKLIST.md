# Tenronis Unity 實作檢查清單

## ✅ 已完成項目

### 核心系統
- [x] 遊戲狀態管理 (GameManager)
- [x] 事件系統 (GameEvents)
- [x] 遊戲初始化器 (GameInitializer)

### 資料系統
- [x] 遊戲枚舉定義 (GameEnums)
- [x] 遊戲常數配置 (GameConstants)
- [x] 方塊資料類別 (BlockData)
- [x] 玩家數據類別 (PlayerStats)
- [x] 俄羅斯方塊定義 (TetrominoDefinitions)
- [x] 關卡ScriptableObject (StageDataSO)
- [x] BuffScriptableObject (BuffDataSO)

### 管理器系統
- [x] 遊戲管理器 (GameManager)
- [x] 網格管理器 (GridManager)
- [x] 玩家管理器 (PlayerManager)
- [x] 戰鬥管理器 (CombatManager)
- [x] 音效管理器 (AudioManager)
- [x] 輸入管理器 (InputManager)

### 遊戲玩法
- [x] 方塊組件 (Block)
- [x] 俄羅斯方塊控制器 (TetrominoController)
- [x] 導彈組件 (Missile)
- [x] 子彈組件 (Bullet)
- [x] 敵人控制器 (EnemyController)
- [x] 技能執行器 (SkillExecutor)

### UI系統
- [x] 主遊戲UI (GameUI)
- [x] Roguelike選單 (RoguelikeMenu)
- [x] 彈出文字 (PopupText)
- [x] 彈出文字管理器 (PopupTextManager)

### 工具類
- [x] 泛型對象池 (ObjectPool)

### 文檔
- [x] README.md (專案說明)
- [x] SETUP_GUIDE.md (設置指南)
- [x] PROJECT_SUMMARY.md (專案總結)
- [x] IMPLEMENTATION_CHECKLIST.md (本檔案)

## 🔧 需要Unity編輯器中完成的工作

### 場景設置
- [ ] 建立MainGame場景
- [ ] 放置所有管理器物件
- [ ] 放置遊戲控制器物件
- [ ] 設置相機位置和大小
- [ ] 建立Grid容器
- [ ] 建立Projectiles容器

### 預製體建立
- [ ] Block預製體（Sprite + Block腳本）
- [ ] Missile預製體（Sprite + Trail + Missile腳本）
- [ ] Bullet預製體（Sprite + Bullet腳本）
- [ ] BuffOption預製體（Button + Icon + Texts）

### ScriptableObject資產

- [ ] 建立主題數據（StageSetSO）
  - [ ] 至少建立1個主題（例如：深淵主題）
  - [ ] 每個主題需要包含三種難度軌道的關卡列表

- [ ] 建立關卡數據（StageDataSO）
  - [ ] 為每個主題的每個難度建立關卡
  - [ ] 範例：Theme1 包含 Easy1~10, Normal1~10, Hard1~10（共30個關卡）
  - [ ] 使用 Auto Balance 功能根據 PDA 和 SP 自動計算敵人屬性
  - [ ] 設置子彈類型配置（8種子彈類型）
  - [ ] Boss關卡設置（isBossStage = true）

- [ ] 建立12個BuffData（6普通 + 4傳奇 + 2技能）

**傳奇強化（4種）**：
  - [ ] Defense Buff（裝甲強化，起始0，無上限，+1 HP/等級）
  - [ ] Volley Buff（協同火力，起始0，無上限，每個位置+1導彈/等級）
  - [ ] TacticalExpansion Buff（戰術擴展，起始0，上限2，解鎖技能）
  - [ ] Heal Buff（緊急修復，立即效果，恢復50% HP）

**普通強化（6種）**：
  - [ ] Salvo Buff（齊射強化，起始1，上限6，多行消除時增加導彈傷害）
  - [ ] Burst Buff（連發強化，起始1，上限6，Combo加成）
  - [ ] Counter Buff（反擊強化，起始1，上限6，新方塊被擊中時反擊）
  - [ ] Explosion Buff（過載爆破，起始1，上限4，增加充能上限）
  - [ ] SpaceExpansion Buff（空間擴充，起始1，上限4，解鎖儲存槽位）
  - [ ] ResourceExpansion Buff（資源擴充，起始0，上限3，增加CP上限）

**技能（2種，通過TacticalExpansion解鎖）**：
  - [ ] Execution（處決技能，消耗5 CP，清除每列底部方塊）
  - [ ] Repair（修補技能，消耗30 CP，填補封閉空洞）

**注意**：
- Execution和Repair不是獨立的Buff，而是通過TacticalExpansion解鎖的技能
- 技能在GameEnums中定義為BuffType，但在實際升級選單中不會出現，只能通過TacticalExpansion解鎖

### 管理器連接
- [ ] GameManager
  - [ ] 連接All Themes列表（StageSetSO）
  - [ ] 連接Normal Buffs陣列（6個普通強化）
  - [ ] 連接Legendary Buffs陣列（4個傳奇強化）
- [ ] GridManager
  - [ ] 連接Block Prefab
  - [ ] 連接Grid Container
  - [ ] 設置Block Size
  - [ ] 設置Grid Offset
- [ ] CombatManager
  - [ ] 連接Missile Prefab
  - [ ] 連接Bullet Prefab
  - [ ] 連接Projectile Container
- [ ] AudioManager
  - [ ] 連接音效Clips（待添加音效資源）
  - [ ] 連接BGM Clips（待添加音樂資源）

### UI建立與連接
- [ ] 建立Canvas
- [ ] 建立MenuPanel
  - [ ] ThemeListPanel（主題選擇面板）
    - [ ] Title Text（選擇主題）
    - [ ] ThemeButtonContainer（存放動態生成的主題按鈕）
  - [ ] DifficultySelectPanel（難度選擇面板）
    - [ ] SelectedThemeTitle（顯示選中的主題名稱）
    - [ ] EasyButton（簡單模式 - Casual）
    - [ ] NormalButton（標準模式 - Standard）
    - [ ] HardButton（專家模式 - Expert）
    - [ ] BackToThemeButton（返回按鈕）
- [ ] 建立Theme Button Prefab（用於動態生成主題按鈕）
- [ ] 建立GameplayPanel
  - [ ] Score Text
  - [ ] Stage Text
  - [ ] Combo Text
  - [ ] Player HP Slider + Text
  - [ ] Player CP Slider + Text
  - [ ] Enemy HP Slider + Text
  - [ ] Explosion Damage Text
  - [ ] **注意**：Execution 和 Repair 已改為消耗CP的技能，需要通過TacticalExpansion解鎖
- [ ] 建立LevelUpPanel
  - [ ] Buff Options Container
  - [ ] LegendaryBuffText（顯示傳奇強化）
  - [ ] CurrentStatsText（顯示當前強化狀態）
- [ ] 建立GameOverPanel
  - [ ] Title Text
  - [ ] Final Score Text
  - [ ] Restart Button
- [ ] 建立VictoryPanel
  - [ ] Title Text
  - [ ] Final Score Text
  - [ ] Menu Button
- [ ] 連接GameUI腳本所有引用
  - [ ] Theme Button Prefab
  - [ ] Theme Button Container
  - [ ] Easy/Normal/Hard Buttons
  - [ ] Back To Theme Button
  - [ ] Selected Theme Title
- [ ] 連接RoguelikeMenu腳本引用

## 📦 可選資源（增強體驗）

### 美術資源
- [ ] 方塊精靈圖（不同顏色）
- [ ] 導彈精靈圖
- [ ] 子彈精靈圖（4種類型各異）
- [ ] 敵人圖示（10種）
- [ ] UI背景圖
- [ ] 按鈕精靈
- [ ] Buff圖示

### 音效資源
- [ ] 導彈發射音效
- [ ] 爆炸音效
- [ ] 方塊旋轉音效
- [ ] 撞擊音效
- [ ] 敵人射擊音效
- [ ] 敵人受擊音效
- [ ] Buff選擇音效
- [ ] UI點擊音效

### 音樂資源
- [ ] 普通戰鬥BGM
- [ ] Boss戰BGM

### 視覺特效
- [ ] 爆炸粒子特效
- [ ] 導彈軌跡特效
- [ ] 方塊破碎特效
- [ ] 擊中閃光特效
- [ ] 螢幕震動效果

### Shader和材質
- [ ] 方塊發光材質
- [ ] HP漸變Shader
- [ ] 背景網格材質
- [ ] UI模糊背景

## 🚀 測試階段

### 基礎功能測試
- [ ] 遊戲能正常啟動到主選單
- [ ] 點擊開始遊戲進入Playing狀態
- [ ] 方塊正常生成和下落
- [ ] 左右移動正常
- [ ] 旋轉正常
- [ ] 硬降正常
- [ ] 消除行檢測正常
- [ ] 導彈發射正常
- [ ] 導彈擊中敵人扣血
- [ ] 敵人射擊正常
- [ ] 子彈擊中方塊扣血
- [ ] 方塊被摧毀後移除
- [ ] 溢出懲罰正常觸發
- [ ] 玩家HP歸零觸發GameOver
- [ ] 敵人HP歸零觸發下一關

### 進階功能測試
- [ ] Combo系統正常累積
- [ ] Combo重置計時器正常
- [ ] 反擊系統正常觸發
- [ ] 齊射傷害加成正確
- [ ] 連發傷害加成正確
- [ ] Roguelike點數正確累積
- [ ] 升級選單正常彈出
- [ ] Buff選擇生效
- [ ] 處決技能正常使用（消耗5 CP）
- [ ] 修復技能正常使用（消耗30 CP，BFS填補空洞）
- [ ] CP系統正常運作（技能消耗、UI更新）
- [ ] 資源擴充Buff正常運作（CP上限增加）
- [ ] 爆炸充能系統正常運作（反擊+5，消排+50，溢出造成傷害）
- [ ] 強化等級限制正常運作（有上限的Buff達到上限後不再增加）
- [ ] UI顯示正常（每行3個強化，傳奇強化單獨顯示）
- [ ] InsertRow（插入普通垃圾行）正常
- [ ] InsertVoidRow（插入虛無垃圾行）正常
- [ ] 8種子彈類型效果正確（Normal, AreaDamage, AddBlock, AddExplosiveBlock, InsertRow, InsertVoidRow, CorruptExplosive, CorruptVoid）

### 關卡測試
- [ ] 主題選擇系統正常運作
- [ ] 難度選擇系統正常運作（Casual, Standard, Expert）
- [ ] 關卡順序正確
- [ ] 關卡難度遞增合理
- [ ] Boss關卡特殊能力正常
- [ ] 三種難度數值差異正確（HP、射速、子彈速度）
- [ ] Auto Balance功能正常運作
- [ ] 通關後顯示勝利界面
- [ ] 分數統計正確

### UI測試
- [ ] 所有UI元素正確顯示
- [ ] 數值更新即時
- [ ] 按鈕功能正常
- [ ] 面板切換流暢
- [ ] 彈出文字效果正常

### 性能測試
- [ ] 長時間遊玩無記憶體洩漏
- [ ] 大量彈藥同時存在不卡頓
- [ ] 幀率穩定在60FPS以上

### 平衡性測試
- [ ] 各Buff實用性平衡
- [ ] 關卡難度曲線合理
- [ ] 升級獲得速率適中
- [ ] 技能充能次數合理

## 🎯 未來擴展方向

### 遊戲模式
- [ ] 無盡模式
- [ ] 時間挑戰模式
- [ ] 每日挑戰

### 玩法擴展
- [ ] 方塊儲存系統（A/S/D/F鍵）
- [ ] 更多Buff種類
- [ ] 更多敵人種類
- [ ] Boss特殊招式
- [ ] 多階段Boss戰

### 系統功能
- [ ] 成就系統
- [ ] 排行榜
- [ ] 數據統計
- [ ] 遊戲設定（音量、畫質）
- [ ] 存檔系統

### 社群功能
- [ ] 分享功能
- [ ] 回放系統
- [ ] 自訂關卡編輯器

## 📝 已知問題與限制

### 目前限制
1. 幽靈方塊視覺未實作（TetrominoController中標記TODO）
2. 方塊儲存系統未實作（原版有A/S/D/F儲存槽）
3. 方塊預覽視覺簡化（TetrominoController.UpdateVisual()）
4. 需要實際音效和音樂資源
5. 需要美術資源提升視覺品質

### 待優化項目
1. 大量彈藥時可使用Job System
2. 可考慮使用ECS重構高頻更新組件
3. 可使用Addressables管理資源
4. 網格渲染可批次化

## ✅ 完成標準

### 最小可玩版本 (MVP)
- [x] 所有核心腳本完成
- [ ] 場景基本設置完成
- [ ] 可以進行完整遊戲流程
- [ ] 基本UI功能正常
- [ ] 至少有3個關卡可玩

### 完整版本
- [x] 所有腳本完成
- [ ] 完整10關卡配置
- [ ] 完整9種Buff配置
- [ ] UI完全功能
- [ ] 有音效和音樂
- [ ] 有視覺特效
- [ ] 通過所有測試

---

**當前進度**: 所有腳本實作完成 ✅  
**下一步**: 在Unity編輯器中進行場景設置和資源配置

