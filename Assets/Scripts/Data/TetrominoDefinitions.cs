using UnityEngine;

namespace Tenronis.Data
{
    /// <summary>
    /// 方塊形狀定義
    /// </summary>
    public struct TetrominoShape
    {
        public string name;
        public int[,] shape;
        public BlockColor color;
        public TetrominoType type;
        
        public TetrominoShape(string name, BlockColor color, TetrominoType type, int[,] shape)
        {
            this.name = name;
            this.color = color;
            this.type = type;
            this.shape = shape;
        }
    }
    
    /// <summary>
    /// 俄羅斯方塊形狀定義
    /// </summary>
    public static class TetrominoDefinitions
    {
        public static readonly TetrominoShape I_SHAPE = new TetrominoShape(
            "I",
            BlockColor.Cyan,
            TetrominoType.I,
            new int[,] {
                { 0, 0, 0, 0 },
                { 1, 1, 1, 1 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 }
            }
        );
        
        public static readonly TetrominoShape J_SHAPE = new TetrominoShape(
            "J",
            BlockColor.Blue,
            TetrominoType.J,
            new int[,] {
                { 1, 0, 0 },
                { 1, 1, 1 },
                { 0, 0, 0 }
            }
        );
        
        public static readonly TetrominoShape L_SHAPE = new TetrominoShape(
            "L",
            BlockColor.Orange,
            TetrominoType.L,
            new int[,] {
                { 0, 0, 1 },
                { 1, 1, 1 },
                { 0, 0, 0 }
            }
        );
        
        public static readonly TetrominoShape O_SHAPE = new TetrominoShape(
            "O",
            BlockColor.Yellow,
            TetrominoType.O,
            new int[,] {
                { 1, 1 },
                { 1, 1 }
            }
        );
        
        public static readonly TetrominoShape S_SHAPE = new TetrominoShape(
            "S",
            BlockColor.Green,
            TetrominoType.S,
            new int[,] {
                { 0, 1, 1 },
                { 1, 1, 0 },
                { 0, 0, 0 }
            }
        );
        
        public static readonly TetrominoShape T_SHAPE = new TetrominoShape(
            "T",
            BlockColor.Purple,
            TetrominoType.T,
            new int[,] {
                { 0, 1, 0 },
                { 1, 1, 1 },
                { 0, 0, 0 }
            }
        );
        
        public static readonly TetrominoShape Z_SHAPE = new TetrominoShape(
            "Z",
            BlockColor.Red,
            TetrominoType.Z,
            new int[,] {
                { 1, 1, 0 },
                { 0, 1, 1 },
                { 0, 0, 0 }
            }
        );
        
        public static TetrominoShape GetRandomTetromino()
        {
            var shapes = new[] { I_SHAPE, J_SHAPE, L_SHAPE, O_SHAPE, S_SHAPE, T_SHAPE, Z_SHAPE };
            return shapes[Random.Range(0, shapes.Length)];
        }
    }
}

