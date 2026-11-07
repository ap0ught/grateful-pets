using System;
using ConsoleLib.Console;

namespace XRL.World.Parts;

[Serializable]
public class Mimic : IPart
{
	public static readonly int ICON_COLOR_PRIORITY = 150;

	public bool CopyColor = true;

	public bool CopyString;

	public bool CopyBackground = true;

	public string ActiveColorString;

	public string ActiveDetailColor;

	public string ActiveTile;

	public string ActiveRenderString;

	public int MaxHostilityRecognitionDistanceMimickingColor = 3;

	public int MaxHostilityRecognitionDistanceMimickingShape;

	public override bool SameAs(IPart p)
	{
		Mimic mimic = p as Mimic;
		if (mimic.CopyColor != CopyColor)
		{
			return false;
		}
		if (mimic.CopyString != CopyString)
		{
			return false;
		}
		if (mimic.CopyBackground != CopyBackground)
		{
			return false;
		}
		if (mimic.ActiveColorString != ActiveColorString)
		{
			return false;
		}
		if (mimic.ActiveDetailColor != ActiveDetailColor)
		{
			return false;
		}
		if (mimic.ActiveTile != ActiveTile)
		{
			return false;
		}
		if (mimic.ActiveRenderString != ActiveRenderString)
		{
			return false;
		}
		if (mimic.MaxHostilityRecognitionDistanceMimickingColor != MaxHostilityRecognitionDistanceMimickingColor)
		{
			return false;
		}
		if (mimic.MaxHostilityRecognitionDistanceMimickingShape != MaxHostilityRecognitionDistanceMimickingShape)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EnteredCellEvent.ID && ID != PooledEvent<GetHostilityRecognitionLimitsEvent>.ID)
		{
			return ID == ZoneBuiltEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		MimicNearbyObject();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		MimicNearbyObject();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetHostilityRecognitionLimitsEvent E)
	{
		if (CopyColor && MaxHostilityRecognitionDistanceMimickingColor >= 0 && (!ActiveColorString.IsNullOrEmpty() || !ActiveDetailColor.IsNullOrEmpty()))
		{
			E.MaxDistance(MaxHostilityRecognitionDistanceMimickingColor);
		}
		if (CopyString && MaxHostilityRecognitionDistanceMimickingShape >= 0 && (!ActiveTile.IsNullOrEmpty() || !ActiveRenderString.IsNullOrEmpty()))
		{
			E.MaxDistance(MaxHostilityRecognitionDistanceMimickingShape);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		if (CopyColor)
		{
			E.ApplyColors(ActiveColorString, ActiveDetailColor, ICON_COLOR_PRIORITY, ICON_COLOR_PRIORITY);
		}
		if (CopyString)
		{
			if (!ActiveTile.IsNullOrEmpty() && !E.Tile.IsNullOrEmpty())
			{
				E.Tile = ActiveTile;
			}
			if (!ActiveRenderString.IsNullOrEmpty())
			{
				E.RenderString = ActiveRenderString;
			}
		}
		return base.Render(E);
	}

	public void MimicNearbyObject()
	{
		Zone currentZone = ParentObject.CurrentZone;
		if (currentZone == null || !currentZone.Built)
		{
			return;
		}
		Cell randomLocalAdjacentCell = ParentObject.CurrentCell.GetRandomLocalAdjacentCell();
		if (randomLocalAdjacentCell == null)
		{
			return;
		}
		int i = 0;
		for (int count = randomLocalAdjacentCell.Objects.Count; i < count; i++)
		{
			GameObject gameObject = randomLocalAdjacentCell.Objects[i];
			if (gameObject.Render == null || gameObject.Render.RenderLayer <= 0 || gameObject.Render.SameAs(ParentObject.Render))
			{
				continue;
			}
			RenderEvent renderEvent = gameObject.RenderForUI(null, AsIfKnown: true);
			if (CopyColor)
			{
				if (CopyBackground)
				{
					if (ActiveColorString != renderEvent.ColorString)
					{
						ActiveColorString = renderEvent.ColorString;
						ActiveDetailColor = renderEvent.DetailColor;
					}
				}
				else
				{
					string text = ColorUtility.StripBackgroundFormatting(renderEvent.ColorString);
					if (ActiveColorString != text)
					{
						ActiveColorString = text;
						ActiveDetailColor = renderEvent.DetailColor;
					}
				}
			}
			if (CopyString)
			{
				ActiveTile = renderEvent.Tile;
				ActiveRenderString = renderEvent.RenderString;
			}
			break;
		}
	}
}
