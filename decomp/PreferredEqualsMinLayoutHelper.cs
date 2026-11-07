using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class PreferredEqualsMinLayoutHelper : MonoBehaviour, ILayoutElement
{
	public MonoBehaviour PreferredSource;

	public bool UseWidth = true;

	public bool UseHeight;

	public ILayoutElement otherLayoutElement => PreferredSource as ILayoutElement;

	int ILayoutElement.layoutPriority
	{
		get
		{
			if (otherLayoutElement == null)
			{
				Debug.LogError(gameObject.name + " layout helper error - otherLayoutElement not set");
			}
			if (otherLayoutElement != null)
			{
				return otherLayoutElement.layoutPriority + 1;
			}
			return 0;
		}
	}

	float ILayoutElement.minWidth
	{
		get
		{
			if (!UseWidth)
			{
				return -1f;
			}
			return otherLayoutElement.preferredWidth;
		}
	}

	float ILayoutElement.minHeight
	{
		get
		{
			if (!UseHeight)
			{
				return -1f;
			}
			return otherLayoutElement.preferredHeight;
		}
	}

	float ILayoutElement.preferredWidth => -1f;

	float ILayoutElement.flexibleWidth => -1f;

	float ILayoutElement.preferredHeight => -1f;

	float ILayoutElement.flexibleHeight => -1f;

	void ILayoutElement.CalculateLayoutInputHorizontal()
	{
	}

	void ILayoutElement.CalculateLayoutInputVertical()
	{
	}
}
