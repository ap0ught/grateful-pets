using System;
using System.Collections.Generic;
using System.Linq;
using Genkit;
using UnityEngine;
using UnityEngine.UI;
using XRL.World.Anatomy;

namespace XRL.UI.Framework;

public class PaperdollLayoutGroup : LayoutGroup, ILayoutElement
{
	public enum Corner
	{
		UpperLeft,
		UpperRight,
		LowerLeft,
		LowerRight
	}

	public enum Axis
	{
		Horizontal,
		Vertical
	}

	[SerializeField]
	protected Corner m_StartCorner;

	[SerializeField]
	protected Axis m_StartAxis;

	[SerializeField]
	protected Vector2 m_CellSize = new Vector2(100f, 100f);

	[SerializeField]
	protected Vector2 m_Spacing = Vector2.zero;

	[SerializeField]
	protected int m_ConstraintCount = 2;

	private bool ConnectorsDirty;

	private List<PaperdollLayoutElement> paperdollElements = new List<PaperdollLayoutElement>();

	private Dictionary<(int x, int y), PaperdollLayoutElement> grid = new Dictionary<(int, int), PaperdollLayoutElement>();

	private List<Rect2D> usedGrids = new List<Rect2D>();

	private List<PaperdollLayoutElement> abstractSlots = new List<PaperdollLayoutElement>();

	private Stack<PaperdollLayoutElement> lastHandLayedOut = new Stack<PaperdollLayoutElement>();

	private int lastPaperdollLayoutColumnsUsed = -1;

	private int gridWidth = 8;

	public int usedRows;

	private int cellCountX;

	private int cellCountY;

	private int usedCellCountX;

	private int usedCellCountY;

	public Corner startCorner
	{
		get
		{
			return m_StartCorner;
		}
		set
		{
			SetProperty(ref m_StartCorner, value);
		}
	}

	public Axis startAxis
	{
		get
		{
			return m_StartAxis;
		}
		set
		{
			SetProperty(ref m_StartAxis, value);
		}
	}

	public Vector2 cellSize
	{
		get
		{
			return m_CellSize;
		}
		set
		{
			SetProperty(ref m_CellSize, value);
		}
	}

	public Vector2 spacing
	{
		get
		{
			return m_Spacing;
		}
		set
		{
			SetProperty(ref m_Spacing, value);
		}
	}

	public int constraintCount
	{
		get
		{
			return m_ConstraintCount;
		}
		set
		{
			SetProperty(ref m_ConstraintCount, value);
		}
	}

	protected PaperdollLayoutGroup()
	{
	}

	public int Leftmost()
	{
		for (int x = -usedCellCountX; x <= usedCellCountX; x++)
		{
			int sy = usedCellCountY / 2;
			int y;
			for (y = 0; y < usedCellCountY; y++)
			{
				PaperdollLayoutElement paperdollLayoutElement = paperdollElements.FirstOrDefault((PaperdollLayoutElement e) => e.gridLocation.Contains(new Point2D(x, sy - y)));
				if (paperdollLayoutElement != null)
				{
					return paperdollElements.IndexOf(paperdollLayoutElement);
				}
				paperdollLayoutElement = paperdollElements.FirstOrDefault((PaperdollLayoutElement e) => e.gridLocation.Contains(new Point2D(x, sy + y)));
				if (paperdollLayoutElement != null)
				{
					return paperdollElements.IndexOf(paperdollLayoutElement);
				}
			}
		}
		return 0;
	}

	public int Rightmost()
	{
		for (int x = usedCellCountX; x >= -usedCellCountX; x--)
		{
			int sy = usedCellCountY / 2;
			int y;
			for (y = 0; y < usedCellCountY; y++)
			{
				PaperdollLayoutElement paperdollLayoutElement = paperdollElements.FirstOrDefault((PaperdollLayoutElement e) => e.gridLocation.Contains(new Point2D(x, sy - y)));
				if (paperdollLayoutElement != null)
				{
					return paperdollElements.IndexOf(paperdollLayoutElement);
				}
				paperdollLayoutElement = paperdollElements.FirstOrDefault((PaperdollLayoutElement e) => e.gridLocation.Contains(new Point2D(x, sy + y)));
				if (paperdollLayoutElement != null)
				{
					return paperdollElements.IndexOf(paperdollLayoutElement);
				}
			}
		}
		return 0;
	}

