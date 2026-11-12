using ParkourDelivery.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ParkourDelivery.UI
{
    /// <summary>
    /// Builds the in-game UI at runtime and keeps it in sync with GameManager:
    /// HUD (lives top left, packages top centre, time top right), countdown,
    /// pause menu and end-of-run panel.
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [SerializeField] private Font font;

        private GameManager game;
        private Text livesText;
        private Text packagesText;
        private Text timeText;
        private Text countdownText;
        private GameObject pausePanel;
        private GameObject endPanel;
        private Text endTitle;
        private Text endScore;
        private Button continueButton;

        private void Start()
        {
            game = GameManager.Instance;
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            EnsureEventSystem();
            BuildCanvas();

            if (game == null) return;
            game.StatsChanged += RefreshHud;
            game.StateChanged += OnStateChanged;
            game.CountdownStep += OnCountdownStep;
            RefreshHud();
            OnStateChanged(game.State);
        }

        private void OnDestroy()
        {
            if (game == null) return;
            game.StatsChanged -= RefreshHud;
            game.StateChanged -= OnStateChanged;
            game.CountdownStep -= OnCountdownStep;
        }

        private void RefreshHud()
        {
            var stats = game.Stats;
            livesText.text = $"Lives: {stats.Lives}";
            packagesText.text = $"Packages: {stats.PackagesCollected}/{stats.PackagesTotal}";
            timeText.text = "Time: " + FormatTime(stats.ElapsedSeconds);
        }

        private void OnCountdownStep(string step)
        {
            countdownText.text = step;
        }

        private void OnStateChanged(GameState state)
        {
            pausePanel.SetActive(state == GameState.Paused);
            endPanel.SetActive(state == GameState.LevelComplete || state == GameState.GameOver);

            if (state == GameState.LevelComplete)
            {
                endTitle.text = "Delivered!";
                endScore.text =
                    $"Packages: {game.Stats.PackagesCollected}/{game.Stats.PackagesTotal}\n" +
                    $"Time: {FormatTime(game.Stats.ElapsedSeconds)}\n" +
                    $"Score: {game.FinalScore}\n" +
                    $"High score: {game.HighScore}";
                continueButton.gameObject.SetActive(true);
                continueButton.GetComponentInChildren<Text>().text = game.HasNextLevel ? "Continue" : "Play again";
            }
            else if (state == GameState.GameOver)
            {
                endTitle.text = "Game over";
                endScore.text = $"Packages: {game.Stats.PackagesCollected}/{game.Stats.PackagesTotal}\nNo lives left.";
                continueButton.gameObject.SetActive(false);
            }
        }

        private static string FormatTime(float seconds)
        {
            int total = Mathf.FloorToInt(seconds);
            return $"{total / 60:00}:{total % 60:00}";
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }

        private void BuildCanvas()
        {
            var canvasGo = new GameObject("GameCanvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            var root = canvasGo.transform;

            livesText = CreateText(root, "Lives", 48, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(40, -30));
            packagesText = CreateText(root, "Packages", 48, TextAnchor.UpperCenter, new Vector2(0.5f, 1), new Vector2(0, -30));
            timeText = CreateText(root, "Time", 48, TextAnchor.UpperRight, new Vector2(1, 1), new Vector2(-40, -30));
            countdownText = CreateText(root, "Countdown", 160, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero);

            pausePanel = CreatePanel(root, "PausePanel");
            CreateText(pausePanel.transform, "Title", 96, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 180)).text = "Paused";
            CreateButton(pausePanel.transform, "Resume", new Vector2(0, 40), () => game.TogglePause());
            CreateButton(pausePanel.transform, "Retry", new Vector2(0, -70), () => game.Retry());
            CreateButton(pausePanel.transform, "Quit", new Vector2(0, -180), () => game.Quit());

            endPanel = CreatePanel(root, "EndPanel");
            endTitle = CreateText(endPanel.transform, "Title", 96, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 260));
            endScore = CreateText(endPanel.transform, "Score", 44, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 90));
            continueButton = CreateButton(endPanel.transform, "Continue", new Vector2(0, -90), () => game.Continue());
            CreateButton(endPanel.transform, "Retry", new Vector2(0, -200), () => game.Retry());
            CreateButton(endPanel.transform, "Quit", new Vector2(0, -310), () => game.Quit());

            pausePanel.SetActive(false);
            endPanel.SetActive(false);
        }

        private Text CreateText(Transform parent, string name, int size, TextAnchor alignment, Vector2 anchor, Vector2 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(900, size * 3.2f);
            var text = go.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
            outline.effectDistance = new Vector2(3, -3);
            return text;
        }

        private static GameObject CreatePanel(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            go.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
            return go;
        }

        private Button CreateButton(Transform parent, string label, Vector2 position, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(label + "Button");
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(420, 90);
            var image = go.AddComponent<Image>();
            image.color = new Color(0.95f, 0.55f, 0.15f);
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);
            var text = CreateText(go.transform, "Label", 44, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero);
            text.text = label;
            return button;
        }
    }
}
