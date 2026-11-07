using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LaundryBear.PlatformServices;

namespace Platform.IO;

public static class Folder
{
	public static StorageResult DirectoryExists(string path)
	{
		if (Path.IsROM(path))
		{
			try
			{
				if (System.IO.Directory.Exists(path))
				{
					return StorageResult.Success;
				}
				return StorageResult.DirectoryNotFound;
			}
			catch (Exception e)
			{
				return e.ExceptionToStorageResult();
			}
		}
		TaskCompletionSource<StorageResult> tcs = new TaskCompletionSource<StorageResult>();
		State.GetStorage().DirectoryExists(path, (StorageResult result) =>
		{
			tcs.SetResult(result);
		});
		return tcs.Task.Result;
	}

	public static async Task<StorageResult> DirectoryExistsAsync(string path)
	{
		if (Path.IsROM(path))
		{
			try
			{
				if (System.IO.Directory.Exists(path))
				{
					return StorageResult.Success;
				}
				return StorageResult.DirectoryNotFound;
			}
			catch (Exception e)
			{
				return e.ExceptionToStorageResult();
			}
		}
		TaskCompletionSource<StorageResult> tcs = new TaskCompletionSource<StorageResult>();
		State.GetStorage().DirectoryExists(path, (StorageResult result) =>
		{
			tcs.SetResult(result);
		});
		return await tcs.Task;
	}

	public static PlatformIOResult CreateDirectory(string path)
	{
		return CreateDirectoryAsync(path).AwaitResult();
	}

	public static async Task<PlatformIOResult> CreateDirectoryAsync(string path)
	{
		if ((await DirectoryExistsAsync(path)).WasSuccessful())
		{
			return new PlatformIOResult(StorageResult.Success);
		}
		string parentPath = Path.GetDirectoryName(path);
		if (parentPath.EndsWith(":"))
		{
			string text = parentPath;
			char directorySeparatorChar = System.IO.Path.DirectorySeparatorChar;
			parentPath = text + directorySeparatorChar;
		}
		if (!(await DirectoryExistsAsync(parentPath)).WasSuccessful())
		{
			PlatformIOResult platformIOResult = await CreateDirectoryAsync(parentPath);
			if (!platformIOResult.WasSuccessful())
			{
				return new PlatformIOResult(platformIOResult.result, "Could not create parent paths");
			}
		}
		return await CreateDirectoryIfParentPresent(path);
	}

	private static Task<PlatformIOResult> CreateDirectoryIfParentPresent(string path)
	{
		TaskCompletionSource<PlatformIOResult> tcs = new TaskCompletionSource<PlatformIOResult>();
		if (Path.IsROM(path))
		{
			tcs.SetResult(new PlatformIOResult(StorageResult.InvalidPermissions, "Can't CreateDirectoryIfParentPresent path " + path + " because it is ROM."));
			return tcs.Task;
		}
		State.GetStorage().CreateDirectory(path, (StorageResult result) =>
		{
			tcs.SetResult(new PlatformIOResult(result));
		});
		return tcs.Task;
	}

	public static PlatformIOResult DeleteRecursive(string path)
	{
		return DeleteRecursiveAsync(path).AwaitResult();
	}

	public static Task<PlatformIOResult> DeleteRecursiveAsync(string path)
	{
		TaskCompletionSource<PlatformIOResult> tcs = new TaskCompletionSource<PlatformIOResult>();
		if (Path.IsROM(path))
		{
			tcs.SetResult(new PlatformIOResult(StorageResult.InvalidPermissions, "Can't call DeleteRecursiveAsync on path " + path + " because it is ROM."));
			return tcs.Task;
		}
		State.GetStorage().DeleteDirectory(path, (StorageResult result) =>
		{
			tcs.SetResult(new PlatformIOResult(result));
		});
		return tcs.Task;
	}

	public static EnumerateDirectoriesResult EnumerateDirectories(string path)
	{
		return EnumerateDirectoriesAsync(path).AwaitResult();
	}

