using UnityEngine;
using UnityEditor;

namespace Tenronis.ScriptableObjects.Editor
{
    /// <summary>
    /// StageDataSO 的自訂 Inspector Editor
    /// 顯示數學平衡模型的計算結果和提供自動平衡工具
    /// </summary>
    [CustomEditor(typeof(StageDataSO))]
    public class StageDataSOEditor : UnityEditor.Editor
    {
        private SerializedProperty stageName;
        private SerializedProperty stageIndex;
        private SerializedProperty isBossStage;
        
        private SerializedProperty difficultyTrack;
        private SerializedProperty autoBalance;
        
        private SerializedProperty playerPDA;
        private SerializedProperty playerSP;
        
        private SerializedProperty rewardBuffCount;
        
        private SerializedProperty maxHp;
        private SerializedProperty shootInterval;
        private SerializedProperty bulletSpeed;
        private SerializedProperty burstCount;
        
        private SerializedProperty normalBullet;
        private SerializedProperty areaBullet;
        private SerializedProperty addBlockBullet;
        private SerializedProperty addExplosiveBlockBullet;
        private SerializedProperty addRowBullet;
        private SerializedProperty addVoidRowBullet;
        private SerializedProperty corruptExplosiveBullet;
        private SerializedProperty corruptVoidBullet;
        
        private SerializedProperty useSmartTargeting;
        private SerializedProperty addBlockTargetsHigh;
        private SerializedProperty areaDamageTargetsLow;
        
        private SerializedProperty enemyIcon;
        private SerializedProperty themeColor;
        
        private bool showCalculatedValues = true;
        private bool showSkills = true;
        
        private void OnEnable()
        {
            // 綁定所有序列化屬性
            stageName = serializedObject.FindProperty("stageName");
            stageIndex = serializedObject.FindProperty("stageIndex");
            isBossStage = serializedObject.FindProperty("isBossStage");
            
            difficultyTrack = serializedObject.FindProperty("difficultyTrack");
            autoBalance = serializedObject.FindProperty("autoBalance");
            
            playerPDA = serializedObject.FindProperty("playerPDA");
            playerSP = serializedObject.FindProperty("playerSP");
            
            rewardBuffCount = serializedObject.FindProperty("rewardBuffCount");
            
            maxHp = serializedObject.FindProperty("maxHp");
            shootInterval = serializedObject.FindProperty("shootInterval");
            bulletSpeed = serializedObject.FindProperty("bulletSpeed");
            burstCount = serializedObject.FindProperty("burstCount");
            
            normalBullet = serializedObject.FindProperty("normalBullet");
            areaBullet = serializedObject.FindProperty("areaBullet");
            addBlockBullet = serializedObject.FindProperty("addBlockBullet");
            addExplosiveBlockBullet = serializedObject.FindProperty("addExplosiveBlockBullet");
            addRowBullet = serializedObject.FindProperty("addRowBullet");
            addVoidRowBullet = serializedObject.FindProperty("addVoidRowBullet");
            corruptExplosiveBullet = serializedObject.FindProperty("corruptExplosiveBullet");
            corruptVoidBullet = serializedObject.FindProperty("corruptVoidBullet");
            
            useSmartTargeting = serializedObject.FindProperty("useSmartTargeting");
            addBlockTargetsHigh = serializedObject.FindProperty("addBlockTargetsHigh");
            areaDamageTargetsLow = serializedObject.FindProperty("areaDamageTargetsLow");
            
            enemyIcon = serializedObject.FindProperty("enemyIcon");
            themeColor = serializedObject.FindProperty("themeColor");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            StageDataSO stageData = (StageDataSO)target;
            
            // ==================== 標題 ====================
            EditorGUILayout.Space(10);
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("關卡數據配置", titleStyle);
            EditorGUILayout.Space(5);
            
            // ==================== 基本資訊 ====================
            DrawSection("關卡資訊", () =>
            {
                EditorGUILayout.PropertyField(stageName, new GUIContent("關卡名稱"));
                EditorGUILayout.PropertyField(stageIndex, new GUIContent("關卡索引"));
                EditorGUILayout.PropertyField(isBossStage, new GUIContent("Boss 關卡"));
            });
            
            // ==================== 難度配置 ====================
            DrawSection("難度配置", () =>
            {
                EditorGUILayout.PropertyField(difficultyTrack, new GUIContent("難度軌道"));
                EditorGUILayout.PropertyField(autoBalance, new GUIContent("啟用自動平衡"));
                
                if (autoBalance.boolValue)
                {
                    EditorGUILayout.HelpBox(
                        "自動平衡已啟用：系統將根據 PDA 和 SP 自動計算敵人屬性。\n" +
                        "修改 PDA 或 SP 時，敵人屬性會自動更新。",
                        MessageType.Info
                    );
                }
            });
            
            // ==================== 玩家能力參數 ====================
            DrawSection("玩家能力參數", () =>
            {
                EditorGUILayout.PropertyField(playerPDA, new GUIContent("玩家 PDA", "Player Damage Availability - 玩家每秒期望輸出傷害"));
                EditorGUILayout.PropertyField(playerSP, new GUIContent("玩家 SP", "Board Stability - 板面穩定性參數（0=極危，1=安全）"));
                
                EditorGUILayout.Space(5);
                
                // 自動平衡按鈕
                GUI.backgroundColor = new Color(0.3f, 0.7f, 1f);
                if (GUILayout.Button("⚙ 應用自動平衡", GUILayout.Height(30)))
                {
                    Undo.RecordObject(stageData, "Apply Auto Balance");
                    stageData.ApplyAutoBalance(playerPDA.floatValue, playerSP.floatValue);
                    EditorUtility.SetDirty(stageData);
                }
                GUI.backgroundColor = Color.white;
            });
            
            // ==================== 計算結果（只讀） ====================
            DrawCalculatedValuesSection(stageData);
            
            // ==================== 過關獎勵 ====================
            DrawSection("過關獎勵", () =>
            {
                EditorGUILayout.PropertyField(rewardBuffCount, new GUIContent("獎勵 Buff 數量"));
            });
            
            // ==================== 敵人屬性 ====================
            DrawSection("敵人屬性", () =>
            {
                GUI.enabled = !autoBalance.boolValue;
                
                EditorGUILayout.PropertyField(maxHp, new GUIContent("最大 HP"));
                EditorGUILayout.PropertyField(shootInterval, new GUIContent("射擊間隔（秒）"));
                EditorGUILayout.PropertyField(bulletSpeed, new GUIContent("子彈速度"));
                EditorGUILayout.PropertyField(burstCount, new GUIContent("連發數量"));
                
                GUI.enabled = true;
                
                if (autoBalance.boolValue)
                {
                    EditorGUILayout.HelpBox("自動平衡已啟用，這些值由系統計算。", MessageType.Info);
                }
            });
            
            // ==================== 敵人技能 ====================
            DrawSkillsSection(stageData);
            
            // ==================== 智能射擊 ====================
            DrawSection("智能射擊系統", () =>
            {
                GUI.enabled = !autoBalance.boolValue;
                
                EditorGUILayout.PropertyField(useSmartTargeting, new GUIContent("啟用智能射擊"));
                
                if (useSmartTargeting.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(addBlockTargetsHigh, new GUIContent("AddBlock 優先高點"));
                    EditorGUILayout.PropertyField(areaDamageTargetsLow, new GUIContent("AreaDamage 優先低點"));
                    EditorGUI.indentLevel--;
                }
                
                GUI.enabled = true;
            });
            
            // ==================== 視覺 ====================
            DrawSection("視覺", () =>
            {
                EditorGUILayout.PropertyField(enemyIcon, new GUIContent("敵人圖標"));
                EditorGUILayout.PropertyField(themeColor, new GUIContent("主題顏色"));
            });
            
            serializedObject.ApplyModifiedProperties();
        }
        
