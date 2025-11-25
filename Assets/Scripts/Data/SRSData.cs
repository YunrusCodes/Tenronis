using UnityEngine;

namespace Tenronis.Data
{
    /// <summary>
    /// SRS (Super Rotation System) 踢牆數據
    /// 基於標準 Tetris 規範
    /// </summary>
    public static class SRSData
    {
        // === JLSTZ 方塊的踢牆表 ===
        
        // 0° → 90° (R)
        private static readonly Vector2Int[] JLSTZ_0_TO_R = new Vector2Int[]
        {
            new Vector2Int(0, 0),   // Test 1: 原位置
            new Vector2Int(-1, 0),  // Test 2: 左移 1
            new Vector2Int(-1, 1),  // Test 3: 左移 1，上移 1
            new Vector2Int(0, -2),  // Test 4: 下移 2
            new Vector2Int(-1, -2)  // Test 5: 左移 1，下移 2
        };
        
        // 90° → 180° (R)
        private static readonly Vector2Int[] JLSTZ_R_TO_2 = new Vector2Int[]
        {
            new Vector2Int(0, 0),   // Test 1
            new Vector2Int(1, 0),   // Test 2: 右移 1
            new Vector2Int(1, -1),  // Test 3: 右移 1，下移 1
            new Vector2Int(0, 2),   // Test 4: 上移 2
            new Vector2Int(1, 2)    // Test 5: 右移 1，上移 2
        };
        
        // 180° → 270° (R)
        private static readonly Vector2Int[] JLSTZ_2_TO_L = new Vector2Int[]
        {
            new Vector2Int(0, 0),   // Test 1
            new Vector2Int(1, 0),   // Test 2: 右移 1
            new Vector2Int(1, 1),   // Test 3: 右移 1，上移 1
            new Vector2Int(0, -2),  // Test 4: 下移 2
            new Vector2Int(1, -2)   // Test 5: 右移 1，下移 2
        };
        
        // 270° → 0° (R)
        private static readonly Vector2Int[] JLSTZ_L_TO_0 = new Vector2Int[]
        {
            new Vector2Int(0, 0),   // Test 1
            new Vector2Int(-1, 0),  // Test 2: 左移 1
            new Vector2Int(-1, -1), // Test 3: 左移 1，下移 1
            new Vector2Int(0, 2),   // Test 4: 上移 2
            new Vector2Int(-1, 2)   // Test 5: 左移 1，上移 2
        };
        
        // === I 方塊的踢牆表（特殊規則）===
        
        // 0° → 90° (R)
        private static readonly Vector2Int[] I_0_TO_R = new Vector2Int[]
        {
            new Vector2Int(0, 0),   // Test 1
            new Vector2Int(-2, 0),  // Test 2: 左移 2
            new Vector2Int(1, 0),   // Test 3: 右移 1
            new Vector2Int(-2, -1), // Test 4: 左移 2，下移 1
            new Vector2Int(1, 2)    // Test 5: 右移 1，上移 2
        };
        
        // 90° → 180° (R)
        private static readonly Vector2Int[] I_R_TO_2 = new Vector2Int[]
        {
            new Vector2Int(0, 0),   // Test 1
            new Vector2Int(-1, 0),  // Test 2: 左移 1
            new Vector2Int(2, 0),   // Test 3: 右移 2
            new Vector2Int(-1, 2),  // Test 4: 左移 1，上移 2
            new Vector2Int(2, -1)   // Test 5: 右移 2，下移 1
        };
        
        // 180° → 270° (R)
        private static readonly Vector2Int[] I_2_TO_L = new Vector2Int[]
        {
            new Vector2Int(0, 0),   // Test 1
            new Vector2Int(2, 0),   // Test 2: 右移 2
            new Vector2Int(-1, 0),  // Test 3: 左移 1
            new Vector2Int(2, 1),   // Test 4: 右移 2，上移 1
            new Vector2Int(-1, -2)  // Test 5: 左移 1，下移 2
        };
        