	public static Task<EnumerateDirectoriesResult> EnumerateDirectoriesAsync(string path)
	{
		TaskCompletionSource<EnumerateDirectoriesResult> tcs = new TaskCompletionSource<EnumerateDirectoriesResult>();
		if (Path.IsROM(path))
		{
			try
			{
				IEnumerable<string> source = System.IO.Directory.EnumerateDirectories(path);
				tcs.SetResult(new EnumerateDirectoriesResult(StorageResult.Success, source.ToArray()));
				return tcs.Task;
			}
			catch (Exception e)
			{
				tcs.SetResult(new EnumerateDirectoriesResult(e.ExceptionToStorageResult(), Array.Empty<string>()));
				return tcs.Task;
			}
		}
		State.GetStorage().EnumerateDirectories(path, (StorageResult result, string[] files) =>
		{
			tcs.SetResult(new EnumerateDirectoriesResult(result, files));
		});
		return tcs.Task;
	}

	public static EnumerateFilesResult EnumerateFilesShallow(string path)
	{
		return EnumerateFilesShallowAsync(path).AwaitResult();
	}

	public static Task<EnumerateFilesResult> EnumerateFilesShallowAsync(string path)
	{
		TaskCompletionSource<EnumerateFilesResult> tcs = new TaskCompletionSource<EnumerateFilesResult>();
		if (Path.IsROM(path))
		{
			try
			{
				IEnumerable<string> source = System.IO.Directory.EnumerateFiles(path);
				tcs.SetResult(new EnumerateFilesResult(StorageResult.Success, source.ToArray()));
				return tcs.Task;
			}
			catch (Exception e)
			{
				tcs.SetResult(new EnumerateFilesResult(e.ExceptionToStorageResult(), Array.Empty<string>()));
				return tcs.Task;
			}
		}
		State.GetStorage().EnumerateFiles(path, (StorageResult result, string[] files) =>
		{
			tcs.SetResult(new EnumerateFilesResult(result, files));
		});
		return tcs.Task;
	}

	public static Task<EnumerateFilesResult> EnumerateFilesRecursiveAsync(string path)
	{
		TaskCompletionSource<EnumerateFilesResult> taskCompletionSource = new TaskCompletionSource<EnumerateFilesResult>();
		if (Path.IsROM(path))
		{
			try
			{
				IEnumerable<string> source = System.IO.Directory.EnumerateFiles(path, "*", System.IO.SearchOption.AllDirectories);
				taskCompletionSource.SetResult(new EnumerateFilesResult(StorageResult.Success, source.ToArray()));
				return taskCompletionSource.Task;
			}
			catch (Exception e)
			{
				taskCompletionSource.SetResult(new EnumerateFilesResult(e.ExceptionToStorageResult(), Array.Empty<string>()));
				return taskCompletionSource.Task;
			}
		}
		StorageResult result = StorageResult.Success;
		List<string> list = new List<string>();
		recurse(path, ref result, list);
		taskCompletionSource.SetResult(new EnumerateFilesResult(result, list.ToArray()));
		return taskCompletionSource.Task;
		static bool recurse(string path2, ref StorageResult reference, List<string> files)
		{
			EnumerateFilesResult result2 = EnumerateFilesShallowAsync(path2).Result;
			if (!result2.WasSuccessful())
			{
				reference = result2.result;
				return false;
			}
			if (result2.files != null)
			{
				files.AddRange(result2.files);
			}
			EnumerateDirectoriesResult result3 = EnumerateDirectoriesAsync(path2).Result;
			if (!result3.WasSuccessful())
			{
				return false;
			}
			string[] directories = result3.directories;
			for (int i = 0; i < directories.Length; i++)
			{
				if (!recurse(directories[i], ref reference, files))
				{
					return false;
				}
			}
			return true;
		}
	}

	public static EnumerateFilesResult EnumerateFiles(string path, SearchOption option)
	{
		return EnumerateFilesAsync(path, option).AwaitResult();
	}

	public static async Task<EnumerateFilesResult> EnumerateFilesAsync(string path, SearchOption option)
	{
		return await (option switch
		{
			SearchOption.AllDirectories => EnumerateFilesRecursiveAsync(path), 
			SearchOption.TopDirectoryOnly => EnumerateFilesShallowAsync(path), 
			_ => throw new NotImplementedException(), 
		});
	}
}
