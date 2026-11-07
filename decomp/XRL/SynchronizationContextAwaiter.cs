using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace XRL;

public struct SynchronizationContextAwaiter(SynchronizationContext context) : INotifyCompletion
{
	private static readonly SendOrPostCallback _postCallback = (object state) =>
	{
		((Action)state)();
	};

	private readonly SynchronizationContext _context = context;

	public bool IsCompleted => _context == SynchronizationContext.Current;

	public void OnCompleted(Action continuation)
	{
		_context.Post(_postCallback, continuation);
	}

	public void GetResult()
	{
	}
}
