using System;

namespace XRL;

public class DataFile : IComparable<DataFile>
{
	public ModInfo Mod;

	public string Path;

	public int Priority;

	public bool IsBase => Mod == null;

	public bool IsMod => Mod != null;

	public static implicit operator string(DataFile File)
	{
		return File.Path;
	}

	public int CompareTo(DataFile Other)
	{
		int num = 0;
		if (Mod == null)
		{
			if (Other.Mod != null)
			{
				return -1;
			}
		}
		else
		{
			if (Other.Mod == null)
			{
				return 1;
			}
			num = Mod.CompareTo(Other.Mod);
			if (num != 0)
			{
				return num;
			}
		}
		num = Other.Priority.CompareTo(Priority);
		if (num != 0)
		{
			return num;
		}
		return string.Compare(Path, Other.Path, StringComparison.Ordinal);
	}

	public override string ToString()
	{
		return Path;
	}
}
