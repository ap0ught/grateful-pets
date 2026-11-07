using System;
using System.Collections.Generic;

namespace XRL.UI.Framework;

public class ScrollContext<Context> : AbstractScrollContext where Context : NavigationContext
{
	public List<Context> contexts = new List<Context>();

	public override int length => contexts.Count;

	public override NavigationContext GetContextAt(int index)
	{
		if (index >= contexts.Count)
		{
			return null;
		}
		return contexts[index];
	}
}
public class ScrollContext<Data, Context> : AbstractScrollContext where Context : NavigationContext
{
	public List<Data> data = new List<Data>();

	public List<Context> contexts = new List<Context>();

	public Func<int> calculateGridWidth = () => -1;

	public override int length => data.Count;

	public override int rowWidth => calculateGridWidth();

	public override NavigationContext GetContextAt(int index)
	{
		if (contexts.Count <= index)
		{
			return null;
		}
		if (contexts[index] is ScrollChildContext scrollChildContext)
		{
			scrollChildContext.index = index;
		}
		return contexts[index];
	}

	public virtual Data GetDataAt(int index)
	{
		return data[index];
	}
}
