using UnityEngine;

namespace ParkourDelivery.Core
{
    /// <summary>
    /// Looping music and one-shot effects driven by GameManager and the player.
    /// Any clip left empty is simply not played.
    /// </summary>
    public class GameAudio : MonoBehaviour
    {
        [SerializeField] private AudioClip music;
        [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.4f;
        [SerializeField] private AudioClip jump;
        [SerializeField] private AudioClip pickup;
        [SerializeField] private AudioClip hurt;
        [SerializeField] private AudioClip delivered;
        [SerializeField] private AudioClip gameOver;
        [SerializeField] [Range(0f, 1f)] private float effectsVolume = 0.8f;

        private AudioSource musicSource;
        private AudioSource effectsSource;
        private GameManager game;

        private void Awake()
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = musicVolume;
            effectsSource = gameObject.AddComponent<AudioSource>();
            effectsSource.playOnAwake = false;
            effectsSource.volume = effectsVolume;
        }

        private void Start()
        {
            game = GameManager.Instance;
            if (music != null)
            {
                musicSource.clip = music;
                musicSource.Play();
            }
            if (game == null) return;
            game.PackageCollected += OnPackageCollected;
            game.PlayerHurt += OnPlayerHurt;
            game.StateChanged += OnStateChanged;
            if (game.Player != null) game.Player.Jumped += OnJumped;
        }

        private void OnDestroy()
        {
            if (game == null) return;
            game.PackageCollected -= OnPackageCollected;
            game.PlayerHurt -= OnPlayerHurt;
            game.StateChanged -= OnStateChanged;
            if (game.Player != null) game.Player.Jumped -= OnJumped;
        }

        private void OnJumped() => Play(jump);
        private void OnPackageCollected() => Play(pickup);
        private void OnPlayerHurt() => Play(hurt);

        private void OnStateChanged(GameState state)
        {
            if (state == GameState.Paused) musicSource.Pause();
            else if (state == GameState.Playing) musicSource.UnPause();

            if (state == GameState.LevelComplete) Play(delivered);
            if (state == GameState.GameOver)
            {
                musicSource.Stop();
                Play(gameOver);
            }
        }

        private void Play(AudioClip clip)
        {
            if (clip != null) effectsSource.PlayOneShot(clip);
        }
    }
}