	public int LeftOf(PaperdollLayoutElement element)
	{
		for (int w = 0; w < usedCellCountY; w++)
		{
			for (int x = element.gridLocation.x1 - 1; x >= -usedCellCountX; x--)
			{
				int y;
				for (y = element.gridLocation.y1; y <= element.gridLocation.y2; y++)
				{
					PaperdollLayoutElement paperdollLayoutElement = paperdollElements.FirstOrDefault((PaperdollLayoutElement e) => e.gridLocation.Contains(new Point2D(x, y - w)));
					if (paperdollLayoutElement == null)
					{
						paperdollLayoutElement = paperdollElements.FirstOrDefault((PaperdollLayoutElement e) => e.gridLocation.Contains(new Point2D(x, y + w)));
					}
					if (paperdollLayoutElement != null)
					{
						return paperdollElements.IndexOf(paperdollLayoutElement);
					}
				}
			}
		}
		return -1;
	}

	public int RightOf(PaperdollLayoutElement element)
	{
		for (int w = 0; w < usedCellCountY; w++)
		{
			for (int x = element.gridLocation.x2 + 1; x <= usedCellCountX; x++)
			{
				int y;
				for (y = element.gridLocation.y1; y <= element.gridLocation.y2; y++)
				{
					PaperdollLayoutElement paperdollLayoutElement = paperdollElements.FirstOrDefault((PaperdollLayoutElement e) => e.gridLocation.Contains(new Point2D(x, y - w)));
					if (paperdollLayoutElement == null)
					{
						paperdollLayoutElement = paperdollElements.FirstOrDefault((PaperdollLayoutElement e) => e.gridLocation.Contains(new Point2D(x, y + w)));
					}
					if (paperdollLayoutElement != null)
					{
						return paperdollElements.IndexOf(paperdollLayoutElement);
					}
				}
			}
		}
		return -1;
	}

	public int BelowOf(PaperdollLayoutElement element)
	{
		for (int w = 0; w < usedCellCountX; w++)
		{
			for (int y = element.gridLocation.y2 + 1; y <= usedCellCountY; y++)
			{
				int x;
				for (x = element.gridLocation.x1; x <= element.gridLocation.x2; x++)
				{
					PaperdollLayoutElement paperdollLayoutElement = paperdollElements.FirstOrDefault((PaperdollLayoutElement e) => e.gridLocation.Contains(new Point2D(x - w, y)));
					if (paperdollLayoutElement == null)
					{
						paperdollLayoutElement = paperdollElements.FirstOrDefault((PaperdollLayoutElement e) => e.gridLocation.Contains(new Point2D(x + w, y)));
					}
					if (paperdollLayoutElement != null)
					{
						return paperdollElements.IndexOf(paperdollLayoutElement);
					}
				}
			}
		}
		return -1;
	}

	public int AboveOf(PaperdollLayoutElement element)
	{
		for (int w = 0; w < usedCellCountX; w++)
		{
			for (int y = element.gridLocation.y1 - 1; y >= 0; y--)
			{
				int x;
				for (x = element.gridLocation.x1; x <= element.gridLocation.x2; x++)
				{
					PaperdollLayoutElement paperdollLayoutElement = paperdollElements.FirstOrDefault((PaperdollLayoutElement e) => e.gridLocation.Contains(new Point2D(x - w, y)));
					if (paperdollLayoutElement == null)
					{
						paperdollLayoutElement = paperdollElements.FirstOrDefault((PaperdollLayoutElement e) => e.gridLocation.Contains(new Point2D(x + w, y)));
					}
					if (paperdollLayoutElement != null)
					{
						return paperdollElements.IndexOf(paperdollLayoutElement);
					}
				}
			}
		}
		return -1;
	}

	public void Setup(List<PaperdollLayoutElement> inboundPaperdollElements)
	{
		paperdollElements.Clear();
		paperdollElements.AddRange(inboundPaperdollElements);
		lastPaperdollLayoutColumnsUsed = -1;
		LayoutBodyPlan();
	}

