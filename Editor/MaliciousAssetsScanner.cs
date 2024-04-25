using System;
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

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            try
            {
                var maliciousAssets = importedAssets
                    .Where(asset => !asset.Contains("Packages/com.safecy.unity-guard"))
                    .Where(IsMaliciousAsset);

                foreach (var maliciousAsset in maliciousAssets)
                {
                    Debug.LogError($"Malicious asset detected: {maliciousAsset}");
                    AssetDatabase.DeleteAsset(maliciousAsset);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during asset postprocessing: {ex.Message}");
            }
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

            if (fileExtension == ".cs" && new FileInfo(assetPath).Length > 1024 * 1024)
            {
                Debug.LogWarning($"Unusually large script file: {assetPath}");
                return true;
            }

            if (fileExtension == ".cs" && ContainsDangerousPatterns(assetPath))
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

        private static bool ContainsDangerousPatterns(string scriptPath)
        {
            try
            {
                var scriptContent = File.ReadAllText(scriptPath);

                return DangerousPatterns.Any(pattern => scriptContent.Contains(pattern));
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading script content: {ex.Message}");
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
                Debug.LogError($"Error analyzing scripts in asset: {ex.Message}");
                return true;
            }

            return false;
        }

        private static bool ContainsMaliciousUrls(string assetPath)
        {
            try
            {
                var dataLines = File.ReadAllLines(DataPath);

                return dataLines.Any(line => assetPath.Contains(line));
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading data.csv: {ex.Message}");
                return true;
            }
        }
    }
}
