using LaundryBear.PlatformServices;

namespace Platform.IO;

public struct LoadTextResult : IStorageErrorable<LoadTextResult>
{
	public StorageResult result;

	public string path;

	public string content;

	public LoadTextResult LogErrorIfFailed()
	{
		result.LogIfFailedWithDetails("Path: " + path);
		return this;
	}

	public LoadTextResult ThrowIfFailed()
	{
		result.ThrowIfFailedWithDetails("Path: " + path);
		return this;
	}

	public bool WasSuccessful()
	{
		return result.WasSuccessful();
	}
}
