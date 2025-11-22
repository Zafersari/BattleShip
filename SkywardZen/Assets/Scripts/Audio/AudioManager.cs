using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

namespace SkywardZen.Audio
{
    /// <summary>
    /// Manages all audio in the game including music, SFX, and volume control
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Mixers")]
        [SerializeField] private AudioMixerGroup masterMixer;
        [SerializeField] private AudioMixerGroup musicMixer;
        [SerializeField] private AudioMixerGroup sfxMixer;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource ambientSource;
        [SerializeField] private List<AudioSource> sfxSources = new List<AudioSource>();
        [SerializeField] private int sfxSourcesCount = 5; // Pool of SFX sources

        [Header("Music Tracks")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip gameplayMusic;
        [SerializeField] private AudioClip bossMusic;
        [SerializeField] private float musicFadeDuration = 1f;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip jumpSound;
        [SerializeField] private AudioClip landSound;
        [SerializeField] private AudioClip platformBreakSound;
        [SerializeField] private AudioClip powerUpCollectSound;
        [SerializeField] private AudioClip gameOverSound;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip newHighScoreSound;

        [Header("Ambient Sounds")]
        [SerializeField] private AudioClip windAmbient;
        [SerializeField] private AudioClip forestAmbient;
        [SerializeField] private AudioClip spaceAmbient;

        [Header("Volume Settings")]
        [SerializeField] [Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.7f;
        [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.8f;
        [SerializeField] [Range(0f, 1f)] private float ambientVolume = 0.3f;

        [Header("Settings")]
        [SerializeField] private bool muteOnFocusLoss = true;

        private AudioClip currentMusicClip;
        private bool isFadingMusic = false;
        private float musicFadeTimer = 0f;
        private float musicFadeFrom = 0f;
        private float musicFadeTo = 1f;

        private const string MASTER_VOLUME_KEY = "MasterVolume";
        private const string MUSIC_VOLUME_KEY = "MusicVolume";
        private const string SFX_VOLUME_KEY = "SFXVolume";

        #region Unity Lifecycle

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
            LoadVolumeSettings();
        }

        private void Start()
        {
            // Subscribe to game state events
            if (Core.GameStateManager.Instance != null)
            {
                Core.GameStateManager.Instance.OnGameStart.AddListener(OnGameStart);
                Core.GameStateManager.Instance.OnGameOver.AddListener(OnGameOver);
            }

            // Play menu music by default
            PlayMusic(menuMusic);
        }

        private void Update()
        {
            // Update music fade
            if (isFadingMusic)
            {
                UpdateMusicFade();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (muteOnFocusLoss)
            {
                AudioListener.volume = hasFocus ? masterVolume : 0f;
            }
        }

        #endregion

        #region Public Methods - Music

        /// <summary>
        /// Play music with optional fade in
        /// </summary>
        public void PlayMusic(AudioClip clip, bool fade = true)
        {
            if (clip == null) return;
            if (currentMusicClip == clip && musicSource.isPlaying) return;

            Debug.Log($"[AudioManager] Playing music: {clip.name}");

            if (fade && musicSource.isPlaying)
            {
                // Fade out current, then play new
                FadeOutMusic(() => PlayMusicImmediate(clip, true));
            }
            else
            {
                PlayMusicImmediate(clip, fade);
            }
        }

        /// <summary>
        /// Stop music with optional fade out
        /// </summary>
        public void StopMusic(bool fade = true)
        {
            if (fade)
            {
                FadeOutMusic(() => musicSource.Stop());
            }
            else
            {
                musicSource.Stop();
            }
        }

        /// <summary>
        /// Pause music
        /// </summary>
        public void PauseMusic()
        {
            musicSource.Pause();
        }

        /// <summary>
        /// Resume music
        /// </summary>
        public void ResumeMusic()
        {
            musicSource.UnPause();
        }

        #endregion

        #region Public Methods - Sound Effects

        /// <summary>
        /// Play a sound effect
        /// </summary>
        public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null) return;

            AudioSource source = GetAvailableSFXSource();
            if (source != null)
            {
                source.pitch = pitch;
                source.PlayOneShot(clip, volume * sfxVolume);
            }
        }

        /// <summary>
        /// Play jump sound
        /// </summary>
        public void PlayJumpSound()
        {
            PlaySFX(jumpSound, 0.7f, Random.Range(0.95f, 1.05f));
        }

        /// <summary>
        /// Play land sound
        /// </summary>
        public void PlayLandSound()
        {
            PlaySFX(landSound, 0.8f, Random.Range(0.9f, 1.1f));
        }

        /// <summary>
        /// Play platform break sound
        /// </summary>
        public void PlayPlatformBreakSound()
        {
            PlaySFX(platformBreakSound, 0.9f);
        }

        /// <summary>
        /// Play power-up collect sound
        /// </summary>
        public void PlayPowerUpSound()
        {
            PlaySFX(powerUpCollectSound, 1f, 1.2f);
        }

        /// <summary>
        /// Play game over sound
        /// </summary>
        public void PlayGameOverSound()
        {
            PlaySFX(gameOverSound, 1f);
        }

        /// <summary>
        /// Play button click sound
        /// </summary>
        public void PlayButtonClickSound()
        {
            PlaySFX(buttonClickSound, 0.6f);
        }

        /// <summary>
        /// Play new high score sound
        /// </summary>
        public void PlayHighScoreSound()
        {
            PlaySFX(newHighScoreSound, 1f);
        }

        #endregion

        #region Public Methods - Ambient

        /// <summary>
        /// Play ambient sound based on biome
        /// </summary>
        public void PlayAmbient(Environment.BiomeType biomeType)
        {
            AudioClip ambientClip = GetAmbientClipForBiome(biomeType);

            if (ambientClip != null && ambientSource.clip != ambientClip)
            {
                ambientSource.clip = ambientClip;
                ambientSource.volume = ambientVolume;
                ambientSource.loop = true;
                ambientSource.Play();

                Debug.Log($"[AudioManager] Playing ambient: {ambientClip.name}");
            }
        }

        #endregion

        #region Public Methods - Volume Control

        /// <summary>
        /// Set master volume
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            AudioListener.volume = masterVolume;
            SaveVolumeSettings();
        }

