namespace AiUnity.NLog.Core.Common;

public struct AsyncLogEventInfo
{
	public LogEventInfo LogEvent { get; private set; }

	public AsyncContinuation Continuation { get; internal set; }

	public AsyncLogEventInfo(LogEventInfo logEvent, AsyncContinuation continuation)
	{
		this = default;
		LogEvent = logEvent;
		Continuation = continuation;
	}

	public static bool operator ==(AsyncLogEventInfo eventInfo1, AsyncLogEventInfo eventInfo2)
	{
		if ((object)eventInfo1.Continuation == eventInfo2.Continuation)
		{
			return eventInfo1.LogEvent == eventInfo2.LogEvent;
		}
		return false;
	}

	public static bool operator !=(AsyncLogEventInfo eventInfo1, AsyncLogEventInfo eventInfo2)
	{
		if ((object)eventInfo1.Continuation == eventInfo2.Continuation)
		{
			return eventInfo1.LogEvent != eventInfo2.LogEvent;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		AsyncLogEventInfo asyncLogEventInfo = (AsyncLogEventInfo)obj;
		return this == asyncLogEventInfo;
	}

	public override int GetHashCode()
	{
		return LogEvent.GetHashCode() ^ Continuation.GetHashCode();
	}
}
