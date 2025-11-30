# Tenronis Unity 專案總結

## 專案概述

此專案是將React TypeScript版本的Tenronis（俄羅斯加農砲）遊戲，使用Unity 6和C#重新實作，完全遵循Unity最佳實踐。

## 技術架構

### 設計模式

1. **單例模式 (Singleton Pattern)**
   - 所有管理器使用單例確保唯一性
   - 實作在 GameManager, GridManager, PlayerManager, CombatManager, AudioManager

2. **事件驅動架構 (Event-Driven Architecture)**
   - 使用 `GameEvents` 靜態類別解耦各系統
   - 系統間通過事件通訊，避免強依賴

3. **對象池模式 (Object Pool Pattern)**
   - 導彈和子彈使用泛型對象池 `ObjectPool<T>`
   - 減少實例化和GC壓力

4. **ScriptableObject 資料驅動**
   - 關卡配置使用 `StageDataSO`
   - Buff配置使用 `BuffDataSO`
   - 易於擴展和調整遊戲平衡

## 核心系統

### 1. 遊戲管理 (Game Management)
- **GameManager.cs**: 遊戲狀態機、關卡進度、Roguelike系統
- **GameInitializer.cs**: 確保所有必要系統都被初始化

### 2. 網格系統 (Grid System)
- **GridManager.cs**: 10x20網格管理、方塊放置、消除檢測
- **Block.cs**: 方塊視覺組件、HP顯示

### 3. 俄羅斯方塊系統 (Tetromino System)
- **TetrominoController.cs**: 方塊生成、移動、旋轉、鎖定
- **TetrominoDefinitions.cs**: 7種經典形狀定義

### 4. 戰鬥系統 (Combat System)
- **CombatManager.cs**: 彈藥管理、碰撞檢測、反擊系統
- **Missile.cs**: 玩家導彈（向上飛行）
- **Bullet.cs**: 敵人子彈（4種類型）

### 5. 玩家系統 (Player System)
- **PlayerManager.cs**: 玩家數據、Combo系統、技能管理
- **SkillExecutor.cs**: 處決和修復技能實作

### 6. 敵人系統 (Enemy System)
- **EnemyController.cs**: 敵人AI、射擊模式、關卡數據讀取
- **StageDataSO.cs**: 關卡配置（HP、射擊間隔、特殊能力）

### 7. UI系統 (UI System)
- **GameUI.cs**: 主遊戲界面、狀態顯示
- **RoguelikeMenu.cs**: 升級選單、Buff選擇
- **PopupText.cs**: 彈出文字效果

### 8. 音效系統 (Audio System)
- **AudioManager.cs**: 音效和音樂管理、BGM切換

### 9. 輸入系統 (Input System)
- **InputManager.cs**: 鍵盤輸入處理

## 遊戲機制

### 核心玩法循環
1. 俄羅斯方塊下落
2. 玩家控制移動/旋轉
3. 鎖定方塊到網格
4. 檢查並消除滿行
5. 發射導彈攻擊敵人
6. 敵人發射子彈攻擊方塊
7. 方塊抵擋子彈
8. 累積傷害獲得升級

### 戰鬥公式

**導彈傷害計算:**
```
基礎傷害 = 2
齊射加成 = (消除行數 - 1) × 齊射等級 × 0.5
連發加成 = 連擊數 × 連發等級 × 0.25
總傷害 = 基礎傷害 + 齊射加成 + 連發加成
```

**反擊系統:**
- 新放置的方塊在0.2秒內被擊中會觸發反擊
- 發射數量 = 反擊等級
- 維持Combo不中斷

### Roguelike系統
- 關卡完成後獲得升級點數（由關卡配置決定）
- 9種Buff可選擇，各有不同的起始等級和上限等級
- **Buff分類**：
  - 普通強化（6種）：有上限等級的強化（Salvo, Burst, Counter, Explosion, SpaceExpansion, ResourceExpansion）
  - 傳奇強化（3種）：無上限或特殊效果的強化（Defense, Volley, Heal）
- **傳奇強化選擇機制**：
  - 當有普通強化達到滿級時，會自動提供傳奇強化選擇機會
  - 傳奇強化選擇時，只從傳奇強化陣列中選擇
  - 如果傳奇強化數量 ≤ 3，直接顯示全部；如果 > 3，隨機選擇3個
- 技能系統：Execution和Repair改為消耗CP使用（5 CP和30 CP）
- 爆炸充能系統：反擊+5充能，消排+50充能，Explosion Buff增加上限（起始1級，最高4級）

## 性能優化

