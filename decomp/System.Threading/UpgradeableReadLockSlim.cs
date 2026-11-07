using System.Runtime.CompilerServices;

namespace System.Threading;

public struct UpgradeableReadLockSlim(ReaderWriterLockSlim Lock) : IDisposable
{
	private ReaderWriterLockSlim Lock = Lock;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Dispose()
	{
		Lock.ExitUpgradeableReadLock();
	}
}
