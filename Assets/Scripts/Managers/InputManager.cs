using UnityEngine;
using Tenronis.Data;
using Tenronis.Managers;
using Tenronis.Gameplay.Tetromino;
using Tenronis.Gameplay.Player;

namespace Tenronis.Managers
{
    /// <summary>
    /// 輸入管理器
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        [Header("設定")]
        [SerializeField] private bool useNewInputSystem = false;
        
        private void Update()
        {
            if (GameManager.Instance == null) return;
            if (GameManager.Instance.CurrentState != GameState.Playing) return;
            
            HandleInput();
        }
        
        /// <summary>
        /// 處理輸入
        /// </summary>
        private void HandleInput()
        {
            // 方塊控制
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                TetrominoController.Instance?.MoveLeft();
            }
            
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                TetrominoController.Instance?.MoveRight();
            }
            
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                TetrominoController.Instance?.MoveDown();
            }
            
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                TetrominoController.Instance?.Rotate();
            }
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TetrominoController.Instance?.HardDrop();
            }
            
            // 技能
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SkillExecutor.ExecuteExecution();
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SkillExecutor.ExecuteRepair();
            }
        }
    }
}

