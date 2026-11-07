using UnityEngine;
using UnityEngine.UI;

namespace Qud.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class RememberMaxSize : MonoBehaviour, ILayoutElement
{
	public bool RememberHeight = true;

	public bool RememberWidth;

	public float RememberedHeight;

	public float RememberedWidth;

	public Vector2 lastScreenSize;

	public bool forceReset;

	public float minWidth
	{
		get
		{
			if (!RememberWidth)
			{
				return -1f;
			}
			return RememberedWidth;
		}
	}

	public float preferredWidth
	{
		get
		{
			if (!RememberWidth)
			{
				return -1f;
			}
			return RememberedWidth;
		}
	}

	public float flexibleWidth => -1f;

	public float minHeight
	{
		get
		{
			if (!RememberHeight)
			{
				return -1f;
			}
			return RememberedHeight;
		}
	}

	public float preferredHeight
	{
		get
		{
			if (!RememberHeight)
			{
				return -1f;
			}
			return RememberedHeight;
		}
	}

	public float flexibleHeight => -1f;

	public int layoutPriority => 10;

	public void Awake()
	{
		forceReset = true;
	}

	public void CalculateLayoutInputHorizontal()
	{
		if (lastScreenSize.x != (float)Screen.width || lastScreenSize.y != (float)Screen.height || forceReset)
		{
			lastScreenSize = new Vector2(Screen.width, Screen.height);
			RememberedHeight = (RememberedWidth = -1f);
			forceReset = false;
		}
		if (RememberWidth)
		{
			RememberWidth = false;
			RememberedWidth = Mathf.Max(RememberedWidth, LayoutUtility.GetPreferredWidth(transform as RectTransform));
			RememberWidth = true;
		}
	}

	public void CalculateLayoutInputVertical()
	{
		if (RememberHeight)
		{
			RememberHeight = false;
			RememberedHeight = Mathf.Max(RememberedHeight, LayoutUtility.GetPreferredHeight(transform as RectTransform));
			RememberHeight = true;
		}
	}
}
