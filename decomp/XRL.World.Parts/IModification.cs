using System;
using XRL.UI;
using XRL.Wish;
using XRL.World.Parts.Skill;
using XRL.World.Tinkering;

namespace XRL.World.Parts;

[Serializable]
[HasWishCommand]
public abstract class IModification : IActivePart
{
	public int Tier;

	public IModification()
	{
		Configure();
	}

	public IModification(int Tier)
		: this()
	{
		ApplyTier(Tier);
	}

	public void ApplyTier(int Tier)
	{
		this.Tier = Tier;
		TierConfigure();
	}

	public virtual void Configure()
	{
	}

	public virtual void TierConfigure()
	{
	}

	public virtual int GetModificationSlotUsage()
	{
		return 1;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetInventoryActionsAlwaysEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsAlwaysEvent E)
	{
		GameObject actor = E.Actor;
		if (actor != null && actor.IsPlayer() && ParentObject != null && !ParentObject.HasPart<TinkerItem>() && E.Actor.HasPart<Tinkering_ReverseEngineer>())
		{
			TinkerItem tinkerItem = ParentObject.RequirePart<TinkerItem>();
			if (tinkerItem != null)
			{
				tinkerItem.Bits = "";
				tinkerItem.CanDisassemble = true;
				tinkerItem.CanBuild = false;
				tinkerItem.HandleEvent(E);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool SameAs(IPart p)
	{
		if ((p as IModification).Tier != Tier)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public virtual string GetModificationDisplayName()
	{
		return null;
	}

	public virtual bool ModificationApplicable(GameObject obj)
	{
		return true;
	}

	public virtual bool BeingAppliedBy(GameObject obj, GameObject who)
	{
		return true;
	}

	public virtual void ApplyModification(GameObject obj)
	{
	}

	public virtual void ApplyModification()
	{
		ApplyModification(ParentObject);
	}

	protected void IncreaseDifficulty(int Amount, GameObject obj = null)
	{
		if (obj == null)
		{
			obj = ParentObject;
		}
		Examiner examiner = (obj.IsCreature ? obj.GetPart<Examiner>() : obj.RequirePart<Examiner>());
		if (examiner != null)
		{
			examiner.Difficulty += Amount;
		}
	}

	protected void IncreaseDifficultyIfDifficult(int Amount, GameObject obj = null)
	{
		if (obj == null)
		{
			obj = ParentObject;
		}
		Examiner part = obj.GetPart<Examiner>();
		if (part != null && part.Difficulty > 0)
		{
			part.Difficulty += Amount;
		}
	}

	protected void IncreaseDifficultyIfComplex(int Amount, GameObject obj = null)
	{
		if (obj == null)
		{
			obj = ParentObject;
		}
		Examiner part = obj.GetPart<Examiner>();
		if (part != null && part.Complexity > 0)
		{
			part.Difficulty += Amount;
		}
	}

	protected void IncreaseComplexity(int Amount, GameObject obj = null)
	{
		if (obj == null)
		{
			obj = ParentObject;
		}
		Examiner examiner = (obj.IsCreature ? obj.GetPart<Examiner>() : obj.RequirePart<Examiner>());
		if (examiner != null)
		{
			examiner.Complexity += Amount;
			obj.RequirePart<IsTechScannableUnlessAlive>();
		}
	}

	protected void IncreaseComplexityIfComplex(int Amount, GameObject obj = null)
	{
		if (obj == null)
		{
			obj = ParentObject;
		}
		Examiner part = obj.GetPart<Examiner>();
		if (part != null && part.Complexity > 0)
		{
			part.Complexity += Amount;
			obj.RequirePart<IsTechScannableUnlessAlive>();
		}
	}

	protected void IncreaseDifficultyAndComplexity(int Amount1, int Amount2, GameObject obj = null)
	{
		if (obj == null)
		{
			obj = ParentObject;
		}
		Examiner examiner = (obj.IsCreature ? obj.GetPart<Examiner>() : obj.RequirePart<Examiner>());
		if (examiner != null)
		{
			examiner.Difficulty += Amount1;
			examiner.Complexity += Amount2;
			obj.RequirePart<IsTechScannableUnlessAlive>();
		}
	}

	protected void IncreaseDifficultyAndComplexityIfComplex(int Amount1, int Amount2, GameObject obj = null)
	{
		if (obj == null)
		{
			obj = ParentObject;
		}
		Examiner part = obj.GetPart<Examiner>();
		if (part != null && part.Complexity > 0)
		{
			part.Difficulty += Amount1;
			part.Complexity += Amount2;
			obj.RequirePart<IsTechScannableUnlessAlive>();
		}
	}

	protected static bool CheckWornSlot(GameObject Object, string Valid1, string Valid2 = null, string Valid3 = null, string Valid4 = null, bool AllowGeneric = true, bool AllowMagnetized = true, bool AllowPartial = true, bool Invert = false)
	{
		Armor part = Object.GetPart<Armor>();
		if (part == null || part.WornOn == null)
		{
			return false;
		}
		if (part.WornOn != Valid1 && part.WornOn != Valid2 && part.WornOn != Valid3 && part.WornOn != Valid4 && (!AllowGeneric || part.WornOn != "*"))
		{
			bool flag = false;
			if (AllowMagnetized && part.WornOn == "Floating Nearby" && Object.HasPart<ModMagnetized>())
			{
				string partParameter = Object.GetBlueprint().GetPartParameter<string>("Armor", "WornOn");
				if (!partParameter.IsNullOrEmpty() && (partParameter == Valid1 || partParameter == Valid2 || partParameter == Valid3 || partParameter == Valid4 || (AllowGeneric && partParameter == "*")))
				{
					flag = true;
				}
			}
			if (!flag)
			{
				return Invert;
			}
		}
		string usesSlots = Object.UsesSlots;
		if (!usesSlots.IsNullOrEmpty())
		{
			if (AllowPartial)
			{
				if (!usesSlots.CachedCommaExpansion().Contains(Valid1) && !usesSlots.CachedCommaExpansion().Contains(Valid2) && !usesSlots.CachedCommaExpansion().Contains(Valid3) && !usesSlots.CachedCommaExpansion().Contains(Valid4))
				{
					return Invert;
				}
			}
			else if (usesSlots != part.WornOn)
			{
				return Invert;
			}
		}
		return !Invert;
	}

	[WishCommand("modify", null)]
	public static void WishModify(string Argument)
	{
		Argument.Split(':', out var objectName, out var modName);
		if (modName.IsNullOrEmpty())
		{
			Popup.ShowFail("No modification specified.");
			return;
		}
		ModEntry modEntry = ModificationFactory.ModList.Find((ModEntry x) => x.Part.EqualsNoCase(modName)) ?? ModificationFactory.ModList.Find((ModEntry x) => x.TinkerDisplayName.EqualsNoCase(modName));
		if (modEntry == null)
		{
			Popup.ShowFail("No modification by the name '" + modName + "' could be found.");
			return;
		}
		GameObjectBlueprint gameObjectBlueprint = GameObjectFactory.Factory.BlueprintList.Find((GameObjectBlueprint x) => x.Name.EqualsNoCase(objectName)) ?? GameObjectFactory.Factory.BlueprintList.Find((GameObjectBlueprint x) => x.CachedDisplayNameStripped.EqualsNoCase(objectName));
		if (gameObjectBlueprint == null)
		{
			Popup.ShowFail("No blueprint by the name '" + objectName + "' could be found.");
			return;
		}
		GameObject gameObject = gameObjectBlueprint.createUnmodified();
		ItemModding.ApplyModification(gameObject, modEntry.Part, gameObject.GetTechTier());
		if (gameObject.Takeable)
		{
			The.Player.TakeObject(gameObject, NoStack: false, Silent: false, 0);
		}
		else
		{
			The.Player.CurrentCell.getClosestEmptyCell().AddObject(gameObject);
		}
	}
}
