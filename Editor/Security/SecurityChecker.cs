using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Logger = UnityGuard.Logging.Logger;

namespace UnityGuard.Security
{
	public static class SecurityChecker
	{
		private static readonly string[] KnownAssetTypes = { ".prefab", ".asset", ".png", ".jpg", ".cs", ".shader" };
		private static readonly string[] DangerousPatterns = { "System.IO", "System.Net", "System.Reflection" };

		public static bool IsSuspiciousAsset(string assetPath)
		{
			if (string.IsNullOrEmpty(assetPath))
			{
				Logger.LogError("Asset path cannot be empty or null.");
				return false;
			}

			if (!File.Exists(assetPath))
			{
				Logger.LogWarning($"Asset file not found: {assetPath}");
				return false;
			}

			var fileExtension = Path.GetExtension(assetPath).ToLower();
			if (!KnownAssetTypes.Contains(fileExtension))
			{
				Logger.LogWarning($"Unknown asset type: {fileExtension}");
				return false;
			}

			if ((fileExtension == ".cs" && ContainsDangerousPatterns(assetPath)) || IsLargeScript(assetPath))
			{
				return true;
			}

			if (ContainsMaliciousScripts(assetPath) || ContainsMaliciousUrls(assetPath))
			{
				return true;
			}

			return false;
		}

		private static bool ContainsDangerousPatterns(string assetPath)
		{
			var content = FileOperations.FileOperations.GetFileContent(assetPath);
			if (string.IsNullOrEmpty(content))
			{
				return false;
			}

			return DangerousPatterns.Any(pattern => content.Contains(pattern));
		}

		private static bool IsLargeScript(string assetPath)
		{
			var fileInfo = new FileInfo(assetPath);
			return fileInfo.Length > 1024 * 1024;
		}

		private static bool ContainsMaliciousScripts(string assetPath)
		{
			try
			{
				var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);

				if (asset == null)
				{
					Logger.LogWarning($"Failed to load asset: {assetPath}");
					return false;
				}

				if (asset is GameObject gameObject)
				{
					var components = gameObject.GetComponents<MonoBehaviour>();
					foreach (var component in components)
					{
						var script = MonoScript.FromMonoBehaviour(component);
						if (script == null) continue;

						if (IsSuspiciousAsset(AssetDatabase.GetAssetPath(script)))
						{
							return true;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.LogException(ex, "Unexpected error during script analysis");
				return false;
			}

			return false;
		}

		private static bool ContainsMaliciousUrls(string assetPath)
		{
			try
			{
				var dataLines = FileOperations.FileOperations.GetFileContent("Packages/com.safecy.unity-guard/Resources/data.csv")?.Split('\n');
				return dataLines != null && dataLines.Any(line => assetPath.Contains(line.Trim()));
			}
			catch (Exception ex)
			{
				Logger.LogException(ex, "Error while reading data.csv");
				return false;
			}
		}
	}
}
