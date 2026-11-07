using LaundryBear.PlatformServices;

namespace Platform.IO;

public struct EnumerateFilesResult(StorageResult result, string[] files) : IStorageErrorable<EnumerateFilesResult>
{
	public StorageResult result = result;

	public string[] files = files;

	public EnumerateFilesResult ThrowIfFailed()
	{
		result.ThrowIfFailedWithDetails("Enumerate Files issue");
		return this;
	}

	public bool WasSuccessful()
	{
		return result.WasSuccessful();
	}

	public EnumerateFilesResult LogErrorIfFailed()
	{
		result.LogIfErrored();
		return this;
	}
}
