using System;
using UnityEngine;
using Tenronis.Data;

namespace Tenronis.Core
{
    /// <summary>
    /// 遊戲事件系統 - 使用靜態事件解耦各系統
    /// </summary>
    public static class GameEvents
    {
        // === 遊戲狀態事件 ===
        public static event Action<GameState> OnGameStateChanged;
        
        // === 方塊事件 ===
        public static event Action OnPieceLocked;
        public static event Action<int> OnRowsCleared; // 參數：消除的行數
        public static event Action OnGridOverflow;
        
        // === 戰鬥事件 ===
        public static event Action<float> OnMissileFired; // 參數：傷害
        public static event Action<float> OnEnemyDamaged; // 參數：傷害
        public static event Action OnEnemyDefeated;
        public static event Action<int> OnPlayerDamaged; // 參數：傷害
        
        // === Combo事件 ===
        public static event Action<int> OnComboChanged; // 參數：當前Combo數
        public static event Action OnComboReset;
        
        // === Roguelike事件 ===
        public static event Action OnBuffAvailable;
        public static event Action<BuffType> OnBuffSelected;
        
        // === UI事件 ===
        public static event Action<string, Color, Vector2> OnShowPopupText; // 文字、顏色、位置
        
        // === 音效事件 ===
        public static event Action OnPlayMissileSound;
        public static event Action OnPlayExplosionSound;
        public static event Action OnPlayRotateSound;
        public static event Action OnPlayImpactSound;
        public static event Action OnPlayCounterFireSound; // 反擊音效
        
        // 觸發方法
        public static void TriggerGameStateChanged(GameState newState) => OnGameStateChanged?.Invoke(newState);
        public static void TriggerPieceLocked() => OnPieceLocked?.Invoke();
        public static void TriggerRowsCleared(int count) => OnRowsCleared?.Invoke(count);
        public static void TriggerGridOverflow() => OnGridOverflow?.Invoke();
        public static void TriggerMissileFired(float damage) => OnMissileFired?.Invoke(damage);
        public static void TriggerEnemyDamaged(float damage) => OnEnemyDamaged?.Invoke(damage);
        public static void TriggerEnemyDefeated() => OnEnemyDefeated?.Invoke();
        public static void TriggerPlayerDamaged(int damage) => OnPlayerDamaged?.Invoke(damage);
        public static void TriggerComboChanged(int combo) => OnComboChanged?.Invoke(combo);
        public static void TriggerComboReset() => OnComboReset?.Invoke();
        public static void TriggerBuffAvailable() => OnBuffAvailable?.Invoke();
        public static void TriggerBuffSelected(BuffType type) => OnBuffSelected?.Invoke(type);
        public static void TriggerShowPopupText(string text, Color color, Vector2 position) => OnShowPopupText?.Invoke(text, color, position);
        public static void TriggerPlayMissileSound() => OnPlayMissileSound?.Invoke();
        public static void TriggerPlayExplosionSound() => OnPlayExplosionSound?.Invoke();
        public static void TriggerPlayRotateSound() => OnPlayRotateSound?.Invoke();
        public static void TriggerPlayImpactSound() => OnPlayImpactSound?.Invoke();
        public static void TriggerPlayCounterFireSound() => OnPlayCounterFireSound?.Invoke();
        
        /// <summary>
        /// 清除所有事件訂閱（場景切換時使用）
        /// </summary>
        public static void ClearAllEvents()
        {
            OnGameStateChanged = null;
            OnPieceLocked = null;
            OnRowsCleared = null;
            OnGridOverflow = null;
            OnMissileFired = null;
            OnEnemyDamaged = null;
            OnEnemyDefeated = null;
            OnPlayerDamaged = null;
            OnComboChanged = null;
            OnComboReset = null;
            OnBuffAvailable = null;
            OnBuffSelected = null;
            OnShowPopupText = null;
            OnPlayMissileSound = null;
            OnPlayExplosionSound = null;
            OnPlayRotateSound = null;
            OnPlayImpactSound = null;
            OnPlayCounterFireSound = null;
        }
    }
}

