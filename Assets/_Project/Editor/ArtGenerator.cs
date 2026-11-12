using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ParkourDelivery.Editor
{
    /// <summary>
    /// Draws the game's own pixel art and sound effects in code and saves them under
    /// Assets/_Project/Art/Generated. Re-running overwrites the files with the same result.
    /// Run from the menu or with -executeMethod ParkourDelivery.Editor.ArtGenerator.GenerateAll.
    /// </summary>
    public static class ArtGenerator
    {
        public const string Folder = "Assets/_Project/Art/Generated";
        public const int PixelsPerUnit = 16;

        // Facade colours loosely follow the coloured buildings in the design sketch.
        private static readonly Color32[] FacadeColors =
        {
            new Color32(238, 214, 92, 255),  // yellow
            new Color32(120, 72, 34, 255),   // brown
            new Color32(186, 44, 40, 255),   // red
            new Color32(92, 108, 140, 255),  // blue grey
        };

        private static readonly Color32 Clear = new Color32(0, 0, 0, 0);

        [MenuItem("Parkour Delivery/Generate Art")]
        public static void GenerateAllFromMenu()
        {
            Generate();
        }

        public static void GenerateAll()
        {
            Generate();
            EditorApplication.Exit(0);
        }

        private static void Generate()
        {
            Directory.CreateDirectory(Folder);

            for (int i = 0; i < FacadeColors.Length; i++)
            {
                Save($"facade_{i}", Facade(FacadeColors[i]));
            }
            Save("roof_edge", RoofEdge());
            Save("spikes", Spikes());
            Save("package", PackageBox());
            Save("bird", BirdSprite());
            Save("flag", Flag());
            Save("mailbox", Mailbox());
            Save("cloud", Cloud());

            SaveWav("pickup", Tones(new[] { 880f, 1320f }, 0.07f));
            SaveWav("delivered", Tones(new[] { 523f, 659f, 784f, 1047f }, 0.12f));

            AssetDatabase.Refresh();
            ConfigureImporters();
            Debug.Log("[ArtGenerator] OK: art generated in " + Folder);
        }

        // 32x32 tile: wall with two windows, frames and a sill.
        private static Texture2D Facade(Color32 wall)
        {
            var tex = New(32, 32, wall);
            var frame = Shade(wall, 0.55f);
            var glass = new Color32(196, 222, 246, 255);
            var shine = new Color32(236, 246, 255, 255);
            foreach (int x0 in new[] { 4, 18 })
            {
                Rect(tex, x0, 9, 10, 16, frame);
                Rect(tex, x0 + 1, 10, 8, 14, glass);
                Rect(tex, x0 + 4, 10, 1, 14, frame); // mullion
                Rect(tex, x0 + 1, 21, 3, 2, shine);
                Rect(tex, x0 - 1, 8, 12, 1, Shade(wall, 0.75f)); // sill
            }
            return tex;
        }

        private static Texture2D RoofEdge()
        {
            var tex = New(16, 8, new Color32(58, 58, 64, 255));
            Rect(tex, 0, 6, 16, 2, new Color32(96, 96, 104, 255));
            Rect(tex, 0, 0, 16, 1, new Color32(36, 36, 40, 255));
            return tex;
        }

        private static Texture2D Spikes()
        {
            var tex = New(16, 8, Clear);
            var metal = new Color32(176, 182, 190, 255);
            var dark = new Color32(98, 104, 112, 255);
            for (int s = 0; s < 2; s++)
            {
                int cx = s * 8 + 4;
                for (int y = 0; y < 8; y++)
                {
                    int half = 4 - y / 2;
                    for (int x = cx - half; x < cx + half; x++) tex.SetPixel(x, y, x < cx ? metal : dark);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D PackageBox()
        {
            var tex = New(16, 16, Clear);
            var cardboard = new Color32(196, 142, 76, 255);
            var edge = new Color32(126, 84, 40, 255);
            var tape = new Color32(236, 214, 150, 255);
            Rect(tex, 1, 1, 14, 13, edge);
            Rect(tex, 2, 2, 12, 11, cardboard);
            Rect(tex, 7, 2, 2, 11, tape);
            Rect(tex, 2, 9, 12, 1, edge);
            Rect(tex, 3, 4, 3, 2, new Color32(250, 250, 250, 255)); // label
            return tex;
        }

        private static Texture2D BirdSprite()
        {
            var tex = New(16, 12, Clear);
            var body = new Color32(44, 44, 54, 255);
            var wing = new Color32(76, 76, 92, 255);
            Rect(tex, 4, 4, 8, 4, body);      // body
            Rect(tex, 11, 6, 3, 3, body);     // head
            Rect(tex, 14, 6, 2, 1, new Color32(242, 170, 40, 255)); // beak
            tex.SetPixel(12, 7, Color.white); // eye
            Rect(tex, 1, 5, 3, 2, body);      // tail
            Rect(tex, 5, 8, 5, 2, wing);      // wing up
            Rect(tex, 6, 10, 3, 2, wing);
            tex.Apply();
            return tex;
        }

        private static Texture2D Flag()
        {
            var tex = New(16, 32, Clear);
            Rect(tex, 2, 0, 2, 32, new Color32(90, 90, 96, 255)); // pole
            Rect(tex, 4, 20, 11, 10, new Color32(255, 255, 255, 255)); // flag (tinted at runtime)
            Rect(tex, 4, 20, 11, 1, new Color32(200, 200, 200, 255));
            return tex;
        }

        private static Texture2D Mailbox()
        {
            var tex = New(16, 32, Clear);
            Rect(tex, 7, 0, 2, 14, new Color32(90, 90, 96, 255)); // post
            var red = new Color32(214, 48, 44, 255);
            Rect(tex, 1, 14, 14, 14, Shade(red, 0.6f));
            Rect(tex, 2, 15, 12, 12, red);
            Rect(tex, 3, 22, 10, 2, new Color32(40, 20, 20, 255)); // slot
            Rect(tex, 5, 17, 6, 3, new Color32(250, 220, 90, 255)); // badge
            return tex;
        }

        private static Texture2D Cloud()
        {
            var tex = New(32, 16, Clear);
            var white = new Color32(250, 252, 255, 255);
            Disc(tex, 9, 6, 6, white);
            Disc(tex, 17, 9, 7, white);
            Disc(tex, 24, 6, 5, white);
            Rect(tex, 4, 1, 25, 6, white);
            tex.Apply();
            return tex;
        }

        private static Texture2D New(int w, int h, Color32 fill)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var pixels = new Color32[w * h];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = fill;
            tex.SetPixels32(pixels);
            tex.Apply();
            return tex;
        }

        private static void Rect(Texture2D tex, int x, int y, int w, int h, Color32 c)
        {
            for (int yy = y; yy < y + h; yy++)
            for (int xx = x; xx < x + w; xx++)
            {
                if (xx >= 0 && yy >= 0 && xx < tex.width && yy < tex.height) tex.SetPixel(xx, yy, c);
            }
            tex.Apply();
        }

        private static void Disc(Texture2D tex, int cx, int cy, int r, Color32 c)
        {
            for (int y = cy - r; y <= cy + r; y++)
            for (int x = cx - r; x <= cx + r; x++)
            {
                if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r && x >= 0 && y >= 0 && x < tex.width && y < tex.height)
                    tex.SetPixel(x, y, c);
            }
        }

        private static Color32 Shade(Color32 c, float f)
        {
            return new Color32((byte)(c.r * f), (byte)(c.g * f), (byte)(c.b * f), c.a);
        }

        private static void Save(string name, Texture2D tex)
        {
            File.WriteAllBytes($"{Folder}/{name}.png", tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void ConfigureImporters()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { Folder }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                bool tiled = path.Contains("facade_") || path.Contains("roof_edge") || path.Contains("spikes");
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = PixelsPerUnit;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.wrapMode = tiled ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                // Tiled draw mode needs a full-rect mesh.
                settings.spriteMeshType = tiled ? SpriteMeshType.FullRect : SpriteMeshType.Tight;
                settings.spriteAlignment = (int)SpriteAlignment.Center;
                importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
            }
        }

        // A short square-ish chime: one tone per frequency, each with a quick decay.
        private static float[] Tones(float[] frequencies, float toneSeconds)
        {
            const int rate = 44100;
            int perTone = (int)(rate * toneSeconds);
            var samples = new float[perTone * frequencies.Length];
            for (int t = 0; t < frequencies.Length; t++)
            for (int i = 0; i < perTone; i++)
            {
                float time = i / (float)rate;
                float envelope = Mathf.Exp(-time * 18f);
                float wave = Mathf.Sin(2f * Mathf.PI * frequencies[t] * time);
                wave += 0.3f * Mathf.Sin(4f * Mathf.PI * frequencies[t] * time);
                samples[t * perTone + i] = 0.45f * envelope * wave;
            }
            return samples;
        }

        private static void SaveWav(string name, float[] samples)
        {
            const int rate = 44100;
            using (var stream = new FileStream($"{Folder}/{name}.wav", FileMode.Create))
            using (var w = new BinaryWriter(stream))
            {
                int dataBytes = samples.Length * 2;
                w.Write(new[] { 'R', 'I', 'F', 'F' });
                w.Write(36 + dataBytes);
                w.Write(new[] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });
                w.Write(16);
                w.Write((short)1);      // PCM
                w.Write((short)1);      // mono
                w.Write(rate);
                w.Write(rate * 2);      // byte rate
                w.Write((short)2);      // block align
                w.Write((short)16);     // bits per sample
                w.Write(new[] { 'd', 'a', 't', 'a' });
                w.Write(dataBytes);
                foreach (var s in samples) w.Write((short)(Mathf.Clamp(s, -1f, 1f) * short.MaxValue));
            }
        }
    }
}
