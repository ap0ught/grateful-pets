using System.IO;

namespace Platform.IO;

public static class FileAttributesExtensions
{
	public static FileAttributes CombineFlags(this FileAttributes a, FileAttributes b)
	{
		return a | b;
	}

	public static bool IsSubset(this FileAttributes superset, FileAttributes subset)
	{
		return (superset & subset) == superset;
	}

	public static bool IsSubset(this System.IO.FileAttributes superset, System.IO.FileAttributes subset)
	{
		return (superset & subset) == superset;
	}

	public static FileAttributes FromSystemIO(System.IO.FileAttributes attributes)
	{
		FileAttributes fileAttributes = FileAttributes.None;
		if (attributes.IsSubset(System.IO.FileAttributes.ReadOnly))
		{
			fileAttributes = fileAttributes.CombineFlags(FileAttributes.ReadOnly);
		}
		if (attributes.IsSubset(System.IO.FileAttributes.Hidden))
		{
			fileAttributes = fileAttributes.CombineFlags(FileAttributes.Hidden);
		}
		if (attributes.IsSubset(System.IO.FileAttributes.ReadOnly))
		{
			fileAttributes = fileAttributes.CombineFlags(FileAttributes.ReadOnly);
		}
		if (attributes.IsSubset(System.IO.FileAttributes.Directory))
		{
			fileAttributes = fileAttributes.CombineFlags(FileAttributes.Directory);
		}
		return fileAttributes;
	}
}
