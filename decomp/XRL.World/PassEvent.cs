using System;
using System.Collections.Generic;

namespace XRL.World;

public abstract class PassEvent<T> : PooledEvent<T> where T : PassEvent<T>, new()
{
	public int Pass = 1;

	public Queue<IEventHandler> Handlers = new Queue<IEventHandler>();

	public void Postprocess(int MaxPasses = 25)
	{
		int count = Handlers.Count;
		while (count > 0)
		{
			Pass++;
			for (int i = 0; i < count; i++)
			{
				try
				{
					IEventHandler handler = Handlers.Dequeue();
					Dispatch(handler);
				}
				catch (Exception x)
				{
					MetricsManager.LogException("T" + $"::Pass {Pass}", x);
				}
			}
			count = Handlers.Count;
			if (Pass == MaxPasses)
			{
				for (int j = 0; j < count; j++)
				{
					Type type = Handlers.Dequeue().GetType();
					MetricsManager.LogAssemblyError(type, "T::Postprocessing ended with persistent handler of type '" + type.FullName + "'.");
				}
				break;
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		Pass = 1;
		Handlers.Clear();
	}
}
