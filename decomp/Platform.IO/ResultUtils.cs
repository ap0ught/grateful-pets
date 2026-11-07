using System;
using System.IO;
using System.Threading.Tasks;
using LaundryBear.PlatformServices;
using Unity.Properties;
using UnityEngine;

namespace Platform.IO;

public static class ResultUtils
{
	public static StorageResult ExceptionToStorageResult(this Exception e)
	{
		if (e == null)
		{
			return StorageResult.Success;
		}
		if (e.InnerException != null)
		{
			e = e.InnerException;
		}
		if (!(e is FileNotFoundException))
		{
			if (!(e is DirectoryNotFoundException))
			{
				if (!(e is DriveNotFoundException))
				{
					if (!(e is PathTooLongException))
					{
						if (!(e is InvalidPathException))
						{
							if (e is UnauthorizedAccessException)
							{
								return StorageResult.InvalidPermissions;
							}
							return StorageResult.UnknownFailure;
						}
						return StorageResult.InvalidPath;
					}
					return StorageResult.PathTooLong;
				}
				return StorageResult.DriveNotFound;
			}
			return StorageResult.DirectoryNotFound;
		}
		return StorageResult.FileNotFound;
	}

	public static Exception StorageResultToException(this StorageResult result, string appendedOnErrorMessage = null)
	{
		if (result.WasSuccessful())
		{
			return null;
		}
		string message = ((appendedOnErrorMessage != null) ? $"{result}\n{appendedOnErrorMessage}" : $"{result}");
		return result switch
		{
			StorageResult.FileNotFound => (Exception)new FileNotFoundException(message), 
			StorageResult.DirectoryNotFound => new DirectoryNotFoundException(message), 
			StorageResult.DriveNotFound => new DriveNotFoundException(message), 
			StorageResult.PathTooLong => new PathTooLongException(message), 
			StorageResult.InUse => new AccessViolationException(message), 
			StorageResult.InvalidPermissions => new UnauthorizedAccessException(message), 
			StorageResult.Corrupted => new IOException(message), 
			StorageResult.AlreadyExists => new InvalidOperationException(message), 
			StorageResult.StorageCountExceeded => new IOException(message), 
			StorageResult.QuotaExceeded => new OutOfStorageException("Storage space is full. Please make more space to avoid loss of data."), 
			_ => new IOException($"Platform.IO:: Unknown Failure from {result}"), 
		};
	}

	public static StorageResult ThrowIfFailed(this StorageResult result)
	{
		if (result == StorageResult.Success)
		{
			return result;
		}
		throw result.StorageResultToException("Platform.IO:: Exception");
	}

	public static StorageResult ThrowIfFailedWithDetails(this StorageResult result, string details)
	{
		if (result == StorageResult.Success)
		{
			return result;
		}
		throw result.StorageResultToException("Platform.IO:: exception, Details " + details);
	}

	public static StorageResult LogIfFailedWithDetails(this StorageResult result, string details)
	{
		if (result != StorageResult.Success)
		{
			Debug.LogError($"Platform.IO:: {result}\nDetails: {details}");
		}
		return result;
	}

	public static T AwaitResult<T>(this Task<T> result)
	{
		result.Wait();
		return result.Result;
	}

	public static StorageResult LogIfErrored(this StorageResult result)
	{
		if (result != StorageResult.Success)
		{
			Debug.LogError($"Platform.IO:: {result}");
		}
		return result;
	}

	public static bool WasSuccessful(this StorageResult result)
	{
		return result == StorageResult.Success;
	}

	public static FileMode ToSystemIOCounterpart(this FileMode mode)
	{
		return mode switch
		{
			FileMode.OpenOrCreate => FileMode.OpenOrCreate, 
			FileMode.Open => FileMode.Open, 
			_ => throw new NotImplementedException(string.Format("Platform.IO:: {0} {1} is not supported", "mode", mode)), 
		};
	}

	public static FileAccess ToSystemIOCounterpart(this FileAccess access)
	{
		return access switch
		{
			FileAccess.Read => FileAccess.Read, 
			FileAccess.ReadWrite => FileAccess.ReadWrite, 
			_ => throw new NotImplementedException(string.Format("Platform.IO:: {0} {1} is not supported", "access", access)), 
		};
	}
}
