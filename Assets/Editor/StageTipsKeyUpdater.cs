using System.Collections.Generic;
using Tenronis.ScriptableObjects;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 將所有 StageDataSO 的 tips 內容，從「完整中文句子」改成對應的
/// UI_Text.csv 的 Key（例如 "[TIPS]_先觀察再落子"）。
///
/// 流程：
/// 1. 依照 UI_Text.csv 目前的 mapping 建立「中文內容 -> [TIPS]_Key」對照表
/// 2. 搜尋專案內所有 StageDataSO（Theme_0～Theme_3 都會被抓到）
/// 3. 對每個 tips[i]，如果內容在對照表內，就替換成對應的 [TIPS]_Key
/// 4. 標記資源已修改並儲存
///
/// 使用方式：
/// - 放在 Assets/Editor/ 資料夾
/// - 在 Unity 選單執行：Tools/Tenronis/Update Stage Tips To Keys
/// </summary>
public static class StageTipsKeyUpdater
{
    private static readonly Dictionary<string, string> ChineseToTipsKey = new()
    {
        // 下面的左邊是「繁中內容」，右邊是 UI_Text.csv 的 Key 欄位
        { "不必急著放置方塊，觀察戰局也是重要的一環", "[TIPS]_先觀察再落子" },
        { "不同性質的工程等級上限也不同", "[TIPS]_工程等級上限差異" },
        { "不要用腐化爆炸的方塊去反擊範圍傷害，否則後果自負", "[TIPS]_腐化爆炸警告" },
        { "反擊也算一次連發", "[TIPS]_反擊亦為連發" },
        { "方塊溢出是需要代價的，如果你無法償還，就會變得不堪一擊", "[TIPS]_溢出必有代價" },
        { "加倍火力可是用純粹的量變引起質變，畢竟恐懼源於火力不足", "[TIPS]_火力不足即恐懼" },
        { "必要的時候就把方塊堆爆吧，清掃戰場，順便來發大的", "[TIPS]_戰術性溢出" },
        { "必要的時候就把方塊堆爆吧，清掃戰場，順便給那怪物來發大的", "[TIPS]_溢出清場重擊" },
        { "用怪物的攻擊幫你打掉虛無方塊吧", "[TIPS]_借敵破虛無" },
        { "如果虛無方塊讓你一直無法攻擊，不妨改用反擊對付", "[TIPS]_虛無時改用反擊" },
        { "如果範圍攻擊命中垃圾行方塊，城堡會承受加倍損害", "[TIPS]_範圍傷害垃圾懲罰" },
        { "成功造出修補那你可幸運了，它可以讓你狠狠制裁怪物", "[TIPS]_修補是轉捩點" },
        { "有些執炮人會用湮滅來閃腐化方塊", "[TIPS]_湮滅閃避腐化" },
        { "你可以一直反擊，但是戰場一定會超亂", "[TIPS]_反擊帶來混亂" },
        { "你可以不斷旋轉方塊來拉延戰局", "[TIPS]_旋轉拖延戰局" },
        { "把方塊硬降在子彈的落點觸發反擊吧", "[TIPS]_硬降觸發反擊" },
        { "垃圾行的方塊如果受到傷害會直接造成城堡損毀", "[TIPS]_垃圾行直傷城堡" },
        { "垃圾行要透過完整排列整排方塊來消除", "[TIPS]_垃圾行消除規則" },
        { "空間擴充能解鎖按鍵S、D、F對應的儲存槽", "[TIPS]_空間擴充儲存鍵" },
        { "按鍵A使用儲存槽，將現有的方塊放入空的槽位，或是與槽位內的方塊進行交換", "[TIPS]_儲存槽交換操作" },
        { "消到虛無方塊會讓你直接無法攻擊", "[TIPS]_虛無封鎖攻擊" },
        { "能使用湮滅或處決的話，消除被怪物亂加的方塊是種策略", "[TIPS]_主動技能控場" },
        { "能使用湮滅的話用來閃避腐化方塊吧", "[TIPS]_湮滅迴避腐化" },
        { "湮滅、處決、修補可能讓你無法償還方塊溢出的代價，這時就會變得不堪一擊", "[TIPS]_主動技能溢出風險" },
        { "等待怪物用添加方塊的子彈幫你補上方塊缺口吧", "[TIPS]_等待添加子彈" },
        { "虛無垃圾行會直接封鎖你的常規攻擊", "[TIPS]_虛無垃圾封鎖" },
        { "傳奇工程[戰術強化] \nLv1 解鎖湮滅  Lv2 解鎖處決 Lv3 解鎖修補", "[TIPS]_戰術擴張等級" },
        { "傳奇工程有鞏固防禦、加倍火力、戰術擴張", "[TIPS]_傳奇工程種類" },
        { "傳奇工程需要透過把一個工程的等級升滿獲得", "[TIPS]_傳奇工程解鎖條件" },
        { "溢出方塊後在乾淨的戰場重整態勢吧!", "[TIPS]_溢出後重整態勢" },
        { "試著用反擊擋住範圍子彈的攻勢吧", "[TIPS]_反擊對抗範圍彈" },
        { "齊射(多行消除)跟連發(連續消除)都能造成額外傷害。", "[TIPS]_齊射與連發傷害" },
        { "撐著吧，也許現在的充能之後才會用到", "[TIPS]_保留充能待機" },
        { "範圍子彈會直接炸掉整個九宮格範圍的方塊", "[TIPS]_範圍彈九宮破壞" },
        { "請記得，這裡是執炮人與怪物之間的戰場，不是益智遊戲", "[TIPS]_這不是益智遊戲" },
        { "請謹慎應對範圍傷害與爆炸方塊的組合", "[TIPS]_範圍爆炸組合警戒" },
        { "鞏固防禦看似不起眼，其實會引起質變，用過的都說讚", "[TIPS]_鞏固防禦質變" },
        { "戰鬥開始時都會回復cp", "[TIPS]_戰鬥開始回復CP" },
        { "爆炸方塊一旦被敵方攻擊破壞，城堡就會受損", "[TIPS]_爆炸方塊城堡風險" },
        { "讓這個攻擊打到你的方塊會完蛋?那用處決攔截吧", "[TIPS]_處決攔截致命攻擊" },
    };

    [MenuItem("Tools/Tenronis/Update Stage Tips To Keys")]
    private static void UpdateAllStageTips()
    {
        // 找出所有 StageDataSO 資產（不限制 Theme 資料夾，全部掃一次最安全）
        string[] guids = AssetDatabase.FindAssets("t:Tenronis.ScriptableObjects.StageDataSO");
        int modifiedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var stage = AssetDatabase.LoadAssetAtPath<StageDataSO>(path);
            if (stage == null)
                continue;

            bool dirty = false;

            for (int i = 0; i < stage.tips.Count; i++)
            {
                string tip = stage.tips[i];
                if (string.IsNullOrEmpty(tip))
                    continue;

                if (ChineseToTipsKey.TryGetValue(tip, out string newKey))
                {
                    if (stage.tips[i] != newKey)
                    {
                        // Debug.Log($"[{path}] tips[{i}] \"{tip}\" -> \"{newKey}\"");
                        stage.tips[i] = newKey;
                        dirty = true;
                    }
                }
            }

            if (dirty)
            {
                EditorUtility.SetDirty(stage);
                modifiedCount++;
            }
        }

        if (modifiedCount > 0)
        {
            AssetDatabase.SaveAssets();
        }

        Debug.Log($"StageTipsKeyUpdater: 已更新 {modifiedCount} 個 StageDataSO 的 tips。");
    }
}

