using System;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace UnityGuard
{
    // Custom asset postprocessor to scan for malicious assets
    public class MaliciousAssetsScanner : AssetPostprocessor
    {
        // This method is called whenever an asset is imported or updated
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string importedAsset in importedAssets)
            {
                if (importedAsset.Contains("Packages/com.safecy.unity-guard"))
                {
                    continue;
                }

                if (IsMaliciousAsset(importedAsset))
                {
                    Debug.LogError($"Malicious asset detected: {importedAsset}"); // Log error for malicious asset
                    AssetDatabase.DeleteAsset(importedAsset); // Delete the asset
                }
            }
        }

        // Method to check if an asset is malicious
        private static bool IsMaliciousAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                throw new ArgumentException("Asset path cannot be empty.");
            }

            // Step 1: Check if the file exists and is a known Unity asset type
            if (!File.Exists(assetPath))
            {
                Debug.LogWarning($"Asset file not found: {assetPath}");
                return true; // Missing asset could be a sign of tampering
            }

            var knownAssetTypes = new[] { ".prefab", ".asset", ".png", ".jpg", ".cs", ".shader" };
            var fileExtension = Path.GetExtension(assetPath).ToLower();

            if (Array.IndexOf(knownAssetTypes, fileExtension) == -1)
            {
                Debug.LogWarning($"Unknown asset type: {fileExtension}");
                return true; // Unrecognized asset type could indicate a risk
            }

            // Step 2: Check for suspicious file sizes (e.g., overly large scripts)
            var fileSize = new FileInfo(assetPath).Length;

            if (fileExtension == ".cs" && fileSize > 1024 * 1024) // Larger than 1 MB
            {
                Debug.LogWarning($"Unusually large script file: {assetPath}");
                return true; // Large scripts could contain unwanted code
            }

            // Step 3: Basic content analysis for scripts
            if (fileExtension == ".cs")
            {
                var scriptContent = File.ReadAllText(assetPath);

                // Check for potentially dangerous code patterns
                var dangerousPatterns = new[]
                {
                "System.IO", // Direct file access
                "System.Net", // Network access
                "System.Reflection" // Reflection
            };

                foreach (var pattern in dangerousPatterns)
                {
                    if (scriptContent.Contains(pattern))
                    {
                        Debug.LogWarning($"Suspicious code pattern found in {assetPath}: {pattern}");
                        return true; // Detected potentially dangerous pattern
                    }
                }
            }

            // Step 4: Check for executable code in non-script assets
            if (fileExtension == ".asset" || fileExtension == ".prefab")
            {
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);

                if (asset == null)
                {
                    Debug.LogWarning($"Failed to load asset: {assetPath}");
                    return true; // Unable to load asset
                }

                // Check for executable code in prefabs and assets
                if (asset is GameObject gameObject)
                {
                    var components = gameObject.GetComponents<MonoBehaviour>();

                    foreach (var component in components)
                    {
                        if (component == null)
                        {
                            continue;
                        }

                        var script = MonoScript.FromMonoBehaviour(component);

                        if (script == null)
                        {
                            continue;
                        }

                        var scriptPath = AssetDatabase.GetAssetPath(script);

                        if (IsMaliciousAsset(scriptPath))
                        {
                            Debug.LogWarning($"Malicious script found in {assetPath}: {scriptPath}");
                            return true; // Detected malicious script
                        }
                    }
                }
            }

            // Step 5: Malicious URL detection from data.csv
            var dataPath = "Packages/com.safecy.unity-guard/Resources/data.csv";
            var dataLines = File.ReadAllLines(dataPath);

            foreach (var line in dataLines)
            {
                if (assetPath.Contains(line))
                {
                    Debug.LogWarning($"Malicious URL detected in {assetPath}: {line}");
                    return true; // Detected malicious URL
                }
            }

            // If all checks pass, the asset is not deemed malicious
            return false;
        }
    }
}