        /// <summary>
        /// 繪製計算結果區塊
        /// </summary>
        private void DrawCalculatedValuesSection(StageDataSO stageData)
        {
            EditorGUILayout.Space(10);
            
            // 標題
            GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold
            };
            
            showCalculatedValues = EditorGUILayout.Foldout(showCalculatedValues, "📊 計算結果（只讀）", true, foldoutStyle);
            
            if (!showCalculatedValues) return;
            
            // 背景框
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // 難度描述
            EditorGUILayout.LabelField("數學難度等級", stageData.DifficultyDescription, EditorStyles.boldLabel);
            
            EditorGUILayout.Space(5);
            
            // 目標擊殺時間
            DrawReadOnlyField("目標擊殺時間", $"{stageData.TargetKillTime:F1} 秒", "06_Balance_Analysis.md");
            
            // 計算 HP
            Color hpColor = (stageData.maxHp == stageData.CalculatedMaxHp) ? Color.green : Color.yellow;
            DrawReadOnlyFieldColored("建議 MaxHP", stageData.CalculatedMaxHp.ToString(), "06_Balance_Analysis.md", hpColor);
            
            // 計算射速
            Color shootColor = Mathf.Approximately(stageData.shootInterval, stageData.CalculatedShootInterval) ? Color.green : Color.yellow;
            DrawReadOnlyFieldColored("建議 ShootInterval", $"{stageData.CalculatedShootInterval:F2} 秒", "04_Difficulty_Model.md", shootColor);
            
            // 計算子彈速度
            Color speedColor = Mathf.Approximately(stageData.bulletSpeed, stageData.CalculatedBulletSpeed) ? Color.green : Color.yellow;
            DrawReadOnlyFieldColored("建議 BulletSpeed", $"{stageData.CalculatedBulletSpeed:F1}", "04_Difficulty_Model.md", speedColor);
            
            // 難度倍率
            DrawReadOnlyField("難度倍率", $"{stageData.DifficultyMultiplier:F2}x", "自訂");
            
