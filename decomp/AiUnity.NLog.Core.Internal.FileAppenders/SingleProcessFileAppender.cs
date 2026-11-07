using System;
using System.IO;
using System.Security;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;

namespace AiUnity.NLog.Core.Internal.FileAppenders;

[SecuritySafeCritical]
internal class SingleProcessFileAppender : BaseFileAppender
{
	private class Factory : IFileAppenderFactory
	{
		BaseFileAppender IFileAppenderFactory.Open(string fileName, ICreateFileParameters parameters)
		{
			return new SingleProcessFileAppender(fileName, parameters);
		}
	}

	public static readonly IFileAppenderFactory TheFactory = new Factory();

	private FileStream file;

	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	public SingleProcessFileAppender(string fileName, ICreateFileParameters parameters)
		: base(fileName, parameters)
	{
		FileInfo fileInfo = new FileInfo(fileName);
		if (fileInfo.Exists)
		{
			FileTouched(fileInfo.LastWriteTime);
		}
		else
		{
			FileTouched();
		}
		file = CreateFileStream(allowConcurrentWrite: false);
	}

	public override void Write(byte[] bytes)
	{
		if (file != null)
		{
			file.Write(bytes, 0, bytes.Length);
			FileTouched();
		}
	}

	public override void Flush()
	{
		if (file != null)
		{
			file.Flush();
			FileTouched();
		}
	}

	public override void Close()
	{
		if (file != null)
		{
			Logger.Trace("Closing '{0}'", FileName);
			file.Close();
			file = null;
		}
	}

	public override bool GetFileInfo(out DateTime lastWriteTime, out long fileLength)
	{
		if (file != null)
		{
			lastWriteTime = LastWriteTime;
			fileLength = file.Length;
			return true;
		}
		lastWriteTime = default;
		fileLength = 0L;
		return false;
	}
}
