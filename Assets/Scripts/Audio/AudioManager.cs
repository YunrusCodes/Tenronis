using UnityEngine;
using Tenronis.Core;
using Tenronis.Data;
using Tenronis.Managers;

namespace Tenronis.Audio
{
    /// <summary>
    /// 音效管理器
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        
    [Header("音效")]
    [SerializeField] private AudioClip missileSound;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private AudioClip rotateSound;
    [SerializeField] private AudioClip impactSound;
    [SerializeField] private AudioClip counterFireSound; // 反擊音效
    [SerializeField] private AudioClip lockSound; // 方塊鎖定音效
    [SerializeField] private AudioClip enemyHitSound;
    [SerializeField] private AudioClip buffSound;
    
    [Header("敵人射擊音效")]
    [SerializeField] private AudioClip enemyShootNormalSound;     // 普通子彈音效
    [SerializeField] private AudioClip enemyShootAddBlockSound;   // 添加方塊音效
    [SerializeField] private AudioClip enemyShootAreaDamageSound; // 範圍傷害音效
    [SerializeField] private AudioClip enemyShootInsertRowSound;  // 插入不可摧毀行音效
    
    [Header("特殊事件音效")]
    [SerializeField] private AudioClip enemyAddBlockSound;        // 敵人製造方塊音效
    [SerializeField] private AudioClip voidNullifySound;          // 虛空抵銷音效
        
        [Header("音樂")]
        [SerializeField] private AudioClip normalBGM;
        [SerializeField] private AudioClip bossBGM;
        
        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;
        
        [Header("設定")]
        [Range(0f, 1f)]
        [SerializeField] private float sfxVolume = 0.7f;
        
        [Range(0f, 1f)]
        [SerializeField] private float musicVolume = 0.5f;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 建立AudioSource（如果沒有）
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
            
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }
            
            UpdateVolumes();
        }
        
        private void Start()
        {
        // 訂閱音效事件
        GameEvents.OnPlayMissileSound += PlayMissileSound;
        GameEvents.OnPlayExplosionSound += PlayExplosionSound;
        GameEvents.OnPlayRotateSound += PlayRotateSound;
        GameEvents.OnPlayImpactSound += PlayImpactSound;
        GameEvents.OnPlayCounterFireSound += PlayCounterFireSound;
        GameEvents.OnPlayLockSound += PlayLockSound;
        GameEvents.OnPlayEnemyShootSound += PlayEnemyShootSound;
        GameEvents.OnPlayEnemyAddBlockSound += PlayEnemyAddBlockSound;
        GameEvents.OnPlayVoidNullifySound += PlayVoidNullifySound;
        
        // 訂閱遊戲事件以控制BGM
        GameEvents.OnGameStateChanged += HandleGameStateChanged;
        }
        
        private void OnDestroy()
        {
        GameEvents.OnPlayMissileSound -= PlayMissileSound;
        GameEvents.OnPlayExplosionSound -= PlayExplosionSound;
        GameEvents.OnPlayRotateSound -= PlayRotateSound;
        GameEvents.OnPlayImpactSound -= PlayImpactSound;
        GameEvents.OnPlayCounterFireSound -= PlayCounterFireSound;
        GameEvents.OnPlayLockSound -= PlayLockSound;
        GameEvents.OnPlayEnemyShootSound -= PlayEnemyShootSound;
        GameEvents.OnPlayEnemyAddBlockSound -= PlayEnemyAddBlockSound;
        GameEvents.OnPlayVoidNullifySound -= PlayVoidNullifySound;
        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        }
        
        /// <summary>
        /// 更新音量
        /// </summary>
        private void UpdateVolumes()
        {
            if (sfxSource != null)
                sfxSource.volume = sfxVolume;
            
            if (musicSource != null)
                musicSource.volume = musicVolume;
        }
        
        /// <summary>
        /// 播放音效
        /// </summary>
        private void PlaySound(AudioClip clip)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip);
            }
        }
        
    // 音效播放方法
    public void PlayMissileSound() => PlaySound(missileSound);
    public void PlayExplosionSound() => PlaySound(explosionSound);
    public void PlayRotateSound() => PlaySound(rotateSound);
    public void PlayImpactSound() => PlaySound(impactSound);
    public void PlayCounterFireSound() => PlaySound(counterFireSound);
    public void PlayLockSound() => PlaySound(lockSound);
    public void PlayEnemyHitSound() => PlaySound(enemyHitSound);
    public void PlayBuffSound() => PlaySound(buffSound);
    
    /// <summary>
    /// 根據子彈類型播放敵人射擊音效
    /// </summary>
    public void PlayEnemyShootSound(BulletType bulletType)
    {
        AudioClip clip = bulletType switch
        {
            BulletType.Normal => enemyShootNormalSound,
            BulletType.AddBlock => enemyShootAddBlockSound,
            BulletType.AreaDamage => enemyShootAreaDamageSound,
            BulletType.InsertRow => enemyShootInsertRowSound,
            _ => enemyShootNormalSound
        };
        
        PlaySound(clip);
    }
    
    /// <summary>
    /// 播放敵人製造方塊音效
    /// </summary>
    public void PlayEnemyAddBlockSound() => PlaySound(enemyAddBlockSound);
    
    /// <summary>
    /// 播放虛空抵銷音效
    /// </summary>
    public void PlayVoidNullifySound() => PlaySound(voidNullifySound);
        
        /// <summary>
        /// 播放BGM
        /// </summary>
        public void PlayBGM(bool isBossTheme)
        {
            AudioClip bgm = isBossTheme ? bossBGM : normalBGM;
            
            if (bgm != null && musicSource != null)
            {
                if (musicSource.clip != bgm)
                {
                    musicSource.clip = bgm;
                    musicSource.Play();
                }
                else if (!musicSource.isPlaying)
                {
                    musicSource.Play();
                }
            }
        }
        
        /// <summary>
        /// 停止BGM
        /// </summary>
        public void StopBGM()
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Stop();
            }
        }
        
        /// <summary>
        /// 處理遊戲狀態變更
        /// </summary>
        private void HandleGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.Playing:
                case GameState.LevelUp:
                    // 從關卡數據讀取是否為Boss關卡
                    bool isBossStage = false;
                    if (GameManager.Instance != null && GameManager.Instance.CurrentStage != null)
                    {
                        isBossStage = GameManager.Instance.CurrentStage.isBossStage;
                    }
                    PlayBGM(isBossStage);
                    break;
                    
                case GameState.Menu:
                case GameState.GameOver:
                case GameState.Victory:
                    StopBGM();
                    break;
            }
        }
        
        /// <summary>
        /// 設置音效音量
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            if (sfxSource != null)
                sfxSource.volume = sfxVolume;
        }
        
        /// <summary>
        /// 設置音樂音量
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
                musicSource.volume = musicVolume;
        }
    }
}

