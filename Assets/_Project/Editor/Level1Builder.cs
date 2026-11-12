using System.Linq;
using ParkourDelivery.Core;
using ParkourDelivery.Gameplay;
using ParkourDelivery.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ParkourDelivery.Editor
{
    /// <summary>
    /// Builds the layout of Level 1 into Assets/_Project/Scenes/Level1.unity from the
    /// tables below. Everything it creates lives under the "Level" and "Game" roots and
    /// is rebuilt on every run; the existing Player and Main Camera are kept and wired up.
    /// Run from the menu or with -executeMethod ParkourDelivery.Editor.Level1Builder.BuildAndExit.
    /// </summary>
    public static class Level1Builder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Level1.unity";
        private const int GroundLayer = 9;
        private const float StreetY = -40f;
        private const string Art = ArtGenerator.Folder;
        private const string Traps = "Assets/ThirdParty";

        // Objects from the original prototype that this layout replaces.
        private static readonly string[] LegacyRoots =
            { "Cube", "Cube (1)", "Cube (2)", "Obstacles", "House", "House (1)", "empire state building" };

        private static readonly Vector2 Spawn = new Vector2(-24f, 1.6f);
        private const float PlayerCeilingCheckY = 0.6f;

        // Buildings: left, right, roof height, facade colour, optional bottom (overhangs).
        private static readonly (string name, float x0, float x1, float top, int color, float bottom)[] Buildings =
        {
            ("Start", -30f, 14f, 0f, 0, StreetY),
            ("B1", 21f, 38f, 3f, 1, StreetY),
            ("B2", 45f, 60f, -1f, 2, StreetY),
            ("B3", 60f, 68f, 5f, 3, StreetY),
            ("B4", 68f, 94f, 9f, 0, StreetY),
            // Wall-jump shaft: walk under the overhang, climb between it and the tower.
            ("ShaftOverhang", 84f, 88f, 33f, 2, 12.2f),
            ("Tower", 94f, 106f, 33f, 1, StreetY),
            ("B5", 112f, 128f, 22f, 1, StreetY),
            ("B6", 134f, 144f, 20f, 2, StreetY),
            // Crouch tunnel: 1.9 units of headroom, the player is 2.56 tall standing, 1.7 crouched.
            ("B7", 148f, 176f, 18f, 3, StreetY),
            ("TunnelRoof", 156f, 170f, 26f, 0, 19.9f),
            ("B8", 180f, 196f, 14f, 0, StreetY),
            // 20-unit gap to B9: only reachable while sprinting.
            ("B9", 216f, 234f, 10f, 2, StreetY),
            ("B10", 240f, 252f, 14f, 1, StreetY),
            ("Finish", 258f, 290f, 12f, 3, StreetY),
        };

        private static readonly Vector2[] Packages =
        {
            new Vector2(4f, 2.2f),
            new Vector2(17.5f, 7f),
            new Vector2(57f, 1f),
            new Vector2(80f, 11.5f),
            new Vector2(91f, 22f),
            new Vector2(125f, 24.5f),
            new Vector2(163f, 19f),
            new Vector2(206f, 18f),
            new Vector2(246f, 16.5f),
            new Vector2(266f, 14.5f),
        };

        // Spikes: left, right, roof height.
        private static readonly (float x0, float x1, float y)[] SpikeStrips =
        {
            (50f, 53f, -1f),
            (244f, 246.5f, 14f),
        };

        // Birds: start position and how far they fly before turning back.
        private static readonly (Vector2 start, Vector2 travel)[] BirdPaths =
        {
            (new Vector2(78f, 14f), new Vector2(8f, 0f)),
            (new Vector2(116f, 27f), new Vector2(9f, 0f)),
            (new Vector2(220f, 15f), new Vector2(10f, 0f)),
            (new Vector2(260f, 17f), new Vector2(12f, 0f)),
        };

        // Checkpoints: x and roof height.
        private static readonly Vector2[] Checkpoints =
        {
            new Vector2(72f, 9f),
            new Vector2(99f, 33f),
            new Vector2(184f, 14f),
        };

        // Death-trap prefabs from the Asset Store packs, placed where a trap fits the route.
        private static readonly (string path, Vector2 position, float scale)[] TrapPlacements =
        {
            ("2D Death Traps Ice/Prefabs/Saw.prefab", new Vector2(139f, 20f), 1f),
            ("2D Death Traps Ice/Prefabs/Swing01.prefab", new Vector2(229f, 19f), 1f),
        };

        private static readonly Vector2 DeliveryPosition = new Vector2(282f, 12f);
        private const int Lives = 3;
        private const float ParSeconds = 120f;

        [MenuItem("Parkour Delivery/Build Level 1")]
        public static void BuildFromMenu()
        {
            Build();
        }

        public static void BuildAndExit()
        {
            Build();
            EditorApplication.Exit(0);
        }

        private static void Build()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            foreach (var root in scene.GetRootGameObjects())
            {
                if (LegacyRoots.Contains(root.name) || root.name == "Level" || root.name == "Game")
                {
                    Object.DestroyImmediate(root);
                }
            }

            var level = new GameObject("Level").transform;
            var geometry = Group(level, "Buildings");
            foreach (var b in Buildings) Building(geometry, b.name, b.x0, b.x1, b.top, b.color, b.bottom);

            var pickups = Group(level, "Packages");
            for (int i = 0; i < Packages.Length; i++) PackageAt(pickups, $"Package {i + 1}", Packages[i]);

            var hazards = Group(level, "Hazards");
            foreach (var s in SpikeStrips) Spikes(hazards, s.x0, s.x1, s.y);
            foreach (var bird in BirdPaths) BirdAt(hazards, bird.start, bird.travel);
            foreach (var t in TrapPlacements) Trap(hazards, $"{Traps}/{t.path}", t.position, t.scale);
            KillZone(hazards);

            var route = Group(level, "Route");
            for (int i = 0; i < Checkpoints.Length; i++) CheckpointAt(route, $"Checkpoint {i + 1}", Checkpoints[i]);
            Delivery(route, DeliveryPosition);

            var decor = Group(level, "Decor");
            for (int i = 0; i < 9; i++) CloudAt(decor, new Vector2(-20f + i * 36f, 30f + (i % 3) * 5f));

            var player = Object.FindFirstObjectByType<PlayerController>();
            if (player == null)
            {
                Debug.LogError("[Level1Builder] FAILED: no PlayerController in the scene");
                return;
            }
            player.transform.position = Spawn;
            foreach (var sr in player.GetComponentsInChildren<SpriteRenderer>()) sr.sortingLayerName = "Player";
            // The ceiling checker must sit above the crouched head (0.41) and below the
            // standing head (1.28), so it sees exactly the space standing up would need.
            if (player.topObstacleCheck != null) player.topObstacleCheck.localPosition = new Vector3(0f, PlayerCeilingCheckY, 0f);

            SetUpCamera(player.transform);
            SetUpGame(player);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

            var roots = string.Join(", ", scene.GetRootGameObjects().Select(r => r.name));
            Debug.Log($"[Level1Builder] OK: built {Buildings.Length} buildings, {Packages.Length} packages; roots: {roots}");
        }

        private static Transform Group(Transform parent, string name)
        {
            var go = new GameObject(name).transform;
            go.SetParent(parent, false);
            return go;
        }

        private static Sprite LoadSprite(string name)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{Art}/{name}.png");
            if (sprite == null) Debug.LogWarning($"[Level1Builder] missing sprite {name}; run Generate Art first");
            return sprite;
        }

        private static SpriteRenderer AddSprite(GameObject go, string sprite, string sortingLayer, int order = 0)
        {
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite(sprite);
            sr.sortingLayerName = sortingLayer;
            sr.sortingOrder = order;
            return sr;
        }

        private static void Building(Transform parent, string name, float x0, float x1, float top, int color, float bottom)
        {
            float width = x1 - x0, height = top - bottom;
            var go = new GameObject(name) { layer = GroundLayer };
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3((x0 + x1) / 2f, (top + bottom) / 2f, 0f);

            var facade = AddSprite(go, $"facade_{color}", "Background");
            facade.drawMode = SpriteDrawMode.Tiled;
            facade.size = new Vector2(width, height);
            go.AddComponent<BoxCollider2D>().size = new Vector2(width, height);

            var edge = new GameObject("RoofEdge");
            edge.transform.SetParent(go.transform, false);
            edge.transform.position = new Vector3((x0 + x1) / 2f, top - 0.25f, 0f);
            var edgeSprite = AddSprite(edge, "roof_edge", "Background", 1);
            edgeSprite.drawMode = SpriteDrawMode.Tiled;
            edgeSprite.size = new Vector2(width + 0.4f, 0.5f);
        }

        private static void PackageAt(Transform parent, string name, Vector2 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 1.2f;
            AddSprite(go, "package", "Foreground");
            go.AddComponent<BoxCollider2D>().isTrigger = true;
            go.AddComponent<Package>();
        }

        private static void Spikes(Transform parent, float x0, float x1, float roof)
        {
            var go = new GameObject("Spikes");
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3((x0 + x1) / 2f, roof + 0.25f, 0f);
            var sr = AddSprite(go, "spikes", "Foreground");
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(x1 - x0, 0.5f);
            var box = go.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(x1 - x0 - 0.2f, 0.4f);
            go.AddComponent<Hazard>();
        }

        private static void BirdAt(Transform parent, Vector2 start, Vector2 travel)
        {
            var go = new GameObject("Bird");
            go.transform.SetParent(parent, false);
            go.transform.position = start;
            go.transform.localScale = Vector3.one * 1.6f;
            AddSprite(go, "bird", "Foreground", 5);
            var circle = go.AddComponent<CircleCollider2D>();
            circle.isTrigger = true;
            circle.radius = 0.3f;
            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            go.AddComponent<Hazard>();
            var bird = go.AddComponent<Bird>();
            Set(bird, "travel", p => p.vector2Value = travel);
        }

        private static void Trap(Transform parent, string path, Vector2 position, float scale)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning($"[Level1Builder] trap prefab missing, skipped: {path}");
                return;
            }
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.transform.position = position;
            go.transform.localScale = Vector3.one * scale;
            // The packs give the dangerous parts colliders; make them triggers that cost a life.
            foreach (var collider in go.GetComponentsInChildren<Collider2D>(true))
            {
                collider.isTrigger = true;
                if (collider.GetComponent<Hazard>() == null) collider.gameObject.AddComponent<Hazard>();
            }
        }

        private static void KillZone(Transform parent)
        {
            var go = new GameObject("KillZone");
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(130f, -22f, 0f);
            var box = go.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(500f, 8f);
            go.AddComponent<Hazard>();
        }

        private static void CheckpointAt(Transform parent, string name, Vector2 roof)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            // The checkpoint's position is where the player respawns: standing on the roof.
            go.transform.position = new Vector3(roof.x, roof.y + 1.4f, 0f);
            var box = go.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(1.5f, 4f);

            var flag = new GameObject("Flag");
            flag.transform.SetParent(go.transform, false);
            flag.transform.position = new Vector3(roof.x, roof.y + 1.5f, 0f);
            flag.transform.localScale = Vector3.one * 1.5f;
            var sr = AddSprite(flag, "flag", "Foreground");
            sr.color = new Color(0.85f, 0.85f, 0.85f);

            var checkpoint = go.AddComponent<Checkpoint>();
            Set(checkpoint, "flag", p => p.objectReferenceValue = sr);
        }

        private static void Delivery(Transform parent, Vector2 roof)
        {
            var go = new GameObject("DeliveryPoint");
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(roof.x, roof.y + 2f, 0f);
            go.transform.localScale = Vector3.one * 2f;
            AddSprite(go, "mailbox", "Foreground");
            var box = go.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(1f, 2f);
            go.AddComponent<DeliveryPoint>();
        }

        private static void CloudAt(Transform parent, Vector2 position)
        {
            var go = new GameObject("Cloud");
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(position.x, position.y, 5f);
            go.transform.localScale = Vector3.one * 3f;
            AddSprite(go, "cloud", "Background", -10).color = new Color(1f, 1f, 1f, 0.9f);
        }

        private static void SetUpCamera(Transform player)
        {
            var camera = Camera.main;
            if (camera == null)
            {
                Debug.LogWarning("[Level1Builder] no main camera");
                return;
            }
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.55f, 0.82f, 0.95f);
            camera.orthographic = true;
            camera.orthographicSize = 11f;
            var follow = camera.GetComponent<CameraFollow>() ?? camera.gameObject.AddComponent<CameraFollow>();
            Set(follow, "target", p => p.objectReferenceValue = player);
            camera.transform.position = new Vector3(player.position.x + 5f, player.position.y + 3f, -10f);
        }

        private static void SetUpGame(PlayerController player)
        {
            var go = new GameObject("Game");
            var manager = go.AddComponent<GameManager>();
            Set(manager, "player", p => p.objectReferenceValue = player);
            Set(manager, "startingLives", p => p.intValue = Lives);
            Set(manager, "parTimeSeconds", p => p.floatValue = ParSeconds);

            go.AddComponent<GameUI>();

            var audio = go.AddComponent<GameAudio>();
            SetClip(audio, "music", $"{Traps}/Epic Rock Track/NEW/Heart of Warrior NEW (Rock Loop).wav");
            SetClip(audio, "jump", $"{Traps}/Attack Jump & Hit Damage Human Sounds/Jump & Attack 1.wav");
            SetClip(audio, "hurt", $"{Traps}/Attack Jump & Hit Damage Human Sounds/Hit & Damage 5.wav");
            SetClip(audio, "gameOver", $"{Traps}/Attack Jump & Hit Damage Human Sounds/Hit & Damage 9.wav");
            SetClip(audio, "pickup", $"{Art}/pickup.wav");
            SetClip(audio, "delivered", $"{Art}/delivered.wav");
        }

        private static void SetClip(GameAudio audio, string field, string path)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip == null) Debug.LogWarning($"[Level1Builder] audio clip missing, left empty: {path}");
            Set(audio, field, p => p.objectReferenceValue = clip);
        }

        private static void Set(Object target, string field, System.Action<SerializedProperty> assign)
        {
            var so = new SerializedObject(target);
            var property = so.FindProperty(field);
            if (property == null)
            {
                Debug.LogError($"[Level1Builder] {target.GetType().Name} has no serialized field '{field}'");
                return;
            }
            assign(property);
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
