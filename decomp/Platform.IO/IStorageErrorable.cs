namespace Platform.IO;

public interface IStorageErrorable<T>
{
	bool WasSuccessful();

	T ThrowIfFailed();

	T LogErrorIfFailed();
}
