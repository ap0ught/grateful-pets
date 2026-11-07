namespace Platform.IO;

public abstract class FileSystemInfo
{
	public const int SAVEDATA_MINIMUM_SPACE_BEFORE_EXPANSION = 4194304;

	protected string rawPath;

	protected FileAttributes attributes;

	protected bool exists;

	protected string extension;

	protected string fullName;

	protected string name;

	public FileAttributes Attributes => attributes;

	public bool Exists => exists;

	public string Extension => Path.GetExtension(FullName);

	public string FullName => rawPath;

	public string Name => Path.GetFileName(FullName);

	protected FileSystemInfo(string fullPath)
	{
		rawPath = fullPath;
		Refresh();
	}

	public abstract void Delete();

	public virtual void Refresh()
	{
		fullName = rawPath;
		name = Path.GetFileName(FullName);
		extension = Path.GetExtension(FullName);
		attributes = FileAttributes.None;
	}

	public override string ToString()
	{
		return $"{GetType().Name} {FullName} [{attributes}]";
	}

	public bool HasAttribute(FileAttributes attribute)
	{
		return attributes.IsSubset(attribute);
	}
}
