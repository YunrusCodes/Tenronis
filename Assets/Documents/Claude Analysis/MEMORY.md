# Tenronis 專案記憶

## 專案概要
- Unity 2D 遊戲：俄羅斯方塊 + 即時戰鬥 + Roguelike 升級
- 支援三語：繁中/英/日
- 主要源碼路徑：`Tenronis/Tenronis/Assets/Scripts/`

## 架構
- **Core**: GameEvents（靜態事件系統）、GameInitializer
- **Managers**: GameManager、GridManager、CombatManager、PlayerManager、InputManager、LanguageManager
- **Gameplay**: TetrominoController、EnemyController、Block、Bullet、Missile、SkillExecutor、PlayerVisualController
- **Data**: GameEnums、GameConstants、BlockData/PlayerStats、SRSData、TetrominoDefinitions
- **ScriptableObjects**: StageDataSO、StageSetSO、BuffDataSO

## 關鍵規則備忘
- 處決技能：清除每列**最頂部**非垃圾方塊（不是最底部）
- 虛無抵銷：只要消除行中有任何 Void 方塊 → **整次消除**不產生導彈
- 溢出：消耗 75 CP，不足則 HP→1 CP→0（不是 50% HP）
- 多項 Buff 起始等級為 1（Salvo/Burst/Counter/Explosion/SpaceExpansion）
- 詳細機制見 [mechanics.md](mechanics.md)
