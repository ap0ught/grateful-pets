using System;
using System.IO;
using System.Threading.Tasks;
using LaundryBear.PlatformServices;
using Newtonsoft.Json;

namespace Platform.IO;

public static class Blob
{
	public static StorageResult WriteAllBytes(string path, byte[] content)
	{
		return WriteAllBytesAsync(path, content).AwaitResult();
	}

	public static Task<StorageResult> WriteAllBytesAsync(string path, byte[] content)
	{
		TaskCompletionSource<StorageResult> tcs = new TaskCompletionSource<StorageResult>();
		if (!Path.IsROM(path))
		{
			State.GetStorage().SaveBlob(path, content, (StorageResult result) =>
			{
				tcs.SetResult(result);
			});
		}
		else
		{
			tcs.SetResult(StorageResult.InvalidPermissions);
		}
		return tcs.Task;
	}

	public static StorageResult WriteAllText(string path, string content)
	{
		return WriteAllTextAsync(path, content).AwaitResult();
	}

	public static Task<StorageResult> WriteAllTextAsync(string path, string content)
	{
		TaskCompletionSource<StorageResult> tcs = new TaskCompletionSource<StorageResult>();
		if (!Path.IsROM(path))
		{
			State.GetStorage().SaveBlob(path, content, (StorageResult result) =>
			{
				tcs.SetResult(result);
			});
		}
		else
		{
			tcs.SetResult(StorageResult.InvalidPermissions);
		}
		return tcs.Task;
	}

	public static Task<LoadBytesResult> ReadAllBytesAsync(string path)
	{
		TaskCompletionSource<LoadBytesResult> tcs = new TaskCompletionSource<LoadBytesResult>();
		if (Path.IsROM(path))
		{
			System.IO.File.ReadAllBytesAsync(path).ContinueWith((Task<byte[]> task) =>
			{
				if (task.IsCompletedSuccessfully)
				{
					tcs.SetResult(new LoadBytesResult
					{
						result = StorageResult.Success,
						path = path,
						content = task.Result
					});
				}
				else
				{
					tcs.SetResult(new LoadBytesResult
					{
						result = task.Exception.ExceptionToStorageResult(),
						path = path,
						content = null
					});
				}
			});
		}
		else
		{
			State.GetStorage().LoadBlob(path, (StorageResult result, byte[] content) =>
			{
				tcs.SetResult(new LoadBytesResult
				{
					result = result,
					path = path,
					content = content
				});
			});
		}
		return tcs.Task;
	}

	public static LoadBytesResult ReadAllBytes(string path)
	{
		return ReadAllBytesAsync(path).AwaitResult();
	}

	public static LoadTextResult ReadAllText(string path)
	{
		return ReadAllTextAsync(path).AwaitResult();
	}

	public static Task<LoadTextResult> ReadAllTextAsync(string path)
	{
		TaskCompletionSource<LoadTextResult> tcs = new TaskCompletionSource<LoadTextResult>();
		if (Path.IsROM(path))
		{
			System.IO.File.ReadAllTextAsync(path).ContinueWith((Task<string> task) =>
			{
				if (task.IsCompletedSuccessfully)
				{
					tcs.SetResult(new LoadTextResult
					{
						result = StorageResult.Success,
						path = path,
						content = task.Result
					});
				}
				else
				{
					tcs.SetResult(new LoadTextResult
					{
						result = task.Exception.ExceptionToStorageResult(),
						path = path,
						content = null
					});
				}
			});
		}
		else
		{
			State.GetStorage().LoadBlob(path, (StorageResult result, string content) =>
			{
				tcs.SetResult(new LoadTextResult
				{
					result = result,
					path = path,
					content = content
				});
			});
		}
		return tcs.Task;
	}

	public static ObjectLoadResult<FileMetadata> ReadMetadata(string path)
	{
		return FileInfoAsync(path).AwaitResult();
	}

	public static Task<ObjectLoadResult<FileMetadata>> FileInfoAsync(string path)
	{
		TaskCompletionSource<ObjectLoadResult<FileMetadata>> tcs = new TaskCompletionSource<ObjectLoadResult<FileMetadata>>();
		if (Path.IsROM(path))
		{
			try
			{
				System.IO.FileInfo fileInfo = new System.IO.FileInfo(path);
				FileMetadata content = new FileMetadata
				{
					SizeInBytes = fileInfo.Length
				};
				tcs.SetResult(ObjectLoadResult<FileMetadata>.CreateSuccess(content));
			}
			catch (Exception e)
			{
				ObjectLoadResult<FileMetadata> result = ObjectLoadResult<FileMetadata>.CreateError(e.ExceptionToStorageResult(), "Could not get file info from ROM path");
				tcs.SetResult(result);
			}
			return tcs.Task;
		}
		State.GetStorage().FileMetadata(path, (StorageResult result3, FileMetadata data) =>
		{
			ObjectLoadResult<FileMetadata> result2 = new ObjectLoadResult<FileMetadata>(new PlatformIOResult(result3), data);
			tcs.SetResult(result2);
		});
		return tcs.Task;
	}

