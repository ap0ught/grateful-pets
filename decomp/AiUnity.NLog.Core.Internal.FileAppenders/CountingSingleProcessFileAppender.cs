using System;
using System.IO;
using System.Security;

namespace AiUnity.NLog.Core.Internal.FileAppenders;

[SecuritySafeCritical]
internal class CountingSingleProcessFileAppender : BaseFileAppender
{
	private class Factory : IFileAppenderFactory
	{
		BaseFileAppender IFileAppenderFactory.Open(string fileName, ICreateFileParameters parameters)
		{
			return new CountingSingleProcessFileAppender(fileName, parameters);
		}
	}

	public static readonly IFileAppenderFactory TheFactory = new Factory();

	private FileStream file;

	private long currentFileLength;

	public CountingSingleProcessFileAppender(string fileName, ICreateFileParameters parameters)
		: base(fileName, parameters)
	{
		FileInfo fileInfo = new FileInfo(fileName);
		if (fileInfo.Exists)
		{
			FileTouched(fileInfo.LastWriteTime);
			currentFileLength = fileInfo.Length;
		}
		else
		{
			FileTouched();
			currentFileLength = 0L;
		}
		file = CreateFileStream(allowConcurrentWrite: false);
	}

	public override void Close()
	{
		if (file != null)
		{
			file.Close();
			file = null;
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

	public override bool GetFileInfo(out DateTime lastWriteTime, out long fileLength)
	{
		lastWriteTime = LastWriteTime;
		fileLength = currentFileLength;
		return true;
	}

	public override void Write(byte[] bytes)
	{
		if (file != null)
		{
			currentFileLength += bytes.Length;
			file.Write(bytes, 0, bytes.Length);
			FileTouched();
		}
	}
}
