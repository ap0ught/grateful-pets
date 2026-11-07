using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using XRL.Language;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class StiltWell : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CheckAttackableEvent>.ID && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CheckAttackableEvent E)
	{
		return false;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Sacrifice", "sacrifice", "Sacrifice", null, 's', FireOnActor: false, 100);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Sacrifice" && GiveArtifacts(E.Actor, ParentObject))
		{
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CanSmartUse");
		Registrar.Register("CommandSmartUse");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanSmartUse")
		{
			return false;
		}
		if (E.ID == "CommandSmartUse")
		{
			GiveArtifacts(E.GetGameObjectParameter("User"), ParentObject);
		}
		return base.FireEvent(E);
	}

	public static int GetArtifactReputationValue(GameObject obj)
	{
		Examiner part = obj.GetPart<Examiner>();
		int result = 0;
		if (part != null && part.Complexity > 0 && Scanning.GetScanTypeFor(obj) != Scanning.Scan.Bio)
		{
			Commerce part2 = obj.GetPart<Commerce>();
			result = ((part2 != null) ? Math.Max(1, (int)(part2.Value / 10.0)) : 10);
		}
		return result;
	}

	public static bool IsValuedArtifact(GameObject Object)
	{
		return GetArtifactReputationValue(Object) > 0;
	}

	public static bool GiveArtifacts(GameObject Object, GameObject Well)
	{
		Inventory inventory = Object.Inventory;
		List<GameObject> list = new List<GameObject>();
		List<string> list2 = new List<string>();
		List<int> list3 = new List<int>();
		List<int> list4 = new List<int>();
		List<IRenderable> list5 = new List<IRenderable>();
		bool flag = false;
		foreach (GameObject @object in inventory.Objects)
		{
			int artifactReputationValue = GetArtifactReputationValue(@object);
			if (artifactReputationValue > 0)
			{
				if (@object.IsMarkedImportantByPlayer())
				{
					flag = true;
					continue;
				}
				list.Add(@object);
				list3.Add(artifactReputationValue);
				list4.Add(@object.Count);
				list5.Add(@object.RenderForUI());
				list2.Add(@object.GetDisplayName(1120) + " [{{C|+" + artifactReputationValue + "}} reputation]");
			}
		}
		if (list2.Count <= 0)
		{
			return The.Player.ShowFailure(flag ? "You have no artifacts to offer that are not important." : "You have no artifacts to offer.");
		}
		List<(int, int)> list6 = (Object.IsPlayer() ? Popup.PickSeveral("", "Choose artifacts to throw down the well.", "", "Sounds/UI/ui_notification", list2, HotkeySpread.get(new string[2] { "Menus", "UINav" }), list4, list5, Well, null, null, -1, 1, 60, 0, -1, RespectOptionNewlines: false, AllowEscape: true) : new List<(int, int)> { (0, 1) });
		if (list6.IsNullOrEmpty())
		{
			return false;
		}
		int num = 0;
		List<GameObject> list7 = new List<GameObject>(list6.Count);
		foreach (var item2 in list6)
		{
			GameObject gameObject = list[item2.Item1];
			int item = item2.Item2;
			if (!gameObject.ConfirmUseImportant(Object, "throw", "down", item))
			{
				continue;
			}
			gameObject.SplitStack(item, The.Player);
			if (gameObject.TryRemoveFromContext())
			{
				num += list3[item2.Item1] * item;
				list7.Add(gameObject);
				if (list3[item2.Item1] >= 200 && Object.IsPlayer())
				{
					Achievement.DONATE_ITEM_200_REP.Unlock();
				}
			}
		}
		if (!list7.IsNullOrEmpty())
		{
			Well.Physics.PlayWorldSound("Sounds/Interact/sfx_interact_well_throw");
			string text = Grammar.MakeAndList(list7.Select((GameObject x) => x.an()).ToList());
			IComponent<GameObject>.XDidYToZ(Object, "throw", text + " down", Well, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
			if (Object.IsPlayer())
			{
				The.Game.PlayerReputation.Modify("Mechanimists", num, "StiltWell");
			}
		}
		return true;
	}
}
