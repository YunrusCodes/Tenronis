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
        
        private SerializedProperty rewardBuffCount;
        
        private SerializedProperty maxHp;
        private SerializedProperty shootInterval;
        private SerializedProperty bulletSpeed;
        
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
            
            rewardBuffCount = serializedObject.FindProperty("rewardBuffCount");
            
            maxHp = serializedObject.FindProperty("maxHp");
            shootInterval = serializedObject.FindProperty("shootInterval");
            bulletSpeed = serializedObject.FindProperty("bulletSpeed");
            
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
            

            // ==================== 過關獎勵 ====================
            DrawSection("過關獎勵", () =>
            {
                EditorGUILayout.PropertyField(rewardBuffCount, new GUIContent("獎勵 Buff 數量"));
            });
            
            // ==================== 敵人屬性 ====================
            DrawSection("敵人屬性", () =>
            {
                EditorGUILayout.PropertyField(maxHp, new GUIContent("最大 HP"));
                EditorGUILayout.PropertyField(shootInterval, new GUIContent("射擊間隔（秒）"));
                EditorGUILayout.PropertyField(bulletSpeed, new GUIContent("子彈速度"));
            });
            
            // ==================== 敵人技能 ====================
            DrawSkillsSection(stageData);
            
            // ==================== 智能射擊 ====================
            DrawSection("智能射擊系統", () =>
            {
                EditorGUILayout.PropertyField(useSmartTargeting, new GUIContent("啟用智能射擊"));
                
                if (useSmartTargeting.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(addBlockTargetsHigh, new GUIContent("AddBlock 優先高點"));
                    EditorGUILayout.PropertyField(areaDamageTargetsLow, new GUIContent("AreaDamage 優先低點"));
                    EditorGUI.indentLevel--;
                }
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
            
            DrawEnemyAbility(normalBullet, "普通子彈", "造成 1 點傷害");
            DrawEnemyAbility(areaBullet, "範圍傷害子彈", "3x3 範圍傷害（Stage 6+）");
            DrawEnemyAbility(addBlockBullet, "添加普通方塊", "在擊中方塊上方添加垃圾方塊（Stage 8+）");
            DrawEnemyAbility(addExplosiveBlockBullet, "添加爆炸方塊", "添加的方塊被擊中時造成 5 點傷害（Stage 10+）");
            DrawEnemyAbility(addRowBullet, "插入普通垃圾行", "從底部插入不可摧毀的垃圾行（Stage 12+）");
            DrawEnemyAbility(addVoidRowBullet, "插入虛無垃圾行", "插入的垃圾行消除時不產生導彈（Stage 15+）");
            DrawEnemyAbility(corruptExplosiveBullet, "腐化爆炸方塊", "將下個方塊的隨機一格變成爆炸方塊（Stage 15+）");
            DrawEnemyAbility(corruptVoidBullet, "腐化虛無方塊", "將下個方塊的隨機一格變成虛無方塊（Stage 17+）");
            
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
            GUI.enabled = enabled.boolValue;
            EditorGUILayout.PropertyField(chance, GUIContent.none);
            GUI.enabled = true;
            
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






