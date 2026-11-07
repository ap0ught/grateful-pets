using LaundryBear.PlatformServices;
using UnityEngine;

namespace Platform.IO;

public struct PlatformIOResult(StorageResult result, string errorDetails = null) : IStorageErrorable<PlatformIOResult>
{
	public StorageResult result = result;

	public string errorDetails = errorDetails;

	public bool WasSuccessful()
	{
		return result.WasSuccessful();
	}

	public PlatformIOResult ThrowIfFailed()
	{
		if (WasSuccessful())
		{
			return this;
		}
		throw result.StorageResultToException(errorDetails);
	}

	public PlatformIOResult LogErrorIfFailed()
	{
		if (!WasSuccessful())
		{
			Debug.LogError("Platform.IO Error:: " + ToString());
		}
		return this;
	}

	public PlatformIOResult LogIfErrored()
	{
		if (!WasSuccessful())
		{
			Debug.LogError("Platform.IO Error:: " + ToString());
		}
		return this;
	}

	public override string ToString()
	{
		return string.Format("{0}: {1}, {2}: {3}", "result", result, "errorDetails", errorDetails);
	}
}
