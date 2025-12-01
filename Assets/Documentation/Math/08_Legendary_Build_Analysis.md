# 08 - 传奇Build路线分析
# Legendary Build Analysis

**文档版本**: 2.1  
**最后更新**: 2025年12月1日  
**核心议题**: 为什么不能只用Volley来看平衡

---

## 📋 目录

1. [Volley Build基准分析](#volley-build基准分析)
2. [当前设计的核心问题](#当前设计的核心问题)
3. [三层玩家×三种Build计算](#三层玩家三种build计算)
4. [Build路线数学角色](#build路线数学角色)
5. [结论与建议](#结论与建议)

---

## 🎯 Volley Build基准分析

### Expert + Volley Build的极限状态

#### 假设配置（Stage 20）

```
玩家层级：Expert（Combo 20-40）
Build选择：
- Volley（协同火力）: Lv 5
- Salvo（齐射强化）: Lv 6
- Burst（连发强化）: Lv 6
- Defense（装甲强化）: Lv 0（未选）
- Tactical（战术扩展）: Lv 0（未选）

核心假设：所有升级机会（32次）优先投入火力系
```

#### 傷害计算

**单发导弹伤害**：
```
DMG_single = D_base + B_salvo + B_burst

已知：
D_base = 2.0
B_salvo = (r-1) × L_salvo × 0.5 = (4-1) × 6 × 0.5 = 9.0
B_burst = C × L_burst × 0.25 = 30 × 6 × 0.25 = 45.0

DMG_single = 2.0 + 9.0 + 45.0 = 56.0
```

**总导弹数量**：
```
N_missiles = r × W × (1 + L_volley)
           = 4 × 10 × (1 + 5)
           = 240发
```

**单次消除总伤害**：
```
DMG_total = 240 × 56.0 = 13,440
```

**这个数字是Stage 20 Boss HP（5000）的2.69倍！**

#### PDA计算（Expert + Volley）

```
λ_clear = λ_pieces / E[r]
        = 0.70 / 3.2
        = 0.219 次/秒

PDA = λ_clear × E[DMG_total]
    = 0.219 × 13,440
    = 2,943 伤害/秒
```

#### T_kill计算

**理论击杀时间**：
```
T_kill = HP(20) / PDA
       = 5,000 / 2,943
       = 1.70秒
```

**实际时间**（含overhead）：
```
T_actual = T_kill × (1 + 0.20)
         = 1.70 × 1.20
         = 2.04秒
```

### Volley Build的关卡表现

#### 完整火力曲线（Expert + Volley路线）

| Stage | HP | L_volley | L_salvo | L_burst | E[C] | DMG_total | PDA | T_kill | T_actual |
|-------|-----|---------|---------|---------|------|-----------|-----|--------|---------|
| 1 | 120 | 0 | 1 | 1 | 5 | 339 | 74 | 1.6s | 1.9s |
| 2 | 160 | 0 | 1 | 1 | 6 | 420 | 92 | 1.7s | 2.0s |
| 3 | 240 | 1 | 1 | 1 | 8 | 730 | 160 | 1.5s | 1.8s |
| 4 | 300 | 1 | 2 | 1 | 10 | 950 | 208 | 1.4s | 1.7s |
| 5 | 400 | 1 | 2 | 2 | 12 | 1,680 | 368 | 1.1s | 1.3s |
| 6 | 500 | 2 | 2 | 2 | 14 | 2,280 | 500 | 1.0s | 1.2s |
| 7 | 600 | 2 | 3 | 2 | 16 | 2,920 | 640 | 0.9s | 1.1s |
| 8 | 700 | 2 | 3 | 3 | 18 | 3,680 | 806 | 0.9s | 1.1s |
| 9 | 900 | 3 | 3 | 3 | 20 | 4,800 | 1,052 | 0.9s | 1.0s |
| 10 | 1000 | 3 | 4 | 3 | 22 | 5,440 | 1,192 | 0.8s | 1.0s |
| 11 | 1200 | 3 | 4 | 4 | 24 | 6,720 | 1,473 | 0.8s | 1.0s |
| 12 | 1400 | 4 | 4 | 4 | 25 | 8,000 | 1,753 | 0.8s | 1.0s |
| 13 | 1600 | 4 | 5 | 4 | 27 | 9,520 | 2,086 | 0.8s | 0.9s |
| 14 | 1800 | 4 | 5 | 5 | 28 | 10,640 | 2,332 | 0.8s | 0.9s |
| 15 | 2000 | 5 | 5 | 5 | 29 | 12,180 | 2,669 | 0.7s | 0.9s |
| 16 | 2200 | 5 | 6 | 5 | 30 | 13,200 | 2,893 | 0.8s | 0.9s |
| 17 | 2600 | 5 | 6 | 6 | 30 | 14,400 | 3,156 | 0.8s | 1.0s |
| 18 | 3000 | 5 | 6 | 6 | 30 | 14,400 | 3,156 | 1.0s | 1.2s |
| 19 | 3600 | 5 | 6 | 6 | 30 | 14,400 | 3,156 | 1.1s | 1.4s |
| 20 | 5000 | 5 | 6 | 6 | 30 | 13,440 | 2,943 | 1.7s | 2.0s |

**注**：Stage 20的DMG略降是因为假设Boss战前Combo稍微下降。

#### 关键发现

**1. 击杀时间极短**
```
全20关平均T_actual：1.3秒
理想战斗时间：20-40秒
差距：15-30倍过快
```

**2. 火力增长曲线失控**
```
Stage 1→10：PDA增长16.1倍
Stage 10→20：PDA增长2.5倍
敌人HP增长：Stage 1→20仅41.7倍

玩家火力（2943） >> 敌人血量（120）的比率 = 24.5倍
```

**3. 单次爆发秒杀**
```
Stage 5-20：单次4行消除 > 敌人血量
意味着Expert玩家可以"一轮秒杀"所有Boss

Stage 10: 5,440 vs 1,000 (5.4倍过量)
Stage 20: 13,440 vs 5,000 (2.7倍过量)
```

---

## ⚠️ 当前设计的核心问题

### 问题1：设计基准错位

#### 当前状态

```
敌人HP/射速/子弹设计 → 似乎以"Expert + Volley"为隐含基准

证据：
1. Stage 20 HP=5000，恰好是Expert+Volley单次爆发(13440)的约37%
2. 射速从3.0s降至1.0s，似乎在补偿玩家的极速击杀
3. 后期子弹复杂度暴增，但对高火力玩家意义不大（瞬间击杀）
```

#### 对不同群体的影响

**对Expert + Volley（设计基准）**：
```
结果：仍然过弱
T_actual：1-2秒（目标20-40秒）
体验：无聊、缺乏挑战
问题：即使以此为基准，HP仍然不足
```

**对Expert + 其他Build**：
```
结果：显著偏弱（但仍过快）
T_actual：5-10秒
体验：略显无聊
问题：Volley的火力优势过于明显
```

**对Intermediate玩家**：
```
结果：Boss过弱，但平常关卡压力大
体验：不平衡（秒杀Boss，但路上受苦）
问题：HP曲线不适合中等火力
```

**对Casual玩家**：
```
结果：敌人"看似"合理，但压力来源错误
体验：
- 前期：勉强应付（子弹慢，类型少）
- 后期：压力来自子弹复杂度，而非HP
问题：射速/子弹设计假设玩家有高火力快速结束战斗
```

### 问题2：数值逻辑的循环矛盾

```
设计逻辑链（推测）：
1. Volley Build火力过高
2. 提升敌人射速/子弹复杂度来增加压力
3. 但高火力玩家瞬间击杀，射速/子弹无意义
4. 低火力玩家面对高射速，压力过大
5. 回到问题1

实际上应该：
1. 先确定目标战斗时长（20-40秒）
2. 根据不同Build的平均火力设计HP
3. 射速/子弹作为战术挑战，而非时间拉长手段
```

### 问题3：传奇Build的价值失衡

#### 当前价值排序（Expert视角）

```
1. Volley：价值 ★★★★★
   - 导弹数翻倍（+100%）
   - 直接影响DPS
   - 无上限（理论可无限堆）
   
2. Burst：价值 ★★★★☆
   - 高Combo时增幅巨大
   - 但依赖Volley的导弹数放大
   
3. Salvo：价值 ★★★☆☆
   - 固定加成
   - 不如Volley的乘法效果
   
4. Defense：价值 ★★☆☆☆
   - 对Expert无意义（瞬间击杀）
   - 方块反正不会被打到
   
5. Tactical：价值 ★☆☆☆☆
   - 技能解锁有用
   - 但高火力下技能使用率低
   
6. Heal：价值 ☆☆☆☆☆
   - Expert几乎不掉血
```

#### 当前价值排序（Casual视角）

```
1. Defense：价值 ★★★★★
   - 生存关键
   - 方块HP+1 = 存活时间翻倍
   
2. Tactical：价值 ★★★★☆
   - 技能是主要输出/防御手段
   - CP上限提升 = 更多技能使用
   
3. Volley：价值 ★★★☆☆
   - 火力提升
   - 但低Combo下增益有限
   
4. ResourceExpansion：价值 ★★★☆☆
   - CP上限 = 更多技能
   
5. Burst：价值 ★★☆☆☆
   - Combo太低（3-6）
   - 增益微弱
   
6. Heal：价值 ★★☆☆☆
   - 有用但不够强
```

**结论**：同一系统，不同层级玩家的最优策略完全不同，但游戏设计只照顾了一端。

---

## 🔀 三层玩家×三种Build计算

### Build路线定义

#### Build A：Volley路线（输出Build）

**核心理念**：最大化DPS，追求最短击杀时间

**升级优先级**：
```
1-10次：Volley → Lv 3
11-15次：Salvo → Lv 5
16-21次：Burst → Lv 6
22-24次：Volley → Lv 5
25-30次：混合强化
31-32次：传奇补充
```

**典型配置（Stage 20）**：
```
Volley: 5
Salvo: 6
Burst: 6
Defense: 0-1
Tactical: 0
ResourceExpansion: 0-1
```

#### Build B：Defense路线（坦克Build）

**核心理念**：最大化生存，通过持久战磨死敌人

**升级优先级**：
```
1-8次：Defense → Lv 8
9-12次：ResourceExpansion → Lv 3
13-14次：Tactical → Lv 2
15-20次：Salvo → Lv 4
21-26次：Burst → Lv 4
27-30次：Volley → Lv 2
31-32次：补充
```

**典型配置（Stage 20）**：
```
Defense: 8
Tactical: 2（技能解锁）
ResourceExpansion: 3（CP=250）
Salvo: 4
Burst: 4
Volley: 2
```

#### Build C：Tactical路线（战术Build）

**核心理念**：依赖技能输出，稳定版面控制

**升级优先级**：
```
1-2次：Tactical → Lv 2（优先解锁技能）
3-6次：ResourceExpansion → Lv 3（CP=250）
7-10次：Defense → Lv 4
11-16次：Salvo → Lv 5
17-22次：Burst → Lv 5
23-26次：Volley → Lv 2
27-32次：Defense继续 → Lv 9
```

**典型配置（Stage 20）**：
```
Tactical: 2（Execution+Repair解锁）
ResourceExpansion: 3（CP=250）
Defense: 9
Salvo: 5
Burst: 5
Volley: 2
```

### 完整计算矩阵

#### Casual × Volley Build

**理论可行性**：低（操作难度高，Combo低，Volley收益有限）

**Stage关键数据**：

| Stage | Config | C | DMG_total | PDA | T_kill | T_actual | SP | 评估 |
|-------|--------|---|-----------|-----|--------|----------|-----|------|
| 1 | 0/1/1 | 3 | 56 | 11 | 10.9s | 19.6s | 0.70 | 可行 |
| 5 | 1/2/2 | 4 | 280 | 55 | 7.3s | 13.1s | 0.62 | 可行 |
| 10 | 2/3/3 | 4.5 | 420 | 82 | 12.2s | 22.0s | 0.48 | 吃力 |
| 15 | 3/4/4 | 5 | 680 | 132 | 15.2s | 27.4s | 0.35 | 危险 |
| 20 | 5/6/6 | 5 | 1,204 | 235 | 21.3s | 38.3s | 0.25 | 极危 |

**分析**：
```
优势：后期火力勉强够用（T_actual接近理想）
劣势：
- SP持续下降（0.70 → 0.25）
- 溢出风险极高（后期>80%）
- 无Defense，方块极易被破坏
- 低Combo下Volley收益递减

结论：不适合Casual玩家（生存压力过大）
```

#### Casual × Defense Build

**理论可行性**：高（生存优先，符合低技能特点）

**Stage关键数据**：

| Stage | Config | C | DMG_total | PDA | T_kill | T_actual | SP | 溢出率 |
|-------|--------|---|-----------|-----|--------|----------|-----|--------|
| 1 | 0/1/1/D2 | 3 | 56 | 11 | 10.9s | 19.6s | 0.78 | 8% |
| 5 | 0/2/2/D4 | 4 | 120 | 23 | 17.4s | 31.3s | 0.72 | 12% |
| 10 | 1/3/3/D6 | 4.5 | 240 | 47 | 21.3s | 38.3s | 0.65 | 18% |
| 15 | 2/4/4/D8 | 5 | 420 | 82 | 24.4s | 43.9s | 0.58 | 25% |
| 20 | 2/4/4/D9 | 5 | 520 | 101 | 49.5s | 89.1s | 0.52 | 30% |

**分析**：
```
优势：
- SP稳定在0.5-0.8（安全/警戒区）
- 溢出率控制在30%以内
- 方块HP增加 = 存活时间翻倍
- Tactical技能可用 = 主动防御手段

劣势：
- Stage 20战斗时间过长（89秒 >> 40秒）
- 火力严重不足（需要近2分钟）
- 但对Casual玩家，长时间≠难（有足够时间思考）

结论：适合Casual玩家（稳定>速度）
```

#### Casual × Tactical Build

**理论可行性**：中高（技能依赖，需要学习技能时机）

**Stage关键数据**：

| Stage | Config | C | 普通DMG | 技能DMG | PDA_total | T_kill | T_actual | SP |
|-------|--------|---|---------|---------|-----------|--------|----------|-----|
| 1 | 0/1/1/D2/T2 | 3 | 56 | 20 | 16 | 7.5s | 13.5s | 0.75 |
| 5 | 0/2/2/D4/T2 | 4 | 120 | 45 | 36 | 11.1s | 20.0s | 0.70 |
| 10 | 1/3/3/D6/T2 | 4.5 | 240 | 80 | 70 | 14.3s | 25.7s | 0.62 |
| 15 | 2/4/4/D8/T2 | 5 | 420 | 140 | 122 | 16.4s | 29.5s | 0.55 |
| 20 | 2/5/5/D9/T2 | 5 | 640 | 180 | 186 | 26.9s | 48.4s | 0.50 |

**PDA_total计算**：
```
PDA_total = PDA_normal + PDA_skill

PDA_skill = (CP_max / avg_CP_cost) / avg_battle_time × avg_skill_damage

Stage 20范例：
CP_max = 250（Resource Lv3）
avg_CP_cost = 15（混合Execution 5 + Repair 30）
avg_battle_time = 48s
avg_skill_damage = 180

PDA_skill ≈ (250/15)/48 × 180 ≈ 62

PDA_total = 124 + 62 = 186
```

**分析**：
```
优势：
- 技能提供额外40-50%输出
- Execution清理顶部 = 降低溢出风险
- Repair填补空洞 = 创造消除机会
- SP稳定在0.50-0.75

劣势：
- 需要学习技能使用时机
- CP管理复杂度高
- Stage 20仍然偏慢（48秒 > 40秒）

结论：适合"高级Casual"玩家（愿意学习技能）
```

#### Intermediate × Volley Build

**理论可行性**：高（中等Combo，Volley收益显著）

**Stage关键数据**：

| Stage | Config | C | DMG_total | PDA | T_kill | T_actual | SP |
|-------|--------|---|-----------|-----|--------|----------|-----|
| 1 | 0/1/1 | 8 | 131 | 33 | 3.6s | 5.0s | 0.72 |
| 5 | 1/2/2 | 10 | 750 | 188 | 2.1s | 3.0s | 0.65 |
| 10 | 3/3/3 | 10 | 1,500 | 375 | 2.7s | 3.8s | 0.55 |
| 15 | 4/5/5 | 11 | 3,200 | 800 | 2.5s | 3.5s | 0.45 |
| 20 | 5/6/6 | 12 | 4,800 | 1,200 | 4.2s | 5.9s | 0.38 |

**分析**：
```
优势：
- 火力强大（Stage 20 仅6秒）
- Combo适中（10-12）使Volley收益最大化
- 操作难度适中

劣势：
- SP偏低（0.38，危险区）
- 缺Defense，版面压力大
- 战斗时间过短（缺乏挑战感）

结论：过于简单，不平衡（需要提升难度）
```

#### Intermediate × Defense Build

**理论可行性**：中（偏保守，不适合中等技能）

**Stage关键数据**：

| Stage | Config | C | DMG_total | PDA | T_kill | T_actual | SP |
|-------|--------|---|-----------|-----|--------|----------|-----|
| 1 | 0/1/1/D2 | 8 | 131 | 33 | 3.6s | 5.0s | 0.82 |
| 5 | 0/2/2/D4 | 10 | 300 | 75 | 5.3s | 7.4s | 0.78 |
| 10 | 1/3/3/D6 | 10 | 500 | 125 | 8.0s | 11.2s | 0.72 |
| 15 | 2/4/4/D8 | 11 | 900 | 225 | 8.9s | 12.5s | 0.68 |
| 20 | 2/4/4/D9 | 12 | 1,080 | 270 | 18.5s | 25.9s | 0.65 |

**分析**：
```
优势：
- SP极高（0.65-0.82，安全）
- 溢出率极低（<10%）
- 容错率高

劣势：
- 火力牺牲过大（Stage 20 需26秒）
- Intermediate玩家不需要这么高的Defense
- 战斗时间接近理想但略显保守

结论：可行但非最优（过度防御）
```

#### Intermediate × Tactical Build

**理论可行性**：高（最平衡的选择）

**Stage关键数据**：

| Stage | Config | C | 普通DMG | 技能DMG | PDA_total | T_kill | T_actual | SP |
|-------|--------|---|---------|---------|-----------|--------|----------|-----|
| 1 | 0/1/1/D2/T2 | 8 | 131 | 35 | 47 | 2.6s | 3.6s | 0.78 |
| 5 | 1/2/2/D4/T2 | 10 | 360 | 80 | 110 | 3.6s | 5.0s | 0.73 |
| 10 | 1/3/3/D5/T2 | 10 | 600 | 140 | 213 | 4.7s | 6.6s | 0.65 |
| 15 | 2/4/4/D7/T2 | 11 | 1,100 | 220 | 392 | 5.1s | 7.1s | 0.58 |
| 20 | 2/5/5/D8/T2 | 12 | 1,440 | 280 | 513 | 9.7s | 13.6s | 0.55 |

**分析**：
```
优势：
- 火力+生存平衡
- SP稳定在0.55-0.78
- 技能提供战术灵活性
- 战斗时间合理（13.6秒，略快但可接受）

劣势：
- 仍然略快（理想20-40秒）
- 需要技能操作

结论：最适合Intermediate的Build（平衡）
```

#### Expert × Volley Build

**已在第一节详细分析，此处汇总**：

```
Stage 20数据：
Config: 5/6/6
C: 30
DMG_total: 13,440
PDA: 2,943
T_kill: 1.7s
T_actual: 2.0s
SP: 0.32（但无意义，瞬间击杀）

结论：极度失衡，Boss无挑战性
```

#### Expert × Defense Build

**理论可行性**：低（Expert不需要Defense）

**Stage关键数据**：

| Stage | Config | C | DMG_total | PDA | T_kill | T_actual | SP |
|-------|--------|---|-----------|-----|--------|----------|-----|
| 1 | 0/1/1/D2 | 20 | 339 | 74 | 1.6s | 1.9s | 0.88 |
| 5 | 1/2/2/D4 | 24 | 1,200 | 263 | 1.5s | 1.8s | 0.85 |
| 10 | 2/3/3/D6 | 26 | 2,100 | 460 | 2.2s | 2.6s | 0.80 |
| 15 | 3/4/4/D8 | 28 | 3,800 | 833 | 2.4s | 2.9s | 0.78 |
| 20 | 3/4/4/D9 | 30 | 4,320 | 947 | 5.3s | 6.4s | 0.75 |

**分析**：
```
优势：
- SP极高（0.75-0.88，完美安全）
- 方块几乎不可破坏
- 零溢出风险

劣势：
- 火力大幅牺牲（但仍然很快，6.4秒）
- Defense对Expert纯属浪费
- 战斗仍然过快

结论：不适合Expert（浪费潜力）
```

#### Expert × Tactical Build

**理论可行性**：中（技能对Expert意义有限）

**Stage关键数据**：

| Stage | Config | C | 普通DMG | 技能DMG | PDA_total | T_kill | T_actual | SP |
|-------|--------|---|---------|---------|-----------|--------|----------|-----|
| 1 | 0/1/1/D2/T2 | 20 | 339 | 40 | 88 | 1.4s | 1.6s | 0.80 |
| 5 | 1/2/2/D3/T2 | 24 | 1,200 | 100 | 294 | 1.4s | 1.6s | 0.75 |
| 10 | 2/3/3/D4/T2 | 26 | 2,100 | 180 | 534 | 1.9s | 2.3s | 0.68 |
| 15 | 3/4/4/D6/T2 | 28 | 3,800 | 280 | 971 | 2.1s | 2.5s | 0.62 |
| 20 | 3/5/5/D7/T2 | 30 | 5,760 | 350 | 1,470 | 3.4s | 4.1s | 0.58 |

**分析**：
```
优势：
- 技能提供额外灵活性
- SP较好（0.58-0.80）
- 战斗时间仍然很快（4.1秒）

劣势：
- Expert很少需要技能（火力足够）
- Tactical投资回报率低
- 战斗仍过快

结论：次优选择（不如Volley）
```

---

## 📊 Build路线数学角色

### Volley路线（输出Build）

#### 数学特性

**核心公式**：
```
PDA_volley = λ_clear × r × W × (1 + L_volley) × DMG_single

关键乘数：(1 + L_volley)
L_volley=0: 1×
L_volley=3: 4×
L_volley=5: 6×

增幅：线性但直接（每级+100%导弹）
```

**适用层级分析**：

```
Expert（最佳匹配）：
- 高Combo（20-40）放大Burst效果
- 高消除率（3.2行）配合Volley乘数
- 操作精准，火力完全释放
- PDA：2,943（Stage 20）

Intermediate（次优）：
- 中Combo（10-12）Burst效果减半
- 中消除率（2.5行）
- PDA：1,200（Stage 20）
- 收益：Expert的41%

Casual（不适合）：
- 低Combo（4-6）Burst效果微弱
- 低消除率（1.8行）
- PDA：235（Stage 20）
- 收益：Expert的8%
- 问题：火力不足+生存压力大
```

**合理数值区间**：

```
当前：L_volley_max = ∞（理论无上限）
问题：高层级玩家可无限堆，失衡

建议：
L_volley_max = 4（上限）
效果：
- L=0: 1×（基准）
- L=1: 2×（翻倍）
- L=2: 3×（3倍）
- L=3: 4×（4倍）
- L=4: 5×（上限）

理由：
1. L=5时Expert PDA=2943 → L=4时PDA=2354（-20%）
2. 仍然强大但不至于"一轮秒杀"
3. 迫使玩家考虑其他Build
```

### Defense路线（坦克Build）

#### 数学特性

**核心公式**：
```
SP_defense = min(1, (HP_block + L_defense) / (10 × λ_bullet))

溢出概率：
P_overflow ≈ 1 - SP_defense

方块存活时间：
T_survive = HP_block(L_defense) / λ_bullet
          = (1 + L_defense) / λ_bullet
```

**适用层级分析**：

```
Casual（最佳匹配）：
- 低火力 = 长战斗时间 = 方块承受更多子弹
- 操作失误多 = 需要容错空间
- SP提升：0.25 → 0.52（Stage 20，+108%）
- 溢出率：80% → 30%（-62.5%）
- 关键效果：从"极危"到"警戒"

Intermediate（有用但非最优）：
- 中等战斗时间，Defense有帮助
- SP提升：0.38 → 0.65（+71%）
- 但牺牲火力导致战斗时长增加

Expert（不需要）：
- 瞬间击杀，方块不会被打到
- Defense投资完全浪费
- SP提升无意义（战斗时间太短）
```

**合理数值区间**：

```
当前：L_defense_max = ∞（无上限）

建议：
L_defense_max = 10

HP_block(L_defense) = 1 + L_defense

效果（Stage 20，λ_bullet=1.0/s）：
L=0:  HP=1,  T_survive=1.0s,  SP_defense=0.10
L=3:  HP=4,  T_survive=4.0s,  SP_defense=0.40
L=6:  HP=7,  T_survive=7.0s,  SP_defense=0.70
L=10: HP=11, T_survive=11.0s, SP_defense=1.00（上限）

理由：
1. L=10时达到SP_defense=1.0（完全防御）
2. 超过10层意义不大（已经不可破坏）
3. 给予Casual足够生存空间（7-11秒/方块）
```

### Tactical路线（战术Build）

#### 数学特性

**核心公式**：

**CP上限**：
```
CP_max = 100 + 50 × L_resource
L_resource=0: 100
L_resource=3: 250（+150%）
```

**技能DPS**：
```
DPS_skill = (CP_max / T_battle) × (1 / avg_CP_cost) × avg_skill_damage

Execution：
cost=5, damage=W×(1+L_volley)×4
效率=damage/cost ∝ (1+L_volley)

Repair：
cost=30, 效果=填补空洞+触发消除
效率高度依赖版面状态
```

**总DPS**：
```
DPS_total = DPS_normal + DPS_skill

DPS_skill通常占20-40%（取决于CP管理）
```

**适用层级分析**：

```
Casual（适合）：
- 技能是主要输出/防御手段
- Execution清理顶部 → 降低溢出风险
- Repair创造消除机会 → 提升火力
- CP=250时，技能使用次数：8-10次/关卡
- DPS提升：+40-60%
- 关键：需要学习技能时机

Intermediate（最佳匹配）：
- 操作能力足以管理CP
- 技能提供战术灵活性
- 平衡火力+生存
- DPS提升：+30-50%
- T_actual：13.6秒（接近理想）

Expert（有用但非最优）：
- 火力已足够，技能意义有限
- Execution可快速清场，但不如Volley暴力
- DPS提升：+20-30%
- 不如专精Volley
```

**合理数值区间**：

```
当前：
- Tactical_max = 2（Lv1解锁Execution，Lv2解锁Repair）
- ResourceExpansion_max = 3（CP=250）

建议：维持当前设计

理由：
1. Tactical=2恰好解锁两个技能，合理
2. CP=250时可支持8-10次技能使用/关卡
3. 再高CP会导致技能spam，失去战术意义

额外建议：
- 考虑技能CD（冷却时间）防止spam
- 或者提升技能cost但增强效果
```

### 三条路线的数学对比

#### Stage 20总结表

| 路线 | 最佳层级 | PDA | T_actual | SP | 火力 | 生存 | 战术 |
|------|---------|-----|----------|-----|------|------|------|
| Volley | Expert | 2,943 | 2.0s | 0.32 | ★★★★★ | ★☆☆☆☆ | ★☆☆☆☆ |
| Defense | Casual | 101 | 89.1s | 0.52 | ★☆☆☆☆ | ★★★★★ | ★★☆☆☆ |
| Tactical | Inter. | 513 | 13.6s | 0.55 | ★★★☆☆ | ★★★★☆ | ★★★★★ |

#### 行为分流解释

**为什么不同玩家选择不同路线**：

```
Expert选Volley：
心理：追求极限输出、挑战速通
能力：高Combo、高操作精度
需求：最短击杀时间
结果：2秒击杀，但缺乏挑战感（设计问题）

Casual选Defense：
心理：害怕死亡、追求稳定
能力：低Combo、操作失误多
需求：降低溢出风险、延长思考时间
结果：89秒击杀，但感觉安全（对他们而言合理）

Intermediate选Tactical：
心理：平衡效率与稳定
能力：中等Combo、愿意学习
需求：战术灵活性、适度挑战
结果：13秒击杀，平衡体验
```

**数学上的必然性**：

```
收益公式：
Benefit(Build, Tier) = f_power(Build) × f_skill(Tier) + f_survival(Build) × f_risk(Tier)

Expert：
- f_skill(Expert) = 高 → Volley的f_power放大
- f_risk(Expert) = 低 → Defense的f_survival无用

Casual：
- f_skill(Casual) = 低 → Volley的f_power减弱
- f_risk(Casual) = 高 → Defense的f_survival关键

Intermediate：
- f_skill(Intermediate) = 中 → 平衡型Build（Tactical）最优
- f_risk(Intermediate) = 中 → 需要适度生存投资
```

---

## 🎯 结论与建议

### 核心结论：为什么不能只用Volley来看平衡

#### 结论1：Volley是Expert专属优势

```
数学证明：

PDA_volley(tier) = λ_clear(tier) × E[r](tier) × W × (1+L_volley) × DMG_single(tier)

其中DMG_single(tier) ∝ C(tier)

展开：
PDA ∝ [λ_clear × E[r]] × (1+L_volley) × C

层级差异：
Expert:  [0.70/3.2 × 3.2] × (1+5) × 30 = 126 × (单位PDA)
Inter:   [0.50/2.5 × 2.5] × (1+5) × 10 = 60 × (单位PDA)
Casual:  [0.35/1.8 × 1.8] × (1+5) × 5  = 21 × (单位PDA)

比率：
Expert : Inter : Casual = 6 : 2.9 : 1

结论：Volley的收益随技能层级呈指数增长
```

**含义**：
- 如果只看Volley平衡，必然以Expert为基准
- 以Expert为基准 → 其他层级完全失衡
- 其他层级在Volley Build下火力严重不足

#### 结论2：当前设计隐含假设Expert+Volley

```
证据链：

1. Stage 20 HP=5000
   Expert+Volley单次爆发=13,440
   比率=2.69
   
   含义：设计者期望Expert需要2-3次消除（6-9秒）
   实际：由于操作速度，只需2秒
   
2. 射速曲线：3.0s → 1.0s（3倍加速）
   如果玩家20秒击杀 → 射弹数 = 20/1.0 = 20发
   如果玩家2秒击杀 → 射弹数 = 2/1.0 = 2发
   
   含义：高射速设计假设长战斗时间
   实际：Expert瞬间击杀，射速无意义
   
3. 后期子弹复杂度（8种类型）
   复杂子弹设计目的：增加战术挑战
   实际：Expert击杀太快，子弹来不及飞到
   
   含义：子弹设计假设玩家需要应对
   实际：Expert根本不需要应对

结论：整个后期设计都以"高火力"为隐含假设
```

#### 结论3：Build多样性是平衡的必要条件

```
系统方程：

设目标战斗时间 T_ideal = 30秒（取中值）
设敌人HP = H

平衡条件：
PDA(tier, build) × T_ideal ≈ H

如果只有Volley Build：
PDA_volley(Expert) = 2943
PDA_volley(Inter) = 1200
PDA_volley(Casual) = 235

所需HP：
Expert:  H = 2943 × 30 = 88,290
Inter:   H = 1200 × 30 = 36,000
Casual:  H = 235 × 30 = 7,050

问题：无法用单一HP满足三个层级
比率：88,290 : 36,000 : 7,050 = 12.5 : 5.1 : 1

如果有三种Build：
Volley(Expert):  PDA=2943, H=88,290
Tactical(Inter): PDA=513,  H=15,390
Defense(Casual): PDA=101,  H=3,030

虽然仍有差距，但可以通过难度模式解决：
- 专家模式：H=88,000（Volley/Expert）
- 标准模式：H=15,000（Tactical/Inter）
- 休闲模式：H=3,000（Defense/Casual）

结论：Build多样性 = 平衡可行性
```

### Defense/Tactical的数学角色

#### Defense的角色：时间缓冲器（Time Buffer）

**数学定义**：
```
T_buffer(L_defense) = (1 + L_defense) / λ_bullet

物理意义：方块被破坏前的存活时间

作用：
- 给予低技能玩家更多思考时间
- 降低溢出概率（方块存活 = 空间稳定）
- 转化长战斗时间为容错空间
```

**在整体设计中的位置**：
```
没有Defense：
Casual面对Stage 20 → 战斗89秒 → 方块被击中89次 → 全部破坏 → 溢出 → 死亡

有Defense（L=9）：
同样战斗89秒 → 方块被击中89次 → 每个方块存活10次击中 → 可承受 → 稳定
```

**合理数值区间**：
```
T_buffer应该覆盖整个战斗时长的10-20%

Stage 20（Casual）：
T_battle = 90秒
T_buffer_ideal = 9-18秒
λ_bullet = 1.0/秒

所需Defense：
L_defense = T_buffer × λ_bullet - 1
          = 15 × 1.0 - 1
          = 14

当前最大Defense（理论无上限）：可以达到14
建议上限：10（覆盖约11秒，足够）

理由：L=10已经可以让Casual在90秒战斗中存活
```

#### Tactical的角色：非线性爆发源（Nonlinear Burst Source）

**数学定义**：
```
DPS_tactical = DPS_base + DPS_skill

DPS_skill = λ_skill × DMG_skill

特点：
- DPS_base：持续但稳定
- DPS_skill：瞬间但间歇
- λ_skill：玩家主动控制（非自动）
```

**非线性特性**：
```
普通Build：DPS随时间线性增长
Tactical Build：DPS有周期性爆发

示例（Intermediate，Stage 20）：
DPS_base = 360（持续）
DPS_skill = 0（待充能）
CP充满（10秒后） → 使用Execution → 瞬间+200伤害
DPS_skill瞬时 = ∞（理论）
DPS_skill平均 = 200/10 = 20

总DPS = 360 + 20 = 380（平均）

但实际体验：
- 有10秒的"蓄力期"
- 有技能的"爆发期"
- 战术节奏感强
```

**在整体设计中的位置**：
```
Volley Build：火力恒定高，无节奏
Defense Build：火力恒定低，持久战
Tactical Build：火力波动，有节奏

对应玩家偏好：
Expert：喜欢"持续高压"（Volley）
Casual：喜欢"稳扎稳打"（Defense）
Intermediate：喜欢"战术节奏"（Tactical）
```

**合理数值区间**：
```
技能DPS占比：
太低（<20%）：技能无意义
太高（>60%）：变成"技能spam"，失去战术性
理想：30-40%

当前（Intermediate，Tactical Build）：
DPS_base = 360
DPS_skill = 153
占比 = 153/(360+153) = 29.8%

评估：合理 ✓

建议：维持当前设计
- Tactical Lv2（解锁2技能）
- ResourceExpansion Lv3（CP=250）
- 不需要调整
```

### 最终建议：三轨制平衡设计

#### 建议1：明确Build分流设计

```
不再尝试"单一平衡"，而是"三轨平衡"：

轨道1：输出轨（Volley Build）
目标玩家：Expert
设计目标：极限火力，最短T_kill
难度模式：专家模式（HP×7.0）
平衡目标：T_actual = 15-25秒

轨道2：坦克轨（Defense Build）
目标玩家：Casual
设计目标：最高生存，最低溢出率
难度模式：休闲模式（HP×0.6）
平衡目标：T_actual = 30-50秒（对他们合理）

轨道3：战术轨（Tactical Build）
目标玩家：Intermediate
设计目标：平衡+节奏
难度模式：标准模式（HP×1.8）
平衡目标：T_actual = 20-35秒
```

#### 建议2：Build引导系统

```
游戏开始时，提供"Build倾向测试"：

问题1：你喜欢的游戏节奏？
A. 快速果断（→ Volley）
B. 稳健持久（→ Defense）
C. 战术灵活（→ Tactical）

问题2：你的俄罗斯方块经验？
A. 老手（→ Volley）
B. 新手（→ Defense）
C. 中等（→ Tactical）

问题3：你更重视？
A. 输出伤害（→ Volley）
B. 不要死（→ Defense）
C. 技能运用（→ Tactical）

根据答案推荐难度+Build路线
```

#### 建议3：Volley上限调整

```
当前问题：Volley无上限 → Expert过强

建议：
1. 硬上限：L_volley_max = 4
2. 或递减收益：
   
   N_missiles(L_volley) = r × W × f_volley(L_volley)
   
   当前：f_volley(L) = 1 + L（线性）
   建议：f_volley(L) = 1 + L × [1/(1+0.1L)]（递减）
   
   效果：
   L=0: f=1.00 (1×)
   L=1: f=1.91 (1.91×)
   L=2: f=2.67 (2.67×)
   L=3: f=3.31 (3.31×)
   L=4: f=3.85 (3.85×)
   L=5: f=4.33 (4.33×)
   
   对比线性：
   L=5线性：6×
   L=5递减：4.33×
   削减：-28%
```

#### 建议4：Defense/Tactical数值锁定

```
Defense：
- 上限L=10
- HP_block = 1 + L
- 理由：L=10时T_buffer=11秒，足够Casual使用

Tactical：
- 维持L=2（两技能解锁）
- ResourceExpansion L=3（CP=250）
- 不需要改动
```

---

## 📚 交叉引用

**基于**：
- ← [02_Combat_Formulas.md](02_Combat_Formulas.md) - 傷害公式
- ← [04_Difficulty_Model.md](04_Difficulty_Model.md) - PDA、SP模型
- ← [06_Balance_Analysis.md](06_Balance_Analysis.md) - 平衡分析
- ← [07_Skill_Tiers_Model.md](07_Skill_Tiers_Model.md) - 三层玩家模型

**影响**：
- → 游戏设计：Build分流系统
- → 难度设计：三轨制平衡
- → 数值调整：Volley上限、Defense上限

**代码影响**：
- `GameConstants.cs`: Volley上限、Defense上限
- `BuffDataSO.cs`: Build引导权重
- `GameManager.cs`: Build推荐系统
- `DifficultySettings.cs`: 三轨难度配置

---

**文档状态**: ✅ 完整  
**核心论证**: ⭐ Volley单一视角的失败  
**替代方案**: ⭐ 三轨制平衡设计  
**实施优先级**: ★★★★★

---

**关键要点**：

**不能只用Volley看平衡的三大原因**：
1. **数学原因**：Volley收益呈指数增长，层级差距6:2.9:1
2. **设计原因**：当前隐含假设Expert+Volley，导致其他情况失衡
3. **玩家原因**：不同层级玩家需要不同Build，单一视角无法满足

**Defense/Tactical的数学角色**：
- **Defense = 时间缓冲器**：转化长战斗时间为容错空间
- **Tactical = 非线性爆发源**：提供战术节奏和灵活性

**合理数值区间**：
- **Volley上限**：4（或递减公式，28%削减）
- **Defense上限**：10（T_buffer=11秒）
- **Tactical设计**：维持当前（技能DPS占30%）