	public void LayoutBodyPlan()
	{
		ConnectorsDirty = true;
		lastHandLayedOut.Clear();
		lastPaperdollLayoutColumnsUsed = cellCountX;
		List<PaperdollLayoutElement> list = paperdollElements.Where((PaperdollLayoutElement e) => !paperdollElements.Any((PaperdollLayoutElement el) => el.data.bodyPart.Parts != null && el.data.bodyPart.Parts.Contains(e.data.bodyPart))).ToList();
		list.Where((PaperdollLayoutElement b) => b.data.bodyPart.Type == "Body").ToList();
		list.Where((PaperdollLayoutElement b) => b.data.bodyPart.Type != "Body").ToList();
		grid.Clear();
		usedGrids.Clear();
		abstractSlots.Clear();
		foreach (PaperdollLayoutElement item in list)
		{
			int num = Math.Max(0, Math.Max(item.data.bodyPart.GetPartCount((BodyPart p) => p.Type == "Arm" && (p.Laterality & 1) != 0) - 1, item.data.bodyPart.GetPartCount((BodyPart p) => p.Type == "Arm" && (p.Laterality & 2) != 0) - 1));
			if (item.data.bodyPart.GetPartCount("Arm") == 0)
			{
				num = Math.Max(0, item.data.bodyPart.GetPartCount((BodyPart p) => p.Type == "Hand") / 2 - 1);
			}
			if (num == 0 && item.data.bodyPart.GetPartCount("Hand") == 0)
			{
				num = Math.Max(0, item.data.bodyPart.GetPartCount((BodyPart p) => p.Type == "Foot") / 2 - 1);
			}
			int num2 = Math.Max(0, Math.Max(item.data.bodyPart.GetPartCount((BodyPart p) => p.Type == "Head") - 1, item.data.bodyPart.GetPartCount((BodyPart p) => p.Type == "Feet") - 1));
			for (int num3 = 0; num3 <= num2; num3++)
			{
				for (int num4 = 0; num4 <= num; num4++)
				{
					grid[(num3, num4)] = item;
				}
			}
			item.gridLocation = new Rect2D(0, 0, num2, num);
			usedGrids.Add(item.gridLocation);
			if (item.data.bodyPart.Parts == null)
			{
				continue;
			}
			foreach (BodyPart part in item.data.bodyPart.Parts)
			{
				PaperdollLayoutElement paperdollLayoutElement = paperdollElements.Where((PaperdollLayoutElement el) => el.data.bodyPart == part).FirstOrDefault();
				if (paperdollLayoutElement == null)
				{
					MetricsManager.LogError($"No layout element for {part}");
				}
				else
				{
					LayoutBodyPart(paperdollLayoutElement, item);
				}
			}
		}
		int num5 = int.MaxValue;
		int num6 = int.MaxValue;
		int num7 = int.MinValue;
		int num8 = int.MinValue;
		foreach (Rect2D usedGrid in usedGrids)
		{
			if (usedGrid.x1 < num5)
			{
				num5 = usedGrid.x1;
			}
			if (usedGrid.y1 < num6)
			{
				num6 = usedGrid.y1;
			}
			if (usedGrid.x2 > num7)
			{
				num7 = usedGrid.x2;
			}
			if (usedGrid.y2 > num8)
			{
				num8 = usedGrid.y2;
			}
		}
		foreach (PaperdollLayoutElement item2 in abstractSlots.Where((PaperdollLayoutElement s) => s.data.bodyPart.Type.Contains("Thrown")))
		{
			int num9 = num8;
			while (num9 >= 0)
			{
				int num10 = num5;
				while (num10 <= num7)
				{
					if (grid.ContainsKey((num10, num9)))
					{
						num10++;
						continue;
					}
					goto IL_0465;
				}
				num9--;
				continue;
				IL_0465:
				item2.connectorDirection = ".";
				item2.clearConnector();
				grid[(num10, num9)] = item2;
				item2.gridLocation = new Rect2D(num10, num9, num10, num9);
				usedGrids.Add(item2.gridLocation);
				break;
			}
		}
		foreach (PaperdollLayoutElement item3 in abstractSlots.Where((PaperdollLayoutElement s) => s.data.bodyPart.Contact && !s.data.bodyPart.Type.Contains("Thrown")))
		{
			int num11 = num8;
			while (num11 >= 0)
			{
				int num12 = num7;
				while (num12 >= num5 / 2)
				{
					if (grid.ContainsKey((num12, num11)))
					{
						num12--;
						continue;
					}
					goto IL_0548;
				}
				num11--;
				continue;
				IL_0548:
				item3.connectorDirection = ".";
				item3.clearConnector();
				grid[(num12, num11)] = item3;
				item3.gridLocation = new Rect2D(num12, num11, num12, num11);
				usedGrids.Add(item3.gridLocation);
				break;
			}
		}
		foreach (PaperdollLayoutElement item4 in abstractSlots.Where((PaperdollLayoutElement s) => !s.data.bodyPart.Contact && !s.data.bodyPart.Type.Contains("Thrown")))
		{
			for (int num13 = num6; num13 <= num8; num13++)
			{
				int num14 = num7;
				while (num14 >= num5)
				{
					if (grid.ContainsKey((num14, num13)))
					{
						num14--;
						continue;
					}
					goto IL_062d;
				}
				continue;
				IL_062d:
				item4.connectorDirection = ".";
				item4.clearConnector();
				grid[(num14, num13)] = item4;
				item4.gridLocation = new Rect2D(num14, num13, num14, num13);
				usedGrids.Add(item4.gridLocation);
				break;
			}
		}
		num5 = int.MaxValue;
		num6 = int.MaxValue;
		foreach (Rect2D usedGrid2 in usedGrids)
		{
			if (usedGrid2.x1 < num5)
			{
				num5 = usedGrid2.x1;
			}
			if (usedGrid2.y1 < num6)
			{
				num6 = usedGrid2.y1;
			}
		}
		foreach (PaperdollLayoutElement paperdollElement in paperdollElements)
		{
			paperdollElement.gridLocation.y1 += -num6;
			paperdollElement.gridLocation.y2 += -num6;
			if (paperdollElement.gridLocation.Width - 1 > num7 - paperdollElement.gridLocation.x1)
			{
				paperdollElement.gridLocation.x2 = paperdollElement.gridLocation.x1 + (num7 - paperdollElement.gridLocation.x1);
			}
			paperdollElement.setSize(paperdollElement.gridLocation.Width - 1, paperdollElement.gridLocation.Height - 1);
		}
	}

