using LaundryBear.PlatformServices;

namespace Platform.IO;

public struct EnumerateDirectoriesResult(StorageResult result, string[] directories) : IStorageErrorable<EnumerateDirectoriesResult>
{
	public StorageResult result = result;

	public string[] directories = directories;

	public bool WasSuccessful()
	{
		return result.WasSuccessful();
	}

	public EnumerateDirectoriesResult ThrowIfFailed()
	{
		result.ThrowIfFailedWithDetails("Enumerate Directories issue");
		return this;
	}

	public EnumerateDirectoriesResult LogErrorIfFailed()
	{
		result.LogIfFailedWithDetails("Enumerate Directories issue");
		return this;
	}
}
