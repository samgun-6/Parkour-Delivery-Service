using System;
using System.Collections;
using ParkourDelivery.Gameplay;
using ParkourDelivery.Input;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ParkourDelivery.Core
{
    public enum GameState
    {
        Countdown,
        Playing,
        Paused,
        LevelComplete,
        GameOver
    }

    /// <summary>
    /// Runs one level: countdown, lives, packages, respawn at checkpoints,
    /// pause, and the end of the run. UI and audio listen to its events.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private PlayerController player;
        [SerializeField] private int startingLives = 3;
        [SerializeField] private float parTimeSeconds = 120f;
        [SerializeField] private float countdownStepSeconds = 0.8f;
        [SerializeField] private float respawnDelaySeconds = 1f;
        [Tooltip("Hits are ignored this long after respawning, so a hazard at the checkpoint cannot chain-kill.")]
        [SerializeField] private float invulnerableSeconds = 1f;
        [Tooltip("Scene loaded by Continue after finishing. Empty replays this level.")]
        [SerializeField] private string nextSceneName = "";

        public GameState State { get; private set; } = GameState.Countdown;
        public RunStats Stats { get; private set; }
        public int FinalScore { get; private set; }
        public int HighScore { get; private set; }
        public bool HasNextLevel => !string.IsNullOrEmpty(nextSceneName);
        public PlayerController Player => player;

        public event Action<GameState> StateChanged;
        public event Action StatsChanged;
        public event Action<string> CountdownStep;
        public event Action PackageCollected;
        public event Action PlayerHurt;

        private PlayerControls controls;
        private Vector3 checkpoint;
        private bool respawning;
        private float invulnerableUntil;

        private string HighScoreKey => "HighScore_" + SceneManager.GetActiveScene().name;

        private void Awake()
        {
            Instance = this;
            controls = new PlayerControls();
            int packages = FindObjectsByType<Package>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;
            Stats = new RunStats(startingLives, packages);
            HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
            if (player != null) checkpoint = player.transform.position;
        }

        private void OnEnable()
        {
            controls.Game.Enable();
        }

        private void OnDisable()
        {
            controls.Game.Disable();
        }

        private void OnDestroy()
        {
            controls.Dispose();
            if (Instance == this) Instance = null;
            Time.timeScale = 1f;
        }

        private void Start()
        {
            StartCoroutine(CountdownRoutine());
        }

        private void Update()
        {
            if (controls.Game.Pause.WasPressedThisFrame()) TogglePause();

            if (State == GameState.Playing)
            {
                Stats.Tick(Time.deltaTime);
                StatsChanged?.Invoke();
            }
        }

        private IEnumerator CountdownRoutine()
        {
            SetState(GameState.Countdown);
            SetPlayerControl(false);
            foreach (var step in new[] { "3", "2", "1", "GO!" })
            {
                CountdownStep?.Invoke(step);
                yield return new WaitForSeconds(countdownStepSeconds);
            }
            CountdownStep?.Invoke("");
            SetPlayerControl(true);
            SetState(GameState.Playing);
        }

        public void CollectPackage()
        {
            if (State != GameState.Playing) return;
            Stats.CollectPackage();
            PackageCollected?.Invoke();
            StatsChanged?.Invoke();
        }

        public void SetCheckpoint(Vector3 position)
        {
            checkpoint = position;
        }

        public void PlayerHit()
        {
            if (State != GameState.Playing || respawning || Time.time < invulnerableUntil) return;
            StartCoroutine(HitRoutine());
        }

        private IEnumerator HitRoutine()
        {
            respawning = true;
            SetPlayerControl(false);
            SetPlayerSimulated(false);
            bool outOfLives = Stats.LoseLife();
            PlayerHurt?.Invoke();
            StatsChanged?.Invoke();

            yield return new WaitForSeconds(respawnDelaySeconds);

            if (outOfLives)
            {
                respawning = false;
                SetState(GameState.GameOver);
                yield break;
            }

            player.transform.position = checkpoint;
            SetPlayerSimulated(true);
            SetPlayerControl(true);
            invulnerableUntil = Time.time + invulnerableSeconds;
            respawning = false;
        }

        public void CompleteLevel()
        {
            if (State != GameState.Playing) return;
            SetPlayerControl(false);
            FinalScore = ScoreCalculator.Calculate(Stats.PackagesCollected, Stats.ElapsedSeconds, parTimeSeconds);
            if (FinalScore > HighScore)
            {
                HighScore = FinalScore;
                PlayerPrefs.SetInt(HighScoreKey, HighScore);
                PlayerPrefs.Save();
            }
            SetState(GameState.LevelComplete);
        }

        public void TogglePause()
        {
            if (State == GameState.Playing)
            {
                Time.timeScale = 0f;
                SetPlayerControl(false);
                SetState(GameState.Paused);
            }
            else if (State == GameState.Paused)
            {
                Time.timeScale = 1f;
                SetPlayerControl(true);
                SetState(GameState.Playing);
            }
        }

        public void Retry()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void Continue()
        {
            if (!HasNextLevel)
            {
                Retry();
                return;
            }
            Time.timeScale = 1f;
            SceneManager.LoadScene(nextSceneName);
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void SetState(GameState state)
        {
            State = state;
            StateChanged?.Invoke(state);
        }

        private void SetPlayerControl(bool enabled)
        {
            if (player == null) return;
            player.enabled = enabled;
            if (!enabled && player.rigBody != null)
            {
                player.rigBody.linearVelocity = new Vector2(0f, Mathf.Min(0f, player.rigBody.linearVelocity.y));
            }
        }

        private void SetPlayerSimulated(bool simulated)
        {
            if (player == null || player.rigBody == null) return;
            player.rigBody.linearVelocity = Vector2.zero;
            player.rigBody.simulated = simulated;
        }

        /// <summary>For tests: wires the player and skips the countdown delay.</summary>
        public void Configure(PlayerController playerController, int lives, float countdownStep, float respawnDelay)
        {
            player = playerController;
            startingLives = lives;
            countdownStepSeconds = countdownStep;
            respawnDelaySeconds = respawnDelay;
            Stats = new RunStats(startingLives, Stats.PackagesTotal);
            checkpoint = player.transform.position;
        }
    }
}
