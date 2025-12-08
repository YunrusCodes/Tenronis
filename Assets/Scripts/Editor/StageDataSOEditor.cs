using UnityEngine;
using UnityEditor;
using Tenronis.ScriptableObjects;

namespace Tenronis.Editor
{
    /// <summary>
    /// </summary>
    [CustomEditor(typeof(StageDataSO))]
    public class StageDataSOEditor : UnityEditor.Editor
    {
        private float lastDifficulty = 0f;
        private string difficultyRating = "";
        private Color difficultyColor = Color.white;
        
        public override void OnInspectorGUI()
        {
            // 计算难度
            StageDataSO data = (StageDataSO)target;
            float difficulty = CalculateDifficulty(data);
            lastDifficulty = difficulty;
            
            // 确定难度评级
            UpdateDifficultyRating(difficulty);
            
            // === 在顶部显示难度信息 ===
            EditorGUILayout.Space(5);
            
            // 显示难度信息（大标题）
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            
            EditorGUILayout.LabelField("🎯 关卡难度分析", titleStyle);
            EditorGUILayout.Space(5);
            
            // 难度分数（彩色大字）
            GUIStyle scoreStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 24,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = difficultyColor }
            };
            
            EditorGUILayout.LabelField($"{difficulty:F1} / 100", scoreStyle);
            
