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
- [ ] 建立10個StageData
  - [ ] Stage 1: 偵察無人機
  - [ ] Stage 2: 突擊機械兵
  - [ ] Stage 3: 攻城坦克
  - [ ] Stage 4: 精英護衛
  - [ ] Stage 5: 末日核心
  - [ ] Stage 6: 重裝機兵
  - [ ] Stage 7: 虛空干擾者
  - [ ] Stage 8: 裂變轟炸機
  - [ ] Stage 9: 深淵巨口
  - [ ] Stage 10: 終焉機械神

- [ ] 建立9個BuffData
  - [ ] Defense Buff（起始0，無上限）
  - [ ] Volley Buff（齊射強化，起始1，上限6）
  - [ ] Heal Buff（立即效果）
  - [ ] Explosion Buff（過載爆破，起始1，上限4，增加充能上限）
  - [ ] Salvo Buff（協同火力，起始0，無上限）
  - [ ] Burst Buff（連發強化，起始1，上限6）
  - [ ] Counter Buff（反擊強化，起始1，上限6）
  - [ ] SpaceExpansion Buff（起始1，上限4）
  - [ ] ResourceExpansion Buff（起始0，上限3）
  - [ ] **注意**：Execution 和 Repair 已改為消耗CP的技能，不再作為Buff

### 管理器連接
- [ ] GameManager
  - [ ] 連接Stages陣列
  - [ ] 連接AvailableBuffs陣列
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
  - [ ] Title Text
  - [ ] Start Button
- [ ] 建立GameplayPanel
  - [ ] Score Text
  - [ ] Stage Text
  - [ ] Combo Text
  - [ ] Player HP Slider + Text
  - [ ] Enemy HP Slider + Text
  - [ ] **注意**：Execution 和 Repair 已改為消耗CP的技能，不再顯示次數UI
- [ ] 建立LevelUpPanel
  - [ ] Buff Options Container
- [ ] 建立GameOverPanel
  - [ ] Title Text
  - [ ] Final Score Text
  - [ ] Restart Button
- [ ] 建立VictoryPanel
  - [ ] Title Text
  - [ ] Final Score Text
  - [ ] Menu Button
- [ ] 連接GameUI腳本所有引用
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
- [ ] 不可摧毀行插入正常
- [ ] 4種子彈類型效果正確

### 關卡測試
- [ ] 10個關卡順序正確
- [ ] 關卡難度遞增合理
- [ ] Boss關卡（5, 10）特殊能力正常
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

