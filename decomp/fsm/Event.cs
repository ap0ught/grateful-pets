using System;

namespace fsm;

public class Event : EventArgs
{
	public static readonly int UNKNOWN = -1;

	public static readonly int NULL = 0;

	public static readonly int NEXT = 1;

	public static readonly int TRIGGER = 2;

	public static readonly int FINISHED = 3;

	public const int USER_FIELD = 1000;

	public int id = UNKNOWN;

	public Event(int _id = -1)
	{
		id = _id;
	}
}
