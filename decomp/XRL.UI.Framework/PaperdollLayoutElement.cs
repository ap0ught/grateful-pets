using Genkit;
using Qud.UI;
using UnityEngine;
using UnityEngine.UI;

namespace XRL.UI.Framework;

public class PaperdollLayoutElement : LayoutElement
{
	public ScrollChildContext context;

	public FrameworkUnityScrollChild scrollChild;

	public EquipmentLineData data;

	public Rect2D gridLocation;

	public RectTransform connectorImage;

	public RectTransform backdrop;

	public string connectorDirection = ".";

	public const int CONNECTOR_WIDTH = 90;

	public const int CONNECTOR_HEIGHT = 120;

	public const int CONNECTOR_X = 0;

	public const int CONNECTOR_Y = 0;

	public void setSize(int x, int y)
	{
		backdrop.sizeDelta = new Vector2((float)(x * 90 / 32) * 32f + 64f, (float)(y * 120 / 32) * 32f + 64f);
	}

	public void clearConnector()
	{
		if (connectorImage.transform.parent != gameObject.transform)
		{
			connectorImage.transform.SetParent(gameObject.transform, worldPositionStays: false);
		}
		connectorImage.sizeDelta = new Vector2(0f, 0f);
		connectorImage.anchoredPosition = new Vector2(0f, 0f);
		if (connectorDirection == "L")
		{
			connectLeft();
		}
		if (connectorDirection == "R")
		{
			connectRight();
		}
		if (connectorDirection == "U")
		{
			connectUp();
		}
		if (connectorDirection == "D")
		{
			connectDown();
		}
	}

	public void connectLeft()
	{
		connectorImage.sizeDelta = new Vector2(90f, 2f);
		connectorImage.anchoredPosition = new Vector2(-90f, 0f);
	}

	public void connectRight()
	{
		connectorImage.sizeDelta = new Vector2(90f, 2f);
		connectorImage.anchoredPosition = new Vector2(0f, 0f);
	}

	public void connectUp()
	{
		connectorImage.sizeDelta = new Vector2(2f, 120f);
		connectorImage.anchoredPosition = new Vector2(0f, -120f);
	}

	public void connectDown()
	{
		connectorImage.sizeDelta = new Vector2(2f, 120f);
		connectorImage.anchoredPosition = new Vector2(0f, 0f);
	}
}
