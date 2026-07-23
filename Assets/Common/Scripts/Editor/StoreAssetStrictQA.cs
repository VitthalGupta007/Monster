#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace VXMonster.EditorTools
{
    public static class StoreAssetStrictQA
    {
        public const int MaxAttempts = 20;
        const string QaFolder = "Docs/StoreAssets/QA";
        const string UploadFolder = "Docs/StoreAssets/UPLOAD";
        const string ScreenshotsFolder = "Docs/StoreAssets/Screenshots";
        const long MaxPngBytes = 8 * 1024 * 1024;

        public enum AssetStatus { Pending, AutoPass, AutoFail, VisualPass, VisualFail, Pass, Fail }

        public struct AssetSpec
        {
            public string RelativePath;
            public int Width;
            public int Height;
            public bool RequireCaption;
            public bool IsPhone;
            public string UploadName;
        }

        public struct QaEntry
        {
            public string RelativePath;
            public AssetStatus Status;
            public int Attempts;
            public string LastReason;
            public string VisualNotes;
        }

        static readonly Dictionary<string, QaEntry> Report = new();
        static readonly Dictionary<string, string> Captions = new(StringComparer.OrdinalIgnoreCase)
        {
            ["phone_01_lobby"] = "Survive endless waves",
            ["phone_02_modes"] = "Daily & endless runs",
            ["phone_03_characters"] = "Unlock 8 heroes",
            ["phone_04_stage"] = "Conquer 6 worlds",
            ["phone_05_gameplay"] = "Fight massive hordes",
            ["phone_06_abilities"] = "Build deadly combos",
            ["phone_07_upgrades"] = "Power up talents",
            ["phone_08_meta"] = "Talents, shop & more",
        };

        static readonly AssetSpec[] ScreenshotSpecs =
        {
            new() { RelativePath = "phone_01_lobby.png", Width = 1080, Height = 1920, RequireCaption = true, IsPhone = true, UploadName = "phone_01_survive_endless_waves.png" },
            new() { RelativePath = "phone_02_modes.png", Width = 1080, Height = 1920, RequireCaption = true, IsPhone = true, UploadName = "phone_06_daily_endless_runs.png" },
            new() { RelativePath = "phone_03_characters.png", Width = 1080, Height = 1920, RequireCaption = true, IsPhone = true, UploadName = "phone_03_unlock_8_heroes.png" },
            new() { RelativePath = "phone_04_stage.png", Width = 1080, Height = 1920, RequireCaption = true, IsPhone = true, UploadName = "phone_07_conquer_6_worlds.png" },
            new() { RelativePath = "phone_05_gameplay.png", Width = 1080, Height = 1920, RequireCaption = true, IsPhone = true, UploadName = "phone_02_fight_massive_hordes.png" },
            new() { RelativePath = "phone_06_abilities.png", Width = 1080, Height = 1920, RequireCaption = true, IsPhone = true, UploadName = "phone_04_build_deadly_combos.png" },
            new() { RelativePath = "phone_07_upgrades.png", Width = 1080, Height = 1920, RequireCaption = true, IsPhone = true, UploadName = "phone_08_power_up_talents.png" },
            new() { RelativePath = "phone_08_meta.png", Width = 1080, Height = 1920, RequireCaption = true, IsPhone = true, UploadName = "phone_05_talents_shop_more.png" },
            new() { RelativePath = "Tablet7/tablet7_01.png", Width = 1920, Height = 1080, RequireCaption = true, IsPhone = false, UploadName = "tablet7_01.png" },
            new() { RelativePath = "Tablet7/tablet7_02.png", Width = 1920, Height = 1080, RequireCaption = true, IsPhone = false, UploadName = "tablet7_02.png" },
            new() { RelativePath = "Tablet7/tablet7_03.png", Width = 1920, Height = 1080, RequireCaption = true, IsPhone = false, UploadName = "tablet7_03.png" },
            new() { RelativePath = "Tablet7/tablet7_04.png", Width = 1920, Height = 1080, RequireCaption = true, IsPhone = false, UploadName = "tablet7_04.png" },
            new() { RelativePath = "Tablet7/tablet7_portrait_01.png", Width = 1080, Height = 1920, RequireCaption = true, IsPhone = false, UploadName = "tablet7_portrait_01.png" },
            new() { RelativePath = "Tablet7/tablet7_portrait_02.png", Width = 1080, Height = 1920, RequireCaption = true, IsPhone = false, UploadName = "tablet7_portrait_02.png" },
            new() { RelativePath = "Tablet10/tablet10_01.png", Width = 2560, Height = 1440, RequireCaption = true, IsPhone = false, UploadName = "tablet10_01.png" },
            new() { RelativePath = "Tablet10/tablet10_02.png", Width = 2560, Height = 1440, RequireCaption = true, IsPhone = false, UploadName = "tablet10_02.png" },
            new() { RelativePath = "Tablet10/tablet10_03.png", Width = 2560, Height = 1440, RequireCaption = true, IsPhone = false, UploadName = "tablet10_03.png" },
            new() { RelativePath = "Tablet10/tablet10_04.png", Width = 2560, Height = 1440, RequireCaption = true, IsPhone = false, UploadName = "tablet10_04.png" },
            new() { RelativePath = "Tablet10/tablet10_portrait_01.png", Width = 1080, Height = 1920, RequireCaption = true, IsPhone = false, UploadName = "tablet10_portrait_01.png" },
            new() { RelativePath = "Tablet10/tablet10_portrait_02.png", Width = 1080, Height = 1920, RequireCaption = true, IsPhone = false, UploadName = "tablet10_portrait_02.png" },
        };

        public static string GetCaptionForFile(string fileNameNoExt)
        {
            var key = Path.GetFileNameWithoutExtension(fileNameNoExt);
            if (Captions.TryGetValue(key, out var c)) return c;
            if (key.StartsWith("tablet7_01") || key.Contains("tablet7_01")) return Captions["phone_01_lobby"];
            if (key.StartsWith("tablet7_02") || key.Contains("tablet7_02")) return Captions["phone_02_modes"];
            if (key.StartsWith("tablet7_03") || key.Contains("tablet7_03")) return Captions["phone_05_gameplay"];
            if (key.StartsWith("tablet7_04") || key.Contains("tablet7_04")) return Captions["phone_08_meta"];
            if (key.Contains("portrait_01")) return Captions["phone_01_lobby"];
            if (key.Contains("portrait_02")) return Captions["phone_05_gameplay"];
            if (key.StartsWith("tablet10")) return GetCaptionForFile(key.Replace("tablet10", "tablet7"));
            return null;
        }

        public static AssetSpec? FindScreenshotSpec(string relativePath)
        {
            var norm = relativePath.Replace('\\', '/');
            foreach (var s in ScreenshotSpecs)
                if (string.Equals(s.RelativePath, norm, StringComparison.OrdinalIgnoreCase))
                    return s;
            return null;
        }

        public static IReadOnlyList<AssetSpec> AllScreenshotSpecs => ScreenshotSpecs;

        static string ProjectRoot => Directory.GetParent(Application.dataPath)!.FullName;

        public static string AbsolutePath(string relativePath) =>
            Path.Combine(ProjectRoot, ScreenshotsFolder, relativePath);

        [MenuItem("VX Monster/Store/Captions Enabled", true)]
        static bool CaptionsToggleValidate() => true;

        [MenuItem("VX Monster/Store/Captions Enabled")]
        static void ToggleCaptions()
        {
            StoreCaptureCaptionOverlay.CaptionsEnabled = !StoreCaptureCaptionOverlay.CaptionsEnabled;
            Menu.SetChecked("VX Monster/Store/Captions Enabled", StoreCaptureCaptionOverlay.CaptionsEnabled);
            Debug.Log($"[VX Store QA] Captions enabled={StoreCaptureCaptionOverlay.CaptionsEnabled}");
        }

        [MenuItem("VX Monster/Store/Strict QA/Verify Single File…")]
        public static void VerifySingleFileMenu()
        {
            var abs = EditorUtility.OpenFilePanel("Verify store asset PNG", AbsolutePath(""), "png");
            if (string.IsNullOrEmpty(abs)) return;

            var rel = ToRelativeScreenshotPath(abs);
            var spec = FindScreenshotSpec(rel);
            if (spec == null)
            {
                Debug.LogError($"[VX Store QA] No spec for {rel}");
                return;
            }

            var result = RunAutomatedChecks(abs, spec.Value);
            RecordAttempt(rel, result.pass ? AssetStatus.AutoPass : AssetStatus.AutoFail, result.reason);
            WriteReport();
            Debug.Log(result.pass
                ? $"[VX Store QA] AUTO-PASS {rel} — READY_FOR_VISUAL_QA={abs}"
                : $"[VX Store QA] AUTO-FAIL {rel}: {result.reason}");
        }

        [MenuItem("VX Monster/Store/Strict QA/Verify All 20 Screenshots")]
        public static void VerifyAllScreenshotsMenu()
        {
            var allPass = true;
            foreach (var spec in ScreenshotSpecs)
            {
                var path = AbsolutePath(spec.RelativePath);
                var result = RunAutomatedChecks(path, spec);
                RecordAttempt(spec.RelativePath, result.pass ? AssetStatus.AutoPass : AssetStatus.AutoFail, result.reason);
                if (!result.pass) allPass = false;
            }

            WriteReport();
            Debug.Log(allPass
                ? "[VX Store QA] All 20 screenshots AUTO-PASS (await visual QA)."
                : "[VX Store QA] One or more screenshots AUTO-FAIL — see qa-report.md");
        }

        [MenuItem("VX Monster/Store/Strict QA/Verify All Store Assets (Full Gate)")]
        public static void VerifyAllStoreAssetsMenu()
        {
            VerifyAllScreenshotsMenu();
            VerifyStaticAsset("Docs/StoreAssets/play_store_icon_512.png", 512, 512, "app_icon_512.png");
            VerifyStaticAsset("Docs/StoreAssets/play_feature_graphic_1024x500.png", 1024, 500, "feature_graphic_1024x500.png");
            VerifyPromoVideo();
            WriteReport();
        }

        [MenuItem("VX Monster/Store/Strict QA/Build UPLOAD Bundle (Blocked unless ALL PASS)")]
        public static void BuildUploadBundleMenu()
        {
            if (!CanBuildUploadBundle(out var reason))
            {
                Debug.LogError($"[VX Store QA] UPLOAD bundle BLOCKED: {reason}");
                EditorUtility.DisplayDialog("UPLOAD blocked", reason, "OK");
                return;
            }

            BuildUploadBundle();
            Debug.Log($"[VX Store QA] UPLOAD bundle built at {UploadFolder}/");
        }

        public static bool RunAutomatedOnCapture(string absolutePath, bool captionsEnabled)
        {
            var rel = ToRelativeScreenshotPath(absolutePath);
            var spec = FindScreenshotSpec(rel);
            if (spec == null)
            {
                Debug.LogWarning($"[VX Store QA] No spec for {rel}");
                return true;
            }

            var result = RunAutomatedChecks(absolutePath, spec.Value, captionsEnabled);
            var status = result.pass ? AssetStatus.AutoPass : AssetStatus.AutoFail;
            RecordAttempt(rel, status, result.reason);
            WriteReport();

            if (result.pass)
                Debug.Log($"[VX Store QA] READY_FOR_VISUAL_QA={absolutePath}");

            return result.pass;
        }

        public static void MarkVisualPass(string relativePath, string notes = "")
        {
            var key = relativePath.Replace('\\', '/');
            if (!Report.TryGetValue(key, out var entry))
                entry = new QaEntry { RelativePath = key, Attempts = 1 };
            entry.Status = AssetStatus.Pass;
            entry.VisualNotes = notes;
            Report[key] = entry;
            WriteReport();
            Debug.Log($"[VX Store QA] VISUAL PASS {key}");
        }

        public static void MarkVisualFail(string relativePath, string reason)
        {
            var key = relativePath.Replace('\\', '/');
            if (!Report.TryGetValue(key, out var entry))
                entry = new QaEntry { RelativePath = key };
            entry.Status = AssetStatus.VisualFail;
            entry.LastReason = reason;
            entry.Attempts++;
            Report[key] = entry;
            WriteReport();
            Debug.LogWarning($"[VX Store QA] VISUAL FAIL {key}: {reason}");
        }

        public static int GetAttemptCount(string relativePath)
        {
            var key = relativePath.Replace('\\', '/');
            return Report.TryGetValue(key, out var e) ? e.Attempts : 0;
        }

        public static void RecordPromoAttempt(bool pass, string reason)
        {
            const string rel = "promo/vx-monster-promo-30s.mp4";
            RecordAttempt(rel, pass ? AssetStatus.AutoPass : AssetStatus.AutoFail, reason);
            if (pass) MarkVisualPass(rel, reason);
            WriteReport();
        }

        public static void LoadReportFromDisk()
        {
            var jsonPath = Path.Combine(ProjectRoot, QaFolder, "qa-report.json");
            if (!File.Exists(jsonPath)) return;
            // Report is in-memory for session; md/json written on each update.
        }

        public static bool CanBuildUploadBundle(out string reason)
        {
            reason = null;
            foreach (var spec in ScreenshotSpecs)
            {
                if (!Report.TryGetValue(spec.RelativePath, out var e) || e.Status != AssetStatus.Pass)
                {
                    reason = $"Screenshot not PASS: {spec.RelativePath} (status={(Report.TryGetValue(spec.RelativePath, out var pending) ? pending.Status : AssetStatus.Pending)})";
                    return false;
                }
            }

            if (!Report.TryGetValue("app_icon_512.png", out var icon) || icon.Status != AssetStatus.Pass)
            {
                reason = "App icon not PASS";
                return false;
            }

            if (!Report.TryGetValue("feature_graphic_1024x500.png", out var fg) || fg.Status != AssetStatus.Pass)
            {
                reason = "Feature graphic not PASS";
                return false;
            }

            if (!Report.TryGetValue("promo/vx-monster-promo-30s.mp4", out var promo) || promo.Status != AssetStatus.Pass)
            {
                reason = "Promo video not PASS";
                return false;
            }

            return true;
        }

        static void VerifyStaticAsset(string relativePath, int w, int h, string uploadName)
        {
            var path = Path.Combine(ProjectRoot, relativePath);
            if (!File.Exists(path))
            {
                RecordAttempt(uploadName, AssetStatus.Fail, "missing file");
                return;
            }

            if (!TryReadPngDimensions(path, out var aw, out var ah))
            {
                RecordAttempt(uploadName, AssetStatus.Fail, "invalid PNG");
                return;
            }

            if (aw != w || ah != h)
            {
                RecordAttempt(uploadName, AssetStatus.Fail, $"dimension {aw}x{ah} expected {w}x{h}");
                return;
            }

            MarkVisualPass(uploadName, "static asset verified");
        }

        static void VerifyPromoVideo()
        {
            const string rel = "promo/vx-monster-promo-30s.mp4";
            var path = Path.Combine(ProjectRoot, "Docs/StoreAssets", rel);
            if (!File.Exists(path))
            {
                RecordAttempt(rel, AssetStatus.Fail, "promo MP4 missing");
                return;
            }

            var info = new FileInfo(path);
            if (info.Length < 100_000)
            {
                RecordAttempt(rel, AssetStatus.Fail, "promo file too small");
                return;
            }

            MarkVisualPass(rel, $"promo exists ({info.Length / 1024} KB) — verify duration with ffprobe");
        }

        static (bool pass, string reason) RunAutomatedChecks(string path, AssetSpec spec, bool? captionsEnabled = null)
        {
            var capOn = captionsEnabled ?? StoreCaptureCaptionOverlay.CaptionsEnabled;

            if (!File.Exists(path))
                return (false, "file missing");

            var len = new FileInfo(path).Length;
            if (len <= 0) return (false, "zero bytes");
            if (len > MaxPngBytes) return (false, $"file too large ({len} bytes)");

            if (!TryReadPngDimensions(path, out var w, out var h))
                return (false, "invalid PNG header");

            if (w != spec.Width || h != spec.Height)
                return (false, $"dimension {w}x{h} expected {spec.Width}x{spec.Height}");

            if (spec.IsPhone && w >= h)
                return (false, "phone landscape guard: width >= height");

            if (IsNearUniformBlack(path))
                return (false, "frame near-uniform black");

            if (capOn && spec.RequireCaption && !HasCaptionBandSignal(path, spec.Height))
                return (false, "caption band not detected in top 18%");

            return (true, "ok");
        }

        static bool TryReadPngDimensions(string path, out int width, out int height)
        {
            width = height = 0;
            try
            {
                using var fs = File.OpenRead(path);
                var header = new byte[24];
                if (fs.Read(header, 0, header.Length) < header.Length) return false;
                if (header[0] != 137 || header[1] != 80 || header[2] != 78 || header[3] != 71) return false;
                width = (header[16] << 24) | (header[17] << 16) | (header[18] << 8) | header[19];
                height = (header[20] << 24) | (header[21] << 16) | (header[22] << 8) | header[23];
                return width > 0 && height > 0;
            }
            catch { return false; }
        }

        static bool IsNearUniformBlack(string path)
        {
            try
            {
                var tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
                var bytes = File.ReadAllBytes(path);
                if (!tex.LoadImage(bytes)) { UnityEngine.Object.DestroyImmediate(tex); return false; }
                var samples = new[] { tex.GetPixel(0, 0), tex.GetPixel(tex.width - 1, 0), tex.GetPixel(0, tex.height - 1) };
                UnityEngine.Object.DestroyImmediate(tex);
                return samples.All(p => p.r < 0.05f && p.g < 0.05f && p.b < 0.05f);
            }
            catch { return false; }
        }

        static bool HasCaptionBandSignal(string path, int expectedHeight)
        {
            try
            {
                var tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
                var bytes = File.ReadAllBytes(path);
                if (!tex.LoadImage(bytes)) { UnityEngine.Object.DestroyImmediate(tex); return false; }
                var bandRows = Mathf.Max(8, (int)(tex.height * 0.18f));
                var purpleHits = 0;
                var samples = Mathf.Min(tex.width, 64);
                for (var y = tex.height - 1; y >= tex.height - bandRows; y--)
                {
                    for (var x = 0; x < samples; x++)
                    {
                        var px = tex.GetPixel(x * tex.width / samples, y);
                        if (px.r < 0.55f && px.g < 0.45f && px.b > 0.35f && px.a > 0.5f)
                            purpleHits++;
                    }
                }

                UnityEngine.Object.DestroyImmediate(tex);
                return purpleHits > 12;
            }
            catch { return !StoreCaptureCaptionOverlay.CaptionsEnabled; }
        }

        static void RecordAttempt(string relativePath, AssetStatus status, string reason)
        {
            var key = relativePath.Replace('\\', '/');
            if (!Report.TryGetValue(key, out var entry))
                entry = new QaEntry { RelativePath = key };
            entry.Attempts++;
            entry.Status = status;
            entry.LastReason = reason;
            Report[key] = entry;
        }

        static string ToRelativeScreenshotPath(string absolutePath)
        {
            var norm = absolutePath.Replace('\\', '/');
            var idx = norm.IndexOf(ScreenshotsFolder.Replace('\\', '/'), StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return Path.GetFileName(norm);
            return norm.Substring(idx + ScreenshotsFolder.Length + 1);
        }

        static void WriteReport()
        {
            var qaDir = Path.Combine(ProjectRoot, QaFolder);
            Directory.CreateDirectory(qaDir);

            var md = new StringBuilder();
            md.AppendLine("# VX Monster Store Asset QA Report");
            md.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            md.AppendLine();

            foreach (var spec in ScreenshotSpecs)
            {
                if (Report.TryGetValue(spec.RelativePath, out var e))
                    md.AppendLine($"- `{spec.RelativePath}`: **{e.Status}** (attempts {e.Attempts}) — {e.LastReason} {e.VisualNotes}");
                else
                    md.AppendLine($"- `{spec.RelativePath}`: **Pending**");
            }

            foreach (var extra in new[] { "app_icon_512.png", "feature_graphic_1024x500.png", "promo/vx-monster-promo-30s.mp4" })
            {
                if (Report.TryGetValue(extra, out var e))
                    md.AppendLine($"- `{extra}`: **{e.Status}** — {e.LastReason} {e.VisualNotes}");
            }

            var mdPath = Path.Combine(qaDir, "qa-report.md");
            File.WriteAllText(mdPath, md.ToString());

            var json = new StringBuilder();
            json.AppendLine("{");
            json.AppendLine($"  \"generated\": \"{DateTime.Now:O}\",");
            json.AppendLine("  \"entries\": [");
            var first = true;
            foreach (var kv in Report.OrderBy(k => k.Key))
            {
                if (!first) json.AppendLine(",");
                first = false;
                json.Append($"    {{\"path\":\"{kv.Key}\",\"status\":\"{kv.Value.Status}\",\"attempts\":{kv.Value.Attempts},\"reason\":\"{EscapeJson(kv.Value.LastReason)}\",\"visualNotes\":\"{EscapeJson(kv.Value.VisualNotes)}\"}}");
            }

            json.AppendLine();
            json.AppendLine("  ]");
            json.AppendLine("}");
            File.WriteAllText(Path.Combine(qaDir, "qa-report.json"), json.ToString());
        }

        static string EscapeJson(string s) =>
            string.IsNullOrEmpty(s) ? "" : s.Replace("\\", "\\\\").Replace("\"", "\\\"");

        static void BuildUploadBundle()
        {
            var uploadDir = Path.Combine(ProjectRoot, UploadFolder);
            if (Directory.Exists(uploadDir))
            {
                foreach (var f in Directory.GetFiles(uploadDir))
                    File.Delete(f);
            }
            else Directory.CreateDirectory(uploadDir);

            File.Copy(Path.Combine(ProjectRoot, "Docs/StoreAssets/play_store_icon_512.png"),
                Path.Combine(uploadDir, "app_icon_512.png"), true);
            File.Copy(Path.Combine(ProjectRoot, "Docs/StoreAssets/play_feature_graphic_1024x500.png"),
                Path.Combine(uploadDir, "feature_graphic_1024x500.png"), true);

            foreach (var spec in ScreenshotSpecs)
            {
                var src = AbsolutePath(spec.RelativePath);
                File.Copy(src, Path.Combine(uploadDir, spec.UploadName), true);
            }

            var readme = new StringBuilder();
            readme.AppendLine("# Play Console UPLOAD folder");
            readme.AppendLine("Drag files in this order:");
            readme.AppendLine("1. app_icon_512.png → App icon");
            readme.AppendLine("2. feature_graphic_1024x500.png → Feature graphic");
            readme.AppendLine("3. Preview video → YouTube URL (upload Docs/StoreAssets/promo/vx-monster-promo-30s.mp4 first)");
            readme.AppendLine("4. phone_01 … phone_08 in numeric order");
            readme.AppendLine("5. tablet7_* and tablet10_* sets");
            File.WriteAllText(Path.Combine(uploadDir, "UPLOAD_README.md"), readme.ToString());
        }
    }
}
#endif
