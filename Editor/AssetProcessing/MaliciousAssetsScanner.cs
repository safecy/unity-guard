using System;
using System.Linq;
using UnityEditor;
using UnityGuard.AssetManagement;
using UnityGuard.Security;
using UnityGuard.Logging;

namespace UnityGuard.AssetProcessing
{
	public class MaliciousAssetsScanner : AssetPostprocessor
	{
		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			try
			{
				var suspiciousAssets = importedAssets
					.Where(asset => !asset.Contains("Packages/com.safecy.unity-guard"))
					.Where(SecurityChecker.IsSuspiciousAsset);

				foreach (var asset in suspiciousAssets)
				{
					AssetManager.FlagAsset(asset);
				}
			}
			catch (Exception ex)
			{
				Logger.LogException(ex, "Error during asset postprocessing");
			}
		}
	}
}
