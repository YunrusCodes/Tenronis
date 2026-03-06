# Tenronis 完整機制文件

> 基於源碼審計，非 markdown 文件。最後更新：2026-03-06

---

## 1. 遊戲流程

### 狀態機 (GameState)
```
Menu → LevelUp → Playing → LevelUp → Playing → ... → Victory
                    ↓
                 GameOver
```

### 關卡流程
1. 選擇主題 (StageSetSO) → 獲取該主題的關卡列表
2. 重置玩家數據、清空網格
3. 獲得第一關的獎勵卡牌 → 進入 LevelUp（選 Buff + 預覽敵人）
4. 進入 Playing → 操作方塊打敵人
5. 敵人 HP 歸零 → CP 全恢復 + 恢復 50% maxHP → 下一關的獎勵卡牌 → LevelUp
6. 所有關卡通過 → Victory（保存 passKey）

---

## 2. 方塊系統

### 棋盤
- 10 × 20 格 + 頂部 2 格緩衝區 (y = -2, -1)
- Y 軸反轉：Grid y=0 在頂部，y=19 在底部
- 座標轉換：`worldY = -gridY * blockSize + offsetY`

### Tetromino
- 七種標準形狀：I/O/T/J/L/S/Z
- 各有對應顏色：Cyan/Yellow/Purple/Blue/Orange/Green/Red
- 隨機生成（`TetrominoDefinitions.GetRandomTetromino()`，非 bag 系統）

### 操作
| 按鍵 | 動作 |
|------|------|
| ← → | 水平移動（支援 DAS 0.15s + ARR 0.03s） |
| ↓ | 軟降（間隔 0.05s） |
| ↑ | 順時針旋轉（SRS 踢牆） |
| Space | 硬降（立即鎖定） |
| A/S/D/F | 儲存/交換方塊（槽位 0-3） |
| 1 | 湮滅技能 |
| 2 | 處決技能 |
| 3 | 修補技能 |

### SRS 旋轉系統
- 完整 SRS 踢牆測試（`SRSData.GetKickOffsets`）
- I 方塊有專用踢牆表
- O 方塊不旋轉
- 旋轉時腐化格子座標同步旋轉

### Lock Delay
- 觸地後開始計時 0.5 秒
- 成功移動/旋轉且仍接地 → 重置計時器（最多 15 次）
- 移動/旋轉後懸空 → 完全退出 Lock Delay
- 硬降忽略 Lock Delay 直接鎖定

### 自動下落
- 每 0.8 秒下落一格（`TICK_RATE = 0.8f`）

### 儲存槽位
- 4 個槽位（A/S/D/F），由 SpaceExpansion 升級解鎖
- 開局解鎖 1 個（`SPACE_EXPANSION_START_LEVEL = 1`）
- 每個槽位**每次方塊落下前只能使用一次**
- 槽位為空 → 存入當前方塊，取出 next
- 槽位不為空 → 交換當前方塊與槽位方塊
- 腐化信息隨方塊一起存取

### 方塊鎖定流程
1. 檢查溢出（鎖定時：緩衝區有格子 → 溢出；網格內重疊 → 溢出）
2. 合併到網格（`MergePieceToGrid`）
   - HP = `BASE_BLOCK_HP(1) + blockDefenseLevel`
   - 腐化格子保留其 BlockType
3. 觸發 `OnPieceLocked` → GridManager 檢查消行
4. 0.1 秒後生成下一個方塊

### 溢出觸發條件
- 生成新方塊時網格內有重疊
- 鎖定時方塊仍在緩衝區 (y < 0)
- 插入垃圾行時頂行已有方塊
- AddBlock 時目標位置在 y=0

---

## 3. 方塊特性

| BlockType | 效果 |
|-----------|------|
| Normal | 無特殊效果 |
| Void | **虛無抵銷**：若消除行中有任何 Void 方塊，整次消除所有行都不發射導彈 |
| Explosive | 被子彈打掉（HP→0）時，對玩家造成 **5 HP** 傷害 |

### 方塊數據 (BlockData)
- `color`: 顏色（7色 + Gray + Garbage）
- `hp / maxHp`: 生命值
- `isIndestructible`: 是否不可摧毀（垃圾行 = true）
- `blockType`: Normal / Void / Explosive
- `createdTime`: 創建時間（用於反擊判定的 0.2 秒窗口）

