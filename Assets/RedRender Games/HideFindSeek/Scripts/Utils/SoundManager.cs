using UnityEngine;

namespace Game.Audio
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        private AudioSource _sfxSource;
        private AudioSource _musicSource;

        private AudioClip _coinSound;
        private AudioClip _winSound;
        private AudioClip _gameOverSound;
        private AudioClip _bgMusic;

        private bool _sfxEnabled = true;
        private bool _musicEnabled = true;

        public bool SfxEnabled
        {
            get { return _sfxEnabled; }
            set
            {
                _sfxEnabled = value;
                PlayerPrefs.SetInt("SoundFXEnabled", value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public bool MusicEnabled
        {
            get { return _musicEnabled; }
            set
            {
                _musicEnabled = value;
                PlayerPrefs.SetInt("MusicEnabled", value ? 1 : 0);
                PlayerPrefs.Save();
                UpdateMusicState();
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Add AudioSources
            _sfxSource = gameObject.AddComponent<AudioSource>();
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;

            // Load settings
            _sfxEnabled = PlayerPrefs.GetInt("SoundFXEnabled", 1) == 1;
            _musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;

            // Load sounds from Resources
            _coinSound = Resources.Load<AudioClip>("CasualGameSounds/Coins");
            if (_coinSound == null)
            {
                _coinSound = Resources.Load<AudioClip>("CasualGameSounds/CoinSound");
            }

            _winSound = Resources.Load<AudioClip>("CasualGameSounds/level complected");
            if (_winSound == null)
            {
                _winSound = Resources.Load<AudioClip>("CasualGameSounds/WinSound");
            }

            _gameOverSound = Resources.Load<AudioClip>("CasualGameSounds/GameOverSound");
            _bgMusic = Resources.Load<AudioClip>("CasualGameSounds/dominoespizzaakacatchme");

            // Play background music
            if (_bgMusic != null)
            {
                _musicSource.clip = _bgMusic;
                UpdateMusicState();
            }
        }

        private void UpdateMusicState()
        {
            if (_musicSource == null) return;

            if (_musicEnabled)
            {
                if (!_musicSource.isPlaying)
                {
                    _musicSource.Play();
                }
                _musicSource.mute = false;
            }
            else
            {
                _musicSource.mute = true;
            }
        }

        public void PlayCoinSound()
        {
            if (_sfxEnabled && _coinSound != null && _sfxSource != null)
            {
                _sfxSource.PlayOneShot(_coinSound);
            }
        }

        public void PlayWinSound()
        {
            if (_sfxEnabled && _winSound != null && _sfxSource != null)
            {
                _sfxSource.PlayOneShot(_winSound);
            }
        }

        public void PlayGameOverSound()
        {
            if (_sfxEnabled && _gameOverSound != null && _sfxSource != null)
            {
                _sfxSource.PlayOneShot(_gameOverSound);
            }
        }
    }
}
