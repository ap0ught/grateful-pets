using System;

namespace Platform;

public static class Utils
{
	public struct OnScopeExit(Action cleanup) : IDisposable
	{
		public Action Cleanup = cleanup;

		public void Dispose()
		{
			Cleanup?.Invoke();
		}
	}

	public static bool IsStrictIO()
	{
		return true;
	}

	public static bool OnlyAOT()
	{
		return false;
	}

	public static bool ShouldOnlyBeCalledOnStandalone(string msg)
	{
		return false;
	}
}
