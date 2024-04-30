using System;
using UnityEngine;

namespace UnityGuard.Logging
{
	public static class Logger
	{
		public static void LogException(Exception ex, string context)
		{
			Debug.LogError($"{context}: {ex.Message}");
		}

		public static void LogError(string message)
		{
			Debug.LogError(message);
		}

		public static void LogWarning(string message)
		{
			Debug.LogWarning(message);
		}
	}
}
