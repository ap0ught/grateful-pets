using System.Runtime.CompilerServices;

namespace System.Threading;

public struct ReadLockSlim(ReaderWriterLockSlim Lock) : IDisposable
{
	private ReaderWriterLockSlim Lock = Lock;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Dispose()
	{
		Lock.ExitReadLock();
	}
}
