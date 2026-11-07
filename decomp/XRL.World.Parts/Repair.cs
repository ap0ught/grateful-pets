using System;
using XRL.UI;
using XRL.World.Effects;
using XRL.World.Tinkering;

namespace XRL.World.Parts;

[Serializable]
public class Repair : IPart, IRepairSifrahHandler
{
	public const string OWNER_ANGER_SUPPRESS = "DontWarnOnRepair";

	public string FixedBlueprint;

	public int Difficulty;

	public override bool SameAs(IPart p)
	{
		Repair repair = p as Repair;
		if (repair.FixedBlueprint != FixedBlueprint)
		{
			return false;
		}
		if (repair.Difficulty != Difficulty)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID && ID != PooledEvent<IsRepairableEvent>.ID)
		{
			return ID == PooledEvent<RepairedEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(IsRepairableEvent E)
	{
		return false;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Repair", "repair", "Repair", null, 'r', FireOnActor: false, 25);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Repair")
		{
			if (E.Actor.GetTotalConfusion() > 0)
			{
				E.Actor.ShowFailure("You're too confused to do that.");
				return false;
			}
			if (ParentObject.CurrentCell != null)
			{
				if (!E.Actor.FlightMatches(ParentObject))
				{
					E.Actor.ShowFailure("You cannot reach " + ParentObject.t() + " to repair " + ParentObject.them + ".");
					return false;
				}
				if (!E.Actor.PhaseMatches(ParentObject))
				{
					E.Actor.ShowFailure("You are out of phase with " + ParentObject.t() + " and cannot repair " + ParentObject.them + ".");
					return false;
				}
			}
			GameObject parentObject = ParentObject;
			if (parentObject != null && parentObject.HasTagOrProperty("NoRepair"))
			{
				E.Actor.ShowFailure("You can't repair that.");
				return false;
			}
			if (E.Actor.AreHostilesNearby() && E.Actor.FireEvent("CombatPreventsRepair"))
			{
				E.Actor.ShowFailure("You can't repair with hostile creatures nearby.");
				return false;
			}
			if (Options.SifrahRepair)
			{
				if (E.Actor.IsPlayer())
				{
					if (!string.IsNullOrEmpty(ParentObject.Owner) && !ParentObject.HasPropertyOrTag("DontWarnOnRepair") && Popup.ShowYesNoCancel(ParentObject.Does("are", int.MaxValue, null, null, null, AsIfKnown: false, Single: true) + " not owned by you, and trying to repair " + ParentObject.them + " risks damaging " + ParentObject.them + ". Are you sure you want to do so?") != DialogResult.Yes)
					{
						return false;
					}
					GameObject inInventory = ParentObject.InInventory;
					if (inInventory != null && inInventory != E.Actor && !string.IsNullOrEmpty(inInventory.Owner) && inInventory.Owner != ParentObject.Owner && !inInventory.HasPropertyOrTag("DontWarnOnRepair") && Popup.ShowYesNoCancel(inInventory.Does("are", int.MaxValue, null, null, null, AsIfKnown: false, Single: true) + " not owned by you, and trying to repair " + ParentObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: true) + " inside " + inInventory.them + " risks causing damage. Are you sure you want to do so?") != DialogResult.Yes)
					{
						return false;
					}
				}
				RepairSifrah repairSifrah = new RepairSifrah(ParentObject, ParentObject.GetComplexity(), Difficulty, E.Actor.Stat("Intelligence"));
				repairSifrah.HandlerID = ParentObject.ID;
				repairSifrah.HandlerPartName = Name;
				repairSifrah.Play(ParentObject);
				if (repairSifrah.InterfaceExitRequested)
				{
					E.RequestInterfaceExit();
				}
			}
			else if (Difficulty <= 0 || E.Actor.MakeSave("Intelligence", Difficulty, null, null, "Tinkering Repair", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
			{
				RepairResultSuccess(E.Actor, ParentObject);
			}
			else
			{
				RepairResultFailure(E.Actor, ParentObject);
			}
			E.Actor.UseEnergy(1000, "Item Repair");
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RepairedEvent E)
	{
		ParentObject.ReplaceWith(GameObject.CreateUnmodified(FixedBlueprint));
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void RepairResultSuccess(GameObject who, GameObject obj)
	{
		who.PlayWorldOrUISound("Sounds/Misc/sfx_interact_artifact_repair");
		if (who.IsPlayer())
		{
			Popup.ShowBlock("You repair " + obj.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
		}
		RepairedEvent.Send(who, obj);
	}

	public void RepairResultExceptionalSuccess(GameObject who, GameObject obj)
	{
		RepairResultSuccess(who, obj);
		string randomBits = BitType.GetRandomBits("3d6".Roll(), obj.GetComplexity());
		if (!string.IsNullOrEmpty(randomBits))
		{
			who.RequirePart<BitLocker>().AddBits(randomBits);
			if (who.IsPlayer())
			{
				Popup.Show("You receive tinkering bits <{{|" + BitType.GetDisplayString(randomBits) + "}}>");
			}
		}
	}

	public void RepairResultPartialSuccess(GameObject who, GameObject obj)
	{
		if (who.IsPlayer())
		{
			Popup.Show("You make some progress repairing " + obj.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
		}
	}

	public void RepairResultFailure(GameObject who, GameObject obj)
	{
		if (who.IsPlayer())
		{
			Popup.Show("You can't figure out how to fix " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
		}
	}

	public void RepairResultCriticalFailure(GameObject who, GameObject obj)
	{
		if (!RepairCriticalFailureEvent.Check(who, obj))
		{
			return;
		}
		if (obj.IsBroken())
		{
			if (!obj.HasPropertyOrTag("CantDestroyOnRepair"))
			{
				obj.PotentiallyAngerOwner(who, "DontWarnOnRepair");
				IComponent<GameObject>.XDidYToZ(who, "accidentally", "destroy", obj, null, "!", null, null, null, who);
				obj.Destroy();
			}
			else
			{
				RepairResultFailure(who, obj);
			}
			return;
		}
		string message = "You think you broke " + obj.them + "...";
		if (obj.ApplyEffect(new Broken()) && obj.IsBroken())
		{
			if (who.IsPlayer())
			{
				Popup.Show(message);
			}
			obj.PotentiallyAngerOwner(who, "DontWarnOnRepair");
		}
		else
		{
			RepairResultFailure(who, obj);
		}
	}
}