---

## 4. 消行機制

### 檢查流程 (`GridManager.CheckAndClearRows`)
1. 遍歷所有行，找出滿行
2. 區分：普通行 / 不可摧毀行
3. 記錄是否包含 Void 方塊（`hasVoidBlocks`）
4. 區分非垃圾行（至少有一個非 Garbage 色方塊的行）
5. 有普通滿行 → 消除（不可摧毀行也一起消除）
6. **先觸發事件**（`TriggerRowsCleared`），再清除方塊
7. 上方行下落填補

### 消行事件參數
```csharp
OnRowsCleared(List<int> clearedRows, List<int> nonGarbageRows, bool hasVoid)
```

---

## 5. 戰鬥系統

### 導彈（玩家 → 敵人）

#### 觸發條件
- 消除行（只有非垃圾行且無虛無抵銷時發射）
- 處決技能
- 湮滅技能硬降破壞方塊
- 反擊

#### 消行導彈公式
```
每格發射數量 = 1 + missileExtraCount (Volley 等級)
每發傷害 = BASE_MISSILE_DAMAGE(1) + salvoBonus + burstBonus

salvoBonus = (有效行數 > 1) ? (min(有效行數,4) - 1) × salvoLevel × 0.5 : 0
burstBonus = burstLevel × comboCount × 0.25
```
- 從每個非垃圾行的每格 (10格/行) 各發射導彈
- 多發導彈以骰子點數方式排列（1-6 有固定圖案，>6 環形排列）

#### 導彈飛行
- 速度 = `MISSILE_SPEED(20)` 向上
- 飛出棋盤頂部 → 命中敵人
- 可攔截敵人子彈（距離 < 0.5f 碰撞抵銷）
- 有穿透機制（`pierce`）

### 子彈（敵人 → 玩家）

#### 敵人射擊
- 按 `shootInterval` 定時射擊（單發）
- 子彈類型由權重隨機決定（各子彈 enabled + chance）
- 可啟用智能射擊：
  - AddBlock 類 → 優先打最高列
  - AreaDamage → 優先打最低列

#### 8 種子彈效果

| 類型 | 效果 |
|------|------|
| Normal | 對命中方塊 -1 HP |
| AreaDamage | 命中點 3×3 範圍各 -1 HP |
| AddBlock | -1 HP + 上方添加普通垃圾方塊（HP = GARBAGE_BLOCK_HP + defenseLevel） |
| AddExplosiveBlock | -1 HP + 上方添加爆炸垃圾方塊 |
| InsertRow | -1 HP + 底部插入一整行不可摧毀普通方塊（HP = 9999 + defenseLevel） |
| InsertVoidRow | -1 HP + 底部插入一整行不可摧毀虛無方塊 |
| CorruptExplosive | -1 HP + 下個 Tetromino 隨機一格腐化為 Explosive |
| CorruptVoid | -1 HP + 下個 Tetromino 隨機一格腐化為 Void |

#### 子彈穿透棋盤
- 子彈超過 Grid 底部（`gridY >= BOARD_HEIGHT`）→ 命中城堡，造成 **10 HP** 傷害

#### 不可摧毀方塊被子彈命中
- 不會被摧毀
- **反傷玩家 10 HP**

### 碰撞處理順序（每幀）
1. 導彈 vs 子彈（互相抵銷）
2. 導彈超出頂部 → 命中敵人
3. 子彈擊中方塊 / 穿透到底部

---

## 6. 反擊系統

### 觸發條件
- `counterFireLevel > 0`
- 方塊被子彈擊中時，該方塊的 `createdTime` 距當前時間 ≤ **0.2 秒**（`COUNTER_FIRE_TIME_WINDOW`）

### 效果
- combo+1
- 發射反擊導彈：數量 = `counterFireLevel`，骰子排列
- 反擊導彈傷害 = `BASE_MISSILE_DAMAGE(1) + burstBonus`
- 爆炸充能 +5（`EXPLOSION_COUNTER_CHARGE`）
- 觸發玩家視覺特效
- 所有子彈類型（Normal/Area/AddBlock 等）都會檢查反擊

---

## 7. Combo 系統

### 增加 Combo 的情況
- 消除行（每次消行 +1，不論消幾行）
- 反擊觸發（+1）
- 湮滅破壞方塊（整次 +1）
- 處決技能（+1）

