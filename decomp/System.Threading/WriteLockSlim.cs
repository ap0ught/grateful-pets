using System.Runtime.CompilerServices;

namespace System.Threading;

public struct WriteLockSlim(ReaderWriterLockSlim Lock) : IDisposable
{
	private ReaderWriterLockSlim Lock = Lock;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Dispose()
	{
		Lock.ExitWriteLock();
	}
}