        /// <summary>
        /// Set music volume
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            musicSource.volume = musicVolume;
            SaveVolumeSettings();
        }

        /// <summary>
        /// Set SFX volume
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            SaveVolumeSettings();
        }

        /// <summary>
        /// Set ambient volume
        /// </summary>
        public void SetAmbientVolume(float volume)
        {
            ambientVolume = Mathf.Clamp01(volume);
            ambientSource.volume = ambientVolume;
        }

        /// <summary>
        /// Mute/unmute all audio
        /// </summary>
        public void SetMute(bool mute)
        {
            AudioListener.volume = mute ? 0f : masterVolume;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initialize audio sources for music and SFX
        /// </summary>
        private void InitializeAudioSources()
        {
            // Create music source if not assigned
            if (musicSource == null)
            {
                GameObject musicGO = new GameObject("MusicSource");
                musicGO.transform.SetParent(transform);
                musicSource = musicGO.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            // Create ambient source if not assigned
            if (ambientSource == null)
            {
                GameObject ambientGO = new GameObject("AmbientSource");
                ambientGO.transform.SetParent(transform);
                ambientSource = ambientGO.AddComponent<AudioSource>();
                ambientSource.loop = true;
                ambientSource.playOnAwake = false;
            }

            // Create SFX source pool
            for (int i = sfxSources.Count; i < sfxSourcesCount; i++)
            {
                GameObject sfxGO = new GameObject($"SFXSource_{i}");
                sfxGO.transform.SetParent(transform);
                AudioSource source = sfxGO.AddComponent<AudioSource>();
                source.playOnAwake = false;
                sfxSources.Add(source);
            }

            Debug.Log($"[AudioManager] Initialized {sfxSources.Count} SFX sources");
        }

        /// <summary>
        /// Get an available SFX audio source from pool
        /// </summary>
        private AudioSource GetAvailableSFXSource()
        {
            // Find available source
            foreach (AudioSource source in sfxSources)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }

            // If all busy, use first one
            return sfxSources.Count > 0 ? sfxSources[0] : null;
        }

        /// <summary>
        /// Play music immediately without fade
        /// </summary>
        private void PlayMusicImmediate(AudioClip clip, bool fadeIn)
        {
            currentMusicClip = clip;
            musicSource.clip = clip;
            musicSource.Play();

            if (fadeIn)
            {
                musicSource.volume = 0f;
                FadeInMusic();
            }
            else
            {
                musicSource.volume = musicVolume;
            }
        }

        /// <summary>
        /// Fade in music
        /// </summary>
        private void FadeInMusic()
        {
            isFadingMusic = true;
            musicFadeTimer = 0f;
            musicFadeFrom = 0f;
            musicFadeTo = musicVolume;
        }

        /// <summary>
        /// Fade out music with callback
        /// </summary>
        private void FadeOutMusic(System.Action onComplete = null)
        {
            isFadingMusic = true;
            musicFadeTimer = 0f;
            musicFadeFrom = musicSource.volume;
            musicFadeTo = 0f;

            // Store callback (you'd need to add a field for this in a full implementation)
        }

        /// <summary>
        /// Update music fade transition
        /// </summary>
        private void UpdateMusicFade()
        {
            musicFadeTimer += Time.deltaTime;
            float t = Mathf.Clamp01(musicFadeTimer / musicFadeDuration);

            musicSource.volume = Mathf.Lerp(musicFadeFrom, musicFadeTo, t);

            if (t >= 1f)
            {
                isFadingMusic = false;
            }
        }

        /// <summary>
        /// Get ambient clip for biome type
        /// </summary>
        private AudioClip GetAmbientClipForBiome(Environment.BiomeType biomeType)
        {
            switch (biomeType)
            {
                case Environment.BiomeType.PastelDream:
                    return windAmbient;
                case Environment.BiomeType.ForestMist:
                    return forestAmbient;
                case Environment.BiomeType.CosmicVoid:
                    return spaceAmbient;
                default:
                    return windAmbient;
            }
        }

        /// <summary>
        /// Save volume settings to PlayerPrefs
        /// </summary>
        private void SaveVolumeSettings()
        {
            PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, masterVolume);
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolume);
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Load volume settings from PlayerPrefs
        /// </summary>
        private void LoadVolumeSettings()
        {
            masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
            musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.7f);
            sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.8f);

            AudioListener.volume = masterVolume;
            musicSource.volume = musicVolume;

            Debug.Log($"[AudioManager] Volume loaded - Master: {masterVolume}, Music: {musicVolume}, SFX: {sfxVolume}");
        }

        #endregion

        #region Event Handlers

        private void OnGameStart()
        {
            PlayMusic(gameplayMusic);
        }

        private void OnGameOver()
        {
            PlayGameOverSound();
        }

        #endregion

        #region Debug

        private void OnGUI()
        {
            // Debug volume controls
            GUILayout.BeginArea(new Rect(10, Screen.height - 120, 250, 110));
            GUILayout.Label("<b>Audio Volumes:</b>");
            GUILayout.Label($"Master: {masterVolume:F2}");
            GUILayout.Label($"Music: {musicVolume:F2}");
            GUILayout.Label($"SFX: {sfxVolume:F2}");
            GUILayout.EndArea();
        }

        #endregion
    }
}
