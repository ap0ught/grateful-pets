using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace XRL;

public sealed class Latcher : IDisposable
{
	public struct Lock(Latcher Parent, bool Write) : IDisposable
	{
		private Latcher Parent = Parent;

		private readonly bool Write = Write;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Release()
		{
			if (Parent != null)
			{
				if (Write)
				{
					Parent.ReleaseWrite();
				}
				else
				{
					Parent.ReleaseRead();
				}
				Parent = null;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void IDisposable.Dispose()
		{
			Release();
		}
	}

	public struct WriteLock(Latcher Parent) : IDisposable
	{
		private Latcher Parent = Parent;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Lock Normalize()
		{
			return new Lock(Parent, Write: true);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Release()
		{
			if (Parent != null)
			{
				Parent.ReleaseWrite();
				Parent = null;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void IDisposable.Dispose()
		{
			Release();
		}
	}

	public struct ReadLock(Latcher Parent) : IDisposable
	{
		private Latcher Parent = Parent;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Lock Normalize()
		{
			return new Lock(Parent, Write: false);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Release()
		{
			if (Parent != null)
			{
				Parent.ReleaseRead();
				Parent = null;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void IDisposable.Dispose()
		{
			Release();
		}
	}

	private readonly SemaphoreSlim Read = new SemaphoreSlim(1, 1);

	private readonly SemaphoreSlim Write = new SemaphoreSlim(1, 1);

	private int Readers;

	public WriteLock TakeWriteLock()
	{
		Write.Wait();
		try
		{
			Read.Wait();
		}
		catch (Exception source)
		{
			Write.Release();
			ExceptionDispatchInfo.Throw(source);
		}
		return new WriteLock(this);
	}

	public ReadLock TakeReadLock()
	{
		Write.Wait();
		if (Interlocked.Increment(ref Readers) == 1)
		{
			try
			{
				Read.Wait();
			}
			catch (Exception source)
			{
				Interlocked.Decrement(ref Readers);
				Write.Release();
				ExceptionDispatchInfo.Throw(source);
			}
		}
		Write.Release();
		return new ReadLock(this);
	}

	public async Task<WriteLock> TakeWriteLockAsync(CancellationToken Token = default(CancellationToken))
	{
		await Write.WaitAsync(Token).ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			await Read.WaitAsync(Token).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception source)
		{
			Write.Release();
			ExceptionDispatchInfo.Throw(source);
		}
		return new WriteLock(this);
	}

	public async Task<ReadLock> TakeReadLockAsync(CancellationToken Token = default(CancellationToken))
	{
		await Write.WaitAsync(Token).ConfigureAwait(continueOnCapturedContext: false);
		if (Interlocked.Increment(ref Readers) == 1)
		{
			try
			{
				await Read.WaitAsync(Token).ConfigureAwait(continueOnCapturedContext: false);
			}
			catch (Exception source)
			{
				Interlocked.Decrement(ref Readers);
				Write.Release();
				ExceptionDispatchInfo.Throw(source);
			}
		}
		Write.Release();
		return new ReadLock(this);
	}

	private void ReleaseWrite()
	{
		Read.Release();
		Write.Release();
	}

	private void ReleaseRead()
	{
		if (Interlocked.Decrement(ref Readers) == 0)
		{
			Read.Release();
		}
	}

	public void Dispose()
	{
		Write.Dispose();
		Read.Dispose();
	}
}