	public bool IsAbstract(PaperdollLayoutElement part)
	{
		return part.data.bodyPart.Abstract;
	}

	public Rect2D GetUnusedSlotInDirection(PaperdollLayoutElement part, List<string> preferredDirection)
	{
		int num = 0;
		while (true)
		{
			for (int i = 1; i < gridWidth + num; i++)
			{
				foreach (string item in preferredDirection)
				{
					for (int j = part.gridLocation.x1; j <= part.gridLocation.x2; j++)
					{
						for (int k = part.gridLocation.y1; k <= part.gridLocation.y2; k++)
						{
							Vector2i vector2i = new Vector2i(j, k);
							for (int l = 0; l < i; l++)
							{
								if (item == "N")
								{
									vector2i.y--;
								}
								if (item == "S")
								{
									vector2i.y++;
								}
								if (item == "E")
								{
									vector2i.x++;
								}
								if (item == "W")
								{
									vector2i.x--;
								}
							}
							if (!grid.ContainsKey((vector2i.x, vector2i.y)) && Math.Abs(vector2i.x) <= cellCountX / 2)
							{
								return new Rect2D(vector2i.x, vector2i.y, vector2i.x, vector2i.y);
							}
						}
					}
				}
			}
			if (preferredDirection.Count < 4)
			{
				break;
			}
			num++;
		}
		return GetUnusedSlotInDirection(part, new List<string> { "N", "S", "E", "W" });
	}

