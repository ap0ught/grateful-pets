using System;

namespace XRL.UI.Framework;

public class CyclicScrollContext<Data, Context> : ScrollContext<Data, Context> where Context : NavigationContext
{
	public bool intentional;

	public Action<int> preSelect;

	public override void Setup()
	{
		for (int i = 0; i < contexts.Count; i++)
		{
			Context val = contexts[i];
			if (val != null)
			{
				val.parentContext = this;
				val.Setup();
			}
		}
		base.Setup();
	}

	public override NavigationContext GetContextAt(int index)
	{
		if (data.Count <= index)
		{
			return null;
		}
		if (contexts.Count == 0)
		{
			return null;
		}
		if (index % contexts.Count < 0 || index % contexts.Count >= contexts.Count)
		{
			return null;
		}
		ScrollChildContext scrollChildContext = contexts[index % contexts.Count] as ScrollChildContext;
		if (scrollChildContext != null && !intentional)
		{
			_ = scrollChildContext.index;
		}
		return scrollChildContext;
	}

	public override int? ContextIndex(NavigationContext to)
	{
		foreach (Context context in contexts)
		{
			if (to.IsInside(context) && context is ScrollChildContext scrollChildContext)
			{
				return scrollChildContext.index;
			}
		}
		return null;
	}

	public override void SelectIndex(int index)
	{
		if (preSelect != null)
		{
			preSelect(index);
		}
		base.SelectIndex(index);
	}
}