	public static StorageResult Exists(string path)
	{
		return ExistsAsync(path).AwaitResult();
	}

	public static Task<StorageResult> ExistsAsync(string path)
	{
		TaskCompletionSource<StorageResult> tcs = new TaskCompletionSource<StorageResult>();
		if (Path.IsROM(path))
		{
			if (System.IO.File.Exists(path))
			{
				tcs.SetResult(StorageResult.Success);
			}
			else
			{
				tcs.SetResult(StorageResult.FileNotFound);
			}
		}
		else
		{
			State.GetStorage().FileExists(path, (StorageResult result) =>
			{
				tcs.SetResult(result);
			});
		}
		return tcs.Task;
	}

	public static StorageResult Delete(string path)
	{
		return DeleteAsync(path).AwaitResult();
	}

	public static async Task<StorageResult> DeleteAsync(string path)
	{
		if (Path.IsROM(path))
		{
			return StorageResult.InvalidPermissions;
		}
		if (!(await ExistsAsync(path)).WasSuccessful())
		{
			return StorageResult.Success;
		}
		TaskCompletionSource<StorageResult> tcs = new TaskCompletionSource<StorageResult>();
		State.GetStorage().DeleteBlob(path, (StorageResult result) =>
		{
			tcs.SetResult(result);
		});
		return await tcs.Task;
	}

	public static async Task<PlatformIOResult> CopyAsync(string sourceFileName, string destFileName, bool overwrite)
	{
		if ((await ExistsAsync(destFileName)).WasSuccessful())
		{
			if (!overwrite)
			{
				return new PlatformIOResult(StorageResult.OperationCancelled, string.Format("Can't copy because {0} already exists and {1} is {2}", destFileName, "overwrite", overwrite));
			}
			StorageResult result = await DeleteAsync(destFileName);
			if (!result.WasSuccessful())
			{
				return new PlatformIOResult(result, "Error deleting " + destFileName + " when trying to overwrite it with a copy of " + sourceFileName + ".");
			}
		}
		LoadBytesResult load = await ReadAllBytesAsync(sourceFileName);
		if (load.result != StorageResult.Success)
		{
			return new PlatformIOResult(load.result, "Error reading from source path \"" + sourceFileName + "\"");
		}
		if (await WriteAllBytesAsync(destFileName, load.content) != StorageResult.Success)
		{
			return new PlatformIOResult(load.result, "Error writing to destination path \"" + destFileName + "\"");
		}
		return new PlatformIOResult(StorageResult.Success);
	}

	public static PlatformIOResult Copy(string sourceFileName, string destFileName, bool overwrite)
	{
		return CopyAsync(sourceFileName, destFileName, overwrite).AwaitResult();
	}

	public static async Task<PlatformIOResult> MoveAsync(string sourceFileName, string destFileName, bool overwrite = false)
	{
		bool flag = !overwrite;
		if (flag)
		{
			flag = (await ExistsAsync(destFileName)).WasSuccessful();
		}
		if (flag)
		{
			return new PlatformIOResult(StorageResult.OperationCancelled, string.Format("Can't move because {0} already exists and {1} is {2}", destFileName, "overwrite", overwrite));
		}
		PlatformIOResult result = await CopyAsync(sourceFileName, destFileName, overwrite);
		if (!result.WasSuccessful())
		{
			return result;
		}
		StorageResult result2 = await DeleteAsync(sourceFileName);
		if (!result2.WasSuccessful())
		{
			return new PlatformIOResult(result2, "Error deleting " + sourceFileName);
		}
		return new PlatformIOResult(StorageResult.Success);
	}

	public static PlatformIOResult Move(string sourceFileName, string destFileName, bool overwrite = false)
	{
		return MoveAsync(sourceFileName, destFileName, overwrite).AwaitResult();
	}

	public static PlatformIOResult WriteAllJson(string path, object content, JsonSerializerSettings? settings = null)
	{
		return WriteAllJsonAsync(path, content, settings).AwaitResult();
	}

