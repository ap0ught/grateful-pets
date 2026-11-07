using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsButcherableCybernetic : IPart
{
	public int BaseSuccessChance = 80;

	[NonSerialized]
	public List<GameObject> Cybernetics = new List<GameObject>(1);

	public override IPart DeepCopy(GameObject Parent)
	{
		CyberneticsButcherableCybernetic cyberneticsButcherableCybernetic = base.DeepCopy(Parent) as CyberneticsButcherableCybernetic;
		cyberneticsButcherableCybernetic.Cybernetics = new List<GameObject>(Cybernetics.Count);
		foreach (GameObject cybernetic in Cybernetics)
		{
			cyberneticsButcherableCybernetic.Cybernetics.Add(cybernetic.DeepCopy());
		}
		return cyberneticsButcherableCybernetic;
	}

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		Writer.WriteGameObjectList(Cybernetics);
		base.Write(Basis, Writer);
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		Reader.ReadGameObjectList(Cybernetics);
		base.Read(Basis, Reader);
	}

	public CyberneticsButcherableCybernetic()
	{
	}

	public CyberneticsButcherableCybernetic(GameObject cybernetic)
	{
		Cybernetics.Add(cybernetic);
	}

	public CyberneticsButcherableCybernetic(List<GameObject> cybernetics)
	{
		Cybernetics.AddRange(cybernetics);
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == ObjectEnteringCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (IsButcherable() && IComponent<GameObject>.ThePlayer.HasSkill("CookingAndGathering_Butchery"))
		{
			E.AddAction("Butcher", "butcher", "Butcher", null, 'b', FireOnActor: false, 20);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Butcher" && AttemptButcher(E.Actor, E.Auto, SkipSkill: false, IntoInventory: false, 0, E.FromCell, E.Generated))
		{
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteringCellEvent E)
	{
		AttemptButcher(E.Object, Automatic: true, SkipSkill: false, IntoInventory: false, 0, E.Cell);
		return base.HandleEvent(E);
	}

	public bool AttemptButcher(GameObject who, bool Automatic = false, bool SkipSkill = false, bool IntoInventory = false, int ChanceMod = 0, Cell FromCell = null, List<GameObject> Tracking = null)
	{
		if (!IsButcherable())
		{
			return false;
		}
		if (!SkipSkill && !who.HasSkill("CookingAndGathering_Butchery"))
		{
			return false;
		}
		if (Automatic)
		{
			if (ParentObject.IsImportant())
			{
				return false;
			}
			if (who.IsPlayer())
			{
				if (who.ArePerceptibleHostilesNearby(logSpot: false, popSpot: false, null, null, null, Options.AutoexploreIgnoreEasyEnemies, Options.AutoexploreIgnoreDistantEnemies))
				{
					return false;
				}
			}
			else if (who.Target != null)
			{
				return false;
			}
			CookingAndGathering_Butchery part = who.GetPart<CookingAndGathering_Butchery>();
			if (part != null && !part.IsMyActivatedAbilityToggledOn(part.ActivatedAbilityID))
			{
				return false;
			}
		}
		if (!who.CheckFrozen())
		{
			return false;
		}
		if (!ParentObject.ConfirmUseImportant(who, "butcher"))
		{
			return false;
		}
		bool flag = ParentObject.GetIntProperty("StoredByPlayer") > 0;
		Cell cell = ParentObject.GetCurrentCell();
		Cell cell2 = FromCell ?? who.GetCurrentCell();
		string text = null;
		if (cell != null && cell != cell2)
		{
			text = Directions.GetDirectionDescription(who, cell2.GetDirectionFromCell(cell));
		}
		int chance = BaseSuccessChance + ChanceMod;
		PlayWorldSound("Sounds/Interact/sfx_interact_cyberneticImplant_butcher");
		foreach (GameObject cybernetic in Cybernetics)
		{
			bool flag2 = IntoInventory || (who.IsPlayer() && cybernetic.ShouldAutoget());
			if (chance.in100() && !ParentObject.IsTemporary && !cybernetic.IsTemporary)
			{
				cybernetic.DeleteStringProperty("CannotEquip");
				if (flag)
				{
					cybernetic.SetIntProperty("FromStoredByPlayer", 1);
				}
				Event obj = Event.New("ObjectExtracted");
				obj.SetParameter("Object", cybernetic);
				obj.SetParameter("Source", ParentObject);
				obj.SetParameter("Actor", who);
				obj.SetParameter("Action", "Butcher");
				cybernetic.FireEvent(obj);
				if (who.IsPlayer())
				{
					IComponent<GameObject>.EmitMessage(who, "{{g|You butcher " + cybernetic.an() + " from " + ParentObject.t() + ((text == null) ? "" : (" " + text)) + ".}}", ' ', !Automatic);
				}
				else if (IComponent<GameObject>.Visible(who))
				{
					IComponent<GameObject>.EmitMessage(who, who.Does("butcher") + " " + cybernetic.an() + " from " + ParentObject.t() + ((text == null) ? "" : (" " + text)) + ".", ' ', !Automatic);
				}
				if (flag2)
				{
					who.TakeObject(cybernetic, NoStack: false, Silent: true, 0, null, Tracking);
				}
				else
				{
					cell.AddObject(cybernetic);
				}
			}
			else if (who.IsPlayer())
			{
				IComponent<GameObject>.EmitMessage(who, "{{r|You rip " + cybernetic.an() + " out of " + ParentObject.t() + ((text == null) ? "" : (" " + text)) + ", but destroy " + cybernetic.them + " in the process.}}", ' ', !Automatic);
			}
			else if (IComponent<GameObject>.Visible(who))
			{
				IComponent<GameObject>.EmitMessage(who, who.Does("rip") + " " + cybernetic.an() + " out of " + ParentObject.t() + ((text == null) ? "" : (" " + text)) + ", but " + who.GetVerb("destroy", PrependSpace: false) + " " + cybernetic.them + " in the process.", ' ', !Automatic);
			}
		}
		Cybernetics.Clear();
		ParentObject.Bloodsplatter(SelfSplatter: false);
		ParentObject.Destroy();
		who.UseEnergy(1000, "Skill");
		return true;
	}

	public bool IsButcherable()
	{
		if (Cybernetics.Count > 0 && ParentObject.Render.Visible)
		{
			return !ParentObject.HasPart<HologramMaterial>();
		}
		return false;
	}
}