### 重置 Combo
- 方塊鎖定後，如果 `counterFireLevel > 0` → 開始 0.3 秒重置倒數
- 0.3 秒內沒有新的消行/反擊 → combo 歸零
- 如果 `counterFireLevel == 0` → 方塊鎖定時立即歸零

---

## 8. 溢出機制

### 觸發時
1. 清空整個棋盤
2. CP >= 75 → 消耗 75 CP
3. CP < 75 → HP 變為 1，CP 歸零
4. 如果有爆炸充能 → 對敵人造成等量傷害，充能歸零
5. 播放大爆炸特效 + 音效

---

## 9. 爆炸充能系統

### 充能來源
| 來源 | 充能量 |
|------|--------|
| 消除行 | +50 (`EXPLOSION_ROW_CLEAR_CHARGE`) |
| 反擊觸發 | +5 (`EXPLOSION_COUNTER_CHARGE`) |

### 釋放
- 溢出時自動釋放
- 傷害 = 當前充能值（直接作為 damage 傳給 `OnEnemyDamaged`）
- 釋放後充能歸零

### 上限
- 初始上限 200（`EXPLOSION_INITIAL_MAX_CHARGE`）
- Explosion Buff 每級 +200，最高 4 級 → 最大上限 1000

---

## 10. 資源系統

### HP（生命值）
- 初始/上限：100（`PLAYER_MAX_HP`）
- 歸零 → Game Over
- 過關後恢復 50% maxHP（非第一關）

### CP（Castle Point）
- 初始/上限：100（`PLAYER_MAX_CP`）
- 用途：技能消耗 + 溢出消耗
- 過關後完全恢復
- ResourceExpansion 每級 +50 上限（保持 CP 比例）

---

## 11. 主動技能

### 湮滅（按鍵 1）
- 解鎖：TacticalExpansion Lv1
- 消耗：5 CP（`ANNIHILATION_CP_COST`）
- 前提：有活躍方塊、尚未處於湮滅狀態
- 效果：
  - 當前方塊進入幽靈穿透狀態（半透明，50% alpha）
  - 碰撞檢測只檢查左右邊界，不檢查底部和方塊佔用
  - 清除當前方塊的腐化信息
  - 無 Ghost Piece 預覽
  - 觸地（超出底部）→ 往上移一格再執行湮滅效果
  - 硬降 → 執行湮滅效果
- 湮滅效果：
  - 遍歷方塊每格，破壞重疊的非不可摧毀方塊
  - 每破壞一格：發射 `1 + volleyExtraMissiles` 發導彈（傷害 = BASE + burstBonus）
  - 至少破壞 1 格 → combo+1
  - 方塊被消耗（不鎖定到網格）

### 處決（按鍵 2）
- 解鎖：TacticalExpansion Lv2
- 消耗：5 CP（`EXECUTION_CP_COST`）
- 前提：有可被處決的方塊（至少一列有非不可摧毀方塊）
- 效果：
  - 遍歷每列（x = 0~9），從頂部向下掃描
  - 跳過不可摧毀方塊（垃圾行）
  - 找到第一個可摧毀方塊 → 移除
  - 每移除一格：發射 `1 + volleyExtraMissiles` 發導彈（傷害 = `EXECUTION_DAMAGE(4)`）
  - combo+1

### 修補（按鍵 3）
- 解鎖：TacticalExpansion Lv3
- 消耗：30 CP（`REPAIR_CP_COST`）
- 前提：存在封閉空洞
- 效果：
  - BFS 從頂行所有空格開始，標記所有可達空格
  - 未被標記的空格 = 封閉空洞
  - 填入灰色方塊（HP = `BASE_BLOCK_HP + defenseLevel`）
  - 自動檢查消行（可觸發導彈發射）

---

## 12. Roguelike 升級系統

### 觸發時機
- 每關開始前的 LevelUp 狀態
- 每關提供 `rewardBuffCount` 張升級卡牌

### 選擇機制
- 每次顯示 3 個選項（基於權重隨機 `spawnWeight`）
- 已滿級的 Buff 不再出現
- 普通池全部滿級 → 自動切換到傳奇池

### 普通強化