	public static async Task<PlatformIOResult> WriteAllJsonAsync(string path, object content, JsonSerializerSettings? settings = null)
	{
		if (settings == null)
		{
			settings = new JsonSerializerSettings();
		}
		string content2;
		try
		{
			content2 = JsonConvert.SerializeObject(content, settings);
		}
		catch (Exception)
		{
			return new PlatformIOResult(StorageResult.UnknownFailure, "Error deserializes file");
		}
		StorageResult result = await WriteAllTextAsync(path, content2);
		if (!result.WasSuccessful())
		{
			return new PlatformIOResult(result, "Error Saving Json File at " + path);
		}
		return new PlatformIOResult(result);
	}

	public static async Task<ObjectLoadResult<T>> ReadJsonAsync<T>(string path, JsonSerializerSettings? settings = null)
	{
		if (settings == null)
		{
			settings = new JsonSerializerSettings();
		}
		LoadTextResult loadTextResult = await ReadAllTextAsync(path);
		if (!loadTextResult.result.WasSuccessful())
		{
			return ObjectLoadResult<T>.CreateError(loadTextResult.result);
		}
		try
		{
			return ObjectLoadResult<T>.CreateSuccess(JsonConvert.DeserializeObject<T>(loadTextResult.content, settings));
		}
		catch (Exception arg)
		{
			return ObjectLoadResult<T>.CreateError(StorageResult.UnknownFailure, string.Format("Error from {0}: {1}", "DeserializeObject", arg));
		}
	}

	public static ObjectLoadResult<T> ReadAllJson<T>(string path, JsonSerializerSettings? settings = null)
	{
		return ReadJsonAsync<T>(path, settings).AwaitResult();
	}

	public static Task<ObjectLoadResult<Stream>> OpenAsync(string path, FileMode mode, FileAccess access)
	{
		TaskCompletionSource<ObjectLoadResult<Stream>> tcs = new TaskCompletionSource<ObjectLoadResult<Stream>>();
		if (Path.IsROM(path))
		{
			if (mode == FileMode.OpenOrCreate || access == FileAccess.ReadWrite)
			{
				tcs.SetResult(ObjectLoadResult<Stream>.CreateError(StorageResult.InvalidPermissions, string.Format("Can't {0} path {1} because it is ROM. {2} is {3} and {4} is {5}.", "OpenAsync", path, "mode", mode, "access", access)));
				return tcs.Task;
			}
			try
			{
				FileStream content = System.IO.File.Open(path, mode.ToSystemIOCounterpart(), access.ToSystemIOCounterpart());
				tcs.SetResult(ObjectLoadResult<Stream>.CreateSuccess(content));
				return tcs.Task;
			}
			catch (Exception ex)
			{
				StorageResult result = ex.ExceptionToStorageResult();
				tcs.SetResult(ObjectLoadResult<Stream>.CreateError(result, ex.Message));
				return tcs.Task;
			}
		}
		bool flag = Exists(path).WasSuccessful();
		if (mode == FileMode.Open && !flag)
		{
			tcs.SetResult(new ObjectLoadResult<Stream>(new PlatformIOResult(StorageResult.FileNotFound, "File didn't exist when opening up stream, but filemode is not set to create"), null));
			return tcs.Task;
		}
		if (mode == FileMode.OpenOrCreate && !flag)
		{
			StorageResult result2 = WriteAllBytes(path, Array.Empty<byte>());
			if (!result2.WasSuccessful())
			{
				tcs.SetResult(new ObjectLoadResult<Stream>(new PlatformIOResult(result2, "Could not create file when opening up stream"), null));
				return tcs.Task;
			}
		}
		State.GetStorage().OpenStream(path, mode, access, (StorageResult result3, Stream stream) =>
		{
			tcs.SetResult(new ObjectLoadResult<Stream>(new PlatformIOResult(result3), stream));
		});
		return tcs.Task;
	}

	public static ObjectLoadResult<Stream> Open(string path, FileMode mode, FileAccess access, FileShare fileShare)
	{
		if (access == FileAccess.ReadWrite)
		{
			return ObjectLoadResult<Stream>.CreateError(StorageResult.InvalidPermissions, "Stream with Write enabled Not Implemented");
		}
		return OpenAsync(path, mode, access).Result;
	}

	public static Task<ObjectLoadResult<Stream>> OpenWriteAsync(string path)
	{
		return OpenAsync(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
	}

	public static Task<ObjectLoadResult<Stream>> OpenWriteAsync(string path, FileMode mode, FileAccess fileAccess)
	{
		return OpenAsync(path, mode, fileAccess);
	}

	public static ObjectLoadResult<Stream> OpenWrite(string path)
	{
		return OpenWriteAsync(path).Result;
	}

	public static Task<ObjectLoadResult<Stream>> OpenReadAsync(string path)
	{
		return OpenAsync(path, FileMode.Open, FileAccess.Read);
	}

	public static ObjectLoadResult<Stream> OpenRead(string path)
	{
		return OpenReadAsync(path).Result;
	}
}
