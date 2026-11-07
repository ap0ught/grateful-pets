using System;
using System.Collections.Generic;
using System.Linq;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class Crayons : IPart
{
	public static readonly string[] BrightColors = new string[7] { "R", "W", "G", "B", "M", "C", "Y" };

	public static readonly string[] BrightColorStrings = BrightColors.Select((string x) => "&" + x).ToArray();

	public static readonly string[] DarkColors = new string[7] { "r", "w", "g", "b", "m", "c", "K" };

	public static readonly string[] AllColors = new string[14]
	{
		"R", "W", "G", "B", "M", "C", "Y", "r", "w", "g",
		"b", "m", "c", "y"
	};

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Draw", "draw", "DrawWithCrayons", null, 'w', FireOnActor: false, 0, 10);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "DrawWithCrayons")
		{
			Cell cell = E.Actor.CurrentCell;
			if (cell == null)
			{
				return false;
			}
			if (cell.OnWorldMap())
			{
				return E.Actor.Fail("You cannot do that on the world map.");
			}
			string text = Popup.AskString("What do you want to draw?", "", "Sounds/UI/ui_notification", null, null, 64);
			if (text.IsNullOrEmpty())
			{
				return false;
			}
			string text2 = Popup.ShowColorPicker("What color do you want to draw with?", 0, null, 60, RespectOptionNewlines: false, AllowEscape: false, null, "", includeNone: false);
			if (text2.IsNullOrEmpty())
			{
				return false;
			}
			string text3 = XRL.UI.PickDirection.ShowPicker("Color");
			if (text3 == null)
			{
				return false;
			}
			Cell cell2 = cell.GetCellFromDirection(text3);
			if (cell2 == null)
			{
				return false;
			}
			if (ParentObject.GetLongProperty("Nanocrayons", 0L) == 1)
			{
				WishResult wishResult = WishSearcher.SearchForCrayonBlueprint(text);
				if (wishResult.Result.IsNullOrEmpty())
				{
					Popup.Show("You're not talented enough to draw that.");
					MetricsManager.LogEvent("NanocrayonFail:" + text);
				}
				else
				{
					cell2.PlayWorldSound("Sounds/Interact/sfx_interact_crayon_draw");
					Popup.Show("You draw a pretty picture.");
					Popup.Show("The picture stretches into the 3rd dimension and becomes real.");
					if (cell2.IsSolid())
					{
						cell2 = cell2.getClosestPassableCell(cell) ?? cell2;
					}
					GameObject gameObject = cell2.AddObject(wishResult.Result);
					Temporary.CarryOver(ParentObject, gameObject);
					Phase.carryOver(E.Actor.PhaseMatches(gameObject) ? ParentObject : E.Actor, gameObject);
					gameObject.Render?.SetForegroundColor(text2);
					ParentObject.Destroy();
					MetricsManager.LogEvent("Nanocrayon:" + text + ":" + text2);
					E.Actor.UseEnergy(1000, "Item Crayons");
					E.RequestInterfaceExit();
				}
			}
			else
			{
				MetricsManager.LogEvent("Crayon:" + text + ":" + text2);
				GameObject highestRenderLayerObject = cell2.GetHighestRenderLayerObject();
				if (highestRenderLayerObject != null && highestRenderLayerObject.Render != null)
				{
					highestRenderLayerObject.Render.DetailColor = text2;
				}
				cell2.PlayWorldSound("Sounds/Interact/sfx_interact_crayon_draw");
				Popup.Show("You draw a pretty picture.");
				E.Actor.UseEnergy(1000, "Item Crayons");
				E.RequestInterfaceExit();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (2.in1000())
		{
			E.Object.SetLongProperty("Nanocrayons", 1L);
		}
		return base.HandleEvent(E);
	}

	public static string GetSubterraneanGrowthColor()
	{
		return Stat.Random(1, 4) switch
		{
			1 => "r", 
			2 => "c", 
			3 => "w", 
			_ => "Y", 
		};
	}

	public static string GetRandomColor(Random R = null)
	{
		return BrightColors.GetRandomElement(R);
	}

	public static string GetRandomDarkColor(Random R = null)
	{
		return DarkColors.GetRandomElement(R);
	}

	public static string GetRandomColorExcept(Predicate<string> test = null, Random R = null)
	{
		return AllColors.Where((string c) => !test(c)).GetRandomElement(R);
	}

	public static string GetRandomColorAll(Random R = null)
	{
		return AllColors.GetRandomElement(R);
	}

	public static List<string> GetRandomDistinctColorsAll(int numColors)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < numColors; i++)
		{
			int num = 0;
			string randomColorAll;
			do
			{
				randomColorAll = GetRandomColorAll();
				num++;
			}
			while (list.Contains(randomColorAll) && num < 20);
			list.Add(randomColorAll);
		}
		return list;
	}
}
