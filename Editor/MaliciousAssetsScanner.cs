using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace UnityGuard
{
    public class MaliciousAssetsScanner : AssetPostprocessor
    {
        private static readonly string[] KnownAssetTypes = { ".prefab", ".asset", ".png", ".jpg", ".cs", ".shader" };
        private static readonly string[] DangerousPatterns = { "System.IO", "System.Net", "System.Reflection" };
        private const string DataPath = "Packages/com.safecy.unity-guard/Resources/data.csv";

        private static Dictionary<string, string> fileCache = new Dictionary<string, string>();

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            try
            {
                var maliciousAssets = importedAssets
                    .Where(asset => !asset.Contains("Packages/com.safecy.unity-guard"))
                    .Where(IsMaliciousAsset);

                foreach (var maliciousAsset in maliciousAssets)
                {
                    LogAndDeleteAsset(maliciousAsset);
                }
            }
            catch (Exception ex)
            {
                LogException(ex, "Error during asset postprocessing");
            }
        }

        private static void LogAndDeleteAsset(string assetPath)
        {
            Debug.LogError($"Malicious asset detected: {assetPath}");
            AssetDatabase.DeleteAsset(assetPath);
        }

        private static bool IsMaliciousAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError("Asset path cannot be empty or null.");
                return true;
            }

            if (!File.Exists(assetPath))
            {
                Debug.LogWarning($"Asset file not found: {assetPath}");
                return true;
            }

            var fileExtension = Path.GetExtension(assetPath).ToLower();
            if (!KnownAssetTypes.Contains(fileExtension))
            {
                Debug.LogWarning($"Unknown asset type: {fileExtension}");
                return true;
            }

            if (fileExtension == ".cs" && (ContainsDangerousPatterns(assetPath) || IsLargeScript(assetPath)))
            {
                return true;
            }

            if ((fileExtension == ".asset" || fileExtension == ".prefab") && ContainsMaliciousScripts(assetPath))
            {
                return true;
            }

            if (ContainsMaliciousUrls(assetPath))
            {
                return true;
            }

            return false;
        }

        private static bool IsLargeScript(string assetPath)
        {
            return new FileInfo(assetPath).Length > 1024 * 1024;
        }

        private static bool ContainsDangerousPatterns(string scriptPath)
        {
            try
            {
                var scriptContent = GetFileContent(scriptPath);

                return DangerousPatterns.Any(pattern => scriptContent.Contains(pattern));
            }
            catch (Exception ex)
            {
                LogException(ex, "Error reading script content");
                return true;
            }
        }

        private static bool ContainsMaliciousScripts(string assetPath)
        {
            try
            {
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);

                if (asset == null)
                {
                    Debug.LogWarning($"Failed to load asset: {assetPath}");
                    return true;
                }

                if (asset is GameObject gameObject)
                {
                    var components = gameObject.GetComponents<MonoBehaviour>();

                    foreach (var component in components)
                    {
                        var script = MonoScript.FromMonoBehaviour(component);
                        if (script == null) continue;

                        if (IsMaliciousAsset(AssetDatabase.GetAssetPath(script)))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex, "Unexpected error during script analysis");
                return true;
            }

            return false;
        }

        private static bool ContainsMaliciousUrls(string assetPath)
        {
            try
            {
                var dataLines = GetFileContent(DataPath).Split('\n');

                return dataLines.Any(line => assetPath.Contains(line.Trim()));
            }
            catch (Exception ex)
            {
                LogException(ex, "Error while reading data.csv");
                return true;
            }
        }

        private static string GetFileContent(string filePath)
        {
            if (fileCache.ContainsKey(filePath))
            {
                return fileCache[filePath];
            }

            try
            {
                var content = File.ReadAllText(filePath);
                fileCache[filePath] = content;
                return content;
            }
            catch (Exception ex)
            {
                LogException(ex, $"Error reading file content: {filePath}");
                return null;
            }
        }

        private static void LogException(Exception ex, string context)
        {
            Debug.LogError($"{context}: {ex.Message}");
        }
    }
}
