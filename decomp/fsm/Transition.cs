using System;

namespace fsm;

public class Transition
{
	public State source;

	public State target;

	public Func<bool> onCheck = () => false;

	public Action onStart;

	public Func<bool> onTransition = () => true;

	public Action onEnd;

	public Machine machine
	{
		get
		{
			if (source != null)
			{
				return source.machine;
			}
			return null;
		}
	}
}
