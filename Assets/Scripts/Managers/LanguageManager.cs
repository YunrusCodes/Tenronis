using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections;
using System.Linq;

namespace Tenronis.Managers
{
    /// <summary>
    /// 語言管理器 - 單例模式
    /// 管理遊戲語言切換和 TMP_Dropdown 同步
    /// </summary>
    public class LanguageManager : MonoBehaviour
    {
        public static LanguageManager Instance { get; private set; }
        
        [Header("語言選單")]
        [Tooltip("語言選擇下拉選單 (TMP_Dropdown)")]
        [SerializeField] private TMP_Dropdown languageDropdown;
        
        [Header("語言設定")]
        [Tooltip("可用的語言代碼列表")]
        [SerializeField] private string[] availableLanguages = { "zh-TW", "en", "ja" };
        
        [Tooltip("語言顯示名稱（用於下拉選單）")]
        [SerializeField] private string[] languageDisplayNames = { "繁體中文", "English", "日本語" };
        
        private const string LANGUAGE_PREF_KEY = "SelectedLanguage";
        private bool isInitializing = false;
        
        // 屬性
        public string CurrentLanguageCode { get; private set; }
        public int CurrentLanguageIndex { get; private set; }
        
        private void Awake()
        {
            // 單例模式
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[LanguageManager] 場景中已存在LanguageManager實例，銷毀重複物件");
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[LanguageManager] 初始化完成 - 單例已建立");
        }
        
        private void Start()
        {
            // 初始化語言系統
            StartCoroutine(InitializeLanguageSystem());
        }
        
        /// <summary>
        /// 初始化語言系統（等待 LocalizationSettings 準備就緒）
        /// </summary>
        private IEnumerator InitializeLanguageSystem()
        {
            // 等待 LocalizationSettings 初始化完成
            yield return LocalizationSettings.InitializationOperation;
            
            // 設置下拉選單選項
            SetupDropdown();
            
            // 載入保存的語言設置或使用系統默認語言
            LoadSavedLanguage();
            
            Debug.Log($"[LanguageManager] 語言系統初始化完成，當前語言: {CurrentLanguageCode}");
        }
        
        /// <summary>
        /// 設置下拉選單選項
        /// </summary>
        private void SetupDropdown()
        {
            if (languageDropdown == null)
            {
                Debug.LogWarning("[LanguageManager] languageDropdown 未設置");
                return;
            }
            
            // 清空現有選項
            languageDropdown.ClearOptions();
            
            // 添加語言選項
            var options = languageDisplayNames.Select(name => new TMP_Dropdown.OptionData(name)).ToList();
            languageDropdown.AddOptions(options);
            
            // 訂閱下拉選單變更事件
            languageDropdown.onValueChanged.RemoveListener(OnLanguageDropdownChanged);
            languageDropdown.onValueChanged.AddListener(OnLanguageDropdownChanged);
        }
        
        /// <summary>
        /// 載入保存的語言設置
        /// </summary>
        private void LoadSavedLanguage()
        {
            // 從 PlayerPrefs 讀取保存的語言代碼
            string savedLanguage = PlayerPrefs.GetString(LANGUAGE_PREF_KEY, "");
            
            // 如果沒有保存的語言，使用 LocalizationSettings 的當前語言
            if (string.IsNullOrEmpty(savedLanguage))
            {
                var currentLocale = LocalizationSettings.SelectedLocale;
                if (currentLocale != null)
                {
                    savedLanguage = currentLocale.Identifier.Code;
                }
                else
                {
                    // 如果 LocalizationSettings 也沒有語言，使用默認（繁體中文）
                    savedLanguage = availableLanguages[0];
                }
            }
            
            // 設置語言
            SetLanguage(savedLanguage);
        }
        
        /// <summary>
        /// 當下拉選單值變更時調用
        /// </summary>
        private void OnLanguageDropdownChanged(int index)
        {
            // 避免初始化時觸發
            if (isInitializing)
                return;
            
            if (index >= 0 && index < availableLanguages.Length)
            {
                string languageCode = availableLanguages[index];
                SetLanguage(languageCode);
            }
        }
        
        /// <summary>
        /// 設置遊戲語言
        /// </summary>
        /// <param name="languageCode">語言代碼 (zh-TW, en, ja)</param>
        public void SetLanguage(string languageCode)
        {
            // 驗證語言代碼是否有效
            int languageIndex = System.Array.IndexOf(availableLanguages, languageCode);
            if (languageIndex < 0)
            {
                Debug.LogWarning($"[LanguageManager] 無效的語言代碼: {languageCode}，使用預設語言");
                languageCode = availableLanguages[0];
                languageIndex = 0;
            }
            
            // 更新當前語言
            CurrentLanguageCode = languageCode;
            CurrentLanguageIndex = languageIndex;
            
            // 設置 LocalizationSettings
            var locale = LocalizationSettings.AvailableLocales.Locales.FirstOrDefault(l => l.Identifier.Code == languageCode);
            if (locale != null)
            {
                isInitializing = true;
                LocalizationSettings.SelectedLocale = locale;
                isInitializing = false;
                
                // 更新下拉選單顯示（如果已設置）
                if (languageDropdown != null && languageDropdown.value != languageIndex)
                {
                    languageDropdown.value = languageIndex;
                }
                
                // 保存到 PlayerPrefs
                PlayerPrefs.SetString(LANGUAGE_PREF_KEY, languageCode);
                PlayerPrefs.Save();
                
                Debug.Log($"[LanguageManager] 語言已切換為: {languageCode} ({languageDisplayNames[languageIndex]})");
            }
            else
            {
                Debug.LogError($"[LanguageManager] 找不到語言 Locale: {languageCode}");
            }
        }
        
        /// <summary>
        /// 根據索引設置語言
        /// </summary>
        /// <param name="index">語言索引 (0=zh-TW, 1=en, 2=ja)</param>
        public void SetLanguageByIndex(int index)
        {
            if (index >= 0 && index < availableLanguages.Length)
            {
                SetLanguage(availableLanguages[index]);
            }
            else
            {
                Debug.LogWarning($"[LanguageManager] 無效的語言索引: {index}");
            }
        }
        
        /// <summary>
        /// 獲取當前語言的顯示名稱
        /// </summary>
        public string GetCurrentLanguageDisplayName()
        {
            if (CurrentLanguageIndex >= 0 && CurrentLanguageIndex < languageDisplayNames.Length)
            {
                return languageDisplayNames[CurrentLanguageIndex];
            }
            return CurrentLanguageCode;
        }
        
        /// <summary>
        /// 手動設置下拉選單引用（用於運行時動態設置）
        /// </summary>
        public void SetLanguageDropdown(TMP_Dropdown dropdown)
        {
            if (dropdown == null)
            {
                Debug.LogWarning("[LanguageManager] 嘗試設置空的 languageDropdown");
                return;
            }
            
            // 取消訂閱舊的下拉選單
            if (languageDropdown != null)
            {
                languageDropdown.onValueChanged.RemoveListener(OnLanguageDropdownChanged);
            }
            
            languageDropdown = dropdown;
            SetupDropdown();
            
            // 設置當前選中值
            if (languageDropdown != null && CurrentLanguageIndex >= 0)
            {
                isInitializing = true;
                languageDropdown.value = CurrentLanguageIndex;
                isInitializing = false;
            }
        }
        
        private void OnDestroy()
        {
            // 取消事件訂閱
            if (languageDropdown != null)
            {
                languageDropdown.onValueChanged.RemoveListener(OnLanguageDropdownChanged);
            }
        }
    }
}
