# 03 - 關卡數據系統

> 數據取自 `ScriptableObjects/StageData/Theme_*/` 目錄下的所有 .asset 文件
> 遊戲結構：**5 個主題 × 約 10 關**（非舊文檔假設的單一 20 關）

---

## 1. 遊戲結構概覽

| 主題 | 代碼名 | 風格描述 | 關卡數 | HP 範圍 | 最終 Boss HP |
|------|--------|---------|--------|---------|-------------|
| Theme_0 | Tutorial | 教學主題 | 10 (含1關停用) | 60 ~ 900 | 900 |
| Theme_1 | Nightmare | 夢魘主題 | 10 | 120 ~ 2000 | 2000 |
| Theme_2 | Inferno | 煉獄主題 | 10 | 140 ~ 1800 | 1800 |
| Theme_3 | Resentment | 怨念主題 | 10 | 120 ~ 2500 | 2500 |
| Theme_4 | Gravity | 重力主題 | 10 | 150 ~ 1400 | 1400 |

### 共通 shootInterval 模式

所有主題的射擊間隔遞減規律相同：

```
Stage 1:  1.8~1.9s
Stage 2:  1.6~1.7s
Stage 3:  1.4~1.6s
Stage 4:  1.2~1.4s
Stage 5:  1.0~1.2s
Stage 6:  1.6s（重置，Boss 後放鬆）
Stage 7:  1.45s
Stage 8:  1.3s
Stage 9:  1.15s
Stage 10: 1.0s
```

### 共通 bulletSpeed 模式

```
Stage 1:  8.0
Stage 2:  8.5
Stage 3:  9.0
Stage 4:  9.5
Stage 5:  10.0
Stage 6:  10.0
Stage 7:  10.5
Stage 8:  11.0
Stage 9:  11.5
Stage 10: 12.0
```

---

## 2. Theme 0 - Tutorial（教學主題）

| 關卡 | 名稱 | HP | 射擊間隔 | 子彈速度 | Boss | 獎勵Buff數 | 攻擊方式 |
|------|------|-----|---------|---------|------|-----------|---------|
| 1 | Brickwall | 60 | 1.8s | 8.0 | - | 0 | **無攻擊**（所有子彈禁用） |
| 2 | Training Trebuchet | 160 | 1.6s | 8.5 | - | 0 | Normal |
| 3 | Training Siege Ram | 200 | 1.4s | 9.0 | - | **3** | Normal |
| 4 | Training Demolisher | 260 | 1.2s | 9.5 | - | 1 | AreaDamage |
| 5 | Training War Tank | 1000 | 1.0s | 10.0 | **Boss** | 1 | Normal + AreaDamage |
| 6 | Excavator | 350 | 1.6s | 10.0 | **Boss** | 2 | Normal + CorruptVoid |
| 7 | *(停用)* | 120 | 1.45s | 10.5 | - | 0 | *(無攻擊)* |
| 8 | Twin-Horn | 500 | 1.3s | 11.0 | - | 3 | AddBlock |
| 9 | Inverted Sigil | 600 | 1.15s | 11.5 | - | 1 | Normal + AddBlock + InsertRow |
| 10 | Dreamwhorl King | 900 | 1.0s | 12.0 | **Boss** | **5** | Normal + AddBlock + InsertVoidRow |

### 子彈概率詳情

| 關卡 | Normal (chance) | Area | AddBlock | AddExp | InsertRow | InsVoid | CorrExp | CorrVoid |
|------|----------------|------|----------|--------|-----------|---------|---------|----------|
| 1 | 禁用 | - | - | - | - | - | - | - |
| 2 | 1.0 | - | - | - | - | - | - | - |
| 3 | 1.0 | - | - | - | - | - | - | - |
| 4 | - | **1.0** | - | - | - | - | - | - |
| 5 | 0.25 | 0.5 | - | - | - | - | - | - |
| 6 | 0.5 | - | - | - | - | - | - | 0.5 |
| 8 | - | - | **1.0** | - | - | - | - | - |
| 9 | 1.0 | - | 0.5 | - | 0.5 | - | - | - |
| 10 | 1.0 | - | 1.0 | - | - | 1.0 | - | - |

---

## 3. Theme 1 - Nightmare（夢魘主題）

