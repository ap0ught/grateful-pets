using LaundryBear.PlatformServices;

namespace Platform.IO;

public struct LoadBytesResult : IStorageErrorable<LoadBytesResult>
{
	public StorageResult result;

	public string path;

	public byte[] content;

	public LoadBytesResult LogErrorIfFailed()
	{
		result.LogIfFailedWithDetails("Path: " + path);
		return this;
	}

	public LoadBytesResult ThrowIfFailed()
	{
		result.ThrowIfFailedWithDetails("Path: " + path);
		return this;
	}

	public bool WasSuccessful()
	{
		return result.WasSuccessful();
	}
}
