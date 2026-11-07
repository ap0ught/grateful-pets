using System;
using System.Collections.Generic;
using System.Text;
using XRL.UI;
using XRL.World.Tinkering;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Tinkering_Disassemble : BaseSkill
{
	private static List<string> ModNames = new List<string>();

	private static StringBuilder ModNamesSB = new StringBuilder();

	public static void Init()
	{
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AllowInventoryStackEvent.ID && ID != AutoexploreObjectEvent.ID && ID != InventoryActionEvent.ID && ID != OwnerGetInventoryActionsEvent.ID && ID != PooledEvent<GetItemElementsEvent>.ID)
		{
			return ID == TookEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("scholarship", 2);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TookEvent E)
	{
		if (E.Context != "Tinkering" && WantToDisassemble(E.Item))
		{
			InventoryActionEvent.Check(E.Item, ParentObject, E.Item, "DisassembleAll", Auto: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AllowInventoryStackEvent E)
	{
		if (WantToDisassemble(E.Item))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AutoexploreObjectEvent E)
	{
		if (!E.AutogetOnlyMode && E.Command == null && WantToDisassemble(E.Item))
		{
			E.Command = "DisassembleAll";
			E.AllowRetry = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OwnerGetInventoryActionsEvent E)
	{
		if (CanBeConsideredScrap(E.Object) && !TinkeringHelpers.ConsiderStandardScrap(E.Object) && E.Actor.IsPlayer() && E.Object.Understood())
		{
			if (CheckScrapToggle(E.Object))
			{
				E.AddAction("Toggle Scrap", "stop treating these as scrap", "ToggleScrap", "scrap", 'S', FireOnActor: true, 0, 0, Override: false, WorksAtDistance: true);
			}
			else
			{
				E.AddAction("Toggle Scrap", "treat these as scrap", "ToggleScrap", "scrap", 'S', FireOnActor: true, 0, 0, Override: false, WorksAtDistance: true);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ToggleScrap" && E.Actor.IsPlayer())
		{
			ToggleScrap(E.Item);
		}
		return base.HandleEvent(E);
	}

	public static string ModProfile(GameObject obj)
	{
		ModNames.Clear();
		int i = 0;
		for (int count = obj.PartsList.Count; i < count; i++)
		{
			if (obj.PartsList[i] is IModification modification)
			{
				ModNames.Add(modification.Name);
			}
		}
		if (ModNames.Count > 0)
		{
			if (ModNames.Count > 1)
			{
				ModNames.Sort();
				ModNamesSB.Clear();
				foreach (string modName in ModNames)
				{
					ModNamesSB.Append('+').Append(modName);
				}
				return ModNamesSB.ToString();
			}
			return "+" + ModNames[0];
		}
		return "";
	}

	public static string ToggleKey(GameObject obj)
	{
		string text = "ScrapToggle_" + obj.GetTinkeringBlueprint() + ModProfile(obj);
		Tinkering_Mine part = obj.GetPart<Tinkering_Mine>();
		if (part != null)
		{
			text = ((part.Timer > 0) ? (text + "/AsBomb") : (text + "/AsMine"));
		}
		return text;
	}

	public static bool CanBeConsideredScrap(GameObject obj)
	{
		if (!obj.IsReal)
		{
			return false;
		}
		TinkerItem part = obj.GetPart<TinkerItem>();
		if (part == null)
		{
			return false;
		}
		if (!part.CanDisassemble)
		{
			return false;
		}
		if (obj.IsCreature)
		{
			return false;
		}
		Tinkering_Mine part2 = obj.GetPart<Tinkering_Mine>();
		if (part2 != null && part2.Armed)
		{
			return false;
		}
		if (obj.HasTag("BaseObject"))
		{
			return false;
		}
		return true;
	}

	public static bool CheckScrapToggle(GameObject obj)
	{
		if (The.Game.GetBooleanGameState(ToggleKey(obj)))
		{
			return obj.Understood();
		}
		return false;
	}

	public static void SetScrapToggle(GameObject obj, bool flag)
	{
		if (flag)
		{
			The.Game.SetBooleanGameState(ToggleKey(obj), Value: true);
		}
		else
		{
			The.Game.RemoveBooleanGameState(ToggleKey(obj));
		}
	}

	public static void ToggleScrap(GameObject obj)
	{
		SetScrapToggle(obj, !CheckScrapToggle(obj));
	}

	public static bool ConsiderScrap(GameObject obj)
	{
		if (!TinkeringHelpers.ConsiderStandardScrap(obj))
		{
			return CheckScrapToggle(obj);
		}
		return true;
	}

	public bool WantToDisassemble(GameObject Object)
	{
		return WantToDisassemble(ParentObject, Object);
	}

	public static bool WantToDisassemble(GameObject Actor, GameObject Object)
	{
		if (!Actor.IsPlayer())
		{
			return false;
		}
		if (!Options.AutoDisassembleScrap)
		{
			return false;
		}
		if (!GameObject.Validate(ref Object))
		{
			return false;
		}
		if (!CanBeConsideredScrap(Object))
		{
			return false;
		}
		if (!ConsiderScrap(Object))
		{
			return false;
		}
		if (!Object.Owner.IsNullOrEmpty())
		{
			return false;
		}
		if (Object.IsImportant())
		{
			return false;
		}
		if (Object.IsInStasis())
		{
			return false;
		}
		if (Object.HasPart<LiquidVolume>() && Object.GetPart<LiquidVolume>().Volume > 0)
		{
			return false;
		}
		return true;
	}
}
