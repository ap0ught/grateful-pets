using UnityEngine;

namespace AiUnity.Common.Attributes;

public class OrderAttribute : PropertyAttribute
{
	public OrderAttribute(int order)
	{
		base.order = order;
	}
}
