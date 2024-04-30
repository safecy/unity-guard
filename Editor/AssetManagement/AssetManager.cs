using UnityEditor;
using UnityGuard.Logging;

namespace UnityGuard.AssetManagement
{
	public static class AssetManager
	{
		public static void FlagAsset(string assetPath)
		{
			Logger.LogError($"Suspicious asset detected: {assetPath}");

			if (EditorUtility.DisplayDialog("Suspicious asset detected", $"The asset at path {assetPath} is considered suspicious. Do you want to delete it?", "Delete", "Keep"))
			{
				AssetDatabase.DeleteAsset(assetPath);
				AssetDatabase.Refresh();

				Logger.LogWarning($"Asset deleted: {assetPath}");
			}
		}
	}
}
