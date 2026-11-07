using UnityEngine.Events;

namespace XRL.UI.Framework;

public class FrameworkUnityIndexSensitiveScrollChild : FrameworkUnityScrollChild, FrameworkScroller.IIndexSensitiveScrollChild
{
	public UnityEvent<int> onScrollIndexChanged;

	public void SetIndex(int index)
	{
		if (onScrollIndexChanged != null)
		{
			onScrollIndexChanged.Invoke(index);
		}
	}
}