	public void LayoutBodyPart(PaperdollLayoutElement part, PaperdollLayoutElement parent)
	{
		if (IsAbstract(part))
		{
			abstractSlots.Add(part);
			return;
		}
		if (part.data.bodyPart.Type == "Hand")
		{
			lastHandLayedOut.Push(part);
		}
		BodyPart bodyPart = part.data.bodyPart;
		List<string> list = null;
		if ((bodyPart.Laterality & 1) != 0)
		{
			list = new List<string> { "W", "N", "S", "E" };
		}
		if ((bodyPart.Laterality & 2) != 0)
		{
			list = new List<string> { "E", "N", "S", "W" };
		}
		if (list == null)
		{
			if (bodyPart.Type == "Head")
			{
				list = new List<string> { "N", "W", "E" };
			}
			if (bodyPart.Type == "Face")
			{
				list = new List<string> { "N", "W", "E", "S" };
			}
			if (bodyPart.Type == "Back")
			{
				list = new List<string> { "S", "W", "E" };
			}
			if (bodyPart.Type == "Arm")
			{
				list = new List<string> { "W", "E", "S", "N" };
			}
			if (bodyPart.Type == "Hand")
			{
				list = new List<string> { "W", "E", "S", "N" };
			}
			if (bodyPart.Type == "Foot")
			{
				list = new List<string> { "S" };
			}
			if (bodyPart.Type == "Hands")
			{
				list = new List<string> { "W", "N" };
			}
			if (bodyPart.Type == "Feet")
			{
				list = new List<string> { "S" };
			}
			if (bodyPart.Type == "Fungal Outcrop")
			{
				list = new List<string> { "S", "E", "W", "N" };
			}
			if (bodyPart.Type == "Icy Outcrop")
			{
				list = new List<string> { "S", "E", "W", "N" };
			}
			if (bodyPart.Type == "Fin")
			{
				list = new List<string> { "S", "E", "W", "N" };
			}
			if (bodyPart.Type == "Roots")
			{
				list = new List<string> { "S", "E", "W" };
			}
			if (bodyPart.Type == "Tail")
			{
				list = new List<string> { "S", "E", "W" };
			}
			if (bodyPart.Type == "Tread")
			{
				list = new List<string> { "S", "E", "W" };
			}
		}
		if (list == null)
		{
			list = new List<string> { "N", "W", "E", "S" };
		}
		PaperdollLayoutElement paperdollLayoutElement = parent;
		if (part.data.bodyPart.Type == "Hands" && lastHandLayedOut.Count > 0)
		{
			paperdollLayoutElement = lastHandLayedOut.Pop();
		}
		part.clearConnector();
		Rect2D unusedSlotInDirection = GetUnusedSlotInDirection(paperdollLayoutElement, list);
		part.connectorDirection = ".";
		if (unusedSlotInDirection.x1 > paperdollLayoutElement.gridLocation.x1)
		{
			part.connectorDirection = "L";
		}
		else if (unusedSlotInDirection.x1 < paperdollLayoutElement.gridLocation.x1)
		{
			part.connectorDirection = "R";
		}
		else if (unusedSlotInDirection.y1 < paperdollLayoutElement.gridLocation.y1)
		{
			part.connectorDirection = "U";
		}
		else if (unusedSlotInDirection.y1 > paperdollLayoutElement.gridLocation.y1)
		{
			part.connectorDirection = "D";
		}
		part.gridLocation = unusedSlotInDirection;
		usedGrids.Add(unusedSlotInDirection);
		grid[(unusedSlotInDirection.x1, unusedSlotInDirection.y1)] = part;
		if (bodyPart.Parts == null)
		{
			return;
		}
		foreach (BodyPart childPart in bodyPart.Parts)
		{
			PaperdollLayoutElement paperdollLayoutElement2 = paperdollElements.Where((PaperdollLayoutElement el) => el.data.bodyPart == childPart).FirstOrDefault();
			if (paperdollLayoutElement2 == null)
			{
				MetricsManager.LogError($"No layout element for child {childPart}");
			}
			else
			{
				LayoutBodyPart(paperdollLayoutElement2, part);
			}
		}
	}

	public override void CalculateLayoutInputHorizontal()
	{
		base.CalculateLayoutInputHorizontal();
		SetLayoutInputForAxis(0f, 0f, -1f, 0);
	}

	public override void CalculateLayoutInputVertical()
	{
		float num = (float)padding.vertical + (cellSize.y + spacing.y) * (float)usedRows - spacing.y;
		SetLayoutInputForAxis(num, num, -1f, 1);
	}

