using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Platform.IO;

public static class Directory
{
	public static bool Exists(string path)
	{
		return Folder.DirectoryExists(path).WasSuccessful();
	}

	public static DirectoryInfo CreateDirectory(string path)
	{
		Folder.CreateDirectory(path).ThrowIfFailed();
		return new DirectoryInfo(path);
	}

	public static void Delete(string path, bool recursive = true)
	{
		if (recursive)
		{
			Folder.DeleteRecursive(path).ThrowIfFailed();
			return;
		}
		Debug.LogError("Not implemented: Delete(string path, bool recursive=false)");
		throw new NotImplementedException("Not implemented: Delete(string path, bool recursive=false)");
	}

	public static async Task<IEnumerable<string>> EnumerateDirectoriesAsync(string path)
	{
		return (await Folder.EnumerateDirectoriesAsync(path)).ThrowIfFailed().directories;
	}

	public static IEnumerable<string> EnumerateDirectories(string path)
	{
		return Folder.EnumerateDirectories(path).ThrowIfFailed().directories;
	}

	public static IEnumerable<string> EnumerateFiles(string path)
	{
		return Folder.EnumerateFilesShallow(path).ThrowIfFailed().files;
	}

	public static async Task<IEnumerable<string>> EnumerateFilesAsync(string path)
	{
		return (await Folder.EnumerateFilesShallowAsync(path)).ThrowIfFailed().files;
	}

	public static IEnumerable<string> EnumerateFiles(string path, SearchOption option)
	{
		return Folder.EnumerateFiles(path, option).files;
	}

	public static IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption option)
	{
		string[] files = Folder.EnumerateFiles(path, option).files;
		if (searchPattern != "*" && searchPattern != null && searchPattern != "*.*")
		{
			List<string> list = new List<string>();
			string[] array = files;
			foreach (string text in array)
			{
				if (Path.IsMatch(text, searchPattern))
				{
					list.Add(text);
				}
			}
			return list;
		}
		return files;
	}

	public static async Task<IEnumerable<string>> EnumerateFilesAsync(string path, SearchOption option)
	{
		return (await Folder.EnumerateFilesAsync(path, option)).ThrowIfFailed().files;
	}

	public static IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(string path)
	{
		List<FileSystemInfo> list = new List<FileSystemInfo>();
		foreach (string item in EnumerateFiles(path))
		{
			list.Add(new FileInfo(item));
		}
		foreach (string item2 in EnumerateDirectories(path))
		{
			list.Add(new DirectoryInfo(item2));
		}
		return list;
	}

	public static string[] GetDirectories(string path)
	{
		return EnumerateDirectories(path).ToArray();
	}
}