| 關卡 | 名稱 | HP | 射擊間隔 | 子彈速度 | Boss | 獎勵Buff數 | 攻擊方式 |
|------|------|-----|---------|---------|------|-----------|---------|
| 1 | Dreamshade | 120 | 1.8s | 8.0 | - | 1 | Normal |
| 2 | Hollow Remains | 160 | 1.6s | 8.5 | - | 1 | Normal |
| 3 | Night Devourer | 200 | 1.4s | 9.0 | - | 1 | Normal |
| 4 | Rift-Horn | 260 | 1.2s | 9.5 | - | 1 | Normal |
| 5 | Abyssal Herald | 500 | 1.0s | 10.0 | **Boss** | 3 | Normal(0.85) + CorruptVoid(0.15) |
| 6 | The One-Eyed Void | 950 | 1.6s | 10.0 | - | 2 | Normal + AddBlock |
| 7 | Reaper of the Last Breath | 1000 | 1.45s | 10.5 | - | 3 | Normal + Area + AddBlock |
| 8 | Twin-Horn Sovereign | 1100 | 1.3s | 11.0 | - | 3 | AddBlock only |
| 9 | Inverted Sigil | 1200 | 1.15s | 11.5 | - | 1 | Normal + AddBlock(0.5) + InsertRow(0.5) |
| 10 | Dreamwhorl King | 2000 | 1.0s | 12.0 | **Boss** | 5 | Normal + AddBlock + InsertVoidRow |

---

## 4. Theme 2 - Inferno（煉獄主題）

**特色**：早期引入 CorruptVoid，後期以 CorruptExplosive 為核心。

| 關卡 | 名稱 | HP | 射擊間隔 | 子彈速度 | Boss | 獎勵Buff數 | 攻擊方式 |
|------|------|-----|---------|---------|------|-----------|---------|
| 1 | Decaying Shade | 140 | 1.8s | 8.0 | - | 1 | Normal(0.9) + CorruptVoid |
| 2 | Rot-Hunter | 190 | 1.6s | 8.5 | - | 1 | Normal(0.9) + CorruptVoid |
| 3 | Rotbound Wraith | 250 | 1.4s | 9.0 | - | 1 | Normal + AddBlock + CorruptVoid(0.3) |
| 4 | Gnawjaw | 310 | 1.2s | 9.5 | - | 1 | Normal(0.55) + AddBlock + CorruptVoid |
| 5 | Cinder Shade | 480 | 1.0s | 10.0 | **Boss** | 3 | Normal(0.6) + AddBlock + AddExpBlock |
| 6 | Infernal Skull | 1080 | 1.6s | 10.0 | - | 2 | Normal(0.85) + **CorrExp** |
| 7 | Infernal Warden | 1200 | 1.45s | 10.5 | - | 3 | Normal(0.85) + CorrExp |
| 8 | Ascendant Ember | 1320 | 1.3s | 11.0 | - | 3 | Normal(0.65) + AddExpBlock + CorrExp |
| 9 | Annihilation Scythe | 1500 | 1.15s | 11.5 | **Boss** | 3 | Normal(0.65) + AddExpBlock + CorrExp + CorrVoid |
| 10 | Eclipse King | 1800 | 1.0s | 12.0 | **Boss** | 1 | Normal(0.4) + Area + AddExpBlock + CorrExp |

---

## 5. Theme 3 - Resentment（怨念主題）

**特色**：Boss 密度最高（5/10關），唯一使用 SmartTargeting 的主題。

| 關卡 | 名稱 | HP | 射擊間隔 | 子彈速度 | Boss | 獎勵Buff數 | 攻擊方式 |
|------|------|-----|---------|---------|------|-----------|---------|
| 1 | Spitebound Dreamshade | 120 | 1.9s | 8.0 | - | 1 | Normal |
| 2 | Infernal Shade | 170 | 1.7s | 8.5 | - | 1 | Normal + Area |
| 3 | Wraith of Resentment | 230 | 1.6s | 9.0 | **Boss** | 1 | Area + CorrExp |
| 4 | Herald of Infernal Abyss | 300 | 1.4s | 9.5 | - | 1 | Area + AddExpBlock |
| 5 | Infernal Howl | 600 | 1.2s | 10.0 | **Boss** | 2 | Area + AddExpBlock + CorrExp |
| 6 | Hatebound Wraith | 1020 | 1.6s | 10.0 | **Boss** | 3 | CorrExp only |
| 7 | Grudge-Eater Larva | 1220 | 1.45s | 10.5 | - | 3 | AddExpBlock + CorrExp |
| 8 | Blood Hexer | 1380 | 1.3s | 11.0 | - | 3 | Area + AddExpBlock + CorrExp |
| 9 | Erosion Coil Beast | 1600 | 1.15s | 11.5 | **Boss** | 3 | Area + InsertRow + **SmartTargeting** |
| 10 | Bloodgrudge King | 2500 | 1.0s | 12.0 | **Boss** | 1 | AddBlock + AddExpBlock + CorrExp |