	public override void SetLayoutHorizontal()
	{
		LayoutChildrenAlongAxis(0);
	}

	public override void SetLayoutVertical()
	{
		LayoutChildrenAlongAxis(1);
	}

	private void LayoutChildrenAlongAxis(int axis)
	{
		if (axis == 0)
		{
			for (int i = 0; i < rectChildren.Count; i++)
			{
				RectTransform rectTransform = rectChildren[i];
				m_Tracker.Add(this, rectTransform, DrivenTransformProperties.Anchors | DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.SizeDelta);
				rectTransform.anchorMin = Vector2.up;
				rectTransform.anchorMax = Vector2.up;
				rectTransform.sizeDelta = cellSize;
			}
			return;
		}
		float x = base.rectTransform.rect.size.x;
		float y = base.rectTransform.rect.size.y;
		cellCountX = 1;
		cellCountY = 1;
		cellCountX = Mathf.Max(1, Mathf.FloorToInt((x - (float)padding.horizontal + spacing.x + 0.001f) / (cellSize.x + spacing.x)));
		cellCountY = Mathf.Max(1, Mathf.FloorToInt((y - (float)padding.vertical + spacing.y + 0.001f) / (cellSize.y + spacing.y)));
		_ = (int)startCorner % 2;
		_ = (int)startCorner / 2;
		if (startAxis == Axis.Horizontal)
		{
			int num = cellCountX;
			usedCellCountX = Mathf.Clamp(cellCountX, 1, rectChildren.Count);
		}
		else
		{
			int num = cellCountY;
			usedCellCountX = Mathf.Clamp(cellCountX, 1, Mathf.CeilToInt((float)rectChildren.Count / (float)num));
		}
		usedCellCountX = cellCountX;
		if (cellCountX != lastPaperdollLayoutColumnsUsed)
		{
			LayoutBodyPlan();
		}
		Vector2 vector = new Vector2((float)usedCellCountX * cellSize.x + (float)(usedCellCountX - 1) * spacing.x, (float)usedRows * cellSize.y + (float)(usedRows - 1) * spacing.y);
		Vector2 vector2 = new Vector2(GetStartOffset(0, vector.x), GetStartOffset(1, vector.y));
		vector2 = new Vector2(base.rectTransform.rect.width / 2f, 0f);
		usedRows = 0;
		usedRows = 0;
		float num2 = 0f;
		for (int j = 0; j < rectChildren.Count; j++)
		{
			PaperdollLayoutElement component = rectChildren[j].GetComponent<PaperdollLayoutElement>();
			float num3 = ((float)component.gridLocation.x1 + (float)component.gridLocation.x2) / 2f - 0.5f;
			float num4 = ((float)component.gridLocation.y1 + (float)component.gridLocation.y2) / 2f;
			if (num4 > num2)
			{
				num2 = num4;
			}
			SetChildAlongAxis(rectChildren[j], 0, vector2.x + (cellSize[0] + spacing[0]) * num3, cellSize[0]);
			SetChildAlongAxis(rectChildren[j], 1, vector2.y + (cellSize[1] + spacing[1]) * num4, cellSize[1]);
			if (component.gridLocation.y2 + 1 > usedRows)
			{
				usedRows = component.gridLocation.y2 + 1;
			}
			PaperdollLayoutElement component2 = rectChildren[j].GetComponent<PaperdollLayoutElement>();
			if (component2 != null && ConnectorsDirty)
			{
				component2.clearConnector();
				component2.connectorImage.transform.SetParent(gameObject.transform, worldPositionStays: true);
				component2.connectorImage.SetAsFirstSibling();
			}
		}
		if (usedCellCountY == usedRows)
		{
			ConnectorsDirty = false;
		}
		usedCellCountY = usedRows;
		Vector2 vector3 = new Vector2(base.rectTransform.sizeDelta.x, (float)usedRows * cellSize.y + (float)padding.top + (float)padding.bottom);
		if (base.rectTransform.sizeDelta != vector3)
		{
			base.rectTransform.sizeDelta = vector3;
		}
	}
}
