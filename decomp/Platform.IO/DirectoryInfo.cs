using System.Collections.Generic;

namespace Platform.IO;

public class DirectoryInfo : FileSystemInfo
{
	public DirectoryInfo(string path)
		: base(path)
	{
	}

	public override void Refresh()
	{
		base.Refresh();
		attributes = attributes.CombineFlags(FileAttributes.Directory);
		exists = Directory.Exists(rawPath);
	}

	public override void Delete()
	{
		Directory.Delete(rawPath);
	}

	public IEnumerable<FileSystemInfo> EnumerateFileSystemInfos()
	{
		return Directory.EnumerateFileSystemInfos(rawPath);
	}

	public IEnumerable<FileInfo> EnumerateFiles(string searchPattern = null, SearchOption option = SearchOption.TopDirectoryOnly)
	{
		IEnumerable<string> enumerable = Directory.EnumerateFiles(rawPath, searchPattern, option);
		List<FileInfo> list = new List<FileInfo>();
		foreach (string item in enumerable)
		{
			list.Add(new FileInfo(item));
		}
		return list;
	}

	public IEnumerable<DirectoryInfo> EnumerateDirectories()
	{
		IEnumerable<string> enumerable = Directory.EnumerateDirectories(rawPath);
		List<DirectoryInfo> list = new List<DirectoryInfo>();
		foreach (string item in enumerable)
		{
			list.Add(new DirectoryInfo(item));
		}
		return list;
	}
}
