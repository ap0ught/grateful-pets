using System.Diagnostics;
using UnityEngine;

public static class exDebug
{
	[Conditional("EX_DEBUG")]
	public static void Assert(bool _test, string _msg = "", bool _logError = true, Object _context = null)
	{
		if (!_test)
		{
			if (_logError)
			{
				UnityEngine.Debug.LogError("Assert Failed! " + _msg, _context);
			}
			else
			{
				UnityEngine.Debug.LogWarning("Assert Failed! " + _msg, _context);
			}
		}
	}
}
