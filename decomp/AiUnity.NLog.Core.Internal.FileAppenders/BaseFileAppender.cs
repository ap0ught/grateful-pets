using System;
using System.IO;
using System.Security;
using System.Threading;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;
using AiUnity.NLog.Core.Time;

namespace AiUnity.NLog.Core.Internal.FileAppenders;

[SecuritySafeCritical]
internal abstract class BaseFileAppender : IDisposable
{
	private readonly Random random = new Random();

	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	public string FileName { get; private set; }

	public DateTime LastWriteTime { get; private set; }

	public DateTime OpenTime { get; private set; }

	public ICreateFileParameters CreateFileParameters { get; private set; }

	public BaseFileAppender(string fileName, ICreateFileParameters createParameters)
	{
		CreateFileParameters = createParameters;
		FileName = fileName;
		OpenTime = TimeSource.Current.Time.ToLocalTime();
		LastWriteTime = DateTime.MinValue;
	}

	public abstract void Write(byte[] bytes);

	public abstract void Flush();

	public abstract void Close();

	public abstract bool GetFileInfo(out DateTime lastWriteTime, out long fileLength);

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			Close();
		}
	}

	protected void FileTouched()
	{
		LastWriteTime = TimeSource.Current.Time.ToLocalTime();
	}

	protected void FileTouched(DateTime dateTime)
	{
		LastWriteTime = dateTime;
	}

	protected FileStream CreateFileStream(bool allowConcurrentWrite)
	{
		int num = CreateFileParameters.ConcurrentWriteAttemptDelay;
		Logger.Trace("Opening {0} with concurrentWrite={1}", FileName, allowConcurrentWrite);
		for (int i = 0; i < CreateFileParameters.ConcurrentWriteAttempts; i++)
		{
			try
			{
				try
				{
					return TryCreateFileStream(allowConcurrentWrite);
				}
				catch (DirectoryNotFoundException)
				{
					if (!CreateFileParameters.CreateDirs)
					{
						throw;
					}
					Directory.CreateDirectory(Path.GetDirectoryName(FileName));
					return TryCreateFileStream(allowConcurrentWrite);
				}
			}
			catch (IOException)
			{
				if (!CreateFileParameters.ConcurrentWrites || !allowConcurrentWrite || i + 1 == CreateFileParameters.ConcurrentWriteAttempts)
				{
					throw;
				}
				int num2 = random.Next(num);
				Logger.Warn("Attempt #{0} to open {1} failed. Sleeping for {2}ms", i, FileName, num2);
				num *= 2;
				Thread.Sleep(num2);
			}
		}
		throw new InvalidOperationException("Should not be reached.");
	}

	private FileStream TryCreateFileStream(bool allowConcurrentWrite)
	{
		FileShare share = FileShare.Read;
		if (allowConcurrentWrite)
		{
			share = FileShare.ReadWrite;
		}
		return new FileStream(FileName, FileMode.Append, FileAccess.Write, share, CreateFileParameters.BufferSize);
	}
}
