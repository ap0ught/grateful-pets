namespace Platform.IO;

public class FileInfo : FileSystemInfo
{
	public long Length;

	public long LengthInMbs;

	public long LengthInKbs;

	public FileInfo(string fullPath)
		: base(fullPath)
	{
	}

	public override void Refresh()
	{
		base.Refresh();
		exists = File.Exists(rawPath);
		if (!exists)
		{
			Length = 0L;
		}
		else
		{
			Length = Blob.ReadMetadata(FullName).ThrowIfFailed().content.SizeInBytes;
		}
		LengthInMbs = Length / 1000000;
		LengthInKbs = Length / 1000;
	}

	public override void Delete()
	{
		File.Delete(rawPath);
	}

	public override string ToString()
	{
		return $"{GetType().Name} {FullName} [attribs: {attributes}, Size: {Length}]";
	}
}