| Buff | 效果 | 起始Lv | 上限 |
|------|------|--------|------|
| Salvo（齊射） | 多行消除時增加導彈傷害，每級 +0.5 倍率 | 1 | 6 |
| Burst（連發） | Combo 傷害加成，每級 +0.25 倍率 | 1 | 6 |
| Counter（反擊） | 反擊導彈數 = 等級 | 1 | 6 |
| Explosion（爆炸充能） | 充能上限每級 +200 | 1 | 4 |
| SpaceExpansion（空間擴充） | 解鎖儲存槽位 | 1 | 4 |
| ResourceExpansion（資源擴充） | CP 上限每級 +50 | 0 | 3 |

### 傳奇強化

| Buff | 效果 | 上限 |
|------|------|------|
| Defense（裝甲） | 所有方塊 HP +1（含玩家方塊和垃圾方塊） | 無上限 |
| Volley（協同火力） | 每格額外多發射 1 發導彈 | 5 |
| TacticalExpansion（戰術擴展） | Lv1 湮滅、Lv2 處決、Lv3 修補 | 3 |

### 注意
- Execution 和 Repair 作為 BuffType 存在但**不出現在選擇池中**（由 TacticalExpansion 解鎖）
- Heal 已廢棄
- Defense 會影響垃圾方塊和不可摧毀方塊的 HP

---

## 13. 敵人系統

### 配置 (StageDataSO)
- `maxHp`: 敵人血量
- `shootInterval`: 射擊間隔（秒）
- `bulletSpeed`: 子彈速度
- 8 種子彈各有 `enabled` + `chance`（權重）
- `useSmartTargeting`: 智能射擊
- `isBossStage`: Boss 關（影響 BGM）
- `rewardBuffCount`: 過關獎勵卡牌數
- `showHint` / `hint`: 關卡提示

### 擊敗流程
1. HP ≤ 0 → `isDefeated = true`，立即停止射擊
2. 清除所有飛行中的敵方子彈
3. 播放擊敗動畫（搖晃+淡化+下沉，共 3 秒）
4. 動畫結束 → 觸發 `OnEnemyDefeated`

### 受傷特效
- 在敵人 Sprite 範圍內隨機位置生成爆炸特效
- 特效數量基於 `intensityLevel`（n = min(level, 4)）
- 排隊系統：每 0.1 秒處理一批

---

## 14. 主題系統

### StageSetSO 包含
- 主題名稱（三語）
- 背景圖片：戰鬥背景、敵人介紹背景、工程師背景
- 玩家圖片
- 關卡列表 (`List<StageDataSO>`)
- 解鎖系統：`passKey`（通關後保存）、`unlockKey`（解鎖條件）
- 首次通關資訊（三語）

---

## 15. 腐化系統

### 觸發
- CorruptExplosive / CorruptVoid 子彈命中方塊時
- 腐化目標：**下一個** Tetromino 的隨機一格

### 數據追蹤
- `nextCorruptedBlocks`: 下個方塊的腐化信息
- 生成新方塊時轉移到 `currentCorruptedBlocks`
- 儲存/交換時腐化信息隨方塊移動
- 旋轉時腐化座標同步旋轉

### 湮滅清除腐化
- 進入湮滅狀態時自動清除當前方塊的所有腐化

---

## 16. 視覺系統

### Ghost Piece
- 顯示硬降位置的半透明預覽
- 湮滅狀態下不顯示
- 地形改變時自動更新（`OnGridChanged`）

### 浮動文字 (PopupText)
- 反擊觸發：綠色「反擊!」
- 城堡受損：紅色「城堡受損」
- 由 `OnShowPopupText` 事件驅動

### 畫面震動 (ScreenShake)
- 導彈攔截子彈時：輕微震動（0.08s, 強度 0.05）

---

## 17. 物件池系統

- 導彈池：初始 50 個
- 子彈池：初始 30 個
- 狀態切換時清理所有彈藥
- 敵人死亡時只清理子彈（導彈保留飛行）

---

## 18. 過關間轉場

### 敵人被擊敗後
1. CP 完全恢復
2. HP 恢復 50% maxHP（非第一關）
3. `currentStageIndex++`
4. 獲得下一關的獎勵卡牌
5. 鎖定當前方塊（如有）：
   - 浮空 → 保留形狀作為 next，不鎖定
   - 有支撐 → 鎖定到網格（觸發消行）
6. 進入 LevelUp 狀態

### 新遊戲開始時
- 進入 Playing 且 stageIndex == 0 → 清空網格
