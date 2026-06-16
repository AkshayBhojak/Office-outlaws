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
        private AudioClip _bgMusic1;
        private AudioClip _bgMusic2;
        private AudioClip _caughtSound;
        private AudioClip _pushSound;

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

            _gameOverSound = Resources.Load<AudioClip>("CasualGameSounds/lose02");
            if (_gameOverSound == null)
            {
                _gameOverSound = Resources.Load<AudioClip>("CasualGameSounds/GameOverSound");
            }

            _caughtSound = Resources.Load<AudioClip>("CasualGameSounds/Caught");
            _pushSound = Resources.Load<AudioClip>("CasualGameSounds/BonusSound");

            _bgMusic1 = Resources.Load<AudioClip>("CasualGameSounds/Pocket_Sized_Heist");
            _bgMusic2 = Resources.Load<AudioClip>("CasualGameSounds/Velvet_Glove_Escape");

            // Play random background music
            PlayRandomBgm();
        }

        public void PlayRandomBgm()
        {
            if (_bgMusic1 == null && _bgMusic2 == null) return;

            AudioClip selected = null;
            if (_bgMusic1 != null && _bgMusic2 != null)
            {
                selected = Random.value > 0.5f ? _bgMusic1 : _bgMusic2;
            }
            else
            {
                selected = _bgMusic1 != null ? _bgMusic1 : _bgMusic2;
            }

            if (selected != null && _musicSource != null)
            {
                if (_musicSource.clip != selected || !_musicSource.isPlaying)
                {
                    _musicSource.clip = selected;
                    UpdateMusicState();
                }
            }
        }

        private bool _isAdMuted = false;

        public void MuteMusicForAd(bool mute)
        {
            _isAdMuted = mute;
            UpdateMusicState();
        }

        private void UpdateMusicState()
        {
            if (_musicSource == null) return;

            if (_musicEnabled && !_isAdMuted)
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

        public void PlayCaughtSound()
        {
            if (_sfxEnabled && _caughtSound != null && _sfxSource != null)
            {
                _sfxSource.PlayOneShot(_caughtSound);
            }
        }

        public void PlayPushSound()
        {
            if (_sfxEnabled && _pushSound != null && _sfxSource != null)
            {
                _sfxSource.PlayOneShot(_pushSound);
            }
        }
    }
}
