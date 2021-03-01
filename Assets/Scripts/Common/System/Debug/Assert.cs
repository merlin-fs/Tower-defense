using UnityEngine;

namespace St.Common.Debug
{
	public static class Assert
	{
		public static void Check(bool term, string msg = "")
		{
			if (!term)
			{
				string stackPoint = "UNKNOWN";
				string[] stackStrings = StackTraceUtility.ExtractStackTrace().Split('\n');
				if (stackStrings.Length >= 2)
					stackPoint = stackStrings[1];

				stackPoint = stackPoint.Replace("at", string.Empty);

				string log = "[Assert ] " + stackPoint + ": " + msg;
				UnityEngine.Debug.LogWarning(log);
			}
		}
	}
}