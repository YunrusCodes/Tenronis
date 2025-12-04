using UnityEngine;
using Tenronis.Data;
using Tenronis.Managers;
using Tenronis.Gameplay.Tetromino;
using Tenronis.Gameplay.Player;

namespace Tenronis.Managers
{
    /// <summary>
    /// 輸入管理器（支援 DAS + ARR）
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        [Header("設定")]
        [SerializeField] private bool useNewInputSystem = false;
        
        [Header("DAS/ARR 設定")]
        [SerializeField] private float dasDelay = 0.15f;      // DAS 延遲時間（秒）
        [SerializeField] private float arrInterval = 0.03f;   // ARR 重複間隔（秒）
        [SerializeField] private float softDropInterval = 0.05f; // 軟降速度
        
        // 水平移動狀態
        private float leftHoldTimer = 0f;
        private float rightHoldTimer = 0f;
        private bool leftDASActive = false;
        private bool rightDASActive = false;
        private float leftARRTimer = 0f;
        private float rightARRTimer = 0f;
        
        // 垂直移動狀態
        private float downHoldTimer = 0f;
        private bool downActive = false;
        
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
            HandleHorizontalMovement();
            HandleVerticalMovement();
            HandleRotation();
            HandleHardDrop();
            HandleHoldPiece();
            HandleSkills();
        }
        
        /// <summary>
        /// 處理水平移動（支援 DAS + ARR）
        /// </summary>
        private void HandleHorizontalMovement()
        {
            bool leftPressed = Input.GetKey(KeyCode.LeftArrow);
            bool rightPressed = Input.GetKey(KeyCode.RightArrow);
            
            // 左移動
            if (leftPressed)
            {
                // 首次按下
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    TetrominoController.Instance?.MoveLeft();
                    leftHoldTimer = 0f;
                    leftDASActive = false;
                    leftARRTimer = 0f;
                }
                // 按住狀態
                else
                {
                    leftHoldTimer += Time.deltaTime;
                    
                    // 達到 DAS 延遲，開始 ARR
                    if (!leftDASActive && leftHoldTimer >= dasDelay)
                    {
                        leftDASActive = true;
                        leftARRTimer = 0f;
                    }
                    
                    // ARR 連續移動
                    if (leftDASActive)
                    {
                        leftARRTimer += Time.deltaTime;
                        if (leftARRTimer >= arrInterval)
                        {
                            TetrominoController.Instance?.MoveLeft();
                            leftARRTimer = 0f;
                        }
                    }
                }
            }
            else
            {
                // 重置左移狀態
                leftHoldTimer = 0f;
                leftDASActive = false;
                leftARRTimer = 0f;
            }
            
            // 右移動
            if (rightPressed)
            {
                // 首次按下
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    TetrominoController.Instance?.MoveRight();
                    rightHoldTimer = 0f;
                    rightDASActive = false;
                    rightARRTimer = 0f;
                }
                // 按住狀態
                else
                {
                    rightHoldTimer += Time.deltaTime;
                    
                    // 達到 DAS 延遲，開始 ARR
                    if (!rightDASActive && rightHoldTimer >= dasDelay)
                    {
                        rightDASActive = true;
                        rightARRTimer = 0f;
                    }
                    
                    // ARR 連續移動
                    if (rightDASActive)
                    {
                        rightARRTimer += Time.deltaTime;
                        if (rightARRTimer >= arrInterval)
                        {
                            TetrominoController.Instance?.MoveRight();
                            rightARRTimer = 0f;
                        }
                    }
                }
            }
            else
            {
                // 重置右移狀態
                rightHoldTimer = 0f;
                rightDASActive = false;
                rightARRTimer = 0f;
            }
        }
        
        /// <summary>
        /// 處理垂直移動（軟降）- 只使用下方向鍵
        /// </summary>
        private void HandleVerticalMovement()
        {
            bool downPressed = Input.GetKey(KeyCode.DownArrow);
            
            if (downPressed)
            {
                // 首次按下
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    TetrominoController.Instance?.MoveDown();
                    downHoldTimer = 0f;
                    downActive = true;
                }
                // 按住持續下降
                else if (downActive)
                {
                    downHoldTimer += Time.deltaTime;
                    if (downHoldTimer >= softDropInterval)
                    {
                        TetrominoController.Instance?.MoveDown();
                        downHoldTimer = 0f;
                    }
                }
            }
            else
            {
                // 重置下移狀態
                downHoldTimer = 0f;
                downActive = false;
            }
        }
        
        /// <summary>
        /// 處理旋轉 - 只使用上方向鍵
        /// </summary>
        private void HandleRotation()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                TetrominoController.Instance?.Rotate();
            }
        }
        
        /// <summary>
        /// 處理硬降
        /// </summary>
        private void HandleHardDrop()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TetrominoController.Instance?.HardDrop();
            }
        }
        
        /// <summary>
        /// 處理儲存/交換方塊 (A、S、D、F)
        /// </summary>
        private void HandleHoldPiece()
        {
            if (TetrominoController.Instance == null) return;
            
            // 儲存位置 0 (按鍵 A)
            if (Input.GetKeyDown(KeyCode.A))
            {
                TetrominoController.Instance.HoldPiece(0);
            }
            
            // 儲存位置 1 (按鍵 S)
            if (Input.GetKeyDown(KeyCode.S))
            {
                TetrominoController.Instance.HoldPiece(1);
            }
            
            // 儲存位置 2 (按鍵 D)
            if (Input.GetKeyDown(KeyCode.D))
            {
                TetrominoController.Instance.HoldPiece(2);
            }
            
            // 儲存位置 3 (按鍵 F)
            if (Input.GetKeyDown(KeyCode.F))
            {
                TetrominoController.Instance.HoldPiece(3);
            }
        }
        
        /// <summary>
        /// 處理技能 (1、2、3)
        /// </summary>
        private void HandleSkills()
        {
            // 處決技能 (1)
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SkillExecutor.ExecuteExecution();
            }
            
            // 修復技能 (2)
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SkillExecutor.ExecuteRepair();
            }
            
            // 湮滅技能 (3)
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SkillExecutor.ExecuteAnnihilation();
            }
        }
    }
}

