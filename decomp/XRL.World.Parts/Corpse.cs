using System;
using System.Collections.Generic;
using XRL.Names;
using XRL.World.Anatomy;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class Corpse : IPart
{
	public int CorpseChance;

	public string CorpseBlueprint;

	public string CorpseRequiresBodyPart;

	public int BurntCorpseChance;

	public string BurntCorpseBlueprint;

	public string BurntCorpseRequiresBodyPart;

	public int VaporizedCorpseChance;

	public string VaporizedCorpseBlueprint;

	public string VaporizedCorpseRequiresBodyPart;

	public int BuildCorpseChance = 100;

	public override bool SameAs(IPart p)
	{
		Corpse corpse = p as Corpse;
		if (corpse.CorpseChance != CorpseChance)
		{
			return false;
		}
		if (corpse.CorpseBlueprint != CorpseBlueprint)
		{
			return false;
		}
		if (corpse.CorpseRequiresBodyPart != CorpseRequiresBodyPart)
		{
			return false;
		}
		if (corpse.BurntCorpseChance != BurntCorpseChance)
		{
			return false;
		}
		if (corpse.BurntCorpseBlueprint != BurntCorpseBlueprint)
		{
			return false;
		}
		if (corpse.BurntCorpseRequiresBodyPart != BurntCorpseRequiresBodyPart)
		{
			return false;
		}
		if (corpse.VaporizedCorpseChance != VaporizedCorpseChance)
		{
			return false;
		}
		if (corpse.VaporizedCorpseBlueprint != VaporizedCorpseBlueprint)
		{
			return false;
		}
		if (corpse.VaporizedCorpseRequiresBodyPart != VaporizedCorpseRequiresBodyPart)
		{
			return false;
		}
		if (corpse.BuildCorpseChance != BuildCorpseChance)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeDeathRemovalEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (ParentObject.GetIntProperty("SuppressCorpseDrops") <= 0)
		{
			ProcessCorpseDrop(E);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	private void ProcessCorpseDrop(BeforeDeathRemovalEvent E)
	{
		IInventory dropInventory = ParentObject.GetDropInventory();
		if (dropInventory == null)
		{
			return;
		}
		Zone inventoryZone = dropInventory.GetInventoryZone();
		if (inventoryZone != null && !inventoryZone.Built && !BuildCorpseChance.in100())
		{
			return;
		}
		Body body = ParentObject.Body;
		GameObject gameObject = null;
		if (ParentObject.Physics?.LastDamagedByType == "Fire" || ParentObject.Physics?.LastDamagedByType == "Light")
		{
			if (BurntCorpseChance > 0 && (BurntCorpseRequiresBodyPart.IsNullOrEmpty() || body?.GetFirstPart(BurntCorpseRequiresBodyPart) != null) && BurntCorpseChance.in100())
			{
				gameObject = GameObject.Create(BurntCorpseBlueprint);
			}
		}
		else if (ParentObject.Physics.LastDamagedByType == "Vaporized")
		{
			if (VaporizedCorpseChance > 0 && (VaporizedCorpseRequiresBodyPart.IsNullOrEmpty() || body?.GetFirstPart(VaporizedCorpseRequiresBodyPart) != null) && VaporizedCorpseChance.in100())
			{
				gameObject = GameObject.Create(VaporizedCorpseBlueprint);
			}
		}
		else if (CorpseChance > 0 && (CorpseRequiresBodyPart.IsNullOrEmpty() || body?.GetFirstPart(CorpseRequiresBodyPart) != null) && CorpseChance.in100())
		{
			gameObject = GameObject.Create(CorpseBlueprint);
		}
		if (gameObject == null)
		{
			return;
		}
		Temporary.CarryOver(ParentObject, gameObject);
		Phase.carryOver(ParentObject, gameObject);
		if (ParentObject.HasProperName)
		{
			gameObject.SetStringProperty("CreatureName", ParentObject.BaseDisplayName);
		}
		else
		{
			string text = NameMaker.MakeName(ParentObject, null, null, null, null, null, null, null, null, null, null, null, null, FailureOkay: true);
			if (text != null)
			{
				gameObject.SetStringProperty("CreatureName", text);
			}
		}
		if (ParentObject.HasID)
		{
			gameObject.SetStringProperty("SourceID", ParentObject.ID);
		}
		gameObject.SetStringProperty("SourceBlueprint", ParentObject.Blueprint);
		if (E.Killer != null && E.Killer != ParentObject)
		{
			if (E.Killer.HasID)
			{
				gameObject.SetStringProperty("KillerID", E.Killer.ID);
			}
			gameObject.SetStringProperty("KillerBlueprint", E.Killer.Blueprint);
		}
		if (!E.ThirdPersonReason.IsNullOrEmpty())
		{
			gameObject.SetStringProperty("DeathReason", E.ThirdPersonReason);
		}
		if (ParentObject.HasProperty("StoredByPlayer") || ParentObject.HasProperty("FromStoredByPlayer"))
		{
			gameObject.SetIntProperty("FromStoredByPlayer", 1);
		}
		dropInventory.AddObjectToInventory(gameObject, null, Silent: false, NoStack: false, FlushTransient: true, null, E);
		string genotype = ParentObject.GetGenotype();
		if (!genotype.IsNullOrEmpty())
		{
			gameObject.SetStringProperty("FromGenotype", genotype);
		}
		if (body != null)
		{
			List<GameObject> list = null;
			foreach (BodyPart part in body.GetParts())
			{
				if (part.Cybernetics != null)
				{
					if (list == null)
					{
						list = Event.NewGameObjectList();
					}
					list.Add(part.Cybernetics);
					UnimplantedEvent.Send(ParentObject, part.Cybernetics, part);
					ImplantRemovedEvent.Send(ParentObject, part.Cybernetics, part);
				}
			}
			if (list != null)
			{
				gameObject.AddPart(new CyberneticsButcherableCybernetic(list));
				gameObject.RemovePart<Food>();
			}
		}
		DroppedEvent.Send(ParentObject, gameObject);
	}
}
