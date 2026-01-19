using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;

namespace Tenronis.Utils
{
    /// <summary>
    /// 本地化輔助類 - 用於獲取本地化字符串
    /// </summary>
    public static class LocalizationHelper
    {
        private const string UI_TEXT_TABLE = "UI_Text";
        
        /// <summary>
        /// 同步獲取本地化字符串（使用 key）
        /// </summary>
        /// <param name="key">本地化 key</param>
        /// <param name="fallback">如果找不到時的後備字符串</param>
        /// <returns>本地化後的字符串</returns>
        public static string GetLocalizedString(string key, string fallback = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                return fallback ?? key;
            }
            
            try
            {
                var stringDatabase = LocalizationSettings.StringDatabase;
                TableReference tableReference = UI_TEXT_TABLE; // 隱式轉換
                TableEntryReference entryReference = key; // 隱式轉換
                
                var localizedString = stringDatabase.GetLocalizedString(tableReference, entryReference);
                
                // 如果獲取失敗（返回空字符串或 null），返回後備字符串
                if (string.IsNullOrEmpty(localizedString))
                {
                    Debug.LogWarning($"[LocalizationHelper] 找不到本地化 key: {key}，使用後備字符串");
                    return fallback ?? key;
                }
                
                return localizedString;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LocalizationHelper] 獲取本地化字符串失敗 (key: {key}): {e.Message}");
                return fallback ?? key;
            }
        }
        
        /// <summary>
        /// 異步獲取本地化字符串（使用 key）
        /// </summary>
        /// <param name="key">本地化 key</param>
        /// <param name="fallback">如果找不到時的後備字符串</param>
        /// <returns>本地化後的字符串</returns>
        public static async Task<string> GetLocalizedStringAsync(string key, string fallback = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                return fallback ?? key;
            }
            
            try
            {
                var stringDatabase = LocalizationSettings.StringDatabase;
                TableReference tableReference = UI_TEXT_TABLE; // 隱式轉換
                TableEntryReference entryReference = key; // 隱式轉換
                
                var handle = stringDatabase.GetLocalizedStringAsync(tableReference, entryReference);
                
                // 等待操作完成
                await handle.Task;
                
                // 檢查操作是否成功
                if (handle.Status == AsyncOperationStatus.Succeeded && !string.IsNullOrEmpty(handle.Result))
                {
                    return handle.Result;
                }
                else
                {
                    Debug.LogWarning($"[LocalizationHelper] 找不到本地化 key: {key}，使用後備字符串");
                    return fallback ?? key;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LocalizationHelper] 獲取本地化字符串失敗 (key: {key}): {e.Message}");
                return fallback ?? key;
            }
        }
        
        /// <summary>
        /// 格式化本地化字符串（支持參數）
        /// </summary>
        /// <param name="key">本地化 key</param>
        /// <param name="args">格式化參數</param>
        /// <returns>格式化後的本地化字符串</returns>
        public static string GetLocalizedStringFormat(string key, params object[] args)
        {
            try
            {
                var stringDatabase = LocalizationSettings.StringDatabase;
                TableReference tableReference = UI_TEXT_TABLE; // 隱式轉換
                TableEntryReference entryReference = key; // 隱式轉換
                
                var localizedString = stringDatabase.GetLocalizedString(tableReference, entryReference, args);
                
                if (string.IsNullOrEmpty(localizedString))
                {
                    Debug.LogWarning($"[LocalizationHelper] 找不到本地化 key: {key}");
                    return key;
                }
                
                return localizedString;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LocalizationHelper] 獲取本地化字符串失敗 (key: {key}): {e.Message}");
                return key;
            }
        }
    }
}
