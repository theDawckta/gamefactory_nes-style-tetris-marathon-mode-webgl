using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GameFactory.EditorTools
{
    /// <summary>
    /// Makes the operator's "ZekeLayout" Unity Editor window layout available in this project's
    /// Window > Layouts menu on any machine.
    ///
    /// Unity window layouts live in the machine-global Editor preferences folder, NOT inside a
    /// project, so a layout saved on one machine never travels with the repo. This installer copies
    /// the version-controlled ZekeLayout.wlt (shipped beside this script) into that global folder on
    /// editor load, but only when it is missing or its contents differ -- so it is a one-time install
    /// per machine and never fights a layout the user has since customised in-menu.
    /// </summary>
    [InitializeOnLoad]
    public static class ZekeLayoutInstaller
    {
        private const string LayoutName = "ZekeLayout";

        static ZekeLayoutInstaller()
        {
            // Defer past the asset-import/domain-reload storm so AssetDatabase queries are valid.
            EditorApplication.delayCall += Install;
        }

        private static void Install()
        {
            try
            {
                var sourcePath = FindSourceWlt();
                if (sourcePath == null || !File.Exists(sourcePath)) return;

                var destDir = GetGlobalLayoutsDir();
                if (string.IsNullOrEmpty(destDir)) return;
                Directory.CreateDirectory(destDir);

                var destPath = Path.Combine(destDir, LayoutName + ".wlt");
                if (File.Exists(destPath) &&
                    File.ReadAllText(destPath) == File.ReadAllText(sourcePath))
                {
                    return; // Already installed and identical -- nothing to do.
                }

                File.Copy(sourcePath, destPath, overwrite: true);
                RefreshLayoutMenu();
                Debug.Log($"[GameFactory] Installed editor layout '{LayoutName}' -> {destPath}. " +
                          "Select it via Window > Layouts.");
            }
            catch (Exception e)
            {
                // Never let a layout convenience break editor startup.
                Debug.LogWarning($"[GameFactory] ZekeLayoutInstaller skipped: {e.Message}");
            }
        }

        // The .wlt imports as a DefaultAsset; locate it through the AssetDatabase so this resolves
        // regardless of where under Assets/ it lives. Path.GetFullPath returns the absolute path.
        private static string FindSourceWlt()
        {
            foreach (var guid in AssetDatabase.FindAssets(LayoutName))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (assetPath.EndsWith(LayoutName + ".wlt", StringComparison.OrdinalIgnoreCase))
                    return Path.GetFullPath(assetPath);
            }
            return null;
        }

        // The folder that backs the Window > Layouts menu. Prefer Unity's own internal path (robust
        // across versions/prefs relocations); fall back to the documented legacy prefs location.
        private static string GetGlobalLayoutsDir()
        {
            try
            {
                var wl = typeof(EditorApplication).Assembly.GetType("UnityEditor.WindowLayout");
                if (wl != null)
                {
                    foreach (var name in new[] { "layoutsDefaultModePath", "layoutsModePreferencesPath", "layoutsPreferencesPath" })
                    {
                        var prop = wl.GetProperty(name, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                        var val = prop?.GetValue(null) as string;
                        // layoutsPreferencesPath is the parent (.../Layouts); the menu reads .../Layouts/default.
                        if (string.IsNullOrEmpty(val)) continue;
                        return name == "layoutsPreferencesPath" ? Path.Combine(val, "default") : val;
                    }
                }
            }
            catch { /* fall through to the manual path */ }

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "Unity", "Editor-5.x", "Preferences", "Layouts", "default");
        }

        private static void RefreshLayoutMenu()
        {
            try
            {
                var wl = typeof(EditorApplication).Assembly.GetType("UnityEditor.WindowLayout");
                var reload = wl?.GetMethod("ReloadWindowLayoutMenu", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                reload?.Invoke(null, null);
            }
            catch { /* menu will populate on next editor restart regardless */ }
        }
    }
}