        // 270° → 0° (R)
        private static readonly Vector2Int[] I_L_TO_0 = new Vector2Int[]
        {
            new Vector2Int(0, 0),   // Test 1
            new Vector2Int(1, 0),   // Test 2: 右移 1
            new Vector2Int(-2, 0),  // Test 3: 左移 2
            new Vector2Int(1, -2),  // Test 4: 右移 1，下移 2
            new Vector2Int(-2, 1)   // Test 5: 左移 2，上移 1
        };
        
        /// <summary>
        /// 獲取 SRS 踢牆偏移表
        /// </summary>
        /// <param name="type">方塊類型</param>
        /// <param name="fromState">當前旋轉狀態 (0-3)</param>
        /// <param name="toState">目標旋轉狀態 (0-3)</param>
        /// <returns>踢牆測試偏移數組</returns>
        public static Vector2Int[] GetKickOffsets(TetrominoType type, int fromState, int toState)
        {
            // I 方塊使用特殊踢牆表
            if (type == TetrominoType.I)
            {
                return GetIKickOffsets(fromState, toState);
            }
            
            // O 方塊不旋轉（但這個不應該被調用）
            if (type == TetrominoType.O)
            {
                return new Vector2Int[] { Vector2Int.zero };
            }
            
            // JLSTZ 方塊使用標準踢牆表
            return GetJLSTZKickOffsets(fromState, toState);
        }
        
        /// <summary>
        /// 獲取 JLSTZ 方塊的踢牆偏移
        /// </summary>
        private static Vector2Int[] GetJLSTZKickOffsets(int fromState, int toState)
        {
            // 確保狀態在 0-3 範圍內
            fromState = ((fromState % 4) + 4) % 4;
            toState = ((toState % 4) + 4) % 4;
            
            // 判斷旋轉方向
            if (fromState == 0 && toState == 1) return JLSTZ_0_TO_R;
            if (fromState == 1 && toState == 2) return JLSTZ_R_TO_2;
            if (fromState == 2 && toState == 3) return JLSTZ_2_TO_L;
            if (fromState == 3 && toState == 0) return JLSTZ_L_TO_0;
            
            // 逆時針旋轉（反向測試）
            if (fromState == 1 && toState == 0) return InvertOffsets(JLSTZ_0_TO_R);
            if (fromState == 2 && toState == 1) return InvertOffsets(JLSTZ_R_TO_2);
            if (fromState == 3 && toState == 2) return InvertOffsets(JLSTZ_2_TO_L);
            if (fromState == 0 && toState == 3) return InvertOffsets(JLSTZ_L_TO_0);
            
            // 默認返回原位置
            return new Vector2Int[] { Vector2Int.zero };
        }
        
        /// <summary>
        /// 獲取 I 方塊的踢牆偏移
        /// </summary>
        private static Vector2Int[] GetIKickOffsets(int fromState, int toState)
        {
            fromState = ((fromState % 4) + 4) % 4;
            toState = ((toState % 4) + 4) % 4;
            
            // 順時針旋轉
            if (fromState == 0 && toState == 1) return I_0_TO_R;
            if (fromState == 1 && toState == 2) return I_R_TO_2;
            if (fromState == 2 && toState == 3) return I_2_TO_L;
            if (fromState == 3 && toState == 0) return I_L_TO_0;
            
            // 逆時針旋轉
            if (fromState == 1 && toState == 0) return InvertOffsets(I_0_TO_R);
            if (fromState == 2 && toState == 1) return InvertOffsets(I_R_TO_2);
            if (fromState == 3 && toState == 2) return InvertOffsets(I_2_TO_L);
            if (fromState == 0 && toState == 3) return InvertOffsets(I_L_TO_0);
            
            return new Vector2Int[] { Vector2Int.zero };
        }
        
        /// <summary>
        /// 反轉偏移（用於逆時針旋轉）
        /// </summary>
        private static Vector2Int[] InvertOffsets(Vector2Int[] offsets)
        {
            Vector2Int[] inverted = new Vector2Int[offsets.Length];
            for (int i = 0; i < offsets.Length; i++)
            {
                inverted[i] = new Vector2Int(-offsets[i].x, -offsets[i].y);
            }
            return inverted;
        }
    }
}

