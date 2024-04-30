using System;
using System.Collections.Generic;
using System.IO;
using UnityGuard.Logging;

namespace UnityGuard.FileOperations
{
	public static class FileOperations
	{
		private static readonly Dictionary<string, string> FileCache = new Dictionary<string, string>();

		public static string GetFileContent(string filePath)
		{
			if (FileCache.ContainsKey(filePath))
			{
				return FileCache[filePath];
			}

			try
			{
				var content = File.ReadAllText(filePath);
				FileCache[filePath] = content;
				return content;
			}
			catch (Exception ex)
			{
				Logger.LogException(ex, $"Error reading file content: {filePath}");
				return null;
			}
		}
	}
}