            // 子彈壓力
            DrawReadOnlyField("敵人壓力 λ_bullet", $"{stageData.BulletPressure:F3} 發/秒", "02_Combat_Formulas.md");
            
            EditorGUILayout.Space(5);
            
            // 提示信息
            if (!autoBalance.boolValue)
            {
                EditorGUILayout.HelpBox("自動平衡已關閉。黃色數值表示當前值與建議值不符。", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("自動平衡已啟用。所有值已自動計算。", MessageType.Info);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// 繪製技能區塊
        /// </summary>
        private void DrawSkillsSection(StageDataSO stageData)
        {
            EditorGUILayout.Space(10);
            
            GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold
            };
            
            showSkills = EditorGUILayout.Foldout(showSkills, "⚔ 敵人技能配置", true, foldoutStyle);
            
            if (!showSkills) return;
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            GUI.enabled = !autoBalance.boolValue;
            
            DrawEnemyAbility(normalBullet, "普通子彈", "造成 1 點傷害");
            DrawEnemyAbility(areaBullet, "範圍傷害子彈", "3x3 範圍傷害（Stage 6+）");
            DrawEnemyAbility(addBlockBullet, "添加普通方塊", "在擊中方塊上方添加垃圾方塊（Stage 8+）");
            DrawEnemyAbility(addExplosiveBlockBullet, "添加爆炸方塊", "添加的方塊被擊中時造成 5 點傷害（Stage 10+）");
            DrawEnemyAbility(addRowBullet, "插入普通垃圾行", "從底部插入不可摧毀的垃圾行（Stage 12+）");
            DrawEnemyAbility(addVoidRowBullet, "插入虛無垃圾行", "插入的垃圾行消除時不產生導彈（Stage 15+）");
            DrawEnemyAbility(corruptExplosiveBullet, "腐化爆炸方塊", "將下個方塊的隨機一格變成爆炸方塊（Stage 15+）");
            DrawEnemyAbility(corruptVoidBullet, "腐化虛無方塊", "將下個方塊的隨機一格變成虛無方塊（Stage 17+）");
            
            GUI.enabled = true;
            
            if (autoBalance.boolValue)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox(
                    "技能密度由難度倍率自動計算：\n" +
                    $"Casual: 0.5x | Standard: 1.0x | Expert: 1.6x\n" +
                    "技能啟用根據關卡進度自動控制。",
                    MessageType.Info
                );
            }
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// 繪製單個技能屬性
        /// </summary>
        private void DrawEnemyAbility(SerializedProperty ability, string label, string tooltip)
        {
            EditorGUILayout.BeginHorizontal();
            
            SerializedProperty enabled = ability.FindPropertyRelative("enabled");
            SerializedProperty chance = ability.FindPropertyRelative("chance");
            
            // 啟用開關
            EditorGUILayout.PropertyField(enabled, GUIContent.none, GUILayout.Width(15));
            
            // 標籤
            EditorGUILayout.LabelField(new GUIContent(label, tooltip), GUILayout.Width(150));
            
            // 機率滑桿
            GUI.enabled = enabled.boolValue && !autoBalance.boolValue;
            EditorGUILayout.PropertyField(chance, GUIContent.none);
            GUI.enabled = !autoBalance.boolValue;
            
            // 百分比顯示
            EditorGUILayout.LabelField($"{chance.floatValue * 100:F0}%", GUILayout.Width(40));
            
            EditorGUILayout.EndHorizontal();
        }
        
        /// <summary>
        /// 繪製區塊
        /// </summary>
        private void DrawSection(string title, System.Action drawContent)
        {
            EditorGUILayout.Space(10);
            
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12
            };
            
            EditorGUILayout.LabelField(title, headerStyle);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            drawContent();
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// 繪製只讀欄位
        /// </summary>
        private void DrawReadOnlyField(string label, string value, string source)
        {
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField(label, GUILayout.Width(150));
            
            GUI.enabled = false;
            EditorGUILayout.TextField(value);
            GUI.enabled = true;
            
            // 來源標籤
            GUIStyle sourceStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight,
                fontSize = 9
            };
            EditorGUILayout.LabelField($"[{source}]", sourceStyle, GUILayout.Width(150));
            
            EditorGUILayout.EndHorizontal();
        }
        
        /// <summary>
        /// 繪製帶顏色的只讀欄位
        /// </summary>
        private void DrawReadOnlyFieldColored(string label, string value, string source, Color color)
        {
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField(label, GUILayout.Width(150));
            
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            GUI.enabled = false;
            EditorGUILayout.TextField(value);
            GUI.enabled = true;
            GUI.backgroundColor = originalColor;
            
            GUIStyle sourceStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight,
                fontSize = 9
            };
            EditorGUILayout.LabelField($"[{source}]", sourceStyle, GUILayout.Width(150));
            
            EditorGUILayout.EndHorizontal();
        }
    }
}