### 已實作優化
1. **對象池**: 導彈和子彈重用，減少GC
2. **事件系統**: 減少Update()中的查詢，包含CP變化事件
3. **批次更新**: 視覺更新使用髒標記
4. **空間劃分**: 碰撞檢測使用網格座標
5. **CP系統**: 技能改為消耗CP，提供更靈活的資源管理

### 潛在優化
1. 使用Job System處理大量彈藥
2. 使用ECS重構頻繁更新的組件
3. 使用Addressables動態載入資源
4. 實作網格視覺的批次渲染

## 代碼規範

### 命名規範
- **類別**: PascalCase (GameManager)
- **方法**: PascalCase (HandleInput)
- **私有欄位**: camelCase (currentState)
- **常數**: UPPER_SNAKE_CASE (BOARD_WIDTH)
- **事件**: On前綴 (OnGameStateChanged)

### 組織結構
- 每個腳本單一職責
- 使用namespace組織
- 公開API在上，私有實作在下
- XML註解說明公開成員

### Unity 6 API使用
- ✅ `FindFirstObjectByType<T>()` 取代 `FindObjectOfType`
- ✅ 使用現代化組件架構
- ✅ 避免使用已過時API

## 擴展性

### 易於擴展的部分
1. **新增關卡**: 建立新的StageDataSO
2. **新增Buff**: 建立新的BuffDataSO並在PlayerManager處理
3. **新增子彈類型**: 擴展BulletType enum和處理邏輯
4. **新增特效**: 訂閱GameEvents添加特效

### 建議新功能
1. **多種模式**: 無盡模式、時間挑戰
2. **成就系統**: 追蹤玩家表現
3. **排行榜**: 分數記錄
4. **方塊儲存系統**: A/S/D/F鍵儲存方塊（已在原版中）
5. **更多敵人種類**: 不同攻擊模式的Boss

## 檔案統計

### 腳本數量
- 核心系統: 3個
- 資料定義: 4個
- 管理器: 6個
- 遊戲玩法: 7個
- UI: 4個
- 音效: 1個
- 工具: 1個
- **總計: 26個C#腳本**

### 程式碼行數（約）
- 總行數: ~3000行
- 註解率: ~20%

## 測試建議

### 功能測試
- [ ] 方塊移動、旋轉、硬降
- [ ] 消除行檢測
- [ ] 導彈發射和碰撞
- [ ] 子彈類型效果
- [ ] 反擊系統觸發
- [ ] Combo計時器
- [ ] 溢出懲罰
- [ ] 關卡切換
- [ ] Buff效果
- [ ] 技能使用

### 平衡測試
- [ ] 各關卡難度曲線
- [ ] Buff權重分配
- [ ] 傷害數值平衡
- [ ] 升級獲得速率

### 性能測試
- [ ] 大量彈藥同時存在
- [ ] 長時間遊玩記憶體穩定
- [ ] 畫面更新流暢度

## 已知限制

1. **方塊儲存系統**: 原版有A/S/D/F儲存槽，Unity版尚未實作
2. **幽靈方塊**: TetrominoController中標記為TODO
3. **視覺特效**: 需要額外的粒子系統和材質
4. **音效資源**: 需要準備實際音效檔案

## 部署清單

### 最小可運行版本需要
- ✅ 所有管理器腳本
- ✅ 所有遊戲玩法腳本
- ✅ 基本UI
- ✅ 至少1個關卡數據
- ✅ 至少3個Buff數據
- ✅ Block預製體
- ✅ Missile預製體
- ✅ Bullet預製體

### 完整版本額外需要
- ⚪ 10個關卡數據（完整配置）
- ⚪ 9個Buff數據（完整配置）
- ⚪ 音效資源（8+ clips）
- ⚪ 音樂資源（2+ tracks）
- ⚪ 粒子特效
- ⚪ 自訂材質和Shader
- ⚪ UI美術資源

## 授權與致謝

本專案基於原React版本改寫，展示如何使用Unity最佳實踐重新實作複雜遊戲機制。

### 技術特點
- ✅ 清晰的架構分層
- ✅ 良好的代碼組織
- ✅ 易於維護和擴展
- ✅ 符合Unity最佳實踐
- ✅ 完整的中文註解

### 適合學習的主題
- 單例模式在Unity中的應用
- 事件驅動架構
- 對象池優化
- ScriptableObject資料驅動設計
- 複雜遊戲邏輯的拆分與組織

---

**製作時間**: 2025年11月
**Unity版本**: Unity 6
**程式語言**: C# 
**專案類型**: 2D遊戲

