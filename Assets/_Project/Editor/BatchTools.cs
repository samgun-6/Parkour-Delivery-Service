using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ParkourDelivery.Editor
{
    /// <summary>
    /// Entry points for headless checks, run with
    /// <c>Unity.exe -batchmode -projectPath . -executeMethod ParkourDelivery.Editor.BatchTools.&lt;Method&gt;</c>.
    /// Each method logs lines prefixed with "[BatchTools]" and exits with 0 on success, 1 on failure.
    /// </summary>
    public static class BatchTools
    {
        private const string SceneFolder = "Assets/_Project/Scenes";
        private const string ShotFolder = "Logs/shots";

        /// <summary>
        /// Opens every scene under Assets/_Project/Scenes and reports missing scripts,
        /// missing prefab assets and object references that point to deleted assets.
        /// </summary>
        public static void ValidateScenes()
        {
            var problems = new List<string>();
            var scenes = AssetDatabase.FindAssets("t:Scene", new[] { SceneFolder })
                .Select(AssetDatabase.GUIDToAssetPath).ToList();

            foreach (var path in scenes)
            {
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                foreach (var root in scene.GetRootGameObjects())
                {
                    foreach (var t in root.GetComponentsInChildren<Transform>(true))
                    {
                        CheckGameObject(path, t.gameObject, problems);
                    }
                }
                Log($"checked {path}");
            }

            foreach (var p in problems) Log("PROBLEM " + p);
            Log(problems.Count == 0 ? "OK: no problems found" : $"FAILED: {problems.Count} problem(s)");
            EditorApplication.Exit(problems.Count == 0 ? 0 : 1);
        }

        private static void CheckGameObject(string scenePath, GameObject go, List<string> problems)
        {
            var where = $"{scenePath}:{GetPath(go.transform)}";

            int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
            if (missing > 0) problems.Add($"{where}: {missing} missing script(s)");

            if (PrefabUtility.IsPrefabAssetMissing(go)) problems.Add($"{where}: prefab asset missing");

            foreach (var component in go.GetComponents<Component>())
            {
                if (component == null) continue;
                var so = new SerializedObject(component);
                var prop = so.GetIterator();
                while (prop.NextVisible(true))
                {
                    if (prop.propertyType != SerializedPropertyType.ObjectReference) continue;
                    if (prop.objectReferenceValue == null && prop.objectReferenceInstanceIDValue != 0)
                    {
                        problems.Add($"{where}: {component.GetType().Name}.{prop.propertyPath} is a broken reference");
                    }
                }
            }
        }

        /// <summary>
        /// Renders the main camera of a scene to PNG files in Logs/shots.
        /// Command-line args: <c>-scene &lt;path&gt;</c> (default Level1) and
        /// <c>-shots "x,y;x,y;..."</c> world positions to centre the camera on
        /// (default: the camera's own position). Optional <c>-ortho &lt;size&gt;</c>.
        /// </summary>
        public static void CaptureShots()
        {
            var args = System.Environment.GetCommandLineArgs();
            string scenePath = GetArg(args, "-scene") ?? $"{SceneFolder}/Level1.unity";
            string shotsArg = GetArg(args, "-shots");
            string orthoArg = GetArg(args, "-ortho");

            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var cam = Camera.main != null ? Camera.main : Object.FindFirstObjectByType<Camera>();
            if (cam == null)
            {
                Log("FAILED: no camera in scene");
                EditorApplication.Exit(1);
                return;
            }

            if (orthoArg != null && float.TryParse(orthoArg, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var ortho))
            {
                cam.orthographicSize = ortho;
            }

            var positions = new List<Vector2>();
            if (string.IsNullOrEmpty(shotsArg))
            {
                positions.Add(cam.transform.position);
            }
            else
            {
                foreach (var pair in shotsArg.Split(';'))
                {
                    var xy = pair.Split(',');
                    positions.Add(new Vector2(
                        float.Parse(xy[0], System.Globalization.CultureInfo.InvariantCulture),
                        float.Parse(xy[1], System.Globalization.CultureInfo.InvariantCulture)));
                }
            }

            Directory.CreateDirectory(ShotFolder);
            const int width = 1280, height = 720;
            var rt = new RenderTexture(width, height, 24);
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            cam.targetTexture = rt;

            for (int i = 0; i < positions.Count; i++)
            {
                var p = positions[i];
                cam.transform.position = new Vector3(p.x, p.y, cam.transform.position.z);
                cam.Render();
                RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();
                string file = Path.Combine(ShotFolder, $"shot_{i:00}.png");
                File.WriteAllBytes(file, tex.EncodeToPNG());
                Log($"wrote {file} at ({p.x}, {p.y})");
            }

            cam.targetTexture = null;
            RenderTexture.active = null;
            Log("OK: shots captured");
            EditorApplication.Exit(0);
        }

        private static string GetArg(string[] args, string name)
        {
            int i = System.Array.IndexOf(args, name);
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
        }

        private static string GetPath(Transform t)
        {
            return t.parent == null ? t.name : GetPath(t.parent) + "/" + t.name;
        }

        private static void Log(string message)
        {
            Debug.Log("[BatchTools] " + message);
        }
    }
}