> **Theme 3 Stage 9 是全遊戲唯一使用 `useSmartTargeting = true` 的關卡。**
> SmartTargeting 會讓子彈瞄準方塊最高的列，而非隨機列。

---

## 6. Theme 4 - Gravity（重力主題）

**特色**：**完全不使用 Normal 和 AreaDamage 子彈**。
所有關卡以 InsertRow / InsertVoidRow / CorruptExplosive 為核心。
這是一個以「空間壓迫」為主的主題，而非直接傷害。

| 關卡 | 名稱 | HP | 射擊間隔 | 子彈速度 | Boss | 獎勵Buff數 | 攻擊方式 |
|------|------|-----|---------|---------|------|-----------|---------|
| 1 | Downforce | 150 | 1.9s | 8.0 | - | 1 | InsertRow |
| 2 | Pressureline | 200 | 1.7s | 8.5 | - | 1 | InsertRow |
| 3 | Weighshift | 260 | 1.6s | 9.0 | - | 1 | InsertRow + CorrExp |
| 4 | Collapse Edge | 330 | 1.4s | 9.5 | - | 1 | InsertRow + CorrExp |
| 5 | Singularity Maw | 500 | 1.2s | 10.0 | **Boss** | 1 | InsertRow + InsertVoidRow |
| 6 | Graviton | 550 | 1.6s | 10.0 | - | 2 | InsertVoidRow |
| 7 | Horizon Tilt | 650 | 1.45s | 10.5 | - | 3 | InsertVoidRow + CorrExp |
| 8 | Event Turbine | 820 | 1.3s | 11.0 | **Boss** | 3 | InsertRow + InsertVoidRow + CorrExp |
| 9 | Orbitfall | 960 | 1.15s | 11.5 | - | 3 | InsertVoidRow + CorrExp |
| 10 | Gravity Overlord | 1400 | 1.0s | 12.0 | **Boss** | 2 | InsertRow + InsertVoidRow + CorrExp |

---

## 7. Boss 分布統計

| 主題 | Boss 關卡 | Boss 總數 | 最終 Boss HP |
|------|----------|----------|-------------|
| Theme_0 | 5, 6, 10 | 3 | 900 |
| Theme_1 | 5, 10 | 2 | 2000 |
| Theme_2 | 5, 9, 10 | 3 | 1800 |
| Theme_3 | 3, 5, 6, 9, 10 | **5** | **2500** |
| Theme_4 | 5, 8, 10 | 3 | 1400 |

---

## 8. Buff 獎勵統計

### 每主題總獎勵 Buff 數

| 主題 | 各關獎勵 | 總計 |
|------|---------|------|
| Theme_0 | 0,0,3,1,1,2,0,3,1,5 | **16** |
| Theme_1 | 1,1,1,1,3,2,3,3,1,5 | **21** |
| Theme_2 | 1,1,1,1,3,2,3,3,3,1 | **19** |
| Theme_3 | 1,1,1,1,2,3,3,3,3,1 | **19** |
| Theme_4 | 1,1,1,1,1,2,3,3,3,2 | **18** |

### 獎勵節奏分析

- Stage 1~4: 每關 0~1 個（慢速成長期）
- Stage 5 (首Boss): 1~3 個（第一次跳躍）
- Stage 6~9: 2~3 個（穩定成長期）
- Stage 10 (最終Boss): 1~5 個（視主題而定）

---

## 9. 主題特色分析

### 威脅維度對比

| 主題 | 直接傷害 | 空間壓迫 | 方塊腐化 | 虛無威脅 |
|------|---------|---------|---------|---------|
| Theme_0 | 中 | 低 | 低 | 低 |
| Theme_1 | 中 | 中 | 低 | 中 |
| Theme_2 | 低 | 低 | **高** | 中 |
| Theme_3 | **高** | 低 | **高** | 低 |
| Theme_4 | **無** | **極高** | 中 | **高** |

### 各主題對抗策略

| 主題 | 關鍵需求 | 推薦優先 Buff |
|------|---------|-------------|
| Tutorial | 基礎機制學習 | 任意 |
| Nightmare | 穩定消除能力 | Salvo/Burst |
| Inferno | 管理腐化方塊 | Defense/Counter |
| Resentment | 高傷害輸出 | Burst/Volley |
| Gravity | 快速消行防溢出 | SpaceExpansion/Explosion |

---

## 交叉引用

### 引用來源
- ← 各 Theme 的 .asset 文件
- ← `StageDataSO.cs` (字段結構)
- ← `EnemyController.cs` (子彈選擇邏輯)

### 被以下文檔使用
- → `05_Balance_Analysis.md` (擊殺時間估算)
