using System;

namespace Platform.IO;

public class OutOfStorageException : Exception
{
	public OutOfStorageException(string message)
		: base(message)
	{
	}
}
