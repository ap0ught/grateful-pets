using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;
using XRL.Core;

namespace Platform.IO;

public class Path
{
	private static Regex sm_rootedToDriveRegex = new Regex("^(?<mountName>[^@:|\\\\]{1,15}):", RegexOptions.Compiled);

	public static char DirectorySeparatorChar => System.IO.Path.DirectorySeparatorChar;

	public static char PathSeparator => System.IO.Path.PathSeparator;

	public static char AltDirectorySeparatorChar => System.IO.Path.AltDirectorySeparatorChar;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string Combine(params string[] paths)
	{
		return System.IO.Path.Combine(paths);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetDirectoryName(string path)
	{
		return System.IO.Path.GetDirectoryName(path);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetFileName(string path)
	{
		return System.IO.Path.GetFileName(path);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetRelativePath(string relativeTo, string path)
	{
		return System.IO.Path.GetRelativePath(relativeTo, path);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetFileNameWithoutExtension(string path)
	{
		return System.IO.Path.GetFileNameWithoutExtension(path);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetExtension(string path)
	{
		return System.IO.Path.GetExtension(path);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool HasExtension(string path)
	{
		return System.IO.Path.HasExtension(path);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string ChangeExtension(string path, string extension)
	{
		return System.IO.Path.ChangeExtension(path, extension);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetFullPath(string path)
	{
		return System.IO.Path.GetFullPath(path);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static char[] GetInvalidFileNameChars()
	{
		return System.IO.Path.GetInvalidFileNameChars();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static char[] GetInvalidPathChars()
	{
		return System.IO.Path.GetInvalidPathChars();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetRootPathRelative(string relativeTo, string path)
	{
		return System.IO.Path.GetRelativePath(relativeTo, path);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string Join(string path1, string path2)
	{
		return System.IO.Path.Join(path1, path2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsPathRootedToMount(string path)
	{
		return State.GetStorage().IsPathRootedToMount(path);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetPathMount(string path)
	{
		return State.GetStorage().GetPathMount(path);
	}

	public static string PersistentDataPath()
	{
		return State.GetStorage().RootPath;
	}

	public static bool IsROM(string path)
	{
		return path.Contains(Application.streamingAssetsPath);
	}

	[Obsolete("Path handling moved back to DataManager and made absolute. Please use DataManager.SyncedPath.")]
	public static string RWSynced(string path)
	{
		return Combine(XRLCore.SyncedPath, path);
	}

	[Obsolete("Path handling moved back to DataManager and made absolute. Please use DataManager.LocalPath.")]
	public static string RWLocal(string path)
	{
		return Combine(XRLCore.LocalPath, path);
	}

	[Obsolete("Path handling moved back to DataManager and made absolute. Please use DataManager.SavePath.")]
	public static string RW(string path)
	{
		return path;
	}

	public static bool IsMatch(string path, string searchPattern)
	{
		if (string.IsNullOrEmpty(searchPattern))
		{
			return string.IsNullOrEmpty(path);
		}
		if (string.IsNullOrEmpty(path) && searchPattern != "*")
		{
			return false;
		}
		int num = 0;
		int i = 0;
		int num2 = -1;
		int num3 = -1;
		while (num < path.Length)
		{
			if (i < searchPattern.Length && (searchPattern[i] == '?' || searchPattern[i] == path[num]))
			{
				num++;
				i++;
				continue;
			}
			if (i < searchPattern.Length && searchPattern[i] == '*')
			{
				num2 = i;
				num3 = num;
				i++;
				continue;
			}
			if (num2 != -1)
			{
				i = num2 + 1;
				num3++;
				num = num3;
				continue;
			}
			return false;
		}
		for (; i < searchPattern.Length && searchPattern[i] == '*'; i++)
		{
		}
		return i == searchPattern.Length;
	}
}