            // 难度评级（彩色）
            GUIStyle ratingStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = difficultyColor }
            };
            
            EditorGUILayout.LabelField(difficultyRating, ratingStyle);
            EditorGUILayout.Space(10);
            
            // 详细数据
            DrawDetailedStats(data, difficulty);
            
            // 添加说明
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "难度评分公式 2.0：\n" +
                "• 攻击密度 (0-20) = (1 / 射击间隔) × 8 + 子弹速度 × 0.8\n" +
                "• 技能压制 (0-70) = Σ(技能概率 × 危险度 × 10) + 智能瞄准(+15)\n" +
                "  威胁度排序: 插入行(12) > 范围(10) > 爆炸块(8) > 普通块(5) > 腐化爆(4) > 腐化虚(3) > 普通(1)\n" +
                "• 战斗长度 (0-10) = Clamp((敌人HP / 3000) × 10, 0, 10)\n" +
                "• 总难度 (0-100) = 攻击密度 + 技能压制 + 战斗长度\n" +
                "注：連發功能已移除，固定為 1",
                MessageType.Info
            );
            
            // 添加分隔线
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space(10);
            
            // === 在底部绘制默认 Inspector ===
            DrawDefaultInspector();
        }
        
        /// <summary>
        /// 计算关卡难度 2.0 - 更准确的难度评估
        /// </summary>
        private float CalculateDifficulty(StageDataSO data)
        {
            // === 1. 攻击密度 (0-20分) ===
            // 注意：連發功能已移除，固定為 1
            float attackScore = (1f / data.shootInterval) * 8f + data.bulletSpeed * 0.8f;
            attackScore = Mathf.Clamp(attackScore, 0f, 20f);

            // === 2. 技能压制 (0-70分) ===
            // 技能危险度权重
            const float DANGER_ADD_ROW = 12f;
            const float DANGER_AREA = 10f;
            const float DANGER_ADD_EXPLOSIVE = 8f;
            const float DANGER_ADD_BLOCK = 5f;
            const float DANGER_CORRUPT_EXPLOSIVE = 4f;
            const float DANGER_CORRUPT_VOID = 3f;
            const float DANGER_NORMAL = 1f;
            
            float skillScore = 0f;
            
            // 计算各技能的威胁度（每次只会触发一种技能）
            if (data.addRowBullet.enabled)
                skillScore += data.addRowBullet.chance * DANGER_ADD_ROW * 10f;
            
            if (data.addVoidRowBullet.enabled)
                skillScore += data.addVoidRowBullet.chance * DANGER_ADD_ROW * 10f; // 虚无行同等威胁
            
            if (data.areaBullet.enabled)
                skillScore += data.areaBullet.chance * DANGER_AREA * 10f;
            
            if (data.addExplosiveBlockBullet.enabled)
                skillScore += data.addExplosiveBlockBullet.chance * DANGER_ADD_EXPLOSIVE * 10f;
            
            if (data.addBlockBullet.enabled)
                skillScore += data.addBlockBullet.chance * DANGER_ADD_BLOCK * 10f;
            
            if (data.corruptExplosiveBullet.enabled)
                skillScore += data.corruptExplosiveBullet.chance * DANGER_CORRUPT_EXPLOSIVE * 10f;
            
            if (data.corruptVoidBullet.enabled)
                skillScore += data.corruptVoidBullet.chance * DANGER_CORRUPT_VOID * 10f;
            
            // 普通子弹的基础威胁（兜底值）
            float normalChance = 1f;
            if (data.addRowBullet.enabled) normalChance -= data.addRowBullet.chance;
            if (data.addVoidRowBullet.enabled) normalChance -= data.addVoidRowBullet.chance;
            if (data.areaBullet.enabled) normalChance -= data.areaBullet.chance;
            if (data.addExplosiveBlockBullet.enabled) normalChance -= data.addExplosiveBlockBullet.chance;
            if (data.addBlockBullet.enabled) normalChance -= data.addBlockBullet.chance;
            if (data.corruptExplosiveBullet.enabled) normalChance -= data.corruptExplosiveBullet.chance;
            if (data.corruptVoidBullet.enabled) normalChance -= data.corruptVoidBullet.chance;
            normalChance = Mathf.Max(0f, normalChance);
            skillScore += normalChance * DANGER_NORMAL * 10f;
            
            // SmartTargeting 额外 +15 分
            if (data.useSmartTargeting)
            {
                skillScore += 15f;
            }
            
            skillScore = Mathf.Clamp(skillScore, 0f, 70f);

            // === 3. 战斗长度 (0-10分) ===
            float timeScore = Mathf.Clamp((data.maxHp / 3000f) * 10f, 0f, 10f);

            // === 总难度 (0-100分) ===
            float difficulty = attackScore + skillScore + timeScore;

            return Mathf.Clamp(difficulty, 0, 100);
        }
        
        /// <summary>
        /// 更新难度评级 (基于 0-100 分制)
        /// </summary>
        private void UpdateDifficultyRating(float difficulty)
        {
            if (difficulty < 20)
            {
                difficultyRating = "⭐ 新手友好 (Tutorial)";
                difficultyColor = new Color(0.5f, 0.8f, 1f); // 淡蓝色
            }
            else if (difficulty < 35)
            {
                difficultyRating = "⭐⭐ 简单 (Easy)";
                difficultyColor = new Color(0.3f, 0.9f, 0.5f); // 绿色
            }
            else if (difficulty < 50)
            {
                difficultyRating = "⭐⭐⭐ 普通 (Normal)";
                difficultyColor = new Color(1f, 0.8f, 0.2f); // 黄色
            }
            else if (difficulty < 65)
            {
                difficultyRating = "⭐⭐⭐⭐ 困难 (Hard)";
                difficultyColor = new Color(1f, 0.5f, 0.1f); // 橙色
            }
            else if (difficulty < 80)
            {
                difficultyRating = "⭐⭐⭐⭐⭐ 极难 (Very Hard)";
                difficultyColor = new Color(1f, 0.2f, 0.2f); // 红色
            }
            else if (difficulty < 90)
            {
                difficultyRating = "💀 噩梦 (Nightmare)";
                difficultyColor = new Color(0.6f, 0f, 0.8f); // 紫色
            }
            else
            {
                difficultyRating = "☠️ 地狱 (Hell)";
                difficultyColor = new Color(0.8f, 0f, 0f); // 深红色
            }
        }
        
        /// <summary>
        /// 绘制详细统计 (2.0 版本)
        /// </summary>
        private void DrawDetailedStats(StageDataSO data, float totalDifficulty)
        {
            // === 重新计算各个组成部分（与 CalculateDifficulty 保持一致）===
            
            // 1. 攻击密度 (0-20)
            // 注意：連發功能已移除，固定為 1
            float attack = (1f / data.shootInterval) * 8f + data.bulletSpeed * 0.8f;
            attack = Mathf.Clamp(attack, 0f, 20f);
            
            // 2. 技能压制 (0-70)
            const float DANGER_ADD_ROW = 12f;
            const float DANGER_AREA = 10f;
            const float DANGER_ADD_EXPLOSIVE = 8f;
            const float DANGER_ADD_BLOCK = 5f;
            const float DANGER_CORRUPT_EXPLOSIVE = 4f;
            const float DANGER_CORRUPT_VOID = 3f;
            const float DANGER_NORMAL = 1f;
            
            float skillScore = 0f;
            
            if (data.addRowBullet.enabled)
                skillScore += data.addRowBullet.chance * DANGER_ADD_ROW * 10f;
            if (data.addVoidRowBullet.enabled)
                skillScore += data.addVoidRowBullet.chance * DANGER_ADD_ROW * 10f;
            if (data.areaBullet.enabled)
                skillScore += data.areaBullet.chance * DANGER_AREA * 10f;
            if (data.addExplosiveBlockBullet.enabled)
                skillScore += data.addExplosiveBlockBullet.chance * DANGER_ADD_EXPLOSIVE * 10f;
            if (data.addBlockBullet.enabled)
                skillScore += data.addBlockBullet.chance * DANGER_ADD_BLOCK * 10f;
            if (data.corruptExplosiveBullet.enabled)
                skillScore += data.corruptExplosiveBullet.chance * DANGER_CORRUPT_EXPLOSIVE * 10f;
            if (data.corruptVoidBullet.enabled)
                skillScore += data.corruptVoidBullet.chance * DANGER_CORRUPT_VOID * 10f;
            
            float normalChance = 1f;
            if (data.addRowBullet.enabled) normalChance -= data.addRowBullet.chance;
            if (data.addVoidRowBullet.enabled) normalChance -= data.addVoidRowBullet.chance;
            if (data.areaBullet.enabled) normalChance -= data.areaBullet.chance;
            if (data.addExplosiveBlockBullet.enabled) normalChance -= data.addExplosiveBlockBullet.chance;
            if (data.addBlockBullet.enabled) normalChance -= data.addBlockBullet.chance;
            if (data.corruptExplosiveBullet.enabled) normalChance -= data.corruptExplosiveBullet.chance;
            if (data.corruptVoidBullet.enabled) normalChance -= data.corruptVoidBullet.chance;
            normalChance = Mathf.Max(0f, normalChance);
            skillScore += normalChance * DANGER_NORMAL * 10f;
            
            float skillScoreBeforeSmart = skillScore;
            if (data.useSmartTargeting)
                skillScore += 15f;
            
            skillScore = Mathf.Clamp(skillScore, 0f, 70f);
            
            // 3. 战斗长度 (0-10)
            float time = Mathf.Clamp((data.maxHp / 3000f) * 10f, 0f, 10f);
            
            // 绘制分类统计
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("📊 难度组成 (2.0)", EditorStyles.boldLabel);
            EditorGUILayout.Space(3);
            
            DrawStatBar("⚔️ 攻击密度", attack, 20f, new Color(1f, 0.3f, 0.3f), $"{attack:F1} / 20");
            DrawStatBar("💥 技能压制", skillScore, 70f, new Color(1f, 0.6f, 0.2f), $"{skillScore:F1} / 70");
            DrawStatBar("⏱️ 战斗长度", time, 10f, new Color(0.3f, 0.7f, 1f), $"{time:F1} / 10");
            
            // SmartTargeting 特殊标注
            if (data.useSmartTargeting)
            {
                EditorGUILayout.Space(3);
                GUIStyle smartStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    normal = { textColor = new Color(1f, 0.8f, 0.2f) },
                    fontStyle = FontStyle.Bold
                };
                EditorGUILayout.LabelField($"   🎯 智能瞄准加成: +15.0", smartStyle);
            }
            
            EditorGUILayout.EndVertical();
            
            // 技能威胁度列表（按危险度排序）
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("🎯 技能配置 (按威胁度排序)", EditorStyles.boldLabel);
            EditorGUILayout.Space(3);
            
            // 创建技能列表并排序
            var skills = new System.Collections.Generic.List<(string name, float chance, float danger, bool enabled)>();
            
            if (data.addRowBullet.enabled) 
                skills.Add(("插入垃圾行", data.addRowBullet.chance, DANGER_ADD_ROW, true));
            if (data.addVoidRowBullet.enabled) 
                skills.Add(("插入虚无行", data.addVoidRowBullet.chance, DANGER_ADD_ROW, true));
            if (data.areaBullet.enabled) 
                skills.Add(("范围伤害", data.areaBullet.chance, DANGER_AREA, true));
            if (data.addExplosiveBlockBullet.enabled) 
                skills.Add(("添加爆炸方块", data.addExplosiveBlockBullet.chance, DANGER_ADD_EXPLOSIVE, true));
            if (data.addBlockBullet.enabled) 
                skills.Add(("添加垃圾方块", data.addBlockBullet.chance, DANGER_ADD_BLOCK, true));
            if (data.corruptExplosiveBullet.enabled) 
                skills.Add(("腐化爆炸", data.corruptExplosiveBullet.chance, DANGER_CORRUPT_EXPLOSIVE, true));
            if (data.corruptVoidBullet.enabled) 
                skills.Add(("腐化虚无", data.corruptVoidBullet.chance, DANGER_CORRUPT_VOID, true));
            
            // 普通子弹（总是显示）
            skills.Add(("普通子弹", normalChance, DANGER_NORMAL, true));
            
            // 按危险度排序
            skills.Sort((a, b) => b.danger.CompareTo(a.danger));
            
            foreach (var skill in skills)
            {
                Color skillColor = skill.danger >= 10 ? new Color(1f, 0.3f, 0.3f) :
                                   skill.danger >= 5 ? new Color(1f, 0.6f, 0.2f) :
                                   skill.danger >= 3 ? new Color(1f, 0.8f, 0.2f) :
                                   new Color(0.7f, 0.7f, 0.7f);
                
                GUIStyle skillStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    normal = { textColor = skillColor }
                };
                
                float contribution = skill.chance * skill.danger * 10f;
                EditorGUILayout.LabelField(
                    $"  • {skill.name} (机率{skill.chance:P0}, 威胁{skill.danger:F0}, 贡献{contribution:F1})", 
                    skillStyle
                );
            }
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// 绘制统计条（支持自定义最大值和显示文本）
        /// </summary>
        private void DrawStatBar(string label, float value, float maxValue, Color barColor, string displayText = null)
        {
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField(label, GUILayout.Width(100));
            
            float percentage = maxValue > 0 ? Mathf.Clamp01(value / maxValue) : 0f;
            Rect barRect = EditorGUILayout.GetControlRect(GUILayout.Height(18));
            
            // 背景
            EditorGUI.DrawRect(barRect, new Color(0.2f, 0.2f, 0.2f));
            
            // 填充条
            Rect fillRect = new Rect(barRect.x, barRect.y, barRect.width * percentage, barRect.height);
            EditorGUI.DrawRect(fillRect, barColor);
            
            // 数值文字
            GUIStyle valueStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
                fontStyle = FontStyle.Bold
            };
            
            string text = displayText ?? $"{value:F1} ({percentage:P0})";
            EditorGUI.LabelField(barRect, text, valueStyle);
            
            EditorGUILayout.EndHorizontal();
        }
    }
}

